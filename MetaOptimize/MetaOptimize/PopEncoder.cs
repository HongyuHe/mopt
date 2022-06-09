﻿// <copyright file="PopEncoder.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MetaOptimize
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ZenLib;

    /// <summary>
    /// The Pop encoder for splitting a network capacity into pieces.
    /// </summary>
    public class PopEncoder<TVar, TSolution> : IEncoder<TVar, TSolution>
    {
        /// <summary>
        /// The solver being used.
        /// </summary>
        public ISolver<TVar, TSolution> Solver { get; set; }

        /// <summary>
        /// The topology for the network.
        /// </summary>
        public Topology Topology { get; set; }

        /// <summary>
        /// The maximum number of paths to use between any two nodes.
        /// </summary>
        public int K { get; set; }

        /// <summary>
        /// The reduced capacity topology for the network.
        /// </summary>
        public Topology ReducedTopology { get; set; }

        /// <summary>
        /// The number of partitions to use.
        /// </summary>
        public int NumPartitions { get; set; }

        /// <summary>
        /// Partitioning of the demands.
        /// </summary>
        public IDictionary<(string, string), int> DemandPartitions { get; set; }

        /// <summary>
        /// The individual encoders for each partition.
        /// </summary>
        public OptimalEncoder<TVar, TSolution>[] PartitionEncoders { get; set; }

        /// <summary>
        /// The demand variables for the network (d_k).
        /// </summary>
        public Dictionary<(string, string), TVar> DemandVariables { get; set; }

        /// <summary>
        /// Create a new instance of the <see cref="PopEncoder{TVar, TSolution}"/> class.
        /// </summary>
        /// <param name="solver">The solver to use.</param>
        /// <param name="topology">The network topology.</param>
        /// <param name="k">The max number of paths between nodes.</param>
        /// <param name="numPartitions">The number of partitions.</param>
        /// <param name="demandPartitions">The demand partitions.</param>
        /// <param name="demandEnforcements"> The demand requirements for individual demands.</param>
        public PopEncoder(ISolver<TVar, TSolution> solver, Topology topology, int k, int numPartitions, IDictionary<(string, string), int> demandPartitions,
            IDictionary<(string, string), double> demandEnforcements = null)
        {
            if (numPartitions <= 0)
            {
                throw new ArgumentOutOfRangeException("Partitions must be greater than zero.");
            }
            if (numPartitions > 10)
            {
                throw new ArgumentOutOfRangeException("You need to adjust the max demand allowed.");
            }

            this.Solver = solver;
            this.Topology = topology;
            this.K = k;
            this.ReducedTopology = topology.SplitCapacity(numPartitions);
            this.NumPartitions = numPartitions;
            this.DemandPartitions = demandPartitions;

            this.PartitionEncoders = new OptimalEncoder<TVar, TSolution>[this.NumPartitions];

            for (int i = 0; i < this.NumPartitions; i++)
            {
                var demandConstraints = new Dictionary<(string, string), double>();

                foreach (var demand in this.DemandPartitions)
                {
                    if (demandEnforcements != null)
                    {
                        demandConstraints[demand.Key] = demandEnforcements[demand.Key];
                    }
                    if (demand.Value != i)
                    {
                        demandConstraints[demand.Key] = 0;
                    }
                }
                this.PartitionEncoders[i] = new OptimalEncoder<TVar, TSolution>(solver, this.ReducedTopology, this.K, demandConstraints);
            }
        }

        private void InitializeVariables(Dictionary<(string, string), TVar> preDemandVariables) {
            // establish the demand variables.
            this.DemandVariables = preDemandVariables;
            if (this.DemandVariables == null) {
                this.DemandVariables = new Dictionary<(string, string), TVar>();
                foreach (var pair in this.Topology.GetNodePairs())
                {
                    this.DemandVariables[pair] = this.Solver.CreateVariable("demand_" + pair.Item1 + "_" + pair.Item2);
                }
            }
        }

        /// <summary>
        /// Encode the problem.
        /// </summary>
        /// <returns>The constraints and maximization objective.</returns>
        public OptimizationEncoding<TVar, TSolution> Encoding(Dictionary<(string, string), TVar> preDemandVariables = null, bool noKKT = false)
        {
            InitializeVariables(preDemandVariables);
            var encodings = new OptimizationEncoding<TVar, TSolution>[NumPartitions];

            // get all the separate encodings.
            for (int i = 0; i < this.NumPartitions; i++)
            {
                Dictionary<(string, string), TVar> partitionPreDemandVariables = null;
                partitionPreDemandVariables = new Dictionary<(string, string), TVar>();
                foreach (var (pair, partitionID) in this.DemandPartitions) {
                    if (partitionID == i) {
                        partitionPreDemandVariables[pair] = this.DemandVariables[pair];
                    }
                }
                encodings[i] = this.PartitionEncoders[i].Encoding(partitionPreDemandVariables, noKKT: noKKT);
            }

            // create new demand variables as the sum of the individual partitions.
            var demandVariables = new Dictionary<(string, string), TVar>();
            foreach (var pair in this.Topology.GetNodePairs())
            {
                int partitionID = this.DemandPartitions[pair];
                demandVariables[pair] = this.PartitionEncoders[partitionID].DemandVariables[pair];
                // var demandVariable = this.Solver.CreateVariable("demand_pop_" + pair.Item1 + "_" + pair.Item2);
                // var polynomial = new Polynomial<TVar>(new Term<TVar>(-1, demandVariable));

                // foreach (var encoder in this.PartitionEncoders)
                // {
                    // polynomial.Terms.Add(new Term<TVar>(1, encoder.DemandVariables[pair]));
                // }
                // this.Solver.AddEqZeroConstraint(polynomial);
                // demandVariables[pair] = demandVariable;
            }

            // compute the objective to optimize.
            var objectiveVariable = this.Solver.CreateVariable("objective_pop");
            if (noKKT)
            {
                var maxDemand = this.Topology.TotalCapacity() * -10;
                this.Solver.AddLeqZeroConstraint(new Polynomial<TVar>(new Term<TVar>(1, objectiveVariable), new Term<TVar>(maxDemand)));
            }
            var objective = new Polynomial<TVar>(new Term<TVar>(-1, objectiveVariable));
            foreach (var encoding in encodings)
            {
                objective.Terms.Add(new Term<TVar>(1, encoding.GlobalObjective));
            }

            this.Solver.AddEqZeroConstraint(objective);

            return new OptimizationEncoding<TVar, TSolution>
            {
                GlobalObjective = objectiveVariable,
                MaximizationObjective = new Polynomial<TVar>(new Term<TVar>(1, objectiveVariable)),
                DemandVariables = demandVariables,
            };
        }

        /// <summary>
        /// Get the optimization solution from the solver solution.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public OptimizationSolution GetSolution(TSolution solution)
        {
            var demands = new Dictionary<(string, string), double>();
            var flows = new Dictionary<(string, string), double>();
            var flowPaths = new Dictionary<string[], double>(new PathComparer());

            var solutions = this.PartitionEncoders.Select(e => e.GetSolution(solution)).ToList();

            // foreach (var pair in this.Topology.GetNodePairs())
            // {
            //     // demands[pair] = solutions.Select(s => s.Demands[pair]).Aggregate((a, b) => a + b);
            //     // flows[pair] = solutions.Select(s => s.Flows[pair]).Aggregate((a, b) => a + b);
            //     int partitionID = this.DemandPartitions[pair];
            //     demands[pair] = solutions[partitionID].Demands[pair];
            //     flows[pair] = solutions[partitionID].Flows[pair];
            // }

            // for (int i = 0; i < this.NumPartitions; i++) {
            //     foreach (var path in solutions[i].FlowsPaths.Keys)
            //     {
            //         // flowPaths[path] = solutions.Select(s => s.FlowsPaths[path]).Aggregate((a, b) => a + b);
            //         flowPaths[path] = solutions[i].FlowsPaths[path];
            //     }
            // }

            foreach (var (pair, variable) in this.DemandVariables)
            {
                demands[pair] = this.Solver.GetVariable(solution, variable);
            }

            for (int i = 0; i < this.NumPartitions; i++) {
                foreach (var (pair, variable) in this.PartitionEncoders[i].FlowVariables)
                {
                    flows[pair] = this.Solver.GetVariable(solution, variable);
                }

                foreach (var (path, variable) in this.PartitionEncoders[i].FlowPathVariables)
                {
                    flowPaths[path] = this.Solver.GetVariable(solution, variable);
                }
            }

            return new OptimizationSolution
            {
                TotalDemandMet = solutions.Select(s => s.TotalDemandMet).Aggregate((a, b) => a + b),
                Demands = demands,
                Flows = flows,
                FlowsPaths = flowPaths,
            };
        }
    }
}

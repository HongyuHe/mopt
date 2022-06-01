// <copyright file="OptimalEncodingTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MetaOptimize.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test that the optimal encoding is working.
    /// </summary>
    [TestClass]
    public class OptimalEncodingTests<TVar, TSol>
    {
        /// <summary>
        /// Function to create a new solver.
        /// </summary>
        internal Func<ISolver<TVar, TSol>> CreateSolver;

        /// <summary>
        /// Test that the optimality encoder works for a topology with one edge.
        /// </summary>
        [TestMethod]
        public void TestOptimalityGapSimple()
        {
            var topology = new Topology();
            topology.AddNode("a");
            topology.AddNode("b");
            topology.AddEdge("a", "b", capacity: 10);

            var solver = CreateSolver();
            var optimalEncoder = new OptimalEncoder<TVar, TSol>(solver, topology, k: 1);
            var encoding = optimalEncoder.Encoding();
            var solverSolution = optimalEncoder.Solver.Maximize(encoding.MaximizationObjective);
            var optimizationSolution = optimalEncoder.GetSolution(solverSolution);

            // Debugging information.
            /* foreach (var c in solver.ConstraintExprs)
            {
                Console.WriteLine(c);
            }

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(optimizationSolution, Newtonsoft.Json.Formatting.Indented)); */

            Assert.IsTrue(TestHelper.IsApproximately(10, optimizationSolution.TotalDemandMet));
            Assert.IsTrue(10 <= optimizationSolution.Demands[("a", "b")]);
            Assert.IsTrue(TestHelper.IsApproximately(10, optimizationSolution.Flows[("a", "b")]));
            Assert.IsTrue(0 <= optimizationSolution.Demands[("b", "a")]);
            Assert.IsTrue(TestHelper.IsApproximately(0, optimizationSolution.Flows[("b", "a")]));
        }

        /// <summary>
        /// Test that the optimal encoder works with a diamond topology.
        /// </summary>
        [TestMethod]
        public void TestOptimalityGapDiamond()
        {
            var topology = new Topology();
            topology.AddNode("a");
            topology.AddNode("b");
            topology.AddNode("c");
            topology.AddNode("d");
            topology.AddEdge("a", "b", capacity: 10);
            topology.AddEdge("a", "c", capacity: 10);
            topology.AddEdge("b", "d", capacity: 10);
            topology.AddEdge("c", "d", capacity: 10);

            var solver = CreateSolver();
            var optimalEncoder = new OptimalEncoder<TVar, TSol>(solver, topology, k: 1);
            var encoding = optimalEncoder.Encoding();
            var solverSolution = optimalEncoder.Solver.Maximize(encoding.MaximizationObjective);
            var optimizationSolution = optimalEncoder.GetSolution(solverSolution);

            // Debugging information.
            /* foreach (var c in solver.ConstraintExprs)
            {
                Console.WriteLine(c);
            }

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(optimizationSolution, Newtonsoft.Json.Formatting.Indented)); */

            Assert.IsTrue(TestHelper.IsApproximately(40, optimizationSolution.TotalDemandMet));
            Assert.IsTrue(10 <= optimizationSolution.Demands[("a", "b")]);
            Assert.IsTrue(TestHelper.IsApproximately(10, optimizationSolution.Flows[("a", "b")]));
            Assert.IsTrue(10 <= optimizationSolution.Demands[("a", "c")]);
            Assert.IsTrue(TestHelper.IsApproximately(10, optimizationSolution.Flows[("a", "c")]));
            Assert.IsTrue(10 <= optimizationSolution.Demands[("b", "d")]);
            Assert.IsTrue(TestHelper.IsApproximately(10, optimizationSolution.Flows[("c", "d")]));
            Assert.IsTrue(TestHelper.IsApproximately(0, optimizationSolution.Flows[("a", "d")]));
        }
    }
}
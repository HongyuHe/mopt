﻿// <copyright file="CliOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MetaOptimize.Cli
{
    using CommandLine;

    /// <summary>
    /// The CLI command line arguments.
    /// </summary>
    public class CliOptions
    {
        /// <summary>
        /// The instance of the command line arguments.
        /// </summary>
        public static CliOptions Instance { get; set; }

        /// <summary>
        /// The topology file path.
        /// </summary>
        [Option('f', "file", Required = true, HelpText = "Topology input file to be processed.")]
        public string TopologyFile { get; set; }

        /// <summary>
        /// The heuristic encoder to use.
        /// </summary>
        [Option('h', "heuristic", Required = true, HelpText = "The heuristic encoder to use (Pop | DemandPinning).")]
        public Heuristic Heuristic { get; set; }

        /// <summary>
        /// The solver we want to use.
        /// </summary>
        [Option('c', "solver", Required = true, HelpText = "The solver that we want to use (Gurobi | Zen)")]
        public SolverChoice SolverChoice { get; set; }

        /// <summary>
        /// Timeout for gurobi solver.
        /// </summary>
        [Option('o', "timeout", Default = double.PositiveInfinity, HelpText = "gurobi solver terminates after the specified time")]
        public double Timeout { get; set; }

        /// <summary>
        /// The number of pop slices to use.
        /// </summary>
        [Option('s', "slices", Default = 2, HelpText = "The number of pop slices to use.")]
        public int PopSlices { get; set; }

        /// <summary>
        /// The threshold for demand pinning.
        /// </summary>
        [Option('t', "pinthreshold", Default = 5, HelpText = "The threshold for the demand pinning heuristic.")]
        public int DemandPinningThreshold { get; set; }

        /// <summary>
        /// The maximum number of paths to use for a demand.
        /// </summary>
        [Option('p', "paths", Default = 2, HelpText = "The maximum number of paths to use for any demand.")]
        public int Paths { get; set; }

        /// <summary>
        /// method for finding gap [search or direct].
        /// </summary>
        [Option('m', "method", Default = MethodChoice.Direct, HelpText = "the method for finding the desirable gap [Direct | Search | FindFeas | Random | HillClimber | SimulatedAnnealing]")]
        public MethodChoice Method { get; set; }

        /// <summary>
        /// if using search, shows how much close to optimal is ok.
        /// </summary>
        [Option('d', "confidence", Default = 0.1, HelpText = "if using search, will find a solution as close as this value to optimal.")]
        public double Confidencelvl { get; set; }

        /// <summary>
        /// if using search, this values is used as the starting gap.
        /// </summary>
        [Option('g', "startinggap", Default = 10, HelpText = "if using search, will start the search from this number.")]
        public double StartingGap { get; set; }

        /// <summary>
        /// an upper bound on all the demands to find more useful advers inputs.
        /// </summary>
        [Option('u', "demandub", Default = -1, HelpText = "an upper bound on all the demands.")]
        public double DemandUB { get; set; }

        /// <summary>
        /// number of trails for random search.
        /// </summary>
        [Option('n', "num", Default = 1, HelpText = "number of trials for random search or hill climber.")]
        public int NumRandom { get; set; }

        /// <summary>
        /// number of neighbors to look.
        /// </summary>
        [Option('k', "neighbors", Default = 1, HelpText = "number of neighbors to search before marking as local optimum [for hill climber | simulated annealing].")]
        public int NumNeighbors { get; set; }

        /// <summary>
        /// initial temperature for simulated annealing.
        /// </summary>
        [Option('t', "inittmp", Default = 1, HelpText = "initial temperature for simulated annealing.")]
        public double InitTmp { get; set; }

        /// <summary>
        /// initial temperature for simulated annealing.
        /// </summary>
        [Option('l', "lambda", Default = 1, HelpText = "temperature decrease factor for simulated annealing.")]
        public double TmpDecreaseFactor { get; set; }

        /// <summary>
        /// seed.
        /// </summary>
        [Option('s', "seed", Default = 1, HelpText = "seed for random generator.")]
        public int Seed { get; set; }

        /// <summary>
        /// seed.
        /// </summary>
        [Option('b', "stddev", Default = 100, HelpText = "standard deviation for generating neighbor for hill climber.")]
        public int StdDev { get; set; }

        /// <summary>
        /// Whether to print debugging information.
        /// </summary>
        [Option('d', "debug", Default = false, HelpText = "Prints debugging messages to standard output.")]
        public bool Debug { get; set; }

        /// <summary>
        /// to show more detailed logs.
        /// </summary>
        [Option('v', "verbose", Default = false, HelpText = "more detailed logs")]
        public bool Verbose { get; set; }
    }

    /// <summary>
    /// The encoding heuristic.
    /// </summary>
    public enum Heuristic
    {
        /// <summary>
        /// The pop heuristic.
        /// </summary>
        Pop,

        /// <summary>
        /// The threshold heuristic.
        /// </summary>
        DemandPinning,
    }
    /// <summary>
    /// The solver we want to use.
    /// </summary>
    public enum SolverChoice
    {
        /// <summary>
        /// The Gurobi solver.
        /// </summary>
        Gurobi,

        /// <summary>
        /// The Zen solver.
        /// </summary>
        Zen,
    }
    /// <summary>
    /// The method we want to use.
    /// </summary>
    public enum MethodChoice
    {
        /// <summary>
        /// directly find the max gap
        /// </summary>
        Direct,
        /// <summary>
        /// search for the max gap with some interval
        /// </summary>
        Search,
        /// <summary>
        /// find a solution with gap at least equal to startinggap.
        /// </summary>
        FindFeas,
        /// <summary>
        /// find a solution with random search.
        /// </summary>
        Random,
        /// <summary>
        /// find a solution with hill climber.
        /// </summary>
        HillClimber,
        /// <summary>
        /// find a solution with simulated annealing.
        /// </summary>
        SimulatedAnnealing,
    }
}

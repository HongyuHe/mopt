namespace MetaOptimize
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Gurobi;

    /// <summary>
    /// PIFO Optimal Encoder.
    /// </summary>
    public class PIFOOptimalEncoder<TVar, TSolution> : IEncoder<TVar, TSolution>
    {
        /// <summary>
        /// The underlying solver.
        /// </summary>
        public ISolver<TVar, TSolution> Solver { get; set; }

        /// <summary>
        /// variable showing whether the $i$-th incomming packet is the $j$-th dequeued packet.
        /// </summary>
        public Dictionary<(int, int), TVar> PlacementVariables { get; set; }

        /// <summary>
        /// variable showing rank of packet i.
        /// </summary>
        public Dictionary<int, TVar> RankVariables { get; set; }

        /// <summary>
        /// Cost of an ordering of packets.
        /// </summary>
        public TVar Cost { get; set; }

        /// <summary>
        /// rank equality constraints.
        /// </summary>
        public IDictionary<int, int> RankEqualityConstraints { get; set; }

        /// <summary>
        /// number of packets.
        /// </summary>
        public int NumPackets;

        /// <summary>
        /// maximum rank of a packets.
        /// </summary>
        public int MaxRank;

        /// <summary>
        /// Create a new instance of the encoder.
        /// </summary>
        public PIFOOptimalEncoder(ISolver<TVar, TSolution> solver, int numPackets, int maxRank)
        {
            Solver = solver;
            NumPackets = numPackets;
            MaxRank = maxRank;
        }

        /// <summary>
        /// initialize variables.
        /// <paramref name="preRankVariables"/> precomputed rank variables.
        /// <paramref name="rankEqualityConstraints"/> rank equality constraints.
        /// </summary>
        private void InitializeVariables(Dictionary<int, TVar> preRankVariables,
            Dictionary<int, int> rankEqualityConstraints)
        {
            RankVariables = new Dictionary<int, TVar>();
            PlacementVariables = new Dictionary<(int, int), TVar>();
            for (int packetID = 0; packetID < NumPackets; packetID++) {
                if (preRankVariables == null) {
                    RankVariables[packetID] = Solver.CreateVariable("rank_" + packetID, GRB.INTEGER, lb: 0, ub: MaxRank);
                } else {
                    RankVariables[packetID] = preRankVariables[packetID];
                }
                // ? I guess this is the position index for a pkt among all pkts.
                for (int place = 0; place < NumPackets; place++) {
                    PlacementVariables[(packetID, place)] = Solver.CreateVariable("place_" + packetID + "_" + place, GRB.BINARY);
                }
            }
            RankEqualityConstraints = rankEqualityConstraints;
            Cost = Solver.CreateVariable("total_cost_optimal");
            CreateAdditionalVariables();
        }

        /// <summary>
        /// for the modified versions to create additional variables.
        /// </summary>
        protected virtual void CreateAdditionalVariables()
        {
        }

        private void EnsureRankEquality() {
            if (RankEqualityConstraints == null) {
                return;
            }

            for (int pid = 0; pid < NumPackets; pid++) {
                // * Constraints are the constant values, and variables are the names of the optimization vars.
                // * (1 x variable_value == -1 x variable_name)
                var constr = new Polynomial<TVar>(
                    new Term<TVar>(-1 * RankEqualityConstraints[pid]),
                    new Term<TVar>(1, RankVariables[pid]));
                Solver.AddEqZeroConstraint(constr);
            }
        }

        /// <summary>
        /// Encode the optimal.
        /// </summary>
        public OptimizationEncoding<TVar, TSolution> Encoding(
            Dictionary<int, TVar> preRankVariables = null,
            Dictionary<int, int> rankEqualityConstraints = null,
            bool verbose = false)
        {
            Utils.logger("initialize variables", verbose);
            InitializeVariables(preRankVariables, rankEqualityConstraints);

            Utils.logger("ensure ranks are equal to input ranks", verbose);
            // ? Not sure why this is necessary.
            EnsureRankEquality();

            Utils.logger("ensure (1) each packet is placed once and (2) each place has only one packet.", verbose);
            for (int i = 0; i < NumPackets; i++) {
                // * The two constraints are both 1 (on the right hand side).
                var sumPerPacket = new Polynomial<TVar>(new Term<TVar>(-1));
                var sumPerPlace = new Polynomial<TVar>(new Term<TVar>(-1));
                for (int j = 0; j < NumPackets; j++) {
                    // * (1): \sum_j placementVar[pkt_i][place_j] - 1 = 0
                    sumPerPacket.Add(new Term<TVar>(1, PlacementVariables[(i, j)]));
                    // * (2): \sum_i placementVar[pkt_i][place_j] - 1 = 0
                    sumPerPlace.Add(new Term<TVar>(1, PlacementVariables[(j, i)]));
                }
                Solver.AddEqZeroConstraint(sumPerPacket);
                Solver.AddEqZeroConstraint(sumPerPlace);
            }

            Utils.logger("Adding additional constraints for modified versions", verbose);
            AddOtherConstraints();

            Utils.logger("computing the cost.", verbose);
            ComputeCost();
            var objective = new Polynomial<TVar>(new Term<TVar>(-1, Cost));
            return new PIFOOptimizationEncoding<TVar, TSolution>
            {
                GlobalObjective = Cost,
                MaximizationObjective = objective,
                RankVariables = RankVariables,
            };
        }

        /// <summary>
        /// additional constraints for the modified variants.
        /// </summary>
        protected virtual void AddOtherConstraints()
        {
        }

        /// <summary>
        /// compute the cost of an ordering of packets.
        /// </summary>
        protected virtual void ComputeCost()
        {
            throw new Exception("not implemented....");
        }

        /// <summary>
        /// return whether packet is admitted to the queue.
        /// </summary>
        protected virtual int GetAdmitSolution(TSolution solution, int packetID)
        {
            return 1;
        }

        /// <summary>
        /// Get the optimization solution from the solver.
        /// </summary>
        public OptimizationSolution GetSolution(TSolution solution)
        {
            var packetRanks = new Dictionary<int, double>();
            var packetOrder = new Dictionary<int, int>();
            var packetAdmit = new Dictionary<int, int>();
            for (int packetID = 0; packetID < NumPackets; packetID++) {
                packetRanks[packetID] = Solver.GetVariable(solution, RankVariables[packetID]);
                for (int place = 0; place < NumPackets; place++) {
                    var placeOrNot = Convert.ToInt32(Solver.GetVariable(solution, PlacementVariables[(packetID, place)]));
                    if (placeOrNot > 0.99) {
                        packetOrder[packetID] = place;
                        break;
                    }
                }
                packetAdmit[packetID] = GetAdmitSolution(solution, packetID);
            }

            return new PIFOOptimizationSolution
            {
                Ranks = packetRanks,
                Order = packetOrder,
                Admit = packetAdmit,
                Cost = Convert.ToInt32(Solver.GetVariable(solution, Cost)),
            };
        }
    }
}
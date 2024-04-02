// <copyright file="OptimizationEncoding.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace MetaOptimize
{
    using System.Collections.Generic;

    /// <summary>
    /// The encoding of an optimization.
    /// </summary>
    public class VBPptimizationEncoding<TVar, TSolution> : OptimizationEncoding<TVar, TSolution>
    {
        /// <summary>
        /// The demand expression for any pair of nodes.
        /// </summary>
        public IDictionary<int, List<TVar>> ItemVariables { get; set; }
    }
}

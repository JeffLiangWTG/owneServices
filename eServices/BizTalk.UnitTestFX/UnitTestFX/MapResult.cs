using System;
using System.Collections.Generic;
using System.Text;

namespace CargoWise.BizTalk.UnitTestFX
{
    /// <summary>
    /// Wraps mapping result data
    /// </summary>
    public struct MapResult
    {
        /// <summary>Did the map comparison succeed</summary>
        public bool Success;

        /// <summary>The actual map result if <see cref="Success"/> is false</summary>
        public string MapOutput;

        /// <summary>
        /// A differencing document to identify the delta between the actual and expected result.
        /// Value only set when <see cref="Success"/> is false</summary>
        /// </summary>
        public string OutputUpdateGram;

    }
}

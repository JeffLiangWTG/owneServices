using System;

namespace Hawking.UnitTest.Tools.Xml
{
    public class XmlDiffResult
    {
        public bool Success;
        public string Expected;
        public string Actual;
        public string OutputUpdateGram;
        public Exception Exception;
    }
}

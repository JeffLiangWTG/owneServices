using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.MasterFiles.GUI
{
	[Serializable]
	public class SyntaxValueNotMatchedException : Exception
	{
		public SyntaxValueNotMatchedException(string dbValue) : base($"No matched syntax value for given value [{dbValue}] stored in database")
		{
		}

#if NETFRAMEWORK
		protected SyntaxValueNotMatchedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}

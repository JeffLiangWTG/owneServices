using System;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class NumberRangeNotSetUpException : Exception
	{
		public NumberRangeNotSetUpException(string parentDescription, string fountainType, string fountainPrefix)
			: base(Res.GetString("478CD1D3-F00F-4F8D-8294-0184EC27AC26", "A number range of Type '{0}' and Prefix '{1}' has not been setup for '{2}'.", fountainType, fountainPrefix, parentDescription))
		{
		}

#if NETFRAMEWORK
		protected NumberRangeNotSetUpException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

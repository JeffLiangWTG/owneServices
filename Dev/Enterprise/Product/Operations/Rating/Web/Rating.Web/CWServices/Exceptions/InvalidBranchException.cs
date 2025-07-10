using System;

namespace Enterprise.Rating.Web
{
	[Serializable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
	public class InvalidBranchException : Exception
	{
		public InvalidBranchException() : base("User HomeBranch is not valid. You must specify a branch that exists.")
		{
		}

		public InvalidBranchException(string branch) : base($"Provided branch of value '{branch}' is not valid. You must specify a branch that exists.")
		{
		}

#if NETFRAMEWORK
		protected InvalidBranchException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

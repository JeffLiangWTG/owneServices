using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	[Serializable]
	public class WhsException : ZException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public WhsException() : base("General Warehouse Exception")
		{
		}

		public WhsException(string desc) : base(desc)
		{
		}

#if NETFRAMEWORK
		protected WhsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class MutexLockDeniedException : WhsException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public MutexLockDeniedException() : base("Mutex Lock Denied")
		{
		}

		public MutexLockDeniedException(string desc) : base(desc)
		{
		}

#if NETFRAMEWORK
		protected MutexLockDeniedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}

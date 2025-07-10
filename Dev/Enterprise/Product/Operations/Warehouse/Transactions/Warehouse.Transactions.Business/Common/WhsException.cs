using System;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Serializable]
	public class PickFailedException : WhsException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public PickFailedException() : base("Pick Failed")
		{
		}

		public PickFailedException(string desc) : base(desc)
		{
		}

#if NETFRAMEWORK
		protected PickFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class NotEnoughStockException : WhsException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public NotEnoughStockException()
			: base("Not enough stock")
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		public NotEnoughStockException(ZDecimal unitsFound)
			: base("Not enough stock")
		{
			UnitsFound = unitsFound;
		}

#if NETFRAMEWORK
		protected NotEnoughStockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public ZDecimal UnitsFound { get; }

#if NETFRAMEWORK
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info?.AddValue(nameof(UnitsFound), UnitsFound.ToString());
			base.GetObjectData(info, context);
		}
#endif
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	#region IBondedWarehouseLink

	public interface IBondedWarehouseLink
	{
		void CreateOrUpdateInwardMovement(IWhsBondedWarehouseTransaction info);
		void UpdateDeclarationReference(ZGuid declarationPK, ZString declarationReference);
		IWhsBondedWarehouseTransaction CreateOrUpdateOutwardMovement(IWhsBondedWarehouseTransaction info, bool continueIfError);
		IWhsBondedWarehouseTransaction GetOutwardMovementDetail(IWhsBondedWarehouseTransaction info);
		void CancelOutwardMovement(ZGuid declarationPK);
		void NotifyGoodsAreClearedForRelease(ZGuid declarationPK, IWhsBondedWarehouseTransaction info);
		BusinessObjectCollection GetEntryKeyLookup(IWhsBondedWarehouseTransactionLine line);
	}

	#endregion

	#region MissingDataException class

	[Serializable]
	public class MissingDataException : ZException
	{
		public MissingDataException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MissingDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion

	#region CannotUpdateStockException class

	[Serializable]
	public class CannotUpdateStockException : ZException
	{
		public CannotUpdateStockException(string detailedHumanReadableMessage)
			: base(detailedHumanReadableMessage)
		{
		}

#if NETFRAMEWORK
		protected CannotUpdateStockException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion
}

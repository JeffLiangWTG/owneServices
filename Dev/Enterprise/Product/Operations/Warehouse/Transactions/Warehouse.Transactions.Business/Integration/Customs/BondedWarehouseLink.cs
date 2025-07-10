using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	#region BondedWarehouseLink class

	public abstract class BondedWarehouseLink : IBondedWarehouseLink
	{
		protected BondedWarehouseLink(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			Factory = factory;
		}

		protected readonly BusinessObjectFactory Factory;

		#region IBondedWarehouseLink Members

		#region Nature 20s

		#region IBondedWarehouseLink_CreateOrUpdateInwardMovement

		void IBondedWarehouseLink.CreateOrUpdateInwardMovement(IWhsBondedWarehouseTransaction info)
		{
			Argument.NotNull(info, "info");
			CreateOrUpdateInwardMovementCore(info);
		}

		protected abstract void CreateOrUpdateInwardMovementCore(IWhsBondedWarehouseTransaction info);

		#endregion

		#region UpdateDeclarationReference

		void IBondedWarehouseLink.UpdateDeclarationReference(ZGuid declarationPK, ZString declarationReference)
		{
			if (!declarationPK.IsValid)
			{
				throw new ArgumentException("DeclarationPK");
			}

			if (!declarationReference.IsValid)
			{
				throw new ArgumentException("DeclarationReference");
			}

			UpdateDeclarationReferenceCore(declarationPK, declarationReference);
		}

		protected abstract void UpdateDeclarationReferenceCore(ZGuid declarationPK, ZString declarationReference);

		#endregion

		#endregion

		#region Nature 30s

		#region IBondedWarehouseLink_CreateOrUpdateOutwardMovement

		IWhsBondedWarehouseTransaction IBondedWarehouseLink.CreateOrUpdateOutwardMovement(IWhsBondedWarehouseTransaction info, bool continueIfError)
		{
			Argument.NotNull(info, "info");
			return CreateOrUpdateOutwardMovementCore(info, continueIfError);
		}

		protected abstract IWhsBondedWarehouseTransaction CreateOrUpdateOutwardMovementCore(IWhsBondedWarehouseTransaction info, bool continueIfError);

		#endregion

		#region IBondedWarehouseLink.GetOutwardMovementDetail

		IWhsBondedWarehouseTransaction IBondedWarehouseLink.GetOutwardMovementDetail(IWhsBondedWarehouseTransaction info)
		{
			return GetOutwardMovementDetailCore(info);
		}

		protected abstract IWhsBondedWarehouseTransaction GetOutwardMovementDetailCore(IWhsBondedWarehouseTransaction info);

		#endregion

		#region IBondedWarehouseLink_CancelOutwardMovement

		void IBondedWarehouseLink.CancelOutwardMovement(ZGuid declarationPK)
		{
			if (!declarationPK.IsValid)
			{
				throw new ArgumentNullException(nameof(declarationPK));
			}

			CancelOutwardMovementCore(declarationPK);
		}

		protected abstract void CancelOutwardMovementCore(ZGuid declarationPK);

		#endregion

		#region IBondedWarehouseLink_NotifyGoodsAreClearedForRelease

		void IBondedWarehouseLink.NotifyGoodsAreClearedForRelease(ZGuid declarationPK, IWhsBondedWarehouseTransaction info)
		{
			if (!declarationPK.IsValid)
			{
				throw new ArgumentNullException(nameof(declarationPK));
			}

			Argument.NotNull(info, "info");
			NotifyGoodsAreClearedForReleaseCore(declarationPK, info);
		}

		protected abstract void NotifyGoodsAreClearedForReleaseCore(ZGuid declarationPK, IWhsBondedWarehouseTransaction info);

		#endregion

		#endregion

		#region Findboxes

		BusinessObjectCollection IBondedWarehouseLink.GetEntryKeyLookup(IWhsBondedWarehouseTransactionLine line)
		{
			return GetEntryKeyLookup(line);
		}

		// module ID = WhsEntryLine
		// for multi-country, may need to pass in a parser to set in the collection
		public abstract BusinessObjectCollection GetEntryKeyLookup(IWhsBondedWarehouseTransactionLine line);

		#endregion

		#endregion
	}

	#endregion
}

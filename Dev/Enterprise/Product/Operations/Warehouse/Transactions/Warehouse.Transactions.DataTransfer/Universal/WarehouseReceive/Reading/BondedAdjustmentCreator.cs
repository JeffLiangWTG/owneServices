using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public static class BondedAdjustmentCreator
	{
		#region AdjustOut - Receive

		public static WhsAdjustment AdjustOut(WhsReceive receive, Action<string> createFailureHandler)
		{
			WhsAdjustment adjustment = null;
			if (!receive.IsFinalised || !receive.IsCustomsTransaction || !(receive.Warehouse.WW_IsVirtualWarehouse || receive.IsImportingForChangeOfInventory))
			{
				createFailureHandler(Res.GetString("06be1750-2577-4138-99ac-f37bb048cdfd", "Only Bonded Dockets that are Finalized (in virtual warehouse or for change of ownership) can be Adjusted out."));
			}
			else
			{
				var mostRecentAdjustment = GetMostRecentAdjustment(receive);
				var newAdjustmentSplit = (byte)((mostRecentAdjustment != null) ? mostRecentAdjustment.WD_ExternalReferenceSplit + 1 : 0);

				adjustment = receive.Factory.New<WhsAdjustment>();
				adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
				adjustment.WD_WW_Whs = receive.WD_WW_Whs;
				adjustment.WD_OH_Client = receive.WD_OH_Client;
				adjustment.WD_ExternalReference = receive.WD_ExternalReference;
				adjustment.WD_ExternalReferenceSplit = newAdjustmentSplit;

				AdjustOutReceiveLines(receive, adjustment);

				using (((IBusinessObjectInternals)adjustment).ResumeValidationForAllDescendantsTemporarily())
				{
					adjustment.FinaliseDocketWithoutUserConfirmation();
				}
				adjustment.IsUniqueExternalReferenceCreatedOnSave = false; // don't auto add DocketID as we now use ExtRef + Split to locate the latest adjustment.
			}

			return adjustment;
		}

		static void AdjustOutReceiveLines(WhsReceive receive, WhsAdjustment newAdjustment)
		{
			foreach (var line in receive.Lines)
			{
				var newLine = newAdjustment.Lines.AddNew();
				newLine.WE_OP = line.WE_OP;
				newLine.WE_WL = line.WE_WL;
				newLine.WE_PalletID = line.WE_PalletID;
				newLine.WE_ReasonCode = "AMD";
				newLine.WE_WHC_NKOriginalInventoryHeldCode = line.WE_WHC_NKCurrentInventoryHeldCode;
				newLine.WE_F3_NKPackType = line.WE_F3_NKPackType;
				newLine.WE_TransactionQuantity = line.WE_TransactionQuantity * (-1);
				newLine.WE_PartAttrib1 = line.WE_PartAttrib1;
				newLine.WE_PartAttrib2 = line.WE_PartAttrib2;
				newLine.WE_PartAttrib3 = line.WE_PartAttrib3;
				newLine.WE_SerialNumber = line.WE_SerialNumber;
				newLine.WE_PackingDate = line.WE_PackingDate;
				newLine.WE_ExpiryDate = line.WE_ExpiryDate;
				newLine.WE_BondedEntryKey = line.WE_BondedEntryKey;
				newLine.WE_PackageGroupId = line.WE_PackageGroupId;
				newLine.WE_PerPackageQty = line.WE_PerPackageQty;
			}
		}

		static WhsAdjustment GetMostRecentAdjustment(WhsDocket docket)
		{
			var existingAdjustmentsQuery = new ZQuery();
			existingAdjustmentsQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);
			existingAdjustmentsQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, docket.WD_ExternalReference);
			existingAdjustmentsQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, docket.WD_OH_Client);
			existingAdjustmentsQuery.OrderBy = WhsDocketSchema.Constants.WD_ExternalReferenceSplit + OrderByClause.Descending;

			return docket.Factory.LoadTop1<WhsAdjustment>(existingAdjustmentsQuery);
		}

		#endregion
	}
}

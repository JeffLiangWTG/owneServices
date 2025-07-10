using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderDeepCloneStrategy : BusinessObjectCloneStrategy
	{
		public CusInBondHeaderDeepCloneStrategy(CusInBondHeader headerToClone)
			: this(headerToClone, headerToClone.BH_GB)
		{
		}

		public CusInBondHeaderDeepCloneStrategy(CusInBondHeader headerToClone, ZGuid bH_GBForCloneResult)
			: base(headerToClone)
		{
			this.bH_GBForCloneResult = bH_GBForCloneResult;
		}

		public CusInBondHeader Clone()
		{
			return (CusInBondHeader)Clone(new BusinessObjectCloneArgs(System.Array.Empty<string>(), true));
		}

		readonly ZGuid bH_GBForCloneResult;

		CusInBondHeader headerToClone
		{
			get { return (CusInBondHeader)base.bizObjToClone; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(
				new string[] {
					CusInBondHeader.Schema.BH_JobReference,
					CusInBondHeader.Schema.BH_ETA
				});
			var clonedResult = (CusInBondHeader)base.CloneInternal(args);

			using (clonedResult.GetValidationSuspender())
			using (clonedResult.SuspendSettingHasChanges())
			{
				clonedResult.BH_GB = bH_GBForCloneResult;
				var billPKsMapping = CopyBills(args, clonedResult);
				CopyMovementHeaders(args, billPKsMapping, clonedResult);
			}

			return clonedResult;
		}

		Dictionary<ZGuid, ZGuid> CopyBills(BusinessObjectCloneArgs args, CusInBondHeader clonedResult)
		{
			args.AddExcludedColumns(
				new string[] {
					CusInBondBill.Schema.B0_BH,
					CusInBondBill.Schema.B0_IssuerCode,
					CusInBondBill.Schema.B0_MasterBillNumber,
					CusInBondBill.Schema.B0_HouseBillNumber,
					CusInBondBill.Schema.B0_ManifestQty,
					CusInBondBill.Schema.B0_ManifestUQ,
					CusInBondBill.Schema.B0_Weight,
					CusInBondBill.Schema.B0_WeightUQ,
					CusInBondBill.Schema.B0_Volume,
					CusInBondBill.Schema.B0_VolumeUQ
				});

			var result = new Dictionary<ZGuid, ZGuid>();
			foreach (CusInBondBill bill in IBusinessObjectCollectionExtensions.ToArray<CusInBondBill>(headerToClone.Bills))
			{
				var clonedBill = (CusInBondBill)bill.Clone(args);

				using (clonedBill.GetValidationSuspender())
				using (clonedBill.SuspendSettingHasChanges())
				{
					clonedBill.B0_BH = clonedResult.PK;
					clonedResult.Bills.Add(clonedBill);
					result.Add(bill.PK, clonedBill.PK);
					CopyJobDocAddresses(args, bill.DocAddresses, clonedBill);
					CopyAdditionalReferences(args, bill.AdditionalReferences, clonedBill);
				}
			}
			return result;
		}

		void CopyJobDocAddresses(BusinessObjectCloneArgs args, JobDocAddressDependentCollection docAddressesToClone, CusInBondBill clonedBill)
		{
			args.AddExcludedColumns(new string[] { JobDocAddress.Schema.E2_ParentID });
			clonedBill.DocAddresses.RemoveAndDeleteAll();
			foreach (JobDocAddress addressToClone in docAddressesToClone.ToArray<JobDocAddress>())
			{
				JobDocAddress clonedAddress = (JobDocAddress)addressToClone.Clone(args);
				using (clonedAddress.GetValidationSuspender())
				using (clonedAddress.SuspendSettingHasChanges())
				{
					clonedAddress.E2_ParentID = clonedBill.PK;
					clonedAddress.E2_ParentTableCode = addressToClone.E2_ParentTableCode;
					clonedBill.DocAddresses.Add(clonedAddress);
				}
				clonedAddress.HasChanges = true;
			}
		}

		void CopyAdditionalReferences(BusinessObjectCloneArgs args, CusInbondBillAddRefCollection additionalReferencesToClone, CusInBondBill clonedBill)
		{
			args.AddExcludedColumns(new string[] { CusInbondBillAddRef.Schema.BR_B0 });
			foreach (CusInbondBillAddRef additionalReferenceToClone in IBusinessObjectCollectionExtensions.ToArray<CusInbondBillAddRef>(additionalReferencesToClone))
			{
				CusInbondBillAddRef clonedAdditionalReference = (CusInbondBillAddRef)additionalReferenceToClone.Clone(args);
				using (clonedAdditionalReference.GetValidationSuspender())
				using (clonedAdditionalReference.SuspendSettingHasChanges())
				{
					clonedAdditionalReference.BR_B0 = clonedBill.PK;
					clonedBill.AdditionalReferences.Add(clonedAdditionalReference);
				}
			}
		}

		void CopyMovementHeaders(BusinessObjectCloneArgs args, Dictionary<ZGuid, ZGuid> billPKsMapping, CusInBondHeader clonedResult)
		{
			args.AddExcludedColumns(
				new string[] {
					CusInBondMoveHeader.Schema.BM_BH,
					CusInBondMoveHeader.Schema.BM_CustomsStatus,
					CusInBondMoveHeader.Schema.BM_MessageStatus,
					CusInBondMoveHeader.Schema.BM_WarehouseTransactionStatus,
					CusInBondMoveHeader.Schema.BM_ExportDate,
					CusInBondMoveHeader.Schema.BM_ArrivalDate,
					CusInBondMoveHeader.Schema.BM_MonetaryValue,
					CusInBondMoveHeader.Schema.BM_TOLDate,
					CusInBondMoveHeader.Schema.BM_InBondClosedDate
				});

			foreach (CusInBondMoveHeader moveHeader in headerToClone.MovementHeaders.ToArray<CusInBondMoveHeader>())
			{
				var clonedMoveHeader = (CusInBondMoveHeader)moveHeader.Clone(args);

				using (clonedMoveHeader.GetValidationSuspender())
				using (clonedMoveHeader.SuspendSettingHasChanges())
				{
					clonedMoveHeader.BM_BH = clonedResult.PK;
					clonedResult.MovementHeaders.Add(clonedMoveHeader);

					CopyMovementDetails(args, billPKsMapping, moveHeader.MovementDetails, clonedMoveHeader);
				}
			}
		}

		void CopyMovementDetails(BusinessObjectCloneArgs args, Dictionary<ZGuid, ZGuid> billPKsMapping, CusInBondMoveDetailCollection movementDetailsToClone, CusInBondMoveHeader clonedMoveHeader)
		{
			args.AddExcludedColumns(
				new string[] {
					CusInBondMoveDetail.Schema.B9_B0,
					CusInBondMoveDetail.Schema.B9_BM,
					CusInBondMoveDetail.Schema.B9_CustomsStatus,
					CusInBondMoveDetail.Schema.B9_SeqNo,
					CusInBondMoveDetail.Schema.B9_InBoundQty
				});

			foreach (CusInBondMoveDetail moveDetailToClone in IBusinessObjectCollectionExtensions.ToArray<CusInBondMoveDetail>(movementDetailsToClone))
			{
				var clonedMoveDetail = (CusInBondMoveDetail)moveDetailToClone.Clone(args);
				using (clonedMoveDetail.GetValidationSuspender())
				using (clonedMoveDetail.SuspendSettingHasChanges())
				{
					clonedMoveDetail.B9_BM = clonedMoveHeader.PK;
					ZGuid billPk;
					if (billPKsMapping.TryGetValue(moveDetailToClone.B9_B0, out billPk))
					{
						clonedMoveDetail.B9_B0 = billPk;
					}
					if (!moveDetailToClone.B9_FirstSecondaryNotifyParty.IsEmpty)
					{
						clonedMoveDetail.B9_FirstSecondaryNotifyParty = moveDetailToClone.B9_FirstSecondaryNotifyParty;
					}

					if (!moveDetailToClone.B9_SecondSecondaryNotifyParty.IsEmpty)
					{
						clonedMoveDetail.B9_SecondSecondaryNotifyParty = moveDetailToClone.B9_SecondSecondaryNotifyParty;
					}

					if (!moveDetailToClone.B9_ThirdSecondaryNotifyParty.IsEmpty)
					{
						clonedMoveDetail.B9_ThirdSecondaryNotifyParty = moveDetailToClone.B9_ThirdSecondaryNotifyParty;
					}

					if (!moveDetailToClone.B9_FourthSecondaryNotifyParty.IsEmpty)
					{
						clonedMoveDetail.B9_FourthSecondaryNotifyParty = moveDetailToClone.B9_FourthSecondaryNotifyParty;
					}
					clonedMoveDetail.B9_InBoundQty = moveDetailToClone.B9_InBoundQty;
					clonedMoveHeader.MovementDetails.Add(clonedMoveDetail);

					CopyContainers(args, moveDetailToClone.Containers, clonedMoveDetail);
					CopySecondaryParties(args, moveDetailToClone.SecondaryNotifyParties, clonedMoveDetail);
				}
			}
		}

		void CopyContainers(BusinessObjectCloneArgs args, CusInBondContainerCollection containersToClone, CusInBondMoveDetail clonedMoveDetail)
		{
			args.AddExcludedColumns(
				new string[] {
					CusInBondContainer.Schema.BC_ParentID,
					CusInBondContainer.Schema.BC_ContainerNum,
					CusInBondContainer.Schema.BC_RC,
					CusInBondContainer.Schema.BC_Seal1,
					CusInBondContainer.Schema.BC_Seal2
				});

			foreach (CusInBondContainer containerToClone in IBusinessObjectCollectionExtensions.ToArray<CusInBondContainer>(containersToClone))
			{
				var clonedContainer = (CusInBondContainer)containerToClone.Clone(args);
				using (clonedContainer.GetValidationSuspender())
				using (clonedContainer.SuspendSettingHasChanges())
				{
					clonedContainer.BC_ParentID = clonedMoveDetail.PK;
					clonedContainer.BC_ParentTableCode = "B9";
					clonedMoveDetail.Containers.Add(clonedContainer);

					CopyCommodities(args, containerToClone.Commodities, clonedContainer);
					CopyHazardousDetails(args, containerToClone.UNDGs, clonedContainer);
				}
			}
		}

		void CopyCommodities(BusinessObjectCloneArgs args, CusInBondCargoDescCollection commoditiesToClone, CusInBondContainer clonedContainer)
		{
			args.AddExcludedColumns(
				new string[] {
					CusInBondCargoDesc.Schema.BY_ParentID,
					CusInBondCargoDesc.Schema.BY_MonetaryValue,
					CusInBondCargoDesc.Schema.BY_PieceCount,
					CusInBondCargoDesc.Schema.BY_ManifestUnitCode,
					CusInBondCargoDesc.Schema.BY_MarksAndNumbers,
					CusInBondCargoDesc.Schema.BY_GrossWeight,
					CusInBondCargoDesc.Schema.BY_GrossWeightUnit,
					CusInBondCargoDesc.Schema.BY_InvoiceQuantity,
					CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo,
					CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber
				});

			foreach (CusInBondCargoDesc commodityToClone in IBusinessObjectCollectionExtensions.ToArray<CusInBondCargoDesc>(commoditiesToClone))
			{
				var clonedCommodity = (CusInBondCargoDesc)commodityToClone.Clone(args);
				using (clonedCommodity.GetValidationSuspender())
				using (clonedCommodity.SuspendSettingHasChanges())
				{
					clonedCommodity.BY_ParentID = clonedContainer.PK;
					clonedCommodity.PartSyncManager.Refresh();
					clonedCommodity.BY_HarmonisedTariff = commodityToClone.BY_HarmonisedTariff;
					clonedContainer.Commodities.Add(clonedCommodity);
					CopyChildCommodities(args, commodityToClone.ChildCommodities, clonedCommodity);
				}
			}
		}

		void CopyChildCommodities(BusinessObjectCloneArgs args, CusInBondCargoDescChildCollection childCommoditiesToClone, CusInBondCargoDesc clonedParentCommodity)
		{
			foreach (CusInBondCargoDesc childCommodityToClone in childCommoditiesToClone.ToArray<CusInBondCargoDesc>())
			{
				var clonedChildCommodity = (CusInBondCargoDesc)childCommodityToClone.Clone(args);
				using (clonedChildCommodity.GetValidationSuspender())
				using (clonedChildCommodity.SuspendSettingHasChanges())
				{
					clonedChildCommodity.BY_ParentID = clonedParentCommodity.PK;
					clonedChildCommodity.PartSyncManager.Refresh();
					clonedChildCommodity.BY_HarmonisedTariff = childCommodityToClone.BY_HarmonisedTariff;
					clonedParentCommodity.ChildCommodities.Add(clonedChildCommodity);
					CopyChildCommodities(args, childCommodityToClone.ChildCommodities, clonedChildCommodity);
				}
			}
		}

		void CopyHazardousDetails(BusinessObjectCloneArgs args, UNDGDataItemCollection hazardousDetailsToClone, CusInBondContainer clonedContainer)
		{
			args.AddExcludedColumns(new string[] { UNDGDataItem.Schema.DI_ParentID });
			foreach (UNDGDataItem hazardousDetailToClone in IBusinessObjectCollectionExtensions.ToArray<UNDGDataItem>(hazardousDetailsToClone))
			{
				UNDGDataItem clonedHazardousDetail = (UNDGDataItem)hazardousDetailToClone.Clone(args);
				using (clonedHazardousDetail.GetValidationSuspender())
				using (clonedHazardousDetail.SuspendSettingHasChanges())
				{
					clonedHazardousDetail.DI_ParentID = clonedContainer.PK;
					clonedContainer.UNDGs.Add(clonedHazardousDetail);
				}
			}
		}

		void CopySecondaryParties(BusinessObjectCloneArgs args, SecondaryNotifyPartyCollection secondaryNotifyPartiesToClone, CusInBondMoveDetail clonedMoveDetail)
		{
			args.AddExcludedColumns(new string[] { SecondaryNotifyParty.Schema.CY_ParentID });
			foreach (var containerToClone in secondaryNotifyPartiesToClone.Cast<SecondaryNotifyParty>().ToArray())
			{
				var clonedSecondaryParty = (SecondaryNotifyParty)containerToClone.Clone(args);
				if (!clonedSecondaryParty.CY_Data.IsEmpty)
				{
					using (clonedSecondaryParty.GetValidationSuspender())
					using (clonedSecondaryParty.SuspendSettingHasChanges())
					{
						clonedSecondaryParty.CY_ParentID = clonedMoveDetail.PK;
						clonedMoveDetail.SecondaryNotifyParties.Add(clonedSecondaryParty);
					}
				}
			}
		}
	}
}

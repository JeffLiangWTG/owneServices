using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestWhenCloneSecondaryNotifyPartyEmptyNoError()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "BND123";
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			header.BH_CarrierSCAC = "APDA";
			header.BH_ImportTransportMode = "30";
			header.BH_ImportConveyanceCountry = "AU";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "V3234";
			header.BH_PortUnladingDCode = "3790";
			header.BH_ETA = new ZDateTime(2010, 3, 2);
			header.BH_FTZMove = ZBool.True;
			header.BH_FIRMS = "DFDA";
			var bill = AddBill(header, "MB1232112");
			var moveHeader = AddMoveHeader(header, "INB323423");
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			moveDetail.B9_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			moveDetail.B9_PreviousITNumber = "PRE1234568";
			moveDetail.B9_InBoundQty = 19;
			moveDetail.B9_FirstSecondaryNotifyParty = "1STN";
			moveDetail.B9_SecondSecondaryNotifyParty = "";
			moveDetail.B9_ThirdSecondaryNotifyParty = "3RDN";
			moveDetail.B9_FourthSecondaryNotifyParty = "";
			moveDetail.B9_SeqNo = moveHeader.MovementDetails.Count.ToString();
			var container1 = AddContainer(moveDetail, "TURE1234567", "SEAL1", "SEAL2", helper.Container20US.PK);
			AddCommodityDetails(container1, "MB1232112");
			AddCommodityDetails(container1, "MB1232112");
			AddHazardousDetails(container1);
			AddHazardousDetails(container1);
			var container2 = AddContainer(moveDetail, "NC", "", "", ZGuid.Empty);
			AddCommodityDetails(container2, "MB1232112");
			AddHazardousDetails(container2);
			var clonedHeader = (CusInBondHeader)new CusInBondHeaderDeepCloneStrategy(header).Clone(new BusinessObjectCloneArgs(System.Array.Empty<string>(), true));
			var cloneMoveDetail = clonedHeader.MovementDetails[0];
			var query = new ZQuery(CusCodeDataSchema.CY_ParentID, cloneMoveDetail.PK);
			query.AddToFilter(CusCodeDataSchema.CY_Type, "SNP");
			var cusCodeDatas = Factory.Load<SecondaryNotifyParty>(query);
			Assert(!cusCodeDatas.Any(x => x.CY_Data.IsEmpty));
		}

		public void TestCusInBondHeaderClone()
		{
			var header = GetHeaderToClone();
			AssertNotEquals(ZString.Empty, header.BH_JobReference);
			Factory.Save();
			var clonedHeader = (CusInBondHeader)new CusInBondHeaderDeepCloneStrategy(header).Clone(new BusinessObjectCloneArgs(System.Array.Empty<string>(), true));
			AssertHeader(header, clonedHeader);
			var billPKs = AssertBills(header.Bills, clonedHeader.Bills);
			AssertMoveHeaders(header.MovementHeaders, clonedHeader.MovementHeaders, billPKs);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedHeader = newFactory.Load<CusInBondHeader>(clonedHeader.PK);
			AssertHeader(header, reloadedHeader);
			billPKs = AssertBills(header.Bills, reloadedHeader.Bills);
			AssertMoveHeaders(header.MovementHeaders, reloadedHeader.MovementHeaders, billPKs);
		}

		public void TestWarehouseDataAreNotCloned()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB323";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Consignor.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			moveDetail.B9_InBoundQty = 10;
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "NC";
			var commodity = PopulateCommodityDetails(container.Commodities.AddNew(), ZString.Empty);
			commodity.BY_PartNumber = ZString.Empty;
			var childCommodity1 = PopulateCommodityDetails(commodity.ChildCommodities.AddNew(), "MB323");
			var childCommodity2 = PopulateCommodityDetails(commodity.ChildCommodities.AddNew(), "MB323");
			var childCommodity2Child = PopulateCommodityDetails(childCommodity2.ChildCommodities.AddNew(), ZString.Empty);
			childCommodity2Child.BY_HarmonisedTariff = "1010201030";
			var clonedHeader = (CusInBondHeader)new CusInBondHeaderDeepCloneStrategy(header).Clone(new BusinessObjectCloneArgs(System.Array.Empty<string>(), true));
			AssertEquals(1, clonedHeader.MovementHeaders.Count);
			var clonedMoveHeader = clonedHeader.MovementHeaders[0];
			AssertEquals(helper.Consignor.MainAddress.PK, clonedMoveHeader.BM_OA_WarehouseAddress);
			AssertEquals(ZString.Empty, clonedMoveHeader.BM_WarehouseTransactionStatus);
			AssertEquals(1, clonedMoveHeader.MovementDetails.Count);
			var clonedMoveDetail = clonedMoveHeader.MovementDetails[0];
			AssertEquals(10, clonedMoveDetail.B9_InBoundQty);
			AssertEquals(1, clonedMoveDetail.Containers.Count);
			var clonedContainer = clonedMoveDetail.Containers[0];
			AssertEquals("", clonedContainer.BC_ContainerNum);
			AssertEquals(1, clonedContainer.Commodities.Count);
			var clonedCommodity = clonedContainer.Commodities[0];
			AssertNotEquals(commodity.PK, clonedCommodity.PK);
			AssertNotEquals(commodity.BY_ParentID, clonedCommodity.BY_ParentID);
			AssertCommodityDetails(clonedCommodity, ZGuid.Empty, ZString.Empty, ZGuid.Empty, ZString.Empty, ZDecimal.Zero, 0, "GOODS DESCRIPTION", "", ZDecimal.Zero, ZString.Empty, ZShort.Zero);
			AssertEquals(2, clonedCommodity.ChildCommodities.Count);
			var clonedChildCommodity1 = clonedCommodity.ChildCommodities[0];
			var clonedChildCommodity2 = clonedCommodity.ChildCommodities[1];
			if (clonedChildCommodity2.ChildCommodities.Count == 0)
			{
				clonedChildCommodity1 = clonedCommodity.ChildCommodities[1];
				clonedChildCommodity2 = clonedCommodity.ChildCommodities[0];
			}

			AssertNotEquals(childCommodity1.PK, clonedChildCommodity1.PK);
			AssertNotEquals(childCommodity1.BY_ParentID, clonedChildCommodity1.BY_ParentID);
			AssertCommodityDetails(clonedChildCommodity1, helper.Consignor.PK, Part.OP_PartNum, Part.PK, "1010.20.1030", ZDecimal.Zero, ZInt.Zero, ZString.Empty, ZString.Empty, ZDecimal.Zero, Part.OP_WeightUQ, ZShort.Zero);
			AssertEquals(0, clonedChildCommodity1.ChildCommodities.Count);
			AssertNotEquals(childCommodity2.PK, clonedChildCommodity2.PK);
			AssertNotEquals(childCommodity2.BY_ParentID, clonedChildCommodity2.BY_ParentID);
			AssertCommodityDetails(clonedChildCommodity2, helper.Consignor.PK, Part.OP_PartNum, Part.PK, ZString.Empty, ZDecimal.Zero, ZInt.Zero, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZShort.Zero);
			AssertEquals(1, clonedChildCommodity2.ChildCommodities.Count);
			var clonedChildCommodity2Child = clonedChildCommodity2.ChildCommodities[0];
			AssertNotEquals(childCommodity2Child.PK, clonedChildCommodity2Child.PK);
			AssertNotEquals(childCommodity2Child.BY_ParentID, clonedChildCommodity2Child.BY_ParentID);
			AssertCommodityDetails(clonedChildCommodity2Child, ZGuid.Empty, ZString.Empty, ZGuid.Empty, "1010.20.1030", ZDecimal.Zero, ZInt.Zero, ZString.Empty, ZString.Empty, ZDecimal.Zero, ZString.Empty, ZShort.Zero);
			AssertEquals(0, clonedChildCommodity2Child.ChildCommodities.Count);
		}

		void AssertMoveHeaders(CusInBondMoveHeaderCollection movementHeaders, CusInBondMoveHeaderCollection clonedMovementHeaders, Dictionary<ZGuid, ZGuid> billPKs)
		{
			AssertEquals(movementHeaders.Count, clonedMovementHeaders.Count);
			IComparer comparer = new CusInBondMoveHeaderComparer();
			movementHeaders.ApplySort(comparer);
			clonedMovementHeaders.ApplySort(comparer);
			for (int i = 0; i < movementHeaders.Count; i++)
			{
				var clonedMovementHeader = clonedMovementHeaders.FirstOrDefault(x => x.BM_InBondCarrierID == movementHeaders[i].BM_InBondCarrierID) as CusInBondMoveHeader;
				AssertNotNull(clonedMovementHeader);
				AssertMoveHeader(movementHeaders[i], clonedMovementHeader, billPKs);
			}
		}

		void AssertMoveHeader(CusInBondMoveHeader moveHeader, CusInBondMoveHeader clonedMoveHeader, Dictionary<ZGuid, ZGuid> billPKs)
		{
			AssertNotEquals(moveHeader.PK, clonedMoveHeader.PK);
			AssertNotEquals(moveHeader.BM_BH, clonedMoveHeader.BM_BH);
			AssertEquals(ZString.Empty, clonedMoveHeader.InBondNumber);
			AssertEquals(InbondCommonTypeList.Codes._1ImmediateTransport, clonedMoveHeader.BM_InBondEntryType);
			AssertEquals("MSCA", clonedMoveHeader.BM_InBondCarrierSCAC);
			AssertEquals("3965", clonedMoveHeader.BM_DestinationPortCode);
			AssertEquals("56894", clonedMoveHeader.BM_ForeignDestPortKCode);
			AssertEquals(ZDecimal.Zero, clonedMoveHeader.BM_MonetaryValue);
			AssertEquals(moveHeader.BM_InBondCarrierID, clonedMoveHeader.BM_InBondCarrierID);
			AssertEquals(YesNoDefaultList.Codes.Yes, clonedMoveHeader.BM_BTAIndicator);
			AssertEquals(ZString.Empty, clonedMoveHeader.BM_CustomsStatus);
			AssertEquals(ZDateTime.Empty, clonedMoveHeader.BM_ArrivalDate);
			AssertEquals(ZString.Empty, clonedMoveHeader.BM_WarehouseTransactionStatus);
			AssertEquals(ZDateTime.Empty, clonedMoveHeader.BM_InBondClosedDate);
			AssertMoveDetails(moveHeader.MovementDetails, clonedMoveHeader.MovementDetails, billPKs);
		}

		void AssertMoveDetails(CusInBondMoveDetailCollection movementDetails, CusInBondMoveDetailCollection clonedMovementDetails, Dictionary<ZGuid, ZGuid> billPKs)
		{
			AssertEquals(movementDetails.Count, clonedMovementDetails.Count);
			List<CusInBondMoveDetail> clonedList = new List<CusInBondMoveDetail>(clonedMovementDetails);
			foreach (CusInBondMoveDetail moveDetail in movementDetails)
			{
				ZGuid billPk = billPKs[moveDetail.B9_B0];
				var clonedMoveDetail = clonedList.Find(x => x.B9_B0 == billPk);
				if (clonedMoveDetail == null)
				{
					Fail("Cannont find corresponding ClonedMoveDeail");
				}
				else
				{
					clonedList.Remove(clonedMoveDetail);
					AssertMoveDetail(moveDetail, clonedMoveDetail);
				}
			}
		}

		void AssertMoveDetail(CusInBondMoveDetail moveDetail, CusInBondMoveDetail clonedMoveDetail)
		{
			AssertNotEquals(moveDetail.PK, clonedMoveDetail.PK);
			AssertNotEquals(moveDetail.B9_BM, clonedMoveDetail.B9_BM);
			AssertNotEquals(moveDetail.B9_B0, clonedMoveDetail.B9_B0);
			AssertEquals(ZString.Empty, clonedMoveDetail.B9_CustomsStatus);
			AssertEquals("PRE1234568", clonedMoveDetail.B9_PreviousITNumber);
			AssertEquals(19, clonedMoveDetail.B9_InBoundQty);
			AssertEquals("1STN", clonedMoveDetail.B9_FirstSecondaryNotifyParty);
			AssertEquals("2NDN", clonedMoveDetail.B9_SecondSecondaryNotifyParty);
			AssertEquals("3RDN", clonedMoveDetail.B9_ThirdSecondaryNotifyParty);
			AssertEquals("4THN", clonedMoveDetail.B9_FourthSecondaryNotifyParty);
			AssertEquals(ZString.Empty, clonedMoveDetail.B9_SeqNo);
			AssertEquals(moveDetail.Containers.Count, clonedMoveDetail.Containers.Count);
			AssertEquals(2, clonedMoveDetail.Containers.Count);
			var containers = moveDetail.Containers;
			var clonedContainers = clonedMoveDetail.Containers;
			AssertContainer(containers[0], clonedContainers[0]);
			AssertContainer(containers[1], clonedContainers[1]);
		}

		void AssertContainer(CusInBondContainer container, CusInBondContainer clonedContainer)
		{
			AssertEquals(ZString.Empty, clonedContainer.BC_ContainerNum);
			AssertNotEquals(container.PK, clonedContainer.PK);
			AssertNotEquals(container.BC_ParentID, clonedContainer.BC_ParentID);
			AssertEquals("B9", clonedContainer.BC_ParentTableCode.ToString());
			AssertEquals(ZString.Empty, clonedContainer.BC_ContainerNum);
			AssertEquals(ZString.Empty, clonedContainer.BC_Seal1);
			AssertEquals(ZString.Empty, clonedContainer.BC_Seal2);
			AssertEquals(ZGuid.Empty, clonedContainer.BC_RC);
			foreach (CusInBondCargoDesc commodity in clonedContainer.Commodities)
			{
				AssertCommodityDetails(commodity);
			}

			foreach (UNDGDataItem undg in clonedContainer.UNDGs)
			{
				AssertHazardousDetails(container.UNDGs[0], clonedContainer.UNDGs[0]);
			}
		}

		void AssertHazardousDetails(UNDGDataItem hazardous, UNDGDataItem clonedHazardous)
		{
			AssertNotEquals(hazardous.PK, clonedHazardous.PK);
			AssertNotEquals(hazardous.DI_ParentID, clonedHazardous.DI_ParentID);
			AssertEquals("BOB TECH", clonedHazardous.DI_TechnicalName);
		}

		void AssertCommodityDetails(CusInBondCargoDesc clonedCommodity)
		{
			AssertCommodityDetails(clonedCommodity, helper.Consignor.PK, Part.OP_PartNum, Part.PK, "1010.20.1030", ZDecimal.Zero, ZInt.Zero, "GOODS DESCRIPTION", ZString.Empty, ZDecimal.Zero, Part.OP_WeightUQ, ZShort.Zero);
		}

		void AssertCommodityDetails(CusInBondCargoDesc clonedCommodity, ZGuid supplierPK, ZString partNumber, ZGuid partPK, ZString tariff, ZDecimal monetaryValue, ZInt pieceCount, ZString description, ZString marksAndNumbers, ZDecimal weight, ZString weightUQ, ZShort warehouseEntryLineNo)
		{
			AssertEquals("clonedCommodity.BY_OH_Supplier", supplierPK, clonedCommodity.BY_OH_Supplier);
			AssertEquals("clonedCommodity.BY_PartNumber", partNumber, clonedCommodity.BY_PartNumber);
			AssertEquals("clonedCommodity.BY_OP_Part", partPK, clonedCommodity.BY_OP_Part);
			AssertEquals("clonedCommodity.BY_FormattedHarmonisedTariff", tariff, clonedCommodity.BY_FormattedHarmonisedTariff);
			AssertEquals("clonedCommodity.BY_MonetaryValue", monetaryValue, clonedCommodity.BY_MonetaryValue);
			AssertEquals("clonedCommodity.BY_PieceCount", pieceCount, clonedCommodity.BY_PieceCount);
			AssertEquals("clonedCommodity.BY_Description", description, clonedCommodity.BY_Description);
			AssertEquals("clonedCommodity.BY_MarksAndNumbers", marksAndNumbers, clonedCommodity.BY_MarksAndNumbers);
			AssertEquals("clonedCommodity.BY_GrossWeight", weight, clonedCommodity.BY_GrossWeight);
			AssertEquals("clonedCommodity.BY_GrossWeightUnit", weightUQ, clonedCommodity.BY_GrossWeightUnit);
			AssertEquals("clonedCommodity.BY_WarehouseEntryLineNo", warehouseEntryLineNo, clonedCommodity.BY_WarehouseEntryLineNo);
		}

		Dictionary<ZGuid, ZGuid> AssertBills(CusInBondBillCollection bills, CusInBondBillCollection clonedBills)
		{
			Dictionary<ZGuid, ZGuid> result = new Dictionary<ZGuid, ZGuid>();
			AssertEquals(bills.Count, clonedBills.Count);
			bills.ApplySort(CusInBondBill.Schema.B0_MasterBillNumber, ListSortDirection.Ascending);
			clonedBills.ApplySort(CusInBondBill.Schema.B0_MasterBillNumber, ListSortDirection.Ascending);
			for (int i = 0; i < bills.Count; i++)
			{
				var bill = bills[i];
				var clonedBill = clonedBills[i];
				AssertBill(bill, clonedBill);
				result.Add(bill.PK, clonedBill.PK);
			}

			return result;
		}

		void AssertBill(CusInBondBill bill, CusInBondBill clonedHill)
		{
			AssertNotEquals(bill.PK, clonedHill.PK);
			AssertNotEquals(bill.B0_BH, clonedHill.B0_BH);
			AssertEquals(ZString.Empty, clonedHill.B0_IssuerCode);
			AssertEquals(ZString.Empty, clonedHill.B0_MasterBillNumber);
			AssertEquals(ZInt.Zero, clonedHill.B0_ManifestQty);
			AssertEquals(ZString.Empty, clonedHill.B0_ManifestUQ);
			AssertEquals(ZDecimal.Zero, clonedHill.B0_Weight);
			AssertEquals(ZString.Empty, clonedHill.B0_WeightUQ);
			AssertEquals("53698", clonedHill.B0_PortOfLadingKCode);
			AssertEquals(ZDecimal.Zero, clonedHill.B0_Volume);
			AssertEquals(ZString.Empty, clonedHill.B0_VolumeUQ);
			AssertEquals("2705", clonedHill.B0_PlaceOfReceiptDCode);
			AssertForeignShipper(bill.ForeignShipper, clonedHill.ForeignShipper);
			AssertConsignee(bill.Consignee, clonedHill.Consignee);
			AssertNotifyParty(bill.NotifyParty, clonedHill.NotifyParty);
			AssertAdditionalReferences(bill.AdditionalReferences, clonedHill.AdditionalReferences);
		}

		void AssertAdditionalReferences(CusInbondBillAddRefCollection additionalReferences, CusInbondBillAddRefCollection clonedAdditionalReferences)
		{
			AssertEquals(additionalReferences.Count, clonedAdditionalReferences.Count);
			additionalReferences.ApplySort(CusInbondBillAddRef.Schema.BR_Qualifier, ListSortDirection.Ascending);
			clonedAdditionalReferences.ApplySort(CusInbondBillAddRef.Schema.BR_Qualifier, ListSortDirection.Ascending);
			for (int i = 0; i < additionalReferences.Count; i++)
			{
				AssertAdditionalReference(additionalReferences[i], clonedAdditionalReferences[i]);
			}
		}

		void AssertAdditionalReference(CusInbondBillAddRef additionalReference, CusInbondBillAddRef clonedAdditionalReference)
		{
			AssertNotEquals(additionalReference.PK, clonedAdditionalReference.PK);
			AssertNotEquals(additionalReference.BR_B0, clonedAdditionalReference.BR_B0);
			AssertEquals(additionalReference.BR_Qualifier, clonedAdditionalReference.BR_Qualifier);
			AssertEquals(additionalReference.BR_ReferenceNum, clonedAdditionalReference.BR_ReferenceNum);
		}

		void AssertNotifyParty(JobDocAddress notifyParty, JobDocAddress clonedNotifyParty)
		{
			AssertNotEquals(notifyParty.PK, clonedNotifyParty.PK);
			AssertNotEquals(notifyParty.E2_ParentID, clonedNotifyParty.E2_ParentID);
			AssertEquals(true, clonedNotifyParty.E2_AddressOverride);
			AssertEquals("NOTIFY PARTY COMPANY", clonedNotifyParty.E2_CompanyName);
			AssertEquals("NOTIFY PARTY ADDRESS 1", clonedNotifyParty.E2_Address1);
			AssertEquals("NOTIFY PARTY ADDRESS 2", clonedNotifyParty.E2_Address2);
			AssertEquals("CHICAGO", clonedNotifyParty.E2_City);
			AssertEquals("NOTIFER THE BUILDER", clonedNotifyParty.E2_Contact);
			AssertEquals("NOTIFER@BUILDER.COM", clonedNotifyParty.E2_Email);
			AssertEquals("61022", clonedNotifyParty.E2_Postcode);
			AssertEquals(Core.Constants.CountryCodes.Italy, clonedNotifyParty.E2_RN_NKCountryCode);
		}

		void AssertConsignee(JobDocAddress consignee, JobDocAddress clonedConsignee)
		{
			AssertNotEquals(consignee.PK, clonedConsignee.PK);
			AssertNotEquals(consignee.E2_ParentID, clonedConsignee.E2_ParentID);
			AssertEquals(false, clonedConsignee.E2_AddressOverride);
			AssertEquals(helper.Consignee.MainAddress.PK, clonedConsignee.E2_OA_Address);
		}

		void AssertForeignShipper(JobDocAddress foreignShipper, JobDocAddress clonedForeignShipper)
		{
			AssertNotEquals(foreignShipper.PK, clonedForeignShipper.PK);
			AssertNotEquals(foreignShipper.E2_ParentID, clonedForeignShipper.E2_ParentID);
			AssertEquals(true, clonedForeignShipper.E2_AddressOverride);
			AssertEquals("FOREIGN SHIPPER COMPANY", clonedForeignShipper.E2_CompanyName);
			AssertEquals("FOREIGN SHIPPER ADDRESS 1", clonedForeignShipper.E2_Address1);
			AssertEquals("FOREIGN SHIPPER ADDRESS 2", clonedForeignShipper.E2_Address2);
			AssertEquals("SYDNEY", clonedForeignShipper.E2_City);
			AssertEquals("BOB THE BUILDER", clonedForeignShipper.E2_Contact);
			AssertEquals("BOB@BUILDER.COM", clonedForeignShipper.E2_Email);
			AssertEquals("+61 (2) 8456 6846", clonedForeignShipper.E2_Fax);
			AssertEquals("+61 (2) 8456 6855", clonedForeignShipper.E2_Phone);
			AssertEquals("2214", clonedForeignShipper.E2_Postcode);
			AssertEquals(Core.Constants.CountryCodes.Australia, clonedForeignShipper.E2_RN_NKCountryCode);
			AssertEquals("NSW", clonedForeignShipper.E2_State);
			AssertEquals("+61 403 112 456", clonedForeignShipper.E2_Mobile);
		}

		void AssertHeader(CusInBondHeader header, CusInBondHeader clonedHeader)
		{
			AssertNotEquals(header.PK, clonedHeader.PK);
			AssertNotEquals("BND123", clonedHeader.BH_JobReference);
			AssertEquals(GlbBranch.CurrentBranch.PK, clonedHeader.BH_GB);
			AssertEquals("Set to Empty when FTZ move", ZString.Empty, clonedHeader.BH_CarrierSCAC);
			AssertEquals("30", clonedHeader.BH_ImportTransportMode);
			AssertEquals("AU", clonedHeader.BH_ImportConveyanceCountry);
			AssertEquals("APL VESSEL", clonedHeader.BH_ImportConveyanceName);
			AssertEquals("V3234", clonedHeader.BH_VoyageNumber);
			AssertEquals("3790", clonedHeader.BH_PortUnladingDCode);
			AssertEquals(ZDateTime.Empty, clonedHeader.BH_ETA);
			AssertEquals(ZBool.True, clonedHeader.BH_FTZMove);
			AssertEquals("DFDA", clonedHeader.BH_FIRMS);
		}

		CusInBondHeader GetHeaderToClone()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_JobReference = "BND123";
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			header.BH_CarrierSCAC = "APDA";
			header.BH_ImportTransportMode = "30";
			header.BH_ImportConveyanceCountry = "AU";
			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "V3234";
			header.BH_PortUnladingDCode = "3790";
			header.BH_ETA = new ZDateTime(2010, 3, 2);
			header.BH_FTZMove = ZBool.True;
			header.BH_FIRMS = "DFDA";
			var bill1 = AddBill(header, "MB1232112");
			var bill2 = AddBill(header, "MB3695854");
			CusInBondMoveHeader moveHeader1 = AddMoveHeader(header, "INB323423", "9685476633");
			AddMoveDetail(moveHeader1, bill1);
			AddMoveDetail(moveHeader1, bill2);
			CusInBondMoveHeader moveHeader2 = AddMoveHeader(header, "INB569875", "9685476634");
			AddMoveDetail(moveHeader2, bill1);
			return header;
		}

		void AddMoveDetail(CusInBondMoveHeader moveHeader, CusInBondBill bill)
		{
			var warehouseEntryNumber = bill.B0_MasterBillNumber;
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			moveDetail.B9_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			moveDetail.B9_PreviousITNumber = "PRE1234568";
			moveDetail.B9_InBoundQty = 19;
			moveDetail.B9_FirstSecondaryNotifyParty = "1STN";
			moveDetail.B9_SecondSecondaryNotifyParty = "2NDN";
			moveDetail.B9_ThirdSecondaryNotifyParty = "3RDN";
			moveDetail.B9_FourthSecondaryNotifyParty = "4THN";
			moveDetail.B9_SeqNo = moveHeader.MovementDetails.Count.ToString();
			CusInBondContainer container1 = AddContainer(moveDetail, "TURE1234567", "SEAL1", "SEAL2", helper.Container20US.PK);
			AddCommodityDetails(container1, warehouseEntryNumber);
			AddCommodityDetails(container1, warehouseEntryNumber);
			AddHazardousDetails(container1);
			AddHazardousDetails(container1);
			CusInBondContainer container2 = AddContainer(moveDetail, "NC", "", "", ZGuid.Empty);
			AddCommodityDetails(container2, warehouseEntryNumber);
			AddHazardousDetails(container2);
		}

		void AddHazardousDetails(CusInBondContainer container)
		{
			UNDGDataItem hazardous = container.UNDGs.AddNew();
			hazardous.DI_TechnicalName = "BOB TECH";
		}

		void AddCommodityDetails(CusInBondContainer container, ZString warehouseEntryNumber)
		{
			PopulateCommodityDetails(container.Commodities.AddNew(), warehouseEntryNumber);
		}

		CusInBondCargoDesc PopulateCommodityDetails(CusInBondCargoDesc commodity, ZString warehouseEntryNumber)
		{
			commodity.BY_OH_Supplier = helper.Consignor.PK;
			commodity.BY_PartNumber = Part.OP_PartNum;
			commodity.BY_MonetaryValue = 1502m;
			commodity.BY_PieceCount = 42;
			commodity.BY_MarksAndNumbers = "MARKS ON THE GOODS";
			commodity.BY_GrossWeight = 49m;
			commodity.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			commodity.BY_WarehouseEntryNumber = warehouseEntryNumber;
			commodity.BY_WarehouseEntryLineNo = 1;
			return commodity;
		}

		US.Business.OrgSupplierPart part;
		US.Business.OrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<US.Business.OrgSupplierPart>();
					part.OP_PartNum = "OP1212@";
					part.OP_Desc = "GOODS DESCRIPTION";
					var relatedOrg = part.RelatedOrganisations.AddSupplier(helper.Consignor);
					var pivot = part.PivotsForBinding.AddNew();
					pivot.CI_OH = relatedOrg.OU_OH;
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
					pivot.CI_TariffNum = "1010201030";
				}

				return part;
			}
		}

		CusInBondContainer AddContainer(CusInBondMoveDetail moveDetail, ZString containerNo, ZString seal1, ZString seal2, ZGuid containerType)
		{
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = containerNo;
			container.BC_Seal1 = seal1;
			container.BC_Seal2 = seal2;
			container.BC_RC = containerType;
			return container;
		}

		CusInBondMoveHeader AddMoveHeader(CusInBondHeader header, ZString inBondNumber, string carrierID = "9685476632")
		{
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.InBondNumber = inBondNumber;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.BM_InBondCarrierSCAC = "MSCA";
			moveHeader.BM_DestinationPortCode = "3965";
			moveHeader.BM_ForeignDestPortKCode = "56894";
			moveHeader.BM_MonetaryValue = 15000m;
			moveHeader.BM_InBondCarrierID = carrierID;
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			moveHeader.BM_InBondClosedDate = ZDateTime.BrettsBirthday;
			return moveHeader;
		}

		CusInBondBill AddBill(CusInBondHeader header, ZString masterBill)
		{
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "BLOD";
			bill.B0_MasterBillNumber = masterBill;
			bill.B0_ManifestQty = 15;
			bill.B0_ManifestUQ = "PK";
			bill.B0_Weight = 130;
			bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			bill.B0_PortOfLadingKCode = "53698";
			bill.B0_Volume = 14;
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.B0_PlaceOfReceiptDCode = "2705";
			var foreignShipper = bill.ForeignShipper;
			foreignShipper.E2_AddressOverride = true;
			foreignShipper.E2_CompanyName = "FOREIGN SHIPPER COMPANY";
			foreignShipper.E2_Address1 = "FOREIGN SHIPPER ADDRESS 1";
			foreignShipper.E2_Address2 = "FOREIGN SHIPPER ADDRESS 2";
			foreignShipper.E2_City = "SYDNEY";
			foreignShipper.E2_Contact = "BOB THE BUILDER";
			foreignShipper.E2_Email = "BOB@BUILDER.COM";
			foreignShipper.E2_Fax = "+61 (2) 8456 6846";
			foreignShipper.E2_Phone = "+61 (2) 8456 6855";
			foreignShipper.E2_Postcode = "2214";
			foreignShipper.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			foreignShipper.E2_State = "NSW";
			foreignShipper.E2_Mobile = "+61 403 112 456";
			bill.Consignee.E2_OA_Address = helper.Consignee.MainAddress.PK;
			var notifyParty = bill.NotifyParty;
			notifyParty.E2_AddressOverride = true;
			notifyParty.E2_CompanyName = "NOTIFY PARTY COMPANY";
			notifyParty.E2_Address1 = "NOTIFY PARTY ADDRESS 1";
			notifyParty.E2_Address2 = "NOTIFY PARTY ADDRESS 2";
			notifyParty.E2_City = "CHICAGO";
			notifyParty.E2_Contact = "NOTIFER THE BUILDER";
			notifyParty.E2_Email = "NOTIFER@BUILDER.COM";
			notifyParty.E2_Postcode = "61022";
			notifyParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			var additionalReference = bill.AdditionalReferences.AddNew();
			additionalReference.BR_Qualifier = ReferenceQualifierList.Codes.CN;
			additionalReference.BR_ReferenceNum = "CN2342";
			additionalReference.BR_Qualifier = ReferenceQualifierList.Codes.CX;
			additionalReference.BR_ReferenceNum = "CX86854";
			return bill;
		}

		DeclarationTestHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new DeclarationTestHelper(Factory);
		}
	}
}

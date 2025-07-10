using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	static class BlockBuilderHelper
	{
		internal static ASESE10 GenerateSE10(UpdateActionCode action, IACECargoReleaseHeader header)
		{
			var se10 = new ASESE10();
			se10.UpdateActionCode = UpdateActionCodeConverter.ConvertToString(action);
			se10.EntryFilerCode = header.EntryFilerCode;

			var entryNumber = ((ICusEntryHeader)header).EntryNumber;
			se10.EntryNumber = entryNumber.IsEmpty ? MQEDIMessage.USEntryNumberPlaceHolder : entryNumber.ToString();
			se10.EntryType = header.EntryType;
			se10.ImporterOfRecordType = header.ImporterOfRecordType;
			se10.ImporterOfRecord = header.ImporterOfRecordNumber;
			se10.ModeOfTransportationMOTCode = header.ModeOfTransportationCode;
			se10.BondTypeCode = header.BondType;
			se10.EstimatedEntryValue = header.TotalValueOfEntrySummary.Round(0);
			se10.PlannedPortOfEntry = header.DistrictPortOfEntry;
			se10.PortOfUnlading = header.PortOfUnlading;
			se10.SplitShipmentReleaseCode = header.SplitShipmentReleaseCode;
			return se10;
		}

		internal static void GenerateBillBlocks(ZString entryType, List<MessageBlock> blocks, IEnumerable<IBillDetails> bills, string scacCodeToPrepend, ZString modeOfTransportationCode)
		{
			foreach (IBillDetails bill in bills)
			{
				if (EntryTypeList.IsCargoManifestGroupingAllowed(entryType, modeOfTransportationCode))
				{
					GenerateForBill(blocks, bill, scacCodeToPrepend);
				}
				else if (EntryTypeList.IsInbondCargoManifestGroupAllowed(entryType))
				{
					GenerateInbondNumberBlock(blocks, bill);
				}
			}
		}

		static void GenerateForBill(List<MessageBlock> blocks, IBillDetails bill, string scacCodeToPrepend)
		{
			bool se16Required = bill.IsSE16Required();

			if (!bill.ITNumber.IsEmpty)
			{
				blocks.Add(GenerateSE15(SEBillTypesList.Codes.InBond, ZString.Empty, bill.ITNumber, ZInt.Zero, false));
			}

			if (!bill.MasterBillNumber.IsEmpty)
			{
				var isMasterBillLowestBill = IsLowestBill(bill.HouseBillNumber, se16Required);
				var masterBillQty = isMasterBillLowestBill && !se16Required ? bill.PackageQuantity : ZInt.Zero;
				var billType = bill.HouseBillNumber.IsEmpty ? (bill.IsExpressTracking ? SEBillTypesList.Codes.ExpressTracking : SEBillTypesList.Codes.RegularBill) : SEBillTypesList.Codes.MasterBill;

				var masterBillNumberToReport = scacCodeToPrepend + bill.MasterBillNumber;
				blocks.Add(GenerateSE15(billType, bill.IssuerCodeOfMasterBillNumber, masterBillNumberToReport, masterBillQty, bill.IsNonAMS));
			}

			if (!bill.HouseBillNumber.IsEmpty)
			{
				var isHouseBillLowestBill = IsLowestBill(bill.SubHouseBillNumber, se16Required);
				var houseBillQty = isHouseBillLowestBill && !se16Required ? bill.PackageQuantity : ZInt.Zero;
				blocks.Add(GenerateSE15(SEBillTypesList.Codes.HouseBill, bill.IssuerCodeOfHouseBillNumber, bill.HouseBillNumber, houseBillQty, bill.IsNonAMS));
			}

			if (!bill.SubHouseBillNumber.IsEmpty)
			{
				var isSubHouseBillLowestBill = IsLowestBill(ZString.Empty, se16Required);
				var subHouseBillQty = isSubHouseBillLowestBill && !se16Required ? bill.PackageQuantity : ZInt.Zero;
				blocks.Add(GenerateSE15(SEBillTypesList.Codes.SubHouseBill, bill.IssuerCodeOfSubHouseBillNumber, bill.SubHouseBillNumber, subHouseBillQty, bill.IsNonAMS));
			}

			blocks.AddRange(GenerateSplitOrNonAMSDetailsAndContainers(bill));
		}

		static void GenerateInbondNumberBlock(List<MessageBlock> blocks, IBillDetails bill)
		{
			if (!bill.ITNumber.IsEmpty)
			{
				blocks.Add(GenerateSE15(SEBillTypesList.Codes.InBond, ZString.Empty, bill.ITNumber, bill.PackageQuantity, false));
			}
		}

		static bool IsLowestBill(ZString lowerBillNumber, bool se16Required)
		{
			return lowerBillNumber.IsEmpty && !se16Required;
		}

		static ASESE15 GenerateSE15(ZString billType, ZString issuer, ZString billNo, ZInt qty, ZBool nonAMS)
		{
			var se15 = new ASESE15();
			se15.BillTypeIndicator = billType;
			se15.IssuerCodeOfBillOfLadingNumber = issuer;
			se15.BillOfLadingNumber = billNo;
			se15.Quantity = qty;
			se15.NonAMSIndicator = nonAMS ? "Y" : "N";
			return se15;
		}

		public static IEnumerable<MessageBlock> GenerateSplitOrNonAMSDetailsAndContainers(IBillDetails bill)
		{
			if (bill.IsSE16Required())
			{
				foreach (IConveyanceOrSplitDetails details in bill.ConveyanceOrSplitDetails)
				{
					yield return GenerateSE16(details);
				}
			}

			foreach (IContainer container in bill.Containers)
			{
				var se17 = new ASESE17();
				se17.EquipmentNumber = container.ContainerNumber;
				yield return se17;
			}
		}

		static ASESE16 GenerateSE16(IConveyanceOrSplitDetails splitBillDetails)
		{
			var se16 = new ASESE16();
			se16.CarrierCode = splitBillDetails.CarrierCode;
			se16.VoyageFlightTripManifestNumber = splitBillDetails.FlightNumber.Left(5);
			se16.DateOfArrival = splitBillDetails.ArrivalDate.Date;
			se16.Quantity = splitBillDetails.Qty;
			se16.UnitOfMeasure = splitBillDetails.UQ;
			se16.ConveyanceName = splitBillDetails.PipelineName;
			return se16;
		}

		internal static IEnumerable<MessageBlock> GenerateSE20Blocks(IACECargoReleaseHeader header)
		{
			yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber, header.DeclarationReferenceNumber);

			if (header.BondType == BondTypeList.Codes.SingleTransactionBond)
			{
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.SuretyCode, header.SuretyCode);
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.BondAmount, header.BondAmount.ToString());
			}
			if (header.IsExpressConsignment)
			{
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.ExpressConsignmentShipment, "Y");
			}
			if (header.KnownImporterIndicator)
			{
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.KnownImporterIndicator, "Y");
			}

			if (!header.RailReferenceNumber.IsEmpty)
			{
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.RailReferenceNumber, header.RailReferenceNumber);
			}

			if (header.IsPerishable)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.Perishable, "Y");
			}

			if (header.IsDomesticCargo)
			{
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.DomesticCargoIndicator, "Y");
			}

			if (ZZCustomsFunctionality.IsCargRlsCESEffective && !header.ConsolidatedFilterCodeAndEntryNumber.IsEmpty)
			{
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.ConsolidatedEntrySummaryNumber, header.ConsolidatedFilterCodeAndEntryNumber);
			}

			if (ZZCustomsFunctionality.IsEDA86Effective && !header.EstimatedDateOfArrivalForEntryType86.IsEmpty)
			{
				yield return GenerateSE20(ReferenceIdentifierCodeList.Codes.EstimatedDateOfArrival, ZString.Format("{0:MMddyy}", header.EstimatedDateOfArrivalForEntryType86));
			}

			if (header.IsSelfCertification)
			{
				yield return BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.SelfCertification, "Y");
			}
		}

		internal static ASESE20 GenerateSE20(ZString qualifier, ZString referenceIdentifier)
		{
			var se20 = new ASESE20();
			se20.ReferenceIdentifierQualifier = qualifier;
			se20.ReferenceIdentifier = referenceIdentifier;
			return se20;
		}

		internal static void GenerateSE20ForDISIndicator(List<MessageBlock> entryHeaderBlocks, ISEAdditionalData additionalData)
		{
			if (additionalData != null && additionalData.DISIndicator)
			{
				entryHeaderBlocks.Add(BlockBuilderHelper.GenerateSE20(ReferenceIdentifierCodeList.Codes.DISReferenceNumber, additionalData.DISIDRefNo));
			}
		}

		internal static ASESE31 GenerateSE31(ZString identifierType, ZString identifier)
		{
			var se31 = new ASESE31();
			se31.IdentifierType = identifierType;
			se31.Identifier = identifier;
			return se31;
		}

		internal static ASESE51 GenerateSE51(ZString identifierType, ZString identifier)
		{
			var se51 = new ASESE51();
			se51.IdentifierType = identifierType;
			se51.Identifier = identifier;
			return se51;
		}

		internal static ASESE60_01 GenerateSE60_01(ZString indicator)
		{
			var se60_01 = new ASESE60_01();
			se60_01.SanctionDisclaimIndicator = indicator;
			return se60_01;
		}

		internal static ASESE61 GenerateSE61Block(ZString ftzTariffNumber)
		{
			var se61 = new ASESE61();
			se61.CurrentHTSNumberForPFStatusMerchandise = ftzTariffNumber;
			return se61;
		}

		internal static IEnumerable<MessageBlock> BuildSanctionsAdditionalInfoGroupingBlocks(ICusEntryLine entryLine)
		{
			foreach (var sanctionInfo in entryLine.SanctionsAdditionalInfos)
			{
				yield return GenerateSE62Block(sanctionInfo.RecordID, sanctionInfo.RecordType, sanctionInfo.FieldName);
				yield return GenerateSE63Block(sanctionInfo.FieldValue);
			}
		}

		internal static ASESE62 GenerateSE62Block(ZString recordID, ZString recordType, ZString fieldName)
		{
			var se62 = new ASESE62();
			se62.RecordID = recordID;
			se62.RecordType = recordType;
			se62.FieldName = fieldName;
			return se62;
		}

		internal static ASESE63 GenerateSE63Block(ZString fieldValue)
		{
			var se63 = new ASESE63();
			se63.FieldValue = fieldValue;
			return se63;
		}
	}
}

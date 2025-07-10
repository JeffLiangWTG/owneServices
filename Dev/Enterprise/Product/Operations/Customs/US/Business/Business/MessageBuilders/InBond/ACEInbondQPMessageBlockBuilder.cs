using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEInbondQPMessageBlockBuilder
	{
		public ACEInbondQPMessageBlockBuilder(IInBondQPHeader inBondHeader)
		{
			this.inBondHeader = inBondHeader;
		}
		readonly IInBondQPHeader inBondHeader;

		public IEnumerable<MessageBlock> Build(InBondQPMessageType messageType, IInBondBillDetails moveDetail = null)
		{
			blocks = null;
			GenerateHeader(inBondHeader, messageType);

			if (messageType == InBondQPMessageType.Original)
			{
				var sortedBills = new List<IInBondBillDetails>(inBondHeader.Bills);
				sortedBills.Sort(new InBondBillDetailsComparer());
				short sequenceNumber = 1;

				foreach (var bill in sortedBills)
				{
					GenerateBillSegmentGroup(messageType, bill, sequenceNumber++);
				}
			}
			else if (messageType == InBondQPMessageType.BillLevelDelete)
			{
				GenerateBillSegmentGroup(messageType, moveDetail, 1);
			}
			return Blocks;
		}

		List<MessageBlock> Blocks
		{
			get { return blocks ?? (blocks = new List<MessageBlock>()); }
		}
		List<MessageBlock> blocks;

		#region Generate Header

		void GenerateHeader(IInBondQPHeader inBondHeader, InBondQPMessageType messageType)
		{
			GenerateQP10(inBondHeader, messageType);

			if (messageType != InBondQPMessageType.Delete && messageType != InBondQPMessageType.BillLevelDelete
				&& (inBondHeader.ImportTransportMode == TransportModeCodes.Codes.AirNonContainer || IsDetailedInBond(inBondHeader.Bills)))
			{
				GenerateQP20(inBondHeader);
			}
		}

		bool IsDetailedInBond(IEnumerable<IInBondBillDetails> bills)
		{
			foreach (IInBondBillDetails bill in bills)
			{
				if (bill.IsDetailedInBond)
				{
					return true;
				}
			}
			return false;
		}

		void GenerateQP10(IInBondQPHeader inBondHeader, InBondQPMessageType messageType)
		{
			INBQP10 result = new INBQP10();
			result.ActionCode = GetActionCode(messageType);

			result.InbondEntryType = inBondHeader.EntryType;
			ZString inBondNumber = inBondHeader.InBondNumber;

			result.InbondNumber = inBondNumber.IsEmpty ? inBondHeader.IsAir ? MQEDIMessage.AirInBondNumberPlaceHolder : MQEDIMessage.InBondNumberPlaceHolder : inBondNumber.ToString();
			result.CarrierCode = inBondHeader.InbondCarrierSCAC;

			if (messageType != InBondQPMessageType.Delete)
			{
				result.USPortOfDestination = inBondHeader.USDestination;

				if (inBondHeader.EntryType == EntryTypeList.Codes.TransportationExportation || inBondHeader.EntryType == EntryTypeList.Codes.ImmediateExportation)
				{
					result.PortOfForeignDestination = inBondHeader.ForeignDestination;
				}

				result.Value = ZInt.ParseSafe(inBondHeader.Value.ToString(0), 0);
				result.InbondCarrierID = inBondHeader.InBondCarrierID;
				result.BTAFDAIndicator = inBondHeader.BTAIndicator ? "Y" : "N";
				if (inBondHeader.FTZIndicator)
				{
					result.ForeignTradeZoneWarehouseIndicator = "Y";
					if (result.CarrierCode.IsEmpty)
					{
						result.CarrierCode = inBondHeader.FTZFirmsCode;
					}
				}
			}

			Blocks.Add(result);
		}

		string GetActionCode(InBondQPMessageType messageType)
		{
			if (messageType == InBondQPMessageType.Original)
			{
				return InBondActionList.AddInBond;
			}
			else if (messageType == InBondQPMessageType.Delete)
			{
				return InBondActionList.DeleteInBondFromAllAssociatedBills;
			}
			else
			{
				return InBondActionList.DeleteInBondFromBill;
			}
		}

		void GenerateQP20(IInBondQPHeader inBondHeader)
		{
			INBQP20 result = new INBQP20();
			result.CarrierCode = inBondHeader.ImportingCarrierSCAC;
			result.ModeOfTransportMOTCode = inBondHeader.ImportTransportMode;
			result.CountryCodeOfImportingCarrier = inBondHeader.ImportingCarrierCountryCode;
			result.ImportingConveyanceName = inBondHeader.ImportingConveyanceName.Left(23);
			result.VoyageFlightTripNumber = inBondHeader.ImportingCarrierVoyageNumber.Left(5);
			result.PortOfImportingConveyanceArrival = inBondHeader.PortOfUnlading;
			result.EstimatedDateOfArrival = inBondHeader.ETAatUnlading.IsValid ? inBondHeader.ETAatUnlading.Date : ZDate.Empty;
			if (inBondHeader.FTZIndicator)
			{
				result.ForeignTradeZoneFIRMSCode = inBondHeader.FTZFirmsCode;
				if (result.CarrierCode.IsEmpty)
				{
					result.CarrierCode = inBondHeader.FTZFirmsCode;
				}
			}

			Blocks.Add(result);
		}

		#endregion

		#region Generate Bill Segment Group

		internal void GenerateBillSegmentGroup(InBondQPMessageType messageType, IInBondBillDetails bill, short sequenceNumber)
		{
			string sequenceString = sequenceNumber.ToString().PadLeft(4, '0');
			GenerateQP30(bill, messageType == InBondQPMessageType.BillLevelDelete ? InBondActionList.DeleteBill : InBondActionList.AddBill, sequenceString);
			bill.SequenceNumber = sequenceString;

			if (messageType != InBondQPMessageType.BillLevelDelete)
			{
				GenerateQP32(bill);//Secondary Notify Parties
				GenerateQP33(bill);//Reference Number

				if (bill.IsDetailedInBond)
				{
					GenerateQP40(bill);
					GenerateParty<INBQP50, INBQP51, INBQP52>(bill.ForeignShipperAddress); //Foreign Shippper
					GenerateParty<INBQP55, INBQP56, INBQP57>(bill.ConsigneeAddress); //Consignee
					GenerateParty<INBQP60, INBQP61, INBQP62>(bill.NotifyPartyAddress); //Notify party
					GenerateContainersForBills(bill);
				}
			}
		}

		#endregion

		#region Bill Details

		void GenerateQP30(IInBondBillDetails bill, string actionCode, string sequenceNumber)
		{
			var result = new INBQP30();
			result.ActionCode = actionCode;
			result.SequenceNumber = sequenceNumber;
			result.IssuerCodeOfMasterBillOfLading = bill.MasterBillIssuerSCAC;

			if (inBondHeader.FTZIndicator && result.IssuerCodeOfMasterBillOfLading.IsEmpty)
			{
				result.IssuerCodeOfMasterBillOfLading = inBondHeader.InbondCarrierSCACOrFirms;
			}
			result.IssuerSequenceOfMasterBillOfLading = bill.MasterBillNumber.KeepAlphanumericCharacters().Left(12);
			result.HouseBillNumber = bill.HouseBillNumber.KeepAlphanumericCharacters().Left(12);
			result.IssuerCodeOfHouseBill = bill.HouseBillIssuerCode;

			if (actionCode != InBondActionList.DeleteBill)
			{
				if (bill.PreviousITType != EntryTypeList.Codes.ConsumptionFTZ && bill.PreviousITType != EntryTypeList.Codes.Warehouse && !inBondHeader.FTZIndicator)
				{
					result.PreviousInbondNumber = bill.PreviousITNumber;
				}
				result.InBondQuantity = new ZDecimal(bill.InBondQuantity);
			}
			Blocks.Add(result);
		}

		void GenerateQP32(IInBondBillDetails bill)
		{
			var snp1 = ZString.Empty;
			var snp2 = ZString.Empty;
			var snp3 = ZString.Empty;
			var snp4 = ZString.Empty;
			foreach (var snp in bill.SecondaryNotifyParties)
			{
				if (!snp.IsEmpty)
				{
					if (snp1.IsEmpty)
					{
						snp1 = snp;
					}
					else if (snp2.IsEmpty)
					{
						snp2 = snp;
					}
					else if (snp3.IsEmpty)
					{
						snp3 = snp;
					}
					else if (snp4.IsEmpty)
					{
						snp4 = snp;
						break;
					}
				}
			}
			if (!snp1.IsEmpty)
			{
				var result = new INBQP32();
				result.SecondaryNotifyPartyCode = snp1;
				result.SecondaryNotifyPartyCode1 = snp2;
				result.SecondaryNotifyPartyCode2 = snp3;
				result.SecondaryNotifyPartyCode3 = snp4;
				Blocks.Add(result);
			}
		}

		void GenerateQP33(IInBondBillDetails bill)
		{
			var refNumbers = new List<IInBondBillReferenceNumber>(bill.RefNumbers);
			refNumbers.Sort(new Comparison<IInBondBillReferenceNumber>((x, y) => x.Qualifier.CompareTo(y.Qualifier)));
			foreach (var refNo in refNumbers)
			{
				var qp33 = new INBQP33();
				qp33.Qualifier = refNo.Qualifier;
				qp33.ReferenceIdentifier = refNo.ReferenceIdentifier;
				Blocks.Add(qp33);
			}
		}

		void GenerateQP40(IInBondBillDetails bill)
		{
			INBQP40 result = new INBQP40();

			if (inBondHeader.FTZIndicator)
			{
				result.ForeignPortOfLading = InBondQPMessageBuilderConstants.FTZDefaultPOLCode;
			}
			else
			{
				result.ForeignPortOfLading = bill.ForeignLadingPortLocalCode;
			}

			result.ManifestQuantity = new ZDecimal(bill.ManifestQuantity);
			result.ManifestUnits = bill.ManifestUQ;
			result.Weight = bill.WeightInWholeNumber;
			result.WeightUnit = bill.WeightUQ;
			result.Volume = bill.VolumeInWholeNumber;
			if (!result.Volume.IsEmpty)
			{
				result.VolumeUnit = bill.VolumeUQ;
			}
			result.PlaceOfPrereceipt = bill.PlaceOfPreReceipt;
			Blocks.Add(result);
		}

		void GenerateParty<X, Y, Z>(JobDocAddress notifyParty)
			where X : MessageBlock, IQPFirstAddressSegment, new()
			where Y : MessageBlock, IQPSecondAddressSegment, new()
			where Z : MessageBlock, IQPPhoneAddressSegment, new()
		{
			if (notifyParty != null && !notifyParty.E2_CompanyName.IsEmpty)
			{
				var addressDetails = ConstructAddressDetails(notifyParty);
				GenerateFirstAddressSegment(new X(), notifyParty.E2_CompanyName, addressDetails.FirstAddressLine);
				GenerateSecondAddressSegment(new Y(), addressDetails.SecondAddressLine);
				GeneratePhoneAddressSegment(new Z(), notifyParty.E2_Phone);
			}
		}

		AddressDetails ConstructAddressDetails(JobDocAddress address)
		{
			ZString addressLine = address.E2_Address1 + " " + address.E2_Address2;
			ZString firstAddressLine = addressLine;
			if (firstAddressLine.IsEmpty)
			{
				firstAddressLine = address.E2_City.SubstringSafe(0, 30) + " " + address.E2_RN_NKCountryCode;
			}

			var secondAddressLine = firstAddressLine.SubstringSafe(35, 35);
			if (!addressLine.IsEmpty)
			{
				secondAddressLine += " " + address.E2_City.SubstringSafe(0, 30) + " " + address.E2_RN_NKCountryCode;
			}
			return new AddressDetails(firstAddressLine, secondAddressLine);
		}

		struct AddressDetails
		{
			public AddressDetails(ZString firstAddressLine, ZString secondAddressLine)
			{
				this.FirstAddressLine = firstAddressLine;
				this.SecondAddressLine = secondAddressLine;
			}

			public readonly ZString FirstAddressLine;
			public readonly ZString SecondAddressLine;
		}

		void GenerateFirstAddressSegment(IQPFirstAddressSegment segment, ZString companyName, ZString addressLine)
		{
			if (!addressLine.IsEmpty)
			{
				segment.CompanyName = companyName.SubstringSafe(0, 35);
				segment.AddressLine1 = addressLine.SubstringSafe(0, 35);
				Blocks.Add(segment as MessageBlock);
			}
		}

		void GenerateSecondAddressSegment(IQPSecondAddressSegment segment, ZString addressLine)
		{
			if (!addressLine.IsEmpty)
			{
				segment.AddressLine2 = addressLine.SubstringSafe(0, 35);
				segment.AddressLine3 = addressLine.SubstringSafe(35, 35);
				Blocks.Add(segment as MessageBlock);
			}
		}

		void GeneratePhoneAddressSegment(IQPPhoneAddressSegment segment, ZString phoneNumber)
		{
			if (!phoneNumber.IsEmpty)
			{
				segment.PhoneNumber = PhoneNumberCalculator.GetUnformattedPhoneNumber(phoneNumber, false);
				Blocks.Add(segment as MessageBlock);
			}
		}

		#endregion

		#region Container Details

		void GenerateContainersForBills(IInBondBillDetails bill)
		{
			foreach (var lineDetailsHeader in bill.LineDetailsHeaders)
			{
				GenerateContainerSegmentGroup(lineDetailsHeader);
			}
		}

		void GenerateContainerSegmentGroup(IInBondLineDetailsHeader lineDetailsHeader)
		{
			GenerateQP65(lineDetailsHeader);

			GenerateTariffLinesAndPieceDescriptionLines(lineDetailsHeader, lineDetailsHeader.TariffLines);

			GenerateHazardousMaterialLines(lineDetailsHeader.HazardousLines);
		}

		void GenerateQP65(IInBondLineDetailsHeader lineDetailsHeader)
		{
			var result = new INBQP65();

			var container = lineDetailsHeader.Container;
			if (container == null)
			{
				result.ContainerNumber = "NC";
			}
			else
			{
				result.ContainerNumber = container.ContainerNumber;
				result.SealNumber1 = container.SealNumber1;
				result.SealNumber2 = container.SealNumber2;
				result.ContainerDescriptionCode = container.ContainerDescriptionCode;
			}
			Blocks.Add(result);
		}

		#endregion

		#region Tariff Lines

		const int marksAndNumberLengthPer72 = 45;
		void GenerateTariffLinesAndPieceDescriptionLines(IInBondLineDetailsHeader lineDetailsHeader, IEnumerable<IInBondTariffLineDetails> tariffLines)
		{
			int numberOf72s = 0;
			foreach (IInBondTariffLineDetails tariffLine in tariffLines)
			{
				ZInt numberOf72sForThisTariffLine = (ZInt)Math.Ceiling((decimal)tariffLine.MarksAndNumbers.Length / marksAndNumberLengthPer72);

				numberOf72s += numberOf72sForThisTariffLine;

				if (numberOf72s > 999)
				{
					break;
				}
			}

			bool shouldLimitOne72PerTariffLine = numberOf72s > 999;
			bool firstFlag = true;
			foreach (var tariffLine in tariffLines)
			{
				GenerateQP70(lineDetailsHeader, tariffLine);

				GenerateCargoPackageAndDescriptionLines(tariffLine, lineDetailsHeader.Container, firstFlag);
				firstFlag = false;
				GenerateMarksAndNumbers(tariffLine, shouldLimitOne72PerTariffLine);
			}
		}

		void GenerateQP70(IInBondLineDetailsHeader lineDetailsHeader, IInBondTariffLineDetails tariffLine)
		{
			var multiClassifications = new List<IInBondTariffLineClassificationDetails>(tariffLine.MultiClassifications);
			if (multiClassifications.Count > 0)
			{
				multiClassifications.ForEach(x => GenerateQP70(x));
			}
			else
			{
				GenerateQP70(tariffLine);
			}
		}

		void GenerateQP70(IInBondTariffLineClassificationDetails tariffLine)
		{
			var result = new INBQP70();

			result.HarmonizedNumber = tariffLine.TariffNumber;
			result.Value = ZInt.ParseSafe(tariffLine.CustomsValue.ToString(0), 0);
			result.Weight = tariffLine.NetWeight.Round(0);
			result.WeightUnit = tariffLine.NetWeightUQ;

			Blocks.Add(result);
		}

		void GenerateCargoPackageAndDescriptionLines(IInBondTariffLineDetails tariffLine, IInBondContainer container, bool first = false)
		{
			if (tariffLine.PieceCount > 0 || !tariffLine.CargoDescription.IsEmpty)
			{
				var firstQP71 = new INBQP71();
				var totalCount = container != null ? (ZDecimal)container.PieceCount : ZDecimal.Zero;
				firstQP71.PieceCount = first && totalCount > 0 ? (ZDecimal)container.PieceCount : totalCount > 0 ? ZDecimal.Zero : tariffLine.PieceCount;
				firstQP71.ManifestUnitCode = tariffLine.ManifestUnitCode;
				firstQP71.Description = tariffLine.CargoDescription.Left(45);
				Blocks.Add(firstQP71);

				var remainingDesc = tariffLine.CargoDescription.SubstringSafe(45);

				while (!remainingDesc.IsEmpty)
				{
					var descToBuildWith = remainingDesc.Left(45);
					var additionalQP71 = new INBQP71();
					additionalQP71.Description = descToBuildWith;
					Blocks.Add(additionalQP71);
					remainingDesc = remainingDesc.SubstringSafe(45);
				}
			}
		}

		void GenerateMarksAndNumbers(IInBondTariffLineDetails tariffLine, bool shouldLimitOne72PerTariffLine)
		{
			var marksAndNumbers = shouldLimitOne72PerTariffLine ? new ZString[] { tariffLine.MarksAndNumbers.Left(marksAndNumberLengthPer72) } : tariffLine.MarksAndNumbers.Split(marksAndNumberLengthPer72);

			foreach (ZString one in marksAndNumbers)
			{
				INBQP72 qp72 = new INBQP72();
				qp72.MarksAndNumbers = one;
				Blocks.Add(qp72);
			}
		}

		#endregion

		#region Hazardous Material Lines

		void GenerateHazardousMaterialLines(IEnumerable<IHazardousMaterial> hazardousLines)
		{
			foreach (IHazardousMaterial hazardousLine in hazardousLines)
			{
				INBQP75 qp75 = new INBQP75();
				qp75.HazardousMaterialCode = hazardousLine.HazMatCode;
				qp75.HazardousMaterialClass = hazardousLine.HazMatClass;
				qp75.HazardousMaterialCodeQualifier = hazardousLine.HazMatQualifier;
				qp75.HazardousMaterialDescription = hazardousLine.HazMatDesc.Left(30);
				qp75.HazardousMaterialContact = hazardousLine.ContactName.Left(24);
				if (hazardousLine.IsFlashPointTempRelevant)
				{
					ZDecimal absoluteTemp = Math.Abs(hazardousLine.FlashPointTemp);
					qp75.FlashpointTemperature = ZInt.ParseSafe(absoluteTemp.Round(0).ToString(), 0);
					qp75.UnitOfMeasureCode = "CE";
					qp75.NegativeIndicator = hazardousLine.FlashPointTemp < 0m ? "N" : "";
				}
				Blocks.Add(qp75);

				if (!hazardousLine.HazMatDesc.SubstringSafe(30).IsEmpty ||
					!hazardousLine.HazMatClassificationDesc.IsEmpty)
				{
					INBQP76 qp76 = new INBQP76();
					qp76.HazardousMaterialDescription = hazardousLine.HazMatDesc.SubstringSafe(30, 29);
					qp76.HazardousMaterialClassification = hazardousLine.HazMatClassificationDesc.Left(30);
					Blocks.Add(qp76);
				}

				if (!hazardousLine.HazMatDesc.SubstringSafe(59).IsEmpty ||
					!hazardousLine.HazMatClassificationDesc.SubstringSafe(30).IsEmpty)
				{
					INBQP76 qp76 = new INBQP76();
					qp76.HazardousMaterialDescription = hazardousLine.HazMatDesc.SubstringSafe(59, 29);
					qp76.HazardousMaterialClassification = hazardousLine.HazMatClassificationDesc.SubstringSafe(30, 30);
					Blocks.Add(qp76);
				}
			}
		}

		#endregion
	}
}

// Tested in C:\Dev\Enterprise\Product\Operations\Customs\US\InBond\Business\BusinessObject\MessageAction\InBondMessageSendingObject.cs

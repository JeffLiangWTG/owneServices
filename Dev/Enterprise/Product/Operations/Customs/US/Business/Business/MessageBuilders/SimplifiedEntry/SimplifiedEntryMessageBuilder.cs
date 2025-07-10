using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	public class SimplifiedEntryMessageBuilder : EntryHeaderMessageBuilder<ACEInputBlockControlGenerator>
	{
		public SimplifiedEntryMessageBuilder(IACECargoReleaseHeader header, UpdateActionCode action, ISEAdditionalData additionalData)
			: base(header, action)
		{
			this.additionalData = additionalData;
		}
		readonly ISEAdditionalData additionalData;

		protected override string ApplicationIdentifier
		{
			get { return ACEApplicationIdentifierCodeList.Codes.CargoRelease; }
		}

		protected override ACEInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			return new ACEInputBlockControlGenerator(entryHeader);
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			switch (action)
			{
				case UpdateActionCode.Delete:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;
					break;
				case UpdateActionCode.Replace:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
					break;
				case UpdateActionCode.Update:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
					break;
				default:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
					break;
			}
		}

		protected override void UpdateMessageBlocks(ACEInputBlockControlGenerator block)
		{
			block.MessageBlocks.AddRange(new SimplifiedEntryBlockBuilder(entryHeader, additionalData, true).Build(action));

			if (entryHeader.IsRemoteLocationFiling)
			{
				block.B.RemotePreparerDistrictPortCode = entryHeader.PreparerDistrictPort;
				block.B.RemotePreparerFilerCode = block.B.FilerCode;
				block.B.RemotePreparerOfficeCode = entryHeader.PreparerOfficeCode;
				block.B.RemotelyFiledIndicator = "1";
			}
		}

		protected override List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			var supportedList = base.GetSupportedUpdateActionCodeList();

			supportedList.Add(UpdateActionCode.Add);
			supportedList.Add(UpdateActionCode.Delete);
			supportedList.Add(UpdateActionCode.Replace);
			supportedList.Add(UpdateActionCode.Update);

			return supportedList;
		}

		protected new IACECargoReleaseHeader entryHeader
		{
			get { return (IACECargoReleaseHeader)base.entryHeader; }
		}
	}

	public class SimplifiedEntryBlockBuilder
	{
		public SimplifiedEntryBlockBuilder(IACECargoReleaseHeader header, ISEAdditionalData additionalData, bool enablePGATracking = false)
		{
			this.header = header;
			this.additionalData = additionalData;
			this.enablePGATracking = enablePGATracking;
		}
		protected readonly IACECargoReleaseHeader header;
		protected readonly ISEAdditionalData additionalData;
		readonly bool enablePGATracking;

		public IEnumerable<MessageBlock> Build(UpdateActionCode action)
		{
			var entryHeaderBlocks = new List<MessageBlock>();
			if (action == UpdateActionCode.Delete)
			{
				var lastMsgBlock = header.LastCRAcceptedMessageBlock;
				if (lastMsgBlock != null)
				{
					entryHeaderBlocks.Add(GenerateSE10FromSentMessage(lastMsgBlock));
				}
				else
				{
					entryHeaderBlocks.Add(BlockBuilderHelper.GenerateSE10(action, header));
				}
			}
			else
			{
				entryHeaderBlocks.Add(BlockBuilderHelper.GenerateSE10(action, header));
				var se11 = GenerateSE11();
				if (!se11.IsEmpty)
				{
					entryHeaderBlocks.Add(se11);
				}
				entryHeaderBlocks.AddRange(BuildBondDetails());
			}

			if (additionalData != null)
			{
				entryHeaderBlocks.Add(GenerateSE13());
			}

			if (action != UpdateActionCode.Delete)
			{
				var scacCodeForFIX = header.ModeOfTransportationCode == TransportModeCodes.Codes.FixedTransportInstallations ? header.CarrierCode : ZString.Empty;
				BlockBuilderHelper.GenerateBillBlocks(header.EntryType, entryHeaderBlocks, header.LowestBillDetails, scacCodeForFIX, header.ModeOfTransportationCode);

				entryHeaderBlocks.AddRange(BlockBuilderHelper.GenerateSE20Blocks(header));
				if (action != UpdateActionCode.Add)
				{
					BlockBuilderHelper.GenerateSE20ForDISIndicator(entryHeaderBlocks, additionalData);
				}

				if (action != UpdateActionCode.Update)
				{
					entryHeaderBlocks.AddRange(GenerateHeaderEntityBlocks(header.Entities));
					foreach (ISimplifiedEntryLine entryLine in header.EntryLines)
					{
						if (entryLine.ZoneStatus != ZoneStatusList.Codes.Domestic)
						{
							GenerateEntryLine(entryLine, entryHeaderBlocks);
						}
					}
				}
			}
			else
			{
				if (additionalData != null)
				{
					entryHeaderBlocks.Add(BlockBuilderHelper.GenerateSE20(additionalData.ReferenceIdentifierQualifier, additionalData.ReferenceIdentifier));
					BlockBuilderHelper.GenerateSE20ForDISIndicator(entryHeaderBlocks, additionalData);
				}
			}

			return entryHeaderBlocks;
		}

		ASESE10 GenerateSE10FromSentMessage(ASESE10 lastAcceptedMessage, UpdateActionCode action = UpdateActionCode.Delete)
		{
			var block10 = new ASESE10();

			if (lastAcceptedMessage != null)
			{
				block10.UpdateActionCode = UpdateActionCodeConverter.ConvertToString(action);
				block10.EntryFilerCode = lastAcceptedMessage.EntryFilerCode;

				block10.EntryNumber = lastAcceptedMessage.EntryNumber;
				block10.EntryType = lastAcceptedMessage.EntryType;
				block10.ImporterOfRecordType = lastAcceptedMessage.ImporterOfRecordType;
				block10.ImporterOfRecord = lastAcceptedMessage.ImporterOfRecord;
				block10.ModeOfTransportationMOTCode = lastAcceptedMessage.ModeOfTransportationMOTCode;
				block10.BondTypeCode = lastAcceptedMessage.BondTypeCode;
				block10.EstimatedEntryValue = lastAcceptedMessage.EstimatedEntryValue;
				block10.PlannedPortOfEntry = lastAcceptedMessage.PlannedPortOfEntry;
				block10.PortOfUnlading = lastAcceptedMessage.PortOfUnlading;
				block10.SplitShipmentReleaseCode = lastAcceptedMessage.SplitShipmentReleaseCode;
			}
			return block10;
		}

		ASESE11 GenerateSE11()
		{
			var se11 = new ASESE11();
			se11.EntryDateElectionCode = header.EntryDateElectionCode;
			se11.ElectedEntryDate = header.ElectedEntryDate;
			se11.LocationOfGoodsFIRMS = EntryTypeList.IsWarehouseType(header.EntryType) ? header.CurrentFirmsCodeForWarehousingEntry : header.LocationOfGoods;
			se11.ElectedExamSiteFIRMS = header.ElectedExamSite;
			se11.VoyageFlightTripManifestNumber = header.VoyageNumber;
			se11.GeneralOrderGONumber = header.GeneralOrderNumber;
			se11.CBPBondedWarehouseFIRMS = header.CBPBondedWarehouseFIRMS;
			se11.OriginatingWarehouseEntryFilerCode = header.EntryFilerCodeOfWarehouseEntry;
			se11.OriginatingWarehouseEntryNumber = header.WarehouseEntryNumber;
			var conveyanceNameOrFTZNumber = ZString.Empty;
			if (header.EntryType == EntryTypeList.Codes.ConsumptionFTZ)
			{
				conveyanceNameOrFTZNumber = !header.ImportFTZNumber.IsEmpty ? "FTZ" + header.ImportFTZNumber : string.Empty;
			}
			else
			{
				conveyanceNameOrFTZNumber = header.ImportingVesselName;
			}

			se11.ConveyanceNameOrFTZZoneID = conveyanceNameOrFTZNumber;
			se11.ImmediateDeliveryIndicator = header.ImmediateDelivery ? "Y" : string.Empty;

			return se11;
		}

		IEnumerable<MessageBlock> BuildBondDetails()
		{
			var aDDCVDBondType = header.ADDCVDBondType;
			if (!aDDCVDBondType.IsEmpty && aDDCVDBondType == BondTypeList.Codes.SingleTransactionBond)
			{
				yield return GenerateSE12(aDDCVDBondType, "A", header.ADDCVDSuretyCode, header.ADDCVDSingleTransactionBondAmount, header.ADDCVDSingleTransactionBondAccNo);
			}
		}

		MessageBlock GenerateSE12(ZString bondType, ZString designation, ZString suretyCode, ZDecimal singleBondAmount, ZString bondProducerAccNo)
		{
			var result = new ASESE12();

			result.BondTypeCode = bondType;
			result.BondDesignationTypeCode = designation;
			result.SuretyCompanyCode = suretyCode;
			result.SingleTransactionBondAmount = Math.Ceiling(singleBondAmount);
			result.SingleTransactionBondProducerAccountNumber = bondProducerAccNo;

			return result;
		}

		ASESE13 GenerateSE13()
		{
			var se13 = new ASESE13();
			se13.ContactName = additionalData.ContactName;
			se13.ContactPhone = additionalData.ContactPhone;
			se13.ReasonCode = additionalData.ReasonCode;
			se13.MultipleCargoDispositionsIndicator = additionalData.MultipleCargoDispositionsIndicator;
			se13.DISIndicator = additionalData.DISIndicator ? "1" : string.Empty;
			se13.SplitShipmentIndicator = header.IsSplitShipment ? "1" : string.Empty;
			return se13;
		}

		internal IEnumerable<MessageBlock> GenerateHeaderEntityBlocks(IEnumerable<ISimplifiedEntryOrganisationDetails> entities)
		{
			CalculateEntitySendingFlags(entities);
			foreach (ISimplifiedEntryOrganisationDetails address in entities)
			{
				if (address.EntityCode == EntityCodeList.Codes.SellingParty && shouldSendSellerOnHeaderLevel ||
					(address.EntityCode == EntityCodeList.Codes.ManufacturerSupplier && shouldSendManufacturerOnHeaderLevel) ||
					(address.EntityCode == EntityCodeList.Codes.Consignee && shouldSendConsigneeOnHeaderLevel) ||
					(address.EntityCode == EntityCodeList.Codes.BuyingParty && shouldSendBuyingPartyOnHeaderLevel) ||
					(address.EntityCode == EntityCodeList.Codes.ShipToParty && shouldSendShipToPartyOnHeaderLevel) ||
					(address.EntityCode == EntityCodeList.Codes.Exporter && shouldSendExporterOnHeaderLevel && gBIIsActive) ||
					(address.EntityCode == EntityCodeList.Codes.Shipper && shouldSendShipperOnHeaderLevel && gBIIsActive) ||
					(address.EntityCode == EntityCodeList.Codes.Distributor && shouldSendDistributorOnHeaderLevel && gBIIsActive) ||
					(address.EntityCode == EntityCodeList.Codes.Packager && shouldSendPackagerOnHeaderLevel && gBIIsActive))
				{
					var block30 = GenerateSE30(address);
					yield return block30;

					if (gBIIsActive)
					{
						foreach (var block31 in GenerateSE31Blocks(address))
						{
							yield return block31;
						}
					}

					if (!block30.EntityName.IsEmpty)
					{
						yield return GenerateSE35(address);
						yield return GenerateSE36(address);
					}
				}
			}
		}
		bool shouldSendSellerOnHeaderLevel;
		bool shouldSendManufacturerOnHeaderLevel;
		bool shouldSendConsigneeOnHeaderLevel;
		bool shouldSendBuyingPartyOnHeaderLevel;
		bool shouldSendShipToPartyOnHeaderLevel;
		bool shouldSendExporterOnHeaderLevel;
		bool shouldSendShipperOnHeaderLevel;
		bool shouldSendDistributorOnHeaderLevel;
		bool shouldSendPackagerOnHeaderLevel;
		readonly bool gBIIsActive = ZZCustomsFunctionality.GBIACTIVE;

		void CalculateEntitySendingFlags(IEnumerable<ISimplifiedEntryOrganisationDetails> entities)
		{
			List<ISimplifiedEntryOrganisationDetails> manufacturersFromLines = null;
			if (HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.ManufacturerSupplier))
			{
				manufacturersFromLines = new List<ISimplifiedEntryOrganisationDetails>();
			}
			List<ISimplifiedEntryOrganisationDetails> sellersFromLines = null;
			if (HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.SellingParty))
			{
				sellersFromLines = new List<ISimplifiedEntryOrganisationDetails>();
			}
			List<ISimplifiedEntryOrganisationDetails> consigneeFromLines = null;
			if (HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.Consignee))
			{
				consigneeFromLines = new List<ISimplifiedEntryOrganisationDetails>();
			}
			List<ISimplifiedEntryOrganisationDetails> buyersFromLines = null;
			if (HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.BuyingParty))
			{
				buyersFromLines = new List<ISimplifiedEntryOrganisationDetails>();
			}
			List<ISimplifiedEntryOrganisationDetails> shipToPartiesFromLines = null;
			if (HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.ShipToParty))
			{
				shipToPartiesFromLines = new List<ISimplifiedEntryOrganisationDetails>();
			}
			if (sellersFromLines != null || manufacturersFromLines != null || consigneeFromLines != null || buyersFromLines != null || shipToPartiesFromLines != null)
			{
				foreach (ISimplifiedEntryLine line in header.EntryLines)
				{
					AddEntityForCalculation(line, EntityCodeList.Codes.ManufacturerSupplier, manufacturersFromLines);
					AddEntityForCalculation(line, EntityCodeList.Codes.SellingParty, sellersFromLines);
					AddEntityForCalculation(line, EntityCodeList.Codes.Consignee, consigneeFromLines);
					AddEntityForCalculation(line, EntityCodeList.Codes.BuyingParty, buyersFromLines);
					AddEntityForCalculation(line, EntityCodeList.Codes.ShipToParty, shipToPartiesFromLines);
				}
			}
			shouldSendManufacturerOnHeaderLevel = manufacturersFromLines != null && manufacturersFromLines.Count <= 1;
			shouldSendSellerOnHeaderLevel = sellersFromLines != null && sellersFromLines.Count <= 1;
			shouldSendConsigneeOnHeaderLevel = consigneeFromLines != null && consigneeFromLines.Count <= 1;
			shouldSendBuyingPartyOnHeaderLevel = buyersFromLines != null && buyersFromLines.Count <= 1;
			shouldSendShipToPartyOnHeaderLevel = shipToPartiesFromLines != null && shipToPartiesFromLines.Count <= 1;

			shouldSendExporterOnHeaderLevel = HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.Exporter);
			shouldSendShipperOnHeaderLevel = HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.Shipper);
			shouldSendDistributorOnHeaderLevel = HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.Distributor);
			shouldSendPackagerOnHeaderLevel = HasEntityOnHeaderLevel(entities, EntityCodeList.Codes.Packager);
		}

		void AddEntityForCalculation(ISimplifiedEntryLine line, ZString entityCode, List<ISimplifiedEntryOrganisationDetails> list)
		{
			if (list != null)
			{
				var entity = line.Entities.FirstOrDefault(x => x.EntityCode == entityCode);
				if (entity != null && !list.Any(x => x.CompanyName == entity.CompanyName))
				{
					list.Add(entity);
				}
			}
		}

		bool HasEntityOnHeaderLevel(IEnumerable<ISimplifiedEntryOrganisationDetails> entities, string entityCode)
		{
			return entities.FirstOrDefault(x => x.EntityCode == entityCode) != null;
		}

		#region Header Level Entity blocks

		ASESE30 GenerateSE30(ISimplifiedEntryOrganisationDetails address)
		{
			var se30 = new ASESE30();
			se30.EntityCode = address.EntityCode;

			if ((se30.EntityCode == EntityCodeList.Codes.Consignee && !address.EntityIdentifier.IsEmpty) ||
				(se30.EntityCode == EntityCodeList.Codes.BuyingParty && !address.EntityIdentifier.IsEmpty))
			{
				se30.EntityIdentifierQualifier = address.EntityIdentifierQualifier;
				se30.EntityIdentifier = address.EntityIdentifier;
			}
			else
			{
				se30.EntityName = address.CompanyName;
			}
			return se30;
		}

		IEnumerable<ASESE31> GenerateSE31Blocks(ISimplifiedEntryOrganisationDetails address)
		{
			foreach (var globalBusinessIdentifier in address.GlobalBusinessIdentifiers)
			{
				if (!globalBusinessIdentifier.IdentifierType.IsEmpty && !globalBusinessIdentifier.Identifier.IsEmpty)
				{
					yield return BlockBuilderHelper.GenerateSE31(globalBusinessIdentifier.IdentifierType, globalBusinessIdentifier.Identifier);
				}
			}
		}

		ASESE35 GenerateSE35(ISimplifiedEntryOrganisationDetails address)
		{
			var se35 = new ASESE35();
			se35.AddressComponentQualifier = AddressComponentQualifiersList.Codes.UnstructuredStreetAddress;
			se35.AddressInformation = address.AddressLine1;

			if (!address.AddressLine2.IsEmpty)
			{
				se35.AddressComponentQualifier1 = AddressComponentQualifiersList.Codes.UnstructuredStreetAddress;
				se35.AddressInformation1 = address.AddressLine2;
			}
			return se35;
		}

		ASESE36 GenerateSE36(ISimplifiedEntryOrganisationDetails address)
		{
			var se36 = new ASESE36();
			se36.CityName = address.City;
			se36.PostalCode = address.PostCode;
			se36.CountryCode = address.Country;
			return se36;
		}

		#endregion

		public void GenerateEntryLine(ISimplifiedEntryLine entryLine, List<MessageBlock> blocks)
		{
			entryLine.ClearPGALineNumbers();

			blocks.AddRange(GenerateSE40AndSE41Blocks(entryLine));
			blocks.AddRange(GenerateLineLevelEntityBlocks(entryLine.Entities));
			blocks.AddRange(GenerateSE60AndSE61Blocks(entryLine));
		}

		public static IEnumerable<MessageBlock> GenerateEntryLinePGABlocks(ISimplifiedEntryLine entryLine, ISEAdditionalData additionalData, bool enablePGATracking, bool buildAllPGAs)
		{
			entryLine.ClearPGALineNumbers();

			foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(entryLine, additionalData, enablePGATracking, isPGACorrection: false, buildOI: true, buildAllPGAs))
			{
				yield return block;
			}

			foreach (ISecondaryTariffLine secondaryLine in entryLine.SecondaryTariffLines)
			{
				foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(secondaryLine, additionalData, enablePGATracking, isPGACorrection: false, buildOI: false, buildAllPGAs))
				{
					yield return block;
				}
			}
		}

		internal IEnumerable<MessageBlock> GenerateLineLevelEntityBlocks(IEnumerable<ISimplifiedEntryOrganisationDetails> entities)
		{
			foreach (ISimplifiedEntryOrganisationDetails address in entities)
			{
				if ((!shouldSendSellerOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.SellingParty) ||
					(!shouldSendManufacturerOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.ManufacturerSupplier) ||
					(!shouldSendConsigneeOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.Consignee) ||
					(!shouldSendBuyingPartyOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.BuyingParty) ||
					(!shouldSendShipToPartyOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.ShipToParty) ||
					(!shouldSendExporterOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.Exporter && gBIIsActive) ||
					(!shouldSendShipperOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.Shipper && gBIIsActive) ||
					(!shouldSendDistributorOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.Distributor && gBIIsActive) ||
					(!shouldSendPackagerOnHeaderLevel && address.EntityCode == EntityCodeList.Codes.Packager && gBIIsActive))
				{
					var block50 = GenerateSE50(address);
					yield return block50;

					if (gBIIsActive)
					{
						foreach (var block51 in GenerateSE51Blocks(address))
						{
							yield return block51;
						}
					}

					if (!block50.EntityName.IsEmpty)
					{
						yield return GenerateSE55(address);
						yield return GenerateSE56(address);
					}
				}
			}
		}

		IEnumerable<MessageBlock> GenerateSE40AndSE41Blocks(ISimplifiedEntryLine entryLine)
		{
			var se40 = new ASESE40();
			se40.LineItemIdentifier = entryLine.CL_LineNumber;
			se40.CountryOfOrigin = entryLine.CountryOfOrigin;
			se40.CommercialInvoiceDescription = entryLine.Description;
			yield return se40;

			if (IsConsumptionFTZ)
			{
				var se41 = new ASESE41();
				se41.ZoneStatus = entryLine.ZoneStatus;
				se41.PrivilegedFTZMerchandiseFilingDate = !entryLine.FTZCurrentTariff.IsEmpty ? entryLine.PrivilegedStatusFilingDate : ZDate.Empty;
				se41.FTZLineItemQuantity = entryLine.FTZLineItemQuantity;
				yield return se41;
			}
		}

		#region Line Level Entity blocks

		ASESE50 GenerateSE50(ISimplifiedEntryOrganisationDetails address)
		{
			var se50 = new ASESE50();
			se50.EntityCode = address.EntityCode;

			if (se50.EntityCode == EntityCodeList.Codes.Consignee && !address.EntityIdentifier.IsEmpty ||
				se50.EntityCode == EntityCodeList.Codes.BuyingParty && !address.EntityIdentifier.IsEmpty)
			{
				se50.EntityIdentifierQualifier = address.EntityIdentifierQualifier;
				se50.EntityIdentifier = address.EntityIdentifier;
			}
			else
			{
				se50.EntityName = address.CompanyName;
			}
			return se50;
		}

		IEnumerable<ASESE51> GenerateSE51Blocks(ISimplifiedEntryOrganisationDetails address)
		{
			foreach (var globalBusinessIdentifier in address.GlobalBusinessIdentifiers)
			{
				if (!globalBusinessIdentifier.IdentifierType.IsEmpty && !globalBusinessIdentifier.Identifier.IsEmpty)
				{
					yield return BlockBuilderHelper.GenerateSE51(globalBusinessIdentifier.IdentifierType, globalBusinessIdentifier.Identifier);
				}
			}
		}

		ASESE55 GenerateSE55(ISimplifiedEntryOrganisationDetails address)
		{
			var se55 = new ASESE55();
			se55.AddressComponentQualifier = AddressComponentQualifiersList.Codes.UnstructuredStreetAddress;
			se55.AddressInformation = address.AddressLine1;

			if (!address.AddressLine2.IsEmpty)
			{
				se55.AddressComponentQualifier1 = AddressComponentQualifiersList.Codes.UnstructuredStreetAddress;
				se55.AddressInformation1 = address.AddressLine2;
			}
			return se55;
		}

		ASESE56 GenerateSE56(ISimplifiedEntryOrganisationDetails address)
		{
			var se56 = new ASESE56();
			se56.CityName = address.City;
			se56.PostalCode = address.PostCode;
			se56.CountryCode = address.Country;
			return se56;
		}

		#endregion

		IEnumerable<MessageBlock> GenerateSE60AndSE61Blocks(ISimplifiedEntryLine entryLine)
		{
			var adjustCustomsValue = ZDecimal.Zero;
			var shouldSendCustomsValueOnChildLine = false;
			if (!entryLine.SecondaryTariffLines.Any(x => x.IsCombineLine) && entryLine.IsSupLine)
			{
				var childLine = entryLine.SecondaryTariffLines.FirstOrDefault();
				if (childLine != null && childLine.ValueInUSD.IsEmpty)
				{
					adjustCustomsValue = entryLine.CL_CustomsValue;
					shouldSendCustomsValueOnChildLine = true;
				}
			}
			yield return GenerateSingleSE60(entryLine.Tariff, shouldSendCustomsValueOnChildLine ? ZDecimal.Zero : entryLine.CL_CustomsValue, entryLine.IsDisclaimSanction ? "Y" : ZString.Empty);
			if (IsConsumptionFTZ && !entryLine.FTZCurrentTariff.IsEmpty)
			{
				yield return BlockBuilderHelper.GenerateSE61Block(entryLine.FTZCurrentTariff);
			}

			foreach(var block in BlockBuilderHelper.BuildSanctionsAdditionalInfoGroupingBlocks(entryLine))
			{
				yield return block;
			}

			entryLine.ClearPGALineNumbers();

			foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(entryLine, additionalData, enablePGATracking))
			{
				yield return block;
			}

			foreach (ISecondaryTariffLine secondaryLine in entryLine.SecondaryTariffLines)
			{
				var wrapper = secondaryLine as SecondaryTariffLineWrapper;
				if (wrapper != null && wrapper.IsSetXLine)
				{
					yield return GenerateSingleSE60(secondaryLine.Tariff, ZDecimal.Zero, secondaryLine.IsDisclaimSanction ? "Y" : ZString.Empty);
				}
				else
				{
					yield return GenerateSingleSE60(secondaryLine.Tariff, adjustCustomsValue.IsEmpty ? secondaryLine.ValueInUSD : adjustCustomsValue, secondaryLine.IsDisclaimSanction ? "Y" : ZString.Empty);
					adjustCustomsValue = ZDecimal.Zero;
				}
				foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(secondaryLine, additionalData, enablePGATracking))
				{
					yield return block;
				}
			}

			if (entryLine is IXVVLine xvvEntry && xvvEntry.IsVParentLine)
			{
				foreach (var vChildEntry in xvvEntry.ChildVLines.Cast<ISimplifiedEntryLine>())
				{
					yield return GenerateSingleSE60(vChildEntry.Tariff, vChildEntry.CL_CustomsValue, vChildEntry.IsDisclaimSanction ? "Y" : ZString.Empty);

					foreach (ISecondaryTariffLine secondaryLine in vChildEntry.SecondaryTariffLines)
					{
						var wrapper = secondaryLine as SecondaryTariffLineWrapper;
						yield return GenerateSingleSE60(secondaryLine.Tariff, secondaryLine.ValueInUSD, secondaryLine.IsDisclaimSanction ? "Y" : ZString.Empty);

						foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(secondaryLine, additionalData, enablePGATracking))
						{
							yield return block;
						}
					}
				}
			}
		}

		ASESE60 GenerateSingleSE60(ZString tariff, ZDecimal value, ZString indicator)
		{
			var se60 = new ASESE60();
			se60.HTSNumber = tariff;
			se60.LineItemValue = value;
			se60.SanctionDisclaimIndicator = indicator;
			return se60;
		}

		ZBool IsConsumptionFTZ
		{
			get { return header.EntryType == EntryTypeList.Codes.ConsumptionFTZ; }
		}
	}
}

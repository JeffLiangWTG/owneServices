using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class EntrySummaryMessageBuilder : EntrySummaryMessageBuilderBase<ABIInputBlockControlGenerator>
	{
		public EntrySummaryMessageBuilder(ICusEntryHeaderMessageAttachee entryHeader, UpdateActionCode action, bool certifyCargoRelease)
			: base(entryHeader, action, certifyCargoRelease)
		{
		}

		protected override void UpdateMessageBlocks(ABIInputBlockControlGenerator block)
		{
			base.UpdateMessageBlocks(block);

			if (entryHeader.IsRemoteLocationFiling)
			{
				block.B.PreparerDistrictPort = entryHeader.PreparerDistrictPort;
				block.B.PreparerFilerCode = entryHeader.PreparerFilerCode;
				block.B.PreparerOfficeCode = entryHeader.PreparerOfficeCode;
				block.B.PreparerIndicator = "1";
			}

			if (action != UpdateActionCode.Delete)
			{
				block.Y.TotalEstimatedDuty += entryHeader.TotalEstimatedDuty;
				block.Y.TotalEstimatedTax += entryHeader.TotalEstimatedTax;
			}
		}

		protected override ABIInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			return new ABIInputBlockControlGenerator(entryHeader);
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			switch (action)
			{
				case UpdateActionCode.Delete:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
					break;
				case UpdateActionCode.Replace:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
					break;
				default:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
					break;
			}
		}
	}

	public abstract class EntrySummaryMessageBuilderBase<T> : EntryHeaderMessageBuilder<T> where T : BlockControlGenerator
	{
		protected EntrySummaryMessageBuilderBase(ICusEntryHeaderMessageAttachee entryHeader, UpdateActionCode action, bool certifyCargoRelease)
			: base(entryHeader, action)
		{
			this.certifyCargoRelease = certifyCargoRelease;
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.EntrySummary; }
		}

		protected override List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			List<UpdateActionCode> supportedList = base.GetSupportedUpdateActionCodeList();
			supportedList.Add(UpdateActionCode.Add);
			supportedList.Add(UpdateActionCode.Delete);
			supportedList.Add(UpdateActionCode.Replace);
			return supportedList;
		}

		protected override void UpdateMessageBlocks(T block)
		{
			block.MessageBlocks.AddRange(GetNewEntrySummaryBlockBuilder().Build(action, certifyCargoRelease));
		}

		protected virtual EntrySummaryBlockBuilder GetNewEntrySummaryBlockBuilder()
		{
			return new EntrySummaryBlockBuilder(entryHeader, ApplicationIdentifier);
		}

		#region Implementation

		readonly bool certifyCargoRelease;

		#endregion
	}

	public class EntrySummaryBlockBuilder
	{
		public EntrySummaryBlockBuilder(ICusEntryHeaderMessageAttachee entryHeader, string applicationIdentifier)
		{
			this.entryHeader = entryHeader;
			this.applicationIdentifier = applicationIdentifier;
		}

		public IEnumerable<MessageBlock> Build(UpdateActionCode action, bool certifyCargoRelease)
		{
			ClearCachedValues();
			BuildCore(action, certifyCargoRelease);
			return EntryHeaderBlock;
		}

		/*
						22 record		NOTE
06 (FTZ consumption)		No			CMR has a job that passed with 22 record, but Work Item WI00033663 made sure 22 is not sent for this entry type
21 (Warehouse)				Yes	
22 (Re-warehouse)			Yes	
31 (warehouse withdrawal)	No	

		*/
		protected virtual void BuildCore(UpdateActionCode action, bool certifyCargoRelease)
		{
			EntryHeaderBlock.Add(MakeENS10(action));
			if (action != UpdateActionCode.Delete)
			{
				EntryHeaderBlock.Add(MakeENS20());
				if (entryHeader.BondType == BondTypeList.Codes.SingleTransactionBond)
				{
					EntryHeaderBlock.Add(MakeENS21());
				}

				bool shouldSend22 = entryHeader.IsBillDetailRequired();
				if (shouldSend22)
				{
					EntryHeaderBlock.AddRange(MakeENS22());
				}

				EntryHeaderBlock.Add(MakeENS30(certifyCargoRelease));

				//TODO: ENS32 WI00007252(Consolidated Summary Transactions)
				EntryHeaderBlock.AddRange(MakeENS34());

				bool hasDumping = false;
				foreach (ICusEntryLine entryline in entryHeader.EntryLines)
				{
					if (!entryline.AntidumpingCaseNumber.IsEmpty || !entryline.CountervailingCaseNumber.IsEmpty)
					{
						hasDumping = true;
						break;
					}
				}

				if (hasDumping)
				{
					EntryHeaderBlock.Add(MakeENS35());
				}

				foreach (ICusEntryLine entryLine in entryHeader.EntryLines)
				{
					EntryHeaderBlock.AddRange(MakeLine(entryLine, certifyCargoRelease));
				}

				EntryHeaderBlock.AddRange(new ENS89Creator().MakeENS89(entryHeader.Fees, entryHeader.BuildEmpty89EvenIfNoFeeExists));

				EntryHeaderBlock.Add(MakeENS90());
			}
		}

		void ClearCachedValues()
		{
			entryHeaderBlock = null;
		}

		protected readonly ICusEntryHeaderMessageAttachee entryHeader;
		protected readonly string applicationIdentifier;

		protected List<MessageBlock> EntryHeaderBlock
		{
			get { return entryHeaderBlock ?? (entryHeaderBlock = new List<MessageBlock>()); }
		}
		List<MessageBlock> entryHeaderBlock;

		ENS10 MakeENS10(UpdateActionCode action)
		{
			ENS10 ens10 = new ENS10();
			ens10.UpdateActionCode = UpdateActionCodeConverter.ConvertToString(action);
			ens10.DistrictPortOfEntry = entryHeader.DistrictPortOfEntry;
			ens10.EntryFilerCode = entryHeader.EntryFilerCode;
			ens10.EntryNumber = MQEDIMessage.USEntryNumberPlaceHolder;
			ens10.EntryType = entryHeader.EntryType;

			if (action != UpdateActionCode.Delete)
			{
				ens10.ImporterOfRecordNumber = entryHeader.ImporterOfRecordNumber;
				ens10.UltimateConsigneeNumber = entryHeader.UltimateConsigneeNumber;
				//TODO: Support consolidated entry summary - add to merge key if needed
				ens10.CBPF4811ReferenceNumber = entryHeader.CBPF4811ReferenceNumber;
				ens10.LiveEntryIndicator = entryHeader.LiveEntry ? 1 : 0;
				ens10.MissingDocumentCodes = entryHeader.MissingDocumentCodes;
				ens10.BondType = entryHeader.BondType;
				ens10.EstimatedEntryDate = entryHeader.EstimatedEntryDate;
				ens10.ElectronicInvoiceIndicator = entryHeader.IsElectronicInvoicing ? "E" : string.Empty;

				ens10.SuretyCode = entryHeader.SuretyCode;
				ens10.StateOfDestination = entryHeader.StateOfDestination.Left(2).ToUpper();

				//if not applicable, it should be space-filled
				ens10.OGALineReleaseIndicator = entryHeader.OGALineReleaseIndicator ? 1 : 0;
			}

			return ens10;
		}

		ENS20 MakeENS20()
		{
			var ens20 = new ENS20();

			var importingVesselNameOrFTZNumber = entryHeader.ImportingVesselName;

			var isConsumptionFTZ = entryHeader.EntryType == EntryTypeList.Codes.ConsumptionFTZ;
			if (isConsumptionFTZ)
			{
				importingVesselNameOrFTZNumber = entryHeader.ImportFTZNumber;
			}

			ens20.ImportingVesselName = importingVesselNameOrFTZNumber.Left(20);
			if (!EntryTypeList.IsExWarehouseType(entryHeader.EntryType) && !isConsumptionFTZ)
			{
				ens20.ModeOfTransportationMOTCode = entryHeader.ModeOfTransportationCode;
			}

			ens20.DistrictPortOfUnlading = entryHeader.DistrictPortOfUnlading;
			ens20.DateOfImportation = entryHeader.DateOfImportation;
			ens20.BrokerReferenceNumber = entryHeader.BrokerReferenceNumber;
			ens20.ClientBranchDesignation = entryHeader.ClientBranchDesignation;

			if (TransportTypeList.IsVoyageFlightNoMandatory(entryHeader.ModeOfTransportationCode) && !isConsumptionFTZ)
			{
				ens20.VoyageFlightTripManifestNumber = entryHeader.VoyageNumber;
			}

			ens20.EstimatedDateOfArrival = entryHeader.EstimatedDateOfArrival;
			ens20.LocationOfGoods = entryHeader.LocationOfGoods;
			ens20.TradeAgreementReconciliationIndicator = entryHeader.NAFTAReconciliation ? "1" : "";
			ens20.OtherReconciliationIndicator = entryHeader.OtherReconciliationIndicator;
			return ens20;
		}

		ENS21 MakeENS21()
		{
			ENS21 ens21 = new ENS21();
			ens21.BondAmount = Math.Ceiling(entryHeader.BondAmount);
			ens21.BondProducerAccountNumber = entryHeader.BondProducerAccountNumber;
			return ens21;
		}

		IEnumerable<MessageBlock> MakeENS22()
		{
			ZString entryType = entryHeader.EntryType;

			foreach (IBillDetails billDetails in entryHeader.LowestBillDetails)
			{
				ENS22 ens22 = new ENS22();
				ens22.InBondNumber = billDetails.ITNumber;
				ens22.MasterBillNumber = billDetails.MasterBillNumber;
				ens22.HouseBillNumber = billDetails.HouseBillNumber;
				ens22.SubHouseBillNumber = billDetails.SubHouseBillNumber;
				ens22.Quantity = billDetails.PackageQuantity;
				ens22.Unit = billDetails.PackageType;
				ens22.ITDate = billDetails.ITDate;
				ens22.IssuerCodeOfMasterBillNumber = billDetails.IssuerCodeOfMasterBillNumber;
				if (!TransportModeCodes.IsAirTransport(entryHeader.ModeOfTransportationCode) && entryHeader.EntryType != EntryTypeList.Codes.ConsumptionFTZ)
				{
					ens22.IssuerCodeOfHouseBillNumber = billDetails.IssuerCodeOfHouseBillNumber;
				}

				yield return ens22;
			}
		}

		ENS30 MakeENS30(bool certifyCargoRelease)
		{
			ENS30 ens30 = new ENS30();
			ens30.EntryFilerCodeOfWarehouseEntry = entryHeader.EntryFilerCodeOfWarehouseEntry;
			ens30.WarehouseEntryNumber = entryHeader.WarehouseEntryNumber;
			ens30.DistrictPortCodeOfWarehouseEntry = entryHeader.DistrictPortCodeOfWarehouseEntry;

			if (EntryTypeList.IsExWarehouseType(entryHeader.EntryType))
			{
				ens30.FinalWarehouseIndicator = entryHeader.FinalWarehouseIndicator ? "1" : "0";
			}

			ens30.SummaryCertificationCode = entryHeader.IsElectronicInvoicing ? "1" : "0";
			ens30.ReleaseCertificationCode = certifyCargoRelease ? 1 : 0;
			ens30.ConsolidatedInformalIndicator = entryHeader.ConsolidatedInformalIndicator;
			ens30.DesignatedExamPort = entryHeader.DesignatedExamPort;
			ens30.PeriodicStatementMonth = entryHeader.PeriodicStatementMonth;
			ens30.PaymentTypeIndicator = entryHeader.PaymentTypeIndicator;
			ens30.PreliminaryStatementPrintDate = entryHeader.PreliminaryStatementPrintDate;
			ens30.CarrierCode = entryHeader.CarrierCode;
			//ens30.TeamNumber = ;// dont set this, this team is assigned by customs
			return ens30;
		}

		class CodeValuePair
		{
			public CodeValuePair(ZString code, ZDecimal value)
			{
				Code = code;
				Value = value;
			}
			public readonly ZString Code;
			public readonly ZDecimal Value;
		}

		IEnumerable<MessageBlock> MakeENS34()
		{
			List<CodeValuePair> pairs = new List<CodeValuePair>();
			if (entryHeader.InformalFee > 0)
			{
				pairs.Add(new CodeValuePair(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, entryHeader.InformalFee));
			}

			if (entryHeader.DutiableMailFee > 0)
			{
				pairs.Add(new CodeValuePair(Core.Constants.USCustoms.FeeCodes.DutiableMail, entryHeader.DutiableMailFee));
			}

			if (entryHeader.ManualSurcharge > 0)
			{
				pairs.Add(new CodeValuePair(Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge, entryHeader.ManualSurcharge));
			}

			if (pairs.Count > 0)
			{
				ENS34 ens34 = new ENS34();
				ens34.ClassCode = pairs[0].Code;
				ens34.Amount = pairs[0].Value;
				if (pairs.Count > 1)
				{
					ens34.ClassCode1 = pairs[1].Code;
					ens34.Amount1 = pairs[1].Value;
				}

				if (pairs.Count > 2)
				{
					ens34.ClassCode2 = pairs[2].Code;
					ens34.Amount2 = pairs[2].Value;
				}

				yield return ens34;
			}
		}

		ENS35 MakeENS35()
		{
			ENS35 ens35 = new ENS35();
			ens35.BondedADDDuty = entryHeader.BondedADDDuty;
			ens35.BondedADDIndicator = entryHeader.BondedADDIndicator ? "1" : "0";
			ens35.PayableADDDuty = entryHeader.PayableADDDuty;
			ens35.BondedCVDDuty = entryHeader.BondedCVDDuty;
			ens35.BondedCVDIndicator = entryHeader.BondedCVDIndicator ? "1" : "0";
			ens35.PayableCVDDuty = entryHeader.PayableCVDDuty;
			ens35.ADDCVDSuretyCode = entryHeader.ADDCVDSuretyCode;
			return ens35;
		}

		IEnumerable<MessageBlock> MakeLine(ICusEntryLine entryLine, bool certifyCargoRelease)
		{
			yield return MakeENS40(entryLine);

			foreach (ENS43 ens43 in MakeENS43(entryLine))
			{
				yield return ens43;
			}

			yield return MakeENS50(entryLine);

			foreach (MessageBlock block in OGABlocksCreator.GetDisclaimingBlocks(entryLine, certifyCargoRelease))
			{
				yield return block;
			}

			foreach (MessageBlock block in PGABlocksCreator.GetLaceyActBlocks(entryLine, certifyCargoRelease))
			{
				yield return block;
			}

			foreach (MessageBlock block in OGABlocksCreator.GetOGABlocks(entryLine, certifyCargoRelease, applicationIdentifier))
			{
				yield return block;
			}

			if (!entryLine.DateOfExportationFromCountryOfOrigin.IsEmpty
				|| !entryLine.VisaNumber.IsEmpty
				|| !entryLine.TextileCategoryNumber.IsEmpty
				|| !entryLine.VisaQuantity.IsEmpty
				|| !entryLine.VisaUnitOfMeasure.IsEmpty
				|| !entryLine.AgricultureLicenseNumber.IsEmpty
				|| !entryLine.CottonCertificateNumberOrganicExemptionCertificateNumber.IsEmpty)
			{
				yield return MakeENS51(entryLine);
			}

			if (!entryLine.ChinaHongKongSWPMIndicator.IsEmpty
				|| !entryLine.CanadianExportCertificateSugar.IsEmpty
				|| !entryLine.WoolLicense.IsEmpty
				|| !entryLine.CBTPACertificationNumber.IsEmpty
				|| !entryLine.MiscellaneousPermitLicenseNumber.IsEmpty
				|| entryLine.IsSoftwoodLumberLine)
			{
				yield return MakeENS52(entryLine);
			}
			//if (needs57) yield return MakeENS57(entryLine);			Future use - not yet in prod as per spec -- add to merge key if required
			yield return MakeENS60(entryLine);

			foreach (IFee fee in entryLine.Fees)
			{
				yield return MakeENS62(entryLine.GetRelevantTariffForFee(fee.Code), fee);
			}

			int lineType = 2;
			foreach (ISecondaryTariffLine tariffLine in entryLine.SecondaryTariffLines)
			{
				switch (lineType)
				{
					case 2:
						yield return MakeENS70(tariffLine);
						break;
					case 3:
						yield return MakeENS80(tariffLine);
						break;
					default:
						yield return MakeENS81(tariffLine);
						break;
				}

				lineType++;

				foreach (MessageBlock block in OGABlocksCreator.GetDisclaimingBlocks(tariffLine, certifyCargoRelease))
				{
					yield return block;
				}

				foreach (MessageBlock block in PGABlocksCreator.GetLaceyActBlocks(tariffLine, certifyCargoRelease))
				{
					yield return block;
				}

				foreach (MessageBlock block in OGABlocksCreator.GetOGABlocks(tariffLine, certifyCargoRelease, applicationIdentifier))
				{
					yield return block;
				}
			}
		}

		#region OGA

		OGABlocksCreator OGABlocksCreator
		{
			get { return ogaBlocksCreator ?? (ogaBlocksCreator = new OGABlocksCreator()); }
		}
		OGABlocksCreator ogaBlocksCreator;

		PGABlocksCreator PGABlocksCreator
		{
			get { return pgaBlocksCreator ?? (pgaBlocksCreator = new PGABlocksCreator()); }
		}
		PGABlocksCreator pgaBlocksCreator;

		#endregion

		ENS40 MakeENS40(ICusEntryLine entryLine)
		{
			ENS40 ens40 = new ENS40();
			ens40.LineItemNumber = entryLine.CL_LineNumber;
			ens40.CountryOfOrigin = entryLine.CountryOfOrigin;

			ens40.Value = entryLine.CL_CustomsValue;
			ens40.GrossWeight = entryLine.GrossWeightInKilograms;
			ens40.ADDSpecificDepositValue = entryLine.ADDSpecificDepositValue;
			ens40.CVDSpecificDepositValue = entryLine.CVDSpecificDepositValue;
			ens40.Charges = entryLine.Charges;
			if (entryHeader.ModeOfTransportationCode == TransportModeCodes.Codes.VesselContainer || entryHeader.ModeOfTransportationCode == TransportModeCodes.Codes.VesselNonContainer || entryHeader.ModeOfTransportationCode == TransportModeCodes.Codes.BorderWaterBorne)
			{
				ens40.PortOfLading = entryLine.PortOfLading;
			}

			ZShort invoiceDelimiter = entryLine.InvDelimter;

			if (invoiceDelimiter != ZShort.Zero)
			{
				ens40.InvoiceDelimiter = "INV" + invoiceDelimiter.ToString().PadLeft(3, '0');
			}

			if (entryHeader.EntryType == EntryTypeList.Codes.ConsumptionFTZ)
			{
				ens40.ZoneStatus = entryLine.ZoneStatus;
				ens40.PrivilegedStatusFilingDate = entryLine.PrivilegedStatusFilingDate;
			}

			ens40.NAFTANetCostIndicator = entryLine.NAFTANetCostIndicator ? "Y" : "";

			return ens40;
		}

		IEnumerable<MessageBlock> MakeENS43(ICusEntryLine entryLine)
		{
			bool isInvoiceByRequestWithBindingRule = false;
			if (!entryLine.PreImportationReviewProgramRulingsNumber.IsEmpty)
			{
				ENS43 ens43 = new ENS43();
				ens43.TypeIndicator = entryLine.PreImportationReviewProgramRulingsType;
				ens43.PreImportationReviewProgramPIRPRulingsNumber = entryLine.PreImportationReviewProgramRulingsNumber;
				yield return ens43;
			}
			else if (entryHeader.IsInvoiceByRequest)
			{
				ENS43 ens43 = new ENS43();
				ens43.TypeIndicator = PIRPRulingTypeList.Codes.BindingRulings;
				ens43.PreImportationReviewProgramPIRPRulingsNumber = "INVREQ";
				yield return ens43;
				isInvoiceByRequestWithBindingRule = true;
			}

			if (entryHeader.IsElectronicInvoicing && !isInvoiceByRequestWithBindingRule)
			{
				foreach (ZString commercialDescriptionLong in entryLine.CommercialDescriptions)
				{
					foreach (ZString commercialDescription in commercialDescriptionLong.Split(70))
					{
						ENS43 ens43 = new ENS43();
						ens43.TypeIndicator = PIRPRulingTypeList.Codes.CommercialDescription;
						ens43.CommercialDescription = commercialDescription;
						yield return ens43;
					}
				}
			}
		}

		ENS50 MakeENS50(ICusEntryLine entryLine)
		{
			ENS50 ens50 = new ENS50();
			ens50.SpecialProgramsIndicatorPrimary = entryLine.SpecialProgramsIndicatorPrimary;
			ens50.TariffNumber1 = entryLine.Tariff;
			ens50.Duty = entryLine.DutyAmount;
			ens50.Quantity1 = entryLine.Quantity1;
			ens50.UnitOfMeasure1 = entryLine.UnitOfMeasure1;
			ens50.Quantity2 = entryLine.Quantity2;
			ens50.UnitOfMeasure2 = entryLine.UnitOfMeasure2;
			ens50.Quantity3 = entryLine.Quantity3;
			ens50.UnitOfMeasure3 = entryLine.UnitOfMeasure3;
			ens50.CountryOfExport = entryLine.CountryOfExport;
			ens50.DateOfExportation = entryLine.DateOfExportation;
			if (!EntryTypeList.IsInformal(entryHeader.EntryType))
			{
				ens50.RelatedPartyIndicator = entryLine.RelatedPartyIndicator ? "Y" : "N";
			}
			ens50.SpecialProgramsIndicatorCountry = entryLine.SpecialProgramsIndicatorCountry;
			ens50.SpecialProgramsIndicatorSecondary = entryLine.SpecialProgramsIndicatorSecondary;
			return ens50;
		}

		ENS51 MakeENS51(ICusEntryLine entryLine)
		{
			ENS51 ens51 = new ENS51();
			ens51.DateOfExportationTextiles = entryLine.DateOfExportationFromCountryOfOrigin;
			ens51.VisaNumber = entryLine.VisaNumber;
			ens51.CategoryNumber = entryLine.TextileCategoryNumber;
			ens51.VisaQuantity = entryLine.VisaQuantity;
			ens51.VisaUnitOfMeasure = entryLine.VisaUnitOfMeasure;
			ens51.AgricultureLicenseNumber = entryLine.AgricultureLicenseNumber;
			ens51.CottonCertificateNumberOrganicExemptionCertificateNumber = entryLine.CottonCertificateNumberOrganicExemptionCertificateNumber;

			return ens51;
		}

		ENS52 MakeENS52(ICusEntryLine entryLine)
		{
			ENS52 ens52 = new ENS52();
			ens52.ChinaHongKongSWPMIndicator = entryLine.ChinaHongKongSWPMIndicator;
			ens52.CanadianExportCertificateSugar = entryLine.CanadianExportCertificateSugar;
			ens52.WoolLicense = entryLine.WoolLicense;
			ens52.CBTPACertificationNumber = entryLine.CBTPACertificationNumber;
			ens52.MiscellaneousPermitLicenseNumber = entryLine.MiscellaneousPermitLicenseNumber;
			if (entryLine.IsSoftwoodLumberLine)
			{
				if (!entryLine.SoftwoodLumberExportPrice.IsEmpty)
				{
					ens52.OtherDataIndicator1 = "01";
					ens52.OtherDataElement1 = entryLine.SoftwoodLumberExportPrice.Round(0).ToString().PadLeft(9, '0');
				}
				ens52.OtherDataIndicator2 = "01";
				ens52.OtherDataElement2 = (entryLine.IsSoftwoodLumberImporterDeclaration ? "Y" : " ") + entryLine.SoftwoodLumberExportCharges.Round(0).ToString().PadLeft(11, '0');
			}

			return ens52;
		}

		ENS60 MakeENS60(ICusEntryLine entryLine)
		{
			ENS60 ens60 = new ENS60();
			ens60.ManufacturerSupplierCode = entryLine.ManufacturerSupplierCode;
			ens60.InternalRevenueServiceIRSTax = entryLine.ExciseTax;

			ens60.CountervailingCaseNumber = entryLine.CountervailingCaseNumber;
			if (!ens60.CountervailingCaseNumber.IsEmpty)
			{
				ens60.BondedCVDIndicator = entryLine.BondedCountervailingDuty ? "1" : "0";
				ens60.CountervailingDuty = entryLine.CountervailingDuty;

				if (entryLine.CVDCaseRateTypeQualifier != DepositRateIndicatorList.Codes.Specific)
				{
					ens60.CVDDepositRate = entryLine.CVDDepositRate;
				}
			}

			ens60.AntidumpingCaseNumber = entryLine.AntidumpingCaseNumber;
			if (!ens60.AntidumpingCaseNumber.IsEmpty)
			{
				ens60.BondedADDIndicator = entryLine.BondedAntidumpingDuty ? "1" : "0";
				ens60.AntidumpingDuty = entryLine.AntidumpingDuty;

				if (entryLine.ADDCaseRateTypeQualifier != DepositRateIndicatorList.Codes.Specific)
				{
					ens60.ADDDepositRate = entryLine.ADDDepositRate;
				}
			}
			return ens60;
		}

		ENS62 MakeENS62(ZString tariffNumber, IFee fee)
		{
			ENS62 ens62 = new ENS62();
			if (fee.Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing &&
				fee.Code != Core.Constants.USCustoms.FeeCodes.HMF)
			{
				ens62.TariffNumber = tariffNumber;
			}
			ens62.ClassCode = fee.Code;
			ens62.UserFeeAmount = fee.Amount.Round(2);
			return ens62;
		}

		ENS70 MakeENS70(ISecondaryTariffLine tariffLine)
		{
			ENS70 ens70 = new ENS70();
			ens70.TariffNumber2 = tariffLine.Tariff;
			ens70.SpecialProgramsIndicatorPrimaryOrCountry = tariffLine.SpecialProgramsIndicatorPrimaryOrCountry;
			ens70.Duty = tariffLine.Duty;
			ens70.Quantity1 = tariffLine.Quantity1;
			ens70.Unit1 = tariffLine.UQ1;
			ens70.Quantity2 = tariffLine.Quantity2;
			ens70.Unit2 = tariffLine.UQ2;
			ens70.Quantity3 = tariffLine.Quantity3;
			ens70.Unit3 = tariffLine.UQ3;
			ens70.Value = tariffLine.ValueInUSD.Round(0);
			ens70.SpecialProgramsIndicatorSecondary = tariffLine.SpecialProgramsIndicatorSecondary;
			return ens70;
		}

		ENS80 MakeENS80(ISecondaryTariffLine tariffLine)
		{
			ENS80 ens80 = new ENS80();
			ens80.TariffNumber3 = tariffLine.Tariff;
			ens80.SpecialProgramsIndicatorPrimaryOrCountry = tariffLine.SpecialProgramsIndicatorPrimaryOrCountry;
			ens80.Duty = tariffLine.Duty;
			ens80.Quantity1 = tariffLine.Quantity1;
			ens80.Unit1 = tariffLine.UQ1;
			ens80.Quantity2 = tariffLine.Quantity2;
			ens80.Unit2 = tariffLine.UQ2;
			ens80.Quantity3 = tariffLine.Quantity3;
			ens80.Unit3 = tariffLine.UQ3;
			ens80.Value = tariffLine.ValueInUSD.Round(0);
			ens80.SpecialProgramsIndicatorSecondary = tariffLine.SpecialProgramsIndicatorSecondary;
			return ens80;
		}

		ENS81 MakeENS81(ISecondaryTariffLine tariffLine)
		{
			ENS81 ens81 = new ENS81();
			ens81.AdditionalTariffNumber = tariffLine.Tariff;
			ens81.SpecialProgramsIndicatorPrimaryOrCountry = tariffLine.SpecialProgramsIndicatorPrimaryOrCountry;
			ens81.Duty = tariffLine.Duty;
			ens81.Quantity1 = tariffLine.Quantity1;
			ens81.Unit1 = tariffLine.UQ1;
			ens81.Quantity2 = tariffLine.Quantity2;
			ens81.Unit2 = tariffLine.UQ2;
			ens81.Quantity3 = tariffLine.Quantity3;
			ens81.Unit3 = tariffLine.UQ3;
			ens81.Value = tariffLine.ValueInUSD.Round(0);
			ens81.SpecialProgramsIndicatorSecondary = tariffLine.SpecialProgramsIndicatorSecondary;
			return ens81;
		}

		ENS90 MakeENS90()
		{
			ENS90 ens90 = new ENS90();
			ens90.TotalEstimatedDuty = entryHeader.TotalEstimatedDuty;
			ens90.GrandTotalEstimatedTax = entryHeader.TotalEstimatedTax;
			ens90.DeferredTaxIndicator = entryHeader.DeferredTaxIndicator;
			ens90.TotalCountervailingDutyAmount = entryHeader.TotalCountervailingDuty;
			ens90.TotalAntidumpingDutyAmount = entryHeader.TotalAntidumpingDuty;
			ens90.GrandTotalFeeAmount = entryHeader.GrandTotalFee;
			ens90.TotalValueOfEntrySummary = entryHeader.TotalValueOfEntrySummary;
			return ens90;
		}
	}
}

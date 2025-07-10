using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public abstract class EntrySummaryPrintingFromMessage : EntrySummary7501Print
	{
		public EntrySummaryPrintingFromMessage(CusEntryHeader entryHeader, EDIMessage outgoingMessage, bool acceptedWithCensusWarnings, EDIMessage bluMessage)
			: base(entryHeader)
		{
			this.outgoingMessage = outgoingMessage;
			this.acceptedWithCensusWarnings = acceptedWithCensusWarnings;
			LineObjectCollection = new EntryMessageLineCollection(Factory);

			entryPrintBills = new EntrySummary7501BillCollection(Factory);
			billPrintManager = new BillPrintingManager(entryHeader, bluMessage, entryHeader.Factory, entryPrintBills);
			blocksForBillPrinting = new List<MessageBlock>();
		}
		readonly bool acceptedWithCensusWarnings;
		protected readonly BillPrintingManager billPrintManager;
		protected readonly List<MessageBlock> blocksForBillPrinting;

		public override ZDateTime FirstBillITDate
		{
			get { return !IsConsumptionFTZ ? billPrintManager.FirstBillITDate : ZDateTime.Empty; }
		}

		public override ZString FirstBillITNO
		{
			get { return !IsConsumptionFTZ ? billPrintManager.FirstBillITNO : ZString.Empty; }
		}

		public override ZString SCACAndMBillNumber
		{
			get { return billPrintManager.SCACAndMBillNumber; }
		}

		/// <summary>
		/// blank, or, the statement date if a release date has been received
		/// </summary>
		public override ZDateTime EntrySummaryFiledDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (entry != null && !Declaration.JE_EntryAuthorisationDate.IsEmpty)
				{
					result = entry.US_PreliminaryStatementPrintDate;
				}

				return result;
			}
		}

		protected ZString PaymentType
		{
			get
			{
				if (!paymentTypeCached.HasValue)
				{
					paymentTypeCached = PaymentTypeFromEntrySummary;

					var messages = Declaration.TransmittedStatementMessagesInDescOrder;
					foreach (MQEDIMessage message in messages)
					{
						if (outgoingMessage.EM_SystemCreateTimeUtc < message.EM_SystemCreateTimeUtc && message.IsSTUMessageAndAccepted())
						{
							var hBlock = message.MessageBlock.MessageBlocks.OfType<IStatementUpdateInputHBlock>().FirstOrDefault();

							if (hBlock != null)
							{
								paymentTypeCached = hBlock.PaymentTypeIndicator;
								break;
							}
						}
					}
				}
				return paymentTypeCached.Value;
			}
		}
		ZString? paymentTypeCached;

		protected ZString GetABIStatusIndicator(ZString paymentType)
		{
			/*
			 ABI/S = ABI statement paid by check or cash
			 ABI/A = ABI statement paid via Automated Clearinghouse (ACH)
			 ABI/P = ABI statement paid on a periodic monthly basis
			 ABI/N = ABI summary not paid on a statement
			*/
			ZString result = ZString.Empty;

			if (paymentType == PaymentTypeList.Codes.IndividualBasis)
			{
				result = "N";
			}
			else
			{
				if (paymentType == PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter
					|| paymentType == PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes
					|| paymentType == PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter
					|| paymentType == PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes)
				{
					if (ImporterOfRecord != null)
					{
						var ior = OrgHeaderWrapper.New(ImporterOfRecord);
						if (ior.ZO_PayMethod == ACHPaymentTypeList.Codes.ImporterCheck)
						{
							result = "S";
						}
					}
				}
			}

			if (result.IsEmpty)
			{
				switch (paymentType)
				{
					case PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode:
					case PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter:
					case PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporterWithSuffixes:
						result = "A";
						break;
					case PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate:
					case PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter:
					case PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporterWithSuffixes:
						result = "P";
						break;
					default:
						result = "N";
						break;
				}
			}

			return result;
		}

		public override ZString UniquePortOfLading
		{
			get
			{
				var result = ZString.Empty;

				if (!IsConsumptionFTZ && IsSeaTransportMode)
				{
					result = GetPortOfLadingFromEntryLines();
				}

				return result;
			}
		}

		protected abstract ZString GetPortOfLadingFromEntryLines();

		#region Importer Properties

		OrgHeader ImporterOfRecord
		{
			get
			{
				if (fImporterOfRecord == null && !iORCannotBeDetermined)
				{
					iORCannotBeDetermined = false;

					var importerOfRecordNumber = ImporterOfRecordCustomsRegNoCore;
					if (importerOfRecordNumber == entry.ImporterOfRecordNumber)
					{
						fImporterOfRecord = entry.ImporterOfRecord;
					}
					else
					{
						if (HasUniqueOrgIDCustomsCode(importerOfRecordNumber))
						{
							fImporterOfRecord = GetOrgFromCustomsCode(importerOfRecordNumber);
						}
						else // Organisation cannot be determined... user will have to update IOR or update organisation codes
						{
							iORCannotBeDetermined = true;
						}
					}
				}

				return fImporterOfRecord;
			}
		}
		OrgHeader fImporterOfRecord;

		OrgAddress ImporterOfRecordCustomsAddress
		{
			get { return iorCustomsAddress ?? (iorCustomsAddress = ImporterOfRecord.GetCustomsAddressDetailsFallingBackToMainAddress()); }
		}
		OrgAddress iorCustomsAddress;

		bool HasUniqueOrgIDCustomsCode(ZString code)
		{
			string[] iDTypes = new string[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber };
			int iDCount = new OrgCusCode.Loader(Factory).GetDatabaseCount(Core.Constants.CountryCodes.UnitedStates, code, iDTypes);

			return iDCount < 2;
		}

		OrgHeader GetOrgFromCustomsCode(ZString code)
		{
			OrgHeader result = null;
			ZQuery cusCodeEINQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
			cusCodeEINQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, code);
			cusCodeEINQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
			OrgCusCode orgCusCode = Factory.LoadTop1<OrgCusCode>(cusCodeEINQuery);

			if (orgCusCode == null)
			{
				ZQuery cusCodeSSNQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.SocialSecurityNumber);
				cusCodeSSNQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, code);
				cusCodeSSNQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				orgCusCode = Factory.LoadTop1<OrgCusCode>(cusCodeSSNQuery);
			}

			if (orgCusCode == null)
			{
				ZQuery cusCodeCBPQuery = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.USACodeTypes.CBPAssignedNumber);
				cusCodeCBPQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, code);
				cusCodeCBPQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				orgCusCode = Factory.LoadTop1<OrgCusCode>(cusCodeCBPQuery);
			}

			if (orgCusCode != null)
			{
				OrgHeader relatedOrg = Factory.Load<OrgHeader>(orgCusCode.OK_OH);
				if (relatedOrg != null)
				{
					result = relatedOrg;
				}
			}

			return result;
		}

		public override ZString ImporterCompanyName
		{
			get
			{
				var addressDetails = (IAddressDetails)ImporterOfRecordCustomsAddress;

				return addressDetails != null ?
							addressDetails.CompanyName.ToString() :
							iORCannotBeDetermined ?
								unableToDetermineImporter : string.Empty;
			}
		}

		public override ZString ImporterAddressLine1
		{
			get
			{
				return ImporterOfRecordCustomsAddress != null ?
					  ImporterOfRecordCustomsAddress.OA_Address1.ToString() :
					  iORCannotBeDetermined ? unableToDetermineImporterLine2 : string.Empty;
			}
		}

		public override ZString ImporterAddressLine2
		{
			get { return ImporterOfRecordCustomsAddress != null ? ImporterOfRecordCustomsAddress.OA_Address2 : ZString.Empty; }
		}

		public override ZString ImporterCity
		{
			get { return ImporterOfRecordCustomsAddress != null ? ImporterOfRecordCustomsAddress.OA_City : ZString.Empty; }
		}

		public override ZString ImporterState
		{
			get { return ImporterOfRecordCustomsAddress != null ? ImporterOfRecordCustomsAddress.OA_State : ZString.Empty; }
		}

		public override ZString ImporterPostCode
		{
			get { return ImporterOfRecordCustomsAddress != null ? ImporterOfRecordCustomsAddress.OA_PostCode : ZString.Empty; }
		}

		#endregion

		#region Ultimate Consignee Properties

		OrgHeader UltimateConsignee
		{
			get
			{
				if (fUltimateConsignee == null && !ultimateConsigneeCannotBeDetermined)
				{
					ultimateConsigneeCannotBeDetermined = false;
					var customsCodeValue = EffectiveUltimateConsigneeCustomsRegNoCore != USConstants.Same ? EffectiveUltimateConsigneeCustomsRegNoCore : ImporterOfRecordCustomsRegNoCore;

					if (EffectiveUltimateConsigneeCustomsRegNoCore != USConstants.Same)
					{
						if (EffectiveUltimateConsigneeCustomsRegNoCore == entry.EffectiveUltimateConsigneeCustomsRegNo)
						{
							fUltimateConsignee = entry.Declaration.ConsigneeOrgAddress;
						}
						else
						{
							if (HasUniqueOrgIDCustomsCode(customsCodeValue))
							{
								fUltimateConsignee = GetOrgFromCustomsCode(customsCodeValue);
							}
							else // Organisation cannot be determined... user will have to update Consignee or update organisation codes
							{
								ultimateConsigneeCannotBeDetermined = true;
							}
						}
					}
				}

				return fUltimateConsignee;
			}
		}
		OrgHeader fUltimateConsignee;

		OrgAddress UltimateConsigneeCustomsAddress
		{
			get { return consigneeCustomsAddress ?? (consigneeCustomsAddress = UltimateConsignee.GetCustomsAddressDetailsFallingBackToMainAddress()); }
		}
		OrgAddress consigneeCustomsAddress;

		public override ZString EffectiveUltimateConsigneeCompanyName
		{
			get
			{
				var addressDetails = (IAddressDetails)UltimateConsigneeCustomsAddress;

				return addressDetails != null ?
							addressDetails.CompanyName.ToString() :
							ultimateConsigneeCannotBeDetermined ?
								unableToDetermineConsignee : string.Empty;
			}
		}

		public override ZString EffectiveUltimateConsigneeAddressLine1
		{
			get
			{
				return UltimateConsigneeCustomsAddress != null ?
					  UltimateConsigneeCustomsAddress.OA_Address1.ToString() :
					  ultimateConsigneeCannotBeDetermined ? unableToDetermineConsigneeLine2 : string.Empty;
			}
		}

		public override ZString EffectiveUltimateConsigneeAddressLine2
		{
			get { return UltimateConsigneeCustomsAddress != null ? UltimateConsigneeCustomsAddress.OA_Address2 : ZString.Empty; }
		}

		public override ZString EffectiveUltimateConsigneeCity
		{
			get { return UltimateConsigneeCustomsAddress != null ? UltimateConsigneeCustomsAddress.OA_City : ZString.Empty; }
		}

		public override ZString EffectiveUltimateConsigneeState
		{
			get
			{
				return EffectiveUltimateConsigneeCustomsRegNoCore == USConstants.Same ?
					  UltimateStateCore :
					  UltimateConsigneeCustomsAddress != null ? UltimateConsigneeCustomsAddress.OA_State : ZString.Empty;
			}
		}

		public override ZString EffectiveUltimateConsigneePostCode
		{
			get { return UltimateConsigneeCustomsAddress != null ? UltimateConsigneeCustomsAddress.OA_PostCode : ZString.Empty; }
		}

		public override ZString UltimateState
		{
			get
			{
				ZString result = ZString.Empty;
				if (UltimateStateCore != EffectiveUltimateConsigneeState)
				{
					result = UltimateStateCore;
				}

				return result;
			}
		}
		protected abstract ZString UltimateStateCore { get; }

		#endregion

		public override ZString SummaryStatus
		{
			get { return entry.Declaration.US_PaperlessEntry.IsEmpty ? OldSummaryStatus : NewSummaryStatus; }
		}

		ZString OldSummaryStatus
		{
			get
			{
				ZString result = ZString.Empty;

				if (entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings ||
					entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings)
				{
					result = USConstants.EntrySummaryDisposition.CensusWarning;
				}
				else if ((entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings ||
					entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings) &&
					entry.Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond)
				{
					result = USConstants.EntrySummaryDisposition.DocsRequired;
				}
				else if (entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings ||
					entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings)
				{
					result = USConstants.EntrySummaryDisposition.Paperless;
				}
				else
				{
					result = CalculatedPaperlessStatusFromDispositions;
				}

				if (result.IsEmpty)
				{
					result = CalculatedPaperlessStatusFromDeclaration;
				}

				return result;
			}
		}

		protected abstract ZString CalculatedPaperlessStatusFromDispositions { get; }
		protected abstract ZString CalculatedPaperlessStatusFromDeclaration { get; }

		ZString NewSummaryStatus
		{
			get
			{
				ZString result = ZString.Empty;
				var declaration = entry.Declaration;

				if (declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings ||
					declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings ||
					(declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.ErrorEntrySummaryReplace && acceptedWithCensusWarnings))
				{
					result = USConstants.EntrySummaryDisposition.CensusWarning;
				}
				else if (declaration.US_PaperlessEntry == YesNoDefaultList.Codes.Yes)
				{
					result = USConstants.EntrySummaryDisposition.Paperless;
				}
				else
				{
					result = USConstants.EntrySummaryDisposition.DocsRequired;
				}

				return result;
			}
		}

		public override ZString SummaryBlockOverflow
		{
			get
			{
				if (!summaryBlockOverflowCached.HasValue)
				{
					int specialChargeCategories = 0;
					if (TotalAntidumpingDutyAmountPayable > 0)
					{ specialChargeCategories++; }
					if (TotalCountervailingDutyPayable > 0)
					{ specialChargeCategories++; }
					summaryBlockOverflowCached = (SummaryChargesCount + specialChargeCategories) > 4 ? "+" : "";
				}

				return summaryBlockOverflowCached.Value;
			}
		}
		ZString? summaryBlockOverflowCached;

		ZInt SummaryChargesCount
		{
			get { return GetSummaryChargesCountFromEntryLines(); }
		}
		protected abstract ZInt GetSummaryChargesCountFromEntryLines();

		public override List<IFee> Fees
		{
			get
			{
				if (fees == null)
				{
					fees = new List<IFee>();
					if (HMFDeMinimis)
					{
						AddFee(Core.Constants.USCustoms.FeeCodes.HMF, 0m);
					}

					if (TotalAntidumpingDutyAmountPayable > 0)
					{
						AddFee("012", TotalAntidumpingDutyAmountPayable);
					}

					if (TotalCountervailingDutyPayable > 0)
					{
						AddFee("013", TotalCountervailingDutyPayable);
					}

					AddFeesFrom89Lines();

					fees.Sort((fee1, fee2) => string.Compare(fee1.Code, fee2.Code));
				}

				return fees;
			}
		}

		protected abstract void AddFeesFrom89Lines();

		public override ZDecimal TotalOther
		{
			get
			{
				ZDecimal result = 0m;

				if (!EntryTypeList.IsNothingPayable(EntryType))
				{
					if (EntryTypeList.IsOnlyHMFPayable(EntryType) || EntryTypeList.IsTIB(EntryType)) // CS00113346 - For Warehouse entry, only HMF is payable on entry & should be included in box 39.
					{
						result = EntryHMF;
					}
					else
					{
						result = GrandTotalFeeAmount;

						if (TotalAntidumpingDutyAmount > 0)
						{
							result += TotalAntidumpingDutyAmountPayable;
						}

						if (TotalCountervailingDutyAmount > 0)
						{
							result += TotalCountervailingDutyPayable;
						}
					}
				}
				return result;
			}
		}

		protected abstract ZDecimal GrandTotalFeeAmount { get; }
		protected abstract ZDecimal TotalAntidumpingDutyAmount { get; }
		protected abstract ZDecimal TotalCountervailingDutyAmount { get; }
		protected abstract ZString PaymentTypeFromEntrySummary { get; }

		protected override ZDecimal TotalLineLevelHMFs
		{
			get
			{
				if (!totalLineLevelHMFs.HasValue)
				{
					totalLineLevelHMFs = GetTotalLineLevelHMFsFromCharges();
				}
				return totalLineLevelHMFs.Value;
			}
		}
		ZDecimal? totalLineLevelHMFs;

		protected abstract ZDecimal GetTotalLineLevelHMFsFromCharges();

		public override ZDecimal TotalCountervailingDutyPayable
		{
			get
			{
				if (!totalCountervailingDutyPayable.HasValue)
				{
					totalCountervailingDutyPayable = GetTotalCountervailingDutyPayableFromCharges();
				}
				return totalCountervailingDutyPayable.Value;
			}
		}
		ZDecimal? totalCountervailingDutyPayable;

		protected abstract ZDecimal GetTotalCountervailingDutyPayableFromCharges();

		public override ZDecimal TotalAntidumpingDutyAmountPayable
		{
			get
			{
				if (!totalAntidumpingDutyAmountPayable.HasValue)
				{
					totalAntidumpingDutyAmountPayable = GetTotalAntidumpingDutyAmountPayableFromCharges();
				}
				return totalAntidumpingDutyAmountPayable.Value;
			}
		}
		ZDecimal? totalAntidumpingDutyAmountPayable;

		protected abstract ZDecimal GetTotalAntidumpingDutyAmountPayableFromCharges();

		public override EntrySummary7501BillCollection EntryPrintBills
		{
			get { return entryPrintBills; }
		}
		readonly EntrySummary7501BillCollection entryPrintBills;

		public override EntrySummary7501LineCollection EntryPrintLines
		{
			get
			{
				if (entryPrintLines == null)
				{
					entryPrintLines = new EntrySummary7501LineCollection(Factory);

					if (DocPrintingData != null && DocPrintingData.Count > 0)
					{
						entryPrintLines = LinesFromMessageAndSnapshot();
					}
				}

				return entryPrintLines;
			}
		}
		internal EntrySummary7501LineCollection entryPrintLines;

		protected abstract EntrySummary7501LineCollection LinesFromMessageAndSnapshot();

		protected bool DetermineShouldPrintInvoiceDetailsFlag<T>(ZGuid invoiceLinePK, Func<T> getNextLine)
			where T : MessageBlock
		{
			var result = false;
			CusEntryLine entryLine = null;
			var invoiceLine = Factory.Load<JobComInvoiceLine>(invoiceLinePK);
			if (invoiceLine != null)
			{
				entryLine = invoiceLine.CusEntryLine != null ?
					(invoiceLine.CusEntryLine.IsSecondaryTariffLine ? invoiceLine.CusEntryLine.ParentLine : invoiceLine.CusEntryLine)
					: null;
			}

			if (entryLine != null)
			{
				result = ((ICusEntryLine)entryLine).InvDelimter > 0 || getNextLine() == null;
			}
			else
			{
				result = getNextLine() == null;
			}
			return result;
		}

		internal List<US7501DocPrinting> DocPrintingData
		{
			get
			{
				if (docPrintingData == null)
				{
					var entry = Declaration.FormalEntry;
					if (entry != null)
					{
						IEnumerable<US7501DocPrinting> docPrintings = entry.US7501DocPrintingData.Find(x => x.US_MsgPK == outgoingMessage.PK);
						docPrintingData = new List<US7501DocPrinting>(new TypedEnumerable<US7501DocPrinting>(docPrintings));

						if (docPrintingData.Count == 0)
						{
							entry.CreateDocPrintingDetails(outgoingMessage.PK);
							docPrintings = entry.US7501DocPrintingData.Find(x => x.US_MsgPK == outgoingMessage.PK);
							docPrintingData = new List<US7501DocPrinting>(new TypedEnumerable<US7501DocPrinting>(docPrintings));
						}
					}
				}

				return docPrintingData;
			}
		}
		List<US7501DocPrinting> docPrintingData;

		internal US7501DocPrinting DocPrintingDataForCurrentLine(ZInt lineNumber)
		{
			US7501DocPrinting docDataForLine = null;
			foreach (US7501DocPrinting docPrintingDetails in DocPrintingData)
			{
				if (docPrintingDetails.US_LineNo == lineNumber && docPrintingDetails.US_SecondaryLineNo == 0)
				{
					docDataForLine = docPrintingDetails;
					break;
				}
			}

			return docDataForLine;
		}

		internal US7501DocPrinting[] DocPrintingDataForCurrentChildLines(ZInt lineNumber)
		{
			List<US7501DocPrinting> docDataForLines = new List<US7501DocPrinting>();

			foreach (US7501DocPrinting docPrintingDetails in DocPrintingData)
			{
				if (docPrintingDetails.US_LineNo == lineNumber && docPrintingDetails.US_SecondaryLineNo > 0)
				{
					docDataForLines.Add(docPrintingDetails);
				}
			}

			return docDataForLines.ToArray();
		}

		internal ZString[] GetDutyPercentageStringsForLine(ZInt lineNumber)
		{
			ZString[] result = new ZString[8];

			foreach (US7501DocPrinting docPrintingDetails in DocPrintingData)
			{
				if (docPrintingDetails.US_LineNo == lineNumber)
				{
					if (docPrintingDetails.US_SecondaryLineNo > 0)
					{
						result[docPrintingDetails.US_SecondaryLineNo] = docPrintingDetails.US_RateAsString;
					}
					else if (docPrintingDetails.US_LineNo == lineNumber)
					{
						result[0] = docPrintingDetails.US_RateAsString;
					}
				}
			}

			return result;
		}

		internal void DetermineWatchSectionPrintingFlags()
		{
			if (DocPrintingData != null && DocPrintingData.Count > 0)
			{
				int currentLineNo = 0;
				foreach (EntryMessageLine lineBlocks in LineObjectCollection)
				{
					currentLineNo++;
					US7501DocPrinting docData = DocPrintingDataForCurrentLine(currentLineNo);
					if (docData != null)
					{
						entryHasProRatedCalculation |= !docData.US_ProRatedLine1.IsEmpty;
						entryHasAdValoremConversionCalculation |= !docData.US_AVWatches.IsEmpty;
					}
				}
			}
		}

		protected override bool IsSeaTransportMode
		{
			get
			{
				return USTransportMode == TransportModeCodes.Codes.VesselContainer ||
					USTransportMode == TransportModeCodes.Codes.VesselNonContainer;
			}
		}

		protected override bool IsAirOrHandCarry
		{
			get
			{
				return USTransportMode == TransportModeCodes.Codes.AirContainer ||
					USTransportMode == TransportModeCodes.Codes.AirNonContainer ||
					USTransportMode == TransportModeCodes.Codes.PassengerHandCarried;
			}
		}

		public override ZString MissingDoc1
		{
			get { return IsConsumptionFTZ ? ZString.Empty : MissingDoc1Core; }
		}
		protected abstract ZString MissingDoc1Core { get; }

		public override ZString MissingDoc2
		{
			get { return IsConsumptionFTZ ? ZString.Empty : MissingDoc2Core; }
		}
		protected abstract ZString MissingDoc2Core { get; }

		internal ZBool MultipleExport
		{
			get { return UniqueCountryOfExport == USConstants.MultipleValueIndicator; }
		}

		internal ZBool MultipleOrigins
		{
			get { return UniqueCountryOfOrigin == USConstants.MultipleValueIndicator; }
		}

		internal ZBool MultipleLadings
		{
			get { return UniquePortOfLading == USConstants.MultipleValueIndicator; }
		}

		internal ZBool MultipleManufacturers
		{
			get { return ManufacturerID == USConstants.MultipleValueIndicator; }
		}

		public override ZString USTransportMode
		{
			get { return IsConsumptionFTZ ? ZString.Empty : USTransportModeCore; }
		}
		protected abstract ZString USTransportModeCore { get; }

		protected ZString MPFRateAsString
		{
			get
			{
				if (!mpfRateAsStringCached.HasValue)
				{
					mpfRateAsStringCached = entry.US_MPFRate;

					if (mpfRateAsStringCached.Value.IsEmpty)
					{
						if (Declaration.DateForMPFCalculation >= new ZDateTime(2011, 10, 01) && outgoingMessage.EM_SystemCreateTimeUtc >= new ZDateTime(2011, 11, 05))
						{
							mpfRateAsStringCached = MPFCalculator.Constants.Rate3464;
						}
						else
						{
							mpfRateAsStringCached = MPFCalculator.Constants.Rate21;
						}
					}
				}
				return mpfRateAsStringCached.Value;
			}
		}
		ZString? mpfRateAsStringCached;

		readonly EDIMessage outgoingMessage;
		internal EntryMessageLineCollection LineObjectCollection;
		bool iORCannotBeDetermined;
		bool ultimateConsigneeCannotBeDetermined;
		const string unableToDetermineImporter = "***        Organization N&A cannot be determined        ***";
		const string unableToDetermineImporterLine2 = "*** multiple organizations with the same Importer No. ***";
		const string unableToDetermineConsignee = "***          Organization N&A cannot be determined           ***";
		const string unableToDetermineConsigneeLine2 = "*** multiple organizations with the same Consignee No. ***";
	}
}

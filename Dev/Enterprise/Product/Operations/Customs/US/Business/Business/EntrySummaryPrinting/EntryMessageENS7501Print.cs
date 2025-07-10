using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntryMessageENS7501Print : EntrySummaryPrintingFromMessage
	{
		public EntryMessageENS7501Print(CusEntryHeader entryHeader, EDIMessage outgoingMessage, MQEDIMessage incomingMessage, EDIMessage bluMessage)
			: base(entryHeader, outgoingMessage, incomingMessage.HasCensusWarnings, bluMessage)
		{
			if (incomingMessage == null)
			{
				throw new ArgumentException("Response message not found");
			}

			var ens43RulingList = new List<ENS43>();
			var ens62ChargesList = new List<ENS62>();
			var ens81AdditionalSecondaryTariffList = new List<ENS81>();

			var outMessageBlocks = outgoingMessage.MessageBlock.MessageBlocks;
			foreach (var block in outMessageBlocks)
			{
				switch (block.MandatoryCharacters)
				{
					case "10":
						ens10 = (ENS10)block;
						break;
					case "20":
						ens20 = (ENS20)block;
						break;
					case "22":
						blocksForBillPrinting.Add(block);
						break;
					case "30":
						ens30 = (ENS30)block;
						break;
					case "35":
						ens35 = (ENS35)block;
						break;
					case "40":
						ens43RulingList = new List<ENS43>();
						ens62ChargesList = new List<ENS62>();
						ens81AdditionalSecondaryTariffList = new List<ENS81>();
						var ens40 = (ENS40)block;
						ens40Lines.Add(ens40);
						printLineObject = new EntryMessageLine(Factory);
						printLineObject.ens40 = ens40;
						LineObjectCollection.Add(printLineObject);
						CalculateHasInvDelimiterBeenSent(ens40);
						break;
					case "42":
						var ens42 = (ENS42)block;
						printLineObject.ens42 = ens42;
						break;
					case "43":
						var ens43 = (ENS43)block;
						ens43RulingList.Add(ens43);
						printLineObject.ens43 = ens43RulingList;
						break;
					case "50":
						var ens50 = (ENS50)block;
						ens50Lines.Add(ens50);
						printLineObject.ens50 = ens50;
						entryHasADDCVDLines |= !ens50.Quantity2.IsEmpty;
						break;
					case "51":
						var ens51 = (ENS51)block;
						printLineObject.ens51 = ens51;
						break;
					case "52":
						var ens52 = (ENS52)block;
						printLineObject.ens52 = ens52;
						break;
					case "60":
						var ens60 = (ENS60)block;
						ens60Lines.Add(ens60);
						printLineObject.ens60 = ens60;
						entryHasADDCVDLines |= !ens60.AntidumpingCaseNumber.IsEmpty || !ens60.CountervailingCaseNumber.IsEmpty;
						break;
					case "62":
						var ens62 = (ENS62)block;
						ens62ChargesList.Add(ens62);
						printLineObject.ens62 = ens62ChargesList;
						break;
					case "70":
						var ens70 = (ENS70)block;
						printLineObject.ens70 = ens70;
						entryHasSecondaryTariffLines = true;
						break;
					case "80":
						var ens80 = (ENS80)block;
						printLineObject.ens80 = ens80;
						entryHasAdditionalTariffLines = true;
						break;
					case "81":
						var ens81 = (ENS81)block;
						ens81AdditionalSecondaryTariffList.Add(ens81);
						printLineObject.ens81 = ens81AdditionalSecondaryTariffList;
						break;
					case "89":
						var ens89 = (ENS89)block;
						ens89Lines.Add(ens89);
						break;
					case "90":
						ens90 = (ENS90)block;
						break;
				}
			}

			DetermineWatchSectionPrintingFlags();
			billPrintManager.PopulateElements(blocksForBillPrinting);

			#region Data from Incoming message

			var inMessageBlocks = incomingMessage.EM_MessageText.Split(80);
			foreach (ZString block in inMessageBlocks)
			{
				string blockType = block.Substring(0, 2);
				switch (blockType)
				{
					case "E0":
						var ense0 = new ENSE0();
						ense0.Deserialise(block);
						if (ense0.ErrorMessageIdentifier == USConstants.Paperless)
						{
							isPaperlessEntry = true;
						}

						if (!ense0.AssignedTeamNumber.IsEmpty)
						{
							assignedTeamNumber = ense0.AssignedTeamNumber;
						}

						break;

					case "E9":
						ensexx.Deserialise(block);
						if (!ensexx.AssignedTeamNumber.IsEmpty)
						{
							assignedTeamNumber = ensexx.AssignedTeamNumber;
						}

						break;
				}
			}

			#endregion
		}

		void CalculateHasInvDelimiterBeenSent(ENS40 ens40)
		{
			hasInvDelimiterBeenSent |= !ens40.InvoiceDelimiter.IsEmpty;
		}

		readonly EntryMessageLine printLineObject;

		readonly ENS10 ens10 = new ENS10();
		readonly ENS20 ens20 = new ENS20();
		readonly ENS30 ens30 = new ENS30();
		readonly ENS35 ens35 = new ENS35();
		readonly List<ENS40> ens40Lines = new List<ENS40>();
		readonly List<ENS50> ens50Lines = new List<ENS50>();
		readonly List<ENS60> ens60Lines = new List<ENS60>();
		readonly List<ENS89> ens89Lines = new List<ENS89>();
		readonly ENS90 ens90 = new ENS90();
		readonly ENSEXX ensexx = new ENSEXX();

		readonly ZBool isPaperlessEntry;
		readonly ZString assignedTeamNumber;
		bool hasInvDelimiterBeenSent;

		#region Overriden Document Properties

		protected override bool IsACE
		{
			get { return false; }
		}

		public override ZDateTime DateOfFirstArrival
		{
			get { return ens20.DateOfImportation; }
		}

		public override ZBool DecFinalWithdrawal
		{
			get { return ens30.FinalWarehouseIndicator == "1"; }
		}

		public override ZBool DeferredTaxToBePaidByEFT
		{
			get { return ens90.DeferredTaxIndicator == TaxDeferIndicatorList.Codes.DeferredTaxWithEFT; }
		}

		protected override ZString EffectiveUltimateConsigneeCustomsRegNoCore
		{
			get
			{
				return IsConsumptionFTZ ?
						  ens10.UltimateConsigneeNumber :
						  ens10.UltimateConsigneeNumber != ens10.ImporterOfRecordNumber ?
							  ens10.UltimateConsigneeNumber :
							  new ZString(USConstants.Same);
			}
		}

		public override ZString EntryFilerCode
		{
			get { return ens10.EntryFilerCode; }
		}

		public override ZString EntryType
		{
			get { return ens10.EntryType.ToString(); }
		}

		public override ZString EntryTypeCode
		{
			get { return "ABI/" + GetABIStatusIndicator(PaymentType) + (ens10.LiveEntryIndicator == 1 ? "/L" : ""); }
		}

		public override ZString EntryNo
		{
			get { return ens10.EntryNumber; }
		}

		public override ZString FormattedEntryNumber
		{
			get
			{
				return ens10.EntryFilerCode + "-" + ens10.EntryNumber.SubstringSafe(0, 7) + "-" + ens10.EntryNumber.SubstringSafe(7, 1);
			}
		}

		public override ZString USTeamNo
		{
			get { return assignedTeamNumber; }
		}

		public override ZString BondType
		{
			get { return ens10.BondType; }
		}

		protected override ZString ImporterOfRecordCustomsRegNoCore
		{
			get { return ens10.ImporterOfRecordNumber; }
		}

		protected override ZString VesselName
		{
			get { return ens20.ImportingVesselName; }
		}

		protected override ZString PipelineName
		{
			get { return ZString.Empty; }
		}

		protected override ZString CarrierCode
		{
			get { return ens30.CarrierCode; }
		}

		protected override ZString FTZNumber
		{
			get { return IsConsumptionFTZ && VesselName.StartsWith("FTZ") ? VesselName : ZString.Empty; }
		}

		protected override ZDate ExportDateFromFirstLine
		{
			get
			{
				var firstElement = ens50Lines.FirstOrDefault();
				return firstElement != null ? firstElement.DateOfExportation : ZDate.Empty;
			}
		}

		protected override bool HasMultiExportDates
		{
			get
			{
				if (!hasMultiExportDates.HasValue)
				{
					hasMultiExportDates = UniqueValueCalculator.HasMultiValues(ens50Lines, x => x.DateOfExportation);
				}
				return hasMultiExportDates.Value;
			}
		}
		bool? hasMultiExportDates;

		public override ZString UniqueCountryOfOrigin
		{
			get
			{
				if (!uniqueCountryOfOriginCached.HasValue)
				{
					uniqueCountryOfOriginCached = UniqueValueCalculator.GetUniqueValue(ens40Lines, x => x.CountryOfOrigin, (ZString)USConstants.MultipleValueIndicator);
				}

				return uniqueCountryOfOriginCached.Value;
			}
		}
		ZString? uniqueCountryOfOriginCached;

		ZBool MultipleRelationships
		{
			get
			{
				if (!multipleRelationships.HasValue)
				{
					multipleRelationships = UniqueValueCalculator.HasMultiValues(ens50Lines, x => x.RelatedPartyIndicator);
				}
				return multipleRelationships.Value;
			}
		}
		ZBool? multipleRelationships;

		public override ZString SchDArrival
		{
			get { return EntryType == EntryTypeList.Codes.ConsumptionFTZ ? ZString.Empty : ens20.DistrictPortOfUnlading; }
		}

		public override ZString SchDEntry
		{
			get { return ens10.DistrictPortOfEntry; }
		}

		protected override ZString USTransportModeCore
		{
			get { return ens20.ModeOfTransportationMOTCode; }
		}

		public override ZString SuretyCode
		{
			get { return ens10.SuretyCode.ToString(); }
		}

		public override ZString ManufacturerID
		{
			get
			{
				if (!manufacturerIDCached.HasValue)
				{
					manufacturerIDCached = UniqueValueCalculator.GetUniqueValue(ens60Lines, x => x.ManufacturerSupplierCode, (ZString)USConstants.MultipleValueIndicator);
				}

				return manufacturerIDCached.Value;
			}
		}
		ZString? manufacturerIDCached;

		protected override ZString MissingDoc1Core
		{
			get { return ens10.MissingDocumentCodes.SubstringSafe(0, 1); }
		}

		protected override ZString MissingDoc2Core
		{
			get { return ens10.MissingDocumentCodes.SubstringSafe(1, 1); }
		}

		protected override ZString UltimateStateCore
		{
			get { return ens10.StateOfDestination; }
		}

		protected override ZString PaymentTypeFromEntrySummary
		{
			get { return ens30.PaymentTypeIndicator; }
		}

		public override ZString UniqueCountryOfExport
		{
			get
			{
				if (!uniqueCountryOfExportCached.HasValue)
				{
					uniqueCountryOfExportCached = ZString.Empty;

					if (!IsConsumptionFTZ)
					{
						uniqueCountryOfExportCached = UniqueValueCalculator.GetUniqueValue(ens50Lines, x => x.CountryOfExport, (ZString)USConstants.MultipleValueIndicator);
					}
					else
					{
						EntryMessageLine lineWithHighestCustomsValue = null;

						foreach (EntryMessageLine line in LineObjectCollection)
						{
							if (lineWithHighestCustomsValue == null || lineWithHighestCustomsValue != line && line.ens40.Value > lineWithHighestCustomsValue.ens40.Value)
							{
								lineWithHighestCustomsValue = line;
							}
						}
						uniqueCountryOfExportCached = lineWithHighestCustomsValue != null ? lineWithHighestCustomsValue.ens50.CountryOfExport : ZString.Empty;
					}
				}
				return uniqueCountryOfExportCached.Value;
			}
		}
		ZString? uniqueCountryOfExportCached;

		protected override ZString GetPortOfLadingFromEntryLines()
		{
			if (!portOfLadingFromEntryLinesCached.HasValue)
			{
				portOfLadingFromEntryLinesCached = UniqueValueCalculator.GetUniqueValue(ens40Lines, x => x.PortOfLading, (ZString)USConstants.MultipleValueIndicator);
			}
			return portOfLadingFromEntryLinesCached.Value;
		}
		ZString? portOfLadingFromEntryLinesCached;

		protected override ZString LocationOfGoodsAndNameCore
		{
			get
			{
				return CachedValueHelper.GetValue(
					ref cachedLocationOfGoodsAndName,
					() =>
					{
						var result = base.LocationOfGoodsAndNameCore;

						if (result.IsEmpty)
						{
							result = billPrintManager.BLUFirmsCode.IsEmpty ? ens20.LocationOfGoods : billPrintManager.BLUFirmsCode;
							var firms = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, result, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
							result += firms != null ? ("/" + firms.ZZD_Description) : string.Empty;
						}

						if (result.IsEmpty && Declaration.IsAir)
						{
							result = billPrintManager.BLUVoyageFlight.IsEmpty ? ens20.VoyageFlightTripManifestNumber : billPrintManager.BLUVoyageFlight;
						}

						return result;
					});
			}
		}

		CachedValue<ZString> cachedLocationOfGoodsAndName;

		protected override ZString Block24ReferenceNumberCore
		{
			get { return ens10.CBPF4811ReferenceNumber; }
		}

		protected override ZString WarehouseEntryNoForWarehouseEntryWithdrawal
		{
			get
			{
				var result = ZString.Empty;

				if (!ens30.WarehouseEntryNumber.IsEmpty)
				{
					result = ens30.EntryFilerCodeOfWarehouseEntry + "-" + ens30.WarehouseEntryNumber.SubstringSafe(0, 7) + "-" + ens30.WarehouseEntryNumber.SubstringSafe(7, 1);
				}

				return result;
			}
		}

		protected override void AddFeesFrom89Lines()
		{
			foreach (ENS89 feeSummary in ens89Lines)
			{
				if (!feeSummary.ClassCode.IsEmpty)
				{
					AddFee(feeSummary.ClassCode, feeSummary.TotalAmount);
				}
				if (!feeSummary.ClassCode1.IsEmpty)
				{
					AddFee(feeSummary.ClassCode1, feeSummary.TotalAmount1);
				}
				if (!feeSummary.ClassCode2.IsEmpty)
				{
					AddFee(feeSummary.ClassCode2, feeSummary.TotalAmount2);
				}
				if (!feeSummary.ClassCode3.IsEmpty)
				{
					AddFee(feeSummary.ClassCode3, feeSummary.TotalAmount3);
				}
				if (!feeSummary.ClassCode4.IsEmpty)
				{
					AddFee(feeSummary.ClassCode4, feeSummary.TotalAmount4);
				}
			}
		}

		protected override ZInt GetSummaryChargesCountFromEntryLines()
		{
			ZInt result = 0;
			List<ZString> differentChargesList = new List<ZString>();

			foreach (EntryMessageLine lineObject in LineObjectCollection)
			{
				foreach (ENS62 chargeDetail in lineObject.ens62)
				{
					string chargeDetailCode = chargeDetail.ClassCode.ToString().PadLeft(3, '0');
					if (!differentChargesList.Contains(chargeDetailCode))
					{
						differentChargesList.Add(chargeDetailCode);
						result++;

						if (result > 4)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		protected override ZString CalculatedPaperlessStatusFromDispositions
		{
			get
			{
				var result = ZString.Empty;

				ZDateTime lastStatusDate = ZDateTime.MinSmallDateTimeValue;
				foreach (ErrorsRecord errRec in Declaration.ENSE0Records)
				{
					if (errRec.StatusDate >= lastStatusDate)
					{
						lastStatusDate = errRec.StatusDate;
						if (errRec.ErrorMessageIdentifier == USConstants.DocsRequiredStatus.DocsRequired || errRec.ErrorMessageIdentifier == USConstants.DocsRequiredStatus.DocsNowRequired || errRec.ErrorMessageIdentifier == USConstants.DocsRequiredStatus.PaperRequired || errRec.NarrativeMessage == USConstants.RecordsRequired || errRec.NarrativeMessage == USConstants.PaperRequired)
						{
							result = USConstants.EntrySummaryDisposition.DocsRequired;
						}
						else if (errRec.ErrorMessageIdentifier == USConstants.Paperless)
						{
							result = USConstants.EntrySummaryDisposition.Paperless;
						}
					}
				}
				return result;
			}
		}

		protected override ZString CalculatedPaperlessStatusFromDeclaration
		{
			get
			{
				var result = ZString.Empty;

				if (isPaperlessEntry
					|| entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings
					|| entry.Declaration.EntrySummaryStatus == ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings)
				{
					result = USConstants.EntrySummaryDisposition.Paperless;
				}
				return result;
			}
		}

		public override ZBool TaxToBeDeferred
		{
			get { return ens90.DeferredTaxIndicator == TaxDeferIndicatorList.Codes.DeferredTax; }
		}

		protected override ZString DeferredTaxIndicator
		{
			get { return ens90.DeferredTaxIndicator; }
		}

		// CS00113346 - For Warehouse entry, Duty is not payable on entry. It should be shown on entry lines but not included in the box 37 & 38 totals.
		public override ZDecimal TotalDutyAmt
		{
			get { return EntryTypeList.IsOnlyHMFPayable(EntryType) || EntryTypeList.IsNothingPayable(EntryType) ? ZDecimal.Zero : ens90.TotalEstimatedDuty; }
		}

		public override ZDecimal TotalEnteredValue
		{
			get { return ens90.TotalValueOfEntrySummary; }
		}

		public override ZDecimal TotalEstTax
		{
			get { return ens90.GrandTotalEstimatedTax; }
		}

		protected override ZDecimal GetTotalLineLevelHMFsFromCharges()
		{
			ZDecimal result = 0;

			foreach (EntryMessageLine lineObject in LineObjectCollection)
			{
				foreach (ENS62 chargeDetail in lineObject.ens62)
				{
					if (chargeDetail.ClassCode == Core.Constants.USCustoms.FeeCodes.HMF)
					{
						result += chargeDetail.UserFeeAmount;
					}
				}
			}
			return result;
		}

		protected override ZDecimal EntryHMF
		{
			get { return ens89Lines.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		protected override ZDecimal GetTotalAntidumpingDutyAmountPayableFromCharges()
		{
			var totalAntidumpingDutyAmountPayable = 0m;
			foreach (ENS60 ens60 in ens60Lines)
			{
				if (ens60.BondedADDIndicator == "0")
				{
					totalAntidumpingDutyAmountPayable += ens60.AntidumpingDuty;
				}
			}
			return totalAntidumpingDutyAmountPayable;
		}

		protected override ZDecimal GetTotalCountervailingDutyPayableFromCharges()
		{
			var totalCountervailingDutyPayable = 0m;
			foreach (ENS60 ens60 in ens60Lines)
			{
				if (ens60.BondedCVDIndicator == "0")
				{
					totalCountervailingDutyPayable += ens60.CountervailingDuty;
				}
			}
			return totalCountervailingDutyPayable;
		}

		protected override ZDecimal GrandTotalFeeAmount
		{
			get { return ens90.GrandTotalFeeAmount; }
		}

		protected override ZDecimal TotalAntidumpingDutyAmount
		{
			get { return ens90.TotalAntidumpingDutyAmount; }
		}

		protected override ZDecimal TotalCountervailingDutyAmount
		{
			get { return ens90.TotalCountervailingDutyAmount; }
		}

		protected override EntrySummary7501LineCollection LinesFromMessageAndSnapshot()
		{
			int currentLineNo = 0;
			ZGuid previousInvoicePK = ZGuid.Empty;

			foreach (EntryMessageLine lineBlocks in LineObjectCollection)
			{
				currentLineNo++;

				ZString[] dutyPercentageStrings = GetDutyPercentageStringsForLine(currentLineNo);
				US7501DocPrinting docData = DocPrintingDataForCurrentLine(currentLineNo);
				US7501DocPrinting[] docDataForChildLines = DocPrintingDataForCurrentChildLines(currentLineNo);

				if (docData != null)
				{
					bool printInvoiceHeading = previousInvoicePK != docData.US_InvoicePK;
					bool printInvoiceDetails = ShouldPrintInvoiceDetails(lineBlocks.ens40, docData.US_InvoiceLinePK);
					lineBlocks.MPFRate = MPFRateAsString;

					var line = new EntryMessageENS7501Line
								(
									lineBlocks,
									MultipleOrigins,
									MultipleExport,
									MultipleLadings,
									MultipleManufacturers,
									MultipleRelationships,
									HasMultiExportDates,
									printInvoiceHeading,
									printInvoiceDetails,
									ens35.ADDCVDSuretyCode,
									dutyPercentageStrings,
									docData,
									docDataForChildLines,
									EntryType
								);
					entryPrintLines.Add(line);

					previousInvoicePK = docData.US_InvoicePK;
				}
			}

			return entryPrintLines;
		}

		//Checks if a current line is an ending line of an invoice 
		bool ShouldPrintInvoiceDetails(ENS40 currentENS40, ZGuid invoiceLinePK)
		{
			bool result = false;

			if (hasInvDelimiterBeenSent)
			{
				result = !currentENS40.InvoiceDelimiter.IsEmpty;
			}
			else
			{
				result = DetermineShouldPrintInvoiceDetailsFlag(invoiceLinePK, () => ens40Lines.Find(x => x.LineItemNumber == currentENS40.LineItemNumber + 1));
			}
			return result;
		}

		protected override bool IsNAFTAReconIndicator
		{
			get { return !ens20.TradeAgreementReconciliationIndicator.IsEmpty; }
		}

		protected override ZString ENSOtherIssueCode
		{
			get { return ens20.OtherReconciliationIndicator; }
		}

		protected override ZString ENSOtherIssueCodeDescription
		{
			get { return Factory.GetCachedValue<ReconIssueCodeList>().GetDescriptionFromCode(ReconIssueCodeList.ConvertFromENSIssueCode(ens20.OtherReconciliationIndicator)); }
		}

		#endregion
	}
}

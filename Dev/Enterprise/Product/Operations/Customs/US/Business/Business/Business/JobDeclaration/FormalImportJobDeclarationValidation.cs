using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataRegistry.Business;

namespace Enterprise.Customs.US.Business
{
	public class FormalImportJobDeclarationValidation : CommonImportJobDeclarationValidation
	{
		public FormalImportJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckWHSInvLineFilter()
		{
			if (Parent.IsInwardBondedWarehousingEnabled && Parent.PackableInvoiceLines.Count > 0)
			{
				ListValidation.WarnIfInvalidPK(Parent.WHSInvLineFilterInfo);
			}
		}

		protected override void CheckWHSPackageFilter()
		{
			if (Parent.IsInwardBondedWarehousingEnabled && Parent.WHSPackLines.Count > 0)
			{
				ListValidation.WarnIfInvalidPK(Parent.WHSPackageFilterInfo);
			}
		}

		protected override void CheckWHSProductFilter()
		{
			if (Parent.IsInwardBondedWarehousingEnabled && Parent.PackableInvoiceLines.Count > 0)
			{
				ListValidation.WarnIfInvalidCode(Parent.WHSProductFilterInfo);
			}
		}

		protected override void CheckJE_OH_ShippingLine()
		{
			base.CheckJE_OH_ShippingLine();

			if (Parent.IsACECargoReleaseValidationMode && Parent.PGAFlags.HasInvoiceLinesWithPST)
			{
				if (Parent.JE_OH_ShippingLine.IsEmpty)
				{
					Parent.JE_OH_ShippingLineInfo.AddMessageError(CarrierRequiredForPST);
				}
				else
				{
					OrganisationValidation.ValidatePGAContact(Parent.JE_OH_ShippingLineInfo, OrgHeaderWrapper.New(Parent.ShippingLine, OrgHeaderWrapper.GetAddressForPSTCarrier));
				}
			}
		}

		internal const string CarrierRequiredForPST = "Carrier is required for Pesticide reporting.";

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();

			var originalApplicationCode = (ZString)Parent.JE_ApplicationCodeInfo.OriginalValue;
			if (!originalApplicationCode.IsEmpty && Parent.JE_ApplicationCode != originalApplicationCode)
			{
				var entrySummaryEntry = Parent.ActiveEntryHeaders.EntrySummaryEntry;

				if (entrySummaryEntry != null && entrySummaryEntry.HasActiveTransactionsWithCustoms && !entrySummaryEntry.HasBeenWithdrawn)
				{
					Parent.JE_ApplicationCodeInfo.AddError(string.Format(CantChangeApplicationCodeUntilWithdrawn, originalApplicationCode));
				}
			}

			ListValidation.ErrorIfInvalidCode(Parent.JE_ApplicationCodeInfo, Parent.Lookups.ApplicationCodeList, (ZArchitecture.Core.NoResString)"Message Mode");

			if (Parent.JE_ApplicationCode.IsEmpty && !Parent.US_EnableENS && !Parent.US_EnableCRL && Parent.US_EnableSPN)
			{
				Parent.JE_ApplicationCodeInfo.AddMessageError(StandAlonePriorNoticeRequiresMessageMode);
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ApplicationCodeInfo, "Message Mode");
			}

			if (Parent.JE_ApplicationCode == JobApplicationCodeList.Codes.ACS && Parent.ShouldForceFilingInACECargoRelease())
			{
				Parent.JE_ApplicationCodeInfo.AddMessageError(MessageModeMustBeACE);
			}

			Parent.AddInfoValidation.ValidateUS_EnableCRL();
			Parent.AddInfoValidation.ValidateUS_EnableSPN();
		}
		internal const string MessageModeMustBeACE = "Message Mode Must be ACE for this entry type.";

		internal const string CantChangeApplicationCodeUntilWithdrawn = "This cannot be changed now as 7501 has been accepted by customs and still active at {0}. Please delete 7501 from {0} first.";
		internal const string StandAlonePriorNoticeRequiresMessageMode = "Stand Alone Prior Notice requires Message Mode. If ACE is selected, ACE Stand Alone Prior Notice will be sent. Otherwise ACS Stand Alone Prior Notice will be sent.";

		protected override void CheckJE_OH_ExternalBroker()
		{
			base.CheckJE_OH_ExternalBroker();

			if (IsEntrySummaryValidationMode && Parent.ExternalBroker != null)
			{
				if (!Parent.HasBIRDCommunicationMode())
				{
					Parent.JE_OH_ExternalBrokerInfo.AddWarning(NoValidCommunicationMethodIsDefinedForBIRD);
				}
			}
		}
		public const string NoValidCommunicationMethodIsDefinedForBIRD = "No EDI communication mode is specified for a module, 'BIRD' against this broker. The system will not be able to send BIRD files automatically.";

		protected override void CheckDecEntryNumber()
		{
			base.CheckDecEntryNumber();

			var entryNumber = Parent.DecEntryNumber;
			if (Parent.IsImportByExternalBroker)
			{
				if (!Parent.US_EntryFilerCode.IsEmpty && !entryNumber.IsEmpty)
				{
					CusEntryNumber[] entryNumbers = CusEntryNumber.Load(Parent.Factory, CusEntryHeaderMessageTypeList.Codes.EntrySummary, entryNumber, Core.Constants.CountryCodes.UnitedStates);

					foreach (CusEntryNumber entryNum in entryNumbers)
					{
						JobDeclaration attachedDec = entryNum.GetJobDeclaration();

						if (attachedDec != null && attachedDec != Parent && entryNum.GetEntryFilerCode() == Parent.US_EntryFilerCode)
						{
							Parent.DecEntryNumberInfo.AddError(string.Format(DuplicateEntryNumber, attachedDec.JE_DeclarationReference));
							break;
						}
					}
				}
			}

			if (Parent.US_EnableENS || Parent.US_EnableCRL)
			{
				if (entryNumber.IsEmpty)
				{
					var reasonNotToAllocate = Parent.DisallowAllocateImportEntryNumber;

					if (!string.IsNullOrEmpty(reasonNotToAllocate))
					{
						Parent.DecEntryNumberInfo.AddMessageError(reasonNotToAllocate);
					}
				}
				else if (entryNumber.Length > MQEDIMessage.USEntryNumberPlaceHolder.Length)
				{
					Parent.DecEntryNumberInfo.AddError(string.Format(MaxLengthExceeded, entryNumber, MQEDIMessage.USEntryNumberPlaceHolder.Length));
				}
			}
		}

		public const string MaxLengthExceeded = "The entry number '{0}' has exceeded the max length '{1}'. Please assign a different number.";
		public const string DuplicateEntryNumber = "The job, '{0}' also has the entered entry number. Please assign a different number.";

		protected override void CheckJE_PrimaryITNumber()
		{
			base.CheckJE_PrimaryITNumber();

			if (IsEntrySummaryOrCargoReleaseValidationMode && Parent.PrimaryMasterBill != null)
			{
				if (Parent.US_EntryType != EntryTypeList.Codes.ReWarehouse && Parent.ITNumbersFromBills.Count == 0 && Parent.IsBillDetailRequired())
				{
					if (Parent.IsBorderMovement)
					{
						if (!Parent.US_SchDArrival.IsEmpty && Parent.US_SchDArrival != Parent.US_SchDEntry)
						{
							Parent.JE_PrimaryITNumberInfo.AddMessageError(ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargePortDifferBCR);
						}
					}
					else if (!IsInSameDistrict(Parent.US_SchDArrival, Parent.US_SchDEntry))
					{
						Parent.JE_PrimaryITNumberInfo.AddMessageError(ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargeDistrictsDiffer);
					}
				}
			}

			Parent.AddInfoValidation.ValidateUS_SchDArrival();
		}

		public static bool IsInSameDistrict(ZString port1, ZString port2)
		{
			return
				port1.IsEmpty
				|| port2.IsEmpty
				|| port1.Left(2) == port2.Left(2)
				|| ((port1 == "4601" || port1 == "4701" || port1 == "1001") && (port2 == "4601" || port2 == "4701" || port2 == "1001"));
		}

		protected override bool ShouldCheckJE_ExportDate
		{
			get { return IsEntrySummaryOrCargoReleaseValidationMode && !Parent.IsConsumptionFTZ; }
		}

		protected override ZString JE_ExportDateMessageRequiredMessage
		{
			get { return ExportDateRequeredForESAndCR; }
		}

		internal const string ExportDateRequeredForESAndCR = "Date Of Export. Date of Export is required for Entry Summary and Cargo Release.";

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();

			if (IsEntrySummaryValidationMode)
			{
				var declaration = Parent;
				if (declaration.JE_DateOfArrival.IsEmpty)
				{
					if (MayRequireWarehousing && !(declaration.IsACE && declaration.IsReWarehouse) || IsConsumptionForNAFTARecon)
					{
						Parent.JE_DateOfArrivalInfo.AddMessageError(ZString.Format(DateOfImportationRequiredForACertainEntryType, declaration.IsACE ? "21/23" : "21/22/23"));
					}
				}
			}
		}
		internal const string DateOfImportationRequiredForACertainEntryType = "Date of Importation is required for entry types {0} and 01/02/06 if flagged for FTA Recon.";

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			if (IsEntrySummaryValidationMode || (Parent.IsACE && IsCargoReleaseValidationMode))
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JE_OH_ImporterInfo, OrgMatchedCustomsRegNoType.NULL, string.Empty, true, true);
			}

			if (IsEntrySummaryValidationMode && IsCBPF4811OnImporterRatherThanDeclarationOrIOR)
			{
				Parent.JE_OH_ImporterInfo.AddMessageError(MoveCBPF4811FromImporterToIOR);
			}
		}
		internal const string MoveCBPF4811FromImporterToIOR = "Please move CBPF 4811 Notify Party/CBPF 4811 Notify Party ID from Importer to Importer of Record (Organization -> Config -> US Defaults).";

		bool IsCBPF4811OnImporterRatherThanDeclarationOrIOR
		{
			get
			{
				var importerWrapper = Parent.ImporterWrapper;
				var iorWrapper = Parent.IORWrapper;
				return Parent.NotifyParty == null && Parent.JE_OH_Importer != Parent.IOROrgPK && importerWrapper != null && iorWrapper != null
					&& !importerWrapper.ZO_NPID.IsEmpty && iorWrapper.ZO_NPID.IsEmpty;
			}
		}

		protected override void CheckJE_MasterBillIssuerSCAC()
		{
			base.CheckJE_MasterBillIssuerSCAC();

			if (Parent.PrimaryMasterBill == null)
			{
				var isACECargoReleaseAndAirTransportMode = Parent.IsACECargoReleaseValidationMode && Parent.IsAir;

				if (!isACECargoReleaseAndAirTransportMode && DoesRequiredMasterBillIssuerSCAC &&
					Parent.JE_MasterBillIssuerSCAC.IsEmpty && Parent.IsMasterBillSCACRequired)
				{
					Parent.JE_MasterBillIssuerSCACInfo.AddMessageError(FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
				}
			}
			else if (Parent.IsACECargoReleaseValidationMode && TransportTypeList.IsIssuerSCACNotAllowed(Parent.JE_TransportMode) && !Parent.JE_MasterBillIssuerSCAC.IsEmpty)
			{
				Parent.JE_MasterBillIssuerSCACInfo.AddMessageError(IssuerSCACiSNotPermitted);
			}
		}

		internal const string IssuerSCACiSNotPermitted = "Issuer SCAC not allowed for this Transport Mode";

		protected override void CheckJE_HouseBillIssuerSCAC()
		{
			base.CheckJE_HouseBillIssuerSCAC();
			if (Parent.IsACECargoReleaseValidationMode && TransportTypeList.IsIssuerSCACNotAllowed(Parent.JE_TransportMode) && !Parent.JE_HouseBillIssuerSCAC.IsEmpty)
			{
				Parent.JE_HouseBillIssuerSCACInfo.AddMessageError(IssuerSCACiSNotPermitted);
			}
		}

		protected virtual bool DoesRequiredMasterBillIssuerSCAC
		{
			get { return true; }
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			if (!Parent.IsConsumptionFTZ)
			{
				base.CheckJE_VoyageFlightNo();

				if (Parent.IsEntrySummaryOrCargoReleaseValidationMode)
				{
					CheckAirOrHandCarryFlightNo();

					if (Parent.VoyageFlightNumber.Length > 5)
					{
						if (Parent.IsAir)
						{
							Parent.JE_VoyageFlightNoInfo.AddWarning(VoyageFlightNoTruncatedAir);
						}
						else if (Parent.IsSea || Parent.IsHandCarry)
						{
							Parent.JE_VoyageFlightNoInfo.AddWarning(VoyageFlightNoTruncated);
						}
					}
				}
			}
			else
			{
				if (!Parent.JE_VoyageFlightNo.IsEmpty)
				{
					Parent.JE_VoyageFlightNoInfo.AddWarning(ValidationConstants.Declaration.DataWillNoBeSentForEntrySummary);
				}
			}
		}

		internal const string VoyageFlightNoTruncated = "Only the first five characters will be sent in the Entry Summary and/or Cargo Release messages.";
		internal const string VoyageFlightNoTruncatedAir = "Only the first five characters after the carrier code will be sent in the Entry Summary and/or Cargo Release messages.";

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();

			Parent.AddInfoValidation.ValidateUS_IsInvoiceByRequest();
			ValidateJE_PrimaryITNumber();
			Parent.AddInfoValidation.ValidateUS_CargoReleaseType();
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();

			if (Parent.IsHandCarry)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.JE_MasterBillInfo);
			}

			if (Parent.IsEntryNumberToBeDefaultedToMasterBill)
			{
				if (Parent.JE_MasterBill.IsEmpty)
				{
					if (Parent.ImportEntryNumber.IsEmpty)
					{
						Parent.JE_MasterBillInfo.AddWarning(EmptyMasterBillNumberWarningForTruck);
					}
					else
					{
						Parent.JE_MasterBillInfo.AddMessageError(EmptyMasterBillNumberMessageErrorForTruck);
					}
				}
				else if (Parent.JE_MasterBill.Length == 11 && Parent.JE_MasterBill.Left(3) == Parent.US_EntryFilerCode && Parent.JE_MasterBill.SubstringSafe(3) != Parent.ImportEntryNumber)
				{
					Parent.JE_MasterBillInfo.AddWarning(MasterBillNumberForMXTruckIsDifferentToEntryNumber);
				}
			}
		}
		internal const string EmptyETAWarning = "You have not entered an ETA.";

		protected override void CheckJE_DateAtFinalDestination()
		{
			if (Parent.JE_DateAtFinalDestination.IsEmpty && USCustomsDataRegistry.Instance.EnableETAValidation.GetValueWithoutFallback(Parent.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				Parent.JE_DateAtFinalDestinationInfo.AddWarning(EmptyETAWarning);
			}
		}

		internal const string EmptyMasterBillNumberWarningForTruck = "No bill of lading entered. When an entry number is assigned, system will copy the filer code and entry number to this field.";
		internal const string EmptyMasterBillNumberMessageErrorForTruck = "No bill of lading entered.";
		internal const string MasterBillNumberForMXTruckIsDifferentToEntryNumber = "The Master Bill appears to be an Entry Number however it does not match the Entry Number on the job.";

		protected override void CheckJE_OA_ConsigneeAddress()
		{
			base.CheckJE_OA_ConsigneeAddress();

			if (!Parent.IsACE && IsCargoReleaseValidationMode || Parent.IsACE && IsEntrySummaryValidationMode)
			{
				bool shouldValidate = Parent.ActiveEntryHeaders.Count == 0;

				if (!shouldValidate && !Parent.IsACE)
				{
					foreach (CusEntryHeader entry in Parent.ActiveEntryHeaders)
					{
						if ((entry.IsCargoRelease || entry.IsBorderCargoRelease) && !entry.US_UseConsigneeNameAddress)//therefore needs some number
						{
							shouldValidate = true;
							break;
						}
					}
				}

				if (shouldValidate)
				{
					OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JE_OA_ConsigneeAddressInfo, OrgMatchedCustomsRegNoType.ECN, ZString.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"), true, false);
				}
			}

			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				var ultimateConsignee = Parent.ConsigneeOrgAddress;
				var importerOfRecord = Parent.IOR;

				if (ultimateConsignee != null && !ultimateConsignee.IsUSOrganisation()
						&& importerOfRecord != null && !importerOfRecord.IsUSOrganisation())
				{
					Parent.JE_OA_ConsigneeAddressInfo.AddWarning(ForeignBasedUltimateConsigneeWithIOR);
				}
			}
		}

		internal const string ForeignBasedUltimateConsigneeWithIOR = "You have entered a foreign-based Ultimate Consignee with a foreign-based Importer of Record.";

		#region IOROrgPK

		protected internal override void CheckIOROrgPK(ZPropertyInfo info)
		{
			base.CheckIOROrgPK(info);
			var declaration = Parent;

			if (IsEntrySummaryOrCargoReleaseValidationMode)
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(info, OrgMatchedCustomsRegNoType.EIN, IORCustomsRegNoRequired, IsImporterOfRecordMandatory, true);
			}

			new AuthorityToActValidator().Validate(declaration, declaration.IOR, info, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, "", PowerOfAttorneyValidator.ExtraMatchingConditionForImportDirectionAndPortOfEntry(Parent.US_SchDEntry));

			declaration.Validation.ValidateJE_OH_Importer();
		}
		internal const string IORCustomsRegNoRequired = "The Importer of Record should have EIN, SSN or CBP assigned number.";

		protected virtual ZBool IsImporterOfRecordMandatory
		{
			get { return true; }
		}

		#endregion

		#region Boolean Flags

		bool MayRequireWarehousing
		{
			get { return EntryTypeList.MayRequireWarehousing(Parent.US_EntryType); }
		}

		/// <summary>
		/// ***Use with caution*** 
		/// It is used to add a warning currently. If you want to be exact, you should consider using InvoiceLine.IsConsumptionForNAFTARecon
		/// </summary>
		bool IsConsumptionForNAFTARecon
		{
			get { return Parent.US_NAFTAReconIndicator && EntryTypeList.IsConsumptionForNAFTARecon(Parent.US_EntryType); }
		}

		public override bool IsMasterBillMandatory
		{
			get
			{
				bool result = false;

				if (Parent.IsMasterBillRelevant && !Parent.IsEntryNumberToBeDefaultedToMasterBill && Parent.US_EntryType != EntryTypeList.Codes.ReWarehouse)
				{
					result = !Parent.IsConsumptionFTZ && (!Parent.JE_MasterBillIssuerSCAC.IsEmpty || !Parent.IsExWarehouse);
				}

				return result;
			}
		}

		protected override bool ShouldValidatePackagesActualPackageCount => IsNonInBondValidationModeOn;

		internal bool IsNonInBondValidationModeOn => !Parent.IsInBondOnly || IsEntrySummaryOrCargoReleaseValidationMode;

		protected internal bool IsCargoReleaseValidationMode => Parent.IsCargoReleaseValidationMode;

		internal bool IsEntrySummaryOrCargoReleaseValidationMode => IsEntrySummaryValidationMode || IsCargoReleaseValidationMode;

		internal bool IsEntrySummaryValidationMode => Parent.IsEntrySummaryValidationMode;

		protected override ZBool ShouldCheckDeclarationWithSameDirectMasterBill => !Parent.IsConsumptionFTZ && !Parent.US_ConsolACE;

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACEImportJobDeclarationValidation : FormalImportJobDeclarationValidation
	{
		public ACEImportJobDeclarationValidation(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool ShouldCheckJE_ExportDate
		{
			get { return IsEntrySummaryValidationMode && !Parent.IsConsumptionFTZ; }
		}

		protected override bool IsVoyageFlightNoRequired
		{
			get { return base.IsVoyageFlightNoRequired && !Parent.IsCargoReleaseWithoutFormalEntry; }
		}

		protected override bool VesselRequired
		{
			get { return base.VesselRequired && !Parent.IsCargoReleaseWithoutFormalEntry; }
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();

			if (Parent.JE_VoyageFlightNo.IsEmpty)
			{
				if (Parent.RequiresPriorNoticeReporting && Parent.IsACECargoCertificationMode)
				{
					Parent.JE_VoyageFlightNoInfo.AddMessageError(VoyageFlightNoRequiredForPriorNotice);
				}
				else if (Parent.US_NonAMS)
				{
					Parent.JE_VoyageFlightNoInfo.AddMessageError(VoyageFlightNoRequiredForNonAMS);
				}
			}
		}
		internal const string VoyageFlightNoRequiredForNonAMS = "This is required when job is flagged as Non-AMS.";
		internal const string VoyageFlightNoRequiredForPriorNotice = "This is required for Prior Notice reporting.";

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();

			var declaration = Parent;
			if (declaration.JE_DateOfArrival.IsEmpty)
			{
				if (declaration.US_NonAMS)
				{
					declaration.JE_DateOfArrivalInfo.AddMessageError(ArrivalDateRequiredForNonAMS);
				}

				if (declaration.IsLowValue)
				{
					declaration.JE_DateOfArrivalInfo.AddMessageError(ArrivalDateRequiredForEntryType86);
				}
			}
			else
			{
				if (declaration.US_EntryType == EntryTypeList.Codes.ReWarehouse)
				{
					declaration.JE_DateOfArrivalInfo.AddMessageError(ArrivalDateNotRequiredForReWarehouse);
				}
			}
		}
		internal const string ArrivalDateRequiredForNonAMS = "Arrival Date is required when job is flagged as Non-AMS.";
		internal const string ArrivalDateRequiredForEntryType86 = "Estimated Date of Arrival is required for Entry Type 86.";
		internal const string ArrivalDateNotRequiredForReWarehouse = "Import Date not allowed for Entry Type 22.";

		protected override void CheckJE_PrimaryITNumber()
		{
			if (Parent.US_NonAMS && !Parent.JE_PrimaryITNumber.IsEmpty)
			{
				Parent.JE_PrimaryITNumberInfo.AddWarning(ITNumberNotRequiredForNonAMSJob);
			}
			else
			{
				base.CheckJE_PrimaryITNumber();
			}
		}
		internal const string ITNumberNotRequiredForNonAMSJob = "In-Bond Number will not be sent in message since the Non-AMS flag is ticked.";

		protected override bool DoesRequiredMasterBillIssuerSCAC
		{
			get
			{
				var declaration = Parent;
				return !declaration.IsACEAutoRoadAndPedTransportMode && !(declaration.IsACEStandalonePNWithoutENSAndCRL && declaration.IsACEENTStandAlonePriorNotice);
			}
		}

		protected override ZBool ShouldCheckDeclarationWithSameDirectMasterBill
		{
			get { return base.ShouldCheckDeclarationWithSameDirectMasterBill && (!Parent.IsHandCarry && !Parent.IsACEAutoRoadAndPedTransportMode); }
		}

		public override bool IsMasterBillMandatory
		{
			get
			{
				var declaration = Parent;
				return base.IsMasterBillMandatory && !(declaration.IsACEStandalonePNWithoutENSAndCRL && declaration.IsACEENTStandAlonePriorNotice);
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			var declaration = Parent;

			if (!declaration.IsTruck && declaration.IsReWarehouse && !declaration.JE_PrimaryITNumber.IsEmpty)
			{
				declaration.JE_TransportModeInfo.AddMessageError(TransportRequiredForITNotEmpty);
			}
		}
		internal const string TransportRequiredForITNotEmpty = "Transport TRK is required for the selected entry type when IT No is entered.";

		protected override void CheckJE_MasterBillIssuerSCAC()
		{
			base.CheckJE_MasterBillIssuerSCAC();
			ValidateUnknownCarrierSCAC(Parent, Parent.JE_MasterBillIssuerSCACInfo);
		}

		protected override void CheckJE_OA_ConsigneeAddress()
		{
			base.CheckJE_OA_ConsigneeAddress();
			var shouldValidate = false;

			if (IsEntrySummaryValidationMode)
			{
				if (!EntryTypeList.IsInformal(Parent.US_EntryType) &&
						Parent.US_EntryType != EntryTypeList.Codes.Appraisement &&
						Parent.US_EntryType != EntryTypeList.Codes.VesselRepair &&
						Parent.US_EntryType != EntryTypeList.Codes.Warehouse &&
						Parent.US_EntryType != EntryTypeList.Codes.ReWarehouse)
				{
					shouldValidate = true;
				}
			}
			else if (Parent.US_EntryType == EntryTypeList.Codes.LowValue)
			{
				shouldValidate = true;
			}

			if (shouldValidate)
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JE_OA_ConsigneeAddressInfo, OrgMatchedCustomsRegNoType.EIN, string.Format(System.Globalization.CultureInfo.CurrentCulture, OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"), true, false);
			}
		}

		#region JE_OH_FDASubmitter
		protected override void CheckJE_OH_FDASubmitter()
		{
			base.CheckJE_OH_FDASubmitter();

			if (Parent.IsFDAValidationMode && Parent.RequiresPriorNoticeReporting && Parent.JE_OH_FDASubmitter.IsEmpty)
			{
				Parent.JE_OH_FDASubmitterInfo.AddMessageError(SubmitterRequireForPriorNotice);
			}
		}
		internal const string SubmitterRequireForPriorNotice = "Submitter is mandatory for Prior Notice reporting.";
		#endregion

		public static void ValidateUnknownCarrierSCAC(JobDeclaration declaration, ZPropertyInfo carrierInfo)
		{
			if ((ZString)carrierInfo.Value == "UNKN" && (declaration.US_EnableCRL || declaration.US_CertifyCargoRelease))
			{
				carrierInfo.AddMessageError(ValidationConstants.UseZZZZForUnknownCarrierSCAC);
			}
		}

		protected internal override void CheckIOROrgPK(ZPropertyInfo info)
		{
			base.CheckIOROrgPK(info);
			if (Parent.AddInfoValidation is ACEImportAddInfoJobDeclarationValidation aceimportAddInfoJobDeclarationValidation)
			{
				aceimportAddInfoJobDeclarationValidation.CheckAgainstLatestNonPSCEntryDataIfPossible(info, x => !Parent.ImporterOfRecordNumber.EqualsIgnoringCase(x.IORNumber), ValidationConstants.PSC.NotAllowedToChangeIORNumber, false);
			}

			if (Parent.IsACECargoReleaseValidationMode)
			{
				if (Parent.PGAFlags.HasInvoiceLinesWithFSIS || Parent.PGAFlags.HasInvoiceLinesWithPST || Parent.PGAFlags.HasInvoiceLinesWithVNE || Parent.PGAFlags.HasInvoiceLinesWithNHTSARequireIOR || Parent.PGAFlags.HasInvoiceLinesWithODSOrTSCAARequireIM || Parent.PGAFlags.HasInvoiceLinesWithACELacey)
				{
					OrganisationValidation.ValidatePGAContact(info, Parent.IORWrapper);
				}
			}
		}

		protected override ZBool IsImporterOfRecordMandatory => Parent.US_EntryType != EntryTypeList.Codes.LowValue || Parent.HasAnyPGADataEitherDeclaredOrDisclaimed;

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			var parent = Parent;

			if (parent.HasMultipleFWSExporters)
			{
				parent.JE_MessageTypeInfo.AddWarning(ValidationConstants.FWS.FWSMultipleExporters);
			}
		}
	}
}

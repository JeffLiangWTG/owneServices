using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FTZJobDeclarationValidation : CommonImportJobDeclarationValidation
	{
		public FTZJobDeclarationValidation(JobDeclaration declaration)
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

		protected override void CheckFTZAdmissionNumber()
		{
			base.CheckFTZAdmissionNumber();

			CheckWHSTransactionExists(Parent.FTZAdmissionNumberInfo, () =>
			{
				var ftzCusEntryNum = Parent.FTZCusEntryNum;
				return ftzCusEntryNum == null || (ftzCusEntryNum.IsInDatabase && !ftzCusEntryNum.CE_EntryNum.Equals(ftzCusEntryNum.CE_EntryNumInfo.OriginalValue));
			});
		}

		protected override void CheckFTZControlNumber()
		{
			base.CheckFTZControlNumber();
			if (!Parent.FTZControlNumberIsAutoAllocated)
			{
				if (Parent.FTZControlNumber.IsEmpty)
				{
					if (!IsFTZPTTValidationModeAndDirectDeliveryMode)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.FTZControlNumberInfo, " Control Number");
					}
				}

				if (!Parent.FTZControlNumber.IsEmpty && !Parent.FTZZoneID.IsEmpty && !Parent.FTZAdmissionNumberFormatted.IsEmpty)
				{
					var isAdmissionNumberUnique = Parent.IsFTZAdmissionNumberUnique(Parent.FTZControlNumber);
					if (!isAdmissionNumberUnique)
					{
						Parent.FTZControlNumberInfo.AddError(FTZAdmissionNumberAlreadyExists);
					}
				}
			}
		}
		internal const string FTZAdmissionNumberAlreadyExists = "This Admission Number is already allocated to an existing FTZ job. Please enter a new number.";

		bool IsFTZPTTValidationModeAndDirectDeliveryMode
		{
			get
			{
				var declaration = Parent;
				return declaration.IsFTZPTTValidationMode && declaration.US_F_DirectDelivery;
			}
		}

		protected override void CheckFTZZoneID()
		{
			base.CheckFTZZoneID();

			if (Parent.FTZZoneID.IsEmpty)
			{
				if (!IsFTZPTTValidationModeAndDirectDeliveryMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.FTZZoneIDInfo, " Zone ID");
				}
			}
			else
			{
				var zoneIDMessageError = GetFTZZoneIDFormatMessageErrorIfInvalid(FieldType.ZoneID, Parent.FTZZoneID);
				if (!zoneIDMessageError.IsEmpty)
				{
					Parent.FTZZoneIDInfo.AddMessageError(zoneIDMessageError);
				}
			}

			if (!Parent.IsZoneIDInFDAApprovedZonesList && Parent.HasFDAReportingRequirements)
			{
				Parent.FTZZoneIDInfo.AddWarning(ZoneIDNotInFTZAllowsFDAList);
			}
		}
		internal const string ZoneIDNotInFTZAllowsFDAList = "FDA reporting is required, but this Zone ID is not in the list of FDA Approved Zones";

		public enum FieldType { ZoneID, FTZNumber }

		public static ZString GetFTZZoneIDFormatMessageErrorIfInvalid(FieldType field, ZString zoneID)
		{
			var messageError = ZString.Empty;
			if (!IsZoneIDValid(field, zoneID))
			{
				messageError = field == FieldType.ZoneID ?
					ResString.GetMultilingualString("F5B9A4C0-35CF-4294-BE16-51C58BF4D29A", $"Zone ID must be 3 digits + 3 alpha numeric + 3 alpha numeric or  3 digits + 2 alpha numeric + 2 digits.") :
					ResString.GetMultilingualString("906B6DAF-6F2D-4F50-B732-E295CC85DCA7", $"FTZ Number must be 3 digits + 3 alpha numeric (+ 3 alpha numeric) or  3 digits + 2 alpha numeric (+ 2 digits).");
			}
			return messageError;
		}

		static ZBool IsZoneIDValid(FieldType field, ZString zoneID)
		{
			var isMatched = ZBool.False;
			if (!zoneID.IsEmpty)
			{
				isMatched = Regex.IsMatch(zoneID, "(^[0-9]{3}[0-9A-Z]{2}[0-9]{2}$)|(^[0-9]{3}[0-9A-Z]{3}[0-9A-Z]{3}$)", RegexOptions.IgnoreCase);

				if (!isMatched && field == FieldType.FTZNumber)
				{
					isMatched = Regex.IsMatch(zoneID, "(^[0-9]{3}[0-9A-Z]{2}$)|(^[0-9]{3}[0-9A-Z]{3}$)", RegexOptions.IgnoreCase);
				}
			}
			return isMatched;
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			if (IsFTZAdmissionValidationMode && IsNotODZAdmissionType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DateOfArrivalInfo, "Import Date");
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
			base.CheckJE_DateOfFirstArrival();
			if (IsFTZAdmissionValidationMode && IsNotODZAdmissionType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DateOfFirstArrivalInfo, "Estimated Date of Arrival");
			}
		}

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();
			if (ShouldValidateContainerMode)
			{
				if (Parent.IsContainerised && Parent.Packages.Count == 0)
				{
					Parent.JE_ContainerModeInfo.AddMessageError(NoPackageEnteredWhenContainerModeIsSelected);
				}
			}
		}
		internal const string NoPackageEnteredWhenContainerModeIsSelected = "The container mode indicates this shipment is containerized, as yet, no packages have been entered.";

		protected override bool VesselRequired
		{
			get { return base.VesselRequired && IsFTZAdmissionValidationMode && !Parent.IsODZ_AdmissionType; }
		}

		public override int VesselNameLength
		{
			get { return JobDeclaration.Schema.FTZVesselNameLength; }
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();
			if (IsFTZAdmissionValidationMode)
			{
				if (Parent.JE_MasterBill.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_MasterBillInfo, "Master Bill");
				}

				if (Parent.JE_MasterBill.Contains(Bill.Constants.GeneratedBillLiteral) &&
					!Parent.IsOC_AdmissionType)
				{
					Parent.JE_MasterBillInfo.AddMessageError(BillNumFormat);
				}
			}
		}
		internal const string BillNumFormat = "Bill Number generated by filer is only accepted for Overage or Status Change admission type.";

		protected override int MasterBillMaxLength
		{
			get { return BillValidator.Constants.MaximumFTZBillLength; }
		}

		#region JE_VoyageFlightNo

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (IsVoyageFlightNoRequired && Parent.IsVoyageFlightNumberVisible)
			{
				CheckAirOrHandCarryFlightNo();
			}
		}

		protected override bool IsVoyageFlightNoRequired
		{
			get { return IsFTZAdmissionValidationMode && IsNotODZAdmissionType; }
		}

		#endregion

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();

			if (IsFTZAdmissionValidationMode && Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RL_NKPortOfArrivalInfo);
			}
		}

		#region CheckIOROrgPK

		protected internal override void CheckIOROrgPK(ZPropertyInfo info)
		{
			base.CheckIOROrgPK(info);
			if (IsFTZAdmissionValidationMode)
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(info, OrgMatchedCustomsRegNoType.EIN, string.Format(CultureInfo.InvariantCulture, OrganisationValidation.EIN_SSN_CBNCodeRequired, "Applicant"), true, false);
			}
		}

		#endregion

		protected override bool ShouldCheckJE_ExportDate
		{
			get { return IsFTZAdmissionValidationMode && IsNotODZAdmissionType; }
		}

		protected override bool ShouldValidateContainerMode
		{
			get { return base.ShouldValidateContainerMode && IsFTZAdmissionValidationMode; }
		}

		bool IsNotODZAdmissionType
		{
			get { return FTZJobDeclarationValidationHelper.IsNotODZAdmissionType(Parent); }
		}

		bool IsFTZAdmissionValidationMode
		{
			get { return FTZJobDeclarationValidationHelper.IsFTZAdmissionValidationMode(Parent); }
		}
	}
}

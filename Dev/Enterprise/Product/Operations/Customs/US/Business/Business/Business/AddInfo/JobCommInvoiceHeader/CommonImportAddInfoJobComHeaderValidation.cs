using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CommonImportAddInfoJobComHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public CommonImportAddInfoJobComHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		#region US_DES

		protected override void CheckUS_DES()
		{
			base.CheckUS_DES();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_DESInfo, Parent.AddInfoLookups.FIRMSList);
		}

		#endregion

		#region US_DestinationState

		protected override void CheckUS_DestinationState()
		{
			base.CheckUS_DestinationState();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_DestinationStateInfo, Parent.AddInfoLookups.USStateList, (NoResString)ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		#endregion

		#region US_TransactionsRelated

		protected override void CheckUS_TransactionsRelated()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TransactionsRelatedInfo, Lookups.US_RelatedOrgList);
		}

		#endregion

		#region US_DeductADDCVDDut

		protected override void CheckUS_DeductADDCVDDuty()
		{
			if (Parent.IsDeductADD_CVDDutyRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DeductADDCVDDutyInfo, "Deduct ADD/CVD Duty");
				ListValidation.MessageErrorIfInvalidCode(Parent.US_DeductADDCVDDutyInfo, Lookups.US_YesNoList);
			}
		}

		#endregion

		#region US_UC_NKCountryOfOrigin

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			base.CheckUS_UC_NKCountryOfOrigin();

			if (ShouldCheckUS_UC_NKCountryOfOrigin)
			{
				string errorTextForOrigin = ExternalValidation.GetErrorTextForCanadaCountryOfOrigin(Parent.US_UC_NKCountryOfOrigin, Parent.US_UC_NKCountryOfExport);
				if (!string.IsNullOrEmpty(errorTextForOrigin))
				{
					Parent.US_UC_NKCountryOfOriginInfo.AddMessageError(errorTextForOrigin);
				}
			}
		}

		protected virtual bool ShouldCheckUS_UC_NKCountryOfOrigin
		{
			get { return true; }
		}

		#endregion

		#region US_UC_NKCountryOfExport
		protected override void CheckUS_UC_NKCountryOfExport()
		{
			base.CheckUS_UC_NKCountryOfExport();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UC_NKCountryOfExportInfo, Lookups.USCountryList);

			var declaration = Parent.JobDeclaration;
			if (declaration != null && !EntryTypeList.IsWarehouseRelatedExceptFTZ(declaration.US_EntryType) && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Parent.US_UC_NKCountryOfExport) == Core.Constants.CountryCodes.UnitedStates)
			{
				Parent.US_UC_NKCountryOfExportInfo.AddMessageError(ValidationConstants.Declaration.ImportEntryShouldNotHaveUSOrPRAsCountryOfExport);
			}

			if (ShouldCheckCanadianCountryOfExport)
			{
				string errorTextForExport = ExternalValidation.GetErrorTextForCanadaCountryOfExport(Parent.US_UC_NKCountryOfExport);

				if (!string.IsNullOrEmpty(errorTextForExport))
				{
					Parent.US_UC_NKCountryOfExportInfo.AddMessageError(errorTextForExport);
				}
			}
		}

		protected virtual bool ShouldCheckCanadianCountryOfExport
		{
			get { return true; }
		}

		#endregion

		#region US_ZoneStatus

		protected override void CheckUS_ZoneStatus()
		{
			base.CheckUS_ZoneStatus();

			if (ShouldCheckUS_ZoneStatus)
			{
				CheckUS_ZoneStatusCommonForImport();
			}
		}

		protected virtual bool ShouldCheckUS_ZoneStatus
		{
			get { return true; }
		}

		#endregion
	}
}

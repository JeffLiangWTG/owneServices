using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CommonImportAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public CommonImportAddInfoJobDeclarationValidation(AddInfoJobDeclaration addInfoJobDeclaration)
			: base(addInfoJobDeclaration)
		{
		}

		#region US_FDACCN

		protected override void CheckUS_FDACCN()
		{
			base.CheckUS_FDACCN();

			if (!Declaration.IsStandAlonePriorNoticeMode)
			{
				if (Parent.US_FDACANType == FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_FDACCNInfo, Parent.Lookups.USStateList);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_FDACCNInfo, Lookups.USCountryList);
				}
			}
		}

		#endregion

		#region US_US_NKLocationOfGoods

		protected override void CheckUS_US_NKLocationOfGoods()
		{
			base.CheckUS_US_NKLocationOfGoods();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_US_NKLocationOfGoodsInfo, Parent.Lookups.FIRMSList, (NoResString)LocationOfGoodsNotOnFile);
		}
		internal const string LocationOfGoodsNotOnFile = "The Location of Goods (FIRMS Code) is not on file.\r\nTo validate that the value is correct, CBP data on the latest FIRMS Codes can be queued from\r\nOperations -> Customs -> Customs Declaration -> Actions -> Reference Files Request -> FIRMS Codes";

		protected void CheckFirmsShouldBeActive()
		{
			var firms = Declaration.LocationOfGoods;
			if (firms == null)
			{
				Parent.US_US_NKLocationOfGoodsInfo.AddMessageError(FirmsFacilityNotActive);
			}
		}
		internal const string FirmsFacilityNotActive = "This Location of Goods (FIRMS) is not active.";

		#endregion

		#region US_DestinationState

		protected override void CheckUS_DestinationState()
		{
			base.CheckUS_DestinationState();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_DestinationStateInfo, Parent.Lookups.USStateList, (NoResString)ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}

		#endregion

		#region US_SchDEntry

		protected override void CheckUS_SchDEntry()
		{
			base.CheckUS_SchDEntry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SchDEntryInfo, Lookups.RegionDistrictPorts, (NoResString)"Port of Entry is invalid.");
		}

		protected void CheckPortOfEntryAndPortOfDischargeWhenITPresents()
		{
			if (!Parent.US_ITDate.IsEmpty)
			{
				if ((Parent.US_SchDEntry == Parent.US_SchDArrival) && !Declaration.IsReWarehouse)
				{
					Parent.US_SchDEntryInfo.AddMessageError(ValidationConstants.Declaration.PortOfEntrySameAsDischargeWhenITPresent);
				}
			}
		}

		#endregion

		#region US_SchDArrival

		protected override void CheckUS_SchDArrival()
		{
			base.CheckUS_SchDArrival();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_SchDArrivalInfo, Lookups.DischargeSchDList);

			if (!Parent.US_SchDArrival.IsEmpty)
			{
				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Parent.Factory, Parent.US_SchDArrival, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				if (port != null && port.GetAttribute(RefCusCodeListAttributeTypes.Codes.Unlading) != YesNoList.Codes.Yes)
				{
					Parent.US_SchDArrivalInfo.AddMessageError(SelectedCodeNotPortOfDischarge);
				}
			}
		}

		internal const string SelectedCodeNotPortOfDischarge = "The code you have selected is not a valid Port of Unlading according to US Port Code files.";

		protected void CheckUS_SchDArrivalRequiredForPriorNotice()
		{
			if (Declaration.RequiresPriorNoticeReporting && Parent.US_SchDArrival.IsEmpty)
			{
				Parent.US_SchDArrivalInfo.AddMessageError(ValidationConstants.PriorNotice.PortOfArrival);
			}
		}

		#endregion

		#region US_UI_NKCarrierSCAC

		protected override void CheckUS_UI_NKCarrierSCAC()
		{
			if (Declaration.IsStandAlonePriorNoticeMode)
			{
				if (Parent.US_UI_NKCarrierSCAC.IsEmpty)
				{
					Parent.US_UI_NKCarrierSCACInfo.AddMessageError(ValidationConstants.PriorNotice.CarrierSCAC);
				}
			}
			else
			{
				base.CheckUS_UI_NKCarrierSCAC();
			}
		}

		#endregion

		#region US_IsHMFApplicable

		protected override void CheckUS_IsHMFApplicable()
		{
			base.CheckUS_IsHMFApplicable();
			if (ShouldCheckUS_IsHMFApplicable)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_IsHMFApplicableInfo, Parent.Lookups.US_YesNoList);

				if (Declaration.IsSea && Parent.US_IsHMFApplicable.IsEmpty)
				{
					Parent.US_IsHMFApplicableInfo.AddMessageError(FeeApplicableForSea);
				}

				if (Declaration.TransportMode == TransportTypeList.Codes.BorderWaterBorne && !Declaration.IsHMFApplicable)
				{
					Parent.US_IsHMFApplicableInfo.AddMessageError(FeeApplicableForBWBMOT);
				}

				if (Declaration.IsHMFApplicable && Declaration.TransportMode != TransportTypeList.Codes.BorderWaterBorne && !Declaration.IsSea)
				{
					Parent.US_IsHMFApplicableInfo.AddMessageError(FeeApplicableOnlyForSeaOrBWBMOT);
				}

				if (!EntryTypeList.IsHMFNotApplicable(Parent.US_EntryType) && Declaration.IsSea && Parent.US_IsHMFApplicable == YesNoDefaultList.Codes.No)
				{
					Parent.US_IsHMFApplicableInfo.AddWarning(FeeLikelyForSea);
				}
			}
		}

		protected virtual bool ShouldCheckUS_IsHMFApplicable
		{
			get { return true; }
		}

		internal const string FeeApplicableForBWBMOT = "If Mode Of Transport 'Border Water Borne' Harbor Maintenance Fee is applicable.";
		internal const string FeeApplicableForSea = "If Mode Of Transport 'Sea', Harbor Maintenance Fee Indicator should be entered.";
		internal const string FeeApplicableOnlyForSeaOrBWBMOT = "Harbor Maintenance Fee applicable only for Mode Of Transport 'Sea' or 'Border Water Borne'.";
		internal const string FeeLikelyForSea = "Transport Mode is 'Sea', but HMF is set to 'No'";

		#endregion

		#region US_EntryType

		protected override void CheckUS_EntryType()
		{
			base.CheckUS_EntryType();
			var iorWrapper = Declaration.IORWrapper;
			if (iorWrapper != null)
			{
				iorWrapper.RestrictedEntryTypes.CheckRestrictedCode(Declaration.US_EntryTypeInfo);
			}
		}

		#endregion

		#region US_FDAContactName

		protected override void CheckUS_FDAContactName()
		{
			base.CheckUS_FDAContactName();

			if (Parent.US_FDAContactName.IsEmpty)
			{
				if (Declaration.IsFDAPriorNoticeValidationRequired)
				{
					Parent.US_FDAContactNameInfo.AddMessageError(ValidationConstants.FDA.Contact);
				}
				if (Declaration.IsFWSEDSValidationRequired)
				{
					Parent.US_FDAContactNameInfo.AddMessageError(ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
				}
			}
		}

		#endregion

		#region US_FDAContactPhoneNo
		protected override void CheckUS_FDAContactPhoneNo()
		{
			base.CheckUS_FDAContactPhoneNo();

			if (Parent.US_FDAContactPhoneNo.IsEmpty)
			{
				if (Declaration.IsFDAPriorNoticeValidationRequired)
				{
					Parent.US_FDAContactPhoneNoInfo.AddMessageError(DomesticPhoneNoValidator.DomesticPhoneNoFormat);
				}
				if (Declaration.IsFWSEDSValidationRequired)
				{
					Parent.US_FDAContactPhoneNoInfo.AddMessageError(ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
				}
			}
			else
			{
				ZString messageError = DomesticPhoneNoValidator.Validate(Parent.US_FDAContactPhoneNo);
				if (!messageError.IsEmpty)
				{
					Parent.US_FDAContactPhoneNoInfo.AddMessageError(messageError);
				}
			}
		}
		#endregion

		#region US_FDAContactEmail
		protected override void CheckUS_FDAContactEmail()
		{
			base.CheckUS_FDAContactEmail();

			if (Parent.US_FDAContactEmail.IsEmpty)
			{
				if (Declaration.IsFDAPriorNoticeValidationRequired)
				{
					Parent.US_FDAContactEmailInfo.AddMessageError(ValidationConstants.FDA.ContactEmail);
				}
				if (Declaration.IsFWSEDSValidationRequired)
				{
					Parent.US_FDAContactEmailInfo.AddMessageError(ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
				}
			}
			else if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_FDAContactEmail))
			{
				Parent.US_FDAContactEmailInfo.AddWarning("Invalid email format");
			}
		}
		#endregion

		#region US_FDAADTA

		protected override void CheckUS_FDAADTA()
		{
			base.CheckUS_FDAADTA();

			if (ShouldCheckFDAADTA)
			{
				if (Parent.US_FDAADTA.IsEmpty)
				{
					Parent.US_FDAADTAInfo.AddMessageError(ValidationConstants.PriorNotice.DateTimeOfArrivalMandatory);
				}
				else if (Parent.US_FDAADTA.IsValid)
				{
					ValidateFDADateTime(Parent.US_FDAADTAInfo);

					if (Parent.US_FDAADTA < ZDateTime.Today.AddDays(-10))
					{
						Parent.US_FDAADTAInfo.AddMessageError(ValidationConstants.PriorNotice.DateOfArrivalRange);
					}

					if (Declaration.HasInvoiceLinesWithFDAPriorNotice)
					{
						if (Parent.US_FDAADTA.Date != Declaration.JE_DateOfArrival.Date)
						{
							Parent.US_FDAADTAInfo.AddWarning(ValidationConstants.PriorNotice.DateOfArrivalMismatch);
						}
					}
					else if (Parent.US_FDAADTA.Date != Declaration.US_EntryDate.Date)
					{
						Parent.US_FDAADTAInfo.AddWarning(ValidationConstants.PriorNotice.DateOfArrivalEntry);
					}
				}
			}
		}

		internal void ValidateFDADateTime(ZPropertyInfo propertyInfo)
		{
			if (Parent.US_FDAADTA.Hour == 24 || Parent.US_FDAADTA.Hour == 0 && Parent.US_FDAADTA.Minute == 0)
			{
				propertyInfo.AddMessageError(ValidationConstants.PriorNotice.TimeOfArrivalFormat);
			}
		}

		protected virtual bool ShouldCheckFDAADTA
		{
			get { return Declaration.IsFDAPriorNoticeValidationRequired; }
		}

		#endregion

		protected override void CheckUS_FDAAPC()
		{
			base.CheckUS_FDAAPC();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_FDAAPCInfo, Lookups.DischargeSchDList);
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class FTZAddInfoJobDeclarationValidation : CommonImportAddInfoJobDeclarationValidation
	{
		public FTZAddInfoJobDeclarationValidation(AddInfoJobDeclaration addInfoJobDeclaration)
			: base(addInfoJobDeclaration)
		{
		}

		protected override void CheckUS_F_DeliveryCode()
		{
			base.CheckUS_F_DeliveryCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_F_DeliveryCodeInfo, Parent.Lookups.FTZDeliveryCodeList);
		}

		#region US_US_NKLocationOfGoods

		protected override void CheckUS_US_NKLocationOfGoods()
		{
			base.CheckUS_US_NKLocationOfGoods();
			CheckFirmsShouldBeActive();

			var firms = Declaration.LocationOfGoods;
			if (firms != null && !firms.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode, Parent.US_SchDEntry))
			{
				Parent.US_US_NKLocationOfGoodsInfo.AddMessageError(ValidationConstants.Declaration.FirmsNotOnTheSameDistrict);
			}

			if (Parent.US_US_NKLocationOfGoods.IsEmpty)
			{
				if (IsFTZPTTValidationMode || (IsFTZAdmissionValidationMode))
				{
					Parent.US_US_NKLocationOfGoodsInfo.AddMessageError(string.Format(ValidationConstants.FTZ.DataRequired, "FIRMS Code"));
				}
			}
		}

		#endregion

		#region US_FDACANType

		protected override void CheckUS_FDACANType()
		{
			base.CheckUS_FDACANType();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_FDACANTypeInfo, Parent.Lookups.FDACANTypes);
		}

		#endregion

		#region US_SchDEntry

		protected override void CheckUS_SchDEntry()
		{
			base.CheckUS_SchDEntry();

			if (IsFTZAdmissionValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SchDEntryInfo, "Port of Entry");

				CheckPortOfEntryAndPortOfDischargeWhenITPresents();
			}
		}

		#endregion

		#region US_SchDArrival

		protected override void CheckUS_SchDArrival()
		{
			base.CheckUS_SchDArrival();

			if (IsFTZAdmissionValidationMode)
			{
				if (IsNotODZAdmissionType)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SchDArrivalInfo, "Port of Unlading");
					CheckPortMatchesTransportMode(Parent.US_SchDArrivalInfo);
				}

				if (Declaration.IsStandAlonePriorNoticeMode)
				{
					CheckUS_SchDArrivalRequiredForPriorNotice();
				}
			}
		}

		#endregion

		#region US_UI_NKCarrierSCAC

		protected override bool IsCarrierSCACRequired
		{
			get { return IsFTZAdmissionValidationMode && IsNotODZAdmissionType && base.IsCarrierSCACRequired; }
		}

		#endregion

		#region Routing Details

		protected override void CheckUS_F_RoutingDetails()
		{
			base.CheckUS_F_RoutingDetails();
			if (IsFTZAdmissionValidationMode)
			{
				if (!string.IsNullOrEmpty(Parent.US_F_RoutingDetails) && !JobDeclaration.IsValidFTZRoutingDetails(Parent.US_F_RoutingDetails))
				{
					Parent.US_F_RoutingDetailsInfo.AddMessageError(RoutingDetailsInvalidFormat);
				}
			}
		}
		internal const string RoutingDetailsInvalidFormat = "Invalid format for ABI Routing Code. Routing Code should be DDPPFLROF (District Port Code, Filer Code, Office Code (if required))";

		#endregion

		#region US_F_AdmissionType

		protected override void CheckUS_F_AdmissionType()
		{
			base.CheckUS_F_AdmissionType();

			if (!Parent.US_F_AdmissionType.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_F_AdmissionTypeInfo, Parent.Lookups.US_FTZAdmissionTypeList);
			}

			if (IsFTZAdmissionValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_F_AdmissionTypeInfo, "Admission type");

				var validationObject = Declaration.Validation;
				validationObject.ValidateJE_TransportMode();
				validationObject.ValidateJE_MasterBillIssuerSCAC();
				validationObject.ValidateJE_VoyageFlightNo();
				validationObject.ValidateJE_ExportDate();
				validationObject.ValidateJE_DateOfArrival();
				validationObject.ValidateJE_DateOfFirstArrival();
				ValidateUS_SchDArrival();
				ValidateUS_EntryDate();
				ValidateUS_UI_NKCarrierSCAC();
			}
		}

		#endregion

		#region US_EntryDate

		protected override void CheckUS_EntryDate()
		{
			base.CheckUS_EntryDate();

			if (IsFTZAdmissionValidationMode && IsNotODZAdmissionType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.US_EntryDateInfo, ValidationConstants.Declaration.EstimatedDateOfArrival);
			}
		}

		#endregion

		bool IsNotODZAdmissionType
		{
			get { return FTZJobDeclarationValidationHelper.IsNotODZAdmissionType(Declaration); }
		}

		bool IsFTZAdmissionValidationMode
		{
			get { return FTZJobDeclarationValidationHelper.IsFTZAdmissionValidationMode(Declaration); }
		}

		bool IsFTZPTTValidationMode
		{
			get { return FTZJobDeclarationValidationHelper.IsFTZPTTValidationMode(Declaration); }
		}

		protected override void CheckUS_F_PNMode()
		{
			base.CheckUS_F_PNMode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_F_PNModeInfo, Parent.Lookups.PriorNoticeModeCodeList);

			if (Parent.US_F_PNMode == PriorNoticeModeCodeList.Codes.O)
			{
				Parent.US_F_PNModeInfo.AddMessageError(ValidationConstants.PriorNotice.ACSPriorNoticeTurnedOff);
			}
		}

		protected override void CheckUS_SPNIDType()
		{
			base.CheckUS_SPNIDType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SPNIDTypeInfo, Declaration.Lookups.SPNIDTypeList);

			if (Declaration.IsFTZPGAStandAlonePriorNotice)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SPNIDTypeInfo);

				if (Declaration.IsFTZFTZStandAlonePriorNotice)
				{
					if (Declaration.FTZControlNumber.IsEmpty)
					{
						Parent.US_SPNIDTypeInfo.AddMessageError(ValidationConstants.PriorNotice.ControlNumberRequiredForFTZStandAlonePriorNotice);
					}

					if (Declaration.FTZZoneID.IsEmpty)
					{
						Parent.US_SPNIDTypeInfo.AddMessageError(ValidationConstants.PriorNotice.ZoneIDRequiredForFTZStandAlonePriorNotice);
					}
				}
				else if (Declaration.IsFTZBLNStandAlonePriorNotice && Declaration.Bills.NumberOfMasterBill == 0)
				{
					Parent.US_SPNIDTypeInfo.AddMessageError(ValidationConstants.PriorNotice.MasterBillNumberRequiredForBLNStandAlonePriorNotice);
				}
			}
		}
	}
}

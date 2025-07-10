using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	class ImportJobDeclarationValidation : ImportCommonJobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		#region Implementation

		#region Vessel
		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (Parent.IsSea && !Parent.JE_VesselName.IsEmpty)
			{
				var vessel = Parent.Vessel;
				if (vessel != null && vessel.RV_RadioCallSign.IsEmpty)
				{
					Parent.JE_VesselNameInfo.AddMessageError(RadioCallSignMandatory);
				}
			}
		}

		public static string RadioCallSignMandatory
		{
			get { return Res.GetString("8c84ec61-7067-4944-821b-9cfc99c29a4c", "Radio call sign must be entered on the selected vessel."); }
		}
		#endregion

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			if (Parent.JE_DateOfArrival.IsEmpty && (Parent.IsAir || Parent.IsSea || Parent.IsRoad))
			{
				Parent.JE_DateOfArrivalInfo.AddMessageError(DateOfArrivalRequired);
			}
		}

		public static string DateOfArrivalRequired
		{
			get { return Res.GetString("8ecd543c-e728-4f78-a7ac-80597c736e4e", "Arrival date is required."); }
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();
			if (!Parent.JE_CargoCarrier.IsEmpty && Parent.JE_HouseBill.StartsWith(Parent.JE_CargoCarrier, System.StringComparison.OrdinalIgnoreCase))
			{
				Parent.JE_HouseBillInfo.AddWarning(CargoCarrierAlreadySentToCustoms);
			}
		}

		public static string CargoCarrierAlreadySentToCustoms
		{
			get { return Res.GetString("9af7e714-4d3f-4152-808e-b46e507a26f5", "Your House Bill seems to start with the Cargo Carrier Code, which is already being sent to Customs. Unless the Cargo Carrier Code forms part of the actual transport document number issued, it should not be included."); }
		}

		#endregion

		#region AddInfo

		protected override void CheckJE_CargoCarrier()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CargoCarrierInfo);
			if (Parent.IsSea || Parent.IsAir)
			{
				if (Parent.JE_CargoCarrier.IsEmpty)
				{
					if (!Parent.JE_HouseBill.IsEmpty)
					{
						Parent.JE_CargoCarrierInfo.AddMessageError(MandatoryIfHouseBillCaptured);
					}
				}
				else if (Parent.JE_HouseBill.IsEmpty)
				{
					Parent.JE_CargoCarrierInfo.AddMessageError(MustBeEmptyIfHouseBillEmpty);
				}
			}
			ValidateJE_HouseBill();
		}
		public static string MandatoryIfHouseBillCaptured
		{
			get { return Res.GetString("d2965443-2806-4205-bbaf-e46aa195dab8", "Required if a House Bill is captured."); }
		}
		public static string MustBeEmptyIfHouseBillEmpty
		{
			get { return Res.GetString("6588f858-2693-4914-9702-bd4f7bf8946c", "Must be empty if no House Bill is captured."); }
		}

		protected override void CheckJE_VATClaimBackIndicator()
		{
			base.CheckJE_VATClaimBackIndicator();
			if (Parent.JE_VATClaimBackIndicator == VATClaimBackIndicatorCodeList.Codes.Yes)
			{
				var vatRegNo = Parent.Importer?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
				if (vatRegNo.IsEmpty)
				{
					Parent.JE_VATClaimBackIndicatorInfo.AddMessageError(GovernmentVATNoRequired);
				}
			}
		}

		public static string GovernmentVATNoRequired
		{
			get { return Res.GetString("e1b04197-884d-447b-b57a-ab9b2808f96c", "The Importer on this Declaration must have a valid VAT Registration Number in order to qualify for VAT 201 returns"); }
		}

		#endregion
	}
}

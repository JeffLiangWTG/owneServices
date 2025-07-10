using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	class ExportJobDeclarationValidation : JobDeclarationValidation
	{
		public ExportJobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public new JobDeclaration Parent
		{
			get { return base.Parent; }
		}

		#region Implementation
		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			OrgHeader supplier = Parent.Supplier;
			if (supplier != null)
			{
				ZString supplierCode = supplier.LocalCustomsSupplierCode;
				if (supplierCode.IsEmpty)
				{
					Parent.JE_OH_SupplierInfo.AddMessageError("The Supplier does not have a Customs Supplier code which is required.");
				}
				else if (!new SouthAfricanOrganisationCodeValidator().IsValid(supplierCode))
				{
					Parent.JE_OH_SupplierInfo.AddMessageError("The Supplier has a Customs Supplier code that is not valid.");
				}
				else if (supplierCode.Equals(ValidationConstants.Declaration.UnregisteredTraderCustomsCode))
				{
					CheckUnregisteredTrader(supplier, Parent.JE_OH_SupplierInfo, "Supplier");
				}
			}
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			if (!Parent.JE_RL_NKOrigin.IsEmpty)
			{
				var origin = Parent.Origin?.Country;
				if (origin == null || (origin.RN_Code != Core.Constants.CountryCodes.SouthAfrica && !origin.IsBLNS))
				{
					Parent.JE_RL_NKOriginInfo.AddMessageError(CountryOfOriginMustBeZAOrBLNS);
				}
			}
		}

		public static string CountryOfOriginMustBeZAOrBLNS
		{
			get { return Res.GetString("9057C0F6-1256-432E-A457-4E122072B175", "Port of Origin must be in South Africa, Botswana, Lesotho, Namibia or Swaziland"); }
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_RL_NKPortOfLoadingInfo, Parent.Lookups.PortOfLoadings);
			if (!Parent.JE_RL_NKPortOfLoading.IsEmpty)
			{
				if (Parent.JE_RL_NKPortOfLoading.Substring(0, 2) != Enterprise.Core.Constants.CountryCodes.SouthAfrica)
				{
					Parent.JE_RL_NKPortOfLoadingInfo.AddMessageError(PortOfLoadingMustBeInZA);
				}
			}
			else
			{
				Parent.JE_RL_NKPortOfLoadingInfo.AddMessageError(PortOfLoadingMandatory);
			}
		}

		public static string PortOfLoadingMustBeInZA
		{
			get { return Res.GetString("fcc1007a-1d77-48e1-a585-5b7a48753e6a", "Port of loading must be in ZA."); }
		}
		public static string PortOfLoadingMandatory
		{
			get { return Res.GetString("52f4b30e-e9ea-4ee3-aaf1-4477f93bccb7", "Port of loading must be specified."); }
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();

			if (!Parent.JE_RL_NKFinalDestinationInfo.HasNotifications())
			{
				if (Parent.JE_RL_NKFinalDestination.IsEmpty)
				{
					Parent.JE_RL_NKFinalDestinationInfo.AddMessageError(FinalDestMandatory);
				}
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (Parent.JE_ExportDate.IsEmpty && (Parent.IsAir || Parent.IsSea || Parent.IsRoad))
			{
				Parent.JE_ExportDateInfo.AddMessageError(DateOfDepartureRequired);
			}
		}
		public static string DateOfDepartureRequired
		{
			get { return Res.GetString("7aa2c9da-b4b0-4589-97fd-27cab13e8196", "Departure date is required."); }
		}

		protected override void ValidateImporterForWarehouseTransactions()
		{
		}

		protected override void ValidateSupplierForWarehouseTransactions()
		{
			base.ValidateSupplierForWarehouseTransactions();
			CheckWHSTransactionExists(Parent.JE_OH_SupplierInfo);
		}

		#endregion

		#region AddInfo

		protected override void CheckJE_VATClaimBackIndicator()
		{
			base.CheckJE_VATClaimBackIndicator();
			if (Parent.JE_VATClaimBackIndicator == VATClaimBackIndicatorCodeList.Codes.Yes)
			{
				var vatRegNo = Parent.Supplier?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;
				if (vatRegNo.IsEmpty)
				{
					Parent.JE_VATClaimBackIndicatorInfo.AddMessageError(GovernmentVATNoRequired);
				}
			}
		}

		public static string GovernmentVATNoRequired
		{
			get { return Res.GetString("b8022be9-781d-44cd-b35e-f0c5230bc993", "The Exporter on this Declaration must have a valid VAT Registration Number in order to qualify for VAT 201 returns"); }
		}

		#endregion
	}
}

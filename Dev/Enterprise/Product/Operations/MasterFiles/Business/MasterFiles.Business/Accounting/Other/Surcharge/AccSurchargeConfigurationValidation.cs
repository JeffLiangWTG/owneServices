//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccSurchargeConfigurationValidation
//
//    This class should be used for overriding validation in AutoAccSurchargeConfigurationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeConfigurationValidation : AutoAccSurchargeConfigurationValidation
	{
		public AccSurchargeConfigurationValidation(AutoAccSurchargeConfiguration parent) : base(parent)
		{
		}

		protected override void CheckASC_Code()
		{
			base.CheckASC_Code();

			MandatoryValidation.CheckEntered(Parent.ASC_CodeInfo);

			if (!Parent.ASC_CodeInfo.HasErrors())
			{
				if (Parent.Company?.AccSurchargeConfigurations.Any(x => x.PK != Parent.PK && ((AccSurchargeConfiguration)x).ASC_Code == Parent.ASC_Code) ?? false)
				{
					Parent.ASC_CodeInfo.AddError(Res.GetString("F4857754-07B8-4DB5-BEA1-0AD77511037A", "Code must be unique within a single company"));
				}
			}

			if (!Parent.ASC_CodeInfo.HasErrors() && (Parent.ASC_Code == AccountingMasterFilesConstants.ReserveSurchargeCodes.All || Parent.ASC_Code == AccountingMasterFilesConstants.ReserveSurchargeCodes.Non))
			{
				Parent.ASC_CodeInfo.AddError(Res.GetString("39732DFD-792F-49C7-BB38-70CA6A30B7E3",
					"Please specify a code other than 'ALL' and 'NON' which is reserved by system to identify 'All surcharges to be applied' or 'No surcharges to be applied'."));
			}
		}

		protected override void CheckASC_Description()
		{
			base.CheckASC_Description();
			MandatoryValidation.CheckEntered(Parent.ASC_DescriptionInfo);
		}

		protected override void CheckASC_Rate()
		{
			base.CheckASC_Rate();

			if (!Parent.ASC_RateInfo.HasErrors() && Parent.ASC_Rate <= 0)
			{
				Parent.ASC_RateInfo.AddError(Res.GetString("77207193-8CC7-4C88-960B-ED556C9E0DB7", "Surcharge Percentage should be greater than 0."));
			}
		}

		protected override void CheckASC_BasisType()
		{
			base.CheckASC_BasisType();
			MandatoryValidation.CheckEntered(Parent.ASC_BasisTypeInfo);

			if (!Parent.ASC_BasisTypeInfo.HasErrors() && Parent.ASC_BasisType != SurchargeBasisTypeList.Codes.ALL)
			{
				if (((AccSurchargeConfiguration)Parent).AccSurchargeBasises.Count == 0)
				{
					Parent.ASC_BasisTypeInfo.AddError(Res.GetString("B9A1B027-1EA3-4752-B34C-65900D93A1A0", "Please specify at least a charge group or a charge code."));
				}
			}
		}

		protected override void CheckASC_AC_ChargeCode()
		{
			base.CheckASC_AC_ChargeCode();

			if (!Parent.ASC_AC_ChargeCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.ASC_AC_ChargeCodeInfo, Parent.Lookups.ChargeCodes);
			}
		}
	}
}
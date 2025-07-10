//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccSurchargeBasisValidation
//
//    This class should be used for overriding validation in AutoAccSurchargeBasisValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeBasisValidation : AutoAccSurchargeBasisValidation
	{
		public AccSurchargeBasisValidation(AutoAccSurchargeBasis parent) : base(parent)
		{
		}

		protected override void CheckASB_AC_ChargeCode()
		{
			base.CheckASB_AC_ChargeCode();

			if (!Parent.ASB_AC_ChargeCodeInfo.HasErrors() && Parent.ASB_ChargeGroup.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ASB_AC_ChargeCodeInfo);
			}

			if (!Parent.ASB_AC_ChargeCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.ASB_AC_ChargeCodeInfo, Parent.Lookups.ChargeCodes);
			}

			if (!Parent.ASB_AC_ChargeCodeInfo.HasErrors() && Parent.ASB_AC_ChargeCode.IsValid)
			{
				if (((AccSurchargeBasis)Parent).SurchargeConfiguration?.AccSurchargeBasises.Any(x => x.PK != Parent.PK && ((AccSurchargeBasis)x).ASB_AC_ChargeCode == Parent.ASB_AC_ChargeCode) ?? false)
				{
					Parent.ASB_AC_ChargeCodeInfo.AddError(Res.GetString("A336D802-8CD7-4611-8CC1-9B3292C9DEA6", "Charge Code must be unique within a single Surcharge."));
				}
			}

			if (!Parent.ASB_AC_ChargeCodeInfo.HasErrors())
			{
				var errorMessage = GetCheckChargeGroupOrChargeCodeOnlyMessage();
				if (!errorMessage.IsEmpty)
				{
					Parent.ASB_AC_ChargeCodeInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckASB_ChargeGroup()
		{
			base.CheckASB_ChargeGroup();

			if (!Parent.ASB_ChargeGroupInfo.HasErrors() && !Parent.ASB_AC_ChargeCode.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.ASB_ChargeGroupInfo);
			}

			if (!Parent.ASB_ChargeGroupInfo.HasErrors() && !Parent.ASB_ChargeGroup.IsEmpty)
			{
				if (((AccSurchargeBasis)Parent).SurchargeConfiguration?.AccSurchargeBasises.Any(x => x.PK != Parent.PK && ((AccSurchargeBasis)x).ASB_ChargeGroup == Parent.ASB_ChargeGroup) ?? false)
				{
					Parent.ASB_ChargeGroupInfo.AddError(Res.GetString("F8EB3B34-40E8-413E-B199-08CB6371F016", "Charge Group must be unique within a single Surcharge."));
				}
			}

			if (!Parent.ASB_ChargeGroupInfo.HasErrors())
			{
				var errorMessage = GetCheckChargeGroupOrChargeCodeOnlyMessage();
				if (!errorMessage.IsEmpty)
				{
					Parent.ASB_ChargeGroupInfo.AddError(errorMessage);
				}
			}
		}

		ZString GetCheckChargeGroupOrChargeCodeOnlyMessage()
		{
			var errorMessage = ZString.Empty;

			if (!Parent.ASB_ChargeGroup.IsEmpty && Parent.ASB_AC_ChargeCode.IsValid)
			{
				errorMessage = Res.GetString("F56C71CB-C660-4886-BE4E-F7F93474DAC7", "For each row, please specify a charge group or charge code only.");
			}

			return errorMessage;
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgInvTypeDeferredChargesValidation
//
//    This class should be used for overriding validation in AutoOrgInvTypeDeferredChargesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvTypeDeferredChargesValidation : AutoOrgInvTypeDeferredChargesValidation
	{
		public OrgInvTypeDeferredChargesValidation(AutoOrgInvTypeDeferredCharges parent) : base(parent)
		{
		}

		protected override void CheckPO_AC()
		{
			base.CheckPO_AC();
			if (Parent.PO_ChargeGroup == ZString.Empty)
			{
				MandatoryValidation.CheckEntered(Parent.PO_ACInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.PO_ACInfo);
			}

			if (!Parent.PO_ACInfo.HasErrors() && !Parent.PO_AC.IsEmpty)
			{
				foreach (OrgInvTypeDeferredCharges deffCharge in ((OrgInvTypeDeferredCharges)Parent).ParentCollection)
				{
					if (deffCharge.PO_AC == Parent.PO_AC && deffCharge.PK != Parent.PK)
					{
						Parent.PO_ACInfo.AddError(Res.GetString("499324CC-F845-4810-96CB-766941514276", "You cannot add row with the same value!"));
						break;
					}
				}

				foreach (OrgInvTypeDeferredCharges deffCharge in ((OrgInvTypeDeferredCharges)Parent).ParentCollection)
				{
					if (deffCharge.PO_ChargeGroup == Parent.ChargeCode.AC_ChargeGroup)
					{
						Parent.PO_ACInfo.AddError(Res.GetString("C4901FA9-6E61-4738-957A-862F1044D7F1", "This charge code consist in charge group which already chosen!"));
						break;
					}
				}

				if (Parent.InvoiceType != null)
				{
					Parent.InvoiceType.Validation.ValidatePI_Calc_IsInclude();
				}
			}
		}

		protected override void CheckPO_ChargeGroup()
		{
			base.CheckPO_ChargeGroup();
			ListValidation.ErrorIfInvalidCode(Parent.PO_ChargeGroupInfo);
			if (Parent.PO_AC == ZGuid.Empty)
			{
				MandatoryValidation.CheckEntered(Parent.PO_ChargeGroupInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.PO_ChargeGroupInfo);
			}

			if (Parent.PO_ChargeGroup != ZString.Empty)
			{
				foreach (OrgInvTypeDeferredCharges deffCharge in ((OrgInvTypeDeferredCharges)Parent).ParentCollection)
				{
					if (deffCharge.PO_ChargeGroup == Parent.PO_ChargeGroup && deffCharge.PK != Parent.PK)
					{
						Parent.PO_ChargeGroupInfo.AddError(Res.GetString("CEC5E3CD-050C-47f2-ACF9-66E5205C786F", "You can't add row with the same value!"));
						break;
					}
				}

				if (Parent.InvoiceType != null)
				{
					Parent.InvoiceType.Validation.ValidatePI_Calc_IsInclude();
				}
			}
		}
	}
}

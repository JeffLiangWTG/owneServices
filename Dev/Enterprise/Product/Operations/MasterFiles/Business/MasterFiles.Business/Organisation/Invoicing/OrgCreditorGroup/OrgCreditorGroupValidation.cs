using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCreditorGroupValidation : AutoOrgCreditorGroupValidation
	{
		public OrgCreditorGroupValidation(AutoOrgCreditorGroup parent)
			: base(parent)
		{
		}

		protected override void CheckOG_Code()
		{
			base.CheckOG_Code();
			MandatoryValidation.CheckEntered(Parent.OG_CodeInfo);

			if (!Parent.OG_CodeInfo.HasErrors() && HasDuplicateCode())
			{
				Parent.OG_CodeInfo.AddError(Res.GetString("BE3BED39-4D64-43cc-AF3C-307094856A4A", "Creditor Group Code must be unique."));
			}
		}

		protected override void CheckOG_Desc()
		{
			base.CheckOG_Desc();
			MandatoryValidation.CheckEntered(Parent.OG_DescInfo);
			if (Parent.OG_Desc.Length < 4)
			{
				Parent.OG_DescInfo.AddError(Res.GetString("d64dbc85-c7df-40e2-8fc8-0a829624c457", "Description must be at least 4 characters."));
			}
		}

		protected override void CheckOG_DefaultHoldOption()
		{
			base.CheckOG_DefaultHoldOption();
			ListValidation.ErrorIfInvalidPK(Parent.OG_DefaultHoldOptionInfo);

			var concreteParent = (OrgCreditorGroup)Parent;

			if (!concreteParent.OG_IsAllowALM && concreteParent.OG_DefaultHoldOption == HoldOptionType.Codes.ALM)
			{
				concreteParent.OG_DefaultHoldOptionInfo.AddError(Res.GetString("6DDB67ED-31D3-4e83-AEB6-2E7EE226A968", "Cannot pick a disabled hold option as default hold option."));
			}
		}

		bool HasDuplicateCode()
		{
			if (Parent.OG_Code.IsEmpty)
			{
				return false;
			}

			var query = new ZQuery(OrgCreditorGroupSchema.OG_Code, Parent.OG_Code);
			query.AddToFilter(OrgCreditorGroupSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return Parent.Factory.Exists(typeof(OrgCreditorGroup), query);
		}
	}
}

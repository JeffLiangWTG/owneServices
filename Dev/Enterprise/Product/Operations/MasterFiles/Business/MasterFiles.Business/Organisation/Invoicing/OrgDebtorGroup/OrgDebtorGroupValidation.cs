//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgDebtorGroupValidation
//
//    This class should be used for overriding validation in AutoOrgDebtorGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupValidation : AutoOrgDebtorGroupValidation
	{
		public OrgDebtorGroupValidation(AutoOrgDebtorGroup parent) : base(parent)
		{
			this.Parent = (OrgDebtorGroup)parent;
		}

		protected override void CheckOJ_Code()
		{
			base.CheckOJ_Code();
			MandatoryValidation.CheckEntered(Parent.OJ_CodeInfo);

			if (!Parent.OJ_CodeInfo.HasErrors() && HasDuplicateCode())
			{
				Parent.OJ_CodeInfo.AddError(Res.GetString("BC00B54B-BE8E-45b4-9CB7-2C7E4407279E", "Debtor Group Code must be unique."));
			}
		}

		protected override void CheckOJ_Desc()
		{
			base.CheckOJ_Desc();
			MandatoryValidation.CheckEntered(Parent.OJ_DescInfo);
			if (Parent.OJ_Desc.Length < 4)
			{
				Parent.OJ_DescInfo.AddError(Res.GetString("eb1fa649-c856-4e48-b116-3810df4a9536", "Class description must be at least 4 characters."));
			}
		}

		new readonly OrgDebtorGroup Parent;

		public void ValidateDefaultBankAccountPK()
		{
			ValidateCalculatedProperty(Parent.DefaultBankAccountPKInfo);
		}

		protected void CheckDefaultBankAccountPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.DefaultBankAccountPKInfo);
			if (Parent.OverrideRegistryCurrencyToBankSetting)
			{
				MandatoryValidation.CheckEntered(Parent.DefaultBankAccountPKInfo);
			}
		}

		bool HasDuplicateCode()
		{
			if (Parent.OJ_Code.IsEmpty)
			{
				return false;
			}

			var query = new ZQuery(OrgDebtorGroupSchema.OJ_Code, Parent.OJ_Code);
			query.AddToFilter(OrgDebtorGroupSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return Parent.Factory.Exists(typeof(OrgDebtorGroup), query);
		}
	}
}

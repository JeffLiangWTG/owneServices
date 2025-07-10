using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.NZ;

namespace Enterprise.Customs.NZ.Business
{
	public class OrgCusCodeValidation : Enterprise.MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
			this.parent = (OrgCusCode)parent;
		}
		readonly OrgCusCode parent;

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();

			if (!parent.OK_CustomsRegNoInfo.HasErrors() && parent.CountryIs(Core.Constants.CountryCodes.NewZealand))
			{
				if (parent.OK_CodeType == OrgCusCode.CodeTypes.GSTCode)
				{
					if (!GSTNumberValidation.ValidateGSTNumber(parent.OK_CustomsRegNo))
					{
						parent.OK_CustomsRegNoInfo.AddError(Res.GetString("358741d2-2496-45ba-90ca-e135a7fc5b8c", "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit."));
					}
				}
			}
		}
	}
}

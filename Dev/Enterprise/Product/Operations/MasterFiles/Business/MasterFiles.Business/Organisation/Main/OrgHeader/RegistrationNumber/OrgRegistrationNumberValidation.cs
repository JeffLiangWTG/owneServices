using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRegistrationNumberValidation : ZValidation
	{
		readonly OrgRegistrationNumber parent;

		public OrgRegistrationNumberValidation(OrgRegistrationNumber parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(OrgRegistrationNumberValidation); }
		}

		protected BusinessObjectFactory Factory
		{
			get { return parent.Factory; }
		}

		protected void CheckNumber()
		{
			if (parent.Organization.RequiredFieldsForOrg.RequireBusinessNumber)
			{
				RefCountry orgCountry = parent.Organization.Country;
				if ((orgCountry != null) && (orgCountry.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					MandatoryValidation.CheckEntered(parent.NumberInfo);
				}
			}

			if (parent.Number.IsEmpty && parent.Organization.PhoneOrBusinessNumberRequiredForOrg && parent.Organization.MainAddress.OA_Phone.IsEmpty)
			{
				parent.NumberInfo.AddError(Res.GetString("142d5672-e30e-44c2-b326-2a76ca66762e", "Please enter either a Phone Number or a Registration Number for this organization."));
			}

			OrgCusCode cusCode = parent.CusCode;
			if (cusCode != null)
			{
				cusCode.Validation.ValidateOK_CustomsRegNo();
				parent.NumberInfo.AddAllNotificationsFrom(cusCode.OK_CustomsRegNoInfo);
			}
		}

		protected void CheckNumberTypeForDisplay()
		{
			MandatoryValidation.CheckEntered(parent.NumberTypeForDisplayInfo);
			ListValidation.ErrorIfInvalidCode(parent.NumberTypeForDisplayInfo);
			CheckSecurityRightOfSSN();
		}

		public override void ValidateAll()
		{
			ValidateNumber();
			ValidateNumberTypeForDisplay();
		}

		public void ValidateNumber()
		{
			ValidateCalculatedProperty(parent.NumberInfo);
		}

		public void ValidateNumberTypeForDisplay()
		{
			ValidateCalculatedProperty(parent.NumberTypeForDisplayInfo);
		}

		void CheckSecurityRightOfSSN()
		{
			if (!OrgCusCode.AllowViewSocialSecurityNumber(parent.NumberType) && (parent.CusCode == null || !parent.CusCode.IsInDatabase))
			{
				parent.NumberTypeForDisplayInfo.AddError(OrgCusCodeValidation.GetFailingMessage(Env.Security.OrgDetailsViewPersonalInformation, null));
			}
		}
	}
}

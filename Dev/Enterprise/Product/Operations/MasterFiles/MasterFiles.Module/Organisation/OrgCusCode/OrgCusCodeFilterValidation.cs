using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCusCodeFilterValidation : ModuleTextFilterValidation
	{
		internal OrgCusCodeFilterValidation(OrgCusCodeFilter parent)
			: base(parent)
		{
		}

		protected override void CheckProperty()
		{
			base.CheckProperty();

			if (Parent.Property == USACodeTypes.SocialSecurityNumber && !Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
			{
				Parent.PropertyInfo.AddError(ResString.GetMultilingualString("718C3119-2739-4B9A-BEEA-D39A925331CD", "You do not have the appropriate security rights to select this Code. If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Maintain -> Master Data -> Organization -> View -> View Personal Information."));
			}
		}
	}
}

using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFDocAddressLookup : JobDocAddressLookups
	{
		public ISFDocAddressLookup(ISFDocAddress parent) : base(parent) { }

		public override CodeDescriptionPairList GovRegNumTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(base.GovRegNumTypes);

				if (!Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
				{
					result.RemoveCode(OrgCusCode.USACodeTypes.SocialSecurityNumber);
				}

				return result;
			}
		}
	}
}

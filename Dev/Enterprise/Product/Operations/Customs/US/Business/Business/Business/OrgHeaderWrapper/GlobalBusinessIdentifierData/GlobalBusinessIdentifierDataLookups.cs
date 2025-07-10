using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class GlobalBusinessIdentifierDataLookups : ZLookups
	{
		public GlobalBusinessIdentifierDataLookups(GlobalBusinessIdentifierData parent) : base(parent)
		{
		}

		protected new GlobalBusinessIdentifierData Parent => (GlobalBusinessIdentifierData)base.Parent;

		public OrgAddressDependentCollection Addresses => Parent.Organization.Addresses;

		public GBISubmissionStatusList SubmissionStatusList => Factory.GetCachedValue<GBISubmissionStatusList>();
	}
}

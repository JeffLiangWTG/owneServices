using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSApplicationControlGeneratorCreator : IApplicationControlGeneratorCreator
	{
		#region IApplicationControlGeneratorCreator Members

		public ApplicationControlGenerator New(string applicationIdentifier, GlbBranch branch)
		{
			return new AMSApplicationControlGenerator(applicationIdentifier, branch);
		}

		#endregion
	}
}

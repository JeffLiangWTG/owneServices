using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceListSecurityProvider
	{
		public bool HasEditConfigurationSecurity
		{
			get { return Env.Security.RefComplianceListEdit.IsAllowed; }
		}
	}
}

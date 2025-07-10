using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.Testing
{
	public class SupervisorOverridesForTesting : SupervisorOverrides
	{
		public SupervisorOverridesForTesting(IBusiness businessEntity, string context)
			: base(businessEntity, context)
		{
			this.businessEntity = businessEntity;
			this.context = context;
		}
		public void AddMessageLogForTesting(string code, string message, bool allowed)
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var checkpoint = new SecurityCheckpoint(code, (NoResString)(code + " 1"), null, security);
			checkpoint.IsAllowed = allowed;
			base.AddMessageLog(checkpoint, message);
		}

		public void ClearMessageLogs()
		{
			base.AuthorisedMessagesForLog.DeleteAll();
			base.UnAuthorisedMessagesForLog.DeleteAll();
		}
	}
}

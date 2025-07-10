using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Services.ServiceHost
{
	public sealed class eAdaptorNextContextSetter : IeAdaptorContextSetter
	{
		public void SetContext()
		{
			var conf = ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig;
			if (conf == null)
			{
				WebAppEnvironment.Setup(eAdaptorConfig.Instance.WebConfig);
			}
			else
			{
				var inboundBranchPK = conf.ECC_GB_Branch;
				if (!inboundBranchPK.IsValid)
				{
					throw new InvalidWebEnvironmentException("Invalid Branch, you need to provide a valid branch in EDI Client Details > Branch");
				}
				var inboundDepartmentPK = conf.ECC_GE_Department;
				if (!inboundDepartmentPK.IsValid)
				{
					throw new InvalidWebEnvironmentException("Invalid Department, you need to provide a valid department in EDI Client Details > Department");
				}
				GlbStaff user;
				if(conf.Party.ECP_GS_SecurityProxy.IsValid)
				{
					var factory = new BusinessObjectFactory
					{
						RefreshEnabled = false
					};

					user = factory.Load<GlbStaff>(conf.Party.ECP_GS_SecurityProxy) ?? throw new InvalidWebEnvironmentException($"Invalid Security Proxy, you need to provide a valid security proxy in EDI Client Details > Security Proxy");
				}
				else
				{
					user = WebAppEnvironment.GetWebUserFromDatabase();
				}
				Env.SetUserContext(new Environment.UserContext(user, inboundBranchPK.ToGuid(), inboundDepartmentPK.ToGuid()));

				var branch = (GlbBranch)Env.CurrentBranch ?? throw new InvalidWebEnvironmentException("Invalid Branch, you need to provide a valid branch in EDI Client Details > Branch");
				var company = Env.CurrentCompany ?? throw new InvalidWebEnvironmentException($"Unable to load company for branch '{branch.GB_Code}'");
				var department = (GlbDepartment)Env.CurrentDepartment ?? throw new InvalidWebEnvironmentException("Invalid Department, you need to provide a valid department in EDI Client Details > Department");
				var userContext = Env.CurrentUserContext ?? throw new InvalidWebEnvironmentException("Unable to load Web user");

				if (!branch.GB_IsActive)
				{
					throw new InvalidWebEnvironmentException($"Branch {branch.GB_Code} is not active, you need to provide an active branch in EDI Client Details > Branch");
				}
				if (!department.GE_IsActive)
				{
					throw new InvalidWebEnvironmentException($"Department {department.GE_Code} is not active, you need to provide an active department in EDI Client Details > Department");
				}
			}
		}
	}
}

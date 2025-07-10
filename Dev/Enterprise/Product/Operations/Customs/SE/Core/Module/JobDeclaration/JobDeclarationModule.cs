using Enterprise.Customs.SE.Business.Declaration;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs.SE.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();
	}
}

using Enterprise.Customs._EUCustomsTemplate_.Business.Declaration;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs._EUCustomsTemplate_.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();
	}
}

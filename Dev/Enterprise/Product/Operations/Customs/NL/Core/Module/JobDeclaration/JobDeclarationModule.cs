using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.Module;

[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
public class JobDeclarationModule : EU.Module.JobDeclarationModule
{
	protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

	protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

	protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);
}

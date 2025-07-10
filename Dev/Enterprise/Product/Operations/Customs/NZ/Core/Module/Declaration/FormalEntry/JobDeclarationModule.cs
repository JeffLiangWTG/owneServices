using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new NZJobDeclarationFilterBusinessObject();

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		protected override IFilterControl GetNewFilterControl() => new NZJobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;
	}
}

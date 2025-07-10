using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module
{
	public class OutwardReportModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.NZ.OutwardReport; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.NZ.OutwardReport);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OutwardReportFilterControl(GridCollection, (OutwardReportFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusEntryNumberCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OutwardReportFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.NZCustomsOutwardReport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ExportManifest; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}

using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefFacilityModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.RefFacility;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.RefFacility;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.RefFacility);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new RefFacilityFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new RefFacilityFilterControl(GridCollection, (RefFacilityFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new RefFacilityCollection(Factory);

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects.Cast<RefFacility>().Any(s => s.RFT_IsSystem))
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("458232C2-D5E4-410B-96EE-45A1DEC7CFBD", "System created reference files cannot be deleted."));
			}
			else
			{
				base.HandleDeleteClickCore(sender, e);
			}
		}
	}
}

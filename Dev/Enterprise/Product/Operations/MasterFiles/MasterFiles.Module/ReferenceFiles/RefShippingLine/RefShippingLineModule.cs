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
	public class RefShippingLineModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.RefShippingLine;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.RefShippingLine;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.RefShippingLine);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new RefShippingLineFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new RefShippingLineFilterControl(GridCollection, (RefShippingLineFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new RefShippingLineCollection(Factory);

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects.Cast<RefShippingLine>().Any(s => s.RSL_IsSystem))
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

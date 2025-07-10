using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.Module
{
	public class StatementsStampDutyModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.TR.StatementsStampDuty;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.StatementsStampDuty;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CustomsStatement);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new StatementsStampDutyFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new StatementsStampDutyFilterControl(GridCollection, (StatementsStampDutyFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new StatementsStampDutyCollection(Factory);

		public override bool AllowNew => true;
		public override bool AllowDelete => true;
		public override bool AllowEdit => true;
		public override bool SupportsWorkflow => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			var exportTextsMenuItem = new ZMenuItem(ResString.GetMultilingualString("534DABAB-A752-46E6-9137-8C23A9914D88", "Export Texts"));
			exportTextsMenuItem.Click += new EventHandler(ExportTextsMenuItem_Click);
			result.Add(exportTextsMenuItem);

			return result.ToArray();
		}

		void ExportTextsMenuItem_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(ResString.GetMultilingualString("7257E46C-B5C7-4CE9-9A1A-C3E8D82B696C", "This menu item will be used in a future work item.", "Information"));
		}
	}
}

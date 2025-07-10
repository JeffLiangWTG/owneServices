using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccComplianceSequenceModule : ZFilterGridModule
	{
		public AccComplianceSequenceModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccComplianceSequence; }
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("74713010-CF83-4c45-ABC2-BFF7B8F35A3D", "Deactivate", "Deletes the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccComplianceSequence);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccComplianceSequenceFilterControl(GridCollection, (AccComplianceSequenceFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccComplianceSequenceCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccComplianceSequenceFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.ComplianceSequences;
			}
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return AccComplianceSequenceWorkflowDescriptor.WorkflowTypeCode; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menus = base.GetNewActionMenuItems().ToList();
			menus.Add(new ZMenuItem(ResString.GetMultilingualString("4703b971-8f45-424f-9268-43246f0fbadb", "Bulk Create Compliance Sequence Books"), new EventHandler(BulkCreateButton_Click)));

			return menus.ToArray();
		}

		void BulkCreateButton_Click(object sender, EventArgs e) => new AccComplianceSequnceBulkController().ShowNewForm();
	}
}

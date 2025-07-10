using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public partial class ContainerManagerModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public ContainerManagerModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AgencyContainerManager; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AgencyContainerManager);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ContainerManagerFilterControl(GridCollection, (ContainerManagerFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefContainerStockCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ContainerManagerFilterStrip();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			MenuItem[] result = base.GetNewActionMenuItems();

			Array.Resize(ref result, result.Length + 1);
			result[result.Length - 1] = new ZMenuItem(ResString.GetMultilingualString("ContainerManagerModule.AddBulkMovements", "Add Bulk Movements"), AddBulkMovements);

			return result;
		}

		void AddBulkMovements(object sender, EventArgs e)
		{
			if (!Env.Security.AgencyContainerManagerEdit.IsAllowed)
			{
				Env.Security.AgencyContainerManagerEdit.ShowError();
			}
			else
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				BulkMovementsHeader header = new BulkMovementsHeader(factory);
				BulkMovementsForm form = new BulkMovementsForm(header);
				SetLastShownBulkMovementsFormForTest(form);
				form.Show();
			}
		}

		partial void SetLastShownBulkMovementsFormForTest(BulkMovementsForm form);

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerContainerControl; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AgencyContainerManager; }
		}

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new ContainerManagerActionSupporter(); }
		}

		#endregion

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.ContainerStockManagerWorkflowDescriptorCode; }
		}
	}
}

#region Test
#if DEBUG

#region Test Members

namespace Enterprise.Freight.Agency.Module
{
	partial class ContainerManagerModule
	{
		public BulkMovementsForm LastShownBulkMovementsFormForTest { get; set; }

		partial void SetLastShownBulkMovementsFormForTest(BulkMovementsForm form)
		{
			LastShownBulkMovementsFormForTest = form;
		}
	}
}

#endregion


#endif
#endregion

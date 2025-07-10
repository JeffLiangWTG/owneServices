using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public abstract class OrgSupplierPartModule : ZFilterGridModule, IOperationalActionSupportable
	{
		protected OrgSupplierPartModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddImportDataMenuItem(Res.GetString("caa2a26a-d0f0-47bb-961d-7d8c4a612125", "From CSV"), ImportFromCSV);
			AddImportDataMenuItem(Res.GetString("55be9236-fe4e-4162-97d9-4ff8bbb4e514", "Last Cost From CSV"), ImportLastCostFromCSV);
			AddInterfaceConnectorImportMenuItem(Res.GetString("19a2460b-3395-4373-b5b3-be58105fa6d1", "&XML Product"), OnProductXmlImport);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem bulkRelationshipsChangeMenuItem = new ZMenuItem(ResString.GetMultilingualString("MasterFiles.OrgSupplierPart.Action.BulkRelationshipsChange", "Bulk Relationships Change"));
			bulkRelationshipsChangeMenuItem.Click += BulkRelationshipsChangeMenuItem_Click;
			result.Add(bulkRelationshipsChangeMenuItem);

			MenuItem bulkDeactivationMenuItem = new ZMenuItem(ResString.GetMultilingualString("04ad2170-d2af-492f-b1c8-e532d3aaa83a", "Bulk Activate/Deactivate Products"));
			bulkDeactivationMenuItem.Click += BulkDeactivationMenuItem_Click;
			result.Add(bulkDeactivationMenuItem);

			return result.ToArray();
		}

		protected void BulkRelationshipsChangeMenuItem_Click(object sender, EventArgs e)
		{
			if (!Env.Security.CustomsSupplierPartModify.IsAllowed)
			{
				Globals.Message.Show(Env.Security.CustomsSupplierPartModify.ErrorMessageForNotAllowed, Res.GetString("1fcec78e-b882-48c8-b867-059f571c9b36", "Access Denied"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				var changer = new OrgSupplierBulkRelationshipChanger(new BusinessObjectFactory());
				changer.EstimatedProductCount = ((BusinessObjectCollection)GridCollection).GetEstimatedLoadCount(FilterBusinessObject.Filter);
				if (ZFormModaliser.ShowDialogAndDispose(new BulkRelationshipChangeForm(changer)) == DialogResult.OK)
				{
					using (var form = new ProgressForm { ShowProgressBar = true, TopMost = true })
					{
						form.Status = Res.GetString("9d803194-4386-4fd6-a08a-5e90888a6929", "Modifying Products...");
						form.Cancelled += delegate
						{ changer.Cancel(); };
						form.Show();

						changer.ProgressChanged += p => RefreshProgressForm(form, p);
						GridCollection.Factory.RefreshEnabled = false;
						changer.ChangeRelatedOrganisations(FilterBusinessObject.Filter);
						GridCollection.Factory.RefreshEnabled = true;
					}

					var message = Res.GetString("f7b9f8b4-6510-47c0-9881-3a868ab93da9"
						, @"Modification completed:

{0} relationships have been added.
{1} relationships have been removed.
{2} relationships have been changed.
{3} relationships have been ignored because the requested update results in validation errors. 
The most likely reason for the error is that the requested change would have resulted in a duplicate product
or the removal of ownership of a product with current stock on hand quantities in the warehouse module.
{4} relationships have been ignored because they did not match the criteria or no change is needed."
						, changer.RelationshipsAdded
						, changer.RelationshipsDeleted
						, changer.RelationshipsUpdated
						, changer.RelationshipsThatFailedValidaton
						, changer.ProductsSkipped);

					Globals.Message.Show(message);
				}
			}
		}

		protected void BulkDeactivationMenuItem_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory();
			var changer = new OrgSupplierBulkDeactivator(factory);
			changer.ProductFilter = FilterBusinessObject.Filter;
			changer.CalculateEstimatedProductCount();

			if (ZFormModaliser.ShowDialogAndDispose(new BulkDeactivationForm(changer)) == System.Windows.Forms.DialogResult.OK)
			{
				var message = Res.GetString("94e2ff63-2e79-47b7-8231-c0b13dd8ae43", "Are you sure you want to continue?");

				var result = Globals.Message.Show(message, Res.GetString("32819a1c-74bc-4af8-8aef-6ac59c1f0a55", "Continue"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (result == DialogResult.OK)
				{
					using (ProgressForm form = new ProgressForm())
					{
						form.ShowProgressBar = true;
						form.TopMost = true;
						form.Status = changer.ShouldDeactivate ?
										Res.GetString("f2f2c864-89ef-4eb6-9282-b0d319c5133a", "Deactivating Products....") :
										Res.GetString("9517be68-07bd-4b88-ac27-0555ac18a00f", "Activating Products...");

						form.Cancelled += delegate
						{ changer.Cancel(); };
						form.Show();

						changer.ProgressChanged += p => RefreshProgressForm(form, p);

						changer.Deactivate();
					}

					ZString msg = changer.ShouldDeactivate ?
							Res.GetString("7632eb5a-f2bc-453b-84ba-ff5ef69f9702", "Deactivated") :
							Res.GetString("665dfa1a-29b2-4093-93d3-51feadac4a9a", "Activated");

					message = Res.GetString("03c76319-0a8d-4dad-8fd5-e3a65d94f14e", "{0} Products {1}", changer.ProductsChanged.ToString().Trim(), msg);
					var notUpdatedProductsCount = changer.EstimatedProductCount - changer.ProductsChanged;
					if (!changer.Cancelled && (notUpdatedProductsCount > 0 || changer.ProductsChanged > 0))
					{
						var notUpdatedErrorMessage = changer.ShouldDeactivate
							? Res.GetString("f875fd31-1781-406f-897d-00ba284453f3",
							@"{0} Products cannot be deactivated, because they have current stock on hand quantities in the warehouse module or there are ASN Lines on un-finalized receives referencing the product. 
You must remove these quantities first, either using a Warehouse Release or Warehouse Adjustment and finalize/cancel any Warehouse Receives with ASN Lines referencing the product. 
Print a Warehouse Stock on Hand report or use the Warehouse Inventory module to find these stock quantities and use the Warehouse Receive module to find any un-finalized receives.",
							notUpdatedProductsCount)
							: Res.GetString("16d62287-06a8-4aeb-ba9f-c828ef3e3175",
							"{0} Products cannot be activated, because they either have a duplicate product code or barcode on other products with the same owner.",
							notUpdatedProductsCount);

						message += "\r\n\r\n" + notUpdatedErrorMessage;
					}

					result = Globals.Message.Show(message, Res.GetString("9aa82077-7e36-45e1-8a18-d1011201a424", "Finished"), MessageBoxButtons.OK, MessageBoxIcon.Information);

					PerformSearchWhenDeactivationProcessComplete();
				}
			}
		}

		static void RefreshProgressForm(ProgressForm form, int percentage)
		{
			if (percentage > form.PercentComplete)
			{
				form.PercentComplete = percentage;
				form.Refresh();
			}
		}

		void PerformSearchWhenDeactivationProcessComplete()
		{
			OrgSupplierPartFilterStripBusinessObject filter = FilterBusinessObject as OrgSupplierPartFilterStripBusinessObject;
			OrgSupplierPartFilterStripControl filterControl = EmbeddedControl as OrgSupplierPartFilterStripControl;

			if (filter != null && filterControl != null)
			{
				try
				{
					filter.OverrideIsIsExpensiveQuery = true;
					filterControl.OverrideShouldShowNumberLoadedMessageBox = true;

					filterControl.Find();
				}
				finally
				{
					filter.OverrideIsIsExpensiveQuery = false;
					filterControl.OverrideShouldShowNumberLoadedMessageBox = false;
				}
			}
		}

		#region Import from CSV

		public void ImportFromCSV(object sender, EventArgs e)
		{
			new ImportProductsFromCSVForm().Show();
		}

		public void ImportLastCostFromCSV(object sender, EventArgs e)
		{
			new ImportLastCostFromCSVForm().Show();
		}

		#endregion

		#region Import From XML

		void OnProductXmlImport(Object sender, EventArgs e)
		{
			GetNewOrgDataTransferImporter().PromptUserAndImport(BillingInterfaceName.ProductXMLImport);
		}

		IXmlDataTransferImporter GetNewOrgDataTransferImporter()
		{
			return ObjectFactory.Get<IXmlDataTransferImporter>("ProductXmlDataTransferDirector");
		}

		#endregion

		public override ModuleIdentifier ID => ModuleIDs.SupplierPart;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.SupplierPart);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgSupplierPartFilterStripControl(GridCollection, (OrgSupplierPartFilterStripBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgSupplierPartCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgSupplierPartFilterStripBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsSupplierPart;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode;

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => GetOperationalActionSupporterCore();

		protected virtual OperationalActionSupporter GetOperationalActionSupporterCore()
		{
			return new OrgSupplierPartActionSupporter();
		}

		#endregion
	}
}

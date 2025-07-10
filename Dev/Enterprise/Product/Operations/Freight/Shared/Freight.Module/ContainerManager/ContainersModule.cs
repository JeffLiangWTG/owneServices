using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Module
{
	public class ContainersModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Containers;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ContainerWorkflowDescriptorCode;

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(Res.GetString("Freight|ContainersModule|ImportFromXMLMenuItem", "From &XML"), new EventHandler(OnImportFromXml_Click));
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> standardMenuItems = new List<MenuItem>();
			standardMenuItems.AddRange(base.GetNewStandardMenuItems());
			if (GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.Australia)
			{
				standardMenuItems.Add(new ZMenuItem("-"));
				standardMenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("262d39a6-b036-44cd-881a-2ca80ad6b4c6", "Pay Storage Charges"), PayStorageCharges_OnClick));
			}
			return standardMenuItems.ToArray();
		}

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.CusContainer };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Containers);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ContainersFilterControl(GridCollection, (ContainerManagerFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ContainerNonDependentCollection(Factory);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			RegisterGuiProviders(factory);
			return factory;
		}

		void RegisterGuiProviders(BusinessObjectFactory factory)
		{
			ServicesSelectionGuiProvider.Register(factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ContainerManagerFilterStrip();
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ForwardingContainer;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Forwarder;

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			using (XmlDataImporterForm form = XmlDataImporterForm.Create(BillingInterfaceName.ContainerXmlImport))
			{
				form.Importer = new ContainerEventsDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void PayStorageCharges_OnClick(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				ZController containerStoragePaymentController = ZControllerFactory.Create(ControllerIDs.ContainerStoragePayment);
				containerStoragePaymentController.ShowEditForm(SelectedBusinessObjects[0]);
			}
		}

#if DEBUG
		internal MenuItem[] GetNewStandardMenuItemsForTest() => GetNewStandardMenuItems();
		internal FilterModuleMenuItemDescriptorCollection ImportMenuItemsForTest => ImportMenuItems;
#endif
	}
}

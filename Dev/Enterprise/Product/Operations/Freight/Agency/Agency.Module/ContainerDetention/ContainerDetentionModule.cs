using System;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public partial class ContainerDetentionModule : ZFilterGridModule, IOperationalActionSupportable, IDocumentBusinessContext
	{
		public ContainerDetentionModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AgencyContainerDetention; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AgencyContainerDetention);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ContainerDetentionFilterControl(GridCollection, (ContainerDetentionFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ContainerDetentionCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ContainerDetentionFilterStrip();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] result = base.GetNewStandardMenuItems();

			MenuItem newSingle = new ZMenuItem(ResString.GetMultilingualString("233bdaed-d500-423b-bd30-3d6010faac1b", "Single Client"), NewSingleDetention);
			newSingle.Name = "Single Client";
			newSingle.DefaultItem = true;

			MenuItem newBulk = new ZMenuItem(ResString.GetMultilingualString("8d3910d1-aa71-4439-a906-b35a75b44cfb", "Multiple Clients"), BulkNewDetention);
			newBulk.Name = "Multiple Clients";

			NewMenuItem.MenuItems.Add(newSingle);
			NewMenuItem.MenuItems.Add(newBulk);

			return result;
		}

		void NewSingleDetention(object sender, EventArgs args)
		{
			ShowNewForm();
		}

		void BulkNewDetention(object sender, EventArgs args)
		{
			if (!Env.Security.AgencyContainerDetentionNew.IsAllowed)
			{
				Env.Security.AgencyContainerDetentionNew.ShowError();
			}
			else
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				BulkDetentionHeader header = new BulkDetentionHeader(newFactory);

				CreateBulkContainerDetentionForm form = new CreateBulkContainerDetentionForm(header);
				SetLastShownFormForTesting(form);
				form.Show();
			}
		}

		partial void SetLastShownFormForTesting(CreateBulkContainerDetentionForm form);

		#region Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerContainerDetention; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AgencyContainerDetention; }
		}

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new ContainerDetentionActionSupporter(); }
		}

		#endregion

		#region IDocumentBusinessContext members

		BusinessContext IDocumentBusinessContext.BusinessContext
		{
			get { return BusinessContext.AgencyDtnAdvice; }
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Testing Members

namespace Enterprise.Freight.Agency.Module
{
	partial class ContainerDetentionModule
	{
		public CreateBulkContainerDetentionForm LastFormForTesting { get; set; }

		partial void SetLastShownFormForTesting(CreateBulkContainerDetentionForm form)
		{
			LastFormForTesting = form;
		}
	}
}

#endregion


#endif
#endregion

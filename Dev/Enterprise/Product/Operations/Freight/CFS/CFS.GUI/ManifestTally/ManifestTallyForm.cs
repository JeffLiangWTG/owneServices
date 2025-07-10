using System;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ManifestTallyForm : ZTemplateForm
	{
		public ManifestTallyForm(TallyContainer packContainerBusiness) : base(packContainerBusiness)
		{
			InitializeComponent();

			MinimumSize = Size;
			SetUpTabPages();
			HookEvents();

			if ((bool)ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICACustomsDataRegistry>().RNSActive.Value)
			{
				PlugIns.Add(ControllerIDs.Customs.CA.RNSMFTallyPlugIn);
			}

			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia && TallyContainer.IsSeaContainer && !TallyContainer.IsExport())
			{
				PlugIns.Add(ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController);
			}
		}

		protected TallyContainer TallyContainer
		{
			get { return (TallyContainer)this.BusinessEntity; }
		}

		#region OWinForm Override

		public override string FormCaption
		{
			get { return Res.GetString("b82a98e0-6fde-497c-ac09-093947a1b89d", "Manifest Tally {0}", TallyContainer.JC_ContainerJobID); }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (TallyContainer != null)
			{
				UnHookEvents();
			}

			base.Dispose(disposing);
		}

		protected void SetUpTabPages()
		{
			PackagesUserControl = new PackagesUserControl();
			PackagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			MainTabPage.Controls.Add(PackagesUserControl);
		}

		protected void ShipmentCollection_OnAttemptedToDeleteDeliveredShipment(object sender, EventArgs args)
		{
			Globals.Message.ShowError(Res.GetString("0fbce843-23a6-494b-b8db-d0d19dc0039a", "This shipment has been saved - you cannot delete it."));
		}

		protected void PackCollection_OnAttemptedToDeleteDeliveredPack(object sender, EventArgs args)
		{
			Globals.Message.ShowError(Res.GetString("381ac9ce-1549-4bb1-8ec0-63937c960970", "This Pack Line has been saved - you cannot delete it."));
		}

		#region Hook / Unhook Events

		void HookEvents()
		{
			TallyContainer.PackUnpackShipments.AttemptedToDeleteSavedShipment += new EventHandler(ShipmentCollection_OnAttemptedToDeleteDeliveredShipment);
			TallyContainer.PackUnpackShipments.AttemptedToDeleteSavedPack += new EventHandler(PackCollection_OnAttemptedToDeleteDeliveredPack);
		}

		void UnHookEvents()
		{
			TallyContainer.PackUnpackShipments.AttemptedToDeleteSavedShipment -= new EventHandler(ShipmentCollection_OnAttemptedToDeleteDeliveredShipment);
			TallyContainer.PackUnpackShipments.AttemptedToDeleteSavedPack -= new EventHandler(PackCollection_OnAttemptedToDeleteDeliveredPack);
		}

		#endregion
	}
}


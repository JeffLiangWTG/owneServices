using System;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsBrokerageUserControl : Customs.GUI.BaseCustomsBrokerageUserControl
	{
		private readonly System.ComponentModel.Container components;

		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Overrides to get Cusdec-specific user controls
		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			return new CustomsInvoiceHeaderUserControl();
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			var declaration = (JobDeclaration)JobDeclaration;
			return declaration.IsTSWCREWriteOff ? new CustomsCREInvoiceLinesUserControl() : new CustomsInvoiceLineUserControl();
		}

		protected override Customs.GUI.BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new CustomsContainersWithTrackingUserControl();
		}

		protected override Customs.GUI.BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new NZCustomsDeclarationUserControl();
		}

		protected override Customs.GUI.BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new CustomsMiscOptionUserControl();
		}

		protected override Customs.GUI.IBasePackingControl GetPackingUserControl()
		{
			return new CustomsPackingUserControl();
		}

		protected override Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new CustomsMessageUserControl();
		}
		#endregion

		protected override void JobDeclaration_MergedSuccessfully()
		{
			// Base functionality selects the Entries Tab. Don't want this in NZ.
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string MiscOrganizaionDoesNotExistWarningMessage =
			"MISC organization does not exist.\n\n" +
			"To add a MISC organization:\n\n" +
			"  Allow organization codes to be edited.\n" +
			"  - Navigate to Maintain > System > Registry.\n" +
			"  - Click on the Organizations > Codes registry.\n" +
			"  - Click on 'Organization Codes can be edited'\n" +
			"  - Check the 'Override Default' box.\n" +
			"  - Select Yes in the Option panel.\n" +
			"  - Click on the Save button.\n" +
			"  - Click on the Close button.\n\n" +
			"  Add a new organization with the code 'MISC'.\n" +
			"  - Navigate to Maintain > Reference Files > Organization\n" +
			"  - Click on the Organizations registry.\n" +
			"  - Click the New button (Shortcut Ctrl-F3).\n" +
			"  - Enter 'MISC' in the Organization Code field.\n" +
			"  - Enter dummy data in all other required fields.\n" +
			"  - Click on the Save & Close button.\n\n" +
			"  Revert the 'Organization Codes can be edited' registry back to default.\n" +
			"  - Navigate to Maintain > System > Registry.\n" +
			"  - Click on the Organizations > Codes registry.\n" +
			"  - Click on 'Organization Codes can be edited'\n" +
			"  - Un-check the 'Override Default' box.\n" +
			"  - Click on the Save button.\n" +
			"  - Click on the Close button.\n\n" +
			"  Restart CargoWise One.\n\n" +
			"If you have any problems, please call your system administrator.";

#if DEBUG
		public bool ReturnEmptyMiscOrganisation;
#endif

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning && !pluginsLoaded)
			{
				pluginsLoaded = true;
				AddPlugins();
			}

			bool isMiscOrganisationRegistryEmpty = Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation.IsEmpty;

#if DEBUG
			isMiscOrganisationRegistryEmpty = ReturnEmptyMiscOrganisation || isMiscOrganisationRegistryEmpty;
#endif

			if (isMiscOrganisationRegistryEmpty)
			{
				string caption = "MISC organization does not exist";
				Globals.Message.ShowWarning(MiscOrganizaionDoesNotExistWarningMessage, caption);
			}
		}
		bool pluginsLoaded;

		internal void AddPlugins(bool forceLoad = false)
		{
			var plugIns = MainTabControl.PlugIns;
			var id = ControllerIDs.Customs.NZ.MAFeBACCaDeclarationPlugin;
			if (plugIns.GetPlugIn(id) == null)
			{
				plugIns.Add(id);
			}

			if (forceLoad)
			{
				plugIns.GetPlugIn(id); //Creates instance of the plug in
			}
		}
	}
}



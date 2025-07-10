using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI.PlugIn
{
	public abstract class CustomsCargoManifestPlugin : CustomsManifestPlugIn
	{
		public CustomsCargoManifestPlugin(IManifestProvider manifestProvider) : base(manifestProvider)
		{
		}

		#region IZPlugIn Members

		protected override MenuItem GetNewTopLevelMenu()
		{
			if (mainMenuItem == null)
			{
				mainMenuItem = new ZMenuItem(MainMenuText);
				SetupTopLevelMenu();
				if (mainMenuItem.MenuItems.Count == 0)
				{
					mainMenuItem.Dispose();
					mainMenuItem = null;
				}
			}
			return mainMenuItem;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return CustomsManifestStatus;
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected CustomsManifestStatus CustomsManifestStatus
		{
			get
			{
				if (fCustomsManifestStatus == null && ManifestProvider != null)
				{
					fCustomsManifestStatus = GetCustomsManifestStatus(ManifestProvider);
				}
				return fCustomsManifestStatus;
			}
		}
		CustomsManifestStatus fCustomsManifestStatus;

		protected override Control GetNewUserControl()
		{
			return new ZManifestMessageHistoryUserControl();
		}

		public sealed override string Name
		{
			get { return NameCore; }
		}

		protected abstract string NameCore { get; }
		#endregion

		#region Implementation

		protected MenuItem mainMenuItem;

		protected virtual MultilingualString MainMenuText
		{
			get { return ResString.GetMultilingualString("CustomCargoManifest|Menu|CustomsExportManifest", "Customs Manifest"); }
		}

		protected abstract CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider);

		#region Menu Item Click Handlers

		protected virtual void SetupTopLevelMenu()
		{
			MenuItem menuItemDeclareManifest = new ZMenuItem(DeclareManifestMenuText);
			menuItemDeclareManifest.Click += delegate
			{ DeclareManifest(); };
			mainMenuItem.MenuItems.Add(menuItemDeclareManifest);

			MenuItem menuItemWithdrawManifest = new ZMenuItem(WithdrawManifestMenuText);
			menuItemWithdrawManifest.Click += delegate
			{ WithdrawManifest(); };
			mainMenuItem.MenuItems.Add(menuItemWithdrawManifest);
		}

		protected virtual MultilingualString DeclareManifestMenuText
		{
			get { return ResString.GetMultilingualString("CustomCargoManifest|Menu|DeclareManifest", "Declare &Manifest"); }
		}

		protected virtual MultilingualString WithdrawManifestMenuText
		{
			get { return ResString.GetMultilingualString("CustomCargoManifest|Menu|WithdrawManifest", "&Withdraw Manifest"); }
		}

		protected virtual void DeclareManifest()
		{
			if (CustomsManifestStatus != null)
			{
				CustomsManifestStatus.DeclareManifest(new SendsMessagesToCustomsGUI());
			}
		}

		protected virtual void WithdrawManifest()
		{
			if (CustomsManifestStatus != null)
			{
				CustomsManifestStatus.WithdrawManifest(new SendsMessagesToCustomsGUI());
			}
		}

		#endregion

		#endregion

	}

	#region CustomsCargoManifestPluginController and it's subclasses and Shipment and Consol

	public abstract class CustomsCargoManifestPluginController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Non persistent top level business object (CustomsManifestStatus)"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not implemented yet");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsCargoManifestOnForwarding; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsCargoManifestOnForwarding; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsCargoManifestOnForwarding; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsCargoManifestOnForwarding; }
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|CustomsCargoManifestPluginController", "Customs Manifest"); }
		}
	}

	public abstract class CustomsCargoManifestPluginToConsolController : CustomsCargoManifestPluginController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CargoManifestPlugInForConsol; }
		}
	}

	#endregion
}

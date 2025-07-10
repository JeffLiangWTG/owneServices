using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class ConsolidatedDeclarationForm : ZTemplateForm
	{
		public ConsolidatedDeclarationForm()
		{
			InitializeComponent();
		}

		public ConsolidatedDeclarationForm(ConsolidatedDeclaration consolidatedDeclaration, IConsolidatedDeclarationFormAdaptationsProvider adaptionsProvider) : base(consolidatedDeclaration)
		{
			this.consolidatedDeclaration = consolidatedDeclaration;
			InitializeComponent();
			WorkflowTabPage.Initialize(consolidatedDeclaration);
			if (adaptionsProvider != null)
			{
				var messagesControl = adaptionsProvider.MessagesTabUserControl;
				messagesControl.Dock = System.Windows.Forms.DockStyle.Fill;
				BindingSource.SetBindingMember(messagesControl, "Messages");
				MessagesTabPage.Controls.Add(messagesControl);
				if (adaptionsProvider.DeclarationGridExtraColumnInfos != null)
				{
					foreach (var column in adaptionsProvider.DeclarationGridExtraColumnInfos)
					{
						JobDeclarationsGrid.ColumnStyles.Add(column);
					}
				}

				panelLayout = adaptionsProvider.HeaderDetailsLayout;
				if (adaptionsProvider.EnableDocumentMenuItem)
				{
					PlugIns.Add(ControllerIDs.DocDataPlugIn);
				}

				var menuBuilder = adaptionsProvider.EDIMenuBuilder;
				MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), menuBuilder.BuildMenu());
				menuBuilder.ConsolidatedDeclaration = consolidatedDeclaration;
			}

			AddAttachDeclarationMenuItem();
			AddRemoveDeclarationMenuItem();

			if (consolidatedDeclaration.LeadDeclaration?.IsPost ?? false)
			{
				JobDeclarationsGrid.GetColumnStyle(AutoJobDeclaration.Schema.JE_HouseBill).CaptionResourceString = Res.GetData("3f49b4c1-ac5b-48b2-8545-bf912cdf2f20", "Parcel Post No.", "Parcel Post Number", "Parcel Post Number of this declaration");
				JobDeclarationsGrid.RemoveFromAvailableColumns(AutoJobDeclaration.Schema.JE_MasterBill);
			}
		}

		public override string FormCaption
		{
			get { return Res.GetString("E296C474-1CAB-4EB1-A3A3-32D8958E145E", "Consolidated Entry {0}", consolidatedDeclaration?.CRD_JobReferenceNumber); }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DynamicPanel.UpdateLayout(panelLayout);
		}

		protected override bool AllowNew => false;

		void EDIMenu_Click(object sender, EventArgs e)
		{
			var ediMenu = (IEDIMenu)sender;
			ediMenu.Declaration = ediMenu.Declaration ?? consolidatedDeclaration.BuildAggregateJobDeclaration();
		}

		void OpenJobDeclarationButton_Click(object sender, EventArgs e)
		{
			PrepareDeclarationsToBeOpened();
			findBox.CurrentCode = consolidatedDeclaration.LeadDeclaration.JE_DeclarationReference;
			(findBox as IFindBoxUserControl).ShowEditOrViewForm();
		}

		void JobDeclarationsGrid_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				var row = JobDeclarationsGrid.HitTest(e.X, e.Y).Row;
				if (row > -1 && row < consolidatedDeclaration.JobDeclarations.Count)
				{
					PrepareDeclarationsToBeOpened();
					findBox.CurrentCode = consolidatedDeclaration.JobDeclarations[row].JE_DeclarationReference;
					(findBox as IFindBoxUserControl).ShowEditOrViewForm();
				}
			}
		}

		protected override Dictionary<IBusiness, ZString> GetListOfBizObjectsToCheckEditing()
		{
			var result = base.GetListOfBizObjectsToCheckEditing();
			foreach (var declaration in consolidatedDeclaration.JobDeclarations)
			{
				var shipment = declaration.Shipment;
				if (shipment != null)
				{
					result.Add(shipment, GenerateShipmentFormCaption(shipment));
				}
				else
				{
					result.Add(declaration, GenerateDeclarationFormCaption(declaration));
				}
			}

			return result;
		}

		protected virtual ZString GenerateDeclarationFormCaption(BaseJobDeclaration declaration)
		{
			return Res.GetString("0103B5A4-AEF1-4BE5-BB1A-165574A3EB11", "Customs Declaration - {0}", declaration.JE_DeclarationReference);
		}

		protected virtual ZString GenerateShipmentFormCaption(ForwardingShipment shipment)
		{
			return shipment.HumanReadableNameWithoutID + " " + shipment.JS_UniqueConsignRef;
		}

		void PrepareDeclarationsToBeOpened()
		{
			if (findBox == null)
			{
				findBox = new ZGuidFindBox();
				findBox.ModuleID = ModuleIDs.Customs.JobDeclaration;
				var declarationList = new BaseJobDeclarationCollection(consolidatedDeclaration.Factory);
				declarationList.AddRange(consolidatedDeclaration.JobDeclarations);
				findBox.List = declarationList;
			}
		}

		void AddRemoveDeclarationMenuItem()
		{
			removeDeclarationMenuItem = new ZMenuItem(RemoveFromConsolidationCaption, RemoveDeclarationMenuItemClick);
			JobDeclarationsGrid.ContextMenu.MenuItems.Add(removeDeclarationMenuItem);
			JobDeclarationsGrid.ContextMenu.Popup += JobDeclarationsGridContextMenu_Popup;
		}

		void AddAttachDeclarationMenuItem()
		{
			attachDeclarationMenuItem = new ZMenuItem(AttachDeclarationCaption, AttachDeclarationMenuItemClick);
			attachDeclarationMenuItem.Enabled = !consolidatedDeclaration.Messages.Any();
			JobDeclarationsGrid.ContextMenu.MenuItems.Add(attachDeclarationMenuItem);
		}

		void AttachDeclarationMenuItemClick(object sender, EventArgs e)
		{
			var caption = AttachDeclarationCaption.Replace("&", "");

			if (consolidatedDeclaration.HasChanges)
			{
				Globals.Message.ShowError(SaveBeforeAttachDeclarationCaption, caption);
				return;
			}

			var jobDeclarationModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration) as ZFilterGridModule;
			jobDeclarationModule.DoNotShowRecentItems = true;
			jobDeclarationModule.DoNotCheckOrSaveChanges = true;
			jobDeclarationModule.FilterBusinessObject.ParentModuleID = ModuleIDs.Customs.ConsolidatedDeclaration;

			if (consolidatedDeclaration.DefaultAttachDeclarationFilter != null)
			{
				jobDeclarationModule.FilterBusinessObject.SetExternalDefaults(consolidatedDeclaration.DefaultAttachDeclarationFilter);
			}

			var form = new NewOrAttachConsolidatedDeclarationForm(consolidatedDeclaration, jobDeclarationModule, true);
			ZFormModaliser.Show(form, this);
		}

		void RemoveDeclarationMenuItemClick(object sender, EventArgs e)
		{
			var caption = RemoveFromConsolidationCaption.Replace("&", "");
			CurrentDeclaration.ConsolidatedEntryProvider.DequeueOrRemoveFromConsolidation(out var resultMessage);
			Globals.Message.ShowInformation(resultMessage, caption);
		}

		void JobDeclarationsGridContextMenu_Popup(object sender, EventArgs e)
		{
			removeDeclarationMenuItem.Visible = CurrentDeclaration?.ConsolidatedEntryProvider.CanRemove ?? false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				removeDeclarationMenuItem?.Dispose();
				findBox?.Dispose();
			}
			base.Dispose(disposing);
		}

		BaseJobDeclaration CurrentDeclaration => JobDeclarationsGrid.ListManager?.GetCurrent() as BaseJobDeclaration;

		ResourceString AttachDeclarationCaption => ResString.GetMultilingualString("12F6FA74-3BA1-4897-815F-4E2FD2AD0BDA", "&Attach Declaration");
		ResourceString RemoveFromConsolidationCaption => ResString.GetMultilingualString("ConsolidatedDeclarationForm|RemoveFromConsolidation", "Remove from Conso&lidation");
		string SaveBeforeAttachDeclarationCaption => Res.GetString("313499D9-2C3F-47BD-851E-6199C331E3D6", "You must save the current Consolidated Declaration details before attaching Declarations.");

		readonly ConsolidatedDeclaration consolidatedDeclaration;
		readonly IPanelLayoutProvider panelLayout;
		ZGuidFindBox findBox;
		ZMenuItem attachDeclarationMenuItem;
		ZMenuItem removeDeclarationMenuItem;
	}
}

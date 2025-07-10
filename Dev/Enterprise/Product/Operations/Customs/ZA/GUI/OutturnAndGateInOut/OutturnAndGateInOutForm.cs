using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Module;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Business.AsycudaManifestHeader;
[assembly: UsesConstants(typeof(DeclarationFilterConstants))]

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutForm : ZTemplateForm
	{
		public OutturnAndGateInOutForm(AsycudaManifestHeader businessObject)
			: base(businessObject)
		{
			AddMessagingMenus();
			SetupActionsMenu();
			WorkflowTabPage.Initialize(businessObject);
			PlugIns.Add(ControllerIDs.Audit);
		}

		public new AsycudaManifestHeader BusinessEntity => (AsycudaManifestHeader)base.BusinessEntity;

		public override string FormCaption
		{
			get
			{
				if (!this.IsDesignMode())
				{
					if (BusinessEntity.IsInDatabase)
					{
						return Res.GetString("OutturnAndGateInOutForm|FormCaptionDb", $"{BusinessEntity.AMA_JobReference}");
					}
				}
				return Res.GetString("OutturnAndGateInOutForm|FormCaption", "Outturn & Gate In/Out");
			}
		}

		void AddMessagingMenus()
		{
			var actionMenuItemIndex = MainMenu.MenuItems.IndexOf(ActionsMenuItem);

			var outturnMenu = new OutturnMenu(BusinessEntity);
			MainMenu.MenuItems.Add(actionMenuItemIndex + 1, outturnMenu);

			var gateInOutMenuItem = new OutturnGateInOutMenu(BusinessEntity);
			MainMenu.MenuItems.Add(actionMenuItemIndex + 2, gateInOutMenuItem);
		}
		void SetupActionsMenu()
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("13aa715e-aa5f-4a0a-8194-351e3293c529", "Copy From Manifest Job"), CopyFromManifestJobMenuItem_Click);
		}

		void CopyFromManifestJobMenuItem_Click(object sender, EventArgs eventArgs)
		{
			if (BusinessEntity.HasChanges)
			{
				Globals.Message.ShowInformation(Res.GetString("d5ff679d-0f93-4132-9eed-64d3db29d000", "Please save the form before copying."));
			}
			else if (BusinessEntity.Containers.Count > 0)
			{
				Globals.Message.ShowInformation(Res.GetString("435f5329-600c-45e4-bb50-778168d5a4e2", "Please delete the containers and save the form before copying."));
			}
			else
			{
				var collection = (Integration.Customs.ASYCUDA.IAsycudaManifestModuleCollection)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.ASYCUDA.IAsycudaManifestModuleCollection>(), BusinessEntity.Factory);
				((IFilterBusinessObjectDefaultsProvider)collection).FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(DeclarationFilterConstants.Country, "Property", (ZString)Core.Constants.CountryCodes.SouthAfrica, false));

				var recordChooser = new AsycudaManifestViewChooser(collection);
				recordChooser.ShowModal(this, delegate(ManifestBase.AsycudaManifestHeader[] selectedHeaders)
				{
					var manifestHeader = selectedHeaders.FirstOrDefault();
					if (manifestHeader != null)
					{
						var copyBO = new BaseAsycudaManifestHeaderCopyBO(manifestHeader, BusinessEntity);
						copyBO.SetReadOnlyIncludingChildren(true);
						if (copyBO.SourceContainers.Count == 0)
						{
							copyBO.CopyValuesFromGlobalManifest();
						}
						if (copyBO.SourceContainers.Count == 1)
						{
							copyBO.SourceContainerToCopy = copyBO.SourceContainers[0];
							copyBO.CopyValuesFromGlobalManifest();
						}
						else if (copyBO.SourceContainers.Count > 1)
						{
							var selectionContainerForm = new ContainersSelectionForm(copyBO);
							ZFormModaliser.Show(selectionContainerForm, this);
						}
					}
				});
			}
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}
	}
}

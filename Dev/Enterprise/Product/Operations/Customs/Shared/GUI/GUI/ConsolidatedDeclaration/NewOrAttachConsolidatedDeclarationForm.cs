using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class NewOrAttachConsolidatedDeclarationForm : ZChildForm
	{
		public NewOrAttachConsolidatedDeclarationForm(ConsolidatedDeclaration businessEntity, ZFilterGridModule module, bool attachDeclaration, BaseJobDeclaration jobDeclaration = null) : base(businessEntity)
		{
			jobDeclarationModule = module;
			AttachDeclaration = attachDeclaration;
			consolidatedDeclaration = businessEntity;
			declaration = jobDeclaration;
			InitializeComponent();
			createButton.CaptionResourceString = CreateButtonResourceString;
			filterControlPanel.Controls.Add(jobDeclarationModule.EmbeddedControl);
		}

		public override IBusiness BusinessEntity => consolidatedDeclaration ?? base.BusinessEntity;

		public override string FormCaption
		{
			get
			{
				if (consolidatedDeclaration == null)
				{
					return Res.GetString("E60C0283-0913-4D68-9D45-077524B57F3B", "New Consolidated Entry");
				}
				return Res.GetString("E296C474-1CAB-4EB1-A3A3-32D8958E145E", "Consolidated Entry {0}", consolidatedDeclaration?.CRD_JobReferenceNumber);
			}
		}

		ResourceStringData CreateButtonResourceString => AttachDeclaration ? Res.GetData("CA4BA535-EA8D-496B-88C5-9605C2655CC6]", "Attach") : Res.GetData("5579CEE6-FE5E-41A0-8089-6DF8B76845EA", "Create");

		void CreateButton_Click(object sender, System.EventArgs e)
		{
			var selected = jobDeclarationModule.DisplayGrid.SelectedElements;

			if (consolidatedDeclaration == null)
			{
				var type = ZControllerFactory.Create(ControllerIDs.Customs.ConsolidatedDeclaration).TypeOfTopLevelBusinessObject;
				consolidatedDeclaration = declaration.Factory.New(type) as ConsolidatedDeclaration;
			}

			if (!AttachDeclaration)
			{
				consolidatedDeclaration.JobDeclarations.RemoveAll();
			}

			if (declaration != null && !selected.Select(x => x.PK).Contains(declaration.PK))
			{
				Globals.Message.ShowError(Res.GetString("56C96FD1-E3B2-486A-8387-00A9B4B105B6", "Consolidated Declaration: Job Number \'{0}\' must be one of the selected rows.", declaration.JobNumber));
			}
			else
			{
				foreach (var bizo in selected)
				{
					if (consolidatedDeclaration.Factory.GetBizOsForPK(bizo.PK.ToGuid()).Length == 0)
					{
						consolidatedDeclaration.JobDeclarations.Add(consolidatedDeclaration.Factory.ImportFromAnotherFactory(bizo));
					}
					else
					{
						consolidatedDeclaration.JobDeclarations.Add(consolidatedDeclaration.Factory.Load<BaseJobDeclaration>(bizo.PK));
					}
				}

				if (declaration != null)
				{
					consolidatedDeclaration.CRD_JE_LeadDeclaration = declaration.PK;
				}

				consolidatedDeclaration.CalculateHeaderFees();

			if (!AttachDeclaration)
			{
				consolidatedDeclaration.OnCreatedWithCustomsDeclarations();
			}

			FireSaveButton();

				if (LastSaveSucceeded)
				{
					if (!AttachDeclaration)
					{
						ShowConsolidatedDeclarationEditForm();
					}

					Close();
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("69DEDA32-290D-42B7-AEE2-666B12423CAA", "Save failed"));
				}
			}
		}

		void ShowConsolidatedDeclarationEditForm()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.Customs.ConsolidatedDeclaration))
			{
				module?.GetNewController().ShowEditForm(consolidatedDeclaration);
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		bool AttachDeclaration { get; }
		readonly ZFilterGridModule jobDeclarationModule;
		ConsolidatedDeclaration consolidatedDeclaration;
		readonly BaseJobDeclaration declaration;
	}
}

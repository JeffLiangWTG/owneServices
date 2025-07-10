using System.Collections.Generic;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.US.GUI
{
	public partial class JobDeclarationForm : BaseJobDeclarationForm
	{
		public JobDeclarationForm() { }

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
			if (declaration != null)
			{
				if (declaration.IsDrawback)
				{
					this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 820);
				}

				declaration.RefreshExRateToLatestRateAvailableIfNeeded();
			}
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		protected override void AddPlugins()
		{
			if (Declaration.IsDrawback)
			{
				PlugIns.AddJobInvoicing(Declaration.InvoicingSupporter);
				PlugIns.Add(ControllerIDs.DocAddresses);
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);
			}
			else
			{
				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Customs.US.InBond, RoutingTabIndex + 1);
				base.AddPlugins();
				PlugIns.Add(ControllerIDs.DocumentVisualizer);
			}
		}

		protected override void ShowNewForm()
		{
			if (ControllerID == ControllerIDs.Customs.US.USLowValueEntriesBill)
			{
				var controller = ZControllerFactory.Create(ControllerID);
				var declaration = controller.Factory.New<JobDeclaration>();
				declaration.US_EntryType = EntryTypeList.Codes.LowValue;
				declaration.JE_MessageType = USJobMessageTypeList.Codes.Import;
				controller.ShowFormForNewEntity(declaration);
			}
			else
			{
				base.ShowNewForm();
			}
		}

		protected override IEDIMenu GetNewTopLevelMenuCore()
		{
			return new EDIMenu();
		}

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl()
		{
			if (Declaration.IsDrawback)
			{
				return new USDrawbackCustomsBrokerageUserControl();
			}
			else
			{
				return new CustomsBrokerageUserControl();
			}
		}

		protected override BaseShipmentAndBrokerageCommon GetShipmentAndBrokergeCommon(Customs.Business.BaseJobDeclaration declaration)
		{
			return new ShipmentAndBrokerageCommon(declaration);
		}

		protected override string FormCaptionCore
		{
			get
			{
				var result = base.FormCaptionCore;
				var usDeclaration = Declaration;
				if (usDeclaration != null)
				{
					if (usDeclaration.IsDrawback)
					{
						result = "Drawback";
					}
					else if (usDeclaration.IsFTZAdmission)
					{
						result = "Foreign Trade Zone Declaration";
					}
				}
				return result;
			}
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories1)
		{
			var factories = new List<CargoWise.Integration.ITransactionParticipant>(factories1);
			if (Declaration.ShouldSaveEDocsMasterFactoryTogether)
			{
				var eDocsFactory = Declaration.DocManagerInfo.MasterFactory;
				if (!factories.Contains(eDocsFactory))
				{
					factories.Add(eDocsFactory);
				}
			}
			base.Save(factories.ToArray());
		}

		protected override bool EnableControlLock
		{
			get { return !Declaration.IsDrawback && !Declaration.IsReconMessageType; }
		}

		public override bool IsResizableByTabPageAllowed => true;
	}
}

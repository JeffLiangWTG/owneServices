using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Module.OperationalActions;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public abstract class JobDeclarationModuleBase : ZFilterGridModule, IModuleCountryAcceptor
	{
		public JobDeclarationModuleBase()
		{
			CountryCode = GlbCompany.CurrentCompany.Country.Code;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobDeclarationFilterBusinessObject();
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, ImportXmlDeclarations);
		}

		protected bool IsCountryCodeSameAsCurrentCountry => CountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public override string WorkflowType => JobInvoicingConsumerTypes.Brokerage.Code;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Customs };

		public override bool AllowEdit => base.AllowEdit && IsCountryCodeSameAsCurrentCountry;

		public override bool AllowNew => base.AllowNew && IsCountryCodeSameAsCurrentCountry;

		public override bool AllowView => base.AllowView && IsCountryCodeSameAsCurrentCountry;

		public override bool AllowDelete => base.AllowDelete && IsCountryCodeSameAsCurrentCountry;

		#region IModuleCountryAcceptor Members

		public ZString CountryCode
		{
			get; set;
		}

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			BaseJobDeclaration jobDeclaration = selectedBusinessObject as BaseJobDeclaration;
			if ((jobDeclaration == null || jobDeclaration.Shipment == null) && !(selectedBusinessObject is DeclarationFromShipmentPuller))
			{
				return GetControllerForStandAlone();
			}
			else
			{
				return GetControllerForShipment();
			}
		}

		protected abstract JobDeclarationController GetControllerForStandAlone();

		protected abstract JobDeclarationShipmentController GetControllerForShipment();

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobDeclarationFilterStripControl(this, GridCollection, (JobDeclarationFilterBusinessObject)FilterBusinessObject);
		}

		protected override void OnBeforePerformSearchCore()
		{
			if (!CountryCode.IsEmpty && typeof(JobDeclarationFilterBusinessObject).IsAssignableFrom(FilterBusinessObject.GetType()))
			{
				((JobDeclarationFilterBusinessObject)FilterBusinessObject).CountryCode = CountryCode;
			}
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new BaseJobDeclarationCollection(Factory);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetGuiProviders(factory);
			return factory;
		}

		protected void SetGuiProviders(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ServicesSelectionGuiProvider.Register(factory);
			}
		}

		protected virtual DeclarationXmlDataTransferDirector GetNewDeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter adapter)
		{
			return new DeclarationXmlDataTransferDirector(adapter, true);
		}

		void ImportXmlDeclarations(object sender, EventArgs args)
		{
			DeclarationXmlDataTransferDirector director = GetNewDeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter.New());
			director.PromptUserAndImport(BillingInterfaceName.DeclarationXmlImport);
		}

		protected override DragAndDropSingleFileHandler GetDragAndDropHandler()
		{
			return delegate (string fileName)
			{
				DeclarationXmlDataTransferDirector director = GetNewDeclarationXmlDataTransferDirector(DeclarationValueObjectDataAdapter.New());
				director.PromptUserAndImport(fileName, BillingInterfaceName.DeclarationXmlImport);
			};
		}

		protected virtual BusinessObject[] GetSelectedElements()
		{
			return Grid.SelectedElements;
		}
	}

	public class JobDeclarationModule : JobDeclarationModuleBase, IOperationalActionSupportable
#if DEBUG
		, IBulkPostingModuleInternalsForTesting
#endif
	{
		public JobDeclarationModule() : base()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override bool SupportsWorkflow => true;

		public override ModuleIdentifier ID => ModuleIDs.Customs.JobDeclaration;

		public OperationalActionSupporter OperationalActionSupporter => new BaseJobDeclarationOperationalActionSupporter();

		protected override JobDeclarationController GetControllerForStandAlone()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration) as JobDeclarationController;
#if DEBUG
			((IFilterGridModuleInternalsForTesting)this).LastController = controller;
#endif
			return controller;
		}

		protected override JobDeclarationShipmentController GetControllerForShipment()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment) as JobDeclarationShipmentController;
#if DEBUG
			((IFilterGridModuleInternalsForTesting)this).LastController = controller;
#endif
			return controller;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsDeclarationEnquiry;

		protected virtual bool HasExWarehouseMenu => false;

		protected internal MenuItem[] GetNewStandardMenuItemsTest() => GetNewStandardMenuItems();
		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menus = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (IsCountryCodeSameAsCurrentCountry && HasExWarehouseMenu && NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(ResString.GetMultilingualString("f5f1cc23-6463-4c0c-bcb1-093356a67744", "New &Declaration"), delegate
				{ ShowNewForm(); });
				NewMenuItem.MenuItems.Add(ResString.GetMultilingualString("66e6bd33-1711-4a98-a1e2-192d4fbdf7a7", "New &Ex-warehouse"), NewExWarehouse_Click);
			}
			return menus.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> menus = new List<MenuItem>();

			if (IsCountryCodeSameAsCurrentCountry)
			{
				menus.AddRange(base.GetNewAdditionalMenuItems());
			}

			return menus.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>();

			KMenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as KMenuItem;
			postMenuItem.MenuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);
			if (IsCountryCodeSameAsCurrentCountry)
			{
				result.AddRange(base.GetNewActionMenuItems());
				result.Add(postMenuItem);
				result.Add(new ZMenuItem("-"));
				result.Insert(result.Count, createFromShipmentMenuItem = new ZMenuItem(ResString.GetMultilingualString("ea61940a-3650-4047-8d8b-4b6e95c8eef8", "Create Declaration on Shipment"), new EventHandler(CreateDeclarationOnExistingShipment_Click)));
				result.Insert(result.Count, importMenuItem = new ZMenuItem(ResString.GetMultilingualString("5dfeac62-a286-4445-811c-03b166ee6131", "Import Declaration from a job created in another country"), new EventHandler(CreateAndImportDeclarationFromAnotherDeclaration_Click)));
			}
			else
			{
				result.Add(postMenuItem);
			}

			AddCopyDeclarationOnlyMenuItem(result);
			AddMultiDeclarationEntryMenuItem(result);
			AddTransportBookingMenuItem(result);
			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, result).AddJobsMenuItem();
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(this, result);

			if (Env.CurrentUser.IsDeveloper && Env.Registry.EnableCustomsDiagnostics)
			{
				result.Add(new ZMenuItem("-"));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA Message Processor Service Task", new EventHandler(RunOneCAMessageProcessCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA Interchange Provider Service Task", new EventHandler(RunOneCAInterchangeProviderCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA eHub Interchange Processor Service Task", new EventHandler(RunOneCAInterchangeEHubProcessorCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA CIG Sender Service Task (for Canada)", new EventHandler(RunOneCIGSenderCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA CIG Receiver Service Task (for Canada)", new EventHandler(RunOneCIGReceiverCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA CBSA Client Sender Service Task", new EventHandler(RunOneSPClientSenderCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA CBSA Client Receiver Service Task", new EventHandler(RunOneSPClientReceiverCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA CBSA Server Sender Service Task", new EventHandler(RunOneSPServerSenderCycle_Click)));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CA CBSA Server Receiver Service Task", new EventHandler(RunOneSPServerReceiverCycle_Click)));
				result.Add(new ZMenuItem("-"));
				result.Insert(result.Count, new ZMenuItem((NoResString)"Run one cycle of CustomsWareService Task", new EventHandler(RunOneCustomsWareCycle_Click)));
			}
			return result.ToArray();
		}

		void RunOneCIGSenderCycle_Click(object sender, EventArgs e)
		{
			Globals.Message.Show((NoResString)"CIG Sender ACI Test");
			Type type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.CIGSender, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAACI, "CAT" });
			MethodInfo methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			Globals.Message.Show((NoResString)"CIG Sender EXP Test");
			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAEXP, "CAT" });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			Globals.Message.Show((NoResString)"CIG Sender IMP Test");
			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAIMP, "CAT" });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			Globals.Message.Show((NoResString)"CIG Sender ACI Prod");
			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAACI, "CAP" });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			Globals.Message.Show((NoResString)"CIG Sender EXP Prod");
			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAEXP, "CAP" });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			Globals.Message.Show((NoResString)"CIG Sender IMP Prod");
			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAIMP, "CAP" });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}

		void RunOneCIGReceiverCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.CIGReceiver, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			MethodInfo methodInfo = type.GetMethod("GetInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}

		void RunOneSPClientSenderCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.SPClientSender, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAACI });
			MethodInfo methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAEXP });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);

			o = Activator.CreateInstance(type, new object[] { EDIInterchange.ApplicationCodes.CAIMP });
			methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}

		void RunOneSPClientReceiverCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.SPClientReceiver, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			MethodInfo methodInfo = type.GetMethod("GetInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}

		void RunOneCAMessageProcessCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.Business.CAMessageProcessor, Enterprise.Customs.CA.Business");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			MethodInfo methodInfo = type.GetMethod("ExecuteBatchForDebug", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, Array.Empty<object>());
			DisplayLog(o);
		}

		void RunOneCAInterchangeProviderCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.Business.BatchProcessor.Sender, Enterprise.Customs.CA.Business");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			MethodInfo methodInfo = type.GetMethod("ExecuteBatchForDebug", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, Array.Empty<object>());
			DisplayLog(o);
		}

		void RunOneCAInterchangeEHubProcessorCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.Business.BatchProcessor.CACInboundeHubInterchangeProcessor, Enterprise.Customs.CA.Business");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			MethodInfo methodInfo = type.GetMethod("ExecuteBatchForDebug", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, Array.Empty<object>());
			DisplayLog(o);
		}

		void RunOneSPServerSenderCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.SPServerSender, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			MethodInfo methodInfo = type.GetMethod("SendInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}

		void RunOneSPServerReceiverCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CA.ServiceTasks.SPServerReceiver, Enterprise.Customs.CA.ServiceTasks");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			MethodInfo methodInfo = type.GetMethod("GetInterchanges", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, null);
			DisplayLog(o);
		}

		void RunOneCustomsWareCycle_Click(object sender, EventArgs e)
		{
			Type type = Type.GetType("Enterprise.Customs.CustomsWare.ServiceTasks.InterchangeProcessorServiceTask, Enterprise.Customs.CustomsWare.ServiceTasks");
			object o = Activator.CreateInstance(type, Array.Empty<object>());
			var methodInfo = type.GetMethod("RunTask", BindingFlags.Public | BindingFlags.Instance);
			methodInfo.Invoke(o, Array.Empty<object>());
			DisplayLog(o);
			Factory.Save();
		}

		protected void DisplayLog(object o)
		{
			var batchProcessor = o as BatchProcess;
			if (batchProcessor != null)
			{
				var builder = new ZStringBuilder();
				foreach (string line in batchProcessor.Logger.DebugLogStrings)
				{
					builder.Append(line);
				}
				builder.Prepend(Res.GetString("71678989-4932-4d66-89f4-3ed8d7c937c3", "Result:"));
				Globals.Message.Show(builder.ToStringWithNewLineBetweenAppends());
			}
			else
			{
				var serviceTask = o as ICustomsServiceTaskProcess;
				if (serviceTask != null)
				{
					var builder = new ZStringBuilder();
					foreach (string line in serviceTask.Logger.DebugLogStrings)
					{
						builder.Append(line);
					}
					builder.Prepend(Res.GetString("71678989-4932-4d66-89f4-3ed8d7c937c3", "Result:"));
					Globals.Message.Show(builder.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		public static MultilingualString CopyDeclarationOnlyText => ResString.GetMultilingualString("9c9e0b3b-5d81-4d14-b0e2-7ffe159cfacf", "Copy Declaration Only");

		protected ZMenuItem createFromShipmentMenuItem;
		protected ZMenuItem importMenuItem;
		public ZMenuItem CopyDeclarationOnlyMenuItem;

		protected virtual void AddCopyDeclarationOnlyMenuItem(List<MenuItem> menuItems)
		{
			CopyDeclarationOnlyMenuItem = new ZMenuItem(CopyDeclarationOnlyText, new EventHandler(CopyDeclaration_Click));
			if (CopyMenuItem != null)
			{
				menuItems.Insert(menuItems.IndexOf(CopyMenuItem, 0, menuItems.Count) + 1, CopyDeclarationOnlyMenuItem);
			}
			else
			{
				if (importMenuItem != null)
				{
					menuItems.Insert(menuItems.IndexOf(importMenuItem, 0, menuItems.Count) + 1, CopyDeclarationOnlyMenuItem);
				}
				else
				{
					menuItems.Insert(menuItems.Count, CopyDeclarationOnlyMenuItem);
				}
			}
		}

		protected virtual void AddMultiDeclarationEntryMenuItem(List<MenuItem> menuItems)
		{
			MenuItem menuItem = new ZMenuItem(ResString.GetMultilingualString("e8b8a671-b357-4276-adba-7ee0bcc95fca", "Multi Declaration Entry"), new EventHandler(MultiDec_Click));
			menuItems.Add(menuItem);
		}

		protected virtual void AddTransportBookingMenuItem(List<MenuItem> menuItems)
		{
			Func<IDtbBookingParent> selectedObjectGetter = () => (IDtbBookingParent)CurrentBusinessObjectInGrid;
			var dtbBookingProvider = ObjectFactory.New<IDtbBookingMenuProvider>(selectedObjectGetter);
			menuItems.Add((MenuItem)dtbBookingProvider.ConstructMenu());
		}

		#region Posting

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
#if DEBUG
			if (((IBulkPostingModuleInternalsForTesting)this).DontDoActualPosting)
			{
				fLastUsedPostingOptionForTest = postingOption;
			}
			else
#endif
			{
				BulkPostingHelper.PostTransactions(postingOption, Grid.SelectedElements);
			}
		}

		IBulkPostingModuleHelper BulkPostingHelper
		{
			get
			{
				if (fBulkPostingHelper == null)
				{
					fBulkPostingHelper = ObjectFactory.Get<IBulkPostingModuleHelper>();
					fBulkPostingHelper.Initialize(Description, false);
				}

				return fBulkPostingHelper;
			}
		}
		IBulkPostingModuleHelper fBulkPostingHelper;

		#region IBulkPostingModuleInternalsForTesting Members

#if DEBUG
		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest => fLastUsedPostingOptionForTest;

		JobInvoicingPostingOption fLastUsedPostingOptionForTest;

		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				MenuItem menuItem = GetNewActionMenuItems().FindByText("&Post");
				IMenuItem iMenuItem = menuItem as IMenuItem;
				if (menuItem != null && iMenuItem == null)
				{
					throw new ApplicationException("Post menu item should have type ZMenuItem");
				}
				return iMenuItem;
			}
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}
#endif

		#endregion

		#endregion

		#region PrintJobProfitDocument

		public void PrintJobProfitDocument()
		{
			var elements = from BaseJobDeclaration element in GetSelectedElements() select (BusinessObject)element.Shipment ?? element;
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, elements.ToArray());
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper => fBulkJobProfitPrintingHelper ?? (fBulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>());

		IBulkJobProfitPrintingModuleHelper fBulkJobProfitPrintingHelper;

		#endregion

		void SafeOpenBrowser(string url)
		{
			try
			{
				WebUrlLauncher.Launch(url);
			}
			catch (Win32Exception ex)
			{
				ErrorReporter.ReportOnce("JobDeclarationModule|SafeOpenBrowser", "Failed to open web address for: " + url, ex);
				Globals.Message.ShowInformation(Res.GetString("60586f86-0f5c-4c23-adad-eca42f14fe08", "The web address for '{0}' could not be opened. Please try copy and pasting it into your web browser instead.", url));
			}
		}

		void NewExWarehouse_Click(object sender, EventArgs e)
		{
			GetControllerForStandAlone().ShowNewExWarehouseForm();
		}

		void CopyDeclaration_Click(object sender, EventArgs e)
		{
			BusinessObject[] selectedElements = GetSelectedElements();

			if (selectedElements != null && selectedElements.Length == 1)
			{
				BaseJobDeclaration selectedDeclaration = (BaseJobDeclaration)selectedElements[0];
				GetControllerForStandAlone().ShowTemplateCopyForm(selectedDeclaration);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("fd9f7b85-bdec-4e18-8411-8dc28546edec", "Please select one Declaration to copy."), Res.GetString("9185c63b-0437-40bb-8a2b-8ab9f4a6b419", "Copy Declaration"));
			}
		}

		void MultiDec_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MultiJobDeclarationHeader header = new MultiJobDeclarationHeader(factory);
			GetNewMultiJobDeclarationHeader(header).Show();
		}

		protected virtual MultiJobDeclarationForm GetNewMultiJobDeclarationHeader(MultiJobDeclarationHeader header)
		{
			return new MultiJobDeclarationForm(header);
		}

		protected internal virtual ImportJobDeclaration GetImportJobDeclarationForCreateAndImportDeclarationFromAnotherDeclaration() => new ImportJobDeclaration(Factory);

		public void CreateAndImportDeclarationFromAnotherDeclaration_Click(object sender, EventArgs e)
		{
			CreateAndImportDeclarationFromAnotherDeclaration(GetImportJobDeclarationForCreateAndImportDeclarationFromAnotherDeclaration());
		}

		public void CreateAndImportDeclarationFromAnotherDeclaration(ImportJobDeclaration importJobDeclaration)
		{
			using (var importDialog = new ImportDeclarationDialog(importJobDeclaration))
			{
				var result = ZFormModaliser.ShowDialogWithoutDispose(importDialog);

				if (result == DialogResult.OK)
				{
					ShowEditForm(importJobDeclaration);
				}
			}
		}

		protected void CreateDeclarationOnExistingShipment_Click(object sender, EventArgs e)
		{
			DeclarationFromShipmentPuller puller = new DeclarationFromShipmentPuller(Factory);
			using (DeclarationFromShipmentPullerDialog pullerDialog = new DeclarationFromShipmentPullerDialog(puller))
			{
				ZFormModaliser.ShowDialogWithoutDispose(pullerDialog);
				if (pullerDialog.CreateClicked)
				{
					ShowEditForm(puller);
				}
			}
		}
	}
}

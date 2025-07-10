using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UniversalDataMenuBuilderTest : TestCaseWithFactory
	{
		public void TestBuildMenuWithPopup()
		{
			var workflowProvider = new Mock<IWorkflowProvider>();
			var shipmentDataContextManager = new Mock<IShipmentDataContextManager>();
			var eventDataContextManager = new Mock<IEventDataContextManager>();
			var transactionDataContextManager = new Mock<ITransactionDataContextManager>();
			var scheduleDataContextManager = new Mock<IScheduleDataContextManager>();
			var activityDataContextManager = new Mock<IActivityDataContextManager>();

			shipmentDataContextManager.Setup(dcm => dcm.ManagesShipments).Returns(true);
			eventDataContextManager.Setup(dcm => dcm.ManagesEvents).Returns(true);
			transactionDataContextManager.Setup(dcm => dcm.ManagesTransactions).Returns(true);
			scheduleDataContextManager.Setup(dcm => dcm.ManagesSchedules).Returns(true);
			activityDataContextManager.Setup(dcm => dcm.ManagesActivities).Returns(true);

			var menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport,
				() => new[] { workflowProvider.Object }, shipmentDataContextManager.Object);

			AssertArrayEqualsByElements("Universal Shipment menu",
				new[] { "Universal Shipment" },
				FormatMenuDescriptors(menuBuilder.Build()));

			menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport,
				() => new[] { workflowProvider.Object }, eventDataContextManager.Object);

			AssertArrayEqualsByElements("Universal Event menu",
				new[] { "Universal Event" },
				FormatMenuDescriptors(menuBuilder.Build()));

			menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport,
				() => new[] { workflowProvider.Object }, transactionDataContextManager.Object);

			AssertArrayEqualsByElements("Universal Transaction menu",
				new[] { "Universal Transaction" },
				FormatMenuDescriptors(menuBuilder.Build()));

			menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport,
				() => new[] { workflowProvider.Object }, scheduleDataContextManager.Object);

			AssertArrayEqualsByElements("Universal Schedule menu",
				new[] { "Universal Schedule" },
				FormatMenuDescriptors(menuBuilder.Build()));

			menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport,
				() => new[] { workflowProvider.Object }, activityDataContextManager.Object);

			AssertArrayEqualsByElements("Universal Activity menu",
				new[] { "Universal Activity" },
				FormatMenuDescriptors(menuBuilder.Build()));

			var dataContextManager = new DummyWithWorkflowDataContextManager();
			menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport,
				() => new[] { workflowProvider.Object }, dataContextManager);

			AssertArrayEqualsByElements("All menus",
				new[]
				{
					"Universal Shipment",
					"Universal Event",
					"Universal Transaction",
					"Universal Schedule",
					"Universal Activity",
				},
				FormatMenuDescriptors(menuBuilder.Build()));
		}

		public void TestMenuWithPopupShowDataExportForm()
		{
			var workflowProvider = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();
			var shipmentDataContextManager = new Mock<IShipmentDataContextManager>();

			shipmentDataContextManager.Setup(dcm => dcm.ManagesShipments).Returns(true);

			var menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ManualDataExport, () => new[] { workflowProvider }, shipmentDataContextManager.Object);

			var menuItemDescr = menuBuilder.Build().ElementAt(0);

			menuItemDescr.Handler(null, EventArgs.Empty);

			AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(ZFormModaliser.LastFormShownDialogForTest.GetType(), typeof(ManualDataExportForm));
			ZFormModaliser.LastFormShownDialogForTest = null;

			workflowProvider.HasChanges = true;

			menuItemDescr.Handler(null, EventArgs.Empty);

			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Please save your changes before sending XML Universal Data.");
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestBuildMenuWithReceipients()
		{
			var workflowProvider = new Mock<IWorkflowProvider>();
			var shipmentDataContextManager = new Mock<IShipmentDataContextManager>();
			shipmentDataContextManager.Setup(dcm => dcm.ManagesShipments).Returns(true);

			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.Print |
					MessageRecipientPartyType.BillToParty;

			var menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ReceipientsList,
				() => new[] { workflowProvider.Object }, shipmentDataContextManager.Object,
				DummyWorkflowDescriptor.Instance);

			AssertArrayEqualsByElements("All menus",
				new[]
				{
					"Bill to Party(s)",
					"Consignee",
					"Organization Proxy"
				},
				FormatMenuDescriptors(menuBuilder.Build()));
		}

		public void TestMenuWithReceipientsShowDataExportProgressForm()
		{
			var workflowProvider = Factory.New<DummyWithWorkflow>();
			var shipmentDataContextManager = new Mock<IShipmentDataContextManager>();
			shipmentDataContextManager.Setup(dcm => dcm.ManagesShipments).Returns(true);

			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.Print;

			var menuBuilder = new UniversalDataMenuBuilder(UniversalDataMenuBuilder.Menus.ReceipientsList,
				() => new[] { workflowProvider }, shipmentDataContextManager.Object, DummyWorkflowDescriptor.Instance);

			var menuItemDescr = menuBuilder.Build().ElementAt(0);

			menuItemDescr.Handler(null, EventArgs.Empty);

			AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals(ZFormModaliser.LastFormShownDialogForTest.GetType(), typeof(ManualDataExportProgressForm));
			ZFormModaliser.LastFormShownDialogForTest = null;

			workflowProvider.HasChanges = true;

			menuItemDescr.Handler(null, EventArgs.Empty);

			AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "Please save your changes before sending XML Universal Data.");
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestSupportedPartyCodesCoverage()
		{
			var allPartyCodes = typeof(MessageRecipientPartyTypeList.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fieldInfo => fieldInfo.IsLiteral)
				.Select(fieldInfo => fieldInfo.GetValue(null))
				.Where(value => value.ToString() != MessageRecipientPartyTypeList.Codes.CartageAgent) // Cartage Agent Recipient should not be visible for Universal Export
				.Where(value => value.ToString() != MessageRecipientPartyTypeList.Codes.SGAccess) // SG Acess Recipient should not be visible for Universal Export
				.Where(value => value.ToString() != MessageRecipientPartyTypeList.Codes.USAirAMS); // US Air AMS should not be visible for Universal Export

			var workflowProvider = Factory.New<DummyWithWorkflow>();
			var shipmentDataContextManager = new Mock<IShipmentDataContextManager>();
			var menuBuilder = new UniversalDataMenuBuilderForTest(UniversalDataMenuBuilder.Menus.ReceipientsList,
				() => new[] { workflowProvider }, shipmentDataContextManager.Object);
			var listGetter = ObjectFactory.New<IAllPossibleRecipientTypesGetter>();
			var recipientCodesThatDoNotSupportUniversalXml = listGetter.GetListOfRecipientsWhichCannotReceiveUniversalXml().Cast<ICodeDescription>().Select(cdp => cdp.Code);

			var handledPartyCodes = menuBuilder.SupportedPartyCodesExposed.Concat(recipientCodesThatDoNotSupportUniversalXml).Concat(new[]
			{
				MessageRecipientPartyTypeList.Codes.WiseNettingSystem,
			});

			var message =
$@"The contents of {nameof(MessageRecipientPartyTypeList)}.{nameof(MessageRecipientPartyTypeList.Codes)} have changed.
These may be valid recipients for Universal XML. This would be an external party (an organisation record) which can have Universal XML sent to them via eHub.
*** It should NEVER include email address recipients as opposed to organisations. This would consitute a free way to bypass the paid version of e2e via eHub. ***
If you have added a new organisation-based recipient, consider adding the recipient to {nameof(UniversalDataMenuBuilder)}.SupportedPartyCodes, meaning this organisation will be able to receive Universal XML.
Otherwise, exclude the new code in either:
	• {nameof(IAllPossibleRecipientTypesGetter)}.{nameof(IAllPossibleRecipientTypesGetter.GetListOfRecipientsWhichCannotReceiveUniversalXml)} -- Preferred
	• OR, the exclusion list in this test -- If for some reason this recipient SHOULD be in the RecipientRoleType enum, but somehow is not expected in this menu builder.
the exclusion list in this test.";

			AssertContainsExactElementsInAnyOrder(message,
				allPartyCodes, handledPartyCodes);
		}

		public void TestBuildUniversalDataReceipientsMenu()
		{
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.OrgProxy;

			var universalDataSupportable = new Mock<IBulkSendUniversalDataSupportable>();
			universalDataSupportable.Setup(supportable => supportable.NameOfSingleObject).Returns("Dummy");
			universalDataSupportable.Setup(supportable => supportable.TypeOfSingleObject).Returns(typeof(DummyWithWorkflow));
			universalDataSupportable.Setup(supportable => supportable.WorkflowDescriptorCode).Returns("DUM");
			universalDataSupportable.Setup(supportable => supportable.GetElementsToSend()).Returns((IEnumerable<IWorkflowProvider>)null);

			var menu = UniversalDataMenuBuilder.BuildUniversalDataReceipientsMenu(universalDataSupportable.Object);

			AssertEquals("Export Dummy To", menu.Text);

			var subMenuItemNames = menu.MenuItems
				.Cast<MenuItem>()
				.Select(mi => mi.Text);

			AssertContainsExactElementsInAnyOrder(new[]
			{
					"Consignee", "Consignor", "Organization Proxy"
				},
			subMenuItemNames);

			menu.MenuItems[0].PerformClick();

			AssertEquals("Please select at least one Dummy.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var dummy = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			universalDataSupportable = new Mock<IBulkSendUniversalDataSupportable>();
			universalDataSupportable.Setup(supportable => supportable.NameOfSingleObject).Returns("Dummy");
			universalDataSupportable.Setup(supportable => supportable.TypeOfSingleObject).Returns(typeof(DummyWithWorkflow));
			universalDataSupportable.Setup(supportable => supportable.WorkflowDescriptorCode).Returns("DUM");
			universalDataSupportable.Setup(supportable => supportable.GetElementsToSend()).Returns(new[] { dummy });

			menu = UniversalDataMenuBuilder.BuildUniversalDataReceipientsMenu(universalDataSupportable.Object);

			menu.MenuItems[0].PerformClick();

			AssertNotNull("export universal data form was shown", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("export universal data form was shown", ZFormModaliser.LastFormShownDialogForTest.GetType(), typeof(ManualDataExportProgressForm));

			universalDataSupportable = new Mock<IBulkSendUniversalDataSupportable>();
			universalDataSupportable.Setup(supportable => supportable.NameOfSingleObject).Returns("Dummy");
			universalDataSupportable.Setup(supportable => supportable.TypeOfSingleObject).Returns(typeof(DummyWithWorkflow));
			universalDataSupportable.Setup(supportable => supportable.WorkflowDescriptorCode).Returns("XXX");
			universalDataSupportable.Setup(supportable => supportable.GetElementsToSend()).Returns(new[] { dummy });

			AssertNull("Invalid workflow descriptor", UniversalDataMenuBuilder.BuildUniversalDataReceipientsMenu(universalDataSupportable.Object));

			universalDataSupportable = new Mock<IBulkSendUniversalDataSupportable>();
			universalDataSupportable.Setup(supportable => supportable.NameOfSingleObject).Returns("Dummy");
			universalDataSupportable.Setup(supportable => supportable.TypeOfSingleObject).Returns(typeof(DummyBizObj));
			universalDataSupportable.Setup(supportable => supportable.WorkflowDescriptorCode).Returns("DUM");
			universalDataSupportable.Setup(supportable => supportable.GetElementsToSend()).Returns(new[] { dummy });

			AssertNull("Invalid data context manager", UniversalDataMenuBuilder.BuildUniversalDataReceipientsMenu(universalDataSupportable.Object));
		}

		#region Implementation

		class UniversalDataMenuBuilderForTest : UniversalDataMenuBuilder
		{
			public UniversalDataMenuBuilderForTest(Menus menu, Func<IEnumerable<IWorkflowProvider>> provider, IDataContextManager dataContextManager)
				: base(menu, provider, dataContextManager)
			{
			}

			public IEnumerable<string> SupportedPartyCodesExposed
			{
				get { return SupportedPartyCodes; }
			}
		}

		string[] FormatMenuDescriptors(IEnumerable<UniversalDataMenuItemDescriptor> descriptors)
		{
			return descriptors.Select(descr => descr.Caption.ToString()).ToArray();
		}

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		#endregion
	}
}

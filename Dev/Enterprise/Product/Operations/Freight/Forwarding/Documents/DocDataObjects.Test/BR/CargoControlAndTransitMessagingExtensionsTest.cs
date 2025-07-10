using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BR
{
	sealed class CargoControlAndTransitMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithResetToOriginal()
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			var shipment = Factory.New<ForwardingShipment>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;

			var dynamicData = new Mock<IDynamicData>();

			var cct = new CargoControlAndTransit(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef);

			dynamicData.SetupGet(d => d.Value).Returns(cct);

			var parameters = new DummyDocDataObjectParameters();
			parameters.LogProvider = Factory.New<DummyEnterpriseBusinessObject>();

			var logProvider = parameters.LogProvider;

			var logCreator = new AdvancedReportMessageLogCreator(DocumentNames.AdvancedCargoReport, Core.Constants.CountryCodes.Brazil);
			var res = logCreator.CreateResetToOriginalLog(logProvider, dynamicData.Object, "zzz");

			AssertEquals("log creator indicated that reset to original log has been created", true, res);

			var logs = logProvider.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.ToArray();

			AssertContainsExactElementsInAnyOrder("logs",
				new[]
				{
						"STU |DEP=CargoWise Support|LOC=BR|MST=Advanced Cargo Report|TYP=Reset To Original"
				},
				logs);
		}

		public void TestContinueWithSendingMessage()
		{
			Helper.CreateNewCCTPassword();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(new CargoControlAndTransit("testtype", "testid"));

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new CargoControlAndTransitMessagingExtensions(document.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("Disallow sending messages to CCT", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Once);

			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			dynamicData.Reset();
			document.Reset();
			notifications.Reset();

			res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertNull("Message sending returns null. (Message sending is deferred to base)", res);

			dynamicData.Verify(fake => fake.Value, Times.Never);
			document.Verify(fake => fake.Data, Times.Never);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment()
		{
			Helper.CreateNewCCTPassword();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(new CargoControlAndTransit("testtype", "testid"));

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new CargoControlAndTransitMessagingExtensions(document.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Disallow sending messages to CCT", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Once);

			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			dynamicData.Reset();
			document.Reset();
			notifications.Reset();

			res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertNull("Message sending returns null. (Message sending is deferred to base)", res);

			dynamicData.Verify(fake => fake.Value, Times.Never);
			document.Verify(fake => fake.Data, Times.Never);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Never);
		}

		public void TestContinueWithSendingMessageWithdrawal()
		{
			Helper.CreateNewCCTPassword();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(new CargoControlAndTransit("testtype", "testid"));

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new CargoControlAndTransitMessagingExtensions(document.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Disallow sending messages to CCT", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Once);

			GlbStaff.CurrentUser.GetBRWrapper().CCTPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			dynamicData.Reset();
			document.Reset();
			notifications.Reset();

			res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertNull("Message sending returns null. (Message sending is deferred to base)", res);

			dynamicData.Verify(fake => fake.Value, Times.Never);
			document.Verify(fake => fake.Data, Times.Never);
			notifications.Verify(fake => fake.ShowMessage("To send messages to CCT you must have a valid certificate loaded against your staff profile.", "Information"), Times.Never);
		}

		#region Implementation

		CargoControlAndTransitMessagingExtensionsTestHelper Helper => helper ?? (helper = new CargoControlAndTransitMessagingExtensionsTestHelper(Factory));
		CargoControlAndTransitMessagingExtensionsTestHelper helper;

		#endregion
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	sealed class CertifiedPickupBuilderTest : TestCaseWithFactory
	{
		#region Test Build

		public void TestBuild()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = DocDataObjects.BE.BelgianPortsConstants.DocumentNames.CPuReleaseRightAcceptDecline,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new CertifiedPickupBuilder(consol, parameters);
			var data = builder.Build();

			data.BillOfLading = "B0001";
			AssertNoMessageError("BOL is required", data.BillOfLadingInfo, "BOL Number is required.");

			data.BillOfLading = string.Empty;
			AssertHasMessageError("BOL is required", data.BillOfLadingInfo, "BOL Number is required.");
		}

		public void TestBuild_TransferMode_TransferDestinationIDValidations()
		{
			var consol = CreateConsol();
			CertifiedPickupContainerEventTestHelper.AddLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			CertifiedPickupContainerEventTestHelper.AddLogForContainer(consol.Containers[1], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = DocDataObjects.BE.BelgianPortsConstants.DocumentNames.CPuReleaseRightTransfer,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new CertifiedPickupBuilder(consol, parameters);
			var data = builder.Build();

			var containerData1 = data.SelectedContainers.ToArray()[0];
			var containerData2 = data.SelectedContainers.ToArray()[1];

			CombineAssertions("Not transfer to forwarder or transporter for container1.", () =>
			{
				Assert("Precondition: Not transfer to forwarder or transporter for container1.", !containerData1.Action.IsTransferToForwarder && !containerData1.Action.IsTransferToTransporter);
				AssertNoErrors("No error for ForwarderId.", data.ForwarderId.ValueInfo);
				AssertNoErrors("No error for TransportCompanyId.", data.TransportCompanyId.ValueInfo);
				AssertHasMessageError("Error for container1", containerData1.ErrorPlaceHolderInfo, "Please select Transfer To Party.");
				AssertNoErrors("No error for container2", containerData2.ErrorPlaceHolderInfo);
			});

			containerData1.Action.IsTransferToForwarder = true;
			CombineAssertions("Transfer to forwarder for container1.", () =>
			{
				Assert("Precondition: Transfer to forwarder for container1.", containerData1.Action.IsTransferToForwarder);
				AssertHasMessageError("Error for ForwarderId.", data.ForwarderId.ValueInfo, "Receiving Forwarder Identification is required. Please provide organization > Config > Registration Numbers / Codes - type 'PSN', 'DUN', 'EOR' or 'BTW'.");
				AssertNoErrors("No error for TransportCompanyId.", data.TransportCompanyId.ValueInfo);
				AssertNoErrors("No error for container1", containerData1.ErrorPlaceHolderInfo);
				AssertNoErrors("No error for container2", containerData2.ErrorPlaceHolderInfo);
			});

			containerData1.Action.IsTransferToTransporter = true;
			CombineAssertions("Transfer to transporter for container1.", () =>
			{
				Assert("Precondition: Transfer to transporter for container1.", containerData1.Action.IsTransferToTransporter);
				AssertNoErrors("No error for ForwarderId.", data.ForwarderId.ValueInfo);
				AssertHasMessageError("Error for TransportCompanyId.", data.TransportCompanyId.ValueInfo, "Transport Company identification code is required. Please provide organization > Config > Registration Numbers / Codes - type 'PSN', 'DUN', 'EOR' or 'BTW'.");
				AssertNoErrors("No error for container1", containerData1.ErrorPlaceHolderInfo);
				AssertNoErrors("No error for container2", containerData2.ErrorPlaceHolderInfo);
			});
		}

		#endregion

		#region	Visibility

		public void TestCertifiedPickupAccptOrDeclineFormFilterMacro()
		{
			var multiModalQuery = new ZQuery()
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Accept/Decline")
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol))
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, "Electronic Messaging/Port Messaging/Import/Certified Pickup (BE)");
			var menuItems = Factory.Load<StmMenuItem>(multiModalQuery);

			AssertEquals(1, menuItems.Length);

			var menuItem = menuItems.First();
			var filterCondition = menuItem.SU_FilterList;
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"HasCertifiedPickupAcceptOrDecline",
				filterCondition);
		}

		public void TestCertifiedPickupTransferFormFilterMacro()
		{
			var multiModalQuery = new ZQuery()
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Transfer")
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol))
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, "Electronic Messaging/Port Messaging/Import/Certified Pickup (BE)");
			var menuItems = Factory.Load<StmMenuItem>(multiModalQuery);

			AssertEquals(1, menuItems.Length);

			var menuItem = menuItems.First();
			var filterCondition = menuItem.SU_FilterList;
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"HasCertifiedPickupTransfer",
				filterCondition);
		}

		public void TestCertifiedPickupRevokeFormFilterMacro()
		{
			var multiModalQuery = new ZQuery()
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Revoke")
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol))
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, "Electronic Messaging/Port Messaging/Import/Certified Pickup (BE)");
			var menuItems = Factory.Load<StmMenuItem>(multiModalQuery);

			AssertEquals(1, menuItems.Length);

			var menuItem = menuItems.First();
			var filterCondition = menuItem.SU_FilterList;
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"HasCertifiedPickupRevoke",
				filterCondition);
		}

		#endregion

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ShippersConsol;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = DocDataObjects.BE.BelgianPortsConstants.DocumentNames.CPuReleaseRightAcceptDecline,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var data = new CertifiedPickupBuilder(consol, parameters).Build();
			AssertEquals(Core.Constants.ContainerModes.FCL, data.ContainerMode.Code);
			AssertEquals(Core.Constants.ContainerModeDescriptions.FCL, data.ContainerMode.Description);
		}

		#endregion

		#region Implement

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			return consol;
		}

		#endregion
	}
}

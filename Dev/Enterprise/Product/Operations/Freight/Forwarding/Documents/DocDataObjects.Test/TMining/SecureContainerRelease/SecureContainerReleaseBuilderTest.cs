using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using TMiningConstants = Enterprise.Freight.Forwarding.Documents.DocDataObjects.TMiningConstants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class SecureContainerReleaseBuilderTest : TestCaseWithFactory
	{
		#region Test Build

		public void TestBuild_TransferMode()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = TMiningConstants.DocumentNames.TMiningSecureContainerReleaseTransfer,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var notSelectedContainer = consol.Containers.AddNew();
			notSelectedContainer.JC_ContainerNum = "CONT3333333";
			notSelectedContainer.JC_ContainerImportDORelease = "REL3333";
			notSelectedContainer.JC_IsNonOperativeReefer = true;

			var builder = new SecureContainerReleaseBuilder(consol, parameters);
			var data = builder.Build();

			AssertEquals("From Mode", data.FormMode, SecureContainerRelease.FormModeTransfer);
			AssertEquals("Operational Port", data.OperationalPort.Code, "BEANR");
			AssertEquals("BOL", data.BillOfLading, "BOL123");
			AssertEquals("ContainerMode", data.ContainerMode.Code, Core.Constants.ContainerModes.FCL);

			AssertEquals("Number of SCR container is 2", 2, data.Containers.Count);

			var containerData1 = data.Containers.ToArray()[0];
			AssertEquals("Container Number", containerData1.Number, "CONT1111111");
			AssertEquals("Release Number", containerData1.ReleaseIdentification, "REL1111");
			AssertEquals("IsTransferToForwarder", containerData1.IsTranferToForwarder, true);
			AssertEquals("IsNonOperativeReefer", containerData1.IsNonOperativeReefer, true);

			var containerData2 = data.Containers.ToArray()[1];
			AssertEquals("Container Number", containerData2.Number, "CONT2222222");
			AssertEquals("Release Number", containerData2.ReleaseIdentification, "REL2222");
			AssertEquals("IsTransferToForwarder", containerData2.IsTranferToForwarder, true);
			AssertEquals("IsNonOperativeReefer", containerData2.IsNonOperativeReefer, false);
		}

		public void TestBuild_RevokeMode()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = TMiningConstants.DocumentNames.TMiningSecureContainerReleaseRevoke,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var notSelectedContainer = consol.Containers.AddNew();
			notSelectedContainer.JC_ContainerNum = "CONT3333333";
			notSelectedContainer.JC_ContainerImportDORelease = "REL3333";
			notSelectedContainer.JC_IsNonOperativeReefer = true;

			var builder = new SecureContainerReleaseBuilder(consol, parameters);
			var data = builder.Build();

			AssertEquals("From Mode", data.FormMode, SecureContainerRelease.FormModeRevoke);
			AssertEquals("Operational Port", data.OperationalPort.Code, "BEANR");
			AssertEquals("BOL", data.BillOfLading, "BOL123");
			AssertEquals("ContainerMode", data.ContainerMode.Code, Core.Constants.ContainerModes.FCL);

			AssertEquals("Number of SCR container is 2", 2, data.Containers.Count);

			var containerData1 = data.Containers.ToArray()[0];
			AssertEquals("Container Number", containerData1.Number, "CONT1111111");
			AssertEquals("Release Number", containerData1.ReleaseIdentification, "REL1111");

			var containerData2 = data.Containers.ToArray()[1];
			AssertEquals("Container Number", containerData2.Number, "CONT2222222");
			AssertEquals("Release Number", containerData2.ReleaseIdentification, "REL2222");
		}

		public void TestAddresses()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("YYY", "YYYBranch", testCompany);

			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE_EORI123", Core.Constants.CountryCodes.Belgium);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "BE_DUN123");

			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = TMiningConstants.DocumentNames.TMiningSecureContainerReleaseTransfer,
					Data = consol
						.Containers
						.OfType<ForwardingContainer>()
						.ToArray()
				};

				var builder = new SecureContainerReleaseBuilder(consol, parameters);
				var data = builder.Build();

				AssertionHelper.AssertAddressData(testCompany.OrgProxy?.MainAddress, data.SendingParty);
				AssertEquals("SendingPartyID EORI", "BE_EORI123", data.SendingPartyIdEOR.Value);
				AssertEquals("SendingPartyID DUN", "BE_DUN123", data.SendingPartyIdDUN.Value);

				AssertionHelper.AssertAddressData(consol.ReceivingForwarderAddress, data.Forwarder);
				AssertEquals("ForwarderID EOR", "DK_EORI123", data.ForwarderIdEOR.Value);
				AssertEquals("ForwarderID DUN", "DK_DUN123", data.ForwarderIdDUN.Value);

				AssertionHelper.AssertAddressData(consol.ArrivalUnpackCFSTransportAddress, data.TransportCompany);
				AssertEquals("TransportCompanyID EOR", "FR_EORI123", data.TransportCompanyIdEOR.Value);
				AssertEquals("TransportCompanyID DUN", "FR_DUN123", data.TransportCompanyIdDUN.Value);
			}
		}

		#endregion

		#region	Visibility

		public void TestSecureContainerReleaseTransferFormFilterMacro()
		{
			var multiModalQuery = new ZQuery()
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Transfer")
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol))
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, "Electronic Messaging/Port Messaging/Import/Secure Cont. Release");
			var menuItems = Factory.Load<StmMenuItem>(multiModalQuery);

			AssertEquals(1, menuItems.Length);

			var menuItem = menuItems.First();
			var filterCondition = menuItem.SU_FilterList;
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"HasSecureContainerReleaseTransfer",
				filterCondition);
		}

		public void TestSecureContainerReleaseRevokeFormFilterMacro()
		{
			var multiModalQuery = new ZQuery()
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Revoke")
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.Consol))
				.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, "Electronic Messaging/Port Messaging/Import/Secure Cont. Release");
			var menuItems = Factory.Load<StmMenuItem>(multiModalQuery);

			AssertEquals(1, menuItems.Length);

			var menuItem = menuItems.First();
			var filterCondition = menuItem.SU_FilterList;
			AssertContains($"{menuItem.SU_MenuName}|{menuItem.PK} - filter contains correct condition",
				"HasSecureContainerReleaseRevoke",
				filterCondition);
		}

		#endregion

		#region Validations

		public void TestValidations()
		{
			var forwarderIDErrorMessage = "Forwarder ID is missing from this organization. Provide Registration Number EOR or DUN.";
			var transportCompanyIDErrorMessage = "Transport Company ID is missing from this organization. Provide Registration Number EOR or DUN.";

			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = TMiningConstants.DocumentNames.TMiningSecureContainerReleaseTransfer,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new SecureContainerReleaseBuilder(consol, parameters);
			var data = builder.Build();

			var container1 = data.Containers.ToList()[0];
			container1.IsTranferToForwarder = true;
			var container2 = data.Containers.ToList()[1];
			container2.IsTranferToForwarder = false;

			AssertNoMessageError("Operational Port required", ((Unloco)data.OperationalPort).CodeInfo, "Operational Port is required.");
			AssertNoMessageError("BOL Number required", data.BillOfLadingInfo, "BOL Number is required.");

			AssertNoMessageError("Forwarder ID required", data.ForwarderIdEOR.ValueInfo, forwarderIDErrorMessage);
			AssertNoMessageError("Forwarder ID required", data.ForwarderIdDUN.ValueInfo, forwarderIDErrorMessage);
			AssertNoMessageError("Transport Company IDrequired", data.TransportCompanyIdEOR.ValueInfo, transportCompanyIDErrorMessage);
			AssertNoMessageError("Transport Company IDrequired", data.TransportCompanyIdDUN.ValueInfo, transportCompanyIDErrorMessage);

			AssertNoMessageError("Container Nubmer required", container1.NumberInfo, "Container Number is required.");
			AssertNoMessageError("Release Identification required", container1.ReleaseIdentificationInfo, "Release Identification is required.");

			data.OperationalPort.Code = string.Empty;
			data.BillOfLading = string.Empty;

			data.ForwarderIdEOR.Value = string.Empty;
			data.ForwarderIdDUN.Value = string.Empty;
			data.TransportCompanyIdEOR.Value = string.Empty;
			data.TransportCompanyIdDUN.Value = string.Empty;

			container1.Number = string.Empty;
			container1.ReleaseIdentification = string.Empty;

			data.ValidateAllIncludingChildren();

			AssertHasMessageError("Operational Port required", ((Unloco)data.OperationalPort).CodeInfo, "Operational Port is required.");
			AssertHasMessageError("BOL Number required", data.BillOfLadingInfo, "BOL Number is required.");

			AssertHasMessageError("Forwarder ID required", data.ForwarderIdEOR.ValueInfo, forwarderIDErrorMessage);
			AssertHasMessageError("Forwarder ID required", data.ForwarderIdDUN.ValueInfo, forwarderIDErrorMessage);
			AssertHasMessageError("Transport Company IDrequired", data.TransportCompanyIdEOR.ValueInfo, transportCompanyIDErrorMessage);
			AssertHasMessageError("Transport Company IDrequired", data.TransportCompanyIdDUN.ValueInfo, transportCompanyIDErrorMessage);

			AssertHasMessageError("Container Nubmer required", container1.NumberInfo, "Container Number is required.");
			AssertHasMessageError("Release Identification required", container1.ReleaseIdentificationInfo, "Release Identification is required.");

			container1.IsTranferToForwarder = false;
			container2.IsTranferToForwarder = false;

			AssertNoMessageError("Forwarder ID required", data.ForwarderIdEOR.ValueInfo, forwarderIDErrorMessage);
			AssertNoMessageError("Forwarder ID required", data.ForwarderIdDUN.ValueInfo, forwarderIDErrorMessage);
			AssertHasMessageError("Transport Company IDrequired", data.TransportCompanyIdEOR.ValueInfo, transportCompanyIDErrorMessage);
			AssertHasMessageError("Transport Company IDrequired", data.TransportCompanyIdDUN.ValueInfo, transportCompanyIDErrorMessage);

			container1.IsTranferToForwarder = true;
			container2.IsTranferToForwarder = true;

			AssertHasMessageError("Forwarder ID required", data.ForwarderIdEOR.ValueInfo, forwarderIDErrorMessage);
			AssertHasMessageError("Forwarder ID required", data.ForwarderIdDUN.ValueInfo, forwarderIDErrorMessage);
			AssertNoMessageError("Transport Company IDrequired", data.TransportCompanyIdEOR.ValueInfo, transportCompanyIDErrorMessage);
			AssertNoMessageError("Transport Company IDrequired", data.TransportCompanyIdDUN.ValueInfo, transportCompanyIDErrorMessage);
		}

		#endregion

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ShippersConsol;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = TMiningConstants.DocumentNames.TMiningSecureContainerReleaseTransfer,
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var data = new SecureContainerReleaseBuilder(consol, parameters).Build();
			AssertEquals(Core.Constants.ContainerModes.FCL, data.ContainerMode.Code);
			AssertEquals(Core.Constants.ContainerModeDescriptions.FCL, data.ContainerMode.Description);
		}

		#endregion

		#region Implement

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "BEANR";
			consol.JK_MasterBillNum = "BOL123";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";
			container1.JC_ContainerImportDORelease = "REL1111";
			container1.JC_IsNonOperativeReefer = true;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";
			container2.JC_ContainerImportDORelease = "REL2222";
			container2.JC_IsNonOperativeReefer = false;

			CreateConsolAddresses(consol);

			return consol;
		}

		void CreateConsolAddresses(ForwardingConsol consol)
		{
			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "MAERSK";
			receivingForwarder.OH_RL_NKClosestPort = "DKAAL";
			receivingForwarder.MainAddress.Address1 = "Unit 13";
			receivingForwarder.MainAddress.Address2 = "4 Lost Lane";
			receivingForwarder.MainAddress.City = "Aalborg";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "DK";

			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DK_EORI123", Core.Constants.CountryCodes.Denmark);
			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DK_DUN123", Core.Constants.CountryCodes.Denmark);

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var transportCompany = Factory.New<OrgHeader>();
			transportCompany.OH_FullName = "Transporter XX";
			transportCompany.OH_RL_NKClosestPort = "FRPAR";
			transportCompany.MainAddress.Address1 = "Unit 13";
			transportCompany.MainAddress.Address2 = "4 Lost Lane";
			transportCompany.MainAddress.City = "Paris";
			transportCompany.MainAddress.Postcode = "2000";
			transportCompany.MainAddress.OA_RN_NKCountryCode = "FR";

			transportCompany.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FR_EORI123", Core.Constants.CountryCodes.France);
			transportCompany.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "FR_DUN123", Core.Constants.CountryCodes.France);

			consol.JK_OA_ArrivalUnpackCFSTransportAddress = transportCompany.MainAddress.PK;
		}

		#endregion
	}
}

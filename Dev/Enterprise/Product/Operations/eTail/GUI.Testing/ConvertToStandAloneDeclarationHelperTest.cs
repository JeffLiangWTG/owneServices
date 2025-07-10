using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing;

public class ConvertToStandAloneDeclarationHelperTest : TestCaseWithFactory
{
	public void TestConvertMultipleConsignmentsToStandAloneDeclarations()
	{
		var forwardingConsolDataContextManagerMock = new Mock<IDataContextManager>();
		ObjectFactory.Substitute("ForwardingConsolDataContextManager", forwardingConsolDataContextManagerMock.Object);

		var shipment = CreateShipment();
		var shipperOrg = CreateOrgHeader("TESSHIP", "Shipper Company", CountryCodes.NewZealand, "TESSHIPADD", "1234 Shipper Street", "Shipper City");
		var consigneeOrg = CreateOrgHeader("TESCONS", "Consignee Testing Company", CountryCodes.Australia, "TESCONSADD", "6666 Consignee Avenue", "Consigneeland");

		var consignment1 = CreateImportConsignment(shipment.PK, shipperOrg.Addresses[0].PK, consigneeOrg.Addresses[0].PK);
		var consignment2 = CreateImportConsignment(shipment.PK, shipperOrg.Addresses[0].PK, consigneeOrg.Addresses[0].PK);

		Factory.Save();

		var helper = new ConvertToStandAloneDeclarationHelper();
		var consignments = new List<HVLVConsignment> { consignment1, consignment2 };

		var trackLogs = new List<(string Caption, string Message, int Progress)>();

		Action<string, string, int> progressUpdateCallback = (caption, message, progress) =>
		{
			trackLogs.Add((caption, message, progress));
		};
		helper.ConvertToStandAloneDeclarations(consignments, progressUpdateCallback, new CancellationTokenSource());
		Factory.Save();

		CombineAssertions("Expected both consignments to have a job declaration reference:", () =>
		{
			AssertNotNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
			AssertNotNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
		});

		CombineAssertions(() =>
		{
			forwardingConsolDataContextManagerMock.Verify(x => x.Init(shipment.Consols[1]), Times.Once());
			AssertNull("DataObjectCacheFactoryService", Factory.ServiceContainer.GetService<DataObjectCacheFactoryService>());
		});
	}

	public void TestConvertMultipleConsignmentsToStandAloneDeclarations_WithMerge()
	{
		var forwardingConsolDataContextManagerMock = new Mock<IDataContextManager>();
		ObjectFactory.Substitute("ForwardingConsolDataContextManager", forwardingConsolDataContextManagerMock.Object);

		var shipment = CreateShipment();
		var shipperOrg = CreateOrgHeader("TESSHIP", "Shipper Company", CountryCodes.NewZealand, "TESSHIPADD", "1234 Shipper Street", "Shipper City");
		var consigneeOrgA = CreateOrgHeader("TESCONA", "Consignee Testing Company A", CountryCodes.Australia, "ADDRESSA", "A Consignee Avenue", "Consignee City A");
		var consigneeOrgB = CreateOrgHeader("TESCONB", "Consignee Testing Company B", CountryCodes.Australia, "ADDRESSB", "B Consignee Avenue", "Consignee City B");

		var consignment1 = CreateImportConsignment(shipment.PK, shipperOrg.Addresses[0].PK, consigneeOrgA.Addresses[0].PK);
		var consignment2 = CreateImportConsignment(shipment.PK, shipperOrg.Addresses[0].PK, consigneeOrgA.Addresses[0].PK);
		var consignment3 = CreateImportConsignment(shipment.PK, shipperOrg.Addresses[0].PK, consigneeOrgB.Addresses[0].PK);

		Factory.Save();

		var helper = new ConvertToStandAloneDeclarationHelper();
		var consignments = new List<HVLVConsignment> { consignment1, consignment2, consignment3 };
		helper.MergeAndConvertToStandAloneDeclarations(consignments);
		Factory.Save();

		var jobDeclarations = Factory.Load<BaseJobDeclaration>(new ZQuery());

		CombineAssertions(() =>
		{
			AssertNotNull("consignment1:", consignment1.StandAloneDeclarationForCurrentCompany);
			AssertNotNull("consignment2:", consignment2.StandAloneDeclarationForCurrentCompany);
			AssertNotNull("consignment3:", consignment3.StandAloneDeclarationForCurrentCompany);

			AssertEquals(2, jobDeclarations.Length);
			AssertEquals("Expected consignment1 and consignment2 to have the same declaration reference because they have the same consignee:", consignment1.StandAloneDeclarationForCurrentCompany, consignment2.StandAloneDeclarationForCurrentCompany);
		});

		CombineAssertions(() =>
		{
			forwardingConsolDataContextManagerMock.Verify(x => x.Init(shipment.Consols[1]), Times.Once());
			AssertNull("DataObjectCacheFactoryService", Factory.ServiceContainer.GetService<DataObjectCacheFactoryService>());
		});
	}

	HVLVForwardingShipment CreateShipment()
	{
		var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
		shipment.JS_RL_NKOrigin = "NZAKL";
		shipment.JS_RL_NKDestination = "AUSYD";

		var departureConsol = shipment.Consols.AddNew();
		departureConsol.JK_RL_NKLoadPort = "NZAKL";
		departureConsol.JK_RL_NKDischargePort = "AUSYD";

		var arrivalConsol = shipment.Consols.AddNew();
		arrivalConsol.JK_RL_NKLoadPort = "AUBNE";
		arrivalConsol.JK_RL_NKDischargePort = "AUSYD";

		return shipment;
	}

	OrgHeader CreateOrgHeader(string code, string fullName, string countryCode, string addressCode, string address1, string city)
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_Code = code;
		org.OH_FullName = fullName;
		var address = org.Addresses.AddNew();
		address.OA_RN_NKCountryCode = countryCode;
		address.OA_Code = addressCode;
		address.OA_Address1 = address1;
		address.OA_City = city;
		return org;
	}

	HVLVConsignment CreateImportConsignment(ZGuid shipmentPK, ZGuid shipperAddressPK, ZGuid consigneeAddressPK)
	{
		var consignment = Factory.New<HVLVConsignment>();
		consignment.Items.AddNew();
		consignment.HVC_JS_ManifestedOnShipment = shipmentPK;
		consignment.HVC_OA_ShipperAddress = shipperAddressPK;
		consignment.HVC_OA_ConsigneeAddress = consigneeAddressPK;
		consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;
		return consignment;
	}
}

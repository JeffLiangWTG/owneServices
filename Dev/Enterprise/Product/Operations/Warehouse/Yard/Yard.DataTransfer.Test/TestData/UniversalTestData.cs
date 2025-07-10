using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	public class UniversalTestData : ICYDUniversalTestData
	{
		readonly UniversalObjectFactory Factory;
		readonly TestErrorLogger Logger;
		UniversalTestOrgs testOrgs;
		WhsWarehouse yard;

		public UniversalTestData(UniversalObjectFactory factory, TestErrorLogger loggerWithTopLevelDataContext)
		{
			this.Factory = Argument.NotNull(factory, "factory");
			Logger = Argument.NotNull(loggerWithTopLevelDataContext, "factory");
		}

		public UniversalTestOrgs TestOrgs
		{
			get { return testOrgs ?? (testOrgs = new UniversalTestOrgs(Factory, Logger)); }
		}

		public WhsWarehouse Yard
		{
			get { return yard ??= UniversalTestHelper.CreateWarehouse(TestOrgs.Org1.MainAddress, Factory); }
		}

		public void SetupDataForTesting()
		{
			yard = UniversalTestHelper.CreateWarehouse(TestOrgs.Org1.MainAddress, Factory);
		}

		public void SetupDataForDelivery(string acceptanceNumber, ZDate fromDate, ZDate toDate, string containerTypeCode, string[] containerNumbers)
		{
			if (yard == null)
			{
				SetupDataForTesting();
			}
			var clientAddress = UniversalTestHelper.CreateOrganization(Factory, "Client", "ClientAddress");
			var receiveAdvice = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, acceptanceNumber, fromDate, toDate);
			UniversalTestHelper.CreateJobDocAddress(Factory, clientAddress, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, receiveAdvice);
			foreach (var containerNumber in containerNumbers)
			{
				var receiveAdviceLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, receiveAdvice, containerTypeCode);
				UniversalTestHelper.CreateYardUnitState(Factory, yard, receiveAdviceLine, containerNumber);
			}
		}

		public void SetupDataForPickup(string acceptanceNumber, string releaseNumber, ZDate fromDate, ZDate toDate, string containerTypeCode, string[] containerNumbers, int orgId = 1)
		{
			if (yard == null)
			{
				SetupDataForTesting();
			}
			var clientAddress = UniversalTestHelper.CreateOrganization(Factory, "Client" + orgId, "ClientAddress");
			var receiveAdvice = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, acceptanceNumber, fromDate, toDate);
			UniversalTestHelper.CreateJobDocAddress(Factory, clientAddress, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, receiveAdvice);
			var releaseAdvice = UniversalTestHelper.CreateReleaseAdvice(Factory, yard, releaseNumber, fromDate, toDate);
			UniversalTestHelper.CreateJobDocAddress(Factory, clientAddress, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, releaseAdvice);
			foreach (var containerNumber in containerNumbers)
			{
				var receiveAdviceLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, receiveAdvice, containerTypeCode);
				var isPreAdvice = !string.IsNullOrEmpty(containerNumber);
				var releaseAdviceLine = UniversalTestHelper.CreateReleaseAdviceLine(Factory, releaseAdvice, containerTypeCode, isPreAdvice: isPreAdvice);
				if (isPreAdvice)
				{
					UniversalTestHelper.CreateYardUnitState(Factory, yard, receiveAdviceLine, containerNumber, releaseAdviceLine);
				}
			}
		}

		public void SetupDataForTransportationUnitDataObjectWriterForDelivery(string acceptanceNumber, ZDate fromDate, ZDate toDate, string clientName, string clientAddress, string vehicleNumber, string transportReference, string containerTypeCode, string[] containerNumbers)
		{
			if (yard == null)
			{
				SetupDataForTesting();
			}
			var receiveAdvice = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, acceptanceNumber, fromDate, toDate);
			var transportationUnit = UniversalTestHelper.CreateTransportationUnit(Factory, yard, vehicleNumber);
			var client = UniversalTestHelper.CreateOrganization(Factory, clientName, clientAddress);
			UniversalTestHelper.CreateJobDocAddress(Factory, client, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportationUnit);

			var deliveries = new List<CYDDelivery>();
			foreach (var containerNumber in containerNumbers)
			{
				var receiveAdviceLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, receiveAdvice, containerTypeCode);
				var yardUnit = UniversalTestHelper.CreateYardUnitState(Factory, yard, receiveAdviceLine, containerNumber, null);
				var delivery = UniversalTestHelper.CreateDelivery(Factory, receiveAdviceLine, transportationUnit, yardUnit, containerTypeCode, 1, transportReference);
				deliveries.Add(delivery);
			}
			UniversalTestHelper.CreateDeliveryHeader(Factory, yard, deliveries.ToArray());
		}

		public void SetupDataForTransportationUnitDataObjectWriterForDelivery_MultiplePRA(string acceptanceNumber1, string acceptanceNumber2, ZDate fromDate, ZDate toDate, string clientName, string clientAddress, string vehicleNumber, string transportReference, string containerTypeCode, string[] containerNumbers)
		{
			if (yard == null)
			{
				SetupDataForTesting();
			}
			var receiveAdvice1 = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, acceptanceNumber1, fromDate, toDate);
			var receiveAdvice2 = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, acceptanceNumber2, fromDate, toDate);
			var transportationUnit = UniversalTestHelper.CreateTransportationUnit(Factory, yard, vehicleNumber);
			var client = UniversalTestHelper.CreateOrganization(Factory, clientName, clientAddress);
			UniversalTestHelper.CreateJobDocAddress(Factory, client, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportationUnit);

			var deliveries = new List<CYDDelivery>();
			for (var i = 0; i < containerNumbers.Length; i++)
			{
				var receiveAdviceLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, i % 2 == 0 ? receiveAdvice1 : receiveAdvice2, containerTypeCode);
				var yardUnit = UniversalTestHelper.CreateYardUnitState(Factory, yard, receiveAdviceLine, containerNumbers[i], null);
				var delivery = UniversalTestHelper.CreateDelivery(Factory, receiveAdviceLine, transportationUnit, yardUnit, containerTypeCode, 1, transportReference);
				deliveries.Add(delivery);
			}
			UniversalTestHelper.CreateDeliveryHeader(Factory, yard, deliveries.ToArray());
		}

		public void SetupDataForTransportationUnitDataObjectWriterForPickup(string releaseNumber, ZDate fromDate, ZDate toDate, string clientName, string clientAddress, string vehicleNumber, string transportReference, string containerTypeCode, string[] containerNumbers)
		{
			if (yard == null)
			{
				SetupDataForTesting();
			}
			var receiveNumber = "REC";
			var releaseAdvice = UniversalTestHelper.CreateReleaseAdvice(Factory, yard, releaseNumber, fromDate, toDate);
			var receiveAdvice = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, receiveNumber, fromDate, toDate);
			var transportationUnit = UniversalTestHelper.CreateTransportationUnit(Factory, yard, vehicleNumber);
			var client = UniversalTestHelper.CreateOrganization(Factory, clientName, clientAddress);
			UniversalTestHelper.CreateJobDocAddress(Factory, client, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportationUnit);

			var pickups = new List<CYDPickup>();
			foreach (var containerNumber in containerNumbers)
			{
				var releaseAdviceLine = UniversalTestHelper.CreateReleaseAdviceLine(Factory, releaseAdvice, containerTypeCode);
				var receiveLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, receiveAdvice, containerTypeCode);
				var yardUnit = UniversalTestHelper.CreateYardUnitState(Factory, yard, receiveLine, containerNumber, releaseAdviceLine);
				var pickup = UniversalTestHelper.CreatePickup(Factory, releaseAdviceLine, transportationUnit, yardUnit, containerTypeCode, 1, transportReference);
				pickups.Add(pickup);
			}
			UniversalTestHelper.CreatePickupHeader(Factory, yard, pickups.ToArray());
		}

		public void SetupDataForTransportationUnitDataObjectWriterForPickup_MultipleRO(string releaseNumber1, string releaseNumber2, ZDate fromDate, ZDate toDate, string clientName, string clientAddress, string vehicleNumber, string transportReference, string containerTypeCode, string[] containerNumbers)
		{
			if (yard == null)
			{
				SetupDataForTesting();
			}
			var receiveNumber1 = "REC1";
			var receiveNumber2 = "REC2";
			var releaseAdvice1 = UniversalTestHelper.CreateReleaseAdvice(Factory, yard, releaseNumber1, fromDate, toDate);
			var releaseAdvice2 = UniversalTestHelper.CreateReleaseAdvice(Factory, yard, releaseNumber2, fromDate, toDate);
			var receiveAdvice1 = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, receiveNumber1, fromDate, toDate);
			var receiveAdvice2 = UniversalTestHelper.CreateReceiveAdvice(Factory, yard, receiveNumber2, fromDate, toDate);
			var transportationUnit = UniversalTestHelper.CreateTransportationUnit(Factory, yard, vehicleNumber);
			var client = UniversalTestHelper.CreateOrganization(Factory, clientName, clientAddress);
			UniversalTestHelper.CreateJobDocAddress(Factory, client, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportationUnit);

			var pickups = new List<CYDPickup>();
			for (var i = 0; i < containerNumbers.Length; i++)
			{
				var releaseAdviceLine = UniversalTestHelper.CreateReleaseAdviceLine(Factory, i % 2 == 0 ? releaseAdvice1 : releaseAdvice2, containerTypeCode);
				var receiveLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, i % 2 == 0 ? receiveAdvice1 : receiveAdvice2, containerTypeCode);
				var yardUnit = UniversalTestHelper.CreateYardUnitState(Factory, yard, receiveLine, containerNumbers[i], releaseAdviceLine);
				var pickup = UniversalTestHelper.CreatePickup(Factory, releaseAdviceLine, transportationUnit, yardUnit, containerTypeCode, 1, transportReference);
				pickups.Add(pickup);
			}
			UniversalTestHelper.CreatePickupHeader(Factory, yard, pickups.ToArray());
		}

		public void SetupReceiveAdviceAndYardUnitStateForReleaseOrderDataObjectReader(RefContainer containerType, CYDReleaseAdviceLine releaseAdviceLine, string organizationFullName = "EDI CUSTOMS BROKERS",
			string type = "CNT", bool isUnloaded = true, bool isLoaded = false, bool isSameWarehouse = true)
		{
			var receiveAdvice = UniversalTestHelper.CreateReceiveAdvice(Factory, Yard, "ACC1", ZDate.Today, ZDate.Today.AddMonths(1));

			var client = UniversalTestHelper.CreateOrganization(Factory, "ACE", "Po Box 201", organizationFullName);
			UniversalTestHelper.CreateJobDocAddress(Factory, client, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress, receiveAdvice);

			var receiveAdviceLine = UniversalTestHelper.CreateReceiveAdviceLine(Factory, receiveAdvice, containerType.RC_Code);
			receiveAdviceLine.UnitLineItem.YLI_Type = type;
			receiveAdviceLine.UnitLineItem.YLI_SealNumber = "seal number";
			receiveAdviceLine.UnitLineItem.YLI_IsEmpty = true;

			var warehouse = isSameWarehouse ? Yard : Factory.NewWithValidTestData<WhsWarehouse>();
			var yardUnitState = UniversalTestHelper.CreateYardUnitState(Factory, warehouse, receiveAdviceLine, "CON1");

			var yardUnitLineItem = UniversalTestHelper.CreateUnitLineItem(Factory, containerType, type, 1, true, "seal number");
			yardUnitState.YUS_YLI_UnitLineItem = yardUnitLineItem.PK;

			var transportationUnit = UniversalTestHelper.CreateTransportationUnit(Factory, yard, "V051502");

			if (isUnloaded)
			{
				UniversalTestHelper.CreateDelivery(Factory, receiveAdviceLine, transportationUnit, yardUnitState, containerType.RC_Code, 1, "TREF008");
				yardUnitState.YUS_UnloadTime = new DateTime(2024, 8, 19);
				yardUnitState.YUS_GS_NKUnloadUser = "E";
			}
			if (isLoaded)
			{
				yardUnitState.YUS_LoadTime = new DateTime(2024, 8, 19);
				yardUnitState.YUS_GS_NKLoadUser = "E";
				UniversalTestHelper.CreatePickup(Factory, releaseAdviceLine, transportationUnit, yardUnitState, containerType.RC_Code, 1, "TREF008");
			}
			if (isUnloaded && !isLoaded)
			{
				var location = Factory.NewWithValidTestData<WhsLocation>();
				location.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
				yardUnitState.YUS_WL_CurrentYardLocation = location.PK;
			}
			Factory.SaveForTesting();
		}
	}
}

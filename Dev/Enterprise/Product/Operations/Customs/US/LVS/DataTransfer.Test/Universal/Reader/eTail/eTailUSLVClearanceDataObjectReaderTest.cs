using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	public class eTailUSLVClearanceDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestExistingConsignmentMatchingIgnoresCharacterCase()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "Wbbb000011212";
			consignment.ULB_OwnerReferenceNumber = "REF123";

			var shipmentDataObject = SetupETailTestingShipmentWithMinimumRequirements();
			shipmentDataObject.SubShipmentCollection[0].SubShipmentCollection[0].OwnerRef = "REF456";

			var newClearance = PopulateFromUXML(shipmentDataObject);
			CombineAssertions(() =>
			{
				AssertEquals("Should not create any new consignments", 1, newClearance.CusUSLVConsignments.Count);
				var newConsignment = newClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single();
				AssertEquals("Should find matching consignment despite different character casing", consignment.PK, newConsignment.PK);
				AssertEquals("ULB_OwnerReferenceNumber should be updated to \"REF456\"", "REF456", newConsignment.ULB_OwnerReferenceNumber);
			});
		}

		public void TestConsignmentMatching()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "WBBB000011212";

			var shipmentDataObject = SetupETailTestingShipmentWithMinimumRequirements();

			var newConsignment = PopulateFromUXML(shipmentDataObject).CusUSLVConsignments.Single();
			AssertEquals("Should find matching consignment", consignment.PK, newConsignment.PK);

			consignment.ULB_HouseBill = "WRONGNUMBER";
			var consignments = PopulateFromUXML(shipmentDataObject).CusUSLVConsignments;
			AssertEquals("Should create another consignment, so 2 total", 2, consignments.Count);
		}

		public void TestClearanceMatching()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";

			var shipmentDataObject = SetupETailTestingShipmentWithMinimumRequirements();

			var newClearance = PopulateFromUXML(shipmentDataObject);
			AssertEquals("Should find match", clearance.PK, newClearance.PK);

			clearance.ULH_MatchingKey = "S00007777";
			newClearance = PopulateFromUXML(shipmentDataObject);
			AssertNotEquals("Should be no match and create new", clearance.PK, newClearance.PK);
		}

		public void TestGivenConsignmentCollection_WhenPopulatingWithMatchingXUSSubshipments_ConsignmentsAreUpdated()
		{
			var businessObjectFactory = new BusinessObjectFactory();
			var clearance = businessObjectFactory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";

			var consignment1 = businessObjectFactory.New<CusUSLVConsignment>();
			var consignment2 = businessObjectFactory.New<CusUSLVConsignment>();
			var consignment3 = businessObjectFactory.New<CusUSLVConsignment>();

			consignment1.ULB_HouseBill = "CONSIGNMENT1";
			consignment1.ULB_PackType = "CNT";
			consignment1.ULB_NumberOfPacks = 3;

			consignment2.ULB_HouseBill = "CONSIGNMENT2";
			consignment2.ULB_PackType = "CNT";
			consignment2.ULB_NumberOfPacks = 3;

			consignment3.ULB_HouseBill = "CONSIGNMENT3";
			consignment3.ULB_PackType = "CNT";
			consignment3.ULB_NumberOfPacks = 3;

			clearance.CusUSLVConsignments.Add(consignment1);
			clearance.CusUSLVConsignments.Add(consignment2);
			clearance.CusUSLVConsignments.Add(consignment3);

			businessObjectFactory.Save();

			var xusMasterShipment = SetupETailTestingShipmentWithMinimumRequirements();
			var xusClearanceAsShipment = xusMasterShipment.SubShipmentCollection.SingleOrDefault();
			xusClearanceAsShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>()
			{
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT1",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT2",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT3",
					WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() {   Code = "PKG" }
				}
			});

			var xusClearance = PopulateFromUXML(xusMasterShipment);

			var xusConsignment1 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT1");
			var xusConsignment2 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT2");
			var xusConsignment3 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT3");

			AssertEquals("There should be three consignments. Three consignments that was in the clearance initially, and then the three that updates them from the XUS.", 3, xusClearance.CusUSLVConsignments.Count);
			CombineAssertions("The consigments from the UXML was not added.", () =>
			{
				AssertNotNull("Consignment1: ", xusConsignment1);
				AssertNotNull("Consignment2: ", xusConsignment2);
				AssertNotNull("Consignment3: ", xusConsignment3);
			});
			CombineAssertions("Properties of the clearance were not updated.", () =>
			{
				AssertEquals("Consignment1: ", "PKG", xusConsignment1.ULB_PackType);
				AssertEquals("Consignment1: ", (ZShort)5, xusConsignment1.ULB_NumberOfPacks);
				AssertEquals("Consignment2: ", "PKG", xusConsignment2.ULB_PackType);
				AssertEquals("Consignment2: ", (ZShort)5, xusConsignment2.ULB_NumberOfPacks);
				AssertEquals("Consignment3: ", "PKG", xusConsignment3.ULB_PackType);
				AssertEquals("Consignment3: ", (ZShort)5, xusConsignment3.ULB_NumberOfPacks);
			});
		}
		public void TestGivenConsignmentCollection_WhenPopulatingWithXUSShipments_ConsignmentsAreAdded()
		{
			var businessObjectFactory = new BusinessObjectFactory();
			var clearance = businessObjectFactory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";

			var consignment1 = businessObjectFactory.New<CusUSLVConsignment>();
			consignment1.ULB_HouseBill = "CONSIGNMENT1";
			consignment1.ULB_PackType = "CNT";
			consignment1.ULB_NumberOfPacks = 3;

			clearance.CusUSLVConsignments.Add(consignment1);

			businessObjectFactory.Save();

			var xusMasterShipment = SetupETailTestingShipmentWithMinimumRequirements();
			var xusClearanceAsShipment = xusMasterShipment.SubShipmentCollection.SingleOrDefault();
			xusClearanceAsShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>()
			{
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT2",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT3",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT4",
					WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() {   Code = "PKG" }
				}
			});

			var xusClearance = PopulateFromUXML(xusMasterShipment);

			var xusConsignment1 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT1");
			var xusConsignment2 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT2");
			var xusConsignment3 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT3");
			var xusConsignment4 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT4");

			AssertEquals("There should be four consignments. One in the clearance consignment collection initially and then three new ones added from the UXML", 4, xusClearance.CusUSLVConsignments.Count);
			CombineAssertions("A consignment was not added to xusClearance.", () =>
			{
				AssertNotNull("Consignment1: ", xusConsignment1);
				AssertNotNull("Consignment2: ", xusConsignment2);
				AssertNotNull("Consignment3: ", xusConsignment3);
				AssertNotNull("Consignment4: ", xusConsignment4);
			});
			CombineAssertions("A consignment does not have correct value for its properties.", () =>
			{
				AssertEquals("Consignment1: ", "CNT", xusConsignment1.ULB_PackType);
				AssertEquals("Consignment1: ", (ZShort)3, xusConsignment1.ULB_NumberOfPacks);
				AssertEquals("Consignment2: ", "PKG", xusConsignment2.ULB_PackType);
				AssertEquals("Consignment2: ", (ZShort)5, xusConsignment2.ULB_NumberOfPacks);
				AssertEquals("Consignment3: ", "PKG", xusConsignment3.ULB_PackType);
				AssertEquals("Consignment3: ", (ZShort)5, xusConsignment3.ULB_NumberOfPacks);
				AssertEquals("Consignment4: ", "PKG", xusConsignment4.ULB_PackType);
				AssertEquals("Consignment4: ", (ZShort)5, xusConsignment4.ULB_NumberOfPacks);
			});
		}

		public void TestGivenConsignmentCollection_WhenPopulatingWithMatchingAndNonMatchingXUSShipments_ConsignmentsAreAddedOrUpdated()
		{
			var businessObjectFactory = new BusinessObjectFactory();
			var clearance = businessObjectFactory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";

			var consignment1 = businessObjectFactory.New<CusUSLVConsignment>();
			consignment1.ULB_HouseBill = "CONSIGNMENT1";
			consignment1.ULB_PackType = "CNT";
			consignment1.ULB_NumberOfPacks = 3;

			var consignment2 = businessObjectFactory.New<CusUSLVConsignment>();
			consignment2.ULB_HouseBill = "CONSIGNMENT2";
			consignment2.ULB_PackType = "CNT";
			consignment2.ULB_NumberOfPacks = 3;

			clearance.CusUSLVConsignments.Add(consignment1);
			clearance.CusUSLVConsignments.Add(consignment2);

			businessObjectFactory.Save();

			var xusMasterShipment = SetupETailTestingShipmentWithMinimumRequirements();
			var xusClearanceAsShipment = xusMasterShipment.SubShipmentCollection.SingleOrDefault();
			xusClearanceAsShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>()
			{
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT1",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT2",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT3",
					WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() {   Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT4",
					WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() {   Code = "PKG" }
				}
			});

			var xusClearance = PopulateFromUXML(xusMasterShipment);

			var xusConsignment1 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT1");
			var xusConsignment2 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT2");
			var xusConsignment3 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT3");
			var xusConsignment4 = xusClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Single(c => c.ULB_HouseBill == "CONSIGNMENT4");

			AssertEquals("xusClearance should have four consigments. Two initially, and two added from the UXML.", 4, xusClearance.CusUSLVConsignments.Count);

			CombineAssertions("A consignment was not added to xusClearance.", () =>
			{
				AssertNotNull("Consignment1: ", xusConsignment1);
				AssertNotNull("Consignment2: ", xusConsignment2);
				AssertNotNull("Consignment3: ", xusConsignment3);
				AssertNotNull("Consignment4: ", xusConsignment4);
			});

			CombineAssertions("A consignment does not have correct value for its properties.", () =>
			{
				AssertEquals("Consignment1: ", "PKG", xusConsignment1.ULB_PackType);
				AssertEquals("Consignment1: ", (ZShort)5, xusConsignment1.ULB_NumberOfPacks);
				AssertEquals("Consignment2: ", "PKG", xusConsignment2.ULB_PackType);
				AssertEquals("Consignment2: ", (ZShort)5, xusConsignment2.ULB_NumberOfPacks);
				AssertEquals("Consignment3: ", "PKG", xusConsignment3.ULB_PackType);
				AssertEquals("Consignment3: ", (ZShort)5, xusConsignment3.ULB_NumberOfPacks);
				AssertEquals("Consignment4: ", "PKG", xusConsignment4.ULB_PackType);
				AssertEquals("Consignment4: ", (ZShort)5, xusConsignment4.ULB_NumberOfPacks);
			});
		}

		public void TestReadIntoBusinessObject_WhenSubShipmentWithoutHouseBill()
		{
			var businessObjectFactory = new BusinessObjectFactory();
			var clearance = businessObjectFactory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";

			var consignment1 = businessObjectFactory.New<CusUSLVConsignment>();
			consignment1.ULB_HouseBill = "CONSIGNMENT1";
			consignment1.ULB_PackType = "CNT";
			consignment1.ULB_NumberOfPacks = 3;

			clearance.CusUSLVConsignments.Add(consignment1);

			businessObjectFactory.Save();

			var xusMasterShipment = SetupETailTestingShipmentWithMinimumRequirements();
			var xusClearanceAsShipment = xusMasterShipment.SubShipmentCollection.SingleOrDefault();
			xusClearanceAsShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>()
			{
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				}
			});

			var uslvClearanceReader = new eTailUSLVClearanceDataObjectReaderTestOnly(xusMasterShipment, xusMasterShipment.SubShipmentCollection[0], new DummyLogger(), Factory);
			uslvClearanceReader.ReadIntoBusinessObject();

			AssertEquals(ZBool.True, uslvClearanceReader.GetHasSubShipmentWithoutHouseBill());
		}

		public void TestReadIntoBusinessObject_WhenHouseBillNumberMatchesToMultipleConsignments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";

			var consignment1 = Factory.New<CusUSLVConsignment>();
			consignment1.ULB_HouseBill = "CONSIGNMENT1";
			consignment1.ULB_PackType = "CNT";
			consignment1.ULB_NumberOfPacks = 3;

			var consignment2 = Factory.New<CusUSLVConsignment>();
			consignment2.ULB_HouseBill = "CONSIGNMENT1";
			consignment2.ULB_PackType = "CNT";
			consignment2.ULB_NumberOfPacks = 3;

			clearance.CusUSLVConsignments.Add(consignment1);
			clearance.CusUSLVConsignments.Add(consignment2);

			Factory.SaveForTesting();

			var xusMasterShipment = SetupETailTestingShipmentWithMinimumRequirements();
			var xusClearanceAsShipment = xusMasterShipment.SubShipmentCollection.SingleOrDefault();

			xusClearanceAsShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>()
			{
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT1",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
			});

			var uslvClearanceReader = new eTailUSLVClearanceDataObjectReaderTestOnly(xusMasterShipment, xusMasterShipment.SubShipmentCollection[0], new DummyLogger(), Factory);
			uslvClearanceReader.ReadIntoBusinessObject();

			var consignmentDict = uslvClearanceReader.GetConsignmentDict();
			AssertNotNull("Precondition:", consignmentDict);

			consignmentDict.TryGetValue("CONSIGNMENT1", out var consignments);
			AssertEquals(2, consignments.Length);
		}

		public void TestReadIntoBusinessObject_WhenSubshipmentContainsDuplicateBillNumbers()
		{
			var businessObjectFactory = new BusinessObjectFactory();
			var clearance = businessObjectFactory.New<CusUSLVClearance>();
			clearance.ULH_MatchingKey = "S00008888";
			clearance.ULH_UseCode = "HVL";

			var consignment1 = businessObjectFactory.New<CusUSLVConsignment>();
			consignment1.ULB_HouseBill = "CONSIGNMENT1";
			consignment1.ULB_PackType = "CNT";
			consignment1.ULB_NumberOfPacks = 3;

			var consignment2 = businessObjectFactory.New<CusUSLVConsignment>();
			consignment2.ULB_HouseBill = "CONSIGNMENT2";
			consignment2.ULB_PackType = "CNT";
			consignment2.ULB_NumberOfPacks = 3;

			clearance.CusUSLVConsignments.Add(consignment1);
			clearance.CusUSLVConsignments.Add(consignment2);

			businessObjectFactory.Save();

			var xusMasterShipment = SetupETailTestingShipmentWithMinimumRequirements();
			var xusClearanceAsShipment = xusMasterShipment.SubShipmentCollection.SingleOrDefault();

			xusClearanceAsShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>()
			{
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT1",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT1",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() { Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT2",
					WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() {   Code = "PKG" }
				},
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "CONSIGNMENT2",
					WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House },
					TotalNoOfPacks = 5,
					TotalNoOfPacksPackageType = new PackageType() {   Code = "PKG" }
				}
			});

			var uslvClearanceReader = new eTailUSLVClearanceDataObjectReaderTestOnly(xusMasterShipment, xusMasterShipment.SubShipmentCollection[0], new DummyLogger(), Factory);
			uslvClearanceReader.ReadIntoBusinessObject();
			var duplicateBillNumberList = uslvClearanceReader.GetDuplicateBillNumberList();
			AssertNotNull("Precondition: ", duplicateBillNumberList);
			AssertEquals(2, duplicateBillNumberList.Count);
		}

		CusUSLVClearance PopulateFromUXML(UniversalShipment shipmentDataObject)
		{
			return new eTailUSLVClearanceDataObjectReader(
			shipmentDataObject,
			shipmentDataObject.SubShipmentCollection[0],
			new DummyLogger(),
			Factory).ReadIntoBusinessObject();
		}

		public void TestPopulateContainerMode()
		{
			var universalShipment = SetupETailTestingShipmentWithMinimumRequirements();
			universalShipment.TransportMode.Code = TransportModes.Air;
			var clearance = ReadIntoUSLVClearance(Factory, universalShipment);
			AssertEquals("NCT", clearance.ULH_ContainerMode);

			universalShipment.TransportMode.Code = TransportModes.Sea;
			clearance = ReadIntoUSLVClearance(Factory, universalShipment);
			AssertEquals("CNT", clearance.ULH_ContainerMode);

			universalShipment.TransportMode.Code = TransportModes.Sea;
			universalShipment.ContainerMode.Code = ContainerModes.LCL;
			clearance = ReadIntoUSLVClearance(Factory, universalShipment);
			AssertEquals("NCT", clearance.ULH_ContainerMode);
		}

		[TestDate(2020, 1, 13)]
		public void TestPopulateDates()
		{
			var clearance = GetSampleClearance(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(new ZDate(2020, 1, 12), clearance.ULH_DepartureDate);
				AssertEquals(new ZDate(2020, 1, 14), clearance.ULH_DischargeDate);
				AssertEquals(new ZDate(2020, 1, 13), clearance.ULH_EntryDate);
			});
		}

		public void TestPopulateUseCode_SetToHVL()
		{
			var clearance = GetSampleClearance(Factory);
			AssertEquals("HVL", clearance.ULH_UseCode);
		}

		public static UniversalShipment SetupETailTestingShipmentWithMinimumRequirements()
		{
			var masterUniversalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			masterUniversalShipment.TransportMode = new CodeDescriptionPair() { Code = TransportModes.Air };
			masterUniversalShipment.ContainerMode = new ContainerMode() { Code = ContainerModes.FCL };

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00008888");
			shipment.SetDateCollection(() => new List<Date>()
			{
				Date.New(DateType.Departure, false, new ZDateTime(2020, 1, 12)),
				Date.New(DateType.Arrival, false, new ZDateTime(2020, 1, 14))
			});

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "CTNR000001"
			};
			packingLine.SetPackedItemCollection(() => new List<PackedItem>()
			{
				new PackedItem()
				{
					CommercialInvoiceLineLink = 666,
					Description = "Basketball shoes"
				}
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GoodsValueCurrency = new Currency() { Code = "CNY" },
				GoodsDescription = "Shoes",
				GoodsValue = 1_280,
				TotalNoOfPieces = 1,
				WayBillNumber = "WBBB000011212",
				WayBillType = new WayBillType() { Code = "HWB" },
				VendorIdentifier = "TestVendor",
				ConsigneeIdentifier = "EIN123",
			};
			subShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
					{
						new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
						{
							Address1 = "72 O'Riordan St",
							AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress)
						},
						new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
						{
							Address1 = "74 O'Riordan St",
							AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress)
						}
					});
			subShipment.SetCustomsReferenceCollection(() => new List<CustomsReference>()
					{
						new CustomsReference()
						{
							Type = new CodeDescriptionPair { Code = nameof(DataContext.Declaration) },
							Reference = "ABC123456"
						}
					});
			subShipment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
								{
									new CommercialInvoiceLine()
									{
										CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
										{
											new CustomsSupportingInformation()
											{
												Country = new Country() { Code = "CN" },
												Tariff = "7891238949"
											}
										},
										CustomsValue = 999,
										Description = "Nike basketball shoes",
										Link = 666,
										PartNo = "AA"
									},
									new CommercialInvoiceLine()
									{
										CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
										{
											new CustomsSupportingInformation()
											{
												Country = new Country() { Code = "CA" },
												Tariff = "7891238948"
											}
										},
										CustomsValue = 1,
										Description = "Some goods",
										Link = 777,
										PartNo = "BB"
									}
								}))
						}
			};
			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packingLine });

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment });

			masterUniversalShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { shipment });

			return masterUniversalShipment;
		}

		public static CusUSLVClearance GetSampleClearance(UniversalObjectFactory factory) => ReadIntoUSLVClearance(factory, SetupETailTestingShipmentWithMinimumRequirements());

		public static CusUSLVClearance ReadIntoUSLVClearance(UniversalObjectFactory factory, UniversalShipment universalShipment)
		{
			var reader = new eTailUSLVClearanceDataObjectReader(universalShipment, universalShipment.SubShipmentCollection.Single(), new DummyLogger(), factory);
			return reader.ReadIntoBusinessObject();
		}

		public class eTailUSLVClearanceDataObjectReaderTestOnly : eTailUSLVClearanceDataObjectReader
		{
			public eTailUSLVClearanceDataObjectReaderTestOnly(UniversalShipment masterShipmentDataObject, UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(masterShipmentDataObject, shipmentDataObject, logger, factory)
			{
			}

			public ZBool GetHasSubShipmentWithoutHouseBill()
			{
				return hasSubShipmentWithoutHouseBill;
			}

			public List<ZString> GetDuplicateBillNumberList()
			{
				return duplicateBillNumberList;
			}

			public Dictionary<ZString, CusUSLVConsignment[]> GetConsignmentDict()
			{
				return consignmentDict;
			}
		}
	}
}

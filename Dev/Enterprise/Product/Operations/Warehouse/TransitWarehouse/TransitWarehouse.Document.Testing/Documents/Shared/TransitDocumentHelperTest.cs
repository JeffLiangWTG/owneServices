using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.Common;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class TransitDocumentHelperTest : TestCaseWithFactory
	{
		void TestGetCustomsStatus_RCN_Core(string rcnDirection, string rcnDestination, string expectedCode, string rcnPreviousLoadPort = "", bool rcnHasCRN = false)
		{
			var warehouse = CreateWhsWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn.WRC_Direction = rcnDirection;
			rcn.WRC_RL_NKDestination = rcnDestination;
			if (!string.IsNullOrEmpty(rcnPreviousLoadPort))
			{
				Helper.CreateTransport(rcn, "FRPAR", rcnPreviousLoadPort);
			}
			if (rcnHasCRN)
			{
				helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			}

			var result = TransitDocumentHelper.GetCustomsStatus(rcn);
			AssertCustomsStatus(result, expectedCode);
		}

		public void TestGetCustomsStatus_RCN_Import_NextDischargeRouting()
		{
			var warehouse = CreateWhsWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			rcn.WRC_RL_NKDestination = "CNNJG";
			Helper.CreateTransport(rcn, "CNNJG", "FRPAR");
			helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			var result = TransitDocumentHelper.GetCustomsStatus(rcn);
			AssertCustomsStatus(result, CIN750CustomsStatus.Codes.Import);
		}

		public void TestGetCustomsStatus_RCN_Import_MatchingExtraPortRouting()
		{
			var warehouse = CreateWhsWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			rcn.WRC_RL_NKDestination = "GBLON";
			Helper.CreateTransport(rcn, "FRSTD", "GBLON");
			helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			var result = TransitDocumentHelper.GetCustomsStatus(rcn);
			AssertCustomsStatus(result, CIN750CustomsStatus.Codes.Domestic);
		}

		public void TestGetCustomsStatus_RCN_Export_EmptyDestination()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Export, "", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_RCN_Import_EmptyDestination()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_RCN_Domestic_EmptyDestination()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_RCN_Export_DestinationNotInEU()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Export, "CNNJG", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_RCN_Import_EmptyDestinationNotInEU()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Import, "CNNJG", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_RCN_Domestic_EmptyDestinationNotInEU()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "CNNJG", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_RCN_Export_DestinationInEU()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Export, "GBLON", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_RCN_Import_PreviousLoadInEU_RCNHasNoCRN_EmptyDestination()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Import, "GBLON");

		public void TestGetCustomsStatus_RCN_Import_PreviousLoadNotInEU_RCNHasCRN_EmptyDestination()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Import, "CNNJG", rcnHasCRN: true);

		public void TestGetCustomsStatus_RCN_Import_PreviousLoadInEU_RCNHasCRN_EmptyDestination()
			=> TestGetCustomsStatus_RCN_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Domestic, "GBLON", rcnHasCRN: true);

		public void TestGetCustomsStatus_RCN_Empty()
			=> TestGetCustomsStatus_RCN_Core("", "", "", "");

		void TestGetCustomsStatus_DCN_Core(string dcnDirection, string dcnDestination, string expectedCode)
		{
			var warehouse = CreateWhsWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_Direction = dcnDirection;
			dcn.WDC_RL_NKDestination = dcnDestination;

			var result = TransitDocumentHelper.GetCustomsStatus(dcn);
			AssertCustomsStatus(result, expectedCode);
		}

		public void TestGetCustomsStatus_DCN_Export_EmptyDestination()
			=> TestGetCustomsStatus_DCN_Core(TransitWarehouseConsignmentDirections.Codes.Export, "", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination()
			=> TestGetCustomsStatus_DCN_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_DCN_Domestic_EmptyDestination()
			=> TestGetCustomsStatus_DCN_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Export_DestinationNotInEU()
			=> TestGetCustomsStatus_DCN_Core(TransitWarehouseConsignmentDirections.Codes.Export, "JPTYO", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_DCN_Import_DestinationNotInEU()
			=> TestGetCustomsStatus_DCN_Core(TransitWarehouseConsignmentDirections.Codes.Import, "JPTYO", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_DCN_Domestic_DestinationNotInEU()
			=> TestGetCustomsStatus_DCN_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "JPTYO", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Export_DestinationInEU()
			=> TestGetCustomsStatus_DCN_Core(TransitWarehouseConsignmentDirections.Codes.Export, "FRPAR", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Empty()
			=> TestGetCustomsStatus_DCN_Core("", "", "");

		public void TestGetCustomsStatus_DCN_Import_DestinationInEU_RCNHasCRN()
		{
			var warehouse = CreateWhsWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			dcn.WDC_RL_NKDestination = "FRPAR";
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var result = TransitDocumentHelper.GetCustomsStatus(dcn);
			AssertCustomsStatus(result, CIN750CustomsStatus.Codes.Domestic);
		}

		public void TestGetCustomsStatus_DCN_Import_DestinationInEU_RCNHasCRN_ExceptNotOutPackages()
		{
			var warehouse = CreateWhsWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_Direction = TransitWarehouseConsignmentDirections.Codes.Import;
			dcn.WDC_RL_NKDestination = "FRPAR";
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			helper.CreateCustomsAdditionalReference(rcn1, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", dispatchConsignment: dcn);

			var result = TransitDocumentHelper.GetCustomsStatus(dcn);
			AssertCustomsStatus(result, CIN750CustomsStatus.Codes.Domestic);
		}

		void TestBuild_CustomsStatus_EmptyDestination_Core(string dcnDirection, string rcnDestination, string expectedCode, string rcnPreviousLoadPort = "", bool rcnHasCRN = false)
		{
			var warehouse = CreateWhsWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_Direction = dcnDirection;
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_RL_NKDestination = rcnDestination;
			if (!string.IsNullOrEmpty(rcnPreviousLoadPort))
			{
				Helper.CreateTransport(rcn, "FRPAR", rcnPreviousLoadPort);
			}
			if (rcnHasCRN)
			{
				helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			}
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var result = TransitDocumentHelper.GetCustomsStatus(dcn);
			AssertCustomsStatus(result, expectedCode);
		}

		public void TestGetCustomsStatus_DCN_Export_EmptyDestination_RCNEmptyDestination()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Export, "", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_RCNEmptyDestination()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_DCN_Domestic_EmptyDestination_RCNEmptyDestination()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Export_EmptyDestination_RCNDestinationNotInEU()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Export, "USNYC", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_RCNDestinationNotInEU()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Import, "USNYC", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_DCN_Domestic_EmptyDestination_RCNDestinationNotInEU()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "USNYC", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Export_EmptyDestination_RCNDestinationInEU()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Export, "ITROM", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Import_PreviousLoadInEU_RCNHasNoCRN_RCNEmptyDestination()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Import, "GBLON");

		public void TestGetCustomsStatus_DCN_Import_PreviousLoadNotInEU_RCNHasCRN_RCNEmptyDestination()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Import, "CNNJG", rcnHasCRN: true);

		public void TestGetCustomsStatus_DCN_Import_PreviousLoadInEU_RCNHasCRN_RCNEmptyDestination()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", CIN750CustomsStatus.Codes.Domestic, "GBLON", rcnHasCRN: true);

		public void TestGetCustomsStatus_DCN_Import_RCNHasCRN_EmptyDestination_RCNDestinationInEU()
			=> TestBuild_CustomsStatus_EmptyDestination_Core(TransitWarehouseConsignmentDirections.Codes.Import, "GBLON", CIN750CustomsStatus.Codes.Domestic, rcnHasCRN: true);

		void TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(string dcnDirection, string rcn1Destination, string rcn2Destination, string expectedCode, int rcnHasPreviousPortInEUQty = 0, int rcnHasCRNQty = 0)
		{
			var warehouse = CreateWhsWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			dcn.WDC_Direction = dcnDirection;
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn1.WRC_RL_NKDestination = rcn1Destination;
			if (rcnHasPreviousPortInEUQty > 0)
			{
				Helper.CreateTransport(rcn1, "FRPAR", "GBLON");
			}
			if (rcnHasCRNQty > 0)
			{
				helper.CreateCustomsAdditionalReference(rcn1, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			}
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			rcn2.WRC_RL_NKDestination = rcn2Destination;
			if (rcnHasPreviousPortInEUQty > 1)
			{
				Helper.CreateTransport(rcn2, "FRPAR", "GBLON");
			}
			if (rcnHasCRNQty > 1)
			{
				helper.CreateCustomsAdditionalReference(rcn2, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN2");
			}
			var packageState2 = Helper.CreatePackageState(rcn2, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var result = TransitDocumentHelper.GetCustomsStatus(dcn);
			AssertCustomsStatus(result, expectedCode);
		}

		public void TestGetCustomsStatus_DCN_Export_EmptyDestination_1RCNInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Export, "", "DEBER", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_1RCNInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", "DEBER", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_DCN_Domestic_EmptyDestination_1RCNInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "", "DEBER", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Export_EmptyDestination_1RCNInEU_1RCNNotInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Export, "AUSYD", "DEBER", CIN750CustomsStatus.Codes.Export);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_1RCNInEU_1RCNNotInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Import, "AUSYD", "DEBER", CIN750CustomsStatus.Codes.Import);

		public void TestGetCustomsStatus_DCN_Domestic_EmptyDestination_1RCNInEU_1RCNNotInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Domestic, "AUSYD", "DEBER", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Export_EmptyDestination_2RCNsInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Export, "GBLON", "DEBER", CIN750CustomsStatus.Codes.Domestic);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_2RCNsInEU_2RCNsHasCRN()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Import, "GBLON", "DEBER", CIN750CustomsStatus.Codes.Domestic, rcnHasCRNQty: 2);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_2RCNsHasCRN()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", "DEBER", CIN750CustomsStatus.Codes.Import, rcnHasCRNQty: 2);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_2RCNsHasPreviousPortInEU()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", "DEBER", CIN750CustomsStatus.Codes.Import, rcnHasPreviousPortInEUQty: 2);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_1RCNHasPreviousPortInEUAndCRN()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", "DEBER", CIN750CustomsStatus.Codes.Import, rcnHasPreviousPortInEUQty: 1, rcnHasCRNQty: 1);

		public void TestGetCustomsStatus_DCN_Import_EmptyDestination_2RCNsHasPreviousPortInEUAndCRN()
			=> TestBuild_CustomsStatus_EmptyDestinationWith2RCNs_Core(TransitWarehouseConsignmentDirections.Codes.Import, "", "DEBER", CIN750CustomsStatus.Codes.Domestic, rcnHasPreviousPortInEUQty: 2, rcnHasCRNQty: 2);

		void AssertCustomsStatus(ICodeDescription result, string expectedCode) =>
			CombineAssertions(() =>
			{
				AssertEquals("CustomesStatus.Code", expectedCode, result.Code);
				switch (expectedCode)
				{
					case CIN750CustomsStatus.Codes.Export:
						AssertEquals("CustomesStatus.Description", CIN750CustomsStatus.Descriptions.Export, result.Description);
						break;
					case CIN750CustomsStatus.Codes.Import:
						AssertEquals("CustomesStatus.Description", CIN750CustomsStatus.Descriptions.Import, result.Description);
						break;
					case CIN750CustomsStatus.Codes.Domestic:
						AssertEquals("CustomesStatus.Description", CIN750CustomsStatus.Descriptions.Domestic, result.Description);
						break;
				}
			});

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		WhsWarehouse CreateWhsWarehouse()
		{
			var branch = Helper.CreateGlbBranch("FRT");
			branch.GB_RL_NKHomePort = "FRPAR";
			branch.GB_RN_NKCountryCode = "FR";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse = Helper.CreateWarehouse("FRT", orgAddress, branch);
			warehouse.WarehouseAddress.Address1 = "address1";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var extraPort = Factory.New<GlbBranchExtraPorts>();
			extraPort.GY_GB = warehouse.WW_GB_RelatedCompanyBranch;
			extraPort.GY_IsValid = true;
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "FRSTD";

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);

			var inLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			warehouse.WW_DefaultInboundDockDoor = inLocation.PK;

			var outLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-2");
			warehouse.WW_DefaultOutboundDockDoor = outLocation.PK;
			return warehouse;
		}
	}
}

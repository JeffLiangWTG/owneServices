using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WarehouseMatchingHelperTest : TransitUniversalTestCase
	{
		#region TestGetWarehouse

		public void TestGetWarehouse_LocalCartageCFS()
		{
			AssertGetWarehouse(DocAddressType.LocalCartageCFS);
		}

		public void TestGetWarehouse_ArrivalCFS()
		{
			AssertGetWarehouse(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW);
		}

		public void TestGetWarehouse_DepartureCFS()
		{
			AssertGetWarehouse(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW);
		}

		void AssertGetWarehouse(DocAddressType docAddressType, RecipientRoleType? recipientRoleType = null)
		{
			Data.SetupForForwardingImport();
			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(docAddressType);
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };

			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == docAddressType.ToString());
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			var warehouse = WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger);
			AssertEquals("The matched warehouse should be equal to the correct warehouse.", warehouse[WhsWarehouseSchema.Constants.PK], Data.WarehouseINTHEMSYD.PK);
		}

		#region GetWarehouse from premise ID

		public void TestGetWarehouse_FromPremiseAddress_Arrival()
		{
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "9914N", CountryCodes.Australia);
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.ATW);

			AssertEquals("The warehouse should be matched from destination premise id.", warehouse[WhsWarehouseSchema.Constants.PK], Data.Warehouse.PK);
			AssertContains("Should log a successful match", "Information - Found Warehouse TWH from Destination Premise (Reference Number - '9914N').", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_Departure()
		{
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.WarehouseCRAHOLSYD, "9922W", CountryCodes.Australia);
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.DTW);

			AssertEquals("The warehouse should be matched from origin premise id.", warehouse[WhsWarehouseSchema.Constants.PK], Data.WarehouseCRAHOLSYD.PK);
			AssertContains("Should log a successful match", "Information - Found Warehouse TW3 from Origin Premise (Reference Number - '9922W').", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_Arrival_WrongPremise()
		{
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.ATW);

			AssertNull("The warehouse should not be matched from destination premise id.", warehouse);
			AssertContains("Should log wrong premise.", "Warning - Could not find Warehouse. No matching Warehouse found from Destination Premise (Reference Number - '9914N').", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_Departure_WrongPremise()
		{
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.DTW);

			AssertNull("The warehouse should not be matched from origin premise id.", warehouse);
			AssertContains("Should log wrong premise.", "Warning - Could not find Warehouse. No matching Warehouse found from Origin Premise (Reference Number - '9922W').", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_Arrival_EmptyPremise()
		{
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.ATW, new[] { (WarehouseAdditionalReferenceTypes.Codes.DestinationPremiseID, "", "AU"), (WarehouseAdditionalReferenceTypes.Codes.OriginPremiseID, "", "AU") });

			AssertNull("The warehouse should not be matched from destination premise id.", warehouse);
			AssertContains("Should log empty premise.", "Warning - Could not find Warehouse. Reference Number is empty in Destination Premise.", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_Departure_EmptyPremise()
		{
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.DTW, new[] { (WarehouseAdditionalReferenceTypes.Codes.DestinationPremiseID, "", "AU"), (WarehouseAdditionalReferenceTypes.Codes.OriginPremiseID, "", "AU") });

			AssertNull("The warehouse should not be matched from origin premise id.", warehouse);
			AssertContains("Should log no premise.", "Warning - Could not find Warehouse. Reference Number is empty in Origin Premise.", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_Arrival_NoPremise()
		{
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.ATW, Array.Empty<(string code, string number, string countryCode)>());

			AssertNull("The warehouse should not be matched from destination premise id.", warehouse);
			AssertContains("Should log no premise.", "Warning - Could not find Warehouse. Destination Premise is not found in UXML.", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_Departure_NoPremise()
		{
			var warehouse = GetWarehouseFromPremiseAddress(RecipientRoleType.DTW, Array.Empty<(string code, string number, string countryCode)>());

			AssertNull("The warehouse should not be matched from origin premise id.", warehouse);
			AssertContains("Should log empty premise.", "Warning - Could not find Warehouse. Origin Premise is not found in UXML.", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_InvalidReceipient()
		{
			var warehouse = GetWarehouseFromPremiseAddress(null, Array.Empty<(string code, string number, string countryCode)>());

			AssertNull("The warehouse should not be matched from origin premise id.", warehouse);
			AssertContains("Should log role type is incorrect.", "Warning - Could not find Warehouse. Role type in the UXML is incorrect for Departure Transit Warehouse and Arrival Transit Warehouse.", Logger.Logs);
		}

		public void TestGetWarehouse_FromPremiseAddress_DuplicateCusOrgCode()
		{
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.WarehouseCRAHOLSYD, "9922W", CountryCodes.Australia);
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.WarehouseCRAHOLSYD, "9922W", CountryCodes.Singapore);

			AssertExceptionThrown<DataObjectReadFailureException>("Duplicate code should throw an exception.", "Multiple Premise addresses were found for premise id '9922W'.", () => GetWarehouseFromPremiseAddress(RecipientRoleType.DTW));
		}

		IColumnIndexer GetWarehouseFromPremiseAddress(RecipientRoleType? recipientRoleType = null, (string code, string number, string countryCode)[] types = null)
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => null);

			types = types ?? new[] { (WarehouseAdditionalReferenceTypes.Codes.DestinationPremiseID, "9914N", "AU"), (WarehouseAdditionalReferenceTypes.Codes.OriginPremiseID, "9922W", "AU") };
			Helper.SetShipmentAdditionalReference(Data.ShipmentDataObject, types);

			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			return WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger);
		}

		public void TestGetWarehouse_NoOrg_NoPremise()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => null);

			var warehouse = WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger);

			AssertNull("The warehouse should not be matched from origin premise id.", warehouse);
			AssertContains("Should log no premise or no organization.", "Warning - Could not find Warehouse. No organization address or premise information is found in UXML.", Logger.Logs);
		}

		#endregion

		#endregion

		#region TestGetWarehouse_Logs

		public void TestGetWarehouse_Logs_LocalCartageCFS_NoOrgCode()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Local Cartage CFS Address ''.";
			TestGetWarehouse_Logs(DocAddressType.LocalCartageCFS, recipientRoleType: default, orgCode: null, address1: null, logForMatching);
		}

		public void TestGetWarehouse_Logs_ArrivalCFS_NoOrgCode()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Arrival CFS Address ''.";
			TestGetWarehouse_Logs(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW, orgCode: null, address1: null, logForMatching);
		}

		public void TestGetWarehouse_Logs_DepartureCFS_NoOrgCode()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Departure CFS Address ''.";
			TestGetWarehouse_Logs(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW, orgCode: null, address1: null, logForMatching);
		}

		public void TestGetWarehouse_Logs_LocalCartageCFS()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Local Cartage CFS Address 'OrgCode - Address1'.";
			TestGetWarehouse_Logs(DocAddressType.LocalCartageCFS, recipientRoleType: default, orgCode: "OrgCode", address1: "Address1", logForMatching);
		}

		public void TestGetWarehouse_Logs_ArrivalCFS()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Arrival CFS Address 'OrgCode - Address1'.";
			TestGetWarehouse_Logs(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW, orgCode: "OrgCode", address1: "Address1", logForMatching);
		}

		public void TestGetWarehouse_Logs_DepartureCFS()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Departure CFS Address 'OrgCode - Address1'.";
			TestGetWarehouse_Logs(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW, orgCode: "OrgCode", address1: "Address1", logForMatching);
		}

		public void TestGetWarehouse_Logs_LocalCartageCFS_OnlyOrgCode()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Local Cartage CFS Address 'OrgCode'.";
			TestGetWarehouse_Logs(DocAddressType.LocalCartageCFS, recipientRoleType: default, orgCode: "OrgCode", address1: null, logForMatching);
		}

		public void TestGetWarehouse_Logs_ArrivalCFS_OnlyOrgCode()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Arrival CFS Address 'OrgCode'.";
			TestGetWarehouse_Logs(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW, orgCode: "OrgCode", address1: null, logForMatching);
		}

		public void TestGetWarehouse_Logs_DepartureCFS_OnlyOrgCode()
		{
			var logForMatching = "Information - Searching for Transit Warehouse matching Departure CFS Address 'OrgCode'.";
			TestGetWarehouse_Logs(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW, orgCode: "OrgCode", address1: null, logForMatching);
		}

		void TestGetWarehouse_Logs(DocAddressType docAddressType, RecipientRoleType recipientRoleType, string orgCode, string address1, string logForMatching = "")
		{
			Data.SetupForForwardingImport();
			Data.Orgs.INTHEMSYD.OH_Code = orgCode;
			Factory.SaveForTesting();
			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(docAddressType);
			cfsAddress.OrganizationCode = orgCode;
			cfsAddress.Address1 = address1;
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };

			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == docAddressType.ToString());
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var warehouse = WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger);

			AssertContains("Should log the start of Warehouse matching", logForMatching, Logger.Logs);

			if (!string.IsNullOrEmpty(orgCode))
			{
				if (docAddressType == DocAddressType.ArrivalCFSAddress)
				{
					AssertContains("Should log a successful match", $"Found Warehouse {Data.WarehouseINTHEMSYD.WW_WarehouseCode} from Arrival CFS Address.", Logger.Logs);
				}
				else if (docAddressType == DocAddressType.DepartureCFSAddress)
				{
					AssertContains("Should log a successful match", $"Found Warehouse {Data.WarehouseINTHEMSYD.WW_WarehouseCode} from Departure CFS Address.", Logger.Logs);
				}
				else
				{
					AssertContains("Should log a successful match", $"Found Warehouse {Data.WarehouseINTHEMSYD.WW_WarehouseCode} from Local Cartage CFS Address.", Logger.Logs);
				}
			}
		}

		public void TestGetWarehouse_NoMatch()
		{
			Data.SetupForForwardingImport();
			Data.WarehouseINTHEMSYD.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Data.WarehouseINTHEMSYD.WW_IsVirtualWarehouse = true;
			Factory.SaveForTesting();
			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.DepartureCFSAddress);
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };

			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == nameof(DocAddressType.DepartureCFSAddress));
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });

			var warehouse = WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger);

			AssertNull(warehouse);

			AssertContains("Should log that no warehouse was found", "Warning - Could not find Warehouse from Departure CFS Address.", Logger.Logs);
		}

		#endregion

		#region TestGetWarehouse_MultipleTransitWarehousesForSameAddress

		public void TestGetWarehouse_MultipleTransitWarehousesForSameAddress_LocalCartageCFS()
		{
			AssertGetWarehouse_MultipleTransitWarehousesForSameAddress(DocAddressType.LocalCartageCFS);
		}

		public void TestGetWarehouse_MultipleTransitWarehousesForSameAddress_ArrivalCFS()
		{
			AssertGetWarehouse_MultipleTransitWarehousesForSameAddress(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW);
		}

		public void TestGetWarehouse_MultipleTransitWarehousesForSameAddress_DepartureCFS()
		{
			AssertGetWarehouse_MultipleTransitWarehousesForSameAddress(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW);
		}

		void AssertGetWarehouse_MultipleTransitWarehousesForSameAddress(DocAddressType docAddressType, RecipientRoleType? recipientRoleType = null)
		{
			Data.SetupForForwardingImport();
			var newBranch = Factory.BOFactory.New<GlbBranch>();
			newBranch.GB_Code = "N1B";
			newBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC;

			var warehouseAddress = Data.WarehouseINTHEMSYD.WW_OA_WarehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_OA_WarehouseAddress = warehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_GB_RelatedCompanyBranch = newBranch.PK;
			Factory.SaveForTesting();

			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(docAddressType);
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };
			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == docAddressType.ToString());
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			var expectedMessage = @"Multiple Active Transit Warehouses TW2, TW3 matching the Address 'INTHEMSYD - Unit 12, Level 3' were found.
Ensure only one Active Transit Warehouse exists matching this Address.";

			AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedMessage, () => WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger));
		}

		#endregion

		#region TestGetWarehouse_TransitAndNonTransitForSameAddress

		public void TestGetWarehouse_TransitAndNonTransitForSameAddress_LocalCartageCFS()
		{
			AssertetWarehouse_TransitAndNonTransitForSameAddress(DocAddressType.LocalCartageCFS);
		}

		public void TestGetWarehouse_TransitAndNonTransitForSameAddress_ArrivalCFS()
		{
			AssertetWarehouse_TransitAndNonTransitForSameAddress(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW);
		}

		public void TestGetWarehouse_TransitAndNonTransitForSameAddress_DepartureCFS()
		{
			AssertetWarehouse_TransitAndNonTransitForSameAddress(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW);
		}

		void AssertetWarehouse_TransitAndNonTransitForSameAddress(DocAddressType docAddressType, RecipientRoleType? recipientRoleType = null)
		{
			Data.SetupForForwardingImport();
			var newBranch = Factory.BOFactory.New<GlbBranch>();
			newBranch.GB_Code = "N1B";
			newBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC;

			var warehouseAddress = Data.WarehouseINTHEMSYD.WW_OA_WarehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_OA_WarehouseAddress = warehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_GB_RelatedCompanyBranch = newBranch.PK;
			Data.WarehouseCRAHOLSYD.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.SaveForTesting();

			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(docAddressType);
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };

			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == docAddressType.ToString());
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			var warehouse = WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger);
			AssertEquals("The matched warehouse should be equal to the correct transit warehouse.", warehouse[WhsWarehouseSchema.Constants.PK], Data.WarehouseINTHEMSYD.PK);
		}

		#endregion

		#region TestGetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress

		public void TestGetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress_LocalCartageCFS()
		{
			AssertetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress(DocAddressType.LocalCartageCFS);
		}

		public void TestGetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress_ArrivalCFS()
		{
			AssertetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW);
		}

		public void TestGetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress_DepartureCFS()
		{
			AssertetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW);
		}

		void AssertetWarehouse_ActiveAndNonActiveTransitWarehouseForSameAddress(DocAddressType docAddressType, RecipientRoleType? recipientRoleType = null)
		{
			Data.SetupForForwardingImport();
			var newBranch = Factory.BOFactory.New<GlbBranch>();
			newBranch.GB_Code = "N1B";
			newBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC;

			var warehouseAddress = Data.WarehouseINTHEMSYD.WW_OA_WarehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_OA_WarehouseAddress = warehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_GB_RelatedCompanyBranch = newBranch.PK;
			Data.WarehouseCRAHOLSYD[WhsWarehouseSchema.Constants.WW_IsActive] = false;
			Factory.SaveForTesting();

			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(docAddressType);
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };

			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == docAddressType.ToString());
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			var warehouse = WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger);
			AssertEquals("The matched warehouse should be equal to the correct warehouse.", warehouse[WhsWarehouseSchema.Constants.PK], Data.WarehouseINTHEMSYD.PK);
		}

		#endregion

		#region TestGetWarehouse_AllWarehousesAreNonActive

		public void TestGetWarehouse_AllWarehousesAreNonActive_LocalCartageCFS()
		{
			AssertGetWarehouse_AllWarehousesAreNonActive(DocAddressType.LocalCartageCFS);
		}

		public void TestGetWarehouse_AllWarehousesAreNonActive_ArrivalCFS()
		{
			AssertGetWarehouse_AllWarehousesAreNonActive(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW);
		}

		public void TestGetWarehouse_AllWarehousesAreNonActive_DepartureCFS()
		{
			AssertGetWarehouse_AllWarehousesAreNonActive(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW);
		}

		void AssertGetWarehouse_AllWarehousesAreNonActive(DocAddressType docAddressType, RecipientRoleType? recipientRoleType = null)
		{
			Data.SetupForForwardingImport();
			var newBranch = Factory.BOFactory.New<GlbBranch>();
			newBranch.GB_Code = "N1B";
			newBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC;

			var warehouseAddress = Data.WarehouseINTHEMSYD.WW_OA_WarehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_OA_WarehouseAddress = warehouseAddress;
			Data.WarehouseCRAHOLSYD.WW_GB_RelatedCompanyBranch = newBranch.PK;
			Data.WarehouseCRAHOLSYD[WhsWarehouseSchema.Constants.WW_IsActive] = false;
			Data.WarehouseINTHEMSYD[WhsWarehouseSchema.Constants.WW_IsActive] = false;
			Factory.SaveForTesting();

			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(docAddressType);
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };

			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == docAddressType.ToString());
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			AssertNull(WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger));
		}

		#endregion

		#region TestGetWarehouse_NoMatchingTransitWarehouse

		public void TestGetWarehouse_NoMatchingTransitWarehouse()
		{
			Data.SetupForForwardingImport();
			var departureCFS = Data.ShipmentDataObject.OrganizationAddressCollection.Single(o => o.AddressType.GetValueOrDefault() == nameof(DocAddressType.DepartureCFSAddress));
			departureCFS.AddressType = nameof(DocAddressType.LocalCartageCFS);
			var arrivalCFS = Data.ShipmentDataObject.OrganizationAddressCollection.Single(o => o.AddressType.GetValueOrDefault() == nameof(DocAddressType.ArrivalCFSAddress));
			arrivalCFS.AddressType = nameof(DocAddressType.LocalCartageCFS);
			Data.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Data.Warehouse.WW_IsVirtualWarehouse = true;
			Factory.SaveForTesting();

			AssertNull(WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger));
		}

		public void TestGetWarehouse_NoWarehouseAddresses()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.OrganizationAddressCollection.Clear();

			AssertNull(WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger));
			AssertContains("should add a log for not found an address.", "Warning - Could not find Warehouse. UXML does not have a valid corresponding address.", Logger.Logs);
		}

		#endregion

		#region TestThrowForNoMatchingWarehouse

		public void TestThrowForNoMatchingWarehouse_LocalCartageCFS_NoOrgCodeOrAddress1()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.LocalCartageCFS, recipientRoleType: null, orgCode: null, address1: null,
 $"Cannot import without a valid Warehouse supplied. Please ensure your CFS Address '' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_ArrivalCFS_NoOrgCodeOrAddress1()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW, orgCode: null, address1: null,
 $"Cannot import without a valid Warehouse supplied. Please ensure your Arrival CFS Address '' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_DepartureCFS_NoOrgCodeOrAddress1()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW, orgCode: null, address1: null,
 $"Cannot import without a valid Warehouse supplied. Please ensure your Departure CFS Address '' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_NoCFSAddress()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.ConsignorDocumentaryAddress, recipientRoleType: null, orgCode: "orgCode", address1: "address1",
 $"Cannot import without a valid Warehouse supplied. Please ensure there is a valid departure/arrival CFS address and that you targeted the correct recipient type.");
		}

		public void TestThrowForNoMatchingWarehouse_LocalCartageCFS()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.LocalCartageCFS, recipientRoleType: null, orgCode: "orgCode", address1: "address1",
 $"Cannot import without a valid Warehouse supplied. Please ensure your CFS Address 'orgCode - address1' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_ArrivalCFS()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW, orgCode: "orgCode", address1: "address1",
 $"Cannot import without a valid Warehouse supplied. Please ensure your Arrival CFS Address 'orgCode - address1' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_DepartureCFS()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW, orgCode: "orgCode", address1: "address1",
 $"Cannot import without a valid Warehouse supplied. Please ensure your Departure CFS Address 'orgCode - address1' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_LocalCartageCFS_OnlyOrgCode()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.LocalCartageCFS, recipientRoleType: null, orgCode: "orgCode", address1: null,
 $"Cannot import without a valid Warehouse supplied. Please ensure your CFS Address 'orgCode' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_ArrivalCFS_OnlyOrgCode()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.ArrivalCFSAddress, RecipientRoleType.ATW, orgCode: "orgCode", address1: null,
 $"Cannot import without a valid Warehouse supplied. Please ensure your Arrival CFS Address 'orgCode' has an active Transit Warehouse.");
		}

		public void TestThrowForNoMatchingWarehouse_DepartureCFS_OnlyOrgCode()
		{
			TestThrowForNoMatchingWarehouse(DocAddressType.DepartureCFSAddress, RecipientRoleType.DTW, orgCode: "orgCode", address1: null,
 $"Cannot import without a valid Warehouse supplied. Please ensure your Departure CFS Address 'orgCode' has an active Transit Warehouse.");
		}

		void TestThrowForNoMatchingWarehouse(DocAddressType docAddressType, RecipientRoleType? recipientRoleType, string orgCode, string address1, string errorMessage)
		{
			var cfsAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(docAddressType);
			cfsAddress.Port = new UNLOCO() { Code = "AUSYD" };
			cfsAddress.OrganizationCode = orgCode;
			cfsAddress.Address1 = address1;

			Data.ShipmentDataObject.OrganizationAddressCollection.RemoveAll(a => a.AddressType.GetValueOrDefault() == docAddressType.ToString());
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType.GetValueOrDefault() } } });

			AssertExceptionThrown(typeof(DataObjectReadFailureException), errorMessage,
				() => WarehouseMatchingHelper.ThrowForNoMatchingWarehouse(Data.ShipmentDataObject, Logger));
		}

		#endregion

		#region Implementation

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory.BOFactory);

		#endregion
	}
}

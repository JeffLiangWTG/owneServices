using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using AccGLHeaderDO = CargoWise.Database.TestFramework.ObjectModel.AccGLHeader;
using Constants = Enterprise.Core.Constants;
using GlbBranchDO = CargoWise.Database.TestFramework.ObjectModel.GlbBranch;
using GlbCompanyDO = CargoWise.Database.TestFramework.ObjectModel.GlbCompany;
using GlbDepartmentDO = CargoWise.Database.TestFramework.ObjectModel.GlbDepartment;
using JobHeaderDO = CargoWise.Database.TestFramework.ObjectModel.JobHeader;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	class LocalTransportSqlViewsTesting : TestCaseWithFactory
	{
		public void TestReport_LocalCartageLegsByVehicleOrTransportCompany_ShowsAmountsForPostedRates()
		{
			var globalDepartment = GlbDepartmentDO.ShallowLoadFromDB(TestConnection)[0];
			var globalBranch = GlbBranchDO.ShallowLoadFromDB(TestConnection)[0];
			var globalCompany = GlbCompanyDO.ShallowLoadFromDB(TestConnection)[0];
			var glHeader = AccGLHeaderDO.ShallowLoadFromDB(TestConnection)[0];

			Helper.InsertIntoDbAndReturnJobHeader_LocalCartageLegsByVehicleOrTransportCompany(globalCompany.PK, globalDepartment.PK, globalBranch.PK, glHeader.PK, TestConnection);

			using (var command = TestConnection.Command("Report_LocalCartageLegsByVehicleOrTransportCompany"))
			{
				Helper.CreateCommand_LocalCartageLegsByVehicleOrTransportCompany(command, globalCompany.PK);
				using (var reader = command.ExecuteReader())
				{
					Assert("Report doesn't return any records", reader.Read());
					// Job Amounts
					AssertEquals("JobWIP is incorrect.", 40m, reader["JobWIP"]);
					AssertEquals("JobREV is incorrect.", 130m, reader["JobREV"]);
					AssertEquals("JobIncome is incorrect.", 170m, reader["JobIncome"]);
					AssertEquals("JobIncomePerLeg is incorrect.", 170m, reader["JobIncomePerLeg"]);
					AssertEquals("JobIncomePerChargeableLeg is incorrect.", 170m, reader["JobIncomePerChargeableLeg"]);
					// Job Only Amounts
					AssertEquals("JobOnlyWIP is incorrect.", 15m, reader["JobOnlyWIP"]);
					AssertEquals("JobOnlyREV is incorrect.", 60m, reader["JobOnlyREV"]);
					AssertEquals("JobOnlyIncome is incorrect.", 75m, reader["JobOnlyIncome"]);
					AssertEquals("JobOnlyIncomePerLeg is incorrect.", 75m, reader["JobOnlyIncomePerLeg"]);
					AssertEquals("JobOnlyIncomePerChargeableLeg is incorrect.", 75m, reader["JobOnlyIncomePerChargeableLeg"]);
					// Leg Only Amounts
					AssertEquals("LegOnlyWIP is incorrect.", 25m, reader["LegOnlyWIP"]);
					AssertEquals("LegOnlyREV is incorrect.", 70m, reader["LegOnlyREV"]);
					AssertEquals("LegOnlyIncome is incorrect.", 95m, reader["LegOnlyIncome"]);
					AssertEquals("LegOnlyIncomeAndJobOnlyIncomePerLeg is incorrect.", 170m, reader["LegOnlyIncomeAndJobOnlyIncomePerLeg"]);
					AssertEquals("LegOnlyIncomeAndJobOnlyIncomePerChargeableLeg is incorrect.", 170m, reader["LegOnlyIncomeAndJobOnlyIncomePerChargeableLeg"]);
					Assert("Report returns more than the expected 1 record", !reader.Read());
				}
			}

			// another company (which has a different job header does not see this data)
			var otherGlobalCompany = GlbCompanyDO.ShallowLoadFromDB(TestConnection, c => c.PK != globalCompany.PK)[0];
			using (var command = TestConnection.Command("Report_LocalCartageLegsByVehicleOrTransportCompany"))
			{
				Helper.CreateCommand_LocalCartageLegsByVehicleOrTransportCompany(command, otherGlobalCompany.PK);
				using (var reader = command.ExecuteReader())
				{
					Assert("Report doesn't return any records", reader.Read());
					// Job Amounts
					AssertEquals("JobWIP is incorrect.", 0m, reader["JobWIP"]);
					AssertEquals("JobREV is incorrect.", 0m, reader["JobREV"]);
					AssertEquals("JobIncome is incorrect.", 0m, reader["JobIncome"]);
					AssertEquals("JobIncomePerLeg is incorrect.", 0m, reader["JobIncomePerLeg"]);
					AssertEquals("JobIncomePerChargeableLeg is incorrect.", 0m, reader["JobIncomePerChargeableLeg"]);
					// Job Only Amounts
					AssertEquals("JobOnlyWIP is incorrect.", 0m, reader["JobOnlyWIP"]);
					AssertEquals("JobOnlyREV is incorrect.", 0m, reader["JobOnlyREV"]);
					AssertEquals("JobOnlyIncome is incorrect.", 0m, reader["JobOnlyIncome"]);
					AssertEquals("JobOnlyIncomePerLeg is incorrect.", 0m, reader["JobOnlyIncomePerLeg"]);
					AssertEquals("JobOnlyIncomePerChargeableLeg is incorrect.", 0m, reader["JobOnlyIncomePerChargeableLeg"]);
					// Leg Only Amounts
					AssertEquals("LegOnlyWIP is incorrect.", 0m, reader["LegOnlyWIP"]);
					AssertEquals("LegOnlyREV is incorrect.", 0m, reader["LegOnlyREV"]);
					AssertEquals("LegOnlyIncome is incorrect.", 0m, reader["LegOnlyIncome"]);
					AssertEquals("LegOnlyIncomeAndJobOnlyIncomePerLeg is incorrect.", 0m, reader["LegOnlyIncomeAndJobOnlyIncomePerLeg"]);
					AssertEquals("LegOnlyIncomeAndJobOnlyIncomePerChargeableLeg is incorrect.", 0m, reader["LegOnlyIncomeAndJobOnlyIncomePerChargeableLeg"]);
					Assert("Report returns more than the expected 1 record", !reader.Read());
				}
			}
		}

		public void TestReport_LocalCartageLegsByVehicleOrTransportCompany_JobInactive()
		{
			var globalDepartment = GlbDepartmentDO.ShallowLoadFromDB(TestConnection)[0];
			var globalBranch = GlbBranchDO.ShallowLoadFromDB(TestConnection)[0];
			var globalCompany = GlbCompanyDO.ShallowLoadFromDB(TestConnection)[0];
			var glHeader = AccGLHeaderDO.ShallowLoadFromDB(TestConnection)[0];

			var jobHeader = Helper.InsertIntoDbAndReturnJobHeader_LocalCartageLegsByVehicleOrTransportCompany(globalCompany.PK, globalDepartment.PK, globalBranch.PK, glHeader.PK, TestConnection);

			using (var command = TestConnection.Command("Report_LocalCartageLegsByVehicleOrTransportCompany"))
			{
				Helper.CreateCommand_LocalCartageLegsByVehicleOrTransportCompany(command, globalCompany.PK);
				using (var reader = command.ExecuteReader())
				{
					Assert("Report doesn't return any records", reader.Read());
					AssertNotEquals("Active job returns null values", DBNull.Value, reader["JobHeaderPK"]);
					Assert("Report returns more than the expected 1 record", !reader.Read());
				}

				JobHeaderDO.UpdateWhere(jobHeader.PK).Set(jh => jh.JH_IsActive, false).Post(TestConnection);
				using (var reader = command.ExecuteReader())
				{
					Assert("Report doesn't return any records", reader.Read());
					AssertEquals("Inactive Job returns non-null values", DBNull.Value, reader["JobHeaderPK"]);
					Assert("Report returns more than the expected 1 record", !reader.Read());
				}
			}
		}

		public void TestReport_TransportProfileReport()
		{
			// 2 leg w/ cartage no job type
			var cartage_NoJobType = Factory.New<CommonCartage>();
			new JobHeader.Loader(cartage_NoJobType).TryLoadOrCreate();
			var move = cartage_NoJobType.ContainerBookedMoves.AddNew();
			var legEmpty = move.CartageLegs.AddNew();
			var legFull = move.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(legEmpty, legFull, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			// 3 legs w/ cartage type
			var cartage_JobType = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE, 3);
			new JobHeader.Loader(cartage_JobType).TryLoadOrCreate();
			cartage_JobType.FirstDocAddress.E2_OA_Address = Helper.CreateOrgHeader("CTO", "CTOADDY").MainAddress.PK;
			cartage_JobType.SecondDocAddress.E2_OA_Address = Helper.CreateOrgHeader("CNE", "CNEADDY").MainAddress.PK;
			Factory.Save();
			var results = LoadTransportProfileReport();
			AssertEquals("Report's SQL should return only 2 records as there is only 2 jobs.", 2, results.Count);
		}

		public void TestReport_TransportProfileReport_Default()
		{
			// Arrange
			var testHelperFirst = new TransportProfileReportTestHelper("First");
			var testHelperSecond = new TransportProfileReportTestHelper("Second");
			var testHelperThird = new TransportProfileReportTestHelper("Third");
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 3);
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var orgHeader1 = Helper.CreateOrgHeader("CTO", "CTOADDY");
			var orgHeader2 = Helper.CreateOrgHeader("CNE", "CNEADDY");
			var orgHeader3 = Helper.CreateOrgHeader("CCL", "CCLADDY");
			cartage.FirstDocAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = orgHeader2.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = orgHeader3.MainAddress.PK;
			testHelperFirst.Evaluate(orgHeader1.MainAddress);
			testHelperSecond.Evaluate(orgHeader2.MainAddress);
			testHelperThird.Evaluate(orgHeader3.MainAddress);
			Factory.Save();
			// Action
			var results = LoadTransportProfileReport();
			// Assert
			var result = results[0];
			testHelperFirst.AssertResult(result);
			testHelperSecond.AssertResult(result);
			testHelperThird.AssertResult(result);
		}

		public void TestReport_TransportProfileReport_WithOverridedAddress()
		{
			// Arrange
			var testHelperFirst = new TransportProfileReportTestHelper("First");
			var testHelperSecond = new TransportProfileReportTestHelper("Second");
			var testHelperThird = new TransportProfileReportTestHelper("Third");
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLImportToCNE, 3);
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var orgHeader1 = Helper.CreateOrgHeader("CTO", "CTOADDY");
			var orgHeader2 = Helper.CreateOrgHeader("CNE", "CNEADDY");
			var orgHeader3 = Helper.CreateOrgHeader("CCL", "CCLADDY");
			cartage.FirstDocAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = orgHeader2.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = orgHeader3.MainAddress.PK;
			testHelperFirst.Evaluate(orgHeader1.MainAddress);
			testHelperSecond.Evaluate(orgHeader2.MainAddress);
			testHelperThird.Evaluate(orgHeader3.MainAddress);
			Factory.Save();
			cartage.FirstDocAddress.E2_AddressOverride = true;
			cartage.SecondDocAddress.E2_AddressOverride = true;
			cartage.ThirdDocAddress.E2_AddressOverride = true;
			testHelperFirst.Evaluate(cartage.FirstDocAddress);
			testHelperSecond.Evaluate(cartage.SecondDocAddress);
			testHelperThird.Evaluate(cartage.ThirdDocAddress);
			Factory.Save();
			// Action
			var results = LoadTransportProfileReport();
			// Assert
			var result = results[0];
			testHelperFirst.AssertResultOverrided(result);
			testHelperSecond.AssertResultOverrided(result);
			testHelperThird.AssertResultOverrided(result);
		}

		public void TestReport_TransportProfileReport_CartageHasNoJobType()
		{
			var testHelperFirst = new TransportProfileReportTestHelper("First");
			var testHelperSecond = new TransportProfileReportTestHelper("Second");
			var testHelperThird = new TransportProfileReportTestHelper("Third");
			var cartage_NoJobType = Factory.New<CommonCartage>();
			cartage_NoJobType.JJ_E3_NKJobType = string.Empty;
			new JobHeader.Loader(cartage_NoJobType).TryLoadOrCreate();
			var move = cartage_NoJobType.ContainerBookedMoves.AddNew();
			move.EW_DisplayOrder = 1;
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			var docAddress1 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageYard, "org4", "add4", "2000", "SYDNEY", "AUSYD", true);
			var docAddress2 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageExporter, "org5", "add5", "2000", "SYDNEY", "AUSYD", true);
			var docAddress3 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageCTO, "org6", "add6", "2000", "SYDNEY", "AUSYD", true);
			leg1.JU_E2PickupAddressID = docAddress1.PK;
			leg1.JU_E2DeliveryAddressID = docAddress2.PK;
			leg1.JU_DisplayOrder = 1;
			leg2.JU_E2PickupAddressID = docAddress2.PK;
			leg2.JU_E2DeliveryAddressID = docAddress3.PK;
			leg2.JU_DisplayOrder = 2;
			testHelperFirst.Evaluate(docAddress1);
			testHelperSecond.Evaluate(docAddress2);
			testHelperThird.Evaluate(docAddress3);
			Factory.Save();
			// Action
			var results = LoadTransportProfileReport();
			// Assert
			var result = results[0];
			testHelperFirst.AssertResultOverrided(result);
			testHelperSecond.AssertResultOverrided(result);
			testHelperThird.AssertResultOverrided(result);
		}

		public void TestReport_TransportProfileReport_JU_DisplayOrderVeryLarge_DoesNotCauseException()
		{
			var cartage_NoJobType = Factory.New<CommonCartage>();
			cartage_NoJobType.JJ_E3_NKJobType = string.Empty;
			new JobHeader.Loader(cartage_NoJobType).TryLoadOrCreate();
			var move = cartage_NoJobType.ContainerBookedMoves.AddNew();
			move.EW_DisplayOrder = 1;
			var leg1 = move.CartageLegs.AddNew();
			var docAddress1 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageExporter, "org5", "add5", "2000", "SYDNEY", "AUSYD", true);
			var docAddress2 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageCTO, "org6", "add6", "2000", "SYDNEY", "AUSYD", true);
			leg1.JU_E2PickupAddressID = docAddress1.PK;
			leg1.JU_E2DeliveryAddressID = docAddress2.PK;
			leg1.JU_DisplayOrder = 1000000000;
			Factory.Save();
			AssertNoExceptionThrown("Arithmetic overflow error caused by large JU_DisplayOrder not being handled properly", () => LoadTransportProfileReport());
		}

		public void TestReport_TransportProfileReport_JU_DisplayOrderVeryLarge_StillUsesJU_DisplayOrder()
		{
			var testHelperFirst = new TransportProfileReportTestHelper("First");
			var testHelperSecond = new TransportProfileReportTestHelper("Second");
			var testHelperThird = new TransportProfileReportTestHelper("Third");
			var cartage_NoJobType = Factory.New<CommonCartage>();
			cartage_NoJobType.JJ_E3_NKJobType = string.Empty;
			new JobHeader.Loader(cartage_NoJobType).TryLoadOrCreate();
			var move = cartage_NoJobType.ContainerBookedMoves.AddNew();
			move.EW_DisplayOrder = 1;
			// Create leg2 before leg1, to ensure that JU_DisplayOrder, not order of creation, determines what is first, second, and third
			var leg2 = move.CartageLegs.AddNew();
			var leg1 = move.CartageLegs.AddNew();
			var docAddress1 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageYard, "org4", "add4", "2000", "SYDNEY", "AUSYD", true);
			var docAddress2 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageExporter, "org5", "add5", "2000", "SYDNEY", "AUSYD", true);
			var docAddress3 = Helper.CreateJobDocAddress(cartage_NoJobType, DocAddressType.LocalCartageCTO, "org6", "add6", "2000", "SYDNEY", "AUSYD", true);
			leg2.JU_E2PickupAddressID = docAddress2.PK;
			leg2.JU_E2DeliveryAddressID = docAddress3.PK;
			leg2.JU_DisplayOrder = 1000000000;
			leg1.JU_E2PickupAddressID = docAddress1.PK;
			leg1.JU_E2DeliveryAddressID = docAddress2.PK;
			leg1.JU_DisplayOrder = 1;
			testHelperFirst.Evaluate(docAddress1);
			testHelperSecond.Evaluate(docAddress2);
			testHelperThird.Evaluate(docAddress3);
			Factory.Save();
			var results = LoadTransportProfileReport();
			var result = results[0];
			testHelperFirst.AssertResultOverrided(result);
			testHelperSecond.AssertResultOverrided(result);
			testHelperThird.AssertResultOverrided(result);
		}

		public void TestTransportProfileReport_MasterBill_ChecksAdditionalReferencesFirst()
		{
			//Arrange
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var cartage = Helper.CreateInternalCartage(shipment);
			var addMab = Helper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.MasterBill, "AddMAB25");
			cartage.JJ_WaybillNumber = "123456789";
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("Master Bill / Waybill is correct", "AddMAB25", row["BillNum"].ToString());
		}

		public void TestTransportProfileReport_MasterBill_MapsFromDeclaration()
		{
			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_MasterBill] = "DEC25818";
			var shipment = Factory.New<CommonShipment>();
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_WaybillNumber = "123456789";
			cartage.JJ_ConsignmentID = "dffsdf/I";
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("Master Bill / Waybill is correct", "DEC25818", row["BillNum"].ToString());
		}

		public void TestTransportProfileReport_MasterBill_MapsFromConsol()
		{
			//Arrange
			var consol = Factory.New<CommonConsol>();
			consol.JK_MasterBillNum = "123456789";
			var shipment = consol.Shipments.AddNew();
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_ParentTableCode = consol.TablePrefix;
			cartage.JJ_ParentID = consol.PK;
			cartage.JJ_WaybillNumber = "Waybill";
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("Master Bill / Waybill is correct", "123456789", row["BillNum"].ToString());
		}

		public void TestTransportProfileReport_MasterBillIsBlankIfSameAsHouseBill()
		{
			//Arrange
			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_MasterBill] = "DEC25818";
			declaration[JobDeclarationSchema.Constants.JE_HouseBill] = "DEC25818";
			var shipment = Factory.New<CommonShipment>();
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ConsignmentID = "dffsdf/I";
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("HouseBill is correct", "DEC25818", row["Bill"].ToString());
			AssertEquals("Master Bill / Waybill is correct", "", row["BillNum"].ToString());
		}

		public void TestTransportProfileReport_MasterBill_IsBlank_IfOnlyHouseBillHasValue()
		{
			//Arrange
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "House Bill";
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_WaybillNumber = "Waybill";
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("Master Bill / Waybill is correct", "", row["BillNum"].ToString());
		}

		public void TestTransportProfileReport_MasterBill_IsSetToWayBill_IfBothBillsAreEmpty()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_WaybillNumber = "WayBillNumber";
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("Master Bill / Waybill is correct", "WayBillNumber", row["BillNum"].ToString());
		}

		public void TestTransportProfileReport_HouseBill_ChecksAdditionalReferencesFirst()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var cartage = Helper.CreateInternalCartage(shipment);
			Helper.AddAdditionalNumberRefenceToCartage(cartage, AdditionalReferenceTypes.Codes.HouseBill, "Correct HouseBill Number");
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("House Bill is correct", "Correct HouseBill Number", row["Bill"].ToString());
		}

		public void TestTransportProfileReport_HouseBill_MapsFromDeclaration()
		{
			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_HouseBill] = "DECHSB";
			declaration[JobDeclarationSchema.Constants.JE_MasterBill] = "DECMAB";
			var shipment = Factory.New<CommonShipment>();
			var cartage = Helper.CreateInternalCartage(shipment);
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ConsignmentID = "dffsdf/I";
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("House Bill is correct", "DECHSB", row["Bill"].ToString());
		}

		public void TestTransportProfileReport_HouseBill_MapsFromShipment()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSE BILL";
			var cartage = Helper.CreateInternalCartage(shipment);
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("House Bill is correct", "HOUSE BILL", row["Bill"].ToString());
		}

		public void TestTransportProfileReport_HouseBill_IsBlank_IfShipmentHouseBillIsBlank()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var cartage = Helper.CreateInternalCartage(shipment);
			Factory.Save();
			//Action
			var results = LoadTransportProfileReport();
			//Assert
			var row = results[0];
			AssertEquals("House Bill is correct", "", row["Bill"].ToString());
		}

		DynamicBusinessObjectCollection LoadTransportProfileReport()
		{
			const string sql = "SELECT * FROM Report_TransportProfileReport(@CurrentCompany, '', '', '', '')";
			var sqlParams = new ZSqlParameterCollection { { "@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK } };
			var results = new DynamicBusinessObjectCollection(Factory);
			results.Load(sql, sqlParams);
			return results;
		}

		class TransportProfileReportTestHelper
		{
			public readonly string OH_Company;
			public readonly string OA_Address1;
			public readonly string OA_Address2;
			public readonly string OA_City;
			public readonly string OA_State;
			public readonly string OA_PostCode;
			public readonly string E2_Company;
			public readonly string E2_Address1;
			public readonly string E2_Address2;
			public readonly string E2_City;
			public readonly string E2_State;
			public readonly string E2_PostCode;
			readonly string prefix;
			public string Company;
			public string Addr1;
			public string Addr2;
			public string City;
			public string State;
			public string PostCode;
			public TransportProfileReportTestHelper(string prefixValue)
			{
				prefix = prefixValue;
				OH_Company = prefix + "OH_Company";
				OA_Address1 = prefix + "OA_Address1";
				OA_Address2 = prefix + "OA_Address2";
				OA_City = prefix + "OA_City";
				OA_State = prefix + "OA_State";
				OA_PostCode = prefix + "OPC";
				E2_Company = prefix + "E2_Company";
				E2_Address1 = prefix + "E2_Address1";
				E2_Address2 = prefix + "E2_Address2";
				E2_City = prefix + "E2_City";
				E2_State = prefix + "E2_State";
				E2_PostCode = prefix + "EPC";
			}

			public void AssertResult(DynamicBusinessObject result)
			{
				SetResult(result);
				AssertEquals(prefix + "Company", OH_Company, Company);
				AssertEquals(prefix + "Address1", OA_Address1, Addr1);
				AssertEquals(prefix + "Address2", OA_Address2, Addr2);
				AssertEquals(prefix + "City", OA_City, City);
				AssertEquals(prefix + "State", OA_State, State);
				AssertEquals(prefix + "PostCode", OA_PostCode, PostCode);
			}

			public void AssertResultOverrided(DynamicBusinessObject result)
			{
				SetResult(result);
				AssertEquals(prefix + "Company", E2_Company, Company);
				AssertEquals(prefix + "Address1", E2_Address1, Addr1);
				AssertEquals(prefix + "Address2", E2_Address2, Addr2);
				AssertEquals(prefix + "City", E2_City, City);
				AssertEquals(prefix + "State", E2_State, State);
				AssertEquals(prefix + "PostCode", E2_PostCode, PostCode);
			}

			public void SetResult(DynamicBusinessObject result)
			{
				Company = ((ZString)result[prefix + "Name"]).ToString();
				Addr1 = ((ZString)result[prefix + "Add1"]).ToString();
				Addr2 = ((ZString)result[prefix + "Add2"]).ToString();
				City = ((ZString)result[prefix + "City"]).ToString();
				State = ((ZString)result[prefix + "State"]).ToString();
				PostCode = ((ZString)result[prefix + "PC"]).ToString();
			}

			public void Evaluate(OrgAddress address)
			{
				address.Header.OH_FullName = OH_Company;
				address.OA_Address1 = OA_Address1;
				address.OA_Address2 = OA_Address2;
				address.OA_City = OA_City;
				address.OA_State = OA_State;
				address.OA_PostCode = OA_PostCode;
			}

			public void Evaluate(JobDocAddress address)
			{
				address.E2_CompanyName = E2_Company;
				address.E2_Address1 = E2_Address1;
				address.E2_Address2 = E2_Address2;
				address.E2_City = E2_City;
				address.E2_State = E2_State;
				address.E2_Postcode = E2_PostCode;
			}
		}

		public void Testvw_Report_TransportMovementBase()
		{
			// 2 legs w/ cartage no job type
			var cartage_NoJobType = Factory.New<CommonCartage>();
			var move = cartage_NoJobType.ContainerBookedMoves.AddNew();
			var legEmpty = move.CartageLegs.AddNew();
			var legFull = move.CartageLegs.AddNew();
			Helper.CreateAndAssignAddresses(legEmpty, legFull, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			// 3 legs w/ cartage type
			var cartage_JobType = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE, 3);
			cartage_JobType.FirstDocAddress.E2_OA_Address = Helper.CreateOrgHeader("CTO", "CTOADDY").MainAddress.PK;
			cartage_JobType.SecondDocAddress.E2_OA_Address = Helper.CreateOrgHeader("CNE", "CNEADDY").MainAddress.PK;
			Factory.Save();
			var reportSql = "select * from Report_TransportMovement(null,null,null,null,'',null,'', '','', '') ";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(reportSql);
			AssertEquals("Report's SQL should return only 5 records as there are 5 legs.", 5, result.Count);
		}

		public void Testvw_Report_TransportMovement_JU_DisplayOrderVeryLarge_DoesNotCauseException()
		{
			var cartage_NoJobType = Factory.New<CommonCartage>();
			var move = cartage_NoJobType.ContainerBookedMoves.AddNew();
			var legEmpty = move.CartageLegs.AddNew();
			var legFull = move.CartageLegs.AddNew();
			legFull.JU_DisplayOrder = 1000000000;
			Helper.CreateAndAssignAddresses(legEmpty, legFull, DocAddressType.LocalCartageYard, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCTO);
			Factory.Save();
			var reportSql = "select * from Report_TransportMovement(null,null,null,null,'',null,'', '','', '') ";
			var result = new DynamicBusinessObjectCollection(Factory);
			AssertNoExceptionThrown("Arithmetic overflow error caused by large JU_DisplayOrder not being handled properly", () => result.Load(reportSql));
			AssertEquals("Report's SQL should return only 2 records as there are 2 legs.", 2, result.Count);
		}

		[TestDate(2016, 08, 22, 11, 52, 03, 05)]
		public void Testvw_Report_TransportMovementBase_ArrivalAndDepartureSlotDates()
		{
			var now = ZDateTime.Now;
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var destination = Helper.CreateCartage(Constants.CartageJobType.NEW_DomesticContainerizedDelivery, 1);
			var origin = Helper.CreateCartage(Constants.CartageJobType.NEW_DomesticContainerizedPickup, 1);
			var cancelledCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_DomesticContainerizedPickup, 1);
			cancelledCartage.JJ_IsCancelled = true;
			var expectedExportSlotDate = now.AddDays(2);
			var expectedImportSlotDate = now.AddDays(3);
			var expectedLineHaulSlotDate = now.AddDays(5);
			var expectedDestinationSlotDate = now.AddDays(7);
			var expectedOriginSlotDate = now.AddDays(10);
			SetSlotDateTime(export, now.AddDays(1), expectedExportSlotDate);
			SetSlotDateTime(import, expectedImportSlotDate, now.AddDays(4));
			SetSlotDateTime(destination, expectedDestinationSlotDate, now.AddDays(8));
			SetSlotDateTime(origin, now.AddDays(9), expectedOriginSlotDate);
			Factory.Save();
			var reportSql = "select * from Report_TransportMovement(null,null,null,null,'',null,'', '','', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(reportSql);
			AssertEquals("Precondition", 4, result.Count);
			AssertSlotDates(result, export, expectedExportSlotDate);
			AssertSlotDates(result, import, expectedImportSlotDate);
			AssertSlotDates(result, destination, expectedDestinationSlotDate);
			AssertSlotDates(result, origin, expectedOriginSlotDate);
		}

		void SetSlotDateTime(CommonCartage cartage, ZDateTime arrival, ZDateTime departure)
		{
			var container = cartage.BookedMovesCollection.Single().CartageLegs.Single().Container;
			container.JC_DepartureSlotDateTime = departure;
			container.JC_ArrivalSlotDateTime = arrival;
		}

		void AssertSlotDates(DynamicBusinessObjectCollection result, CommonCartage cartage, ZDateTime expectedSlotDate)
		{
			var dynamicBizO = result.Single(r => (ZGuid)r["JobCartagePK"] == cartage.PK);
			AssertEquals(expectedSlotDate.ToSmallDateTime(), ((ZDateTime)dynamicBizO["SlotDate"]));
		}

		public void Testvw_Report_TransportMovement_DifferentLocalClients()
		{
			var localClient1 = Helper.CreateOrgHeader("C1", "M1");
			var localClient2 = Helper.CreateOrgHeader("C2", "M2");
			var now = ZDateTime.Now;
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var loaderForExport = new JobHeader.Loader(export);
			var loaderForImport = new JobHeader.Loader(import);
			loaderForExport.TryLoadOrCreate();
			loaderForImport.TryLoadOrCreate();
			export.Job.LocalChargesPK = localClient1.PK;
			import.Job.LocalChargesPK = localClient2.PK;
			Factory.Save();
			var reportSqlLocalClientPK = $"select * from Report_TransportMovement('{localClient1.PK}', null, null, null, '', null, '', '','', '')";
			var localClientParameterResult = new DynamicBusinessObjectCollection(Factory);
			localClientParameterResult.Load(reportSqlLocalClientPK);
			AssertEquals(export.PK, localClientParameterResult.Single()["JobCartagePK"]);
		}

		public void Testvw_Report_TransportMovement_PickupFromOrg()
		{
			var pickupFromOrg1 = Helper.CreateOrgHeader("P1", "MP1");
			var pickupFromOrg2 = Helper.CreateOrgHeader("P2", "MP2");
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			SetupPickupOrgs(export, pickupFromOrg1);
			SetupPickupOrgs(import, pickupFromOrg2);
			Factory.Save();
			var sql = $"select * from Report_TransportMovement(null, '{pickupFromOrg1.PK}', null, null, '', null, '', '', '', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			AssertEquals(export.PK, result.Single()["JobCartagePK"]);
		}

		void SetupPickupOrgs(CommonCartage cartage, OrgHeader pickupFrom)
		{
			var move = cartage.BookedMovesCollection.AddNew();
			var leg = Factory.New<CommonCartageLeg>();
			leg.JU_PlannedPickupTime = ZDateTime.Now.AddDays(4);
			leg.JU_EW = move.PK;
			cartage.FirstDocAddress.E2_OA_Address = pickupFrom.MainAddress.PK;
			leg.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
		}

		public void Testvw_Report_TransportMovement_DeliverToOrg()
		{
			var deliveryToOrg1 = Helper.CreateOrgHeader("D1", "MD1");
			var deliveryToOrg2 = Helper.CreateOrgHeader("D2", "MD2");
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "abc";
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "XYZ";
			var sailing1 = Helper.CreateSailing(vessel1, "123", "AUSYD", "NZAKL", ZDateTime.Empty);
			var sailing2 = Helper.CreateSailing(vessel2, "456", "AUBNE", "NZAKL", ZDateTime.Empty);
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			SetupDeliveryOrg(export, deliveryToOrg1);
			SetupDeliveryOrg(import, deliveryToOrg2);
			Factory.Save();
			var sql = $"select * from Report_TransportMovement(null, null, '{deliveryToOrg1.PK}', null,'', null, '', '', '', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			AssertEquals(export.PK, result.Single()["JobCartagePK"]);
		}

		void SetupDeliveryOrg(CommonCartage cartage, OrgHeader deliveryTo)
		{
			var move = cartage.BookedMovesCollection.AddNew();
			var leg = Factory.New<CommonCartageLeg>();
			leg.JU_PlannedPickupTime = ZDateTime.Now.AddDays(4);
			leg.JU_EW = move.PK;
			cartage.SecondDocAddress.E2_OA_Address = deliveryTo.MainAddress.PK;
			leg.JU_E2DeliveryAddressID = cartage.SecondDocAddress.PK;
		}

		public void Testvw_Report_TransportMovement_CartageVessel()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "abc";
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "XYZ";
			var sailing1 = Helper.CreateSailing(vessel1, "123", "AUSYD", "NZAKL", ZDateTime.Empty);
			var sailing2 = Helper.CreateSailing(vessel2, "456", "AUBNE", "NZAKL", ZDateTime.Empty);
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			export.JJ_JX_Sailing = sailing1.PK;
			import.JJ_JX_Sailing = sailing2.PK;
			Factory.Save();
			var sql = "select * from Report_TransportMovement(null, null, null, 'abc', '', null, '', '', '', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			AssertEquals(export.PK, result.Single()["JobCartagePK"]);
		}

		public void Testvw_Report_TransportMovement_CartageVoyage()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "abc";
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "XYZ";
			var sailing1 = Helper.CreateSailing(vessel1, "123", "AUSYD", "NZAKL", ZDateTime.Empty);
			var sailing2 = Helper.CreateSailing(vessel2, "456", "AUBNE", "NZAKL", ZDateTime.Empty);
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			export.JJ_JX_Sailing = sailing1.PK;
			import.JJ_JX_Sailing = sailing2.PK;
			Factory.Save();
			var sql = "select * from Report_TransportMovement(null, null, null, null, '123', null, '', '', '', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			AssertEquals(export.PK, result.Single()["JobCartagePK"]);
		}

		public void Testvw_Report_TransportMovement_CartageType()
		{
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			Factory.Save();
			var sql = $"select * from Report_TransportMovement(null, null, null, null, '', '{Constants.CartageJobType.NEW_FCLExportPack}', '', '', '', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			AssertEquals(export.PK, result.Single()["JobCartagePK"]);
		}

		[TestDate(2016, 08, 22, 11, 52, 03, 05)]
		public void Testvw_Report_TransportMovement_PlannedPickupTime()
		{
			var localClient1 = Helper.CreateOrgHeader("C1", "M1");
			var localClient2 = Helper.CreateOrgHeader("C2", "M2");
			var pickupFromOrg1 = Helper.CreateOrgHeader("P1", "MP1");
			var pickupFromOrg2 = Helper.CreateOrgHeader("P2", "MP2");
			var deliveryToOrg1 = Helper.CreateOrgHeader("D1", "MD1");
			var deliveryToOrg2 = Helper.CreateOrgHeader("D2", "MD2");
			var now = ZDateTime.Now;
			var export = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLExportPack, 1);
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var destination = Helper.CreateCartage(Constants.CartageJobType.NEW_DomesticContainerizedDelivery, 1);
			SetupPickupAndDeliveryOrgs(export, now.AddDays(1));
			SetupPickupAndDeliveryOrgs(import, now.AddDays(3));
			Factory.Save();
			var yesterday = now.AddDays(-1);
			var tomorrow = now.AddDays(2);
			var sql = $"select * from Report_TransportMovement(null, null, null, null, '', null, '', '', '{yesterday.ToShortDateString()} 00:00:00', '{tomorrow.ToShortDateString()} 00:00:00')";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			AssertEquals(export.PK, result.Single()["JobCartagePK"]);
		}

		void SetupPickupAndDeliveryOrgs(CommonCartage cartage, ZDateTime pickupTime)
		{
			var move = cartage.BookedMovesCollection.AddNew();
			var pickupLeg = Factory.New<CommonCartageLeg>();
			pickupLeg.JU_EW = move.PK;
			pickupLeg.JU_PlannedPickupTime = pickupTime;
		}

		public void Testvw_Report_TransportMovement_ShipmentBelongsToMoreThanOneConsol_TwoArrialConsols()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			var consol1Transport = consol1.Transports.AddNew("USLAX", "SGCHG");
			var link1 = Factory.New<JobConShipLink>();
			link1.JN_JK = consol1.PK;
			link1.JN_JS = shipment.PK;
			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			var consol2Transport = consol1.Transports.AddNew("AUSYD", "SGSIN");
			var link2 = Factory.New<JobConShipLink>();
			link2.JN_JK = consol2.PK;
			link2.JN_JS = shipment.PK;
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			import.JJ_ConsignmentID = "S1/I";
			import.JJ_ParentID = shipment.PK;
			import.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();
			var sql = $"select * from Report_TransportMovement(null, null, null, null, '', null, '', '', '', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			AssertNoExceptionThrown("Should not throw an error even though shipment has two arrival consols.", () => result.Load(sql));
			AssertEquals(import.PK, result.Single()["JobCartagePK"]);
		}

		public void Testvw_Report_TransportMovement_ShipmentBelongsToMoreThanOneConsol_TwoDepartureConsols()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			var consol1Transport = consol1.Transports.AddNew("SGCHG", "USLAX");
			var link1 = Factory.New<JobConShipLink>();
			link1.JN_JK = consol1.PK;
			link1.JN_JS = shipment.PK;
			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			var consol2Transport = consol1.Transports.AddNew("SGSIN", "AUSYD");
			var link2 = Factory.New<JobConShipLink>();
			link2.JN_JK = consol2.PK;
			link2.JN_JS = shipment.PK;
			var import = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			import.JJ_ConsignmentID = "S1/I";
			import.JJ_ParentID = shipment.PK;
			import.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();
			var sql = $"select * from Report_TransportMovement(null, null, null, null, '', null, '', '', '', '')";
			var result = new DynamicBusinessObjectCollection(Factory);
			AssertNoExceptionThrown("Should not throw an error even though shipment has two departure consols.", () => result.Load(sql));
			AssertEquals(import.PK, result.Single()["JobCartagePK"]);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}

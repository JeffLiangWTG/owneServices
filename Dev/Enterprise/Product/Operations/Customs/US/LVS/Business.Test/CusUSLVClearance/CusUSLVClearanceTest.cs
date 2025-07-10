using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVClearance))]
	internal class CusUSLVClearanceTest : EnterpriseBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public void TestShouldPopulateUniqueClusterKey()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var clearance1 = factory1.New<CusUSLVClearance>();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			clearance1.OnSaving();
			var key1 = clearance1.ULH_ClusterKey;
			dbConnection.RollbackTransaction();

			var clearance2 = factory2.New<CusUSLVClearance>();
			factory2.Save();
			var key2 = clearance2.ULH_ClusterKey;

			AssertEquals(false, key1.IsEmpty);
			AssertEquals(false, key2.IsEmpty);
			AssertEquals(key1, key2);

			dbConnection.BeginTransaction();
			AssertEquals(key1, clearance1.ULH_ClusterKey);

			factory1.Save();
			AssertNotEquals(key1, clearance1.ULH_ClusterKey);
		}

		public void TestPortOfLadingRefLocoMappings()
		{
			var unLOCO1 = Factory.New<RefUNLOCO>();
			unLOCO1.RL_Code = "!ZZ11";
			unLOCO1.RL_PortName = "Crystal Lawns 1";
			var unLOCO2 = Factory.New<RefUNLOCO>();
			unLOCO2.RL_Code = "!ZZ22";
			unLOCO2.RL_PortName = "Crystal Lawns 2";
			CreateRefLocoMap("60001", "!ZZ11", USLocoMapSystemUsageList.Codes.SCK);
			CreateRefLocoMap("60002", "!ZZ11", USLocoMapSystemUsageList.Codes.SCK);
			CreateRefLocoMap("60003", "!ZZ22", USLocoMapSystemUsageList.Codes.SCK);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60003", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_RL_NKPortOfLoading = "!ZZ11";
			AssertEquals(true, clearance.ULH_PortOfLoadingIsDropEdit);
			AssertEquals(2, clearance.PortOfLadingRefLocoMappings.Count);
			Assert(clearance.PortOfLadingRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60001"));
			Assert(clearance.PortOfLadingRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60002"));

			clearance.ULH_RL_NKPortOfLoading = "!ZZ22";
			AssertEquals(false, clearance.ULH_PortOfLoadingIsDropEdit);
			AssertEquals(1, clearance.PortOfLadingRefLocoMappings.Count);
			Assert(clearance.PortOfLadingRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60003"));
		}

		public void TestPortOfDischargeRefLocoMappings()
		{
			var unLOCO = Factory.New<RefUNLOCO>();
			unLOCO.RL_Code = "!ZZ22";
			unLOCO.RL_PortName = "Crystal Lawns";
			CreateRefLocoMap("ZZ11", "!ZZ22", USLocoMapSystemUsageList.Codes.Sea);
			CreateRefLocoMap("ZZFF", "!ZZ22", USLocoMapSystemUsageList.Codes.Sea);
			CreateRefLocoMap("ZZDD", "!ZZ22", USLocoMapSystemUsageList.Codes.Air);

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZ11", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZFF", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZDD", "Test Name", startDate, endDate);
			Factory.Save();

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_RL_NKPortOfDischarge = "!ZZ22";
			AssertEquals(true, clearance.ULH_PortOfDischargeIsDropEdit);
			AssertEquals(2, clearance.PortOfDischargeRefLocoMappings.Count);
			Assert(clearance.PortOfDischargeRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "ZZ11"));
			Assert(clearance.PortOfDischargeRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "ZZFF"));

			clearance.ULH_TransportMode = "AIR";
			AssertEquals(false, clearance.ULH_PortOfDischargeIsDropEdit);
			AssertEquals(1, clearance.PortOfDischargeRefLocoMappings.Count);
			Assert(clearance.PortOfDischargeRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "ZZDD"));
		}

		void CreateRefLocoMap(string localPortCode, string locoPort, string usage)
		{
			var locoMapping = Factory.New<RefLocoMap>();
			locoMapping.RY_LocalPortCode = localPortCode;
			locoMapping.RY_RL_NKLocoPort = locoPort;
			locoMapping.RY_SystemUsage = usage;
			locoMapping.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping.RY_IsSystem = false;
		}

		public void TestSetDefaultContainerModeIfNeeded()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportModes.Road;
			clearance.ULH_ContainerMode = ContainerModes.Bulk;
			clearance.ULH_TransportMode = TransportModes.Air;
			AssertEquals("ContainerMode should default to NCT", ContainerModes.NonContainerised, clearance.ULH_ContainerMode);

			clearance.ULH_TransportMode = TransportModes.Road;
			clearance.ULH_ContainerMode = ZString.Empty;

			clearance.ULH_TransportMode = TransportModes.Truck;
			AssertEquals("ContainerMode should default to NCT", ContainerModes.NonContainerised, clearance.ULH_ContainerMode);

			clearance.ULH_TransportMode = TransportModes.Mail;
			AssertEquals("ContainerMode should be cleared", ZString.Empty, clearance.ULH_ContainerMode);

			using (USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				clearance.ULH_ContainerMode = ContainerModes.Bulk;
				clearance.ULH_TransportMode = TransportModes.Air;
				AssertEquals("ContainerMode should not change", ContainerModes.Bulk, clearance.ULH_ContainerMode);

				clearance.ULH_TransportMode = TransportModes.Truck;
				AssertEquals("ContainerMode should not change", ContainerModes.Bulk, clearance.ULH_ContainerMode);
			}
		}

		public void TestULH_EntryFilerCodeIsReadonly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(CusUSLVClearance), CusUSLVClearance.Schema.ULH_EntryFilerCode, false, a => a.IsReadOnly);
		}

		public void TestNumberFountainForClusterKeyAndJobNumber()
		{
			var shipment1 = Factory.New<CusUSLVClearance>();
			var shipment2 = Factory.New<CusUSLVClearance>();

			Factory.Save();

			AssertEquals(2, shipment1.ULH_ClusterKey);
			AssertEquals("SEC00000002", shipment1.ULH_JobNumber);
			AssertEquals(3, shipment2.ULH_ClusterKey);
			AssertEquals("SEC00000003", shipment2.ULH_JobNumber);

			var factory = new BusinessObjectFactory();

			var shipment3 = factory.New<CusUSLVClearance>();
			factory.Save();
			AssertEquals(4, shipment3.ULH_ClusterKey);
			AssertEquals("SEC00000004", shipment3.ULH_JobNumber);
		}

		public void TestHumanReadableName()
		{
			Factory.Save();
			AssertEquals("SEC00000001", shipment.HumanReadableName);
		}

		public void TestWorkflowType()
		{
			AssertEquals(WorkflowDescriptors.CusUSLVClearanceWorkflowDescriptorCode, ((IWorkflowProviderCore)shipment).WorkflowType);
		}

		public void TestULH_Calc_USTransportMode()
		{
			var type = typeof(CusUSLVClearance);
			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<ReadOnlyAttribute>(type, nameof(CusUSLVClearance.ULH_Calc_USTransportMode), false, a => a.IsReadOnly);
				AssertHasCustomAttribute<MaxLengthAttribute>(type, nameof(CusUSLVClearance.ULH_Calc_USTransportMode), false, a => a.MaxLength == 2);
			});
		}

		public void TestListAttributes()
		{
			var type = typeof(CusUSLVClearance);
			CombineAssertions(() =>
			{
				AssertListAttribute(nameof(CusUSLVClearance.ULH_TransportMode), "Lookups.ULH_TransportModeList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_ContainerMode), "Lookups.ULH_ContainerModeList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_MasterBillIssuerSCAC), "Lookups.ULH_MasterBillIssuerSCACList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_ConveyanceName), "Lookups.Vessels");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_CarrierSCAC), "Lookups.ULH_CarrierSCACList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_PortOfLoading), "Lookups.ULH_PortOfLoadingList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_PortOfEntry), "Lookups.ULH_PortOfEntryList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_PortOfDischarge), "Lookups.ULH_PortOfDischargeList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_PreparerDistrictPort), "Lookups.ULH_PreparerDistrictPortList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_RL_NKPortOfLoading), "Lookups.ULH_RL_NKPortOfLoadingList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_RL_NKPortOfDischarge), "Lookups.ULH_RL_NKPortOfDischargeList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_IORType), "Lookups.ULH_IORTypeList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_US_NKLocationOfGoods), "Lookups.ULH_US_NKLocationOfGoodsList");
				AssertListAttribute(nameof(CusUSLVClearance.ULH_US_NKCentralizedExamSite), "Lookups.ULH_US_NKCentralizedExamSiteList");

				void AssertListAttribute(string propertyName, string expectedListName)
				{
					AssertHasCustomAttribute<ListAttribute>(type, propertyName, false, a => a.ListDataSourceMember == expectedListName);
				}
			});
		}

		public void TestPropertiesForBinding()
		{
			CombineAssertions(() =>
			{
				AssertBindingPropertyValue(s => s.IsNotEmptyForBinding, true, true, true, true, true);
				AssertBindingPropertyValue(s => s.IsAirForBinding, ifAir: true);
				AssertBindingPropertyValue(s => s.IsSeaForBinding, ifSea: true);
				AssertBindingPropertyValue(s => s.IsRailForBinding, ifRail: true);
				AssertBindingPropertyValue(s => s.IsRoadOrTruckForBinding, ifTruck: true);
				AssertBindingPropertyValue(s => s.IsMailForBinding, ifMail: true);
				AssertBindingPropertyValue(s => s.IsRailOrRoadForBinding, ifRail: true, ifTruck: true);
				AssertBindingPropertyValue(s => s.IsRailOrRoadOrMailForBinding, ifRail: true, ifTruck: true, ifMail: true);

				void AssertBindingPropertyValue(Func<CusUSLVClearance, bool> propertyGetter, bool ifAir = false, bool ifSea = false, bool ifRail = false, bool ifTruck = false, bool ifMail = false, bool ifEmpty = false)
				{
					shipment.ULH_TransportMode = TransportTypeList.Codes.Air;
					AssertEquals(ifAir, propertyGetter(shipment));

					shipment.ULH_TransportMode = TransportTypeList.Codes.Sea;
					AssertEquals(ifSea, propertyGetter(shipment));

					shipment.ULH_TransportMode = TransportTypeList.Codes.Rail;
					AssertEquals(ifRail, propertyGetter(shipment));

					shipment.ULH_TransportMode = TransportTypeList.Codes.Truck;
					AssertEquals(ifTruck, propertyGetter(shipment));

					shipment.ULH_TransportMode = TransportTypeList.Codes.Mail;
					AssertEquals(ifMail, propertyGetter(shipment));

					shipment.ULH_TransportMode = ZString.Empty;
					AssertEquals(ifEmpty, propertyGetter(shipment));
				}
			});
		}

		public void TestClusterKeyCascadeSet()
		{
			var consignment = shipment.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			var pga = item.CusUSLVItemPGAs.AddNew();
			AssertEquals("pre-condition", 0, consignment.ULB_ClusterKey);
			AssertEquals("pre-condition", 0, item.ULI_ClusterKey);
			AssertEquals("pre-condition", 0, pga.ULP_ClusterKey);

			shipment.ULH_ClusterKey = 666;
			AssertEquals(666, consignment.ULB_ClusterKey);
			AssertEquals(666, item.ULI_ClusterKey);
			AssertEquals(666, pga.ULP_ClusterKey);
		}

		public void TestCascadeDelete()
		{
			var consignment = shipment.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			var pga = item.CusUSLVItemPGAs.AddNew();

			Assert("pre-condition", !consignment.IsDeleted);
			Assert("pre-condition", !item.IsDeleted);
			Assert("pre-condition", !pga.IsDeleted);

			shipment.Delete();

			Assert(consignment.IsDeleted);
			Assert(item.IsDeleted);
			Assert(pga.IsDeleted);
		}

		public void TestDefaultRegistrationNumberByType()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "MYIMPORTER";
			var code1 = importer.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			code1.OK_CustomsRegNo = "01-1546879";
			code1.OK_RN_NKCodeCountry = "US";

			var code2 = importer.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			code2.OK_CustomsRegNo = "111111";
			code2.OK_RN_NKCodeCountry = "US";

			var code3 = importer.CustomsCodes.AddNew();
			code3.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			code3.OK_CustomsRegNo = "333-44-5112";
			code3.OK_RN_NKCodeCountry = "US";
			Factory.Save();

			shipment.ULH_OH_Importer = importer.PK;

			shipment.ULH_IORType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			AssertEquals("01-1546879", shipment.ULH_IORReference);

			shipment.ULH_IORType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			AssertEquals("111111", shipment.ULH_IORReference);

			shipment.ULH_IORType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals("333-44-5112", shipment.ULH_IORReference);

			shipment.ULH_IORType = "";
			AssertEquals("", shipment.ULH_IORReference);

			var importerCanNotMatch = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			shipment.ULH_OH_Importer = importerCanNotMatch.PK;
			AssertEquals("", shipment.ULH_IORReference);

			shipment.ULH_IORType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			shipment.ULH_OH_Importer = importer.PK;
			AssertEquals("01-1546879", shipment.ULH_IORReference);
		}

		public void TestDefaultRegistrationTypeByImporter()
		{
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "MYIMPORTER1";
			var code1 = importer1.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			code1.OK_CustomsRegNo = "01-1546879";
			code1.OK_RN_NKCodeCountry = "US";

			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "MYIMPORTER2";
			var code2 = importer2.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			code2.OK_CustomsRegNo = "111111";
			code2.OK_RN_NKCodeCountry = "US";

			var importer3 = Factory.New<OrgHeader>();
			importer3.OH_Code = "MYIMPORTER3";
			var code3 = importer3.CustomsCodes.AddNew();
			code3.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			code3.OK_CustomsRegNo = "333-44-5112";
			code3.OK_RN_NKCodeCountry = "US";

			var importer4 = Factory.New<OrgHeader>();
			importer4.OH_Code = "MYIMPORTER4";
			var code4 = importer4.CustomsCodes.AddNew();
			code4.OK_CodeType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			code4.OK_CustomsRegNo = "0001111";
			code4.OK_RN_NKCodeCountry = "US";
			Factory.Save();

			shipment.ULH_OH_Importer = importer1.PK;
			AssertEquals("EIN Type", shipment.ULH_IORType, "EIN");
			AssertEquals("EIN Num", shipment.ULH_IORReference, "01-1546879");

			shipment.ULH_IORType = "";
			shipment.ULH_IORReference = "";
			shipment.ULH_OH_Importer = importer2.PK;
			AssertEquals("CBN Type", shipment.ULH_IORType, "CBN");
			AssertEquals("CBN Num", shipment.ULH_IORReference, "111111");

			shipment.ULH_IORType = "";
			shipment.ULH_IORReference = "";
			shipment.ULH_OH_Importer = importer3.PK;
			AssertEquals("SSN Type", shipment.ULH_IORType, "SSN");
			AssertEquals("SSN Num", shipment.ULH_IORReference, "333-44-5112");

			shipment.ULH_IORType = "";
			shipment.ULH_IORReference = "";
			shipment.ULH_OH_Importer = importer4.PK;
			AssertEquals("FEI Type", shipment.ULH_IORType, "");
			AssertEquals("FEI Num", shipment.ULH_IORReference, "");

			shipment.ULH_IORType = "EIN";
			shipment.ULH_IORReference = "01-1546869";
			shipment.ULH_OH_Importer = importer1.PK;
			AssertEquals(shipment.ULH_IORType, "EIN");
			AssertEquals(shipment.ULH_IORReference, "01-1546879");

			shipment.ULH_OH_Importer = importer2.PK;
			AssertEquals(shipment.ULH_IORType, "CBN");
			AssertEquals(shipment.ULH_IORReference, "111111");

			shipment.ULH_OH_Importer = importer4.PK;
			AssertEquals(shipment.ULH_IORType, "CBN");
			AssertEquals(shipment.ULH_IORReference, "111111");
		}

		public void TestMatckingKey_WhenUseCodeIsHVL_IsReadOnly()
		{
			var clearance1 = Factory.NewWithValidTestData<CusUSLVClearance>();
			var clearance2 = Factory.NewWithValidTestData<CusUSLVClearance>();

			clearance1.ULH_UseCode = LVSConstants.ETailUseCode;
			clearance2.ULH_UseCode = "TES";

			CombineAssertions("Matching Key should be read only when Use Code is HVL", () =>
			{
				AssertEquals("UseCode: HVL", true, clearance1.ULH_MatchingKeyInfo.ReadOnly);
				AssertEquals("UseCode: TES", false, clearance2.ULH_MatchingKeyInfo.ReadOnly);
			});
		}

		public void TestIEDocsProvider()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			var docProvider = clearance as IEDocsProvider;

			AssertNotNull("Precondition: Clearance should implement IEDocsProvider", docProvider);

			AssertNotNull(docProvider.DocManagerInfo);
			CombineAssertions("DocManagerInfo should be set correctly", () =>
			{
				AssertEquals("DocManager Code:", DocManagerCodes.USLowValueEntries, docProvider.DocManagerInfo.DocManagerCode);
				AssertContainsExactElementsInAnyOrder("Related objects:", new[] { consignment }, docProvider.DocManagerInfo.RelatedObjects);
			});
		}

		public void TestDeleteOrgWithRequiredDocumentsDeleted()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var document = clearance.RequiredDocuments.AddNew();
			Factory.Save();

			AssertEquals("Required documents should contain 1 records", 1, clearance.RequiredDocuments.Count);
			AssertNotNull("Required document should exist", new BusinessObjectFactory().Load<JobRequiredDocument>(document.PK));

			clearance.Delete();
			Factory.Save();

			AssertNull("Required documents should be deleted", new BusinessObjectFactory().Load<JobRequiredDocument>(document.PK));
		}

		public void TestIHaveRequiredDocuments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "TestMaster";
			Assert(clearance is IHaveRequiredDocuments);
			var iHaveRequiredDocuments = (IHaveRequiredDocuments)clearance;

			AssertEquals(ZString.Empty, iHaveRequiredDocuments.HouseBill);
			AssertEquals("TestMaster", iHaveRequiredDocuments.MasterBill);
			AssertNull(iHaveRequiredDocuments.ExportBroker);
			AssertEquals(iHaveRequiredDocuments.TableCode, CusUSLVClearanceSchema.Constants.Prefix);
			AssertEquals(0, iHaveRequiredDocuments.AdditionalRefTypes.Count);
			AssertEquals("RequiredDocuments", typeof(JobRequiredDocumentDependentCollection), iHaveRequiredDocuments.RequiredDocuments.GetType());
			AssertEquals(clearance, iHaveRequiredDocuments.UltimateDocumentParent);
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValueWithEnvCurrentUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "11111222223333388";
			staff.GS_Code = "ANN";
			staff.GS_LoginName = "ANN";
			staff.GS_FullName = "ANNE666666ANNE666666ANNE666666ANNE666666$$";
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var clearance = Factory.New<CusUSLVClearance>();
				AssertEquals("Max length 15", "111112222233333", clearance.ULH_ContactPhone);

				var defaultFilerContactInfo = new DefaultFilerContactInformation();
				defaultFilerContactInfo.ContactName = "";
				defaultFilerContactInfo.ContactPhone = "";

				using (USCustomsDataRegistry.Instance.DefaultFilerContactInformation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, defaultFilerContactInfo))
				{
					var clearance2 = Factory.New<CusUSLVClearance>();
					AssertEquals("Max length 40", "ANNE666666ANNE666666ANNE666666ANNE666666", clearance2.ULH_ContactName);
					AssertEquals("Max length 15", "111112222233333", clearance2.ULH_ContactPhone);
				}
			}
		}
		#region Default Value Test

		public void TestDefaultValue_ULH_MasterBillIssuerSCAC()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var issuerSCAC = Factory.New<USCarrierCombined>();
			issuerSCAC.UI_Code = "DU!!";
			issuerSCAC.UI_AirwayBillPrefix = "DUM";
			clearance.ULH_MasterBillIssuerSCAC = "D!!M";
			AssertNullOrEmpty(clearance.ULH_MasterBill);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Air;
			AssertNullOrEmpty(clearance.ULH_MasterBill);

			clearance.ULH_MasterBillIssuerSCAC = issuerSCAC.UI_Code;
			AssertEquals("Change to DUM when ULH_MasterBill is empty", "DUM", clearance.ULH_MasterBill);

			clearance.ULH_MasterBillIssuerSCAC = ZString.Empty;
			clearance.ULH_MasterBill = "AA";
			clearance.ULH_MasterBillIssuerSCAC = issuerSCAC.UI_Code;
			AssertEquals("Not change to DUM when ULH_MasterBill length is not 3 or empty", "AA", clearance.ULH_MasterBill);

			clearance.ULH_MasterBill = "AAA";
			clearance.ULH_TransportMode = TransportTypeList.Codes.Auto;
			clearance.ULH_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Changed to DUM when ULH_MasterBill length is 3", "DUM", clearance.ULH_MasterBill);

			clearance.ULH_MasterBill = ZString.Empty;
			var issuerSCAC2 = Factory.New<USCarrierCombined>();
			issuerSCAC2.UI_Code = "DU!!";
			issuerSCAC2.UI_AirwayBillPrefix = "OOO";
			clearance.ULH_MasterBillIssuerSCAC = ZString.Empty;
			clearance.ULH_MasterBillIssuerSCAC = issuerSCAC2.UI_Code;
			AssertEquals("Not set when there are duplicates code", ZString.Empty, clearance.ULH_MasterBill);
		}

		public void TestRemoveDashAndSpacesForMasterBillWhenTransportModeIsAir()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "A- -B";
			AssertEquals("A- -B", clearance.ULH_MasterBill);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Air;
			clearance.ULH_MasterBill = "A- -B";
			AssertEquals("AB", clearance.ULH_MasterBill);

			clearance.ULH_MasterBill = "C- D -D";
			AssertEquals("CDD", clearance.ULH_MasterBill);
		}

		public void TestDefaultValue_PortOfLoadingAndRL_NKPortOfLoading()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60267", "60267 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "3786", "3786 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			CreateLocoMapIfNotExists("3786", "USCHI", USLocoMapSystemUsageList.Codes.SCK, true);
			CreateLocoMapIfNotExists("60267", "AUSYD", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("45657", "USLAX", USLocoMapSystemUsageList.Codes.All, true);

			clearance.ULH_RL_NKPortOfLoading = "USCHI";
			AssertEquals("3786", clearance.ULH_PortOfLoading);

			clearance.ULH_RL_NKPortOfLoading = ZString.Empty;
			clearance.ULH_PortOfLoading = ZString.Empty;
			clearance.ULH_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("60267", clearance.ULH_PortOfLoading);

			clearance.ULH_PortOfLoading = ZString.Empty;
			clearance.ULH_PortOfLoading = "3786";
			clearance.ULH_RL_NKPortOfLoading = ZString.Empty;
			clearance.ULH_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Set Default Value When Value Is Not Empty", "60267", clearance.ULH_PortOfLoading);

			clearance.ULH_RL_NKPortOfLoading = ZString.Empty;
			clearance.ULH_PortOfLoading = ZString.Empty;
			clearance.ULH_PortOfLoading = "3786";
			AssertEquals("USCHI", clearance.ULH_RL_NKPortOfLoading);

			clearance.ULH_PortOfLoading = ZString.Empty;
			clearance.ULH_RL_NKPortOfLoading = ZString.Empty;
			clearance.ULH_RL_NKPortOfLoading = "AUSYD";
			clearance.ULH_PortOfLoading = "3786";
			AssertEquals("Set Default Value When Value Is Not Empty", "USCHI", clearance.ULH_RL_NKPortOfLoading);

			clearance.ULH_PortOfLoading = ZString.Empty;
			clearance.ULH_RL_NKPortOfLoading = ZString.Empty;
			clearance.ULH_RL_NKPortOfLoading = "USLAX";
			AssertEquals("Not Set Default Value When Not SCK", ZString.Empty, clearance.ULH_PortOfLoading);
		}

		public void TestDefaultValue_PortOfDischargeAndRL_NK_PortOfDischarge()
		{
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3786", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2704", "Test Name", startDate, endDate);
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			CreateLocoMapIfNotExists("3786", "USAAA", USLocoMapSystemUsageList.Codes.All, true);
			CreateLocoMapIfNotExists("2704", "USBBB", USLocoMapSystemUsageList.Codes.All, true);

			clearance.ULH_RL_NKPortOfDischarge = "USAAA";
			AssertEquals("3786", clearance.ULH_PortOfDischarge);

			clearance.ULH_RL_NKPortOfDischarge = ZString.Empty;
			clearance.ULH_PortOfDischarge = ZString.Empty;
			clearance.ULH_RL_NKPortOfDischarge = "USBBB";
			AssertEquals("2704", clearance.ULH_PortOfDischarge);

			clearance.ULH_PortOfDischarge = ZString.Empty;
			clearance.ULH_PortOfDischarge = "3786";
			clearance.ULH_RL_NKPortOfDischarge = ZString.Empty;
			clearance.ULH_RL_NKPortOfDischarge = "USBBB";
			AssertEquals("Set Default Value When Value Is Not Empty", "2704", clearance.ULH_PortOfDischarge);

			clearance.ULH_RL_NKPortOfDischarge = ZString.Empty;
			clearance.ULH_PortOfDischarge = ZString.Empty;
			clearance.ULH_PortOfDischarge = "3786";
			AssertEquals("USAAA", clearance.ULH_RL_NKPortOfDischarge);

			clearance.ULH_PortOfDischarge = ZString.Empty;
			clearance.ULH_RL_NKPortOfDischarge = ZString.Empty;
			clearance.ULH_RL_NKPortOfDischarge = "USBBB";
			clearance.ULH_PortOfDischarge = "3786";
			AssertEquals("Set Default Value When Value Is Not Empty", "USAAA", clearance.ULH_RL_NKPortOfDischarge);
		}

		public void TestDefaultValue_ULH_EntryFilerCode()
		{
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "XJ5";
			using (USCustomsDataRegistry.Instance.EntryFiler.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, entryFiler))
			{
				var clearance = Factory.New<CusUSLVClearance>();
				AssertEquals("XJ5", clearance.ULH_EntryFilerCode);
			}

			entryFiler.EntryFilerCode = ZString.Empty;
			using (USCustomsDataRegistry.Instance.EntryFiler.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, entryFiler))
			{
				var clearance = Factory.New<CusUSLVClearance>();
				AssertEquals(ZString.Empty, clearance.ULH_EntryFilerCode);
			}
		}

		public void TestDefaultValue_ULH_RemoteLocationFiling()
		{
			var registryItemsCollection = new BranchDistrictPortCollection(new ZArchitecture.Environment.FallbackLevel(GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			GlbStaff.CurrentUser.GS_GB_HomeBranch = Guid.Empty;

			using (USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemsCollection))
			{
				clearance.ULH_RemoteLocationFiling = true;
				clearance.ULH_PortOfEntry = "2127";
				AssertEquals("Set to false when HomeBranch is null", false, clearance.ULH_RemoteLocationFiling);

				clearance.ULH_RemoteLocationFiling = true;
				clearance.ULH_PortOfEntry = "3901";
				AssertEquals("Set to false when HomeBranch is null", false, clearance.ULH_RemoteLocationFiling);

				GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				clearance.ULH_RemoteLocationFiling = true;
				clearance.ULH_PortOfEntry = ZString.Empty;
				AssertEquals("Set to false when ULH_PortOfEntry is empty", false, clearance.ULH_RemoteLocationFiling);

				clearance.ULH_RemoteLocationFiling = true;
				clearance.ULH_PortOfEntry = "1234";
				AssertEquals("Set to false when BranchDistrictPortRelationship is empty", false, clearance.ULH_RemoteLocationFiling);
			}

			var item1 = registryItemsCollection.AddNew();
			item1.PortCode = "21";
			item1.BranchPK = GlbBranch.CurrentBranch.PK;

			using (USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemsCollection))
			{
				clearance.ULH_RemoteLocationFiling = true;
				clearance.ULH_PortOfEntry = "2127";
				AssertEquals("Set to false when matching to BranchDistrictPortRelationship", false, clearance.ULH_RemoteLocationFiling);

				clearance.ULH_PortOfEntry = "3901";
				AssertEquals("Set to true when not matching to BranchDistrictPortRelationship", true, clearance.ULH_RemoteLocationFiling);

				var clearance2 = Factory.New<CusUSLVClearance>();
				clearance2.ULH_TransportMode = TransportTypeList.Codes.Truck;
				clearance2.ULH_RemoteLocationFiling = true;
				clearance2.ULH_PortOfDischarge = "3901";
				clearance2.ULH_RemoteLocationFiling = false;
				AssertEquals("No loop exception when setting ULH_RemoteLocationFiling or ULH_PortOfEntry", "3901", clearance2.ULH_PortOfEntry);
				AssertEquals(true, clearance2.ULH_RemoteLocationFiling);
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestDefaultValue_ULH_ArrivalDate()
		{
			CreateLocoMapIfNotExists("3786", "USCHI", USLocoMapSystemUsageList.Codes.All, true);
			CreateLocoMapIfNotExists("2704", "USLAX", USLocoMapSystemUsageList.Codes.All);

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_PortOfDischarge = "3786";
			clearance.ULH_PortOfEntry = "2704";
			clearance.ULH_DischargeDate = ZDate.Today;
			Assert(clearance.ULH_EntryDate.IsEmpty);

			clearance.ULH_PortOfEntry = "3786";
			AssertEquals(ZDate.Today, clearance.ULH_EntryDate);

			clearance.ULH_EntryDate = ZDate.Today;
			clearance.ULH_DischargeDate = ZDate.Today.AddDays(1);
			AssertNotEquals("Not set to same when date already has value", clearance.ULH_DischargeDate, clearance.ULH_EntryDate);

			clearance.ULH_EntryDate = ZDate.Empty;
			clearance.ULH_DischargeDate = ZDate.Today.AddDays(2);
			AssertEquals(ZDate.Today.AddDays(2), clearance.ULH_DischargeDate);
			AssertEquals("Set to same when date has no value", clearance.ULH_DischargeDate, clearance.ULH_EntryDate);
		}

		[TestDate(2019, 11, 11)]
		public void TestDefaultValue_DefaultDates_TRK()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			Assert(clearance.ULH_EntryDate.IsEmpty);
			Assert(clearance.ULH_DepartureDate.IsEmpty);
			Assert(clearance.ULH_DischargeDate.IsEmpty);

			var today = ZDate.Today;
			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals(today, clearance.ULH_EntryDate);
			AssertEquals(today, clearance.ULH_DepartureDate);
			AssertEquals(today, clearance.ULH_DischargeDate);
		}

		public void TestDefaultValue_DefaultEntryPort_TRK()
		{
			CreateLocoMapIfNotExists("3786", "USCHI", USLocoMapSystemUsageList.Codes.All, true);
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_RemoteLocationFiling = true;
			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			clearance.ULH_PortOfDischarge = "3786";
			AssertNotEquals("3786", clearance.ULH_PortOfEntry);

			clearance.ULH_RemoteLocationFiling = false;
			AssertEquals("3786", clearance.ULH_PortOfEntry);

			clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertNotEquals("3786", clearance.ULH_PortOfEntry);

			clearance.ULH_PortOfDischarge = "3786";
			AssertEquals("3786", clearance.ULH_PortOfEntry);

			clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_PortOfDischarge = "3786";
			AssertNotEquals("3786", clearance.ULH_PortOfEntry);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("3786", clearance.ULH_PortOfEntry);

			clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_PortOfEntry = "1234";
			clearance.ULH_PortOfDischarge = "3786";
			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("Not change when already has value", "1234", clearance.ULH_PortOfEntry);
		}

		public void TestDefaultValue_DefaultIssuerSCACCode_TRK()
		{
			var scac1 = Factory.New<USCarrierCombined>();
			scac1.UI_Code = "Z!!1";
			var scac2 = Factory.New<USCarrierCombined>();
			scac2.UI_Code = "Z!!2";

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertNotEquals("Z!!1", clearance.ULH_MasterBillIssuerSCAC);

			clearance.ULH_CarrierSCAC = "Z!!1";
			AssertEquals("Z!!1", clearance.ULH_MasterBillIssuerSCAC);

			clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_CarrierSCAC = "Z!!1";
			AssertNotEquals("Z!!1", clearance.ULH_MasterBillIssuerSCAC);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("Z!!1", clearance.ULH_MasterBillIssuerSCAC);

			clearance.ULH_CarrierSCAC = "Z!!2";
			AssertEquals("Not change when already has value", "Z!!1", clearance.ULH_MasterBillIssuerSCAC);
		}

		public void TestDefaultValue_ULB_NonAMSIndicator()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			AssertEquals(false, consignment1.ULB_NonAMSIndicator);
			AssertEquals(false, consignment2.ULB_NonAMSIndicator);

			clearance.ULH_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals(true, consignment1.ULB_NonAMSIndicator);
			AssertEquals(true, consignment2.ULB_NonAMSIndicator);

			var newBizO = clearance.CusUSLVConsignments.AddNew();
			AssertEquals(true, newBizO.ULB_NonAMSIndicator);
			AssertEquals(false, newBizO.HasChanges);

			consignment1.ULB_NonAMSIndicator = false;
			clearance.ULH_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals(true, consignment1.ULB_NonAMSIndicator);

			consignment1.ULB_NonAMSIndicator = true;
			consignment2.ULB_NonAMSIndicator = true;
			clearance.ULH_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(false, consignment1.ULB_NonAMSIndicator);
			AssertEquals(false, consignment2.ULB_NonAMSIndicator);

			newBizO = clearance.CusUSLVConsignments.AddNew();
			AssertEquals(false, newBizO.ULB_NonAMSIndicator);
			AssertEquals(false, newBizO.HasChanges);
		}

		#endregion

		public void TestInitCusUSLVConsignmentsToSend()
		{
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment3 = shipment.CusUSLVConsignments.AddNew();
			var consignment4 = shipment.CusUSLVConsignments.AddNew();
			consignment4.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Add);
			AssertContainsExactElementsInAnyOrder(new[] { consignment3 }, shipment.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment));

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Delete);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, shipment.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment));

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Replace);
			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, shipment.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment));

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Update);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, shipment.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment));
		}

		public void TestShowOnlySelectedConsignmentBill()
		{
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Replace);

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(shipment, consignment2);
			var selectedConsignment = clearanceWrapper.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment);

			AssertContainsExactElementsInAnyOrder(new[] { consignment2 }, selectedConsignment);
		}

		public void TestShowAllConsignmentBills_WhenNoConsignmentsAreSelected()
		{
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Replace);

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(shipment);
			var selectedConsignment = clearanceWrapper.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment);

			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, selectedConsignment);
		}

		public void TestScheduleDProperties_MaxLength()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			CombineAssertions("Schedule D Port Codes should be max 4 characters", () =>
			{
				AssertEquals("PortOfEntry", 4, clearance.ULH_PortOfEntryInfo.MaxLength);
				AssertEquals("PortOfDischarge", 4, clearance.ULH_PortOfDischargeInfo.MaxLength);
				AssertEquals("PreparerDistrictPort", 4, clearance.ULH_PreparerDistrictPortInfo.MaxLength);
			});
		}

		public void TestPreparerOfficeCode_WhenRemoteFiling_DefaultsToRegistryValue()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			using (USCustomsDataRegistry.Instance.PreparerOfficeCode.SetTemporaryValue(Guid.Empty, clearance.RegistryBranchPK, Guid.Empty, "AB"))
			{
				clearance.ULH_RemoteLocationFiling = true;
				AssertEquals("Value should have been defaulted from registry", "AB", clearance.ULH_PreparerOfficeCode);
			}
		}

		public void TestPreparerOfficeCode_WhenNotRemoteFiling_ClearsValue()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_RemoteLocationFiling = true;
			clearance.ULH_PreparerOfficeCode = "AB";

			clearance.ULH_RemoteLocationFiling = false;
			AssertEquals("Value should have been cleared", string.Empty, clearance.ULH_PreparerOfficeCode);
		}

		public void TestPreparerOfficeCode_WhenNotRemoteFiling_IsReadOnly()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			clearance.ULH_RemoteLocationFiling = false;
			AssertEquals("RemoteLocationFiling Diabled: PreparerOfficeCode should be read-only", true, clearance.ULH_PreparerOfficeCodeInfo.ReadOnly);

			clearance.ULH_RemoteLocationFiling = true;
			AssertEquals("RemoteLocationFiling Enabled: PreparerOfficeCode should not be read-only", false, clearance.ULH_PreparerOfficeCodeInfo.ReadOnly);
		}

		public void TestPreparerDistrictPort_WhenRemoteFiling_DefaultsToRegistryValue()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			using (USCustomsDataRegistry.Instance.PreparerDistrictPort.SetTemporaryValue(Guid.Empty, clearance.RegistryBranchPK, Guid.Empty, "1234"))
			{
				clearance.ULH_RemoteLocationFiling = true;
				AssertEquals("Value should have been defaulted from registry", "1234", clearance.ULH_PreparerDistrictPort);
			}
		}

		public void TestPreparerDistrictPort_WhenNotRemoteFiling_ClearsValue()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_RemoteLocationFiling = true;
			clearance.ULH_PreparerDistrictPort = "1234";

			clearance.ULH_RemoteLocationFiling = false;
			AssertEquals("Value should have been cleared", string.Empty, clearance.ULH_PreparerDistrictPort);
		}

		public void TestPreparerDistrictPort_WhenNotRemoteFiling_IsReadOnly()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			clearance.ULH_RemoteLocationFiling = false;
			AssertEquals("RemoteLocationFiling Diabled: PreparerDistrictPort should be read-only", true, clearance.ULH_PreparerDistrictPortInfo.ReadOnly);

			clearance.ULH_RemoteLocationFiling = true;
			AssertEquals("RemoteLocationFiling Enabled: PreparerDistrictPort should not be read-only", false, clearance.ULH_PreparerDistrictPortInfo.ReadOnly);
		}

		public void TestBranch_WhenAnyConsignmentCSAReceived_IsReadOnly()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			var consignment3 = clearance.CusUSLVConsignments.AddNew();

			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			consignment2.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			consignment3.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			AssertEquals("No CSA Consignments, should not be read-only", false, clearance.ULH_GBInfo.ReadOnly);

			consignment2.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			consignment3.ULB_MessageStatus = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			AssertEquals("2 CSA Consignments, should be read-only", true, clearance.ULH_GBInfo.ReadOnly);

			consignment2.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			AssertEquals("1 CSA Consignments, should still be read-only", true, clearance.ULH_GBInfo.ReadOnly);

			consignment3.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			AssertEquals("No more CSA Consignments, should be read-only again", false, clearance.ULH_GBInfo.ReadOnly);
		}

		public void TestSetDefaultValues_ForContactName_AndContactPhone_WhenRegistryPopulated()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "Ian Ng";
			staff.GS_LoginName = "INN";
			staff.GS_IsSystemAccount = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var registry = USCustomsDataRegistry.Instance.DefaultFilerContactInformation.Value;

				CombineAssertions("Preconditions", () =>
				{
					AssertEquals(Env.CurrentUser, staff);
					AssertEquals(Env.CurrentUser.WorkPhone, staff.GS_WorkPhone);
					AssertEquals(Env.CurrentUser.FullName, staff.GS_FullName);
				});

				var defaultFilerContactInfo = new DefaultFilerContactInformation();
				defaultFilerContactInfo.ContactName = "Not Ian Ng";
				defaultFilerContactInfo.ContactPhone = "0487654321";

				using (USCustomsDataRegistry.Instance.DefaultFilerContactInformation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, defaultFilerContactInfo))
				{
					registry = USCustomsDataRegistry.Instance.DefaultFilerContactInformation.Value;
					var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
					Factory.Save();

					AssertEquals("clearance contact name should be same as name set in registry", registry.ContactName, clearance.ULH_ContactName);
					AssertEquals("clearance contact number should be same as contact number set in registry", registry.ContactName, clearance.ULH_ContactName);
				}

				defaultFilerContactInfo.ContactName = ZString.Empty;
				defaultFilerContactInfo.ContactPhone = ZString.Empty;
				using (USCustomsDataRegistry.Instance.DefaultFilerContactInformation.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, defaultFilerContactInfo))
				{
					registry = USCustomsDataRegistry.Instance.DefaultFilerContactInformation.Value;
					var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
					Factory.Save();

					AssertEquals("clearance contact name should be same as name set in registry", "", clearance.ULH_ContactName);
					AssertEquals("clearance contact number should be same as contact number set in registry", "", clearance.ULH_ContactName);
				}
			}
		}

		public void TestSetDefaultValues_ForContactName_AndContactPhone_WhenRegistryIsEmpty()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "Ian Ng";
			staff.GS_LoginName = "ING";
			staff.GS_IsSystemAccount = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var registry = USCustomsDataRegistry.Instance.DefaultFilerContactInformation;
				var defaultFilerContactInfo = new DefaultFilerContactInformation();
				defaultFilerContactInfo.ContactName = ZString.Empty;
				defaultFilerContactInfo.ContactPhone = ZString.Empty;

				using (registry.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, defaultFilerContactInfo))
				{
					AssertEquals(Env.CurrentUser, staff);
					AssertEquals(Env.CurrentUser.WorkPhone, staff.GS_WorkPhone);
					AssertEquals(Env.CurrentUser.FullName, staff.GS_FullName);

					Assert(registry.Value.ContactName.IsEmpty);
					Assert(registry.Value.ContactPhone.IsEmpty);

					var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
					Factory.Save();

					AssertEquals("clearance contact name should be same as name of current user", Env.CurrentUser.FullName, clearance.ULH_ContactName);
					AssertEquals("clearance contact name should be same as phone of current user", Env.CurrentUser.WorkPhone, clearance.ULH_ContactPhone);
				}

				defaultFilerContactInfo.ContactName = ZString.Empty;
				defaultFilerContactInfo.ContactPhone = "555556666699999";
				using (registry.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, defaultFilerContactInfo))
				{
					Assert(registry.Value.ContactName.IsEmpty);
					Assert(!registry.Value.ContactPhone.IsEmpty);

					var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
					Factory.Save();

					AssertEquals("clearance contact name should be same as name of current user", string.Empty, clearance.ULH_ContactName);
					AssertEquals("clearance contact name should be same as phone of current user", "555556666699999", clearance.ULH_ContactPhone);
				}
			}
		}

		public void TestHasEntryTypeInformalFreeDutiableOnAnyConsignment()
		{
			var clearance = Factory.New<CusUSLVClearance>();

			var consignment = clearance.CusUSLVConsignments.AddNew();
			Assert(!clearance.HasEntryTypeInformalFreeDutiableOnAnyConsignment);

			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			Assert(clearance.HasEntryTypeInformalFreeDutiableOnAnyConsignment);
		}

		public void TestConsolidatedSummaryDeclarations_WhenNoMatchingDeclaration_ShouldReturnEmptyList()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "55555";

			var declarationWithoutMatchingLog = Factory.New<JobDeclaration>();
			declarationWithoutMatchingLog.JE_MasterBill = "55555";
			declarationWithoutMatchingLog.JE_DeclarationReference = "B0001";

			var cancelledDeclaration = Factory.New<JobDeclaration>();
			cancelledDeclaration.JE_MasterBill = "55555";
			cancelledDeclaration.JE_DeclarationReference = "B0002";
			cancelledDeclaration.JE_IsCancelled = true;

			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "B0002"));
			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "INVALID"));
			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, ""));
			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Assigned, "B0001"));
			clearance.Logs.AddNew(AutoEvents.CustomsEntryStatus, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "B0001"));

			AssertEquals("Should return empty list", 0, clearance.ConsolidatedSummaryDeclarations.Count);
		}

		public void TestConsolidatedSummaryDeclarations_WhenMatchingDeclarations_ShouldReturnMatchingDeclarations()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "55555";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterBill = "11111";
			declaration1.JE_DeclarationReference = "B0001";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MasterBill = "22222";
			declaration2.JE_DeclarationReference = "B0002";

			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "B0001"));
			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "B0002"));

			AssertEquals(2, clearance.ConsolidatedSummaryDeclarations.Count);
		}

		public void TestHasConsolidatedSummaryDeclarations_WhenNoMatchingDeclaration_ShouldReturnFalse()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "55555";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "55555";

			Assert(!clearance.HasConsolidatedSummaryDeclarations);
		}

		public void TestHasConsolidatedSummaryDeclarations_WhenMatchingDeclarations_ShouldReturnTrue()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_MasterBill = "11111";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterBill = "55555";
			declaration1.JE_DeclarationReference = "B0001";

			clearance.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "B0001"));

			Assert(clearance.HasConsolidatedSummaryDeclarations);
		}

		#region Implementation

		CusUSLVClearance shipment;

		protected override void SetUp()
		{
			shipment = Factory.NewWithValidTestData<CusUSLVClearance>();
			base.SetUp();
		}

		void CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = false)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			if (isSystem)
			{
				codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);
			}

			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
				Factory.Save();
			}

			Factory.Save();
		}

		#endregion
	}
}

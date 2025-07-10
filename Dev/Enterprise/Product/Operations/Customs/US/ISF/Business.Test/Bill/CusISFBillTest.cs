using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFBill))]
	sealed class CusISFBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var header = Factory.New<CusISFHeader>();
			var bill = header.ReferenceDatas.AddNew();
			AssertEquals(true, bill.BB_CustomsStatusInfo.ReadOnly);
			AssertEquals(true, bill.BB_MatchDateInfo.ReadOnly);
		}

		public void TestBB_FirstMatchedDate()
		{
			var header = Factory.New<CusISFHeader>();
			var bill = header.ReferenceDatas.AddNew();
			bill.BB_MatchDate = new ZDateTime(2014, 1, 28);
			AssertEquals(new ZDateTime(2014, 1, 28), bill.BB_FirstMatchedDate);
			bill.BB_MatchDate = new ZDateTime(2014, 1, 29);
			AssertEquals(new ZDateTime(2014, 1, 28), bill.BB_FirstMatchedDate);
			Assert(bill.BB_FirstMatchedDateInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestAccessingIWorkflowTriggerEventSourceDoesNotThrowException()
		{
			var bill = Factory.New<CusISFBill>();
			bill.BB_BillNum = "BB_BillNum";
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill.BB_CustomsStatus = DispositionCodeList.Codes.S1;
		}

		public void TestIsFullNameOfISFImporter()
		{
			var header = Factory.New<CusISFHeader>();
			var bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = BillTypeList.Codes.FullNameOfISFImporter;
			AssertEquals(true, bill.IsFullNameOfISFImporter);
			bill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			AssertEquals(false, bill.IsFullNameOfISFImporter);
		}

		public void TestWorkflowTriggerForEventsOnBills()
		{
			var header = Factory.New<CusISFHeader>();
			var bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = "BM";
			bill1.BB_BillNum = "HW23423";
			var task1 = header.WorkflowItems.Triggers.AddNew();
			task1.P9_Description = "CLEAR ISF ADD";
			task1.TriggerConditions.TriggerEventCode = Events.MessageStatusChange.Code;
			task1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task1.TriggerConditions.TriggerConditionValue = "CIO";
			var notification1 = task1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = "NTF";
			notification1.PQ_TriggerParty = "EML";
			notification1.PQ_EmailAddr = "dummy1@where.com";
			var task2 = header.WorkflowItems.Triggers.AddNew();
			task2.P9_Description = "BILL STATUS";
			task2.TriggerConditions.TriggerEventCode = AutoEvents.MessageStatusChange.Code;
			task2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task2.TriggerConditions.TriggerConditionValue = "S2";
			var notification2 = task2.ProcessTaskNotifications.AddNew();
			notification2.PQ_TriggerType = "NTF";
			notification2.PQ_TriggerParty = "EML";
			notification2.PQ_EmailAddr = "dummy2@where.com";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<CusISFHeader>(header.PK);
			AssertEquals(1, header.ReferenceDatas.Count);
			bill1 = (CusISFBill)header.ReferenceDatas.FindByPK(bill1.PK);
			header.Logs.AddNew(AutoEvents.MessageStatusChange, "CIO");
			bill1.Logs.AddNew(Events.MessageStatusChange, "S2");
			newFactory.Save();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Table, "ProcessTasks");
			query.FetchOnlyFromLocalCache = true;
			var logs = newFactory.Load<StmALog>(query);
			AssertEquals(2, logs.Length);
			var checkLog1 = logs[0];
			var checkLog2 = logs[1];
			if (checkLog1.SL_Parent == task2.PK)
			{
				checkLog1 = logs[1];
				checkLog2 = logs[0];
			}

			AssertEquals(task1.PK, checkLog1.SL_Parent);
			AssertEquals(task2.PK, checkLog2.SL_Parent);
		}

		public void TestDeleteWhenNoBillNumIsEntered()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillNum = "Hello World";
			Factory.Save();
			AssertEquals(false, bill.IsDeleted);
			bill.BB_BillNum = ZString.Empty;
			Factory.Save();
			AssertEquals(true, bill.IsDeleted);
		}

		public void TestPropertiesHumanReadableName()
		{
			CusISFBill bill = Factory.New<CusISFBill>();
			AssertEquals("Reference Type", bill.BB_BillTypeInfo.HumanReadableName);
			AssertEquals("Reference Type Description", bill.BB_BillTypeDescriptionInfo.HumanReadableName);
			AssertEquals("Reference Data", bill.BB_BillNumInfo.HumanReadableName);
		}

		public void TestIShipmentReferenceIDMembers()
		{
			CusISFBill bill = Factory.New<CusISFBill>();
			IShipmentReferenceID shipmentReferenceID = bill;
			bill.BB_BillNum = "BB12311213";
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			AssertEquals(ShipmentReferenceIdentifierTypeList.Codes.OceanBillOfLading, shipmentReferenceID.CodeQualifier);
			AssertEquals("BB12311213", shipmentReferenceID.ShipmentReferenceIdentifier);
			bill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			AssertEquals(ShipmentReferenceIdentifierTypeList.Codes.HouseBillOfLading, shipmentReferenceID.CodeQualifier);
			AssertEquals("BB12311213", shipmentReferenceID.ShipmentReferenceIdentifier);
		}

		public void TestBillTypeDescriptonAndNumber()
		{
			CusISFBill bill = Factory.New<CusISFBill>();
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill.BB_BillNum = "1234567890";
			AssertEquals(BillTypeList.Descriptions.OceanBillOfLading + ":" + bill.BB_BillNum, bill.BillTypeDescriptonAndNumber);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			AssertEquals(BillTypeList.Descriptions.MasterBillOfLading + ":" + bill.BB_BillNum, bill.BillTypeDescriptonAndNumber);
		}

		public void TestIReferenceDataMembers()
		{
			CusISFBill bill = Factory.New<CusISFBill>();
			IReferenceData referenceData = bill;
			bill.BB_BillNum = "BB12311213";
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			AssertEquals(ZString.Empty, referenceData.CodeQualifier);
			AssertEquals(ZString.Empty, referenceData.ReferenceData);
			bill.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			AssertEquals(ReferenceDataCodeList.Codes.MasterBillOfLading, referenceData.CodeQualifier);
			AssertEquals("BB12311213", referenceData.ReferenceData);
			bill.BB_BillType = BillTypeList.Codes.SuretyCode;
			AssertEquals(ReferenceDataCodeList.Codes.SuretyCode, referenceData.CodeQualifier);
			AssertEquals("BB12311213", referenceData.ReferenceData);
			bill.BB_BillType = BillTypeList.Codes.USCBPEntryNumber;
			AssertEquals(ReferenceDataCodeList.Codes.USCBPEntryNumber, referenceData.CodeQualifier);
			AssertEquals("BB12311213", referenceData.ReferenceData);
			bill.BB_BillType = BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber;
			AssertEquals(ReferenceDataCodeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber, referenceData.CodeQualifier);
			bill.BB_BillType = BillTypeList.Codes.BondReferenceNumber;
			AssertEquals(ReferenceDataCodeList.Codes.BondReferenceNumber, referenceData.CodeQualifier);
			bill.BB_BillType = BillTypeList.Codes.UserDefinedReferenceNumber;
			AssertEquals(ReferenceDataCodeList.Codes.UserDefinedReferenceNumber, referenceData.CodeQualifier);
			bill.BB_BillType = BillTypeList.Codes.FullNameOfISFImporter;
			bill.BB_BillNum = "IMPORTER*NAME";
			AssertEquals(ReferenceDataCodeList.Codes.FullNameOfISFImporter, referenceData.CodeQualifier);
			AssertEquals("IMPORTER NAME", referenceData.ReferenceData);
		}

		public void TestGetDeclarationNumbers()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration1.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration1.JE_HouseBill = "12345670";
			declaration1.JE_HouseBillIssuerSCAC = "APLU";
			declaration1.JE_MasterBill = "12345671";
			declaration1.JE_MasterBillIssuerSCAC = "APLU";
			declaration1.JE_DeclarationReference = "B00001";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration2.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration2.JE_HouseBill = "12345671";
			declaration2.JE_HouseBillIssuerSCAC = "APLU";
			declaration2.JE_MasterBill = "12345672";
			declaration2.JE_MasterBillIssuerSCAC = "APLU";
			declaration2.JE_DeclarationReference = "B00002";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration3.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration3.JE_MasterBill = "12345671";
			declaration3.JE_MasterBillIssuerSCAC = "APLU";
			declaration3.JE_DeclarationReference = "B00003";
			var header1 = Factory.New<CusISFHeader>();
			var header2 = Factory.New<CusISFHeader>();
			var header3 = Factory.New<CusISFHeader>();
			Factory.Save();
			header1.BF_HouseBill = "APLU12345671";
			header2.BF_OceanBill = "APLU12345671";
			header3.BF_MasterBill = "APLU12345671";
			AssertEquals("B00002", header1.DeclarationJobNumber);
			AssertEquals("B00001,B00003", header2.DeclarationJobNumber);
			AssertEquals(ZString.Empty, header3.DeclarationJobNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return header.ReferenceDatas.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CusISFHeader header = factory.New<CusISFHeader>();
			header.BF_OH_Importer = factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill.BB_BillNum = "BLBL";
			return bill;
		}
	}
}

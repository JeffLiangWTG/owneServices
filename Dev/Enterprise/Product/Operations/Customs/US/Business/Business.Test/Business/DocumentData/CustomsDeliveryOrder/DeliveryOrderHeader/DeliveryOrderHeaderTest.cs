using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderHeader))]
	sealed class DeliveryOrderHeaderTest : DeclarationDocumentDataTest<DeliveryOrderHeader>
	{
		public void TestUS_DeliveryInstructions()
		{
			DeliveryOrderHeader header = Factory.New<DeliveryOrderHeader>();
			header.HasChanges = false;
			AssertEquals("", header.US_DeliveryInstructions);
			StmNote[] deliveryInstructionsNotes = header.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
			AssertEquals(0, deliveryInstructionsNotes.Length);
			header.US_DeliveryInstructions = "TEST";
			deliveryInstructionsNotes = header.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
			AssertEquals(1, deliveryInstructionsNotes.Length);
			AssertEquals("TEST", header.US_DeliveryInstructions);
			header.US_DeliveryInstructions = "";
			deliveryInstructionsNotes = header.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
			AssertEquals(0, deliveryInstructionsNotes.Length);
			AssertEquals("", header.US_DeliveryInstructions);
		}

		public void TestDataRefreshOnFetchedRows_CS00141228()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(0, dec.DeliveryOrderHeaders.Count);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var decInOtherFactory = newFactory.Load<JobDeclaration>(dec.PK);
			var orderHeaderInOtherFactory = decInOtherFactory.DeliveryOrderHeaders.AddNew();
			orderHeaderInOtherFactory.US_ShouldPrint = true;
			var orderBillInOtherFactory = orderHeaderInOtherFactory.DeliveryOrderBills.AddNew();
			orderBillInOtherFactory.CY_Data = "HB1";
			var orderContainerInOtherFactory = orderHeaderInOtherFactory.DeliveryOrderContainers.AddNew();
			orderContainerInOtherFactory.US_ContainerNumber = "CONT1";
			var orderLineInOtherFactory = orderHeaderInOtherFactory.DeliveryOrderLines.AddNew();
			orderLineInOtherFactory.US_GoodsDescription = "GOODS STUFF";
			var orderHazmatInOtherFactory = orderHeaderInOtherFactory.DeliveryOrderHazmats.AddNew();
			orderHazmatInOtherFactory.US_ProperShippingName = "STUFF";
			newFactory.Save();
			AssertEquals(1, dec.DeliveryOrderHeaders.Count);
			var ad = dec.WHSPackLines.Count; // cause fetch hint to load CusAddInfo
			var gd = dec.ContractNumbers.Count; // cause fetch hint to load CusCodeData
			orderBillInOtherFactory.CY_Data = "HB2";
			orderContainerInOtherFactory.US_ContainerNumber = "CONT2";
			orderLineInOtherFactory.US_GoodsDescription = "GOODS STUFF2";
			orderHazmatInOtherFactory.US_ProperShippingName = "STUFF2";
			newFactory.Save();
			var orderHeader = dec.DeliveryOrderHeaders[0];
			AssertEquals(1, orderHeader.DeliveryOrderBills.Count);
			AssertEquals("HB2", ((DeliveryOrderBill)orderHeader.DeliveryOrderBills.FindByPK(orderBillInOtherFactory.PK)).CY_Data);
			AssertEquals(1, orderHeader.DeliveryOrderContainers.Count);
			AssertEquals("CONT2", ((DeliveryOrderContainer)orderHeader.DeliveryOrderContainers.FindByPK(orderContainerInOtherFactory.PK)).US_ContainerNumber);
			AssertEquals(1, orderHeader.DeliveryOrderContainers.Count);
			AssertEquals("GOODS STUFF2", ((DeliveryOrderLine)orderHeader.DeliveryOrderLines.FindByPK(orderLineInOtherFactory.PK)).US_GoodsDescription);
			AssertEquals(1, orderHeader.DeliveryOrderHazmats.Count);
			AssertEquals("STUFF2", ((DeliveryOrderHazmat)orderHeader.DeliveryOrderHazmats.FindByPK(orderHazmatInOtherFactory.PK)).US_ProperShippingName);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			ICusAddInfoTypeSupporter supporter = header;
			supporter.AssertType(typeof(DeliveryOrderContainer), CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer);
			supporter.AssertType(typeof(DeliveryOrderHazmat), CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat);
			supporter.AssertType(typeof(DeliveryOrderLine), CusAddInfoTypeAttribute.Codes.USDeliveryOrderLine);
			supporter.AssertType(null, "ZZ!");
			var container = header.DeliveryOrderContainers.AddNew();
			container.US_ContainerMode = "1";
			var hazMat = header.DeliveryOrderHazmats.AddNew();
			hazMat.US_EmergencyContactNmber = "1";
			var line = header.DeliveryOrderLines.AddNew();
			line.US_GoodsDescription = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			CusAddInfo addInfo = newFactory.Load<CusAddInfo>(container.PK);
			AssertEquals(typeof(DeliveryOrderContainer), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(hazMat.PK);
			AssertEquals(typeof(DeliveryOrderHazmat), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(line.PK);
			AssertEquals(typeof(DeliveryOrderLine), addInfo.GetType());
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			ICusCodeDataTypeSupporter supporter = header;
			supporter.AssertType(typeof(DeliveryOrderBill), DeliveryOrderHeader.Constants.CusCodeDataBill);
			supporter.AssertType(null, "ZZ!");
			var bill = header.DeliveryOrderBills.AddNew();
			bill.CY_Code = "1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<Customs.Business.CusCodeData>(bill.PK);
			AssertEquals(typeof(DeliveryOrderBill), addInfo.GetType());
		}

		public void TestSpecificProperties()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VesselName = "VESSEL TEST";
			declaration.JE_VoyageFlightNo = "RN234";
			declaration.JE_MasterBillIssuerSCAC = "ABCD";
			declaration.US_EntryFilerCode = "BDC";
			declaration.ImportEntryNumber = "12345678";
			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals("", header.US_PreviousITNo);
			AssertEquals("", header.Data.US_PreviousITNo);
			header.US_PreviousITNo = "1234567890a";
			AssertEquals("1234567890a", header.US_PreviousITNo);
			AssertEquals("1234567890a", header.Data.US_PreviousITNo);
			header.US_PreviousITNo = "1234567a";
			AssertEquals("1234567a", header.US_PreviousITNo);
			AssertEquals("1234567a", header.Data.US_PreviousITNo);
			AssertEquals("VESSEL TEST", header.ImportingCarrier);
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "RN234";
			AssertEquals("RN234 ABCD", header.ImportingCarrier);
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.JE_VoyageFlightNo = "RN234";
			AssertEquals("RN234", header.ImportingCarrier);
			AssertEquals("BDC-1234567-8", header.FormattedEntryNumber);
			declaration.ImportEntryNumber = ZString.Empty;
			AssertEquals(ZString.Empty, header.FormattedEntryNumber);
		}

		public void TestPrintLongBranchName()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "AAAAAAAAAABBBBBBBBBBBDDDDDDDDDDDRRRRRRRRRRREEEEEEEEEEwwwwwwwwwwQQQQQQQQQmmmmmmmmm";
			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.Branch.GB_OH_OrgProxy = org1.PK;
			declaration.JE_GB = currentBranch.PK;
			DeliveryOrderHeader deliveryOrderPrint = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals("Branch Name", "AAAAAAAAAABBBBBBBBBBBDDDDDDDDDDDRRRRRRRRRRREEEEEEEEEEwwwwwwwwwwQQQQQQQQQmmmmmmmmm", deliveryOrderPrint.BranchName);
		}

		public void TestPrintsBranchAddress()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = currentBranch.PK;
			DeliveryOrderHeader deliveryOrderPrint = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals("Branch Name", currentBranch.OrgProxy.OH_FullName, deliveryOrderPrint.BranchName);
			AssertEquals("Branch Address 1", currentBranch.OrgProxy.MainAddress.OA_Address1, deliveryOrderPrint.BranchAddress1);
			AssertEquals("Branch Address 2", currentBranch.OrgProxy.MainAddress.OA_Address2, deliveryOrderPrint.BranchAddress2);
			AssertEquals("Branch City", currentBranch.OrgProxy.MainAddress.OA_City, deliveryOrderPrint.BranchCity);
			AssertEquals("Branch State", currentBranch.OrgProxy.MainAddress.OA_State, deliveryOrderPrint.BranchState);
			AssertEquals("Branch Post Code", currentBranch.OrgProxy.MainAddress.OA_PostCode, deliveryOrderPrint.BranchPostCode);
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			deliveryOrderPrint = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals("Branch Name", currentBranch.GB_BranchName, deliveryOrderPrint.BranchName);
			AssertEquals("Branch Address 1", currentBranch.GB_Address1, deliveryOrderPrint.BranchAddress1);
			AssertEquals("Branch Address 2", currentBranch.GB_Address2, deliveryOrderPrint.BranchAddress2);
			AssertEquals("Branch City", currentBranch.GB_City, deliveryOrderPrint.BranchCity);
			AssertEquals("Branch State", currentBranch.GB_State, deliveryOrderPrint.BranchState);
			AssertEquals("Branch Post Code", currentBranch.GB_PostCode, deliveryOrderPrint.BranchPostCode);
		}

		public void TestShipperEffective()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var org3 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			var order = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals(org1.PK, order.US_OH_Shipper);
			declaration.JE_OH_Importer = org2.PK;
			AssertEquals(org2.PK, order.US_OH_Shipper);
			order.US_OH_Shipper = org3.PK;
			AssertEquals(org3.PK, order.US_OH_Shipper);
			AssertEquals(org2.PK, declaration.JE_OH_Importer);
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals(org3.PK, order.US_OH_Shipper);
			AssertEquals(org1.PK, declaration.JE_OH_Importer);
		}

		public void TestDefault()
		{
			DeliveryOrderHeader header = Factory.New<DeliveryOrderHeader>();
			AssertEquals(JobDeclarationSchema.Constants.Prefix, header.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader, header.B7_Type);
		}

		public void TestDelete()
		{
			DeliveryOrderHeader header = Factory.New<DeliveryOrderHeader>();
			DeliveryOrderLine line = header.DeliveryOrderLines.AddNew();
			header.Delete();
			AssertEquals(true, header.IsDeleted);
			AssertEquals(true, line.IsDeleted);
		}

		public void TestDeleteWhenEmpty()
		{
			DeliveryOrderHeader header = Factory.New<DeliveryOrderHeader>();
			header.B7_ParentID = ZGuid.NewZGuid();
			header.B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			header.US_PrepaidCollect = "C";
			Factory.Save();
			AssertEquals(false, header.IsDeleted);
			header.US_PrepaidCollect = ZString.Empty;
			Factory.Save();
			AssertEquals(true, header.IsDeleted);
			header = Factory.New<DeliveryOrderHeader>();
			header.B7_ParentID = ZGuid.NewZGuid();
			header.B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			header.US_PrepaidCollect = "C";
			DeliveryOrderLine line = header.DeliveryOrderLines.AddNew();
			line.US_GoodsDescription = "Goods";
			Factory.Save();
			AssertEquals(false, header.IsDeleted);
			header.US_PrepaidCollect = ZString.Empty;
			Factory.Save();
			AssertEquals(false, header.IsDeleted);
			header.DeliveryOrderLines.DeleteAll();
			Factory.Save();
			AssertEquals(false, header.IsDeleted);
			header.US_PrepaidCollect = "C";
			Factory.Save();
			AssertEquals(false, header.IsDeleted);
			header.US_PrepaidCollect = ZString.Empty;
			Factory.Save();
			AssertEquals(true, header.IsDeleted);
		}

		public void TestDisclaimer()
		{
			USCustomsDataRegistry.Instance.CustomsDeliveryOrderDisclaimer.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "HELLO WORLD");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			DeliveryOrderHeader deliveryOrderPrint = declaration.DeliveryOrderHeaders.AddNew();
			AssertEquals("HELLO WORLD", deliveryOrderPrint.Disclaimer);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var result = declaration.DeliveryOrderHeaders.AddNew();
			result.US_PreviousITNo = "IT32";
			return result;
		}
	}
}

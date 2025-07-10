using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeader.Loader))]
	class CusInBondMoveHeaderLoaderTest : LoaderTestCase
	{
		public void TestFindByEntryNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = "OTT1";
			var moveHeader = header.MovementHeader;
			moveHeader.BM_ManifestSequenceNumber = "000123";
			moveHeader.MSNCusEntryNumber.CE_EntryLineReference = "OTT1";
			var loader = new CusInBondMoveHeader.Loader(Factory);
			AssertNull(loader.FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany("OTT1", "000124"));
			AssertEquals(moveHeader, loader.FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany("OTT1", "000123"));
			AssertNull(loader.FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany("OTT2", "000123"));
			var branch = Factory.New<GlbBranch>();
			header.BH_GB = branch.PK;
			AssertNull(loader.FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany("OTT1", "000123"));
			moveHeader.BM_ManifestSequenceNumber = "000000";
			AssertNull("Should not match to default value", loader.FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany("OTT1", "000000"));
			moveHeader.BM_ManifestSequenceNumber = "000001";
			AssertNull("Should not match to default value", loader.FindByCarrierCodeAndManifestSequenceNumberAndCurrentCompany("OTT1", "000001"));
		}

		public void TestMatchingInBondBillWithOceanBillData()
		{
			var header1 = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header1.BH_ApplicationCode = "INB";
			header1.BH_CarrierSCAC = "CARL";
			header1.BH_GB = Env.CurrentBranch.PK;
			header1.BH_VoyageNumber = "ST013";
			header1.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			header1.BH_PortUnladingDCode = "2704";
			header1.BH_ETA = ZDate.BrettsBirthday;
			var moveHeader1 = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader1.BM_BH = header1.PK;
			moveHeader1.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			moveHeader1.InBondNumber = "333210146";
			var header1Bill = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			header1Bill.B0_BH = header1.PK;
			header1Bill.B0_IssuerCode = "CARL";
			header1Bill.B0_MasterBillNumber = "CBP01307";
			var moveHederDetail1 = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveHederDetail1.B9_BM = moveHeader1.PK;
			moveHederDetail1.B9_B0 = header1Bill.PK;
			var billAddRef = Factory.New<Integration.Customs.US.InBond.ICusInbondBillAddRef>();
			billAddRef.BR_B0 = header1Bill.PK;
			billAddRef.BR_Qualifier = "OB";
			billAddRef.BR_ReferenceNum = "XXXWHB1190001101";
			var header2 = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header2.BH_ApplicationCode = "INB";
			header2.BH_CarrierSCAC = "CARL";
			header2.BH_GB = Env.CurrentBranch.PK;
			header2.BH_VoyageNumber = "ST013";
			header2.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			header2.BH_PortUnladingDCode = "2704";
			header2.BH_ETA = ZDate.BrettsBirthday;
			var moveHeader2 = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader2.BM_BH = header2.PK;
			moveHeader2.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			moveHeader2.InBondNumber = "333210147";
			var header2Bill = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			header2Bill.B0_BH = header1.PK;
			header2Bill.B0_IssuerCode = "CARL";
			header2Bill.B0_MasterBillNumber = "CBP01307";
			var moveHederDetail2 = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveHederDetail2.B9_BM = moveHeader2.PK;
			moveHederDetail2.B9_B0 = header2Bill.PK;
			Factory.Save();
			var loader = new CusInBondMoveHeader.Loader(Factory);
			var foundMoveHeader = loader.FindByManifestDataAndBillOfLading("CARL", "HYUNDAI SINGAPORE", "ST013", "2704", ZDate.BrettsBirthday, "CARL", "CBP01307", "OB", "XXXWHB1190001101", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.MasterInBond);
			AssertEquals(moveHeader1.InBondNumber, foundMoveHeader.InBondNumber);
		}

		public void TestFindByBillOfLadingData()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ApplicationCode = "AMS";
			header.BH_CarrierSCAC = "8CHN";
			header.BH_ETA = ZDate.BrettsBirthday;
			header.BH_ImportConveyanceName = "APL";
			header.BH_VoyageNumber = "V123";
			header.BH_PortUnladingDCode = "2704";
			header.BH_SystemCreateTimeUtc = new ZDateTime(2019, 6, 13);
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "OTT1";
			bill1.B0_MasterBillNumber = "H1MB1";
			var moveHeader = header.MovementHeader;
			var loader = new CusInBondMoveHeader.Loader(Factory);
			var foundMoveHeader = loader.FindByManifestDataAndBillOfLading("8CHN", "APL", "V123", "2704", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS);
			AssertEquals(moveHeader, foundMoveHeader); //AMS : for only one bill
			header.BH_IsActive = false;
			foundMoveHeader = loader.FindByManifestDataAndBillOfLading("8CHN", "APL", "V123", "2704", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS);
			AssertEquals("AMS : for only inactive header", moveHeader, foundMoveHeader);
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT1";
			bill2.B0_MasterBillNumber = "H1MB1";
			var pttMovement1 = header.PTTMovements.AddNew();
			pttMovement1.MovementDetails.AddNew(bill2.PK);
			var reference = bill2.ShipmentReferenceDetails.AddNew();
			reference.BR_Qualifier = "OB";
			reference.BR_ReferenceNum = "XXXWHB1190001101";
			foundMoveHeader = loader.FindByManifestDataAndBillOfLading("8CHN", "APL", "V123", "2704", ZDate.BrettsBirthday, "OTT1", "H1MB1", "OB", "XXXWHB1190001101", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer);
			AssertEquals(pttMovement1, foundMoveHeader); //AMS : for ocean bill reference number matched
			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "OTT2";
			bill3.B0_MasterBillNumber = "H1MB2";
			pttMovement1.MovementDetails.AddNew(bill3.PK);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_ApplicationCode = "AMS";
			header2.BH_CarrierSCAC = "8CHN";
			header2.BH_ETA = ZDate.BrettsBirthday;
			header2.BH_ImportConveyanceName = "APL2";
			header2.BH_VoyageNumber = "V123";
			header2.BH_PortUnladingDCode = "2704";
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header2.OceanBill.B0_IssuerCode = "OTT2";
			header2.OceanBill.B0_MasterBillNumber = "H1MB2";
			var moveHeader2 = header2.OceanBillPTTMovement;
			foundMoveHeader = loader.FindByManifestDataAndBillOfLading("8CHN", "APL2", "V123", "2704", ZDate.BrettsBirthday, "OTT2", "H1MB2", "OB", "XXXWHB1190001101", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer);
			AssertEquals(moveHeader2, foundMoveHeader); //AMS : for NVOCC with matching ocean bill details
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_ApplicationCode = "AMS";
			header3.BH_CarrierSCAC = "8CHN";
			header3.BH_ETA = ZDate.BrettsBirthday;
			header3.BH_ImportConveyanceName = "APL";
			header3.BH_VoyageNumber = "V123";
			header3.BH_PortUnladingDCode = "2704";
			header3.BH_SystemCreateTimeUtc = new ZDateTime(2019, 6, 12);
			var bill4 = header3.Bills.AddNew();
			bill4.B0_IssuerCode = "OTT1";
			bill4.B0_MasterBillNumber = "H1MB1";
			var moveHeader3 = header3.MovementHeader;
			foundMoveHeader = loader.FindByManifestDataAndBillOfLading("8CHN", "APL", "V123", "2704", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS);
			AssertEquals("AMS : for bill with active header", moveHeader3, foundMoveHeader);
			header3.BH_IsActive = false;
			foundMoveHeader = loader.FindByManifestDataAndBillOfLading("8CHN", "APL", "V123", "2704", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS);
			AssertEquals("AMS : It exists multiple bills with inactive header, choose the recent one", moveHeader, foundMoveHeader);
		}

		public void TestFindByManifestDataAndBillOfLading()
		{
			var newFactory = new BusinessObjectFactory();
			var headerInOtherFactory = newFactory.New<CusInBondHeader>();
			headerInOtherFactory.BH_CarrierSCAC = "OTT1";
			headerInOtherFactory.BH_ImportConveyanceName = "APL V";
			headerInOtherFactory.BH_VoyageNumber = "V234";
			headerInOtherFactory.BH_PortUnladingDCode = "2704";
			headerInOtherFactory.BH_ETA = ZDate.BrettsBirthday;
			var amsMoveHeaderInOtherFactory = headerInOtherFactory.MovementHeader;
			var header1Bill1InOtherFactory = headerInOtherFactory.Bills.AddNew();
			header1Bill1InOtherFactory.B0_IssuerCode = "OTT2";
			header1Bill1InOtherFactory.B0_MasterBillNumber = "H1MB1";
			var header1Bill2InOtherFactory = headerInOtherFactory.Bills.AddNew();
			header1Bill2InOtherFactory.B0_IssuerCode = "OTT2";
			header1Bill2InOtherFactory.B0_MasterBillNumber = "H1MB2";
			headerInOtherFactory.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			newFactory.Save();
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ApplicationCode = "AMS";
			header1.BH_CarrierSCAC = "OTT1";
			header1.BH_ImportConveyanceName = "APL V";
			header1.BH_VoyageNumber = "V234";
			header1.BH_PortUnladingDCode = "2704";
			header1.BH_ETA = ZDate.BrettsBirthday;
			var amsMoveHeader1 = header1.MovementHeader;
			var header1Bill1 = header1.Bills.AddNew();
			header1Bill1.B0_IssuerCode = "OTT2";
			header1Bill1.B0_MasterBillNumber = "H1MB1";
			var header1Bill2 = header1.Bills.AddNew();
			header1Bill2.B0_IssuerCode = "OTT2";
			header1Bill2.B0_MasterBillNumber = "H1MB2";
			var pptMoveHeader1 = header1.PTTMovements.AddNew();
			pptMoveHeader1.BM_InBondCarrierID = "PTTINB1";
			pptMoveHeader1.MovementDetails.AddNew(header1Bill1.PK);
			pptMoveHeader1.MovementDetails.AddNew(header1Bill2.PK);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_CarrierSCAC = "OTT1";
			header2.BH_ImportConveyanceName = "APL V";
			header2.BH_VoyageNumber = "V234";
			header2.BH_PortUnladingDCode = "2704";
			header2.BH_ETA = ZDate.BrettsBirthday;
			var amsMoveHeader2 = header2.MovementHeader;
			var header2Bill1 = header2.Bills.AddNew();
			header2Bill1.B0_IssuerCode = "OTT3";
			header2Bill1.B0_MasterBillNumber = "H1MB1";
			var header2Bill2 = header2.Bills.AddNew();
			header2Bill2.B0_IssuerCode = "OTT2";
			header2Bill2.B0_MasterBillNumber = "H1MB3";
			var pptMoveHeader2 = header2.PTTMovements.AddNew();
			pptMoveHeader2.BM_InBondCarrierID = "PTTINB2";
			pptMoveHeader2.MovementDetails.AddNew(header2Bill1.PK);
			pptMoveHeader2.MovementDetails.AddNew(header2Bill2.PK);
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_CarrierSCAC = "OTT2";
			header3.BH_ImportConveyanceName = "APL V";
			header3.BH_VoyageNumber = "V234";
			header3.BH_PortUnladingDCode = "2704";
			header3.BH_ETA = ZDate.BrettsBirthday;
			var amsMoveHeader3 = header3.MovementHeader;
			var header3Bill1 = header3.Bills.AddNew();
			header3Bill1.B0_IssuerCode = "OTT3";
			header3Bill1.B0_MasterBillNumber = "H1MB1";
			var header3Bill2 = header3.Bills.AddNew();
			header3Bill2.B0_IssuerCode = "OTT2";
			header3Bill2.B0_MasterBillNumber = "H1MB3";
			var pptMoveHeader3 = header3.PTTMovements.AddNew();
			pptMoveHeader3.BM_InBondCarrierID = "PTTINB2";
			pptMoveHeader3.MovementDetails.AddNew(header3Bill1.PK);
			pptMoveHeader3.MovementDetails.AddNew(header3Bill2.PK);
			var loader = new CusInBondMoveHeader.Loader(Factory);
			AssertEquals(pptMoveHeader2, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT3", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
			AssertEquals(amsMoveHeader2, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT3", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS));
			AssertNull(loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT3", "H1MB1", "", "", "", null));
			AssertNull(loader.FindByManifestDataAndBillOfLading(ZString.Empty, "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT3", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS));
			AssertNull(loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, ZString.Empty, "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS));
			AssertNull(loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT3", ZString.Empty, "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS));
			AssertEquals(amsMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, ZString.Empty, ZString.Empty, "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS));
			AssertEquals(pptMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT2", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
			AssertEquals(amsMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT2", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS));
		}

		public void TestMatchLowercaseLettersData()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ApplicationCode = "AMS";
			header1.BH_CarrierSCAC = "OTT1";
			header1.BH_ImportConveyanceName = "APL V";
			header1.BH_VoyageNumber = "v234";
			header1.BH_PortUnladingDCode = "2704";
			header1.BH_ETA = ZDate.BrettsBirthday;
			var amsMoveHeader1 = header1.MovementHeader;
			var header1Bill1 = header1.Bills.AddNew();
			header1Bill1.B0_IssuerCode = "OTT2";
			header1Bill1.B0_MasterBillNumber = "H1MB1";
			var header1Bill2 = header1.Bills.AddNew();
			header1Bill2.B0_IssuerCode = "OTT2";
			header1Bill2.B0_MasterBillNumber = "H1MB2";
			var pptMoveHeader1 = header1.PTTMovements.AddNew();
			pptMoveHeader1.BM_InBondCarrierID = "PTTINB1";
			pptMoveHeader1.MovementDetails.AddNew(header1Bill1.PK);
			pptMoveHeader1.MovementDetails.AddNew(header1Bill2.PK);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_CarrierSCAC = "OTT1";
			header2.BH_ImportConveyanceName = "APL V";
			header2.BH_VoyageNumber = "v234";
			header2.BH_PortUnladingDCode = "2704";
			header2.BH_ETA = ZDate.BrettsBirthday;
			var amsMoveHeader2 = header2.MovementHeader;
			var header2Bill1 = header2.Bills.AddNew();
			header2Bill1.B0_IssuerCode = "OTT3";
			header2Bill1.B0_MasterBillNumber = "h1mb1";
			var header2Bill2 = header2.Bills.AddNew();
			header2Bill2.B0_IssuerCode = "OTT2";
			header2Bill2.B0_MasterBillNumber = "H1MB3";
			var pptMoveHeader2 = header2.PTTMovements.AddNew();
			pptMoveHeader2.BM_InBondCarrierID = "PTTINB2";
			pptMoveHeader2.MovementDetails.AddNew(header2Bill1.PK);
			pptMoveHeader2.MovementDetails.AddNew(header2Bill2.PK);
			var loader = new CusInBondMoveHeader.Loader(Factory);
			AssertEquals("It can be matched with small letters", pptMoveHeader2, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT3", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
			AssertEquals(amsMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, ZString.Empty, ZString.Empty, "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.AMS));
		}

		public void TestFindByManifestDataAndBillOfLadingClosestETA()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_JobReference = "JOB1";
			header1.BH_CarrierSCAC = "OTT1";
			header1.BH_ImportConveyanceName = "APL V";
			header1.BH_VoyageNumber = "V234";
			header1.BH_PortUnladingDCode = "2704";
			header1.BH_ETA = new ZDateTime(2012, 4, 10, 2, 30, 0);
			var moveHeader1 = header1.MovementHeader;
			var bill1 = header1.Bills.AddNew();
			bill1.B0_IssuerCode = "OTT2";
			bill1.B0_MasterBillNumber = "MB1";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_JobReference = "JOB2";
			header2.BH_CarrierSCAC = "OTT1";
			header2.BH_ImportConveyanceName = "APL V";
			header2.BH_VoyageNumber = "V234";
			header2.BH_PortUnladingDCode = "2704";
			header2.BH_ETA = new ZDateTime(2012, 4, 8, 1, 30, 0);
			var moveHeader2 = header2.MovementHeader;
			var bill2 = header2.Bills.AddNew();
			bill2.B0_IssuerCode = "OTT2";
			bill2.B0_MasterBillNumber = "MB1";
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_JobReference = "JOB3";
			header3.BH_CarrierSCAC = "OTT1";
			header3.BH_ImportConveyanceName = "APL V";
			header3.BH_VoyageNumber = "V234";
			header3.BH_PortUnladingDCode = "2704";
			header3.BH_ETA = new ZDateTime(2012, 3, 9);
			var moveHeader3 = header3.MovementHeader;
			var bill3 = header3.Bills.AddNew();
			bill3.B0_IssuerCode = "OTT2";
			bill3.B0_MasterBillNumber = "MB1";
			var header4 = Factory.New<CusInBondHeader>();
			header4.BH_JobReference = "JOB4";
			header4.BH_CarrierSCAC = "OTT3";
			header4.BH_ImportConveyanceName = "APL V";
			header4.BH_VoyageNumber = "V234";
			header4.BH_PortUnladingDCode = "2704";
			header4.BH_ETA = new ZDateTime(2012, 5, 10, 2, 30, 1);
			var moveHeader4 = header4.MovementHeader;
			var bill4 = header4.Bills.AddNew();
			bill4.B0_IssuerCode = "OTT2";
			bill4.B0_MasterBillNumber = "MB1";
			var header5 = Factory.New<CusInBondHeader>();
			header5.BH_JobReference = "JOB5";
			header5.BH_CarrierSCAC = "OTT3";
			header5.BH_ImportConveyanceName = "APL V";
			header5.BH_VoyageNumber = "V234";
			header5.BH_PortUnladingDCode = "2704";
			header5.BH_ETA = new ZDateTime(2012, 5, 8, 1, 30, 1);
			var moveHeader5 = header5.MovementHeader;
			var bill5 = header5.Bills.AddNew();
			bill5.B0_IssuerCode = "OTT2";
			bill5.B0_MasterBillNumber = "MB1";
			var header6 = Factory.New<CusInBondHeader>();
			header6.BH_JobReference = "JOB6";
			header6.BH_CarrierSCAC = "OTT3";
			header6.BH_ImportConveyanceName = "APL V";
			header6.BH_VoyageNumber = "V234";
			header6.BH_PortUnladingDCode = "2704";
			header6.BH_ETA = new ZDateTime(2012, 3, 11);
			var moveHeader6 = header6.MovementHeader;
			var bill6 = header6.Bills.AddNew();
			bill6.B0_IssuerCode = "OTT2";
			bill6.B0_MasterBillNumber = "MB1";
			Factory.Save();
			var loader = new CusInBondMoveHeader.Loader(Factory);
			AssertEquals(moveHeader3, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", new ZDate(2012, 3, 9), "OTT2", "MB1", "", "", "", x => true));
			AssertEquals("moveHeader2 as it is closest to 2012-04-09", moveHeader2, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", new ZDate(2012, 4, 9), "OTT2", "MB1", "", "", "", x => true));
			AssertEquals("moveHeader6 as it is match 2012-03-11, no carrier match", moveHeader6, loader.FindByManifestDataAndBillOfLading("OTT4", "APL V", "V234", "2704", new ZDate(2012, 3, 11), "OTT2", "MB1", "", "", "", x => true));
			AssertEquals("moveHeader5 as it is closet to 2012-03-11, no carrier match", moveHeader5, loader.FindByManifestDataAndBillOfLading("OTT4", "APL V", "V234", "2704", new ZDate(2012, 5, 9), "OTT2", "MB1", "", "", "", x => true));
			AssertEquals(moveHeader3, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", new ZDate(2012, 3, 9), "", "", "", "", "", x => true));
			AssertEquals("t can be matched with small letters", moveHeader3, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "v234", "2704", new ZDate(2012, 3, 9), "", "", "", "", "", x => true));
			AssertEquals("moveHeader2 as it is closest to 2012-04-09", moveHeader2, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", new ZDate(2012, 4, 9), "", "", "", "", "", x => true));
			AssertNull("no carrier match", loader.FindByManifestDataAndBillOfLading("OTT4", "APL V", "V234", "2704", new ZDate(2012, 3, 11), "", "", "", "", "", x => true));
			AssertNull("no carrier match", loader.FindByManifestDataAndBillOfLading("OTT4", "APL V", "V234", "2704", new ZDate(2012, 5, 9), "", "", "", "", "", x => true));
		}

		public void TestFindByManifestDataAndBillOfLadingForInBond()
		{
			var newFactory = new BusinessObjectFactory();
			var header = newFactory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = "INB";
			header.BH_CarrierSCAC = "CARL";
			header.BH_GB = Env.CurrentBranch.PK;
			header.BH_VoyageNumber = "ST013";
			header.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			header.BH_PortUnladingDCode = "2704";
			header.BH_ETA = ZDate.BrettsBirthday;
			var moveHeader = newFactory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			moveHeader.InBondNumber = "333210146";
			var headerBill = newFactory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			headerBill.B0_BH = header.PK;
			headerBill.B0_IssuerCode = "CARL";
			headerBill.B0_MasterBillNumber = "CBP01307";
			var moveHederDetail = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveHederDetail.B9_BM = moveHeader.PK;
			moveHederDetail.B9_B0 = headerBill.PK;
			newFactory.Save();
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_CarrierSCAC = "OTT1";
			header1.BH_ImportConveyanceName = "APL V";
			header1.BH_VoyageNumber = "V234";
			header1.BH_PortUnladingDCode = "2704";
			header1.BH_ETA = ZDate.BrettsBirthday;
			var amsMoveHeader1 = header1.MovementHeader;
			var header1Bill1 = header1.Bills.AddNew();
			header1Bill1.B0_IssuerCode = "OTT2";
			header1Bill1.B0_MasterBillNumber = "H1MB1";
			var header1Bill2 = header1.Bills.AddNew();
			header1Bill2.B0_IssuerCode = "OTT2";
			header1Bill2.B0_MasterBillNumber = "H1MB2";
			var pptMoveHeader1 = header1.PTTMovements.AddNew();
			pptMoveHeader1.BM_InBondCarrierID = "PTTINB1";
			pptMoveHeader1.MovementDetails.AddNew(header1Bill1.PK);
			pptMoveHeader1.MovementDetails.AddNew(header1Bill2.PK);
			var loader = new CusInBondMoveHeader.Loader(Factory);
			AssertEquals(pptMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT2", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
			var attachee = loader.FindByManifestDataAndBillOfLading("CARL", "HYUNDAI SINGAPORE", "ST013", "2704", ZDate.BrettsBirthday, "CARL", "CBP01307", "", "", "333210146", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.MasterInBond);
			AssertEquals(moveHeader.InBondNumber, attachee.InBondNumber);
			AssertNull(loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT3", "H1MB1", "", "", "", null));
		}

		public void TestFindManifestDataWithLineScheduleD()
		{
			var newFactory = new BusinessObjectFactory();
			var header = newFactory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = "INB";
			header.BH_CarrierSCAC = "CARL";
			header.BH_GB = Env.CurrentBranch.PK;
			header.BH_VoyageNumber = "ST013";
			header.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			header.BH_PortUnladingDCode = "2704";
			header.BH_ETA = ZDate.Today.AddHours(1);
			var moveHeader = newFactory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			moveHeader.InBondNumber = "333210146";
			var headerBill = newFactory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			headerBill.B0_BH = header.PK;
			headerBill.B0_IssuerCode = "CARL";
			headerBill.B0_MasterBillNumber = "CBP01307";
			var moveHederDetail = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveHederDetail.B9_BM = moveHeader.PK;
			moveHederDetail.B9_B0 = headerBill.PK;
			newFactory.Save();
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_CarrierSCAC = "OTT1";
			header1.BH_ImportConveyanceName = "APL V";
			header1.BH_VoyageNumber = "V234";
			header1.BH_PortUnladingDCode = "2704";
			header1.BH_ETA = ZDate.Today.AddHours(2);
			header1.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var header1Bill1 = header1.Bills.AddNew();
			header1Bill1.B0_IssuerCode = "OTT1";
			header1Bill1.B0_MasterBillNumber = "H1MB1";
			header1Bill1.B0_InBondPortOfDestDCode = "1101";
			var header1Bill2 = header1.Bills.AddNew();
			header1Bill2.B0_IssuerCode = "OTT1";
			header1Bill2.B0_MasterBillNumber = "H1MB2";
			header1Bill2.B0_InBondPortOfDestDCode = "3901";
			var header1Bill3 = header1.Bills.AddNew();
			header1Bill3.B0_IssuerCode = "OTT1";
			header1Bill3.B0_MasterBillNumber = "H1MB1";
			var header1Bill4 = header1.Bills.AddNew();
			header1Bill4.B0_IssuerCode = "OTT1";
			header1Bill4.B0_MasterBillNumber = "H1MB2";
			var pptMoveHeader1 = header1.PTTMovements.AddNew();
			pptMoveHeader1.BM_InBondCarrierID = "PTTINB1";
			pptMoveHeader1.MovementDetails.AddNew(header1Bill1.PK);
			pptMoveHeader1.MovementDetails.AddNew(header1Bill2.PK);
			var loader = new CusInBondMoveHeader.Loader(Factory);
			CombineAssertions(() =>
			{
				AssertEquals(header1.IsNVOCCHeader, false);
				AssertEquals("1.Matched by bill", pptMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "1101", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
				AssertEquals("2.Matched by bill", pptMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "3901", ZDate.BrettsBirthday, "OTT1", "H1MB2", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
				AssertEquals("3.Matched by bill", null, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
				header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
				AssertEquals(header1.IsNVOCCHeader, true);
				AssertEquals("1.Matched by header level", null, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "1101", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
				AssertEquals("2.Matched by header level", null, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "3901", ZDate.BrettsBirthday, "OTT1", "H1MB2", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
				AssertEquals("3.Matched by header level", pptMoveHeader1, loader.FindByManifestDataAndBillOfLading("OTT1", "APL V", "V234", "2704", ZDate.BrettsBirthday, "OTT1", "H1MB1", "", "", "", x => x.Item2.SupApplicationCode == SubApplicationCodeList.Codes.PermitToTransfer));
			});
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusInBondMoveHeader.Loader(Factory);
		}
	}
}

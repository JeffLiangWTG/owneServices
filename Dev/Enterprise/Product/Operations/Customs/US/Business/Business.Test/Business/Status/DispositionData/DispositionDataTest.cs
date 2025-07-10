using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DispositionData))]
	class DispositionDataTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DispositionData>
	{
		public void TestDefault()
		{
			DispositionData data = Factory.New<DispositionData>();
			AssertEquals(CusAddInfoTypeAttribute.Codes.USDisposition, data.B7_Type);
		}

		public void TestProperties()
		{
			DispositionData code = Bill.DispositionCodes.AddNew();
			code.US_DispositionDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, code.US_DispositionDate);
			code.US_Code = CargoReleaseProcessingResultList.Codes.PaperlessEntry;
			AssertEquals(CargoReleaseProcessingResultList.Codes.PaperlessEntry, code.US_Code);
			code.US_Order = 1;
			AssertEquals((ZShort)1, code.US_Order);
			code.US_ReleaseDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, code.US_ReleaseDate);

			code.US_FTZIDType = "1";
			code.US_FTZNumber = "1530001110";
			AssertEquals("1 1530001110", code.IDTypeNumber);
			code.Factory.Save();
			DispositionData code1 = Factory.Load<DispositionData>(code.PK);
			AssertEquals("1 1530001110", code1.IDTypeNumber);
		}

		public virtual void TestDispositionCodeDesc()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);

			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode, "SO50RecordDispCode", dataGrouping.ZZZ_DataGrouping);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType1.ZZK_CodeType, "91", "No Bill Match", startDate, endDate);
			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableCRL = true;
			var code = Bill.DispositionCodes.AddNew();

			code.US_Code = Enterprise.Customs.US.Messaging.Business.DispositionList.Codes.A1;
			AssertEquals("Parent Loading", Bill, code.Parent);
			AssertEquals(Enterprise.Customs.US.Messaging.Business.DispositionList.Descriptions.A1, code.DispositionCodeDesc);

			code.US_Code = Enterprise.Customs.US.Messaging.Business.DispositionList.Codes._91;
			AssertEquals("Parent Loading", Bill, code.Parent);
			AssertEquals("Transfer of liability for in-bond/No Bill Match", code.DispositionCodeDesc);

			code.US_Source = BillDispositionSourceList.Codes.IS;
			AssertEquals(Enterprise.Customs.US.Messaging.Business.DispositionList.Descriptions._91, code.DispositionCodeDesc);

			code.US_Source = BillDispositionSourceList.Codes.SO;
			AssertEquals("No Bill Match", code.DispositionCodeDesc);

			code.US_Code = "Z1";
			code.US_Source = BillDispositionSourceList.Codes.CQ;
			AssertEquals(DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode).GetDescriptionFromCode("Z1"), code.DispositionCodeDesc);
		}

		public void TestReleaseOriginDesc()
		{
			DispositionData code = Bill.DispositionCodes.AddNew();
			code.US_ReleaseOrigin = ReleaseOriginCodeList.Codes.OtherAgencyReviewCompleted;
			AssertEquals(ReleaseOriginCodeList.Codes.OtherAgencyReviewCompleted, code.US_ReleaseOrigin);
			AssertEquals(ReleaseOriginCodeList.Descriptions.OtherAgencyReviewCompleted, code.ReleaseOriginDesc);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Bill.DispositionCodes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			var data = declaration.Bills.AddNew().DispositionCodes.AddNew();
			data.US_Code = "03";
			data.US_DispositionDate = ZDateTime.Today;
			return data;
		}

		protected override IEnumerable<DispositionData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.DispositionCodes.AddNewIfNotExist("1", ZDateTime.Today);
			var bill = declaration.Bills.AddNew();
			yield return bill.DispositionCodes.AddNewIfNotExist("2", ZDateTime.Today);
			var container = declaration.CusContainers.AddNew();
			yield return container.DispositionCodes.AddNewIfNotExist("3", ZDateTime.Today);
			var inBondHeader = factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			var inBondBill = factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			inBondBill.B0_BH = inBondHeader.PK;
			var inBondMoveHeader = (CusInBondMoveHeader)factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			inBondMoveHeader.BM_BH = inBondHeader.PK;
			var inBondMoveDetail = (CusInBondMoveDetail)factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			inBondMoveDetail.B9_BM = inBondMoveHeader.PK;
			inBondMoveDetail.B9_B0 = inBondBill.PK;
			yield return new DispositionDataCollection(inBondMoveDetail).AddNewIfNotExist("4", ZDateTime.Today);
			var inBondContainer = factory.New<Integration.Customs.US.InBond.ICusInBondContainer>();
			inBondContainer.BC_ParentID = inBondMoveDetail.PK;
			inBondContainer.BC_ParentTableCode = inBondMoveDetail.TablePrefix;
			yield return new DispositionDataCollection((BusinessObject)inBondContainer).AddNewIfNotExist("5", ZDateTime.Today);
			var amsHeader = factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			yield return new DispositionDataCollection((BusinessObject)amsHeader).AddNewIfNotExist("6", ZDateTime.Today);
			var amsBill = factory.New<Integration.Customs.US.USAMS.ICusInBondBill>();
			amsBill.B0_BH = amsHeader.PK;
			yield return new DispositionDataCollection((BusinessObject)amsBill).AddNewIfNotExist("7", ZDateTime.Today);
		}

		Bill Bill
		{
			get { return fBill ?? (fBill = Declaration.Bills.AddNew()); }
		}
		Bill fBill;

		public JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OGADispositionDetail))]
	sealed class OGADispositionDetailTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<OGADispositionDetail>
	{
		public void TestDefault()
		{
			OGADispositionDetail detail = Factory.New<OGADispositionDetail>();
			AssertEquals(CusAddInfoTypeAttribute.Codes.USOGADispositionDetail, detail.B7_Type);
		}

		public void TestRefTypeAndReasonDesc()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "100", "DETAINED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "102", "REFUSED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "103", "PARTIAL RELEASE AND REFUSE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "104", "PGA INSPECTION NEEDED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "105", "EXAM", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "106", "EXAM NOTIFY", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "107", "FOREIGN ULTIMATE CONSIGNEE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "108", "MISSING REGISTRATION", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "109", "REGISTRATION NOT ON FILE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var detail = DispositionCode.OGADispositionDetails.AddNew();
			AssertEquals("Parent Loading", DispositionCode, detail.Parent);

			detail.US_ReferenceIDQualifier = OGADispositionReferenceQualifierList.Codes.PermitNum;
			AssertEquals("02 - Permit Number", detail.ReferenceQualifierCodeDesc);

			List<ZString> subReasonList = new List<ZString>();
			subReasonList.Add("100");
			subReasonList.Add("102");
			subReasonList.Add("103");
			subReasonList.Add("104");
			subReasonList.Add("105");
			subReasonList.Add("106");
			subReasonList.Add("107");
			subReasonList.Add("108");
			subReasonList.Add("109");
			subReasonList.Add(ZString.Empty);
			detail.SubReasonCodes = subReasonList;

			AssertEquals("DETAINED", detail.SubReasonCodeDesc1);
			AssertEquals("REFUSED", detail.SubReasonCodeDesc2);
			AssertEquals("PARTIAL RELEASE AND REFUSE", detail.SubReasonCodeDesc3);
			AssertEquals("PGA INSPECTION NEEDED", detail.SubReasonCodeDesc4);
			AssertEquals("EXAM", detail.SubReasonCodeDesc5);
			AssertEquals("EXAM NOTIFY", detail.SubReasonCodeDesc6);
			AssertEquals("FOREIGN ULTIMATE CONSIGNEE", detail.SubReasonCodeDesc7);
			AssertEquals("MISSING REGISTRATION", detail.SubReasonCodeDesc8);
			AssertEquals("REGISTRATION NOT ON FILE", detail.SubReasonCodeDesc9);
			AssertEquals(ZString.Empty, detail.SubReasonCodeDesc10);
		}

		public OGADispositionData DispositionCode
		{
			get { return fDispositionCode ?? (fDispositionCode = Factory.New<OGADispositionData>()); }
			set { fDispositionCode = value; }
		}
		OGADispositionData fDispositionCode;

		protected override BusinessObject GetNewBusinessObject()
		{
			return DispositionCode.OGADispositionDetails.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var dispositionCode = declaration.OGADispositionCodes.AddNew();
			var data = dispositionCode.OGADispositionDetails.AddNew();
			data.US_ReferenceIDQualifier = OGADispositionReferenceQualifierList.Codes.PriorNoticeConfirmationNum;
			data.US_ReferenceID = "1111111";
			data.US_ReceiptDateTime = ZDateTime.Today;
			List<ZString> subReasonCodes = new List<ZString> { OGADispositionSubReasonList.Codes._133 };
			data.SubReasonCodes = subReasonCodes;
			dispositionCode.US_Code = FDALineLevelDispositionCodeList.Codes._02;
			dispositionCode.US_DispositionDate = ZDateTime.Today;
			return data;
		}
	}
}

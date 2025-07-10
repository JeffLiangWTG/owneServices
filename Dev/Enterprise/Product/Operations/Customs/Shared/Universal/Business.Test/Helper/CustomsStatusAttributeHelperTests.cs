using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	public class CustomsStatusAttributeHelperTests : TestCaseWithFactory
	{
		public void TestShouldUpdateEntryNumber()
		{
			Assert("ShouldUpdateEntryNumber", !CustomsStatusAttributeHelper.ShouldUpdateEntryNumber(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IUpdateEntryNumber;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldUpdateEntryNumber", CustomsStatusAttributeHelper.ShouldUpdateEntryNumber(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldUpdateReleaseDate()
		{
			Assert("ShouldUpdateReleaseDate", !CustomsStatusAttributeHelper.ShouldUpdateReleaseDate(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IUpdateReleaseDate;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldUpdateReleaseDate", CustomsStatusAttributeHelper.ShouldUpdateReleaseDate(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldUpdateBondedWhs()
		{
			Assert("ShouldUpdateBondedWhs", !CustomsStatusAttributeHelper.ShouldUpdateBondedWhs(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IUpdateBondedWhs;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldUpdateBondedWhs", CustomsStatusAttributeHelper.ShouldUpdateBondedWhs(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldCancelBondedWhs()
		{
			Assert("ShouldCancelBondedWhs", !CustomsStatusAttributeHelper.ShouldCancelBondedWhs(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.ICancelBondedWhs;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldCancelBondedWhs", CustomsStatusAttributeHelper.ShouldCancelBondedWhs(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldUpdateCustomsStatus()
		{
			Assert("ShouldUpdateCustomsStatus", !CustomsStatusAttributeHelper.ShouldUpdateCustomsStatus(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IUpdateCustomsStatus;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldUpdateCustomsStatus", CustomsStatusAttributeHelper.ShouldUpdateCustomsStatus(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldUpdateCIQStatus()
		{
			Assert("ShouldUpdateCIQStatus", !CustomsStatusAttributeHelper.ShouldUpdateCIQStatus(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IUpdateCIQStatus;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldUpdateCIQStatus", CustomsStatusAttributeHelper.ShouldUpdateCIQStatus(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldNotify()
		{
			Assert("ShouldNotify", !CustomsStatusAttributeHelper.ShouldNotify(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.INotify;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldNotify", CustomsStatusAttributeHelper.ShouldNotify(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldSendEntryDocs()
		{
			Assert("ShouldSendEntryDocs", !CustomsStatusAttributeHelper.ShouldSendEntryDocs(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.ISendEntryDocs;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldSendEntryDocs", CustomsStatusAttributeHelper.ShouldSendEntryDocs(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldAddEntryDocsToEDocs()
		{
			Assert("ShouldAddEntryDocsToEDocs", !CustomsStatusAttributeHelper.ShouldAddEntryDocsToEDocs(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IAddEntryDocsToEDocs;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldAddEntryDocsToEDocs", CustomsStatusAttributeHelper.ShouldAddEntryDocsToEDocs(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestAllowCancel()
		{
			Assert("AllowCancel", !CustomsStatusAttributeHelper.AllowCancel(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IAllowCancel;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("AllowCancel", !CustomsStatusAttributeHelper.AllowCancel(Factory, "XXX", "ZA", ZDateTime.Now));
			attribute.ZZE_Value = "true";
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("AllowCancel", CustomsStatusAttributeHelper.AllowCancel(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestCustomsCancelled()
		{
			Assert("CustomsCancelled", !CustomsStatusAttributeHelper.IsStatusCancelled(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.ICustomsCancelled;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("CustomsCancelled", CustomsStatusAttributeHelper.IsStatusCancelled(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestCustomsCommenced()
		{
			Assert("CustomsCommenced", !CustomsStatusAttributeHelper.IsStatusCommenced(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.CustomsCommenced;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("CustomsCommenced", CustomsStatusAttributeHelper.IsStatusCommenced(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestCustomsCleared()
		{
			Assert("CustomsCleared", !CustomsStatusAttributeHelper.IsStatusCleared(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.CustomsCleared;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("CustomsCleared", !CustomsStatusAttributeHelper.IsStatusCleared(Factory, "XXX", "ZA", ZDateTime.Now));
			attribute.ZZE_Value = "true";
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("CustomsCleared", CustomsStatusAttributeHelper.IsStatusCleared(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestShouldPostCustomsAPInvoice()
		{
			Assert("PostCustomsAPInvoice", !CustomsStatusAttributeHelper.ShouldPostCustomsAPInvoice(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IPostCustomsAPInvoice;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("PostCustomsAPInvoice", !CustomsStatusAttributeHelper.ShouldPostCustomsAPInvoice(Factory, "XXX", "ZA", ZDateTime.Now));
			attribute.ZZE_Value = "true";
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("PostCustomsAPInvoice", CustomsStatusAttributeHelper.ShouldPostCustomsAPInvoice(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestCustomsRejected()
		{
			Assert("CustomsRejected", !CustomsStatusAttributeHelper.IsStatusRejected(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.CustomsRejected;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("CustomsRejected", !CustomsStatusAttributeHelper.IsStatusRejected(Factory, "XXX", "ZA", ZDateTime.Now));
			attribute.ZZE_Value = "true";
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("CustomsRejected", CustomsStatusAttributeHelper.IsStatusRejected(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestIsStatusFitToMarkPayInfoAsAwaitingResponse()
		{
			Assert("IsStatusFitToUpdateAllPaymentInfo", !CustomsStatusAttributeHelper.IsStatusFitToMarkPayInfoAsAwaitingResponse(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IMarkEntryPayInfoAwaitingResp;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("IsStatusFitToUpdateAllPaymentInfo", !CustomsStatusAttributeHelper.IsStatusFitToMarkPayInfoAsAwaitingResponse(Factory, "XXX", "ZA", ZDateTime.Now));
			attribute.ZZE_Value = "true";
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("IsStatusFitToUpdateAllPaymentInfo", CustomsStatusAttributeHelper.IsStatusFitToMarkPayInfoAsAwaitingResponse(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		ZZRefCusCodeListCombined cusCodeList;
		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus;
			cusCodeList.ZZD_Code = "XXX";
			cusCodeList.ZZD_Description = "XXX Desc";
			cusCodeList.ZZD_CountryOrGrouping = "ZA";
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddMonths(-1);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();
		}
	}
}

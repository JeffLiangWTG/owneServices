using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class ManifestToOpenValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_SubType()
		{
			CombineAssertions("Validation for CSI_SubType", () =>
			{
				manifestToOpen.CSI_SubType = ZString.Empty;
				AssertHasMessageErrorContaining("Mandatory Validation", manifestToOpen.CSI_SubTypeInfo, "You have not entered an Opening Style.");
				manifestToOpen.CSI_SubType = "L";
				AssertNoMessageErrorContaining("Mandatory Validation", manifestToOpen.CSI_SubTypeInfo, "You have not entered an Opening Style.");
			});
		}

		public void TestCheckCSI_ReferenceNumber2()
		{
			CombineAssertions("Validation for CSI_ReferenceNumber2", () =>
			{
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Manifestlevel;
				manifestToOpen.CSI_ReferenceNumber2 = ZString.Empty;
				AssertHasMessageErrorContaining("Mandatory Validation", manifestToOpen.CSI_ReferenceNumber2Info, "You have not entered a Registration No.");
				manifestToOpen.CSI_ReferenceNumber2 = "ABCDE";
				AssertNoMessageErrorContaining("Mandatory Validation", manifestToOpen.CSI_ReferenceNumber2Info, "You have not entered a Registration No.");
			});
		}

		public void TestCheckBillNo()
		{
			CombineAssertions("Validation for Bill No.", () =>
			{
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlevel;
				manifestToOpen.BillNo = "22067777IM12345678";
				AssertNoMessageErrorContaining("Billlevel Mandatory Validation", manifestToOpen.BillNoInfo, "You have not entered a Bill No.");
				manifestToOpen.BillNo = ZString.Empty;
				AssertHasMessageErrorContaining("Billlevel Mandatory Validation", manifestToOpen.BillNoInfo, "You have not entered a Bill No.");
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
				manifestToOpen.BillNo = "22067777IM12345679";
				AssertNoMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.BillNoInfo, "You have not entered a Bill No.");
				manifestToOpen.BillNo = ZString.Empty;
				AssertHasMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.BillNoInfo, "You have not entered a Bill No.");
			});
		}

		public void TestCheckCSI_LineNo()
		{
			CombineAssertions("Validation for CSI_LineNo", () =>
			{
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
				manifestToOpen.CSI_LineNo = 123;
				AssertNoMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_LineNoInfo, "You have not entered a Bill Line No.");
				manifestToOpen.CSI_LineNo = ZShort.Zero;
				AssertHasMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_LineNoInfo, "You have not entered a Bill Line No.");
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlevel;
				manifestToOpen.CSI_LineNo = ZShort.Zero;
				manifestToOpen.Validation.ValidateCSI_LineNo();
				AssertNoMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_LineNoInfo, "You have not entered a Bill Line No.");
			});
		}

		public void TestCheckCSI_Quantity()
		{
			CombineAssertions("Validation for CSI_Quantity", () =>
			{
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
				manifestToOpen.CSI_Quantity = ZDecimal.Zero;
				AssertHasMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_QuantityInfo, "You have not entered a Total No. of Packs.");
				manifestToOpen.CSI_Quantity = 123456;
				AssertNoMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_QuantityInfo, "You have not entered a Total No. of Packs.");
				manifestToOpen.CSI_Quantity = -1000;
				AssertHasErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_QuantityInfo, "Please enter a non-negative value.");
				manifestToOpen.CSI_Quantity = 1000;
				AssertNoErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_QuantityInfo, "Please enter a non-negative value.");
			});
		}

		public void TestCheckCSI_Quantity2()
		{
			CombineAssertions("Validation for CSI_Quantity", () =>
			{
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
				manifestToOpen.CSI_Quantity2 = ZDecimal.Zero;
				AssertHasMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_Quantity2Info, "You have not entered a Deduction No. of Packs.");
				manifestToOpen.CSI_Quantity2 = 123456;
				AssertNoMessageErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_Quantity2Info, "You have not entered a Deduction No. of Packs.");
				manifestToOpen.CSI_Quantity2 = -1000;
				AssertHasErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_Quantity2Info, "Please enter a non-negative value.");
				manifestToOpen.CSI_Quantity2 = 1000;
				AssertNoErrorContaining("Billlinelevel Mandatory Validation", manifestToOpen.CSI_Quantity2Info, "Please enter a non-negative value.");
			});
		}

		public void TestCheckCSI_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRCWH", "TRCWH");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRCWH", "A0002", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();
			CombineAssertions("Validation for CSI_CustomsOffice", () =>
			{
				manifestToOpen.CSI_CustomsOffice = "X0001";
				manifestToOpen.Validation.ValidateAll();
				AssertHasMessageErrorContaining(manifestToOpen.CSI_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
				manifestToOpen.CSI_CustomsOffice = "A0002";
				manifestToOpen.Validation.ValidateAll();
				AssertNoMessageErrorContaining(manifestToOpen.CSI_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCSI_Description()
		{
			CombineAssertions("Validation for CSI_Description", () =>
			{
				manifestToOpen.CSI_SubType = SubTypeListForManifestToOpen.Codes.Billlinelevel;
				manifestToOpen.CSI_ReferenceNumber2 = "ABCDE";
				manifestToOpen.BillNo = "22067777IM12345678";
				manifestToOpen.CSI_LineNo = 1;
				manifestToOpen.Procedure = true;
				manifestToOpen.CSI_Description = ZString.Empty;
				var manifestToOpen2 = header.ManifestsToOpenList.AddNew();
				manifestToOpen2.CSI_ReferenceNumber2 = "ABCDE";
				manifestToOpen2.BillNo = "22067777IM12345678";
				manifestToOpen2.CSI_LineNo = 2;
				manifestToOpen2.Procedure = true;
				AssertHasMessageErrorContaining("ManifestsToOpenList Mandatory Validation", manifestToOpen.CSI_DescriptionInfo, "You have not entered a Description.");
				AssertNoMessageErrorContaining("ManifestsToOpenList2 Mandatory Validation", manifestToOpen2.CSI_DescriptionInfo, "You have not entered a Description.");
				manifestToOpen.CSI_Description = "XXX";
				AssertNoMessageErrorContaining("ManifestsToOpenList Mandatory Validation", manifestToOpen.CSI_DescriptionInfo, "You have not entered a Description.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestToOpen = header.ManifestsToOpenList.AddNew();
		}

		ManifestToOpen manifestToOpen;
		AsycudaManifestHeader header;
	}
}

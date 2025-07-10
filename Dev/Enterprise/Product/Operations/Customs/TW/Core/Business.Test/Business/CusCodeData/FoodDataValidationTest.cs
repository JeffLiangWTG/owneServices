using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class FoodDataValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckContent()
		{
			foodData.Content = -1;
			AssertHasMessageError(foodData.ContentInfo, ValidationConstants.InvoiceLine.InvalidValue(foodData.CY_CodeInfo.HumanReadableName));
			foodData.Content = 100;
			AssertNoMessageErrors(foodData.ContentInfo);
			foodData.Content = 9999999999.9999;
			AssertNoMessageErrors(foodData.ContentInfo);
			foodData.Content = 10000000000;
			AssertHasError(foodData.ContentInfo, "The number 10,000,000,000 is too large, the maximum value allowed for Content is 9,999,999,999.9999.");
		}

		[ExpectNoExceptions]
		public void TestCheckCY_CodeList()
		{
			foodData.CY_Code = "Z~Z";
			NUnit.Framework.Assert.That(!foodData.CY_CodeInfo.Notifications.GetMessageErrors().ContainsNotificationContaining(ListValidation.InvalidCodeMessageError.ToString()), NUnit.Framework.Is.True, "Do not check code list");
		}

		public void TestCheckCY_Data()
		{
			foodData.CY_Data = "\u5e38\u6eab";
			AssertNoMessageErrors(foodData.CY_DataInfo);
			var controllingMsgHeaderHelper = new ControllingMsgHeaderTestHelper(Factory);
			var jobDeclartion = controllingMsgHeaderHelper.New(new string[] { "IF", "DN", "CD" });
			var header = jobDeclartion.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", true);
			var food = line1.FoodDataCollection.AddNew();
			food.CY_Data = ZString.Empty;
			AssertNoMessageErrorContaining(food.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			food.CY_Data = "AA";
			AssertNoMessageErrorContaining(food.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", false);
			food = line1.FoodDataCollection.AddNew();
			food.CY_Data = ZString.Empty;
			AssertNoMessageErrorContaining(food.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			ControllingMsgHeaderTestHelper.SetControllingAgencyIsForCAHeader(line1, "CD", true);
			food.Content = 1M;
			food.CY_Data = ZString.Empty;
			AssertHasMessageErrorContaining(food.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
			food = line1.FoodDataCollection.AddNew();
			food.Content = 1M;
			food.CY_Data = "AA";
			AssertNoMessageErrorContaining(food.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var jobComInvoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			foodData = jobComInvoiceLine.FoodDataCollection.AddNew();
		}

		FoodData foodData;
	}
}

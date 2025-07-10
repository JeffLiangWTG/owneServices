using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105MessageHelper))]
	sealed class NX5105MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return CareateNX5105MessageHelper(TestXml);
		}

		NX5105MessageHelper CareateNX5105MessageHelper(string testXml)
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = "ICD";
			testMessage.EM_MessageText = testXml;
			return new NX5105MessageHelper(testMessage);
		}

		public static string TestXml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00336218.NX5105.xml");
		public static string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00336218.NX5105.html");
		public void TestErrorsIfMissingXmlNodes()
		{
			var xElement = XElement.Parse(TestXml);
			xElement.Descendants().Where(x => x.Name.LocalName == "tw_BondedGoodsMonthlyReport").Remove();
			AssertNoExceptionThrown(() => CareateNX5105MessageHelper(xElement.ToString()).ToHtml());
		}

		public void TestToHtml()
		{
			var helper = (NX5105MessageHelper)GetNewBusinessObject();
			var html = helper.ToHtml();
			AssertXMLEquals("xml must be the same", TestHtml, html);
		}

		public void TestDisplayTaxRateNumericEvenWhenZero()
		{
			var xElement = XElement.Parse(TestXml);
			xElement.Descendants().Where(x => x.Name.LocalName == "tw_DutyTaxFeeAmount").ForEach(dutyTaxFeeAmount => dutyTaxFeeAmount.Descendants().Where(n => n.Name.LocalName == "tw_TaxRateNumeric").ForEach(rate => rate.SetValue(0)));
			var html = CareateNX5105MessageHelper(xElement.ToString()).ToHtml();
			string expectedHtml = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00362795.N5105_DisplayTaxRateNumeric.html");
			AssertXMLEquals(expectedHtml, html);
		}
	}
}

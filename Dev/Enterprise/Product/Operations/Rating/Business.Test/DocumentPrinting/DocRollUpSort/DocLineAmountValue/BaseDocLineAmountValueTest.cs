using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort.DocLineAmountValue;

namespace Enterprise.Rating.Business.Test.DocumentPrinting.DocRollUpSort.DocLineAmountValue
{
	abstract class BaseDocLineAmountValueTest : TestCaseWithFactory
	{
		protected void AssertProperties(BaseDocLineAmountValue docLineAmount, string[] expectedProperties)
			=> AssertContainsExactElementsInAnyOrder
			(
				"Properties",
				expectedProperties,
				docLineAmount.Property.Select(value => $"{value.Key}:{value.Value}")
			);

		protected void AssertValues(BaseDocLineAmountValue docLineAmount, string[] expectedValues)
			=> AssertContainsExactElementsInAnyOrder
			(
				"Values",
				expectedValues,
				docLineAmount.Values.Select(value => $"{value.Key}:{value.Value}")
			);

		protected void AssertQuotationLineList(BaseDocLineAmountValue docLineAmount, string[] expectedQuotationLineList)
			=> AssertContainsExactElementsInAnyOrder
			(
				"QuotationLineList",
				expectedQuotationLineList,
				docLineAmount.GetQuotationLineList(DummyRateLine).Select(quotationLine => quotationLine.ToString())
			);

		RateLine DummyRateLine
		{
			get
			{
				if (rateLine == null)
				{
					var ratingHeader = Factory.New<ClientRate>();
					ratingHeader.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
					var rateEntry = ratingHeader.AddRateEntry("ORG");
					rateLine = rateEntry.RateLines.AddNew();
				}
				return rateLine;
			}
		}
		RateLine rateLine;
	}
}

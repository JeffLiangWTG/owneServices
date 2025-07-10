using System.Globalization;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.DocumentPrinting.DocAmount.Test
{
	abstract class DocAmountBaseTest : TestCase
	{
		public abstract void TestAmount();

		protected static void AssertDocAmountForCurrentCulture(DocAmount docAmount, string culture1, string culture2, string expectedAmount1, string expectedAmount2)
		{
			var initialCulture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture1);
				AssertEquals("culture1", expectedAmount1, docAmount.AmountAsString);

				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture2);
				AssertEquals("culture2", expectedAmount2, docAmount.AmountAsString);
			}
			finally
			{
				CultureInfo.CurrentCulture = initialCulture;
			}
		}

		protected static void AssertDocAmountForCurrentCompanyCulture(DocAmount docAmount, string culture1, string culture2, string expectedAmount1, string expectedAmount2)
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture(culture1)))
			{
				AssertEquals("culture1", expectedAmount1, docAmount.AmountAsString);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture(culture2)))
			{
				AssertEquals("culture2", expectedAmount2, docAmount.AmountAsString);
			}
		}
	}
}

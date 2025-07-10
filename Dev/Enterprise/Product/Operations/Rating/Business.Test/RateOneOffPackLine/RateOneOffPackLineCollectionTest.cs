using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	[TestedType(typeof(RateOneOffPackLineCollection))]
	public class RateOneOffPackLineCollectionTest : ActiveBusinessObjectCollectionTestCase<RateOneOffPackLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var looseCargo = Factory.NewWithValidTestData<RateOneOffPackLine>();
			return looseCargo;
		}

		protected override RateOneOffPackLineCollection GetCollectionToTest()
		{
			return new RateOneOffPackLineCollection(OneOffQuote);
		}

		public void TestLooseCargoItemCount()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.TH_OneTimeQuote = true;

			var loose1 = testQuote.CurrentOneOffQuote.LooseCargo.AddNew();
			loose1.TPL_PackLineCount = 3;

			var loose2 = testQuote.CurrentOneOffQuote.LooseCargo.AddNew();
			loose2.TPL_PackLineCount = 2;

			AssertEquals("5 packages in total", 5, testQuote.CurrentOneOffQuote.LooseCargo.LooseCargoPackageCount);
		}

		protected RateOneOffShipment OneOffQuote
		{
			get
			{
				if (oneOffQuote == null)
				{
					oneOffQuote = Factory.NewWithValidTestData<RateOneOffShipment>();
				}
				return oneOffQuote;
			}
		}
		RateOneOffShipment oneOffQuote;
	}
}

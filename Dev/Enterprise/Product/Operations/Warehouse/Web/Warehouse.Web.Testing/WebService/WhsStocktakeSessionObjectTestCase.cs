using System;
using Enterprise.Warehouse.Web.WebService.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsStocktakeSessionObjectTestCase : TestCase
	{
		#region Test Cases

		public void TestConstructor()
		{
			var stocktake = new WhsStocktakeInfo();
			var linesToCount = new WhsStocktakeLineInfoCollection();
			var stocktakeSessionObject = new WhsStocktakeSessionObject(stocktake, linesToCount);

			AssertEquals(stocktake, stocktakeSessionObject.Stocktake);
			AssertEquals(linesToCount, stocktakeSessionObject.LinesToCount);
		}

		public void TestGetLocationsToCount()
		{
			var stocktakeSessionObject = new WhsStocktakeSessionObject(new WhsStocktakeInfo(), GetLinesToCount());

			var locations = stocktakeSessionObject.GetLocationsToCount();

			AssertEquals("Locations Count", 3, locations.Count);
			AssertEquals("Location1", Location1, locations[0]);
			AssertEquals("Location3", Location3, locations[1]);
			AssertEquals("Location2", Location2, locations[2]);
		}

		public void TestGetLinesToCountForLocation()
		{
			var stocktakeSessionObject = new WhsStocktakeSessionObject(new WhsStocktakeInfo(), GetLinesToCount());

			var locations = stocktakeSessionObject.GetLocationsToCount();

			AssertEquals("Locations Count", 3, locations.Count);
			AssertLinesToCountForLocation("Location 1", Location1, 1, stocktakeSessionObject.GetLinesToCountForLocation(locations[0]));
			AssertLinesToCountForLocation("Location 3", Location3, 3, stocktakeSessionObject.GetLinesToCountForLocation(locations[1]));
			AssertLinesToCountForLocation("Location 2", Location2, 2, stocktakeSessionObject.GetLinesToCountForLocation(locations[2]));
		}

		public void TestGetLine()
		{
			var stocktakeSessionObject = new WhsStocktakeSessionObject(new WhsStocktakeInfo(), GetLinesToCount());

			foreach (var line in stocktakeSessionObject.LinesToCount)
			{
				AssertEquals(line, stocktakeSessionObject.GetLine(line.PK));
			}
		}

		#endregion

		#region Implementation

		void AssertLinesToCountForLocation(string message, string expectedLocation, int expectedLinesCount, WhsStocktakeLineInfoCollection actualLinesToCount)
		{
			AssertEquals(message + " - Lines Count", expectedLinesCount, actualLinesToCount.Count);
			foreach (var line in actualLinesToCount)
			{
				AssertEquals(message + " - Location", expectedLocation, line.LocationString);
			}
		}

		WhsStocktakeLineInfoCollection GetLinesToCount()
		{
			var linesToCount = new WhsStocktakeLineInfoCollection();
			linesToCount.Add(CreateLineInfo(Guid.NewGuid(), Location1));

			for (var i = 0; i < 3; i++)
			{
				linesToCount.Add(CreateLineInfo(Guid.NewGuid(), Location3));
			}

			for (var i = 0; i < 2; i++)
			{
				linesToCount.Add(CreateLineInfo(Guid.NewGuid(), Location2));
			}

			return linesToCount;
		}

		WhsStocktakeLineInfo CreateLineInfo(Guid pK, string location)
		{
			WhsStocktakeLineInfo lineInfo = new WhsStocktakeLineInfo();
			lineInfo.PK = pK;
			lineInfo.LocationString = location;
			return lineInfo;
		}

		const string Location1 = "L1";
		const string Location2 = "L2";
		const string Location3 = "L3";

		#endregion
	}
}

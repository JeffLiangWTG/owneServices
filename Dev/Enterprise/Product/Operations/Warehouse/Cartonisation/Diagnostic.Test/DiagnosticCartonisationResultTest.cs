using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Cartonisation.Business;
using Enterprise.Warehouse.Cartonisation.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	public class DiagnosticCartonisationResultTest : TestCase
	{
		public void TestToString()
		{
			var carton = new DummyCartonDefinition(10, 11, 12, 100, 1, 500, 1000, 90, "M", "M3", "KG", 500);
			carton.CartonName = "Big Box";
			var cartons = new CartonsCollection();
			cartons.Add(carton);

			var itemToPack = new DummyCartonisableItem(Guid.NewGuid(), 4, new DummyCartonisableItemDefinition(20, 21, 22, 23, 24, "CM", "D3", "G", false));
			itemToPack.ItemDefinition.ProductName = "Item";
			itemToPack.Location = "Location";
			var itemsToPack = new ItemsToPackCollection();
			itemsToPack.Add(itemToPack);

			var resultItem1 = new CartonWithItems();
			resultItem1.CartonPK = carton.PK.ToGuid();
			var contentResults1 = new List<IContentResult>();
			contentResults1.Add(new ContentResult(itemToPack.PK.ToGuid(), 2));
			contentResults1.Add(new ContentResult(Guid.NewGuid(), 1)); // unknown item
			resultItem1.Items = contentResults1;

			var resultItem2 = new CartonWithItems();
			resultItem2.CartonPK = Guid.NewGuid();
			resultItem2.Items = new List<IContentResult>();

			var result = new List<ICartonWithItems>();
			result.Add(resultItem1);
			result.Add(resultItem2);

			var diagResult = new DiagnosticCartonisationResult(cartons, itemsToPack, result, new TimeSpan(11, 22, 33));
			AssertMultilineASCIIEquals("Strings must match", string.Format(@"Algorithm took 11:22:33 to run
Big Box (12x11x10 M  500M3) Max Weight = 100KG Cost = 500 :
-->  2 x Item (20x22x21 CM, 24G) from Location
Produced CartonisableItemPK='{0}' was not in original list.
Produced CartonPK='{1}' was not in original list.", contentResults1[1].CartonisableItemPK, resultItem2.CartonPK), diagResult.ToString());
		}
	}
}

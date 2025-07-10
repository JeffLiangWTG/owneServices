using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using NUnit.Framework;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class UniversalPackingLineExtensionTest : TestCase
	{
		public void TestHasInnerPackingLines()
		{
			var packingLine = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			Assert("Packing line has no inners", !packingLine.HasInnerPackingLines());

			packingLine.SetPackingLineCollection(() => new List<UniversalPackingLine> { new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance) });
			Assert("Packing line has inners", packingLine.HasInnerPackingLines());
		}

		public void TestGetLastKnownTransitWarehouseStatusDateTime()
		{
			var packingLine = new UniversalPackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.LoadDate = new ZDateTime(2020, 6, 1, 10, 17, 0);
			packingLine.UnloadDate = ZDateTime.Empty;
			AssertEquals("LoadDate", new ZDateTime(2020, 6, 1, 10, 17, 0), packingLine.GetLastKnownTransitWarehouseStatusDateTime());

			packingLine.LoadDate = ZDateTime.Empty;
			AssertEquals("UnloadDate", ZDateTime.Empty, packingLine.GetLastKnownTransitWarehouseStatusDateTime());

			packingLine.UnloadDate = new ZDateTime(2020, 6, 1, 10, 18, 0);
			AssertEquals("UnloadDate", new ZDateTime(2020, 6, 1, 10, 18, 0), packingLine.GetLastKnownTransitWarehouseStatusDateTime());
		}
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.Rating.Integration;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Business.Testing
{
	public class RateTypeAttributeTest : TestCase
	{
		public void TestRateCategories()
		{
			foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
			{
				var fieldInfo = typeof(RatingConstants.RateCategory).GetField(rateCategory, BindingFlags.Public | BindingFlags.Static);
				Assert(rateCategory + " has RateType Attribute", fieldInfo.IsDefined(typeof(RateTypeAttribute), false));
				AssertEquals(rateCategory + " - the constant name and the value are the same", rateCategory, fieldInfo.Name);
				AssertNotNull(rateCategory + "RateEntryCollection is defined",
					Type.GetType("Enterprise.Rating.Business." + rateCategory + "RateEntryCollection, Enterprise.Rating.Business"));
			}
		}

		public void TestGetRateType()
		{
			var expectedRateTypes = new Dictionary<string, object[]>();
			expectedRateTypes.Add(RatingConstants.RateCategory.AIR, new object[] { RateType.Forwarding, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.FCL, new object[] { RateType.Forwarding, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.LCL, new object[] { RateType.Forwarding, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.ORG, new object[] { RateType.Forwarding, false, true, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.DST, new object[] { RateType.Forwarding, false, false, true });

			// Customs Rates
			expectedRateTypes.Add(RatingConstants.RateCategory.CAI, new object[] { RateType.Customs, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.CFC, new object[] { RateType.Customs, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.CLC, new object[] { RateType.Customs, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.COR, new object[] { RateType.Customs, false, true, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.CDS, new object[] { RateType.Customs, false, false, true });

			expectedRateTypes.Add(RatingConstants.RateCategory.SCO, new object[] { RateType.Shipping, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.SNC, new object[] { RateType.Shipping, true, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.SOR, new object[] { RateType.Shipping, false, true, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.SDE, new object[] { RateType.Shipping, false, false, true });
			expectedRateTypes.Add(RatingConstants.RateCategory.SED, new object[] { RateType.ShippingExportDetention, false, true, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.SID, new object[] { RateType.ShippingImportDetention, false, false, true });
			expectedRateTypes.Add(RatingConstants.RateCategory.PAC, new object[] { RateType.CFS, false, true, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.UNP, new object[] { RateType.CFS, false, false, true });
			expectedRateTypes.Add(RatingConstants.RateCategory.CST, new object[] { RateType.CFS, false, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.WHS, new object[] { RateType.Warehouse, false, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.TRW, new object[] { RateType.TransitWarehouse, false, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.TWU, new object[] { RateType.TransitWarehouseTransportationUnit, false, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.TRN, new object[] { RateType.LocalTransport, false, true, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.TBC, new object[] { RateType.TransportBookings, false, true, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.CYD, new object[] { RateType.ContainerYard, false, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.CYU, new object[] { RateType.ContainerYardTransportationUnit, false, false, false });
			expectedRateTypes.Add(RatingConstants.RateCategory.CYM, new object[] { RateType.ContainerYard, false, false, false });

			foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
			{
				var fieldInfo = typeof(RatingConstants.RateCategory).GetField(rateCategory, BindingFlags.Public | BindingFlags.Static);
				AssertEquals(rateCategory + " RateType", expectedRateTypes[rateCategory][0], RatingConstants.RateCategory.GetRateType(rateCategory));
				AssertEquals(rateCategory + " IsFreight", expectedRateTypes[rateCategory][1], RatingConstants.RateCategory.IsFreight(rateCategory));
				AssertEquals(rateCategory + " IsOrigin", expectedRateTypes[rateCategory][2], RatingConstants.RateCategory.IsOrigin(rateCategory));
				AssertEquals(rateCategory + " IsDestination", expectedRateTypes[rateCategory][3], RatingConstants.RateCategory.IsDestination(rateCategory));
			}
		}

		public void TestGetRateCategories()
		{
			AssertRateCategories(RateType.Forwarding, RateCategoryGroup.All, "AIR", "DST", "FCL", "LCL", "ORG");
			AssertRateCategories(RateType.Forwarding, RateCategoryGroup.Freight, "AIR", "FCL", "LCL");
			AssertRateCategories(RateType.Forwarding, RateCategoryGroup.NonFreight, "DST", "ORG");
			AssertRateCategories(RateType.Forwarding, RateCategoryGroup.Origin, "ORG");
			AssertRateCategories(RateType.Forwarding, RateCategoryGroup.Destination, "DST");
			AssertRateCategories(RateType.Forwarding, RateCategoryGroup.OtherSupplementary);

			AssertRateCategories(RateType.Shipping, RateCategoryGroup.All, "SCO", "SNC", "SOR", "SDE");
			AssertRateCategories(RateType.Shipping, RateCategoryGroup.Freight, "SCO", "SNC");
			AssertRateCategories(RateType.Shipping, RateCategoryGroup.NonFreight, "SOR", "SDE");
			AssertRateCategories(RateType.Shipping, RateCategoryGroup.Origin, "SOR");
			AssertRateCategories(RateType.Shipping, RateCategoryGroup.Destination, "SDE");
			AssertRateCategories(RateType.Shipping, RateCategoryGroup.OtherSupplementary);

			AssertRateCategories(RateType.ShippingExportDetention, RateCategoryGroup.All, "SED");
			AssertRateCategories(RateType.ShippingExportDetention, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.ShippingExportDetention, RateCategoryGroup.NonFreight, "SED");
			AssertRateCategories(RateType.ShippingExportDetention, RateCategoryGroup.Origin, "SED");
			AssertRateCategories(RateType.ShippingExportDetention, RateCategoryGroup.Destination);
			AssertRateCategories(RateType.ShippingExportDetention, RateCategoryGroup.OtherSupplementary);

			AssertRateCategories(RateType.ShippingImportDetention, RateCategoryGroup.All, "SID");
			AssertRateCategories(RateType.ShippingImportDetention, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.ShippingImportDetention, RateCategoryGroup.NonFreight, "SID");
			AssertRateCategories(RateType.ShippingImportDetention, RateCategoryGroup.Origin);
			AssertRateCategories(RateType.ShippingImportDetention, RateCategoryGroup.Destination, "SID");
			AssertRateCategories(RateType.ShippingImportDetention, RateCategoryGroup.OtherSupplementary);

			AssertRateCategories(RateType.CFS, RateCategoryGroup.All, "CST", "PAC", "UNP");
			AssertRateCategories(RateType.CFS, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.CFS, RateCategoryGroup.NonFreight, "CST", "PAC", "UNP");
			AssertRateCategories(RateType.CFS, RateCategoryGroup.Origin, "PAC");
			AssertRateCategories(RateType.CFS, RateCategoryGroup.Destination, "UNP");
			AssertRateCategories(RateType.CFS, RateCategoryGroup.OtherSupplementary, "CST");

			AssertRateCategories(RateType.Warehouse, RateCategoryGroup.All, "WHS");
			AssertRateCategories(RateType.Warehouse, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.Warehouse, RateCategoryGroup.NonFreight, "WHS");
			AssertRateCategories(RateType.Warehouse, RateCategoryGroup.Origin);
			AssertRateCategories(RateType.Warehouse, RateCategoryGroup.Destination);
			AssertRateCategories(RateType.Warehouse, RateCategoryGroup.OtherSupplementary, "WHS");

			AssertRateCategories(RateType.TransitWarehouse, RateCategoryGroup.All, "TRW");
			AssertRateCategories(RateType.TransitWarehouse, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.TransitWarehouse, RateCategoryGroup.NonFreight, "TRW");
			AssertRateCategories(RateType.TransitWarehouse, RateCategoryGroup.Origin);
			AssertRateCategories(RateType.TransitWarehouse, RateCategoryGroup.Destination);
			AssertRateCategories(RateType.TransitWarehouse, RateCategoryGroup.OtherSupplementary, "TRW");

			AssertRateCategories(RateType.TransitWarehouseTransportationUnit, RateCategoryGroup.All, "TWU");
			AssertRateCategories(RateType.TransitWarehouseTransportationUnit, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.TransitWarehouseTransportationUnit, RateCategoryGroup.NonFreight, "TWU");
			AssertRateCategories(RateType.TransitWarehouseTransportationUnit, RateCategoryGroup.Origin);
			AssertRateCategories(RateType.TransitWarehouseTransportationUnit, RateCategoryGroup.Destination);
			AssertRateCategories(RateType.TransitWarehouseTransportationUnit, RateCategoryGroup.OtherSupplementary, "TWU");

			AssertRateCategories(RateType.LocalTransport, RateCategoryGroup.All, "TRN");
			AssertRateCategories(RateType.LocalTransport, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.LocalTransport, RateCategoryGroup.NonFreight, "TRN");
			AssertRateCategories(RateType.LocalTransport, RateCategoryGroup.Origin, "TRN");
			AssertRateCategories(RateType.LocalTransport, RateCategoryGroup.Destination);
			AssertRateCategories(RateType.LocalTransport, RateCategoryGroup.OtherSupplementary);

			AssertRateCategories(RateType.TransportBookings, RateCategoryGroup.All, "TBC");
			AssertRateCategories(RateType.TransportBookings, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.TransportBookings, RateCategoryGroup.NonFreight, "TBC");
			AssertRateCategories(RateType.TransportBookings, RateCategoryGroup.Origin, "TBC");
			AssertRateCategories(RateType.TransportBookings, RateCategoryGroup.Destination);
			AssertRateCategories(RateType.TransportBookings, RateCategoryGroup.OtherSupplementary);

			AssertRateCategories(RateType.ContainerYard, RateCategoryGroup.All, "CYD", "CYM");
			AssertRateCategories(RateType.ContainerYard, RateCategoryGroup.Freight);
			AssertRateCategories(RateType.ContainerYard, RateCategoryGroup.NonFreight, "CYD", "CYM");
			AssertRateCategories(RateType.ContainerYard, RateCategoryGroup.Origin);
			AssertRateCategories(RateType.ContainerYard, RateCategoryGroup.Destination);
			AssertRateCategories(RateType.ContainerYard, RateCategoryGroup.OtherSupplementary, "CYD", "CYM");

			var allTypes = RateType.Forwarding | RateType.Shipping | RateType.CFS | RateType.Warehouse | RateType.TransportBookings | RateType.LocalTransport | RateType.ContainerYard;
			AssertRateCategories(allTypes, RateCategoryGroup.All, "AIR", "CST", "DST", "FCL", "LCL", "ORG", "SCO", "SNC", "SOR", "SDE", "PAC", "TRN", "TBC", "UNP", "WHS", "CYD", "CYM");
			AssertRateCategories(allTypes, RateCategoryGroup.Freight, "AIR", "FCL", "LCL", "SCO", "SNC");
			AssertRateCategories(allTypes, RateCategoryGroup.NonFreight, "CST", "DST", "ORG", "SOR", "SDE", "PAC", "TRN", "TBC", "UNP", "WHS", "CYD", "CYM");
			AssertRateCategories(allTypes, RateCategoryGroup.Origin, "ORG", "SOR", "PAC", "TRN", "TBC");
			AssertRateCategories(allTypes, RateCategoryGroup.Destination, "DST", "SDE", "UNP");
			AssertRateCategories(allTypes, RateCategoryGroup.OtherSupplementary, "CST", "WHS", "CYD", "CYM");
		}

		void AssertRateCategories(RateType rateType, RateCategoryGroup rateCategoryGroup, params string[] expected)
		{
			var result = RatingConstants.RateCategory.GetRateCategories(rateType, rateCategoryGroup);
			AssertContainsExactElementsInAnyOrder(string.Format("{0}, {1}", rateType, rateCategoryGroup), expected, result);
		}

		public void TestGetDataContexts()
		{
			var expectedDataContexts = new Dictionary<string, DataContext[]>();
			expectedDataContexts.Add(RatingConstants.RateCategory.AIR, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.FCL, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.LCL, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.ORG, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.DST, new DataContext[] { DataContext.Rating, DataContext.Quotation });

			// Customs Rates
			expectedDataContexts.Add(RatingConstants.RateCategory.CAI, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.CFC, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.CLC, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.COR, new DataContext[] { DataContext.Rating, DataContext.Quotation });
			expectedDataContexts.Add(RatingConstants.RateCategory.CDS, new DataContext[] { DataContext.Rating, DataContext.Quotation });

			expectedDataContexts.Add(RatingConstants.RateCategory.SCO, new DataContext[] { DataContext.ShippingRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.SNC, new DataContext[] { DataContext.ShippingRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.SOR, new DataContext[] { DataContext.ShippingRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.SDE, new DataContext[] { DataContext.ShippingRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.SED, new DataContext[] { DataContext.ShippingDetentionRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.SID, new DataContext[] { DataContext.ShippingDetentionRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.PAC, new DataContext[] { DataContext.CFSRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.UNP, new DataContext[] { DataContext.CFSRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.CST, new DataContext[] { DataContext.None });
			expectedDataContexts.Add(RatingConstants.RateCategory.WHS, new DataContext[] { DataContext.WarehouseRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.TRW, new DataContext[] { DataContext.WarehouseRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.TWU, new DataContext[] { DataContext.WarehouseRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.TRN, new DataContext[] { DataContext.TransportRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.TBC, new DataContext[] { DataContext.TransportRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.CYD, new DataContext[] { DataContext.ContainerYardRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.CYU, new DataContext[] { DataContext.ContainerYardRating });
			expectedDataContexts.Add(RatingConstants.RateCategory.CYM, new DataContext[] { DataContext.ContainerYardRating });

			foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
			{
				var fieldInfo = typeof(RatingConstants.RateCategory).GetField(rateCategory, BindingFlags.Public | BindingFlags.Static);
				var expected = expectedDataContexts[rateCategory];
				var actual = RatingConstants.RateCategory.GetDataContexts(rateCategory);
				AssertEquals(rateCategory + " DataContexts.Length", expected.Length, actual.Length);
				for (var i = 0; i < expected.Length; i++)
				{
					AssertEquals(string.Format("{0} DataContexts[{1}]", rateCategory, i), expected[i], actual[i]);
				}
			}
		}
	}
}

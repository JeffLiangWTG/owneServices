using System;
using System.Text;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;
using NUnit.Framework;
using Microsoft.OData.Edm;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	class CompareHelperFixture
	{
		[Test]
		public void Compare()
		{
			var a = "a";
			var b = "A";

			Assert.False(CompareHelper.MatchPropertyValues(a.GetType(), a, b, Operations.Equals));
			Assert.True(CompareHelper.MatchPropertyValues(a.GetType(), a, b, Operations.Equals, StringComparison.OrdinalIgnoreCase));

			var c = 1;
			var d = 2;

			Assert.False(CompareHelper.MatchPropertyValues(c.GetType(), c, d, Operations.Equals));

			var e = "ABC";
			var f = "AB";

			Assert.True(CompareHelper.MatchPropertyValues(e.GetType(), e, f, Operations.StartsWith));
			Assert.False(CompareHelper.MatchPropertyValues(a.GetType(), e, f, Operations.Equals));

			var g = 2;
			var h = 2;

			Assert.True(CompareHelper.MatchPropertyValues(g.GetType(), g, h, Operations.Equals));
			Assert.Throws(typeof(InvalidOperationException), () => CompareHelper.MatchPropertyValues(g.GetType(), g, h, Operations.StartsWith));

			var i = new Date(2024, 1, 1);
			var j = new Date(2024, 1, 1);
			Assert.That(CompareHelper.MatchPropertyValues(i.GetType(), i, j, Operations.Equals));

			var unloco1 = new SerializedGeometry() { Geography = new GeographyWellKnownValue() { CoordinateSystemId = 4326, WellKnownText = "POINT(-148.083 -19.900)" } };
			var unloco2 = new SerializedGeometry() { Geography = new GeographyWellKnownValue() { CoordinateSystemId = 4326, WellKnownText = "POINT(-148.083 -19.900)" } };
			Assert.True(CompareHelper.MatchPropertyValues(unloco1.GetType(), unloco1, unloco2, Operations.Equals));
			unloco2 = new SerializedGeometry() { Geography = new GeographyWellKnownValue() { CoordinateSystemId = 4326, WellKnownText = "POINT(-158.083 -29.900)" } };
			Assert.False(CompareHelper.MatchPropertyValues(unloco1.GetType(), unloco1, unloco2, Operations.Equals));
			unloco2 = new SerializedGeometry() { Geography = new GeographyWellKnownValue() { CoordinateSystemId = 4326, WellKnownText = "SRID=4326;POINT(-148.083 -19.900)" } };
			Assert.True(CompareHelper.MatchPropertyValues(unloco1.GetType(), unloco1, unloco2, Operations.Equals));

			var byte1 = Encoding.Unicode.GetBytes("byte Test");
			var byte2 = Encoding.Unicode.GetBytes("byte Test");
			Assert.True(CompareHelper.MatchPropertyValues(typeof(byte[]), byte1, byte2));
		}

		[Test]
		public void IsEqualsStringCaseSensitive()
		{
			Assert.True(CompareHelper.IsEquals("a", "A"));
			Assert.False(CompareHelper.IsEquals("a", "A", StringComparison.Ordinal));
		}
	}
}

using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class PackingLineExtensionsTest : TestCase
	{
		#region GetPackTypeCode

		public void TestGetPackTypeCode_Null()
		{
			IPackingLine[] packingLines = null;
			AssertEquals("GetPackTypeCode", string.Empty, packingLines.GetPackTypeCode());
		}

		public void TestGetPackTypeCode_EmptyCollection()
		{
			var packingLines = System.Array.Empty<IPackingLine>();
			AssertEquals("GetPackTypeCode", string.Empty, packingLines.GetPackTypeCode());
		}

		public void TestGetPackTypeCode_OnePackingLine()
		{
			var packingLine = new Mock<IPackingLine>();
			var packageType = new Mock<ICodeDescription>();

			packageType
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine
				.SetupGet(p => p.PackageType)
				.Returns(packageType.Object);

			var packingLines = new[]
			{
				packingLine.Object
			};

			AssertEquals("GetPackTypeCode", Core.Constants.PkgUnit.Bottle, packingLines.GetPackTypeCode());
		}

		public void TestGetPackTypeCode_PackingLines_SameUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var packageType1 = new Mock<ICodeDescription>();

			packageType1
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine1
				.SetupGet(p => p.PackageType)
				.Returns(packageType1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var packageType2 = new Mock<ICodeDescription>();

			packageType2
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine2
				.SetupGet(p => p.PackageType)
				.Returns(packageType2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("GetPackTypeCode", Core.Constants.PkgUnit.Bottle, packingLines.GetPackTypeCode());
		}

		public void TestGetPackTypeCode_PackingLines_DifferentUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var packageType1 = new Mock<ICodeDescription>();

			packageType1
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine1
				.SetupGet(p => p.PackageType)
				.Returns(packageType1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var packageType2 = new Mock<ICodeDescription>();

			packageType2
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bag);

			packingLine2
				.SetupGet(p => p.PackageType)
				.Returns(packageType2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("GetPackTypeCode", Core.Constants.PkgUnit.Package, packingLines.GetPackTypeCode());
		}

		#endregion

		#region TryGetPackTypeCode

		public void TestTryGetPackTypeCode_Null()
		{
			IPackingLine[] packingLines = null;

			AssertEquals("TryGetPackTypeCode", false, packingLines.TryGetPackTypeCode(out var packTypeCode));
			AssertEquals("TryGetPackTypeCode", string.Empty, packTypeCode);
		}

		public void TestTryGetPackTypeCode_EmptyCollection()
		{
			var packingLines = System.Array.Empty<IPackingLine>();

			AssertEquals("TryGetPackTypeCode", false, packingLines.TryGetPackTypeCode(out var packTypeCode));
			AssertEquals("TryGetPackTypeCode", string.Empty, packTypeCode);
		}

		public void TestTryGetPackTypeCode_OnePackingLine()
		{
			var packingLine = new Mock<IPackingLine>();
			var packageType = new Mock<ICodeDescription>();

			packageType
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine
				.SetupGet(p => p.PackageType)
				.Returns(packageType.Object);

			var packingLines = new[]
			{
				packingLine.Object
			};

			AssertEquals("TryGetPackTypeCode", true, packingLines.TryGetPackTypeCode(out var packTypeCode));
			AssertEquals("TryGetPackTypeCode", Core.Constants.PkgUnit.Bottle, packTypeCode);
		}

		public void TestTryGetPackTypeCode_PackingLines_SameUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var packageType1 = new Mock<ICodeDescription>();

			packageType1
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine1
				.SetupGet(p => p.PackageType)
				.Returns(packageType1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var packageType2 = new Mock<ICodeDescription>();

			packageType2
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine2
				.SetupGet(p => p.PackageType)
				.Returns(packageType2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("TryGetPackTypeCode", true, packingLines.TryGetPackTypeCode(out var packTypeCode));
			AssertEquals("TryGetPackTypeCode", Core.Constants.PkgUnit.Bottle, packTypeCode);
		}

		public void TestTryGetPackTypeCode_PackingLines_DifferentUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var packageType1 = new Mock<ICodeDescription>();

			packageType1
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bottle);

			packingLine1
				.SetupGet(p => p.PackageType)
				.Returns(packageType1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var packageType2 = new Mock<ICodeDescription>();

			packageType2
				.SetupGet(pt => pt.Code)
				.Returns(Core.Constants.PkgUnit.Bag);

			packingLine2
				.SetupGet(p => p.PackageType)
				.Returns(packageType2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("TryGetPackTypeCode", false, packingLines.TryGetPackTypeCode(out var packTypeCode));
			AssertEquals("TryGetPackTypeCode", string.Empty, packTypeCode);
		}

		#endregion

		#region HaveSameUnitOfWeight

		public void TestHaveSameUnitOfWeight_Null()
		{
			IPackingLine[] packingLines = null;
			AssertEquals("HaveSameUnitOfWeight", false, packingLines.HaveSameUnitOfWeight());
		}

		public void TestHaveSameUnitOfWeight_SameUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var weight1 = new Mock<IMeasurement>();
			var unitOfWeight1 = new Mock<ICodeDescription>();

			unitOfWeight1
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Weight.Kilograms);

			weight1
				.SetupGet(w => w.Unit)
				.Returns(unitOfWeight1.Object);

			packingLine1
				.SetupGet(p => p.Weight)
				.Returns(weight1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var weight2 = new Mock<IMeasurement>();
			var unitOfWeight2 = new Mock<ICodeDescription>();

			unitOfWeight2
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Weight.Kilograms);

			weight2
				.SetupGet(w => w.Unit)
				.Returns(unitOfWeight2.Object);

			packingLine2
				.SetupGet(p => p.Weight)
				.Returns(weight2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("HaveSameUnitOfWeight", true, packingLines.HaveSameUnitOfWeight());
		}

		public void TestHaveSameUnitOfWeight_DifferentUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var weight1 = new Mock<IMeasurement>();
			var unitOfWeight1 = new Mock<ICodeDescription>();

			unitOfWeight1
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Weight.Kilograms);

			weight1
				.SetupGet(w => w.Unit)
				.Returns(unitOfWeight1.Object);

			packingLine1
				.SetupGet(p => p.Weight)
				.Returns(weight1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var weight2 = new Mock<IMeasurement>();
			var unitOfWeight2 = new Mock<ICodeDescription>();

			unitOfWeight2
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Weight.Ounces);

			weight2
				.SetupGet(w => w.Unit)
				.Returns(unitOfWeight2.Object);

			packingLine2
				.SetupGet(p => p.Weight)
				.Returns(weight2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("HaveSameUnitOfWeight", false, packingLines.HaveSameUnitOfWeight());
		}

		#endregion

		#region HaveSameUnitOfWeight

		public void TestHaveSameUnitOfVolume_Null()
		{
			IPackingLine[] packingLines = null;
			AssertEquals("HaveSameUnitOfVolume", false, packingLines.HaveSameUnitOfVolume());
		}

		public void TestHaveSameUnitOfVolume_SameUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var volume1 = new Mock<IMeasurement>();
			var unitOfVolume1 = new Mock<ICodeDescription>();

			unitOfVolume1
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Volume.CubicMetres);

			volume1
				.SetupGet(w => w.Unit)
				.Returns(unitOfVolume1.Object);

			packingLine1
				.SetupGet(p => p.Volume)
				.Returns(volume1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var volume2 = new Mock<IMeasurement>();
			var unitOfVolume2 = new Mock<ICodeDescription>();

			unitOfVolume2
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Volume.CubicMetres);

			volume2
				.SetupGet(w => w.Unit)
				.Returns(unitOfVolume2.Object);

			packingLine2
				.SetupGet(p => p.Volume)
				.Returns(volume2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("HaveSameUnitOfVolume", true, packingLines.HaveSameUnitOfVolume());
		}

		public void TestHaveSameUnitOfVolume_DifferentUnit()
		{
			var packingLine1 = new Mock<IPackingLine>();
			var volume1 = new Mock<IMeasurement>();
			var unitOfVolume1 = new Mock<ICodeDescription>();

			unitOfVolume1
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Volume.CubicMetres);

			volume1
				.SetupGet(w => w.Unit)
				.Returns(unitOfVolume1.Object);

			packingLine1
				.SetupGet(p => p.Volume)
				.Returns(volume1.Object);

			var packingLine2 = new Mock<IPackingLine>();
			var volume2 = new Mock<IMeasurement>();
			var unitOfVolume2 = new Mock<ICodeDescription>();

			unitOfVolume2
				.SetupGet(uw => uw.Code)
				.Returns(Core.Constants.Volume.CubicFeet);

			volume2
				.SetupGet(w => w.Unit)
				.Returns(unitOfVolume2.Object);

			packingLine2
				.SetupGet(p => p.Volume)
				.Returns(volume2.Object);

			var packingLines = new[]
			{
				packingLine1.Object,
				packingLine2.Object
			};

			AssertEquals("HaveSameUnitOfVolume", false, packingLines.HaveSameUnitOfVolume());
		}

		#endregion
	}
}

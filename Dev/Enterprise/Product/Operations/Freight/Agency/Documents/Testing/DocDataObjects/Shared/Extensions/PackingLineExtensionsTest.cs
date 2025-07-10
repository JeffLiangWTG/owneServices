using Enterprise.DocumentVisualizer.DocDataObjects;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class PackingLineExtensionsTest : TestCase
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
	}
}

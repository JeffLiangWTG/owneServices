using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PackagingTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestQuantityQuantity()
		{
			NUnit.Framework.Assert.That(packaging.QuantityQuantity, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(packaging.TypeCode, NUnit.Framework.Is.EqualTo("KG").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMarksNumbers()
		{
			NUnit.Framework.Assert.That(packaging.MarksNumbers, NUnit.Framework.Is.EqualTo("F342").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackagingMaterialDescription()
		{
			moveLine.TW_IsCoPackaged = true;
			NUnit.Framework.Assert.That(packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo("GG534").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCombination()
		{
			moveLine.TW_IsCoPackaged = false;
			NUnit.Framework.Assert.That(packaging.Combination, NUnit.Framework.Is.EqualTo(ZString.Empty));
			moveLine.TW_IsCoPackaged = true;
			NUnit.Framework.Assert.That(packaging.Combination, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var arrivalBill = header.ArrivalBill;
			arrivalBill.B0_ManifestUQ = "KG";
			var movementBill = header.MovementBill;
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			moveLine = moveDetail.InBondMoveLineItem;
			moveLine.BI_MarksAndNumbers = "F342";
			moveLine.BI_PackagingDescription = "GG534";
			packaging = new Packaging(moveLine, header);
		}

		CusInBondMoveLineItem moveLine;
		IPackaging packaging;
	}
}

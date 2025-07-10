using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingWarpper))]
	sealed class PackingWarpperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPackingWarpper()
		{
			IPackaging packing = new PackingWarpper(ZDateTime.BrettsBirthday, "TPC", 123.456M);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(packing.PackingDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday.Date), "PackingDateTime");
				NUnit.Framework.Assert.That(packing.QuantityQuantity, NUnit.Framework.Is.EqualTo(123.456M).Using(CustomComparers.TypeComparison), "QuantityQuantity");
				NUnit.Framework.Assert.That(packing.TypeCode, NUnit.Framework.Is.EqualTo("TPC").Using(CustomComparers.TypeComparison), "TypeCode");
				NUnit.Framework.Assert.That(packing.MarksNumbers.ToString(), NUnit.Framework.Is.Null.Or.Empty, "MarksNumbers - should be [null] or [empty]");
				NUnit.Framework.Assert.That(packing.PackagingMaterialDescription.ToString(), NUnit.Framework.Is.Null.Or.Empty, "PackagingMaterialDescription - should be [null] or [empty]");
				NUnit.Framework.Assert.That(packing.Combination.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Combination - should be [null] or [empty]");
			});
		}
	}
}

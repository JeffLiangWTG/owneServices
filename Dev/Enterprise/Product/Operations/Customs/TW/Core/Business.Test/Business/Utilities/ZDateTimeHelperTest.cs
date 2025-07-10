using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ZDateTimeHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestToTaiWanDateString()
		{
			NUnit.Framework.Assert.That(ZDateTime.Empty.ToTaiWanDateString(), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(ZDateTime.Invalid.ToTaiWanDateString(), NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(new ZDateTime(2020, 6, 3).ToTaiWanDateString(), NUnit.Framework.Is.EqualTo("109年06月03日").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestToTaiWanShortYear()
		{
			NUnit.Framework.Assert.That(new ZDateTime(2019, 01, 01).ToTaiWanShortYear(), NUnit.Framework.Is.EqualTo("08").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(new ZDateTime(2018, 01, 01).ToTaiWanShortYear(), NUnit.Framework.Is.EqualTo("07").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(new ZDateTime(2017, 01, 01).ToTaiWanShortYear(), NUnit.Framework.Is.EqualTo("06").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(new ZDateTime(2007, 01, 01).ToTaiWanShortYear(), NUnit.Framework.Is.EqualTo("96").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestToTaiWanYear()
		{
			NUnit.Framework.Assert.That(new ZDateTime(2019, 01, 01).ToTaiWanYear(), NUnit.Framework.Is.EqualTo("108").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(new ZDateTime(2018, 01, 01).ToTaiWanYear(), NUnit.Framework.Is.EqualTo("107").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(new ZDateTime(2017, 01, 01).ToTaiWanYear(), NUnit.Framework.Is.EqualTo("106").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestToTaiWanShortDate()
		{
			NUnit.Framework.Assert.That(new ZDateTime(2019, 01, 01).ToTaiWanShortDate(), NUnit.Framework.Is.EqualTo("080101").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(new ZDateTime(2018, 05, 02).ToTaiWanShortDate(), NUnit.Framework.Is.EqualTo("070502").Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(new ZDateTime(2017, 11, 12).ToTaiWanShortDate(), NUnit.Framework.Is.EqualTo("061112").Using(CustomComparers.TypeComparison));
		}
	}
}

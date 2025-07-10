using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BoxNumberProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetBoxNumberList()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var provider = new BoxNumberProvider() as IBoxNumberProvider;
			var list = provider.GetBoxNumberList(Factory, "A");
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.Cast<CodeDescriptionPair>().Any(x => x.Code == "600"), NUnit.Framework.Is.True);
			list = provider.GetBoxNumberList(Factory, "B");
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.Cast<CodeDescriptionPair>().Any(x => x.Code == "100"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(list.Cast<CodeDescriptionPair>().Any(x => x.Code == "300"), NUnit.Framework.Is.True);
			list = provider.GetBoxNumberList(Factory, "C");
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestGetDefaultBoxNumber()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var provider = new BoxNumberProvider() as IBoxNumberProvider;
			NUnit.Framework.Assert.That(provider.GetDefaultBoxNumber(Factory, "A"), NUnit.Framework.Is.EqualTo("600").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(provider.GetDefaultBoxNumber(Factory, "B"), NUnit.Framework.Is.EqualTo("100").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(provider.GetDefaultBoxNumber(Factory, "C"), NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}

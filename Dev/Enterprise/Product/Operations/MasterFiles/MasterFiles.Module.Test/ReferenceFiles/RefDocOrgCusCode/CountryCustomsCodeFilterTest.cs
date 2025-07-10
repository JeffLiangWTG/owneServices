using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CountryCustomsCodeFilter))]
	sealed class CountryCustomsCodeFilterTest : ModuleCodeFilterTest
	{
		public void TestUpdateTypeListDelegate()
		{
			var isDelegateCalled = false;

			void updateListDelegate(IList list, ZString property) => isDelegateCalled = true;

			ZQuery queryDelegate(ZString value1, ZString value2) => new ZQuery();

			var filter = new CountryCustomsCodeFilter("Test Filter", queryDelegate, new RefCountryCollection(Factory), new CodeDescriptionPairList(), updateListDelegate, null)
			{
				Property1 = "AU"
			};

			AssertEquals(true, isDelegateCalled);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CodeDescriptionPairList defaultListDelegate() => new CodeDescriptionPairList();

			ZQuery queryDelegate(ZString value1, ZString value2) => new ZQuery();

			return new CountryCustomsCodeFilter("TEST",
				queryDelegate,
				new RefCountryCollection(Factory),
				new CodeDescriptionPairList(),
				null,
				defaultListDelegate);
		}
	}
}

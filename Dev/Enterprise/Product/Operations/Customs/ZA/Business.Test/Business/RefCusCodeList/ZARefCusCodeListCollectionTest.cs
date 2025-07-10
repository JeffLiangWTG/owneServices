using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ZARefCusCodeListCollection))]
	sealed class ZZRefCusCodeListCombinedCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(ZARefCusCodeListCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
			=> new ZARefCusCodeListCollection(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today);
	}
}

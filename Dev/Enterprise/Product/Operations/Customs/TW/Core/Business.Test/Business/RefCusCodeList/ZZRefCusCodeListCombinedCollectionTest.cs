using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWRefCusCodeListCollection))]
	sealed class ZZRefCusCodeListCombinedCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(TWRefCusCodeListCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TWRefCusCodeListCollection(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today);
		}
	}
}

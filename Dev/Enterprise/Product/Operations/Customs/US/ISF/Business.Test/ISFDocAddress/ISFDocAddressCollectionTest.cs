using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFDocAddressCollection))]
	sealed class ISFDocAddressCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(ISFDocAddressCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new ISFDocAddressCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ISFDocAddress>();
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFDocAddressCollectionForPlugin))]
	sealed class ISFDocAddressCollectionForPluginTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(ISFDocAddressCollectionForPlugin);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var collection = new ISFDocAddressCollection(Factory);
			return new ISFDocAddressCollectionForPlugin(collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ISFDocAddress>();
	}
}

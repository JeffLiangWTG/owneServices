using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(CusPersonCollection))]
	sealed class CusPersonCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(CusPersonCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => Factory.NewWithValidTestData<AsycudaManifestHeader>().Persons;
	}
}

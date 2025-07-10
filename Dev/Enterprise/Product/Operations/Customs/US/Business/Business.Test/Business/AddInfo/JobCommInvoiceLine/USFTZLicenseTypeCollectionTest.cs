using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USFTZLicenseTypeCollection))]
	sealed class USFTZLicenseTypeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(USFTZLicenseTypeCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new USFTZLicenseTypeCollection(Factory, ZDate.Today);
	}
}

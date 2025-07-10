using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USAESLicenseCodeCollection))]
	sealed class USAESLicenseCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(USAESLicenseCodeCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USAESLicenseCodeCollection(Factory);
		}
	}
}

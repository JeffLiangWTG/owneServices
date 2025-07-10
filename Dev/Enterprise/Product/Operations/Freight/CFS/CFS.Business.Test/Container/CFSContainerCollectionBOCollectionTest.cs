using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSContainerCollection))]
	internal sealed class CFSContainerCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CFSLoadListConsol parent = Factory.New<CFSLoadListConsol>();
			return new CFSContainerCollection(parent, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CFSContainer>();
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CFSContainerCollection);
		}
	}
}

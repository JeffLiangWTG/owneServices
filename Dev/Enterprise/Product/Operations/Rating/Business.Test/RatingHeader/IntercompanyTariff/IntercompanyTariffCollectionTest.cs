using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(IntercompanyTariffCollection))]
	public class IntercompanyTariffCollectionTest : RatingHeaderCollectionBaseTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IntercompanyTariffCollection(Factory);
		}

		protected override Type GetExpectedFindBoxListProviderType()
		{
			return typeof(RatingHeaderFindBoxListProviderForOrganisationCode);
		}
	}
}

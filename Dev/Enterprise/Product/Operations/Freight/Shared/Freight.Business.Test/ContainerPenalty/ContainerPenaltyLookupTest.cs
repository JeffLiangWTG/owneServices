using CargoWise.EntityFramework;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerPenaltyLookupTest : TestCase
	{
		public void TestPenaltyTypeList()
		{
			var expected = new[]
			{
				Constants.ContainerPenaltyPenaltyType.Codes.Detention,
				Constants.ContainerPenaltyPenaltyType.Codes.Storage,
				Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention
			};

			var factory = new BusinessObjectFactory();
			var containerPenalty = factory.New<ContainerPenalty>();
			AssertContainsExactElementsInAnyOrder(expected, containerPenalty.Lookups.PenaltyTypeList.GetAllCodes());
		}

		public void TestCreditorTypeList()
		{
			var expected = new[]
			{
				Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				Constants.ContainerPenaltyCreditorType.Codes.CTO,
				Constants.ContainerPenaltyCreditorType.Codes.Transport
			};

			var factory = new BusinessObjectFactory();
			var containerPenalty = factory.New<ContainerPenalty>();
			AssertContainsExactElementsInAnyOrder(expected, containerPenalty.Lookups.CreditorTypeList.GetAllCodes());
		}

		public void TestTimeUnitList()
		{
			var expected = new[]
			{
				Constants.ContainerPenaltyTimeUnit.Codes.Days,
				Constants.ContainerPenaltyTimeUnit.Codes.Hours
			};

			var factory = new BusinessObjectFactory();
			var containerPenalty = factory.New<ContainerPenalty>();
			AssertContainsExactElementsInAnyOrder(expected, containerPenalty.Lookups.TimeUnitList.GetAllCodes());
		}

		public void TestProcessTypeList()
		{
			var expected = new[]
			{
				Constants.ContainerPenaltyProcessType.Export,
				Constants.ContainerPenaltyProcessType.Import
			};

			var factory = new BusinessObjectFactory();
			var containerPenalty = factory.New<ContainerPenalty>();
			AssertContainsExactElementsInAnyOrder(expected, containerPenalty.Lookups.ProcessTypeList.GetAllCodes());
		}
	}
}

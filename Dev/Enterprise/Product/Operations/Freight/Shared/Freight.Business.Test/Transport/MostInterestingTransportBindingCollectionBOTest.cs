using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(MostInterestingTransportBindingCollection))]
	sealed class MostInterestingTransportBindingCollectionBOTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Transports.ParentDeleting();
			return new MostInterestingTransportBindingCollection(consol);
		}

		#endregion
	}
}

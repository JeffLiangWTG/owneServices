using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(PortArrivalDetailCollection))]
	class PortArrivalDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PortArrivalDetailCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortArrivalDetail(Factory);
		}

		protected override PortArrivalDetailCollection GetCollectionToTest()
		{
			return ArrivalDetails;
		}

		PortArrivalDetailCollection ArrivalDetails
		{
			get
			{
				if (arrivalDetails == null)
				{
					var header = Factory.New<CusInBondHeader>();
					arrivalDetails = header.PortArrivalDetails;
				}
				return arrivalDetails;
			}
		}
		PortArrivalDetailCollection arrivalDetails;
	}
}

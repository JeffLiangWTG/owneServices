using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class PreAdviceTransportCollectionTest : BaseFreightTest
	{
		#region TestSetDefaultsForNewChild

		public void TestSetDefaultsForNewChild()
		{
			PreAdvice.PreAdviceTransports.RemoveAndDeleteAll();

			Transport transport1 = PreAdvice.PreAdviceTransports.AddNew();
			AssertEquals((short)1, transport1.JW_LegOrder);

			Transport transport2 = PreAdvice.PreAdviceTransports.AddNew();
			AssertEquals((short)2, transport2.JW_LegOrder);

			transport2.JW_LegOrder = 4;
			Transport transport3 = PreAdvice.PreAdviceTransports.AddNew();
			AssertEquals((short)5, transport3.JW_LegOrder);
		}

		#endregion

		#region Implementation

		JobShipmentPreplanning PreAdvice
		{
			get
			{
				if (preadvice == null)
				{
					preadvice = Factory.New<JobShipmentPreplanning>();
				}

				return preadvice;
			}
		}

		JobShipmentPreplanning preadvice;

		#endregion
	}
}

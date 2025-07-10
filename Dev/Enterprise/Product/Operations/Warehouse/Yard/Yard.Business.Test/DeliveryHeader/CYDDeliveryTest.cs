using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDDelivery))]
	class CYDDeliveryTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDDelivery>();
		}

		#region TransportReference

		public void TestDeliveryHasTransportReference()
		{
			var delivery = Factory.New<CYDDelivery>();
			AssertEquals(ZString.Empty, delivery.YDL_TransportReference);
		}

		public void TestLengthOfTransportReference()
		{
			var delivery = Factory.New<CYDDelivery>();

			AssertEquals(35, delivery.YDL_TransportReferenceInfo.MaxLength);

			delivery.YDL_TransportReference += new string('1', 35);

			AssertNoErrors(delivery.YDL_TransportReferenceInfo);

			AssertExceptionThrown<MaxLengthExceededException>(() =>
			{
				delivery.YDL_TransportReference += "1";
			});

			ErrorReporter.Clear();
		}

		public void TestReceiveAdviceLineProperty()
		{
			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			AssertEquals(null, delivery.ReceiveAdviceLine);

			var receiveAdviceLine = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();
			delivery.YDL_YRL_ReceiveAdviceLine = receiveAdviceLine.PK;
			AssertEquals(receiveAdviceLine, delivery.ReceiveAdviceLine);
		}

		public void TestLinkedYardUnitProperty()
		{
			var delivery = Factory.NewWithValidTestData<CYDDelivery>();
			AssertEquals(null, delivery.LinkedYardUnit);

			var yardUnit = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnit.YUS_YDL_Delivery = delivery.PK;
			AssertEquals(yardUnit, delivery.LinkedYardUnit);
		}

		#endregion
	}
}

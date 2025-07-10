using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class ReceivingContainerManagementNumberStrategyTest : TestCaseWithFactory
	{
		public void TestGetMessageReferenceNumber()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ReceivingContainerManagementNumberStrategy(null, null));
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var numberStrategy = new ReceivingContainerManagementNumberStrategy(Factory, "IronMan");
				AssertEquals("1", numberStrategy.GetMessageReferenceNumber());
				AssertEquals("2", numberStrategy.GetMessageReferenceNumber());

				var expectedNumberFountain = Env.NumberFountains.EDIFACTNumberFountain("M", "IronMan", ReceivingContainerManagementNumberStrategy.ReceiverCode);
				expectedNumberFountain.SetNext(Factory, 13);

				AssertEquals("13", numberStrategy.GetMessageReferenceNumber());
				AssertEquals("14", numberStrategy.GetMessageReferenceNumber());
			}
		}
	}
}

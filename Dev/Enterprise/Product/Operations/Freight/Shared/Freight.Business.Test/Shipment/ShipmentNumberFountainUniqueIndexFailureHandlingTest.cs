using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	public class ShipmentNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		public void TestNullConsignRefDoesNotThrowException()
		{
			// Create shipment that will use number fountain for JS_UniqueConsignRef, some existing JS_UniqueConsignRef is required in the factory for number fountain fix
			Factory.New<CommonShipmentForTest>();
			Factory.Save();

			var notificationMock = new Mock<INotificationHandler>();

			var shipment = Factory.New<CommonShipmentForTest>();
			AssertNoExceptionThrown(() =>
				shipment
					.UniqueIndexFailureHandlersForTest
					.Single()
					.NotifyUserAndAttemptToResolve(
						notificationMock.Object,
						shipment.UniqueIndexFailureHandlersForTest.Single().HandledUniqueIndexNames.Single()
					)
			);
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override Type BizOTypeToTest
		{
			get { return typeof(CommonShipment); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return JobShipmentSchema.JS_UniqueConsignRef; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.JobShipmentNumber; }
		}

		#endregion
	}
}

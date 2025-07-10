using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class StmALogExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestIsHoldCodeChangeEvent

		public void TestIsHoldCodeChangeEvent()
		{
			AssertEquals(false, ((StmALog)null).IsHoldCodeChangeEvent());
			var receive = Factory.New<WhsReceive>();
			var receiveLine = receive.Lines.AddNew();
			AssertEquals(false,
				receiveLine.Logs
					.AddNew(Events.ServiceCommenced,
						new KeyValuePair<string, string>(
							CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
							Constants.EventReferenceParameterTypes.HoldCode)).IsHoldCodeChangeEvent());
			AssertEquals(false,
				receiveLine.Logs.AddNew(Events.ChangeOfIdentifier,
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason,
						Constants.EventReferenceParameterTypes.HoldCode)).IsHoldCodeChangeEvent());
			AssertEquals(false,
				receiveLine.Logs.AddNew(Events.ChangeOfIdentifier,
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
						Constants.EventReferenceParameterTypes.Adjustment)).IsHoldCodeChangeEvent());
			AssertEquals(true,
				receiveLine.Logs.AddNew(Events.ChangeOfIdentifier,
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
						Constants.EventReferenceParameterTypes.HoldCode)).IsHoldCodeChangeEvent());
		}

		#endregion
	}
}

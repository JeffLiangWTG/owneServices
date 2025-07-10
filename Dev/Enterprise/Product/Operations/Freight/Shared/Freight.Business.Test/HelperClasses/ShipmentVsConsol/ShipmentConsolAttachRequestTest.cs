using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentConsolAttachRequestTest : TestCase
	{
		public void TestDefaultValues()
		{
			var request = new ShipmentConsolAttachRequest(null, null);
			Assert(request.Errors.IsEmpty);
			Assert(request.Warnings.IsEmpty);
		}

		public void TestProperties()
		{
			bool errorCalled = false;
			Func<ZString> errorGetter = () =>
			{
				errorCalled = true;
				return "error!!!";
			};

			bool warningCalled = false;
			Func<ZString> warningGetter = () =>
			{
				warningCalled = true;
				return "warning !!!";
			};

			var request = new ShipmentConsolAttachRequest(errorGetter, warningGetter);
			AssertEquals("Precondition: lazy property", false, errorCalled);
			AssertEquals("Precondition: lazy property", false, warningCalled);

			AssertEquals("error!!!", request.Errors);
			AssertEquals("Property value was set", true, errorCalled);
			AssertEquals("Precondition: lazy property", false, warningCalled);

			AssertEquals("warning !!!", request.Warnings);
			AssertEquals("Property value was set", true, warningCalled);

			errorCalled = false;
			warningCalled = false;

			AssertEquals("error!!!", request.Errors);
			AssertEquals("Value created on first access, message getter is not called anymore", false, errorCalled);

			AssertEquals("warning !!!", request.Warnings);
			AssertEquals("Value created on first access, message getter is not called anymore", false, warningCalled);
		}
	}
}

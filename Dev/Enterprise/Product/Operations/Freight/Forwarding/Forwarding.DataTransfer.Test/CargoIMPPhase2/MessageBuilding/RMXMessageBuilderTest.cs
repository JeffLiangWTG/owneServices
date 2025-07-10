using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class RMXMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2009, 12, 9, 19, 34, 24)]
		public void TestIncorrectMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			RMXMessageBuilder builder = new RMXMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			ZString message = builder.MessageText;
			AssertEquals(@"RMX,2
MSG,09DEC09 1934
SHD,,,09DEC09
", message);
			AssertEquals(@"Error: Required field empty ('Forwarder Identification' registry item)
Error: Required field empty (House Bill Number)
", buffer.AsString);

			buffer.Clear();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);
			shipment.JS_HouseBill = "12345678901234567,:;";
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABCD");
			message = builder.MessageText;
			AssertEquals(@"RMX,2
MSG,09DEC09 1934
SHD,ABC,12345678901234567,09DEC09
", message);
			AssertEquals(@"Error: 'Forwarder Identification' registry item: value can't be formatted, Input data must consist of 3 'Upper case alphabetic characters'
Error: House Bill Number: value can't be formatted, Input data must consist of 'Text character - only comprising upper case alphabetic, numeric, hyphen, full stop, forward slash and space' with length from 0 to 20
", buffer.AsString);
		}

		[TestDate(2009, 12, 9, 19, 34, 24)]
		public void TestMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			shipment.JS_HouseBill = "12345678901234567890";
			Factory.Save();

			RMXMessageBuilder builder = new RMXMessageBuilder();
			builder.Shipment = shipment;
			NotificationBuffer buffer = new NotificationBuffer();
			builder.Notifications = new NonDuplicateNotificationBuffer(buffer);

			ZString message = builder.MessageText;
			AssertEquals(@"RMX,2
MSG,09DEC09 1934
SHD,ABC,12345678901234567890,09DEC09
", message);
			AssertEquals("", buffer.AsString);
		}
	}
}

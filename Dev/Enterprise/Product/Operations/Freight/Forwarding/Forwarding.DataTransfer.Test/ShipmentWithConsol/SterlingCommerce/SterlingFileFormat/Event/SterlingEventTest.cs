using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingEvent))]
	public class SterlingEventTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingEvent();
		}

		public void TestUpdateSterlingEventFields()
		{
			Enterprise.DataTransfer.Xml.XsdVersion1.Event eventToAdd = new Enterprise.DataTransfer.Xml.XsdVersion1.Event();
			eventToAdd.Source = "1";
			eventToAdd.Code = "COD";
			eventToAdd.CodeDescription = "DESC";
			ZDateTime time = ZDateTime.Now;
			eventToAdd.DateTime = time;
			eventToAdd.PostedDateTime = time.AddHours(1);
			eventToAdd.Information = "Information";
			eventToAdd.User = "User";
			eventToAdd.IsEstimatedDate = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@true;
			SterlingEvent @event = new SterlingEvent();
			@event.SourceEvent = eventToAdd;
			AssertEquals(@event.Source, "1");
			AssertEquals(@event.Code, "COD");
			AssertEquals(@event.CodeDescription, "DESC");
			AssertEquals(@event.DateTime, time.ToString("yyyy-MM-dd HH:mm:ss zzzzzz"));
			AssertEquals(@event.PostedDateTime, time.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss zzzzzz"));
			AssertEquals(@event.Information, "Information");
			AssertEquals(@event.User, "User");
			AssertEquals(@event.IsEstimatedDate, "true");
		}

		public void TestRecordSterlingEvent()
		{
			Enterprise.DataTransfer.Xml.XsdVersion1.Event eventToAdd = new Enterprise.DataTransfer.Xml.XsdVersion1.Event();
			eventToAdd.Source = "1";
			eventToAdd.Code = "COD";
			eventToAdd.CodeDescription = "DESC";
			ZDateTime time = ZDateTime.Now;
			eventToAdd.DateTime = time;
			eventToAdd.PostedDateTime = time.AddHours(1);
			eventToAdd.Information = "Information";
			eventToAdd.User = "User";
			eventToAdd.IsEstimatedDate = Enterprise.DataTransfer.Xml.XsdVersion1.TrueFalse.@true;
			SterlingEvent @event = new SterlingEvent();
			@event.SourceEvent = eventToAdd;
			ZString expectedString = "EVT|1|COD|DESC|" + time.ToString("yyyy-MM-dd HH:mm:ss zzzzzz") + "|" + time.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss zzzzzz") + "|Information|User|true>\r\n";
			AssertEquals(expectedString, @event.Record);
		}

		public void TestSterlingEventRecord()
		{
			//AssertEquals("Only one event is triggered", 1, SterlingForTest.E1ventInfo.Count);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingEventRecord1, SterlingForTest.EventInfo[0].Record);
		}
		const string ExpectedSterlingEventRecord1 = "EVT|EventSource|EventCode|CodeDesription|2008-01-01 01:01:01 +11:00|2008-03-03 03:03:03 +11:00|EventInformation|EventUser|false>\r\n";
	}
}

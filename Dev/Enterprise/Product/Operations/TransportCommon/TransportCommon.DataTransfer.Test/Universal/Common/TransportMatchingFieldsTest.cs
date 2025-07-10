using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public class TransportMatchingFieldsTest : TestCaseWithFactory
	{
		#region TestProperties

		public void TestProperties()
		{
			var now = ZDateTimeOffset.Now;
			var eventData = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Depot, "Sydney", "P1");
			var matchingFields = new TransportMatchingFields(eventData, Factory, "C1");
			AssertEquals("AVL", matchingFields.EventType);
			AssertEquals("ABCD", matchingFields.TransportReference);
			AssertEquals(now.AddDays(1), matchingFields.EventTime);
			AssertEquals(Constants.Facilities.Code.Depot, matchingFields.Facility);
			AssertEquals(DocAddressType.LocalCartageCFS, matchingFields.FacilityAddressType);
			AssertEquals("Sydney", matchingFields.City);
			AssertEquals("C1", matchingFields.ContainerNumber);
			AssertEquals("P1", matchingFields.PackageID);

			var eventDataWithInvalidFacilityType = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), "XYZ", "Sydney");
			var matchingFieldsWithInvalidTypes = new TransportMatchingFields(eventDataWithInvalidFacilityType, Factory, "C2");
			AssertEquals("XYZ", matchingFieldsWithInvalidTypes.Facility);
			AssertEquals(DocAddressType.None, matchingFieldsWithInvalidTypes.FacilityAddressType);
			AssertEquals("C2", matchingFieldsWithInvalidTypes.PackageID);
		}

		public void TestCityAndFacilityFromEventReference()
		{
			var xmlEvent = new Event();
			xmlEvent.EventReference = "|LOC=SYDNEY|FAC=DEPOT";
			var matchingFields = new TransportMatchingFields(xmlEvent, Factory);
			AssertEquals("DEPOT", matchingFields.Facility);
			AssertEquals("SYDNEY", matchingFields.City);
		}

		#endregion

		#region TestIsMatchingAddress

		public void TestIsMatchingAddress_DocAddressWithoutCityAndAddressType()
		{
			var now = ZDateTimeOffset.Now;
			var address = CreateJobDocAddress("", DocAddressType.None);

			var eventDataForSydneyDepot = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Depot, "Sydney");
			var matchingFieldsForSydneyDepot = new TransportMatchingFields(eventDataForSydneyDepot, Factory);
			AssertEquals("No Address type on doc address and does not compare with city. Therefore must be false.", false, matchingFieldsForSydneyDepot.IsMatchingAddress(address, false));
			AssertEquals("Although there is no address type and city must compare with city, therefore must be false.", false, matchingFieldsForSydneyDepot.IsMatchingAddress(address));

			var eventDataWithoutAddressType = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), "", "Sydney");
			var matchingFieldsForEventWithoutAddressType = new TransportMatchingFields(eventDataWithoutAddressType, Factory);
			AssertEquals("No Address type on event and doesn't compare with city. Therefore must be true.", true, matchingFieldsForEventWithoutAddressType.IsMatchingAddress(address, false));
			AssertEquals("Although there is no address type, city must be compared, therefore must be false.", false, matchingFieldsForEventWithoutAddressType.IsMatchingAddress(address));

			var eventDataWithoutCity = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Depot, "");
			var matchingFieldsForEventWithoutCity = new TransportMatchingFields(eventDataWithoutCity, Factory);
			AssertEquals("Event has an address type but docaddress does not, Therefore must be false.", false, matchingFieldsForEventWithoutCity.IsMatchingAddress(address, false));
			AssertEquals("Event has an address type but docaddress does not, therefore must be false.", false, matchingFieldsForEventWithoutCity.IsMatchingAddress(address));

			var eventDataWithoutCityAndWithoutAddressType = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), "", "");
			var matchingFieldsForEventWithoutCityAndWithoutAddressType = new TransportMatchingFields(eventDataWithoutCityAndWithoutAddressType, Factory);
			AssertEquals("Event does not have an address type and city must not be compared, Therefore must be true.", true, matchingFieldsForEventWithoutCityAndWithoutAddressType.IsMatchingAddress(address, false));
			AssertEquals("Event does not have an address type and event does not have a city, therefore must be true.", true, matchingFieldsForEventWithoutCityAndWithoutAddressType.IsMatchingAddress(address));
		}

		public void TestIsMatchingAddress_DocAddressWithCityAndNoAddressType()
		{
			var now = ZDateTimeOffset.Now;
			var address = CreateJobDocAddress("Sydney", DocAddressType.None);

			var eventDataForSydneyDepot = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Depot, "SYDNEY");
			var matchingFieldsForSydneyDepot = new TransportMatchingFields(eventDataForSydneyDepot, Factory);
			AssertEquals("Address type does not match, therefore must be false.", false, matchingFieldsForSydneyDepot.IsMatchingAddress(address, false));
			AssertEquals("Although city matches no address type is given, therefore must be false.", false, matchingFieldsForSydneyDepot.IsMatchingAddress(address));
		}

		public void TestIsMatchingAddress_DocAddressWithoutCityAndWithAddressType()
		{
			var now = ZDateTimeOffset.Now;
			var address = CreateJobDocAddress("", DocAddressType.LocalCartageCFS);

			var eventDataForSydneyDepot = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Depot, "Sydney");
			var eventDataForSydneyConsignee = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Consignee, "Sydney");
			var matchingFieldsForSydneyDepot = new TransportMatchingFields(eventDataForSydneyDepot, Factory);
			var matchingFieldsForSydneyConsignee = new TransportMatchingFields(eventDataForSydneyConsignee, Factory);
			AssertEquals("Address type matches and does not compare with city. Therefore must be true.", true, matchingFieldsForSydneyDepot.IsMatchingAddress(address, false));
			AssertEquals("Address type matches and but must compare with city. Therefore must be false.", false, matchingFieldsForSydneyDepot.IsMatchingAddress(address));

			AssertEquals("Address type does not match. Therefore must be false.", false, matchingFieldsForSydneyConsignee.IsMatchingAddress(address, false));
			AssertEquals("Address type does not match and must compare with city which is not given, therefore must be false.", false, matchingFieldsForSydneyConsignee.IsMatchingAddress(address));
		}

		public void TestIsMatchingAddress_DocAddressWithCityAndWithAddressType()
		{
			var now = ZDateTimeOffset.Now;
			var address = CreateJobDocAddress("Sydney", DocAddressType.LocalCartageCFS);

			var eventDataForSydneyDepot = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Depot, "Sydney");
			var eventDataForOrangeDepot = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Depot, "Orange");
			var eventDataForSydneyConsignee = CreateXMLEvent("AVL", "ABCD", now.AddDays(1), Constants.Facilities.Code.Consignee, "Sydney");
			var matchingFieldsForSydneyDepot = new TransportMatchingFields(eventDataForSydneyDepot, Factory);
			var matchingFieldsForOrangeDepot = new TransportMatchingFields(eventDataForOrangeDepot, Factory);
			var matchingFieldsForSydneyConsignee = new TransportMatchingFields(eventDataForSydneyConsignee, Factory);
			AssertEquals("Address type matches and must not compare with city, therefore must be true.", true, matchingFieldsForSydneyDepot.IsMatchingAddress(address, false));
			AssertEquals("Address type and city matches, therefore must be true.", true, matchingFieldsForSydneyDepot.IsMatchingAddress(address));

			AssertEquals("Address type matches and does not have to compare with city, therefore must be true. Therefore must be true.", true, matchingFieldsForOrangeDepot.IsMatchingAddress(address, false));
			AssertEquals("Although address type matches does not match with city, therefore must be false.", false, matchingFieldsForOrangeDepot.IsMatchingAddress(address));

			AssertEquals("Address type does not match therefore must be false.", false, matchingFieldsForSydneyConsignee.IsMatchingAddress(address, false));
			AssertEquals("Address type does not match therefore must be false.", false, matchingFieldsForSydneyConsignee.IsMatchingAddress(address));
		}

		JobDocAddress CreateJobDocAddress(string city, DocAddressType addressType)
		{
			var address = Factory.New<JobDocAddress>();
			address.City = city;
			address.DocAddressType = addressType;
			return address;
		}

		#endregion

		#region CreateXMLEvent

		Event CreateXMLEvent(string eventCode, string transportReference, ZDateTimeOffset eventTime, string facility, string location, string packageId = "")
		{
			var xmlEvent = new Event();
			xmlEvent.EventType = eventCode;
			xmlEvent.EventTime = eventTime;
			xmlEvent.ContextCollection = new List<Context>()
			{
				new Context() { Type = nameof(Event.ContextTypes.TransportReference), Value = transportReference },
				new Context() { Type = nameof(Event.ContextTypes.TransportBookingPackageID), Value = packageId }
			};
			xmlEvent.EventParameters = new EventParameters();
			xmlEvent.EventParameters.Facility = facility;
			xmlEvent.EventParameters.Location = location;
			return xmlEvent;
		}

		#endregion
	}
}

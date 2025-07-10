using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ServiceDataContextManager))]
	public class ServiceDataContextManagerTest : DataContextManagerTestCase<ServiceDataContextManager, JobService>
	{
		protected override JobService GetNewBusinessObjectForTesting()
		{
			return Factory.NewWithValidTestData<JobService>();
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		public void TestGetEventContextValues_WhenExternalServiceIdIsNotEmpty()
		{
			var expectedXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>Service</Type>
          <Key>WTLKKK00000043</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <EventTime>1971-09-18T00:00:00.000+10:00</EventTime>
    <EventType>SVR</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ServiceId</Type>
        <Value>WTLKKK00000043</Value>
      </Context>
      <Context>
        <Type>ServiceType</Type>
        <Value>FUM</Value>
      </Context>
      <Context>
        <Type>ServiceCount</Type>
        <Value>1</Value>
      </Context>
      <Context>
        <Type>ServiceNotes</Type>
        <Value>Note</Value>
      </Context>
      <Context>
        <Type>ServiceRate</Type>
        <Value>150</Value>
      </Context>
      <Context>
        <Type>ServiceRateCurrency</Type>
        <Value>AUD</Value>
      </Context>
      <Context>
        <Type>ServiceMeasurementBasis</Type>
        <Value>DY</Value>
      </Context>
      <Context>
        <Type>ServiceSubLocation</Type>
        <Value>123</Value>
      </Context>
      <Context>
        <Type>ServiceDuration</Type>
        <Value>1900-01-04T00:00:00</Value>
      </Context>
      <Context>
        <Type>ServiceReference</Type>
        <Value>REF1111111</Value>
      </Context>
      <Context>
        <Type>ServiceContractor</Type>
        <Value>XXXXXX</Value>
      </Context>
      <Context>
        <Type>ServiceLocation</Type>
        <Value>YYYYYY</Value>
      </Context>
      <Context>
        <Type>ServiceLocationAddress</Type>
        <Value>ZZZZZZ</Value>
      </Context>
      <Context>
        <Type>ExternalServiceId</Type>
        <Value>WTLKKK00000044</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			TestGetEventContextValues("WTLKKK00000044", expectedXml);
		}

		public void TestGetEventContextValues_WhenExternalServiceIdIsEmpty()
		{
			var expectedXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>Service</Type>
          <Key>WTLKKK00000043</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <EventTime>1971-09-18T00:00:00.000+10:00</EventTime>
    <EventType>SVR</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ServiceId</Type>
        <Value>WTLKKK00000043</Value>
      </Context>
      <Context>
        <Type>ServiceType</Type>
        <Value>FUM</Value>
      </Context>
      <Context>
        <Type>ServiceCount</Type>
        <Value>1</Value>
      </Context>
      <Context>
        <Type>ServiceNotes</Type>
        <Value>Note</Value>
      </Context>
      <Context>
        <Type>ServiceRate</Type>
        <Value>150</Value>
      </Context>
      <Context>
        <Type>ServiceRateCurrency</Type>
        <Value>AUD</Value>
      </Context>
      <Context>
        <Type>ServiceMeasurementBasis</Type>
        <Value>DY</Value>
      </Context>
      <Context>
        <Type>ServiceSubLocation</Type>
        <Value>123</Value>
      </Context>
      <Context>
        <Type>ServiceDuration</Type>
        <Value>1900-01-04T00:00:00</Value>
      </Context>
      <Context>
        <Type>ServiceReference</Type>
        <Value>REF1111111</Value>
      </Context>
      <Context>
        <Type>ServiceContractor</Type>
        <Value>XXXXXX</Value>
      </Context>
      <Context>
        <Type>ServiceLocation</Type>
        <Value>YYYYYY</Value>
      </Context>
      <Context>
        <Type>ServiceLocationAddress</Type>
        <Value>ZZZZZZ</Value>
      </Context>
      <Context>
        <Type>ShipmentNumber</Type>
        <Value>AAAAAA</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			TestGetEventContextValues(ZString.Empty, expectedXml);
		}

		public void TestOnUniversalEventAddedCore()
		{
			var contractor = Factory.NewWithValidTestData<OrgHeader>();
			contractor.OH_Code = "XXXXXX";

			var location = Factory.NewWithValidTestData<OrgHeader>();
			location.OH_Code = "YYYYYY";
			location.MainAddress.OA_Code = "ZZZZZZ";

			var dummyConsignmentWithServices1 = Factory.NewWithValidTestData<DummyConsignmentWithServices>();
			dummyConsignmentWithServices1.ShipmentNumberForTest = "AAAAAA";
			var dummyConsignmentWithServices2 = Factory.NewWithValidTestData<DummyConsignmentWithServices>();

			var service1 = dummyConsignmentWithServices1.Services.AddNew();
			service1.ES_ServiceId = "WTLKKK00000044";
			var service2 = dummyConsignmentWithServices2.Services.AddNew();
			service2.ES_ServiceId = "WTLKKK00000043";

			Factory.SaveForTesting();

			var eventAdded = new Event
			{
				EventType = AutoEvents.ServiceRequestedCode,
				EventTime = new ZDateTimeOffset(2023, 12, 11),
				ContextCollection = new List<Context>
				{
					new Context { Type = nameof(ContextTypes.ServiceId), Value = "WTLKKK00000043" },
					new Context { Type = nameof(ContextTypes.ServiceType), Value = Core.Constants.FreightServiceType.Codes.Fumigation },
					new Context { Type = nameof(ContextTypes.ServiceCount), Value = "1" },
					new Context { Type = nameof(ContextTypes.ServiceNotes), Value = "Note" },
					new Context { Type = nameof(ContextTypes.ServiceRate), Value = "150" },
					new Context { Type = nameof(ContextTypes.ServiceRateCurrency), Value = Core.Constants.CurrencyCodes.Australia },
					new Context { Type = nameof(ContextTypes.ServiceMeasurementBasis), Value = JobServiceInfo.Constants.Codes.Day },
					new Context { Type = nameof(ContextTypes.ServiceSubLocation), Value = "123" },
					new Context { Type = nameof(ContextTypes.ServiceDuration), Value = "1900-01-04T00:00:00" },
					new Context { Type = nameof(ContextTypes.ServiceReference), Value = "REF1111111" },
					new Context { Type = nameof(ContextTypes.ServiceContractor), Value = "XXXXXX" },
					new Context { Type = nameof(ContextTypes.ServiceLocation), Value = "YYYYYY" },
					new Context { Type = nameof(ContextTypes.ServiceLocationAddress), Value = "ZZZZZZ" }
				}
			};

			var manager = (IEventDataContextManager)service1.GetUniversalDataContextManager();
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);
			AssertEquals("service1.ES_ExternalServiceId", "WTLKKK00000043", service1.ES_ExternalServiceId);
			AssertEquals("service1.ES_ServiceCode", Core.Constants.FreightServiceType.Codes.Fumigation, service1.ES_ServiceCode);
			AssertEquals("service1.ES_ServiceCount", 1m, service1.ES_ServiceCount);
			AssertEquals("service1.ES_ServiceNote", "Note", service1.ES_ServiceNote);
			AssertEquals("service1.ES_ServiceRate", 150m, service1.ES_ServiceRate);
			AssertEquals("service1.ES_RX_NKServiceRateCurrency", Core.Constants.CurrencyCodes.Australia, service1.ES_RX_NKServiceRateCurrency);
			AssertEquals("service1.ES_MeasurementBasis", JobServiceInfo.Constants.Codes.Day, service1.ES_MeasurementBasis);
			AssertEquals("service1.ES_SubLocation", "123", service1.ES_SubLocation);
			AssertEquals("service1.ES_Duration", (ZDateTime)TimeSpan.FromDays(3), service1.ES_Duration);
			AssertEquals("service1.ES_References", "REF1111111", service1.ES_References);
			AssertEquals("service1.ES_Booked", new ZDateTime(2023, 12, 11), service1.ES_Booked);
			AssertEquals("service1.ES_Completed", ZDateTime.Empty, service1.ES_Completed);
			AssertEquals("service1.ES_OH_Contractor", contractor.PK, service1.ES_OH_Contractor);
			AssertEquals("service1.ES_OA_Location", location.MainAddress.PK, service1.ES_OA_Location);
			AssertEquals("service2.ES_ExternalServiceId", "WTLKKK00000044", service2.ES_ExternalServiceId);

			eventAdded = new Event
			{
				EventType = AutoEvents.ServiceCompletedCode,
				EventTime = new ZDateTimeOffset(2023, 12, 12),
				ContextCollection = new List<Context>
				{
					new Context { Type = nameof(ContextTypes.ServiceId), Value = "WTLKKK00000043" },
					new Context { Type = nameof(ContextTypes.ServiceContractor), Value = ZString.Empty },
					new Context { Type = nameof(ContextTypes.ServiceLocation), Value = ZString.Empty },
					new Context { Type = nameof(ContextTypes.ServiceLocationAddress), Value = ZString.Empty }
				}
			};

			manager.OnUniversalEventAdded(messageLogger, eventAdded);
			AssertEquals("service1.ES_ExternalServiceId", "WTLKKK00000043", service1.ES_ExternalServiceId);
			AssertEquals("service1.ES_ServiceCode", Core.Constants.FreightServiceType.Codes.Fumigation, service1.ES_ServiceCode);
			AssertEquals("service1.ES_ServiceCount", 1m, service1.ES_ServiceCount);
			AssertEquals("service1.ES_ServiceNote", "Note", service1.ES_ServiceNote);
			AssertEquals("service1.ES_ServiceRate", 150m, service1.ES_ServiceRate);
			AssertEquals("service1.ES_RX_NKServiceRateCurrency", ZString.Empty, service1.ES_RX_NKServiceRateCurrency);
			AssertEquals("service1.ES_MeasurementBasis", JobServiceInfo.Constants.Codes.Day, service1.ES_MeasurementBasis);
			AssertEquals("service1.ES_SubLocation", "123", service1.ES_SubLocation);
			AssertEquals("service1.ES_Duration", (ZDateTime)TimeSpan.FromDays(3), service1.ES_Duration);
			AssertEquals("service1.ES_References", "REF1111111", service1.ES_References);
			AssertEquals("service1.ES_Booked", new ZDateTime(2023, 12, 11), service1.ES_Booked);
			AssertEquals("service1.ES_Completed", new ZDateTime(2023, 12, 12), service1.ES_Completed);
			AssertEquals("service1.ES_OH_Contractor", ZGuid.Empty, service1.ES_OH_Contractor);
			AssertEquals("service1.ES_OA_Location", ZGuid.Empty, service1.ES_OA_Location);
			AssertEquals("service2.ES_ExternalServiceId", "WTLKKK00000044", service2.ES_ExternalServiceId);
		}

		void TestGetEventContextValues(ZString externalServiceId, string expectedXml)
		{
			var contractor = Factory.NewWithValidTestData<OrgHeader>();
			contractor.OH_Code = "XXXXXX";

			var location = Factory.NewWithValidTestData<OrgHeader>();
			location.OH_Code = "YYYYYY";
			location.MainAddress.OA_Code = "ZZZZZZ";

			var dummyConsignmentWithServices = Factory.NewWithValidTestData<DummyConsignmentWithServices>();
			dummyConsignmentWithServices.ShipmentNumberForTest = "AAAAAA";

			var service = dummyConsignmentWithServices.Services.AddNew();
			service.ES_ServiceId = "WTLKKK00000043";
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service.ES_ServiceCount = 1;
			service.ES_ServiceNote = "Note";
			service.ES_ServiceRate = 150m;
			service.ES_OA_Location = location.MainAddress.PK;
			service.ES_RX_NKServiceRateCurrency = Core.Constants.CurrencyCodes.Australia;
			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Day;
			service.ES_SubLocation = "123";
			service.ES_Duration = new TimeSpan(3, 0, 0, 0);
			service.ES_References = "REF1111111";
			service.ES_Completed = ZDateTime.Now;
			service.ES_OH_Contractor = contractor.PK;
			service.ES_ExternalServiceId = externalServiceId;

			var logBO = service.Logs.AddNew(AutoEvents.ServiceRequested);
			using (logBO.LockForUpdatingKeyFieldsForTesting())
			{
				logBO.SL_EventTime = ZDateTime.BrettsBirthday;
			}

			var manager = (IEventDataContextManager)service.GetUniversalDataContextManager();
			var writer = manager.GetEventDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.FOR, service)));
			using (var universalEvent = writer.GetDataObject(logBO))
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(universalEvent, stream);

				using (var reader = new StreamReader(stream))
				{
					var result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("Expected Universal Shipment Message", expectedXml, result);
				}
			}
		}
	}
}

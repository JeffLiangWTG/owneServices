using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ConsolContainerLinkerTest : TestCaseWithFactory
	{
		public void TestGetLogParent_ContainerExists_TransportLegWithATA()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_ATA = ZDateTime.Today;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON000";

			Factory.Save();

			AssertNotNull("Precondition: Transport with JW_ATA set exists", consol.Transports.OfType<Transport>().FirstOrDefault(t => !t.JW_ATA.IsEmpty));

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNotNull("Result should not be null.", resultContainer);
			AssertEquals("Container should be returned", resultContainer.PK, container.PK);
		}

		public void TestGetLogParent_ContainerExists_TransportLegWithATD()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_ATD = ZDateTime.Today;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON000";

			Factory.Save();

			AssertNotNull("Precondition: Transport with JW_ATD set exists", consol.Transports.OfType<Transport>().FirstOrDefault(t => !t.JW_ATD.IsEmpty));

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNotNull("Result should not be null.", resultContainer);
			AssertEquals("Container should be returned", resultContainer.PK, container.PK);
		}

		public void TestGetLogParent_ContainerExists_NoTransportLegWithATAorATD()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON000";

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNotNull("Result should not be null.", resultContainer);
			AssertEquals("Container should be returned", resultContainer.PK, container.PK);
		}

		public void TestGetLogParent_ContainerExists_ConsolHasMessageSentEvent()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var log = consol.Logs.AddNew();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.MessageSentCode;
				log.Parameters.Add(new KeyValuePair<string, string>(Params.MessageType, "Shipping Instruction"));
				log.Parameters.Add(new KeyValuePair<string, string>(Params.Department, "Carrier"));
			}

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON000";

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNotNull("Result should not be null.", resultContainer);
			AssertEquals("Container should be returned", resultContainer.PK, container.PK);
		}

		public void TestGetLogParent_ContainerExists_ConsolHasNoMessageSentEvent()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON000";

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNotNull("Result should not be null.", resultContainer);
			AssertEquals("Container should be returned", resultContainer.PK, container.PK);
		}

		public void TestGetLogParent_UpdateContainer_TransportLegWithATA()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });
			context.Setup(m => m.ContainerISOCode).Returns("35H1");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_ATA = ZDateTime.Today;

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNull("Container should not be created", resultContainer);
		}

		public void TestGetLogParent_UpdateContainer_TransportLegWithATD()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });
			context.Setup(m => m.ContainerISOCode).Returns("35H1");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_ATD = ZDateTime.Today;

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNull("Container should not be created", resultContainer);
		}

		public void TestGetLogParent_UpdateContainer_TransportLegWithNoATAorATD()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });
			context.Setup(m => m.ContainerISOCode).Returns("35H1");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNotNull("Container has been created.", resultContainer);
		}

		public void TestGetLogParent_UpdateContainer_ConsolHasMessageSentEvent()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });
			context.Setup(m => m.ContainerISOCode).Returns("35H1");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var log = consol.Logs.AddNew();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.MessageSentCode;
				log.Parameters.Add(new KeyValuePair<string, string>(Params.MessageType, "Shipping Instruction"));
				log.Parameters.Add(new KeyValuePair<string, string>(Params.Department, "Carrier"));
			}

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNull("Container should not be created", resultContainer);
		}

		public void TestGetLogParent_UpdateContainer_ConsolHasNoMessageSentEvent()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });
			context.Setup(m => m.ContainerISOCode).Returns("35H1");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var log = consol.Logs.AddNew();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.StatusUpdatedCode;
				log.Parameters.Add(new KeyValuePair<string, string>(Params.MessageType, "Shipping Instruction"));
				log.Parameters.Add(new KeyValuePair<string, string>(Params.Department, "Carrier"));
			}

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();
			AssertNotNull("Container has been created.", resultContainer);
		}

		public void TestGetLogParent_UpdateContainer_AutomaticContainerCreation_IsAlwaysCreate()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });
			context.Setup(m => m.ContainerISOCode).Returns("35H1");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_ATD = ZDateTime.Today.AddDays(-1);
			transport.JW_ATA = ZDateTime.Today;

			var log = consol.Logs.AddNew();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.MessageSentCode;
				log.Parameters.Add(new KeyValuePair<string, string>(Params.MessageType, "Shipping Instruction"));
				log.Parameters.Add(new KeyValuePair<string, string>(Params.Department, "Carrier"));
			}

			Factory.Save();

			var automaticContainerCreation = new AutomaticContainerCreation();
			automaticContainerCreation.IsNeverCreate = true;

			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var consolLinker = new ConsolContainerLinker(consol, logger);
				var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();

				AssertNull("Container should not be created", resultContainer);
			}

			automaticContainerCreation.IsCreateUpToATDOrShippingInstruction = true;

			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var consolLinker = new ConsolContainerLinker(consol, logger);
				var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();

				AssertNull("Container should not be created", resultContainer);
			}

			automaticContainerCreation.IsAlwaysCreate = true;

			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var consolLinker = new ConsolContainerLinker(consol, logger);
				var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();

				AssertNotNull("Container has been created.", resultContainer);
			}
		}

		public void TestGetLogParent_WhenNewContainerIsMeasurementsOutOfRange()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			consol.Containers.RemoveAndDeleteAll();

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			for (int i = 0; i < 5; i++)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.FillWithValidTestData();
				packLine.JL_ActualWeight = 900000m;
				packLine.JL_PackageCount = 1;
				packLine.JL_JC = ZGuid.Empty;
			}

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				"Container allocated pack line weight and/or volume would exceed the database maximum value.",
				() => consolLinker.GetLogParent(eventValueObject.Object));
		}

		public void TestGetLogParent_WhenContainerIsNull()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "CON000" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";
			consol.JK_ConsolMode = "ULD";

			consol.Containers.RemoveAndDeleteAll();

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object);

			AssertEquals(consol.Containers[0].PK, result.FirstOrDefault().PK);
			AssertEquals(@"Information - Created new container CON000.
Information - Container number advised: CON000.", logger.Logs);

			eventValueObject = mock.Create<IXmlEventValueObject>();
			context = mock.Create<IXmlEventValueObjectContextValueList>();
			logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns(string.Empty);
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			consolLinker = new ConsolContainerLinker(consol, logger);
			result = consolLinker.GetLogParent(eventValueObject.Object);

			AssertEquals(2, consol.Containers.Count);
			AssertEquals(consol.Containers[1].PK, result.FirstOrDefault().PK);
			AssertEquals(@"Information - Created new container 00001.
Information - Container number advised: 00001.", logger.Logs);
		}

		public void TestGetLogParent_WhenConsolModeIsLSE()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "123456" });

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.Containers.AddNew();

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object);

			AssertNull("Should not return a container when transport mode is Air and consol mode is LSE", result);
			AssertEquals("Information - This Air Consol is of Loose (LSE) type. Containers are not required for loose packages and will not be created for this Consol.", logger.Logs);
		}

		public void TestGetLogParent_ContainerNumberExceedsMaxLength()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "12345678901234567890123456798" });

			var consol = Factory.New<CommonConsol>();
			consol.Containers.AddNew();

			Factory.Save();

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object);

			AssertNull("Should not return a container when the container number exceeds max length", result);
			AssertEquals("Information - Container Number 12345678901234567890123456798 exceeds the valid max length. Containers will not be created for this Consol.", logger.Logs);
		}

		public void TestGetLogParent_MAWBNumberNotAvailable_ReturnContainerByContainerNumber()
		{
			var linker = GetLinkerAndPrepareTestData();
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "00002" });

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("container number should be from ContainerNumber as the MAWBNumber is NOT available", "00001", resultContainer.JC_ContainerNum);
		}

		public void TestGetLogParent_ContainerExists_FallbackISOTypeWithGroupFallback()
		{
			var ref35H0 = NewRefContainer("35H0", "35H0");
			var ref35H1 = NewRefContainer("35H1", "35H1");
			ref35H1.RC_IsActive = false;

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = ref35H1.PK;
			container.JC_ContainerNum = "TEST1000001";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);

			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("35H2");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 35H1, fallback with Group Code HI", ref35H1.PK, resultContainer.JC_RC);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		public void TestGetLogParent_ContainerExists_WithoutContainerNumber_AutomaticContainerCreation_NeverCreate()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var logger = new DummyXmlImportLogger();

			var consol = Factory.New<CommonConsol>();
			var linker = new ConsolContainerLinker(consol, logger);

			Factory.Save();

			Func<ZShort, RefContainer, CommonContainer> addContainer = (count, containerType) =>
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerCount = count;
				container.JC_RC = containerType.PK;
				Factory.Save();

				return container;
			};

			Func<string, string, IXmlEventValueObject> createEvent = (containerNumber, isoCode) =>
			{
				var mock = new MockRepository(MockBehavior.Default);
				var eventValueObject = mock.Create<IXmlEventValueObject>();
				var context = mock.Create<IXmlEventValueObjectContextValueList>();
				eventValueObject.Setup(m => m.Context).Returns(context.Object);
				context.Setup(m => m.MAWBNumber).Returns("");
				context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { containerNumber });
				context.Setup(m => m.ContainerISOCode).Returns(isoCode);
				return eventValueObject.Object;
			};

			var containerEvent20GP = createEvent("CONT1234567", ref20GP.RC_ISOType);
			var containerSingle20GP = addContainer(1, ref20GP);

			var automaticContainerCreation = new AutomaticContainerCreation();

			automaticContainerCreation.IsNeverCreate = true;
			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var resultContainer = linker.GetLogParent(containerEvent20GP)?.FirstOrDefault();

				AssertNull(resultContainer);
			}

			automaticContainerCreation.IsCreateUpToATDOrShippingInstruction = true;
			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var resultContainer = linker.GetLogParent(containerEvent20GP)?.FirstOrDefault();

				AssertNotNull(resultContainer);
				AssertEquals("CONT1234567", resultContainer.JC_ContainerNum);
			}

			automaticContainerCreation.IsAlwaysCreate = true;
			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var resultContainer = linker.GetLogParent(containerEvent20GP)?.FirstOrDefault();

				AssertNotNull(resultContainer);
				AssertEquals("CONT1234567", resultContainer.JC_ContainerNum);
			}
		}

		public void TestGetLogParent_ContainerExists_WithoutContainerNumberFallbackISOTypeWithGroupFallback()
		{
			var ref45RE = NewRefContainer("45RE", "42R0");
			var ref48RE = NewRefContainer("48RE", "42R0");

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = ref48RE.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);

			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1111111" });
			context.Setup(m => m.ContainerISOCode).Returns("42R0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("container PK", container.PK, resultContainer.PK);
			AssertEquals("container number", "TEST1111111", resultContainer.JC_ContainerNum);
			AssertEquals("container type should not have changed", ref48RE.PK, resultContainer.JC_RC);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		public void TestGetLogParent_ContainerExists_WithoutContainerNumberFallbackISOTypeWithGroupFallback_InActiveISO()
		{
			var ref45U0 = NewRefContainer("45U0", "45U0");
			var ref48U0 = NewRefContainer("48U0", "48U0");
			var ref48U1 = NewRefContainer("48U1", "48U1");
			ref48U1.RC_IsActive = false;

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = ref48U0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);

			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1111111" });
			context.Setup(m => m.ContainerISOCode).Returns("48U1");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("container PK", container.PK, resultContainer.PK);
			AssertEquals("container number", "TEST1111111", resultContainer.JC_ContainerNum);
			AssertEquals("container type should not have changed", ref48U0.PK, resultContainer.JC_RC);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		public void TestGetLogParent_ContainerExists_WithoutContainerNumber_DifferentISOCode()
		{
			var ref45G0 = NewRefContainer("45G0", "45G0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref45G0.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref45G0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "2222";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);

			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1111111" });
			context.Setup(m => m.ContainerISOCode).Returns("45G1");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("container number", "TEST1111111", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be empty", ZGuid.Empty, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_ContainerExists_WithoutContainerNumberFallbackISOTypeWithGroupFallback_SameISOGroup()
		{
			var ref45G0 = NewRefContainer("45G0", "45G0");
			var ref45G1 = NewRefContainer("45G1", "45G1");

			var consol1 = Factory.New<CommonConsol>();
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref45G0.PK;

			var linker = new ConsolContainerLinker(consol1, new DummyXmlImportLogger());

			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1111111" });
			context.Setup(m => m.ContainerISOCode).Returns("45G1");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("container PK", container1.PK, resultContainer.PK);
			AssertEquals("container number", "TEST1111111", resultContainer.JC_ContainerNum);
			AssertEquals("Same ISO Group, container type should not have changed", ref45G0.PK, resultContainer.JC_RC);
			AssertEquals("should not add any new container to consol", 1, consol1.Containers.Count);
		}

		public void TestGetLogParent_ContainerExists_WithoutContainerNumberFallbackISOTypeWithGroupFallback_ContainerCountMoreThanOne()
		{
			var ref45G0 = NewRefContainer("45G0", "45G0");
			var ref45G1 = NewRefContainer("45G1", "45G1");
			var ref45RE = NewRefContainer("45RE", "45R0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			container1.JC_RC = ref45G0.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref45RE.PK;

			var linker = new ConsolContainerLinker(consol, new DummyXmlImportLogger());

			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1111111" });
			context.Setup(m => m.ContainerISOCode).Returns("45G1");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("should add new container to consol", 3, consol.Containers.Count);
			AssertEquals("Container count should be reduced", (ZShort)1, container1.JC_ContainerCount);

			var container3 = consol.Containers[2];
			AssertEquals("container PK", container3.PK, resultContainer.PK);
			AssertEquals("container number", "TEST1111111", container3.JC_ContainerNum);
			AssertEquals("Same ISO Group, container type should not have changed", ref45G0.PK, container3.JC_RC);
		}

		public void TestGetLogParent_MAWBNumberIsAvailable_ReturnContainerByMAWBNumber()
		{
			var linker = GetLinkerAndPrepareTestData();
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "00002" });

			var resultContainer = linker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertEquals("container number should be from ULDIdentification as the MAWBNumber is available", "00002", resultContainer.JC_ContainerNum);
		}

		public void TestGetLogParent_MAWBNumberIsAvailable_ReturnULDIdContainers()
		{
			var linker = GetLinkerAndPrepareTestData();
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00003" });
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "00001", "00002" });

			var resultContainers = linker.GetLogParent(eventValueObject.Object).Select(c => c.JC_ContainerNum);
			AssertContainsExactElementsInAnyOrder(new[] { "00001", "00002" }, resultContainers);
		}

		public void TestGetLogParent_AddCIDEventToContainer()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var ref20RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");

			var logger = new DummyXmlImportLogger();

			var consol = Factory.New<CommonConsol>();
			var linker = new ConsolContainerLinker(consol, logger);

			Factory.Save();

			Func<ZShort, RefContainer, CommonContainer> addContainer = (count, containerType) =>
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerCount = count;
				container.JC_RC = containerType.PK;
				Factory.Save();

				return container;
			};

			Func<string, string, IXmlEventValueObject> createEvent = (containerNumber, isoCode) =>
			{
				var mock = new MockRepository(MockBehavior.Default);
				var eventValueObject = mock.Create<IXmlEventValueObject>();
				var context = mock.Create<IXmlEventValueObjectContextValueList>();
				eventValueObject.Setup(m => m.Context).Returns(context.Object);
				context.Setup(m => m.MAWBNumber).Returns("");
				context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { containerNumber });
				context.Setup(m => m.ContainerISOCode).Returns(isoCode);
				return eventValueObject.Object;
			};

			Action reset = () =>
			{
				consol.Containers.RemoveAndDeleteAll();
				logger.ResetLog();
			};

			// TEST 1a: Event matches single container, same container type

			var containerEvent20GP = createEvent("CONT1234567", ref20GP.RC_ISOType);

			var containerSingle20GP = addContainer(1, ref20GP);
			var resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle20GP, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(containerSingle20GP, resultContainer);
			AssertEquals(@"Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			// TEST 1b: Event should not match single container, container type is inactive. Should make container type empty

			containerSingle20GP = addContainer(1, ref20GP);
			ref20GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle20GP, ZString.Empty, ref20GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertNotEquals(containerSingle20GP, resultContainer);
			AssertEquals(string.Format(@"Warning - There is 1 Container Type with ISO Code: {0}, but it is inactive
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", ref20GP.RC_ISOType), logger.Log);

			ref20GP.RC_IsActive = true;
			reset();

			// TEST 2a: Event should not match single container which has a different length, new container is created.

			var containerSingle40GP = addContainer(1, ref40GP);
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle40GP, ZString.Empty, ref40GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ref20GP.PK, 1, true);
			AssertNotEquals(containerSingle40GP, resultContainer);
			AssertEquals(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			// TEST 2b: Event should matches single container which has an inactive container type, new container is created.

			containerSingle40GP = addContainer(1, ref40GP);
			ref20GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle40GP, ZString.Empty, ref40GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertNotEquals(containerSingle40GP, resultContainer);
			AssertEquals(string.Format(@"Warning - There is 1 Container Type with ISO Code: {0}, but it is inactive
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", ref20GP.RC_ISOType), logger.Log);

			ref20GP.RC_IsActive = true;
			reset();

			// TEST 2c: Event should not match single container which has an inactive container type, event has different active container type, should update ISO type

			containerSingle40GP = addContainer(1, ref40GP);
			ref40GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle40GP, ZString.Empty, ref40GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ref20GP.PK, 1, true);
			AssertNotEquals(containerSingle40GP, resultContainer);
			AssertEquals(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", logger.Log);

			ref40GP.RC_IsActive = true;
			reset();

			// TEST 2d: Event should not match single container which has an inactive container type, event has different inactive container type, should set ISO type to empty

			containerSingle40GP = addContainer(1, ref40GP);
			ref20GP.RC_IsActive = false;
			ref40GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle40GP, ZString.Empty, ref40GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertNotEquals(containerSingle40GP, resultContainer);
			AssertEquals(string.Format(@"Warning - There is 1 Container Type with ISO Code: {0}, but it is inactive
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", ref20GP.RC_ISOType), logger.Log);

			ref20GP.RC_IsActive = true;
			ref40GP.RC_IsActive = true;
			reset();

			// TEST 3: Event matches single container, picks one with same container type

			containerSingle20GP = addContainer(1, ref20GP);
			containerSingle40GP = addContainer(1, ref40GP);
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle20GP, "CONT1234567", ref20GP.PK, 1, true);
			AssertContainer(containerSingle40GP, string.Empty, ref40GP.PK, 1);
			AssertEquals(containerSingle20GP, resultContainer);
			AssertEquals(@"Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			// TEST 4a: Event matches multi-container, same container type

			var containerMultiple20GP = addContainer(3, ref20GP);
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerMultiple20GP, string.Empty, ref20GP.PK, 2);
			AssertContainer(resultContainer, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			// TEST 4b: Event should not match multi-container, same inactive container type. Should show warning about inactive type and give existing and new containers empty type.

			containerMultiple20GP = addContainer(3, ref20GP);
			ref20GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerMultiple20GP, string.Empty, ref20GP.PK, 3);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertEquals(string.Format(@"Warning - There is 1 Container Type with ISO Code: {0}, but it is inactive
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", ref20GP.RC_ISOType), logger.Log);

			ref20GP.RC_IsActive = true;
			reset();

			// TEST 5: Event does not match because there is only multi-container with non-matching container type

			var containerMultiple40GP = addContainer(3, ref40GP);
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerMultiple40GP, string.Empty, ref40GP.PK, 3);
			AssertContainer(resultContainer, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			// TEST 6a: Event matches multi-container, picks one with same container type

			containerMultiple20GP = addContainer(3, ref20GP);
			containerMultiple40GP = addContainer(3, ref40GP);
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerMultiple20GP, string.Empty, ref20GP.PK, 2);
			AssertContainer(containerMultiple40GP, string.Empty, ref40GP.PK, 3);
			AssertContainer(resultContainer, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			// TEST 6b: Event matches multi-container, but the one with same container type is inactive. Should unset the new container and its match's RefContainer

			containerMultiple20GP = addContainer(3, ref20GP);
			containerMultiple40GP = addContainer(3, ref40GP);
			ref20GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerMultiple20GP, string.Empty, ref20GP.PK, 3);
			AssertContainer(containerMultiple40GP, string.Empty, ref40GP.PK, 3);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertEquals(string.Format(@"Warning - There is 1 Container Type with ISO Code: {0}, but it is inactive
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", ref20GP.RC_ISOType), logger.Log);

			ref20GP.RC_IsActive = true;
			reset();

			// TEST 7a: Event matches single container, no/invalid container type on event

			var containerEventNoType = createEvent("CONT1234567", string.Empty);
			var containerEventInvalidType = createEvent("CONT1234567", "XYZ");

			containerSingle20GP = addContainer(1, ref20GP);
			resultContainer = linker.GetLogParent(containerEventNoType).FirstOrDefault();
			AssertContainer(containerSingle20GP, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(containerSingle20GP, resultContainer);
			AssertEquals(@"Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			containerSingle20GP = addContainer(1, ref20GP);
			resultContainer = linker.GetLogParent(containerEventInvalidType).FirstOrDefault();
			AssertContainer(containerSingle20GP, ZString.Empty, ref20GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertNotEquals(containerSingle20GP, resultContainer);
			AssertEquals(string.Format(@"Warning - There are no Container Types with ISO Code: {0}
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", containerEventInvalidType.Context.ContainerISOCode), logger.Log);

			reset();

			// TEST 7b: Event should not match single container with inactive ISOType, no/invalid container type on event. Should set container type to empty

			containerSingle20GP = addContainer(1, ref20GP);
			ref20GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEventNoType).FirstOrDefault();
			AssertContainer(containerSingle20GP, ZString.Empty, ref20GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertNotEquals(containerSingle20GP, resultContainer);
			AssertEquals(string.Format(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567."), logger.Log);

			ref20GP.RC_IsActive = true;
			reset();

			containerSingle20GP = addContainer(1, ref20GP);
			ref20GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEventInvalidType).FirstOrDefault();
			AssertContainer(containerSingle20GP, ZString.Empty, ref20GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertNotEquals(containerSingle20GP, resultContainer);
			AssertEquals(string.Format(@"Warning - There are no Container Types with ISO Code: {0}
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", containerEventInvalidType.Context.ContainerISOCode), logger.Log);

			ref20GP.RC_IsActive = true;
			reset();

			// TEST 8: Event matches multi-container, no container type on event

			containerMultiple20GP = addContainer(3, ref20GP);
			resultContainer = linker.GetLogParent(containerEventNoType).FirstOrDefault();
			AssertContainer(containerMultiple20GP, string.Empty, ref20GP.PK, 2);
			AssertContainer(resultContainer, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			containerMultiple20GP = addContainer(3, ref20GP);
			resultContainer = linker.GetLogParent(containerEventInvalidType).FirstOrDefault();
			AssertContainer(containerMultiple20GP, string.Empty, ref20GP.PK, 3);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertEquals(string.Format(@"Warning - There are no Container Types with ISO Code: {0}
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", containerEventInvalidType.Context.ContainerISOCode), logger.Log);

			reset();

			// TEST 9: Event does not match because event does not specify container type and there is more than one container entry

			containerSingle20GP = addContainer(1, ref20GP);
			containerSingle40GP = addContainer(1, ref40GP);
			resultContainer = linker.GetLogParent(containerEventNoType).FirstOrDefault();
			AssertContainer(containerSingle20GP, string.Empty, ref20GP.PK, 1);
			AssertContainer(containerSingle40GP, string.Empty, ref40GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertEquals(@"Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			containerSingle20GP = addContainer(1, ref20GP);
			containerSingle40GP = addContainer(1, ref40GP);
			resultContainer = linker.GetLogParent(containerEventInvalidType).FirstOrDefault();
			AssertContainer(containerSingle20GP, string.Empty, ref20GP.PK, 1);
			AssertContainer(containerSingle40GP, string.Empty, ref40GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertEquals(string.Format(@"Warning - There are no Container Types with ISO Code: {0}
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", containerEventInvalidType.Context.ContainerISOCode), logger.Log);

			reset();

			// TEST 10: Event does not match with single container which has 2 active container types

			containerSingle20GP = addContainer(1, ref20GP);
			var duplicateRef20GP = Factory.NewWithValidTestData<RefContainer>();
			duplicateRef20GP.RC_ISOType = ref20GP.RC_ISOType;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle20GP, "CONT1234567", ref20GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(string.Format(@"Warning - 2 or more active Container Types have ISO Code: {0}, so none of these will be used.
Information - Container number advised: CONT1234567.", containerEvent20GP.Context.ContainerISOCode), logger.Log);

			duplicateRef20GP.RC_IsActive = false;
			reset();

			// TEST 11: Event does not match with single container which has 2 inactive container types

			containerSingle20GP = addContainer(1, ref20GP);
			duplicateRef20GP.RC_IsActive = false;
			ref20GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle20GP, string.Empty, ref20GP.PK, 1);
			AssertContainer(resultContainer, "CONT1234567", ZGuid.Empty, 1, true);
			AssertEquals(string.Format(@"Warning - 2 or more inactive Container Types have ISO Code: {0}, so none of these will be used.
Information - Created new container CONT1234567.
Information - Container number advised: CONT1234567.", containerEvent20GP.Context.ContainerISOCode), logger.Log);

			ref20GP.RC_IsActive = true;
			reset();

			// TEST 12: Event matches with single container which has 1 active container type and 1 inactive container type

			containerSingle20GP = addContainer(1, ref20GP);
			resultContainer = linker.GetLogParent(containerEvent20GP).FirstOrDefault();
			AssertContainer(containerSingle20GP, "CONT1234567", ref20GP.PK, 1, true);
			AssertEquals(containerSingle20GP, resultContainer);
			AssertEquals(@"Information - Container number advised: CONT1234567.", logger.Log);

			reset();

			// TEST 13: No containers and event has inactive container type
			var containerEvent40GP = createEvent("CONT7654321", ref40GP.RC_ISOType);

			ref40GP.RC_IsActive = false;
			resultContainer = linker.GetLogParent(containerEvent40GP).FirstOrDefault();
			AssertContainer(resultContainer, "CONT7654321", ZGuid.Empty, 1, true);
			AssertEquals(string.Format(@"Warning - There is 1 Container Type with ISO Code: {0}, but it is inactive
Information - Created new container CONT7654321.
Information - Container number advised: CONT7654321.", containerEvent40GP.Context.ContainerISOCode), logger.Log);

			ref40GP.RC_IsActive = true;
			reset();
		}

		public void TestGetLogParent_LowercaseContainerID()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns(string.Empty);
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "abc" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			consol.Containers.RemoveAndDeleteAll();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "abc";
			Factory.Save();

			AssertEquals("ABC", container.JC_ContainerNum);
			AssertEquals(1, consol.Containers.Count);

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();

			AssertNotNull("Container has been updated.", resultContainer);
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(1, consol.Containers.Count);

			context.Setup(m => m.MAWBNumber).Returns("0123");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "aBc" });

			consolLinker = new ConsolContainerLinker(consol, logger);
			resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();

			AssertNotNull("Container has been updated.", resultContainer);
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(1, consol.Containers.Count);
		}

		public void TestGetLogParent_GroupContainerIDsIgnoreLetterCase()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var logger = new TestErrorLogger();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns(string.Empty);
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "abc", "ABC", "abC" });
			context.Setup(m => m.CarriersBookingReference).Returns("TST");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "1234";
			consol.JK_BookingReference = "TST";

			consol.Containers.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals(0, consol.Containers.Count);

			var consolLinker = new ConsolContainerLinker(consol, logger);
			var resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();

			AssertNotNull("Container has been created.", resultContainer);
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(1, consol.Containers.Count);

			consol.Containers.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals(0, consol.Containers.Count);

			context.Setup(m => m.MAWBNumber).Returns("0123");
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "aBc", "Abc" });

			consolLinker = new ConsolContainerLinker(consol, logger);
			resultContainer = consolLinker.GetLogParent(eventValueObject.Object)?.FirstOrDefault();

			AssertNotNull("Container has been created.", resultContainer);
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(1, consol.Containers.Count);
		}

		void AssertContainer(CommonContainer container, string expectNumber, ZGuid expectTypePK, short expectCount, bool shouldCheckCIDLog = false)
		{
			AssertEquals("Container numbers did not match", expectNumber, container.JC_ContainerNum);
			AssertEquals("PKs did not match", expectTypePK, container.JC_RC);
			AssertEquals("Container count did not match", expectCount, container.JC_ContainerCount);

			if (shouldCheckCIDLog)
			{
				var parameters = new[]
				{
					Params.New.AsKeyFor(expectNumber),
					Params.Type.AsKeyFor(Constants.EventReferenceParameterTypes.ContainerID),
					Params.Reason.AsKeyFor(Constants.EventReferenceParameterReasons.ContainerNumberAdvised)
				};

				var logs = container.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ChangeOfIdentifierCode))
					.Where(x => x.SL_Reference == StmALog.GenerateEventReference("", parameters));
				AssertEquals(1, logs.Count());
			}
		}

		#region Container Type Update When Container Number Matched

		public void TestGetLogParent_KeepContainerType_WhenContainerNumberMatched()
		{
			var ref35H1 = NewRefContainer("35H1", "35H1");
			var ref36H0 = NewRefContainer("36H0", "36H0");

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = ref35H1.PK;
			container.JC_ContainerNum = "TEST1000001";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("36H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 35H1 and unchanged", ref35H1.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		#endregion

		#region Container Type Update for Containers without Container Number

		public void TestGetLogParent_WithoutContainerNumber_ExcludeContainersWithInActiveContainerType_CreateNewContainer()
		{
			var ref48U0 = NewRefContainer("48U0", "48U0");
			var ref48U1 = NewRefContainer("48U1", "48U1");
			ref48U1.RC_IsActive = false;

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = ref48U1.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1111111" });
			context.Setup(m => m.ContainerISOCode).Returns("48U1");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1111111", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 48U0", ref48U0.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 2, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_ExcludeContainersWithInActiveContainerType_UseActiveContainerType()
		{
			var ref48U0 = NewRefContainer("48U0", "48U0");
			var ref48U1 = NewRefContainer("48U1", "48U1");
			ref48U1.RC_IsActive = false;

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref48U1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref48U0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1111111" });
			context.Setup(m => m.ContainerISOCode).Returns("48U1");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container PK", container2.PK, resultContainer.PK);
			AssertEquals("container number", "TEST1111111", resultContainer.JC_ContainerNum);
			AssertEquals("container type should use active type", ref48U0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container2.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 2, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_WithoutCountainerTypeInXML_OneContainer()
		{
			var ref35H1 = NewRefContainer("35H1", "35H1");

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = ref35H1.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should keep 35H1 and unchanged", ref35H1.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_WithoutCountainerTypeInXML_TwoContainers()
		{
			var ref35H1 = NewRefContainer("35H1", "35H1");
			var ref36H0 = NewRefContainer("36H0", "36H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref35H1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be empty", ZGuid.Empty, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_ExactMatch()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref36H0 = NewRefContainer("36H0", "36H0");
			var ref3CH0 = NewRefContainer("3CH0", "3CH0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CH0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("36H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 36H0", ref36H0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container2.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_InvalidContainerType()
		{
			var ref35H1 = NewRefContainer("35H1", "35H1");
			var ref36H0 = NewRefContainer("36H0", "36H0");
			var ref37H0 = NewRefContainer("37H0", "37H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref35H1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref37H0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("39Z5");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be empty", ZGuid.Empty, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_MatchedByLength_Width_FirstGroupLetter()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref36H0 = NewRefContainer("36H0", "36H0");
			var ref3CH0 = NewRefContainer("3CH0", "3CH0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CH0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 36H0", ref36H0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container2.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_OneContainer_MatchedByLength_Width_Height_FirstGroupLetter_WhenTwoContainerTypesShareOneIsoContainerType()
		{
			var ref3MH0 = NewRefContainer("3MH0", "36H0");
			var ref36H0 = NewRefContainer("36H0", "36H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("36H1");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be unchanged", ref3MH0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container1.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_MatchedByLength_Height_FirstGroupLetter()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref36H0 = NewRefContainer("36H0", "36H0");
			var ref34H0 = NewRefContainer("34H0", "34H0");
			var ref3FH0 = NewRefContainer("3FH0", "3FH0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref34H0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("3FH0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 36H0", ref36H0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container2.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_MatchedByLength_FirstGroupLetter()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref3NP0 = NewRefContainer("3NP0", "3NP0");
			var ref3CP0 = NewRefContainer("3CP0", "3CP0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref3NP0.PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CP0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 3MH1", ref3MH1.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container1.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_NoRefContainerForIncomingISOCode_MatchesOnFirstThreeLetters()
		{
			var ref45R0 = NewRefContainer("45R0", "45R0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref45R0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("45R1");

			AssertNull("Precondition - No RefContainer exists with ISOCode 45R1", Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "45R1")).FirstOrDefault());

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number - should have matched from Containers ISO Codes", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 45R0", ref45R0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container1.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_NoRefContainerForIncomingISOCode_MatchesOnFirstAndThirdLetters()
		{
			var ref45R0 = NewRefContainer("45R0", "45R0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref45R0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("40R1");

			AssertNull("Precondition - No RefContainer exists with ISOCode 40R1", Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, "40R1")).FirstOrDefault());

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number - should have matched from Containers ISO Codes", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 45R0", ref45R0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container1.PK, resultContainer.PK);
			AssertEquals("should not add any new container to consol", 1, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_ContainerCountGreaterThanOne_MatchedByLength_FirstGroupLetter()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref3NP0 = NewRefContainer("3NP0", "3NP0");
			var ref3CP0 = NewRefContainer("3CP0", "3CP0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			container1.JC_RC = ref3MH1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 3;
			container2.JC_RC = ref3NP0.PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 4;
			container3.JC_RC = ref3CP0.PK;

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container1 JC_ContainerCount should be 1.", (ZShort)1, container1.JC_ContainerCount);
			AssertEquals("container2 JC_ContainerCount should be unchanged.", (ZShort)3, container2.JC_ContainerCount);
			AssertEquals("container3 JC_ContainerCount should be unchanged.", (ZShort)4, container3.JC_ContainerCount);
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 3MH1", ref3MH1.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_ExactMatch()
		{
			var ref35H1 = NewRefContainer("35H1", "35H1");
			var ref36H0 = NewRefContainer("36H0", "36H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref36H0.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "2222";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref35H1.PK;
			container3.JC_RH_NKContainerCommodityCode = "AFAT";
			container3.JC_ReleaseNum = "3333";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("36H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 36H0", ref36H0.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_Length_Width_FirstGroupLetter()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref36H0 = NewRefContainer("36H0", "36H0");
			var ref39H0 = NewRefContainer("39H0", "39H0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "2222";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref39H0.PK;
			container3.JC_RH_NKContainerCommodityCode = "AFAT";
			container3.JC_ReleaseNum = "3333";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 38H0", ref38H0.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_Length_Height_FirstGroupLetter()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref36H0 = NewRefContainer("36H0", "36H0");
			var ref3PH0 = NewRefContainer("3PH0", "3PH0");
			var ref3FH0 = NewRefContainer("3FH0", "3FH0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref36H0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "2222";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3PH0.PK;
			container3.JC_RH_NKContainerCommodityCode = "AFAT";
			container3.JC_ReleaseNum = "3333";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("3FH0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 38H0", ref3FH0.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_Length_FirstGroupLetter()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref3NP0 = NewRefContainer("3NP0", "3NP0");
			var ref3CH0 = NewRefContainer("3CH0", "3CH0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref3NP0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "2222";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CH0.PK;
			container3.JC_RH_NKContainerCommodityCode = "AFAT";
			container3.JC_ReleaseNum = "3333";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 38H0", ref38H0.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		#endregion

		#region Container Type Update for Multiple Containers without Container Number Depending on Commodity/Release Number/ContainerCount

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_Length_FirstGroupLetter_SameCommodityReleaseNumberAndSameContainerCount()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref3NP0 = NewRefContainer("3NP0", "3NP0");
			var ref3CH0 = NewRefContainer("3CH0", "3CH0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref3NP0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "1111";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CH0.PK;
			container3.JC_RH_NKContainerCommodityCode = "AFAT";
			container3.JC_ReleaseNum = "1111";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			Assert("container type should be 3MH1 or 3CH0", new[] { ref3MH1.PK, ref3CH0.PK }.Contains(resultContainer.JC_RC));
			Assert("Updated container should be container1 or container3.", new[] { container1.PK, container3.PK }.Contains(resultContainer.PK));
			AssertEquals("should not create a new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_Length_FirstGroupLetter_SameCommodityReleaseNumberAndDifferentContainerCount()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref3NP0 = NewRefContainer("3NP0", "3NP0");
			var ref3CH0 = NewRefContainer("3CH0", "3CH0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 3;
			container1.JC_RC = ref3MH1.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 2;
			container2.JC_RC = ref3NP0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "1111";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CH0.PK;
			container3.JC_RH_NKContainerCommodityCode = "AFAT";
			container3.JC_ReleaseNum = "1111";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 3CH0", ref3CH0.PK, resultContainer.JC_RC);
			AssertEquals("container PK", container3.PK, resultContainer.PK);
			AssertEquals("JC_ContainerCount should be 1", (ZShort)1, resultContainer.JC_ContainerCount);
			AssertEquals("should not create a new container to consol", 3, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_Length_FirstGroupLetter_DifferentCommodity()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref3NP0 = NewRefContainer("3NP0", "3NP0");
			var ref3CH0 = NewRefContainer("3CH0", "3CH0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref3NP0.PK;
			container2.JC_RH_NKContainerCommodityCode = "COFF";
			container2.JC_ReleaseNum = "1111";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CH0.PK;
			container3.JC_RH_NKContainerCommodityCode = "GLAS";
			container3.JC_ReleaseNum = "1111";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 38H0", ref38H0.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		public void TestGetLogParent_WithoutContainerNumber_MultipleContainers_SameScores_Length_FirstGroupLetter_DifferentReleaseNumber()
		{
			var ref3MH1 = NewRefContainer("3MH1", "3MH1");
			var ref3NP0 = NewRefContainer("3NP0", "3NP0");
			var ref3CH0 = NewRefContainer("3CH0", "3CH0");
			var ref38H0 = NewRefContainer("38H0", "38H0");

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerCount = 1;
			container1.JC_RC = ref3MH1.PK;
			container1.JC_RH_NKContainerCommodityCode = "AFAT";
			container1.JC_ReleaseNum = "1111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerCount = 1;
			container2.JC_RC = ref3NP0.PK;
			container2.JC_RH_NKContainerCommodityCode = "AFAT";
			container2.JC_ReleaseNum = "2222";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerCount = 1;
			container3.JC_RC = ref3CH0.PK;
			container3.JC_RH_NKContainerCommodityCode = "AFAT";
			container3.JC_ReleaseNum = "3333";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });
			context.Setup(m => m.ContainerISOCode).Returns("38H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000001", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 38H0", ref38H0.PK, resultContainer.JC_RC);
			AssertEquals("should create a new container to consol", 4, consol.Containers.Count);
		}

		#endregion

		#region Container Type Update When Container Number Mismatched

		public void TestGetLogParent_CreateNewContainerAndUpdateContainerType_WhenContainerNumberMismatched()
		{
			var ref35H1 = NewRefContainer("35H1", "35H1");
			var ref36H0 = NewRefContainer("36H0", "36H0");

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_RC = ref35H1.PK;
			container.JC_ContainerNum = "TEST1000001";

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000002" });
			context.Setup(m => m.ContainerISOCode).Returns("36H0");

			var resultContainer = linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("container number", "TEST1000002", resultContainer.JC_ContainerNum);
			AssertEquals("container type should be 36H0", ref36H0.PK, resultContainer.JC_RC);
			AssertEquals("should add new container to consol", 2, consol.Containers.Count);
		}

		public void TestGetLogParent_CreateNewContainerandUpdateContainerType_ContainerTypeNRQ()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			var shipment = consol.Shipments.AddNew();

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "1234";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			AssertEquals("Precondition: OA_VerifiesContainerGrossWeight", true, shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight);

			var logger = new DummyXmlImportLogger();
			var linker = new ConsolContainerLinker(consol, logger);
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "TEST1000001" });

			linker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("should add new container to consol", 1, consol.Containers.Count);
			AssertNoExceptionThrown("Shipment Container Type does not prevent saving", Factory.Save);
		}

		#endregion

		#region Implementation

		ConsolContainerLinker GetLinkerAndPrepareTestData()
		{
			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "00001";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "00002";

			return new ConsolContainerLinker(consol, null);
		}

		RefContainer NewRefContainer(ZString code, ZString isoType)
		{
			var refContainer = RefContainer.New(Factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;

			return refContainer;
		}

		class DummyXmlImportLogger : IXmlImportLogger
		{
			void IXmlImportLogger.FireDataImportedToBusinessObject(BusinessObject targetBO)
			{
				throw new NotImplementedException();
			}

			bool IXmlImportLogger.HasIgnoredModule
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			bool IXmlImportLogger.IsUpdatingConsol
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			bool IXmlImportLogger.OrgMatchingDisabled => false;

			void IXmlImportLogger.LogBoth(LogType type, string message)
			{
				notifications.Add(string.Format("{0} - {1}", type.ToString(), message));
			}

			List<string> notifications = new List<string>();

			public string Log
			{
				get { return string.Join("\r\n", notifications); }
			}

			public void ResetLog()
			{
				notifications = new List<string>();
			}

			void IXmlImportLogger.LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
			{
				throw new NotImplementedException();
			}

			void IXmlImportLogger.LogErrorToServiceTaskOnly(string message)
			{
				throw new NotImplementedException();
			}

			IDataContextDataObject IXmlImportLogger.TopLevelDataContext
			{
				get { throw new NotImplementedException(); }
			}

			ITopLevelDataObject IXmlImportLogger.TopLevelDataObject
			{
				get { throw new NotImplementedException(); }
			}

			IEnumerable<IValidationRule> IXmlImportLogger.ValidationRuleCollection { get; set; }

			void ISimpleLogger.Log(LogType type, string message)
			{
				throw new NotImplementedException();
			}

			IEnumerable<ISimpleLog> ISimpleLogResult.Logs
			{
				get { throw new NotImplementedException(); }
			}
		}

		#endregion
	}
}

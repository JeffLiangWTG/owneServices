using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ConsolLinkerTest : TestCaseWithFactory
	{
		public void TestMatchConsol_DoNotCreateNewContainerWhenSystemFindingMultpleConsolsInOneYearRange()
		{
			var expectedMessage = @"System cannot create new Container C00001 on Consol CON01 as there are multiple consols with the same Booking Number BOKK and Master Bill Number 1234.
Matched Consolidations:
CON01,CON02";
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var now = ZDateTime.UtcNow;

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CON01";
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_UniqueConsignRef = "CON02";
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = now;

			var logger = new TestErrorLogger();
			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object)[0];

			AssertEquals("Should not find or create a container", consol1.PK, result.PK);
			AssertEquals("Should not create a new container as consol1 and consol2 are found and their created time are close.(No more than one year)", 0, consol1.Containers.Count);

			AssertContains(expectedMessage, logger.GetErrors());

			consol2.JK_SystemCreateTimeUtc = now.AddDays(-1000);

			logger = new TestErrorLogger();
			consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			result = consolLinker.GetLogParent(eventValueObject.Object)[0];

			AssertEquals("Should create a new container as consol1 and consol2 are found and their created time are far away.", 1, consol1.Containers.Count);
			AssertEquals("Should create a new container as consol1 and consol2 are found and their created time are far away.", result.PK, consol1.Containers[0].PK);

			AssertNotContains(expectedMessage, logger.GetErrors());
		}

		public void TestMatchCoLoadConsol_DoNotCreateNewContainerWhenSystemFindingMultpleConsolsInOneYearRange()
		{
			var expectedMessage = @"System cannot create new Container C00001 on Consol CON01 as there are multiple consols with the same Booking Number BOKK and Master Bill Number 1234.
Matched Consolidations:
CON01,CON02";
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var now = ZDateTime.UtcNow;

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CON01";
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_CoLoadBookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_UniqueConsignRef = "CON02";
			consol2.JK_CoLoadMasterBill = "1234";
			consol2.JK_CoLoadBookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = now;

			var logger = new TestErrorLogger();
			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object)[0];

			AssertEquals("Should not find or create a container", consol1.PK, result.PK);
			AssertEquals("Should not create a new container as consol1 and consol2 are found and their created time are close.(No more than one year)", 0, consol1.Containers.Count);

			AssertContains(expectedMessage, logger.GetErrors());

			consol2.JK_SystemCreateTimeUtc = now.AddDays(-1000);

			logger = new TestErrorLogger();
			consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			result = consolLinker.GetLogParent(eventValueObject.Object)[0];

			AssertEquals("Should create a new container as consol1 and consol2 are found and their created time are far away.", 1, consol1.Containers.Count);
			AssertEquals("Should create a new container as consol1 and consol2 are found and their created time are far away.", result.PK, consol1.Containers[0].PK);

			AssertNotContains(expectedMessage, logger.GetErrors());
		}

		public void TestGetLogParent_ULDContainer()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.ULDIdentifications).Returns(new List<ZString> { "C00002", "C00003" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var now = ZDateTime.UtcNow;

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_UniqueConsignRef = "CON01";
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "C00002";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			consol2.JK_UniqueConsignRef = "CON02";
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = now;
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "C00003";

			var logger = new TestErrorLogger();
			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object).FirstOrDefault();
			AssertNotNull(result);
			AssertEquals(container1.PK, result.PK);
		}

		public void TestMatchConsol_DoNotCreateNewContainerWhenSystemFindingMultpleConsolsInOneYearRange_BookingRefOnlyErrorMessage()
		{
			var expectedMessage = @"System cannot create new Container C00001 on Consol C00001001 as there are multiple consols with the same Booking Number BOOK.
Consolidations with the same Booking Number:
C00001000,C00001001";
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();
			var now = ZDateTime.UtcNow;

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "";
			consol1.JK_BookingReference = "BOOK";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "BOOK";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(2);

			Factory.Save();

			var logger = new TestErrorLogger();
			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertContains(expectedMessage, logger.GetErrors());
		}

		public void TestMatchConsol_DoNotCreateNewContainerWhenSystemFindingMultpleConsolsInOneYearRange_MasterBillOnlyErrorMessage()
		{
			var expectedMessage = @"System cannot create new Container C00001 on Consol C00001001 as there are multiple consols with the same Master Bill Number 1234.
Consolidations with the same Master Bill Number:
C00001000,C00001001";
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();
			var now = ZDateTime.UtcNow;

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(2);

			Factory.Save();

			var logger = new TestErrorLogger();
			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertContains(expectedMessage, logger.GetErrors());
		}

		public void TestMatchCoLoadConsol_DoNotCreateNewContainerWhenSystemFindingMultpleConsolsInOneYearRange_BookingRefOnlyErrorMessage()
		{
			var expectedMessage = @"System cannot create new Container C00001 on Consol C00001001 as there are multiple consols with the same Booking Number BOOK.
Consolidations with the same Booking Number:
C00001000,C00001001";
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();
			var now = ZDateTime.UtcNow;

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "";
			consol1.JK_CoLoadBookingReference = "BOOK";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = "";
			consol2.JK_CoLoadBookingReference = "BOOK";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(2);

			Factory.Save();

			var logger = new TestErrorLogger();
			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertContains(expectedMessage, logger.GetErrors());
		}

		public void TestMatchCoLoadConsol_DoNotCreateNewContainerWhenSystemFindingMultpleConsolsInOneYearRange_MasterBillOnlyErrorMessage()
		{
			var expectedMessage = @"System cannot create new Container C00001 on Consol C00001001 as there are multiple consols with the same Master Bill Number 1234.
Consolidations with the same Master Bill Number:
C00001000,C00001001";
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();
			var now = ZDateTime.UtcNow;

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_CoLoadBookingReference = "";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = "1234";
			consol2.JK_CoLoadBookingReference = "";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(2);

			Factory.Save();

			var logger = new TestErrorLogger();
			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertContains(expectedMessage, logger.GetErrors());
		}

		public void TestMatchConsol_DoNotCreateNewContainer_WhenAutomaticContainerCreationRegistryIsNeverCreate()
		{
			var expectedMessage = @"Container Automation has received information on container C00001 for Consol CON01.
Container cannot be created on the Consol as ""Registry > Freight > Global Tracking > Automatic Container Creation"" is ""Never Create"".";
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_UniqueConsignRef = "CON01";
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOOK";

			var automaticContainerCreation = new AutomaticContainerCreation();
			automaticContainerCreation.IsNeverCreate = true;

			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var logger = new TestErrorLogger();
				var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
				var result = consolLinker.GetLogParent(eventValueObject.Object)[0];

				AssertEquals("Should not find or create a container.", consol1.PK, result.PK);
				AssertEquals("Should not create a new container as AutomaticContainerCreation registry is never create.", 0, consol1.Containers.Count);
				AssertContains(expectedMessage, logger.GetWarnings());
			}

			automaticContainerCreation.IsCreateUpToATDOrShippingInstruction = true;

			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var logger = new TestErrorLogger();
				var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
				var result = consolLinker.GetLogParent(eventValueObject.Object)[0];

				AssertEquals("Should create a new container.", 1, consol1.Containers.Count);
				AssertEquals("Should create a new container as AutomaticContainerCreation registry is create up to ATD or Shipping Instruction.", result.PK, consol1.Containers[0].PK);
				AssertNotContains(expectedMessage, logger.GetWarnings());
			}

			automaticContainerCreation.IsAlwaysCreate = true;

			using (FreightDataRegistry.Instance.AutomaticContainerCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, automaticContainerCreation))
			{
				var logger = new TestErrorLogger();
				var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object, logger);
				var result = consolLinker.GetLogParent(eventValueObject.Object)[0];

				AssertEquals("Should create a new container.", 1, consol1.Containers.Count);
				AssertEquals("Should create a new container as AutomaticContainerCreation registry is always create.", result.PK, consol1.Containers[0].PK);
				AssertNotContains(expectedMessage, logger.GetWarnings());
			}
		}

		public void TestMatchConsol_UEContainsCBRandMBL_MatchAgainstCBR()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();
			var now = ZDateTime.UtcNow;

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("CBR");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "";
			consol1.JK_BookingReference = "CBR";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object);
			AssertEquals("consol1 should be the best match and add the container", 1, consol1.Containers.Count);
			AssertEquals(consol1.Containers[0].PK, result[0].PK);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "CBR";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(-500);

			Factory.Save();

			result = consolLinker.GetLogParent(eventValueObject.Object);
			AssertNull("Should return null when MBN doesn't match to any consols but multiple consols are found by CBR", result);
		}

		public void TestMatchConsol_UEContainsCBRandMBL_MatchAgainstCBR_SameContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();
			var now = ZDateTime.UtcNow;

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("CBR");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "";
			consol1.JK_BookingReference = "CBR";
			consol1.JK_SystemCreateTimeUtc = now.AddDays(1);
			consol1.Containers.AddNew().JC_ContainerNum = "C00001";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object);

			AssertEquals("consol1 should be the best match and add the container", 1, consol1.Containers.Count);
			AssertEquals(consol1.Containers[0].PK, result[0].PK);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "CBR";
			consol2.JK_SystemCreateTimeUtc = now.AddDays(-500);
			consol2.Containers.AddNew().JC_ContainerNum = "C00001";

			Factory.Save();

			result = consolLinker.GetLogParent(eventValueObject.Object);
			AssertNull("Should return null when MBN doesn't match to any consols but multiple consols are found by CBR", result);
		}

		public void TestMatchConsol_WithBookingReferenceMBLNumberAndContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(7);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = new ZDateTime(2016, 10, 27);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "2222";
			consol3.JK_BookingReference = "BOKK";

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object)[0];

			AssertEquals("consol1 should be the best match and add the container", 1, consol1.Containers.Count);
			AssertEquals(consol1.Containers[0].PK, result.PK);
		}

		public void TestMatchConsol_WithBookingReferenceAndContainerNumber_WithoutMBLNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(7);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "2222";
			consol2.JK_BookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = new ZDateTime(2016, 10, 27);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "2222";
			consol3.JK_BookingReference = "ABCD";

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals("consol1 should be the best match and add the container", 1, consol1.Containers.Count);
			AssertEquals(consol1.Containers[0].PK, result.PK);
		}

		public void TestMatchConsol_WithBookingReference_WithoutMBLNumberAndContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(7);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "";
			consol3.JK_BookingReference = "ABCD";

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals("consol1 should be the best match", consol1.PK, result.PK);
		}

		public void TestMatchConsol_WithBookingReferenceAndMBLNumber_WithoutContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(7);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_BookingReference = "ABCD";

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol1.PK, result.PK);
		}

		public void TestMatchConsol_WithMasterBillAndContainerNumber_WithoutBookingReference()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";
			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "00001";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "BOKK";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals("return the matched exsiting container", container.PK, result.PK);
		}

		public void TestMatchConsol_WithMBLNumber_WithoutBookingReferenceAndContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now;

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "";
			consol2.JK_SystemCreateTimeUtc = new ZDateTime(2016, 10, 27);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "";
			consol3.JK_BookingReference = "BOKK";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals("consol1 should be the best match", consol1.PK, result.PK);
		}

		public void TestMatchConsol_ReturnPossibleMatch()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";
			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "00001";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "BOKK";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals("return the match exsiting container", container.PK, result.PK);

			consol1.Containers.RemoveAll();

			Factory.Save();

			result = consolLinker.GetLogParent(eventValueObject.Object).First();
			AssertEquals(1, consol1.Containers.Count);
			AssertEquals(consol1.Containers[0].PK, result.PK);
		}

		public void TestMatchConsol_ShouldReturnNull_WithoutMasterBillAndBookingReference()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.CarriersBookingReference).Returns("");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "";
			consol1.JK_BookingReference = "";
			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "00001";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "BOKK";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object);
			AssertNull("Should return null when Master Bill and Booking Reference are both empty", result);
		}

		public void TestMatchConsol_ShouldReturnNull_WhenLoadPortMatchesFromOneConsolAndDischargePortMatchesFromAnother()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.MBOLOriginUNLOCO).Returns("AUMEL");
			context.Setup(m => m.MBOLDestinationUNLOCO).Returns("SGSIN");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object);

			AssertNull("Should return null when Load Port matches from one consol and Discharge Port matches from another", result);
		}

		public void TestMatchCoLoadConsolUsingCoLoadBookingReference()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_CoLoadBookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(7);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = "1234";
			consol2.JK_CoLoadBookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_CoLoadMasterBill = "";
			consol3.JK_CoLoadBookingReference = "ABCD";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol1, result);
		}

		public void TestMatchCoLoadAndNormalConsol()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_CoLoadBookingReference = "BOKK1";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = "1234";
			consol2.JK_CoLoadBookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_BookingReference = "ABCD";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_TransportMode = Constants.TransportModes.Sea;
			consol4.JK_MasterBillNum = "1233";
			consol4.JK_BookingReference = "BOKK";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals(consol2, result);
		}

		public void TestMatchCoLoadAndNormalConsol_WhenCoLoadMasterBillAndBookingReferenceAreEmpty()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "BOKK1";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_BookingReference = "ABCD";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_TransportMode = Constants.TransportModes.Sea;
			consol4.JK_MasterBillNum = "1233";
			consol4.JK_BookingReference = "BOKK";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals(consol2, result);
		}

		public void TestMatchCoLoadAndNormalConsol_MatchMasterBillOnly()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_CoLoadBookingReference = string.Empty;
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = "1234";
			consol2.JK_CoLoadBookingReference = string.Empty;
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_BookingReference = string.Empty;
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_TransportMode = Constants.TransportModes.Sea;
			consol4.JK_MasterBillNum = "1234";
			consol4.JK_BookingReference = string.Empty;
			consol4.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol3, result);
		}

		public void TestMatchCoLoadAndNormalConsol_ActiveConsolsOnly()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_CoLoadBookingReference = "BOKK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);
			consol1.JK_IsCancelled = false;

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = "1234";
			consol2.JK_CoLoadBookingReference = "BOKK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);
			consol2.JK_IsCancelled = false;

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_BookingReference = "BOKK";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);
			consol3.JK_IsCancelled = false;

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_TransportMode = Constants.TransportModes.Sea;
			consol4.JK_MasterBillNum = "1234";
			consol4.JK_BookingReference = "BOKK";
			consol4.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			consol4.JK_IsCancelled = false;

			Factory.Save();

			var consolLinker1 = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result1 = consolLinker1.GetLogParent(eventValueObject.Object).First();
			AssertEquals("All consols are active so consol1 is latest", consol1, result1);

			consol1.JK_IsCancelled = true;

			Factory.Save();

			var consolLinker2 = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result2 = consolLinker2.GetLogParent(eventValueObject.Object).First();
			AssertEquals("Consol1 is inactive so consol2 is latest", consol2, result2);
		}

		public void TestMatchCoLoadAndNormalConsol_MatchCarrierBookingRefOnly()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = string.Empty;
			consol1.JK_CoLoadBookingReference = "BOOK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = string.Empty;
			consol2.JK_CoLoadBookingReference = "BOOK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = string.Empty;
			consol3.JK_BookingReference = "BOOK";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_TransportMode = Constants.TransportModes.Sea;
			consol4.JK_MasterBillNum = string.Empty;
			consol4.JK_BookingReference = "BOOK";
			consol4.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object);

			AssertNull("Should return null when MBN doesn't match to any consols but multiple consols are found by CBR", result);
		}

		public void TestMatchCoLoadAndNormalConsol_MatchMasterBillPrimarily()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns((List<ZString>)null);
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_CoLoadBookingReference = "GOODS";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_CoLoadMasterBill = string.Empty;
			consol2.JK_CoLoadBookingReference = "BOOK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = string.Empty;
			consol3.JK_BookingReference = "BOOK";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(7);

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_TransportMode = Constants.TransportModes.Sea;
			consol4.JK_MasterBillNum = string.Empty;
			consol4.JK_BookingReference = "CAR";
			consol4.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			var consol5 = Factory.New<CommonConsol>();
			consol5.JK_TransportMode = Constants.TransportModes.Sea;
			consol5.JK_MasterBillNum = string.Empty;
			consol5.JK_BookingReference = "CAR";
			consol5.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol1, result);
		}

		public void TestMatchCoLoadAndNormalConsol_MatchCarrierC1CCodeOnly()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.CarrierC1CCode).Returns("C1CO");

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol1, result);
		}

		public void TestMatchCoLoadAndNormalConsol_MatchLoadPortOnly()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.MBOLOriginUNLOCO).Returns("AUMEL");
			context.Setup(m => m.MBOLDestinationUNLOCO).Returns("SGSIN");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol1, result);
		}

		public void TestMatchCoLoadAndNormalConsol_MatchDischargePortOnly()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.MBOLOriginUNLOCO).Returns("AUMEL");
			context.Setup(m => m.MBOLDestinationUNLOCO).Returns("SGSIN");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_CoLoadMasterBill = "1234";
			consol1.JK_RL_NKDischargePort = "SGSIN";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_RL_NKDischargePort = "CNSHA";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol1, result);
		}

		public void TestMatchConsolAgainstMasterBillAndContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = string.Empty;
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);
			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "C00001";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "1234";
			consol2.JK_BookingReference = string.Empty;
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "1234";
			consol3.JK_BookingReference = string.Empty;
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(container, result);
		}

		public void TestMatchConsolAgainstBookingReferenceAndContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "C00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "";
			consol1.JK_BookingReference = "BOOK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);
			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "C00001";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "BOOK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(5);
			consol2.Containers.AddNew().JC_ContainerNum = "C00002";

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "";
			consol3.JK_BookingReference = "BOOK";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(container, result);
		}

		public void TestMatchConsolAgainstBookingReferenceEmptyMasterBillNoContainerNumber()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("");
			context.Setup(m => m.CarriersBookingReference).Returns("BOOK");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "";
			consol1.JK_BookingReference = "BOOK";
			consol1.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(10);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "";
			consol2.JK_BookingReference = "BOOK";
			consol2.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-500);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = Constants.TransportModes.Sea;
			consol3.JK_MasterBillNum = "";
			consol3.JK_BookingReference = "CARS";
			consol3.JK_SystemCreateTimeUtc = ZDateTime.Now.AddDays(20);

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).Single();

			AssertEquals(consol1, result);
		}

		public void TestMatchTransportLegByLocationEventParameterForDeparture()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.EventType).Returns("DEP");
			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");
			context.Setup(m => m.LegOriginUNLOCO).Returns("USLAX");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "00001";

			var transportLeg = consol1.Transports[0];
			transportLeg.JW_RL_NKLoadPort = "USLAX";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals("Transport Leg match", transportLeg.PK, result.PK);

			transportLeg.JW_RL_NKLoadPort = "CNSHA";

			Factory.Save();

			result = consolLinker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("Fallback to container when transport leg does not match", container.PK, result.PK);
		}

		public void TestMatchTransportLegByLocationEventParameterForArrival()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.EventType).Returns("ARV");
			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");
			context.Setup(m => m.LegDestinationUNLOCO).Returns("AUBNE");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "00001";

			var transportLeg = consol1.Transports[0];
			transportLeg.JW_RL_NKDiscPort = "AUBNE";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals("Transport Leg match", transportLeg.PK, result.PK);

			transportLeg.JW_RL_NKDiscPort = "CNSHA";

			Factory.Save();

			result = consolLinker.GetLogParent(eventValueObject.Object).First();
			AssertEquals("Fallback to container when transport leg does not match", container.PK, result.PK);
		}

		public void TestDoesNotMatchTransportLegByLocationEventParameterForOtherEvent()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.EventType).Returns("ATH");
			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("");
			context.Setup(m => m.MBOLNumber).Returns("1234");
			context.Setup(m => m.ContainerNumbers).Returns(new List<ZString> { "00001" });
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");
			context.Setup(m => m.LegOriginUNLOCO).Returns("USLAX");
			context.Setup(m => m.LegDestinationUNLOCO).Returns("MYKUL");

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "00001";

			var transportLeg = consol1.Transports[0];
			transportLeg.JW_RL_NKLoadPort = "USLAX";
			transportLeg.JW_RL_NKDiscPort = "MYKUL";

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(eventValueObject.Object).First();

			AssertEquals("Matches container even though ports match", container.PK, result.PK);
		}

		public void TestGetLogParent_FLO_MatchContainer_UnlinkTransportWhenVesselDoesNotMatch()
		{
			var @event = new Event();
			@event.EventType = "FLO";
			@event.EventParameters = new EventParameters
			{
				TransportMode = Core.Constants.TransportModes.Sea,
				Facility = CargoWise.EventReference.Constants.Facilities.Code.Terminal
			};

			@event.ContextCollection = new List<Context>()
			{
				new Context { Type = new ContextType { Type = nameof(Event.ContextTypes.MBOLNumber) }, Value = "1234" },
				new Context { Type = new ContextType { Type = nameof(Event.ContextTypes.MAWBNumber) }, Value = "" },
				new Context { Type = new ContextType { Type = nameof(Event.ContextTypes.ContainerNumber) }, Value = "00001" },
				new Context { Type = new ContextType { Type = nameof(Event.ContextTypes.LegOriginUNLOCO) }, Value = "USLAX" },
				new Context { Type = new ContextType { Type = nameof(Event.ContextTypes.LegDestinationUNLOCO) }, Value = "MYKUL" },
				new Context { Type = new ContextType { Type = nameof(Event.ContextTypes.VesselName) }, Value = "NEW VESSEL" },
			};

			var helper = new Mock<IUniversalFreightHelper>();

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "1234";
			consol1.JK_BookingReference = "";

			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "00001";

			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "USLAX", "MYKUL", "DNN VESSEL", "VOY123");
			var transportLeg = consol1.Transports[0];
			transportLeg.JW_RL_NKLoadPort = "USLAX";
			transportLeg.JW_RL_NKDiscPort = "MYKUL";
			transportLeg.JW_JX = sailing.PK;
			transportLeg.JW_IsLinked = true;

			Factory.Save();

			var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
			var result = consolLinker.GetLogParent(@event).First();

			AssertEquals("Matches container", container.PK, result.PK);
			AssertEquals("Transport is unlinked because vessel does not match", false, transportLeg.JW_IsLinked);
			AssertContains("STU log reference for Container Automation.", "Change of Vessel Detected", transportLeg.Logs.GetAllLogs().Cast<StmALog>().First(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).SL_Reference);
		}

		public void TestGetLogParent_ShouldNotMatchAIRConsol_WhenItsCreatedDateIsBeforeMAWBRecyclePeriod()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var eventValueObject = mock.Create<IXmlEventValueObject>();
			var context = mock.Create<IXmlEventValueObjectContextValueList>();
			var helper = mock.Create<IUniversalFreightHelper>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("1234");
			context.Setup(m => m.MBOLNumber).Returns("");

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "1234";

			using (FreightDataRegistry.Instance.MAWBRecyclePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				consol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-10);
				Factory.Save();

				var consolLinker = new ConsolLinker<CommonConsol>(Factory, helper.Object);
				var result = consolLinker.GetLogParent(eventValueObject.Object);
				AssertNotNull("Consol should match", result);
				AssertEquals("Consol should match", consol.PK, result[0].PK);

				consol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-11);
				Factory.Save();

				result = consolLinker.GetLogParent(eventValueObject.Object);
				AssertNull("Consol should not match", result);
			}
		}
	}
}

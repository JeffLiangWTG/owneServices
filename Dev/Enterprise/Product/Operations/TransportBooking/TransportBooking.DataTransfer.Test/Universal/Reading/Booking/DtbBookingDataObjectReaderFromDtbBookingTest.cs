using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingDataObjectReaderFromDtbBookingTest : DtbBookingDataObjectReaderTest
	{
		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_RecipientRoleMissing()
		{
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			Assert("Booking must not be converted to agent booking with billing.", !booking.KM_IsAgentBooking);
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_DifferentServiceCode()
		{
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.AMD;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			transportBooking.DataContext.SetWorkflowInfo(workflow);
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			Assert("Booking must not be converted to agent booking with billing.", !booking.KM_IsAgentBooking);
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_Success()
		{
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.CBA;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			transportBooking.DataContext.SetWorkflowInfo(workflow);
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			Assert("Booking is now marked as agent booking.", booking.KM_IsAgentBooking);
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_ForTesting_WhenTestCbaIdSetAndRecipientRoleIsBookingParty_Success()
		{
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TESTSENDER"))
			{
				var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New()
				};
				transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
				var workflow = new WorkflowInfo();
				var recipientRole = new RecipientRoleDetail();
				recipientRole.Type = RecipientRoleType.BKP;
				workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
				transportBooking.DataContext.SetWorkflowInfo(workflow);
				var interchange = Factory.New<XmlEDIInterchange>();
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				interchange.EI_From = "TESTSENDER";
				var message = Factory.New<XmlEDIMessage>();
				message.EM_EI = interchange.PK;
				Logger.SourceMessage = message;
				Logger.TopLevelDataObject = transportBooking;

				var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
				var booking = reader.ReadIntoBusinessObject();

				Assert("Test Carrier Booking Agent Id conditions met, so booking must be marked as agent booking.", booking.KM_IsAgentBooking);
			}
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_ForTesting_WhenTestCbaIdSetAndRecipientRoleIsNotBookingParty_DoNotMarkAsAgentBooking()
		{
			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TESTSENDER"))
			{
				var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New()
				};
				transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
				var workflow = new WorkflowInfo();
				var recipientRole = new RecipientRoleDetail();
				var someRoleOtherThanBookingParty = RecipientRoleType.ACT;
				recipientRole.Type = someRoleOtherThanBookingParty;
				workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
				transportBooking.DataContext.SetWorkflowInfo(workflow);
				var interchange = Factory.New<XmlEDIInterchange>();
				interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
				interchange.EI_From = "TESTSENDER";
				var message = Factory.New<XmlEDIMessage>();
				message.EM_EI = interchange.PK;
				Logger.SourceMessage = message;
				Logger.TopLevelDataObject = transportBooking;

				var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
				var booking = reader.ReadIntoBusinessObject();

				Assert("Test Carrier Booking Agent Id conditions not met, so booking should not be marked as agent booking.", !booking.KM_IsAgentBooking);
			}
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_UnauthorisedSender()
		{
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.CBA;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			transportBooking.DataContext.SetWorkflowInfo(workflow);
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "NEW PHONE WHO DIS?";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			Assert("Booking must not be marked as agent booking as sender is not authorised.", !booking.KM_IsAgentBooking);
			AssertContains("The sender is not authorized to use the service code \"CBA\".", Logger.GetWarnings());
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_Success_AsSubShipment()
		{
			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			consol.DataContext.AddDataTarget(DataContextType.TransportBookingConsolidation, "");
			consol.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.CBA;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			consol.DataContext.SetWorkflowInfo(workflow);
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { transportBooking });
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = consol;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(consol, Logger, Factory, Factory.New<DtbBookingConsolidation>(), consol);
			var booking = reader.ReadIntoBusinessObject();

			Assert("Booking should be marked as agent booking.", booking.KM_IsAgentBooking);
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_HasParent()
		{
			var parent = Factory.BOFactory.New<IForwardingShipment>();
			var consol = Factory.New<DtbBookingConsolidation>();
			consol.KB_ParentID = parent.PK;
			consol.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var transportBookingBO = Factory.New<DtbBooking>();
			transportBookingBO.KM_KB_Booking = consol.PK;
			Factory.SaveForTesting();
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, transportBookingBO.KM_JobID);
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.CBA;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			transportBooking.DataContext.SetWorkflowInfo(workflow);
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			AssertEquals("Must not replace existing parent.", JobShipmentSchema.Constants.Prefix, booking.ConsolidationSingleJob.KB_ParentTableCode);
			AssertEquals("Must not replace existing parent.", parent.PK, booking.ConsolidationSingleJob.KB_ParentID);
			Assert("Booking must not be marked as agent booking.", !booking.KM_IsAgentBooking);
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_HasJobHeader()
		{
			var consol = Factory.New<DtbBookingConsolidation>();
			var transportBookingBO = Factory.New<DtbBooking>();
			transportBookingBO.KM_KB_Booking = consol.PK;
			new JobHeader.Loader(transportBookingBO).TryCreate();
			Factory.SaveForTesting();
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, transportBookingBO.KM_JobID);
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.CBA;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			transportBooking.DataContext.SetWorkflowInfo(workflow);
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			AssertNotNull("Must not delete existing JobHeader on Transport Booking.", new JobHeader.Loader(transportBookingBO).Load());
			Assert("Booking should now be marked as agent booking", booking.KM_IsAgentBooking);
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_HasJobHeaderWithCharges()
		{
			ErrorReporter.Clear();

			var consol = Factory.New<DtbBookingConsolidation>();
			var transportBookingBO = Factory.New<DtbBooking>();
			transportBookingBO.KM_KB_Booking = consol.PK;
			var jobHeader = new JobHeader.Loader(transportBookingBO).TryCreate();
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_GC = jobHeader.JH_GC;
			transactionLine.AL_JH = jobHeader.PK;
			Factory.SaveForTesting();

			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, transportBookingBO.KM_JobID);
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.CBA;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			transportBooking.DataContext.SetWorkflowInfo(workflow);
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			var updatedJobHeader = Factory.Load<JobHeader>(jobHeader.PK);
			Assert("Booking should now have an agent booking", booking.KM_IsAgentBooking);
			CombineAssertions("JobHeader should now have agent booking as parent", () =>
			{
				Assert("Should have not reported error", !ErrorReporter.LastExceptionsReported().Any());
				AssertEquals("JobHeader parent ID should still be booking", booking.PK, updatedJobHeader.JH_ParentID);
				AssertEquals("JobHeader parent table code should still be 'KM' (DtbBooking)", DtbBookingSchema.Constants.Prefix, updatedJobHeader.JH_ParentTableCode);
			});

			ErrorReporter.Clear();
		}

		public void TestBeforePopulateBusinessObject_MarkAsAgentBooking_eAdaptor()
		{
			var transportBooking = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New()
			};
			transportBooking.DataContext.AddDataTarget(DataContextType.TransportBooking, "");
			var workflow = new WorkflowInfo();
			var recipientRole = new RecipientRoleDetail();
			recipientRole.Type = RecipientRoleType.TPC;
			recipientRole.ServiceCode = ServiceCodeType.CBA;
			workflow.RecipientRoles = new List<RecipientRoleDetail> { recipientRole };
			transportBooking.DataContext.SetWorkflowInfo(workflow);
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			interchange.EI_From = "SMARTFREIGHT_EAD";
			var message = Factory.New<XmlEDIMessage>();
			message.EM_EI = interchange.PK;
			Logger.SourceMessage = message;
			Logger.TopLevelDataObject = transportBooking;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(transportBooking, Logger, Factory, Factory.New<DtbBookingConsolidation>(), transportBooking);
			var booking = reader.ReadIntoBusinessObject();

			Assert("Booking must not be marked as agent booking.", !booking.KM_IsAgentBooking);
		}

		public void TestGetExistingBusinessObject()
		{
			var factory = new UniversalObjectFactory();
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consolidation = factory.New<DtbBookingConsolidation>();
			var booking = factory.New<DtbBooking>();

			var readerWithoutBooking = new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, Logger, factory, consolidation, bookingDataObject);
			var readerWithBooking = new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, Logger, factory, consolidation, bookingDataObject, booking);
			AssertNull(((ITopLevelDataObjectReader)readerWithoutBooking).GetExistingBusinessObject());
			AssertEquals(booking, ((ITopLevelDataObjectReader)readerWithBooking).GetExistingBusinessObject());
		}

		public void TestInstructions()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "CONT123", ContainerType = new ContainerType { Code = "20GP" } },
				new Container { Link = 2, ContainerNumber = "CONT456", ContainerType = new ContainerType { Code = "40GP" } }
			});

			bookingDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { Link = 3, ReferenceNumber = "PACK123" },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { Link = 4, ReferenceNumber = "PACK456" }
			});

			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			var packageJobReader = new PkgPackageJobDataObjectReader(bookingDataObject, Logger, factory, consolidation);
			var packageJob = packageJobReader.ReadIntoBusinessObject();
			var containerCONT123 = packageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("CONT123");
			var containerCONT456 = packageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("CONT456");
			var packagePACK123 = packageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("PACK123");
			var packagePACK456 = packageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("PACK456");

			bookingDataObject.SetInstructionCollection(() => new DataObjectList<Instruction>());
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { ServiceInstruction = "INSTRUCTION1" };
			instructionDataObject.SetInstructionContainerLinkCollection(() => new List<InstructionContainerLink>());
			instructionDataObject.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>());

			// Add container confirmations
			var containerLinkCONT123 = new InstructionContainerLink();
			containerLinkCONT123.ContainerLink = 1;
			containerLinkCONT123.Quantity = 3;
			containerLinkCONT123.ConfirmationCollection = new List<Confirmation>
			{
				new Confirmation { Reference = "CREF1" }, new Confirmation { Reference = "CREF2" }, // confirmations only for container
				new Confirmation { Reference = "IREF1", Quantity = 3 }, new Confirmation { Reference = "IREF2", Quantity = 3 }, // confirmations for all packages
				new Confirmation { Reference = "CPREF1" } // emulate having the same confirmation on both the loose and container divots
			};

			var containerLinkCONT456 = new InstructionContainerLink();
			containerLinkCONT456.ContainerLink = 2;
			containerLinkCONT456.Quantity = 4;
			containerLinkCONT456.ConfirmationCollection = new List<Confirmation>
			{
				new Confirmation { Reference = "IREF1", Quantity = 4 }, new Confirmation { Reference = "IREF2", Quantity = 4 }, // confirmations for all packages
				new Confirmation { Reference = "CREF3" } // for a container only
			};

			instructionDataObject.InstructionContainerLinkCollection.Add(containerLinkCONT123);
			instructionDataObject.InstructionContainerLinkCollection.Add(containerLinkCONT456);

			// Add loose package confirmations
			var packageLinkPACK123 = new InstructionPackingLineLink();
			packageLinkPACK123.PackingLineLink = 3;
			packageLinkPACK123.Quantity = 1;
			packageLinkPACK123.ConfirmationCollection = new List<Confirmation>
			{   new Confirmation { Reference = "PREF1" }, new Confirmation { Reference = "PREF2" }, // confirmations only for loose packages
				new Confirmation { Reference = "IREF1", Quantity = 1 }, new Confirmation { Reference = "IREF2", Quantity = 1 },  // confirmations for all packages
				new Confirmation { Reference = "CPREF1" } // emulate having the same confirmation on both the loose and container divots
			};

			var packageLinkPACK456 = new InstructionPackingLineLink();
			packageLinkPACK456.PackingLineLink = 4;
			packageLinkPACK456.Quantity = 1;
			packageLinkPACK456.ConfirmationCollection = new List<Confirmation>
			{
				new Confirmation { Reference = "IREF1", Quantity = 1 }, new Confirmation { Reference = "IREF2", Quantity = 1 },  // confirmations for all packages
				new Confirmation { Reference = "PREF3" }  // for a loose package Only
			};

			instructionDataObject.InstructionPackingLineLinkCollection.Add(packageLinkPACK123);
			instructionDataObject.InstructionPackingLineLinkCollection.Add(packageLinkPACK456);

			bookingDataObject.InstructionCollection.Add(instructionDataObject);

			var reader = new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, Logger, factory, consolidation, bookingDataObject, packageJobReader: packageJobReader);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals(1, booking.Instructions.Count);

			var instruction = booking.Instructions[0];
			AssertEquals("INSTRUCTION1", instruction.KN_ServiceInstruction);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "IREF1", "IREF2" }, instruction.Confirmations.Where(i => i.PackageDivot == null).Select(c => c.KK_ReferenceNum));
			AssertEquals(4, instruction.PackageDivots.Count);

			var containerPackageDivotForCONT123 = instruction.PackageDivots.Single(d => d.KD_KP_Package == containerCONT123.PK && d.KD_Quantity == 3);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CREF1", "CREF2", "CPREF1" }, containerPackageDivotForCONT123.ConfirmationsDivotOnly.Select(c => c.KK_ReferenceNum));

			var containerPackageDivotForCONT456 = instruction.PackageDivots.Single(d => d.KD_KP_Package == containerCONT456.PK && d.KD_Quantity == 4);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CREF3" }, containerPackageDivotForCONT456.ConfirmationsDivotOnly.Select(c => c.KK_ReferenceNum));

			var packagePackageDivotForPACK123 = instruction.PackageDivots.Single(d => d.KD_KP_Package == packagePACK123.PK && d.KD_Quantity == 1);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PREF1", "PREF2", "CPREF1" }, packagePackageDivotForPACK123.ConfirmationsDivotOnly.Select(c => c.KK_ReferenceNum));

			var packagePackageDivotForPack456 = instruction.PackageDivots.Single(d => d.KD_KP_Package == packagePACK456.PK && d.KD_Quantity == 1);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "PREF3" }, packagePackageDivotForPack456.ConfirmationsDivotOnly.Select(c => c.KK_ReferenceNum));
		}

		public void TestSettingTemplateDoesNotCreateInstructions()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			bookingDataObject.LocalTransportJobType = new CodeDescriptionPair4Char { Code = "EFDR" };
			var consolidation = Helper.CreateConsolidation();
			var reader = GetNewReader(bookingDataObject, Logger, consolidation, bookingDataObject);
			var booking = reader.ReadIntoBusinessObject();
			AssertEquals("EFDR", booking.KM_KT_NKBookingTemplate);
			AssertEquals(0, booking.Instructions.Count);
		}

		public void TestAdditionalReferences_AdditionalReferenceCollection_SourceHasNoRefs()
		{
			var factory = new UniversalObjectFactory();
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.TransportMode = new CodeDescriptionPair();
			consol.TransportMode.Code = TransportModes.Air;
			consol.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.TransportReference }, ReferenceNumber = "CONSOLIDATION-TRF" });
			consol.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.OrderNumber }, ReferenceNumber = "CONSOLIDATION-ORD" });

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TransportMode = new CodeDescriptionPair();
			shipment.TransportMode.Code = TransportModes.Air;
			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber }, ReferenceNumber = "Shipment-ETBN" });
			shipment.AdditionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = AdditionalReferenceTypes.Codes.BookingPartyReference }, ReferenceNumber = "Shipment-BPR" });

			var reader = GetNewReader(shipment, Logger, consolidation, consol, booking);
			var bookingReadIn = reader.ReadIntoBusinessObject();
			AssertEquals("Booking AdditionalReferenceNumbers Count", 2, bookingReadIn.AdditionalReferenceNumbers.Count);

			var additionalReference1 = bookingReadIn.AdditionalReferenceNumbers[0];
			AssertEquals("additionalReference1.CE_EntryNum", "Shipment-ETBN", additionalReference1.CE_EntryNum);
			AssertEquals("additionalReference1.CE_EntryType", AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, additionalReference1.CE_EntryType);

			var additionalReference2 = bookingReadIn.AdditionalReferenceNumbers[1];
			AssertEquals("additionalReference2.CE_EntryNum", "Shipment-BPR", additionalReference2.CE_EntryNum);
			AssertEquals("additionalReference2.CE_EntryType", AdditionalReferenceTypes.Codes.BookingPartyReference, additionalReference2.CE_EntryType);
		}

		protected override DtbBookingDataObjectReader GetNewReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBooking booking, UniversalShipment topLevelDO)
		{
			return GetNewReader(shipment, logger, Factory.New<DtbBookingConsolidation>(), topLevelDO, booking, factory);
		}

		protected override DtbBookingDataObjectReader GetNewReader(UniversalShipment bookingDataObject, IXmlImportLogger logger, DtbBookingConsolidation consolidation, UniversalShipment topLevelDO, DtbBooking booking = null, UniversalObjectFactory uFactory = null)
		{
			return new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, logger, uFactory ?? new UniversalObjectFactory(), consolidation, topLevelDO, booking);
		}
	}

	[TestedType(typeof(DtbBookingDataObjectReaderFromDtbBooking.InstructionDataObjectCollectionReader))]
	class InstructionDataObjectCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			bookingDataObject.SetInstructionCollection(() => new DataObjectList<Instruction>());
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { ServiceInstruction = "INSTRUCTION1", Sequence = 1 };
			bookingDataObject.InstructionCollection.Add(instructionDataObject);
			bookingDataObject.InstructionCollection.Content = CollectionContent.Partial;
			var dtbBookingDataObjectReaderFromDtbBooking = new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, Logger, factory, consolidation, bookingDataObject);

			var booking = consolidation.Bookings.AddNew();
			var instruction1 = booking.Instructions.AddNew();
			instruction1.KN_Sequence = 1;
			var instruction2 = booking.Instructions.AddNew();
			instruction2.KN_Sequence = 2;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking.InstructionDataObjectCollectionReader(dtbBookingDataObjectReaderFromDtbBooking, booking, bookingDataObject.InstructionCollection);

			var yetToBeModifiedInstruction = booking.Instructions.Single(x => x.KN_Sequence == 1);
			AssertEquals("Precondition", "", yetToBeModifiedInstruction.ServiceInstruction);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching DtbBookingInstruction.
Information - Populating DtbBookingInstruction...
".Trim(), logger.Logs);

			AssertEquals("Should retain instruction not mentioned in reader", 2, booking.Instructions.Count);
			var modifiedInstruction = booking.Instructions.Single(x => x.KN_Sequence == 1);
			AssertEquals("Should be updated with the information in the instruction data object", "INSTRUCTION1", modifiedInstruction.ServiceInstruction);
		}

		public void TestNonMatchingSequence()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			bookingDataObject.SetInstructionCollection(() => new DataObjectList<Instruction>());
			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { ServiceInstruction = "INSTRUCTION3", Sequence = 3 };
			bookingDataObject.InstructionCollection.Add(instructionDataObject);
			bookingDataObject.InstructionCollection.Content = CollectionContent.Partial;
			var dtbBookingDataObjectReaderFromDtbBooking = new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, Logger, factory, consolidation, bookingDataObject);

			var booking = consolidation.Bookings.AddNew();
			var instruction1 = booking.Instructions.AddNew();
			instruction1.KN_Sequence = 1;
			var instruction2 = booking.Instructions.AddNew();
			instruction2.KN_Sequence = 2;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking.InstructionDataObjectCollectionReader(dtbBookingDataObjectReaderFromDtbBooking, booking, bookingDataObject.InstructionCollection);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Information - Populating DtbBookingInstruction...
".Trim(), logger.Logs);

			AssertEquals("Should include new instruction and existing instructions", 3, booking.Instructions.Count);
			var newInstruction = booking.Instructions.Single(x => x.KN_Sequence == 3);
			AssertEquals("New instruction should be populated with ServiceInstruction data", "INSTRUCTION3", newInstruction.ServiceInstruction);
		}

		public void TestNoSequence()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			bookingDataObject.SetInstructionCollection(() => new DataObjectList<Instruction>());
			var instructionDataObjectA = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { ServiceInstruction = "INSTRUCTIONA" };
			bookingDataObject.InstructionCollection.Add(instructionDataObjectA);
			var instructionDataObjectB = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { ServiceInstruction = "INSTRUCTIONB" };
			bookingDataObject.InstructionCollection.Add(instructionDataObjectB);
			bookingDataObject.InstructionCollection.Content = CollectionContent.Partial;
			var dtbBookingDataObjectReaderFromDtbBooking = new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, Logger, factory, consolidation, bookingDataObject);

			var booking = consolidation.Bookings.AddNew();
			var instruction1 = booking.Instructions.AddNew();
			instruction1.KN_Sequence = 1;
			var instruction2 = booking.Instructions.AddNew();
			instruction2.KN_Sequence = 2;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking.InstructionDataObjectCollectionReader(dtbBookingDataObjectReaderFromDtbBooking, booking, bookingDataObject.InstructionCollection);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Information - Populating DtbBookingInstruction...
Information - Successfully loaded matching DtbBookingInstruction.
Information - Populating DtbBookingInstruction...
".Trim(), logger.Logs);

			AssertEquals("Should include only last instruction without sequence number", 3, booking.Instructions.Count);
			var newInstruction = booking.Instructions.Single(x => x.KN_Sequence == 0);
			AssertEquals("New instruction should be populated with information from last data object when sequence numbers are not supplied", "INSTRUCTIONB", newInstruction.ServiceInstruction);
		}

		public void TestRepeatedSequence()
		{
			var bookingDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var factory = new UniversalObjectFactory();
			var consolidation = factory.New<DtbBookingConsolidation>();
			bookingDataObject.SetInstructionCollection(() => new DataObjectList<Instruction>());
			var instructionDataObject3A = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { ServiceInstruction = "INSTRUCTION3A", Sequence = 3 };
			var instructionDataObject3B = new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { ServiceInstruction = "INSTRUCTION3B", Sequence = 3 };
			bookingDataObject.InstructionCollection.Add(instructionDataObject3A);
			bookingDataObject.InstructionCollection.Add(instructionDataObject3B);
			bookingDataObject.InstructionCollection.Content = CollectionContent.Partial;
			var dtbBookingDataObjectReaderFromDtbBooking = new DtbBookingDataObjectReaderFromDtbBooking(bookingDataObject, Logger, factory, consolidation, bookingDataObject);

			var booking = consolidation.Bookings.AddNew();
			var instruction1 = booking.Instructions.AddNew();
			instruction1.KN_Sequence = 1;
			var instruction2 = booking.Instructions.AddNew();
			instruction2.KN_Sequence = 2;

			var reader = new DtbBookingDataObjectReaderFromDtbBooking.InstructionDataObjectCollectionReader(dtbBookingDataObjectReaderFromDtbBooking, booking, bookingDataObject.InstructionCollection);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - No matching DtbBookingInstruction found, creating new DtbBookingInstruction.
Information - Populating DtbBookingInstruction...
Information - Successfully loaded matching DtbBookingInstruction.
Information - Populating DtbBookingInstruction...
".Trim(), logger.Logs);

			AssertEquals("Should include only last instruction with repeating sequence number", 3, booking.Instructions.Count);
			var newInstruction = booking.Instructions.Single(x => x.KN_Sequence == 3);
			AssertEquals("New instruction should be populated with information from last data object when sequence numbers are identical", "INSTRUCTION3B", newInstruction.ServiceInstruction);
		}

		TestErrorLogger Logger => logger ?? (logger = new TestErrorLogger());

		TestErrorLogger logger;
	}
}

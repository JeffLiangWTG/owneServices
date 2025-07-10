using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestNoExecptionThrownWhenAddInfoTypeIsNotSupport()
		{
			using (Enterprise.ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(false))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationDataObject.GoodsDescription = "TEST ADDINFO";
				declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = "ACE" };
				declarationDataObject.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea };

				var vdeDataObject = new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USPGAVehicleDetails },
					AddInfoCollection = AddInfoCollectionCreator.CreateCollection("*BuildMonth=01" + "*ProgramCode=FDA" + "*ProcessingCode=FDA")
				};

				var fdaDataObject = new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USACEFDA },
					AddInfoCollection = AddInfoCollectionCreator.CreateCollection("*LineNo=1"),
					AddInfoGroupCollection = new List<AddInfoGroup>(new[] { vdeDataObject })
				};
				var invoiceLineDataObject = SetupCommercialInvoiceLine(1, addInfoGroupCollection: new List<AddInfoGroup>(new[] { fdaDataObject }));
				invoiceLineDataObject.LinePrice = 1500m;
				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);
				declarationDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
				declarationDataObject.AdditionalReferenceCollection.Add(new AdditionalReference() { ReferenceNumber = "REF0001", Type = new EntryType() { Code = "RRN" } });
				declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>());
				declarationDataObject.NoteCollection.Add(new Note() { Description = "Note0001" });
				declarationDataObject.AllowUpdateOfCustomsDeclarationAfterCommencement = true;

				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
				var manager = new UniversalMessageProcessingManager(xmlSessionTracker);
				AssertNoExceptionThrown(() => manager.Process(message));

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertContains("Error - The 'VDE' node should not be nested under the 'FDA' node. Please ensure that 'VDE' is placed at the correct hierarchical level.", xmlSessionTracker.ToString());
			}
		}

		public void TestApplicationCodeMappingWithDeclarationMatched()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_DeclarationReference = "BX00004";
				declaration.JE_ApplicationCode = "DG";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Factory.SaveForTesting();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "BX00004");

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair()
					{
						Code = JobMessageTypeList.Codes.Import
					},
					MessagingApplicationCode = new CodeDescriptionPair()
					{
						Code = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted
					}
				};
				declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
					"*Bool=Y" +
					"*Code=Z1K" +
					"*Date=2020-11-25 17:36:48" +
					"*DateOnly=2019-01-31" +
					"*AnotherDecimal=873.2974" +
					"*Description=HI BOB" +
					"*Number=78234" +
					"*Short=845" +
					"*IsSystem=Y"));

				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
				var manager = new UniversalMessageProcessingManager(xmlSessionTracker);
				manager.Process(message);

				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
				AssertContains("Error - The value of MessagingApplicationCode tag(BTH) is different from the application code of the target declaration(DG).", xmlSessionTracker.ToString());

				declaration.Reload();
				AssertEquals("EXP", declaration.JE_MessageType);
				AssertEquals("DG", declaration.JE_ApplicationCode);
			}
		}

		public void TestApplicationCodeMappingWithNoDeclarationMatchedIfSubmissionTypeIsITF()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertApplicationCodeMappingWithNoDeclarationMatched(DeclarationApplicationCodeListForRegistry.Codes.Interfaced);
			}
		}

		public void TestApplicationCodeMappingWithNoDeclarationMatchedIfSubmissionTypeIsBTH()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertApplicationCodeMappingWithNoDeclarationMatched(DeclarationApplicationCodeListForRegistry.Codes.Interfaced);
			}
		}

		public void TestApplicationCodeMappingWithNoDeclarationMatchedIfSubmissionTypeIsBIT()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			{
				AssertApplicationCodeMappingWithNoDeclarationMatched(DeclarationApplicationCodeListForRegistry.Codes.Interfaced);
			}
		}

		public void TestApplicationCodeMappingWithNoDeclarationMatchedIfSubmissionTypeIsBLT()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			{
				var msg = "MessagingApplicationCode 'ITF' is not a valid application code for FR Customs Declaration with message type 'IMP' based on registry 'Local Country Customs Interface' setting.";
				AssertExceptionThrown<MessageProcessingBusinessFailureException>("Should throw exception message", msg, () => AssertApplicationCodeMappingWithNoDeclarationMatched("DG"));
			}
		}

		public void TestApplicationCodeMappingWithRegistryIsNotTurnedOn()
		{
			localCountryCustomsInterface?.Dispose();
			var msg = "MessagingApplicationCode 'ITF' is not a valid application code for FR Customs Declaration with message type 'IMP' based on registry 'Local Country Customs Interface' setting.";
			AssertExceptionThrown<MessageProcessingBusinessFailureException>("Should throw exception message", msg, () => AssertApplicationCodeMappingWithNoDeclarationMatched("DG"));
		}

		void AssertApplicationCodeMappingWithNoDeclarationMatched(ZString applicationCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair()
					{
						Code = JobMessageTypeList.Codes.Import
					},
					MessagingApplicationCode = new CodeDescriptionPair()
					{
						Code = DeclarationApplicationCodeListForRegistry.Codes.Interfaced
					}
				};
				declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
					"*Bool=Y" +
					"*Code=Z1K" +
					"*Date=2020-11-25 17:36:48" +
					"*DateOnly=2019-01-31" +
					"*AnotherDecimal=873.2974" +
					"*Description=HI BOB" +
					"*Number=78234" +
					"*Short=845" +
					"*IsSystem=Y"));

				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
				var manager = new UniversalMessageProcessingManager(xmlSessionTracker);
				manager.Process(message);

				var declarationBO = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GC, CurrentCompany.PK));
				AssertEquals(applicationCode, declarationBO.JE_ApplicationCode);
			}
		}

		public void TestApplicationCodeMappingWithNoDeclarationMatchedButIncorrectITFRegistry()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Philippines))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair()
					{
						Code = JobMessageTypeList.Codes.Import
					},
					MessagingApplicationCode = new CodeDescriptionPair()
					{
						Code = DeclarationApplicationCodeList.Codes.Builtin
					}
				};

				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
				var manager = new UniversalMessageProcessingManager(xmlSessionTracker);

				var expectedMsg =
					"MessagingApplicationCode 'BLT' is not a valid application code for PH Customs Declaration with message type 'IMP' based on registry 'Local Country Customs Interface' setting.";

				AssertExceptionThrown<MessageProcessingBusinessFailureException>("Should throw exception message", expectedMsg , () => manager.Process(message));

				var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GC, CurrentCompany.PK));
				AssertNull(declaration);
			}
		}

		public void TestApplicationCodeMappingWithNoDeclarationMatchedIfMessagingApplicationCodeIsNotITFAndNotITFRegistry()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair()
					{
						Code = JobMessageTypeList.Codes.Import
					},
					MessagingApplicationCode = new CodeDescriptionPair()
					{
						Code = "DG"
					}
				};
				declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
					"*Bool=Y" +
					"*Code=Z1K" +
					"*Date=2020-11-25 17:36:48" +
					"*DateOnly=2019-01-31" +
					"*AnotherDecimal=873.2974" +
					"*Description=HI BOB" +
					"*Number=78234" +
					"*Short=845" +
					"*IsSystem=Y"));

				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
				var manager = new UniversalMessageProcessingManager(xmlSessionTracker);

				var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GC, CurrentCompany.PK));
				AssertNull("[PreCondition]: declaration", declaration);

				manager.Process(message);

				declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GC, CurrentCompany.PK));
				AssertEquals("DG", declaration.JE_ApplicationCode);
			}
		}

		public void TestApplicationCodeMappingWithNoDeclarationMatchedIfMessagingApplicationCodeIsNotInTheList()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;

			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair()
					{
						Code = JobMessageTypeList.Codes.Import
					},
					MessagingApplicationCode = new CodeDescriptionPair()
					{
						Code = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted
					}
				};
				declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
					"*Bool=Y" +
					"*Code=Z1K" +
					"*Date=2020-11-25 17:36:48" +
					"*DateOnly=2019-01-31" +
					"*AnotherDecimal=873.2974" +
					"*Description=HI BOB" +
					"*Number=78234" +
					"*Short=845" +
					"*IsSystem=Y"));

				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var xmlSessionTracker = new XmlSessionTracker(serviceTaskLog);
				var manager = new UniversalMessageProcessingManager(xmlSessionTracker);

				var expectedMsg = "MessagingApplicationCode 'BTH' is not a valid application code for FR Customs Declaration with message type 'IMP' based on registry 'Local Country Customs Interface' setting.";

				AssertExceptionThrown<MessageProcessingBusinessFailureException>("Should throw exception", expectedMsg, () => manager.Process(message));

				var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_GC, CurrentCompany.PK));
				AssertNull(declaration);
			}
		}

		public void TestApplicationCodeForNonIntegratedCountryAndNonStandardButIsValid()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Lithuania))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationDataObject.GoodsDescription = "TEST ADDINFO";
				declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = "EMC" };

				var reader = new JobDeclarationDataObjectReaderForImportCountrySpecificRelatedDataTest(declarationDataObject, logger, Factory, null, true);
				AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => reader.ReadIntoBusinessObject());

				reader.ValidApplicationCodeForTesting = "EMC";
				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			}
		}

		public void TestUpdateSpecificDataWhenDeclarationMessagesHaveBeenSent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_DeclarationReference = "BX00004";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_MasterBill = "MB2343";
				Factory.SaveForTesting();

				var declarationDataObject = SetupDeclaration(null, "MB2343", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationDataObject.GoodsDescription = "TEST ADDINFO";
				declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = "ACE" };
				declarationDataObject.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea };
				var invoiceLineDataObject = SetupCommercialInvoiceLine(1, null, null);
				invoiceLineDataObject.LinePrice = 1500m;
				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", null, new DataObjectList<CommercialInvoiceHeader>(new[] { SetupCommercialInvoiceHeaderData(null, null, new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject })) }), null);
				declarationDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
				declarationDataObject.AdditionalReferenceCollection.Add(new AdditionalReference() { ReferenceNumber = "REF0001", Type = new EntryType() { Code = "RRN" } });
				declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>());
				declarationDataObject.NoteCollection.Add(new Note() { Description = "Note0001" });
				declarationDataObject.AllowUpdateOfCustomsDeclarationAfterCommencement = true;

				CombineAssertions("Before Data Import", () =>
				{
					AssertEquals("Transport mode", Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
					AssertEquals("Count of invoices", 0, declaration.Invoices.Count);
					AssertEquals("Count of invoice lines", 0, declaration.InvoiceLines.Count);
					AssertEquals("Count of additional references", 0, declaration.AdditionalReferenceNumbers.Count);
					AssertEquals("Count of notes", 0, declaration.NotesOfDeclarationOrShipment.GetAllNotes().Count);
				});

				var reader = new JobDeclarationDataObjectReaderForImportCountrySpecificRelatedDataTest(declarationDataObject, logger, Factory, null, false);
				reader.ReadIntoBusinessObject();

				CombineAssertions("After Data Import", () =>
				{
					AssertEquals("Transport mode", Core.Constants.TransportModes.Air, declaration.JE_TransportMode);
					AssertEquals("Count of invoices", 0, declaration.Invoices.Count);
					AssertEquals("Count of invoice lines", 0, declaration.InvoiceLines.Count);

					var additionalReferences = declaration.AdditionalReferenceNumbers;
					AssertEquals("Count of additional references.", 1, additionalReferences.Count);
					AssertEquals("RRN", additionalReferences[0].CE_EntryType);
					AssertEquals("REF0001", additionalReferences[0].CE_EntryNum);

					var notes = declaration.NotesOfDeclarationOrShipment.GetAllNotes().ToArray<StmNote>();
					AssertEquals("Count of notes.", 1, notes.Length);
					AssertEquals("Note0001", notes[0].ST_Description);
				});

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Information - Updated Declaration BX00004 from UniversalShipment.".Trim(), logger.Logs);
			}
		}

		public void TestJE_MessageTypeShouldEqualsToXML()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var supplier = CreateOrganisation("ABC", "ABC!@#1");
				supplier.OH_RL_NKClosestPort = "NLAAM";
				var importerDocumentaryAddress = CreateOrganisation("DEF", "DEF!@#1");
				importerDocumentaryAddress.OH_RL_NKClosestPort = "NLACK";
				var supplierDocumentaryAddress = CreateOrganisation("GHI", "GHI!@#1");
				supplierDocumentaryAddress.OH_RL_NKClosestPort = "NLAJM";

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import }
				};
				declarationDataObject.AddOrgAddress(writeManager, supplier, DocAddressType.Supplier);
				declarationDataObject.AddOrgAddress(writeManager, importerDocumentaryAddress, DocAddressType.ImporterDocumentaryAddress);
				declarationDataObject.AddOrgAddress(writeManager, supplierDocumentaryAddress, DocAddressType.SupplierDocumentaryAddress);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("Should Be IMP", JobMessageTypeList.Codes.Import, declarationBO.JE_MessageType);
			}
		}

		public void TestJE_MessageTypeShouldCalculatedWhenMessytypeOfXMLIsNull()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var supplier = CreateOrganisation("ABC", "ABC!@#1");
				supplier.OH_RL_NKClosestPort = "NLAAM";
				var importerDocumentaryAddress = CreateOrganisation("DEF", "DEF!@#1");
				importerDocumentaryAddress.OH_RL_NKClosestPort = "NLACK";
				var supplierDocumentaryAddress = CreateOrganisation("GHI", "GHI!@#1");
				supplierDocumentaryAddress.OH_RL_NKClosestPort = "NLAJM";

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext
				};
				declarationDataObject.AddOrgAddress(writeManager, supplier, DocAddressType.Supplier);
				declarationDataObject.AddOrgAddress(writeManager, importerDocumentaryAddress, DocAddressType.ImporterDocumentaryAddress);
				declarationDataObject.AddOrgAddress(writeManager, supplierDocumentaryAddress, DocAddressType.SupplierDocumentaryAddress);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("Should Be Calculated to EXP", JobMessageTypeList.Codes.Export, declarationBO.JE_MessageType);
			}
		}

		public void TestDataObjectReadFailureException_IfDeleteAnExistingInstructionCannotBeDeleted()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = mockDeclaration.Object;
			mockDeclaration.Protected().Setup<EntryInstructionProvider>("GetCustomsEntryInstructionProviderCore").Returns(new EntryInstructionProvider(declaration));

			var mockEntryInstruction = Factory.NewMoq<CusEntryInstruction>();
			mockEntryInstruction.Setup(m => m.CanDelete).Returns(false);
			mockEntryInstruction.Setup(m => m.ReasonForNotAbleToDelete)
				.Returns((NoResString)"Entry Instruction XX has a linked guarantee and cannot be deleted.");

			declaration.CustomsEntryInstructions.Add(mockEntryInstruction.Object);
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declaration.JE_DeclarationReference = "B00001000";

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			declarationDataObject.DataContext.DataTargetCollection.First().Key = "B00001000";

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("An existing instruction cannot be deleted", "Entry Instruction XX has a linked guarantee and cannot be deleted.", () =>
			{
				reader.ReadIntoBusinessObject();
			});
		}

		public void TestDoNotDeleteInvoiceHeaderIfCommercialInvoiceCollectionIsPartial()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INV1";
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line1.JI_Description = "INV1LINE1";
				invoice1Line1.JI_MatchingKey = "MATCH001";

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "INV2";
				var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
				invoice2Line1.JI_Description = "INV2LINE1";
				invoice2Line1.JI_MatchingKey = "MATCH002";

				declaration.JE_AutoWeightApportion = true;
				Factory.SaveForTesting();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var invoiceLineDataObject1 = new CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 1",
					DataImportMatchingKey = "MATCH001",
					LinePrice = 200,
					Weight = 0m
				};

				var invoiceDataObject1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
					Weight = 300,
					WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms },
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject1 })));

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = new CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject1 }) { Content = CollectionContent.Partial }
					}
				};
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("Have 2 invoice header", 2, declarationBO.Invoices.Count);
				AssertCollectionContains(invoice1, declarationBO.Invoices);
				AssertCollectionContains(invoice2, declarationBO.Invoices);
				AssertEquals("Invoice 1 has 1 invoice line", 1, invoice1.InvoiceLines.Count);
				AssertEquals("Invoice 2 has 1 invoice line", 1, invoice2.InvoiceLines.Count);
				AssertEquals(true, invoice1Line1.IsDeleted);
				AssertCollectionNotContains(invoice1Line1, invoice1.InvoiceLines);
				AssertCollectionContains(invoice2Line1, invoice2.InvoiceLines);
			}
		}

		public void TestImportDeclarationAddInfoChildData()
		{
			var declaration = Factory.New<JobDeclarationAddInfoChildSupporter>();
			var addInfoChild = (DummyBusinessObject)declaration.AddInfoChild;
			addInfoChild.Z0_Guid = declaration.PK;
			addInfoChild.Z0_Bool = ZBool.False;
			addInfoChild.Z0_Code = "Z2K";
			addInfoChild.Z0_Date = new ZDateTime(2020, 2, 25);
			addInfoChild.Z0_DateOnly = new ZDate(2019, 2, 1);
			addInfoChild.Z0_AnotherDecimal = 150.50m;
			addInfoChild.Z0_Description = "HI JOE";
			addInfoChild.Z0_Number = 85625;
			addInfoChild.Z0_Short = 1245;
			Factory.SaveForTesting();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair()
				{
					Code = JobMessageTypeList.Codes.Import
				}
			};
			declarationDataObject.SetAddInfoCollection(() => AddInfoCollectionCreator.CreateCollection(
				"*Bool=Y" +
				"*Code=Z1K" +
				"*Date=2020-11-25 17:36:48" +
				"*DateOnly=2019-01-31" +
				"*AnotherDecimal=873.2974" +
				"*Description=HI BOB" +
				"*Number=78234" +
				"*Short=845" +
				"*IsSystem=Y"));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			CombineAssertions(delegate
			{
				AssertSame(declaration, declarationBO);
				AssertEquals("Z0_Bool", ZBool.True, addInfoChild.Z0_Bool);
				AssertEquals("Z0_Code", "Z1K", addInfoChild.Z0_Code);
				AssertEquals("Z0_Date", new ZDateTime(2020, 2, 25), addInfoChild.Z0_Date);
				AssertEquals("Z0_DateOnly", new ZDate(2019, 2, 1), addInfoChild.Z0_DateOnly);
				AssertEquals("Z0_AnotherDecimal", 873.297m, addInfoChild.Z0_AnotherDecimal);
				AssertEquals("Z0_Description", "HI BOB", addInfoChild.Z0_Description);
				AssertEquals("Z0_Number", 78234, addInfoChild.Z0_Number);
				AssertEquals("Z0_Short", (short)1245, addInfoChild.Z0_Short);
			});
		}

		public void TestPopulateTransportDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				declarationDataObject.PortOfLoading = new UNLOCO() { Code = "SGSIN" };
				declarationDataObject.PortOfDischarge = new UNLOCO() { Code = "AUSYD" };
				declarationDataObject.VesselName = "ALS KRONOS";
				declarationDataObject.VoyageFlightNo = "015S";
				declarationDataObject.LloydsIMO = "VLI2020";
				declarationDataObject.TransportMode = new CodeDescriptionPair() { Code = "SEA" };

				declarationDataObject.SetTransportLegCollection(() =>
				{
					return new DataObjectList<TransportLeg>()
					{
						new TransportLeg() { VesselName = "FORWARDER OF AU", VoyageFlightNo = "006S", VesselLloydsIMO = "VLI2032", LegOrder = 1, PortOfDischarge = new UNLOCO() { Code = "SGSIN" } },
						new TransportLeg() { TransportMode = TransportMode.Air, VesselName = "AIR OF AUCKLAND", VoyageFlightNo = "017S", VesselLloydsIMO = "VLI3950", LegOrder = 2, PortOfDischarge = new UNLOCO() { Code = "AUBNE" } },
						new TransportLeg() { TransportMode = TransportMode.Sea, VesselName = "SPIRIT OF AUCKLAND", VoyageFlightNo = "038S", VesselLloydsIMO = "VLI6820", LegOrder = 3, PortOfDischarge = new UNLOCO() { Code = "AUBNE" } },
						new TransportLeg() { TransportMode = TransportMode.Sea, VesselName = "SEA OF AUCKLAND", VoyageFlightNo = "097S", VesselLloydsIMO = "VLI5802", LegOrder = 4, PortOfDischarge = new UNLOCO() { Code = "AUBNE" } },
					};
				});

				var dataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
				dataContext.ActionPurpose = new CodeDescriptionPair() { Code = string.Empty };

				declarationDataObject.DataContext = dataContext;

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declaration = reader.ReadIntoBusinessObject();

				CombineAssertions("Normal Purpose - Our system needs to use the value from top level object level directly.", () =>
				{
					AssertEquals("VesselName", "ALS KRONOS", declaration.JE_VesselName);
					AssertEquals("VoyageFlightNo", "015S", declaration.JE_VoyageFlightNo);
					AssertEquals("VesselLloydsIMO", "VLI2020", declaration.JE_LloydsIMO);
				});

				dataContext.ActionPurpose = new CodeDescriptionPair() { Code = "E2E" };

				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				declaration = reader.ReadIntoBusinessObject();

				CombineAssertions("E2E - Our system needs to create the declaration using the transport details and discharge port of the leg that first enters the country of the declaration.", () =>
				{
					AssertEquals("VesselName", "SPIRIT OF AUCKLAND", declaration.JE_VesselName);
					AssertEquals("VoyageFlightNo", "038S", declaration.JE_VoyageFlightNo);
					AssertEquals("VesselLloydsIMO", "VLI6820", declaration.JE_LloydsIMO);
				});

				declarationDataObject.SetTransportLegCollection(() => null);

				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				declaration = reader.ReadIntoBusinessObject();

				CombineAssertions("E2E - If there is no matched transport, system needs to use the value from top level object level directly.", () =>
				{
					AssertEquals("VesselName", "ALS KRONOS", declaration.JE_VesselName);
					AssertEquals("VoyageFlightNo", "015S", declaration.JE_VoyageFlightNo);
					AssertEquals("VesselLloydsIMO", "VLI2020", declaration.JE_LloydsIMO);
				});

				declarationDataObject.SetTransportLegCollection(() =>
				{
					return new DataObjectList<TransportLeg>()
					{
						new TransportLeg() { VesselName = "UNMATCHED FORWARDER OF AU", VoyageFlightNo = "006S", VesselLloydsIMO = "VLI2032", LegOrder = 1, PortOfDischarge = new UNLOCO() { Code = "SGSIN" } },
						new TransportLeg() { TransportMode = TransportMode.Air, VesselName = "UNMATCHED AIR OF AUCKLAND", VoyageFlightNo = "017S", VesselLloydsIMO = "VLI3950", LegOrder = 2, PortOfDischarge = new UNLOCO() { Code = "USCHI" } },
						new TransportLeg() { TransportMode = TransportMode.Sea, VesselName = "UNMATCHED SPIRIT OF AUCKLAND", VoyageFlightNo = "038S", VesselLloydsIMO = "VLI6820", LegOrder = 3, PortOfDischarge = new UNLOCO() { Code = "NZAKL" } },
						new TransportLeg() { TransportMode = TransportMode.Sea, VesselName = "UNMATCHED SEA OF CHINA", VoyageFlightNo = "097S", VesselLloydsIMO = "VLI5802", LegOrder = 4, PortOfDischarge = new UNLOCO() { Code = "CNSHA" } },
					};
				});

				CombineAssertions("E2E - If there is no matched transport, system needs to use the value from top level object level directly.", () =>
				{
					AssertEquals("VesselName", "ALS KRONOS", declaration.JE_VesselName);
					AssertEquals("VoyageFlightNo", "015S", declaration.JE_VoyageFlightNo);
					AssertEquals("VesselLloydsIMO", "VLI2020", declaration.JE_LloydsIMO);
				});
			}
		}

		public void TestGetReader_CountrySpecificReaderHasLowPriorityThanApplicationCodeSpecific()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MessagingApplicationCode = new CodeDescriptionPair { Code = "EMC" }
				};
				var reader = new CustomsShipmentDataObjectReaderProvider().GetReader(declarationDataObject, logger, Factory, null);
				var euDataTransferAssembly = AssemblyLoader.LoadAssembly("Enterprise.Customs.EU.EMCS.DataTransfer");
				var typeOfEMCSDeclarationDataObjectReader = euDataTransferAssembly.GetType("Enterprise.Customs.EU.EMCS.DataTransfer.EMCSDeclarationDataObjectReader");
				Assert(typeOfEMCSDeclarationDataObjectReader.IsInstanceOfType(reader));

				var gbDataTransferAssembly = AssemblyLoader.LoadAssembly("Enterprise.Customs.GB.DataTransfer");
				var typeOfGBDeclarationDataObjectReader = gbDataTransferAssembly.GetType("Enterprise.Customs.GB.DataTransfer.Universal.DeclarationDataObjectReader");
				reader = new CustomsShipmentDataObjectReaderProvider().GetReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), logger, Factory, null);
				Assert(typeOfGBDeclarationDataObjectReader.IsInstanceOfType(reader));
			}
		}

		public void TestJE_CustomsProfile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				declarationDataObject.CustomsProfileIdentifier = new ValueTypePair
				{
					Value = "123"
				};

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("123", declarationBO.JE_CustomsProfile);
			}
		}

		public void TestSupplierImporterPickupDeliveryDocumentaryDocAddressNotImported()
		{
			var message = GetQueuedUniversalShipmentMessage(ShipmentDeclarationWithSupplierImporterPickupDeliveryDocumentaryDocAddress);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Added Shipment from UniversalShipment.
Added Consol from UniversalShipment.
Successfully saved Consol C00001000 with 1 x BaseJobDeclaration, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "S00001000"));
			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals(0, declaration.DocAddresses.Count);
		}

		public void TestGetExisitingBusinessObject_MatchByEntryDetails()
		{
			// Env, ZA
			// prepare message,
			// new Declaration BLT
			// AddEntryHeader
			// Process message, no BO Found
			// new Declaration ITF
			// AddEntryHeader with same entryNumber
			// Process message, found the ITF job

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B03243223";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = JobMessageTypeList.Codes.Import;
				entry.CH_BGMReference = "LRN3242";
				entry.EntryNumber = "ENT3423";
				var entryLine = entry.AllEntryLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV324";
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_Description = "BOB THE BUILDER";
				var declarationDataObject = (UniversalShipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
				declarationDataObject.DataContext = null;

				CombineAssertions("ITF", () =>
				{
					declaration.JE_ApplicationCode = "ITF";
					declaration.Invoices.DeleteAll();
					Factory.SaveForTesting();

					var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
					var declarationBO = reader.ReadIntoBusinessObject();
					AssertEquals("Should have matched", declaration, declarationBO);
					AssertEquals("declarationBO.CustomsEntryHeaders.Count", 1, declarationBO.CustomsEntryHeaders.Count);
					AssertEquals("entry", entry, declarationBO.CustomsEntryHeaders[0]);
					AssertEquals("entry.EntryNumber", "ENT3423", entry.EntryNumber);
					AssertEquals("entry.AllEntryLines.Count", 1, entry.AllEntryLines.Count);
					AssertNotEquals("entryLine", entryLine, entry.AllEntryLines[0]);
					entryLine = entry.AllEntryLines[0];
					AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
					invoice = declarationBO.Invoices[0];
					AssertEquals("invoice.JZ_InvoiceNumber", "INV324", invoice.JZ_InvoiceNumber);
					AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
					invoiceLine = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_CL", entryLine.PK, invoiceLine.JI_CL);
					AssertEquals("invoiceLine.JI_Description", "BOB THE BUILDER", invoiceLine.JI_Description);
				});

				CombineAssertions("BLT", () =>
				{
					declaration.JE_ApplicationCode = "BLT";
					declaration.Invoices.DeleteAll();
					Factory.SaveForTesting();

					var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
					var declarationBO = reader.ReadIntoBusinessObject();
					AssertNotEquals("Should not have matched", declaration, declarationBO);
					AssertEquals("Assuming that the default is", "ITF", declarationBO.JE_ApplicationCode);
					AssertEquals("declarationBO.CustomsEntryHeaders.Count", 1, declarationBO.CustomsEntryHeaders.Count);
					entry = declarationBO.CustomsEntryHeaders[0];
					AssertEquals("entry.EntryNumber", "ENT3423", entry.EntryNumber);
					AssertEquals("entry.AllEntryLines.Count", 1, entry.AllEntryLines.Count);
					entryLine = entry.AllEntryLines[0];
					AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
					invoice = declarationBO.Invoices[0];
					AssertEquals("invoice.JZ_InvoiceNumber", "INV324", invoice.JZ_InvoiceNumber);
					AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
					invoiceLine = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_CL", entryLine.PK, invoiceLine.JI_CL);
					AssertEquals("invoiceLine.JI_Description", "BOB THE BUILDER", invoiceLine.JI_Description);
				});
			}
		}

		public void TestImportInvoiceLineContainerLinkFromDummyPackingLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>() {
					(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT001" }),
					(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT002" }),
					(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT003" })
				});
				declarationDataObject.CommercialInfo = new CommercialInfo();
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
				var invHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
				invHeader.InvoiceNumber = "INVHEADER";
				invHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
				{
					new CommercialInvoiceLine() { Link = 3, Description = "LineDesc1" },
					new CommercialInvoiceLine() { Link = 1, Description = "LineDesc2" },
					new CommercialInvoiceLine() { Link = 4, Description = "LineDesc3" },
				});
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invHeader);
				declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
				var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerNumber = "CONT001",
				};
				packingLine1.SetPackedItemCollection(() => new List<PackedItem>()
				{
					new PackedItem() { CommercialInvoiceLineLink = 3 },
					new PackedItem() { CommercialInvoiceLineLink = 1 }
				});
				var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerNumber = "CONT002",
				};
				packingLine2.SetPackedItemCollection(() => new List<PackedItem>()
				{
					new PackedItem() { CommercialInvoiceLineLink = 3 },
					new PackedItem() { CommercialInvoiceLineLink = 2 }
				});
				declarationDataObject.PackingLineCollection.Add(packingLine1);
				declarationDataObject.PackingLineCollection.Add(packingLine2);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals(3, declarationBO.CusContainers.Count);
				AssertEquals(3, declarationBO.InvoiceLines.Count);
				var linepk1 = declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "LineDesc1").PK;
				var linepk2 = declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "LineDesc2").PK;
				var linepk3 = declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "LineDesc3").PK;
				var cont1 = declarationBO.CusContainers.OfType<BaseCusContainer>().FirstOrDefault(x => x.CO_ContainerNumber == "CONT001");
				var cont2 = declarationBO.CusContainers.OfType<BaseCusContainer>().FirstOrDefault(x => x.CO_ContainerNumber == "CONT002");
				var cont3 = declarationBO.CusContainers.OfType<BaseCusContainer>().FirstOrDefault(x => x.CO_ContainerNumber == "CONT003");
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { linepk1, linepk2 }, cont1.InvoiceLinePivotCollection.OfType<CusContainerInvoiceLinePivot>().Select(x => x.C2_JI));
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { linepk1 }, cont2.InvoiceLinePivotCollection.OfType<CusContainerInvoiceLinePivot>().Select(x => x.C2_JI));
				AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), cont3.InvoiceLinePivotCollection.OfType<CusContainerInvoiceLinePivot>().Select(x => x.C2_JI));
			}
		}

		public void TestImportCusContainerFromPackingLineCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
				var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerNumber = "CONT001",
				};
				declarationDataObject.PackingLineCollection.Add(packingLine1);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var cont1 = declarationBO.CusContainers.OfType<BaseCusContainer>().FirstOrDefault(x => x.CO_ContainerNumber == "CONT001");
				AssertNotNull("Should have found a container with number CONT001", cont1);
				AssertEquals("CO_DataModel", string.Empty, cont1.CO_DataModel);
				Factory.SaveForTesting();
				AssertEquals("CO_DataModel", "ZA", cont1.CO_DataModel);
			}
		}

		public void TestImportSellerAddressIntoDeclaration()
		{
			var seller = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			seller.OH_IsConsignor = ZBool.True;

			var sellerAddress = seller.Addresses.AddNew(OrgAddressType.Office, false);
			sellerAddress.OA_Address1 = "AD2";
			sellerAddress.OA_City = "BB";
			sellerAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = Constants.AddressTypes.Seller,
				OrganizationCode = seller.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			AssertEquals(declarationBOToLoad.JE_OA_SellerAddress, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should have Seller Address set.", sellerAddress.PK, declarationBOToLoad.JE_OA_SellerAddress);

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have Seller Address set.", sellerAddress.PK, declarationBOToLoad.JE_OA_SellerAddress);
		}

		public void TestImportShipToPartyAddressIntoDeclaration()
		{
			var shipToParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			shipToParty.OH_IsConsignee = ZBool.True;

			var shipToPartyAddress = shipToParty.Addresses.AddNew(OrgAddressType.Office, false);
			shipToPartyAddress.OA_Address1 = "AD2";
			shipToPartyAddress.OA_City = "BB";
			shipToPartyAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ShipToParty),
				OrganizationCode = shipToParty.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			AssertEquals(declarationBOToLoad.JE_OA_ShipToPartyAddress, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should have ShipToParty Address set.", shipToPartyAddress.PK, declarationBOToLoad.JE_OA_ShipToPartyAddress);

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have ShipToParty Address set.", shipToPartyAddress.PK, declarationBOToLoad.JE_OA_ShipToPartyAddress);
		}

		public void TestImportCustomsOffice()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.CustomsOffice = new CodeDescriptionPair10Char() { Code = "CODE123", Description = "DESC123" };
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("CODE123", declarationBO.JE_CustomsOffice);
		}

		public void TestImportRealFieldsMappedToAddInfo()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>(new[]
			{
					new AddInfo() { Key = Constants.AddInfoKeys.Declaration.InlandModeOfTransport, Value = "ROA" },
					new AddInfo() { Key = Constants.AddInfoKeys.Declaration.UseOwnerRefAsQuarantineRef, Value = "Y" }
			}));
			declarationDataObject.TransportNationality = new Country() { Code = "NZ" };
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("declarationBO.JE_TransportModeInland", "ROA", declarationBO.JE_TransportModeInland);
				AssertEquals("declarationBO.JE_RN_NKTransportNationality", "NZ", declarationBO.JE_RN_NKTransportNationality);
				AssertEquals("declarationBO.SupportUseOwnerRefAsQuarantineRefUsage", true, declarationBO.SupportUseOwnerRefAsQuarantineRefUsage);
				AssertEquals("declarationBO.JE_UseOwnerRefAsQuarantineRef", ZBool.True, declarationBO.JE_UseOwnerRefAsQuarantineRef);
				declarationBO.Delete();
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("declarationBO.JE_TransportModeInland", "ROA", declarationBO.JE_TransportModeInland);
				AssertEquals("declarationBO.JE_RN_NKTransportNationality", "NZ", declarationBO.JE_RN_NKTransportNationality);
				AssertEquals("declarationBO.SupportUseOwnerRefAsQuarantineRefUsage", false, declarationBO.SupportUseOwnerRefAsQuarantineRefUsage);
				AssertEquals("declarationBO.JE_UseOwnerRefAsQuarantineRef", ZBool.False, declarationBO.JE_UseOwnerRefAsQuarantineRef);
			}
		}

		public void TestImportLocationOfGood()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO" };
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);

			declarationBO.JE_LocationOfGoods = ZString.Empty;
			declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO", Description = "" };
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);

			declarationBO.JE_LocationOfGoods = ZString.Empty;
			declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO", Description = "LOC" };
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);

			declarationBO.JE_LocationOfGoods = ZString.Empty;
			declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO", Description = "LOCGOO S" };
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("LOCGOO S", declarationBO.JE_LocationOfGoods);
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declarationBO.Delete();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				declarationDataObject.DataContext = dataContext;
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("", declarationBO.JE_LocationOfGoods);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				declarationBO.Delete();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				declarationDataObject.DataContext = dataContext;
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);
			}
		}

		public void TestImportCarrierCodeFromOrganizationAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var testOrg = Factory.NewWithValidTestData<OrgHeader>();
				testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CARR", Core.Constants.CountryCodes.SouthAfrica);
				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.AddOrgAddress(writeManager, testOrg.MainAddress, DocAddressType.Carrier);
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("CARR", declarationBO.JE_CarrierCode);
			}
		}

		public void TestImportCarrierCodeFromOrganizationAddress_NoExceptionWithoutCCC()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var testOrg = Factory.NewWithValidTestData<OrgHeader>();
				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.AddOrgAddress(writeManager, testOrg.MainAddress, DocAddressType.Carrier);

				AssertNoExceptionThrown("Should no exception", () =>
				{
					var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
					var declarationBO = reader.ReadIntoBusinessObject();
				});
			}
		}

		public void TestImportGoodsOrigin()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.GoodsOrigin = new CodeDescriptionPair() { Code = "ZZ", Description = "ZZ territory" };
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("ZZ", declarationBO.JE_GoodsOrigin);
		}

		public void TestImportGoodsDestination()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.GoodsDestination = Core.Constants.CountryCodes.Germany;
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("DE", declarationBO.JE_GoodsDestination);
		}

		public void TestImportDeclarantIntoDeclaration()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.AddOrgAddress(writeManager, testOrg, AddressTypes.Declarant);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(testOrg.MainAddress.PK, declarationBO.JE_OA_DeclarantAddress);
		}

		public void TestImportControllingAgent()
		{
			AssertFillValueOnOrganisation(DocAddressType.ControllingAgent, JobDeclarationSchema.JE_OH_ControllingAgent.Name);
		}

		public void TestImportControllingCustomer()
		{
			AssertFillValueOnOrganisation(DocAddressType.ControllingCustomer, JobDeclarationSchema.JE_OH_ControllingCustomer.Name);
		}

		public void TestImportExternalBroker()
		{
			AssertFillValueOnOrganisation(DocAddressType.ExternalBroker, JobDeclarationSchema.JE_OH_ExternalBroker.Name);
		}

		public void TestImportManufacturerAddressIntoDeclaration()
		{
			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;

			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Office, false);
			supplierAddress.OA_Address1 = "AD1";
			supplierAddress.OA_City = "AA";
			supplierAddress.OA_PostCode = "1111";

			var manufacturer = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			manufacturer.OH_IsConsignor = ZBool.True;

			var manufacturerAddress = manufacturer.Addresses.AddNew(OrgAddressType.Office, false);
			manufacturerAddress.OA_Address1 = "AD2";
			manufacturerAddress.OA_City = "BB";
			manufacturerAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.SupplierDocumentaryAddress),
				OrganizationCode = supplier.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OA_ManufacturerAddress, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should only have Manufacturer Address set when AddressType is Manufacturer.", ZGuid.Empty, declarationBOToLoad.JE_OA_ManufacturerAddress);

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				OrganizationCode = manufacturer.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have Manufacturer Address set.", manufacturerAddress.PK, declarationBOToLoad.JE_OA_ManufacturerAddress);
		}

		public void TestImportSoldToPartyAddressIntoDeclaration()
		{
			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignee = ZBool.True;

			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Office, false);
			supplierAddress.OA_Address1 = "AD1";
			supplierAddress.OA_City = "AA";
			supplierAddress.OA_PostCode = "1111";

			var soldtoparty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			soldtoparty.OH_IsConsignee = ZBool.True;

			var soldtopartyAddress = soldtoparty.Addresses.AddNew(OrgAddressType.Office, false);
			soldtopartyAddress.OA_Address1 = "AD2";
			soldtopartyAddress.OA_City = "BB";
			soldtopartyAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				OrganizationCode = supplier.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OA_SoldToPartyAddress, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should not have SoldToParty Address set when AddressType is nor SoldToParty.", ZGuid.Empty, declarationBOToLoad.JE_OA_SoldToPartyAddress);

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = Constants.AddressTypes.SoldToParty,
				OrganizationCode = soldtoparty.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have SoldToParty Address set.", soldtopartyAddress.PK, declarationBOToLoad.JE_OA_SoldToPartyAddress);
		}

		public void TestImportExporterIntoDeclaration()
		{
			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignee = ZBool.True;

			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Office, false);
			supplierAddress.OA_Address1 = "AD1";
			supplierAddress.OA_City = "AA";
			supplierAddress.OA_PostCode = "1111";

			var exporter = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			exporter.OH_IsConsignee = ZBool.True;

			var exporterAddress = exporter.Addresses.AddNew(OrgAddressType.Office, false);
			exporterAddress.OA_Address1 = "AD2";
			exporterAddress.OA_City = "BB";
			exporterAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				OrganizationCode = supplier.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OH_Exporter, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should not have Exporter set when AddressType is nor Exporter.", ZGuid.Empty, declarationBOToLoad.JE_OH_Exporter);

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Exporter),
				OrganizationCode = exporter.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have Exporter set.", exporter.PK, declarationBOToLoad.JE_OH_Exporter);
		}

		public void TestImportExporterIntoDeclarationForSpecificOrganisationType()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory.BOFactory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			OrgHeader unmatchedOrg = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			var exporterAddress = unmatchedOrg.Addresses.AddNew(OrgAddressType.Office, false);
			exporterAddress.OA_Address1 = "AD2";
			exporterAddress.OA_City = "BB";
			exporterAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Exporter),
				OrganizationCode = unmatchedOrg.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});
			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			ZQuery loadQuery = new ZQuery(StmNoteSchema.ST_ParentID, declarationBO2.PK);
			loadQuery.AddToFilter(StmNoteSchema.ST_Table, "JobDeclaration");
			var notes = Factory.Load<StmNote>(loadQuery);
			AssertEquals(1, notes.Length);
			AssertContains("Consignor", notes[0].ST_NoteText);
		}

		public void TestImportConsigneeAddressIntoDeclaration()
		{
			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignee = ZBool.True;

			var supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Office, false);
			supplierAddress.OA_Address1 = "AD1";
			supplierAddress.OA_City = "AA";
			supplierAddress.OA_PostCode = "1111";

			var consigneeAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consigneeAddress.OH_IsConsignee = ZBool.True;

			var consigneeAddressAddress = consigneeAddress.Addresses.AddNew(OrgAddressType.Office, false);
			consigneeAddressAddress.OA_Address1 = "AD2";
			consigneeAddressAddress.OA_City = "BB";
			consigneeAddressAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				OrganizationCode = supplier.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OA_ConsigneeAddress, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should not have ConsigneeAddress set when AddressType is nor ConsigneeAddress.", ZGuid.Empty, declarationBOToLoad.JE_OA_ConsigneeAddress);

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = Constants.AddressTypes.UltimateConsignee,
				OrganizationCode = consigneeAddress.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have ConsigneeAddress set.", consigneeAddressAddress.PK, declarationBOToLoad.JE_OA_ConsigneeAddress);
		}

		public void TestJobMaterialChanges_ShouldResetAllCommoditiyRiskStatus_ForStandAloneDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var header = declaration.Invoices.AddNew();
			var invoiceLine1 = header.InvoiceLines.AddNew();
			invoiceLine1.JI_ClusterKey = 10001;
			invoiceLine1.JI_Tariff = "090121";
			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_JZ = header.PK;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRisk.Business.ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = declaration.PK;
			complianceRiskStatus.COR_ParentTableCode = declaration.TablePrefix;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "090121";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "123456";
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			Factory.SaveForTesting();

			var eventLog = Factory.NewWithValidTestData<ComplianceRisk.Business.StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			eventLog.SCE_ParentID = declaration.PK;

			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			using (ZArchitecture.Environment.RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationDataObject = SetupDeclaration(null, "MYMASTER",
					new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();

				AssertNotNull("Compliance risk status should be created.", complianceRiskStatus);
				AssertEquals(declaration.PK, complianceRiskStatus.COR_ParentID);
				AssertEquals(declaration.TablePrefix, complianceRiskStatus.COR_ParentTableCode);
				AssertEquals("Commodities risk status NCH", 2, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceRisk.Business.ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
			}
		}

		public void TestComplianceUniversalDataObjectReader_ShouldNotCreateComplianceRiskStatus_ForNonStandAloneDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declaration.JE_GS_NKCusAgent = ZString.Empty;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var header = declaration.Invoices.AddNew();
			var invoiceLine1 = header.InvoiceLines.AddNew();
			invoiceLine1.JI_ClusterKey = 10001;
			invoiceLine1.JI_Tariff = "090121";
			invoiceLine1.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_JZ = header.PK;

			Factory.SaveForTesting();

			AssertEquals(false, declaration.IsStandAlone);

			var eventLog = Factory.NewWithValidTestData<ComplianceRisk.Business.StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			eventLog.SCE_ParentID = declaration.PK;

			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityInvoiceLineMocksForTest(true))
			using (ZArchitecture.Environment.RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationDataObject = SetupDeclaration(null, "MYMASTER",
					new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();

				var complianceRiskStatus = Factory.LoadTop1<ComplianceRisk.Business.ComplianceRiskStatus>(new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, declarationBO.PK));
				AssertNull("Compliance risk status should not be created.", complianceRiskStatus);
			}
		}

		public void TestImporConsigneeIntoDeclaration()
		{
			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignee = ZBool.True;

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Address1 = "AD1";
			supplierAddress.OA_City = "AA";
			supplierAddress.OA_PostCode = "1111";

			var consignee = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var consigneeAddress = consignee.MainAddress;
			consigneeAddress.OA_Address1 = "AD2";
			consigneeAddress.OA_City = "BB";
			consigneeAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				OrganizationCode = supplier.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OH_Consignee, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should not have consignee.", ZGuid.Empty, declarationBOToLoad.JE_OH_Consignee);

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = Constants.AddressTypes.IntermediateConsignee,
				OrganizationCode = consignee.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have consignee set.", consigneeAddress.OA_OH, declarationBOToLoad.JE_OH_Consignee);
		}

		public void TestImportBuyingAgentIntoDeclaration()
		{
			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignee = ZBool.True;

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Address1 = "AD1";
			supplierAddress.OA_City = "AA";
			supplierAddress.OA_PostCode = "1111";

			var buyingAgent = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			buyingAgent.OH_IsConsignee = ZBool.True;

			var buyingAgentAddress = buyingAgent.MainAddress;
			buyingAgentAddress.OA_Address1 = "AD2";
			buyingAgentAddress.OA_City = "BB";
			buyingAgentAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				OrganizationCode = supplier.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OH_BuyingAgent, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should not have buyingAgent.", ZGuid.Empty, declarationBOToLoad.JE_OH_BuyingAgent);

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = Constants.AddressTypes.BuyingAgent,
				OrganizationCode = buyingAgent.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have buyingAgent set.", buyingAgentAddress.OA_OH, declarationBOToLoad.JE_OH_BuyingAgent);
		}

		public void TestImportSellingAgentIntoDeclaration()
		{
			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Address1 = "AD1";
			supplierAddress.OA_City = "AA";
			supplierAddress.OA_PostCode = "1111";

			var sellingAgent = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			sellingAgent.OH_IsConsignee = ZBool.True;

			var sellingAgentAddress = sellingAgent.MainAddress;
			sellingAgentAddress.OA_Address1 = "AD2";
			sellingAgentAddress.OA_City = "BB";
			sellingAgentAddress.OA_PostCode = "2222";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				OrganizationCode = supplier.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OH_SellingAgent, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should not have sellingAgent.", ZGuid.Empty, declarationBOToLoad.JE_OH_SellingAgent);

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = Constants.AddressTypes.SellingAgent,
				OrganizationCode = sellingAgent.OH_Code,
				AddressShortCode = "AD2",
				Address1 = "AD2",
				City = "BB",
				Postcode = "2222"
			});

			var reader2 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			AssertEquals("Declaration should have sellingAgent.", sellingAgentAddress.OA_OH, declarationBOToLoad.JE_OH_SellingAgent);
		}

		public void TestImportServiceRateIntoDeclaration()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			var services = new[]
			{
				new AdditionalService
				{
					ServiceCode = new CodeDescriptionPair { Code = "SC1" },
					ServiceCount = 1,
					ServiceRate = 123.45,
					ServiceRateCurrency = "GBP",
					MeasurementBasis = JobServiceInfo.Constants.Codes.Hour
				},
				new AdditionalService
				{
					ServiceCode = new CodeDescriptionPair { Code = "SC2" },
					ServiceCount = 2,
					ServiceRate = 234.56,
					ServiceRateCurrency = "USD",
					MeasurementBasis = JobServiceInfo.Constants.Codes.FlatRate
				}
			};
			declarationDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			declarationDataObject.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>(services));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var result = reader.ReadIntoBusinessObject();

			AssertEquals(services.Length, result.Services.Count);
			for (var i = 0; i < services.Length; i++)
			{
				AssertEquals(services[i].ServiceCode.Code, result.Services[i].ES_ServiceCode);
				AssertEquals(services[i].ServiceCount, result.Services[i].ES_ServiceCount);
				AssertEquals(services[i].ServiceRate, result.Services[i].ES_ServiceRate);
				AssertEquals(services[i].ServiceRateCurrency, result.Services[i].ES_RX_NKServiceRateCurrency);
				AssertEquals(services[i].MeasurementBasis, result.Services[i].ES_MeasurementBasis);
			}
		}

		public void TestImportRepresentativeIntoDeclaration()
		{
			var representative = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

			var representativeAddress = representative.MainAddress;
			representativeAddress.OA_Address1 = "AD1";
			representativeAddress.OA_City = "AA";
			representativeAddress.OA_PostCode = "1111";

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.Representative),
				OrganizationCode = representative.OH_Code,
				AddressShortCode = "AD1",
				Address1 = "AD1",
				City = "AA",
				Postcode = "1111"
			});

			AssertEquals(declarationBOToLoad.JE_OA_Representative, ZGuid.Empty);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Declaration should have Representative", representativeAddress.PK, declarationBOToLoad.JE_OA_Representative);
		}

		public void TestImportUniqueConsignmentReferenceIntoDeclaration()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.UniqueConsignmentReference = "2GB836911119000-LLL25646";

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var result = reader.ReadIntoBusinessObject();
			AssertEquals(declarationDataObject.UniqueConsignmentReference, result.JE_UCR);
		}

		public void TestCustomsWarehouseAddress()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var org = Factory.New<OrgHeader>();

				org.OH_Code = "ANDLOG";
				org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
				org.OH_RL_NKClosestPort = "AUSYD";

				var address = Factory.NewWithValidTestData<OrgAddress>();

				address.OA_Address1 = "AD2";
				address.OA_City = "SH";
				address.OA_PostCode = "2036";
				address.OA_OH = org.PK;
				address.OA_RL_NKRelatedPortCode = "AUSYD";

				eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-10).ToDateTime());
				var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
				declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
				declarationBOToLoad.JE_MasterBill = "MYMASTER";
				declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
				declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now;
				var entry = declarationBOToLoad.CustomsEntryHeaders.AddNew();
				declarationBOToLoad.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardUpdatedPending;
				declarationBOToLoad.JE_GoodsDescription = "ABC";
				var warehouseAddress = declarationBOToLoad.DocAddresses.AddNew(DocAddressType.CustomsWarehouseAddress);
				warehouseAddress.E2_City = "NJ";
				warehouseAddress.E2_Address1 = "AD1";
				Factory.SaveForTesting();

				Assert("Job has WHS Transaction", declarationBOToLoad.HasWHSTransaction);

				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
				declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.CustomsWarehouseAddress),
					AddressShortCode = "KNZADDCODE",
					OrganizationCode = "KNZORGCODE",
					CompanyName = "KNZ THE BUILDER",
					Address1 = "AD2",
					City = "SH"
				});
				declarationDataObject.GoodsDescription = "TCWA";

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("The CustomsWarehouseAddress should not update when the job has WHS Transaction", declarationBO.JE_GoodsDescription, "ABC");
				AssertContains("There are active warehouse transactions.", logger.Logs);
			}
		}

		public void TestCommercialInvoicesInvoiceLineCount()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject.CommercialInfo = new CommercialInfo();
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
				var invHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
				invHeader.InvoiceNumber = "INVHEADER";
				invHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
				{
					new CommercialInvoiceLine() { Link = 1, Description = "LineDesc1" },
					new CommercialInvoiceLine() { Link = 2, Description = "LineDesc2" },
					new CommercialInvoiceLine() { Link = 3, Description = "LineDesc3" },
				});

				var invoiceDataObject2 = SetupCommercialInvoiceHeaderData2(commercialInvoiceLineCollection: new DataObjectList<CommercialInvoiceLine>());
				var subGroupDataObject = new CommercialInfo()
				{
					Name = "GROUP1",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject2 }) { Content = CollectionContent.Partial },
				};

				declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invHeader);
				var mock = new Mock<JobDeclarationDataObjectReader>(new object[] { declarationDataObject, logger, Factory, Factory.New<ForwardingShipment>() });
				mock.CallBase = true;
				mock
					.Protected()
					.Setup<ZInt>("MaxInvoiceLineCount")
					.Returns(2);

				logger.ClearLogs();

				AssertExceptionThrown<DataObjectReadFailureException>(@"Cannot populate business object because:
Exceeded maximum number of invoice lines of 2", () =>
				{
					mock.Object.ReadIntoBusinessObject();
				});
				mock.VerifyAll();
			}
		}

		public void TestCommercialInvoiceCollectionWithImportKeyMatchedInvoiceLine()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.Netherlands))
			{
				var entryHeaderDataObject = SetupEntryHeader("IMP", "MS2", "ES3", "MATCH", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), null);
				var entryLineDataObject1 = SetupEntryLine(1, "1020304051", 2585.87m, 0.84m, 800.96m, new CodeDescriptionPair() { Code = "NO", Description = "NO DESC" }, "DESC 1", new CodeDescriptionPair() { Code = "AAA", Description = "TEST 1" });
				var entryLineDataObject2 = SetupEntryLine(2, "1020304052", 2585.87m, 0.84m, 800.96m, new CodeDescriptionPair() { Code = "NO", Description = "NO DESC" }, "DESC 2", new CodeDescriptionPair() { Code = "AAA", Description = "TEST 2" });
				entryHeaderDataObject.EntryLineCollection = new List<EntryLine> { entryLineDataObject1, entryLineDataObject2 };

				var entryNumber = SetupEntryNumber(new EntryType() { Code = "IMP", Description = "W89 DESC" }, "W986548", ZBool.False, new EntryStatus() { Code = "02", Description = "NotClear" }, new ZDateTime(2017, 7, 7));
				entryHeaderDataObject.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { entryNumber });

				var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" }, addEntryHeaders: new List<EntryHeader>() { entryHeaderDataObject });
				declarationDataObject.CommercialInfo = new CommercialInfo();
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();

				var invoiceDataObject = SetupCommercialInvoiceHeaderData2(commercialInvoiceLineCollection: new DataObjectList<CommercialInvoiceLine>());
				invoiceDataObject.InvoiceNumber = "INVHEADER";
				invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
					{
						new CommercialInvoiceLine() { Link = 1, Description = "LineDesc1", DataImportMatchingKey = "TestMatchingKey01", EntryLineNumber = 1 },
						new CommercialInvoiceLine() { Link = 2, Description = "LineDesc2", DataImportMatchingKey = "TestMatchingKey02", EntryLineNumber = 2 },
					});

				declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invoiceDataObject);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declaration = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals(2, declaration.InvoiceLines.Count);
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				AssertEquals(2, declaration.CustomsEntryHeaders[0].AllEntryLines.Count);
				var invoiceLine1 = declaration.InvoiceLines[0];
				invoiceLine1.JI_CL = declaration.CustomsEntryHeaders[0].AllEntryLines[0].PK;
				var invoiceLine2 = declaration.InvoiceLines[1];
				invoiceLine2.JI_CL = declaration.CustomsEntryHeaders[0].AllEntryLines[1].PK;
				invoiceLine2.JI_BrandName = "TestBrandName";
				Factory.SaveForTesting();

				declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Clear();
				var invoiceDataObject2 = SetupCommercialInvoiceHeaderData2(commercialInvoiceLineCollection: new DataObjectList<CommercialInvoiceLine>());
				invoiceDataObject2.InvoiceNumber = "INVHEADER";
				invoiceDataObject2.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
				{
					new CommercialInvoiceLine() { Link = 1, Description = "LineDesc1-1", DataImportMatchingKey = "TestMatchingKey01", EntryLineNumber = 1 },
					new CommercialInvoiceLine() { Link = 2, Description = "LineDesc3", DataImportMatchingKey = "TestMatchingKey03", EntryLineNumber = 2 },
				});
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invoiceDataObject2);

				_ = reader.ReadIntoBusinessObject();

				Factory.SaveForTesting();

				var factory = new BusinessObjectFactory();
				var invoiceLinesReloaded = factory.Load<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_ClusterKey, declaration.JE_ClusterKey));
				AssertEquals("should be 2 invoice lines after the second import", 2, invoiceLinesReloaded.Length);
				AssertEquals("should find invoiceLine1's JI_PK", true, invoiceLinesReloaded.Select(i => i.PK).Any(i => i == invoiceLine1.PK));
				AssertEquals("should find invoiceLine1's JI_Description", "LineDesc1-1", invoiceLinesReloaded.First(i => i.PK == invoiceLine1.PK).JI_Description);
				AssertEquals("should not find invoiceLine2's JI_PK", false, invoiceLinesReloaded.Select(i => i.PK).Any(i => i == invoiceLine2.PK));
				AssertEquals("new invoice line's description", "LineDesc3", invoiceLinesReloaded.First(i => i.PK != invoiceLine1.PK).JI_Description);
			}
		}

		public void TestCommercialInvoiceCollectionWithDifferentContent()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.CommercialInfo = new CommercialInfo();
			declarationDataObject.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invHeader.InvoiceNumber = "INVHEADER";
			invHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
				{
					new CommercialInvoiceLine() { Link = 1, Description = "LineDesc1" },
				});

			var invoiceDataObject2 = SetupCommercialInvoiceHeaderData2(commercialInvoiceLineCollection: new DataObjectList<CommercialInvoiceLine>());
			var subGroupDataObject = new CommercialInfo()
			{
				Name = "GROUP1",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject2 }) { Content = CollectionContent.Partial },
			};

			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invHeader);
			declarationDataObject.CommercialInfo.SubGroupCollection = new List<CommercialInfo>(new[] { subGroupDataObject });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertContains("All Commercial Invoice Collection's Content attribute must be identical.", logger.Logs);

			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Content = CollectionContent.Complete;
			logger.ClearLogs();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertContains("All Commercial Invoice Collection's Content attribute must be identical.", logger.Logs);

			subGroupDataObject.CommercialInvoiceCollection.Content = null;
			logger.ClearLogs();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertContains("All Commercial Invoice Collection's Content attribute must be identical.", logger.Logs);
		}

		public void TestCommercialInvoiceWithSameInvoiceNumberAndSupplier()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);

			var supplier = CreateOrganisation("CATE", "ABC!@#11");
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			var supplierDataObj = declarationDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);

			declarationDataObject.CommercialInfo = new CommercialInfo();
			declarationDataObject.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();

			var invHeader1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invHeader1.InvoiceNumber = "INVHEADER";
			invHeader1.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
				{
					new CommercialInvoiceLine() { Link = 1, Description = "LineDesc1" },
				});
			invHeader1.Supplier = supplierDataObj;
			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invHeader1);

			var invHeader2 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invHeader2.InvoiceNumber = "INVHEADER";
			invHeader2.Supplier = supplierDataObj;
			var subGroupDataObject = new CommercialInfo()
			{
				Name = "GROUP1",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invHeader2 })
			};
			declarationDataObject.CommercialInfo.SubGroupCollection = new List<CommercialInfo>(new[] { subGroupDataObject });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertContains("There is a duplicate Commercial Invoice having same Invoice Number 'INVHEADER'", logger.Logs);
		}

		public void TestCommercialInvoiceWithSameInvoiceNumberAndSupplierIsNull()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);

			var supplier = CreateOrganisation("CATE", "ABC!@#11");
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			var supplierDataObj = declarationDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);

			declarationDataObject.CommercialInfo = new CommercialInfo();
			declarationDataObject.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();

			var invHeader1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invHeader1.InvoiceNumber = "INVHEADER";
			invHeader1.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
				{
					new CommercialInvoiceLine() { Link = 1, Description = "LineDesc1" },
				});
			declarationDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invHeader1);

			var invHeader2 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invHeader2.InvoiceNumber = "INVHEADER";
			invHeader2.Supplier = supplierDataObj;
			var subGroupDataObject = new CommercialInfo()
			{
				Name = "GROUP1",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invHeader2 })
			};
			declarationDataObject.CommercialInfo.SubGroupCollection = new List<CommercialInfo>(new[] { subGroupDataObject });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertContains("There is a duplicate Commercial Invoice having same Invoice Number 'INVHEADER'", logger.Logs);
		}

		public void TestMatchingUsingOwnerRefShouldInludesCurrentCompany()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC!@#456";
			org1.OH_FullName = "BOB THE BUILDER";
			org1.MainAddress.OA_Address1 = "BOB'S ADDRESS";

			var cnCompany = Factory.New<GlbCompany>();
			cnCompany.GC_Code = "CN$";
			cnCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cnCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var cnBranch = cnCompany.Branches.AddNew();
			cnBranch.GB_Code = "CN#";
			cnBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_OwnerRef = "BOB123456";
			declaration1.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration1.JE_SystemCreateTimeUtc = ZDateTime.UtcNow;
			declaration1.JE_OH_Importer = org1.PK;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_OwnerRef = "BOB123456";
			declaration2.JE_GB = cnBranch.PK;
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			declaration2.JE_OH_Importer = org1.PK;

			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_OwnerRef = "BOB123456";
			declaration3.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration3.JE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			declaration3.JE_OH_Importer = org1.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(cnCompany);
			dataContext.CodesMappedToTarget = false;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "");
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "BOB123456",
				GoodsDescription = "HELLO WORLD",
			};
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var writeManager = new DataWritingManager(new ActionInfo(null, declaration2));
			declarationDataObject.AddOrgAddress(writeManager, org1, DocAddressType.ImporterDocumentaryAddress);
			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			Factory.SaveForTesting();
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			declaration1.Reload();
			AssertEquals("declaration1.JE_GoodsDescription", ZString.Empty, declaration1.JE_GoodsDescription);
			declaration2.Reload();
			AssertEquals("declaration2.JE_GoodsDescription", "HELLO WORLD", declaration2.JE_GoodsDescription);
			declaration3.Reload();
			AssertEquals("declaration3.JE_GoodsDescription", ZString.Empty, declaration3.JE_GoodsDescription);
		}

		public void TestMatchingUsingOwnerRefShouldNotLoadJobsOfOtherCompaniesFromDB()
		{
			eAdaptorRegistry.Instance.UniversalXMLUseCombinedReferenceAndPartyIDMatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC!@#456";
			org1.OH_FullName = "BOB THE BUILDER";
			org1.MainAddress.OA_Address1 = "BOB'S ADDRESS";

			var cnCompany = Factory.New<GlbCompany>();
			cnCompany.GC_Code = "CN$";
			cnCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cnCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var cnBranch = cnCompany.Branches.AddNew();
			cnBranch.GB_Code = "CN#";
			cnBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var deCompany = Factory.New<GlbCompany>();
			deCompany.GC_Code = "DE$";
			deCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			deCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var deBranch = deCompany.Branches.AddNew();
			deBranch.GB_Code = "DE#";
			deBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_OwnerRef = "BOB123456";
			declaration2.JE_GB = cnBranch.PK;
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			declaration2.JE_OH_Importer = org1.PK;

			var declaration3 = Factory.New<BaseJobDeclaration>();
			declaration3.JE_OwnerRef = "BOB123456";
			declaration3.JE_GB = deBranch.PK;
			declaration3.JE_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			declaration3.JE_OH_Importer = org1.PK;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(deCompany);
			dataContext.CodesMappedToTarget = false;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "");
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "BOB123456"
			};
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var writeManager = new DataWritingManager(new ActionInfo(null, declaration3));
			declarationDataObject.AddOrgAddress(writeManager, org1, DocAddressType.ImporterDocumentaryAddress);
			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			AssertNoExceptionThrown("Enterprise.ZArchitecture.Environment.DeveloperNotificationException ---> Enterprise.ZArchitecture.Environment.DeveloperNotificationException: CreateBusinessObjectsFromRows attempted to return a business object of incompatible type." +
									"\r\nExpected type: Enterprise.Customs.EU.Business.Declaration.JobDeclaration;" +
									"\r\nActual object type: Enterprise.Customs.CN.Business.JobDeclaration;", () => manager.Process(message));
		}

		public void TestCartageEquipmentDetailsFromXMLIsUsed()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			declarationDataObject.DataContext = dataContext;
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			declarationDataObject.TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea };
			declarationDataObject.LocalProcessing = new LocalProcessing()
			{
				FCLDeliveryEquipmentNeeded = new CodeDescriptionPair() { Code = "DJC" },
				FCLPickupEquipmentNeeded = new CodeDescriptionPair() { Code = "DAN" }
			};
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[]
			{
				new Container(DefaultDataObjectWriterStrategy.TestInstance)
				{
					FCL_LCL_AIR = new ContainerMode() { Code = Core.Constants.TransportModes.Sea },
					ContainerNumber = "TURE3223"
				}
			}));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("declarationBO.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded", "DJC", declarationBO.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("declarationBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded", "DAN", declarationBO.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
		}

		public void TestTransportLegAreMatchedBasedOnLoadAndDischargePorts()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_VoyageFlightNo = "QF45";
			declaration.JE_RL_NKOrigin = "USCHI";
			declaration.JE_RL_NKPortOfLoading = "USCHI";
			declaration.JE_RL_NKPortOfArrival = "AUBNE";
			declaration.JE_RL_NKFinalDestination = "AUBNE";
			declaration.JE_DateAtOrigin = new ZDateTime(2013, 4, 24);
			declaration.JE_ExportDate = new ZDateTime(2013, 4, 24);
			declaration.JE_DateOfArrival = new ZDateTime(2013, 4, 26);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2013, 4, 26);
			var transport1 = declaration.Transports[0];
			var transport2 = declaration.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			transport2.JW_VoyageFlight = "QF45";
			transport2.JW_RL_NKLoadPort = "FJSUV";
			transport2.JW_RL_NKDiscPort = "AUBNE";
			transport2.JW_ETD = new ZDateTime(2013, 4, 25);
			transport2.JW_ETA = new ZDateTime(2013, 4, 26);
			transport1.JW_VoyageFlight = "QF1";
			transport1.JW_RL_NKLoadPort = "USCHI";
			transport1.JW_RL_NKDiscPort = "FJSUV";
			transport1.JW_ETD = new ZDateTime(2013, 4, 24);
			transport1.JW_ETA = new ZDateTime(2013, 4, 25);
			var dataObject = (UniversalShipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("VoyageFlightNo", "QF45", dataObject.VoyageFlightNo);
			AssertEquals("dataObject.PortOfLoading.Code", "USCHI", dataObject.PortOfLoading.Code);
			AssertEquals("dataObject.PortOfDischarge.Code", "AUBNE", dataObject.PortOfDischarge.Code);
			AssertEquals("dataObject.TransportLegCollection.Count", 2, dataObject.TransportLegCollection.Count);
			var transport1DataObject = dataObject.TransportLegCollection[0];
			AssertEquals("transport1DataObject.PortOfLoading.Code", "USCHI", transport1DataObject.PortOfLoading.Code);
			AssertEquals("transport1DataObject.PortOfDischarge.Code", "FJSUV", transport1DataObject.PortOfDischarge.Code);
			AssertEquals("transport1DataObject.EstimatedDeparture", new ZDateTime(2013, 4, 24), transport1DataObject.EstimatedDeparture);
			AssertEquals("transport1DataObject.EstimatedArrival", new ZDateTime(2013, 4, 25), transport1DataObject.EstimatedArrival);
			var transport2DataObject = dataObject.TransportLegCollection[1];
			AssertEquals("transport2DataObject.PortOfLoading.Code", "FJSUV", transport2DataObject.PortOfLoading.Code);
			AssertEquals("transport2DataObject.PortOfDischarge.Code", "AUBNE", transport2DataObject.PortOfDischarge.Code);
			AssertEquals("transport2DataObject.EstimatedDeparture", new ZDateTime(2013, 4, 25), transport2DataObject.EstimatedDeparture);
			AssertEquals("transport2DataObject.EstimatedArrival", new ZDateTime(2013, 4, 26), transport2DataObject.EstimatedArrival);
			var reader = new JobDeclarationDataObjectReader(dataObject, logger, Factory);
			var declaration2 = reader.ReadIntoBusinessObject();
			AssertNotEquals(declaration, declaration2);
			AssertEquals(2, declaration2.Transports.Count);
			var transport1Dec2 = declaration2.Transports[0];
			AssertEquals("QF1", transport1Dec2.JW_VoyageFlight);
			AssertEquals("USCHI", transport1Dec2.JW_RL_NKLoadPort);
			AssertEquals("FJSUV", transport1Dec2.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2013, 4, 24), transport1Dec2.JW_ETD);
			AssertEquals(new ZDateTime(2013, 4, 25), transport1Dec2.JW_ETA);
			var transport2Dec2 = declaration2.Transports[1];
			AssertEquals("QF45", transport2Dec2.JW_VoyageFlight);
			AssertEquals("FJSUV", transport2Dec2.JW_RL_NKLoadPort);
			AssertEquals("AUBNE", transport2Dec2.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2013, 4, 25), transport2Dec2.JW_ETD);
			AssertEquals(new ZDateTime(2013, 4, 26), transport2Dec2.JW_ETA);
			dataObject.SetTransportLegCollection(() => null);
			reader = new JobDeclarationDataObjectReader(dataObject, logger, Factory);
			var declaration3 = reader.ReadIntoBusinessObject();
			AssertNotEquals(declaration, declaration2);
			AssertEquals(1, declaration3.Transports.Count);
			var transportDec3 = declaration3.Transports[0];
			AssertEquals("QF45", transportDec3.JW_VoyageFlight);
			AssertEquals("USCHI", transportDec3.JW_RL_NKLoadPort);
			AssertEquals("AUBNE", transportDec3.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2013, 4, 24), transportDec3.JW_ETD);
			AssertEquals(new ZDateTime(2013, 4, 26), transportDec3.JW_ETA);
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			reader = new JobDeclarationDataObjectReader(dataObject, logger, Factory);
			var declaration4 = reader.ReadIntoBusinessObject();
			AssertNotEquals(declaration4, declaration3);
			AssertEquals(0, declaration4.Transports.Count);
		}

		public void TestReceivingForwarderIsImportedOnImportDecs()
		{
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothForwardersXML, "IMP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
			AssertEquals("declaration.ForwarderName", "FAMOUS PACIFIC SHIPPING AUSTRALIA P/L", declaration.ForwarderName);
		}

		public void TestFreightOverrideIsFalseIfXmlIsSameAsSynchronisation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60267", "60267 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2704", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.SaveForTesting();

			AssertFreightOverrideSynchronisationData(ZBool.False, @"
        <CustomsContainerMode>
          <Code>CNT</Code>
        </CustomsContainerMode>
");
		}

		public void TestFreightOverrideIsTrueIfXmlIsNotSameAsSynchronisation()
		{
			AssertFreightOverrideSynchronisationData(ZBool.True, @"
        <CustomsContainerMode>
          <Code>NCT</Code>
        </CustomsContainerMode>
");
		}

		public void TestSendingForwarderIsImportedOnExportDecs()
		{
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothForwardersXML, "EXP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "EXP", declaration.JE_MessageType);
			AssertEquals("declaration.ForwarderName", "ACA INTERNATIONAL PTY LTD", declaration.ForwarderName);
		}

		public void TestDeclarationWithPackingImportedIntoDecNotSupportPacking()
		{
			foreach (var countryCode in new[] { Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.Singapore })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var message = GetQueuedUniversalShipmentMessage(DeclarationWithPackingImportedIntoDecNotSupportPacking, factoryToUse: new BusinessObjectFactory());
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var manager = new UniversalMessageProcessingManager(serviceTaskLog);
					AssertNoExceptionThrown(() => { manager.Process(message); });
				}
			}
		}

		public void TestArrivalCTOIsImportedOnImportDecs()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "ANDLOG";
			org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 ANDREW ROAD";
			address.OA_City = "ANDREW";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTOXML, "IMP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address", declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", address.OA_Address1);
		}

		public void TestDepartureCTOIsImportedOnExportDecs()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "LACLOG";
			org.OH_FullName = "LACHLAN LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 LACHLAN ROAD";
			address.OA_City = "LACHLAN";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTOXML, "EXP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "EXP", declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address", declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 LACHLAN ROAD", address.OA_Address1);
		}

		public void TestOverrideArrivalCTOIsImportedOnImportDecs()
		{
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithOverrideWithCTO, "IMP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", declaration.ContainerTerminalOperatorDocAddress.E2_Address1);
		}

		public void TestArrivalCFSIsImportedOnImportDecs()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "ANDLOG";
			org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 ANDREW ROAD";
			address.OA_City = "ANDREW";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCFSXML, "IMP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
			AssertNotNull("declaration.DepotDocAddress.E2_OA_Address", declaration.DepotDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.DepotDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", address.OA_Address1);
		}

		public void TestDepartureCFSIsImportedOnExportDecs()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "LACLOG";
			org.OH_FullName = "LACHLAN LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 LACHLAN ROAD";
			address.OA_City = "LACHLAN";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCFSXML, "EXP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "EXP", declaration.JE_MessageType);
			AssertNotNull("declaration.DepotDocAddress.E2_OA_Address", declaration.DepotDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.DepotDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 LACHLAN ROAD", address.OA_Address1);
		}

		public void TestOverrideArrivalCFSIsImportedOnImportDecs()
		{
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithOverrideWithCFS, "IMP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", declaration.DepotDocAddress.E2_Address1);
		}

		public void TestContainerYardEmptyReturnIsImportedOnImportDecs()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "ANDLOG";
			org.OH_FullName = "ANDREWS LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 ANDREW ROAD";
			address.OA_City = "ANDREW";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTYXML, "IMP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerYardDocAddress.E2_OA_Address", declaration.ContainerYardDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerYardDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", address.OA_Address1);
		}

		public void TestContainerYardEmptyPickupIsImportedOnExportDecs()
		{
			var org = Factory.New<OrgHeader>();

			org.OH_Code = "LACLOG";
			org.OH_FullName = "LACHLAN LOGISTICS SOLUTIONS";
			org.OH_RL_NKClosestPort = "AUSYD";

			var address = Factory.NewWithValidTestData<OrgAddress>();

			address.OA_Address1 = "1 LACHLAN ROAD";
			address.OA_City = "LACHLAN";
			address.OA_PostCode = "2036";
			address.OA_OH = org.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithBothCTYXML, "EXP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
			}

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "EXP", declaration.JE_MessageType);
			AssertNotNull("declaration.ContainerYardDocAddress.E2_OA_Address", declaration.ContainerYardDocAddress.E2_OA_Address);
			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, declaration.ContainerYardDocAddress.E2_OA_Address));
			AssertEquals("Address1 Inserted", "1 LACHLAN ROAD", address.OA_Address1);
		}

		public void TestOverrideContainerYardEmptyReturnIsImportedOnImportDecs()
		{
			var message = GetQueuedUniversalShipmentMessage(string.Format(DeclarationWithOverrideWithCTY, "IMP"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
			AssertEquals("Address1 Inserted", "1 ANDREW ROAD", declaration.ContainerYardDocAddress.E2_Address1);
		}

		public void TestRecipientRoleSetsEntryTypeForImportDecs()
		{
			var message = GetQueuedUniversalShipmentMessage(MinimalistImportDeclarationXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);
		}

		public void TestRecipientRoleSetsEntryTypeForExportDecs()
		{
			var message = GetQueuedUniversalShipmentMessage(MinimalistExportDeclarationXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should be able to load a Declaration with a Decl Ref of B00001000.", declaration);

			AssertEquals("declaration.JE_MessageType", "EXP", declaration.JE_MessageType);
		}

		public void TestSupplierAndImporterMatchingPreferredOrder()
		{
			var consigneeDelivery = CreateOrganisation("BOB", "ABC!@#1");
			var consigneeDoc = CreateOrganisation("JACK", "ABC!@#2");
			var consigneeAddress = CreateOrganisation("JANE", "ABC!@#3");
			var importerDelivery = CreateOrganisation("PETE", "ABC!@#4");
			var importerDoc = CreateOrganisation("MARY", "ABC!@#5");
			var importer = CreateOrganisation("MAT", "ABC!@#6");
			var consignorPickup = CreateOrganisation("JOE", "ABC!@#7");
			var consignorDoc = CreateOrganisation("MARK", "ABC!@#8");
			var supplierPickup = CreateOrganisation("SUE", "ABC!@#9");
			var supplierDoc = CreateOrganisation("KATE", "ABC!@#10");
			var supplier = CreateOrganisation("CATE", "ABC!@#11");

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext
			};
			var consigneeDeliveryDataObj = declarationDataObject.AddOrgAddress(writeManager, consigneeDelivery, DocAddressType.ConsigneePickupDeliveryAddress);
			var consigneeDocDataObj = declarationDataObject.AddOrgAddress(writeManager, consigneeDoc, DocAddressType.ConsigneeDocumentaryAddress);
			var consigneeAddressDataObj = declarationDataObject.AddOrgAddress(writeManager, consigneeAddress, DocAddressType.ConsigneeAddress);
			var importerDeliveryDataObj = declarationDataObject.AddOrgAddress(writeManager, importerDelivery, DocAddressType.ImporterPickupDeliveryAddress);
			var importerDocDataObj = declarationDataObject.AddOrgAddress(writeManager, importerDoc, DocAddressType.ImporterDocumentaryAddress);
			var importerDataObj = declarationDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			var consignorPickupDataObj = declarationDataObject.AddOrgAddress(writeManager, consignorPickup, DocAddressType.ConsignorPickupDeliveryAddress);
			var consignorDocDataObj = declarationDataObject.AddOrgAddress(writeManager, consignorDoc, DocAddressType.ConsignorDocumentaryAddress);
			var supplierPickupDataObj = declarationDataObject.AddOrgAddress(writeManager, supplierPickup, DocAddressType.SupplierPickupDeliveryAddress);
			var supplierDocDataObj = declarationDataObject.AddOrgAddress(writeManager, supplierDoc, DocAddressType.SupplierDocumentaryAddress);
			var supplierDataObj = declarationDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should match Importer", importer.PK, declarationBO.JE_OH_Importer);
			AssertEquals("Should match Supplier", supplier.PK, declarationBO.JE_OH_Supplier);

			var orgAddressCollection = declarationDataObject.OrganizationAddressCollection;
			orgAddressCollection.Remove(importerDataObj);
			orgAddressCollection.Remove(supplierDataObj);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should match ImporterDocumentaryAddress", importerDoc.PK, declarationBO.JE_OH_Importer);
			AssertEquals("Should match SupplierDocumentaryAddress", supplierDoc.PK, declarationBO.JE_OH_Supplier);

			orgAddressCollection.Remove(importerDocDataObj);
			orgAddressCollection.Remove(supplierDocDataObj);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should match ImporterPickupDeliveryAddress", importerDelivery.PK, declarationBO.JE_OH_Importer);
			AssertEquals("Should match SupplierPickupDeliveryAddress", supplierPickup.PK, declarationBO.JE_OH_Supplier);

			orgAddressCollection.Remove(importerDeliveryDataObj);
			orgAddressCollection.Remove(supplierPickupDataObj);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should match ConsigneeAddress", consigneeAddress.PK, declarationBO.JE_OH_Importer);
			AssertEquals("Should match ConsignorDocumentaryAddress", consignorDoc.PK, declarationBO.JE_OH_Supplier);

			orgAddressCollection.Remove(consigneeAddressDataObj);
			orgAddressCollection.Remove(consignorDocDataObj);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should match ConsigneeDocumentaryAddress", consigneeDoc.PK, declarationBO.JE_OH_Importer);
			AssertEquals("Should match ConsignorPickupDeliveryAddress", consignorPickup.PK, declarationBO.JE_OH_Supplier);

			orgAddressCollection.Remove(consigneeDocDataObj);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Should match ConsigneePickupDeliveryAddress", consigneeDelivery.PK, declarationBO.JE_OH_Importer);
		}

		public void TestPopulateSupplierAndImporterWhenUseSupplierAndImporterAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INV1";
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line1.JI_Description = "INV1LINE1";
				declaration.JE_AutoWeightApportion = true;
				Factory.SaveForTesting();

				var consigneeDelivery = CreateOrganisation("BOB", "ABC!@#1");
				var consigneeDoc = CreateOrganisation("JACK", "ABC!@#2");
				var consigneeAddress = CreateOrganisation("JANE", "ABC!@#3");
				var importerDelivery = CreateOrganisation("PETE", "ABC!@#4");
				var importerDoc = CreateOrganisation("MARY", "ABC!@#5");
				var importer = CreateOrganisation("MAT", "ABC!@#6");
				var consignorPickup = CreateOrganisation("JOE", "ABC!@#7");
				var consignorDoc = CreateOrganisation("MARK", "ABC!@#8");
				var supplierPickup = CreateOrganisation("SUE", "ABC!@#9");
				var supplierDoc = CreateOrganisation("KATE", "ABC!@#10");
				var supplier = CreateOrganisation("CATE", "ABC!@#11");

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext
				};
				var consigneeDeliveryDataObj = declarationDataObject.AddOrgAddress(writeManager, consigneeDelivery, DocAddressType.ConsigneePickupDeliveryAddress);
				var consigneeDocDataObj = declarationDataObject.AddOrgAddress(writeManager, consigneeDoc, DocAddressType.ConsigneeDocumentaryAddress);
				var consigneeAddressDataObj = declarationDataObject.AddOrgAddress(writeManager, consigneeAddress, DocAddressType.ConsigneeAddress);
				var importerDeliveryDataObj = declarationDataObject.AddOrgAddress(writeManager, importerDelivery, DocAddressType.ImporterPickupDeliveryAddress);
				var importerDocDataObj = declarationDataObject.AddOrgAddress(writeManager, importerDoc, DocAddressType.ImporterDocumentaryAddress);
				var importerDataObj = declarationDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
				var consignorPickupDataObj = declarationDataObject.AddOrgAddress(writeManager, consignorPickup, DocAddressType.ConsignorPickupDeliveryAddress);
				var consignorDocDataObj = declarationDataObject.AddOrgAddress(writeManager, consignorDoc, DocAddressType.ConsignorDocumentaryAddress);
				var supplierPickupDataObj = declarationDataObject.AddOrgAddress(writeManager, supplierPickup, DocAddressType.SupplierPickupDeliveryAddress);
				var supplierDocDataObj = declarationDataObject.AddOrgAddress(writeManager, supplierDoc, DocAddressType.SupplierDocumentaryAddress);
				var supplierDataObj = declarationDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				Assert(declarationBO.UseImporterAddress);
				Assert(declarationBO.UseSupplierAddress);
				AssertEquals("Should match Importer", importer.PK, declarationBO.JE_OH_Importer);
				AssertEquals("Should match Supplier", supplier.PK, declarationBO.JE_OH_Supplier);
				AssertEquals("Should match ImporterAddress", importer.MainAddress.PK, declarationBO.JE_OA_ImporterAddress);
				AssertEquals("Should match SupplierAddress", supplier.MainAddress.PK, declarationBO.JE_OA_SupplierAddress);

				var orgAddressCollection = declarationDataObject.OrganizationAddressCollection;
				orgAddressCollection.Remove(importerDataObj);
				orgAddressCollection.Remove(supplierDataObj);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("Should match ImporterDocumentaryAddress", importerDoc.PK, declarationBO.JE_OH_Importer);
				AssertEquals("Should match SupplierDocumentaryAddress", supplierDoc.PK, declarationBO.JE_OH_Supplier);
				AssertEquals("Should match ImporterDocumentaryAddress", importerDoc.MainAddress.PK, declarationBO.JE_OA_ImporterAddress);
				AssertEquals("Should match SupplierDocumentaryAddress", supplierDoc.MainAddress.PK, declarationBO.JE_OA_SupplierAddress);
			}
		}

		public void TestJE_OverrideFreightDefaultsDoNotFlipWhenImportTwice()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			var declaration = (BaseJobDeclaration)shipment.Declarations.FirstOrDefault();
			Assert(declaration.JE_OverrideFreightDefaults);

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();

			declaration = (BaseJobDeclaration)shipment.Declarations.FirstOrDefault();
			Assert(declaration.JE_OverrideFreightDefaults);

			Factory.FireCleanupAfterSaving();
		}

		public void TestMessageTypeWillNotBeOverlapAfterSupplierAddressSet()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "Z!";
				staff.GS_FullName = "DUMMY BOB";

				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;
				branch2.GB_Code = "B@#";
				branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				consignor.OH_IsConsignor = ZBool.True;
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				consignee.OH_IsConsignee = ZBool.True;
				var carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				carrier.OH_IsShippingLine = ZBool.True;
				var forwarderQuery = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, new[] { consignor.PK, consignee.PK, carrier.PK });
				forwarderQuery.AddToFilter(OrgHeaderSchema.OH_IsForwarder, ZBool.True);
				var forwarder = Factory.LoadTop1<OrgHeader>(forwarderQuery);

				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var declarationDataObject = SetupDeclaration("OW324", "MB1HB1", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				declarationDataObject.Branch = new Branch() { Code = "B@#" };
				declarationDataObject.ContainerCount = 1;
				declarationDataObject.MessageType.Code = JobMessageTypeList.Codes.Import;
				declarationDataObject.ShipmentIncoTerm.Code = Core.Constants.IncoTerms.CostAndFreight;
				var masterBillDataObject = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				var masterBillHouseBillDataObject = SetupAdditionalBill("MB1HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
				declarationDataObject.CustomsBroker = Staff.New(staff);
				declarationDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				declarationDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);

				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var readerMock = new Mock<JobDeclarationDataObjectReader>(new object[] { declarationDataObject, Logger, Factory, Factory.New<ForwardingShipment>() });
				readerMock.CallBase = true;
				var declaration = Factory.New<BaseJobDeclaration>();
				readerMock
					.Protected()
					.Setup<BaseJobDeclaration>("GetNewBusinessObjectCore")
					.Returns(declaration);
				readerMock.Object.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				Factory.FireCleanupAfterSaving();
				AssertNotNull(declaration);
				AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
				readerMock.VerifyAll();

				declarationDataObject = SetupDeclaration("OW324", "MB1HB1", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				declarationDataObject.Branch = new Branch() { Code = "B@#" };
				declarationDataObject.ContainerCount = 1;
				declarationDataObject.MessageType.Code = JobMessageTypeList.Codes.Export;
				declarationDataObject.ShipmentIncoTerm.Code = Core.Constants.IncoTerms.CostAndFreight;
				masterBillDataObject = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				masterBillHouseBillDataObject = SetupAdditionalBill("MB1HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
				declarationDataObject.CustomsBroker = Staff.New(staff);
				declarationDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				declarationDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
				declarationDataObject.AddOrgAddress(writeManager, carrier, AddressTypes.ShippingLine);
				declarationDataObject.AddOrgAddress(writeManager, forwarder, AddressTypes.Forwarder);

				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var readerMock2 = new Mock<JobDeclarationDataObjectReader>(new object[] { declarationDataObject, Logger, Factory, Factory.New<ForwardingShipment>() });
				declaration = Factory.New<BaseJobDeclaration>();
				readerMock2
					.Protected()
					.Setup<BaseJobDeclaration>("GetNewBusinessObjectCore")
					.Returns(declaration);
				readerMock2.Object.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				Factory.FireCleanupAfterSaving();
				AssertNotNull(declaration);
				AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
				readerMock2.VerifyAll();
			}
		}

		public void TestDeclarationDataAreSetInASpecificOrder()
		{
			localCountryCustomsInterface?.Dispose();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "Z!";
				staff.GS_FullName = "DUMMY BOB";

				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;
				branch2.GB_Code = "B@#";
				branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				consignor.OH_IsConsignor = ZBool.True;
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				consignee.OH_IsConsignee = ZBool.True;
				var declarant = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

				var carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				carrier.OH_IsShippingLine = ZBool.True;
				var forwarderQuery = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, new[] { consignor.PK, consignee.PK, carrier.PK });
				forwarderQuery.AddToFilter(OrgHeaderSchema.OH_IsForwarder, ZBool.True);
				var forwarder = Factory.LoadTop1<OrgHeader>(forwarderQuery);

				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var declarationDataObject = SetupDeclaration("OW324", "MB1HB1", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				declarationDataObject.Branch = new Branch() { Code = "B@#" };
				declarationDataObject.ContainerCount = 1;
				declarationDataObject.MessageType.Code = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
				declarationDataObject.ShipmentIncoTerm.Code = Core.Constants.IncoTerms.CostAndFreight;
				var masterBillDataObject = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				var masterBillHouseBillDataObject = SetupAdditionalBill("MB1HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
				declarationDataObject.CustomsBroker = Staff.New(staff);
				declarationDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				declarationDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
				declarationDataObject.AddOrgAddress(writeManager, declarant, AddressTypes.Declarant);
				declarationDataObject.AddOrgAddress(writeManager, carrier, AddressTypes.ShippingLine);
				declarationDataObject.AddOrgAddress(writeManager, forwarder, AddressTypes.Forwarder);
				declarationDataObject.SetDateCollection(() => new List<Date>(new[]
				{
					Date.New(DateType.Departure, ZBool.True, new ZDateTime(2012, 4, 5, 1, 2, 3)),
					Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2012, 4, 6, 2, 3, 4)),
					Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2012, 5, 6, 3, 4, 5)),
					Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2012, 5, 7, 4, 5, 6)),
					Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2012, 5, 8, 5, 6, 7))
				}));
				declarationDataObject.PortOfLoading.Code = "AU123";
				declarationDataObject.PortOfDischarge.Code = "NZ345";
				declarationDataObject.CustomsProfileIdentifier = new ValueTypePair { Value = "PROF002" };
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOC002" };
				declarationDataObject.SubLocationAtClearance = new CodeDescriptionPair35Char() { Code = "SUBLOC2" };
				declarationDataObject.CustomsOffice = new CodeDescriptionPair10Char() { Code = "CUSOF2" };

				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var readerMock = new Mock<JobDeclarationDataObjectReader>(new object[] { declarationDataObject, Logger, Factory, Factory.New<ForwardingShipment>() });
				readerMock.CallBase = true;
				var declaration = Factory.New<BaseJobDeclaration>();
				readerMock
					.Protected()
					.Setup<BaseJobDeclaration>("GetNewBusinessObjectCore")
					.Returns(declaration);
				var expectedOrders = new List<InfoValueChangeData>(new[]
				{
					new InfoValueChangeData(declaration.JE_OverrideFreightDefaultsInfo, ZBool.False, ZBool.True),
					new InfoValueChangeData(declaration.JE_GBInfo, ZGuid.Empty, branch2.PK),
					new InfoValueChangeData(declaration.JE_OA_DeclarantAddressInfo, ZGuid.Empty, declarant.MainAddress.PK),
					new InfoValueChangeData(declaration.JE_OH_SupplierInfo, ZGuid.Empty, consignor.PK),
					new InfoValueChangeData(declaration.JE_OH_ImporterInfo, ZGuid.Empty, consignee.PK),
					new InfoValueChangeData(declaration.JE_MessageTypeInfo, ZString.Empty, (ZString)JobMessageTypeList.Codes.ImportDeclarationByExternalBroker),
					new InfoValueChangeData(declaration.JE_MessageSubTypeInfo, (ZString)"FRM", (ZString)"ST1"),
					new InfoValueChangeData(declaration.JE_CustomsProfileInfo, ZString.Empty, (ZString)"PROF002"),
					new InfoValueChangeData(declaration.JE_LocationOfGoodsInfo, ZString.Empty, (ZString)"LOC002"),
					new InfoValueChangeData(declaration.JE_SubLocationOfGoodsInfo, ZString.Empty, (ZString)"SUBLOC2"),
					new InfoValueChangeData(declaration.JE_CustomsOfficeInfo, ZString.Empty, (ZString)"CUSOF2"),
					new InfoValueChangeData(declaration.JE_TransportModeInfo, ZString.Empty, (ZString)"SEA"),
					new InfoValueChangeData(declaration.JE_ContainerModeInfo, ZString.Empty, (ZString)"CNT"),
					new InfoValueChangeData(declaration.JE_RS_NKServiceLevelInfo, (ZString)"STD", (ZString)"STD"),
					new InfoValueChangeData(declaration.JE_MasterBillInfo, ZString.Empty, (ZString)"MB1"),
					new InfoValueChangeData(declaration.JE_VesselNameInfo, ZString.Empty, (ZString)"BUNGA DELIMA"),
					new InfoValueChangeData(declaration.JE_VoyageFlightNoInfo, ZString.Empty, (ZString)"343L"),
					new InfoValueChangeData(declaration.JE_FolioInfo, ZString.Empty, (ZString)"F234"),
					new InfoValueChangeData(declaration.JE_RL_NKPortOfLoadingInfo, (ZString)"ZAJNB", (ZString)"AU123"),
					new InfoValueChangeData(declaration.JE_ExportDateInfo, ZDateTime.Empty, new ZDateTime(2012, 4, 6, 2, 3, 4)),
					new InfoValueChangeData(declaration.JE_RL_NKPortOfArrivalInfo, (ZString)"AUSYD", (ZString)"NZ345"),
					new InfoValueChangeData(declaration.JE_DateOfArrivalInfo, ZDateTime.Empty, new ZDateTime(2012, 5, 7, 4, 5, 6)),
					new InfoValueChangeData(declaration.JE_HouseBillInfo, ZString.Empty, (ZString)"MB1HB1"),
					new InfoValueChangeData(declaration.JE_RL_NKOriginInfo, (ZString)"ZAJNB", (ZString)"NZDUD"),
					new InfoValueChangeData(declaration.JE_DateAtOriginInfo, ZDateTime.Empty, new ZDateTime(2012, 4, 5, 1, 2, 3)),
					new InfoValueChangeData(declaration.JE_RL_NKFinalDestinationInfo, (ZString)"AUSYD", (ZString)"AUBDG"),
					new InfoValueChangeData(declaration.JE_DateAtFinalDestinationInfo, ZDateTime.Empty, new ZDateTime(2012, 5, 8, 5, 6, 7)),
					new InfoValueChangeData(declaration.JE_GoodsDescriptionInfo, ZString.Empty, (ZString)"RAT HATS"),
					new InfoValueChangeData(declaration.JE_OwnerRefInfo, ZString.Empty, (ZString)"OW324"),
					new InfoValueChangeData(declaration.JE_TotalWeightInfo, ZDecimal.Zero, (ZDecimal)34.56m),
					new InfoValueChangeData(declaration.JE_TotalWeightUnitInfo, ZString.Empty, (ZString)"KT"),
					new InfoValueChangeData(declaration.JE_TotalVolumeInfo, ZDecimal.Zero, (ZDecimal)23.45m),
					new InfoValueChangeData(declaration.JE_TotalVolumeUnitInfo, ZString.Empty, (ZString)"CF"),
					new InfoValueChangeData(declaration.JE_TotalNoOfPiecesInfo, ZInt.Zero, (ZInt)112),
					new InfoValueChangeData(declaration.JE_LandedPiecesInfo, ZInt.Zero, (ZInt)306),
					new InfoValueChangeData(declaration.JE_ContainerCountInfo, ZShort.Zero, (ZShort)1),
					new InfoValueChangeData(declaration.JE_TotalNoOfPacksInfo, ZInt.Zero, (ZInt)45),
					new InfoValueChangeData(declaration.JE_TotalNoOfPacksPackTypeInfo, ZString.Empty, (ZString)"KEG"),
					new InfoValueChangeData(declaration.JE_ShipmentIncoTermInfo, (ZString)Core.Constants.IncoTerms.FreeOnBoard, (ZString)Core.Constants.IncoTerms.CostAndFreight),
					new InfoValueChangeData(declaration.JE_OH_ShippingLineInfo, ZGuid.Empty, carrier.PK),
					new InfoValueChangeData(declaration.JE_OH_ForwarderInfo, ZGuid.Empty, forwarder.PK),
					new InfoValueChangeData(declaration.JE_GS_NKCusAgentInfo, ZString.Empty, (ZString)"Z!"),
					new InfoValueChangeData(declaration.JE_MergeByInfo, (ZString)"NON", (ZString)"TRF"),
					new InfoValueChangeData(declaration.JE_PaymentMethodInfo, ZString.Empty, (ZString)"BRK"),
					new InfoValueChangeData(declaration.JE_PaidByInfo, ZString.Empty, (ZString)"BRK")
				});

				AssertDataWasSetInSpecificOrder(expectedOrders, () => readerMock.Object.ReadIntoBusinessObject());
				Factory.FireCleanupAfterSaving();
				readerMock.VerifyAll();
			}
		}

		public void TestImportingLandedCostingDoesNotDeleteExistingInvoiceIfNotNeeded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MasterBill = "MYMASTER";
				declaration.JE_HouseBill = "MYHOUSE";
				declaration.JE_OwnerRef = "OWN324";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				Factory.SaveForTesting();
				var shipmentDataObject = SetupDeclaration("OWN324", "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				var masterBillDataObject = SetupAdditionalBill("MYMASTER", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				var masterBillHouseBillDataObject = SetupAdditionalBill("MYHOUSE", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MYMASTER", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				shipmentDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));

				shipmentDataObject.CommercialInfo = new CommercialInfo()
				{
					DateOfLandedCostProcessing = new ZDateTime(2015, 1, 25),
					TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost() }),
				};

				var reader = new JobDeclarationDataObjectReader(shipmentDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				Factory.FireCleanupAfterSaving();
				AssertEquals(declaration, declarationBO);
				AssertNotNull(declarationBO);
				AssertEquals("declarationBO.JE_OwnerRef", "OWN324", declarationBO.JE_OwnerRef);
				AssertEquals("declarationBO.JE_MasterBill", "MYMASTER", declarationBO.JE_MasterBill);
				AssertEquals("declarationBO.JE_HouseBill", "MYHOUSE", declarationBO.JE_HouseBill);
				AssertEquals("declarationBO.JobComInvoiceGroupHeaders.Count", 1, declarationBO.JobComInvoiceGroupHeaders.Count);
				var topGroupInvoice = declarationBO.JobComInvoiceGroupHeaders[0];
				AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
				AssertEquals(false, invoice.IsDeleted);
				AssertEquals("declarationBO.Invoices[0]", invoice, declarationBO.Invoices[0]);
				AssertEquals("declarationBO.InvoiceLines.Count", 1, declarationBO.InvoiceLines.Count);
				AssertEquals(false, invoiceLine.IsDeleted);
				AssertEquals("declarationBO.InvoiceLines[0]", invoiceLine, declarationBO.InvoiceLines[0]);
				var landedCostHeader = (LandedCosting.ILandedCostHeader)declarationBO.LandedCostHeaderForDocuments;
				AssertNotNull("landedCostHeader should have been created", landedCostHeader);
				AssertEquals("landedCostHeader.LT_DateOfProcessing should not be imported", ZDateTime.Empty, landedCostHeader.LT_DateOfProcessing);
				var costInputs = Factory.BOFactory.Load<LandedCosting.ILandCostInput>(new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK));
				AssertEquals("costInputs.Length", 1, costInputs.Length);
				AssertLandCostInputContents(costInputs[0], topGroupInvoice);
				var histories = Factory.BOFactory.Load<LandedCosting.ILandedCostHistory>(new ZQuery(LandedCostHistorySchema.LH_LT, landedCostHeader.PK));
				AssertEquals("histories should not be imported", 0, histories.Length);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Updated Declaration B00001000 from UniversalShipment.
Information - Successfully saved Declaration B00001000 with 2 x Bill, 1 x LandCostInput.".Trim(), Logger.Logs);

				shipmentDataObject.CommercialInfo.TransportLogisticsCostCollection = null;
				Logger.ClearLogs();
				reader = new JobDeclarationDataObjectReader(shipmentDataObject, Logger, Factory);
				declarationBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				Factory.FireCleanupAfterSaving();
				AssertEquals(declaration, declarationBO);
				AssertNotNull(declarationBO);
				AssertEquals("declarationBO.JE_OwnerRef", "OWN324", declarationBO.JE_OwnerRef);
				AssertEquals("declarationBO.JE_MasterBill", "MYMASTER", declarationBO.JE_MasterBill);
				AssertEquals("declarationBO.JE_HouseBill", "MYHOUSE", declarationBO.JE_HouseBill);
				AssertEquals("declarationBO.JobComInvoiceGroupHeaders.Count", 1, declarationBO.JobComInvoiceGroupHeaders.Count);
				topGroupInvoice = declarationBO.JobComInvoiceGroupHeaders[0];
				AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
				AssertEquals(false, invoice.IsDeleted);
				AssertEquals("declarationBO.Invoices[0]", invoice, declarationBO.Invoices[0]);
				AssertEquals("declarationBO.InvoiceLines.Count", 1, declarationBO.InvoiceLines.Count);
				AssertEquals(false, invoiceLine.IsDeleted);
				AssertEquals("declarationBO.InvoiceLines[0]", invoiceLine, declarationBO.InvoiceLines[0]);
				AssertEquals("landedCostHeader.IsDeleted", false, ((BusinessObject)landedCostHeader).IsDeleted);
				AssertEquals(landedCostHeader, (LandedCosting.ILandedCostHeader)declarationBO.LandedCostHeaderForDocuments);
				AssertEquals("landedCostHeader.LT_DateOfProcessing should not be imported", ZDateTime.Empty, landedCostHeader.LT_DateOfProcessing);
				var costInput = costInputs[0];
				AssertEquals("costInput.IsDeleted", false, ((BusinessObject)costInput).IsDeleted);
				costInputs = Factory.BOFactory.Load<LandedCosting.ILandCostInput>(new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK));
				AssertEquals("costInputs.Length", 1, costInputs.Length);
				AssertEquals("costInput", costInput, costInputs[0]);
				AssertLandCostInputContents(costInput, topGroupInvoice);
				histories = Factory.BOFactory.Load<LandedCosting.ILandedCostHistory>(new ZQuery(LandedCostHistorySchema.LH_LT, landedCostHeader.PK));
				AssertEquals("histories should not be imported", 0, histories.Length);
				AssertMultilineASCIIEquals("Logger.Logs", @"Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Updated Declaration B00001000 from UniversalShipment.
Information - Successfully saved Declaration B00001000 with 2 x Bill.".Trim(), Logger.Logs);
			}
		}

		public void TestImportOwnerRefWithMaxLengthThatComesFromBizOAndNotSchema()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				AssertEquals("Pre-req, maxlength of GB JE_OwnerRef is 21 and comes from the business object", 21, dec.JE_OwnerRefInfo.MaxLength);
				var shipmentDataObject = SetupDeclaration("12345678911234567892123456789312345", "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				var reader = new JobDeclarationDataObjectReader(shipmentDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				Factory.FireCleanupAfterSaving();
				AssertEquals("123456789112345678921", declarationBO.JE_OwnerRef);
				AssertNotContains("'JE_OwnerRef' has been exceeded", Logger.Logs);
			}
		}

		public void TestDoNotImportOwnerRefIfTheNodeDoesNotExist()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_OwnerRef = "123456";
			var shipmentDataObject1 = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			shipmentDataObject1.DataContext.DataTargetCollection.First().Key = "B00001000";
			var reader1 = new JobDeclarationDataObjectReader(shipmentDataObject1, Logger, Factory);
			var declarationBO1 = reader1.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			Factory.FireCleanupAfterSaving();
			AssertEquals("123456", declarationBO1.JE_OwnerRef);

			var shipmentDataObject2 = SetupDeclaration("", "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			shipmentDataObject2.DataContext.DataTargetCollection.First().Key = "B00001000";
			var reader2 = new JobDeclarationDataObjectReader(shipmentDataObject2, Logger, Factory);
			var declarationBO2 = reader2.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			Factory.FireCleanupAfterSaving();
			AssertEquals("", declarationBO2.JE_OwnerRef);
		}

		public void TestImportingLandedCosting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipmentDataObject = SetupDeclaration("OWN324", "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				var masterBillDataObject = SetupAdditionalBill("MYMASTER", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				var masterBillHouseBillDataObject = SetupAdditionalBill("MYHOUSE", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MYMASTER", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				shipmentDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
				var containerData = SetupContainer();
				containerData.SetTransportLogisticsCostCollection(() => new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost2() }));

				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerData }));
				shipmentDataObject.CommercialInfo = new CommercialInfo()
				{
					DateOfLandedCostProcessing = new ZDateTime(2015, 1, 25),
					TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost() }),
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
							TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost2() })
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LandedCostDetail = new LandedCostDetail(DefaultDataObjectWriterStrategy.TestInstance) { MarkUp1 = 10m },
									TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost() })
								}
							})))
					})
				};

				var reader = new JobDeclarationDataObjectReader(shipmentDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				Factory.FireCleanupAfterSaving();

				AssertNotNull(declarationBO);
				AssertEquals("declarationBO.JE_OwnerRef", "OWN324", declarationBO.JE_OwnerRef);
				AssertEquals("declarationBO.JE_MasterBill", "MYMASTER", declarationBO.JE_MasterBill);
				AssertEquals("declarationBO.JE_HouseBill", "MYHOUSE", declarationBO.JE_HouseBill);
				AssertEquals("declarationBO.JobComInvoiceGroupHeaders.Count", 1, declarationBO.JobComInvoiceGroupHeaders.Count);
				declarationBO.CusContainers.Load();
				AssertEquals("declarationBO.CusContainers.Count", 1, declarationBO.CusContainers.Count);
				var container = declarationBO.CusContainers[0];
				var topGroupInvoice = declarationBO.JobComInvoiceGroupHeaders[0];
				AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
				var invoice = declarationBO.Invoices[0];
				AssertEquals("declarationBO.InvoiceLines.Count", 1, declarationBO.InvoiceLines.Count);
				var invoiceLine = declarationBO.InvoiceLines[0];
				var landedCostHeader = (LandedCosting.ILandedCostHeader)declarationBO.LandedCostHeaderForDocuments;
				AssertNotNull("landedCostHeader should have been created", landedCostHeader);
				AssertEquals("landedCostHeader.LT_DateOfProcessing should not be imported", ZDateTime.Empty, landedCostHeader.LT_DateOfProcessing);
				var costInputs = Factory.BOFactory.Load<LandedCosting.ILandCostInput>(new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK));
				AssertEquals("costInputs.Length", 4, costInputs.Length);
				var costInput1 = costInputs.FirstOrDefault(x => x.LI_ParentID == container.PK);
				AssertLandCostInputContents2(costInput1, container);
				var costInput2 = costInputs.FirstOrDefault(x => x.LI_ParentID == topGroupInvoice.PK);
				AssertLandCostInputContents(costInput2, topGroupInvoice);
				var costInput3 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoice.PK);
				AssertLandCostInputContents2(costInput3, invoice);
				var costInput4 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoiceLine.PK);
				AssertLandCostInputContents(costInput4, invoiceLine);
				var histories = Factory.BOFactory.Load<LandedCosting.ILandedCostHistory>(new ZQuery(LandedCostHistorySchema.LH_LT, landedCostHeader.PK));
				AssertEquals("histories should not be imported", 0, histories.Length);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching JobDeclaration found, creating new JobDeclaration.
Information - Populating JobDeclaration...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching CusContainer found, creating new CusContainer.
Information - Populating CusContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Added Declaration (Master Bill='MYMASTER' House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully saved Declaration B00001000 with 2 x Bill, 1 x ForwardingContainer, 1 x CusContainer, 4 x LandCostInput.".Trim(), Logger.Logs);

				landedCostHeader.LT_DateOfProcessing = new ZDateTime(2015, 3, 5);
				var container2 = declarationBO.CusContainers.AddNew();
				container2.CO_ContainerNumber = "CONT7565455";
				costInput1.LI_CostAmount = 12.684m;
				var costInput5 = Factory.BOFactory.New<LandedCosting.ILandCostInput>();
				costInput5.LI_AC_ChargeCode = AccChargeCode1.PK;
				costInput5.LI_LT = costInput1.LI_LT;
				costInput5.LI_ParentID = container2.PK;
				costInput5.LI_ParentTableCode = container2.TablePrefix;
				costInput5.LI_CostAmount = 10m;
				costInput5.LI_ChargeDescription = "BOB 3";
				costInput5.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
				costInput5.LI_DistributeCostBy = "!SD";
				costInput5.LI_LandedCostGroup = (ZByte)4;
				costInput5.LI_ServiceExRate = 1.2321m;

				var costInput6 = Factory.BOFactory.New<LandedCosting.ILandCostInput>();
				costInput6.LI_AC_ChargeCode = AccChargeCode1.PK;
				costInput6.LI_LT = costInput1.LI_LT;
				costInput6.LI_ParentID = container.PK;
				costInput6.LI_ParentTableCode = container.TablePrefix;
				costInput6.LI_CostAmount = 10.20m;
				costInput6.LI_ChargeDescription = "BOB 3";
				costInput6.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
				costInput6.LI_DistributeCostBy = "!SD";
				costInput6.LI_LandedCostGroup = (ZByte)4;
				costInput6.LI_ServiceExRate = 1.2321m;

				Logger.ClearLogs();
				reader = new JobDeclarationDataObjectReader(shipmentDataObject, Logger, Factory);
				AssertEquals(declarationBO, reader.ReadIntoBusinessObject());
				Factory.FireCleanupAfterSaving();
				AssertEquals("declarationBO.JE_OwnerRef", "OWN324", declarationBO.JE_OwnerRef);
				AssertEquals("declarationBO.JE_MasterBill", "MYMASTER", declarationBO.JE_MasterBill);
				AssertEquals("declarationBO.JE_HouseBill", "MYHOUSE", declarationBO.JE_HouseBill);
				AssertEquals("declarationBO.JobComInvoiceGroupHeaders.Count", 1, declarationBO.JobComInvoiceGroupHeaders.Count);
				AssertEquals("topGroupInvoice.IsDeleted", false, topGroupInvoice.IsDeleted);
				AssertEquals(topGroupInvoice, declarationBO.JobComInvoiceGroupHeaders[0]);
				AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
				AssertCollectionNotContains(invoice, declarationBO.Invoices);
				AssertEquals("invoice.IsDeleted", true, invoice.IsDeleted);
				invoice = declarationBO.Invoices[0];
				AssertEquals("declarationBO.InvoiceLines.Count", 1, declarationBO.InvoiceLines.Count);
				AssertCollectionNotContains(invoiceLine, declarationBO.InvoiceLines);
				AssertEquals("invoiceLine.IsDeleted", true, invoiceLine.IsDeleted);
				invoiceLine = declarationBO.InvoiceLines[0];
				AssertEquals(landedCostHeader, declarationBO.LandedCostHeaderForDocuments);
				AssertEquals("landedCostHeader.LT_DateOfProcessing", new ZDateTime(2015, 3, 5), landedCostHeader.LT_DateOfProcessing);
				declarationBO.CusContainers.Load();
				AssertEquals("declarationBO.CusContainers.Count", 1, declarationBO.CusContainers.Count);
				AssertEquals(container, declarationBO.CusContainers[0]);
				AssertEquals("container2.IsDeleted", true, container2.IsDeleted);

				costInputs = Factory.BOFactory.Load<LandedCosting.ILandCostInput>(new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK));
				AssertEquals("costInputs.Length", 5, costInputs.Length);
				AssertEquals("costInput1.IsDeleted", false, ((BusinessObject)costInput1).IsDeleted);
				AssertEquals("costInput2.IsDeleted", true, ((BusinessObject)costInput2).IsDeleted);
				AssertEquals("costInput3.IsDeleted", true, ((BusinessObject)costInput3).IsDeleted);
				AssertEquals("costInput4.IsDeleted", true, ((BusinessObject)costInput4).IsDeleted);
				AssertEquals("costInput5.IsDeleted", true, ((BusinessObject)costInput5).IsDeleted);
				AssertEquals("costInput6.IsDeleted", false, ((BusinessObject)costInput6).IsDeleted);
				AssertEquals(costInput1, costInputs.FirstOrDefault(x => x.LI_ParentID == container.PK));
				AssertLandCostInputContents2(costInput1, container);
				costInput2 = costInputs.FirstOrDefault(x => x.LI_ParentID == topGroupInvoice.PK);
				AssertLandCostInputContents(costInput2, topGroupInvoice);
				costInput3 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoice.PK);
				AssertLandCostInputContents2(costInput3, invoice);
				costInput4 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoiceLine.PK);
				AssertLandCostInputContents(costInput4, invoiceLine);
				AssertLandCostInputContents(costInput6, container, AccChargeCode1.PK, "BOB 3", 10.20m, Core.Constants.CurrencyCodes.NewZealand, "!SD", 4, 1.2321m);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Successfully loaded matching CusContainer.
Information - Populating CusContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Successfully loaded matching LandCostInput.
Information - Populating LandCostInput...
Information - Updated Declaration B00001000 from UniversalShipment.".Trim(), Logger.Logs);
			}
		}

		public void TestImportingAdditionalBills_UseDefaulting_True() => AssertImportingAdditionalBills(true);
		public void TestImportingAdditionalBills_UseDefaulting_False() => AssertImportingAdditionalBills(false);

		void AssertImportingAdditionalBills(bool useDefaulting)
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useDefaulting))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var shipmentDataObject = SetupDeclaration("OWN324", "MYHOUSE1", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				var masterBill1DataObject = SetupAdditionalBill("MYMASTER1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, null, null, null, null, null, null);
				var masterBill1HouseBill1DataObject = SetupAdditionalBill("MYHOUSE1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, null, "MYMASTER1", null, null, null, null);
				var masterBill1HouseBill2DataObject = SetupAdditionalBill("MYHOUSE2", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, null, "MYMASTER1", null, null, null, null);
				var masterBill2DataObject = SetupAdditionalBill("MYMASTER2", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, null, null, null, null, null, null);
				var masterBill2HouseBill1DataObject = SetupAdditionalBill("MYHOUSE3", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, null, "MYMASTER2", null, null, null, null);
				var masterBill2HouseBill2DataObject = SetupAdditionalBill("MYHOUSE4", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, null, "MYMASTER2", null, null, null, null);
				var masterBill3DataObject = SetupAdditionalBill("", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, null, null, null, null, null, null);
				var masterBill3HouseBill1DataObject = SetupAdditionalBill("MYHOUSE1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, null, "", null, null, null, null);
				shipmentDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
				{
					masterBill1DataObject, masterBill1HouseBill1DataObject, masterBill1HouseBill2DataObject,
					masterBill2DataObject, masterBill2HouseBill1DataObject, masterBill2HouseBill2DataObject,
					masterBill3DataObject, masterBill3HouseBill1DataObject
				}));

				var reader = new JobDeclarationDataObjectReader(shipmentDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);
				Factory.FireCleanupAfterSaving();

				AssertNotNull(declarationBO);
				AssertEquals("declarationBO.JE_OwnerRef", "OWN324", declarationBO.JE_OwnerRef);
				AssertEquals("declarationBO.JE_MasterBill", "MYMASTER1", declarationBO.JE_MasterBill);
				AssertEquals("declarationBO.JE_HouseBill", "MYHOUSE1", declarationBO.JE_HouseBill);
				AssertEquals("declarationBO.LowestBills.Count", 5, declarationBO.LowestBills.Count);
				var masterBills = new List<Bill>();
				var houseBills = new List<Bill>();
				foreach (var bill in declarationBO.Bills.Cast<Bill>())
				{
					if (bill.IsMasterBill)
					{
						masterBills.Add(bill);
					}
					else
					{
						houseBills.Add(bill);
					}
				}
				AssertEquals("masterBills.Count", 3, masterBills.Count);
				var primaryMasterBill = declarationBO.PrimaryMasterBill;
				masterBills.Remove(primaryMasterBill);
				AssertEquals("masterBills.Count", 2, masterBills.Count);
				var masterBill2 = masterBills[0];
				var masterBill3 = masterBills[1];
				if (masterBill2.CU_BillNum.IsEmpty)
				{
					masterBill2 = masterBills[1];
					masterBill3 = masterBills[0];
				}

				AssertBill(primaryMasterBill, "MYMASTER1", ZGuid.Empty, 2);
				AssertBill(masterBill2, "MYMASTER2", ZGuid.Empty, 2);
				AssertBill(masterBill3, "", ZGuid.Empty, 1);

				AssertEquals("houseBills.Count", 5, houseBills.Count);
				var primaryHouseBill = declarationBO.PrimaryHouseBill;
				houseBills.Remove(primaryHouseBill);
				AssertEquals("houseBills.Count", 4, houseBills.Count);
				houseBills = houseBills.OrderBy(x => x.CU_BillNum).ToList();

				AssertBill(primaryHouseBill, "MYHOUSE1", primaryMasterBill.PK, 0);
				AssertBill(houseBills[0], "MYHOUSE1", masterBill3.PK, 0);
				AssertBill(houseBills[1], "MYHOUSE2", primaryMasterBill.PK, 0);
				AssertBill(houseBills[2], "MYHOUSE3", masterBill2.PK, 0);
				AssertBill(houseBills[3], "MYHOUSE4", masterBill2.PK, 0);
			}
		}

		void AssertBill(Bill bill, ZString billNumber, ZGuid parentBillPK, int childBillsCount)
		{
			AssertEquals("bill.CU_BillNum", billNumber, bill.CU_BillNum);
			AssertEquals("bill.CU_CU_ParentBill", parentBillPK, bill.CU_CU_ParentBill);
			AssertEquals("bill.ChildBills.Count", childBillsCount, bill.ChildBills.Count);
		}

		[TestDate(2011, 12, 12)]
		public void TestImportingOfChargeLines()
		{
			using (Factory.BOFactory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var localCurrency = GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency;
				var shipmentDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });

				shipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "B00001000");

				shipmentDataObject.AdditionalTerms = "Add Me Some Terms";
				shipmentDataObject.WayBillNumber = "MYHOUSE";
				shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

				shipmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.JobCosting.Branch = new Branch();
				shipmentDataObject.JobCosting.Branch.Code = "SYD";
				shipmentDataObject.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());
				ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", new ZDateTime(2011, 10, 04), null, new ZDateTime(2011, 10, 04), 100.00m, 100.00m, localCurrency, null,
					"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, null, "FIN", 100.00m, 100.00m, localCurrency, null);
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLine1);
				ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", new ZDateTime(2011, 10, 04), null, new ZDateTime(2011, 10, 04), 200.00m, 200.00m, localCurrency, null,
					"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, null, "FIN", 250.00m, 250.00m, localCurrency, null);
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLine2);
				chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;
				chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine2.ImportMetaData.Instruction = InstructionType.Insert;

				JobHeader[] jobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "S00001000").AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("jobs.Length", 0, jobs.Length);

				JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery());
				AssertEquals("charges.Length", 0, charges.Length);

				var reader = new JobDeclarationDataObjectReader(shipmentDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);

				AssertNotNull(declarationBO);

				AssertEquals("declarationBO.JE_HouseBill", "MYHOUSE", declarationBO.JE_HouseBill);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching JobDeclaration found, creating new JobDeclaration.
Information - Populating JobDeclaration...
Warning - Whilst importing Charge Line: Job Number= Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Whilst importing Charge Line: Job Number= Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Revenue Override Comment: You have not entered a Revenue Override Comment.
Information - Added Declaration from UniversalShipment.
Information - Successfully saved Declaration B00001000.
".Trim(), Logger.Logs);

				jobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "B00001000").AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("jobs.Length", 1, jobs.Length);

				charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobs[0].PK));
				AssertEquals("charges.Length", 2, charges.Length);

				JobCharge fRTCharge;
				JobCharge bAFCharge;

				if (charges[0].ChargeCode.AC_Code == "FRT")
				{
					fRTCharge = charges[0];
					bAFCharge = charges[1];
				}
				else
				{
					fRTCharge = charges[1];
					bAFCharge = charges[0];
				}

				AssertCharge(fRTCharge, chargeLine1);
				AssertCharge(bAFCharge, chargeLine2);
			}
		}

		public void TestFieldsThatShouldNotBeImportedForNonStandaloneDeclaration()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "BRK";
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_SubType1 = "SEA";
			processTaskTemplate.P0_SubType2 = "IMP";

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";
			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Z!1";
			org1.OH_FullName = "BOB THE BUILDER";
			org1.MainAddress.OA_Address1 = "BOB STREET";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Z!2";
			org2.OH_FullName = "WENDY THE DESTROYER";
			org2.MainAddress.OA_Address1 = "WENDY STREET";
			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = System.Guid.Empty;
			registryInstance.ShipmentCustomDecimalNo1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Deci Deca", ""));
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
			};
			declarationDataObject.LocalProcessing = LocalProcessingDataObjectReaderTest.SetupLocalProcessing(new CodeDescriptionPair() { Code = "BOB", Description = "THE BUILDER" }, new CodeDescriptionPair() { Code = "WEN", Description = "THE DESTROYER" }, new CodeDescriptionPair() { Code = "JOE", Description = "THE PEACEMAKER" }, new CodeDescriptionPair() { Code = "JAY", Description = "THE LAZY" });
			declarationDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Deci Deca", new ZDecimal(1.3)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>(new[] { SetupNote() }));
			declarationDataObject.AddOrgAddress(writeManager, org1.MainAddress, AddressTypes.PickupLocalCartage);
			declarationDataObject.AddOrgAddress(writeManager, org2.MainAddress, AddressTypes.DeliveryLocalCartage);
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { SetupTransportLeg() }));

			ITopLevelDataObjectReader reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			BusinessObject declarationAsBO = declaration;
			reader.ReadIntoBusinessObject(ref declarationAsBO);
			AssertEquals("JE_OverrideFreightDefaults", ZBool.True, declaration.JE_OverrideFreightDefaults);

			var docsAndCartage = declaration.DocsAndCartage;
			AssertEquals("docsAndCartage.JP_FCLPickupEquipmentNeeded", ZString.Empty, docsAndCartage.JP_FCLPickupEquipmentNeeded);
			AssertEquals("docsAndCartage.JP_CustomDecimal1", 1.3m, docsAndCartage.JP_CustomDecimal1);
			AssertEquals("docsAndCartage.JP_OA_DeliveryCartageCoAddr", org2.MainAddress.PK, docsAndCartage.JP_OA_DeliveryCartageCoAddr);
			AssertEquals("docsAndCartage.JP_OA_PickupCartageCoAddr", org1.MainAddress.PK, docsAndCartage.JP_OA_PickupCartageCoAddr);

			var firstDateField = declaration.GetUserDefinedValue<ZDateTime>("First Date");
			AssertEquals("firstDateField", new ZDateTime(2011, 1, 1), firstDateField);
			StmNote[] note = declaration.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("No note 'DOG FLOGGER!!' should exists.", 0, note.Length);

			declaration.Transports.Load();
			AssertEquals("declaration.Transports.Count", 0, declaration.Transports.Count);

			declaration = Factory.New<BaseJobDeclaration>();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			declarationAsBO = declaration;
			reader.ReadIntoBusinessObject(ref declarationAsBO);
			AssertEquals("JE_OverrideFreightDefaults", ZBool.False, declaration.JE_OverrideFreightDefaults);

			docsAndCartage = declaration.DocsAndCartage;
			LocalProcessingDataObjectReaderTest.AssertContents(
				docsAndCartage,
				"",
				ZDateTime.Empty,
				ZDateTime.Empty,
				ZDateTime.Empty,
				ZDateTime.Empty,
				"ARRCARREF1",
				ZDateTime.Empty,
				ZDateTime.Empty,
				0m,
				ZDateTime.Empty,
				0m,
				"WEN",
				"JOE",
				new ZDateTime(2011, 1, 8),
				new ZDateTime(2011, 1, 9),
				new ZDateTime(2011, 1, 10),
				new ZDateTime(2011, 1, 11),
				0,
				6.78m,
				new ZDateTime(2011, 1, 12),
				new ZDateTime(2011, 1, 13),
				new ZDateTime(2011, 1, 14),
				new ZDateTime(2011, 1, 15),
				new ZDateTime(2011, 1, 16),
				TimeSpan.FromDays(16),
				89.65m,
				TimeSpan.FromDays(17),
				15.98m,
				ZBool.True,
				ZBool.False,
				ZBool.True,
				ZBool.False,
				"JAY",
				8,
				7,
				65.43m,
				5,
				4,
				32.10m);
			AssertEquals("docsAndCartage.JP_FCLPickupEquipmentNeeded", ZString.Empty, docsAndCartage.JP_FCLPickupEquipmentNeeded);
			AssertEquals("docsAndCartage.JP_CustomDecimal1", 1.3m, docsAndCartage.JP_CustomDecimal1);
			AssertEquals("docsAndCartage.JP_OA_DeliveryCartageCoAddr", org2.MainAddress.PK, docsAndCartage.JP_OA_DeliveryCartageCoAddr);
			AssertEquals("docsAndCartage.JP_OA_PickupCartageCoAddr", org1.MainAddress.PK, docsAndCartage.JP_OA_PickupCartageCoAddr);

			firstDateField = declaration.GetUserDefinedValue<ZDateTime>("First Date");
			AssertEquals("firstDateField", new ZDateTime(2011, 1, 1), firstDateField);
			note = declaration.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);
			AssertContents(note[0]);

			declaration.Transports.Load();
			AssertEquals("declaration.Transports.Count", 1, declaration.Transports.Count);
			AssertContents(declaration.Transports[0]);
		}

		public void TestDeclarationDefaultingBehaviour()
		{
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			var consignorPickupAddress = consignor.Addresses.AddNew();
			consignorPickupAddress.OA_Address1 = "PICKUP 1";
			consignorPickupAddress.AddAddressType(OrgAddressType.Pickup);
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consigneeDeliveryAddress = consignee.Addresses.AddNew();
			consigneeDeliveryAddress.OA_Address1 = "DELIERY 1";
			consigneeDeliveryAddress.AddAddressType(OrgAddressType.Delivery);
			var buyerSupplierLink = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			buyerSupplierLink.OL_RX_NKDefaultCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var linkTrnMode = buyerSupplierLink.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_RL_NKDischargePort = "USCHI";
			linkTrnMode.PF_RL_NKLoadPort = "AUSYD";
			linkTrnMode.PF_RL_NKPlaceOfDeliveryPort = "USNYC";
			linkTrnMode.PF_RL_NKPlaceOfReceivalPort = "AUBNE";
			linkTrnMode.PF_RS_NKDefaultServiceLevel = "ABC";
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.LCL;
			linkTrnMode.PF_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			var carrier = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			carrier.OH_IsShippingLine = ZBool.True;
			linkTrnMode.PF_OH_CarrierLine = carrier.PK;
			Factory.SaveForTesting();
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var declarationDataObject = SetupDeclaration("OWNER123", "MB1HB1", new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House });
			var masterBillDataObject = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
			var masterBillHouseBillDataObject = SetupAdditionalBill("MB1HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
			declarationDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			declarationDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
			declarationDataObject.SetDateCollection(() => new List<Date>(new[]
			{
				Date.New(DateType.Departure, ZBool.True, new ZDateTime(2012, 4, 5, 1, 2, 3)),
				Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2012, 4, 6, 2, 3, 4)),
				Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2012, 5, 6, 3, 4, 5)),
				Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2012, 5, 7, 4, 5, 6)),
				Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2012, 5, 8, 5, 6, 7))
			}));

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);
			Factory.SaveAtEndOfImport(logger);
			CombineAssertions(delegate
			{
				AssertContents(declarationBO, JobMessageTypeList.Codes.Import, Core.Constants.ContainerModes.Containerised, "RAT HATS", "NZDUD",
					"NZCHC", "", "AUSYD", "AUBDG", "BUNGA DELIMA",
					"343L", 0, 45, "KEG", 23.45m,
					"CF", 34.56m, "KT", Core.Constants.TransportModes.Sea, "OWNER123",
					"MB1", "MB1HB1", "", ServiceLevel1.RS_Code, ZBool.True,
					"EFT", OrgConstants.MergeInvoiceLines.Tariff, 112, 306, "7819369",
					"SP", "AGREF123", "F234", PaymentPartyCodeDescriptionList.Codes.Broker, MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, "FOB",
					789.012m);
				AssertEquals("JE_OH_Supplier", consignor.PK, declarationBO.JE_OH_Supplier);
				AssertEquals("JE_OH_Importer", consignee.PK, declarationBO.JE_OH_Importer);
				AssertEquals("JE_DateAtOrigin", new ZDateTime(2012, 4, 5, 1, 2, 3), declarationBO.JE_DateAtOrigin);
				AssertEquals("JE_ExportDate", new ZDateTime(2012, 4, 6, 2, 3, 4), declarationBO.JE_ExportDate);
				AssertEquals("JE_DateOfFirstArrival", new ZDateTime(2012, 5, 6, 3, 4, 5), declarationBO.JE_DateOfFirstArrival);
				AssertEquals("JE_DateOfArrival", new ZDateTime(2012, 5, 7, 4, 5, 6), declarationBO.JE_DateOfArrival);
				AssertEquals("JE_DateAtFinalDestination", new ZDateTime(2012, 5, 8, 5, 6, 7), declarationBO.JE_DateAtFinalDestination);
				AssertEquals("JE_OH_ShippingLine", ZGuid.Empty, declarationBO.JE_OH_ShippingLine);
				AssertEquals("declarationBO.DocAddresses.Count", 5, declarationBO.DocAddresses.Count);
			});

			declarationBO.Delete();

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				AssertContents(declarationBO, JobMessageTypeList.Codes.Import, Core.Constants.ContainerModes.Containerised, "RAT HATS", "AUBNE",
					"AUSYD", "", "USCHI", "AUBDG", "BUNGA DELIMA",
					"343L", 0, 45, "KEG", 23.45m,
					"CF", 34.56m, "KT", Core.Constants.TransportModes.Sea, "OWNER123",
					"MB1", "MB1HB1", "", "ABC", ZBool.True,
					"EFT", OrgConstants.MergeInvoiceLines.Tariff, 112, 306, "7819369",
					"SP", "AGREF123", "F234", PaymentPartyCodeDescriptionList.Codes.Broker, MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, "FOB",
					789.012m);
				AssertEquals("JE_OH_Supplier", consignor.PK, declarationBO.JE_OH_Supplier);
				AssertEquals("JE_OH_Importer", consignee.PK, declarationBO.JE_OH_Importer);
				AssertEquals("JE_DateAtOrigin", new ZDateTime(2012, 4, 5, 1, 2, 3), declarationBO.JE_DateAtOrigin);
				AssertEquals("JE_ExportDate", new ZDateTime(2012, 4, 6, 2, 3, 4), declarationBO.JE_ExportDate);
				AssertEquals("JE_DateOfFirstArrival", new ZDateTime(2012, 5, 6, 3, 4, 5), declarationBO.JE_DateOfFirstArrival);
				AssertEquals("JE_DateOfArrival", new ZDateTime(2012, 5, 7, 4, 5, 6), declarationBO.JE_DateOfArrival);
				AssertEquals("JE_DateAtFinalDestination", new ZDateTime(2012, 5, 8, 5, 6, 7), declarationBO.JE_DateAtFinalDestination);
				AssertEquals("JE_OH_ShippingLine", carrier.PK, declarationBO.JE_OH_ShippingLine);
				AssertEquals("declarationBO.DocAddresses.Count, plus WarehouseDocAddress", 8, declarationBO.DocAddresses.Count);
				var jobDocAddressBOSupplier = declarationBO.DocAddresses.FindByDocAddressType(DocAddressType.SupplierDocumentaryAddress);
				var jobDocAddressBOSupplierPickup = declarationBO.DocAddresses.FindByDocAddressType(DocAddressType.SupplierPickupDeliveryAddress);
				var jobDocAddressBOImporter = declarationBO.DocAddresses.FindByDocAddressType(DocAddressType.ImporterDocumentaryAddress);
				var jobDocAddressBOImporterDelivery = declarationBO.DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress);
				AssertEquals("jobDocAddressBOSupplier.IsEmpty", false, jobDocAddressBOSupplier.IsEmpty);
				AssertEquals("jobDocAddressBOSupplier.E2_OA_Address", consignor.MainAddress.PK, jobDocAddressBOSupplier.E2_OA_Address);
				AssertEquals("jobDocAddressBOSupplierPickup.IsEmpty", false, jobDocAddressBOSupplierPickup.IsEmpty);
				AssertEquals("jobDocAddressBOSupplierPickup.E2_OA_Address", consignorPickupAddress.PK, jobDocAddressBOSupplierPickup.E2_OA_Address);
				AssertEquals("jobDocAddressBOImporter.IsEmpty", false, jobDocAddressBOImporter.IsEmpty);
				AssertEquals("jobDocAddressBOImporter.E2_OA_Address", consignee.MainAddress.PK, jobDocAddressBOImporter.E2_OA_Address);
				AssertEquals("jobDocAddressBOImporterDelivery.IsEmpty", false, jobDocAddressBOImporterDelivery.IsEmpty);
				AssertEquals("jobDocAddressBOImporterDelivery.E2_OA_Address", consigneeDeliveryAddress.PK, jobDocAddressBOImporterDelivery.E2_OA_Address);
			});
		}

		public void TestEmptyContainerModePopulatesInDeclaration()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CustomsContainerMode = new ContainerMode
				{
					Code = ""
				},
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				WayBillNumber = "OB323",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			};
			shipmentData.SetContainerCollection(() => new DataObjectList<Container>(new[]
			{
				new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "" }
			}));

			var reader = new JobDeclarationDataObjectReader(shipmentData, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("If UXML specifically sets ContainerMode to empty, that is what should be set on the declaration", ZString.Empty, declarationBO.ContainerMode);
		}

		public void TestExistingContainerModeInDeclarationIsNotOverridden()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				WayBillNumber = "MYMASTER",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			};

			shipmentData.SetContainerCollection(() => new DataObjectList<Container>(new[]
			{
				new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "" }
			}));

			var reader = new JobDeclarationDataObjectReader(shipmentData, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Do not override existing declaration Container Mode", "LCL", declarationBO.ContainerMode);
		}

		public void TestCustomFieldsOnDeclarationAreImported()
		{
			#region Setup Template

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "BRK";
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_SubType1 = "SEA";
			processTaskTemplate.P0_SubType2 = "IMP";

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnBool2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool2.XC_Name = "Flag This!";
			genCustomColumnBool2.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool2);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			Factory.SaveForTesting();

			#endregion

			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = System.Guid.Empty;
			registryInstance.ShipmentCustomDate1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg First Date", ""));
			registryInstance.ShipmentCustomDate2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg LAst Date", ""));
			registryInstance.ShipmentCustomDecimalNo1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Deci Deca", ""));
			registryInstance.ShipmentCustomDecimalNo2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg + 1 point zero", ""));
			registryInstance.ShipmentCustomFlag1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Flag this!", ""));
			registryInstance.ShipmentCustomFlag2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Flagger", ""));
			registryInstance.ShipmentCustomText1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg TeXtual context", ""));
			registryInstance.ShipmentCustomText2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Customs are customary", ""));

			var declarationDataObject = SetupDeclaration(null, null, null);
			declarationDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			// Registry Fields
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Textual context", new ZString("HI PEOPLE THIS MESSAGE IS TOO LONG")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Customs are customary", new ZString("HOWDY")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg First Date", new ZDateTime(2012, 1, 1)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Last Date", new ZDateTime(2012, 1, 2)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Deci Deca", new ZDecimal(1.3)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg + 1 point zero", new ZDecimal(9999999999.99)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Flag this!", new ZString("I am NOT a BOOLEAN!!")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Flagger", ZBool.True));

			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO").PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1')));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are cUstomary", new ZString("GOODBYE")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last DatE", new ZDateTime(2011, 1, 2)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(1.3)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			declarationDataObject.CustomizedFieldCollection.Add(incorrectCustomField);
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integraler", ZInt.Zero));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				var docsAndCartage = declarationBO.DocsAndCartage;
				AssertEquals("docsAndCartage.JP_CustomAttrib1", "HI PEOPLE THIS", docsAndCartage.JP_CustomAttrib1);
				AssertEquals("docsAndCartage.JP_CustomAttrib2", "HOWDY", docsAndCartage.JP_CustomAttrib2);
				AssertEquals("docsAndCartage.JP_CustomDate1", new ZDateTime(2012, 1, 1), docsAndCartage.JP_CustomDate1);
				AssertEquals("docsAndCartage.JP_CustomDate2", new ZDateTime(2012, 1, 2), docsAndCartage.JP_CustomDate2);
				AssertEquals("docsAndCartage.JP_CustomDecimal1", 1.3m, docsAndCartage.JP_CustomDecimal1);
				AssertEquals("docsAndCartage.JP_CustomDecimal2", 999999999m, docsAndCartage.JP_CustomDecimal2);
				AssertEquals("docsAndCartage.JP_CustomFlag2", true, docsAndCartage.JP_CustomFlag2);

				var customFields = declarationBO.GetUserDefinedValues();
				var result = "";
				foreach (var customField in customFields)
				{
					result += customField.PropertyName + " - " + customField.Value + "\r\n";
				}

				AssertMultilineASCIIEquals("All Custom Fields should have been imported with none extra", string.Format(@"
+ 1 point zero - 1.3
Bogus Custom Field - I am BOGUS
Customs are cUstomary - GOODBYE
Deci Deca - 0.3
First Date - 01-Jan-11 00:00:00
Flagger - Y
Integer Mate - 42
Last DatE - 02-Jan-11 00:00:00
Textual context - {0}
			", "HELLO".PadRight(100, '1')).Trim(), result);

				AssertMultilineASCIIEquals("logger.Logs", string.Format(@"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Warning - Attempted to insert 34 characters into Field [JP_CustomAttrib1] which has a maximum length of 15 characters. Field was truncated.
Warning - Attempted to insert '9999999999.99' into Field [JP_CustomDecimal2] which has a maximum numeric value of '999999999'. Field was truncated to the max value.
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!!]. Value must be a valid Boolean (true or false).
Warning - Attempted to insert 101 characters into Field [Textual context] which has a maximum length of 100 characters. Field was truncated.
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - Added Declaration from UniversalShipment.
", GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, GenCustomAddOnValueSchema.XV_Data.MaxLength).Trim(), Logger.Logs);
			});
		}

		public void TestCustomFieldsOnDeclarationAreImported_FromFreight()
		{
			#region Setup Template

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "BRK";
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_SubType1 = "SEA";
			processTaskTemplate.P0_SubType2 = "IMP";

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnBool2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool2.XC_Name = "Flag This!";
			genCustomColumnBool2.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool2);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			Factory.SaveForTesting();

			#endregion

			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = System.Guid.Empty;
			registryInstance.ShipmentCustomDate1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg First Date", ""));
			registryInstance.ShipmentCustomDate2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg LAst Date", ""));
			registryInstance.ShipmentCustomDecimalNo1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Deci Deca", ""));
			registryInstance.ShipmentCustomDecimalNo2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg + 1 point zero", ""));
			registryInstance.ShipmentCustomFlag1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Flag this!", ""));
			registryInstance.ShipmentCustomFlag2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Flagger", ""));
			registryInstance.ShipmentCustomText1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg TeXtual context", ""));
			registryInstance.ShipmentCustomText2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Reg Customs are customary", ""));

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, null);
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001387");

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext
			};

			var declarationDataObject = SetupDeclaration(null, null, null);
			dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001387");

			declarationDataObject.DataContext = dataContext;
			declarationDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			// Registry Fields
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Textual context", new ZString("HI PEOPLE THIS MESSAGE IS TOO LONG")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Customs are customary", new ZString("HOWDY")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg First Date", new ZDateTime(2012, 1, 1)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Last Date", new ZDateTime(2012, 1, 2)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Deci Deca", new ZDecimal(1.3)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg + 1 point zero", new ZDecimal(9999999999.99)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Flag this!", new ZString("I am NOT a BOOLEAN!!")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Reg Flagger", ZBool.True));

			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO").PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1')));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are cUstomary", new ZString("GOODBYE")));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last DatE", new ZDateTime(2011, 1, 2)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(1.3)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			declarationDataObject.CustomizedFieldCollection.Add(incorrectCustomField);
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integraler", ZInt.Zero));
			declarationDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { declarationDataObject });

			var reader = new JobDeclarationDataObjectReader(shipment, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				var docsAndCartage = declarationBO.DocsAndCartage;
				AssertEquals("docsAndCartage.JP_CustomAttrib1", "HI PEOPLE THIS", docsAndCartage.JP_CustomAttrib1);
				AssertEquals("docsAndCartage.JP_CustomAttrib2", "HOWDY", docsAndCartage.JP_CustomAttrib2);
				AssertEquals("docsAndCartage.JP_CustomDate1", new ZDateTime(2012, 1, 1), docsAndCartage.JP_CustomDate1);
				AssertEquals("docsAndCartage.JP_CustomDate2", new ZDateTime(2012, 1, 2), docsAndCartage.JP_CustomDate2);
				AssertEquals("docsAndCartage.JP_CustomDecimal1", 1.3m, docsAndCartage.JP_CustomDecimal1);
				AssertEquals("docsAndCartage.JP_CustomDecimal2", 999999999m, docsAndCartage.JP_CustomDecimal2);
				AssertEquals("docsAndCartage.JP_CustomFlag2", true, docsAndCartage.JP_CustomFlag2);

				var customFields = declarationBO.GetUserDefinedValues();
				var result = "";
				foreach (var customField in customFields)
				{
					result += customField.PropertyName + " - " + customField.Value + "\r\n";
				}

				AssertMultilineASCIIEquals("All Custom Fields should have been imported with none extra", string.Format(@"
+ 1 point zero - 1.3
Bogus Custom Field - I am BOGUS
Customs are cUstomary - GOODBYE
Deci Deca - 0.3
First Date - 01-Jan-11 00:00:00
Flagger - Y
Integer Mate - 42
Last DatE - 02-Jan-11 00:00:00
Textual context - {0}
			", "HELLO".PadRight(100, '1')).Trim(), result);

				AssertMultilineASCIIEquals("logger.Logs", string.Format(@"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Warning - Attempted to insert 34 characters into Field [JP_CustomAttrib1] which has a maximum length of 15 characters. Field was truncated.
Warning - Attempted to insert '9999999999.99' into Field [JP_CustomDecimal2] which has a maximum numeric value of '999999999'. Field was truncated to the max value.
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!!]. Value must be a valid Boolean (true or false).
Warning - Attempted to insert 101 characters into Field [Textual context] which has a maximum length of 100 characters. Field was truncated.
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - Added Declaration from UniversalShipment.
", GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, GenCustomAddOnValueSchema.XV_Data.MaxLength).Trim(), Logger.Logs);
			});
		}

		public void TestLocalProcessingOnDeclarationAreImported()
		{
			var declarationDataObject = SetupDeclaration(null, null, null);
			declarationDataObject.LocalProcessing = LocalProcessingDataObjectReaderTest.SetupLocalProcessing(new CodeDescriptionPair() { Code = "BOB", Description = "THE BUILDER" }, new CodeDescriptionPair() { Code = "WEN", Description = "THE DESTROYER" }, new CodeDescriptionPair() { Code = "JOE", Description = "THE PEACEMAKER" }, new CodeDescriptionPair() { Code = "JAY", Description = "THE LAZY" });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				var docsAndCartage = declarationBO.DocsAndCartage;
				LocalProcessingDataObjectReaderTest.AssertContents(
				docsAndCartage,
				"",
				ZDateTime.Empty,
				ZDateTime.Empty,
				ZDateTime.Empty,
				ZDateTime.Empty,
				"ARRCARREF1",
				ZDateTime.Empty,
				ZDateTime.Empty,
				0m,
				ZDateTime.Empty,
				0m,
				"WEN",
				"JOE",
				new ZDateTime(2011, 1, 8),
				new ZDateTime(2011, 1, 9),
				new ZDateTime(2011, 1, 10),
				new ZDateTime(2011, 1, 11),
				0,
				6.78m,
				new ZDateTime(2011, 1, 12),
				new ZDateTime(2011, 1, 13),
				new ZDateTime(2011, 1, 14),
				new ZDateTime(2011, 1, 15),
				new ZDateTime(2011, 1, 16),
				TimeSpan.FromDays(16),
				89.65m,
				TimeSpan.FromDays(17),
				15.98m,
				ZBool.True,
				ZBool.False,
				ZBool.True,
				ZBool.False,
				"JAY",
				8,
				7,
				65.43m,
				5,
				4,
				32.10m);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Added Declaration from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLocalProcessingOnDeclarationAreImported_FromFreight()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, null);
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001387");

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext
			};

			var declarationDataObject = SetupDeclaration(null, null, null);
			dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001387");

			declarationDataObject.DataContext = dataContext;

			declarationDataObject.LocalProcessing = LocalProcessingDataObjectReaderTest.SetupLocalProcessing(new CodeDescriptionPair() { Code = "BOB", Description = "THE BUILDER" }, new CodeDescriptionPair() { Code = "WEN", Description = "THE DESTROYER" }, new CodeDescriptionPair() { Code = "JOE", Description = "THE PEACEMAKER" }, new CodeDescriptionPair() { Code = "JAY", Description = "THE LAZY" });

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { declarationDataObject });

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				var docsAndCartage = declarationBO.DocsAndCartage;
				LocalProcessingDataObjectReaderTest.AssertContents(
				docsAndCartage,
				"",
				ZDateTime.Empty,
				ZDateTime.Empty,
				ZDateTime.Empty,
				ZDateTime.Empty,
				"ARRCARREF1",
				ZDateTime.Empty,
				ZDateTime.Empty,
				0m,
				ZDateTime.Empty,
				0m,
				"WEN",
				"JOE",
				new ZDateTime(2011, 1, 8),
				new ZDateTime(2011, 1, 9),
				new ZDateTime(2011, 1, 10),
				new ZDateTime(2011, 1, 11),
				0,
				6.78m,
				new ZDateTime(2011, 1, 12),
				new ZDateTime(2011, 1, 13),
				new ZDateTime(2011, 1, 14),
				new ZDateTime(2011, 1, 15),
				new ZDateTime(2011, 1, 16),
				TimeSpan.FromDays(16),
				89.65m,
				TimeSpan.FromDays(17),
				15.98m,
				ZBool.True,
				ZBool.False,
				ZBool.True,
				ZBool.False,
				"JAY",
				8,
				7,
				65.43m,
				5,
				4,
				32.10m);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Added Declaration from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLocalProcessing_AdditionalServicesDefault()
		{
			var declarationDataObject = SetupDeclaration(null, null, null);
			declarationDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			declarationDataObject.LocalProcessing.SetAdditionalServiceCollection(() => new DataObjectList<AdditionalService>(new[] { new AdditionalService() { ServiceCode = new CodeDescriptionPair() { Code = "ABC" } } }));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			declarationDataObject.DataContext.DataTargetCollection.First().Key = declarationBO.JE_DeclarationReference;
			declarationDataObject.LocalProcessing.AdditionalServiceCollection.Clear();
			declarationDataObject.LocalProcessing.AdditionalServiceCollection.Add(new AdditionalService() { ServiceCode = new CodeDescriptionPair() { Code = "XYZ" } });
			declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals("The existing services should be combined with imported ones by default", 2, declarationBO.DocsAndCartage.Services.Count);
		}

		public void TestEmptyMasterBill()
		{
			var declarationDataObject = SetupDeclaration(null, "", new WayBillType() { Code = "MWB", Description = "Master Waybill" });

			var declarationWithEmptyMasterBill = Factory.New<BaseJobDeclaration>();
			declarationWithEmptyMasterBill.JE_MasterBill = "";
			declarationWithEmptyMasterBill.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);
			AssertNotEquals("New declaration was created", declarationWithEmptyMasterBill.PK, declarationBO.PK);
		}

		public void TestAdditionalReferenceNumbers()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);

			declarationDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>(new[] { SetupAdditionalReference(), SetupAdditionalReference2() }));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				AssertEquals("declarationBO.AdditionalReferenceNumbers.Count", 2, declarationBO.AdditionalReferenceNumbers.Count);

				AssertAdditionalReferenceNumberContents(declarationBO.AdditionalReferenceNumbers[0]);
				AssertAdditionalReferenceNumberContents2(declarationBO.AdditionalReferenceNumbers[1]);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
				".Trim(), logger.Logs);
			});

			var newLogger = new TestErrorLogger();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, newLogger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				AssertEquals("declarationBO.AdditionalReferenceNumbers.Count", 2, declarationBO.AdditionalReferenceNumbers.Count);

				AssertAdditionalReferenceNumberContents(declarationBO.AdditionalReferenceNumbers[0]);
				AssertAdditionalReferenceNumberContents2(declarationBO.AdditionalReferenceNumbers[1]);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestInvalidMAWBRecyclePeriodWillMatchAll()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>(new[] { SetupAdditionalReference() }));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declaration, declarationBO);
		}

		public void TestDeletingExistingAdditionalReferenceNumbersNotInXML()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var additionalReferenceNumber1 = declarationBOToLoad.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber1.CE_EntryType = "AKL";
			additionalReferenceNumber1.CE_EntryNum = "340234";
			additionalReferenceNumber1.CE_EntryIsSystemGenerated = true;
			var additionalReferenceNumber2 = declarationBOToLoad.AdditionalReferenceNumbers.AddNew();
			additionalReferenceNumber2.CE_EntryType = "AMS";
			additionalReferenceNumber2.CE_EntryNum = "D3234dd";
			additionalReferenceNumber2.CE_EntryIsSystemGenerated = true;

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>(new[] { SetupAdditionalReference() }));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);

			CombineAssertions(delegate
			{
				AssertEquals("additionalReferenceNumber1.IsDeleted", true, additionalReferenceNumber1.IsDeleted);
				AssertEquals("additionalReferenceNumber2.IsDeleted", false, additionalReferenceNumber2.IsDeleted);
				AssertEquals("declarationBO.AdditionalReferenceNumbers.Count", 1, declarationBO.AdditionalReferenceNumbers.Count);
				AssertEquals(additionalReferenceNumber2, declarationBO.AdditionalReferenceNumbers[0]);
				AssertAdditionalReferenceNumberContents(additionalReferenceNumber2);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestWithTransportLegs()
		{
			var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { SetupTransportLeg(), SetupTransportLeg2() }));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			#region Check Contents of declaration Business Object

			declarationBO.Transports.Load();
			AssertEquals("declarationBO.Transports.Count", 2, declarationBO.Transports.Count);

			CombineAssertions(delegate
			{
				AssertContents(declarationBO, null, null, "MYHOUSE");
				AssertContents(declarationBO.Transports[0]);
				AssertContents2(declarationBO.Transports[1]);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUMEL Destination: AUSYD
Information - Attempting to get Schedule for the Transport Leg
Information - A Schedule has been found and linked to the Transport Leg.
Warning - Matching 'LEG2SMITH':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestDeletingExistingTransportLegsNotInXML()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var transport1 = declarationBOToLoad.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport1.JW_Vessel = "VESSEL 1";
			var transport2 = declarationBOToLoad.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_Vessel = "VESSEL 2";

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { SetupTransportLeg() }));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("transport1.IsDeleted", true, transport1.IsDeleted);
				AssertEquals("transport2.IsDeleted", false, transport2.IsDeleted);
				AssertContents(declarationBO, null, "MYMASTER", null);
				AssertEquals("declarationBO.Transports.Count", 1, declarationBO.Transports.Count);
				AssertEquals(transport2, declarationBO.Transports[0]);
				AssertContents(transport2);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestDoesNotDeleteExistingTransportLegsNotInXMLWithPartialAttribute()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var transport1 = declarationBOToLoad.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport1.JW_Vessel = "VESSEL 1";
			var transport2 = declarationBOToLoad.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "NZAKL";
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_Vessel = "VESSEL 2";

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { SetupTransportLeg() }));
			declarationDataObject.TransportLegCollection.Content = CollectionContent.Partial;
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("transport1.IsDeleted", false, transport1.IsDeleted);
				AssertEquals("transport2.IsDeleted", false, transport2.IsDeleted);
				AssertContents(declarationBO, null, "MYMASTER", null);
				AssertEquals("declarationBO.Transports.Count", 2, declarationBO.Transports.Count);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithDates()
		{
			var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			declarationDataObject.SetDateCollection(() => new List<Date>());
			declarationDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2010, 1, 3)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.True, new ZDateTime(2010, 1, 4)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2010, 1, 5)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.EntryDate, ZBool.False, new ZDateTime(2010, 1, 6)));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertContents(declarationBO, null, null, "MYHOUSE");
				AssertEquals("declarationBO.JE_ExportDate", new ZDateTime(2010, 1, 3), declarationBO.JE_ExportDate);
				AssertEquals("declarationBO.JE_DateOfFirstArrival", new ZDateTime(2010, 1, 4), declarationBO.JE_DateOfFirstArrival);
				AssertEquals("declarationBO.JE_DateOfArrival", new ZDateTime(2010, 1, 5), declarationBO.JE_DateOfArrival);
				AssertEquals("declarationBO.JE_EntryDate", new ZDateTime(2010, 1, 6), declarationBO.JE_EntryDate);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithNotes()
		{
			var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>(new[] { SetupNote(), SetupNote2() }));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			#region Check Contents of declaration Business object

			StmNote[] note = declarationBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);
			StmNote[] note2 = declarationBO.Notes.FindByDescription("WORM EATER!!");
			AssertEquals("Note Collection contains 'WORM EATER!!' note.", 1, note2.Length);

			CombineAssertions(delegate
			{
				AssertContents(declarationBO, null, null, "MYHOUSE");
				AssertContents(note[0]);
				AssertContents2(note2[0]);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: WORM EATER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestDeletingExistingNotesNotInXML()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var note1 = declarationBOToLoad.Notes.AddNew(true, "CAT FLOGGER!!", "MUEW");
			var note2 = declarationBOToLoad.Notes.AddNew(true, "DOG FLOGGER!!", "WOOPH");

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>(new[] { SetupNote() }) { Content = CollectionContent.Complete });
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("note1.IsDeleted", true, note1.IsDeleted);
				AssertEquals("note2.IsDeleted", false, note2.IsDeleted);
				AssertContents(declarationBO, null, "MYMASTER", null);

				StmNote[] note = declarationBO.Notes.FindByDescription("DOG FLOGGER!!");
				AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);
				AssertEquals(note2, note[0]);
				AssertContents(note2);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching StmNote.
Information - Populating StmNote...
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestKeepingExistingNotesNotInXML()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var note1 = declarationBOToLoad.Notes.AddNew(true, "CAT FLOGGER!!", "MUEW");
			var note2 = declarationBOToLoad.Notes.AddNew(true, "DOG FLOGGER!!", "WOOPH");

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>(new[] { SetupNote() }) { Content = CollectionContent.Partial });
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("note1.IsDeleted", false, note1.IsDeleted);
				AssertEquals("note2.IsDeleted", false, note2.IsDeleted);
				AssertContents(declarationBO, null, "MYMASTER", null);

				StmNote[] note = declarationBO.Notes.FindByDescription("DOG FLOGGER!!");
				AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);
				AssertEquals(note2, note[0]);
				AssertContents(note2);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching StmNote.
Information - Populating StmNote...
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestWithOrgAddresses()
		{
			var matchingOrgAddressDataObject = SetupAddressData_INTHEMSYD(nameof(DocAddressType.ImporterDocumentaryAddress));
			var newOrgAddressDataObject = SetupAddressData_INTHEMSYD(nameof(DocAddressType.SupplierDocumentaryAddress));
			var unknownOrgAddressDataObject = SetupAddressData_INTHEMSYD(nameof(DocAddressType.ShipToParty));
			var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { matchingOrgAddressDataObject, newOrgAddressDataObject, unknownOrgAddressDataObject }));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			#region Check Contents of declaration Business object

			var jobDocAddressBOImporter = declarationBO.DocAddresses.FindByDocAddressType(DocAddressType.ImporterDocumentaryAddress);
			var jobDocAddressBOSupplier = declarationBO.DocAddresses.FindByDocAddressType(DocAddressType.SupplierDocumentaryAddress);
			var jobDocAddressBOShipToParty = declarationBO.DocAddresses.FindByDocAddressType(DocAddressType.ShipToParty);

			CombineAssertions(delegate
			{
				AssertContents(declarationBO, null, null, "MYHOUSE");
				AssertContentsMatches_INTHEMSYD(jobDocAddressBOImporter, DocAddressTypes.Codes.ImporterDocumentaryAddress);
				AssertContentsMatches_INTHEMSYD(jobDocAddressBOSupplier, DocAddressTypes.Codes.SupplierDocumentaryAddress);
				AssertNull(jobDocAddressBOShipToParty);
				AssertNull("Organisation Collection without the localclient address does not add it in on the declaration business object", declarationBO.Job);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ImporterDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ImporterDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ShipToParty':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ImporterDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ShipToParty':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Unknown Address Type [ShipToParty] found. Job Document Address not imported.
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion

			var orgAddressDataObject1 = SetupAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var orgAddressDataObject2 = SetupAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "INTHEMSYD";
			org.OH_IsConsignee = true;
			org.OH_IsConsignor = true;
			org.OH_FullName = "In The Moment";
			org.MainAddress.OA_Address1 = "Unit 12, Level 3";
			org.MainAddress.OA_Address2 = "233 Here St";
			org.MainAddress.OA_City = "ThereVille";
			org.MainAddress.OA_State = "OfBliss";
			org.MainAddress.OA_PostCode = "1233";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Starshine Moonbeam";
			contact.OC_Email = "s.m@moment.com.au";
			contact.OC_Fax = "234098234";
			contact.OC_Mobile = "234098293";
			contact.OC_Phone = "1239813209";
			org.CustomsCodes.AddNew("GST", "55555");
			Factory.SaveForTesting();

			declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { orgAddressDataObject1, orgAddressDataObject2 }));

			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(orgAddressDataObject1.CompanyName, declarationBO.Importer.OH_FullName);
			AssertEquals(orgAddressDataObject2.CompanyName, declarationBO.Supplier.OH_FullName);
		}

		public void TestWithLocalClientAddresses()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				var importerOrgAddressDataObject = SetupAddressData_INTHEMSYD(nameof(DocAddressType.ImporterDocumentaryAddress));
				var supplierOrgAddressDataObject = SetupAddressData_INTHEMSYD(nameof(DocAddressType.SupplierDocumentaryAddress));
				var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { importerOrgAddressDataObject, supplierOrgAddressDataObject }));
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				CombineAssertions(delegate
				{
					AssertNull("Organisation Collection without the localclient address does not add it in the declaration business object", declarationBO.Job);
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ImporterDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ImporterDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ImporterDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'SupplierDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
				});

				logger.ClearLogs();
				var localClientAddressDataObject = SetupAddressData_INTHEMSYD(nameof(DocAddressType.LocalClient));
				var declarationDataObject1 = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				declarationDataObject1.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { localClientAddressDataObject }));
				var reader1 = new JobDeclarationDataObjectReader(declarationDataObject1, logger, Factory);
				var declarationBO1 = reader1.ReadIntoBusinessObject();
				CombineAssertions(delegate
				{
					AssertNull("The localclient address NOT matched with existed Organisation will not add to the declaration business object", declarationBO1.Job);
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Warning - Matching 'LocalClient':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
				});

				logger.ClearLogs();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "INTHEMSYD";
				org.OH_IsConsignee = true;
				org.OH_IsConsignor = true;
				org.OH_FullName = "In The Moment";
				org.MainAddress.OA_Address1 = "Unit 12, Level 3";
				org.MainAddress.OA_Address2 = "233 Here St";
				org.MainAddress.OA_City = "ThereVille";
				org.MainAddress.OA_PostCode = "1233";
				org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				org.MainAddress.OA_State = "OfBliss";
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "Starshine Moonbeam";
				contact.OC_Email = "s.m@moment.com.au";
				contact.OC_Fax = "234098234";
				contact.OC_Mobile = "234098293";
				contact.OC_Phone = "1239813209";
				org.CustomsCodes.AddNew("GST", "55555");

				Factory.SaveForTesting();
				var declarationDataObject2 = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				declarationDataObject2.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { localClientAddressDataObject }));
				var reader2 = new JobDeclarationDataObjectReader(declarationDataObject2, logger, Factory);
				var declarationBO2 = reader2.ReadIntoBusinessObject();
				Factory.FireCleanupAfterSaving();
				CombineAssertions(delegate
				{
					AssertNotNull("The matched localclient address with the new JobDeclaration will add to the declaration business object", declarationBO2.Job);
					AssertEquals("JH_OA_LocalChargesAddr_ZAddress.Address", "In The Moment\r\n233 Here St ThereVille OfBliss 1233", declarationBO2.Job.JH_OA_LocalChargesAddr_ZAddress.Address.ToString());
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Matching 'LocalClient':- Matched to 'INMOMESYD' address 'Unit 12, Level 3' with a score of 390.
Information - Added Declaration from UniversalShipment.
			".Trim(), logger.Logs);
				});

				logger.ClearLogs();
				var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
				declarationBOToLoad.JE_MasterBill = "MYMASTER";
				declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				new JobHeader.Loader(declarationBOToLoad).TryLoadOrCreate();
				var declarationDataObject3 = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
				declarationDataObject3.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { localClientAddressDataObject }));
				var reader3 = new JobDeclarationDataObjectReader(declarationDataObject3, logger, Factory);
				var declarationBO3 = reader3.ReadIntoBusinessObject();
				CombineAssertions(delegate
				{
					AssertNotNull("The matched localclient address with the existed JobDeclaration will add to the declaration business object", declarationBO3.Job);
					AssertEquals("JH_OA_LocalChargesAddr_ZAddress.Address", "In The Moment\r\n233 Here St ThereVille OfBliss 1233", declarationBO3.Job.JH_OA_LocalChargesAddr_ZAddress.Address.ToString());
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Matching 'LocalClient':- Matched to 'INMOMESYD' address 'Unit 12, Level 3' with a score of 390.
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
				});
			}
		}

		public void TestBillRelationshipIsImportedCorrectly_UseDefaulting_True() => AssertBillRelationshipIsImportedCorrectly(true);
		public void TestBillRelationshipIsImportedCorrectly_UseDefaulting_False() => AssertBillRelationshipIsImportedCorrectly(false);
		void AssertBillRelationshipIsImportedCorrectly(bool useDefaulting)
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useDefaulting))
			{
				var masterBill1 = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				var masterBill2 = SetupAdditionalBill("MB2", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 4, 1), null, null, 20m, Core.Constants.PkgUnit.Package, "Package");
				var masterBill1HouseBill1 = SetupAdditionalBill("HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				var masterBill1HouseBill2 = SetupAdditionalBill("HB2", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 3), "MB1", null, 4m, Core.Constants.PkgUnit.Unit, "Unit");
				var masterBill2HouseBill1 = SetupAdditionalBill("HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 4, 2), "MB2", null, 15m, Core.Constants.PkgUnit.Unit, "Package");
				var masterBill2HouseBill2 = SetupAdditionalBill("HB2", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 4, 3), "MB2", null, 5m, Core.Constants.PkgUnit.Unit, "Package");
				var masterBill1HouseBill2SubHouse1 = SetupAdditionalBill("SB1", WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse, new ZDateTime(2011, 3, 4), "HB2", AddInfoCollectionCreator.CreateCollection(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber + "=MB1"), 4m, Core.Constants.PkgUnit.Bundle, "Bundle");
				var masterBill2HouseBill2SubHouse1 = SetupAdditionalBill("SB1", WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse, new ZDateTime(2011, 4, 3), "HB2", AddInfoCollectionCreator.CreateCollection(Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber + "=MB2"), 5m, Core.Constants.PkgUnit.Box, "BOX");

				var declarationDataObject = SetupDeclaration(null, masterBill1HouseBill2.BillNumber, masterBill1HouseBill2.BillType, AddInfoCollectionCreator.CreateCollection(Constants.AddInfoKeys.Declaration.MasterWayBillNumber + "=MB1"));
				// Add Additional Bill in a random order
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill2HouseBill2SubHouse1, masterBill2HouseBill1, masterBill1, masterBill2HouseBill2, masterBill1HouseBill2SubHouse1, masterBill1HouseBill1, masterBill1HouseBill2, masterBill2, }));
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);
				declarationBO.Bills.Load();
				var bills = declarationBO.Bills.OfType<Bill>().ToArray();
				AssertEquals("declarationBO.Bills.Count", 8, bills.Length);
				var mb1 = bills.First(x => x.CU_BillNum == "MB1" && x.CU_BillType == BillTypeList.Codes.MasterBill && x.CU_CU_ParentBill.IsEmpty);
				var mb2 = bills.First(x => x.CU_BillNum == "MB2" && x.CU_BillType == BillTypeList.Codes.MasterBill && x.CU_CU_ParentBill.IsEmpty);
				var mb1hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == BillTypeList.Codes.HouseBill && x.CU_CU_ParentBill == mb1.PK);
				var mb1hb2 = bills.First(x => x.CU_BillNum == "HB2" && x.CU_BillType == BillTypeList.Codes.HouseBill && x.CU_CU_ParentBill == mb1.PK);
				var mb2hb1 = bills.First(x => x.CU_BillNum == "HB1" && x.CU_BillType == BillTypeList.Codes.HouseBill && x.CU_CU_ParentBill == mb2.PK);
				var mb2hb2 = bills.First(x => x.CU_BillNum == "HB2" && x.CU_BillType == BillTypeList.Codes.HouseBill && x.CU_CU_ParentBill == mb2.PK);
				var mb1hb2sh1 = bills.First(x => x.CU_BillNum == "SB1" && x.CU_BillType == BillTypeList.Codes.SubHouseBill && x.CU_CU_ParentBill == mb1hb2.PK);
				var mb2hb2sh1 = bills.First(x => x.CU_BillNum == "SB1" && x.CU_BillType == BillTypeList.Codes.SubHouseBill && x.CU_CU_ParentBill == mb2hb2.PK);
				AssertEquals("MB1", declarationBO.JE_MasterBill);
				AssertEquals("HB2", declarationBO.JE_HouseBill);
				AssertEquals(mb1, declarationBO.PrimaryMasterBill);
				AssertEquals(mb1hb2, declarationBO.PrimaryHouseBill);
			}
		}

		public void TestWithBillsAndContainersAndPackLines()
		{
			var masterBill1 = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
			var masterBill2 = SetupAdditionalBill("MB2", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 4, 1), null, null, 20m, Core.Constants.PkgUnit.Package, "Package");
			var masterBill1HouseBill1 = SetupAdditionalBill("MB1HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
			var masterBill1HouseBill2 = SetupAdditionalBill("MB1HB2", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 3), "MB1", null, 4m, Core.Constants.PkgUnit.Unit, "Unit");
			var masterBill2HouseBill1 = SetupAdditionalBill("MB2HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 4, 2), "MB2", null, 20m, Core.Constants.PkgUnit.Unit, "Unit");
			var masterBill1HouseBill2SubHouse1 = SetupAdditionalBill("MB1HB2SB1", WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse, new ZDateTime(2011, 3, 4), "MB1HB2", null, 4m, Core.Constants.PkgUnit.Bundle, "Bundle");
			var masterBill2HouseBill1SubHouse1 = SetupAdditionalBill("MB2HB1SB1", WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse, new ZDateTime(2011, 4, 3), "MB2HB1", null, 20m, Core.Constants.PkgUnit.Box, "BOX");

			var containerDataObject1 = SetupContainer();
			var containerDataObject2 = SetupContainer2();
			var packingLineDataObject1 = SetupPackingLine(masterBill1HouseBill2.BillNumber, masterBill1HouseBill2.BillType, containerDataObject1.ContainerNumber);
			var packingLineDataObject2 = SetupPackingLine(masterBill1.BillNumber, masterBill1.BillType, containerDataObject2.ContainerNumber);
			var packingLineDataObject3 = SetupPackingLine2(masterBill1HouseBill2.BillNumber, masterBill1HouseBill2.BillType, containerDataObject1.ContainerNumber);
			var packingLineDataObject4 = SetupPackingLine(masterBill2HouseBill1SubHouse1.BillNumber, masterBill2HouseBill1SubHouse1.BillType, containerDataObject2.ContainerNumber, "MARKS4", 10, new PackageType() { Code = "NO", Description = "Number" }, 11, 5, "SYMBOL4");
			var packingLineDataObject5 = SetupPackingLine(new ZString("MB3"), new WayBillType() { Code = WayBillTypeList.Codes.Master }, containerDataObject2.ContainerNumber, "MARKS5", 10, new PackageType() { Code = "NO", Description = "Number" }, 11, 5, "SYMBOL5");

			var declarationDataObject = SetupDeclaration(null, masterBill1HouseBill2.BillNumber, masterBill1HouseBill2.BillType);
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject2, containerDataObject1 }));
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill1, masterBill2, masterBill1HouseBill1, masterBill1HouseBill2, masterBill2HouseBill1, masterBill1HouseBill2SubHouse1, masterBill2HouseBill1SubHouse1 }));
			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject1, packingLineDataObject2, packingLineDataObject3, packingLineDataObject4, packingLineDataObject5 }));
			declarationDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);

			AssertNotNull(declarationBO);

			#region Check Contents of declaration Business object

			declarationBO.CusContainers.Load();
			AssertEquals("declarationBO.CusContainers.Count", 2, declarationBO.CusContainers.Count);
			declarationBO.Bills.Load();
			AssertEquals("declarationBO.Bills.Count", 8, declarationBO.Bills.Count);
			declarationBO.Packages.Load();
			AssertEquals("declarationBO.Packages.Count", 5, declarationBO.Packages.Count);
			declarationBO.PackingGroups.Load();
			AssertEquals("declarationBO.Bills.Count", 4, declarationBO.PackingGroups.Count);
			declarationBO.Bills.Sort(Bill.Schema.CU_BillNum);

			CombineAssertions(delegate
			{
				AssertContents(declarationBO, null, "MB1", "MB1HB2", 2);
				var containerNumber1 = containerDataObject1.ContainerNumber.GetValueOrDefault();
				var containerNumber2 = containerDataObject2.ContainerNumber.GetValueOrDefault();
				var container1 = declarationBO.CusContainers[0];
				var container2 = declarationBO.CusContainers[1];
				if (container2.CO_ContainerNumber == containerNumber1)
				{
					container1 = declarationBO.CusContainers[1];
					container2 = declarationBO.CusContainers[0];
				}
				AssertContents(container1);
				AssertContents2(container2);
				var bill1 = declarationBO.Bills[0];
				AssertContents(bill1, "MB1", BillTypeList.Codes.MasterBill, ZGuid.Empty, new ZDateTime(2011, 3, 1), 10m, Core.Constants.PkgUnit.Box, "");
				var bill2 = declarationBO.Bills[1];
				AssertContents(bill2, "MB1HB1", BillTypeList.Codes.HouseBill, declarationBO.Bills[0].PK, new ZDateTime(2011, 3, 2), 6m, Core.Constants.PkgUnit.Piece, "");
				var bill3 = declarationBO.Bills[2];
				AssertContents(bill3, "MB1HB2", BillTypeList.Codes.HouseBill, declarationBO.Bills[0].PK, new ZDateTime(2011, 3, 3), 4m, Core.Constants.PkgUnit.Unit, "");
				var bill4 = declarationBO.Bills[3];
				AssertContents(bill4, "MB1HB2SB1", BillTypeList.Codes.SubHouseBill, declarationBO.Bills[2].PK, new ZDateTime(2011, 3, 4), 4m, Core.Constants.PkgUnit.Bundle, "");
				var bill5 = declarationBO.Bills[4];
				AssertContents(bill5, "MB2", BillTypeList.Codes.MasterBill, ZGuid.Empty, new ZDateTime(2011, 4, 1), 20m, Core.Constants.PkgUnit.Package, "");
				var bill6 = declarationBO.Bills[5];
				AssertContents(bill6, "MB2HB1", BillTypeList.Codes.HouseBill, declarationBO.Bills[4].PK, new ZDateTime(2011, 4, 2), 20m, Core.Constants.PkgUnit.Unit, "");
				var bill7 = declarationBO.Bills[6];
				AssertContents(bill7, "MB2HB1SB1", BillTypeList.Codes.SubHouseBill, declarationBO.Bills[5].PK, new ZDateTime(2011, 4, 3), 20m, Core.Constants.PkgUnit.Box, "");
				var bill8 = declarationBO.Bills[7];
				AssertContents(bill8, "MB3", BillTypeList.Codes.MasterBill, ZGuid.Empty, ZDateTime.Empty, ZDecimal.Zero, "", "");

				var packages = new List<BasePackage>(new TypedEnumerable<BasePackage>(declarationBO.Packages));
				var package = declarationBO.Packages.GetElementWithHouseBillAndContainer(bill1.CU_BillUniqueCode, containerNumber2);
				AssertContents(package, bill1.CU_BillUniqueCode, containerNumber2);
				packages.Remove(package);
				package = declarationBO.Packages.GetElementWithHouseBillAndContainer(bill7.CU_BillUniqueCode, containerNumber2);
				AssertContents(package, bill7.CU_BillUniqueCode, containerNumber2, "MARKS4", 10, "NO", 11, 5, "SYMBOL4");
				packages.Remove(package);
				package = declarationBO.Packages.GetElementWithHouseBillAndContainer(bill8.CU_BillUniqueCode, containerNumber2);
				AssertContents(package, bill8.CU_BillUniqueCode, containerNumber2, "MARKS5", 10, "NO", 11, 5, "SYMBOL5");
				packages.Remove(package);
				AssertEquals(2, packages.Count);
				var package1 = packages[0];
				var package2 = packages[1];
				if (package2.CW_ShippingSymbol == packingLineDataObject1.ShippingSymbol.GetValueOrDefault())
				{
					package1 = packages[1];
					package2 = packages[0];
				}
				AssertContents(package1, bill3.CU_BillUniqueCode, containerNumber1);
				AssertContents2(package2, bill3.CU_BillUniqueCode, containerNumber1);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Information - No matching BaseCusContainer found, creating new BaseCusContainer.
Information - Populating BaseCusContainer...
Warning - Container Type 'KD20' is invalid.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'KD20' is invalid.
Information - No matching BaseCusContainer found, creating new BaseCusContainer.
Information - Populating BaseCusContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Warning - Cannot find Bill (Type:'MB', Number:'MB3') for packing line; new Bill added.
Information - No matching BasePackage found, creating new BasePackage.
Information - Populating BasePackage...
Information - Added Declaration (Master Bill='MB1' House Bill='MB1HB1,MB1HB2,MB2HB1') from UniversalShipment.
Information - Successfully saved Declaration B00001000 with 7 x Bill, 2 x ForwardingContainer, 2 x BaseCusContainer, 5 x BasePackage.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestDeletingExistingContainersNotInXML()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var container1 = declarationBOToLoad.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_Weight = 1023m;
			var container2 = declarationBOToLoad.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OOCL0000027";
			container2.CO_Weight = 1023m;

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { SetupContainer() }));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("note1.IsDeleted", true, container1.IsDeleted);
				AssertEquals("note2.IsDeleted", false, container2.IsDeleted);
				AssertContents(declarationBO, null, "MYMASTER", null, 1);
				AssertEquals("declarationBO.CusContainers.Count", 1, declarationBO.CusContainers.Count);
				AssertEquals(container2, declarationBO.CusContainers[0]);
				AssertContents(container2);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching BaseCusContainer.
Information - Populating BaseCusContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestContainersWithCompleteUpdate()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_DeclarationReference = "B00001000";
			var container1 = declarationBOToLoad.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_Seal = "123";
			container1.CO_Weight = 1023m;
			var container2 = declarationBOToLoad.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OOCL0000027";
			container2.CO_Seal = "456";
			container2.CO_Weight = 1023m;

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerNumber = "CONT2";
			containerDataObject1.Seal = "666";
			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject2.ContainerNumber = "OOCL0000027";
			containerDataObject2.Seal = "888";

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.DataContext.DataTargetCollection.First().Key = "B00001000";
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			declarationDataObject.ContainerCollection.Content = CollectionContent.Complete;
			declarationDataObject.ContainerCollection.Add(containerDataObject1);
			declarationDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			declarationBO.CusContainers.Load();

			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("container1 is deleted", true, container1.IsDeleted);
				AssertEquals("container2 is not deleted", false, container2.IsDeleted);
				AssertEquals("declarationBO.CusContainers.Count", 2, declarationBO.CusContainers.Count);
				AssertEquals("OOCL0000027", declarationBO.CusContainers[0].CO_ContainerNumber);
				AssertEquals("888", declarationBO.CusContainers[0].CO_Seal);
				AssertEquals("CONT2", declarationBO.CusContainers[1].CO_ContainerNumber);
				AssertEquals("666", declarationBO.CusContainers[1].CO_Seal);
			});

			#endregion
		}

		public void TestContainersWithPartialUpdate()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_DeclarationReference = "B00001000";
			var container1 = declarationBOToLoad.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_Seal = "123";
			container1.CO_Weight = 1023m;
			var container2 = declarationBOToLoad.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OOCL0000027";
			container2.CO_Seal = "456";
			container2.CO_Weight = 1023m;

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject1.ContainerNumber = "CONT2";
			containerDataObject1.Seal = "666";
			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerDataObject2.ContainerNumber = "OOCL0000027";
			containerDataObject2.Seal = "888";

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.DataContext.DataTargetCollection.First().Key = "B00001000";
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			declarationDataObject.ContainerCollection.Content = CollectionContent.Partial;
			declarationDataObject.ContainerCollection.Add(containerDataObject1);
			declarationDataObject.ContainerCollection.Add(containerDataObject2);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			declarationBO.CusContainers.Load();

			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("container1 is not deleted", false, container1.IsDeleted);
				AssertEquals("container2 is not deleted", false, container2.IsDeleted);
				AssertEquals("declarationBO.CusContainers.Count", 3, declarationBO.CusContainers.Count);
				AssertEquals("CONT1", declarationBO.CusContainers[0].CO_ContainerNumber);
				AssertEquals("123", declarationBO.CusContainers[0].CO_Seal);
				AssertEquals("OOCL0000027", declarationBO.CusContainers[1].CO_ContainerNumber);
				AssertEquals("888", declarationBO.CusContainers[1].CO_Seal);
				AssertEquals("CONT2", declarationBO.CusContainers[2].CO_ContainerNumber);
				AssertEquals("666", declarationBO.CusContainers[2].CO_Seal);
			});

			#endregion
		}

		public void TestDeletingExistingBillsNotInXML()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_HouseBill = "MYHOUSE";
			declarationBOToLoad.JE_MasterBill = "MB2";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var bill1 = declarationBOToLoad.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "MB1";
			bill1.CU_IssueDate = new ZDateTime(2011, 6, 2);
			var bill2 = declarationBOToLoad.PrimaryMasterBill;
			bill2.CU_IssueDate = new ZDateTime(2011, 6, 3);

			var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
			{
				SetupAdditionalBill("MB2", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX"),
				SetupAdditionalBill("MYHOUSE", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB2", null, 10m, Core.Constants.PkgUnit.Box, "BOX")
			}));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(declarationBOToLoad, declarationBO);

			#region Check Contents of declaration Business Object

			CombineAssertions(delegate
			{
				AssertEquals("bill1.IsDeleted", true, bill1.IsDeleted);
				AssertEquals("bill2.IsDeleted", false, bill2.IsDeleted);
				AssertContents(declarationBO, null, "MB2", "MYHOUSE");
				var bill = declarationBO.Bills.FindByBillNumberAndType("MB2", BillTypeList.Codes.MasterBill);
				AssertEquals(bill2, bill);
				AssertContents(bill2, "MB2", BillTypeList.Codes.MasterBill, ZGuid.Empty, new ZDateTime(2011, 3, 1), 10m, Core.Constants.PkgUnit.Box, "");
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - Updated Declaration (Master Bill='MB2' House Bill='MYHOUSE') from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestCusAgent()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "Z!";
			staff.GS_FullName = "DUMMY BOB";

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.CustomsBroker = Staff.New(staff);

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				AssertEquals("declarationBO.JE_GS_NKCusAgent", "Z!", declarationBO.JE_GS_NKCusAgent);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Updated Declaration B00001000 from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestLocalCartageCompany()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Z!1";
			org1.OH_FullName = "BOB THE BUILDER";
			org1.MainAddress.OA_Address1 = "BOB STREET";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Z!2";
			org2.OH_FullName = "WENDY THE DESTROYER";
			org2.MainAddress.OA_Address1 = "WENDY STREET";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.AddOrgAddress(writeManager, org1.MainAddress, AddressTypes.PickupLocalCartage);
			declarationDataObject.AddOrgAddress(writeManager, org2.MainAddress, AddressTypes.DeliveryLocalCartage);

			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_GS_NKCusAgent = ZString.Empty;
			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				var docsAndCartage = declarationBO.DocsAndCartage;
				AssertEquals("docsAndCartage.JP_OA_DeliveryCartageCoAddr", org2.MainAddress.PK, docsAndCartage.JP_OA_DeliveryCartageCoAddr);
				AssertEquals("docsAndCartage.JP_OA_PickupCartageCoAddr", org1.MainAddress.PK, docsAndCartage.JP_OA_PickupCartageCoAddr);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Matching 'DeliveryLocalCartage':- Matched to 'Z!2' by code, address 'WENDY STREET' (only address).
Information - Matching 'PickupLocalCartage':- Matched to 'Z!1' by code, address 'BOB STREET' (only address).
Information - Updated Declaration B00001000 from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestDeclarationCusAddInfoAndCusCodeData()
		{
			var contractNumberString = "CNN";
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.US.IJobDeclaration>();
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var declarationCusAddInfoTypeSupporter = (Integration.Customs.ICusAddInfoTypeSupporter)declaration;
			Type type = null;
			Assert(declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITDoc, out type));
			var itdocAddInfo = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3) + "=AREA1234";
			Assert(declarationCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USOGADisposition, out type));
			var ogaDispositionAddInfo = USOGADispositionDataAddInfoSchema.Constants.US_Code.Substring(3) + "=I3";
			var declarationCusCodeDataTypeSupporter = (Integration.Customs.ICusCodeDataTypeSupporter)declaration;
			Assert(declarationCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(contractNumberString, out type));
			var itdocDataObject = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USITDoc },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(itdocAddInfo)
			};
			var ogaDispositionDataObject = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USOGADisposition },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(ogaDispositionAddInfo)
			};
			var contractNumberDataObject = new CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = contractNumberString },
				SubType = new CodeDescriptionPair35Char() { Code = contractNumberString },
				Reference = "OB1234"
			};

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>(new[] { itdocDataObject, ogaDispositionDataObject }));
			declarationDataObject.SetCustomsReferenceCollection(() => new List<CustomsReference>(new[] { contractNumberDataObject }));

			BaseJobDeclaration declarationBO;
			declaration.Company.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				declarationDataObject.DataContext = dataContext;
				var reader = new CustomsShipmentDataObjectReaderProvider().GetReader(declarationDataObject, logger, Factory, null);
				BusinessObject bizObj = null;
				reader.ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
			}
			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				AssertEquals("declarationBO", declaration, declarationBO);
				var cusAddInfoBOs = LoadCusAddInfo(declarationBO.TablePrefix, declarationBO.PK);
				AssertEquals(2, cusAddInfoBOs.Length);
				var itdocBO = cusAddInfoBOs[0];
				var ogaDispositionBO = cusAddInfoBOs[1];
				if (CusAddInfoTypeAttribute.Codes.USITDoc.Equals(ogaDispositionBO.GetValue(CusAddInfoSchema.B7_Type)))
				{
					itdocBO = cusAddInfoBOs[1];
					ogaDispositionBO = cusAddInfoBOs[0];
				}
				AssertCusAddInfoContents(itdocBO, declarationBO.TablePrefix, declarationBO.PK, CusAddInfoTypeAttribute.Codes.USITDoc, partialAddInfoData: itdocAddInfo);
				AssertCusAddInfoContents(ogaDispositionBO, declarationBO.TablePrefix, declarationBO.PK, CusAddInfoTypeAttribute.Codes.USOGADisposition, partialAddInfoData: ogaDispositionAddInfo);
				var cusCodeDataBOs = LoadCusCodeData(declarationBO.TablePrefix, declarationBO.PK);
				AssertEquals(1, cusCodeDataBOs.Length);
				var contractNumberBO = cusCodeDataBOs[0];
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestCreatingDeclarationUseMutex()
		{
			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S324322";
			shipment.JS_HouseBill = "HB396855";
			newFactory.Save();

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB396855",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
			};
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, "S324322");
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			declarationDataObject.DataContext = dataContext;

			using (var mutex = Enterprise.Customs.Common.DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK))
			{
				mutex.Lock();
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				AssertExceptionThrown(typeof(MessageProcessingBusinessFailureException), "Could not create a new declaration; someone else is already in the process of creating a declaration for related Shipment (S324322).", () => manager.Process(message));
			}
		}

		public void TestGetDeclarationForShipmentOnCreating()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S324322";
			shipment.JS_HouseBill = "HB396855";
			Factory.SaveForTesting();

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HB396855",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
			};
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.ForwardingShipment, "S324322");
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			declarationDataObject.DataContext = dataContext;

			shipment = Factory.Load<ForwardingShipment>(shipment.PK);
			var reader = new JobDeclarationDataObjectReaderForImportCountrySpecificRelatedDataTest(declarationDataObject, Logger, Factory, shipment, true);
			AssertNull(reader.TryGetExistingBusinessObject());

			reader.AfterGetDeclarationFromShipment = () =>
			{
				using (var mutex = DeclarationBeingCreatedForShipmentMutexCreator.Create(shipment.PK))
				{
					mutex.Lock();
					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var decCreatedInAnotherFactory = newFactory.New<BaseJobDeclaration>();
					decCreatedInAnotherFactory.JE_JS = shipment.PK;
					newFactory.Save();
				}
			};

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertExceptionThrown<MessageProcessingBusinessFailureException>("Processing failed", "Could not create a new declaration; someone else has already created a declaration for related Shipment (S324322).",
					() => reader.ReadIntoBusinessObject());

				AssertEquals("Only one declaration created", 1, Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK)).Length);
			}
		}

		public void TestGetCustomsContainerMode()
		{
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			declarationDataObject.DataContext = dataContext;

			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			declarationDataObject.ContainerCollection.Add(new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "121212" });
			declarationDataObject.ContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.BreakBulk };

			var declarationBO = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, declarationBO.JE_ContainerMode);

			declarationDataObject.ContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.BuyersConsol };
			declarationBO = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals(Core.Constants.ContainerModes.Containerised, declarationBO.JE_ContainerMode);
		}

		public void TestGetContainerMode_FromFreight()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Import, "FCL", Core.Constants.ContainerModes.FCL);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Import, "BCN", Core.Constants.ContainerModes.FCLMixedShipper);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Import, "BLK", Core.Constants.ContainerModes.Bulk);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Import, "BBK", Core.Constants.ContainerModes.BreakBulk);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Import, "LQD", Core.Constants.ContainerModes.Liquid);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Import, "UNK", Core.Constants.ContainerModes.LCL);

				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Export, "FCL", Core.Constants.ContainerModes.Containerised);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Export, "BCN", Core.Constants.ContainerModes.Containerised);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Export, "BLK", Core.Constants.ContainerModes.Bulk);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Export, "BBK", Core.Constants.ContainerModes.Containerised);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Export, "LQD", Core.Constants.ContainerModes.Liquid);
				AssertContainerMode_FromShipment(JobMessageTypeList.Codes.Export, "UNK", Core.Constants.ContainerModes.Containerised);

				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Import, "FCL", Core.Constants.ContainerModes.FCL);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Import, "BCN", Core.Constants.ContainerModes.FCLMixedShipper);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Import, "BLK", Core.Constants.ContainerModes.Bulk);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Import, "BBK", Core.Constants.ContainerModes.BreakBulk);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Import, "LQD", Core.Constants.ContainerModes.Liquid);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Import, "UNK", Core.Constants.ContainerModes.LCL);

				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Export, "FCL", Core.Constants.ContainerModes.Containerised);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Export, "BCN", Core.Constants.ContainerModes.Containerised);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Export, "BLK", Core.Constants.ContainerModes.Bulk);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Export, "BBK", Core.Constants.ContainerModes.Containerised);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Export, "LQD", Core.Constants.ContainerModes.Liquid);
				AssertContainerMode_FromSubShipment(JobMessageTypeList.Codes.Export, "UNK", Core.Constants.ContainerModes.Containerised);

				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Import, "FCL", Core.Constants.ContainerModes.FCL);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Import, "BCN", Core.Constants.ContainerModes.BuyersConsol);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Import, "BLK", Core.Constants.ContainerModes.Bulk);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Import, "BBK", Core.Constants.ContainerModes.BreakBulk);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Import, "LQD", Core.Constants.ContainerModes.Liquid);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Import, "UNK", "UNK");

				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Export, "FCL", Core.Constants.ContainerModes.FCL);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Export, "BCN", Core.Constants.ContainerModes.BuyersConsol);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Export, "BLK", Core.Constants.ContainerModes.Bulk);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Export, "BBK", Core.Constants.ContainerModes.BreakBulk);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Export, "LQD", Core.Constants.ContainerModes.Liquid);
				AssertContainerMode_FromCustomsContainerMode(JobMessageTypeList.Codes.Export, "UNK", "UNK");
			}
		}

		public void TestContainerMode_E2E()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipment_E2E.xml")));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MSCUZ7810733");
				query.AddToFilter(JobDeclarationSchema.JE_HouseBill, SQLComparisonOperator.Equal, "S00584400");
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(query);

				AssertNotNull(declaration);

				AssertEquals("declaration.JE_ContainerMode", "FCX", declaration.JE_ContainerMode);
			}
		}

		public void TestDeclarationDates()
		{
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			declarationDataObject.DataContext = dataContext;
			declarationDataObject.SetDateCollection(() => new List<Date>());
			declarationDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2012, 1, 1)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.False, new ZDateTime(2012, 2, 1)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.True, new ZDateTime(2012, 3, 1)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.True, new ZDateTime(2012, 4, 1)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.True, new ZDateTime(2012, 5, 1)));
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			AssertEquals(5, declarationDataObject.DateCollection.Count);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				AssertEquals("declarationBO.JE_DateAtFinalDestination", new ZDateTime(2012, 1, 1), declarationBO.JE_DateAtFinalDestination);
				AssertEquals("declarationBO.JE_DateAtOrigin", new ZDateTime(2012, 2, 1), declarationBO.JE_DateAtOrigin);
				AssertEquals("declarationBO.JE_DateOfArrival", new ZDateTime(2012, 3, 1), declarationBO.JE_DateOfArrival);
				AssertEquals("declarationBO.JE_DateOfFirstArrival", new ZDateTime(2012, 4, 1), declarationBO.JE_DateOfFirstArrival);
				AssertEquals("declarationBO.JE_ExportDate", new ZDateTime(2012, 5, 1), declarationBO.JE_ExportDate);
			});

			declarationDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2012, 1, 2)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2012, 2, 2)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2012, 3, 2)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2012, 4, 2)));
			declarationDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2012, 5, 2)));
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			AssertEquals(10, declarationDataObject.DateCollection.Count);
			declarationBO = reader.ReadIntoBusinessObject();

			AssertNotNull(declarationBO);

			CombineAssertions(delegate
			{
				AssertEquals("declarationBO.JE_DateAtFinalDestination", new ZDateTime(2012, 1, 2), declarationBO.JE_DateAtFinalDestination);
				AssertEquals("declarationBO.JE_DateAtOrigin", new ZDateTime(2012, 2, 2), declarationBO.JE_DateAtOrigin);
				AssertEquals("declarationBO.JE_DateOfArrival", new ZDateTime(2012, 3, 2), declarationBO.JE_DateOfArrival);
				AssertEquals("declarationBO.JE_DateOfFirstArrival", new ZDateTime(2012, 4, 2), declarationBO.JE_DateOfFirstArrival);
				AssertEquals("declarationBO.JE_ExportDate", new ZDateTime(2012, 5, 2), declarationBO.JE_ExportDate);
			});
		}

		public void TestCalculateMessageType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declarationDataObject = SetupDeclaration("OWNER123", "MB1HB1", new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House });
				declarationDataObject.MessageType = null;
				declarationDataObject.PortOfOrigin = new UNLOCO() { Code = "NZDUD", Name = "Dunedin" };
				declarationDataObject.PortOfDestination = new UNLOCO() { Code = "USLAX", Name = "Christchurch" };

				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				Factory.SaveAtEndOfImport(logger);
				AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Import, declarationBO.JE_MessageType);

				declarationDataObject.PortOfOrigin = new UNLOCO() { Code = "USLAX", Name = "Dunedin" };
				declarationDataObject.PortOfDestination = new UNLOCO() { Code = "NZAKL", Name = "Christchurch" };
				reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				Factory.SaveAtEndOfImport(logger);
				AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Export, declarationBO.JE_MessageType);

				declarationDataObject.PortOfDischarge = new UNLOCO() { Code = "NZAKL", Name = "Dunedin" };
				declarationDataObject.PortOfDestination = null;
				reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				Factory.SaveAtEndOfImport(logger);
				AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Export, declarationBO.JE_MessageType);

				declarationDataObject.PortOfOrigin = null;
				declarationDataObject.PortOfLoading = new UNLOCO() { Code = "USLAX", Name = "Dunedin" };
				reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				Factory.SaveAtEndOfImport(logger);
				AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Export, declarationBO.JE_MessageType);

				declarationDataObject.PortOfLoading = new UNLOCO() { Code = "AUSYD", Name = "Dunedin" };
				declarationDataObject.PortOfDischarge = new UNLOCO() { Code = "USCHI", Name = "CHI" };
				reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				Factory.SaveAtEndOfImport(logger);
				AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Import, declarationBO.JE_MessageType);

				var declarationforupdate = Factory.New<BaseJobDeclaration>();
				declarationforupdate.JE_DeclarationReference = "B00001111";
				Factory.SaveForTesting();

				declarationDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001111");
				declarationDataObject.MessageType = new CodeDescriptionPair() { Code = "EXP" };
				declarationDataObject.PortOfLoading = new UNLOCO() { Code = "AUSYD", Name = "Dunedin" };
				declarationDataObject.PortOfDischarge = new UNLOCO() { Code = "USCHI", Name = "CHI" };
				reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				Factory.SaveAtEndOfImport(logger);
				AssertEquals("declarationBO.JE_MessageType", JobMessageTypeList.Codes.Export, declarationBO.JE_MessageType);
			}
		}

		public void TestMultipleBillsMatchedForPackingLine()
		{
			var declarationDataObject = SetupDeclaration(null, "12345678", new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master });
			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			var bill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "12345678",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }
			};
			bill1.SetAddInfoCollection(() => new List<AddInfo> { new AddInfo() { Key = "", Value = "APLU" } });
			var bill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = ZString.Empty,
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
			};
			var bill3 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = ZString.Empty,
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }
			};
			bill2.SetAddInfoCollection(() => new List<AddInfo> { new AddInfo() { Key = "", Value = "APLU" } });
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill> { bill1, bill2, bill3 });

			declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = "12345678",
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PackQty = 100
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = ZString.Empty,
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					PackQty = 200
				}
			});
			declarationDataObject.PackingLineCollection.Content = CollectionContent.Complete;

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertContains("Warning - Cannot add a package as there are multiple bill records (Type:'HB', Number:'') matched.", logger.Logs);

			declarationBO.Packages.Load();
			AssertEquals(1, declarationBO.Packages.Count);
			AssertEquals(100, declarationBO.Packages[0].CW_PackQty);
		}

		public void TestImportToMatchingShipmentUsingDataContextKey()
		{
			logger.ClearLogs();
			CombineAssertions("Not Creating Declaration - Due to Mutex", () =>
			{
				var testingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				Factory.SaveForTesting();
				var targetKey = testingShipment.JS_UniqueConsignRef;

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, targetKey);

				var assertDeclaration = Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, testingShipment.JS_UniqueConsignRef));
				AssertEquals("PRE: no dec for relatedShipment", 0, assertDeclaration.Length);
				AssertNull("PRE: no dec for relatedShipment", testingShipment.Declarations.FirstOrDefault());

				using (var mutex = Enterprise.Customs.Common.DeclarationBeingCreatedForShipmentMutexCreator.Create(testingShipment.PK))
				{
					mutex.Lock();
					var reader = new JobDeclarationDataObjectReader(shipmentDataObject, logger, Factory, null);

					AssertExceptionThrown<MessageProcessingBusinessFailureException>("Could not create a new declaration; someone else is already in the process of creating a declaration for relatedShipment (" + testingShipment.JS_UniqueConsignRef + ").", () => { reader.ReadIntoBusinessObject(); });

					assertDeclaration = Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, testingShipment.JS_UniqueConsignRef));
					AssertEquals("no dec for relatedShipment", 0, assertDeclaration.Length);
					AssertNull("no dec for relatedShipment", testingShipment.Declarations.FirstOrDefault());
					AssertMultilineASCIIEquals("LogText", @"Information - Fail to find Declaration, try locating matching Shipment using Data Target key.", logger.Logs);
				}
			});

			logger.ClearLogs();
			CombineAssertions("Creating Declaration - Ignore Query User", () =>
			{
				var testingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				Factory.SaveForTesting();
				var targetKey = testingShipment.JS_UniqueConsignRef;

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, targetKey);
				shipmentDataObject.PortOfOrigin = new UNLOCO() { Code = "NZDUD", Name = "Dunedin" };
				shipmentDataObject.PortOfDestination = new UNLOCO() { Code = "CNSHA", Name = "whatever" };

				var assertDeclarations = Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, testingShipment.JS_UniqueConsignRef));
				AssertEquals("PRE: no dec for relatedShipment", 0, assertDeclarations.Length);
				AssertNull("PRE: no dec for relatedShipment", testingShipment.Declarations.FirstOrDefault());

				var reader = new JobDeclarationDataObjectReader(shipmentDataObject, logger, Factory, null);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				assertDeclarations =
					Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference,
						testingShipment.JS_UniqueConsignRef));
				AssertEquals("declaration created for relatedShipment", 1, assertDeclarations.Length);
				var outputDeclaration = assertDeclarations.FirstOrDefault();
				AssertEquals("declaration created for relatedShipment - DeclarationReference", testingShipment.JS_UniqueConsignRef,
					outputDeclaration.JE_DeclarationReference);
				AssertEquals("declaration created for relatedShipment - OverrideFlag", true,
					outputDeclaration.JE_OverrideFreightDefaults);
				AssertEquals("declaration created for relatedShipment - Value", "NZDUD", outputDeclaration.JE_RL_NKOrigin);
				AssertEquals("declaration created for relatedShipment - Value", "CNSHA", outputDeclaration.JE_RL_NKFinalDestination);
				AssertMultilineASCIIEquals("LogText",
					@"Information - Fail to find Declaration, try locating matching Shipment using Data Target key.
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Added Declaration from UniversalShipment."
					, logger.Logs);
				Factory.FireCleanupAfterSaving();
			});

			logger.ClearLogs();
			CombineAssertions("Creating Declaration", () =>
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ZAJNB";
				var testingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				testingShipment.JS_RL_NKOrigin = "AUSYD";
				testingShipment.JS_RL_NKDestination = "ZAJNB";
				testingShipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.OrgProxy.PK;
				Factory.SaveForTesting();
				var targetKey = testingShipment.JS_UniqueConsignRef;

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, targetKey);
				shipmentDataObject.PortOfOrigin = new UNLOCO() { Code = "NZDUD", Name = "Dunedin" };
				shipmentDataObject.PortOfDestination = new UNLOCO() { Code = "CNSHA", Name = "whatever" };

				var assertDeclarations = Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, testingShipment.JS_UniqueConsignRef));
				AssertEquals("PRE: no dec for relatedShipment", 0, assertDeclarations.Length);
				AssertNull("PRE: no dec for relatedShipment", testingShipment.Declarations.FirstOrDefault());

				var reader = new JobDeclarationDataObjectReader(shipmentDataObject, logger, Factory, null);

				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				assertDeclarations = Factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, testingShipment.JS_UniqueConsignRef));
				AssertEquals("declaration created for relatedShipment", 1, assertDeclarations.Length);
				var outputDeclaration = assertDeclarations.FirstOrDefault();
				AssertEquals("declaration created for relatedShipment - DeclarationReference", testingShipment.JS_UniqueConsignRef, outputDeclaration.JE_DeclarationReference);
				AssertEquals("declaration created for relatedShipment - OverrideFlag", true, outputDeclaration.JE_OverrideFreightDefaults);
				AssertEquals("declaration created for relatedShipment - Value", "NZDUD", outputDeclaration.JE_RL_NKOrigin);
				AssertEquals("declaration created for relatedShipment - Value", "CNSHA", outputDeclaration.JE_RL_NKFinalDestination);
				AssertMultilineASCIIEquals("LogText", @"Information - Fail to find Declaration, try locating matching Shipment using Data Target key.
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Added Declaration from UniversalShipment."
			, logger.Logs);
				Factory.FireCleanupAfterSaving();
			});
		}

		[TestDate(2019, 11, 11)]
		public void TestFillDeclarationWithSuspendedSettersProperties_US()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MASTERDEFER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PortOfDischarge = new UNLOCO() { Code = "USCHI", Name = "Chicago" },
				};
				declarationDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>()
					{
						new EntryInstruction()
						{
							Style = "AAA",
							SubStyle = new CodeDescriptionPair() { Code = "S", Description = "Sub Style" }
						}
					});
				declarationDataObject.SetDateCollection(() => new List<Date>()
					{
						new Date()
						{
							Type = DateType.Arrival,
							IsEstimate = true,
							Value = new ZDateTime(2019, 11, 23)
						}
					});

				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);

				var declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("", declarationBO.JE_LocationOfGoods);
				AssertEquals("", declarationBO.JE_SubLocationOfGoods);

				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "DEU" };
				declarationDataObject.SubLocationAtClearance = new CodeDescriptionPair35Char() { Code = "SUB" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);

				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("DEU", declarationBO.JE_LocationOfGoods);
				AssertEquals("SUB", declarationBO.JE_SubLocationOfGoods);
			}
		}

		[ExpectNoExceptions]
		public void TestImportCountrySpecificRelatedData()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.JE_MasterBill = "MYMASTER";
			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			var reader = new JobDeclarationDataObjectReaderForImportCountrySpecificRelatedDataTest(declarationDataObject, logger, Factory, null, true);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);
			AssertEquals("B00000001", declarationBO.JE_DeclarationReference);

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "B00000002";
			declaration2.JE_MasterBill = "MYMASTER2";
			Factory.SaveForTesting();

			declarationDataObject = SetupDeclaration(null, "MYMASTER2", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			reader = new JobDeclarationDataObjectReaderForImportCountrySpecificRelatedDataTest(declarationDataObject, logger, Factory, null, false);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);
			AssertEquals("B00000002", declarationBO.JE_DeclarationReference);
		}

		public void TestPopulateOrders()
		{
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			declarationDataObject.GoodsDescription = "declaration desc";
			declarationDataObject.DataContext = dataContext;

			declarationDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			declarationDataObject.RelatedShipmentCollection.Add(orderData);
			declarationDataObject.RelatedShipmentCollection[0].GoodsDescription = "order desc";

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory, null);
			var declaration = reader.ReadIntoBusinessObject();

			AssertNotNull(declaration);
			AssertEquals("Order has been attached to the declaration", 1, declaration.AttachedOrders.Count);
			var order = declaration.AttachedOrders[0];
			AssertEquals("ORDER ME", order.JD_OrderNumber);
			AssertEquals("order desc", order.JD_OrderGoodsDescription);
			AssertEquals(new ZByte(2), order.JD_OrderNumberSplit);
			AssertEquals(false, logger.HasErrors);
			AssertEquals(false, logger.HasWarnings);
			AssertEquals("declaration desc", declaration.JE_GoodsDescription);
			AssertMultilineASCIIEquals("LogText", @"Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - No matching Order found, creating new Order.
Information - Populating Order...
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Added Order ORDER ME-2 from UniversalShipment.
Information - Added Declaration from UniversalShipment."
				, logger.Logs);
			Factory.FireCleanupAfterSaving();
		}

		public void TestCusEntryHeaderForIntegratedCountryWithMultipleDeclarationsMatched()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration1.JE_DeclarationReference = "B00000001";
				var cusEntryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
				cusEntryHeader1.CH_MessageType = "IMP";
				cusEntryHeader1.CH_BGMReference = "IMP20180207000001";

				var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration2.JE_DeclarationReference = "B00000002";
				var cusEntryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				cusEntryHeader2.CH_MessageType = "IMP";
				cusEntryHeader2.CH_BGMReference = "IMP20180207000001";

				Factory.SaveForTesting();
				var declarationDataObject = SetupDeclarationAndEntryHeaders("MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" }, "IMP", "IMP20180207000001");
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertNull("Importation is rejected.", declarationBO);
				AssertContains("Importation is rejected.", "Cannot import Entry Details when there are multiple declarations matched with same entry details.", logger.Logs);
			}
		}

		public void TestCusEntryHeaderForIntegratedCountryWithNoDeclarationsMatchedAndNoBillDetails()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declarationDataObject = SetupDeclarationAndEntryHeaders(null, null, "IMP", ZString.Empty);
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				AssertCustomsEntryHeadersAndEntryLines(declarationBO);
			}
		}

		public void TestCusEntrtyHeaderForIntegratedCountryWithBillDetailsExistsAndNoDeclarationsMatched()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declarationDataObject = SetupDeclarationAndEntryHeaders("MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" }, "IMP", ZString.Empty);
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				AssertCustomsEntryHeadersAndEntryLines(declarationBO);
			}
		}

		public void TestInvalidCompanyCodeForIntegratedCountryWillUseCurrentCompany()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declarationDataObject = SetupDeclaration("OWN324", "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				declarationDataObject.Branch = new Branch { Code = "B99" };
				var masterBillDataObject = SetupAdditionalBill("MYMASTER", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				var masterBillHouseBillDataObject = SetupAdditionalBill("MYHOUSE", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MYMASTER", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
				var entryHeaderDataObject = SetupEntryHeader("IMP", "SNT", ZString.Empty, "BG89756/1", 0m, null, null);
				entryHeaderDataObject.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber() });
				entryHeaderDataObject.EntryLineCollection = new List<EntryLine>(new[] { SetupEntryLine() });
				declarationDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>() { entryHeaderDataObject });
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				var invalidCompanyCode = @"      <Company>
        <Code>E99</Code>";
				message.EM_MessageText = message.EM_MessageText.Replace(@"      <Company>
        <Code>EDI</Code>", invalidCompanyCode).Replace("				      <DataProvider>EDIDATEDI</DataProvider>", "").Replace("      <EnterpriseID>EDI</EnterpriseID>", "");
				AssertContains(invalidCompanyCode, message.EM_MessageText);
				var codesMappedToTarget = "<CodesMappedToTarget>true</CodesMappedToTarget>";
				AssertContains(codesMappedToTarget, message.EM_MessageText);

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.Branches.Add(Factory.NewWithValidTestData<GlbBranch>());
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_Code = "B99";
				branch.GB_IsActive = false;
				company.Branches.Add(branch);
				Factory.SaveForTesting();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				using (Environment.DisposableEnvironment.ForCompany(company.GC_Code))
				{
					manager.Process(message);
				}
				AssertContains("ERROR - Message Rejected as Branch 'B99' is inactive.", serviceTaskLog.ToString());
				AssertNull(Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000")));
				message.EM_MessageText = message.EM_MessageText.Replace(codesMappedToTarget, "<CodesMappedToTarget>false</CodesMappedToTarget>");
				message.EM_Status = EDIMessage.Status.Queued;
				serviceTaskLog.ClearLogs();
				manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				AssertNotContains("ERROR - Could not load the environment settings to process the XML data for Company ('E99'), Branch ('B99').", serviceTaskLog.ToString());
				var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
				AssertEquals("Current branch is used", GlbBranch.CurrentBranch.PK, declaration.JE_GB);
			}
		}

		public void TestAutoWeghtAportionSuspend()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INV1";
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line1.JI_Description = "INV1LINE1";
				declaration.JE_AutoWeightApportion = true;
				Factory.SaveForTesting();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var invoiceLineDataObject1 = new CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 1",
					LinePrice = 200,
					Weight = 0m
				};
				var invoiceLineDataObject2 = new CommercialInvoiceLine()
				{
					LineNo = 2,
					Description = "LINE 2",
					LinePrice = 100,
					Weight = 0m
				};

				var invoiceDataObject1 = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Weight = 300,
					WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 })));

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = new CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject1 })
					}
				};
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals("Have 1 invoice header", 1, declarationBO.Invoices.Count);
				AssertEquals("Have 2 invoice lines", 2, declarationBO.InvoiceLines.Count);
				AssertEquals("No WeightApportion", 0m, declarationBO.InvoiceLines[0].JI_Weight);
				AssertEquals("No WeightApportion", 0m, declarationBO.InvoiceLines[1].JI_Weight);
				Assert("No WeightApportion", !declarationBO.JE_AutoWeightApportion);
			}
		}

		public void TestDeleteExistingInvoiceDetailsNotLinkedToEntryForIntegratedCountry()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INV1";
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line1.JI_Description = "INV1LINE1";
				var subGroup1 = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
				subGroup1.JZ_InvoiceNumber = "SUBGROUP1";
				var subGroup1Invoice1 = subGroup1.JobComInvoiceHeaders.AddNew();
				subGroup1Invoice1.JZ_InvoiceNumber = "INV2";
				var subGroup1Invoice1Line1 = subGroup1Invoice1.JobComInvoiceLines.AddNew();
				subGroup1Invoice1Line1.JI_Description = "INV2LINE1";
				var subGroup2 = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
				subGroup2.JZ_InvoiceNumber = "SUBGROUP2";
				var subGroup2_1 = subGroup2.JobComInvoiceGroupHeaders.AddNew();
				subGroup2_1.JZ_InvoiceNumber = "SUBGROUP2_1";
				var subGroup2_1Invoice1 = subGroup2_1.JobComInvoiceHeaders.AddNew();
				subGroup2_1Invoice1.JZ_InvoiceNumber = "INV3";
				var subGroup2_1Invoice1Line1 = subGroup2_1Invoice1.JobComInvoiceLines.AddNew();
				subGroup2_1Invoice1Line1.JI_Description = "INV3LINE1";
				var topGroupInvoie = declaration.TopGroupInvoice;

				Factory.SaveForTesting();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				};
				declarationDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>(new[]
					{
						new EntryHeader()
						{
							Type = new EntryType() { Code = JobMessageTypeList.Codes.Import },
							Reference = declaration.JE_DeclarationReference + "/1",
							EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[]
							{
								SetupEntryNumber(new EntryType() { Code = CusEntryNumberTypes.Standard.MovementReferenceNumber }, "W986548", ZBool.False)
							}),
							EntryLineCollection = new List<EntryLine>(new []
							{
								SetupEntryLine()
							})
						}
					}));
				// Test no data should be delete when existing job has not entry detail
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals(declaration, declarationBO);
				AssertEquals("topGroupInvoie.IsDeleted", false, topGroupInvoie.IsDeleted);
				AssertEquals("topGroupInvoie", topGroupInvoie, declarationBO.TopGroupInvoice);
				AssertEquals("declarationBO.AllGroupHeaders.Count", 4, declarationBO.AllGroupHeaders.Count);
				AssertCollectionContains("topGroupInvoie", topGroupInvoie, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup1", subGroup1, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup2", subGroup2, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup2_1", subGroup2_1, declarationBO.AllGroupHeaders);
				AssertEquals("declarationBO.Invoices.Count", 3, declarationBO.Invoices.Count);
				AssertCollectionContains("invoice1", invoice1, declarationBO.Invoices);
				AssertCollectionContains("subGroup1Invoice1", subGroup1Invoice1, declarationBO.Invoices);
				AssertCollectionContains("subGroup2_1Invoice1", subGroup2_1Invoice1, declarationBO.Invoices);
				AssertEquals("declarationBO.InvoiceLines.Count", 3, declarationBO.InvoiceLines.Count);
				AssertCollectionContains("invoice1Line1", invoice1Line1, declarationBO.InvoiceLines);
				AssertCollectionContains("subGroup1Invoice1Line1", subGroup1Invoice1Line1, declarationBO.InvoiceLines);
				AssertCollectionContains("subGroup2_1Invoice1Line1", subGroup2_1Invoice1Line1, declarationBO.InvoiceLines);
				AssertEquals("invoice1.IsDeleted", false, invoice1.IsDeleted);
				AssertEquals("invoice1Line1.IsDeleted", false, invoice1Line1.IsDeleted);
				AssertEquals("subGroup1.IsDeleted", false, subGroup1.IsDeleted);
				AssertEquals("subGroup1Invoice1.IsDeleted", false, subGroup1Invoice1.IsDeleted);
				AssertEquals("subGroup1Invoice1Line1.IsDeleted", false, subGroup1Invoice1Line1.IsDeleted);
				AssertEquals("subGroup2.IsDeleted", false, subGroup2.IsDeleted);
				AssertEquals("subGroup2_1.IsDeleted", false, subGroup2_1.IsDeleted);
				AssertEquals("subGroup2_1Invoice1.IsDeleted", false, subGroup2_1Invoice1.IsDeleted);
				AssertEquals("subGroup2_1Invoice1Line1.IsDeleted", false, subGroup2_1Invoice1Line1.IsDeleted);
				declarationBO.CustomsEntryHeaders.Load();
				AssertEquals("declarationBO.CustomsEntryHeaders.Count", 1, declarationBO.CustomsEntryHeaders.Count);
				var entry = declarationBO.CustomsEntryHeaders[0];
				AssertEquals("entry.CH_MessageType", JobMessageTypeList.Codes.Import, entry.CH_MessageType);
				AssertEquals("entry.CH_BGMReference", declaration.JE_DeclarationReference + "/1", entry.CH_BGMReference);
				AssertEquals("entry.EntryNumber", "W986548", entry.EntryNumber);
				entry.AllEntryLines.Load();
				AssertEquals("entry.AllEntryLines.Count", 1, entry.AllEntryLines.Count);
				var entryLine = entry.AllEntryLines[0];
				var subGroup3 = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
				subGroup3.JZ_InvoiceNumber = "SUBGROUP2";
				var subGroup3_1 = subGroup3.JobComInvoiceGroupHeaders.AddNew();
				subGroup3_1.JZ_InvoiceNumber = "SUBGROUP2_1";
				var subGroup3_1Invoice1 = subGroup3_1.JobComInvoiceHeaders.AddNew();
				subGroup3_1Invoice1.JZ_InvoiceNumber = "INV4";
				var subGroup3_1Invoice1Line1 = subGroup3_1Invoice1.JobComInvoiceLines.AddNew();
				subGroup3_1Invoice1Line1.JI_Description = "INV4LINE1";
				subGroup3_1Invoice1Line1.JI_CL = entryLine.PK;
				Factory.SaveForTesting();

				// Existing invoice should not be deleted as not commercial info was specified
				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals(declaration, declarationBO);
				declarationBO.CustomsEntryHeaders.Load();
				AssertEquals("declarationBO.CustomsEntryHeaders.Count", 1, declarationBO.CustomsEntryHeaders.Count);
				AssertEquals("declarationBO.CustomsEntryHeaders[0]", entry, declarationBO.CustomsEntryHeaders[0]);
				AssertEquals("entry.CH_MessageType", JobMessageTypeList.Codes.Import, entry.CH_MessageType);
				AssertEquals("entry.CH_BGMReference", declaration.JE_DeclarationReference + "/1", entry.CH_BGMReference);
				AssertEquals("entry.EntryNumber", "W986548", entry.EntryNumber);
				entry.AllEntryLines.Load();
				AssertEquals("entry.AllEntryLines.Count", 1, entry.AllEntryLines.Count);
				AssertNotEquals("entry.AllEntryLines[0]", entryLine, entry.AllEntryLines[0]);
				AssertEquals("entryLine.IsDeleted", true, entryLine.IsDeleted);
				AssertEquals("topGroupInvoie.IsDeleted", false, topGroupInvoie.IsDeleted);
				AssertEquals("topGroupInvoie", topGroupInvoie, declarationBO.TopGroupInvoice);
				AssertEquals("declarationBO.AllGroupHeaders.Count", 6, declarationBO.AllGroupHeaders.Count);
				AssertCollectionContains("topGroupInvoie", topGroupInvoie, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup1", subGroup1, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup2", subGroup2, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup2_1", subGroup2_1, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup3", subGroup3, declarationBO.AllGroupHeaders);
				AssertCollectionContains("subGroup3_1", subGroup3_1, declarationBO.AllGroupHeaders);
				AssertEquals("declarationBO.Invoices.Count", 4, declarationBO.Invoices.Count);
				AssertCollectionContains("invoice1", invoice1, declarationBO.Invoices);
				AssertCollectionContains("subGroup1Invoice1", subGroup1Invoice1, declarationBO.Invoices);
				AssertCollectionContains("subGroup2_1Invoice1", subGroup2_1Invoice1, declarationBO.Invoices);
				AssertCollectionContains("subGroup3_1Invoice1", subGroup3_1Invoice1, declarationBO.Invoices);
				AssertEquals("declarationBO.InvoiceLines.Count", 3, declarationBO.InvoiceLines.Count);
				AssertCollectionContains("invoice1Line1", invoice1Line1, declarationBO.InvoiceLines);
				AssertCollectionContains("subGroup1Invoice1Line1", subGroup1Invoice1Line1, declarationBO.InvoiceLines);
				AssertCollectionContains("subGroup2_1Invoice1Line1", subGroup2_1Invoice1Line1, declarationBO.InvoiceLines);
				AssertEquals("invoice1.IsDeleted", false, invoice1.IsDeleted);
				AssertEquals("invoice1Line1.IsDeleted", false, invoice1Line1.IsDeleted);
				AssertEquals("subGroup1.IsDeleted", false, subGroup1.IsDeleted);
				AssertEquals("subGroup1Invoice1.IsDeleted", false, subGroup1Invoice1.IsDeleted);
				AssertEquals("subGroup1Invoice1Line1.IsDeleted", false, subGroup1Invoice1Line1.IsDeleted);
				AssertEquals("subGroup2.IsDeleted", false, subGroup2.IsDeleted);
				AssertEquals("subGroup2_1.IsDeleted", false, subGroup2_1.IsDeleted);
				AssertEquals("subGroup2_1Invoice1.IsDeleted", false, subGroup2_1Invoice1.IsDeleted);
				AssertEquals("subGroup2_1Invoice1Line1.IsDeleted", false, subGroup2_1Invoice1Line1.IsDeleted);
				AssertEquals("subGroup3.IsDeleted", false, subGroup3.IsDeleted);
				AssertEquals("subGroup3_1.IsDeleted", false, subGroup3_1.IsDeleted);
				AssertEquals("subGroup3_1Invoice1.IsDeleted", false, subGroup3_1Invoice1.IsDeleted);
				AssertEquals("subGroup3_1Invoice1Line1.IsDeleted", true, subGroup3_1Invoice1Line1.IsDeleted);
				Factory.SaveForTesting();

				// Existing invoice details should be deleted as commercial info was specified
				declarationDataObject.CommercialInfo = new CommercialInfo();
				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				declarationBO = reader.ReadIntoBusinessObject();
				AssertEquals(declaration, declarationBO);
				declarationBO.CustomsEntryHeaders.Load();
				AssertEquals("declarationBO.CustomsEntryHeaders.Count", 1, declarationBO.CustomsEntryHeaders.Count);
				AssertEquals("declarationBO.CustomsEntryHeaders[0]", entry, declarationBO.CustomsEntryHeaders[0]);
				AssertEquals("entry.CH_MessageType", JobMessageTypeList.Codes.Import, entry.CH_MessageType);
				AssertEquals("entry.CH_BGMReference", declaration.JE_DeclarationReference + "/1", entry.CH_BGMReference);
				AssertEquals("entry.EntryNumber", "W986548", entry.EntryNumber);
				AssertEquals("declarationBO.AllGroupHeaders.Count", 1, declarationBO.AllGroupHeaders.Count);
				AssertEquals("topGroupInvoie.IsDeleted", false, topGroupInvoie.IsDeleted);
				AssertEquals("topGroupInvoie", topGroupInvoie, declarationBO.AllGroupHeaders[0]);
				AssertEquals("topGroupInvoie", topGroupInvoie, declarationBO.TopGroupInvoice);
				AssertEquals("declarationBO.Invoices.Count", 0, declarationBO.Invoices.Count);
				AssertEquals("declarationBO.InvoiceLines.Count", 0, declarationBO.InvoiceLines.Count);
				AssertEquals("invoice1.IsDeleted", true, invoice1.IsDeleted);
				AssertEquals("invoice1Line1.IsDeleted", true, invoice1Line1.IsDeleted);
				AssertEquals("subGroup1.IsDeleted", true, subGroup1.IsDeleted);
				AssertEquals("subGroup1Invoice1.IsDeleted", true, subGroup1Invoice1.IsDeleted);
				AssertEquals("subGroup1Invoice1Line1.IsDeleted", true, subGroup1Invoice1Line1.IsDeleted);
				AssertEquals("subGroup2.IsDeleted", true, subGroup2.IsDeleted);
				AssertEquals("subGroup2_1.IsDeleted", true, subGroup2_1.IsDeleted);
				AssertEquals("subGroup2_1Invoice1.IsDeleted", true, subGroup2_1Invoice1.IsDeleted);
				AssertEquals("subGroup2_1Invoice1Line1.IsDeleted", true, subGroup2_1Invoice1Line1.IsDeleted);
				AssertEquals("subGroup3.IsDeleted", true, subGroup3.IsDeleted);
				AssertEquals("subGroup3_1.IsDeleted", true, subGroup3_1.IsDeleted);
				AssertEquals("subGroup3_1Invoice1.IsDeleted", true, subGroup3_1Invoice1.IsDeleted);
			}
		}

		public void TestCusEntryHeaderForIntegratedCountryMatchingInvoiceDetails()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declarationDataObject = SetupDeclaration("OWN324", "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
				var masterBillDataObject = SetupAdditionalBill("MYMASTER", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
				var masterBillHouseBillDataObject = SetupAdditionalBill("MYHOUSE", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MYMASTER", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject, masterBillHouseBillDataObject }));
				var entryHeaderDataObject1 = SetupEntryHeader("IMP", "SNT", ZString.Empty, "BG89756/1", 0m, null, null);
				var entryNumber1 = SetupEntryNumber();
				var entryNumber2 = SetupEntryNumber2();
				entryHeaderDataObject1.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { entryNumber1, entryNumber2 });
				entryHeaderDataObject1.EntryLineCollection = new List<EntryLine>(new[] { SetupEntryLine(), SetupEntryLine2() });
				var entryHeaderDataObject2 = SetupEntryHeader("IMP", "SNT", ZString.Empty, "BG89756/2", 0m, null, null);
				var entryNumber3 = SetupEntryNumber();
				entryNumber3.Number = "JDK23222";
				entryHeaderDataObject2.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { entryNumber3 });
				entryHeaderDataObject2.EntryLineCollection = new List<EntryLine>(new[] { SetupEntryLine() });
				declarationDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>() { entryHeaderDataObject1, entryHeaderDataObject2 });

				var invoiceLineDataObject1 = new CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 1",
					EntryNumber = entryNumber3.Number,
					EntryLineNumber = 1
				};
				var invoiceLineDataObject2 = new CommercialInvoiceLine()
				{
					LineNo = 2,
					Description = "LINE 2",
					EntryNumber = entryNumber2.Number,
					EntryLineNumber = 1
				};
				var invoiceLineDataObject3 = new CommercialInvoiceLine()
				{
					LineNo = 3,
					Description = "LINE 3",
					EntryNumber = entryNumber1.Number,
					EntryLineNumber = 2
				};
				var invoiceDataObject1 = SetupCommercialInvoiceHeaderData(commercialInvoiceLineCollection: new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3 }));

				var invoiceLineDataObject4 = new CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 4",
					EntryNumber = entryNumber1.Number,
					EntryLineNumber = 2
				};
				var invoiceDataObject2 = SetupCommercialInvoiceHeaderData2(commercialInvoiceLineCollection: new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject4 }));

				var subGroupDataObject = new CommercialInfo()
				{
					Name = "GROUP1",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject2 }),
				};
				declarationDataObject.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceDataObject1 }),
					SubGroupCollection = new List<CommercialInfo>(new[] { subGroupDataObject })
				};
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				declarationBO.CustomsEntryHeaders.Load();
				AssertEquals("declarationBO.CustomsEntryHeaders.Count", 2, declarationBO.CustomsEntryHeaders.Count);
				var entry1 = declarationBO.CustomsEntryHeaders[0];
				var entry2 = declarationBO.CustomsEntryHeaders[1];
				if (entry2.CH_BGMReference == "BG89756/1")
				{
					entry2 = declarationBO.CustomsEntryHeaders[0];
					entry1 = declarationBO.CustomsEntryHeaders[1];
				}
				AssertEquals("entry1.CH_BGMReference", "BG89756/1", entry1.CH_BGMReference);
				entry1.AllEntryLines.Load();
				AssertEquals("entry1.AllEntryLines.Count", 2, entry1.AllEntryLines.Count);
				var entry1Line1 = entry1.AllEntryLines[0];
				var entry1Line2 = entry1.AllEntryLines[1];
				if (entry1Line2.CL_LineNumber == (short)1)
				{
					entry1Line2 = entry1.AllEntryLines[0];
					entry1Line1 = entry1.AllEntryLines[1];
				}
				AssertEquals("entry2.CH_BGMReference", "BG89756/2", entry2.CH_BGMReference);
				entry2.AllEntryLines.Load();
				AssertEquals("entry2.AllEntryLines.Count", 1, entry2.AllEntryLines.Count);
				var entry2Line1 = entry2.AllEntryLines[0];
				var topGroupInvoice = declarationBO.TopGroupInvoice;
				declarationBO.AllGroupHeaders.Load();
				AssertEquals("topGroupInvoice.JobComInvoiceGroupHeaders.Count", 1, topGroupInvoice.JobComInvoiceGroupHeaders.Count);
				var subGroup = topGroupInvoice.JobComInvoiceGroupHeaders[0];
				AssertEquals("subGroup.JZ_InvoiceNumber", "GROUP1", subGroup.JZ_InvoiceNumber);
				AssertEquals("declarationBO.Invoices.Count", 2, declarationBO.Invoices.Count);
				var invoice1 = declarationBO.Invoices[0];
				var invoice2 = declarationBO.Invoices[1];
				if (invoice2.JZ_InvoiceNumber == invoiceDataObject1.InvoiceNumber.Value)
				{
					invoice2 = declarationBO.Invoices[0];
					invoice1 = declarationBO.Invoices[1];
				}
				AssertEquals("invoice1.JZ_InvoiceNumber", invoiceDataObject1.InvoiceNumber, invoice1.JZ_InvoiceNumber);
				AssertEquals("invoice1.JZ_JZ_GroupInvoiceFK", topGroupInvoice.PK, invoice1.JZ_JZ_GroupInvoiceFK);
				declarationBO.InvoiceLines.Load();
				AssertEquals("invoice1.JobComInvoiceLines.Count", 3, invoice1.JobComInvoiceLines.Count);
				var invoice1Line1 = invoice1.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 1);
				var invoice1Line2 = invoice1.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 2);
				var invoice1Line3 = invoice1.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 3);
				AssertEquals("invoice1Line1.JI_CL", entry2Line1.PK, invoice1Line1.JI_CL);
				AssertEquals("invoice1Line1.JI_Description", "LINE 1", invoice1Line1.JI_Description);
				AssertEquals("invoice1Line2.JI_CL", entry1Line1.PK, invoice1Line2.JI_CL);
				AssertEquals("invoice1Line2.JI_Description", "LINE 2", invoice1Line2.JI_Description);
				AssertEquals("invoice1Line3.JI_CL", entry1Line2.PK, invoice1Line3.JI_CL);
				AssertEquals("invoice1Line3.JI_Description", "LINE 3", invoice1Line3.JI_Description);

				AssertEquals("invoice2.JZ_InvoiceNumber", invoiceDataObject2.InvoiceNumber, invoice2.JZ_InvoiceNumber);
				AssertEquals("invoice2.JZ_JZ_GroupInvoiceFK", subGroup.PK, invoice2.JZ_JZ_GroupInvoiceFK);
				AssertEquals("invoice2.JobComInvoiceLines.Count", 1, invoice2.JobComInvoiceLines.Count);
				var invoice2Line1 = invoice2.JobComInvoiceLines[0];
				AssertEquals("invoice2Line1.JI_CL", entry1Line2.PK, invoice2Line1.JI_CL);
				AssertEquals("invoice2Line1.JI_Description", "LINE 4", invoice2Line1.JI_Description);

				entryHeaderDataObject1.Reference = "BG89756/3";
				entryHeaderDataObject1.EntryLineCollection = new List<EntryLine>(new[] { SetupEntryLine() });
				declarationDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>() { entryHeaderDataObject1 });
				Factory.SaveForTesting();

				var invoiceLineDataObject5 = new CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 5",
					EntryNumber = entryNumber1.Number,
					EntryLineNumber = 1
				};
				invoiceDataObject1.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject5 }));

				var invoiceLineDataObject6 = new CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 6",
					EntryNumber = entryNumber1.Number,
					EntryLineNumber = 1
				};

				invoiceDataObject2.InvoiceNumber = "KD234224";
				invoiceDataObject2.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLineDataObject6 }));
				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				AssertEquals("declarationBO should have been matched", declarationBO, reader.ReadIntoBusinessObject());
				declarationBO.CustomsEntryHeaders.Load();
				AssertEquals("declarationBO.CustomsEntryHeaders.Count", 2, declarationBO.CustomsEntryHeaders.Count);
				AssertCollectionContains(entry1, declarationBO.CustomsEntryHeaders);
				AssertCollectionContains(entry2, declarationBO.CustomsEntryHeaders);
				AssertEquals("entry1.CH_BGMReference", "BG89756/3", entry1.CH_BGMReference);
				entry1.AllEntryLines.Load();
				AssertEquals("entry1.AllEntryLines.Count", 1, entry1.AllEntryLines.Count);
				AssertEquals("entry1Line1.IsDeleted", true, entry1Line1.IsDeleted);
				AssertEquals("entry1Line2.IsDeleted", true, entry1Line2.IsDeleted);
				AssertNotEquals("entry1Line1", entry1Line1, entry1.AllEntryLines[0]);
				entry1Line1 = entry1.AllEntryLines[0];

				AssertEquals("entry2.CH_BGMReference", "BG89756/2", entry2.CH_BGMReference);
				entry2.AllEntryLines.Load();
				AssertEquals("entry2.AllEntryLines.Count", 1, entry2.AllEntryLines.Count);
				AssertEquals("entry2Line1", entry2Line1, entry2.AllEntryLines[0]);
				AssertEquals("topGroupInvoice", topGroupInvoice, declarationBO.TopGroupInvoice);
				AssertEquals("topGroupInvoice.IsDeleted", false, topGroupInvoice.IsDeleted);
				declarationBO.AllGroupHeaders.Load();
				AssertEquals("topGroupInvoice.JobComInvoiceGroupHeaders.Count", 1, topGroupInvoice.JobComInvoiceGroupHeaders.Count);
				AssertEquals("subGroup", subGroup, topGroupInvoice.JobComInvoiceGroupHeaders[0]);
				AssertEquals("subGroup.IsDeleted", false, subGroup.IsDeleted);
				AssertEquals("subGroup.JZ_InvoiceNumber", "GROUP1", subGroup.JZ_InvoiceNumber);
				AssertEquals("declarationBO.Invoices.Count", 2, declarationBO.Invoices.Count);
				AssertEquals("invoice1.IsDeleted", false, invoice1.IsDeleted);
				AssertEquals("invoice2.IsDeleted", true, invoice2.IsDeleted);
				AssertCollectionContains(invoice1, declarationBO.Invoices);
				var invoice3 = declarationBO.Invoices[1];
				if (invoice3 == invoice1)
				{
					invoice3 = declarationBO.Invoices[0];
				}
				AssertEquals("invoice1.JZ_InvoiceNumber", invoiceDataObject1.InvoiceNumber, invoice1.JZ_InvoiceNumber);
				AssertEquals("invoice1.JZ_JZ_GroupInvoiceFK", topGroupInvoice.PK, invoice1.JZ_JZ_GroupInvoiceFK);
				declarationBO.InvoiceLines.Load();
				AssertEquals("invoice1.JobComInvoiceLines.Count", 2, invoice1.JobComInvoiceLines.Count);
				AssertEquals("invoice1Line1.IsDeleted", false, invoice1Line1.IsDeleted);
				AssertCollectionContains(invoice1Line1, invoice1.JobComInvoiceLines);
				AssertEquals("invoice1Line2.IsDeleted", true, invoice1Line2.IsDeleted);
				AssertEquals("invoice1Line3.IsDeleted", true, invoice1Line3.IsDeleted);
				invoice1Line2 = invoice1.JobComInvoiceLines[1];
				if (invoice1Line2 == invoice1Line1)
				{
					invoice1Line2 = invoice1.JobComInvoiceLines[0];
				}
				AssertEquals("invoice1Line1.JI_CL", entry2Line1.PK, invoice1Line1.JI_CL);
				AssertEquals("invoice1Line1.JI_Description", "LINE 1", invoice1Line1.JI_Description);
				AssertEquals("invoice1Line2.JI_CL", entry1Line1.PK, invoice1Line2.JI_CL);
				AssertEquals("invoice1Line2.JI_Description", "LINE 5", invoice1Line2.JI_Description);

				AssertEquals("invoice2Line1.IsDeleted", true, invoice2Line1.IsDeleted);

				AssertEquals("invoice3.JZ_InvoiceNumber", invoiceDataObject2.InvoiceNumber, invoice3.JZ_InvoiceNumber);
				AssertEquals("invoice3.JZ_JZ_GroupInvoiceFK", subGroup.PK, invoice3.JZ_JZ_GroupInvoiceFK);
				AssertEquals("invoice3.JobComInvoiceLines.Count", 1, invoice3.JobComInvoiceLines.Count);
				var invoice3Line1 = invoice3.JobComInvoiceLines[0];
				AssertEquals("invoice3Line1.JI_CL", entry1Line1.PK, invoice3Line1.JI_CL);
				AssertEquals("invoice3Line1.JI_Description", "LINE 6", invoice3Line1.JI_Description);
				Factory.SaveForTesting();

				subGroupDataObject.Name = "GROUP2";
				invoiceLineDataObject6.Description = "LINE 7";
				declarationDataObject.CommercialInfo.CommercialInvoiceCollection = null;
				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				AssertEquals("declarationBO should have been matched", declarationBO, reader.ReadIntoBusinessObject());
				declarationBO.CustomsEntryHeaders.Load();
				AssertEquals("declarationBO.CustomsEntryHeaders.Count", 2, declarationBO.CustomsEntryHeaders.Count);
				AssertCollectionContains(entry1, declarationBO.CustomsEntryHeaders);
				AssertCollectionContains(entry2, declarationBO.CustomsEntryHeaders);
				AssertEquals("entry1.CH_BGMReference", "BG89756/3", entry1.CH_BGMReference);
				entry1.AllEntryLines.Load();
				AssertEquals("entry1.AllEntryLines.Count", 1, entry1.AllEntryLines.Count);
				AssertEquals("entry1Line1.IsDeleted", true, entry1Line1.IsDeleted);
				AssertNotEquals("entry1Line1", entry1Line1, entry1.AllEntryLines[0]);
				entry1Line1 = entry1.AllEntryLines[0];

				AssertEquals("entry2.CH_BGMReference", "BG89756/2", entry2.CH_BGMReference);
				entry2.AllEntryLines.Load();
				AssertEquals("entry2.AllEntryLines.Count", 1, entry2.AllEntryLines.Count);
				AssertEquals("entry2Line1", entry2Line1, entry2.AllEntryLines[0]);
				AssertEquals("topGroupInvoice", topGroupInvoice, declarationBO.TopGroupInvoice);
				AssertEquals("topGroupInvoice.IsDeleted", false, topGroupInvoice.IsDeleted);
				declarationBO.AllGroupHeaders.Load();
				AssertEquals("topGroupInvoice.JobComInvoiceGroupHeaders.Count", 1, topGroupInvoice.JobComInvoiceGroupHeaders.Count);
				AssertEquals("subGroup.IsDeleted", true, subGroup.IsDeleted);
				AssertNotEquals(subGroup, topGroupInvoice.JobComInvoiceGroupHeaders[0]);
				var subGroup2 = topGroupInvoice.JobComInvoiceGroupHeaders[0];
				AssertEquals("subGroup2.JZ_InvoiceNumber", "GROUP2", subGroup2.JZ_InvoiceNumber);
				AssertEquals("declarationBO.Invoices.Count", 2, declarationBO.Invoices.Count);
				AssertEquals("invoice1.IsDeleted", false, invoice1.IsDeleted);
				AssertEquals("invoice3.IsDeleted", false, invoice3.IsDeleted);
				AssertCollectionContains(invoice1, declarationBO.Invoices);
				AssertCollectionContains(invoice3, declarationBO.Invoices);
				AssertEquals("invoice1.JZ_InvoiceNumber", invoiceDataObject1.InvoiceNumber, invoice1.JZ_InvoiceNumber);
				AssertEquals("invoice1.JZ_JZ_GroupInvoiceFK", topGroupInvoice.PK, invoice1.JZ_JZ_GroupInvoiceFK);
				declarationBO.InvoiceLines.Load();
				AssertEquals("invoice1.JobComInvoiceLines.Count", 1, invoice1.JobComInvoiceLines.Count);
				AssertEquals("invoice1Line1.IsDeleted", false, invoice1Line1.IsDeleted);
				AssertEquals("invoice1Line1", invoice1Line1, invoice1.JobComInvoiceLines[0]);
				AssertEquals("invoice1Line2.IsDeleted", true, invoice1Line2.IsDeleted);
				AssertEquals("invoice1Line1.JI_CL", entry2Line1.PK, invoice1Line1.JI_CL);
				AssertEquals("invoice1Line1.JI_Description", "LINE 1", invoice1Line1.JI_Description);

				AssertEquals("invoice3Line1.IsDeleted", true, invoice3Line1.IsDeleted);

				AssertEquals("invoice3.JZ_InvoiceNumber", invoiceDataObject2.InvoiceNumber, invoice3.JZ_InvoiceNumber);
				AssertEquals("invoice3.JZ_JZ_GroupInvoiceFK", subGroup2.PK, invoice3.JZ_JZ_GroupInvoiceFK);
				AssertEquals("invoice3.JobComInvoiceLines.Count", 1, invoice3.JobComInvoiceLines.Count);
				AssertNotEquals("invoice3Line1", invoice3Line1, invoice3.JobComInvoiceLines[0]);
				invoice3Line1 = invoice3.JobComInvoiceLines[0];
				AssertEquals("invoice3Line1.JI_CL", entry1Line1.PK, invoice3Line1.JI_CL);
				AssertEquals("invoice3Line1.JI_Description", "LINE 7", invoice3Line1.JI_Description);
			}
		}

		public void TestCusEntrtyHeaderForIntegratedCountryWithBillDetailsExistsAndOneDeclarationsMatched()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_DeclarationReference = "B00001234";
				declaration.JE_MasterBill = "MYMASTER";
				var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				cusEntryHeader.CH_MessageType = "IMP";
				var entryNum = CusEntryNumber.LoadOrCreate(cusEntryHeader, "B12", Core.Constants.CountryCodes.China);
				entryNum.CE_EntryNum = "B32432";

				Factory.SaveForTesting();

				var declarationDataObject = SetupDeclarationAndEntryHeaders("MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" }, "IMP", ZString.Empty);
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				AssertEquals("B00001234", declarationBO.JE_DeclarationReference);
				AssertCustomsEntryHeadersAndEntryLines(declarationBO);
			}
		}

		public void TestCusEntrtyHeaderForIntegratedCountryWhenMultipleDeclarationsMatchedWithBillDetailsAndEntryDetailsExist()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration1.JE_DeclarationReference = "B00000002";
				declaration1.JE_MasterBill = "MYMASTER";

				var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration2.JE_DeclarationReference = "B00000003";
				declaration2.JE_MasterBill = "MYMASTER";

				Factory.SaveForTesting();

				var declarationDataObject = SetupDeclarationAndEntryHeaders("MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" }, "IMP", ZString.Empty);
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertNull("Importation is rejected.", declarationBO);
				AssertContains("Importation is rejected.", "Cannot import Entry Details when there are multiple declarations matched with same bill details.", logger.Logs);
			}
		}

		public void TestGetExistingBusinessObjectUsingBills_MultipleDeclarationsMatched_MatchingAnyBranch()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration1.JE_DeclarationReference = "B00000002";
				declaration1.JE_MasterBill = "MYMASTER";

				var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration2.JE_DeclarationReference = "B00000003";
				declaration2.JE_MasterBill = "MYMASTER";
				declaration2.JE_GB = GlbCompany.CurrentCompany.Branches.First(x => x.PK != declaration1.JE_GB).PK;
				Factory.SaveForTesting();

				var declarationDataObject = SetupDeclarationAndEntryHeaders("MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" }, "IMP", ZString.Empty);
				declarationDataObject.Branch = new Branch { Code = GlbBranch.CurrentBranch.GB_Code };

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				CombineAssertions(() =>
				{
					AssertNull("Importation is rejected.", declarationBO);
					AssertContains("Importation is rejected.", "Cannot import Entry Details when there are multiple declarations matched with same bill details.", logger.Logs);
				});
			}
		}

		public void TestCustomsCommercialInvoiceOnly_CreateNewDeclaration()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			var declaration = (BaseJobDeclaration)shipment.Declarations.FirstOrDefault();
			AssertNotNull(declaration);
			Assert(declaration.JE_OverrideFreightDefaults);

			shipmentDataObject = GetCustomsCommericalInvoiceOnlyUniversalShipment(null);

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(Logger);
			Factory.FireCleanupAfterSaving();

			CombineAssertions(() =>
			{
				declaration = (BaseJobDeclaration)shipment.Declarations.FirstOrDefault();
				AssertNotNull(declaration);
				Assert("JE_OverrideFreightDefaults is set to false", !declaration.JE_OverrideFreightDefaults);
				AssertEquals("declaration.JE_TransportMode", TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
				AssertEquals("declaration.JE_MasterBill", ZString.Empty, declaration.JE_MasterBill);
				AssertEquals("declaration.JE_HouseBill", "MB1HB2", declaration.JE_HouseBill);
				declaration.CusContainers.Load();
				AssertEquals("declaration.CusContainers.Count", 0, declaration.CusContainers.Count);
				declaration.Bills.Load();
				AssertEquals("declaration.Bills.Count", 1, declaration.Bills.Count);
				declaration.Packages.Load();
				AssertEquals("declaration.Packages.Count", 4, declaration.Packages.Count);
				declaration.PackingGroups.Load();
				AssertEquals("declaration.PackingGroups.Count", 1, declaration.PackingGroups.Count);
				var topGroupInvoice = declaration.TopGroupInvoice;
				AssertEquals("declaration.TopGroupInvoice.JobComInvoiceHeaders.Count", 1, topGroupInvoice.JobComInvoiceHeaders.Count);
				var invoice = topGroupInvoice.JobComInvoiceHeaders[0];
				AssertEquals("declaration.Invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
			});
		}

		public void TestCustomsCommericalInvoiceOnly_ExistingDeclarationMatched()
		{
			var existingShipment = Factory.New<ForwardingShipment>();
			existingShipment.FillWithValidTestData();
			existingShipment.JS_UniqueConsignRef = "S00000001";

			var existingDeclaration = Factory.New<BaseJobDeclaration>();
			existingDeclaration.JE_JS = existingShipment.PK;
			existingDeclaration.JE_OverrideFreightDefaults = true;
			existingDeclaration.JE_TransportMode = TransportTypeList.Codes.Mail;

			var masterBill = existingDeclaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB0001";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillNum = "HB0001";

			var packingGroup = existingDeclaration.PackingGroups[0];
			packingGroup.CR_CU_HouseBill = houseBill.PK;

			var container = Factory.New<BaseCusContainer>();
			container.CO_ContainerNumber = "CN0001";
			packingGroup.CR_CO_Container = container.PK;
			existingDeclaration.CusContainers.Add(container);

			var package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 2;
			package.CW_PackType = "TT";

			var invoiceHeader1 = existingDeclaration.TopGroupInvoice.JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV11";

			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 9;

			var invoiceHeader2 = existingDeclaration.TopGroupInvoice.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV45";

			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 10;
			invoiceLine2.JI_MatchingKey = "MK0002";

			Factory.SaveForTesting();

			var shipmentDataObject = GetCustomsCommericalInvoiceOnlyUniversalShipment(existingShipment.JS_UniqueConsignRef);

			var invoiceCollection = shipmentDataObject.CommercialInfo.CommercialInvoiceCollection;

			invoiceCollection.Add(new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV45",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[]
				{
					new CommercialInvoiceLine()
					{
						LineNo = 2,
						Description = "2",
						DataImportMatchingKey = "MK0002"
					}
				})
			{ Content = CollectionContent.Partial })));

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();
			var declaration = (BaseJobDeclaration)shipment.Declarations.FirstOrDefault();
			Factory.SaveAtEndOfImport(Logger);
			Factory.FireCleanupAfterSaving();

			CombineAssertions(() =>
			{
				AssertEquals("Match Shipment found", existingShipment.PK, shipment.PK);
				AssertEquals("Match Declration found", existingDeclaration.PK, declaration.PK);

				Assert("should not touch JE_OverrideFreightDefaults", declaration.JE_OverrideFreightDefaults);
				AssertEquals("should not touch declaration.JE_TransportMode", TransportTypeList.Codes.Mail, declaration.JE_TransportMode);
				AssertEquals("should not touch declaration.JE_MasterBill", existingDeclaration.JE_MasterBill, declaration.JE_MasterBill);
				AssertEquals("should not touch declaration.JE_HouseBill", existingDeclaration.JE_HouseBill, declaration.JE_HouseBill);

				declaration.CusContainers.Load();
				AssertEquals("should not touch declaration.CusContainers", 1, declaration.CusContainers.Count);
				AssertEquals("should not touch declaration.CusContainers", container.PK, declaration.CusContainers[0].PK);
				AssertEquals("should not touch declaration.CusContainers", container.CO_ContainerNumber, declaration.CusContainers[0].CO_ContainerNumber);

				declaration.Bills.Load();
				AssertEquals("should not touch declaration.Bills", 2, declaration.Bills.Count);
				AssertEquals("should not touch declaration.Bills", masterBill.PK, declaration.Bills.FindByBillType(BillTypeList.Codes.MasterBill)[0].PK);
				AssertEquals("should not touch declaration.Bills", masterBill.CU_BillNum, declaration.Bills.FindByBillType(BillTypeList.Codes.MasterBill)[0].CU_BillNum);
				AssertEquals("should not touch declaration.Bills", houseBill.PK, declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill)[0].PK);
				AssertEquals("should not touch declaration.Bills", houseBill.CU_BillNum, declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill)[0].CU_BillNum);

				declaration.Packages.Load();
				AssertEquals("should not touch declaration.Packages", 1, declaration.Packages.Count);
				AssertEquals("should not touch declaration.Packages", package.PK, declaration.Packages[0].PK);
				AssertEquals("should not touch declaration.Packages", package.CW_PackQty, declaration.Packages[0].CW_PackQty);
				AssertEquals("should not touch declaration.Packages", package.CW_PackType, declaration.Packages[0].CW_PackType);
				declaration.PackingGroups.Load();
				AssertEquals("should not touch declaration.PackingGroups", 1, declaration.PackingGroups.Count);

				var invoiceHeaders = declaration.TopGroupInvoice.JobComInvoiceHeaders.Cast<BaseJobComInvoiceHeader>();
				AssertEquals("declaration.TopGroupInvoice.JobComInvoiceHeaders.Count", 2, invoiceHeaders.Count());

				var expectedInvoiceNumbers = new[] { "INV23", "INV45" };
				var actualInvoiceNumbers = invoiceHeaders.Select(c => c.JZ_InvoiceNumber);

				AssertContainsExactElementsInAnyOrder("should sync all numbers from the data object", expectedInvoiceNumbers, actualInvoiceNumbers);

				Assert("the first existing commercial Invoice Header should be deleted", invoiceHeader1.IsDeleted);
				Assert("the second existing commercial Invoice Header should not be deleted as there is a matching key in the Partial collection", !invoiceHeader2.IsDeleted);

				var newInvoice = invoiceHeaders.First(c => c.JZ_InvoiceNumber == "INV23");

				AssertNotEquals("new commercial Invoice should be created", newInvoice.PK, invoiceHeader1.PK);
				AssertEquals("newInvoice.JobComInvoiceLines.Count", 1, newInvoice.JobComInvoiceLines.Count);
				AssertEquals("new commercial Invoice Line should be created", (short)1, newInvoice.JobComInvoiceLines[0].JI_LineNo);

				newInvoice = invoiceHeaders.First(c => c.JZ_InvoiceNumber == "INV45");

				AssertEquals("existing commercial Invoice should be found", newInvoice.PK, invoiceHeader2.PK);
				AssertEquals("newInvoice.JobComInvoiceLines.Count", 1, newInvoice.JobComInvoiceLines.Count);

				Assert("existing commercial Invoice Line should not be deleted as there is a matching key in the Partial collection", !invoiceLine2.IsDeleted);
				AssertEquals("new commercial Invoice Line should be created but the LineNo recalculated by InvoiceLineLineNumberGenerator after all invoicelines imported", (short)1, invoiceHeader2.JobComInvoiceLines[0].JI_LineNo);
			});
		}

		public void TestCustomsCommericalInvoiceOnly_LandedCostingData()
		{
			var shipmentDataObject = GetCustomsCommericalInvoiceOnlyUniversalShipment(null);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();
			var declaration = (BaseJobDeclaration)shipment.Declarations.FirstOrDefault();
			Factory.SaveAtEndOfImport(Logger);
			Factory.FireCleanupAfterSaving();

			var topGroupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			AssertEquals("declarationBO.Invoices.Count", 1, declaration.Invoices.Count);
			var invoice = declaration.Invoices[0];
			AssertEquals("declarationBO.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			var landedCostHeader = (LandedCosting.ILandedCostHeader)declaration.LandedCostHeaderForDocuments;
			AssertNotNull("landedCostHeader should have been created", landedCostHeader);
			AssertEquals("landedCostHeader.LT_DateOfProcessing should not be imported", ZDateTime.Empty, landedCostHeader.LT_DateOfProcessing);
			var costInputs = Factory.BOFactory.Load<LandedCosting.ILandCostInput>(new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK));
			AssertEquals("costInputs.Length", 3, costInputs.Length);
			var costInput1 = costInputs.FirstOrDefault(x => x.LI_ParentID == topGroupInvoice.PK);
			AssertLandCostInputContents(costInput1, topGroupInvoice);
			var costInput2 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoice.PK);
			AssertLandCostInputContents2(costInput2, invoice);
			var costInput3 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoiceLine.PK);
			AssertLandCostInputContents(costInput3, invoiceLine);
			var histories = Factory.BOFactory.Load<LandedCosting.ILandedCostHistory>(new ZQuery(LandedCostHistorySchema.LH_LT, landedCostHeader.PK));
			AssertEquals("histories should not be imported", 0, histories.Length);

			landedCostHeader.LT_DateOfProcessing = new ZDateTime(2015, 3, 5);
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT7565455";
			var costInput4 = Factory.BOFactory.New<LandedCosting.ILandCostInput>();
			costInput4.LI_AC_ChargeCode = AccChargeCode1.PK;
			costInput4.LI_LT = costInput1.LI_LT;
			costInput4.LI_ParentID = container.PK;
			costInput4.LI_ParentTableCode = container.TablePrefix;
			costInput4.LI_CostAmount = 12m;
			costInput4.LI_ChargeDescription = "BOB 3";
			costInput4.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
			costInput4.LI_DistributeCostBy = "!SD";
			costInput4.LI_LandedCostGroup = (ZByte)4;
			costInput4.LI_ServiceExRate = 1.2321m;

			var costInput5 = Factory.BOFactory.New<LandedCosting.ILandCostInput>();
			costInput5.LI_AC_ChargeCode = AccChargeCode1.PK;
			costInput5.LI_LT = costInput1.LI_LT;
			costInput5.LI_ParentID = invoice.PK;
			costInput5.LI_ParentTableCode = invoice.TablePrefix;
			costInput5.LI_CostAmount = 10m;
			costInput5.LI_ChargeDescription = "BOB 3";
			costInput5.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
			costInput5.LI_DistributeCostBy = "!SD";
			costInput5.LI_LandedCostGroup = (ZByte)4;
			costInput5.LI_ServiceExRate = 1.2321m;

			var costInput6 = Factory.BOFactory.New<LandedCosting.ILandCostInput>();
			costInput6.LI_AC_ChargeCode = AccChargeCode1.PK;
			costInput6.LI_LT = costInput1.LI_LT;
			costInput6.LI_ParentID = invoiceLine.PK;
			costInput6.LI_ParentTableCode = invoiceLine.TablePrefix;
			costInput6.LI_CostAmount = 10.20m;
			costInput6.LI_ChargeDescription = "BOB 3";
			costInput6.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
			costInput6.LI_DistributeCostBy = "!SD";
			costInput6.LI_LandedCostGroup = (ZByte)4;
			costInput6.LI_ServiceExRate = 1.2321m;

			Logger.ClearLogs();
			shipmentDataObject = GetCustomsCommericalInvoiceOnlyUniversalShipment(shipment.JS_UniqueConsignRef);
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);

			CombineAssertions(() =>
			{
				AssertEquals(shipment, reader.ReadIntoBusinessObject());
				AssertEquals(declaration, shipment.Declarations.FirstOrDefault());
				AssertEquals("topGroupInvoice.IsDeleted", false, topGroupInvoice.IsDeleted);
				AssertEquals(topGroupInvoice, declaration.JobComInvoiceGroupHeaders[0]);
				AssertEquals("declaration.Invoices.Count", 1, declaration.Invoices.Count);
				AssertCollectionNotContains(invoice, declaration.Invoices);
				AssertEquals("invoice.IsDeleted", true, invoice.IsDeleted);
				invoice = declaration.Invoices[0];
				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
				AssertCollectionNotContains(invoiceLine, declaration.InvoiceLines);
				AssertEquals("invoiceLine.IsDeleted", true, invoiceLine.IsDeleted);
				invoiceLine = declaration.InvoiceLines[0];
				AssertEquals(landedCostHeader, declaration.LandedCostHeaderForDocuments);
				AssertEquals("landedCostHeader.LT_DateOfProcessing", new ZDateTime(2015, 3, 5), landedCostHeader.LT_DateOfProcessing);

				costInputs = Factory.BOFactory.Load<LandedCosting.ILandCostInput>(new ZQuery(LandCostInputSchema.LI_LT, landedCostHeader.PK));
				AssertEquals("costInputs.Length", 4, costInputs.Length);
				AssertEquals("costInput1.IsDeleted", true, ((BusinessObject)costInput1).IsDeleted);
				AssertEquals("costInput2.IsDeleted", true, ((BusinessObject)costInput2).IsDeleted);
				AssertEquals("costInput3.IsDeleted", true, ((BusinessObject)costInput3).IsDeleted);
				AssertEquals("costInput4.IsDeleted", false, ((BusinessObject)costInput4).IsDeleted);
				AssertEquals("costInput5.IsDeleted", true, ((BusinessObject)costInput5).IsDeleted);
				AssertEquals("costInput6.IsDeleted", true, ((BusinessObject)costInput6).IsDeleted);
				costInput1 = costInputs.FirstOrDefault(x => x.LI_ParentID == topGroupInvoice.PK);
				AssertLandCostInputContents(costInput1, topGroupInvoice);
				costInput2 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoice.PK);
				AssertLandCostInputContents2(costInput2, invoice);
				costInput3 = costInputs.FirstOrDefault(x => x.LI_ParentID == invoiceLine.PK);
				AssertLandCostInputContents(costInput3, invoiceLine);
				AssertEquals(costInput4, costInputs.FirstOrDefault(x => x.LI_ParentID == container.PK));
				AssertLandCostInputContents(costInput4, container, AccChargeCode1.PK, "BOB 3", 12m, Core.Constants.CurrencyCodes.NewZealand, "!SD", 4, 1.2321m);
			});
		}

		public void TestUpdateMessageType()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_DeclarationReference = "B00001000";
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.SaveForTesting();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.DataContext.DataTargetCollection.First().Key = "B00001000";
			declarationDataObject.MessageType = null;

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals(declarationBOToLoad, declarationBO);
			AssertEquals("JE_MessageType should not be changed", JobMessageTypeList.Codes.Import, declarationBO.JE_MessageType);

			declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Export };
			declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals("JE_MessageType should be changed", JobMessageTypeList.Codes.Export, declarationBO.JE_MessageType);
		}

		public void TestDefermentAccountNumber()
		{
			const string defermentAccountNumber = "SINGLEDEFER12345";
			var declarationDataObject = SetupDeclaration(null, "TEST1MB", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.DefermentAccountNumber = defermentAccountNumber;

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals(defermentAccountNumber, declarationBO.JE_DefermentAccountNumber);
		}

		public void TestIATALoadPortIsImported()
		{
			var declarationDataObject = SetupDeclaration(null, "TEST1MB", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.CustomsValuationPort = new CodeDescriptionPair() { Code = "SYD" };

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals("SYD", declarationBO.JE_IATALoadPort);
		}

		public void TestDoMergeMutex()
		{
			localCountryCustomsInterface?.Dispose();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.Invoices.AddNew();
				declaration.FilteredInvoiceLines.AddNew();
				NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = false;
				declaration.DoMerge();
				Factory.SaveForTesting();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext
				};

				using (var mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DoMerge" + declaration.PK.ToString()))
				{
					mutex.Lock();
					var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
					try
					{
						var declarationBO = reader.ReadIntoBusinessObject();
					}
					catch (DataObjectReadFailureException ex)
					{
						AssertContains("is in the process of merging this job; system cannot merge this data as it will result in a different entry details.\r\nPlease retry merging when the other user has finished.", ex.Message);
					}

					mutex.Unlock();
					try
					{
						var declarationBO = reader.ReadIntoBusinessObject();
						Assert(declarationBO.DoMergeMutex.IsLocked);
						Factory.SaveForTesting();
						Assert(!declarationBO.DoMergeMutex.IsLocked);
					}
					catch (DataObjectReadFailureException)
					{
						Assert(false);
					}
				}
				NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = true;
			}
		}

		public void TestImportWorkflowExceptions()
		{
			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			AssertNull("No ExceptionCollection on XML", declarationDataObject.ExceptionCollection);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("No ExceptionCollection, should import without errors ", 0, declarationBO.WorkflowItems.Exceptions.Count);
			AssertContains("Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.", logger.Logs);

			Factory.SaveForTesting();
			logger.ClearLogs();

			declarationDataObject.SetExceptionCollection(() => new List<WorkflowException>());
			declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals("No Exception on ExceptionCollection, should import without errors ", 0, declarationBO.WorkflowItems.Exceptions.Count);
			AssertContains("Information - Successfully loaded matching BaseJobDeclaration.", logger.Logs);

			Factory.SaveForTesting();
			logger.ClearLogs();

			var now = ZDateTimeOffset.Now;

			var workflowException = new[]
			{
				new WorkflowException()
				{
					Description = "Exception 1",
					Date = now,
				},
				new WorkflowException()
				{
					Description = "Exception 2",
					Date = now,
					Actioned = true
				},
			};

			declarationDataObject.SetExceptionCollection(() => workflowException.ToList());
			declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals("Exceptions on ExceptionCollection, should import without errors ", 2, declarationBO.WorkflowItems.Exceptions.Count);

			var exception1 = declarationBO.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 1");
			AssertNotNull("Exception 1 is in declaration", exception1);
			AssertEquals(now, exception1.P9_ActualDateOffset);
			AssertEquals(false, exception1.IsExceptionActioned);

			var exception2 = declarationBO.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 2");
			AssertNotNull("Exception 2 is in declaration", exception2);
			AssertEquals(now, exception2.P9_ActualDateOffset);
			AssertEquals(true, exception2.IsExceptionActioned);

			AssertContains("Information - Populating Exception: Exception 1...", logger.Logs);
			AssertContains("Information - Populating Exception: Exception 2...", logger.Logs);
		}

		public void TestDefaultingFieldsPopulatedAndCanSave_JobDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.Containerised, Description = "Containerized" }
				};
				declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
				var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerNumber = "CONT001",
				};
				declarationDataObject.PackingLineCollection.Add(packingLine1);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();

				AssertNoExceptionThrown(() => Factory.SaveAtEndOfImport(logger));

				declarationBO.CusContainers.Load();
				var cont1 = declarationBO.CusContainers.OfType<BaseCusContainer>().FirstOrDefault(x => x.CO_ContainerNumber == "CONT001");
				AssertNotNull("Should have found a container with number CONT001", cont1);
				AssertEquals("CO_DataModel", "ZA", cont1.CO_DataModel);
			}
		}

		public void TestFCLPickupEquipmentNeededCleared_ForImportDeclaration()
		{
			var message = GetQueuedUniversalShipmentMessage(E2EMessagingImportDeclarationXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should load import declaration.", declaration);
			AssertEquals("declaration.JE_MessageType", "IMP", declaration.JE_MessageType);

			AssertEquals("JP_FCLPickupEquipmentNeeded should be empty", ZString.Empty, declaration.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
			AssertEquals("JP_EstimatedPickup should be empty", ZString.Empty, declaration.DocsAndCartage.JP_EstimatedPickup.ToString());
			AssertEquals("JP_PickupCartageAdvised should be empty", ZString.Empty, declaration.DocsAndCartage.JP_PickupCartageAdvised.ToString());
			AssertEquals("JP_PickupCartageCompleted should be empty", ZString.Empty, declaration.DocsAndCartage.JP_PickupCartageCompleted.ToString());
			AssertEquals("JP_PickupLabourTime should be empty", ZString.Empty, declaration.DocsAndCartage.JP_PickupLabourTime.ToString());
			AssertEquals("JP_PickupLabourCharge should be 0", 0m, declaration.DocsAndCartage.JP_PickupLabourCharge);
			AssertEquals("JP_PickupTruckWaitTime should be empty", ZString.Empty, declaration.DocsAndCartage.JP_PickupTruckWaitTime.ToString());
			AssertEquals("JP_PickupTruckWaitCharge should be 0", 0m, declaration.DocsAndCartage.JP_PickupTruckWaitCharge);
			AssertEquals("JP_LCLAirStorageDaysOrHours should be 0", (byte)0, declaration.DocsAndCartage.JP_LCLAirStorageDaysOrHours);
			AssertEquals("JP_PickupRequiredFrom should be empty", ZString.Empty, declaration.DocsAndCartage.JP_PickupRequiredFrom.ToString());
			AssertEquals("JP_PickupRequiredBy should be empty", ZString.Empty, declaration.DocsAndCartage.JP_PickupRequiredBy.ToString());
		}

		public void TestFCLDeliveryEquipmentNeededCleared_ForExportDeclaration()
		{
			var message = GetQueuedUniversalShipmentMessage(E2EMessagingExportDeclarationXML);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.
".Trim(), serviceTaskLog.ToString());
			});

			var declaration = Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));

			AssertNotNull("Should load export declaration.", declaration);
			AssertEquals("declaration.JE_MessageType", "EXP", declaration.JE_MessageType);

			AssertEquals("JP_FCLDeliveryEquipmentNeeded should be empty", ZString.Empty, declaration.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("JP_FCLStorageCommences should be empty", ZString.Empty, declaration.DocsAndCartage.JP_FCLStorageCommences.ToString());
			AssertEquals("JP_EstimatedDelivery should be empty", ZString.Empty, declaration.DocsAndCartage.JP_EstimatedDelivery.ToString());
			AssertEquals("JP_DeliveryCartageAdvised should be empty", ZString.Empty, declaration.DocsAndCartage.JP_DeliveryCartageAdvised.ToString());
			AssertEquals("JP_DeliveryCartageCompleted should be empty", ZString.Empty, declaration.DocsAndCartage.JP_DeliveryCartageCompleted.ToString());
			AssertEquals("JP_DeliveryLabourTime should be empty", ZString.Empty, declaration.DocsAndCartage.JP_DeliveryLabourTime.ToString());
			AssertEquals("JP_DeliveryLabourCharge should be 0", 0m, declaration.DocsAndCartage.JP_DeliveryLabourCharge);
			AssertEquals("JP_DeliveryTruckWaitTime should be empty", ZString.Empty, declaration.DocsAndCartage.JP_DeliveryTruckWaitTime.ToString());
			AssertEquals("JP_DeliveryTruckWaitCharge should be 0", 0m, declaration.DocsAndCartage.JP_DeliveryTruckWaitCharge);
			AssertEquals("JP_DeliveryRequiredFrom should be empty", ZString.Empty, declaration.DocsAndCartage.JP_DeliveryRequiredFrom.ToString());
			AssertEquals("JP_DeliveryRequiredBy should be empty", ZString.Empty, declaration.DocsAndCartage.JP_DeliveryRequiredBy.ToString());
		}

		protected UniversalShipment GetNewOrderDataObject()
		{
			var consigneeAddressCRAHOLSYDDataObject = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			var consigneeOrgCRAHOLSYD = new OrganisationDataObjectReader(consigneeAddressCRAHOLSYDDataObject, new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			Factory.SaveForTesting();

			var orderDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderDataObject.DataContext = DataContextFactory.New();
			orderDataObject.DataContext.AddDataTarget(DataContextType.OrderManagerOrder, null);
			orderDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderDataObject.OrganizationAddressCollection.Add(OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress));
			return orderDataObject;
		}

		void AssertFillValueOnOrganisation(DocAddressType addressType, string columnName)
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.AddOrgAddress(writeManager, testOrg, addressType);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			AssertEquals(testOrg.PK, declarationBO[columnName]);
		}

		void AssertFreightOverrideSynchronisationData(ZBool overrideFreightDefaults, ZString customsContainerData)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var unmatchedOrgPK = OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalTestFiles("UniversalShipmentWithShipmentAndConsol.xml")).Replace("<!-- CustomsContainerMode -->", customsContainerData));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("Service Task Log", @"
Added Declaration (Master Bill='OB1510121218' House Bill='HB1510121218') from UniversalShipment.
Added Shipment (House Bill='HB1510121218') from UniversalShipment.
Added Consol (Master Bill='OB1510121218') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='OB1510121218') with 2 x ForwardingContainer, 1 x Transport, 1 x ForwardingShipmentStmNote, 2 x ForwardingPackLine, 2 x Bill, 2 x CusContainer, 2 x Package, 1 x GroupInvoiceCharge, 1 x JobDeclaration, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertContains("message.GetLogNoteText()", @"
Added Declaration (Master Bill='OB1510121218' House Bill='HB1510121218') from UniversalShipment.
Added Shipment (House Bill='HB1510121218') from UniversalShipment.
Added Consol (Master Bill='OB1510121218') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='OB1510121218') with 2 x ForwardingContainer, 1 x Transport, 1 x ForwardingShipmentStmNote, 2 x ForwardingPackLine, 2 x Bill, 2 x CusContainer, 2 x Package, 1 x GroupInvoiceCharge, 1 x JobDeclaration, 1 x ForwardingShipment.
".Trim(), logNoteText);

					var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, "C00001000"));
					AssertEquals("consol.Shipments.Count", 1, consol.Shipments.Count);
					var shipment = consol.Shipments[0];
					var declarations = shipment.Declarations;
					AssertEquals("declarations.Length", 1, declarations.Length);
					var declaration = (BaseJobDeclaration)declarations[0];
					AssertEquals("declaration.JE_OverrideFreightDefaults", overrideFreightDefaults, declaration.JE_OverrideFreightDefaults);
				});
			}
		}

		void AssertContainerMode_FromCustomsContainerMode(string messagetype, string containerModeInXml, string expectedContainerMode)
		{
			var declarationDataContext = DataContextFactory.New();
			declarationDataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001387");
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = declarationDataContext,
				ContainerMode = new ContainerMode { Code = containerModeInXml },
			};
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "121212" } });

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				MessageType = new CodeDescriptionPair { Code = messagetype, Description = messagetype },
				ContainerMode = new ContainerMode { Code = containerModeInXml },
				CustomsContainerMode = new ContainerMode { Code = containerModeInXml },
			};
			shipment.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "121212" } });

			var reader = new JobDeclarationDataObjectReader(shipment, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(expectedContainerMode, declarationBO.JE_ContainerMode);
		}

		void AssertContainerMode_FromShipment(string messagetype, string containerModeInXml, string expectedContainerMode)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				MessageType = new CodeDescriptionPair { Code = messagetype, Description = messagetype },
				ContainerMode = new ContainerMode { Code = containerModeInXml },
			};
			shipment.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "121212" } });

			var reader = new JobDeclarationDataObjectReader(shipment, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(expectedContainerMode, declarationBO.JE_ContainerMode);
		}

		void AssertContainerMode_FromSubShipment(string messagetype, string containerModeInXml, string expectedContainerMode)
		{
			var declarationDataContext = DataContextFactory.New();
			declarationDataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001387");
			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = declarationDataContext,
				ContainerMode = new ContainerMode { Code = containerModeInXml },
			};
			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "121212" } });

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				MessageType = new CodeDescriptionPair { Code = messagetype, Description = messagetype },
			};
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { declarationDataObject });

			var reader = new JobDeclarationDataObjectReader(shipment, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(expectedContainerMode, declarationBO.JE_ContainerMode);
		}

		UniversalShipment SetupDeclarationAndEntryHeaders(ZString wayBillNumber, WayBillType wayBillType, ZString entryHeaderType, ZString entryHeaderReference)
		{
			var declarationDataObject = SetupDeclaration(null, wayBillNumber, wayBillType);
			var entryHeaderDataObject = SetupEntryHeader(entryHeaderType, "SNT", ZString.Empty, entryHeaderReference, 0m, null, null);
			entryHeaderDataObject.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber() });
			entryHeaderDataObject.EntryLineCollection = new List<EntryLine>(new[] { SetupEntryLine() });
			entryHeaderDataObject.EntryHeaderChargeCollection = new List<EntryHeaderCharge>(new[] { SetupEntryHeaderCharge() });
			entryHeaderDataObject.RelatedEntryHeaderCollection = new List<EntryHeader>(new[]
					{
						SetupEntryHeader("IM$", "MS1", "ES1", "BG89756", 869.54m, new ZDateTime(2012, 4, 4), new ZDateTime(2012, 4, 5))
					});
			declarationDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>() { entryHeaderDataObject });

			return declarationDataObject;
		}

		void AssertCustomsEntryHeadersAndEntryLines(BaseJobDeclaration declarationBO)
		{
			declarationBO.CustomsEntryHeaders.Load();
			AssertEquals("2 EntryHeaders are imported.", 2, declarationBO.CustomsEntryHeaders.Count);
			AssertEquals("IMP", declarationBO.CustomsEntryHeaders[0].CH_MessageType);
			AssertEquals("IM$", declarationBO.CustomsEntryHeaders[1].CH_MessageType);

			declarationBO.CustomsEntryHeaders[0].AllEntryLines.Load();
			AssertEquals("1 EntryLine is imported.", 1, declarationBO.CustomsEntryHeaders[0].AllEntryLines.Count);

			declarationBO.CustomsEntryHeaders[1].AllEntryLines.Load();
			AssertEquals("No EntryLines are imported.", 0, declarationBO.CustomsEntryHeaders[1].AllEntryLines.Count);
		}

		UniversalShipment GetCustomsCommericalInvoiceOnlyUniversalShipment(string shipmentReference)
		{
			var masterBill1 = SetupAdditionalBill("MB1", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, null, 10m, Core.Constants.PkgUnit.Box, "BOX");
			var masterBill2 = SetupAdditionalBill("MB2", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 4, 1), null, null, 20m, Core.Constants.PkgUnit.Package, "Package");
			var masterBill1HouseBill1 = SetupAdditionalBill("MB1HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 2), "MB1", null, 6m, Core.Constants.PkgUnit.Piece, "Piece");
			var masterBill1HouseBill2 = SetupAdditionalBill("MB1HB2", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 3, 3), "MB1", null, 4m, Core.Constants.PkgUnit.Unit, "Unit");
			var masterBill2HouseBill1 = SetupAdditionalBill("MB2HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 4, 2), "MB2", null, 20m, Core.Constants.PkgUnit.Unit, "Unit");
			var masterBill1HouseBill2SubHouse1 = SetupAdditionalBill("MB1HB2SB1", WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse, new ZDateTime(2011, 3, 4), "MB1HB2", null, 4m, Core.Constants.PkgUnit.Bundle, "Bundle");
			var masterBill2HouseBill1SubHouse1 = SetupAdditionalBill("MB2HB1SB1", WayBillTypeList.Codes.SubHouse, WayBillTypeList.Descriptions.SubHouse, new ZDateTime(2011, 4, 3), "MB2HB1", null, 20m, Core.Constants.PkgUnit.Box, "BOX");

			var containerDataObject1 = SetupContainer();
			var containerDataObject2 = SetupContainer2();
			var packingLineDataObject1 = SetupPackingLine(masterBill1HouseBill2.BillNumber, masterBill1HouseBill2.BillType, containerDataObject1.ContainerNumber);
			var packingLineDataObject2 = SetupPackingLine(masterBill1.BillNumber, masterBill1.BillType, containerDataObject2.ContainerNumber);
			var packingLineDataObject3 = SetupPackingLine2(masterBill1HouseBill2.BillNumber, masterBill1HouseBill2.BillType, containerDataObject1.ContainerNumber);
			var packingLineDataObject4 = SetupPackingLine(masterBill2HouseBill1SubHouse1.BillNumber, masterBill2HouseBill1SubHouse1.BillType, containerDataObject2.ContainerNumber, "MARKS4", 10, new PackageType() { Code = "NO", Description = "Number" }, 11, 5, "SYMBOL4");

			var commercialInfo = new CommercialInfo()
			{
				DateOfLandedCostProcessing = new ZDateTime(2015, 1, 25),
				TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost() }),
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV23",
						TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost2() }),
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[]
						{
							new CommercialInvoiceLine()
							{
								LineNo = 1,
								Description = "1",
								LandedCostDetail = new LandedCostDetail(DefaultDataObjectWriterStrategy.TestInstance) { MarkUp1 = 10m },
								TransportLogisticsCostCollection = new List<TransportLogisticsCost>(new[] { SetupTransportLogisticsCost() })
							}
						})))
				})
			};

			var shipmentDataObject = SetupDeclaration(null, masterBill1HouseBill2.BillNumber, masterBill1HouseBill2.BillType, null, DataContextType.CustomsCommercialInvoice);
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentReference);
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject2, containerDataObject1 }));
			shipmentDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill1, masterBill2, masterBill1HouseBill1, masterBill1HouseBill2, masterBill2HouseBill1, masterBill1HouseBill2SubHouse1, masterBill2HouseBill1SubHouse1 }));
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineDataObject1, packingLineDataObject2, packingLineDataObject3, packingLineDataObject4 }));
			shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;
			shipmentDataObject.CommercialInfo = commercialInfo;

			return shipmentDataObject;
		}

		const string ShipmentDeclarationWithSupplierImporterPickupDeliveryDocumentaryDocAddress = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <Shipment>
        <DataContext>
            <DataProvider>SDBIT</DataProvider>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <AgentsReference>A80096</AgentsReference>
        <AWBServiceLevel>
            <Code>STD</Code>
            <Description>Standard</Description>
        </AWBServiceLevel>
        <ContainerCount>1</ContainerCount>
        <ContainerMode>
            <Code>LSE</Code>
            <Description>Loose</Description>
        </ContainerMode>
        <FreightRateCurrency>
            <Code>AUD</Code>
            <Description>Australia, Dollars</Description>
        </FreightRateCurrency>
        <IsCFSRegistered>true</IsCFSRegistered>
        <IsDirectBooking>false</IsDirectBooking>
        <IsForwardRegistered>false</IsForwardRegistered>
        <IsNeutralMaster>false</IsNeutralMaster>
        <ContainerCollection />
        <DateCollection>
            <Date>
                <Type>Departure</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2018-01-16 13:58:00.0</Value>
            </Date>
            <Date>
                <Type>Arrival</Type>
                <IsEstimate>true</IsEstimate>
                <Value>2018-01-17 06:30:00.0</Value>
            </Date>
        </DateCollection>
        <OrganizationAddressCollection>
            <OrganizationAddress>
                <AddressType>ReceivingForwarderAddress</AddressType>
                <CompanyName>SAVINO DEL BENE AUSTRALIA PTY LTD</CompanyName>
                <OrganizationCode>SAVDELSYD</OrganizationCode>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>ShippingLineAddress</AddressType>
                <CompanyName>QANTAS AIRWAYS LTD.</CompanyName>
                <OrganizationCode>QANAIRSYD</OrganizationCode>
            </OrganizationAddress>
            <OrganizationAddress>
                <AddressType>SendingForwarderAddress</AddressType>
                <Address1>Unit B, 15/F., Kings Wing Plaza 2,1 On Kwan Street, Shatin, New Territories</Address1>
                <City>Hong Kong</City>
                <CompanyName>Savino Del Bene China ltd Hong Kong</CompanyName>
                <Contact>USE INTERNAL E:davidg @sdb.it</Contact>
                <Country>
                    <Code>HK</Code>
                </Country>
                <Email>hongkonghq @savinodelbene.com</Email>
                <Fax>+852 2330 8728</Fax>
                <GovRegNum>16814138-000</GovRegNum>
                <GovRegNumType>
                    <Code>ABN</Code>
                    <Description>Australian Business Number</Description>
                </GovRegNumType>
                <Mobile>+852 2330 8555</Mobile>
                <Postcode></Postcode>
            </OrganizationAddress>
        </OrganizationAddressCollection>
        <SubShipmentCollection>
            <SubShipment>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                        </DataTarget>
                        <DataTarget>
                            <Type>CustomsDeclaration</Type>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <ActualChargeable>24.000</ActualChargeable>
                <OrganizationAddressCollection>
                    <OrganizationAddress>
                        <AddressType>ConsigneeDocumentaryAddress</AddressType>
                        <Address1>86-108 CASTLEREAGH STREET</Address1>
                        <City>SYDNEY</City>
                        <CompanyName>DAVID JONES LIMITED</CompanyName>
                        <Contact> Leanne Scott</Contact>
                        <Country>
                            <Code>AU</Code>
                        </Country>
                        <Email></Email>
                        <Fax></Fax>
                        <GovRegNumType>
                            <Code>ABN</Code>
                            <Description>Australian Business Number</Description>
                        </GovRegNumType>
                        <Mobile></Mobile>
                        <OrganizationCode>DAVJONSYD2</OrganizationCode>
                        <Postcode>2000</Postcode>
                    </OrganizationAddress>
                    <OrganizationAddress>
                        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
                        <Address1>86-108 CASTLEREAGH STREET</Address1>
                        <City>SYDNEY</City>
                        <CompanyName>DAVID JONES LIMITED</CompanyName>
                        <Contact> Leanne Scott</Contact>
                        <Country>
                            <Code>AU</Code>
                        </Country>
                        <Email></Email>
                        <Fax></Fax>
                        <GovRegNumType>
                            <Code>ABN</Code>
                            <Description>Australian Business Number</Description>
                        </GovRegNumType>
                        <Mobile></Mobile>
                        <OrganizationCode>DAVJONSYD2</OrganizationCode>
                        <Postcode>2000</Postcode>
                    </OrganizationAddress>
                    <OrganizationAddress>
                        <AddressType>ConsignorDocumentaryAddress</AddressType>
                        <Address1>VIALE DEI MILLE, 37</Address1>
                        <City>MILANO</City>
                        <CompanyName>3.1 PHILLIP LIM INTERNATIONAL LLC</CompanyName>
                        <Country>
                            <Code>IT</Code>
                        </Country>
                        <Email></Email>
                        <Fax></Fax>
                        <GovRegNum>IT07544270965</GovRegNum>
                        <GovRegNumType>
                            <Code>ABN</Code>
                            <Description>Australian Business Number</Description>
                        </GovRegNumType>
                        <Mobile></Mobile>
                        <Postcode>20129</Postcode>
                    </OrganizationAddress>
                    <OrganizationAddress>
                        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
                        <Address1>VIALE DEI MILLE, 37</Address1>
                        <City>MILANO</City>
                        <CompanyName>3.1 PHILLIP LIM INTERNATIONAL LLC</CompanyName>
                        <Country>
                            <Code>IT</Code>
                        </Country>
                        <Email></Email>
                        <Fax></Fax>
                        <GovRegNum>IT07544270965</GovRegNum>
                        <GovRegNumType>
                            <Code>ABN</Code>
                            <Description>Australian Business Number</Description>
                        </GovRegNumType>
                        <Mobile></Mobile>
                        <Postcode>20129</Postcode>
                    </OrganizationAddress>
                    <OrganizationAddress>
                        <AddressType>SendersLocalClient</AddressType>
                        <Address1>86-108 CASTLEREAGH STREET</Address1>
                        <City>SYDNEY</City>
                        <CompanyName>DAVID JONES LIMITED</CompanyName>
                        <Contact> Leanne Scott</Contact>
                        <Country>
                            <Code>AU</Code>
                        </Country>
                        <Email></Email>
                        <Fax></Fax>
                        <GovRegNumType>
                            <Code>ABN</Code>
                            <Description>Australian Business Number</Description>
                        </GovRegNumType>
                        <Mobile></Mobile>
                        <OrganizationCode>DAVJONSYD2</OrganizationCode>
                        <Postcode>2000</Postcode>
                    </OrganizationAddress>
                    <OrganizationAddress>
                        <AddressType>ImportBroker</AddressType>
                        <Address1>Suite 1, 247 King Street</Address1>
                        <City>Mascot</City>
                        <CompanyName>Savino Del Bene Australia Pty Ltd Sydney</CompanyName>
                        <Contact> Daniele Rea</Contact>
                        <Country>
                            <Code>AU</Code>
                        </Country>
                        <Email>sydney @savinodelbene.com</Email>
                        <Fax>+61 29 289 3444</Fax>
                        <GovRegNum>69098199270</GovRegNum>
                        <GovRegNumType>
                            <Code>ABN</Code>
                            <Description>Australian Business Number</Description>
                        </GovRegNumType>
                        <Mobile>+61 29 289 3400</Mobile>
                        <OrganizationCode>SAVDELSYD</OrganizationCode>
                        <Postcode>2020</Postcode>
                    </OrganizationAddress>
                </OrganizationAddressCollection>
            </SubShipment>
        </SubShipmentCollection>
    </Shipment>
</UniversalShipment>";

		const string DeclarationWithPackingImportedIntoDecNotSupportPacking = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>BillOfLading</Type>
          <Key>V00001001</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DUK</Code>
        <Country>
          <Code>GB</Code>
          <Name>United Kingdom</Name>
        </Country>
        <Name>UK Demo Company</Name>
      </Company>
      <DataProvider>HYESNLDUK</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>LON</Code>
        <Name>LONDON BRANCH</Name>
      </EventBranch>
      <EventDepartment>
        <Code>FES</Code>
        <Name>Forwarding Export Sea</Name>
      </EventDepartment>
      <EventType>
        <Code></Code>
      </EventType>
      <EventUser>
        <Code>SNL</Code>
        <Name>Sharon Leeson</Name>
      </EventUser>
      <ServerID>SNL</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2013-07-25T09:07:34.433</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <BookingConfirmationReference></BookingConfirmationReference>
    <CFSReference>V00001001</CFSReference>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <GoodsDescription>GENERAL PURPOSE GOODS</GoodsDescription>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsShipping>true</IsShipping>
    <NoCopyBills>1</NoCopyBills>
    <NoOriginalBills>0</NoOriginalBills>
    <OuterPacks>20</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PaymentMethod>
      <Code>PPD</Code>
      <Description>Prepaid</Description>
    </PaymentMethod>
    <PaidBy>
      <Code>BRK</Code>
      <Description>Broker</Description>
    </PaidBy>
    <PortOfDestination>
      <Code>AGANU</Code>
      <Name>Antigua</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>AGANU</Code>
      <Name>Antigua</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>GBPME</Code>
      <Name>Portsmouth</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>GBLON</Code>
      <Name>London</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>EBL</Code>
      <Description>Express Bill of Lading</Description>
    </ReleaseType>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentStatus>
      <Code>CNF</Code>
      <Description>Confirmed</Description>
    </ShipmentStatus>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <TotalVolume>46.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>AUTO ATLAS</VesselName>
    <VoyageFlightNo>S11</VoyageFlightNo>
    <WayBillNumber>V00001001</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AdditionalReferenceCollection>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <AirVentFlow>0.0</AirVentFlow>
        <AirVentFlowRateUnit>
          <Code></Code>
        </AirVentFlowRateUnit>
        <ArrivalCartageAdvised></ArrivalCartageAdvised>
        <ArrivalCartageComplete></ArrivalCartageComplete>
        <ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
        <ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
        <ArrivalCartageRef></ArrivalCartageRef>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
        <ArrivalPickupByRail>false</ArrivalPickupByRail>
        <ArrivalSlotDateTime></ArrivalSlotDateTime>
        <ArrivalSlotReference></ArrivalSlotReference>
        <Commodity>
          <Code></Code>
        </Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
        <ContainerDetentionDays>0</ContainerDetentionDays>
        <ContainerImportDORelease></ContainerImportDORelease>
        <ContainerNumber>SCCU5214794</ContainerNumber>
        <ContainerParkEmptyPickupGateOut>2013-05-25T00:00:00</ContainerParkEmptyPickupGateOut>
        <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
        <ContainerQuality>
          <Code></Code>
        </ContainerQuality>
        <ContainerStatus>
          <Code></Code>
        </ContainerStatus>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DeliveryMode></DeliveryMode>
        <DeliverySequence>0</DeliverySequence>
        <DepartureCartageAdvised></DepartureCartageAdvised>
        <DepartureCartageComplete></DepartureCartageComplete>
        <DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
        <DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
        <DepartureCartageRef></DepartureCartageRef>
        <DepartureDeliveryByRail>false</DepartureDeliveryByRail>
        <DepartureDockReceipt></DepartureDockReceipt>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime></DepartureSlotDateTime>
        <DepartureSlotReference></DepartureSlotReference>
        <DunnageWeight>0.000</DunnageWeight>
        <EmptyReadyForReturn></EmptyReadyForReturn>
        <EmptyRequired></EmptyRequired>
        <EmptyReturnedBy></EmptyReturnedBy>
        <EmptyReturnRef></EmptyReturnRef>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <FCL_LCL_AIR>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </FCL_LCL_AIR>
        <FCLAvailable></FCLAvailable>
        <FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
        <FCLOnBoardVessel>2013-05-30T00:00:00</FCLOnBoardVessel>
        <FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
        <FCLStorageCharge>0.0000</FCLStorageCharge>
        <FCLStorageCommences></FCLStorageCommences>
        <FCLStorageDays>0</FCLStorageDays>
        <FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
        <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
        <FCLUnloadFromVessel></FCLUnloadFromVessel>
        <FCLWharfGateIn>2013-05-29T00:00:00</FCLWharfGateIn>
        <FCLWharfGateOut></FCLWharfGateOut>
        <GoodsValue>0.0000</GoodsValue>
        <GoodsValueCurrency>
          <Code></Code>
        </GoodsValueCurrency>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>12380.000</GrossWeight>
        <HumidityPercent>0</HumidityPercent>
        <IsCFSRegistered>false</IsCFSRegistered>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsDamaged>false</IsDamaged>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <IsShipperOwned>false</IsShipperOwned>
        <LCLAvailable></LCLAvailable>
        <LCLStorageCommences></LCLStorageCommences>
        <LCLUnpack></LCLUnpack>
        <LengthUnit>
          <Code>FT</Code>
          <Description>Feet</Description>
        </LengthUnit>
        <Link>1</Link>
        <OverhangBack>0.000</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0.000</OverhangRight>
        <OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
        <OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
        <PackDate></PackDate>
        <RefrigGeneratorID></RefrigGeneratorID>
        <ReleaseNum></ReleaseNum>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <SetPointTemp>0.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <StowagePosition></StowagePosition>
        <TareWeight>2280.000</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <TotalHeight>8.500</TotalHeight>
        <TotalLength>20.000</TotalLength>
        <TotalWidth>8.000</TotalWidth>
        <TrainWagonNumber></TrainWagonNumber>
        <UnpackGang></UnpackGang>
        <UnpackShed></UnpackShed>
        <VolumeCapacity>0.000</VolumeCapacity>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <WeightCapacity>0.000</WeightCapacity>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>

      </Container>
      <Container>
        <AirVentFlow>0.0</AirVentFlow>
        <AirVentFlowRateUnit>
          <Code></Code>
        </AirVentFlowRateUnit>
        <ArrivalCartageAdvised></ArrivalCartageAdvised>
        <ArrivalCartageComplete></ArrivalCartageComplete>
        <ArrivalCartageDemurrageCharge>0.0000</ArrivalCartageDemurrageCharge>
        <ArrivalCartageDemurrageTime></ArrivalCartageDemurrageTime>
        <ArrivalCartageRef></ArrivalCartageRef>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ArrivalEstimatedDelivery></ArrivalEstimatedDelivery>
        <ArrivalPickupByRail>false</ArrivalPickupByRail>
        <ArrivalSlotDateTime></ArrivalSlotDateTime>
        <ArrivalSlotReference></ArrivalSlotReference>
        <Commodity>
          <Code></Code>
        </Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerDetentionCharge>0.0000</ContainerDetentionCharge>
        <ContainerDetentionDays>0</ContainerDetentionDays>
        <ContainerImportDORelease></ContainerImportDORelease>
        <ContainerNumber>MAEU1454788</ContainerNumber>
        <ContainerParkEmptyPickupGateOut></ContainerParkEmptyPickupGateOut>
        <ContainerParkEmptyReturnGateIn></ContainerParkEmptyReturnGateIn>
        <ContainerQuality>
          <Code></Code>
        </ContainerQuality>
        <ContainerStatus>
          <Code></Code>
        </ContainerStatus>
        <ContainerType>
          <Code>20GP</Code>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DeliveryMode></DeliveryMode>
        <DeliverySequence>0</DeliverySequence>
        <DepartureCartageAdvised></DepartureCartageAdvised>
        <DepartureCartageComplete></DepartureCartageComplete>
        <DepartureCartageDemurrageCharge>0.0000</DepartureCartageDemurrageCharge>
        <DepartureCartageDemurrageTime></DepartureCartageDemurrageTime>
        <DepartureCartageRef></DepartureCartageRef>
        <DepartureDeliveryByRail>false</DepartureDeliveryByRail>
        <DepartureDockReceipt></DepartureDockReceipt>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime></DepartureSlotDateTime>
        <DepartureSlotReference></DepartureSlotReference>
        <DunnageWeight>0.000</DunnageWeight>
        <EmptyReadyForReturn></EmptyReadyForReturn>
        <EmptyRequired>2012-10-06T00:00:00</EmptyRequired>
        <EmptyReturnedBy></EmptyReturnedBy>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <FCL_LCL_AIR>
          <Code>FCL</Code>
          <Description>Full Container Load</Description>
        </FCL_LCL_AIR>
        <FCLAvailable></FCLAvailable>
        <FCLHeldInTransitStaging>false</FCLHeldInTransitStaging>
        <FCLOnBoardVessel></FCLOnBoardVessel>
        <FCLStorageArrivedUnderbond>false</FCLStorageArrivedUnderbond>
        <FCLStorageCharge>0.0000</FCLStorageCharge>
        <FCLStorageCommences></FCLStorageCommences>
        <FCLStorageDays>0</FCLStorageDays>
        <FCLStorageModuleOnlyMaster></FCLStorageModuleOnlyMaster>
        <FCLStorageUnderbondCleared></FCLStorageUnderbondCleared>
        <FCLUnloadFromVessel></FCLUnloadFromVessel>
        <FCLWharfGateIn></FCLWharfGateIn>
        <FCLWharfGateOut></FCLWharfGateOut>
        <GoodsValue>0.0000</GoodsValue>
        <GoodsValueCurrency>
          <Code></Code>
        </GoodsValueCurrency>
        <GoodsWeight>0</GoodsWeight>
        <GrossWeight>12392.000</GrossWeight>
        <HumidityPercent>0</HumidityPercent>
        <IsCFSRegistered>false</IsCFSRegistered>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsDamaged>false</IsDamaged>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsSealOk>true</IsSealOk>
        <IsShipperOwned>false</IsShipperOwned>
        <LCLAvailable></LCLAvailable>
        <LCLStorageCommences></LCLStorageCommences>
        <LCLUnpack></LCLUnpack>
        <LengthUnit>
          <Code>FT</Code>
          <Description>Feet</Description>
        </LengthUnit>
        <Link>2</Link>
        <OverhangBack>0.000</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0.000</OverhangRight>
        <OverrideFCLAvailableStorage>false</OverrideFCLAvailableStorage>
        <OverrideLCLAvailableStorage>false</OverrideLCLAvailableStorage>
        <PackDate></PackDate>
        <RefrigGeneratorID></RefrigGeneratorID>
        <ReleaseNum></ReleaseNum>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <SetPointTemp>0.000</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <StowagePosition></StowagePosition>
        <TareWeight>2280.000</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <TotalHeight>8.500</TotalHeight>
        <TotalLength>20.000</TotalLength>
        <TotalWidth>8.000</TotalWidth>
        <TrainWagonNumber></TrainWagonNumber>
        <UnpackGang></UnpackGang>
        <UnpackShed></UnpackShed>
        <VolumeCapacity>0.000</VolumeCapacity>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <WeightCapacity>0.000</WeightCapacity>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>

      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2012-10-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2012-10-21T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2012-11-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2013-05-30T00:00:00</Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2013-05-30T00:00:00</Value>
      </Date>
    </DateCollection>

    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>SCCU5214794</ContainerNumber>
        <ContainerPackingOrder>1</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <DetailedDescription>GENERAL PURPOSE GOODS</DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>0.000</Height>
        <ItemNo>0</ItemNo>
        <Length>0.000</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Meters</Description>
        </LengthUnit>
        <LinePrice>0.0000</LinePrice>
        <LoadingMeters>0.000</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0.000</OutturnedHeight>
        <OutturnedLength>0.000</OutturnedLength>
        <OutturnedVolume>0.000</OutturnedVolume>
        <OutturnedWeight>0.000</OutturnedWeight>
        <OutturnedWidth>0.000</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <Volume>23.000</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <Weight>0.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0.000</Width>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code>GEN</Code>
          <Description>General</Description>
        </Commodity>
        <ContainerLink>2</ContainerLink>
        <ContainerNumber>MAEU1454788</ContainerNumber>
        <ContainerPackingOrder>1</ContainerPackingOrder>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <DetailedDescription>GENERAL PURPOSE GOODS</DetailedDescription>
        <EndItemNo>0</EndItemNo>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode></HarmonisedCode>
        <Height>0.000</Height>
        <ItemNo>0</ItemNo>
        <Length>0.000</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Meters</Description>
        </LengthUnit>
        <LinePrice>0.0000</LinePrice>
        <LoadingMeters>0.000</LoadingMeters>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <OutturnDamagedQty>0</OutturnDamagedQty>
        <OutturnedHeight>0.000</OutturnedHeight>
        <OutturnedLength>0.000</OutturnedLength>
        <OutturnedVolume>0.000</OutturnedVolume>
        <OutturnedWeight>0.000</OutturnedWeight>
        <OutturnedWidth>0.000</OutturnedWidth>
        <OutturnPillagedQty>0</OutturnPillagedQty>
        <OutturnQty>0</OutturnQty>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <ReferenceNumber></ReferenceNumber>
        <Volume>23.000</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <Weight>0.000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>0.000</Width>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

		const string DeclarationWithBothForwardersXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>

        <AddressType>ReceivingForwarderAddress</AddressType>
        <AddressShortCode>PST: LEVEL 3 BAYVIEW TOWE</AddressShortCode>
        <OrganizationCode>FAMPAC</OrganizationCode>
        <Address1>LEVEL 3 BAYVIEW TOWER</Address1>
        <Address2>1753 BOTANY RD, BANKSMEADOW  NSW</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>FAMOUS PACIFIC SHIPPING AUSTRALIA P/L</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>2019</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AddressShortCode>PST: 2ND FLOOR, 482 KINGS</AddressShortCode>
        <OrganizationCode>ACAINT</OrganizationCode>
        <Address1>2ND FLOOR, 482 KINGSFORD SMITH DRIVE</Address1>
        <Address2>HAMILTON, QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>ACA INTERNATIONAL PTY LTD</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode>4007</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithBothCTOXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
<AddressType>ArrivalCTOAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
        <AddressShortCode>LACHLANS LOGISTICS</AddressShortCode>
        <OrganizationCode>LACLOG</OrganizationCode>
        <Address1>1 LACHLAN STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>LACHLANS LOGISTICS COMPANY</CompanyName>
        <Contact>LACHLAN</Contact>
        <Country>
          <Code>AU</Code>
          <Name>AUSTRALIA</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>SYDNEY</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithOverrideWithCTO = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
<AddressType>ArrivalCTOAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithBothCFSXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
<AddressType>ArrivalCFSAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <AddressShortCode>LACHLANS LOGISTICS</AddressShortCode>
        <OrganizationCode>LACLOG</OrganizationCode>
        <Address1>1 LACHLAN STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>LACHLANS LOGISTICS COMPANY</CompanyName>
        <Contact>LACHLAN</Contact>
        <Country>
          <Code>AU</Code>
          <Name>AUSTRALIA</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>SYDNEY</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithOverrideWithCFS = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
<AddressType>ArrivalCFSAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithBothCTYXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
<AddressType>ContainerYardEmptyReturnAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ContainerYardEmptyPickupAddress</AddressType>
        <AddressShortCode>LACHLANS LOGISTICS</AddressShortCode>
        <OrganizationCode>LACLOG</OrganizationCode>
        <Address1>1 LACHLAN STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>LACHLANS LOGISTICS COMPANY</CompanyName>
        <Contact>LACHLAN</Contact>
        <Country>
          <Code>AU</Code>
          <Name>AUSTRALIA</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>SYDNEY</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string DeclarationWithOverrideWithCTY = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

			<RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <MessageType>
      <Code>{0}</Code>
    </MessageType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
<AddressType>ContainerYardEmptyReturnAddress</AddressType>
        <AddressShortCode>ANDREWS LOGISTICS</AddressShortCode>
        <OrganizationCode>ANDLOG</OrganizationCode>
        <Address1>1 ANDREW ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>ANDREW</City>
        <CompanyName>ANDREWS LOGISTICS SOLUTIONS</CompanyName>
        <Contact>ANDREW</Contact>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2036</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string MinimalistImportDeclarationXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRI</Code>
          <Description>Import Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <GoodsDescription>BODGY HAT</GoodsDescription>
    <WayBillNumber>BLUDGER</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

		const string MinimalistExportDeclarationXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRE</Code>
          <Description>Export Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <GoodsDescription>BODGY HAT</GoodsDescription>
    <WayBillNumber>BLUDGER</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

		const string E2EMessagingImportDeclarationXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
        <Code>XXX</Code>
          <Country>
            <Code>NZ</Code>
            <Name>New Zealand</Name>
          </Country>
        <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRI</Code>
          <Description>Import Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
    <GoodsDescription>BODGY HAT</GoodsDescription>
    <WayBillNumber>TBA</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
    <LocalProcessing>
       <ArrivalCartageRef></ArrivalCartageRef>
       <DeliveryCartageAdvised></DeliveryCartageAdvised>
       <DeliveryCartageCompleted></DeliveryCartageCompleted>
       <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
       <DeliveryLabourTime></DeliveryLabourTime>
       <DeliveryRequiredBy></DeliveryRequiredBy>
       <DeliveryRequiredFrom></DeliveryRequiredFrom>
       <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
       <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
       <EstimatedDelivery></EstimatedDelivery>
       <EstimatedPickup></EstimatedPickup>
       <FCLDeliveryEquipmentNeeded>
         <Code>PSL</Code>
         <Description>Premise Supplies Lift</Description>
       </FCLDeliveryEquipmentNeeded>
       <FCLPickupEquipmentNeeded>
         <Code>WUP</Code>
         <Description>Premise Supplies Lift</Description>
       </FCLPickupEquipmentNeeded>
       <FCLStorageCommences></FCLStorageCommences>
       <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
       <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
       <LCLStorageCommences></LCLStorageCommences>
       <PickupCartageAdvised></PickupCartageAdvised>
       <PickupCartageCompleted></PickupCartageCompleted>
       <PickupLabourCharge>0.0000</PickupLabourCharge>
       <PickupLabourTime></PickupLabourTime>
       <PickupRequiredBy></PickupRequiredBy>
       <PickupRequiredFrom></PickupRequiredFrom>
       <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
       <PickupTruckWaitTime></PickupTruckWaitTime>
    </LocalProcessing>
  </Shipment>
</UniversalShipment>";

		const string E2EMessagingExportDeclarationXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <Company>
       <Code>XXX</Code>
       <Country>
         <Code>NZ</Code>
         <Name>New Zealand</Name>
       </Country>
       <Name>XXX VAN BUGGER FORWARDING</Name>
      </Company>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Export Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
    <GoodsDescription>BODGY HAT</GoodsDescription>
    <WayBillNumber>TBA</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
      <LocalProcessing>
       <ArrivalCartageRef></ArrivalCartageRef>
       <DeliveryCartageAdvised></DeliveryCartageAdvised>
       <DeliveryCartageCompleted></DeliveryCartageCompleted>
       <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
       <DeliveryLabourTime></DeliveryLabourTime>
       <DeliveryRequiredBy></DeliveryRequiredBy>
       <DeliveryRequiredFrom></DeliveryRequiredFrom>
       <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
       <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
       <EstimatedDelivery></EstimatedDelivery>
       <EstimatedPickup></EstimatedPickup>
       <FCLDeliveryEquipmentNeeded>
         <Code>PSL</Code>
         <Description>Premise Supplies Lift</Description>
       </FCLDeliveryEquipmentNeeded>
       <FCLPickupEquipmentNeeded>
         <Code>WUP</Code>
         <Description>Premise Supplies Lift</Description>
       </FCLPickupEquipmentNeeded>
       <FCLStorageCommences></FCLStorageCommences>
       <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
       <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
       <LCLStorageCommences></LCLStorageCommences>
       <PickupCartageAdvised></PickupCartageAdvised>
       <PickupCartageCompleted></PickupCartageCompleted>
       <PickupLabourCharge>0.0000</PickupLabourCharge>
       <PickupLabourTime></PickupLabourTime>
       <PickupRequiredBy></PickupRequiredBy>
       <PickupRequiredFrom></PickupRequiredFrom>
       <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
       <PickupTruckWaitTime></PickupTruckWaitTime>
    </LocalProcessing>
  </Shipment>
</UniversalShipment>";

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			localCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface);
		}
		IDisposable localCountryCustomsInterface;

		protected override void TearDown()
		{
			base.TearDown();
			localCountryCustomsInterface?.Dispose();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;

		sealed class JobDeclarationDataObjectReaderForImportCountrySpecificRelatedDataTest : JobDeclarationDataObjectReader
		{
			public JobDeclarationDataObjectReaderForImportCountrySpecificRelatedDataTest(UniversalShipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment, bool canPopulateDeclaration)
				: base(declarationDataObject, logger, factory, forwardingShipment)
			{
				this.canPopulateDeclaration = canPopulateDeclaration;
			}
			readonly bool canPopulateDeclaration;

			protected override bool CanPopulateDeclaration(BaseJobDeclaration declaration) => canPopulateDeclaration;

			protected override void ImportCountrySpecificRelatedData(BaseJobDeclaration declaration)
			{
				if (!canPopulateDeclaration)
				{
					throw new InvalidOperationException("ImportCountrySpecificRelatedData invoked when CanPopulateDeclaration returns false");
				}
			}

			public Action AfterGetDeclarationFromShipment;

			protected override BaseJobDeclaration GetDeclarationFromShipment()
			{
				var declaration = base.GetDeclarationFromShipment();
				AfterGetDeclarationFromShipment?.Invoke();
				return declaration;
			}

			public ZString ValidApplicationCodeForTesting { get; set; } = ZString.Empty;
			protected override bool IsValidApplicationCode(BaseJobDeclaration declaration, ZString applicationCode)
			{
				return base.IsValidApplicationCode(declaration, applicationCode) || (!ValidApplicationCodeForTesting.IsEmpty && applicationCode == ValidApplicationCodeForTesting);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class StandaloneCommercialInvoiceDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportInvoiceLineCollection_Partial()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
				var buyer = CreateOrganisation("BUYER", "TSTORG01");

				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_OH_Supplier = supplier.PK;
				invoice.JZ_OH_Buyer = buyer.PK;

				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Description = "Line1";
				line1.JI_MatchingKey = "MATCH001";

				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_Description = "Line2";
				line2.JI_MatchingKey = "MATCH002";

				var line3 = invoice.JobComInvoiceLines.AddNew();
				line3.JI_Description = "Line3";
				line3.JI_ParentID = line2.PK;
				line3.JI_MatchingKey = "MATCH003";

				var line4 = invoice.JobComInvoiceLines.AddNew();
				line4.JI_Description = "Line4";
				line4.JI_ParentID = line2.PK;
				line4.JI_MatchingKey = "MATCH004";

				var line5 = invoice.JobComInvoiceLines.AddNew();
				line5.JI_Description = "Line5";
				line5.JI_MatchingKey = string.Empty;

				var line6 = invoice.JobComInvoiceLines.AddNew();
				line6.JI_Description = "Line6";
				line6.JI_MatchingKey = "MATCH00X";

				Factory.SaveForTesting();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 1,
					Description = "ImportLine001",
					DataImportMatchingKey = "MATCH001"
				};

				var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 2,
					Description = "ImportLine002",
					DataImportMatchingKey = "MATCH002"
				};

				var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 3,
					Description = "ImportLine003",
					DataImportMatchingKey = "MATCH003",
					ParentLineNo = 2
				};

				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
					{
						invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3
					})
				{ Content = CollectionContent.Partial }));

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
					}
				};

				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var standaloneInvoice = reader.ReadIntoBusinessObject();

				AssertEquals(standaloneInvoice.PK, invoice.PK);

				var lines = new[] { line1, line2, line5, line6 };
				Assert("These lines should not be deleted as they are not child line.", lines.All(c => !c.IsDeleted));
				Assert("This child line should not be deleted as there is a matching key in the XML.", !line3.IsDeleted);
				Assert("This child line should be deleted as there is not a matching key in the XML.", line4.IsDeleted);

				var expectedDescriptions = new[] { "ImportLine001", "ImportLine002", "ImportLine003", "Line5", "Line6" };
				var actualDescriptions = invoice.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

				AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
			}
		}

		public void TestImportInvoiceLineCollection_Complete()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
				var buyer = CreateOrganisation("BUYER", "TSTORG01");

				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_OH_Supplier = supplier.PK;
				invoice.JZ_OH_Buyer = buyer.PK;

				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Description = "Line1";
				line1.JI_MatchingKey = "MATCH001";

				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_Description = "Line2";
				line2.JI_MatchingKey = "MATCH002";

				var line3 = invoice.JobComInvoiceLines.AddNew();
				line3.JI_Description = "Line3";
				line3.JI_ParentID = line2.PK;
				line3.JI_MatchingKey = "MATCH003";

				var line4 = invoice.JobComInvoiceLines.AddNew();
				line4.JI_Description = "Line4";
				line4.JI_ParentID = line2.PK;
				line4.JI_MatchingKey = "MATCH004";

				var line5 = invoice.JobComInvoiceLines.AddNew();
				line5.JI_Description = "Line5";
				line5.JI_MatchingKey = string.Empty;

				var line6 = invoice.JobComInvoiceLines.AddNew();
				line6.JI_Description = "Line6";
				line6.JI_MatchingKey = "MATCH006";

				var line7 = invoice.JobComInvoiceLines.AddNew();
				line7.JI_Description = "Line7";
				line7.JI_ParentID = line6.PK;
				line7.JI_MatchingKey = "MATCH007";

				Factory.SaveForTesting();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 1,
					Description = "ImportLine001",
					DataImportMatchingKey = "MATCH001"
				};

				var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 2,
					Description = "ImportLine002",
					DataImportMatchingKey = "MATCH002"
				};

				var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 3,
					Description = "ImportLine003",
					DataImportMatchingKey = "MATCH003",
					ParentLineNo = 2
				};

				var invoiceLineDataObject4 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 4,
					Description = "ImportLine005",
					DataImportMatchingKey = "MATCH005",
				};

				var invoiceLineDataObject5 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 5,
					Description = "ImportLine007",
					DataImportMatchingKey = "MATCH007",
					ParentLineNo = 4
				};

				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
					{
						invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3, invoiceLineDataObject4, invoiceLineDataObject5
					})
				{ Content = CollectionContent.Complete }));

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
					}
				};

				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var standaloneInvoice = reader.ReadIntoBusinessObject();

				AssertEquals(standaloneInvoice.PK, invoice.PK);

				var lines = new[] { line1, line2 };
				Assert("These lines should not be deleted as they have a matching key in the XML.", lines.All(c => !c.IsDeleted));

				lines = new[] { line4, line5, line6 };
				Assert("This child line should be deleted as there is not a matching key in the XML.", lines.All(c => c.IsDeleted));

				Assert("This child line should not be deleted as there is a matching key in the XML.", !line3.IsDeleted);
				Assert("This child line should be deleted as there is a matching key with its parent line in the XML.", line7.IsDeleted);

				var expectedDescriptions = new[] { "ImportLine001", "ImportLine002", "ImportLine003", "ImportLine005", "ImportLine007" };
				var actualDescriptions = invoice.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

				AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
			}
		}

		public void TestInvoiceHeaderMessageType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				GlbDepartment.CurrentDepartment.GE_Import = true;
				var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
				var buyer = CreateOrganisation("BUYER", "TSTORG01");

				Factory.SaveForTesting();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 1,
					Description = "ImportLine001",
					DataImportMatchingKey = "MATCH001"
				};

				var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 2,
					Description = "ImportLine002",
					DataImportMatchingKey = "MATCH002"
				};

				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }) { Content = CollectionContent.Partial }));

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, Constants.AddressTypes.SupplierAddress);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, Constants.AddressTypes.BuyerAddress);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Export },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
					}
				};

				logger.ClearLogs();

				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var standaloneInvoice = reader.ReadIntoBusinessObject();

				AssertEquals(JobMessageTypeList.Codes.Export, standaloneInvoice.JZ_MessageType);
			}
		}

		public void TestInvoiceHeaderMessageTypeWhenNotSupplied()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;

			var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
			var buyer = CreateOrganisation("BUYER", "TSTORG01");

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine
			{
				LineNo = 1,
				Description = "ImportLine001",
				DataImportMatchingKey = "MATCH001"
			};

			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1001",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1 }) { Content = CollectionContent.Partial }));

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, Constants.AddressTypes.SupplierAddress);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, Constants.AddressTypes.BuyerAddress);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
				}
			};

			logger.ClearLogs();

			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
			var standaloneInvoice = reader.ReadIntoBusinessObject();

			AssertEquals(JobMessageTypeList.Codes.Import, standaloneInvoice.JZ_MessageType);
		}

		public void TestHasDuplicateMatchingKey()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
				var buyer = CreateOrganisation("BUYER", "TSTORG01");

				Factory.SaveForTesting();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 1,
					Description = "ImportLine001",
					DataImportMatchingKey = "MATCH001"
				};

				var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 2,
					Description = "ImportLine002",
					DataImportMatchingKey = "MATCH001"
				};

				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }) { Content = CollectionContent.Partial }));

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
					}
				};

				logger.ClearLogs();

				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				reader.ReadIntoBusinessObject();

				var expectedError = "These matching keys are used on two or more different invoice lines linking to the same invoice header.";
				AssertContains("Should have the error as the invoiceLineDataObject1 and invoiceLineDataObject2 are using a same matching key.", expectedError, logger.GetErrors());

				invoiceLineDataObject2.DataImportMatchingKey = string.Empty;
				logger.ClearLogs();

				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				reader.ReadIntoBusinessObject();

				AssertNotContains("Should not have the error as the invoiceLineDataObject1 and invoiceLineDataObject2 are using different matching keys.", expectedError, logger.GetErrors());
			}
		}

		public void TestMatchingExistingInvoice()
		{
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var declaration = Factory.New<BaseJobDeclaration>();
			var topGroupInvoice = Factory.New<BaseJobComInvoiceGroupHeader>();
			topGroupInvoice.JZ_OH_Supplier = consignor.PK;
			topGroupInvoice.JZ_OH_Buyer = consignee.PK;
			topGroupInvoice.JZ_InvoiceDate = new ZDateTime(2012, 11, 10);
			topGroupInvoice.JZ_JE = declaration.PK;
			var decInvoice = Factory.New<BaseJobComInvoiceHeader>();
			decInvoice.JZ_OH_Supplier = consignor.PK;
			decInvoice.JZ_OH_Buyer = consignee.PK;
			decInvoice.JZ_InvoiceNumber = "INVABC123";
			decInvoice.JZ_InvoiceDate = new ZDateTime(2012, 11, 11);
			decInvoice.JZ_JE = declaration.PK;

			var standaloneInvoiceOld = Factory.New<BaseJobComInvoiceHeader>();
			standaloneInvoiceOld.JZ_OH_Supplier = consignor.PK;
			standaloneInvoiceOld.JZ_OH_Buyer = consignee.PK;
			standaloneInvoiceOld.JZ_InvoiceNumber = "INVABC123";
			standaloneInvoiceOld.JZ_InvoiceDate = new ZDateTime(2012, 11, 7);

			var standaloneInvoiceNew = Factory.New<BaseJobComInvoiceHeader>();
			standaloneInvoiceNew.JZ_OH_Supplier = consignor.PK;
			standaloneInvoiceNew.JZ_OH_Buyer = consignee.PK;
			standaloneInvoiceNew.JZ_InvoiceNumber = "INVABC123";
			standaloneInvoiceNew.JZ_InvoiceDate = new ZDateTime(2012, 11, 8);

			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, declaration));
			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INVABC123",
				InvoiceAmount = 150m
			};
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection = null;

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
			};

			var newFactory = new UniversalObjectFactory();
			var invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew as all fields were matched", standaloneInvoiceNew.PK, invoice.PK);
			newFactory.SaveForTesting();

			invoiceDataObject.OrganizationAddressCollection = new List<OrganizationAddress>
			{
				invoiceDataObject.Supplier,
				invoiceDataObject.Buyer
			};
			invoiceDataObject.Supplier = null;
			invoiceDataObject.Buyer = null;
			newFactory = new UniversalObjectFactory();
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew as all fields were matched using fallback", standaloneInvoiceNew.PK, invoice.PK);
			newFactory.SaveForTesting();

			declarationDataObject.Branch.Code = "A#@";
			newFactory = new UniversalObjectFactory();
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew though Branch is invalid", standaloneInvoiceNew.PK, invoice.PK);
			Factory.SaveForTesting();

			declarationDataObject.Branch = null;
			newFactory = new UniversalObjectFactory();
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, newFactory).ReadIntoBusinessObject();
			AssertEquals("Should match to standaloneInvoiceNew though Branch is empty", standaloneInvoiceNew.PK, invoice.PK);
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules_MatchingAnyBranch()
		{
			var anotherBranchInCurrentCompany = GlbCompany.CurrentCompany.Branches.First(x => x.PK != GlbBranch.CurrentBranch.PK);
			var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
			var buyer = CreateOrganisation("BUYER", "TSTORG01");

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "INVABC123";
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OH_Buyer = buyer.PK;
			invoice.JZ_InvoiceDate = new ZDateTime(2023, 09, 12);

			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_InvoiceNumber = "INVABC123";
			invoice2.JZ_OH_Supplier = supplier.PK;
			invoice2.JZ_OH_Buyer = buyer.PK;
			invoice2.JZ_InvoiceDate = new ZDateTime(2023, 09, 14);
			invoice2.JZ_GB = anotherBranchInCurrentCompany.PK;
			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INVABC123",
				InvoiceAmount = 150m
			};
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection = null;

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
			};

			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
			var invoiceBO = reader.ReadIntoBusinessObject();
			AssertEquals(invoiceBO.PK, invoice2.PK);
		}

		public void TestClearingExistingData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var consignor = CreateOrganisation("CONSIGNOR", "ABC@#$1");
				var consignee = CreateOrganisation("CONSIGNEE", "ABC@#$2");
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<Integration.Customs.GB.IJobDeclaration>();
				var standaloneInvoice = declaration.Invoices.AddNew();
				standaloneInvoice.JZ_OH_Supplier = consignor.PK;
				standaloneInvoice.JZ_OH_Buyer = consignee.PK;
				standaloneInvoice.JZ_InvoiceNumber = "INVABC123";

				var invoiceCharge = standaloneInvoice.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 200m);

				var invoiceLine = standaloneInvoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1010101010";
				declaration.Invoices.RemoveFromRelationship(standaloneInvoice);
				standaloneInvoice.JZ_JE = ZGuid.Empty;
				declaration.Delete();

				var invoiceCusSupportingInfoTypeSupporter = (Integration.Customs.ICusSupportingInfoTypeSupporter)standaloneInvoice;
				var invoiceCusSupportingInfoTypes = invoiceCusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes();
				var supportingDocumentType = invoiceCusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument];
				AssertEquals(false, invoiceCusSupportingInfoTypes.ContainsKey("AGA"));
				var supportingDocument = (CusSupportingInfo)Factory.New(supportingDocumentType);
				supportingDocument.CSI_ParentID = standaloneInvoice.PK;
				supportingDocument.CSI_ParentTableCode = standaloneInvoice.TablePrefix;
				supportingDocument.CSI_ReferenceNumber = "12";
				var cusSupportingInfoUnknown = Factory.New<CusSupportingInfo>();
				cusSupportingInfoUnknown.CSI_Type = "AGA";
				cusSupportingInfoUnknown.CSI_ParentID = standaloneInvoice.PK;
				cusSupportingInfoUnknown.CSI_ParentTableCode = standaloneInvoice.TablePrefix;
				cusSupportingInfoUnknown.CSI_ReferenceNumber = "D";

				var transport = standaloneInvoice.Transports.AddNew();
				transport.JW_RL_NKDiscPort = "ZZDDD";
				var note = standaloneInvoice.Notes.AddNew(true, "HELLO", "WHAT");

				var container1 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				container1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
				container1.J2_ReferenceNumber = "CONT1";
				var container2 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				container2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
				container2.J2_ReferenceNumber = "CONT2";

				var masterBill1 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				masterBill1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
				masterBill1.J2_ReferenceNumber = "MB1";
				var masterBill2 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				masterBill2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
				masterBill2.J2_ReferenceNumber = "MB2";

				var houseBill1 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				houseBill1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
				houseBill1.J2_ReferenceNumber = "HB1";
				var houseBill2 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				houseBill2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
				houseBill2.J2_ReferenceNumber = "HB2";

				var subHouseBill1 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				subHouseBill1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.SH;
				subHouseBill1.J2_ReferenceNumber = "SB1";
				var subHouseBill2 = standaloneInvoice.InvoiceHeaderRefs.AddNew();
				subHouseBill2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.SH;
				subHouseBill2.J2_ReferenceNumber = "SB2";

				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, declaration));

				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INVABC123",
					InvoiceAmount = 150m,
					PaymentNumber = "PN123"
				};
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
				invoiceDataObject.OrganizationAddressCollection = null;

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					Branch = new Branch() { Code = "B@#" },
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
				};

				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var invoice = reader.ReadIntoBusinessObject();
				AssertEquals(standaloneInvoice, invoice);
				AssertEquals("PN123", invoice.JZ_PaymentNo);
				AssertEquals(false, invoiceCharge.IsDeleted);
				AssertEquals(false, invoiceLine.IsDeleted);
				AssertEquals(false, supportingDocument.IsDeleted);
				AssertEquals(false, cusSupportingInfoUnknown.IsDeleted);
				AssertEquals(false, transport.IsDeleted);
				AssertEquals(true, note.IsDeleted);
				AssertEquals(false, container1.IsDeleted);
				AssertEquals(false, container2.IsDeleted);
				AssertEquals(false, masterBill1.IsDeleted);
				AssertEquals(false, masterBill2.IsDeleted);
				AssertEquals(false, houseBill1.IsDeleted);
				AssertEquals(false, houseBill2.IsDeleted);
				AssertEquals(false, subHouseBill1.IsDeleted);
				AssertEquals(false, subHouseBill2.IsDeleted);

				declarationDataObject.CommercialInfo.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { new UniversalCustoms.CommercialCharge() { ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.Commission }, Amount = 200m, IsApportionedCharge = ZBool.False } });
				standaloneInvoice.JZ_JE = ZGuid.Empty;
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				AssertEquals(standaloneInvoice, invoice);
				AssertEquals(true, invoiceCharge.IsDeleted);
				AssertEquals(1, standaloneInvoice.Charges.Count);
				invoiceCharge = standaloneInvoice.Charges.OfType<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.Commission && x.J7_Amount == 200m);
				AssertNotNull(invoiceCharge);
				AssertEquals(false, invoiceLine.IsDeleted);
				AssertEquals(false, supportingDocument.IsDeleted);
				AssertEquals(false, cusSupportingInfoUnknown.IsDeleted);
				AssertEquals(false, transport.IsDeleted);
				AssertEquals(true, note.IsDeleted);
				AssertEquals(false, container1.IsDeleted);
				AssertEquals(false, container2.IsDeleted);
				AssertEquals(false, masterBill1.IsDeleted);
				AssertEquals(false, masterBill2.IsDeleted);
				AssertEquals(false, houseBill1.IsDeleted);
				AssertEquals(false, houseBill2.IsDeleted);
				AssertEquals(false, subHouseBill1.IsDeleted);
				AssertEquals(false, subHouseBill2.IsDeleted);

				declarationDataObject.CommercialInfo.CommercialChargeCollection = null;
				invoiceDataObject.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { new UniversalCustoms.CommercialCharge() { ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.Commission }, Amount = 200m, IsApportionedCharge = ZBool.False } });
				standaloneInvoice.JZ_JE = ZGuid.Empty;
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				AssertEquals(standaloneInvoice, invoice);
				AssertEquals(true, invoiceCharge.IsDeleted);
				AssertEquals(1, standaloneInvoice.Charges.Count);
				invoiceCharge = standaloneInvoice.Charges.OfType<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.Commission && x.J7_Amount == 200m);
				AssertNotNull(invoiceCharge);
				AssertEquals(false, invoiceLine.IsDeleted);
				AssertEquals(false, supportingDocument.IsDeleted);
				AssertEquals(false, cusSupportingInfoUnknown.IsDeleted);
				AssertEquals(false, transport.IsDeleted);
				AssertEquals(true, note.IsDeleted);
				AssertEquals(false, container1.IsDeleted);
				AssertEquals(false, container2.IsDeleted);
				AssertEquals(false, masterBill1.IsDeleted);
				AssertEquals(false, masterBill2.IsDeleted);
				AssertEquals(false, houseBill1.IsDeleted);
				AssertEquals(false, houseBill2.IsDeleted);
				AssertEquals(false, subHouseBill1.IsDeleted);
				AssertEquals(false, subHouseBill2.IsDeleted);

				standaloneInvoice.JZ_JE = ZGuid.Empty;
				invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { new UniversalCustoms.CommercialInvoiceLine() { HarmonisedCode = "1010101010" } }));
				declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { PortOfLoading = new UNLOCO() { Code = "AUSYD" } } }));
				declarationDataObject.SetNoteCollection(() => new DataObjectList<Note>(new[] { new Note() { IsCustomDescription = true, Description = "HELLO", NoteText = "WHAT" } }) { Content = CollectionContent.Complete });
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				AssertEquals(standaloneInvoice, invoice);
				AssertEquals(true, invoiceLine.IsDeleted);
				AssertEquals(1, invoice.JobComInvoiceLines.Count);
				AssertEquals("1010101010", invoice.JobComInvoiceLines[0].JI_Tariff);
				AssertEquals(false, supportingDocument.IsDeleted);
				AssertEquals(false, cusSupportingInfoUnknown.IsDeleted);
				AssertEquals(true, transport.IsDeleted);
				AssertEquals(1, invoice.Transports.Count);
				AssertEquals("AUSYD", invoice.Transports[0].JW_RL_NKLoadPort);
				AssertEquals(true, note.IsDeleted);
				var notes = invoice.Notes.FindByDescription("HELLO");
				AssertEquals(1, notes.Length);
				AssertEquals("WHAT", notes[0].ST_NoteDataAsText);
				AssertEquals(false, container1.IsDeleted);
				AssertEquals(false, container2.IsDeleted);
				AssertEquals(false, masterBill1.IsDeleted);
				AssertEquals(false, masterBill2.IsDeleted);
				AssertEquals(false, houseBill1.IsDeleted);
				AssertEquals(false, houseBill2.IsDeleted);
				AssertEquals(false, subHouseBill1.IsDeleted);
				AssertEquals(false, subHouseBill2.IsDeleted);

				standaloneInvoice.JZ_JE = ZGuid.Empty;
				invoiceDataObject.CustomsSupportingInformationCollection = new List<UniversalCustoms.CustomsSupportingInformation>(new[] { new UniversalCustoms.CustomsSupportingInformation() { Category = new CodeDescriptionPair() { Code = Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument }, ReferenceNumber = "58" } });
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				AssertEquals(standaloneInvoice, invoice);
				AssertEquals(true, supportingDocument.IsDeleted);
				AssertEquals(false, cusSupportingInfoUnknown.IsDeleted);
				var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, invoice.PK);
				query.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument);
				var cusAddInfos = Factory.Load<CusSupportingInfo>(query);
				AssertEquals(1, cusAddInfos.Length);
				AssertEquals(false, container1.IsDeleted);
				AssertEquals(false, container2.IsDeleted);
				AssertEquals(false, masterBill1.IsDeleted);
				AssertEquals(false, masterBill2.IsDeleted);
				AssertEquals(false, houseBill1.IsDeleted);
				AssertEquals(false, houseBill2.IsDeleted);
				AssertEquals(false, subHouseBill1.IsDeleted);
				AssertEquals(false, subHouseBill2.IsDeleted);

				standaloneInvoice.JZ_JE = ZGuid.Empty;
				declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT1" } }));
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				AssertEquals(standaloneInvoice, invoice);
				AssertEquals(false, container1.IsDeleted);
				AssertEquals(true, container2.IsDeleted);
				var containers = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN).ToArray();
				AssertEquals(1, containers.Length);
				AssertEquals(container1, containers[0]);
				AssertEquals(false, masterBill1.IsDeleted);
				AssertEquals(false, masterBill2.IsDeleted);
				AssertEquals(false, houseBill1.IsDeleted);
				AssertEquals(false, houseBill2.IsDeleted);
				AssertEquals(false, subHouseBill1.IsDeleted);
				AssertEquals(false, subHouseBill2.IsDeleted);

				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
					{
						new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } },
						new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House } },
						new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "SB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse } }
					}));
				standaloneInvoice.JZ_JE = ZGuid.Empty;
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				AssertEquals(standaloneInvoice, invoice);
				AssertEquals(false, masterBill1.IsDeleted);
				AssertEquals(true, masterBill2.IsDeleted);
				var bills = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB).ToArray();
				AssertEquals(1, bills.Length);
				AssertEquals(masterBill1, bills[0]);
				AssertEquals(false, houseBill1.IsDeleted);
				AssertEquals(true, houseBill2.IsDeleted);
				bills = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB).ToArray();
				AssertEquals(1, bills.Length);
				AssertEquals(houseBill1, bills[0]);
				AssertEquals(false, subHouseBill1.IsDeleted);
				AssertEquals(true, subHouseBill2.IsDeleted);
				bills = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH).ToArray();
				AssertEquals(1, bills.Length);
				AssertEquals(subHouseBill1, bills[0]);
			}
		}

		public void TestSupplierAndBuyerMatch()
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
			var buyer = CreateOrganisation("JOO", "ABC!@#12");
			var supplier2 = CreateOrganisation("GARY", "ABC!@#13");
			Factory.SaveForTesting();

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INVABC123",
				InvoiceAmount = 150m
			};
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier2, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection.Clear();
			var consigneeDeliveryDataObj = invoiceDataObject.AddOrgAddress(writeManager, consigneeDelivery, DocAddressType.ConsigneePickupDeliveryAddress);
			var consigneeDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, consigneeDoc, DocAddressType.ConsigneeDocumentaryAddress);
			var consigneeAddressDataObj = invoiceDataObject.AddOrgAddress(writeManager, consigneeAddress, DocAddressType.ConsigneeAddress);
			var importerDeliveryDataObj = invoiceDataObject.AddOrgAddress(writeManager, importerDelivery, DocAddressType.ImporterPickupDeliveryAddress);
			var importerDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, importerDoc, DocAddressType.ImporterDocumentaryAddress);
			var importerDataObj = invoiceDataObject.AddOrgAddress(writeManager, importer, AddressTypes.Importer);
			var consignorPickupDataObj = invoiceDataObject.AddOrgAddress(writeManager, consignorPickup, DocAddressType.ConsignorPickupDeliveryAddress);
			var consignorDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, consignorDoc, DocAddressType.ConsignorDocumentaryAddress);
			var supplierPickupDataObj = invoiceDataObject.AddOrgAddress(writeManager, supplierPickup, DocAddressType.SupplierPickupDeliveryAddress);
			var supplierDocDataObj = invoiceDataObject.AddOrgAddress(writeManager, supplierDoc, DocAddressType.SupplierDocumentaryAddress);
			var supplierDataObj = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }))
			};
			var invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("invoice.JZ_OH_Supplier", supplier2.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OH_Buyer", buyer.PK, invoice.JZ_OH_Buyer);

			invoiceDataObject.Supplier = null;
			invoiceDataObject.Buyer = null;
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("invoice.JZ_OH_Supplier", supplier.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OH_Buyer", importer.PK, invoice.JZ_OH_Buyer);

			invoiceDataObject.OrganizationAddressCollection.Remove(supplierDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(importerDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("invoice.JZ_OH_Supplier", supplierDoc.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OH_Buyer", importerDoc.PK, invoice.JZ_OH_Buyer);

			invoiceDataObject.OrganizationAddressCollection.Remove(supplierDocDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(importerDocDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("invoice.JZ_OH_Supplier", supplierPickup.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OH_Buyer", importerDelivery.PK, invoice.JZ_OH_Buyer);

			invoiceDataObject.OrganizationAddressCollection.Remove(supplierPickupDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(importerDeliveryDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("invoice.JZ_OH_Supplier", consignorDoc.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OH_Buyer", consigneeAddress.PK, invoice.JZ_OH_Buyer);

			invoiceDataObject.OrganizationAddressCollection.Remove(consignorDocDataObj);
			invoiceDataObject.OrganizationAddressCollection.Remove(consigneeAddressDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("invoice.JZ_OH_Supplier", consignorPickup.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OH_Buyer", consigneeDoc.PK, invoice.JZ_OH_Buyer);

			invoiceDataObject.OrganizationAddressCollection.Remove(consigneeDocDataObj);
			invoice = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("invoice.JZ_OH_Supplier", consignorPickup.PK, invoice.JZ_OH_Supplier);
			AssertEquals("invoice.JZ_OH_Buyer", consigneeDelivery.PK, invoice.JZ_OH_Buyer);
		}

		public void TestImportStandaloneCommercialInvoiceData()
		{
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "B@#";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import }
			};
			var masterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
			var masterBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
			var masterBill1HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
			var masterBill1HouseBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
			var masterBill2HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB2" };
			var masterBill1HouseBill2SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB2SB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "MB1HB2" };
			var masterBill2HouseBill1SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2HB1SB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "MB2HB1" };

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT123ABC" };
			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT456DEF" };

			var transport1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Sea, LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "AUSYD" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" }, EstimatedDeparture = new ZDateTime(2012, 3, 1), EstimatedArrival = new ZDateTime(2012, 3, 11) };
			var transport2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Sea, LegOrder = 2, PortOfLoading = new UNLOCO() { Code = "USLAX" }, PortOfDischarge = new UNLOCO() { Code = "USCHI" }, EstimatedDeparture = new ZDateTime(2012, 3, 12), EstimatedArrival = new ZDateTime(2012, 3, 13) };

			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject2, containerDataObject1 }));
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill1, masterBill2, masterBill1HouseBill1, masterBill1HouseBill2, masterBill2HouseBill1, masterBill1HouseBill2SubHouse1, masterBill2HouseBill1SubHouse1 }));
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { transport1, transport2 }));

			var invoiceLineDataObject1Charge = new UniversalCustoms.CommercialCharge() { ChargeType = Commission, Currency = ForeignCurrency, Amount = 100m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			var invoiceLineDataObject2Charge = new UniversalCustoms.CommercialCharge() { ChargeType = Discount, Currency = ForeignCurrency, Amount = 150m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			var invoiceLineDataObject1 = SetupCommercialInvoiceLine(1, commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject1Charge }));
			invoiceLineDataObject1.HarmonisedCode = "1010101010";
			var invoiceLineDataObject2 = SetupCommercialInvoiceLine2(2, commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject2Charge }));
			invoiceLineDataObject2.HarmonisedCode = "2020202020";

			var invoiceDataObjectCharge1 = new UniversalCustoms.CommercialCharge() { ChargeType = DeductionCharge, Currency = ForeignCurrency, Amount = 50m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			var invoiceDataObjectCharge2 = new UniversalCustoms.CommercialCharge() { ChargeType = OverseasFreight, Currency = ForeignCurrency, Amount = 300m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV123", null, null, 1500.32m, ForeignCurrency, new ZDateTime(2012, 3, 25, 13, 3, 2), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.CostInsuranceAndFreight },
				10.4m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1202.53m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.25m, 100m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), null);
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection = null;
			invoiceDataObject.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceDataObjectCharge1, invoiceDataObjectCharge2 });
			invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }));

			var invoiceGroupDataObjectCharge = new UniversalCustoms.CommercialCharge() { ChargeType = OverseasInsurance, Currency = ForeignCurrency, Amount = 200m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }), chargeCollection: new List<UniversalCustoms.CommercialCharge>(new[] { invoiceGroupDataObjectCharge }));

			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
			var invoice = reader.ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);
			AssertNotNull(invoice);
			var fakeDeclaration = invoice.JobDeclaration;
			AssertNotNull(fakeDeclaration);
			CombineAssertions(delegate
			{
				Assert(!fakeDeclaration.JE_AutoWeightApportion);
				AssertContents(invoice, "INV123", consignee.PK, consignor.PK, 1500.32m, ForeignCurrencyBO.RX_Code, new ZDateTime(2012, 3, 25, 13, 3, 2), Core.Constants.IncoTerms.CostInsuranceAndFreight,
					10.4m, Core.Constants.Volume.CubicMetres, 1202.53m, Core.Constants.Weight.Pounds, 11.11m, Core.Constants.Weight.Kilograms, FixedRate.GetCodeAsUpperCase(), 1.5m, 1.25m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), 100m);

				AssertEquals("invoice.Charges.Count", 3, invoice.Charges.Count);
				AssertNotNull(invoice.Charges.OfType<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge && x.J7_Amount == 50m && x.J7_RX_NKCurrency == ForeignCurrencyBO.RX_Code && x.J7_ExchangeRateType == FixedRate.GetCodeAsUpperCase() && x.J7_ExchangeRate == 1.5m));
				AssertNotNull(invoice.Charges.OfType<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight && x.J7_Amount == 300m && x.J7_RX_NKCurrency == ForeignCurrencyBO.RX_Code && x.J7_ExchangeRateType == FixedRate.GetCodeAsUpperCase() && x.J7_ExchangeRate == 1.5m));
				AssertNotNull(invoice.Charges.OfType<BaseInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasInsurance && x.J7_Amount == 200m && x.J7_RX_NKCurrency == ForeignCurrencyBO.RX_Code && x.J7_ExchangeRateType == FixedRate.GetCodeAsUpperCase() && x.J7_ExchangeRate == 1.5m));

				AssertEquals("invoice.Transports.Count", 2, invoice.Transports.Count);
				AssertNotNull(invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_LegOrder == 1 && x.JW_RL_NKLoadPort == "AUSYD" && x.JW_RL_NKDiscPort == "USLAX" && x.JW_ETD == new ZDateTime(2012, 3, 1) && x.JW_ETA == new ZDateTime(2012, 3, 11)));
				AssertNotNull(invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_LegOrder == 2 && x.JW_RL_NKLoadPort == "USLAX" && x.JW_RL_NKDiscPort == "USCHI" && x.JW_ETD == new ZDateTime(2012, 3, 12) && x.JW_ETA == new ZDateTime(2012, 3, 13)));

				AssertEquals("invoice.JobComInvoiceLines.Count", 2, invoice.JobComInvoiceLines.Count);
				var invoiceLine1 = invoice.JobComInvoiceLines[0];
				AssertContents(invoiceLine1, 1);
				AssertEquals("invoiceLine1.JI_Tariff", "1010101010", invoiceLine1.JI_Tariff);
				AssertEquals("invoiceLine1.Charges.Count", 1, invoiceLine1.Charges.Count);
				AssertNotNull(invoiceLine1.Charges.OfType<BaseInvoiceLineCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.Commission && x.J7_Amount == 100m && x.J7_RX_NKCurrency == ForeignCurrencyBO.RX_Code && x.J7_ExchangeRateType == FixedRate.GetCodeAsUpperCase() && x.J7_ExchangeRate == 1.5m));

				var invoiceLine2 = invoice.JobComInvoiceLines[1];
				AssertContents2(invoiceLine2, 2);
				AssertEquals("invoiceLine2.JI_Tariff", "2020202020", invoiceLine2.JI_Tariff);
				AssertEquals("invoinvoiceLine2ice.Charges.Count", 1, invoiceLine2.Charges.Count);
				AssertNotNull(invoiceLine2.Charges.OfType<BaseInvoiceLineCharge>().FirstOrDefault(x => x.J7_ChargeType == CustomsChargeTypeList.Codes.Discount && x.J7_Amount == 150m && x.J7_RX_NKCurrency == ForeignCurrencyBO.RX_Code && x.J7_ExchangeRateType == FixedRate.GetCodeAsUpperCase() && x.J7_ExchangeRate == 1.5m));

				AssertEquals("invoice.InvoiceHeaderRefs.Count", 9, invoice.InvoiceHeaderRefs.Count);
				AssertNotNull("Container CONT123ABC should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN && x.J2_ReferenceNumber == "CONT123ABC"));
				AssertNotNull("Container CONT456DEF should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN && x.J2_ReferenceNumber == "CONT456DEF"));
				AssertNotNull("MasterBill MB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB && x.J2_ReferenceNumber == "MB1"));
				AssertNotNull("MasterBill MB2 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.MB && x.J2_ReferenceNumber == "MB2"));
				AssertNotNull("HouseBill MB1HB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB1HB1"));
				AssertNotNull("HouseBill MB1HB2 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB1HB2"));
				AssertNotNull("HouseBill MB2HB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB && x.J2_ReferenceNumber == "MB2HB1"));
				AssertNotNull("SubHouseBill MB1HB2SB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH && x.J2_ReferenceNumber == "MB1HB2SB1"));
				AssertNotNull("SubHouseBill MB2HB1SB1 should exists", invoice.InvoiceHeaderRefs.FirstOrDefault(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.SH && x.J2_ReferenceNumber == "MB2HB1SB1"));
			});
		}

		public void TestImportAdvanceShippingNoticeData()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				consignor.OH_IsConsignor = ZBool.True;
				consignor.OH_RL_NKClosestPort = "AUSYD";
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				consignee.OH_IsConsignee = ZBool.True;
				consignee.OH_RL_NKClosestPort = "USCHI";
				var branch2 = Factory.New<GlbBranch>();
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;
				branch2.GB_Code = "B@#";
				branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

				var part1 = Factory.New<Business.OrgSupplierPart>();
				part1.OP_PartNum = "PART1";
				part1.OP_Weight = 100m;
				part1.OP_WeightUQ = "HG";
				part1.RelatedOrganisations.AddSupplier(consignor);
				part1.RelatedOrganisations.AddOwner(consignee);
				var part2 = Factory.New<Business.OrgSupplierPart>();
				part2.OP_PartNum = "PART2";
				part2.OP_Weight = 200m;
				part2.OP_WeightUQ = "HG";
				part2.RelatedOrganisations.AddSupplier(consignor);
				part2.RelatedOrganisations.AddOwner(consignee);

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					Branch = new Branch() { Code = "B@#" },
				};

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine() { LineNo = 1, PartNo = "PART1", InvoiceQuantity = 1 };
				var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine() { LineNo = 2, PartNo = "PART2", InvoiceQuantity = 1 };

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV123", null, null, 1500.32m, ForeignCurrency, new ZDateTime(2012, 3, 25, 13, 3, 2), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.CostInsuranceAndFreight },
					10.4m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1202.53m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.25m, 100m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), null);
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
				invoiceDataObject.OrganizationAddressCollection = null;
				invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }));

				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }));

				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var invoice = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);
				AssertNotNull(invoice);
				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_StandAloneInvoiceDirection", "IMP", invoice.JZ_StandAloneInvoiceDirection);

					AssertEquals("invoice.JobComInvoiceLines.Count", 2, invoice.JobComInvoiceLines.Count);
					var invoiceLine1 = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine1.JI_OP", part1.PK, invoiceLine1.JI_OP);
					AssertEquals("invoiceLine1.JI_Weight", 100m, invoiceLine1.JI_Weight);
					AssertEquals("invoiceLine1.JI_WeightUQ", "HG", invoiceLine1.JI_WeightUQ);

					var invoiceLine2 = invoice.JobComInvoiceLines[1];
					AssertEquals("invoiceLine2.JI_OP", part2.PK, invoiceLine2.JI_OP);
					AssertEquals("invoiceLine2.JI_Weight", 200m, invoiceLine2.JI_Weight);
					AssertEquals("invoiceLine2.JI_WeightUQ", "HG", invoiceLine2.JI_WeightUQ);
				});

				declarationDataObject.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.MoreCodes.AdvanceShippingNotice };
				reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				invoice = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);
				AssertNotNull(invoice);
				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_StandAloneInvoiceDirection", "ASN", invoice.JZ_StandAloneInvoiceDirection);
					AssertEquals("invoice.JZ_MessageType", "ASN", invoice.JZ_MessageType);

					AssertEquals("invoice.JobComInvoiceLines.Count", 2, invoice.JobComInvoiceLines.Count);
					var invoiceLine1 = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine1.JI_OP has NOT been defaulted", ZGuid.Empty, invoiceLine1.JI_OP);
					AssertEquals("invoiceLine1.JI_Weight has NOT been defaulted", 0m, invoiceLine1.JI_Weight);
					AssertEquals("invoiceLine1.JI_WeightUQ has NOT been defaulted", "KG", invoiceLine1.JI_WeightUQ);

					var invoiceLine2 = invoice.JobComInvoiceLines[1];
					AssertEquals("invoiceLine2.JI_OP has NOT been defaulted", ZGuid.Empty, invoiceLine2.JI_OP);
					AssertEquals("invoiceLine2.JI_Weight has NOT been defaulted", 0m, invoiceLine2.JI_Weight);
					AssertEquals("invoiceLine2.JI_WeightUQ has NOT been defaulted", "KG", invoiceLine2.JI_WeightUQ);
				});
			}
		}

		public void TestImportGTMUxmlShipmentWithUCKServiceCodeWillUnlockStandaloneCommercialInvoice()
		{
			(var declarationDataObject, var invoiceDataObject, var recipientRole, var consignor, var consignee) = CreateGTMShipmentData();

			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateOrGetExistingRefSysConfigType(StandaloneCommercialInvoiceDataObjectReader.RefSysConfigCodeGtm, "The GTM System mailbox", "The GTM System mailbox ID");
			helper.CreateOrUpdateExistingRefSysConfig(StandaloneCommercialInvoiceDataObjectReader.RefSysConfigCodeGtm, "NEO_COMPLIANCE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = invoiceDataObject.InvoiceNumber.Value;
			invoice.JZ_OH_Supplier = consignor.PK;
			invoice.JZ_OH_Buyer = consignee.PK;
			ICustomsFileParent customsFileParent = invoice;
			customsFileParent.LockFile(string.Empty);
			var lockLog = invoice.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);

			recipientRole.ServiceCode = ServiceCodeType.UCK;
			Factory.SaveForTesting();

			using (Factory.BOFactory.AddDisposableService())
			{
				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				AssertSame(invoice, reader.ReadIntoBusinessObject());
				AssertEquals("customsFileParent.IsLocked", false, customsFileParent.IsLocked);
				var utcNow = ZDateTime.UtcNow;
				Factory.SaveAtEndOfImport(logger);

				var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.UniversalDataMessaging);
				var interchanges = Factory.Load<EDIInterchange>(interchangeQuery);
				AssertEquals("No UXML export when UCK", 0, interchanges.Length);
				AssertEquals("No DEX when UCK", 0, invoice.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DataExport).Count());
				Assert("lockLog - Should cancel all existing LCK events.", lockLog.SL_IsCancelled);
				var unlockLogs = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.UnlockForEditCode));
				AssertEquals("UCK logs", 1, unlockLogs.Length);
				AssertEquals("SL_Reference", MessageRecipientPartyTypeList.Codes.GlobalTradeManagement, unlockLogs[0].SL_Reference);
			}
		}

		public void TestFactory_Saved_ForGtm_Success()
		{
			(var declarationDataObject, var invoiceDataObject, _, _, _) = CreateGTMShipmentData();

			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateOrGetExistingRefSysConfigType(StandaloneCommercialInvoiceDataObjectReader.RefSysConfigCodeGtm, "The GTM System mailbox", "The GTM System mailbox ID");
			helper.CreateOrUpdateExistingRefSysConfig(StandaloneCommercialInvoiceDataObjectReader.RefSysConfigCodeGtm, "NEO_COMPLIANCE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.SaveForTesting();

			using (Factory.BOFactory.AddDisposableService())
			{
				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var invoice = reader.ReadIntoBusinessObject();
				invoice.JZ_JE = ZGuid.Empty; // removing hooking to fake declaration
				var invoice2 = reader.ReadIntoBusinessObject(); // Ensure that we only hooked to factory saved only once.
				AssertSame(invoice, invoice2);
				var utcNow = ZDateTime.UtcNow;
				Factory.SaveAtEndOfImport(logger);

				AssertNotContains(@"Unable to export Commercial Invoice to eHub for Global Trade Management as eHub ID configuration is missing.", logger.Logs);
				AssertNotNull(invoice);

				var invoiceDb = Factory.Load<BaseJobComInvoiceHeader>(invoice.PK);
				AssertNotNull(invoiceDb);

				var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
				interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.UniversalDataMessaging);
				var interchanges = Factory.Load<EDIInterchange>(interchangeQuery);
				AssertEquals(1, interchanges.Length);
				var interchange = interchanges[0];
				AssertEquals("interchange.EI_To", "NEO_COMPLIANCE", interchange.EI_To);
				AssertEquals("interchange.EI_TransportType", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);

				var exportLog = invoice.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DataExport).Single();
				var uxmlMessage = exportLog.RelatedEDIMessage.Message;
				AssertSame(interchange, uxmlMessage.Interchange);
				AssertContains("uxmlMessage.EM_MessageText", @"<DataSource>
          <Type>CustomsCommercialInvoice</Type>", uxmlMessage.EM_MessageText);
				AssertContains("uxmlMessage.EM_MessageText", @"<InvoiceNumber>INV123</InvoiceNumber>", uxmlMessage.EM_MessageText);
			}
		}

		public void TestFactory_Saved_ForGtm_NotSend()
		{
			(var declarationDataObject, var invoiceDataObject, _, _, _) = CreateGTMShipmentData();
			var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
			var invoice = reader.ReadIntoBusinessObject();
			var utcNow = ZDateTime.UtcNow;
			Factory.SaveAtEndOfImport(logger);

			AssertContains(@"Unable to export Commercial Invoice to eHub for Global Trade Management as eHub ID configuration is missing.", logger.Logs);
			AssertNotNull(invoice);

			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, utcNow);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.UniversalDataMessaging);
			var interchanges = Factory.Load<EDIInterchange>(interchangeQuery);
			AssertEquals(0, interchanges.Length);

			AssertNull("No DEX events", invoice.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		(Shipment declarationDataObject, UniversalCustoms.CommercialInvoiceHeader invoiceDataObject, RecipientRole recipientRole, OrgHeader consignor, OrgHeader consignee) CreateGTMShipmentData()
		{
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "B@#";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var recipientRole = new RecipientRole { Code = RecipientRoleType.GTM };
			dataContext.RecipientRoleCollection = new List<IRecipientRoleDataObject> { recipientRole };

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import }
			};
			var masterBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
			var masterBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master } };
			var masterBill1HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
			var masterBill1HouseBill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB1" };
			var masterBill2HouseBill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House }, ParentBillNumber = "MB2" };
			var masterBill1HouseBill2SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB1HB2SB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "MB1HB2" };
			var masterBill2HouseBill1SubHouse1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "MB2HB1SB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse }, ParentBillNumber = "MB2HB1" };

			var containerDataObject1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT123ABC" };
			var containerDataObject2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT456DEF" };

			var transport1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Sea, LegOrder = 1, PortOfLoading = new UNLOCO() { Code = "AUSYD" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" }, EstimatedDeparture = new ZDateTime(2012, 3, 1), EstimatedArrival = new ZDateTime(2012, 3, 11) };
			var transport2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Sea, LegOrder = 2, PortOfLoading = new UNLOCO() { Code = "USLAX" }, PortOfDischarge = new UNLOCO() { Code = "USCHI" }, EstimatedDeparture = new ZDateTime(2012, 3, 12), EstimatedArrival = new ZDateTime(2012, 3, 13) };

			declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject2, containerDataObject1 }));
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBill1, masterBill2, masterBill1HouseBill1, masterBill1HouseBill2, masterBill2HouseBill1, masterBill1HouseBill2SubHouse1, masterBill2HouseBill1SubHouse1 }));
			declarationDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[] { transport1, transport2 }));

			var invoiceLineDataObject1Charge = new UniversalCustoms.CommercialCharge() { ChargeType = Commission, Currency = ForeignCurrency, Amount = 100m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			var invoiceLineDataObject2Charge = new UniversalCustoms.CommercialCharge() { ChargeType = Discount, Currency = ForeignCurrency, Amount = 150m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			var invoiceLineDataObject1 = SetupCommercialInvoiceLine(1, commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject1Charge }));
			invoiceLineDataObject1.HarmonisedCode = "1010101010";
			var invoiceLineDataObject2 = SetupCommercialInvoiceLine2(2, commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[] { invoiceLineDataObject2Charge }));
			invoiceLineDataObject2.HarmonisedCode = "2020202020";

			var invoiceDataObjectCharge1 = new UniversalCustoms.CommercialCharge() { ChargeType = DeductionCharge, Currency = ForeignCurrency, Amount = 50m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			var invoiceDataObjectCharge2 = new UniversalCustoms.CommercialCharge() { ChargeType = OverseasFreight, Currency = ForeignCurrency, Amount = 300m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV123", null, null, 1500.32m, ForeignCurrency, new ZDateTime(2012, 3, 25, 13, 3, 2), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.CostInsuranceAndFreight },
				10.4m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1202.53m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.25m, 100m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), null);
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection = null;
			invoiceDataObject.CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[] { invoiceDataObjectCharge1, invoiceDataObjectCharge2 });
			invoiceDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }));

			var invoiceGroupDataObjectCharge = new UniversalCustoms.CommercialCharge() { ChargeType = OverseasInsurance, Currency = ForeignCurrency, Amount = 200m, AgreedExchangeRate = 1.5m, ExchangeRateType = FixedRate };
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group", invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }), chargeCollection: new List<UniversalCustoms.CommercialCharge>(new[] { invoiceGroupDataObjectCharge }));
			return (declarationDataObject, invoiceDataObject, recipientRole, consignor, consignee);
		}

		public void TestLockFile_ForGtm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
				var buyer = CreateOrganisation("BUYER", "TSTORG01");

				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_OH_Supplier = supplier.PK;
				invoice.JZ_OH_Buyer = buyer.PK;

				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_Description = "Line1";
				line1.JI_MatchingKey = "MATCH001";

				Factory.SaveForTesting();

				Assert(!((ICustomsFileParent)invoice).IsLocked);

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

				var recipientRole = new RecipientRole { Code = RecipientRoleType.GTM };
				dataContext.RecipientRoleCollection = new List<IRecipientRoleDataObject> { recipientRole };

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 1,
					Description = "ImportLine001",
					DataImportMatchingKey = "MATCH001"
				};

				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
					{
						invoiceLineDataObject1
					})
				{ Content = CollectionContent.Complete }));

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
					}
				};

				var reader = new StandaloneCommercialInvoiceDataObjectReader(declarationDataObject, invoiceDataObject, logger, Factory);
				var standaloneInvoice = reader.ReadIntoBusinessObject();

				Assert("For GTM invoice should be locked", ((ICustomsFileParent)invoice).IsLocked);
			}
		}

		public void TestIsForGlobalTradeManagement()
		{
			TestCase(true, true, true);
			TestCase(true, false, false);
			TestCase(false, true, false);
			TestCase(false, false, false);
			void TestCase(bool hasCommercialInvoice, bool hasGtmRecipient, bool expectedResult)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
				{
					var supplier = CreateOrganisation("SUPPLIER", "TSTORG00");
					var buyer = CreateOrganisation("BUYER", "TSTORG01");

					var dataContext = DataContextFactory.New();
					dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
					if (hasCommercialInvoice)
					{
						dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);
					}
					
					if (hasGtmRecipient)
					{
						var recipientRole = new RecipientRole { Code = RecipientRoleType.GTM };
						dataContext.RecipientRoleCollection = new List<IRecipientRoleDataObject> { recipientRole };
					}

					var invoiceDataObject =
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV1"
						};

					var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
					invoiceDataObject.Supplier =
						invoiceDataObject.AddOrgAddress(writeManager, supplier, AddressTypes.Supplier);
					invoiceDataObject.Buyer =
						invoiceDataObject.AddOrgAddress(writeManager, buyer, AddressTypes.Importer);

					var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						DataContext = dataContext,
						MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
						CommercialInfo = new UniversalCustoms.CommercialInfo()
						{
							CommercialInvoiceCollection =
								new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
								{
									invoiceDataObject
								})
						}
					};

					var reader = new TestableStandaloneCommercialInvoiceDataObjectReader(declarationDataObject,
						invoiceDataObject, logger, Factory);

					var result = reader.IsForGlobalTradeManagement_Exposed;
					AssertEquals(expectedResult, result);
				}
			}
		}
		sealed class TestableStandaloneCommercialInvoiceDataObjectReader : StandaloneCommercialInvoiceDataObjectReader
		{
			public TestableStandaloneCommercialInvoiceDataObjectReader(
				Shipment shipmentDataObject,
				UniversalCustoms.CommercialInvoiceHeader invoiceHeaderDataObject,
				IXmlImportLogger logger,
				UniversalObjectFactory factory)
				: base(shipmentDataObject, invoiceHeaderDataObject, logger, factory)
			{
			}

			public bool IsForGlobalTradeManagement_Exposed => IsForGlobalTradeManagement;
		}
	}
}

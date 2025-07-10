using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class CommercialInvoiceHeaderDataObjectReaderWithDataImportMatchingKeyTest : DataObjectReaderTest
	{
		public void TestGetReasonForNotAbleToUpdate()
		{
			#region Prepare Data

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

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
				DataImportMatchingKey = "Match001"
			};

			var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine
			{
				LineNo = 1,
				Description = "ImportLine003",
				DataImportMatchingKey = "MATCH002"
			};

			var invoiceDataObject1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }) { Content = CollectionContent.Partial }));

			var invoiceDataObject2 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV2",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject3 }) { Content = CollectionContent.Partial }));

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject1, invoiceDataObject2 })
				}
			};

			#endregion

			void AssertReasonForNotAbleToUpdate(string message, string error)
			{
				logger.ClearLogs();

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				reader.ReadIntoBusinessObject();

				AssertEquals(message, error, logger.GetErrors());
			}

			var expectedError = @"Cannot populate invoice header data because:
These matching keys are used on two or more different invoice lines linking to the same invoice header.
MATCH001";

			AssertReasonForNotAbleToUpdate("Should has the expected error as the invoiceLineDataObject1 and invoiceLineDataObject2 are using same matching key.", expectedError);

			invoiceLineDataObject2.DataImportMatchingKey = "MATCH002";
			expectedError = string.Empty;

			AssertReasonForNotAbleToUpdate("Should not have the expected error as the invoiceLineDataObject2 and invoiceLineDataObject3 link to different invoice header.", expectedError);

			invoiceLineDataObject2.DataImportMatchingKey = string.Empty;
			expectedError = @"Cannot populate invoice header data because:
A non-empty element <DataImportMatchingKey> is required in the case of 'Partial' collection.";

			AssertReasonForNotAbleToUpdate("Should has the expected error as the DataImportMatchingKey of invoiceLineDataObject2 is empty.", expectedError);

			invoiceDataObject1.CommercialInvoiceLineCollection.Content = CollectionContent.Complete;
			expectedError = string.Empty;

			AssertReasonForNotAbleToUpdate("Should not have the expected error content of invoiceDataObject2.CommercialInvoiceLineCollection is not Partial.", expectedError);
		}

		public void TestMatchedNormalLines()
		{
			void AssertImportInvoiceLineCollectionInCountry(string countryCode)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					#region Prepare Data

					var newFactory = NewUniversalObjectFactory();
					var declaration = newFactory.NewWithValidTestData<BaseJobDeclaration>();

					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_InvoiceNumber = "INV1";

					var line1 = invoice.JobComInvoiceLines.AddNew();
					line1.JI_Description = "Line1";
					line1.JI_MatchingKey = "MATCH001";

					var line2 = invoice.JobComInvoiceLines.AddNew();
					line2.JI_Description = "Line2";
					line2.JI_MatchingKey = string.Empty;

					var line3 = invoice.JobComInvoiceLines.AddNew();
					line3.JI_Description = "Line3";
					line3.JI_MatchingKey = "MATCH003";

					newFactory.SaveForTesting();

					var dataContext = DataContextFactory.New();
					dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
					dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

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
						Description = "ImportLine00X",
						DataImportMatchingKey = "MATCH00X"
					};

					var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV1",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3 })
						{ Content = CollectionContent.Partial }));

					var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						DataContext = dataContext,
						MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
						CommercialInfo = new UniversalCustoms.CommercialInfo()
						{
							CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
						}
					};

					#endregion

					CombineAssertions("Assertions in Partial mode.", () =>
					{
						newFactory = NewUniversalObjectFactory();

						var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, newFactory);

						var jobDeclaration = reader.ReadIntoBusinessObject();
						AssertEquals("Should find the target delcaration and import data on it.", declaration.PK, jobDeclaration.PK);

						AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
						AssertEquals("Have 5 invoice lines", 5, jobDeclaration.InvoiceLines.Count);

						var jobComInvoiceHeader = jobDeclaration.Invoices[0];
						AssertEquals("Should find the target invoice header and import data on it.", invoice.PK, jobComInvoiceHeader.PK);

						var invoiceLine1 = newFactory.Load<BaseJobComInvoiceLine>(line1.PK);
						var invoiceLine2 = newFactory.Load<BaseJobComInvoiceLine>(line2.PK);
						var invoiceLine3 = newFactory.Load<BaseJobComInvoiceLine>(line3.PK);

						AssertNotNull("This line should not be deleted in Partial mode.", invoiceLine1);
						AssertNotNull("This line should not be deleted in Partial mode.", invoiceLine2);
						AssertNotNull("This line should not be deleted in Partial mode.", invoiceLine3);

						var expectedDescriptions = new[] { "ImportLine001", "Line2", "ImportLine002", "Line3", "ImportLine00X" };
						var actualDescriptions = jobComInvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

						AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
					});

					CombineAssertions("Assertions in Complete mode.", () =>
					{
						newFactory = NewUniversalObjectFactory();

						invoiceDataObject.CommercialInvoiceLineCollection.Content = CollectionContent.Complete;

						var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, newFactory);

						var jobDeclaration = reader.ReadIntoBusinessObject();
						AssertEquals("Should find the target delcaration and import data on it.", declaration.PK, jobDeclaration.PK);

						AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
						AssertEquals("Have 3 invoice lines", 3, jobDeclaration.InvoiceLines.Count);

						var jobComInvoiceHeader = jobDeclaration.Invoices[0];
						AssertEquals("Should find the target invoice header and import data on it.", invoice.PK, jobComInvoiceHeader.PK);

						var invoiceLine1 = newFactory.Load<BaseJobComInvoiceLine>(line1.PK);
						var invoiceLine2 = newFactory.Load<BaseJobComInvoiceLine>(line2.PK);
						var invoiceLine3 = newFactory.Load<BaseJobComInvoiceLine>(line3.PK);

						AssertNotNull("This line should be not deleted in Complete mode as there is a matching key in XML.", invoiceLine1);
						AssertNull("This line should be deleted in Complete mode as the JI_MatchingKey is empty.", invoiceLine2);
						AssertNull("This line should be deleted in Complete mode as the JI_MatchingKey is not in XML.", invoiceLine3);

						var expectedDescriptions = new[] { "ImportLine001", "ImportLine002", "ImportLine00X" };
						var actualDescriptions = jobComInvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

						AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
					});
				}
			}

			AssertImportInvoiceLineCollectionInCountry(Core.Constants.CountryCodes.China);
			AssertImportInvoiceLineCollectionInCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		public void TestMatchedChildrenLines()
		{
			void AssertImportInvoiceLineCollectionInCountry(string countryCode)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					#region Prepare Data

					var newFactory = NewUniversalObjectFactory();
					var declaration = newFactory.NewWithValidTestData<BaseJobDeclaration>();

					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_InvoiceNumber = "INV1";

					var line1 = invoice.JobComInvoiceLines.AddNew();
					line1.JI_Description = "Line1";
					line1.JI_MatchingKey = "MATCH001";

					var line2 = invoice.JobComInvoiceLines.AddNew();
					line2.JI_Description = "Line2";
					line2.JI_ParentID = line1.PK;
					line2.JI_MatchingKey = string.Empty;

					var line3 = invoice.JobComInvoiceLines.AddNew();
					line3.JI_Description = "Line3";
					line3.JI_ParentID = line1.PK;
					line3.JI_MatchingKey = "MATCH003";

					var line4 = invoice.JobComInvoiceLines.AddNew();
					line4.JI_Description = "Line4";
					line4.JI_MatchingKey = "MATCH004";

					var line5 = invoice.JobComInvoiceLines.AddNew();
					line5.JI_Description = "Line5";
					line5.JI_ParentID = line4.PK;
					line5.JI_MatchingKey = string.Empty;

					var line6 = invoice.JobComInvoiceLines.AddNew();
					line6.JI_Description = "Line6";
					line6.JI_ParentID = line4.PK;
					line6.JI_MatchingKey = "MATCH006";

					var line7 = invoice.JobComInvoiceLines.AddNew();
					line7.JI_Description = "Line7";
					line7.JI_MatchingKey = "MATCH007";

					var line8 = invoice.JobComInvoiceLines.AddNew();
					line8.JI_Description = "Line8";
					line8.JI_ParentID = line7.PK;
					line8.JI_MatchingKey = "MATCH008";

					newFactory.SaveForTesting();

					var dataContext = DataContextFactory.New();
					dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
					dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

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
						DataImportMatchingKey = "MATCH002",
						ParentLineNo = 1
					};

					var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine
					{
						LineNo = 3,
						Description = "ImportLine003",
						DataImportMatchingKey = "MATCH003",
						ParentLineNo = 1
					};

					var invoiceLineDataObject4 = new UniversalCustoms.CommercialInvoiceLine
					{
						LineNo = 4,
						Description = "ImportLine004",
						DataImportMatchingKey = "MATCH006",
						ParentLineNo = 1
					};

					var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV1",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3, invoiceLineDataObject4
							})
						{ Content = CollectionContent.Partial }));

					var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						DataContext = dataContext,
						MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
						CommercialInfo = new UniversalCustoms.CommercialInfo()
						{
							CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
						}
					};

					#endregion

					CombineAssertions("Assertions in Partial mode.", () =>
					{
						newFactory = NewUniversalObjectFactory();

						var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, newFactory);

						var jobDeclaration = reader.ReadIntoBusinessObject();
						AssertEquals("Should find the target delcaration and import data on it.", declaration.PK, jobDeclaration.PK);

						AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
						AssertEquals("Have 8 invoice lines", 8, jobDeclaration.InvoiceLines.Count);

						var jobComInvoiceHeader = jobDeclaration.Invoices[0];
						AssertEquals("Should find the target invoice header and import data on it.", invoice.PK, jobComInvoiceHeader.PK);

						var invoiceLine1 = newFactory.Load<BaseJobComInvoiceLine>(line1.PK);
						var invoiceLine2 = newFactory.Load<BaseJobComInvoiceLine>(line2.PK);
						var invoiceLine3 = newFactory.Load<BaseJobComInvoiceLine>(line3.PK);
						var invoiceLine4 = newFactory.Load<BaseJobComInvoiceLine>(line4.PK);
						var invoiceLine5 = newFactory.Load<BaseJobComInvoiceLine>(line5.PK);
						var invoiceLine6 = newFactory.Load<BaseJobComInvoiceLine>(line6.PK);
						var invoiceLine7 = newFactory.Load<BaseJobComInvoiceLine>(line7.PK);
						var invoiceLine8 = newFactory.Load<BaseJobComInvoiceLine>(line8.PK);

						var lineForKeep = new[] { invoiceLine1, invoiceLine3, invoiceLine4, invoiceLine5, invoiceLine7, invoiceLine8 };

						Assert("These lines should not be deleted in Partial mode.", lineForKeep.All(c => c != null));

						AssertNull("This line should be deleted in Partial mode as there is no matching key in the XML.", invoiceLine2);
						AssertNull("This line should be deleted in Partial mode as it links to a different parent line from the XML.", invoiceLine6);

						var expectedDescriptions = new[] { "ImportLine001", "ImportLine002", "ImportLine003", "ImportLine004", "Line4", "Line5", "Line7", "Line8" };
						var actualDescriptions = jobComInvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

						AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
					});

					CombineAssertions("Assertions in Complete mode.", () =>
					{
						newFactory = NewUniversalObjectFactory();

						invoiceDataObject.CommercialInvoiceLineCollection.Content = CollectionContent.Complete;

						var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, newFactory);

						var jobDeclaration = reader.ReadIntoBusinessObject();
						AssertEquals("Should find the target delcaration and import data on it.", declaration.PK, jobDeclaration.PK);

						AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
						AssertEquals("Have 4 invoice lines", 4, jobDeclaration.InvoiceLines.Count);

						var jobComInvoiceHeader = jobDeclaration.Invoices[0];
						AssertEquals("Should find the target invoice header and import data on it.", invoice.PK, jobComInvoiceHeader.PK);

						var invoiceLine1 = newFactory.Load<BaseJobComInvoiceLine>(line1.PK);
						var invoiceLine2 = newFactory.Load<BaseJobComInvoiceLine>(line2.PK);
						var invoiceLine3 = newFactory.Load<BaseJobComInvoiceLine>(line3.PK);
						var invoiceLine4 = newFactory.Load<BaseJobComInvoiceLine>(line4.PK);
						var invoiceLine5 = newFactory.Load<BaseJobComInvoiceLine>(line5.PK);
						var invoiceLine6 = newFactory.Load<BaseJobComInvoiceLine>(line6.PK);
						var invoiceLine7 = newFactory.Load<BaseJobComInvoiceLine>(line7.PK);
						var invoiceLine8 = newFactory.Load<BaseJobComInvoiceLine>(line8.PK);

						var lineForKeep = new[] { invoiceLine1, invoiceLine3 };

						Assert("These lines should not be deleted in Complete mode.", lineForKeep.All(c => c != null));

						AssertNull("This line should be deleted in Complete mode as the JI_MatchingKey is empty.", invoiceLine2);
						AssertNull("This line should be deleted in Complete mode as the JI_MatchingKey is not in XML.", invoiceLine4);
						AssertNull("This line should be deleted in Complete mode as the JI_MatchingKey is empty.", invoiceLine5);
						AssertNull("This line should be deleted in Complete mode as it links to a different parent line from the XML .", invoiceLine6);
						AssertNull("This line should be deleted in Complete mode as the JI_MatchingKey is not in XML.", invoiceLine7);
						AssertNull("This line should be deleted in Complete mode as the invoiceLine7 is deleted.", invoiceLine8);

						var expectedDescriptions = new[] { "ImportLine001", "ImportLine002", "ImportLine003", "ImportLine004" };
						var actualDescriptions = jobComInvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

						AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
					});
				}
			}

			AssertImportInvoiceLineCollectionInCountry(Core.Constants.CountryCodes.China);
			AssertImportInvoiceLineCollectionInCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		public void TestMatchedLinesWithDifferentRelationshipSetting()
		{
			void AssertImportInvoiceLineCollectionInCountry(string countryCode)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					#region Prepare Data

					var newFactory = NewUniversalObjectFactory();
					var declaration = newFactory.NewWithValidTestData<BaseJobDeclaration>();

					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_InvoiceNumber = "INV1";

					var line1 = invoice.JobComInvoiceLines.AddNew();
					line1.JI_Description = "Line1";
					line1.JI_MatchingKey = "MATCH001";

					var line2 = invoice.JobComInvoiceLines.AddNew();
					line2.JI_Description = "Line2";
					line2.JI_ParentID = line1.PK;
					line2.JI_MatchingKey = "MATCH002";

					var line3 = invoice.JobComInvoiceLines.AddNew();
					line3.JI_Description = "Line3";
					line3.JI_MatchingKey = "MATCH003";

					var line4 = invoice.JobComInvoiceLines.AddNew();
					line4.JI_Description = "Line4";
					line4.JI_MatchingKey = "MATCH004";

					var line5 = invoice.JobComInvoiceLines.AddNew();
					line5.JI_Description = "Line5";
					line5.JI_ParentID = line4.PK;
					line5.JI_MatchingKey = "MATCH005";

					newFactory.SaveForTesting();

					var dataContext = DataContextFactory.New();
					dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
					dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

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
						DataImportMatchingKey = "MATCH003",
						ParentLineNo = 1
					};

					var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine
					{
						LineNo = 3,
						Description = "ImportLine003",
						DataImportMatchingKey = "MATCH005"
					};

					var invoiceLineDataObject4 = new UniversalCustoms.CommercialInvoiceLine
					{
						LineNo = 4,
						Description = "ImportLine004",
						DataImportMatchingKey = "MATCH004"
					};

					var invoiceLineDataObject5 = new UniversalCustoms.CommercialInvoiceLine
					{
						LineNo = 5,
						Description = "ImportLine005",
						DataImportMatchingKey = "MATCH006",
						ParentLineNo = 3
					};

					var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV1",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3, invoiceLineDataObject4, invoiceLineDataObject5
							})
						{ Content = CollectionContent.Partial }));

					var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						DataContext = dataContext,
						MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
						CommercialInfo = new UniversalCustoms.CommercialInfo()
						{
							CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject })
						}
					};

					#endregion

					CombineAssertions("Assertions in Partial mode.", () =>
					{
						newFactory = NewUniversalObjectFactory();

						var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, newFactory);

						var jobDeclaration = reader.ReadIntoBusinessObject();
						AssertEquals("Should find the target delcaration and import data on it.", declaration.PK, jobDeclaration.PK);

						AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
						AssertEquals("Have 5 invoice lines", 5, jobDeclaration.InvoiceLines.Count);

						var jobComInvoiceHeader = jobDeclaration.Invoices[0];
						AssertEquals("Should find the target invoice header and import data on it.", invoice.PK, jobComInvoiceHeader.PK);

						var invoiceLine1 = newFactory.Load<BaseJobComInvoiceLine>(line1.PK);
						var invoiceLine2 = newFactory.Load<BaseJobComInvoiceLine>(line2.PK);
						var invoiceLine3 = newFactory.Load<BaseJobComInvoiceLine>(line3.PK);
						var invoiceLine4 = newFactory.Load<BaseJobComInvoiceLine>(line4.PK);
						var invoiceLine5 = newFactory.Load<BaseJobComInvoiceLine>(line5.PK);

						var lineForKeep = new[] { invoiceLine1, invoiceLine2, invoiceLine4 };

						Assert("These lines should not be deleted in Partial mode.", lineForKeep.All(c => c != null));

						AssertNull("This line should be deleted in Partial mode as it's a parent line in DB but a child line in XML.", invoiceLine3);
						AssertNull("This line should be deleted in Partial mode as it's a child line in DB but a parent line in XML.", invoiceLine5);

						var expectedDescriptions = new[] { "ImportLine001", "ImportLine002", "ImportLine003", "ImportLine004", "ImportLine005" };
						var actualDescriptions = jobComInvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

						AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
					});

					CombineAssertions("Assertions in Complete mode.", () =>
					{
						newFactory = NewUniversalObjectFactory();

						invoiceDataObject.CommercialInvoiceLineCollection.Content = CollectionContent.Complete;

						var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, newFactory);

						var jobDeclaration = reader.ReadIntoBusinessObject();
						AssertEquals("Should find the target delcaration and import data on it.", declaration.PK, jobDeclaration.PK);

						AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
						AssertEquals("Have 5 invoice lines", 5, jobDeclaration.InvoiceLines.Count);

						var jobComInvoiceHeader = jobDeclaration.Invoices[0];
						AssertEquals("Should find the target invoice header and import data on it.", invoice.PK, jobComInvoiceHeader.PK);

						var invoiceLine1 = newFactory.Load<BaseJobComInvoiceLine>(line1.PK);
						var invoiceLine2 = newFactory.Load<BaseJobComInvoiceLine>(line2.PK);
						var invoiceLine3 = newFactory.Load<BaseJobComInvoiceLine>(line3.PK);
						var invoiceLine4 = newFactory.Load<BaseJobComInvoiceLine>(line4.PK);
						var invoiceLine5 = newFactory.Load<BaseJobComInvoiceLine>(line5.PK);

						var lineForKeep = new[] { invoiceLine1, invoiceLine4 };

						Assert("These lines should not be deleted in Complete mode.", lineForKeep.All(c => c != null));

						AssertNull("This line should be deleted in Complete mode as the JI_MatchingKey is not in XML.", invoiceLine2);
						AssertNull("This line should be deleted in Complete mode as it's a parent line in DB but a child line in XML.", invoiceLine3);
						AssertNull("This line should be deleted in Complete mode as it's a child line in DB but a parent line in XML.", invoiceLine5);

						var expectedDescriptions = new[] { "ImportLine001", "ImportLine002", "ImportLine003", "ImportLine004", "ImportLine005" };
						var actualDescriptions = jobComInvoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().Select(c => c.JI_Description);

						AssertContainsExactElementsInAnyOrder("Should sync all values from the XML.", expectedDescriptions, actualDescriptions);
					});
				}
			}

			AssertImportInvoiceLineCollectionInCountry(Core.Constants.CountryCodes.China);
			AssertImportInvoiceLineCollectionInCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		public void TestImportingInvoiceLineContentPartial_InvoiceHeaderContentIsComplete()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(CollectionContent.Complete, CollectionContent.Partial);
				AssertEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 7, invoiceLines.Length);
				AssertInvoiceLine("Existing Line 1", invoiceLines[0], 1, "Line1", "MATCH001", line1.PK);
				AssertInvoiceLine("Existing Line 3", invoiceLines[1], 2, "Line3", line3.PK);
				AssertInvoiceLine("New Line 1", invoiceLines[2], 3, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[3], 4, "Line6Import", "MATCH006");
				AssertInvoiceLine("Existing Line 2", invoiceLines[4], 5, "Line2Import", "MATCH002", line2.PK);
				AssertInvoiceLine("New Line 4", invoiceLines[5], 6, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[6], 7, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentComplete_InvoiceHeaderContentIsComplete()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(CollectionContent.Complete, CollectionContent.Complete);
				AssertEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 5, invoiceLines.Length);
				AssertNull("Existing Line 1", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line1.PK));
				AssertNull("Existing Line 3", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line3.PK));
				AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[1], 2, "Line6Import", "MATCH006");
				AssertInvoiceLine("Existing Line 2", invoiceLines[2], 3, "Line2Import", "MATCH002", line2.PK);
				AssertInvoiceLine("New Line 4", invoiceLines[3], 4, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[4], 5, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentNull_InvoiceHeaderContentIsComplete()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(CollectionContent.Complete, null);
				AssertNotEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 5, invoiceLines.Length);
				AssertNull("Existing Line 1", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line1.PK));
				AssertNull("Existing Line 2", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line2.PK));
				AssertNull("Existing Line 3", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line3.PK));
				AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[1], 2, "Line6Import", "MATCH006");
				AssertInvoiceLine("New Line 3", invoiceLines[2], 3, "Line2Import", "MATCH002");
				AssertInvoiceLine("New Line 4", invoiceLines[3], 4, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[4], 5, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentPartial_InvoiceHeaderContentIsPartial()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(CollectionContent.Partial, CollectionContent.Partial);
				AssertEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 7, invoiceLines.Length);
				AssertInvoiceLine("Existing Line 1", invoiceLines[0], 1, "Line1", "MATCH001", line1.PK);
				AssertInvoiceLine("Existing Line 3", invoiceLines[1], 2, "Line3", line3.PK);
				AssertInvoiceLine("New Line 1", invoiceLines[2], 3, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[3], 4, "Line6Import", "MATCH006");
				AssertInvoiceLine("Existing Line 2", invoiceLines[4], 5, "Line2Import", "MATCH002", line2.PK);
				AssertInvoiceLine("New Line 4", invoiceLines[5], 6, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[6], 7, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentComplete_InvoiceHeaderContentIsPartial()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(CollectionContent.Partial, CollectionContent.Complete);
				AssertEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 5, invoiceLines.Length);
				AssertNull("Existing Line 1", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line1.PK));
				AssertNull("Existing Line 3", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line3.PK));
				AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[1], 2, "Line6Import", "MATCH006");
				AssertInvoiceLine("Existing Line 2", invoiceLines[2], 3, "Line2Import", "MATCH002", line2.PK);
				AssertInvoiceLine("New Line 4", invoiceLines[3], 4, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[4], 5, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentNull_InvoiceHeaderContentIsPartial()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(CollectionContent.Partial, null);
				AssertEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 5, invoiceLines.Length);
				AssertNull("Existing Line 1", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line1.PK));
				AssertNull("Existing Line 2", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line2.PK));
				AssertNull("Existing Line 3", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line3.PK));
				AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[1], 2, "Line6Import", "MATCH006");
				AssertInvoiceLine("New Line 3", invoiceLines[2], 3, "Line2Import", "MATCH002");
				AssertInvoiceLine("New Line 4", invoiceLines[3], 4, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[4], 5, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentPartial_InvoiceHeaderContentIsNull()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(null, CollectionContent.Partial);
				AssertEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 7, invoiceLines.Length);
				AssertInvoiceLine("Existing Line 1", invoiceLines[0], 1, "Line1", "MATCH001", line1.PK);
				AssertInvoiceLine("Existing Line 3", invoiceLines[1], 2, "Line3", line3.PK);
				AssertInvoiceLine("New Line 1", invoiceLines[2], 3, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[3], 4, "Line6Import", "MATCH006");
				AssertInvoiceLine("Existing Line 2", invoiceLines[4], 5, "Line2Import", "MATCH002", line2.PK);
				AssertInvoiceLine("New Line 4", invoiceLines[5], 6, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[6], 7, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentComplete_InvoiceHeaderContentIsNull()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(null, CollectionContent.Complete);
				AssertEquals("Should match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 5, invoiceLines.Length);
				AssertNull("Existing Line 1", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line1.PK));
				AssertNull("Existing Line 3", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line3.PK));
				AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[1], 2, "Line6Import", "MATCH006");
				AssertInvoiceLine("Existing Line 2", invoiceLines[2], 3, "Line2Import", "MATCH002", line2.PK);
				AssertInvoiceLine("New Line 4", invoiceLines[3], 4, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[4], 5, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentNull_InvoiceHeaderContentIsNull()
		{
			CombineAssertions(() =>
			{
				(var invoice, var line1, var line2, var line3, var invoiceFromImport) = SetupDataForImportInvoiceLineWithContentTesting(null, null);
				AssertNotEquals("Should not match Invoice", invoice.PK, invoiceFromImport.PK);
				var invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
				AssertEquals("No of invoice lines", 5, invoiceLines.Length);
				AssertNull("Existing Line 1", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line1.PK));
				AssertNull("Existing Line 2", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line2.PK));
				AssertNull("Existing Line 3", invoiceFromImport.Factory.Load<BaseJobComInvoiceLine>(line3.PK));
				AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line5Import", "MATCH005");
				AssertInvoiceLine("New Line 2", invoiceLines[1], 2, "Line6Import", "MATCH006");
				AssertInvoiceLine("New Line 3", invoiceLines[2], 3, "Line2Import", "MATCH002");
				AssertInvoiceLine("New Line 4", invoiceLines[3], 4, "Line4Import", "MATCH004");
				AssertInvoiceLine("New Line 5", invoiceLines[4], 5, "LineNullImport", "MATCHNULL");
			});
		}

		public void TestImportingInvoiceLineContentComplete_WithEmptyMatchingKeys()
		{
			BaseJobComInvoiceHeader invoice;
			BaseJobComInvoiceLine line1;
			BaseJobComInvoiceHeader invoiceFromImport;
			BaseJobComInvoiceLine[] invoiceLines;

			(invoice, line1, invoiceFromImport) = SetupDataForImportInvoiceLineWithEmptyMatchingKeys(CollectionContent.Complete, CollectionContent.Complete);
			invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
			AssertEquals("No of invoice lines", 1, invoiceLines.Length);
			AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line1Import", "");

			(invoice, line1, invoiceFromImport) = SetupDataForImportInvoiceLineWithEmptyMatchingKeys(CollectionContent.Partial, CollectionContent.Complete);
			invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
			AssertEquals("No of invoice lines", 1, invoiceLines.Length);
			AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line1Import", "");

			(invoice, line1, invoiceFromImport) = SetupDataForImportInvoiceLineWithEmptyMatchingKeys(null, CollectionContent.Complete);
			invoiceLines = invoiceFromImport.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ThenBy(x => x.JI_Description).ToArray();
			AssertEquals("No of invoice lines", 1, invoiceLines.Length);
			AssertInvoiceLine("New Line 1", invoiceLines[0], 1, "Line1Import", "");
		}

		(BaseJobComInvoiceHeader invoice, BaseJobComInvoiceLine line1, BaseJobComInvoiceHeader invoiceFromImport) SetupDataForImportInvoiceLineWithEmptyMatchingKeys(CollectionContent? invoiceCollectionContent, CollectionContent? invoiceLineCollectionContent)
		{
			var newFactory = NewUniversalObjectFactory();
			var declaration = newFactory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-NOKEY";

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "Line1";
			line1.JI_MatchingKey = ZString.Empty;

			newFactory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

			var commercialInvoiceLineCollectionDataObject = new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
			{
				new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = null, Description = "Line1Import", DataImportMatchingKey = ""
				}
			})
			{
				Content = invoiceLineCollectionContent
			};
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV-NOKEY",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => commercialInvoiceLineCollectionDataObject))
					})
					{
						Content = invoiceCollectionContent
					}
				}
			};
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, new TestErrorLogger(), newFactory);
			var jobDeclaration = reader.ReadIntoBusinessObject();
			AssertEquals("Should match Declaration", declaration.PK, jobDeclaration.PK);
			AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
			var invoiceFromImport = jobDeclaration.Invoices[0];
			return (invoice, line1, invoiceFromImport);
		}

		(BaseJobComInvoiceHeader invoice, BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2, BaseJobComInvoiceLine line3, BaseJobComInvoiceHeader invoiceFromImport) SetupDataForImportInvoiceLineWithContentTesting(CollectionContent? invoiceCollectionContent, CollectionContent? invoiceLineCollectionContent)
		{
			var newFactory = NewUniversalObjectFactory();
			var declaration = newFactory.New<BaseJobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "Line1";
			line1.JI_MatchingKey = "MATCH001";

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Description = "Line2";
			line2.JI_MatchingKey = "MATCH002";

			var line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_Description = "Line3";
			line3.JI_MatchingKey = ZString.Empty;

			newFactory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

			var commercialInvoiceLineCollectionDataObject = new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
			{
				new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = null, Description = "LineNullImport", DataImportMatchingKey = "MATCHNULL"
				},
				new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 1, Description = "Line5Import", DataImportMatchingKey = "MATCH005"
				},
				new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 2, Description = "Line6Import", DataImportMatchingKey = "MATCH006"
				},
				new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 3, Description = "Line2Import", DataImportMatchingKey = "MATCH002"
				},
				new UniversalCustoms.CommercialInvoiceLine
				{
					LineNo = 4, Description = "Line4Import", DataImportMatchingKey = "MATCH004"
				}
			})
			{
				Content = invoiceLineCollectionContent
			};
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV1",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => commercialInvoiceLineCollectionDataObject))
					})
					{
						Content = invoiceCollectionContent
					}
				}
			};
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var reader = new JobDeclarationDataObjectReader(declarationDataObject, new TestErrorLogger(), newFactory);
			var jobDeclaration = reader.ReadIntoBusinessObject();
			AssertEquals("Should match Declaration", declaration.PK, jobDeclaration.PK);
			AssertEquals("Have 1 invoice header", 1, jobDeclaration.Invoices.Count);
			var invoiceFromImport = jobDeclaration.Invoices[0];
			return (invoice, line1, line2, line3, invoiceFromImport);
		}

		void AssertInvoiceLine(string messagePrefix, BaseJobComInvoiceLine invoiceLine, ZShort lineNo, ZString description, ZString matchingKey, ZGuid? pk = null)
		{
			AssertInvoiceLineNoAndDescriptionAndPK(messagePrefix, invoiceLine, lineNo, description, pk);
			AssertEquals(messagePrefix + " MatchingKey", matchingKey, invoiceLine.JI_MatchingKey);
		}

		void AssertInvoiceLine(string messagePrefix, BaseJobComInvoiceLine invoiceLine, ZShort lineNo, ZString description, ZGuid? pk = null)
		{
			AssertInvoiceLineNoAndDescriptionAndPK(messagePrefix, invoiceLine, lineNo, description, pk);
			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID;
			AssertEquals(messagePrefix + " MatchingKey", true, new Regex(prefix + "\\d{8}").IsMatch(invoiceLine.JI_MatchingKey));
		}

		void AssertInvoiceLineNoAndDescriptionAndPK(string messagePrefix, BaseJobComInvoiceLine invoiceLine, ZShort lineNo, ZString description, ZGuid? pk = null)
		{
			AssertEquals(messagePrefix + " LineNo", lineNo, invoiceLine.JI_LineNo);
			AssertEquals(messagePrefix + " Description", description, invoiceLine.JI_Description);
			if (pk.HasValue)
			{
				AssertEquals(messagePrefix + " PK", pk, invoiceLine.PK);
			}
		}
	}
}

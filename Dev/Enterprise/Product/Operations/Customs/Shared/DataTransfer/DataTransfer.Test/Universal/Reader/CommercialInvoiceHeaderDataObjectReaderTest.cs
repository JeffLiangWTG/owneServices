using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using static Enterprise.Integration.Customs;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using IAdditionalLineTariffDetailParent = Enterprise.Customs.Business.IAdditionalLineTariffDetailParent;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestImportCustomAttributesOnInvoiceLine()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>  new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									CustomAttributeCollection = new List<UniversalCustoms.CustomAttribute>()
									{
										new UniversalCustoms.CustomAttribute() { Key = Constants.CustomAttributeKeys.CustomAttribute1, Value = "CustomAttrib 1" },
										new UniversalCustoms.CustomAttribute() { Key = Constants.CustomAttributeKeys.CustomAttribute2, Value = "CustomAttrib 2" },
										new UniversalCustoms.CustomAttribute() { Key = Constants.CustomAttributeKeys.CustomAttribute3, Value = "CustomAttrib 3" },
										new UniversalCustoms.CustomAttribute() { Key = Constants.CustomAttributeKeys.CustomAttribute4, Value = "CustomAttrib 4" },
										new UniversalCustoms.CustomAttribute() { Key = Constants.CustomAttributeKeys.CustomAttribute5, Value = "CustomAttrib 5" },
										new UniversalCustoms.CustomAttribute() { Key = Constants.CustomAttributeKeys.CustomAttribute6, Value = "CustomAttrib 6" },
										new UniversalCustoms.CustomAttribute() { Key = "CustomAttributeXXX", Value = "CustomAttrib 7" },
									}
								}
							}))
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var line = declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("invoiceLine.JI_CustomAttrib1", "CustomAttrib 1", line.JI_CustomAttrib1);
				AssertEquals("invoiceLine.JI_CustomAttrib2", "CustomAttrib 2", line.JI_CustomAttrib2);
				AssertEquals("invoiceLine.JI_CustomAttrib3", "CustomAttrib 3", line.JI_CustomAttrib3);
				AssertEquals("invoiceLine.JI_CustomAttrib4", "CustomAttrib 4", line.JI_CustomAttrib4);
				AssertEquals("invoiceLine.JI_CustomAttrib5", "CustomAttrib 5", line.JI_CustomAttrib5);
				AssertEquals("invoiceLine.JI_CustomAttrib6", "CustomAttrib 6", line.JI_CustomAttrib6);
				AssertEquals("Unknown CustomAttribute key 'CustomAttributeXXX'", logger.GetWarnings());
			});
		}

		public void TestMatchingInvoiceLineWillDeleteExistingCharges()
		{
			var existingDeclaration = Factory.New<BaseJobDeclaration>();
			existingDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			existingDeclaration.JE_OwnerRef = "BOB1";
			existingDeclaration.JE_MasterBill = "MB23422";
			var entryHeader = existingDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = JobMessageTypeList.Codes.Export;
			entryHeader.EntryNumber = "11122233300012";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var existingInvoice = existingDeclaration.Invoices.AddNew();
			existingInvoice.JZ_InvoiceNumber = "INV123ABC";
			existingInvoice.JZ_InvoiceAmount = 1000m;
			existingInvoice.Charges.AddNew("OFT", 100m);
			existingInvoice.Charges.AddNew("ONS", 200m);
			existingInvoice.GroupCharges.AddNew("DIS", 60m);

			var existingInvoiceLine1 = existingInvoice.InvoiceLines.AddNew();
			existingInvoiceLine1.JI_LineNo = 1;
			existingInvoiceLine1.JI_MatchingKey = "INVLINE123ABC";
			existingInvoiceLine1.JI_CL = entryLine1.PK;
			existingInvoiceLine1.JI_LinePrice = 600m;
			existingInvoiceLine1.ApportionedCharges.AddNew("OFT", 60m);
			existingInvoiceLine1.ApportionedCharges.AddNew("ONS", 120m);
			existingInvoiceLine1.Charges.AddNew("DIS", 10m);

			var existingInvoiceLine2 = existingInvoice.InvoiceLines.AddNew();
			existingInvoiceLine2.JI_LineNo = 2;
			existingInvoiceLine2.JI_MatchingKey = "INVLINE456ABC";
			existingInvoiceLine2.JI_CL = entryLine2.PK;
			existingInvoiceLine2.JI_LinePrice = 400m;
			existingInvoiceLine2.ApportionedCharges.AddNew("OFT", 40m);
			existingInvoiceLine2.ApportionedCharges.AddNew("ONS", 80m);
			existingInvoiceLine2.Charges.AddNew("DIS", 20m);

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var entryHeaderObject = new UniversalCustoms.EntryHeader()
			{
				Type = new EntryType() { Code = entryHeader.CH_MessageType },
				Reference = existingDeclaration.JE_DeclarationReference + "/11122233300012"
			};
			entryHeaderObject.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber(new EntryType() { Code = entryHeader.CH_MessageType, Description = "B12 DESC" }, "11122233300012", ZBool.False) });

			var entryLineObject1 = new UniversalCustoms.EntryLine() { LineNumber = 1 };
			var entryLineObject2 = new UniversalCustoms.EntryLine() { LineNumber = 2 };
			entryHeaderObject.EntryLineCollection = new List<UniversalCustoms.EntryLine>() { entryLineObject1, entryLineObject2 };

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "BOB1",
				WayBillNumber = "MB23422",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV123ABC",
							CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[]
							{
								new UniversalCustoms.CommercialCharge()
								{
									ChargeType = new CodeDescriptionPair { Code = "OFT" },
									Amount = 150m,
								},
								new UniversalCustoms.CommercialCharge()
								{
									ChargeType = new CodeDescriptionPair { Code = "ONS" },
									Amount = 220m,
								},
								new UniversalCustoms.CommercialCharge()
								{
									ChargeType = new CodeDescriptionPair { Code = "DIS" },
									Amount = 30m,
									IsApportionedCharge = true,
								},
							})
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									DataImportMatchingKey = "INVLINE123ABC", LineNo = 1, EntryLineNumber = 1, EntryNumber = "11122233300012", LinePrice = 400m,
									CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[]
									{
										new UniversalCustoms.CommercialCharge()
										{
											ChargeType = new CodeDescriptionPair { Code = "OFT" },
											Amount = 55m,
											IsApportionedCharge = true,
										},
										new UniversalCustoms.CommercialCharge()
										{
											ChargeType = new CodeDescriptionPair { Code = "ONS" },
											Amount = 110m,
											IsApportionedCharge = true,
										},
										new UniversalCustoms.CommercialCharge()
										{
											ChargeType = new CodeDescriptionPair { Code = "DIS" },
											Amount = 15m,
											IsApportionedCharge = false,
										},
										new UniversalCustoms.CommercialCharge()
										{
											ChargeType = new CodeDescriptionPair { Code = "PAC" },
											Amount = 25m,
											IsApportionedCharge = false,
										},
									})
								},
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									DataImportMatchingKey = "INVLINE456ABC", LineNo = 2, EntryLineNumber = 2, EntryNumber = "11122233300012",
								}
							}) { Content = CollectionContent.Partial }
						))
					})
				}
			}.AdditionalSetup(x => x.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>() { entryHeaderObject }));

			var reader = new JobDeclarationDataObjectReader(shipment, logger, Factory);
			var declaration = reader.ReadIntoBusinessObject();
			var invoice = declaration.Invoices.Single();

			CombineAssertions(() =>
			{
				AssertEquals("invoice.GroupCharges.Count - we don't import apportioned charges", 0, invoice.GroupCharges.Count);
				AssertEquals("invoice.Charges.Count", 2, invoice.Charges.Count);
				AssertEquals("invoice.Charges - OFT", 150m, invoice.Charges.GetCharge("OFT").Single().J7_Amount);
				AssertEquals("invoice.Charges - ONS", 220m, invoice.Charges.GetCharge("ONS").Single().J7_Amount);

				AssertEquals("invoice.InvoiceLines.Count", 2, invoice.InvoiceLines.Count);
				var invoiceLine = invoice.InvoiceLines[0].JI_MatchingKey == "INVLINE123ABC" ? invoice.InvoiceLines[0] : invoice.InvoiceLines[1];
				AssertEquals("invoiceLine.PK", existingInvoiceLine1.PK, invoiceLine.PK);
				AssertEquals("invoiceLine.ApportionedCharges.Count - we don't import apportioned charges", 0, invoiceLine.ApportionedCharges.Count);
				AssertEquals("invoiceLine.Charges.Count", 2, invoiceLine.Charges.Count);
				AssertEquals("invoiceLine.Charges - DIS", 15m, invoiceLine.Charges.GetCharge("DIS").Single().J7_Amount);
				AssertEquals("invoiceLine.Charges - PAC", 25m, invoiceLine.Charges.GetCharge("PAC").Single().J7_Amount);
			});
		}

		public void TestClassUsageComment()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.InvoiceLines.RemoveAndDeleteAll();

			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new UniversalCustoms.CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>() }
			};

			var invoiceCollection = declarationDataObject.CommercialInfo.CommercialInvoiceCollection;

			invoiceCollection.Add(new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV0001",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
				{
					new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						LineNo = 1,
						Description = "LINE0001",
						ClassUsageComment = "TEST USAGE COMMENT",
						ClassUsageCommentStaff = new Staff() { Code = "TST" }
					}
				}))));

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory, null);
			declaration = reader.ReadIntoBusinessObject();

			var invoiceLine = declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().First(c => c.JI_Description == "LINE0001");

			AssertEquals("Should set the value on JI_ClassUsageComment", "TEST USAGE COMMENT", invoiceLine.JI_ClassUsageComment);
			AssertEquals("Should set the value on JI_GS_NKClassUsageCommentReviewer", "TST", invoiceLine.JI_GS_NKClassUsageCommentReviewer);
			Assert("Should be true as there is a reviewer.", invoiceLine.JI_IsClassUsageCommentRead);
		}

		public void TestLinkOrderLinesToInvoiceLines()
		{
			var orgheader = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var businessOrder = Factory.BOFactory.New<Freight.Forwarding.Orders.Business.Order>();
			businessOrder.JD_OrderNumber = "MYTESTORDER1";
			businessOrder.JD_OrderNumberSplit = 1;
			businessOrder.BuyerPK = orgheader.PK;
			var orderLine1 = businessOrder.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			var orderLine2 = businessOrder.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;

			var businessOrder2 = Factory.BOFactory.New<Freight.Forwarding.Orders.Business.Order>();
			businessOrder2.JD_OrderNumber = "MYTESTORDER1";
			businessOrder2.JD_OrderNumberSplit = 0;
			businessOrder2.BuyerPK = orgheader.PK;
			var orderLine11 = businessOrder2.OrderLines.AddNew();
			orderLine11.JO_LineNo = 1;
			var orderLine12 = businessOrder2.OrderLines.AddNew();
			orderLine12.JO_LineNo = 2;
			var orderLine3 = businessOrder2.OrderLines.AddNew();
			orderLine3.JO_LineNo = 3;
			Factory.SaveForTesting();

			var businessOrder3 = Factory.BOFactory.New<Freight.Forwarding.Orders.Business.Order>();
			businessOrder3.JD_OrderNumber = "MYTESTORDER1";
			businessOrder3.JD_OrderNumberSplit = 2;
			businessOrder3.BuyerPK = orgheader.PK;
			var orderLine4 = businessOrder3.OrderLines.AddNew();
			orderLine4.JO_LineNo = 4;
			Factory.SaveForTesting();

			var declarationforupdate = Factory.New<BaseJobDeclaration>();
			declarationforupdate.JE_DeclarationReference = "B00001111";
			declarationforupdate.AttachedOrders.Add(businessOrder);
			declarationforupdate.AttachedOrders.Add(businessOrder2);
			Factory.SaveForTesting();

			var consigneeAddressCRAHOLSYDDataObject = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsigneeDocumentaryAddress);
			var consigneeOrgCRAHOLSYD = new OrganisationDataObjectReader(consigneeAddressCRAHOLSYDDataObject, new UniversalDataBuss.Core.Testing.TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header;
			Factory.SaveForTesting();

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001111");
			declarationDataObject.DataContext = dataContext;
			declarationDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			declarationDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ImporterDocumentaryAddress),
				OrganizationCode = orgheader.OH_Code,
				Address1 = "1804 Fudrucker Way",
				City = "BOTANY",
				Postcode = "2035"
			});

			declarationDataObject.SetRelatedShipmentCollection(() => new List<Shipment>());

			var orderData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			orderData.DataContext = DataContextFactory.New();
			orderData.DataContext.AddDataTarget(DataContextType.OrderManagerOrder, "MYTESTORDER1~1~" + orgheader.OH_Code);

			orderData.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "MYTESTORDER1", OrderNumberSplit = new ZByte(1) };
			orderData.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>
			{
				new OrderLine { LineNumber = 1 },
				new OrderLine { LineNumber = 2 }
			});
			declarationDataObject.RelatedShipmentCollection.Add(orderData);

			var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				Description = "LINE 1",
				OrderLineLink = 1,
				OrderNumber = "MYTESTORDER1-1"
			};
			var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 2,
				Description = "LINE 2",
				OrderLineLink = 2,
				OrderNumber = "MYTESTORDER1-1"
			};

			var invoiceDataObject1 = SetupCommercialInvoiceHeaderData(commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1, invoiceLineDataObject2 }));
			declarationDataObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject1 }),
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			var invoiceLine1 = declarationBO.InvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 1);
			AssertEquals(orderLine1.PK, invoiceLine1.JI_JO);

			var invoiceLine2 = declarationBO.InvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 2);
			AssertEquals(orderLine2.PK, invoiceLine2.JI_JO);

			declarationforupdate.JE_DeclarationReference = "B00001112";
			Factory.SaveForTesting();
			var dataContext2 = DataContextFactory.New();
			dataContext2.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext2.AddDataTarget(DataContextType.CustomsDeclaration, "B00001112");
			declarationDataObject.DataContext = dataContext2;

			CombineAssertions("Default get split 0", () =>
			{
				var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					LineNo = 3,
					Description = "LINE 3",
					OrderLineLink = 3,
					OrderNumber = "MYTESTORDER1"
				};

				var orderData2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				orderData2.DataContext = DataContextFactory.New();
				orderData2.DataContext.AddDataTarget(DataContextType.OrderManagerOrder, "MYTESTORDER1~0~" + orgheader.OH_Code);

				orderData2.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "MYTESTORDER1", OrderNumberSplit = new ZByte(0) };
				orderData2.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>
				{
					new OrderLine { LineNumber = 3 }
				});
				declarationDataObject.RelatedShipmentCollection.Add(orderData2);

				declarationDataObject.CommercialInfo.CommercialInvoiceCollection.First().CommercialInvoiceLineCollection.Add(invoiceLineDataObject3);
				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				declarationBO = reader.ReadIntoBusinessObject();
				invoiceLine1 = declarationBO.InvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 3);
				AssertEquals(orderLine3.PK, invoiceLine1.JI_JO);
			});

			declarationforupdate.JE_DeclarationReference = "B00001113";
			Factory.SaveForTesting();
			var dataContext3 = DataContextFactory.New();
			dataContext3.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext3.AddDataTarget(DataContextType.CustomsDeclaration, "B00001113");
			declarationDataObject.DataContext = dataContext3;
			CombineAssertions("Get split 2", () =>
			{
				var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					LineNo = 4,
					Description = "LINE 4",
					OrderLineLink = 4,
					OrderNumber = "MYTESTORDER1-2"
				};

				var orderData2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				orderData2.DataContext = DataContextFactory.New();
				orderData2.DataContext.AddDataTarget(DataContextType.OrderManagerOrder, "MYTESTORDER1~2~" + orgheader.OH_Code);
				orderData2.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "MYTESTORDER1", OrderNumberSplit = new ZByte(2) };
				orderData2.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>
				{
					new OrderLine { LineNumber = 4 }
				});
				declarationDataObject.RelatedShipmentCollection.Add(orderData2);

				declarationDataObject.CommercialInfo.CommercialInvoiceCollection.First().CommercialInvoiceLineCollection.Add(invoiceLineDataObject3);
				reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				declarationBO = reader.ReadIntoBusinessObject();
				invoiceLine1 = declarationBO.InvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_LineNo == 4);
				AssertEquals(orderLine4.PK, invoiceLine1.JI_JO);
			});
		}

		public void TestEntryLink_IntegratedCountryButNotIntegratedJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = (BaseJobDeclaration)Factory.BOFactory.New<ZA.IJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_DeclarationReference = "B03243223";
				var entryInstruction1 = (CusEntryInstruction)Factory.BOFactory.New<ZA.ICusEntryInstruction>();
				entryInstruction1.CEI_JE = declaration.PK;
				entryInstruction1.CEI_Style = "10";
				var entryInstruction2 = (CusEntryInstruction)Factory.BOFactory.New<ZA.ICusEntryInstruction>();
				entryInstruction2.CEI_JE = declaration.PK;
				entryInstruction2.CEI_Style = "20";
				var entryInstruction3 = (CusEntryInstruction)Factory.BOFactory.New<ZA.ICusEntryInstruction>();
				entryInstruction3.CEI_JE = declaration.PK;
				entryInstruction3.CEI_Style = "30";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = JobMessageTypeList.Codes.Import;
				entry.CH_CEI_Instruction = entryInstruction2.PK;
				entry.CH_BGMReference = "LRN3242";
				entry.EntryNumber = "ENT3423";
				var entryLine = entry.AllEntryLines.AddNew();
				entryLine.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV324";
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction2.PK;
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_Description = "BOB THE BUILDER";
				Factory.SaveForTesting();

				var declarationDataObject = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
				declarationDataObject.DataContext = DataContextFactory.New();
				declarationDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				CombineAssertions("ITF", () =>
				{
					declaration.JE_ApplicationCode = "ITF";
					declaration.Invoices.DeleteAll();
					declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
					declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.RemoveAndDeleteAll();
					Factory.SaveForTesting();
					var factory = new UniversalObjectFactory();
					var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, factory);
					var declarationBO = reader.ReadIntoBusinessObject();
					AssertEquals("Should have matched", declaration.PK, declarationBO.PK);
					var customsEntryInstructions = declarationBO.CustomsEntryInstructionProvider.CustomsEntryInstructions;
					AssertEquals("customsEntryInstructions.Count", 3, customsEntryInstructions.Count);
					entryInstruction1 = customsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(x => x.CEI_Style == "10");
					entryInstruction2 = customsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(x => x.CEI_Style == "20");
					entryInstruction3 = customsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(x => x.CEI_Style == "30");
					AssertEquals("entryInstruction1.CEI_Style", "10", entryInstruction1.CEI_Style);
					AssertEquals("entryInstruction2.CEI_Style", "20", entryInstruction2.CEI_Style);
					AssertEquals("entryInstruction3.CEI_Style", "30", entryInstruction3.CEI_Style);
					AssertEquals("declarationBO.CustomsEntryHeaders.Count", 1, declarationBO.CustomsEntryHeaders.Count);
					entry = declarationBO.CustomsEntryHeaders[0];
					AssertEquals("entry.EntryNumber", "ENT3423", entry.EntryNumber);
					AssertEquals("entry.CH_CEI_Instruction", entryInstruction2.PK, entry.CH_CEI_Instruction);
					entry.AllEntryLines.Load();
					AssertEquals("entry.AllEntryLines.Count", 1, entry.AllEntryLines.Count);
					entryLine = entry.AllEntryLines[0];
					AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
					invoice = declarationBO.Invoices[0];
					AssertEquals("invoice.JZ_InvoiceNumber", "INV324", invoice.JZ_InvoiceNumber);
					AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
					invoiceLine = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_CEI", entryInstruction2.PK, invoiceLine.JI_CEI);
					AssertEquals("invoiceLine.JI_CL", entryLine.PK, invoiceLine.JI_CL);
					AssertEquals("invoiceLine.JI_Description", "BOB THE BUILDER", invoiceLine.JI_Description);

					declarationBO.Invoices.DeleteAll();
					declarationBO.CustomsEntryHeaders.RemoveAndDeleteAll();
					declarationBO.CustomsEntryInstructionProvider.CustomsEntryInstructions.RemoveAndDeleteAll();
					factory.SaveForTesting();
				});

				CombineAssertions("BLT", () =>
				{
					declaration.JE_ApplicationCode = "BLT";
					declaration.Invoices.DeleteAll();
					declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
					declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.RemoveAndDeleteAll();
					Factory.SaveForTesting();
					declarationDataObject.MessagingApplicationCode = ListHelper.GetWithDescription<CodeDescriptionPair>(declaration.JE_ApplicationCode, declaration.Lookups.ApplicationCodeList);

					var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, new UniversalObjectFactory());
					var declarationBO = reader.ReadIntoBusinessObject();
					AssertEquals("Should have matched", declaration.PK, declarationBO.PK);
					var customsEntryInstructions = declarationBO.CustomsEntryInstructionProvider.CustomsEntryInstructions;
					AssertEquals("customsEntryInstructions.Count", 3, customsEntryInstructions.Count);
					entryInstruction1 = customsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(x => x.CEI_Style == "10");
					entryInstruction2 = customsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(x => x.CEI_Style == "20");
					entryInstruction3 = customsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault(x => x.CEI_Style == "30");
					AssertEquals("entryInstruction1.CEI_Style", "10", entryInstruction1.CEI_Style);
					AssertEquals("entryInstruction2.CEI_Style", "20", entryInstruction2.CEI_Style);
					AssertEquals("entryInstruction3.CEI_Style", "30", entryInstruction3.CEI_Style);
					AssertEquals("declarationBO.CustomsEntryHeaders.Count", 0, declarationBO.CustomsEntryHeaders.Count);
					AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
					invoice = declarationBO.Invoices[0];
					AssertEquals("invoice.JZ_InvoiceNumber", "INV324", invoice.JZ_InvoiceNumber);
					AssertEquals("invoice.JobComInvoiceLines.Count", 1, invoice.JobComInvoiceLines.Count);
					invoiceLine = invoice.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_CEI", entryInstruction2.PK, invoiceLine.JI_CEI);
					AssertEquals("invoiceLine.JI_CL", ZGuid.Empty, invoiceLine.JI_CL);
					AssertEquals("invoiceLine.JI_Description", "BOB THE BUILDER", invoiceLine.JI_Description);
				});
			}
		}

		public void TestImportRelatedIndicatorOnInvoiceHeader()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							RelatedIndicator = new CodeDescriptionPair() { Code = "E" }
						}
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("E", declarationBO.Invoices.OfType<BaseJobComInvoiceHeader>().FirstOrDefault().JZ_RelatedIndicator);
		}

		public void TestImportShipToPartyAddressIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var shipToParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			shipToParty.OH_IsConsignee = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.AddOrgAddress(writeManager, shipToParty, DocAddressType.ShipToParty);

			var invoice2 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have ShipToParty Address set.", ZGuid.Empty, invoice.JZ_OA_ShipToPartyAddress);
			});
			AssertEquals("Invoice should have ShipToParty Address set.", shipToParty.MainAddress.PK, invoice2.JZ_OA_ShipToPartyAddress);
		}

		public void TestImportShipToPartyAddressIntoInvoiceLine()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceLineData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);

			var invoice1 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have ShipToParty Address set.", ZGuid.Empty, invoice.JZ_OA_ShipToPartyAddress);
			});
			AssertEquals("Invoice should have 1 invoice line", 1, invoice1.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should only have ShipToParty Address set when AddressType is ShipToParty.", ZGuid.Empty, invoice1.JobComInvoiceLines[0].JI_OA_ShipToPartyAddress);

			var shipToParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			shipToParty.OH_IsConsignee = ZBool.True;
			invoiceLineData.AddOrgAddress(writeManager, shipToParty, DocAddressType.ShipToParty);

			var invoice2 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have ShipToParty Address set.", ZGuid.Empty, invoice.JZ_OA_ShipToPartyAddress);
			});
			AssertEquals("Invoice should have 1 invoice line", 1, invoice2.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should have ShipToParty Address set.", shipToParty.MainAddress.PK, invoice2.JobComInvoiceLines[0].JI_OA_ShipToPartyAddress);
		}

		public void TestImportBuyerAgentIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var buyerAgent = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			buyerAgent.OH_IsConsignee = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.AddOrgAddress(writeManager, buyerAgent, Constants.AddressTypes.BuyingAgent);

			var invoice1 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have BuyerAgent set.", ZGuid.Empty, invoice.JZ_OH_BuyerAgent);
			});
			AssertEquals("Invoice should have BuyerAgent set.", buyerAgent.PK, invoice1.JZ_OH_BuyerAgent);
		}

		public void TestImportSellingAgentIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var sellingAgent = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			sellingAgent.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.AddOrgAddress(writeManager, sellingAgent, Constants.AddressTypes.SellingAgent);

			var invoice1 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have SellingAgent set.", ZGuid.Empty, invoice.JZ_OH_SellingAgent);
			});
			AssertEquals("Invoice should have BuyerAgent set.", sellingAgent.PK, invoice1.JZ_OH_SellingAgent);
		}

		public void TestImportSellerAddressIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var seller = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			seller.OH_IsConsignee = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.AddOrgAddress(writeManager, seller, Constants.AddressTypes.Seller);

			var invoice2 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have Seller Address set.", ZGuid.Empty, invoice.JZ_OA_SellerAddress);
			});
			AssertEquals("Invoice should have Seller Address set.", seller.MainAddress.PK, invoice2.JZ_OA_SellerAddress);
		}

		public void TestImportSellerAddressIntoInvoiceLine()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceLineData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);

			var invoice1 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have Seller Address set.", ZGuid.Empty, invoice.JZ_OA_SellerAddress);
			});
			AssertEquals("Invoice should have 1 invoice line", 1, invoice1.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should only have Seller Address set when AddressType is Seller.", ZGuid.Empty, invoice1.JobComInvoiceLines[0].JI_OA_Seller);

			var seller = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			seller.OH_IsConsignor = ZBool.True;
			invoiceLineData.AddOrgAddress(writeManager, seller, Constants.AddressTypes.Seller);

			var invoice2 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have Seller Address set.", ZGuid.Empty, invoice.JZ_OA_SellerAddress);
			});
			AssertEquals("Invoice should have 1 invoice line", 1, invoice2.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should have Seller Address set.", seller.MainAddress.PK, invoice2.JobComInvoiceLines[0].JI_OA_Seller);
		}

		public void TestImportExporterAddressIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var import = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			import.OH_IsConsignee = ZBool.True;
			AssertNotEquals("Import should have Address set.", ZGuid.Empty, import.MainAddress.PK);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.Supplier = invoiceData.AddOrgAddress(writeManager, import, DocAddressType.Manufacturer);

			var invoice1 = ImportExporterAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should not have Exporter Address set when AddressType is not Exporter.", ZGuid.Empty, invoice1.JZ_OA_ExporterAddress);

			var exporter = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			exporter.OH_IsConsignee = ZBool.True;
			AssertNotEquals("Exporter should have Address set.", ZGuid.Empty, exporter.MainAddress.PK);

			invoiceData.AddOrgAddress(writeManager, exporter, DocAddressType.Exporter);

			var invoice2 = ImportExporterAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have Exporter Address set.", exporter.MainAddress.PK, invoice2.JZ_OA_ExporterAddress);
		}

		public void TestImportExporterAddressIntoInvoiceHeaderForSpecificOrganisationType()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory.BOFactory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			OrgHeader unmatchedOrg = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			invoiceData.AddOrgAddress(writeManager, unmatchedOrg, DocAddressType.Exporter);
			var invoice = ImportExporterAddressHelperFunc(invoiceData);
			Factory.SaveForTesting();

			ZQuery loadQuery = new ZQuery(StmNoteSchema.ST_ParentID, invoice.PK);
			loadQuery.AddToFilter(StmNoteSchema.ST_Table, "JobComInvoiceHeader");
			var notes = Factory.Load<StmNote>(loadQuery);
			AssertEquals(1, notes.Length);
			AssertContains("Consignor", notes[0].ST_NoteText);
		}

		public void TestImportExporterAddressIntoInvoiceLine()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var import = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			import.OH_IsConsignee = ZBool.True;
			AssertNotEquals("Supplier should have Address set.", ZGuid.Empty, import.MainAddress.PK);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceLineData.AddOrgAddress(writeManager, import, DocAddressType.Manufacturer);

			var invoice1 = ImportExporterAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice1.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should not have Exporter Address set when AddressType is not Exporter.", ZGuid.Empty, invoice1.JobComInvoiceLines[0].JI_OA_ExporterAddress);

			var exporter = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			exporter.OH_IsConsignee = ZBool.True;
			AssertNotEquals("Exporter should have Address set.", ZGuid.Empty, exporter.MainAddress.PK);

			invoiceLineData.AddOrgAddress(writeManager, exporter, DocAddressType.Exporter);
			var invoice2 = ImportExporterAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice2.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should have Main Address set.", exporter.MainAddress.PK, invoice2.JobComInvoiceLines[0].JI_OA_ExporterAddress);
		}

		public void TestImportExporterAddressIntoInvoiceLineForSpecificOrganisationType()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory.BOFactory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			OrgHeader unmatchedOrg = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			invoiceLineData.AddOrgAddress(writeManager, unmatchedOrg, DocAddressType.Exporter);
			var invoice = ImportExporterAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice.JobComInvoiceLines.Count);
			Factory.SaveForTesting();

			ZQuery loadQuery = new ZQuery(StmNoteSchema.ST_ParentID, invoice.JobComInvoiceLines[0].PK);
			loadQuery.AddToFilter(StmNoteSchema.ST_Table, "JobComInvoiceLine");
			var notes = Factory.Load<StmNote>(loadQuery);
			AssertEquals(1, notes.Length);
			AssertContains("Consignor", notes[0].ST_NoteText);
		}

		public void TestConsigneeAddressIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var consigneeAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consigneeAddress.OH_IsConsignee = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.AddOrgAddress(writeManager, consigneeAddress, Constants.AddressTypes.UltimateConsignee);

			var invoice2 = ImportConsigneeAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have Consignee Address set.", consigneeAddress.MainAddress.PK, invoice2.JZ_OA_ConsigneeAddress);
		}

		public void TestImporConsigneeAddressIntoInvoiceLine()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceLineData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);

			var invoice1 = ImportConsigneeAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice1.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should only have Consignee Address set when AddressType is ConsigneeAddress.", ZGuid.Empty, invoice1.JobComInvoiceLines[0].JI_OA_ConsigneeAddress);

			var consigneeAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consigneeAddress.OH_IsConsignor = ZBool.True;
			invoiceLineData.AddOrgAddress(writeManager, consigneeAddress, Constants.AddressTypes.UltimateConsignee);

			var invoice2 = ImportConsigneeAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice2.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should have Consignee Address set.", consigneeAddress.MainAddress.PK, invoice2.JobComInvoiceLines[0].JI_OA_ConsigneeAddress);
		}

		public void TestImportConsigneeIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var consignee = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.AddOrgAddress(writeManager, consignee, Constants.AddressTypes.IntermediateConsignee);

			var invoice = ImportConsigneeHelperFunc(invoiceData);
			AssertEquals("Invoice should have Consignee set.", consignee.PK, invoice.JZ_OH_Consignee);
		}

		public void TestBuyerAddressOnInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var testOrg = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			testOrg.OH_IsConsignee = true;
			var buyerAddress = testOrg.Addresses.AddNew();
			buyerAddress.OA_Address1 = "TEST BUYER ADDRESS";
			var supplierAddress = testOrg.Addresses.AddNew();
			supplierAddress.OA_Address1 = "TEST SUPPLIER ADDRESS";
			var intermCneAddress = testOrg.Addresses.AddNew();
			intermCneAddress.OA_Address1 = "TEST INTERM ADDRESS";

			var writerManger = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.AddOrgAddress(writerManger, buyerAddress, Constants.AddressTypes.BuyerAddress);
			invoiceData.AddOrgAddress(writerManger, supplierAddress, Constants.AddressTypes.SupplierAddress);
			invoiceData.AddOrgAddress(writerManger, intermCneAddress, Constants.AddressTypes.IntermediateConsignee);

			var invoice1 = ImportOrganizationAddressHelperFunc(invoiceData, (invoice) =>
			{
				AssertEquals("Invoice should not have Buyer Address set", ZGuid.Empty, invoice.JZ_OA_BuyerAddress);
				AssertEquals("Invoice should not have Supplier set", ZGuid.Empty, invoice.JZ_OH_Supplier);
				AssertEquals("Invoice should not have Supplier Address set", ZGuid.Empty, invoice.JZ_OA_SupplierAddress);
				AssertEquals("Invoice should not have Interm Cne Address set", ZGuid.Empty, invoice.JZ_OA_IntermediateConsigneeAddress);
			});
			AssertEquals("Invoice should not have Buyer Address set.", ZGuid.Empty, invoice1.JZ_OA_BuyerAddress);
			AssertEquals("Invoice should not have Supplier set", ZGuid.Empty, invoice1.JZ_OH_Supplier);
			AssertEquals("Invoice should not have Supplier Address set.", ZGuid.Empty, invoice1.JZ_OA_SupplierAddress);
			AssertEquals("Invoice should have Interm Cne Address set.", intermCneAddress.PK, invoice1.JZ_OA_IntermediateConsigneeAddress);
		}

		public void TestImportBondedWarehouseDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
						{
							new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()
								{
									new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
									{
										BondedWarehouseQuantity = 100m,
										BondedWarehouseQuantityUnit = new CodeDescriptionPair() { Code = "UNT" },
										BondedWarehouseRemarks = "Remarks",
										BondedWHSOrderNumber = "Test Order Number",
										BondedWHSOrderLineNumber = 5
									}
								}))
						}
					}
				};

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoiceBO = declarationBO.Invoices[0];
				var invoiceLineBO = invoiceBO.InvoiceLines[0];
				AssertEquals("invoiceLineBO.JI_BondedWhsQuantity", 100m, invoiceLineBO.JI_BondedWhsQuantity);
				AssertEquals("invoiceLineBO.JI_BondedWhsUnitQty", "UNT", invoiceLineBO.JI_BondedWhsUnitQty);
				AssertEquals("invoiceLineBO.JI_BondedWarehouseRemarks", "Remarks", invoiceLineBO.JI_BondedWarehouseRemarks);
				AssertEquals("invoiceLineBO.JI_BondedWHSOrderNumber", "Test Order Number", invoiceLineBO.JI_BondedWHSOrderNumber);
				AssertEquals("invoiceLineBO.JI_BondedWHSOrderLineNumber", (short)5, invoiceLineBO.JI_BondedWHSOrderLineNumber);
			}
		}

		public void TestImportValuationCodeOnInvoiceHeader()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ValuationCode = new CodeDescriptionPair() { Code = "E" }
						}
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("E", declarationBO.Invoices.OfType<BaseJobComInvoiceHeader>().FirstOrDefault().JZ_ValuationCode);
		}

		public void TestImportValuationMarkupOnInvoiceLine()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									ValuationMarkup = 15,
								}
							}))
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(15m, declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault().JI_ValuationMarkup);
		}

		public void TestImportBrandModelOnInvoiceLine()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									BrandName = "BrandName",
									Model = "Model",
									LocalDescription = "LocalDescription",
								}
							}))
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var line = declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault();
			AssertEquals("invoiceLine.JI_BrandName", "BrandName", line.JI_BrandName);
			AssertEquals("invoiceLine.JI_Model", "Model", line.JI_Model);
			AssertEquals("invoiceLine.JI_NDescription", "LocalDescription", line.JI_NDescription);
		}

		public void TestImportAdditionalFieldsOnInvoiceLine()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									TaxType = new CodeDescriptionPair4Char() { Code = "VXXY" },
									CountryOfExport = new Country() { Code = Core.Constants.CountryCodes.NewZealand },
									RelatedIndicator = new CodeDescriptionPair() { Code = "E" },
									ValuationCode = new CodeDescriptionPair() { Code = "VC" },
									StateOfOrigin = new State() { Code = "MN" }
								}
							}))
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("declarationBO.InvoiceLines.Count", 1, declarationBO.InvoiceLines.Count);
			var invoiceLineBO = declarationBO.InvoiceLines[0];
			AssertEquals("JI_ZZF_NKTaxType is trimed to 4 chars because the max length of JI_ZZF_NKTaxType is 4.", "VXXY", invoiceLineBO.JI_ZZF_NKTaxType);
			AssertEquals("JI_RN_NKCountryOfExport", Core.Constants.CountryCodes.NewZealand, invoiceLineBO.JI_RN_NKCountryOfExport);
			AssertEquals("JI_RelatedIndicator", "E", invoiceLineBO.JI_RelatedIndicator);
			AssertEquals("JI_ValuationCode", "VC", invoiceLineBO.JI_ValuationCode);
			AssertEquals("JI_StateOrRegionOfOrigin", "MN", invoiceLineBO.JI_StateOrRegionOfOrigin);
		}

		public void TestImportAdditionalLineTariffDetailsOnInvoiceLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
						{
							new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>
								{
									new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
									{
										AdditionalLineTariffDetailCollection = new List<UniversalCustoms.AdditionalLineTariffDetail>()
										{
											new UniversalCustoms.AdditionalLineTariffDetail() { Type = new CodeDescriptionPair5Char() { Code = "11A" }, Tariff = "11ATRF", Value = 1.1m },
											new UniversalCustoms.AdditionalLineTariffDetail() { Type = new CodeDescriptionPair5Char() { Code = "22A" }, Tariff = "22ATRF", Value = 2.2m },
										}
									}
								}))
						}
					}
				};

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				AssertContainsExactElementsInAnyOrder(
					new string[] {
					"11A 11ATRF 1.1",
					"22A 22ATRF 2.2",
					}
					, (declarationBO.InvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault() as IAdditionalLineTariffDetailParent).CusLineTariffDetails.OfType<CusLineTariffDetail>().Select(x => $"{x.BZ_Type} {x.BZ_Tariff} {x.BZ_Value}")
					);
			}
		}

		public void TestImportSoldToPartyAddressIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var import = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			import.OH_IsConsignee = ZBool.True;
			AssertNotEquals("Import should have Address set.", ZGuid.Empty, import.MainAddress.PK);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.Supplier = invoiceData.AddOrgAddress(writeManager, import, DocAddressType.Manufacturer);

			var invoice1 = ImportSoldToPartyAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should not have SoldToParty Address set when AddressType is not SoldToParty.", ZGuid.Empty, invoice1.JZ_OA_SoldToPartyAddress);

			var soldtoparty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			soldtoparty.OH_IsConsignee = ZBool.True;
			AssertNotEquals("SoldToParty should have Address set.", ZGuid.Empty, soldtoparty.MainAddress.PK);

			invoiceData.AddOrgAddress(writeManager, soldtoparty, Constants.AddressTypes.SoldToParty);

			var invoice2 = ImportSoldToPartyAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have SoldToParty Address set.", soldtoparty.MainAddress.PK, invoice2.JZ_OA_SoldToPartyAddress);
		}

		public void TestImportSoldToPartyAddressIntoInvoiceLine()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var import = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			import.OH_IsConsignee = ZBool.True;
			AssertNotEquals("Supplier should have Address set.", ZGuid.Empty, import.MainAddress.PK);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceLineData.AddOrgAddress(writeManager, import, DocAddressType.Manufacturer);

			var invoice1 = ImportSoldToPartyAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice1.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should not have SoldToParty Address set when AddressType is not SoldToParty.", ZGuid.Empty, invoice1.JobComInvoiceLines[0].JI_OA_SoldToPartyAddress);

			var soldtoparty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			soldtoparty.OH_IsConsignee = ZBool.True;
			AssertNotEquals("SoldToParty should have Address set.", ZGuid.Empty, soldtoparty.MainAddress.PK);

			invoiceLineData.AddOrgAddress(writeManager, soldtoparty, Constants.AddressTypes.SoldToParty);
			var invoice2 = ImportSoldToPartyAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice2.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should have Main Address set.", soldtoparty.MainAddress.PK, invoice2.JobComInvoiceLines[0].JI_OA_SoldToPartyAddress);
		}

		public void TestImportManufacturerAddressIntoInvoiceHeader()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;
			AssertNotEquals("Supplier should have Address set.", ZGuid.Empty, supplier.MainAddress.PK);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceData.Supplier = invoiceData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);

			var invoice1 = ImportManufacturerAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should only have Manufacturer Address set when AddressType is Manufacturer.", ZGuid.Empty, invoice1.JZ_OA_ManufacturerAddress);

			var manufacturer = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			manufacturer.OH_IsConsignor = ZBool.True;
			AssertNotEquals("Manufacturer should have Address set.", ZGuid.Empty, manufacturer.MainAddress.PK);

			invoiceData.AddOrgAddress(writeManager, manufacturer, DocAddressType.Manufacturer);

			var invoice2 = ImportManufacturerAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have Manufacturer Address set.", manufacturer.MainAddress.PK, invoice2.JZ_OA_ManufacturerAddress);
		}

		public void TestImportManufacturerAddressIntoInvoiceLine()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;
			AssertNotEquals("Supplier should have Address set.", ZGuid.Empty, supplier.MainAddress.PK);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceLineData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);

			var invoice1 = ImportManufacturerAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice1.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should only have Manufacturer Address set when AddressType is Manufacturer.", ZGuid.Empty, invoice1.JobComInvoiceLines[0].JI_OA_ManufacturerAddress);

			var manufacturer = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			manufacturer.OH_IsConsignor = ZBool.True;
			AssertNotEquals("Manufacturer should have Address set.", ZGuid.Empty, manufacturer.MainAddress.PK);

			invoiceLineData.AddOrgAddress(writeManager, manufacturer, DocAddressType.Manufacturer);

			var invoice2 = ImportManufacturerAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice2.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should have Manufacturer Address set.", manufacturer.MainAddress.PK, invoice2.JobComInvoiceLines[0].JI_OA_ManufacturerAddress);
		}

		public void TestImportConsigneeAddressIntoInvoiceLine()
		{
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LineNo = 1,
				CustomsQuantity = 43008m,
				InvoiceQuantity = 2688m,
				LinePrice = 14160.38m,
				NetWeightUnit = new UnitOfWeight() { Code = "T" }
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceNumber = "INV1",
			}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineData })));

			var supplier = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			supplier.OH_IsConsignor = ZBool.True;
			AssertNotEquals("Supplier should have Address set.", ZGuid.Empty, supplier.MainAddress.PK);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			invoiceLineData.AddOrgAddress(writeManager, supplier, DocAddressType.SupplierDocumentaryAddress);

			var invoice1 = ImportConsigneeAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice1.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should only have Consignee Address set when AddressType is Consignee.", ZGuid.Empty, invoice1.JobComInvoiceLines[0].JI_OA_ConsigneeAddress);

			var consignee = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignee.OH_IsConsignor = ZBool.True;
			AssertNotEquals("Consignee should have Address set.", ZGuid.Empty, consignee.MainAddress.PK);

			invoiceLineData.AddOrgAddress(writeManager, consignee, DocAddressType.ConsigneeAddress);

			var invoice2 = ImportConsigneeAddressHelperFunc(invoiceData);
			AssertEquals("Invoice should have 1 invoice line", 1, invoice2.JobComInvoiceLines.Count);
			AssertEquals("Invoice line should have Consignee Address set.", consignee.MainAddress.PK, invoice2.JobComInvoiceLines[0].JI_OA_ConsigneeAddress);
		}

		public void TestImportParentLineNoOnInvoiceLine()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 3, Description = "3", ParentLineNo = 0 };
			var invoiceLineDataObject2 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 2, Description = "2", ParentLineNo = 3 };
			var invoiceLineDataObject3 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, Description = "1" };
			var invoiceLineDataObject4 = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 4, Description = "4", ParentLineNo = 1 };

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3, invoiceLineDataObject4
							})))
					})
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);

			#region Check Contents of declaration Business object

			CombineAssertions(delegate
			{
				var topGroupInvoice = declarationBO.TopGroupInvoice;
				AssertEquals("topGroupInvoice.JobComInvoiceHeaders.Count", 1, topGroupInvoice.JobComInvoiceHeaders.Count);
				var invoice = topGroupInvoice.JobComInvoiceHeaders[0];
				AssertEquals("invoice.JobComInvoiceLines.Count", 4, invoice.JobComInvoiceLines.Count);
				var invoiceLine1 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "1" && x.JI_LineNo == 1);
				AssertNotNull("invoiceLine1", invoiceLine1);
				AssertEquals("invoiceLine1.JI_ParentID", ZGuid.Empty, invoiceLine1.JI_ParentID);
				AssertEquals("invoiceLine1.JI_ParentTableCode", "", invoiceLine1.JI_ParentTableCode);
				var invoiceLine3 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "3" && x.JI_LineNo == 3);
				AssertNotNull("invoiceLine3", invoiceLine3);
				AssertEquals("invoiceLine3.JI_ParentID", ZGuid.Empty, invoiceLine3.JI_ParentID);
				AssertEquals("invoiceLine3.JI_ParentTableCode", "", invoiceLine3.JI_ParentTableCode);
				var invoiceLine2 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "2" && x.JI_LineNo == 2);
				AssertNotNull("invoiceLine2", invoiceLine2);
				AssertEquals("invoiceLine2.JI_ParentID", invoiceLine3.PK, invoiceLine2.JI_ParentID);
				AssertEquals("invoiceLine2.JI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, invoiceLine2.JI_ParentTableCode);
				var invoiceLine4 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "4" && x.JI_LineNo == 4);
				AssertNotNull("invoiceLine4", invoiceLine4);
				AssertEquals("invoiceLine4.JI_ParentID", invoiceLine1.PK, invoiceLine4.JI_ParentID);
				AssertEquals("invoiceLine4.JI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, invoiceLine4.JI_ParentTableCode);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion

			invoiceLineDataObject2.ParentLineNo = 2;
			logger.ClearLogs();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);
			#region Check Contents of declaration Business object

			CombineAssertions(delegate
			{
				var topGroupInvoice = declarationBO.TopGroupInvoice;
				AssertEquals("topGroupInvoice.JobComInvoiceHeaders.Count", 1, topGroupInvoice.JobComInvoiceHeaders.Count);
				var invoice = topGroupInvoice.JobComInvoiceHeaders[0];
				AssertEquals("invoice.JobComInvoiceLines.Count", 4, invoice.JobComInvoiceLines.Count);
				var invoiceLine1 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "1" && x.JI_LineNo == 1);
				AssertNotNull("invoiceLine1", invoiceLine1);
				AssertEquals("invoiceLine1.JI_ParentID", ZGuid.Empty, invoiceLine1.JI_ParentID);
				AssertEquals("invoiceLine1.JI_ParentTableCode", "", invoiceLine1.JI_ParentTableCode);
				var invoiceLine3 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "3" && x.JI_LineNo == 3);
				AssertNotNull("invoiceLine3", invoiceLine3);
				AssertEquals("invoiceLine3.JI_ParentID", ZGuid.Empty, invoiceLine3.JI_ParentID);
				AssertEquals("invoiceLine3.JI_ParentTableCode", "", invoiceLine3.JI_ParentTableCode);
				var invoiceLine2 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "2" && x.JI_LineNo == 2);
				AssertNotNull("invoiceLine2", invoiceLine2);
				AssertEquals("invoiceLine2.JI_ParentID", ZGuid.Empty, invoiceLine2.JI_ParentID);
				AssertEquals("invoiceLine2.JI_ParentTableCode", "", invoiceLine2.JI_ParentTableCode);
				var invoiceLine4 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "4" && x.JI_LineNo == 4);
				AssertNotNull("invoiceLine4", invoiceLine4);
				AssertEquals("invoiceLine4.JI_ParentID", invoiceLine1.PK, invoiceLine4.JI_ParentID);
				AssertEquals("invoiceLine4.JI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, invoiceLine4.JI_ParentTableCode);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Error - Commercial Invoice Line cannot not have the same LineNo and ParentLineNo ('2').
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion

			invoiceLineDataObject2.ParentLineNo = 5;
			logger.ClearLogs();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);
			#region Check Contents of declaration Business object

			CombineAssertions(delegate
			{
				var topGroupInvoice = declarationBO.TopGroupInvoice;
				AssertEquals("topGroupInvoice.JobComInvoiceHeaders.Count", 1, topGroupInvoice.JobComInvoiceHeaders.Count);
				var invoice = topGroupInvoice.JobComInvoiceHeaders[0];
				AssertEquals("invoice.JobComInvoiceLines.Count", 4, invoice.JobComInvoiceLines.Count);
				var invoiceLine1 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "1" && x.JI_LineNo == 1);
				AssertNotNull("invoiceLine1", invoiceLine1);
				AssertEquals("invoiceLine1.JI_ParentID", ZGuid.Empty, invoiceLine1.JI_ParentID);
				AssertEquals("invoiceLine1.JI_ParentTableCode", "", invoiceLine1.JI_ParentTableCode);
				var invoiceLine3 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "3" && x.JI_LineNo == 3);
				AssertNotNull("invoiceLine3", invoiceLine3);
				AssertEquals("invoiceLine3.JI_ParentID", ZGuid.Empty, invoiceLine3.JI_ParentID);
				AssertEquals("invoiceLine3.JI_ParentTableCode", "", invoiceLine3.JI_ParentTableCode);
				var invoiceLine2 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "2" && x.JI_LineNo == 2);
				AssertNotNull("invoiceLine2", invoiceLine2);
				AssertEquals("invoiceLine2.JI_ParentID", ZGuid.Empty, invoiceLine2.JI_ParentID);
				AssertEquals("invoiceLine2.JI_ParentTableCode", "", invoiceLine2.JI_ParentTableCode);
				var invoiceLine4 = invoice.JobComInvoiceLines.OfType<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "4" && x.JI_LineNo == 4);
				AssertNotNull("invoiceLine4", invoiceLine4);
				AssertEquals("invoiceLine4.JI_ParentID", invoiceLine1.PK, invoiceLine4.JI_ParentID);
				AssertEquals("invoiceLine4.JI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, invoiceLine4.JI_ParentTableCode);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Warning - Cannot not find ParentLineNo ('5') for Commercial Invoice Line with LineNo ('2').
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion

			invoiceLineDataObject2.ParentLineNo = null;
			invoiceLineDataObject3.LineNo = null;
			invoiceLineDataObject4.ParentLineNo = null;
			logger.ClearLogs();
			reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			declarationBO = reader.ReadIntoBusinessObject();
			AssertNotNull(declarationBO);
			#region Check Contents of declaration Business object

			CombineAssertions(delegate
			{
				var topGroupInvoice = declarationBO.TopGroupInvoice;
				AssertEquals("topGroupInvoice.JobComInvoiceHeaders.Count", 1, topGroupInvoice.JobComInvoiceHeaders.Count);
				var invoice = topGroupInvoice.JobComInvoiceHeaders[0];
				var invoiceLines = invoice.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().ToArray();
				AssertEquals("invoice.JobComInvoiceLines.Count", 4, invoiceLines.Length);
				var invoiceLine1 = invoiceLines.First(x => x.JI_LineNo == 3);
				AssertEquals("invoiceLine1.JI_LineNo", (short)3, invoiceLine1.JI_LineNo);
				AssertEquals("invoiceLine1.JI_Description", "3", invoiceLine1.JI_Description);
				AssertEquals("invoiceLine1.JI_ParentID", ZGuid.Empty, invoiceLine1.JI_ParentID);
				AssertEquals("invoiceLine1.JI_ParentTableCode", "", invoiceLine1.JI_ParentTableCode);
				var invoiceLine2 = invoiceLines.First(x => x.JI_LineNo == 2);
				AssertEquals("invoiceLine2.JI_LineNo", (short)2, invoiceLine2.JI_LineNo);
				AssertEquals("invoiceLine2.JI_Description", "2", invoiceLine2.JI_Description);
				AssertEquals("invoiceLine2.JI_ParentID", ZGuid.Empty, invoiceLine2.JI_ParentID);
				AssertEquals("invoiceLine2.JI_ParentTableCode", "", invoiceLine2.JI_ParentTableCode);
				var invoiceLine3 = invoiceLines.First(x => x.JI_LineNo == 1);
				AssertEquals("invoiceLine3.JI_LineNo, recalculated by InvoiceLineLineNumberGenerator after all invoicelines imported though its original set value is 0", (short)1, invoiceLine3.JI_LineNo);
				AssertEquals("invoiceLine3.JI_Description", "1", invoiceLine3.JI_Description);
				AssertEquals("invoiceLine3.JI_ParentID", ZGuid.Empty, invoiceLine3.JI_ParentID);
				AssertEquals("invoiceLine3.JI_ParentTableCode", "", invoiceLine3.JI_ParentTableCode);
				var invoiceLine4 = invoiceLines.First(x => x.JI_LineNo == 4);
				AssertEquals("invoiceLine4.JI_LineNo", (short)4, invoiceLine4.JI_LineNo);
				AssertEquals("invoiceLine4.JI_Description", "4", invoiceLine4.JI_Description);
				AssertEquals("invoiceLine4.JI_ParentID", ZGuid.Empty, invoiceLine4.JI_ParentID);
				AssertEquals("invoiceLine4.JI_ParentTableCode", "", invoiceLine4.JI_ParentTableCode);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestCannotLinkToEntryLineWhenThereIsMultipleEntryLineMatches()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var entryHeaderObject1 = new UniversalCustoms.EntryHeader()
				{
					Type = new EntryType() { Code = "EXP" },
					EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber(new EntryType() { Code = "EXP" }, "11122233300012", ZBool.False) }),
					EntryLineCollection = new List<UniversalCustoms.EntryLine>() { new UniversalCustoms.EntryLine() { LineNumber = 1 } }
				};

				var entryHeaderObject2 = new UniversalCustoms.EntryHeader()
				{
					Type = new EntryType() { Code = "IMP" },
					EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber(new EntryType() { Code = "IMP" }, "11122233300012", ZBool.False) }),
					EntryLineCollection = new List<UniversalCustoms.EntryLine>() { new UniversalCustoms.EntryLine() { LineNumber = 1 } }
				};

				var declarationObject = SetupDeclaration("BOB1", "MB23422", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>() { entryHeaderObject1, entryHeaderObject2 });
				declarationObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
						{
							new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								InvoiceNumber = "INVOICE 1",
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
								{
									new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryLineNumber = 1, EntryNumber = "11122233300012" },
								})))
						})
				};

				var message = GetQueuedUniversalShipmentMessage(declarationObject);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Multiple Entry Lines were matched to Commercial Invoice Line '1' (InvoiceNumber='INVOICE 1', EntryNumber='11122233300012', EntryLineNumber='1').
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
No matching CusEntryHeader found, creating new CusEntryHeader.
Populating CusEntryHeader...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryLine found, creating new CusEntryLine.
Populating CusEntryLine...
No matching CusEntryHeader found, creating new CusEntryHeader.
Populating CusEntryHeader...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryLine found, creating new CusEntryLine.
Populating CusEntryLine...
Error - Multiple Entry Lines were matched to Commercial Invoice Line '1' (InvoiceNumber='INVOICE 1', EntryNumber='11122233300012', EntryLineNumber='1').
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
				".Trim(), logNoteText);

				AssertNull(Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB23422")));
			}
		}

		public void TestCannotLinkToEntryLineWhenWhenDataIsNotMatched()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declarationObject = SetupDeclaration("BOB1", "MB23422", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>()
				{
					new UniversalCustoms.EntryHeader()
					{
						Type = new EntryType() { Code = "EXP" },
						EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber(new EntryType() { Code = "EXP" }, "11122233300012", ZBool.False) }),
						EntryLineCollection = new List<UniversalCustoms.EntryLine>() { new UniversalCustoms.EntryLine() { LineNumber = 1 } }
					}
				});
				declarationObject.CommercialInfo = new UniversalCustoms.CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
						{
							new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								InvoiceNumber = "INVOICE 1",
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
								{
									new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryLineNumber = 1, EntryNumber = "MISSINGENTRYNUMBER" },
								})))
						})
				};

				var message = GetQueuedUniversalShipmentMessage(declarationObject);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - No Entry Line was matched to Commercial Invoice Line '1' (InvoiceNumber='INVOICE 1', EntryNumber='MISSINGENTRYNUMBER', EntryLineNumber='1').
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
No matching CusEntryHeader found, creating new CusEntryHeader.
Populating CusEntryHeader...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryLine found, creating new CusEntryLine.
Populating CusEntryLine...
Error - No Entry Line was matched to Commercial Invoice Line '1' (InvoiceNumber='INVOICE 1', EntryNumber='MISSINGENTRYNUMBER', EntryLineNumber='1').
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
".Trim(), logNoteText);
			}
		}

		public void TestCannotLinkToEntryLineWhenEntryLineNumberIsNotSpecified()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declarationObject = SetupDeclaration("BOB1", "MB23422", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>()
				{
					new UniversalCustoms.EntryHeader()
					{
						Type = new EntryType() { Code = "EXP" },
						EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber(new EntryType() { Code = "EXP" }, "11122233300012", ZBool.False) }),
						EntryLineCollection = new List<UniversalCustoms.EntryLine>() { new UniversalCustoms.EntryLine() { LineNumber = 1 } }
					}
				});
				declarationObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
						{
							new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								InvoiceNumber = "INVOICE 1",
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
								{
									new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryNumber = "11122233300012" },
								})))
						})
				};

				var message = GetQueuedUniversalShipmentMessage(declarationObject);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Commercial Invoice Line must specified EntryLineNumber when EntryNumber is specified.
Added Declaration (Master Bill='MB23422') from UniversalShipment.
Successfully saved Declaration B00001000 with 1 x CusEntryNumber, 1 x CusEntryLine, 1 x CusEntryHeader.".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
No matching CusEntryHeader found, creating new CusEntryHeader.
Populating CusEntryHeader...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryLine found, creating new CusEntryLine.
Populating CusEntryLine...
Error - Commercial Invoice Line must specified EntryLineNumber when EntryNumber is specified.
Added Declaration (Master Bill='MB23422') from UniversalShipment.
Successfully saved Declaration B00001000 with 1 x CusEntryNumber, 1 x CusEntryLine, 1 x CusEntryHeader.
				".Trim(), logNoteText);
			}
		}

		public void TestCannotLinkToEntryLineWhenEntryNumberIsNotSpecified()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declarationObject = SetupDeclaration("BOB1", "MB23422", new WayBillType() { Code = WayBillTypeList.Codes.Master });
				declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>()
				{
					new UniversalCustoms.EntryHeader()
					{
						Type = new EntryType() { Code = "EXP" },
						EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber(new EntryType() { Code = "EXP" }, "11122233300012", ZBool.False) }),
						EntryLineCollection = new List<UniversalCustoms.EntryLine>() { new UniversalCustoms.EntryLine() { LineNumber = 1 } }
					}
				});
				declarationObject.CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
						{
							new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								InvoiceNumber = "INVOICE 1",
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
								{
									new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryLineNumber = 1 },
								})))
						})
				};

				var message = GetQueuedUniversalShipmentMessage(declarationObject);

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Commercial Invoice Line must specified EntryNumber when EntryLineNumber is specified.
Added Declaration (Master Bill='MB23422') from UniversalShipment.
Successfully saved Declaration B00001000 with 1 x CusEntryNumber, 1 x CusEntryLine, 1 x CusEntryHeader.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
No matching CusEntryHeader found, creating new CusEntryHeader.
Populating CusEntryHeader...
No matching CusEntryNumber found, creating new CusEntryNumber.
Populating CusEntryNumber...
No matching CusEntryLine found, creating new CusEntryLine.
Populating CusEntryLine...
Error - Commercial Invoice Line must specified EntryNumber when EntryLineNumber is specified.
Added Declaration (Master Bill='MB23422') from UniversalShipment.
Successfully saved Declaration B00001000 with 1 x CusEntryNumber, 1 x CusEntryLine, 1 x CusEntryHeader.
				".Trim(), logNoteText);
			}
		}

		public void TestLinkToEntryLine()
		{
			using (TemporarilySetCountryAndInterfaced(Core.Constants.CountryCodes.China))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INVOICE 1";
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				invoiceLine1.JI_LineNo = 1;
				var invoiceLine2 = invoice1.InvoiceLines.AddNew();
				invoiceLine2.JI_LineNo = 2;
				var invoiceLine3 = invoice1.InvoiceLines.AddNew();
				invoiceLine3.JI_LineNo = 3;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "INVOICE 2";
				var invoiceLine4 = invoice2.InvoiceLines.AddNew();
				invoiceLine4.JI_LineNo = 1;

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = "EXP";
				entryHeader.EntryNumber = "11122233300012";

				var declarationObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

				var entryHeaderObject = new UniversalCustoms.EntryHeader()
				{
					Type = new EntryType() { Code = entryHeader.CH_MessageType },
					Reference = declaration.JE_DeclarationReference + "/11122233300012"
				};
				entryHeaderObject.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber(new EntryType() { Code = entryHeader.CH_MessageType, Description = "B12 DESC" }, "11122233300012", ZBool.False) });

				var entryLineObject1 = new UniversalCustoms.EntryLine() { LineNumber = 1 };
				var entryLineObject2 = new UniversalCustoms.EntryLine() { LineNumber = 2 };
				entryHeaderObject.EntryLineCollection = new List<UniversalCustoms.EntryLine>() { entryLineObject1, entryLineObject2 };

				var commercialInfo = new UniversalCustoms.CommercialInfo();
				commercialInfo.CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
				{
					new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INVOICE 1",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
						{
							new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryLineNumber = 1, EntryNumber = "11122233300012" },
							new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 2, EntryLineNumber = 1, EntryNumber = "11122233300012" },
							new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 3, EntryLineNumber = 1, EntryNumber = "11122233300012" }
						}))),
					new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INVOICE 2",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
						{
							new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1, EntryLineNumber = 2, EntryNumber = "11122233300012" }
						})))
				});

				declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>() { entryHeaderObject });
				declarationObject.CommercialInfo = commercialInfo;

				var declarationBO = new JobDeclarationDataObjectReader(declarationObject, logger, Factory).ReadIntoBusinessObject();
				AssertNotNull(declarationBO);
				declarationBO.CustomsEntryHeaders.Load();
				var cusEntryHeader = declarationBO.CustomsEntryHeaders[0];
				cusEntryHeader.AllEntryLines.Load();
				AssertEquals(cusEntryHeader.AllEntryLines[0].PK, declarationBO.InvoiceLines[0].JI_CL);
				AssertEquals(cusEntryHeader.AllEntryLines[0].PK, declarationBO.InvoiceLines[1].JI_CL);
				AssertEquals(cusEntryHeader.AllEntryLines[0].PK, declarationBO.InvoiceLines[2].JI_CL);
				AssertEquals(cusEntryHeader.AllEntryLines[1].PK, declarationBO.InvoiceLines[3].JI_CL);
			}
		}

		public void TestInvoiceLineNumber_NoDuplicateLineNo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_DeclarationReference = "B00001";
				Factory.SaveForTesting();

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var declarationObject1 = CreateDeclarationShipment(1, "11122233300001");
				var declarationObject2 = CreateDeclarationShipment(3, "11122233300002");

				var declarationBO = new JobDeclarationDataObjectReader(declarationObject1, logger, Factory).ReadIntoBusinessObject();
				var declarationBO2 = new JobDeclarationDataObjectReader(declarationObject2, logger, Factory).ReadIntoBusinessObject();

				CombineAssertions(() =>
				{
					AssertEquals("Update B00001", declarationBO.PK, declarationBO2.PK);
					declarationBO.CustomsEntryHeaders.Load();
					AssertEquals("entry1 Count", 2, declarationBO.CustomsEntryHeaders[0].AllEntryLines.Count);
					AssertEquals("entry2 Count", 2, declarationBO.CustomsEntryHeaders[1].AllEntryLines.Count);

					var invoices = declaration.Invoices;
					AssertEquals("Invoice Count", 1, invoices.Count);
					AssertEquals("Invoice Number", "INVOICE 1", invoices[0].JZ_InvoiceNumber);
					AssertEquals("InvoiceLine Count", 4, invoices[0].InvoiceLines.Count);
					AssertContainsExactElementsInExactOrder("LineNo", new ZShort[] { 1, 2, 3, 4 }, invoices[0].InvoiceLines.Cast<BaseJobComInvoiceLine>().Select(x => x.JI_LineNo).ToArray());
				});

				Shipment CreateDeclarationShipment(int invoiceLineNo, string entryNumber)
				{
					var declarationObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
					{
						DataContext = dataContext
					};

					var entryHeaderObject = new UniversalCustoms.EntryHeader()
					{
						Type = new EntryType() { Code = "IMP" },
						Reference = entryNumber,
						EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[]
						{
							SetupEntryNumber(new EntryType() { Code = "MRN", Description = "B12 DESC" }, entryNumber, ZBool.False)
						})
					};
					var entryLineObject1 = new UniversalCustoms.EntryLine() { LineNumber = 1 };
					var entryLineObject2 = new UniversalCustoms.EntryLine() { LineNumber = 2 };
					entryHeaderObject.EntryLineCollection = new List<UniversalCustoms.EntryLine>() { entryLineObject1, entryLineObject2 };

					var commercialInfo1 = new UniversalCustoms.CommercialInfo();
					commercialInfo1.CommercialInvoiceCollection =
						new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
						{
							new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								InvoiceNumber = "INVOICE 1",
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
									new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
									{
										new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
										{
											LineNo = invoiceLineNo, EntryLineNumber = 1, EntryNumber = entryNumber
										},
										new UniversalCustoms.CommercialInvoiceLine(
											DefaultDataObjectWriterStrategy.TestInstance)
										{
											LineNo = invoiceLineNo + 1, EntryLineNumber = 2, EntryNumber = entryNumber
										}
									})))
						});

					declarationObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>() { entryHeaderObject });
					declarationObject.CommercialInfo = commercialInfo1;
					return declarationObject;
				}
			}
		}

		public void TestStandaloneInvoice_DataForSingleInvoiceOnly()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceData1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData1.Buyer = invoiceData1.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData1.Supplier = invoiceData1.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData1.InvoiceNumber = "INV1";
			SetupNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(invoiceData1, "DESC 1 - INV1", "DESC 2 - INV1");
			var invoiceData2 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData2.Buyer = invoiceData2.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData2.Supplier = invoiceData2.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData2.InvoiceNumber = "INV2";
			SetupNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(invoiceData2, "DESC 1 - INV2", "DESC 2 - INV2");

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[]
					{
						SetupCommercialCharge(ZBool.False, 150m, Commission, LocalCurrency, DistributeByValue, null, null, FullApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Prepaid)
					}),
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData1, invoiceData2 })
				},
			};
			shipmentData.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[]
				{
					new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Road, LegOrder = 1, VoyageFlightNo = "R1", PortOfLoading = new UNLOCO() { Code = "AUSYD" }, PortOfDischarge = new UNLOCO() { Code = "AUMEL" } },
					new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Road, LegOrder = 2, VoyageFlightNo = "R2", PortOfLoading = new UNLOCO() { Code = "AUMEL" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" } }
				}));

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData1, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			var charge = invoice.Charges.GetChargeByChargeName(Commission.Description.Value);
			AssertNull("Group Header charges should not be imported as there multiple invoices", charge);
			invoice.Transports.Load();
			AssertEquals("invoice.Transports.Count", 0, invoice.Transports.Count);
			AssertNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(invoice.Notes);

			shipmentData.CommercialInfo.CommercialInvoiceCollection.Remove(invoiceData2);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			charge = invoice.Charges.GetChargeByChargeName(Commission.Code.Value);
			AssertNotNull("Group Header charge should be imported", charge);
			invoice.Transports.Load();
			AssertEquals("invoice.Transports.Count", 2, invoice.Transports.Count);
			AssertNotNull("R1 Transport", invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_VoyageFlight == "R1"));
			AssertNotNull("R2 Transport", invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_VoyageFlight == "R2"));
			AssertNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(invoice.Notes);
		}

		public void TestStandaloneInvoice_TransportLegCollectionWithPartialAttribute()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceData1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData1.Buyer = invoiceData1.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData1.Supplier = invoiceData1.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData1.InvoiceNumber = "INV1";

			SetupNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(invoiceData1, "DESC 1 - INV1", "DESC 2 - INV1");

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[]
					{
						SetupCommercialCharge(ZBool.False, 150m, Commission, LocalCurrency, DistributeByValue, null, null, FullApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Prepaid)
					}),
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData1 })
				}
			};
			shipmentData.SetTransportLegCollection(() => new DataObjectList<TransportLeg>(new[]
				{
					new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Road, LegOrder = 1, VoyageFlightNo = "R1", PortOfLoading = new UNLOCO() { Code = "AUSYD" }, PortOfDischarge = new UNLOCO() { Code = "AUMEL" } },
					new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance) { TransportMode = TransportMode.Road, LegOrder = 2, VoyageFlightNo = "R2", PortOfLoading = new UNLOCO() { Code = "AUMEL" }, PortOfDischarge = new UNLOCO() { Code = "USLAX" } }
				}));

			shipmentData.TransportLegCollection.Content = CollectionContent.Partial;

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var transport1 = invoice.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUPER";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_VoyageFlight = "R0";

			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData1, Logger, CurrentCompanyHelper, topGroupInvoice);
			var charge = invoice.Charges.GetChargeByChargeName(Commission.Description.Value);

			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			charge = invoice.Charges.GetChargeByChargeName(Commission.Code.Value);
			AssertNotNull("Group Header charge should be imported", charge);
			invoice.Transports.Load();
			AssertEquals("invoice.Transports.Count", 3, invoice.Transports.Count);
			AssertNotNull("R0 Transport", invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_VoyageFlight == "R0"));
			AssertNotNull("R1 Transport", invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_VoyageFlight == "R1"));
			AssertNotNull("R2 Transport", invoice.Transports.OfType<Transport>().FirstOrDefault(x => x.JW_VoyageFlight == "R2"));
			AssertNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(invoice.Notes);
		}

		public void TestStandaloneInvoice_MergeNoteCollection()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.Buyer = invoiceData.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData.Supplier = invoiceData.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData.InvoiceNumber = "INV1";
			invoiceData.NoteCollection = new List<Note>(new[]
				{
					SetupPublicAAACustomNote("Description Invoice", "inv note1"),
					SetupPublicAAACustomNote("Description Shared", "inv note2")
				});

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				},
			};
			shipmentData.SetNoteCollection(() => new DataObjectList<Note>(new[]
				{
					SetupPublicAAACustomNote("Description Decalaration", "dec note1"),
					SetupPublicAAACustomNote("Description Shared", "dec note2")
				}));

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			var invoiceNotes = invoice.Notes;
			AssertNotNull(invoiceNotes);
			AssertEquals("invoiceNotes.GetAllNotes().Count", 3, invoiceNotes.GetAllNotes().Count);
			AssertContainsPublicAAACustomNote(invoiceNotes, "Description Invoice", "inv note1");
			AssertContainsPublicAAACustomNote(invoiceNotes, "Description Shared", "inv note2");
			AssertContainsPublicAAACustomNote(invoiceNotes, "Description Decalaration", "dec note1");
		}

		public void TestStandaloneInvoiceContainers()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var container1Data = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CN1" };
			var container2Data = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CN2" };
			var container3Data = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CN3" };
			var container4Data = new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CN4" };
			var invoiceData1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData1.Buyer = invoiceData1.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData1.Supplier = invoiceData1.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData1.InvoiceNumber = "INV1";
			invoiceData1.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] {
				new  UniversalCustoms.CommercialInvoiceLine() { ContainerNumber = "CN3", HarmonisedCode = "1010101010" },
				new  UniversalCustoms.CommercialInvoiceLine() { ContainerNumber = "CN5", HarmonisedCode = "2020202020" },
			}));
			var invoiceData2 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData2.Buyer = invoiceData2.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData2.Supplier = invoiceData2.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData2.InvoiceNumber = "INV2";

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData1, invoiceData2 })
				}
			};
			shipmentData.SetContainerCollection(() => new DataObjectList<Container>(new[] { container1Data, container2Data, container3Data, container4Data }));

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData1, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			var containers = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN).ToArray();
			AssertEquals("Should only contain container number from invoicelines as there are multiple invoices", 2, containers.Length);
			AssertNotNull("CN3 container", containers.FirstOrDefault(x => x.J2_ReferenceNumber == "CN3"));
			AssertNotNull("CN5 container", containers.FirstOrDefault(x => x.J2_ReferenceNumber == "CN5"));

			invoice.InvoiceHeaderRefs.DeleteAll();
			shipmentData.CommercialInfo.CommercialInvoiceCollection.Remove(invoiceData2);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			containers = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.CN).ToArray();
			AssertEquals("Should contains all containers and those from invoicelines as there is only one invoice", 5, containers.Length);
			AssertNotNull("CN1", containers.FirstOrDefault(x => x.J2_ReferenceNumber == "CN1"));
			AssertNotNull("CN2", containers.FirstOrDefault(x => x.J2_ReferenceNumber == "CN2"));
			AssertNotNull("CN3", containers.FirstOrDefault(x => x.J2_ReferenceNumber == "CN3"));
			AssertNotNull("CN4", containers.FirstOrDefault(x => x.J2_ReferenceNumber == "CN4"));
			AssertNotNull("CN5", containers.FirstOrDefault(x => x.J2_ReferenceNumber == "CN5"));
		}

		public void TestStandaloneInvoiceBills()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var bill1Data = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB1", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House } };
			var bill2Data = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB2", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House } };
			var bill3Data = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB3", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House } };
			var bill4Data = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "HB4", BillType = new WayBillType() { Code = WayBillTypeList.Codes.House } };
			var invoiceData1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData1.Buyer = invoiceData1.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData1.Supplier = invoiceData1.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData1.InvoiceNumber = "INV1";
			invoiceData1.BillNumber = "HB1";
			invoiceData1.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var invoiceData2 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData2.Buyer = invoiceData2.AddOrgAddress(writeManager, consignee, DocAddressType.BuyerDocumentaryAddress);
			invoiceData2.Supplier = invoiceData2.AddOrgAddress(writeManager, consignor, DocAddressType.SupplierDocumentaryAddress);
			invoiceData2.InvoiceNumber = "INV2";
			invoiceData2.BillNumber = "HB2";
			invoiceData2.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData1, invoiceData2 })
				}
			};
			shipmentData.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { bill1Data, bill2Data, bill3Data, bill4Data }));

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData1, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			var bills = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB).ToArray();
			AssertEquals("Should only contain invoiceData1.BillNumber as there are multiple invoices", 1, bills.Length);
			AssertEquals("Should match invoiceData1.BillNumber", "HB1", bills[0].J2_ReferenceNumber);

			invoice.InvoiceHeaderRefs.DeleteAll();
			shipmentData.CommercialInfo.CommercialInvoiceCollection.Remove(invoiceData2);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			bills = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.HB).ToArray();
			AssertEquals("Should contains all AdditionalBills and invoiceData1.BillNumber as there is only one invoice", 4, bills.Length);
			AssertNotNull("HB1", bills.FirstOrDefault(x => x.J2_ReferenceNumber == "HB1"));
			AssertNotNull("HB2", bills.FirstOrDefault(x => x.J2_ReferenceNumber == "HB2"));
			AssertNotNull("HB3", bills.FirstOrDefault(x => x.J2_ReferenceNumber == "HB3"));
			AssertNotNull("HB4", bills.FirstOrDefault(x => x.J2_ReferenceNumber == "HB4"));
		}

		public void TestStandaloneInvoiceAdditionalReferences()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var additionalReferenceData = new AdditionalReference()
			{
				Type = new EntryType() { Code = InvoiceHeaderRefsTypeList.Codes.RP },
				ReferenceNumber = "JOHN SMITH"
			};

			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.InvoiceNumber = "INV1";

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(invoiceData.Yield())
				}
			};
			shipmentData.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>(additionalReferenceData.Yield()));

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			var additionalReferences = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.RP).ToArray();
			AssertEquals(1, additionalReferences.Length);
			AssertNotNull(additionalReferences.FirstOrDefault(x => x.J2_ReferenceNumber == "JOHN SMITH"));

			additionalReferenceData.ReferenceNumber = "JOE BLOW";
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));
			additionalReferences = invoice.InvoiceHeaderRefs.OfType<JobComInvoiceHeaderRefs>().Where(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.RP).ToArray();
			AssertEquals(1, additionalReferences.Length);
			AssertNotNull(additionalReferences.FirstOrDefault(x => x.J2_ReferenceNumber == "JOE BLOW"));
		}

		public void TestInvoiceIsLinkedToRelatedBill()
		{
			var invoiceDataObject = SetupCommercialInvoiceHeaderData();
			invoiceDataObject.BillNumber = "B1";
			invoiceDataObject.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "B1";
			declaration.JE_HouseBill = "B1";
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
			var invoice = reader.ReadIntoBusinessObject();
			AssertEquals("Should be linked to HouseBill", declaration.PrimaryHouseBill.PK, invoice.JZ_CU_RelatedHouseBill);

			invoiceDataObject.BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			invoice = reader.ReadIntoBusinessObject();
			AssertEquals("Should be linked to MasterBill", declaration.PrimaryMasterBill.PK, invoice.JZ_CU_RelatedHouseBill);
		}

		public void TestInvoiceHeaderDataAreSetInASpecificOrderForStandalone()
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
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV123", null, null, 1500.32m, new Currency() { Code = "SRD" }, new ZDateTime(2012, 3, 25, 13, 3, 2), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.ExWorks },
				10.4m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1202.53m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, new CodeDescriptionPair() { Code = Enterprise.Customs.Business.ChargeExchangeRateTypeList.Codes.FixedRate }, 1.3m, 1.25m, 100m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), null);
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection = null;

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = new Branch() { Code = "B@#" },
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new UniversalCustoms.CommercialInfo() { Name = "GROUP", CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject }) }
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			invoice.JZ_GB = ZGuid.Empty;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice, null);

			var expectedOrders = new List<InfoValueChangeData>(new[]
			{
				new InfoValueChangeData(invoice.JZ_StandAloneInvoiceDirectionInfo, ZString.Empty, (ZString)JobMessageTypeList.Codes.Import),
				new InfoValueChangeData(invoice.JZ_InvoiceNumberInfo, ZString.Empty, (ZString)"INV123"),
				new InfoValueChangeData(invoice.JZ_InvoiceDateInfo, ZDateTime.Empty, new ZDateTime(2012, 3, 25, 13, 3, 2)),
				new InfoValueChangeData(invoice.JZ_GBInfo, ZGuid.Empty, branch2.PK),
				new InfoValueChangeData(invoice.JZ_InvoiceAmountInfo, ZDecimal.Zero, (ZDecimal)1500.32m),
				new InfoValueChangeData(invoice.JZ_RX_NKInvoice_CurrencyInfo, ZString.Empty, (ZString)"SRD"),
				new InfoValueChangeData(invoice.JZ_InvoiceCurrExRateInfo, ZDecimal.Zero, (ZDecimal)1.3m),
				new InfoValueChangeData(invoice.JZ_IncoTermInfo, ZString.Empty, (ZString)Core.Constants.IncoTerms.ExWorks),
				new InfoValueChangeData(invoice.JZ_WeightInfo, ZDecimal.Zero, (ZDecimal)1202.53m),
				new InfoValueChangeData(invoice.JZ_WeightUQInfo, ZString.Empty, (ZString)Core.Constants.Weight.Pounds),
				new InfoValueChangeData(invoice.JZ_NetWeightInfo, ZDecimal.Zero, (ZDecimal)11.11m),
				new InfoValueChangeData(invoice.JZ_NetWeightUQInfo, ZString.Empty, (ZString)Core.Constants.Weight.Kilograms)
			});

			AssertDataWasSetInSpecificOrder(expectedOrders, () => reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(declarationDataObject)));
		}

		public void TestInvoiceHeaderDataAreSetInASpecificOrder()
		{
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			consignor.OH_IsConsignor = ZBool.True;
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			consignee.OH_IsConsignee = ZBool.True;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV123", null, null, 1500.32m, new Currency() { Code = "SRD" }, new ZDateTime(2012, 3, 25, 13, 3, 2), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.ExWorks },
				10.4m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1202.53m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, new CodeDescriptionPair() { Code = Enterprise.Customs.Business.ChargeExchangeRateTypeList.Codes.FixedRate }, 1.3m, 1.25m, 100m, "PO234", 1500m, 1.31m, new ZDateTime(2012, 3, 30), null);
			invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
			invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
			invoiceDataObject.OrganizationAddressCollection = null;

			var declaration = Factory.New<BaseJobDeclaration>();
			var topGroupInvoice = declaration.TopGroupInvoice;
			var invoice = topGroupInvoice.JobComInvoiceHeaders.AddNew();
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var readerMock = new Mock<CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice, null, null, null);
			readerMock.CallBase = true;
			readerMock
				.Protected()
				.Setup<BaseJobComInvoiceHeader>("GetNewInvoice")
				.Returns(invoice);

			var expectedOrders = new List<InfoValueChangeData>(new[]
			{
				new InfoValueChangeData(invoice.JZ_InvoiceNumberInfo, ZString.Empty, (ZString)"INV123"),
				new InfoValueChangeData(invoice.JZ_OH_SupplierInfo, ZGuid.Empty, consignor.PK),
				new InfoValueChangeData(invoice.JZ_OH_BuyerInfo, ZGuid.Empty, consignee.PK),
				new InfoValueChangeData(invoice.JZ_InvoiceAmountInfo, ZDecimal.Zero, (ZDecimal)1500.32m),
				new InfoValueChangeData(invoice.JZ_RX_NKInvoice_CurrencyInfo, ZString.Empty, (ZString)"SRD"),
				new InfoValueChangeData(invoice.JZ_InvoiceCurrExRateInfo, ZDecimal.Zero, (ZDecimal)1.3m),
				new InfoValueChangeData(invoice.JZ_IncoTermInfo, ZString.Empty, (ZString)Core.Constants.IncoTerms.ExWorks),
				new InfoValueChangeData(invoice.JZ_WeightInfo, ZDecimal.Zero, (ZDecimal)1202.53m),
				new InfoValueChangeData(invoice.JZ_WeightUQInfo, ZString.Empty, (ZString)Core.Constants.Weight.Pounds),
				new InfoValueChangeData(invoice.JZ_InvoiceCurrLandedCostExRateInfo, ZDecimal.Zero, (ZDecimal)1.25m),
				new InfoValueChangeData(invoice.JZ_NoOfPacksInfo, ZDecimal.Zero, (ZDecimal)100m),
				new InfoValueChangeData(invoice.JZ_NetWeightInfo, ZDecimal.Zero, (ZDecimal)11.11m),
				new InfoValueChangeData(invoice.JZ_NetWeightUQInfo, ZString.Empty, (ZString)Core.Constants.Weight.Kilograms),
			});

			AssertDataWasSetInSpecificOrder(expectedOrders, () => readerMock.Object.ReadIntoBusinessObject(false));
			readerMock.VerifyAll();
		}

		public void TestSetInvoiceLinePrice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var topGroupInvoice = declaration.TopGroupInvoice;
			var invoice = topGroupInvoice.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0m;

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var invoiceLineDataObject = SetupCommercialInvoiceLine(1,
					"Have Line Price",
					150m,
					new CodeDescriptionPair() { Code = "Q1" },
					1545.43m,
					"PART1231",
					1.13m,
					new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet },
					1503.24m,
					new UnitOfWeight() { Code = Core.Constants.Weight.Pounds });

				invoiceLineDataObject.UnitPrice = null;
				invoiceLineDataObject.InvoiceQuantity = null;

				var invoiceDataObject = SetupCommercialInvoiceHeaderData(commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 1 }, invoiceLineDataObject }));

				var mock = new Mock<CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice, null, null, null);
				mock.CallBase = true;
				mock
					.Protected()
					.Setup<BaseJobComInvoiceHeader>("GetNewInvoice")
					.Returns(invoice);
				mock
					.Protected()
					.Setup<BaseJobComInvoiceLine>("GetInvoiceLine", ItExpr.IsAny<UniversalCustoms.CommercialInvoiceLine>(), ItExpr.IsAny<BaseJobComInvoiceHeader>())
					.Returns(invoiceLine);

				mock.Object.ReadIntoBusinessObject(false);
				AssertEquals("Should populate the value from the LinePrice of invoiceLineDataObject.", 1545.43m, invoiceLine.JI_LinePrice);

				invoiceLine.JI_LinePrice = 657.84m;
				invoiceLineDataObject.LinePrice = null;
				invoiceLineDataObject.UnitPrice = 0m;
				invoiceLineDataObject.InvoiceQuantity = 0m;

				mock.Object.ReadIntoBusinessObject(false);
				AssertEquals("Should not populate any value from the invoiceLineDataObject as the LinePrice is null.", 657.84m, invoiceLine.JI_LinePrice);

				invoiceLine.JI_LinePrice = 32.972m;
				invoiceLineDataObject.UnitPrice = 150m;
				invoiceLineDataObject.InvoiceQuantity = 20m;

				mock.Object.ReadIntoBusinessObject(false);
				AssertEquals("Should populate the value from the UnitPrice * InvoiceQuantity.", 3000m, invoiceLine.JI_LinePrice);
				mock.VerifyAll();
			}
		}

		public void TestInvoiceLineDataAreSetInASpecificOrder()
		{
			var invoiceLineDataObject = SetupCommercialInvoiceLine(1, "HELLO WORLD", 150m, new CodeDescriptionPair() { Code = "Q1" }, 1545.43m, "PART1231",
				1.13m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet }, 1503.24m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds });
			invoiceLineDataObject.PreviousEntryNumber = "123456789";
			invoiceLineDataObject.PreviousEntryLineNumber = 2;
			invoiceLineDataObject.CustomsQuantity = 142.53m;
			invoiceLineDataObject.CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "Q2" };
			invoiceLineDataObject.CountryOfOrigin = new Country() { Code = "OC" };
			invoiceLineDataObject.Commodity = new Commodity() { Code = "CM1" };
			invoiceLineDataObject.NetWeight = 1423.52m;
			invoiceLineDataObject.NetWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.MetricCarat };
			invoiceLineDataObject.HarmonisedCode = "1010101010";
			invoiceLineDataObject.OrderNumber = "ORD2342";
			invoiceLineDataObject.ParentLineNo = 2;

			var invoiceDataObject = SetupCommercialInvoiceHeaderData(commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { LineNo = 2 }, invoiceLineDataObject }));

			var declaration = Factory.New<BaseJobDeclaration>();
			var topGroupInvoice = declaration.TopGroupInvoice;
			var invoice = topGroupInvoice.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 2;
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var readerMock = new Mock<CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice, null, null, null);
			readerMock.CallBase = true;
			readerMock
				.Protected()
				.Setup<BaseJobComInvoiceHeader>("GetNewInvoice")
				.Returns(invoice);
			readerMock
				.Protected()
				.Setup<BaseJobComInvoiceLine>("GetInvoiceLine", ItExpr.IsAny<UniversalCustoms.CommercialInvoiceLine>(), ItExpr.IsAny<BaseJobComInvoiceHeader>())
				.Returns(invoiceLine2);
			readerMock
				.Protected()
				.Setup<BaseJobComInvoiceLine>("GetNewInvoiceLine", ItExpr.IsAny<BaseJobComInvoiceHeader>())
				.Returns(invoiceLine);

			var expectedOrders = new List<InfoValueChangeData>(new[]
			{
				new InfoValueChangeData(invoiceLine.JI_ParentIDInfo, ZGuid.Empty, invoiceLine2.PK),
				new InfoValueChangeData(invoiceLine.JI_PartNoInfo, ZString.Empty, (ZString)"PART1231"),
				new InfoValueChangeData(invoiceLine.JI_TariffInfo, ZString.Empty, (ZString)"1010101010"),
				new InfoValueChangeData(invoiceLine.JI_InvoiceQuantityInfo, ZDecimal.Zero, (ZDecimal)150m),
				new InfoValueChangeData(invoiceLine.JI_InvoiceUQInfo, ZString.Empty, (ZString)"Q1"),
				new InfoValueChangeData(invoiceLine.JI_LinePriceInfo, ZDecimal.Zero, (ZDecimal)1545.43m),
				new InfoValueChangeData(invoiceLine.JI_DescriptionInfo, ZString.Empty, (ZString)"HELLO WORLD"),
				new InfoValueChangeData(invoiceLine.JI_CountryOfOriginInfo, ZString.Empty, (ZString)"OC"),
				new InfoValueChangeData(invoiceLine.JI_RH_NKCommodity_CodeInfo, ZString.Empty, (ZString)"CM1"),
				new InfoValueChangeData(invoiceLine.JI_WeightInfo, ZDecimal.Zero, (ZDecimal)1503.24m),
				new InfoValueChangeData(invoiceLine.JI_WeightUQInfo, ZString.Empty, (ZString)Core.Constants.Weight.Pounds),
				new InfoValueChangeData(invoiceLine.JI_NetWeightInfo, ZDecimal.Zero, (ZDecimal)1423.52m),
				new InfoValueChangeData(invoiceLine.JI_NetWeightUQInfo, ZString.Empty, (ZString)Core.Constants.Weight.MetricCarat),
				new InfoValueChangeData(invoiceLine.JI_VolumeInfo, ZDecimal.Zero, (ZDecimal)1.13m),
				new InfoValueChangeData(invoiceLine.JI_VolumeUQInfo, ZString.Empty, (ZString)Core.Constants.Volume.CubicFeet),
				new InfoValueChangeData(invoiceLine.JI_OrderNumberInfo, ZString.Empty, (ZString)"ORD2342"),
				new InfoValueChangeData(invoiceLine.JI_CustomsQuantityInfo, ZDecimal.Zero, (ZDecimal)142.53m),
				new InfoValueChangeData(invoiceLine.JI_CustomsUnitQtyInfo, ZString.Empty, (ZString)"Q2"),
				new InfoValueChangeData(invoiceLine.JI_PreviousEntryNumberInfo, ZString.Empty, (ZString)"123456789"),
				new InfoValueChangeData(invoiceLine.JI_PreviousEntryLineNumberInfo, ZShort.Zero, (ZShort)2)
			});

			AssertDataWasSetInSpecificOrder(expectedOrders, () => readerMock.Object.ReadIntoBusinessObject(false));
			readerMock.VerifyAll();
		}

		public void TestInvoiceAndInvoiceLineAddInfoMapping()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });

				var expectedInvoice1AddInfo = string.Format("{0}=LT1*{1}=1", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_ValueForDiscount.Substring(3));
				var invoice1AddInfo = "HEL@#@12=123*" + expectedInvoice1AddInfo;
				var expectedInvoice1Line1AddInfo = string.Format("{0}=LT1*{1}=1000.0", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_CustomsValue.Substring(3));
				var invoice1Line1AddInfo = "HEL@#@12=123*" + expectedInvoice1Line1AddInfo;
				var expectedInvoice1Line2AddInfo = string.Format("{0}=LT2*{1}=2000.0", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_CustomsValue.Substring(3));
				var invoice1Line2AddInfo = "HEL@#@12=123*" + expectedInvoice1Line2AddInfo;

				var expectedInvoice2AddInfo = string.Format("{0}=LT2*{1}=2", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_ValueForDiscount.Substring(3));
				var invoice2AddInfo = "HEL@#@12=123*" + expectedInvoice2AddInfo;
				var expectedInvoice2Line1AddInfo = string.Format("{0}=2T1*{1}=1000.0", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_CustomsValue.Substring(3));
				var invoice2Line1AddInfo = "HEL@#@12=123*" + expectedInvoice2Line1AddInfo;
				var expectedInvoice2Line2AddInfo = string.Format("{0}=2T2*{1}=2000.0", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_CustomsValue.Substring(3));
				var invoice2Line2AddInfo = "HEL@#@12=123*" + expectedInvoice2Line2AddInfo;

				var expectedInvoice3AddInfo = string.Format("{0}=LT3*{1}=3", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_ValueForDiscount.Substring(3));
				var invoice3AddInfo = "HEL@#@12=123*" + expectedInvoice3AddInfo;
				var expectedInvoice3Line1AddInfo = string.Format("{0}=3T1*{1}=1000.0", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_CustomsValue.Substring(3));
				var invoice3Line1AddInfo = "HEL@#@12=123*" + expectedInvoice3Line1AddInfo;
				var expectedInvoice3Line2AddInfo = string.Format("{0}=3T2*{1}=2000.0", USAddInfoSchema.Constants.US_LicenseType.Substring(3), USAddInfoSchema.Constants.US_CustomsValue.Substring(3));
				var invoice3Line2AddInfo = "HEL@#@12=123*" + expectedInvoice3Line2AddInfo;

				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group",
					invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						SetupCommercialInvoiceHeaderData(
							addInfoCollection: AddInfoCollectionCreator.CreateCollection(invoice1AddInfo),
							commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								SetupCommercialInvoiceLine(1, AddInfoCollectionCreator.CreateCollection(invoice1Line1AddInfo)),
								SetupCommercialInvoiceLine2(2, AddInfoCollectionCreator.CreateCollection(invoice1Line2AddInfo))
							})
						),
						SetupCommercialInvoiceHeaderData2(
							addInfoCollection: AddInfoCollectionCreator.CreateCollection(invoice2AddInfo),
							commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								SetupCommercialInvoiceLine2(1, AddInfoCollectionCreator.CreateCollection(invoice2Line1AddInfo)),
								SetupCommercialInvoiceLine(2, AddInfoCollectionCreator.CreateCollection(invoice2Line2AddInfo))
							})
						)
					}),
					groupCollection: new List<UniversalCustoms.CommercialInfo>(new[]
					{
						SetupCommercialInfo("Group 1",
							invoiceCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
							{
								SetupCommercialInvoiceHeaderData2(
									addInfoCollection: AddInfoCollectionCreator.CreateCollection(invoice3AddInfo),
									commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
									{
										SetupCommercialInvoiceLine(1, AddInfoCollectionCreator.CreateCollection(invoice3Line1AddInfo)),
										SetupCommercialInvoiceLine(2, AddInfoCollectionCreator.CreateCollection(invoice3Line2AddInfo))
									})
								)
							})
						)
					})
				);

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();

				AssertNotNull(declarationBO);

				#region Check Contents of declaration Business object

				CombineAssertions(delegate
				{
					declarationBO.AllGroupHeaders.Load();
					AssertEquals("declarationBO.AllGroupHeaders.Count", 2, declarationBO.AllGroupHeaders.Count);
					var topGroupInvoice = declarationBO.TopGroupInvoice;
					AssertEquals("topGroupInvoice.JZ_InvoiceNumber", "All Invoices", topGroupInvoice.JZ_InvoiceNumber);
					AssertEquals("topGroupInvoice.JobComInvoiceHeaders.Count", 2, topGroupInvoice.JobComInvoiceHeaders.Count);

					var invoice1 = topGroupInvoice.JobComInvoiceHeaders[0];
					AssertEquals("invoice1.JZ_AddInfo", expectedInvoice1AddInfo, invoice1.JZ_AddInfo);
					AssertEquals("invoice1.JobComInvoiceLines.Count", 2, invoice1.JobComInvoiceLines.Count);
					var invoice1Line1 = invoice1.JobComInvoiceLines[0];
					AssertEquals("invoice1Line1.JZ_AddInfo", expectedInvoice1Line1AddInfo, invoice1Line1.JI_AddInfo);
					var invoice1Line2 = invoice1.JobComInvoiceLines[1];
					AssertEquals("invoice1Line2.JZ_AddInfo", expectedInvoice1Line2AddInfo, invoice1Line2.JI_AddInfo);

					var invoice2 = topGroupInvoice.JobComInvoiceHeaders[1];
					AssertEquals("invoice2.JZ_AddInfo", expectedInvoice2AddInfo, invoice2.JZ_AddInfo);
					AssertEquals("invoice2.JobComInvoiceLines.Count", 2, invoice2.JobComInvoiceLines.Count);
					var invoice2Line1 = invoice2.JobComInvoiceLines[0];
					AssertEquals("invoice2Line1.JZ_AddInfo", expectedInvoice2Line1AddInfo, invoice2Line1.JI_AddInfo);
					var invoice2Line2 = invoice2.JobComInvoiceLines[1];
					AssertEquals("invoice2Line2.JZ_AddInfo", expectedInvoice2Line2AddInfo, invoice2Line2.JI_AddInfo);

					AssertEquals("topGroupInvoice.JobComInvoiceGroupHeaders.Count", 1, topGroupInvoice.JobComInvoiceGroupHeaders.Count);
					var groupHeader1 = topGroupInvoice.JobComInvoiceGroupHeaders[0];
					AssertEquals("groupHeader1.JZ_InvoiceNumber", "Group 1", groupHeader1.JZ_InvoiceNumber);
					AssertEquals("groupHeader1.JobComInvoiceHeaders.Count", 1, groupHeader1.JobComInvoiceHeaders.Count);

					var groupHeader1Invoice1 = groupHeader1.JobComInvoiceHeaders[0];
					AssertEquals("groupHeader1Invoice1.JZ_AddInfo", expectedInvoice3AddInfo, groupHeader1Invoice1.JZ_AddInfo);
					AssertEquals("groupHeader1Invoice1.JobComInvoiceLines.Count", 2, groupHeader1Invoice1.JobComInvoiceLines.Count);
					var groupHeader1Invoice1Line1 = groupHeader1Invoice1.JobComInvoiceLines[0];
					AssertEquals("groupHeader1Invoice1Line1.JZ_AddInfo", expectedInvoice3Line1AddInfo, groupHeader1Invoice1Line1.JI_AddInfo);
					var groupHeader1Invoice1Line2 = groupHeader1Invoice1.JobComInvoiceLines[1];
					AssertEquals("groupHeader1Invoice1Line2.JZ_AddInfo", expectedInvoice3Line2AddInfo, groupHeader1Invoice1Line2.JI_AddInfo);
				});

				#endregion
			}
		}

		public void TestWithCommercialData()
		{
			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(true);

			var declarationDataObject = SetupDeclaration(null, "MYHOUSE", new WayBillType() { Code = "HWB", Description = "House Waybill" });
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group",
				new List<UniversalCustoms.CommercialCharge>(new[]
				{
					SetupCommercialCharge(ZBool.False, ZDecimal.Zero, Discount, null, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.False, ZBool.False, ZBool.True, ZBool.False, 10m, Prepaid),
					SetupCommercialCharge(ZBool.False, 150m, Commission, LocalCurrency, DistributeByValue, null, null, FullApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Prepaid),
					SetupCommercialCharge(ZBool.False, 1000m, OverseasFreight, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Collect),
					SetupCommercialCharge(ZBool.False, 500m, OverseasInsurance, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Collect)
				}),
				new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
				{
					SetupCommercialInvoiceHeaderData(
						commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
						{
							SetupCommercialCharge(ZBool.False, 150m, ExWorks, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, Collect),
							SetupCommercialCharge(ZBool.False, 600m, LandingCharges, LocalCurrency, DistributeByVolume, null, null, PartialApportionment, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.True, ZDecimal.Zero, Collect)
						}),
						commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
						{
							SetupCommercialInvoiceLine(
								1,
								commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
								{
									SetupCommercialCharge(ZBool.False, 1200m, Commission, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.True, ZBool.False, 20m, null),
									SetupCommercialCharge(ZBool.False, 750m, DeductionCharge, ForeignCurrency, DistributeByValue, FixedRate, 1.5m, PartialApportionment, ZBool.False, ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZDecimal.Zero, null)
								})
							),
							SetupCommercialInvoiceLine2(
								2,
								commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
								{
									SetupCommercialCharge(ZBool.False, 300m, PackingCost, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, null)
								})
							)
						})
					),
					SetupCommercialInvoiceHeaderData2(
						commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
						{
							SetupCommercialCharge(ZBool.False, 500.00, ForeignInlandFreight, LocalCurrency, DistributeByWeight, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, Collect)
						}),
						commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
						{
							SetupCommercialInvoiceLine2(
								1,
								commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
								{
									SetupCommercialCharge(ZBool.False, 100m, LandingCharges, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.False, ZBool.False, ZBool.False, ZBool.True, ZDecimal.Zero, null)
								})
							),
							SetupCommercialInvoiceLine(
								2,
								commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
								{
									SetupCommercialCharge(ZBool.False, 375m, AdditionCharge, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, null)
								})
							)
						})
					)
				}),
				new List<UniversalCustoms.CommercialInfo>(new[]
				{
					SetupCommercialInfo("Group 1",
						new List<UniversalCustoms.CommercialCharge>(new[]
						{
							SetupCommercialCharge(ZBool.False, 600m, OverseasFreight, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Collect),
							SetupCommercialCharge(ZBool.False, 300m, OverseasInsurance, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Collect),
							SetupCommercialCharge(ZBool.False, 100m, AdditionCharge, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, Collect) // this apportion charge will be ignored
						}),
						new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
						{
							SetupCommercialInvoiceHeaderData2(
								commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
								{
									SetupCommercialCharge(ZBool.False, 1500.00, PackingCost, LocalCurrency, DistributeByWeight, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, Collect),
									SetupCommercialCharge(ZBool.False, 100m, AdditionCharge, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, Prepaid) // this apportion charge will be ignored
								}),
								commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
								{
									SetupCommercialInvoiceLine(
										1,
										commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
										{
											SetupCommercialCharge(ZBool.False, 750m, ExWorks, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, null)
										})
									),
									SetupCommercialInvoiceLine(
										2,
										commercialInvoiceChargeCollection: new List<UniversalCustoms.CommercialCharge>(new[]
										{
											SetupCommercialCharge(ZBool.False, 500m, ExWorks, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.False, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, null),
											SetupCommercialCharge(ZBool.False, 100m, AdditionCharge, LocalCurrency, DistributeByValue, null, null, PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZBool.True, ZDecimal.Zero, null) // this apportion charge will be ignored
										})
									)
								})
							)
						})
					)
				})
			);

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var localCurrencyCode = declarationBO.LocalCurrencyCode;

			AssertNotNull(declarationBO);

			#region Check Contents of declaration Business object

			CombineAssertions(delegate
			{
				AssertContents(declarationBO, null, null, "MYHOUSE");
				declarationBO.AllGroupHeaders.Load();
				AssertEquals("declarationBO.AllGroupHeaders.Count", 2, declarationBO.AllGroupHeaders.Count);
				var topGroupInvoice = declarationBO.TopGroupInvoice;
				AssertEquals("topGroupInvoice.JZ_InvoiceNumber", "All Invoices", topGroupInvoice.JZ_InvoiceNumber);
				AssertEquals("topGroupInvoice.Charges.Count", 4, topGroupInvoice.Charges.Count);
				AssertContents(topGroupInvoice.Charges[0], ZBool.False, ZDecimal.Zero, CustomsChargeTypeList.Codes.Discount, ZString.Empty, ChargeDistributeByList.Codes.Value, ZString.Empty, ZDecimal.Zero, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.False, ZBool.True, ZBool.False, 10m, Core.Constants.PaymentType.Prepaid);
				AssertContents(topGroupInvoice.Charges[1], ZBool.False, 150m, CustomsChargeTypeList.Codes.Commission, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.FullApportionment, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Prepaid);
				AssertContents(topGroupInvoice.Charges[2], ZBool.False, 1000m, CustomsChargeTypeList.Codes.OverseasFreight, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Collect);
				AssertContents(topGroupInvoice.Charges[3], ZBool.False, 500m, CustomsChargeTypeList.Codes.OverseasInsurance, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Collect);
				AssertEquals("topGroupInvoice.JobComInvoiceHeaders.Count", 2, topGroupInvoice.JobComInvoiceHeaders.Count);

				var invoice1 = topGroupInvoice.JobComInvoiceHeaders[0];
				AssertContents(invoice1);
				var foundNotes = invoice1.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be one UNMATCHED note", 1, foundNotes.Length);
				ZString serialisedNoteText = @"Organisation Type: Consignor
Owner Code: 
EDI Code: TOAPOLOGISE
Organisation Name: Too Late To Apologise
Address Line 1: Unit 24, Level 10
Address Line 2: 455 There St
City: Big City
Post Code: 56845
State or Province: Small State
Country: US
Doc Address Type: 
 
Organisation Type: Consignee
Owner Code: 
EDI Code: INTHEMSYD
Organisation Name: In The Moment
Address Line 1: Unit 12, Level 3
Address Line 2: 233 Here St
City: ThereVille
Post Code: 1233
State or Province: OfBliss
Country: AU
Doc Address Type: 
 ";
				AssertMultilineASCIIEquals("Note text should describe TOPGROUPINV1 organizations", serialisedNoteText, foundNotes[0].ST_NoteText);
				AssertEquals("invoice1.Charges.Count", 2, invoice1.Charges.Count);
				AssertContents(invoice1.Charges[0], ZBool.False, 150m, CustomsChargeTypeList.Codes.ExWorks, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Collect);
				AssertContents(invoice1.Charges[1], ZBool.False, 600m, CustomsChargeTypeList.Codes.LandingCharges, localCurrencyCode, ChargeDistributeByList.Codes.Volume, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.False, ZBool.False, ZBool.True, ZDecimal.Zero, Core.Constants.PaymentType.Collect);

				AssertEquals("invoice1.JobComInvoiceLines.Count", 2, invoice1.JobComInvoiceLines.Count);
				var invoice1Line1 = invoice1.JobComInvoiceLines[0];
				AssertContents(invoice1Line1, 1);
				AssertEquals("invoice1Line1.Charges.Count", 2, invoice1Line1.Charges.Count);
				AssertContents(invoice1Line1.Charges[0], ZBool.False, 1200m, CustomsChargeTypeList.Codes.Commission, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, 20m, ZString.Empty);
				AssertContents(invoice1Line1.Charges[1], ZBool.False, 750m, CustomsChargeTypeList.Codes.DeductionCharge, ForeignCurrencyBO.RX_Code, ChargeDistributeByList.Codes.Value, Enterprise.Customs.Common.ChargeExchangeRateTypeList.Codes.FixedRate, 1.5m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.False, ZBool.True, ZBool.False, ZDecimal.Zero, ZString.Empty);

				var invoice1Line2 = invoice1.JobComInvoiceLines[1];
				AssertContents2(invoice1Line2, 2);
				AssertEquals("invoice1Line2.Charges.Count", 1, invoice1Line2.Charges.Count);
				AssertContents(invoice1Line2.Charges[0], ZBool.False, 300m, CustomsChargeTypeList.Codes.PackingCost, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, ZString.Empty);

				var invoice2 = topGroupInvoice.JobComInvoiceHeaders[1];
				AssertContents2(invoice2);
				foundNotes = invoice2.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be one UNMATCHED note", 1, foundNotes.Length);
				serialisedNoteText = @"Organisation Type: Consignor
Owner Code: 
EDI Code: INTHEMSYD
Organisation Name: In The Moment
Address Line 1: Unit 12, Level 3
Address Line 2: 233 Here St
City: ThereVille
Post Code: 1233
State or Province: OfBliss
Country: AU
Doc Address Type: 
 
Organisation Type: Consignee
Owner Code: 
EDI Code: TOAPOLOGISE
Organisation Name: Too Late To Apologise
Address Line 1: Unit 24, Level 10
Address Line 2: 455 There St
City: Big City
Post Code: 56845
State or Province: Small State
Country: US
Doc Address Type: 
 ";
				AssertMultilineASCIIEquals("Note text should describe TOPGROUPINV2 organizations", serialisedNoteText, foundNotes[0].ST_NoteText);
				AssertEquals("invoice2.Charges.Count", 1, invoice2.Charges.Count);
				AssertContents(invoice2.Charges[0], ZBool.False, 500.00, CustomsChargeTypeList.Codes.ForeignInlandFreight, localCurrencyCode, ChargeDistributeByList.Codes.Weight, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Collect);
				AssertEquals("invoice2.JobComInvoiceLines.Count", 2, invoice2.JobComInvoiceLines.Count);

				var invoice2Line1 = invoice2.JobComInvoiceLines[0];
				AssertContents2(invoice2Line1, 1);
				AssertEquals("invoice2Line1.Charges.Count", 1, invoice2Line1.Charges.Count);
				AssertContents(invoice2Line1.Charges[0], ZBool.False, 100m, CustomsChargeTypeList.Codes.LandingCharges, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.False, ZBool.False, ZBool.True, ZDecimal.Zero, ZString.Empty);

				var invoice2Line2 = invoice2.JobComInvoiceLines[1];
				AssertContents(invoice2Line2, 2);
				AssertEquals("invoice2Line2.Charges.Count", 1, invoice2Line2.Charges.Count);
				AssertContents(invoice2Line2.Charges[0], ZBool.False, 375m, CustomsChargeTypeList.Codes.AdditionCharge, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, ZString.Empty);

				AssertEquals("topGroupInvoice.JobComInvoiceGroupHeaders.Count", 1, topGroupInvoice.JobComInvoiceGroupHeaders.Count);
				var groupHeader1 = topGroupInvoice.JobComInvoiceGroupHeaders[0];
				AssertEquals("groupHeader1.JZ_InvoiceNumber", "Group 1", groupHeader1.JZ_InvoiceNumber);
				AssertEquals("groupHeader1.Charges.Count should 2 as apportioned charges should not be imported", 2, groupHeader1.Charges.Count);
				AssertContents(groupHeader1.Charges[0], ZBool.False, 600m, CustomsChargeTypeList.Codes.OverseasFreight, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Collect);
				AssertContents(groupHeader1.Charges[1], ZBool.False, 300m, CustomsChargeTypeList.Codes.OverseasInsurance, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.False, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Collect);
				AssertEquals("groupHeader1.JobComInvoiceHeaders.Count", 1, groupHeader1.JobComInvoiceHeaders.Count);

				var groupHeader1Invoice1 = groupHeader1.JobComInvoiceHeaders[0];
				AssertContents2(groupHeader1Invoice1);
				foundNotes = groupHeader1Invoice1.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				AssertEquals("There should be one UNMATCHED note", 1, foundNotes.Length);
				serialisedNoteText = @"Organisation Type: Consignor
Owner Code: 
EDI Code: INTHEMSYD
Organisation Name: In The Moment
Address Line 1: Unit 12, Level 3
Address Line 2: 233 Here St
City: ThereVille
Post Code: 1233
State or Province: OfBliss
Country: AU
Doc Address Type: 
 
Organisation Type: Consignee
Owner Code: 
EDI Code: TOAPOLOGISE
Organisation Name: Too Late To Apologise
Address Line 1: Unit 24, Level 10
Address Line 2: 455 There St
City: Big City
Post Code: 56845
State or Province: Small State
Country: US
Doc Address Type: 
 
";
				AssertMultilineASCIIEquals("Note text should describe TOPGROUPINV2 organizations", serialisedNoteText, foundNotes[0].ST_NoteText);
				AssertEquals("groupHeader1Invoice1.Charges.Count should be 1", 1, groupHeader1Invoice1.Charges.Count);
				AssertContents(groupHeader1Invoice1.Charges[0], ZBool.False, 1500.00, CustomsChargeTypeList.Codes.PackingCost, localCurrencyCode, ChargeDistributeByList.Codes.Weight, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, Core.Constants.PaymentType.Collect);
				AssertNull("groupHeader1Invoice1.GroupCharges should not have AdditionCharge as it should not be imported", groupHeader1Invoice1.GroupCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.AdditionCharge));

				AssertEquals("groupHeader1Invoice1.JobComInvoiceLines.Count", 2, groupHeader1Invoice1.JobComInvoiceLines.Count);

				var groupHeader1Invoice1Line1 = groupHeader1Invoice1.JobComInvoiceLines[0];
				AssertContents(groupHeader1Invoice1Line1, 1);
				AssertEquals("groupHeader1Invoice1Line1.Charges.Count should 1 as apportioned charges should not be imported", 1, groupHeader1Invoice1Line1.Charges.Count);
				AssertContents(groupHeader1Invoice1Line1.Charges[0], ZBool.False, 750m, CustomsChargeTypeList.Codes.ExWorks, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.True, ZBool.False, ZDecimal.Zero, ZString.Empty);

				var groupHeader1Invoice1Line2 = groupHeader1Invoice1.JobComInvoiceLines[1];
				AssertContents(groupHeader1Invoice1Line2, 2);
				AssertEquals("groupHeader1Invoice1Line2.Charges.Count should 1 as apportioned charges should not be imported", 1, groupHeader1Invoice1Line2.Charges.Count);
				AssertContents(groupHeader1Invoice1Line2.Charges[0], ZBool.False, 500m, CustomsChargeTypeList.Codes.ExWorks, localCurrencyCode, ChargeDistributeByList.Codes.Value, ZString.Empty, 0m, ApportionmentTypeList.Codes.PartialApportionment, ZBool.True, ZBool.True, ZBool.False, ZBool.False, ZDecimal.Zero, ZString.Empty);
				AssertNull("groupHeader1Invoice1Line2.ApportionedCharges should not have AdditionCharge as it should not be imported", groupHeader1Invoice1Line2.ApportionedCharges.GetChargeByChargeName(CustomsChargeTypeList.Codes.AdditionCharge));

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching BaseJobDeclaration found, creating new BaseJobDeclaration.
Information - Populating BaseJobDeclaration...
Information - No matching BaseGroupInvoiceCharge found, creating new BaseGroupInvoiceCharge.
Information - Populating BaseGroupInvoiceCharge...
Information - No matching BaseGroupInvoiceCharge found, creating new BaseGroupInvoiceCharge.
Information - Populating BaseGroupInvoiceCharge...
Information - No matching BaseGroupInvoiceCharge found, creating new BaseGroupInvoiceCharge.
Information - Populating BaseGroupInvoiceCharge...
Information - No matching BaseGroupInvoiceCharge found, creating new BaseGroupInvoiceCharge.
Information - Populating BaseGroupInvoiceCharge...
Information - Matching 'Supplier':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - Matching 'Importer':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
Information - No matching BaseInvoiceCharge found, creating new BaseInvoiceCharge.
Information - Populating BaseInvoiceCharge...
Information - No matching BaseInvoiceCharge found, creating new BaseInvoiceCharge.
Information - Populating BaseInvoiceCharge...
Information - Matching 'Supplier':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - Matching 'Importer':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
Information - No matching BaseInvoiceCharge found, creating new BaseInvoiceCharge.
Information - Populating BaseInvoiceCharge...
Information - No matching BaseGroupInvoiceCharge found, creating new BaseGroupInvoiceCharge.
Information - Populating BaseGroupInvoiceCharge...
Information - No matching BaseGroupInvoiceCharge found, creating new BaseGroupInvoiceCharge.
Information - Populating BaseGroupInvoiceCharge...
Information - Matching 'Supplier':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - Matching 'Importer':- No match found - Assigned to UNMATCHED organization (Code: UNMATCHED)
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
Information - No matching BaseInvoiceLineCharge found, creating new BaseInvoiceLineCharge.
Information - Populating BaseInvoiceLineCharge...
Information - No matching BaseInvoiceCharge found, creating new BaseInvoiceCharge.
Information - Populating BaseInvoiceCharge...
Information - Added Declaration from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestNoSupplierAndBuyerAddressMatchingInInvoiceHeaderFillOrganizationsCore_WhenAddressOverrideIsTrue()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

				var invoiceLineAddInfo1 = string.Format("{0}=1.23*{1}=FR", AUAddInfoSchema.ZA_ISS.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3));
				var invoiceLineDataObject1 = SetupCommercialInvoiceLine(
					1,
					AddInfoCollectionCreator.CreateCollection(invoiceLineAddInfo1),
					null
				);
				var invoiceLineAddInfo2 = string.Format("{0}=1.23*{1}=NZ", AUAddInfoSchema.ZA_ISS.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3));
				var invoiceLineDataObject2 = SetupCommercialInvoiceLine2(
					2,
					AddInfoCollectionCreator.CreateCollection(invoiceLineAddInfo2),
					null
				);
				var invoiceLineAddInfo3 = string.Format("{0}=1.23*{1}=AU", AUAddInfoSchema.ZA_ISS.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3));
				var invoiceLineDataObject3 = SetupCommercialInvoiceLine2(
					3,
					AddInfoCollectionCreator.CreateCollection(invoiceLineAddInfo3),
					null
				);

				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var invoiceAddInfo = string.Format("{0}=ZA*{1}=TV", AUAddInfoSchema.ZA_ORG.Name.Substring(3), AUAddInfoSchema.ZA_VALB_Hidden.Name.Substring(3));
				var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV6854", null, null, 8685.54m, null, new ZDateTime(2011, 4, 2), null, 96.87m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicYards }, 86.69m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilotonnes }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.55m, 2.5m, "PA123", 1234.43m, 2.3m, new ZDateTime(2012, 2, 3), new CodeDescriptionPair() { Code = "MS1" },
					AddInfoCollectionCreator.CreateCollection(invoiceAddInfo), null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
				{
					invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3
				}), null, null);
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);

				var faker = new FakeDeclarationCreatorForInvoice(Factory.New<BaseJobComInvoiceHeader>());
				var declaration = (BaseJobDeclaration)faker.HeaderData;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var topGroupInvoice = declaration.TopGroupInvoice;
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				var invoiceBO = reader.ReadIntoBusinessObject();
				AssertNotNull(invoiceBO);

				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_OH_Buyer", consignee.PK, invoiceBO.JZ_OH_Buyer);
					AssertEquals("invoice.JZ_OH_Supplier", consignor.PK, invoiceBO.JZ_OH_Supplier);
				});

				invoiceBO.Delete();
				Factory.SaveForTesting();
				invoiceDataObject.Supplier.AddressOverride = true;
				invoiceDataObject.Buyer.AddressOverride = true;
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				invoiceBO = reader.ReadIntoBusinessObject();
				AssertNotNull(invoiceBO);

				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_OH_Buyer", ZGuid.Empty, invoiceBO.JZ_OH_Buyer);
					AssertEquals("invoice.JZ_OH_Supplier", ZGuid.Empty, invoiceBO.JZ_OH_Supplier);
				});
			}
		}

		public void TestUpdatingExistingMatchingInvoice_ShouldUpdateBothSupplierAndSupplierAddress()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consignorOld = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				Factory.SaveForTesting();

				var faker = new FakeDeclarationCreatorForInvoice(Factory.New<BaseJobComInvoiceHeader>());

				var declaration = (BaseJobDeclaration)faker.HeaderData;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

				var topGroupInvoice = declaration.TopGroupInvoice;
				topGroupInvoice.JZ_OH_Supplier = consignorOld.PK;
				topGroupInvoice.JZ_OA_SupplierAddress = consignorOld.MainAddress.PK;

				var invoice = declaration.Invoices.First();
				invoice.JZ_OH_Supplier = consignorOld.PK;
				invoice.JZ_OA_SupplierAddress = consignorOld.MainAddress.PK;
				invoice.JZ_InvoiceNumber = "WI00791935";
				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var invoiceDataObject = SetupCommercialInvoiceHeaderData(invoice.JZ_InvoiceNumber, null, null, 8685.54m, new Currency { Code = "AUD" }, new ZDateTime(2011, 4, 2), null, 96.87m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicCentimeters }, 86.69m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.55m, 2.5m, "PA123", 1234.43m, 2.3m, new ZDateTime(2012, 2, 3), new CodeDescriptionPair() { Code = "MS1" });
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Supplier.Address1 = null;

				var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				var invoiceBO = reader.ReadIntoBusinessObject(matchExistingInvoice: true);
				AssertNotNull(invoiceBO);

				CombineAssertions(delegate
				{
					AssertEquals("invoice.JZ_OH_Buyer", consignee.PK, invoiceBO.JZ_OH_Buyer);
					AssertEquals("invoice.JZ_OH_Supplier", consignor.PK, invoiceBO.JZ_OH_Supplier);
					Assert("invoice.JZ_OA_SupplierAddress must be empty or belong to Supplier org", invoiceBO.JZ_OA_SupplierAddress.IsEmpty || invoiceBO.JZ_OA_SupplierAddress == consignor.MainAddress.PK);
				});
			}
		}

		public void TestInvoiceDefaultingBehaviour()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
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
				var consignorMiscServ = consignor.MiscServ;
				consignorMiscServ.OM_RX_NKEXDefCurrency = Core.Constants.CurrencyCodes.Australia;
				consignorMiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;
				var buyerSupplierLink = consignor.BuyerLinks.AddNew(consignee);
				buyerSupplierLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				buyerSupplierLink.OL_RX_NKDefaultCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				var linkTrnMode = buyerSupplierLink.OrgSupBuyLinkTrnModes[0];
				linkTrnMode.PF_TransportMode = ZString.Empty;
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

				var invoiceLineAddInfo1 = string.Format("{0}=1.23*{1}=FR", AUAddInfoSchema.ZA_ISS.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3));
				var invoiceLineDataObject1 = SetupCommercialInvoiceLine(
					1,
					AddInfoCollectionCreator.CreateCollection(invoiceLineAddInfo1),
					null
				);
				var invoiceLineAddInfo2 = string.Format("{0}=1.23*{1}=NZ", AUAddInfoSchema.ZA_ISS.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3));
				var invoiceLineDataObject2 = SetupCommercialInvoiceLine2(
					2,
					AddInfoCollectionCreator.CreateCollection(invoiceLineAddInfo2),
					null
				);
				var invoiceLineAddInfo3 = string.Format("{0}=1.23*{1}=AU", AUAddInfoSchema.ZA_ISS.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3));
				var invoiceLineDataObject3 = SetupCommercialInvoiceLine2(
					3,
					AddInfoCollectionCreator.CreateCollection(invoiceLineAddInfo3),
					null
				);
				var part1 = Factory.New<Business.OrgSupplierPart>();
				part1.OP_PartNum = invoiceLineDataObject1.PartNo.GetValueOrDefault();
				part1.OP_Desc = "GOODS 1 PART DESC";
				part1.RelatedOrganisations.AddSupplier(consignor);
				part1.RelatedOrganisations.AddOwner(consignee);
				var part1Classification = part1.ClassificationsForBinding.AddNew();
				part1Classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				part1Classification.CC_LookupCode = part1.OP_PartNum + "LKP";
				part1Classification.CC_TariffNum = "1010101010";
				part1Classification.CC_Description = "GOODS 1 CLASS DESC";
				part1.PivotsForBinding[0].CI_ChildType = ClassificationTypeList.Codes.HTI;

				var part2 = Factory.New<Business.OrgSupplierPart>();
				part2.OP_PartNum = invoiceLineDataObject2.PartNo.GetValueOrDefault();
				part2.OP_Desc = "GOODS 2 PART DESC";
				part2.RelatedOrganisations.AddSupplier(consignor);
				part2.RelatedOrganisations.AddOwner(consignee);
				var part2Classification = part2.ClassificationsForBinding.AddNew();
				part2Classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				part2Classification.CC_LookupCode = part2.OP_PartNum + "LKP";
				part2Classification.CC_TariffNum = "2020202020";
				part2Classification.CC_Description = "GOODS 2 CLASS DESC";
				part2.PivotsForBinding[0].CI_ChildType = ClassificationTypeList.Codes.HTI;

				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var invoiceAddInfo = string.Format("{0}=ZA*{1}=TV", AUAddInfoSchema.ZA_ORG.Name.Substring(3), AUAddInfoSchema.ZA_VALB_Hidden.Name.Substring(3));
				var invoiceDataObject = SetupCommercialInvoiceHeaderData("INV6854", null, null, 8685.54m, null, new ZDateTime(2011, 4, 2), null, 96.87m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicYards }, 86.69m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilotonnes }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, FixedRate, 1.5m, 1.55m, 2.5m, "PA123", 1234.43m, 2.3m, new ZDateTime(2012, 2, 3), new CodeDescriptionPair() { Code = "MS1" },
					AddInfoCollectionCreator.CreateCollection(invoiceAddInfo), null, new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
				{
					invoiceLineDataObject1, invoiceLineDataObject2, invoiceLineDataObject3
				}), null, null);
				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);

				var faker = new FakeDeclarationCreatorForInvoice(Factory.New<BaseJobComInvoiceHeader>());
				var declaration = (BaseJobDeclaration)faker.HeaderData;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var topGroupInvoice = declaration.TopGroupInvoice;
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				var invoiceBO = reader.ReadIntoBusinessObject();
				AssertNotNull(invoiceBO);

				CombineAssertions(delegate
				{
					AssertContents(invoiceBO, "INV6854", consignee.PK, consignor.PK, 8685.54m, ZString.Empty, new ZDateTime(2011, 4, 2), ZString.Empty, 96.87m, Core.Constants.Volume.CubicYards, 86.69m, Core.Constants.Weight.Kilotonnes, 11.11m, Core.Constants.Weight.Kilograms, FixedRate.Code.GetValueOrDefault(), 1.5m, 1.55m, "PA123", 1234.43m, 2.3m, new ZDateTime(2012, 2, 3), 2.5m, invoiceAddInfo);
					AssertEquals("invoiceBO.JobComInvoiceLines.Count", 3, invoiceBO.JobComInvoiceLines.Count);
					var invoiceLine1 = invoiceBO.JobComInvoiceLines[0];
					AssertContents(invoiceLine1, 1, invoiceLineAddInfo1);
					AssertEquals("invoiceLine1.JI_Tariff", "", invoiceLine1.JI_Tariff);
					AssertEquals("invoiceLine1.JI_CC", ZGuid.Empty, invoiceLine1.JI_CC);
					AssertEquals("invoiceLine1.JI_OP", ZGuid.Empty, invoiceLine1.JI_OP);
					var invoiceLine2 = invoiceBO.JobComInvoiceLines[1];
					AssertContents2(invoiceLine2, 2, invoiceLineAddInfo2);
					AssertEquals("invoiceLine2.JI_Tariff", "", invoiceLine2.JI_Tariff);
					AssertEquals("invoiceLine2.JI_CC", ZGuid.Empty, invoiceLine2.JI_CC);
					AssertEquals("invoiceLine2.JI_OP", ZGuid.Empty, invoiceLine2.JI_OP);
					var invoiceLine3 = invoiceBO.JobComInvoiceLines[2];
					AssertContents2(invoiceLine3, 3, invoiceLineAddInfo3);
					AssertEquals("invoiceLine3.JI_Tariff", "", invoiceLine3.JI_Tariff);
					AssertEquals("invoiceLine3.JI_CC", ZGuid.Empty, invoiceLine3.JI_CC);
					AssertEquals("invoiceLine3.JI_OP", ZGuid.Empty, invoiceLine3.JI_OP);
				});

				invoiceBO.Delete();
				Factory.SaveForTesting();
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				invoiceBO = reader.ReadIntoBusinessObject();
				AssertNotNull(invoiceBO);

				CombineAssertions(delegate
				{
					AssertContents(invoiceBO, "INV6854", consignee.PK, consignor.PK, 8685.54m, Core.Constants.CurrencyCodes.UnitedStates, new ZDateTime(2011, 4, 2), Core.Constants.IncoTerms.CostAndInsurance, 96.87m, Core.Constants.Volume.CubicYards, 86.69m, Core.Constants.Weight.Kilotonnes, 11.11m, Core.Constants.Weight.Kilograms, FixedRate.Code.GetValueOrDefault(), 1.5m, 1.55m, "PA123", 1234.43m, 2.3m, new ZDateTime(2012, 2, 3), 2.5m, "HeaderREL_Hidden=N*" + invoiceAddInfo);
					AssertEquals("invoiceBO.JobComInvoiceLines.Count", 3, invoiceBO.JobComInvoiceLines.Count);
					var invoiceLine1 = invoiceBO.JobComInvoiceLines[0];
					AssertContents(invoiceLine1, 1, invoiceLineAddInfo1);
					AssertEquals("invoiceLine1.JI_Tariff", "1010.10.10 10", invoiceLine1.JI_Tariff);
					AssertEquals("invoiceLine1.JI_CC", part1Classification.PK, invoiceLine1.JI_CC);
					AssertEquals("invoiceLine1.JI_OP", part1.PK, invoiceLine1.JI_OP);
					var invoiceLine2 = invoiceBO.JobComInvoiceLines[1];
					AssertContents2(invoiceLine2, 2, invoiceLineAddInfo2);
					AssertEquals("invoiceLine2.JI_Tariff", "2020.20.20 20", invoiceLine2.JI_Tariff);
					AssertEquals("invoiceLine2.JI_CC", part2Classification.PK, invoiceLine2.JI_CC);
					AssertEquals("invoiceLine2.JI_OP", part2.PK, invoiceLine2.JI_OP);
					var invoiceLine3 = invoiceBO.JobComInvoiceLines[2];
					AssertContents2(invoiceLine3, 3, invoiceLineAddInfo3);
					AssertEquals("invoiceLine3.JI_Tariff", "2020.20.20 20", invoiceLine3.JI_Tariff);
					AssertEquals("invoiceLine3.JI_CC", part2Classification.PK, invoiceLine3.JI_CC);
					AssertEquals("invoiceLine3.JI_OP", part2.PK, invoiceLine3.JI_OP);
				});
			}
		}

		public void TestInvoiceLineDefaultingBehaviour()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				consignor.OH_IsConsignor = ZBool.True;

				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				consignee.OH_IsConsignee = ZBool.True;

				var part = Factory.New<Business.OrgSupplierPart>();
				part.OP_PartNum = "PARTABC123";
				part.OP_Desc = "GOODS 1 PART DESC";
				part.OP_StockKeepingUnit = "PCE";
				part.RelatedOrganisations.AddSupplier(consignor);
				part.RelatedOrganisations.AddOwner(consignee);

				var partClassification = part.ClassificationsForBinding.AddNew();
				partClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
				partClassification.CC_LookupCode = "PARTABC123LKP";
				partClassification.CC_TariffNum = "7013.31.00 06";
				partClassification.CC_Description = "GOODS 1 CLASS DESC";
				part.PivotsForBinding[0].CI_ChildType = ClassificationTypeList.Codes.HTI;

				var partUnit = part.PartUnits.AddNew();
				partUnit.OF_QuantityInParent = 1m;
				partUnit.OF_ParentPackType = "NO";
				partUnit.OF_PackType = "PCE";

				var invoiceLineDataObject = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					LineNo = 1,
					CustomsQuantity = 43008m,
					CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "NO" },
					InvoiceQuantity = 2688m,
					InvoiceQuantityUnit = new CodeDescriptionPair() { Code = "BOX" },
					LinePrice = 14160.38m,
					NetWeightUnit = new UnitOfWeight() { Code = "T" },
					PartNo = "PARTABC123",
				};

				Factory.SaveForTesting();

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var invoiceDataObject = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV456856",
					IncoTerm = new CodeDescriptionPair() { Code = "DDP" },
					InvoiceCurrency = new Currency() { Code = "HK" },
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject })));

				invoiceDataObject.Supplier = invoiceDataObject.AddOrgAddress(writeManager, consignor, AddressTypes.Supplier);
				invoiceDataObject.Buyer = invoiceDataObject.AddOrgAddress(writeManager, consignee, AddressTypes.Importer);

				var faker = new FakeDeclarationCreatorForInvoice(Factory.New<BaseJobComInvoiceHeader>());
				var declaration = (BaseJobDeclaration)faker.HeaderData;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				var topGroupInvoice = declaration.TopGroupInvoice;

				var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				var invoiceBO = reader.ReadIntoBusinessObject();
				AssertNotNull(invoiceBO);

				CombineAssertions(delegate
				{
					AssertEquals("INV456856", invoiceBO.JZ_InvoiceNumber);
					AssertEquals("invoiceBO.JobComInvoiceLines.Count", 1, invoiceBO.JobComInvoiceLines.Count);
					AssertEquals("invoiceLine.JZ_IncoTerm", "DDP", invoiceBO.JZ_IncoTerm);
					AssertEquals("invoiceLine.JZ_RX_NKInvoice_Currency", "HK", invoiceBO.JZ_RX_NKInvoice_Currency);

					var invoiceLine = invoiceBO.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_Tariff", "7013.31.00 06", invoiceLine.JI_Tariff);
					AssertEquals("invoiceLine.JI_CC", partClassification.PK, invoiceLine.JI_CC);
					AssertEquals("invoiceLine.JI_OP", part.PK, invoiceLine.JI_OP);
					AssertEquals("invoiceLine.JI_InvoiceQuantity", 2688m, invoiceLine.JI_InvoiceQuantity);
					AssertEquals("invoiceLine.JI_InvoiceUQ", "BOX", invoiceLine.JI_InvoiceUQ);
					AssertEquals("invoiceLine.JI_CustomsQuantity", 43008m, invoiceLine.JI_CustomsQuantity);
					AssertEquals("invoiceLine.JI_CustomsUnitQty", "NO", invoiceLine.JI_CustomsUnitQty);
					AssertEquals("invoiceLine.JI_LinePrice", 14160.38m, invoiceLine.JI_LinePrice);
					AssertEquals("invoiceLine.JI_NetWeightUQ", "T", invoiceLine.JI_NetWeightUQ);
				});
				invoiceBO.Delete();
				invoiceLineDataObject.HarmonisedCode = "1020.30.40 50";

				reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				invoiceBO = reader.ReadIntoBusinessObject();
				AssertNotNull(invoiceBO);

				CombineAssertions(delegate
				{
					AssertEquals("INV456856", invoiceBO.JZ_InvoiceNumber);
					AssertEquals("invoiceBO.JobComInvoiceLines.Count", 1, invoiceBO.JobComInvoiceLines.Count);
					AssertEquals("invoiceLine.JZ_IncoTerm", "DDP", invoiceBO.JZ_IncoTerm);
					AssertEquals("invoiceLine.JZ_RX_NKInvoice_Currency", "HK", invoiceBO.JZ_RX_NKInvoice_Currency);

					var invoiceLine = invoiceBO.JobComInvoiceLines[0];
					AssertEquals("invoiceLine.JI_Tariff", "1020.30.40 50", invoiceLine.JI_Tariff);
					AssertEquals("invoiceLine.JI_CC", ZGuid.Empty, invoiceLine.JI_CC);
					AssertEquals("invoiceLine.JI_OP", part.PK, invoiceLine.JI_OP);
					AssertEquals("invoiceLine.JI_InvoiceQuantity", 2688m, invoiceLine.JI_InvoiceQuantity);
					AssertEquals("invoiceLine.JI_InvoiceUQ", "BOX", invoiceLine.JI_InvoiceUQ);
					AssertEquals("invoiceLine.JI_CustomsQuantity", 43008m, invoiceLine.JI_CustomsQuantity);
					AssertEquals("invoiceLine.JI_CustomsUnitQty", "NO", invoiceLine.JI_CustomsUnitQty);
					AssertEquals("invoiceLine.JI_LinePrice", 14160.38m, invoiceLine.JI_LinePrice);
					AssertEquals("invoiceLine.JI_NetWeightUQ", "T", invoiceLine.JI_NetWeightUQ);
				});
			}
		}

		public void TestInvoiceCusAddInfoAndCusCodeData()
		{
			var usDeclaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			usDeclaration.JE_MasterBill = "USMWB123";
			usDeclaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var invoice = usDeclaration.Invoices.AddNew();
			var invoiceCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)invoice;
			var invoiceRelatedDocument = "RLD";
			var relatedDocumentAirWaybillNumber = "AW";
			Assert(invoiceCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(invoiceRelatedDocument, out var type));
			invoice.Delete();
			var erDeclaration = Factory.New<BaseJobDeclaration>();
			erDeclaration.JE_MasterBill = "EUMWB123";
			erDeclaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var guranteeeAddInfo = TestAddInfoSchema.Constants.UZ_String.Substring(3) + "=12";
			var taxAddInfo = TestAddInfoSchema.Constants.UZ_String.Substring(3) + "=32";
			var taxDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.GBTax },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(taxAddInfo)
			};
			var relatedDocumentDataObject = new UniversalCustoms.CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = invoiceRelatedDocument },
				SubType = new CodeDescriptionPair35Char() { Code = relatedDocumentAirWaybillNumber },
				Reference = "AW1234"
			};
			var declarationDataObject = SetupDeclaration(null, "USMWB123", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group",
				null,
				new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
				{
					SetupCommercialInvoiceHeaderData(
						customsReferenceCollection: new List<UniversalCustoms.CustomsReference>(new[] { relatedDocumentDataObject }),
						addInfoGroupCollection: new List<UniversalCustoms.AddInfoGroup>(new[] { taxDataObject })
					)
				})
			);
			BaseJobDeclaration declarationBO;

			usDeclaration.Company.SetCountry(Core.Constants.CountryCodes.UnitedStates);
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
				AssertEquals(usDeclaration, declarationBO);
				AssertEquals("Invoice", 1, declarationBO.Invoices.Count);
				var invoiceBO = declarationBO.Invoices[0];
				var cusAddInfoBOs = LoadCusAddInfo(invoiceBO.TablePrefix, invoiceBO.PK);
				AssertEquals(0, cusAddInfoBOs.Length);
				var cusCodeDataBOs = LoadCusCodeData(invoiceBO.TablePrefix, invoiceBO.PK);
				AssertEquals(1, cusCodeDataBOs.Length);
				var relatedDocumentBO = cusCodeDataBOs[0];
				AssertCusCodeDataContents(relatedDocumentBO, invoiceBO.TablePrefix, invoiceBO.PK, invoiceRelatedDocument, relatedDocumentAirWaybillNumber, "AW1234", ZBool.False, ZShort.Zero);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Warning - Matching 'Supplier':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Warning - Matching 'Importer':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Updated Declaration (Master Bill='USMWB123') from UniversalShipment.
".Trim(), logger.Logs);
			});

			declarationDataObject.WayBillNumber = "EUMWB123";
			logger.ClearLogs();
			declarationBO = null;
			erDeclaration.Company.SetCountry(Core.Constants.CountryCodes.Eritrea);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			using (BaseJobComInvoiceHeaderTypeDecider.SetupDefaultTypeForUnsupportedCountryForTesting(typeof(JobComInvoiceHeaderWithInterfaceForTesting)))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				declarationDataObject.DataContext = dataContext;
				var mockProvider = new Mock<IUniversalCustomsDataObjectProvider>();
				mockProvider.CallBase = true;
				var list = new CodeDescriptionPairList();
				list.AddPair(CusAddInfoTypeAttribute.Codes.GBTax, "Tax");
				mockProvider
					.Setup(m => m.TableSpecificCusAddInfoTypeList(It.IsAny<ZString>(), It.IsAny<string>()))
					.Returns(list);
				var providers = new Hashtable
				{
					{ Core.Constants.CountryCodes.Eritrea, new TestObjectHandle(mockProvider.Object) }
				};

				using (ObjectFactory.Substitute("UniversalCustomsDataObjectProviders", providers))
				{
					var invoiceData = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0];
					var invoiceForTesting = Factory.New<JobComInvoiceHeaderWithInterfaceForTesting>();
					invoiceForTesting.JZ_JE = erDeclaration.PK;
					invoiceForTesting.JZ_InvoiceNumber = invoiceData.InvoiceNumber.GetValueOrDefault();
					invoiceForTesting.JZ_JZ_GroupInvoiceFK = erDeclaration.TopGroupInvoice.PK;
					invoiceForTesting.getCusAddInfoTypeForTesting = () =>
					{
						var dictionary = new Dictionary<ZString, Type>();
						dictionary.Add(CusAddInfoTypeAttribute.Codes.GBTax, typeof(CusAddInfo<AddInfoWithTypeCodeGBTax>));
						return dictionary;
					};
					var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, logger, new UniversalDataObjectReaderHelper(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode), erDeclaration.TopGroupInvoice);
					CombineAssertions(delegate
					{
						var invoiceBO = reader.ReadIntoBusinessObject(true);
						var cusAddInfoBOs = LoadCusAddInfo(invoiceBO.TablePrefix, invoiceBO.PK);
						AssertEquals(1, cusAddInfoBOs.Length);
						var taxBO = cusAddInfoBOs[0];
						AssertCusAddInfoContents(taxBO, invoiceBO.TablePrefix, invoiceBO.PK, CusAddInfoTypeAttribute.Codes.GBTax, partialAddInfoData: taxAddInfo);

						var cusCodeDataBOs = LoadCusCodeData(invoiceBO.TablePrefix, invoiceBO.PK);
						AssertEquals(0, cusCodeDataBOs.Length);
						AssertMultilineASCIIEquals("logger.Logs", @"
Warning - Matching 'Supplier':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Warning - Matching 'Importer':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Customs Reference was not processed as there is no support for Table with code 'JZ'.
".Trim(), logger.Logs);
					});
				}

				mockProvider.VerifyAll();
			}
		}

		public void TestInvoiceLineUNDGDataItem()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineDataObject = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				HazardousMaterial = new UniversalCustoms.HazardousMaterial()
				{
					Code = "0004",
					UNDGCollection = new List<UNDG>(new[]
					{
						new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
						{
							UNDGCode = "0004a"
						}
					})
				}
			};

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group",
				null,
				new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
				{
					SetupCommercialInvoiceHeaderData(
						commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
						{
							invoiceLineDataObject
						})
					)
				})
			);
			BaseJobDeclaration declarationBO;
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
				AssertEquals("Invoice", 1, declarationBO.Invoices.Count);
				var invoiceBO = declarationBO.Invoices[0];
				AssertEquals("InvoiceLine", 1, invoiceBO.JobComInvoiceLines.Count);
				var invoiceLineBO = invoiceBO.JobComInvoiceLines[0];
				AssertEquals("invoiceLineBO.JI_HazMatCode", "0004", invoiceLineBO.JI_HazMatCode);
				AssertEquals("invoiceLineBO.UNDGs.Count", 1, invoiceLineBO.UNDGs.Count);
				var undgBO = invoiceLineBO.UNDGs[0];
				AssertEquals("undgBO.DI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, undgBO.DI_ParentTableCode);
				AssertEquals("undgBO.DI_ParentID", invoiceLineBO.PK, undgBO.DI_ParentID);
				AssertEquals("undgBO.Substance.DG_Code", "0004a", undgBO.Substance?.DG_Code);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Warning - Matching 'Supplier':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Warning - Matching 'Importer':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestInvoiceLineCusAddInfoAndCusCodeData()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			declaration.JE_MasterBill = "MYMASTER";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCusAddInfoSupporter = (ICusAddInfoTypeSupporter)invoiceLine;
			Type type = null;
			Assert(invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USDOT, out type));
			Assert(invoiceLineCusAddInfoSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USFDA, out type));

			var dotAddInfo = USDOTAddInfoSchema.Constants.US_DOTCommercialDesc.Substring(3) + "=DESC1234";
			var dotDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDOT },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(dotAddInfo)
			};
			var fdaAddInfo = USFDAAddInfoSchema.Constants.US_FDACommercialDesc.Substring(3) + "=FDADESC123";
			var fdaDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USFDA },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(fdaAddInfo)
			};
			var invoiceLineCusCodeDataSupporter = (ICusCodeDataTypeSupporter)invoiceLine;
			var feeTypeString = "FEE";
			var feeMerchandiseProcessing = "499";
			Assert(invoiceLineCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(feeTypeString, out type));
			var invoiceLineDataObject = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>(new[] { dotDataObject, fdaDataObject }),
				CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
				{
					SetupCustomsReference2(feeTypeString, feeMerchandiseProcessing)
				})
			};

			var declarationDataObject = SetupDeclaration(null, "MYMASTER", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group",
				null,
				new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
				{
					SetupCommercialInvoiceHeaderData(
						commercialInvoiceLineCollection: new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
						{
							invoiceLineDataObject
						})
					)
				})
			);

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
				AssertEquals("Invoice", 1, declarationBO.Invoices.Count);
				var invoiceBO = declarationBO.Invoices[0];
				AssertEquals("InvoiceLine", 1, invoiceBO.JobComInvoiceLines.Count);
				var invoiceLineBO = invoiceBO.JobComInvoiceLines[0];
				var cusAddInfoBOs = LoadCusAddInfo(invoiceLineBO.TablePrefix, invoiceLineBO.PK);
				AssertEquals("CusAddInfo", 2, cusAddInfoBOs.Length);
				var dotBO = cusAddInfoBOs[0];
				var fdaBO = cusAddInfoBOs[1];
				if (CusAddInfoTypeAttribute.Codes.USDOT.Equals(fdaBO[CusAddInfoSchema.Constants.B7_Type]))
				{
					dotBO = cusAddInfoBOs[1];
					fdaBO = cusAddInfoBOs[0];
				}
				AssertCusAddInfoContents(dotBO, invoiceLineBO.TablePrefix, invoiceLineBO.PK, CusAddInfoTypeAttribute.Codes.USDOT, partialAddInfoData: dotAddInfo);
				AssertCusAddInfoContents(fdaBO, invoiceLineBO.TablePrefix, invoiceLineBO.PK, CusAddInfoTypeAttribute.Codes.USFDA, partialAddInfoData: fdaAddInfo);
				var cusCodeDataBOs = LoadCusCodeData(invoiceLineBO.TablePrefix, invoiceLineBO.PK);
				AssertEquals("cusCodeDataBOs", 1, cusCodeDataBOs.Length);
				var feeBO = cusCodeDataBOs[0];
				AssertCusCodeDataContents2(feeBO, invoiceLineBO.TablePrefix, invoiceLineBO.PK, feeTypeString, feeMerchandiseProcessing);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Warning - Matching 'Supplier':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Warning - Matching 'Importer':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Updated Declaration (Master Bill='MYMASTER') from UniversalShipment.
".Trim(), logger.Logs);
			});
		}

		public void TestContainerInvoiceLinePivot()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var invoiceLine1Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { Link = 2, Description = "BOB" };
			var invoiceLine2Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { Link = 1, Description = "WENDY" };
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "OB323",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				ContainerNumber = "CONT1",
			};
			packingLine1.SetPackedItemCollection(() => new List<PackedItem>(new[]
			{
				SetupPackedItem(2, 100m, 200m, 10, 150m),
				SetupPackedItem(1, 110m, 210m, 11, 151m)
			}));
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "OB323",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				ContainerNumber = "CONT2",
			};
			packingLine2.SetPackedItemCollection(() => new List<PackedItem>(new[]
			{
				SetupPackedItem(3, 100m, 200m, 10, 150m)
			}));
			var packingLine3 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "OB323",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				ContainerNumber = "CONT4",
			};
			packingLine3.SetPackedItemCollection(() => new List<PackedItem>(new[]
			{
				SetupPackedItem(2, null, null, null, null)
			}));
			var packingLine4 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "OB323",
				BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				ContainerNumber = "CONT5",
			};
			packingLine4.SetPackedItemCollection(() => new List<PackedItem>(new[]
			{
				SetupPackedItem(1, 120m, 220m, 12, 152m)
			}));
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				WayBillNumber = "OB323",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLine1Data, invoiceLine2Data })))
					})
				},
			};
			shipmentData.SetContainerCollection(() => new DataObjectList<Container>(new[]
			{
				new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT4" },
				new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT1" },
				new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT2" },
				new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT5" }
			}));
			shipmentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine1, packingLine2, packingLine3, packingLine4 })
			{
				Content = CollectionContent.Complete
			});

			var reader = new JobDeclarationDataObjectReader(shipmentData, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals(4, declarationBO.CusContainers.Count);
			var containerBOs = declarationBO.CusContainers.OfType<BaseCusContainer>().ToArray();
			var containerBO1 = containerBOs.First(x => x.CO_ContainerNumber == "CONT1");
			var containerBO2 = containerBOs.First(x => x.CO_ContainerNumber == "CONT2");
			var containerBO4 = containerBOs.First(x => x.CO_ContainerNumber == "CONT4");
			var containerBO5 = containerBOs.First(x => x.CO_ContainerNumber == "CONT5");
			AssertEquals(1, declarationBO.Invoices.Count);
			var invoiceBO = declarationBO.Invoices[0];
			AssertEquals(2, invoiceBO.JobComInvoiceLines.Count);
			var invoiceLine1BO = invoiceBO.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "BOB");
			var invoiceLine2BO = invoiceBO.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "WENDY");
			invoiceLine1BO.ContainersPivot.Load();
			AssertEquals(2, invoiceLine1BO.ContainersPivot.Count);
			var pivotBOs = invoiceLine1BO.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().ToArray();
			var pivotBO1 = pivotBOs.First(x => x.C2_CO == containerBO1.PK);
			var pivotBO2 = pivotBOs.First(x => x.C2_CO == containerBO4.PK);
			AssertContents(pivotBO1, 100m, 200m, 10, 150m);
			AssertContents(pivotBO2, ZDecimal.Zero, ZDecimal.Zero, ZInt.Zero, ZDecimal.Zero);
			invoiceLine2BO.ContainersPivot.Load();
			AssertEquals(2, invoiceLine2BO.ContainersPivot.Count);
			pivotBOs = invoiceLine2BO.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().ToArray();
			pivotBO1 = pivotBOs.First(x => x.C2_CO == containerBO1.PK);
			pivotBO2 = pivotBOs.First(x => x.C2_CO == containerBO5.PK);
			AssertContents(pivotBO1, 110m, 210m, 11, 151m);
			AssertContents(pivotBO2, 120m, 220m, 12, 152m);
		}

		public void TestInvoiceCustomizedFieldCollection()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.CustomizedFieldCollection = new List<CustomizedField>();
			invoiceData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField1", (ZString)"Hello World"));
			invoiceData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField2", ZDateTime.BrettsBirthday));
			invoiceData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField3", (ZDecimal)200.5));
			invoiceData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField4", (ZBool)true));
			invoiceData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField5", (ZString)"Hello".PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1')));

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			AssertEquals("Count of customs fields", 5, invoice.GetUserDefinedValues().Count());
			AssertEquals("CustomField1", "Hello World", invoice.GetUserDefinedValue<ZString>("CustomField1"));
			AssertEquals("CustomField2", ZDateTime.BrettsBirthday, invoice.GetUserDefinedValue<ZDateTime>("CustomField2"));
			AssertEquals("CustomField3", (ZDecimal)200.5, invoice.GetUserDefinedValue<ZDecimal>("CustomField3"));
			AssertEquals("CustomField4", true, invoice.GetUserDefinedValue<ZBool>("CustomField4"));
			AssertEquals("CustomField5", "Hello".PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength, '1'), invoice.GetUserDefinedValue<ZString>("CustomField5"));
			AssertEquals("Attempted to insert 101 characters into Field [CustomField5] which has a maximum length of 100 characters. Field was truncated.", Logger.GetWarnings());
		}

		public void TestImportValuationDate()
		{
			var invoiceDataObject = SetupCommercialInvoiceHeaderData();
			invoiceDataObject.ValuationDateOverride = new ZDateTime(2016, 9, 26, 17, 12, 0);
			invoiceDataObject.BillNumber = "MB11111111";
			invoiceDataObject.BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "MB11111111";
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
			var invoice = reader.ReadIntoBusinessObject();
			AssertEquals(new ZDateTime(2016, 9, 26, 17, 12, 0), invoice.JZ_ValuationDateOverride);
		}

		public void TestImportNAddInfoProperty_DefaultEnable() => AssertImportNAddInfoProperty(true);

		public void TestImportNAddInfoProperty_DefaultDisable() => AssertImportNAddInfoProperty(false);

		void AssertImportNAddInfoProperty(bool useDefaulting)
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useDefaulting))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var invoiceLineData = new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
				{
					new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Link = 1,
						Description = "BOB",
						AddInfoCollection = AddInfoCollectionCreator.CreateCollection("DtyPymntMthd=CAS*Compositions=組成*Group=群組")
					}
				});

				var invoiceDataObject = SetupCommercialInvoiceHeaderData(commercialInvoiceLineCollection: invoiceLineData);
				var declaration = Factory.New<BaseJobDeclaration>();
				var topGroupInvoice = declaration.TopGroupInvoice;
				var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceDataObject, Logger, CurrentCompanyHelper, topGroupInvoice);
				var invoice = reader.ReadIntoBusinessObject();
				AssertEquals("JI_DtyPymntMthd", "CAS", invoice.JobComInvoiceLines[0][TWJobComInvoiceLineSchema.Constants.JI_DtyPymntMthd]);
				AssertEquals("JI_Compositions", "組成", invoice.JobComInvoiceLines[0][TWJobComInvoiceLineSchema.Constants.JI_Compositions]);
				AssertEquals("JI_Group", "群組", invoice.JobComInvoiceLines[0][TWJobComInvoiceLineSchema.Constants.JI_Group]);
			}
		}

		public void TestInvoiceLineCustomizedFieldCollection()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine { Link = 2, Description = "BOB" };
			invoiceData.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>
			{
				invoiceLineData
			});
			invoiceLineData.CustomizedFieldCollection = new List<CustomizedField>();
			invoiceLineData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField1", (ZString)"Hello World"));
			invoiceLineData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField2", ZDateTime.BrettsBirthday));
			invoiceLineData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField3", (ZDecimal)200.5));
			invoiceLineData.CustomizedFieldCollection.Add(CustomizedField.New("CustomField4", (ZBool)true));

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			var invoiceLineBO = invoice.JobComInvoiceLines[0];

			AssertEquals("Count of customs fields", 4, invoiceLineBO.GetUserDefinedValues().Count());
			AssertEquals("CustomField1", "Hello World", invoiceLineBO.GetUserDefinedValue<ZString>("CustomField1"));
			AssertEquals("CustomField2", ZDateTime.BrettsBirthday, invoiceLineBO.GetUserDefinedValue<ZDateTime>("CustomField2"));
			AssertEquals("CustomField3", (ZDecimal)200.5, invoiceLineBO.GetUserDefinedValue<ZDecimal>("CustomField3"));
			AssertEquals("CustomField4", true, invoiceLineBO.GetUserDefinedValue<ZBool>("CustomField4"));
		}

		public void TestImportCustomsFourthQuantity()
		{
			var existingDeclaration = Factory.New<BaseJobDeclaration>();
			var existingInvoice = existingDeclaration.Invoices.AddNew();
			existingInvoice.JZ_InvoiceNumber = "INV123ABC";
			var existingInvoiceLine = existingInvoice.InvoiceLines.AddNew();
			existingInvoiceLine.JI_MatchingKey = "INVLINE123ABC";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV123ABC",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									DataImportMatchingKey = "INVLINE123ABC",
									CustomsFourthQuantity = 10.25m,
									CustomsFourthQuantityUnit = new CodeDescriptionPair6Char()
									{
										Code = "MTQ",
										Description = "Cubic meter"
									}
								}
							})))
					})
				}
			};

			var reader = new JobDeclarationDataObjectReader(shipment, logger, Factory);
			var declaration = reader.ReadIntoBusinessObject();
			var invoice = declaration.Invoices.Single();
			var invoiceLine = (BaseJobComInvoiceLine)invoice.InvoiceLines.Single();

			CombineAssertions(() =>
			{
				AssertEquals("JI_CustomsFourthQuantity", 10.25m, invoiceLine.JI_CustomsFourthQuantity);
				AssertEquals("JI_CustomsFourthUnitQty", "MTQ", invoiceLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestImportCustomsFifthQuantity()
		{
			var existingDeclaration = Factory.New<BaseJobDeclaration>();
			var existingInvoice = existingDeclaration.Invoices.AddNew();
			existingInvoice.JZ_InvoiceNumber = "INV123ABC";
			var existingInvoiceLine = existingInvoice.InvoiceLines.AddNew();
			existingInvoiceLine.JI_MatchingKey = "INVLINE123ABC";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV123ABC",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									DataImportMatchingKey = "INVLINE123ABC",
									CustomsFifthQuantity = 16.55m,
									CustomsFifthQuantityUnit = new CodeDescriptionPair6Char()
									{
										Code = "KGM",
										Description = "Kilogram"
									}
								}
							})))
					})
				}
			};

			var reader = new JobDeclarationDataObjectReader(shipment, logger, Factory);
			var declaration = reader.ReadIntoBusinessObject();
			var invoice = declaration.Invoices.Single();
			var invoiceLine = (BaseJobComInvoiceLine)invoice.InvoiceLines.Single();

			CombineAssertions(() =>
			{
				AssertEquals("JI_CustomsFifthQuantity", 16.55m, invoiceLine.JI_CustomsFifthQuantity);
				AssertEquals("JI_CustomsFifthUnitQty", "KGM", invoiceLine.JI_CustomsFifthUnitQty);
			});
		}

		public void TestImportMarksAndNumbersOnInvoiceHeader()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							MarksAndNumbers = "Test Marks and Numbers 16/10/2020"
						}
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("Test Marks and Numbers 16/10/2020", declarationBO.Invoices.OfType<BaseJobComInvoiceHeader>().FirstOrDefault().JZ_MarksAndNumbers);
		}

		public void TestImportPackagePivots_WhenPackedQuantityIsZero()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var invoiceLine1Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { Link = 1, Description = "BOB", LineNo = 1 };
			var invoiceLine2Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { Link = 2, Description = "WENDY", LineNo = 2 };
			var invoiceLine3Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { Link = 3, Description = "PETER", LineNo = 3 };

			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { BillNumber = "OB323" };
			packingLine1.SetPackedItemCollection(() => new List<PackedItem>()
			{
				new PackedItem() { CommercialInvoiceLineLink = 1, PackedQuantity = 100 },
				new PackedItem() { CommercialInvoiceLineLink = 2, PackedQuantity = 0 },
				new PackedItem() { CommercialInvoiceLineLink = 3, PackedQuantity = 0 },
			});

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea },
				WayBillNumber = "OB323",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLine1Data, invoiceLine2Data, invoiceLine3Data })))
					}),
				},
			};
			shipmentData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine1 }) { Content = CollectionContent.Complete });

			var reader = new JobDeclarationDataObjectReaderSupportPackagePivot(shipmentData, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			declarationBO.Packages.Load();
			AssertEquals("Declaration Packages count", 1, declarationBO.Packages.Count);

			AssertEquals("Invoice Header count", 1, declarationBO.Invoices.Count);
			var invoiceBO = declarationBO.Invoices[0];

			AssertEquals("Invoice Lines count", 3, invoiceBO.JobComInvoiceLines.Count);
			var invoiceLine1BO = invoiceBO.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "BOB");
			var invoiceLine2BO = invoiceBO.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "WENDY");
			var invoiceLine3BO = invoiceBO.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "PETER");

			invoiceLine1BO.PackagesPivot.Load();
			AssertEquals("Package", 1, invoiceLine1BO.PackagesPivot.Count);

			invoiceLine2BO.PackagesPivot.Load();
			AssertEquals("Package with PackedQuantity=0 should still be linked to Inv. Line => Package Count", 1, invoiceLine2BO.PackagesPivot.Count);

			invoiceLine3BO.PackagesPivot.Load();
			AssertEquals("Package with PackedQuantity=0 should still be linked to Inv. Line => Package Count", 1, invoiceLine3BO.PackagesPivot.Count);
		}

		public void TestImportJobComInvLineComponentInventory()
		{
			var existingDeclaration = Factory.New<BaseJobDeclaration>();
			var existingInvoice = existingDeclaration.Invoices.AddNew();
			existingInvoice.JZ_InvoiceNumber = "INV123ABC";
			var existingInvoiceLine = existingInvoice.InvoiceLines.AddNew();
			existingInvoiceLine.JI_MatchingKey = "INVLINE123ABC";
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV123ABC",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
								{
									DataImportMatchingKey = "INVLINE123ABC",
									AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>()
									{
										new UniversalCustoms.AddInfoGroup()
										{
											Type = new CodeDescriptionPair() { Code = "ALI", Description = "Allocation Info" },
											AddInfoCollection = new List<AddInfo>()
											{
												new AddInfo() { Key = "AllocationKey", Value = "AK_Value" },
												new AddInfo() { Key = "Quantity", Value = "7" }
											}
										},
										new UniversalCustoms.AddInfoGroup()
										{
											Type = new CodeDescriptionPair() { Code = "ALI", Description = "Allocation Info" },
											AddInfoCollection = new List<AddInfo>()
											{
												new AddInfo() { Key = "AllocationKey", Value = "AK_Value 2" },
												new AddInfo() { Key = "Quantity", Value = "7.9" }
											}
										},
									}
								}
							})))
					})
				}
			};

			var reader = new JobDeclarationDataObjectReader(shipment, logger, Factory);
			var declaration = reader.ReadIntoBusinessObject();
			var invoice = declaration.Invoices.Single();
			var invoiceLine = (BaseJobComInvoiceLine)invoice.InvoiceLines.Single();

			AssertEquals(2, invoiceLine.ComponentInventoryCollection.Count);
			var component1 = (JobComInvLineComponentInventory)invoiceLine.ComponentInventoryCollection.FirstOrDefault();
			var component2 = invoiceLine.ComponentInventoryCollection[1];
			CombineAssertions(() =>
			{
				AssertEquals("JIV_AllocationKey", "AK_Value", component1.JIV_AllocationKey);
				AssertEquals("JIV_QuantityToDraw", 7m, component1.JIV_QuantityToDraw);
				AssertEquals("JIV_AllocationKey", "AK_Value 2", component2.JIV_AllocationKey);
				AssertEquals("JIV_QuantityToDraw", 7.9m, component2.JIV_QuantityToDraw);
			});
		}

		public void TestDeliveryTerms()
		{
			var invoiceData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceData.DeliveryTerms = "Delivery Terms test 1";

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fakeDeclaration.HeaderData;
			var topGroupInvoice = declaration.TopGroupInvoice;
			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			AssertEquals("JZ_AdditionalTerms", "Delivery Terms test 1", invoice.JZ_AdditionalTerms);
		}

		public void TestLoadsForAddingHintsLoadedInBatches()
		{
			var whsFactory = new BusinessObjectFactory();
			var whsHelper = new WhsDataTestHelper(whsFactory);
			var importer = whsHelper.Importer;

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()))
					}
				}
			};

			var invoiceHeader = declarationDataObject.CommercialInfo.CommercialInvoiceCollection[0];

			var numToCreate = 111;

			for (var i = 1; i <= numToCreate; i++)
			{
				invoiceHeader.CommercialInvoiceLineCollection.Add(new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					LineNo = i,
					DataImportMatchingKey = $"DIMK{i}",
					PartNo = $"Part{i}"
				});

				var part = whsHelper.CreateProduct(importer.PK, $"Part{i}");
				var cusClass = whsFactory.New<BaseCusClassPartPivot>();
				cusClass.CI_OP = part.PK;
			}
			whsFactory.Save();

			var uoFactory = new UniversalObjectFactory();
			var boFactory = uoFactory.BOFactory;

			using (boFactory.EnableTableHitQueryCollection(new[]
			{
				MasterFiles.Business.OrgSupplierPart.Schema.TableName,
				BaseCusClassPartPivot.Schema.TableName,
				CusCodeData.Schema.TableName
			}))
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				boFactory.ResetDatabaseLoadCount();
				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, uoFactory);
				var declarationBO = reader.ReadIntoBusinessObject();
				boFactory.ExecuteAllFetchHints();

				CombineAssertions(() =>
				{
					AssertTableHitsWithPartialQuery(boFactory, MasterFiles.Business.OrgSupplierPart.Schema.TableName, 2, $"WHERE ({MasterFiles.Business.OrgSupplierPart.Schema.OP_PartNum} in (");
					AssertTableHitsWithPartialQuery(boFactory, BaseCusClassPartPivot.Schema.TableName, 2, $"WHERE ({BaseCusClassPartPivot.Schema.CI_OP} in (");
					AssertTableHitsWithPartialQuery(boFactory, BaseCusClassPartPivot.Schema.TableName, 2, $"WHERE ({BaseCusClassPartPivot.Schema.CI_CI_Parent} in (");
					AssertTableHitsWithPartialQuery(boFactory, CusCodeData.Schema.TableName, 2, $"WHERE ({CusCodeData.Schema.CY_ParentID} in (");
				});
			}
		}

		void AssertTableHitsWithPartialQuery(BusinessObjectFactory factory, string tableName, int count, string partialQuery)
		{
			var tblHitCount = factory.TableSelects.First(x => x.TableName == tableName);
			AssertNotNull($"Table '{tableName}' not queried", tblHitCount);
			var queries = tblHitCount.Queries.Where(x => x.Query.Contains(partialQuery)).ToList();
			AssertEquals($"Table: '{tableName}' Query Count: {count} Pattern: {partialQuery}", count, queries.Count);
		}

		BaseJobComInvoiceHeader ImportOrganizationAddressHelperFunc(UniversalCustoms.CommercialInvoiceHeader invoiceData, Action<BaseJobComInvoiceHeader> validateOrgAddress)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var topGroupInvoice = ((BaseJobDeclaration)fakeDeclaration.HeaderData).TopGroupInvoice;

			validateOrgAddress.Invoke(invoice);
			AssertEquals("Invoice should have 0 invoice lines", 0, invoice.JobComInvoiceLines.Count);

			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			return invoice;
		}

		BaseJobComInvoiceHeader ImportExporterAddressHelperFunc(UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var topGroupInvoice = ((BaseJobDeclaration)fakeDeclaration.HeaderData).TopGroupInvoice;

			AssertEquals("Invoice should not have Exporter Address set.", ZGuid.Empty, invoice.JZ_OA_ExporterAddress);
			AssertEquals("Invoice should have 0 invoice lines", 0, invoice.JobComInvoiceLines.Count);

			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			return invoice;
		}

		BaseJobComInvoiceHeader ImportConsigneeHelperFunc(UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var topGroupInvoice = ((BaseJobDeclaration)fakeDeclaration.HeaderData).TopGroupInvoice;

			AssertEquals("Invoice should not have Consignee set.", ZGuid.Empty, invoice.JZ_OH_Consignee);
			AssertEquals("Invoice should have 0 invoice lines", 0, invoice.JobComInvoiceLines.Count);

			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			return invoice;
		}

		BaseJobComInvoiceHeader ImportSoldToPartyAddressHelperFunc(UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var topGroupInvoice = ((BaseJobDeclaration)fakeDeclaration.HeaderData).TopGroupInvoice;

			AssertEquals("Invoice should not have SoldToParty Address set.", ZGuid.Empty, invoice.JZ_OA_SoldToPartyAddress);
			AssertEquals("Invoice should have 0 invoice lines", 0, invoice.JobComInvoiceLines.Count);

			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			return invoice;
		}

		BaseJobComInvoiceHeader ImportManufacturerAddressHelperFunc(UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var topGroupInvoice = ((BaseJobDeclaration)fakeDeclaration.HeaderData).TopGroupInvoice;

			AssertEquals("Invoice should not have Manufacturer Address set.", ZGuid.Empty, invoice.JZ_OA_ManufacturerAddress);
			AssertEquals("Invoice should have 0 invoice lines", 0, invoice.JobComInvoiceLines.Count);

			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			return invoice;
		}

		BaseJobComInvoiceHeader ImportConsigneeAddressHelperFunc(UniversalCustoms.CommercialInvoiceHeader invoiceData)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsCommercialInvoice, null);

			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceData })
				}
			};

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = new FakeDeclarationCreatorForInvoice(invoice);
			var topGroupInvoice = ((BaseJobDeclaration)fakeDeclaration.HeaderData).TopGroupInvoice;

			AssertEquals("Invoice should not have Consignee Address set.", ZGuid.Empty, invoice.JZ_OA_ConsigneeAddress);
			AssertEquals("Invoice should have 0 invoice lines", 0, invoice.JobComInvoiceLines.Count);

			var reader = new CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>(invoiceData, Logger, CurrentCompanyHelper, topGroupInvoice);
			reader.ReadIntoBusinessObject(ref invoice, new CommercialInvoiceHeaderRelatedData(shipmentData));

			return invoice;
		}

		void SetupNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(UniversalCustoms.CommercialInvoiceHeader invoiceData, string noteText1, string noteText2)
		{
			invoiceData.NoteCollection = new List<Note>(new[]
				{
					SetupPublicAAACustomNote("DESC 1!!", noteText1),
					SetupPublicAAACustomNote("DESC 2!!", noteText2)
				});
		}

		void AssertNoteCollection_HelpTestStandaloneInvoice_DataForSingleInvoiceOnly(Notes invoiceNotes)
		{
			AssertNotNull(invoiceNotes);
			AssertEquals("invoiceNotes.GetAllNotes().Count", 2, invoiceNotes.GetAllNotes().Count);
			AssertContainsPublicAAACustomNote(invoiceNotes, "DESC 1!!", "DESC 1 - INV1");
			AssertContainsPublicAAACustomNote(invoiceNotes, "DESC 2!!", "DESC 2 - INV1");
		}

		static IDisposable TemporarilySetCountryAndInterfaced(string countryCode)
		{
			IDisposable temporarilySetCountryToChina = null;
			IDisposable setTemporaryLocalCountryCustomsInterfaceValue = null;
			return new DisposableAction(() =>
			{
				temporarilySetCountryToChina = GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode);
				var customsInterface = new LocalCountryCustomsInterface
				{
					RecipientID = "RecipientID",
					SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced
				};
				setTemporaryLocalCountryCustomsInterfaceValue = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			}, () =>
			{
				setTemporaryLocalCountryCustomsInterfaceValue.Dispose();
				temporarilySetCountryToChina.Dispose();
			});
		}

		sealed class JobDeclarationDataObjectReaderSupportPackagePivot : JobDeclarationDataObjectReader
		{
			public JobDeclarationDataObjectReaderSupportPackagePivot(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null) : base(declarationDataObject, logger, factory, forwardingShipment)
			{
			}

			protected override BaseJobDeclaration GetNewBusinessObjectCore() => factory.New<JobDeclarationSupportPackagePivot>();
		}

		sealed class JobDeclarationSupportPackagePivot : BaseJobDeclaration
		{
			public JobDeclarationSupportPackagePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;
		}

		[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
		sealed class AddInfoWithTypeCodeGBTax : TestAddInfo
		{
			public AddInfoWithTypeCodeGBTax(ZPropertyInfo addInfoPropertyInfo)
				: base(addInfoPropertyInfo)
			{
			}
		}
	}
}

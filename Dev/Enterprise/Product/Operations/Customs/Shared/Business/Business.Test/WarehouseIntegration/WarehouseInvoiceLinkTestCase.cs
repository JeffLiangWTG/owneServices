using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WarehouseInvoiceLinkTestCase : WarehouseInvoiceLinkTestCase<BaseJobDeclaration>
	{
	}

	public abstract class WarehouseInvoiceLinkTestCase<T> : TestCaseWithFactory
				where T : BaseJobDeclaration
	{
		public virtual void TestEntryKeyTitle()
		{
			AssertEquals("Entry Key Reference", InvoiceLink.EntryKeyTitle);
		}

		public virtual void TestSetupInvoiceLineFromTransactionLine()
		{
			DummyBondedWarehouseTransactionLine transactionLine = new DummyBondedWarehouseTransactionLine();
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.FillWithValidTestData();
			Declaration.JE_OH_Supplier = supplier.PK;
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.FillWithValidTestData();
			Declaration.JE_OH_Importer = buyer.PK;

			RefCountry country = Factory.New<RefCountry>();
			country.FillWithValidTestData();
			country.Code = "~~";

			MasterFiles.Business.OrgSupplierPart part = MasterFiles.Business.OrgSupplierPart.New(Factory);
			part.OP_PartNum = "~~";
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BaseCusClassification classification = Factory.NewWithValidTestData<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification.CC_RN_NKCountryCode = Declaration.CountryCode;
			classification.CC_LookupCode = "~~";
			classification.CC_TariffNum = "0001.01.01";
			classification.CC_Description = "TESTDESCRIPTION";

			BaseCusClassPartPivot partClassPivot = Factory.NewWithValidTestData<BaseCusClassPartPivot>();
			partClassPivot.CI_OP = part.PK;
			partClassPivot.CI_CC = classification.PK;

			Factory.Save();

			transactionLine.ExposedProduct = part;
			transactionLine.ExposedCustomsQuantity = 99.9m;
			transactionLine.ExposedCustomsQuantityUnit = "XX";
			transactionLine.ExposedQuantity = 10.3m;
			transactionLine.ExposedQuantityUnit = "ZX";
			transactionLine.ExposedValueForDuty = 232.2499m;
			transactionLine.ExposedCountryOfOrigin = country;
			transactionLine.ExposedBondedWarehouseQuantity = 10.3m;
			transactionLine.ExposedBondedWarehouseQuantityUnit = "ZX";
			transactionLine.ExposedPartAttrib1 = "Attrib1";
			transactionLine.ExposedPartAttrib2 = "Attrib2";
			transactionLine.ExposedPartAttrib3 = "Attrib3";
			transactionLine.ExposedSerialNumber = "SerialNumber";

			DummyBondedWarehouseTransaction transaction = new DummyBondedWarehouseTransaction();
			SendsMessagesToCustomsShutterUpperer messages = new SendsMessagesToCustomsShutterUpperer();
			Declaration.MessageInitiator = messages;
			transaction.ExposedLines = GetNewWhsBondedWarehouseTransactionLineCollection(transactionLine);
			transaction.Problems.WarningList.Add("Fill in the cartons and re-sync.");
			InvoiceLink.CreateOrUpdateInvoices(transaction);
			BaseJobComInvoiceLine invoiceLine = Declaration.FilteredInvoiceLines[0];

			AssertEquals("Product code", transactionLine.Product.OP_PartNum, invoiceLine.JI_PartNo);
			AssertEquals("ValueForDuty", 232.25m, invoiceLine.JI_LinePrice);
			AssertEquals("Quantity", transactionLine.Quantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("QuantityUnit", transactionLine.QuantityUnit, invoiceLine.JI_InvoiceUQ);
			AssertEquals("BondedWarehouseQuantity", transactionLine.BondedWarehouseQuantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("BondedWarehouseQuantityUnit", transactionLine.BondedWarehouseQuantityUnit, invoiceLine.JI_InvoiceUQ);
			AssertEquals("CustomsQuantity", transactionLine.CustomsQuantity, invoiceLine.JI_CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", transactionLine.CustomsQuantityUnit, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("CountryOfOrigin", transactionLine.CountryOfOrigin.RN_Code, invoiceLine.JI_CountryOfOrigin);
			AssertEquals("JI_Tariff", "0001.01.01", invoiceLine.JI_Tariff);
			AssertEquals("JI_PartAttrib1", "Attrib1", invoiceLine.JI_PartAttrib1);
			AssertEquals("JI_PartAttrib2", "Attrib2", invoiceLine.JI_PartAttrib2);
			AssertEquals("JI_PartAttrib2", "Attrib3", invoiceLine.JI_PartAttrib3);
			AssertEquals("JI_SerialNumber", "SerialNumber", invoiceLine.JI_SerialNumber);
			AssertEquals("Stock has been released from the bonded warehouse. However, there are some warnings/recommendations:\r\nFill in the cartons and re-sync.", messages.Warning);
		}

		public void TestCreateOrUpdateInvoicesWithExistingLine()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
				DummyBondedWarehouseTransactionLine transactionLine = new DummyBondedWarehouseTransactionLine();
				OrgHeader supplier = OrgHeader.New(Factory);
				supplier.FillWithValidTestData();
				Declaration.JE_OH_Supplier = supplier.PK;
				OrgHeader buyer = OrgHeader.New(Factory);
				buyer.FillWithValidTestData();
				Declaration.JE_OH_Importer = buyer.PK;

				RefCountry country = Factory.New<RefCountry>();
				country.FillWithValidTestData();
				country.Code = "~~";

				MasterFiles.Business.OrgSupplierPart part = MasterFiles.Business.OrgSupplierPart.New(Factory);
				part.OP_PartNum = "~~";
				OrgPartRelation relation = part.RelatedOrganisations.AddNew();
				relation.OU_OH = supplier.PK;
				relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				relation = part.RelatedOrganisations.AddNew();
				relation.OU_OH = buyer.PK;
				relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

				BaseCusClassification classification = Factory.NewWithValidTestData<BaseCusClassification>();
				classification.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
				classification.CC_RN_NKCountryCode = Declaration.CountryCode;
				classification.CC_LookupCode = "~~";
				classification.CC_TariffNum = "0001.01.01";
				classification.CC_Description = "TESTDESCRIPTION";

				BaseCusClassPartPivot partClassPivot = Factory.NewWithValidTestData<BaseCusClassPartPivot>();
				partClassPivot.CI_OP = part.PK;
				partClassPivot.CI_CC = classification.PK;

				transactionLine.ExposedProduct = part;
				transactionLine.ExposedCustomsQuantity = 99.9m;
				transactionLine.ExposedCustomsQuantityUnit = "XX";
				transactionLine.ExposedQuantity = 10.3m;
				transactionLine.ExposedQuantityUnit = "UNT";
				transactionLine.ExposedValueForDuty = 232.2499m;
				transactionLine.ExposedCountryOfOrigin = country;
				transactionLine.ExposedBondedWarehouseQuantity = 10.3m;
				transactionLine.ExposedBondedWarehouseQuantityUnit = "UNT";

				BaseJobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
				BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = transactionLine.Product.OP_PartNum;
				invoiceLine.JI_LinePrice = 232.25m;
				invoiceLine.JI_InvoiceQuantity = transactionLine.Quantity;
				invoiceLine.JI_InvoiceUQ = transactionLine.QuantityUnit;
				invoiceLine.JI_BondedWhsQuantity = transactionLine.BondedWarehouseQuantity;
				invoiceLine.JI_BondedWhsUnitQty = transactionLine.BondedWarehouseQuantityUnit;
				invoiceLine.JI_CustomsQuantity = transactionLine.CustomsQuantity;
				invoiceLine.JI_CustomsUnitQty = transactionLine.CustomsQuantityUnit;
				invoiceLine.JI_CountryOfOrigin = transactionLine.CountryOfOrigin.RN_Code;
				invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);

				Factory.Save();

				partClassPivot.CI_CC = ZGuid.Empty;

				DummyBondedWarehouseTransaction transaction = new DummyBondedWarehouseTransaction();
				SendsMessagesToCustomsShutterUpperer messages = new SendsMessagesToCustomsShutterUpperer();
				Declaration.MessageInitiator = messages;
				transaction.ExposedLines = GetNewWhsBondedWarehouseTransactionLineCollection(transactionLine);
				InvoiceLink.CreateOrUpdateInvoices(transaction);

				AssertEquals("Still 1 line and exception did not occur", 1, Declaration.FilteredInvoiceLines.Count);
			}
		}

		public void TestErrorHandling()
		{
			DummyBondedWarehouseTransactionLine transactionLine = new DummyBondedWarehouseTransactionLine();
			BaseJobComInvoiceLine invoiceLine = Declaration.FilteredInvoiceLines.AddNew();
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			transactionLine.PartAttrib1Problems.ErrorList.Add("BondID1.1");
			transactionLine.PartAttrib1Problems.ErrorList.Add("BondID1.2");
			transactionLine.PartAttrib2Problems.ErrorList.Add("BondID2.1");
			transactionLine.PartAttrib2Problems.ErrorList.Add("BondID2.2");
			transactionLine.PartAttrib3Problems.ErrorList.Add("BondID3.1");
			transactionLine.PartAttrib3Problems.ErrorList.Add("BondID3.2");
			transactionLine.SerialNumberProblems.ErrorList.Add("BondID4.1");
			transactionLine.SerialNumberProblems.ErrorList.Add("BondID4.2");
			transactionLine.EntryKeyProblems.ErrorList.Add("EntryKey1");
			transactionLine.EntryKeyProblems.ErrorList.Add("EntryKey2");
			transactionLine.WarehouseProblems.ErrorList.Add("Warehouse1");
			transactionLine.WarehouseProblems.ErrorList.Add("Warehouse2");
			transactionLine.QuantityProblems.ErrorList.Add("Quantity1");
			transactionLine.QuantityProblems.ErrorList.Add("Quantity2");

			transactionLine.PartAttrib1Problems.WarningList.Add("WBondID1.1");
			transactionLine.PartAttrib1Problems.WarningList.Add("WBondID1.2");
			transactionLine.PartAttrib2Problems.WarningList.Add("WBondID2.1");
			transactionLine.PartAttrib2Problems.WarningList.Add("WBondID2.2");
			transactionLine.PartAttrib3Problems.WarningList.Add("WBondID3.1");
			transactionLine.PartAttrib3Problems.WarningList.Add("WBondID3.2");
			transactionLine.SerialNumberProblems.WarningList.Add("WBondID4.1");
			transactionLine.SerialNumberProblems.WarningList.Add("WBondID4.2");
			transactionLine.EntryKeyProblems.WarningList.Add("WEntryKey1");
			transactionLine.EntryKeyProblems.WarningList.Add("WEntryKey2");
			transactionLine.WarehouseProblems.WarningList.Add("WWarehouse1");
			transactionLine.WarehouseProblems.WarningList.Add("WWarehouse2");
			transactionLine.QuantityProblems.WarningList.Add("WQuantity1");
			transactionLine.QuantityProblems.WarningList.Add("WQuantity2");

			DummyBondedWarehouseTransaction transaction = new DummyBondedWarehouseTransaction();
			transaction.ExposedLines = GetNewWhsBondedWarehouseTransactionLineCollection(transactionLine);

			InvoiceLink.CreateOrUpdateInvoices(transaction);
			AssertEquals("Cannot release stock from the bonded warehouse. Check the errors on lines for details.", ((SendsMessagesToCustomsShutterUpperer)Declaration.MessageInitiator).Warning);

			transaction.Problems.ErrorList.Add("Bad1");
			transaction.Problems.ErrorList.Add("Bad2");
			InvoiceLink.CreateOrUpdateInvoices(transaction);
			AssertEquals("Cannot release stock from the bonded warehouse as:\r\nBad1\r\nBad2", ((SendsMessagesToCustomsShutterUpperer)Declaration.MessageInitiator).Warning);

			AssertEquals("HasError", true, invoiceLine.JI_PartAttrib1Info.HasError("BondID1.1"));
			AssertEquals("HasError", true, invoiceLine.JI_PartAttrib1Info.HasError("BondID1.2"));
			AssertEquals("HasError", true, invoiceLine.JI_PartAttrib2Info.HasError("BondID2.1"));
			AssertEquals("HasError", true, invoiceLine.JI_PartAttrib2Info.HasError("BondID2.2"));
			AssertEquals("HasError", true, invoiceLine.JI_PartAttrib3Info.HasError("BondID3.1"));
			AssertEquals("HasError", true, invoiceLine.JI_PartAttrib3Info.HasError("BondID3.2"));
			AssertEquals("HasError", true, invoiceLine.JI_SerialNumberInfo.HasError("BondID4.1"));
			AssertEquals("HasError", true, invoiceLine.JI_SerialNumberInfo.HasError("BondID4.2"));
			AssertEquals("HasError", true, invoiceLine.JI_AddInfoInfo.HasError("EntryKey1"));
			AssertEquals("HasError", true, invoiceLine.JI_AddInfoInfo.HasError("EntryKey2"));
			AssertEquals("HasError", true, invoiceLine.JI_AddInfoInfo.HasError("Warehouse1"));
			AssertEquals("HasError", true, invoiceLine.JI_AddInfoInfo.HasError("Warehouse2"));
			AssertEquals("HasError", true, invoiceLine.JI_InvoiceQuantityInfo.HasError("Quantity1"));
			AssertEquals("HasError", true, invoiceLine.JI_InvoiceQuantityInfo.HasError("Quantity2"));

			AssertEquals("HasWarning", true, invoiceLine.JI_PartAttrib1Info.HasWarning("WBondID1.1"));
			AssertEquals("HasWarning", true, invoiceLine.JI_PartAttrib1Info.HasWarning("WBondID1.2"));
			AssertEquals("HasWarning", true, invoiceLine.JI_PartAttrib2Info.HasWarning("WBondID2.1"));
			AssertEquals("HasWarning", true, invoiceLine.JI_PartAttrib2Info.HasWarning("WBondID2.2"));
			AssertEquals("HasWarning", true, invoiceLine.JI_PartAttrib3Info.HasWarning("WBondID3.1"));
			AssertEquals("HasWarning", true, invoiceLine.JI_PartAttrib3Info.HasWarning("WBondID3.2"));
			AssertEquals("HasWarning", true, invoiceLine.JI_SerialNumberInfo.HasWarning("WBondID4.1"));
			AssertEquals("HasWarning", true, invoiceLine.JI_SerialNumberInfo.HasWarning("WBondID4.2"));
			AssertEquals("HasWarning", true, invoiceLine.JI_AddInfoInfo.HasWarning("WEntryKey1"));
			AssertEquals("HasWarning", true, invoiceLine.JI_AddInfoInfo.HasWarning("WEntryKey2"));
			AssertEquals("HasWarning", true, invoiceLine.JI_AddInfoInfo.HasWarning("WWarehouse1"));
			AssertEquals("HasWarning", true, invoiceLine.JI_AddInfoInfo.HasWarning("WWarehouse2"));
			AssertEquals("HasWarning", true, invoiceLine.JI_InvoiceQuantityInfo.HasWarning("WQuantity1"));
			AssertEquals("HasWarning", true, invoiceLine.JI_InvoiceQuantityInfo.HasWarning("WQuantity2"));
		}

		public void TestIsExWarehouse()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("IsExWarehouse", true, InvoiceLink.IsExWarehouse);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExWarehouse", false, InvoiceLink.IsExWarehouse);
		}

		public void TestDeclarationPK()
		{
			AssertEquals(Declaration.PK, InvoiceLink.DeclarationPK);
		}

		public void TestOwnersReference()
		{
			Declaration.JE_OwnerRef = "ABCDEF";
			AssertEquals(Declaration.JE_OwnerRef, InvoiceLink.OwnersReference);
		}

		public void TestImporter()
		{
			Declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			AssertEquals(Declaration.Importer, InvoiceLink.Importer);
		}

		public void TestCreateOrUpdateInvoicesUpdatesInvoicesRebuildsAutoLinesAndUsesExistingHeaderOrCreatesIfNone()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddYears(1).ToDateTime());
			Declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("One default header", 1, Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			BaseJobComInvoiceHeader header1 = Declaration.Invoices[0];
			BaseJobComInvoiceLine nonAutoLine = Declaration.FilteredInvoiceLines.AddNew();
			BaseJobComInvoiceLine autoLine = Declaration.FilteredInvoiceLines.AddNew();
			autoLine.SetUseBondedWarehouseAutomationForTesting(true);
			nonAutoLine.SetUseBondedWarehouseAutomationForTesting(false);
			WarehouseTransaction.ExposedLines = GetNewWhsBondedWarehouseTransactionLineCollection();
			InvoiceLink.CreateOrUpdateInvoices(WarehouseTransaction);
			AssertEquals("Non auto line survived", 1, Declaration.FilteredInvoiceLines.Count);
			AssertEquals("Non auto line survived", nonAutoLine, Declaration.FilteredInvoiceLines[0]);
			AssertEquals("One default group header", 1, Declaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("One default header", 1, Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals("One default header", header1, Declaration.Invoices[0]);

			header1.Delete();
			AssertEquals("No header", 0, Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			InvoiceLink.CreateOrUpdateInvoices(WarehouseTransaction);
			AssertEquals("One default header", 1, Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
		}

		public void TestCreateOrUpdateInvoicesCreatesOneHeaderAndOneInvoiceLineForEveryTransactionLine()
		{
			int count = 7;
			WarehouseTransaction.ExposedLines = CreateLines(count);
			InvoiceLink.CreateOrUpdateInvoices(WarehouseTransaction);

			AssertEquals("Lines", count, Declaration.FilteredInvoiceLines.Count);
			AssertEquals("One default group header", 1, Declaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("Header", 1, Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
		}

		public void TestCreateOrUpdateInvoicesCreatesHeaderWithCorrectTotal()
		{
			WarehouseTransaction.ExposedLines = CreateLines(2);
			((DummyBondedWarehouseTransactionLine)WarehouseTransaction.ExposedLines[0]).ExposedValueForDuty = 100.1m;
			((DummyBondedWarehouseTransactionLine)WarehouseTransaction.ExposedLines[1]).ExposedValueForDuty = 200.2m;
			InvoiceLink.CreateOrUpdateInvoices(WarehouseTransaction);
			AssertEquals("Header InvoiceAmount", 300.3m, Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_InvoiceAmount);
			AssertEquals("Header Currency", Declaration.LocalCurrencyCode, Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_RX_NKInvoice_Currency);
		}

		public void TestDeleteAutomationInvoiceLinesNotInMap()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var org = Factory.New<OrgHeader>();
				var warehouse = Factory.New<IWhsWarehouse>();
				warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
				Declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				Declaration.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
				Declaration.SetSupportsBondedWarehousingForTesting(true);
				var nonAutoLine = Declaration.FilteredInvoiceLines.AddNew();
				var autoLine = Declaration.FilteredInvoiceLines.AddNew();
				autoLine.SetUseBondedWarehouseAutomationForTesting(true);
				nonAutoLine.SetUseBondedWarehouseAutomationForTesting(false);
				WarehouseTransaction.ExposedLines = GetNewWhsBondedWarehouseTransactionLineCollection();
				InvoiceLink.CreateOrUpdateInvoices(WarehouseTransaction);
				AssertEquals("Non auto line survived", 1, Declaration.FilteredInvoiceLines.Count);
				AssertEquals("Non auto line survived", nonAutoLine, Declaration.FilteredInvoiceLines[0]);
			}
		}

		protected IWhsBondedWarehouseTransactionLineCollection CreateLines(int numberLinesToCreate)
		{
			IWhsBondedWarehouseTransactionLineCollection lines = GetNewWhsBondedWarehouseTransactionLineCollection();
			for (int i = 0; i < numberLinesToCreate; i++)
			{
				DummyBondedWarehouseTransactionLine line = new DummyBondedWarehouseTransactionLine();
				line.ExposedQuantity = 5;
				lines.Add(line);
			}
			return lines;
		}

		public void TestCreateOrUpdateInvoicesCallsSetupInvoiceLineFromTransactionLine()
		{
			DummyWarehouseInvoiceLink link = new DummyWarehouseInvoiceLink(Declaration);
			WarehouseTransaction.ExposedLines = CreateLines(9);
			link.CreateOrUpdateInvoices(WarehouseTransaction);
			AssertEquals("Calls to SetupInvoiceLineFromTransactionLine", 9, link.SetupInvoiceLineCount);
		}

		#region DummyWarehouseInvoiceLink

		class DummyWarehouseInvoiceLink : WarehouseInvoiceLink
		{
			public DummyWarehouseInvoiceLink(BaseJobDeclaration dec) : base(dec)
			{ }

			public int SetupInvoiceLineCount;
			protected override void SetupInvoiceLineFromTransactionLine(IWhsBondedWarehouseTransactionLine transactionLine, BaseJobComInvoiceLine invoiceLine)
			{
				SetupInvoiceLineCount++;
				base.SetupInvoiceLineFromTransactionLine(transactionLine, invoiceLine);
			}
		}

		#endregion

		#region DummyBondedWarehouseTransaction

		public class DummyBondedWarehouseTransaction : IWhsBondedWarehouseTransaction
		{
			#region IBondedWarehouseTransaction Members

			public bool HasErrors
			{
				get { return Problems.ErrorList.Count > 0 || LinesHaveErrors; }
			}

			bool LinesHaveErrors
			{
				get
				{
					foreach (IWhsBondedWarehouseTransactionLine line in Lines)
					{
						if (line.QuantityProblems.ErrorList.Count > 0
							|| line.WarehouseProblems.ErrorList.Count > 0
							|| line.PartAttrib1Problems.ErrorList.Count > 0
							|| line.PartAttrib2Problems.ErrorList.Count > 0
							|| line.SerialNumberProblems.ErrorList.Count > 0)
						{
							return true;
						}
					}
					return false;
				}
			}

			public IOrgHeader TransportCompany
			{
				get
				{
					// TODO:  Add NoLinesTransaction.TransportCompany getter implementation
					return null;
				}
			}

			public NotificationCollection Problems
			{
				get
				{
					if (fProblems == null)
					{
						fProblems = new NotificationCollection();
					}

					return fProblems;
				}
			}
			NotificationCollection fProblems;

			public IWhsBondedWarehouseTransactionLineCollection ExposedLines = ObjectFactory.Get<IWhsBondedWarehouseTransactionLineCollection>();

			public IWhsBondedWarehouseTransactionLineCollection Lines
			{
				get
				{
					return ExposedLines;
				}
			}

			IWhsWarehouseTransactionLineCollection IWhsWarehouseTransaction.Lines
			{
				get { return Lines; }
			}

			public bool IsWarehousedByExternalAgentExposed;
			public bool IsWarehousedByExternalAgent { get { return IsWarehousedByExternalAgentExposed; } }

			public ZDateTime Date
			{
				get
				{
					// TODO:  Add NoLinesTransaction.ArrivalDate getter implementation
					return new ZDateTime();
				}
			}

			public ZDateTime ArrivalDate
			{
				get { return Date; }
			}

			public IOrgAddress Warehouse
			{
				get
				{
					// TODO:  Add NoLinesTransaction.Warehouse getter implementation
					return null;
				}
			}

			public IOrgHeader Client
			{
				get
				{
					// TODO:  Add NoLinesTransaction.Client getter implementation
					return null;
				}
			}

			public IEnumerable<AdditionalReference> AdditionalReferences
			{
				get
				{
					// TODO:  Add NoLinesTransaction.AdditionalReferences getter implementation
					return null;
				}
			}

			public ZString Reference
			{
				get
				{
					// TODO:  Add NoLinesTransaction.DeclarationReference getter implementation
					return ZString.Empty;
				}
			}

			public ZString DeclarationReference
			{
				get { return Reference; }
			}

			public ZGuid ExternalPK
			{
				get { return ZGuid.Empty; }
			}

			public ZGuid DeclarationPK
			{
				get { return ExternalPK; }
			}

			public ZString JobNumber
			{
				get
				{
					// TODO:  Add NoLinesTransaction.DeclarationReference getter implementation
					return ZString.Empty;
				}
			}

			bool IWhsWarehouseTransaction.HasWarnings
			{
				//  TODO:  Complete this
				get { return false; }
			}

			#endregion
		}

		#endregion

		#region DummyBondedWarehouseTransactionLine

		public class DummyBondedWarehouseTransactionLine : BondedWarehouseLineProblemProvider, IWhsBondedWarehouseTransactionLine
		{
			#region IBondedWarehouseTransactionLine Members

			public ZGuid UniqueKey
			{
				get { return ZGuid.Empty; }
			}

			public ZString PartAttrib1
			{
				get { return ExposedPartAttrib1; }
			}
			public ZString ExposedPartAttrib1;

			public ZString PartAttrib2
			{
				get { return ExposedPartAttrib2; }
			}
			public ZString ExposedPartAttrib2;

			public ZString PartAttrib3
			{
				get { return ExposedPartAttrib3; }
			}
			public ZString ExposedPartAttrib3;

			public ZString SerialNumber
			{
				get { return ExposedSerialNumber; }
			}
			public ZString ExposedSerialNumber;

			public ZDecimal CustomsQuantity
			{
				get { return ExposedCustomsQuantity; }
			}
			public ZDecimal ExposedCustomsQuantity;

			public ZString CustomsQuantityUnit
			{
				get { return ExposedCustomsQuantityUnit; }
			}
			public ZString ExposedCustomsQuantityUnit;

			public ZDecimal CustomsSecondQuantity
			{
				get { return ExposedCustomsSecondQuantity; }
			}
			public ZDecimal ExposedCustomsSecondQuantity;

			public ZString CustomsSecondQuantityUnit
			{
				get { return ExposedCustomsSecondQuantityUnit; }
			}
			public ZString ExposedCustomsSecondQuantityUnit;

			public ZDecimal CustomsThirdQuantity
			{
				get { return ExposedCustomsThirdQuantity; }
			}
			public ZDecimal ExposedCustomsThirdQuantity;

			public ZString CustomsThirdQuantityUnit
			{
				get { return ExposedCustomsThirdQuantityUnit; }
			}
			public ZString ExposedCustomsThirdQuantityUnit;

			public ZDecimal BondedWarehouseQuantity
			{
				get { return ExposedBondedWarehouseQuantity; }
			}
			public ZDecimal ExposedBondedWarehouseQuantity;

			public ZString BondedWarehouseQuantityUnit
			{
				get { return ExposedBondedWarehouseQuantityUnit; }
			}
			public ZString ExposedBondedWarehouseQuantityUnit;

			public ZString AddInfo
			{
				get
				{
					// TODO:  Add DummyBondedWarehouseTransactionLine.AddInfo getter implementation
					return new ZString();
				}
			}

			public ZShort OriginalEntryLineNumber
			{
				get
				{
					// TODO:  Add DummyBondedWarehouseTransactionLine.OriginalEntryLineNumber getter implementation
					return new ZShort();
				}
			}

			public ZShort ExposedEntryLineNumber;
			public ZShort EntryLineNumber
			{
				get
				{
					return ExposedEntryLineNumber;
				}
			}

			public ZDecimal ExposedValueForDuty;
			public ZDecimal ValueForDuty
			{
				get { return ExposedValueForDuty; }
			}

			public Money ExposedTILV;
			public IMoney TILV
			{
				get { return ExposedTILV; }
			}

			public ZDecimal ExposedQuantity;
			public ZDecimal Quantity
			{
				get
				{
					return ExposedQuantity;
				}
			}

			public ZDateTime EntryDate
			{
				get
				{
					// TODO:  Add DummyBondedWarehouseTransactionLine.EntryDate getter implementation
					return new ZDateTime();
				}
			}

			public RefCountry ExposedCountryOfOrigin;
			public IRefCountry CountryOfOrigin
			{
				get
				{
					return ExposedCountryOfOrigin;
				}
			}

			public MasterFiles.Business.OrgSupplierPart ExposedProduct;
			public IOrgSupplierPart Product
			{
				get
				{
					return ExposedProduct;
				}
			}

			public ZString OriginalEntryKey
			{
				get
				{
					return new ZString();
				}
			}

			public ZString ExposedEntryKey;
			public ZString EntryKey
			{
				get
				{
					return ExposedEntryKey;
				}
			}

			public ZString ExposedQuantityUnit;
			public ZString QuantityUnit
			{
				get
				{
					return ExposedQuantityUnit;
				}
			}

			public OrgAddress ExposedWarehouse;
			public IOrgAddress Warehouse
			{
				get { return ExposedWarehouse; }
			}

			NotificationCollection IWhsBondedWarehouseTransactionLine.BondedWarehouseQuantityProblems
			{
				//  TODO:  Complete this
				get { return null; }
			}

			bool IWhsWarehouseTransactionLine.HasWarnings
			{
				//  TODO:  Complete this
				get { return false; }
			}

			#endregion
		}

		#endregion

		public void TestNotifyChangedToFromExWarehousingFiresEvent()
		{
			eventCalledCount = 0;
			InvoiceLink.IsExWarehouseChanged += new IsExWarehouseChangedEvent(InvoiceLink_OnEnabledChanged);
			AssertEquals("Event not called yet", 0, eventCalledCount);
			InvoiceLink.NotifyChangedToFromExWarehousing();
			AssertEquals("EventCalledCount", 1, eventCalledCount);
			InvoiceLink.NotifyChangedToFromExWarehousing();
			AssertEquals("EventCalledCount", 2, eventCalledCount);
		}

		void InvoiceLink_OnEnabledChanged()
		{
			eventCalledCount++;
		}
		int eventCalledCount;

		[ExpectNoExceptions]
		public void TestNotifyEnabledChangedDoesNotThrowExceptionWhenEventIsNotHooked()
		{
			InvoiceLink.NotifyChangedToFromExWarehousing();
		}

		protected DummyBondedWarehouseTransaction WarehouseTransaction
		{
			get
			{
				if (fWarehouseTransaction == null)
				{
					fWarehouseTransaction = new DummyBondedWarehouseTransaction();
				}

				return fWarehouseTransaction;
			}
		}
		DummyBondedWarehouseTransaction fWarehouseTransaction;

		protected T Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = MockDeclaration.Object;
					PopulateDeclaration(fDeclaration);
				}
				return fDeclaration;
			}
		}
		T fDeclaration;

		protected virtual void PopulateDeclaration(T declaration)
		{
		}

		protected Mock<T> MockDeclaration
		{
			get
			{
				if (mockDeclaration == null)
				{
					mockDeclaration = Factory.NewMoq<T>();
					mockDeclaration.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(false);
				}
				return mockDeclaration;
			}
		}
		Mock<T> mockDeclaration;

		protected WarehouseInvoiceLink InvoiceLink
		{
			get
			{
				if (fInvoiceLink == null)
				{
					fInvoiceLink = GetNewInvoiceLink();
				}

				return fInvoiceLink;
			}
		}
		WarehouseInvoiceLink fInvoiceLink;

		protected virtual WarehouseInvoiceLink GetNewInvoiceLink()
		{
			return new WarehouseInvoiceLink(Declaration);
		}

		IWhsBondedWarehouseTransactionLineCollection GetNewWhsBondedWarehouseTransactionLineCollection(params IWhsBondedWarehouseTransactionLine[] lines)
		{
			Type type = ObjectFactory.GetType<IWhsBondedWarehouseTransactionLineCollection>();
			var collection = (IWhsBondedWarehouseTransactionLineCollection)Activator.CreateInstance(type);
			collection.AddRange(lines);

			return collection;
		}
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class BondedWarehouseTransactionLineTest : TestCaseWithFactory
	{
		public void TestUniqueKey()
		{
			// James TODO
			Assert(true);
			//AssertEquals(Creator.InvoiceLine1.PK, Creator.Entry1TransactionLine.UniqueKey);
		}

		public void TestWarehouse()
		{
			AssertEquals(Creator.Entry1TransactionLine.Warehouse, Creator.Declaration.WarehouseDocAddress.Address);
		}

		public void TestTILV()
		{
			AssertEquals("Default value", Money.Empty, Creator.Entry1TransactionLine.TILV);
		}

		public void TestPartAttribute1()
		{
			Creator.InvoiceLine1.JI_PartAttrib1 = "Attrib1";
			AssertEquals("BondID1", "Attrib1", Creator.Entry1TransactionLine.PartAttrib1);
		}

		public void TestPartAttribute2()
		{
			Creator.InvoiceLine1.JI_PartAttrib2 = "Attrib2";
			AssertEquals("BondID2", "Attrib2", Creator.Entry1TransactionLine.PartAttrib2);
		}

		public void TestPartAttribute3()
		{
			Creator.InvoiceLine1.JI_PartAttrib3 = "Attrib3";
			AssertEquals("BondID3", "Attrib3", Creator.Entry1TransactionLine.PartAttrib3);
		}

		public void TestSerialNumber()
		{
			Creator.InvoiceLine1.JI_SerialNumber = "SerialNumber";
			AssertEquals("BondID4", "SerialNumber", Creator.Entry1TransactionLine.SerialNumber);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestGetEntryDateForWEANeedsToBeOverriden()
		{
			var creator = new MergedDeclarationCreator<BaseJobDeclaration>(Factory, DeclarationApplicationCodeList.Codes.Builtin);
			creator.Declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			BondedWarehouseTransactionLine line = new BondedWarehouseTransactionLine(creator.EntryLine1);
			object x = line.EntryDate;
		}

		[ExpectNoExceptions()]
		public void TestMergedEntriesSupported()
		{
			var creator = new MergedDeclarationCreator2Line<BaseJobDeclaration>(Factory, DeclarationApplicationCodeList.Codes.Builtin);
			creator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			creator.Declaration.DoMerge();
			new BondedWarehouseTransactionLine(creator.EntryLine1);
		}

		public void TestAddInfo()
		{
			var mock = new Mock<BondedWarehouseTransactionLine>(Creator.EntryLine1);
			IWhsBondedWarehouseTransactionLine line = mock.Object;
			AssertEquals("Default addinfo", "", line.AddInfo);
			mock.Protected().Setup<ZString>("AddInfoString").Returns(new ZString("noodle"));
			AssertEquals("Calls through to AddInfoString", "noodle", line.AddInfo);
		}

		public void TestCountryOfOrigin()
		{
			Creator.InvoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, Creator.Entry1TransactionLine.CountryOfOrigin.RN_Code);
		}

		public void TestCustomsQuantity()
		{
			AssertEquals("IWhsBondedWarehouseTransactionUnits", 100.1m, Creator.Entry1TransactionLine.CustomsQuantity);
		}

		public void TestCustomsQuantityUnit()
		{
			Creator.InvoiceLine1.JI_CustomsUnitQty = "CU";
			AssertEquals("CU", Creator.Entry1TransactionLine.CustomsQuantityUnit);
		}

		public void TestCustomsSecondQuantity()
		{
			AssertEquals("CustomsSecondQuantity", 10.1m, Creator.Entry1TransactionLine.CustomsSecondQuantity);
		}

		public void TestCustomsSecondQuantityUnit()
		{
			Creator.InvoiceLine1.JI_CustomsSecondUnitQty = "CU";
			AssertEquals("CU", Creator.Entry1TransactionLine.CustomsSecondQuantityUnit);
		}

		public void TestCustomsThirdQuantity()
		{
			AssertEquals("CustomsThirdQuantity", 1.1m, Creator.Entry1TransactionLine.CustomsThirdQuantity);
		}

		public void TestCustomsThirdQuantityUnit()
		{
			Creator.InvoiceLine1.JI_CustomsThirdUnitQty = "CU";
			AssertEquals("CU", Creator.Entry1TransactionLine.CustomsThirdQuantityUnit);
		}

		public void TestQuantity()
		{
			AssertEquals("IWhsBondedWarehouseTransactionUnits", 200.2m, Creator.Entry1TransactionLine.Quantity);
		}

		public void TestBondedQuantityUnit()
		{
			Creator.InvoiceLine1.JI_InvoiceUQ = "AB";
			AssertEquals("AB", Creator.Entry1TransactionLine.BondedWarehouseQuantityUnit);
		}

		public void TestBondedWarehouseQuantityQuantity()
		{
			Creator.InvoiceLine1.JI_InvoiceQuantity = 123.3m;
			AssertEquals("IWhsBondedWarehouseTransactionUnits", 123.3m, Creator.Entry1TransactionLine.BondedWarehouseQuantity);
		}

		public void TestQuantityUnit()
		{
			Creator.InvoiceLine1.JI_InvoiceUQ = "UU";
			AssertEquals("UU", Creator.Entry1TransactionLine.QuantityUnit);
		}

		public void TestPart()
		{
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.FillWithValidTestData();
			buyer.OH_Code = "C~S";

			Creator.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Creator.Declaration.JE_OH_Importer = buyer.PK;

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "C~S";
			part.RelatedOrganisations.AddOrganisationIfNotExist(buyer.PK, OrgPartRelation.RelationshipTypes.Owner);

			Creator.InvoiceLine1.JI_PartNo = "C~S";
			Factory.Save();

			AssertNotNull("Part loaded", Creator.InvoiceLine1.Part);
			AssertEquals("Correct part returned from interface", part, Creator.Entry1TransactionLine.Product);
		}

		public void TestEntryLineNmber()
		{
			Creator.EntryLine1.CL_LineNumber = 33;
			AssertEquals((short)33, Creator.Entry1TransactionLine.EntryLineNumber);
		}

		public void TestDutiableAmountInLocalCurrency()
		{
			Creator.InvoiceLine1.JI_LinePrice = 123.1m;
			AssertEquals(123.1m, Creator.Entry1TransactionLine.ValueForDuty);
		}

		public void TestKey()
		{
			Creator.Entry1.EntryNumber = "XXX";
			AssertEquals("XXX", Creator.Entry1TransactionLine.EntryKey);
		}

		public void TestEntryDate()
		{
			Factory.AllowMultipleBusinessObjectsAroundOneRow = false;
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			var mockEntryHeader = Factory.NewMoq<CusEntryHeader>();
			mockEntryHeader.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2005, 8, 23));

			CusEntryHeader entry = mockEntryHeader.Object;
			var mockEntryLine = Factory.NewMoq<CusEntryLine>();
			mockEntryLine.Setup(m => m.Header).Returns(entry);
			CusEntryLine entryLine = mockEntryLine.Object;

			var mock1 = Factory.NewMoq<BaseJobComInvoiceLine>();
			mock1.Object.JI_CL = entryLine.PK;

			AssertEquals(new ZDateTime(2005, 8, 23), ((IBondedWarehouseTransactionLineProvider)entryLine).TransactionLine.EntryDate);
		}

		WhsIntegrationMergedDecCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new WhsIntegrationMergedDecCreator(Factory, DeclarationApplicationCodeList.Codes.Builtin);
				}

				return fCreator;
			}
		}
		WhsIntegrationMergedDecCreator fCreator;

		protected class WhsIntegrationMergedDecCreator : MergedDeclarationCreator<BaseJobDeclaration>
		{
			public WhsIntegrationMergedDecCreator(BusinessObjectFactory factory) : base(factory)
			{
			}

			public WhsIntegrationMergedDecCreator(BusinessObjectFactory factory, string applicationCode)
			: base(factory, applicationCode)
			{
			}

			public IWhsBondedWarehouseTransactionLine Entry1TransactionLine
			{
				get { return ((IBondedWarehouseTransactionLineProvider)EntryLine1).TransactionLine; }
			}

			protected override void SetupDeclarationPreMerge(BaseJobDeclaration declaration)
			{
				base.SetupDeclarationPreMerge(declaration);
				fDeclaration.FilteredInvoiceLines[0].SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				fDeclaration.FilteredInvoiceLines[0].JI_InvoiceQuantity = 200.2m;
				fDeclaration.FilteredInvoiceLines[0].JI_CustomsQuantity = 100.1m;
				fDeclaration.FilteredInvoiceLines[0].JI_CustomsSecondQuantity = 10.1m;
				fDeclaration.FilteredInvoiceLines[0].JI_CustomsThirdQuantity = 1.1m;
			}
		}
	}
}

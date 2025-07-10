using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportCustomsDeclarationDocumentWrapper))]
	abstract class ExportCustomsDeclarationDocumentWrapperAbstractTest<TExportCustomsDeclarationDocumentWrapper> : NonPersistentBusinessObjectTestCase
		where TExportCustomsDeclarationDocumentWrapper : ExportCustomsDeclarationDocumentWrapper
	{
		OrgHeader supplier;
		OrgHeader buyer;
		GlbStaff broker;
		RefVessel vessel1;
		RefCurrency USDCurrency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
		RefCurrency JPYCurrency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Japan);

		protected JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		protected CusEntryHeader entryHeader;

		[ExpectNoExceptions]
		public void TestDeclarationID()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "AA  07094AD515";
			entryHeader.CH_Status = "AWO";
			NUnit.Framework.Assert.That(wrapper.DeclarationID, NUnit.Framework.Is.EqualTo("AA  07094AD515").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDeclarationIDFormatted()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DeclarationIDFormatted, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryHeader.EntryNumber = "AA  07094AD515";
			entryHeader.CH_Status = "AWO";
			NUnit.Framework.Assert.That(wrapper.DeclarationIDFormatted, NUnit.Framework.Is.EqualTo("AA/  /07/094/AD515").Using(CustomComparers.TypeComparison));
			entryHeader.EntryNumber = "AA";
			NUnit.Framework.Assert.That(wrapper.DeclarationIDFormatted, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.EntryNumber = "AB  07094AD515";
			NUnit.Framework.Assert.That(wrapper.DeclarationIDFormatted, NUnit.Framework.Is.EqualTo("AB/  /07/094/AD515").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupSupplier();
			SetupBuyer();
			SetupBroker();
			SetUpGoodsLocation();
			CurrencyConverterTestHelper.SetExchangeRate(Factory, USDCurrency, 0.04m, new ZDateTime(2019, 8, 30), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, JPYCurrency, 4m, new ZDateTime(2019, 8, 30), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			SetupCustomsProcedure();
			SetupVessel();
			Factory.Save();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			entryInstruction = declaration.CusEntryInstruction;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
		}

		[ExpectNoExceptions]
		public void TestGoodsItemListSections_DashDisplayWhenAllGroupingsAreSame()
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceLine1.JI_Group = "";
			invoiceLine2.JI_Group = "";
			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(12));
			NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));

			invoiceLine1.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2  ";
			invoiceLine2.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2  ";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(12));

			CombineAssertions("grouping for the first line", () =>
			{
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			});

			CombineAssertions("grouping for the second line", () =>
			{
				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[6].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[7].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[8].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			});

			invoiceLine1.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2 is intentionally made to be very loooooooooooooooooooooooooooooooooong  ";
			invoiceLine2.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2 is intentionally made to be very loooooooooooooooooooooooooooooooooong  ";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(15));
			CombineAssertions("grouping for the first line", () =>
			{
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[0].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2 is intentionally made").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("to be very").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("loooooooooooooooooooooooooooooooooong").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[6].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[6].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(false));
			});

			CombineAssertions("grouping for the second line", () =>
			{
				NUnit.Framework.Assert.That(sections[8].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 1").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[8].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(sections[9].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping Line 2 is intentionally made").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[10].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("to be very").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[11].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("loooooooooooooooooooooooooooooooooong").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[12].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[13].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[14].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[14].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(false));
			});

			invoiceLine1.JI_Group = "  This grouping is specially designed so that it spans into exactly three lines  ";
			invoiceLine2.JI_Group = "  This grouping is specially designed so that it spans into exactly three lines  ";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(12));

			CombineAssertions("grouping for the first line", () =>
			{
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[0].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("This grouping is specially designed so").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("that it spans into exactly three lines").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[4].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(false));

				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			});

			CombineAssertions("grouping for the first line", () =>
			{
				NUnit.Framework.Assert.That(sections[6].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("This grouping is specially designed so").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[6].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(sections[7].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("that it spans into exactly three lines").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[8].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[9].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(sections[10].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(sections[10].ShowEvenRowIsEmpty, NUnit.Framework.Is.EqualTo(false));

				NUnit.Framework.Assert.That(sections[11].Box33DescriptionOfGoods_Line2.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestGoodsItemListSections_DashDisplayWhenGroupingsAreDifferent()
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceLine1.JI_Group = "";
			invoiceLine2.JI_Group = "  Grouping  ";
			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(12), "sections.Length");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[0].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box33DescriptionOfGoods_Line3 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[2].Box33DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[4].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box33DescriptionOfGoods_Line3 - should be [null] or [empty]");
			});

			entryLine1.CL_Description = "Entry Line 1-1\r\nEntry Line 1-2\r\nEntry Line 1-3";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(10), "sections.Length");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1-1").Using(CustomComparers.TypeComparison), "sections[0].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("Entry Line 1-2").Using(CustomComparers.TypeComparison), "sections[0].Box33DescriptionOfGoods_Line3");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("Entry Line 1-3").Using(CustomComparers.TypeComparison), "sections[0].Box33DescriptionOfGoods_Line4");

				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[1].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[2].Box33DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[4].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box33DescriptionOfGoods_Line3 - should be [null] or [empty]");
			});

			entryLine1.CL_Description = "Entry Line 1";
			invoiceLine1.JI_Group = "  Grouping  ";
			invoiceLine2.JI_Group = "";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(12), "sections.Length");

				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[1].Box33DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[3].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box33DescriptionOfGoods_Line3 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[4].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box33DescriptionOfGoods_Line3 - should be [null] or [empty]");
			});

			invoiceLine1.JI_Group = "  Grouping  ";
			invoiceLine2.JI_Group = "  Another Grouping  ";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections.Length, NUnit.Framework.Is.EqualTo(12), "sections.Length");

				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[0].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Grouping").Using(CustomComparers.TypeComparison), "sections[1].Box33DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[2].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 1").Using(CustomComparers.TypeComparison), "sections[3].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[3].Box33DescriptionOfGoods_Line3 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[4].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");
				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Another Grouping").Using(CustomComparers.TypeComparison), "sections[5].Box33DescriptionOfGoods_Line1");
				NUnit.Framework.Assert.That(sections[6].Box33DescriptionOfGoods_Line1.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[6].Box33DescriptionOfGoods_Line1 - should be [null] or [empty]");

				NUnit.Framework.Assert.That(sections[7].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line 2").Using(CustomComparers.TypeComparison), "sections[7].Box33DescriptionOfGoods_Line2");
				NUnit.Framework.Assert.That(sections[7].Box33DescriptionOfGoods_Line3.ToString(), NUnit.Framework.Is.Null.Or.Empty, "sections[7].Box33DescriptionOfGoods_Line3 - should be [null] or [empty]");
			});
		}

		[ExpectNoExceptions]
		public void TestFirstPageGoodsItemListSections()
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "ACR";
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2m;
			invoiceLine2.JI_InvoiceUQ = "AMH";
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceLine1.JI_Group = "";
			invoiceLine2.JI_Group = "";
			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(11));
				NUnit.Framework.Assert.That(GetAllSectionsDetail(), NUnit.Framework.Is.EqualTo(@"Entry Line 1



Entry Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsCode(), NUnit.Framework.Is.EqualTo(@"...-



...-



Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsWeightAndQuantity(), NUnit.Framework.Is.EqualTo(@"0 KGM
1 ACR


0 KGM
2 AMH


----------------------
0 KGM
1 ACR
2 AMH
vvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsFOBValue(), NUnit.Framework.Is.EqualTo(@"<Currency(0,TWD)>



<Currency(0,TWD)>



----------------------
<Currency(0,TWD)>
vvvvvvvvvv").Using(CustomComparers.TypeComparison));

				invoiceLine1.JI_Group = "  Grouping Line 11 ";
				invoiceLine2.JI_Group = "  Grouping Line 21 ";
				wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(11));
				NUnit.Framework.Assert.That(GetAllSectionsDetail(), NUnit.Framework.Is.EqualTo(@"Grouping Line 11








Entry Line 1






Grouping Line 21








Entry Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsCode(), NUnit.Framework.Is.EqualTo(@"...-















...-



Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsWeightAndQuantity(), NUnit.Framework.Is.EqualTo(@"0 KGM
1 ACR














0 KGM
2 AMH


----------------------
0 KGM
1 ACR
2 AMH
vvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsFOBValue(), NUnit.Framework.Is.EqualTo(@"<Currency(0,TWD)>















<Currency(0,TWD)>



----------------------
<Currency(0,TWD)>
vvvvvvvvvv").Using(CustomComparers.TypeComparison));

				invoiceLine1.JI_Group = "  Grouping Line 11  \r\n  Grouping Line 12 \r\n  Grouping Line 13 ";
				wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				firstPageSections = wrapper.FirstPageGoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

				NUnit.Framework.Assert.That(firstPageSections.Length, NUnit.Framework.Is.EqualTo(14));
				NUnit.Framework.Assert.That(GetAllSectionsDetail(), NUnit.Framework.Is.EqualTo(@"Grouping Line 11



Grouping Line 12



Grouping Line 13








Entry Line 1






Grouping Line 21








Entry Line 2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsCode(), NUnit.Framework.Is.EqualTo(@"...-















...-").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsWeightAndQuantity(), NUnit.Framework.Is.EqualTo(@"0 KGM
1 ACR














0 KGM
2 AMH").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsFOBValue(), NUnit.Framework.Is.EqualTo(@"<Currency(0,TWD)>















<Currency(0,TWD)>").Using(CustomComparers.TypeComparison));
			});

			ZString GetAllSectionsDetail()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box33DescriptionOfGoods_Line1 + "\r\n" + c.Box33DescriptionOfGoods_Line2 + "\r\n" + c.Box33DescriptionOfGoods_Line3 + "\r\n" + c.Box33DescriptionOfGoods_Line4 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsCode()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box34ImportExportPermitNumberAndItemNumber_Line1 + "\r\n" + c.Box34ImportExportPermitNumberAndItemNumber_Line2 + "\r\n" + c.Box35CCCCode + "\r\n" + c.Box35BondedGoodsCodeAndAssignedNumber + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsWeightAndQuantity()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box37NetWeight + "\r\n" + c.Box38QuantityAndUnit + "\r\n" + c.Box39StatisticsQuantityAndUnit_Line1 + "\r\n" + c.Box39StatisticsQuantityAndUnit_Line2 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsFOBValue()
			{
				var result = ZString.Empty;
				firstPageSections.ForEach(c => result += c.Box40FOBValue_Line1 + "\r\n" + c.Box40FOBValue_Line2 + "\r\n" + c.Box40FOBValue_Line3 + "\r\n" + c.Box40FOBValue_Line4 + "\r\n");
				return result.Trim();
			}
		}

		[ExpectNoExceptions]
		public void TestOtherPageGoodsItemListSections()
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "ACR";
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "AMH";
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceLine1.JI_Group = "  Grouping Line 11 ";
			invoiceLine2.JI_Group = "  Grouping Line 21 ";

			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(!wrapper.OtherPageGoodsItemListSections.Any(), NUnit.Framework.Is.True);

			invoiceLine2.JI_Group = "  Grouping Line 1  \r\n  Grouping Line 2 ";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var otherPageSections = wrapper.OtherPageGoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(GetAllSectionsDetail().ToString(), NUnit.Framework.Is.Null.Or.Empty);
				NUnit.Framework.Assert.That(GetAllSectionsCode(), NUnit.Framework.Is.EqualTo(ZString.Empty));
				NUnit.Framework.Assert.That(GetAllSectionsWeightAndQuantity(), NUnit.Framework.Is.EqualTo(ZString.Empty));
				NUnit.Framework.Assert.That(GetAllSectionsFOBValue(), NUnit.Framework.Is.EqualTo(ZString.Empty));
			});

			entryLine1.CL_Description = CreateDescription(10);
			entryLine2.CL_Description = CreateDescription(52);
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			otherPageSections = wrapper.OtherPageGoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(GetAllSectionsDetail(), NUnit.Framework.Is.EqualTo(@"Grouping Line 2








1
2
3
4



5



6



7



8



9



10



11



12



13



14



15



16



17



18



19



20



21



22



23



24



25



26



27



28



29



30



31



32



33



34



35



36



37



38



39



40



41



42



43



44



45



46



47



48



49



50



51



52").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsCode(), NUnit.Framework.Is.EqualTo(@"...-



















































































































































































































Total:").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsWeightAndQuantity(), NUnit.Framework.Is.EqualTo(@"0 KGM
2 AMH


















































































































































































































----------------------
0 KGM
1 ACR
2 AMH
vvvvvvvvvvvv").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(GetAllSectionsFOBValue(), NUnit.Framework.Is.EqualTo(@"<Currency(0,TWD)>



















































































































































































































----------------------
<Currency(0,TWD)>
vvvvvvvvvv").Using(CustomComparers.TypeComparison));
			});

			ZString CreateDescription(int lineCount)
			{
				var description = new ZStringBuilder();
				for (var i = 1; i <= lineCount; ++i)
				{
					description.AppendLine(i.ToString());
				}
				return description.ToString();
			}

			ZString GetAllSectionsDetail()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box33DescriptionOfGoods_Line1 + "\r\n" + c.Box33DescriptionOfGoods_Line2 + "\r\n" + c.Box33DescriptionOfGoods_Line3 + "\r\n" + c.Box33DescriptionOfGoods_Line4 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsCode()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box34ImportExportPermitNumberAndItemNumber_Line1 + "\r\n" + c.Box34ImportExportPermitNumberAndItemNumber_Line2 + "\r\n" + c.Box35CCCCode + "\r\n" + c.Box35BondedGoodsCodeAndAssignedNumber + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsWeightAndQuantity()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box37NetWeight + "\r\n" + c.Box38QuantityAndUnit + "\r\n" + c.Box39StatisticsQuantityAndUnit_Line1 + "\r\n" + c.Box39StatisticsQuantityAndUnit_Line2 + "\r\n");
				return result.Trim();
			}

			ZString GetAllSectionsFOBValue()
			{
				var result = ZString.Empty;
				otherPageSections.ForEach(c => result += c.Box40FOBValue_Line1 + "\r\n" + c.Box40FOBValue_Line2 + "\r\n" + c.Box40FOBValue_Line3 + "\r\n" + c.Box40FOBValue_Line4 + "\r\n");
				return result.Trim();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestTrademarkImage()
		{
			var declaration = entryHeader.Declaration;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line 1";
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line 2";
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_CL = entryLine2.PK;

			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			invoiceLine1.TrademarkStorageDocsGuid = doc1.UniqueKey;
			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();
			NUnit.Framework.Assert.That(sections[0].TrademarkImage1, NUnit.Framework.Is.Not.EqualTo(default(System.Drawing.Image)));
			NUnit.Framework.Assert.That(sections[1].TrademarkImage1, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)));
			NUnit.Framework.Assert.That(sections[2].TrademarkImage1, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)));

			invoiceLine1.JI_BrandName = "UnitedSic (UNITED SILICON CARBIDE,  INC.  )";
			wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();
			NUnit.Framework.Assert.That(sections[0].TrademarkImage2, NUnit.Framework.Is.Not.EqualTo(default(System.Drawing.Image)));
			NUnit.Framework.Assert.That(sections[1].TrademarkImage2, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)));
			NUnit.Framework.Assert.That(sections[2].TrademarkImage2, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestDetailsWithTrademarkImage()
		{
			var declaration = entryHeader.Declaration;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_Description = "Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text";
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_Description = "Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text Entry Line text";
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_CL = entryLine2.PK;

			var decDocManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			var doc1 = decDocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\TestBitmap.bmp"), MessageConstants.DocumentTypes.TDM);
			invoiceLine1.TrademarkStorageDocsGuid = doc1.UniqueKey;
			var wrapper = new ExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var sections = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().ToArray();
			NUnit.Framework.Assert.That(sections[0].TrademarkImage1, NUnit.Framework.Is.Not.EqualTo(default(System.Drawing.Image)));
			NUnit.Framework.Assert.That(sections[1].TrademarkImage1, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)));

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 0 Box33Line 1");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line").Using(CustomComparers.TypeComparison), "Section 0 Box33Line 2");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("text Entry Line text Entry").Using(CustomComparers.TypeComparison), "Section 0 Box33Line 3");
				NUnit.Framework.Assert.That(sections[0].Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 0 Box33Line 4");
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text Entry").Using(CustomComparers.TypeComparison), "Section 1 Box33Line 1");
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 1 Box33Line 2");
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 1 Box33Line 3");
				NUnit.Framework.Assert.That(sections[1].Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 1 Box33Line 4");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Line text Entry Line text Entry Line").Using(CustomComparers.TypeComparison), "Section 2 Box33Line 1");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 2 Box33Line 2");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 2 Box33Line 3");
				NUnit.Framework.Assert.That(sections[2].Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 2 Box33Line 4");
			});

			NUnit.Framework.Assert.That(sections[2].TrademarkImage1, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)));
			NUnit.Framework.Assert.That(sections[3].TrademarkImage1, NUnit.Framework.Is.EqualTo(default(System.Drawing.Image)));

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("text Entry Line text").Using(CustomComparers.TypeComparison), "Section 3 Box33Line 1");
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 3 Box33Line 2");
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 3 Box33Line 3");
				NUnit.Framework.Assert.That(sections[3].Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 3 Box33Line 4");
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 4 Box33Line 1");
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text Entry").Using(CustomComparers.TypeComparison), "Section 4 Box33Line 2");
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("Line text Entry Line text Entry Line").Using(CustomComparers.TypeComparison), "Section 4 Box33Line 3");
				NUnit.Framework.Assert.That(sections[4].Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo("text Entry Line text Entry Line text").Using(CustomComparers.TypeComparison), "Section 4 Box33Line 4");
				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line1, NUnit.Framework.Is.EqualTo("Entry Line text Entry Line text Entry").Using(CustomComparers.TypeComparison), "Section 5 Box33Line 1");
				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 5 Box33Line 2");
				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 5 Box33Line 3");
				NUnit.Framework.Assert.That(sections[5].Box33DescriptionOfGoods_Line4, NUnit.Framework.Is.EqualTo(ZString.Empty), "Section 5 Box33Line 4");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "556644", Core.Constants.CountryCodes.Taiwan);
			var translatedAddress = orgHeader.MainAddress.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			translatedAddress.OTA_CompanyName = "新加坡商敦豪全球貨運物流股份有限公司台灣";
			entryInstruction.CEI_BoxNumber = "123";
			declaration.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;

			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.Declarant, NUnit.Framework.Is.EqualTo("新加坡商敦豪全球貨運物流股份有限公司台灣 123\r\nTWAEO-556644").Using(CustomComparers.TypeComparison), "Declarant");
			NUnit.Framework.Assert.That(wrapper.DedicatedStaff, NUnit.Framework.Is.EqualTo(System.Environment.NewLine + "1234").Using(CustomComparers.TypeComparison), "DedicatedStaff");
		}

		[ExpectNoExceptions]
		public void TestTrasportMode()
		{
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.TransportMode, NUnit.Framework.Is.EqualTo("空運").Using(CustomComparers.TypeComparison), "TransportMode");

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.TransportMode, NUnit.Framework.Is.EqualTo("海運").Using(CustomComparers.TypeComparison), "TransportMode");
		}

		[ExpectNoExceptions]
		public void TestDeclarationType()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			entryInstruction.CEI_Style = "G5";
			NUnit.Framework.Assert.That(wrapper.DeclarationTypeCode, NUnit.Framework.Is.EqualTo("G5").Using(CustomComparers.TypeComparison), "DeclarationTypeCode");
			NUnit.Framework.Assert.That(wrapper.DeclarationTypeDescription, NUnit.Framework.Is.EqualTo("國貨出口").Using(CustomComparers.TypeComparison), "DeclarationTypeDescription");

			entryInstruction.CEI_Style = "F4";
			NUnit.Framework.Assert.That(wrapper.DeclarationTypeCode, NUnit.Framework.Is.EqualTo("F4").Using(CustomComparers.TypeComparison), "DeclarationTypeCode");
			NUnit.Framework.Assert.That(wrapper.DeclarationTypeDescription, NUnit.Framework.Is.EqualTo("自由港區與他自由港\r\n區、課稅區間之交易").Using(CustomComparers.TypeComparison), "DeclarationTypeDescription");
		}

		[ExpectNoExceptions]
		public void TestDeclDocTypeCodeAndDescription()
		{
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DeclDocTypeCodeAndDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "DeclDocTypeCodeAndDescription");

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.ExportCustoms;
			NUnit.Framework.Assert.That(wrapper.DeclDocTypeCodeAndDescription, NUnit.Framework.Is.EqualTo("5-出口證明用\r\n聯").Using(CustomComparers.TypeComparison), "DeclDocTypeCodeAndDescription");

			declaration.JE_DeclDocType = ExportDeclDocTypeList.Codes.CustomsProcessingRecords;
			NUnit.Framework.Assert.That(wrapper.DeclDocTypeCodeAndDescription, NUnit.Framework.Is.EqualTo("1-海關處理紀\r\n錄聯").Using(CustomComparers.TypeComparison), "DeclDocTypeCodeAndDescription");

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DeclDocType = ImportDeclDocTypeList.Codes.ImportCustoms;
			NUnit.Framework.Assert.That(wrapper.DeclDocTypeCodeAndDescription, NUnit.Framework.Is.EqualTo("2-進口證明用\r\n聯").Using(CustomComparers.TypeComparison), "DeclDocTypeCodeAndDescription");
		}

		[ExpectNoExceptions]
		public void TestBOMPageCount()
		{
			entryInstruction.CEI_BOMPageCount = 15;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.BOMPageCount, NUnit.Framework.Is.EqualTo(15).Using(CustomComparers.TypeComparison), "BOMPageCount");
		}

		[ExpectNoExceptions]
		public void TestVesselRegNum()
		{
			declaration.JE_VesselArrivalReg = "ABC456";
			declaration.JE_SLD = "QW67";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.VesselRegNum1, NUnit.Framework.Is.EqualTo("ABC456").Using(CustomComparers.TypeComparison), "VesselRegNum1");
			NUnit.Framework.Assert.That(wrapper.VesselRegNum2, NUnit.Framework.Is.EqualTo("QW67").Using(CustomComparers.TypeComparison), "VesselRegNum2");
		}

		[ExpectNoExceptions]
		public void TestExportTransportID()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = vessel1.RV_Code;
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.ExportTransportID, NUnit.Framework.Is.EqualTo("CallSign").Using(CustomComparers.TypeComparison), "ExportTransportID");
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "FL123";
			NUnit.Framework.Assert.That(wrapper.ExportTransportID, NUnit.Framework.Is.EqualTo("FL 123").Using(CustomComparers.TypeComparison), "ExportTransportID");
		}

		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			declaration.JE_VoyageFlightNo = "RTY567";

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.JourneyID, NUnit.Framework.Is.EqualTo("RTY567").Using(CustomComparers.TypeComparison), "JourneyID");
		}

		[TestDate(2019, 8, 27, 16, 40, 0)]
		[ExpectNoExceptions]
		public void TestDateOfDeclaration()
		{
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			NUnit.Framework.Assert.That(wrapper.DateOfDeclaration, NUnit.Framework.Is.EqualTo("108年08月27日").Using(CustomComparers.TypeComparison), "DateOfDeclaration");
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 8, 28);
			NUnit.Framework.Assert.That(wrapper.DateOfDeclaration, NUnit.Framework.Is.EqualTo("108年08月28日").Using(CustomComparers.TypeComparison), "DateOfDeclaration");
		}

		[ExpectNoExceptions]
		public void TestLoadingUnloadingPorts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			declaration.JE_RL_NKOrigin = "JPARI";
			declaration.JE_RL_NKFinalDestination = "JPZHO";

			NUnit.Framework.Assert.That(wrapper.PlaceOfLoadingCode, NUnit.Framework.Is.EqualTo("JPARI").Using(CustomComparers.TypeComparison), "PlaceOfLoadingCode");
			NUnit.Framework.Assert.That(wrapper.PlaceOfLoadingName, NUnit.Framework.Is.EqualTo("Ariake, Tokyo").Using(CustomComparers.TypeComparison), "PlaceOfLoadingName");
			NUnit.Framework.Assert.That(wrapper.DestinationCode, NUnit.Framework.Is.EqualTo("JPZHO").Using(CustomComparers.TypeComparison), "DestinationCode");
			NUnit.Framework.Assert.That(wrapper.DestinationName, NUnit.Framework.Is.EqualTo("Hino, Tokyo").Using(CustomComparers.TypeComparison), "DestinationName");

			declaration.JE_RL_NKOrigin = "CNZ99";
			declaration.JE_Z99PortOfOrigin = "中国";
			declaration.JE_RL_NKFinalDestination = "TWZ99";
			declaration.JE_Z99FinalDestination = "台湾";
			NUnit.Framework.Assert.That(wrapper.PlaceOfLoadingName, NUnit.Framework.Is.EqualTo("中国").Using(CustomComparers.TypeComparison), "PlaceOfLoadingName");
			NUnit.Framework.Assert.That(wrapper.DestinationName, NUnit.Framework.Is.EqualTo("台湾").Using(CustomComparers.TypeComparison), "DestinationName");
		}

		[ExpectNoExceptions]
		public void TestGoodsLocation()
		{
			using (TWCustomsDataRegistry.Instance.DefaultPrintingGoodsLocationDescription.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_LocationOfGoods = "XXXX0124";
				entryInstruction.CEI_GoodsLocation = "XXXX0123";
				var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				NUnit.Framework.Assert.That(wrapper.LocationOfGoodsCode1, NUnit.Framework.Is.EqualTo("XXXX0123 XXXXXX").Using(CustomComparers.TypeComparison), "LocationOfGoodsCode1");
				NUnit.Framework.Assert.That(wrapper.LocationOfGoodsCode2, NUnit.Framework.Is.EqualTo("XXXX0124 WWWWWW").Using(CustomComparers.TypeComparison), "LocationOfGoodsCode2");
			}

			using (TWCustomsDataRegistry.Instance.DefaultPrintingGoodsLocationDescription.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_LocationOfGoods = "XXXX0124";
				entryInstruction.CEI_GoodsLocation = "XXXX0123";
				var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
				NUnit.Framework.Assert.That(wrapper.LocationOfGoodsCode1, NUnit.Framework.Is.EqualTo("XXXX0123").Using(CustomComparers.TypeComparison), "LocationOfGoodsCode1");
				NUnit.Framework.Assert.That(wrapper.LocationOfGoodsCode2, NUnit.Framework.Is.EqualTo("XXXX0124").Using(CustomComparers.TypeComparison), "LocationOfGoodsCode2");
			}
		}

		[ExpectNoExceptions]
		public void TestJobNumber()
		{
			declaration.JE_DeclarationReference = "XXXX0124";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.JobNumber, NUnit.Framework.Is.EqualTo("XXXX0124").Using(CustomComparers.TypeComparison), "JobNumber Should be");
		}

		[ExpectNoExceptions]
		public void TestOwnerReference()
		{
			declaration.JE_OwnerRef = "XXXX0124";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.OwnerReference, NUnit.Framework.Is.EqualTo("XXXX0124").Using(CustomComparers.TypeComparison), "OwnerReference Should be");
		}

		[ExpectNoExceptions]
		public void TestEntryReleaseDate()
		{
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2017, 2, 1);
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.EntryReleaseDate, NUnit.Framework.Is.EqualTo("2017-02-01").Using(CustomComparers.TypeComparison), "EntryReleaseDate Should be");
		}

		[ExpectNoExceptions]
		public void TestCreateUser()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "TW1";
			staff1.GS_LoginName = "STAFF1";
			staff1.GS_FullName = "TW STAFF1";

			declaration.JE_SystemCreateUser = "TW1";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.CreateUser, NUnit.Framework.Is.EqualTo("TW STAFF1").Using(CustomComparers.TypeComparison), "CreateUser Should be");

			declaration.JE_SystemCreateUser = "XX4";
			NUnit.Framework.Assert.That(wrapper.CreateUser, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "CreateUser Should be");
		}

		[ExpectNoExceptions]
		public void TestAirWaybillNIL()
		{
			var header = new TestTWCreator(Factory).CreateOrganizationForJobDocAddress();
			var address = header.MainAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = header.PK;
			importerDocumentaryAddress.E2_OA_Address = address.PK;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ001", Core.Constants.CountryCodes.Taiwan);
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			entryInstruction.CEI_WHSMonth = "2";
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_HouseBill = "HB14578562";

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(wrapper.MasterAirWaybill, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "MasterAirWaybill");
			NUnit.Framework.Assert.That(wrapper.HouseAirWaybill, NUnit.Framework.Is.EqualTo("HB14578562").Using(CustomComparers.TypeComparison), "HouseAirWaybill");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(wrapper.MasterAirWaybill, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "MasterAirWaybill");
			NUnit.Framework.Assert.That(wrapper.HouseAirWaybill, NUnit.Framework.Is.EqualTo("HB14578562").Using(CustomComparers.TypeComparison), "HouseAirWaybill");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(wrapper.MasterAirWaybill, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "MasterAirWaybill");
			NUnit.Framework.Assert.That(wrapper.HouseAirWaybill, NUnit.Framework.Is.EqualTo("HB14578562").Using(CustomComparers.TypeComparison), "HouseAirWaybill");

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			NUnit.Framework.Assert.That(wrapper.MasterAirWaybill, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "MasterAirWaybill");
			NUnit.Framework.Assert.That(wrapper.HouseAirWaybill, NUnit.Framework.Is.EqualTo("HB14578562").Using(CustomComparers.TypeComparison), "HouseAirWaybill");
		}

		[ExpectNoExceptions]
		public void TestAirWaybillSea()
		{
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "MB13545855";
			declaration.JE_HouseBill = "HB14578562";

			var codes = new string[]
			{
				Constants.DeclarationTypes.Export.B1,
				Constants.DeclarationTypes.Export.D1,
				Constants.DeclarationTypes.Export.G3,
				Constants.DeclarationTypes.Export.G5,
				Constants.DeclarationTypes.Export.F5,
			};
			foreach (var code in codes)
			{
				entryInstruction.CEI_Style = code;
				NUnit.Framework.Assert.That(wrapper.MasterAirWaybill, NUnit.Framework.Is.EqualTo("MB13545855").Using(CustomComparers.TypeComparison), "MasterAirWaybill with code: " + code);
				NUnit.Framework.Assert.That(wrapper.HouseAirWaybill, NUnit.Framework.Is.EqualTo("HB14578562").Using(CustomComparers.TypeComparison), "HouseAirWaybill with code: " + code);
			}
		}

		[ExpectNoExceptions]
		public void TestAirWaybillAir()
		{
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "MB13545855";
			declaration.JE_HouseBill = "HB14578562";

			var codes = new string[]
			{
				Constants.DeclarationTypes.Export.B1,
				Constants.DeclarationTypes.Export.B8,
				Constants.DeclarationTypes.Export.B9,
				Constants.DeclarationTypes.Export.D1,
				Constants.DeclarationTypes.Export.D5,
				Constants.DeclarationTypes.Export.G3,
				Constants.DeclarationTypes.Export.G5,
				Constants.DeclarationTypes.Export.F4,
				Constants.DeclarationTypes.Export.F5,
			};

			foreach (var code in codes)
			{
				entryInstruction.CEI_Style = code;
				NUnit.Framework.Assert.That(wrapper.MasterAirWaybill, NUnit.Framework.Is.EqualTo("MB13545855").Using(CustomComparers.TypeComparison), "MasterAirWaybill with code: " + code);
				NUnit.Framework.Assert.That(wrapper.HouseAirWaybill, NUnit.Framework.Is.EqualTo("HB14578562").Using(CustomComparers.TypeComparison), "HouseAirWaybill with code: " + code);
			}
		}

		[ExpectNoExceptions]
		public void TestDutyDrawback()
		{
			entryInstruction.CEI_DutyRefund = false;
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DutyDrawback, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));

			entryInstruction.CEI_DutyRefund = true;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DutyDrawback, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestModeOfTransportSea()
		{
			var seaModes = new Tuple<string, string>[]
			{
				new Tuple<string, string>(ContainerModeList.Codes.BreakBulk, TransportCodeList.Codes.SeaPackedSundryGoods),
				new Tuple<string, string>(ContainerModeList.Codes.Containerized, TransportCodeList.Codes.SeaContainer),
				new Tuple<string, string>(ContainerModeList.Codes.Bulk, TransportCodeList.Codes.SeaBulkGoods),
				new Tuple<string, string>(ContainerModeList.Codes.OwnPropulsion, TransportCodeList.Codes.SeaSelfPropelledGoods),
				new Tuple<string, string>(ContainerModeList.Codes.HandCarry, TransportCodeList.Codes.SeaPassengerOrCREW),
				new Tuple<string, string>(ContainerModeList.Codes.Express, TransportCodeList.Codes.SeaExpressDelivery),
				new Tuple<string, string>(ContainerModeList.Codes.Mail, TransportCodeList.Codes.SeaMail),
				new Tuple<string, string>(ContainerModeList.Codes.Other, TransportCodeList.Codes.Other),
			};
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			foreach (var mode in seaModes)
			{
				declaration.JE_ContainerMode = mode.Item1;
				NUnit.Framework.Assert.That(wrapper.ModeOfTransport, NUnit.Framework.Is.EqualTo(mode.Item2).Using(CustomComparers.TypeComparison), string.Format(CultureInfo.InvariantCulture, "When Transport Type is Sea and Container Mode is {0}, ModeOfTransport should be ", mode.Item2));
			}
		}

		[ExpectNoExceptions]
		public void TestModeOfTransportAir()
		{
			var airModes = new Tuple<string, string>[]
			{
				new Tuple<string, string>(ContainerModeList.Codes.Loose, TransportCodeList.Codes.AirNotExpressDelivery),
				new Tuple<string, string>(ContainerModeList.Codes.Express, TransportCodeList.Codes.AirExpressDelivery),
				new Tuple<string, string>(ContainerModeList.Codes.OwnPropulsion, TransportCodeList.Codes.AirSelfPropelledGoods),
				new Tuple<string, string>(ContainerModeList.Codes.HandCarry, TransportCodeList.Codes.AirPassengerOrCREW),
				new Tuple<string, string>(ContainerModeList.Codes.Mail, TransportCodeList.Codes.AirMail),
				new Tuple<string, string>(ContainerModeList.Codes.Other, TransportCodeList.Codes.Other),
			};
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			foreach (var mode in airModes)
			{
				declaration.JE_ContainerMode = mode.Item1;
				NUnit.Framework.Assert.That(wrapper.ModeOfTransport, NUnit.Framework.Is.EqualTo(mode.Item2).Using(CustomComparers.TypeComparison), string.Format(CultureInfo.InvariantCulture, "When Transport Type is Air and Container Mode is {0}, ModeOfTransport should be ", mode.Item2));
			}
		}

		[ExpectNoExceptions]
		public void TestExportVesselName()
		{
			declaration.JE_VesselName = "VN1254";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.ExportVesselName, NUnit.Framework.Is.EqualTo("VN1254").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestTotalInvoiceAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 10m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 20m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "FOB";
			var entryInstruction = declaration.CusEntryInstruction;

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 4;
			var charges = invoice.Charges;

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_InvoiceQuantity = 2;

			var oFTCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			var oNSCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 90m, Core.Constants.CurrencyCodes.UnitedStates);
			var aDDCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			var dEDCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge, 60m, Core.Constants.CurrencyCodes.UnitedStates);

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			AssertEquals("Total Invoice Amount - Single Currency", 3330m, wrapper.TotalInvoiceAmount);
			AssertEquals("Total Invoice Amount Currency - Single Currency", "USD", wrapper.TotalInvoiceAmountCurrencyCode);
			aDDCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			AssertEquals("Total Invoice Amount - Multiple Currencies", 3430m, wrapper.TotalInvoiceAmount);
			AssertEquals("Total Invoice Amount Currency - Multiple Currencies", "USD", wrapper.TotalInvoiceAmountCurrencyCode);
		}

		[TestDate(2021, 10, 21)]
		[ExpectNoExceptions]
		public void TestItemChargeAmountWhenExportAndEXW()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 27.675m, new ZDateTime(2021, 10, 21), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_InvoiceAmount = 23268m;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;

			var invoice1Charges = invoice1.Charges;
			var charge1 = invoice1Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 220m, Core.Constants.CurrencyCodes.UnitedStates);
			charge1.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge1.J7_IsIncludedInITOT = false;
			charge1.J7_IsDutiable = false;
			charge1.J7_IsGSTApplicable = true;

			var charge2 = invoice1Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 1415.28m, Core.Constants.CurrencyCodes.UnitedStates);
			charge2.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge2.J7_IsIncludedInITOT = true;
			charge2.J7_IsDutiable = true;
			charge2.J7_IsGSTApplicable = false;

			var charge3 = invoice1Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 25m, Core.Constants.CurrencyCodes.UnitedStates);
			charge3.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_IsDutiable = true;
			charge3.J7_IsGSTApplicable = false;

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 40000;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 0.189;

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 42000;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 0.374;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.ItemChargeAmount, NUnit.Framework.Is.EqualTo(22097.72m).Using(CustomComparers.TypeComparison), "ItemChargeAmount with single currency");
			NUnit.Framework.Assert.That(wrapper.ItemChargeAmountCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "ItemChargeAmount with single currencies");
		}

		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestCharges()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 10m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.EuropeanUnion, 20m, new ZDateTime(2016, 01, 01), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "CIF";
			var entryInstruction = declaration.CusEntryInstruction;

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = "PARTNO1";
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceChargeCollection = invoice.Charges;

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_PartNo = "PARTNO1";
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCE";

			var oftCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 102m, Core.Constants.CurrencyCodes.UnitedStates);
			var aDDCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 101m, Core.Constants.CurrencyCodes.UnitedStates);
			var dedCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge, 60m, Core.Constants.CurrencyCodes.UnitedStates);

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.Freight, NUnit.Framework.Is.EqualTo("<FormatNumber(102,2)>").Using(CustomComparers.TypeComparison), "Freight with single currency");
				NUnit.Framework.Assert.That(wrapper.FreightCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Freight with single currencies");
				NUnit.Framework.Assert.That(wrapper.InsuranceFee, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "InsuranceFee with single currency");
				NUnit.Framework.Assert.That(wrapper.InsuranceFeeCurrencyCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "InsuranceFee with single currencies");
				NUnit.Framework.Assert.That(wrapper.OtherChargeAmount, NUnit.Framework.Is.EqualTo("<FormatNumber(101,2)>").Using(CustomComparers.TypeComparison), "OtherChargeAmount with single currency");
				NUnit.Framework.Assert.That(wrapper.OtherChargeAmountCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "OtherChargeAmount with single currencies");
				NUnit.Framework.Assert.That(wrapper.OtherDeductionAmount, NUnit.Framework.Is.EqualTo("<FormatNumber(60,2)>").Using(CustomComparers.TypeComparison), "OtherDeductionAmount with single currency");
				NUnit.Framework.Assert.That(wrapper.OtherDeductionAmountCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "OtherDeductionAmount with single currencies");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmount, NUnit.Framework.Is.EqualTo(2898m).Using(CustomComparers.TypeComparison), "ItemChargeAmount with single currency");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmountCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "ItemChargeAmount with single currencies");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmountLocalCurrency, NUnit.Framework.Is.EqualTo(28980m).Using(CustomComparers.TypeComparison), "ItemChargeAmountLocalCurrency with single currency");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmountLocalCurrencyCode, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison), "ItemChargeAmountLocalCurrency with single currencies");
			});

			oftCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.Freight, NUnit.Framework.Is.EqualTo("<FormatNumber(204,2)>").Using(CustomComparers.TypeComparison), "Freight with single currency");
				NUnit.Framework.Assert.That(wrapper.FreightCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "Freight with single currencies");
				NUnit.Framework.Assert.That(wrapper.InsuranceFee, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "InsuranceFee with multiple currency");
				NUnit.Framework.Assert.That(wrapper.InsuranceFeeCurrencyCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "InsuranceFee with multiple currencies");
				NUnit.Framework.Assert.That(wrapper.OtherChargeAmount, NUnit.Framework.Is.EqualTo("<FormatNumber(101,2)>").Using(CustomComparers.TypeComparison), "OtherChargeAmount with multiple currency");
				NUnit.Framework.Assert.That(wrapper.OtherChargeAmountCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "OtherChargeAmount with multiple currencies");
				NUnit.Framework.Assert.That(wrapper.OtherDeductionAmount, NUnit.Framework.Is.EqualTo("<FormatNumber(60,2)>").Using(CustomComparers.TypeComparison), "OtherDeductionAmount with multiple currency");
				NUnit.Framework.Assert.That(wrapper.OtherDeductionAmountCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "OtherDeductionAmount with multiple currencies");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmount, NUnit.Framework.Is.EqualTo(2796m).Using(CustomComparers.TypeComparison), "ItemChargeAmount with multiple currency");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmountCurrencyCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "ItemChargeAmount with multiple currencies");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmountLocalCurrency, NUnit.Framework.Is.EqualTo(27960m).Using(CustomComparers.TypeComparison), "ItemChargeAmountLocalCurrency with multiple currency");
				NUnit.Framework.Assert.That(wrapper.ItemChargeAmountLocalCurrencyCode, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison), "ItemChargeAmountLocalCurrency with multiple currencies");
			});
		}

		[ExpectNoExceptions]
		public void TestExporter()
		{
			supplier.MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "CBF22", Core.Constants.CountryCodes.Taiwan);
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ExporterID, NUnit.Framework.Is.EqualTo("6666666").Using(CustomComparers.TypeComparison), "ExporterID");
				NUnit.Framework.Assert.That(wrapper.ExporterChineseName, NUnit.Framework.Is.EqualTo("Supplier company name(OTA).").Using(CustomComparers.TypeComparison), "ExporterChineseName");
				NUnit.Framework.Assert.That(wrapper.ExporterName, NUnit.Framework.Is.EqualTo("Supplier company name.").Using(CustomComparers.TypeComparison), "ExporterName");
				NUnit.Framework.Assert.That(wrapper.ExporterChineseAddress, NUnit.Framework.Is.EqualTo("Taipei Minsheng E.Rd.Taipei Minsheng W.Rd.").Using(CustomComparers.TypeComparison), "ExporterChineseAddress");
				NUnit.Framework.Assert.That(wrapper.ExporterAddress, NUnit.Framework.Is.EqualTo("88899 ADDRESS LINE1. 88899 ADDRESS LINE2. TAIWAN").Using(CustomComparers.TypeComparison), "ExporterAddress");
				NUnit.Framework.Assert.That(wrapper.ExporterAddressLine, NUnit.Framework.Is.EqualTo(@"Taipei Minsheng E.Rd.Taipei Minsheng W.Rd.
88899 ADDRESS LINE1. 88899 ADDRESS LINE2. TAIWAN").Using(CustomComparers.TypeComparison), "ExporterAddressLine");
				NUnit.Framework.Assert.That(wrapper.ExporterAEOCode, NUnit.Framework.Is.EqualTo("TWAEO-999999").Using(CustomComparers.TypeComparison), "ExporterAEOCode");
				NUnit.Framework.Assert.That(wrapper.ExporterCustomsControlID, NUnit.Framework.Is.EqualTo("CBF22").Using(CustomComparers.TypeComparison), "ExporterCustomsControlID");
				NUnit.Framework.Assert.That(wrapper.ExporterPaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo("888888").Using(CustomComparers.TypeComparison), "ExporterPaymentOnAccountBusinessID");
			});

			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.HideEXPExporterTradChineseAddr = true;
			jobDeclarationDocumentAddressConfig.HideEXPExporterEnglishAddr = false;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ExporterChineseAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ExporterChineseAddress");
				NUnit.Framework.Assert.That(wrapper.ExporterAddress, NUnit.Framework.Is.EqualTo("88899 ADDRESS LINE1. 88899 ADDRESS LINE2. TAIWAN").Using(CustomComparers.TypeComparison), "ExporterAddress");
				NUnit.Framework.Assert.That(wrapper.ExporterAddressLine, NUnit.Framework.Is.EqualTo("88899 ADDRESS LINE1. 88899 ADDRESS LINE2. TAIWAN").Using(CustomComparers.TypeComparison), "ExporterAddressLine");
			});

			jobDeclarationDocumentAddressConfig.HideEXPExporterTradChineseAddr = false;
			jobDeclarationDocumentAddressConfig.HideEXPExporterEnglishAddr = true;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.ExporterChineseAddress, NUnit.Framework.Is.EqualTo("Taipei Minsheng E.Rd.Taipei Minsheng W.Rd.").Using(CustomComparers.TypeComparison), "ExporterChineseAddress");
				NUnit.Framework.Assert.That(wrapper.ExporterAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "ExporterAddress");
				NUnit.Framework.Assert.That(wrapper.ExporterAddressLine, NUnit.Framework.Is.EqualTo(@"Taipei Minsheng E.Rd.Taipei Minsheng W.Rd.").Using(CustomComparers.TypeComparison), "ExporterAddressLine");
			});
		}

		[ExpectNoExceptions]
		public void TestBuyer()
		{
			buyer.MainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ11", Core.Constants.CountryCodes.Taiwan);
			declaration.ImporterDocumentaryAddress.E2_OA_Address = buyer.MainAddress.PK;
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.BuyerID, NUnit.Framework.Is.EqualTo("B222222").Using(CustomComparers.TypeComparison), "BuyerID");
				NUnit.Framework.Assert.That(wrapper.BuyerChineseName, NUnit.Framework.Is.EqualTo("Buyer company name(OTA).").Using(CustomComparers.TypeComparison), "BuyerChineseName");
				NUnit.Framework.Assert.That(wrapper.BuyerName, NUnit.Framework.Is.EqualTo("Buyer company name.").Using(CustomComparers.TypeComparison), "BuyerName");
				NUnit.Framework.Assert.That(wrapper.BuyerChineseAddress, NUnit.Framework.Is.EqualTo("No. 195, Sec. 3, Jianguo N. Rd.,Zhongshan Dist., Taipei City 104, Taiwan (R.O.C.)").Using(CustomComparers.TypeComparison), "BuyerChineseAddress");
				NUnit.Framework.Assert.That(wrapper.BuyerAddress, NUnit.Framework.Is.EqualTo("12345 ADDRESS LINE1. 12345 ADDRESS LINE2. TAIWAN").Using(CustomComparers.TypeComparison), "BuyerAddress");
				NUnit.Framework.Assert.That(wrapper.BuyerAEOCode, NUnit.Framework.Is.EqualTo("TWAEO-BAEO001").Using(CustomComparers.TypeComparison), "BuyerAEOCode");
				NUnit.Framework.Assert.That(wrapper.BuyerCustomControlID, NUnit.Framework.Is.EqualTo("EPZ11").Using(CustomComparers.TypeComparison), "BuyerCustomControlID");
			});

			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.HideEXPBuyerTradChineseAddr = true;
			jobDeclarationDocumentAddressConfig.HideEXPBuyerEnglishAddr = false;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.BuyerChineseAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "BuyerChineseAddress");
				NUnit.Framework.Assert.That(wrapper.BuyerAddress, NUnit.Framework.Is.EqualTo("12345 ADDRESS LINE1. 12345 ADDRESS LINE2. TAIWAN").Using(CustomComparers.TypeComparison), "BuyerAddress");
			});

			jobDeclarationDocumentAddressConfig.HideEXPBuyerTradChineseAddr = false;
			jobDeclarationDocumentAddressConfig.HideEXPBuyerEnglishAddr = true;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(wrapper.BuyerChineseAddress, NUnit.Framework.Is.EqualTo("No. 195, Sec. 3, Jianguo N. Rd.,Zhongshan Dist., Taipei City 104, Taiwan (R.O.C.)").Using(CustomComparers.TypeComparison), "BuyerChineseAddress");
				NUnit.Framework.Assert.That(wrapper.BuyerAddress, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "BuyerAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestDutyMethodCode()
		{
			declaration.JE_PaymentMethod = "1";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DutyMethodCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTradeTermsCondition()
		{
			entryHeader.CH_DeclarationIncoterm = "CIF";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.TradeTermsConditionCode, NUnit.Framework.Is.EqualTo("CIF").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2019, 08, 30)]
		[ExpectNoExceptions]
		public void TestExchangeRate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "CIF";
			var entryInstruction = declaration.CusEntryInstruction;

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_PartNo = "PARTNO1";
			invoiceLine1.JI_InvoiceQuantity = 4;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceChargeCollection = invoice.Charges;

			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_PartNo = "PARTNO1";
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCE";

			var oFTCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 200m, Core.Constants.CurrencyCodes.UnitedStates);
			oFTCharge.J7_IsIncludedInITOT = false;
			oFTCharge.J7_IsDutiable = true;
			oFTCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			oFTCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			var oNSCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 90m, Core.Constants.CurrencyCodes.UnitedStates);
			oNSCharge.J7_IsIncludedInITOT = false;
			oNSCharge.J7_IsDutiable = true;
			oNSCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			oNSCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			var aDDCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			aDDCharge.J7_IsIncludedInITOT = false;
			aDDCharge.J7_IsDutiable = true;
			aDDCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			aDDCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			var dEDCharge = invoiceChargeCollection.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge, 60m, Core.Constants.CurrencyCodes.UnitedStates);
			dEDCharge.J7_IsIncludedInITOT = true;
			dEDCharge.J7_IsDutiable = false;
			dEDCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			dEDCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.ExchangeRate, NUnit.Framework.Is.EqualTo(0.04m).Using(CustomComparers.TypeComparison), "ExchangeRate with single currency");

			aDDCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.ExchangeRate, NUnit.Framework.Is.EqualTo(0.04m).Using(CustomComparers.TypeComparison), "ExchangeRate with multiple currency");
		}

		[ExpectNoExceptions]
		public void TestTotalNumPackagesWithUnit()
		{
			declaration.JE_TotalNoOfPacks = 2;
			declaration.JE_TotalNoOfPacksPackType = "UNT";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.TotalNumPackagesWithUnit, NUnit.Framework.Is.EqualTo("2 / UNT").Using(CustomComparers.TypeComparison), "TotalNumPackagesWithUnit");
		}

		[ExpectNoExceptions]
		public void TestPackagingMaterialDescription()
		{
			entryInstruction.CEI_IsCoPackaged = true;
			entryInstruction.CEI_PackageDescription = "package desc test";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo("package desc test").Using(CustomComparers.TypeComparison), "PackagingMaterialDescription");
		}

		[ExpectNoExceptions]
		public void TestGrossWeight()
		{
			declaration.JE_TotalWeight = 1220m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Grams;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.GrossWeight, NUnit.Framework.Is.EqualTo("1.22").Using(CustomComparers.TypeComparison), "GrossWeight");
		}

		[ExpectNoExceptions]
		public void TestFees()
		{
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_CustomsOffice = "AA";

			entryInstruction.CEI_Style = "B9";
			entryInstruction.CEI_CustomsOffice = "AA";

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader1.JZ_InvoiceCurrExRate = 0.04m;
			invoiceHeader1.JZ_InvoiceAmount = 1150000m;
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_JZ = invoiceHeader1.PK;
			invoiceLine1.JI_LinePrice = 600000m;
			invoiceLine1.JI_Procedure = "9U";
			var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_JZ = invoiceHeader1.PK;
			invoiceLine2.JI_LinePrice = 550000m;
			invoiceLine1.JI_Procedure = "9U";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.TradePromotionFee, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "TradePromotionFee");
			NUnit.Framework.Assert.That(wrapper.TradePromotionFeeCurrencyCode, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison), "TradePromotionFeeCurrencyCode");
			NUnit.Framework.Assert.That(wrapper.TotalFee, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "TotalFee");
			NUnit.Framework.Assert.That(wrapper.TotalFeeCurrencyCode, NUnit.Framework.Is.EqualTo("TWD").Using(CustomComparers.TypeComparison), "TotalFeeCurrencyCode");

			var fee = entryHeader.DutyTaxFeeCharges.AddNew();
			fee.ChargeType = DutyTaxFeeCodeList.Codes.B51;
			fee.ChargeAmount = 100M;
			NUnit.Framework.Assert.That(wrapper.TradePromotionFee, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "TradePromotionFee");
			NUnit.Framework.Assert.That(wrapper.TotalFee, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "TotalFee");

			fee.ChargeType = DutyTaxFeeCodeList.Codes.B52;
			fee.ChargeAmount = 100M;
			NUnit.Framework.Assert.That(wrapper.TradePromotionFee, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "TradePromotionFee");
			NUnit.Framework.Assert.That(wrapper.TotalFee, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "TotalFee");

			fee.ChargeType = DutyTaxFeeCodeList.Codes.B59;
			fee.ChargeAmount = 100M;
			NUnit.Framework.Assert.That(wrapper.TradePromotionFee, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "TradePromotionFee");
			NUnit.Framework.Assert.That(wrapper.TotalFee, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "TotalFee");
		}

		[ExpectNoExceptions]
		public void TestClearanceType()
		{
			entryHeader.EntryNumber = "CABF0945600030";
			var entryNumber = entryHeader.CusEntryNumber;
			entryNumber.CE_EntryStatus = ClearanceStatusCodeList.Codes.C1;
			Factory.Save();

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.ClearanceType, NUnit.Framework.Is.EqualTo("C1").Using(CustomComparers.TypeComparison), "ClearanceType when C1");

			entryNumber.CE_EntryStatus = ClearanceStatusCodeList.Codes.C2;
			NUnit.Framework.Assert.That(wrapper.ClearanceType, NUnit.Framework.Is.EqualTo("C2").Using(CustomComparers.TypeComparison), "ClearanceType when C2");

			entryNumber.CE_EntryStatus = ClearanceStatusCodeList.Codes.C3M;
			NUnit.Framework.Assert.That(wrapper.ClearanceType, NUnit.Framework.Is.EqualTo("C3M").Using(CustomComparers.TypeComparison), "ClearanceType when C3M");

			entryNumber.CE_EntryStatus = ClearanceStatusCodeList.Codes.C3X;
			NUnit.Framework.Assert.That(wrapper.ClearanceType, NUnit.Framework.Is.EqualTo("C3X").Using(CustomComparers.TypeComparison), "ClearanceType when C3X");
		}

		[ExpectNoExceptions]
		public void TestClearanceCode()
		{
			entryInstruction.CEI_ExamMode = "5";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.ClearanceCode, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison), "ClearanceCode");
		}

		[ExpectNoExceptions]
		public void TestDuplicateNum()
		{
			var duplicate = entryInstruction.DeclarationDuplicates.AddNew();
			duplicate.CY_Type = CusCodeDataTypeList.Codes.DeclarationDuplicate;
			duplicate.CY_Code = "5";
			duplicate.CY_Data = "1";

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DuplicateNum, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison), "DuplicateNum");
			NUnit.Framework.Assert.That(wrapper.NumOfCopies, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "NumOfCopies");
		}

		[ExpectNoExceptions]
		public void TestMarksAndNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentNotes = shipment.Notes.VisibleNotes;
			var noteD = shipmentNotes.AddNew();
			noteD.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteD.ST_NoteContextModule = nameof(StmNoteContextModule.D);
			noteD.ST_NoteText = "D Marks & Numbers";

			var noteA = shipmentNotes.AddNew();
			noteA.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			noteA.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			noteA.ST_NoteText = "A Marks & Numbers";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CEI = entryInstruction.PK;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceHeader1.JZ_MarksAndNumbers = "MARKS AND NUMBER 1";
			invoiceHeader2.JZ_MarksAndNumbers = "MARKS AND NUMBER 1";

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.MarksAndNumbersFront1, NUnit.Framework.Is.EqualTo("D Marks & Numbers").Using(CustomComparers.TypeComparison));

			shipmentNotes.Remove(noteD);
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.MarksAndNumbersFront1, NUnit.Framework.Is.EqualTo("MARKS AND NUMBER 1").Using(CustomComparers.TypeComparison));

			invoiceHeader1.JZ_MarksAndNumbers = ZString.Empty;
			invoiceHeader2.JZ_MarksAndNumbers = ZString.Empty;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.MarksAndNumbersFront1, NUnit.Framework.Is.EqualTo("A Marks & Numbers").Using(CustomComparers.TypeComparison));

			shipmentNotes.Remove(noteA);
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.MarksAndNumbersFront1, NUnit.Framework.Is.EqualTo("N/M").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOtherDeclarationsFrontForTransportTypeSea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AA";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_MarksAndNumbers = "TEST MARKS";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			entryInstruction.TW_TradersRemarks = "Traders Remarks Note Line 1";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var containers = declaration.CusContainers;
			AddNewContainerForOtherDeclarationsFrontTest(containers, invoiceHeader, entryHeader, "UUUU1234568");

			var marksNumbers = new ZStringBuilder();
			for (var i = 1; i <= 9; ++i)
			{
				marksNumbers.AppendLine(i.ToString());
			}

			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions("Mark&Number、Container、TradersRemarks not exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9

UUUU1234568

其他申報事項：
Traders Remarks Note Line 1




").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(!wrapper.Part2Sections.Any(), NUnit.Framework.Is.True);
			});

			entryInstruction.TW_TradersRemarks = @"Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3
Traders Remarks Note Line 4
Traders Remarks Note Line 5
Traders Remarks Note Line 6";

			marksNumbers.AppendLine("10");
			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			AddNewContainerForOtherDeclarationsFrontTest(containers, invoiceHeader, entryHeader, "UUUU1234597");

			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions("TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
10
UUUU1234568
UUUU1234597
其他申報事項：
Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3
Traders Remarks Note Line 4
其他申報事項資料列印於後
").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列其他申報事項資料*****)
Traders Remarks Note Line 5
Traders Remarks Note Line 6").Using(CustomComparers.TypeComparison));
			});

			marksNumbers.AppendLine("11");
			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
標記資料列印於後
UUUU1234568
UUUU1234597
其他申報事項：
Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3
Traders Remarks Note Line 4
其他申報事項資料列印於後
").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
10
11
(*****續列其他申報事項資料*****)
Traders Remarks Note Line 5
Traders Remarks Note Line 6").Using(CustomComparers.TypeComparison));
			});

			AddNewContainerForOtherDeclarationsFrontTest(containers, invoiceHeader, entryHeader, "UUUU1234569");
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、Container、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
標記資料列印於後
UUUU1234568
貨櫃號碼資料列印於後
其他申報事項：
Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3
Traders Remarks Note Line 4
其他申報事項資料列印於後
").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
10
11
(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569
(*****續列其他申報事項資料*****)
Traders Remarks Note Line 5
Traders Remarks Note Line 6").Using(CustomComparers.TypeComparison));
			});

			entryInstruction.TW_OverrideTradersRemarks = false;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、Container exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
標記資料列印於後
UUUU1234568
貨櫃號碼資料列印於後






").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
10
11
(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569").Using(CustomComparers.TypeComparison));
			});

			invoiceHeader.JZ_MarksAndNumbers = "1";
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Container exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1









UUUU1234568
貨櫃號碼資料列印於後






").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569").Using(CustomComparers.TypeComparison));
			});

			entryInstruction.TW_TradersRemarks = @"Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4";
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Container、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1









UUUU1234568
貨櫃號碼資料列印於後
其他申報事項：
Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note
Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note
其他申報事項資料列印於後
").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列貨櫃號碼資料*****)
UUUU1234597
UUUU1234569
(*****續列其他申報事項資料*****)
Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4").Using(CustomComparers.TypeComparison));
			});

			invoiceHeader.JZ_MarksAndNumbers = @"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10 Mark & Number Line10 Mark & Number Line10 Mark & Number Line10";
			containers.DeleteAll();
			entryInstruction.TW_OverrideTradersRemarks = false;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark &Number exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number
Line1 Mark & Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
標記資料列印於後








").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
Mark & Number Line9
Mark & Number Line10 Mark & Number Line10 Mark & Number Line10 Mark & Number Line10").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestOtherDeclarationsFrontForTransportTypeAir()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "AA";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_MarksAndNumbers = "TEST MARKS";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			entryInstruction.TW_TradersRemarks = "Traders Remarks Note Line 1";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var marksNumbers = new ZStringBuilder();
			for (var i = 1; i <= 11; ++i)
			{
				marksNumbers.AppendLine(i.ToString());
			}

			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions("Mark&Number、TradersRemarks not exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
10
11

其他申報事項：
Traders Remarks Note Line 1




").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(!wrapper.Part2Sections.Any(), NUnit.Framework.Is.True);
			});

			declaration.JE_OH_Importer = ZGuid.Empty;
			entryInstruction.TW_TradersRemarks = @"Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4";

			marksNumbers.AppendLine("12");
			invoiceHeader.JZ_MarksAndNumbers = marksNumbers.ToString();

			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			CombineAssertions("TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"1
2
3
4
5
6
7
8
9
10
11
12
其他申報事項：
Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note
Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note
其他申報事項資料列印於後
").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列其他申報事項資料*****)
Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4").Using(CustomComparers.TypeComparison));
			});

			invoiceHeader.JZ_MarksAndNumbers = @"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10
Mark & Number Line11
Mark & Number Line12 Mark & Number Line12 Mark & Number Line12 Mark & Number Line12";
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number、TradersRemarks exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number
Line1 Mark & Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10
標記資料列印於後
其他申報事項：
Traders Remarks Note Line 1 Traders Remarks Note Line 1 Traders Remarks Note
Line 1 Traders Remarks Note Line 1
Traders Remarks Note Line 2
Traders Remarks Note Line 3 Traders Remarks Note Line 3 Traders Remarks Note
其他申報事項資料列印於後
").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
Mark & Number Line11
Mark & Number Line12 Mark & Number Line12 Mark & Number Line12 Mark & Number Line12
(*****續列其他申報事項資料*****)
Line 3 Traders Remarks Note Line 3 Traders Remarks Note Line 3
Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4 Traders Remarks Note Line 4").Using(CustomComparers.TypeComparison));
			});

			entryInstruction.TW_OverrideTradersRemarks = false;
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			CombineAssertions("Mark&Number exceeded", () =>
			{
				NUnit.Framework.Assert.That(GetCombinedOtherDeclarationsFront(wrapper), NUnit.Framework.Is.EqualTo(@"Mark & Number Line1 Mark & Number Line1 Mark & Number Line1 Mark & Number
Line1 Mark & Number Line1
Mark & Number Line2
Mark & Number Line3
Mark & Number Line4
Mark & Number Line5
Mark & Number Line6
Mark & Number Line7
Mark & Number Line8
Mark & Number Line9
Mark & Number Line10
標記資料列印於後






").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(wrapper.Part2Sections[0].OtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"(*****續列標記資料*****)
Mark & Number Line11
Mark & Number Line12 Mark & Number Line12 Mark & Number Line12 Mark & Number Line12").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestOtherDeclarationsFrontForContainer()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var invoiceHeader = declaration.Invoices.AddNew();

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var combinedOtherDeclarationsFront = GetCombinedOtherDeclarationsFront(wrapper);
			NUnit.Framework.Assert.That(combinedOtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"N/M

















").Using(CustomComparers.TypeComparison));

			var container = AddNewContainerForOtherDeclarationsFrontTest(declaration.CusContainers, invoiceHeader, entryHeader, "UUUU1234568");
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			combinedOtherDeclarationsFront = GetCombinedOtherDeclarationsFront(wrapper);
			NUnit.Framework.Assert.That(combinedOtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"N/M









UUUU1234568







").Using(CustomComparers.TypeComparison));

			container.CO_SecondSeal = "222";

			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			combinedOtherDeclarationsFront = GetCombinedOtherDeclarationsFront(wrapper);
			NUnit.Framework.Assert.That(combinedOtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"N/M









UUUU1234568/222







").Using(CustomComparers.TypeComparison));

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "RCC";
			container.CO_RC = refContainer.PK;
			container.CO_Seal = "111";
			container.CO_FCL_LCL_AIR = "LCL";

			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			combinedOtherDeclarationsFront = GetCombinedOtherDeclarationsFront(wrapper);
			NUnit.Framework.Assert.That(combinedOtherDeclarationsFront, NUnit.Framework.Is.EqualTo(@"N/M









UUUU1234568/RCC/3/111/222







").Using(CustomComparers.TypeComparison));
		}

		ZString GetCombinedOtherDeclarationsFront(ExportCustomsDeclarationDocumentWrapper wrapper)
		{
			var sb = new ZStringBuilder();
			sb.AppendLine(wrapper.MarksAndNumbersFront1);
			sb.AppendLine(wrapper.MarksAndNumbersFront2);
			sb.AppendLine(wrapper.MarksAndNumbersFront3);
			sb.AppendLine(wrapper.MarksAndNumbersFront4);
			sb.AppendLine(wrapper.MarksAndNumbersFront5);
			sb.AppendLine(wrapper.MarksAndNumbersFront6);
			sb.AppendLine(wrapper.MarksAndNumbersFront7);
			sb.AppendLine(wrapper.MarksAndNumbersFront8);
			sb.AppendLine(wrapper.MarksAndNumbersFront9);
			sb.AppendLine(wrapper.MarksAndNumbersFront10);
			sb.AppendLine(wrapper.MarksAndNumbersFront11);
			sb.AppendLine(wrapper.MarksAndNumbersFront12);
			sb.AppendLine(wrapper.MarksAndNumbersFront13);
			sb.AppendLine(wrapper.MarksAndNumbersFront14);
			sb.AppendLine(wrapper.MarksAndNumbersFront15);
			sb.AppendLine(wrapper.MarksAndNumbersFront16);
			sb.AppendLine(wrapper.MarksAndNumbersFront17);
			sb.AppendLine(wrapper.MarksAndNumbersFront18);
			return sb.ToString();
		}

		CusContainer AddNewContainerForOtherDeclarationsFrontTest(ICusContainerCollection<CusContainer> containers, JobComInvoiceHeader invHeader, CusEntryHeader entryHeader, ZString containerNum)
		{
			var container = containers.AddNew();
			container.CO_ContainerNumber = containerNum;
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var invoiceLine = invHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.ContainersPivot.AddPivotFor(container);
			return container;
		}

		protected void BasicSetupForTestEntryLines()
		{
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VoyageFlightNo = "V.14";
			declaration.JE_RL_NKPortOfLoading = "TWCYI";
			declaration.PortOfLoading.RL_PortName = "Chiayi";
			declaration.JE_RL_NKFinalDestination = "JPARI";
			declaration.FinalDestination.RL_PortName = "Ariake, Tokyo";
			declaration.JE_HouseBill = "HHH11";
			declaration.JE_MasterBill = "MMM11";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_VesselName = "Exelion";
			declaration.JE_OH_Buyer = buyer.PK;
			declaration.JE_VesselArrivalReg = "06094A";
			declaration.JE_SLD = "ABCD";
			declaration.JE_PaymentMethod = "1";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			declaration.JE_LocationOfGoods = "XXXX0124";
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = "PKG";

			entryInstruction.CEI_BOMPageCount = 10;
			entryInstruction.CEI_Style = "B9";
			entryInstruction.CEI_CustomsOffice = "AA";
			entryInstruction.CEI_IsCoPackaged = true;
			entryInstruction.CEI_PackageDescription = "Package desc.";
			entryInstruction.CEI_ExamMode = "9";
			entryInstruction.CEI_GoodsLocation = "XXXX0123";
			var decDuplicate = entryInstruction.DeclarationDuplicates.AddNew();
			decDuplicate.CY_Type = CusCodeDataTypeList.Codes.DeclarationDuplicate;
			decDuplicate.CY_Code = "5";
			decDuplicate.CY_Data = "1";

			entryHeader.EntryNumber = "AAAB0812300003";

			var invoiceHeader = declaration.Invoices.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;

			invoiceHeader.JZ_MarksAndNumbers = "Marks01";
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceCurrExRate = 31.5m;
			invoiceHeader.JZ_InvoiceAmount = 600000m;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_IncoTerm = new ZString(Core.Constants.IncoTerms.FreeOnBoard);

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Tariff = "10082100005";
			invoiceLine1.JI_Weight = 1000m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine1.JI_NetWeight = 2m;
			invoiceLine1.JI_LinePrice = 600000m;
			invoiceLine1.JI_InvoiceQuantity = 2;
			invoiceLine1.JI_InvoiceUQ = "SET";
			invoiceLine1.JI_HazMatCode = "1110";
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_CustomsQuantity = 2;
			invoiceLine1.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 10m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine1.JI_Procedure = "9U";
			invoiceLine1.JI_PrimaryPreference = "STD";
			invoiceLine1.JI_BondedGoodsCode = "YB";
			invoiceLine1.JI_Group = "group1";
			invoiceLine1.JI_CountryOfOrigin = "TW";
			var doc2 = invoiceLine1.CitesPermitCusSupportingCollection.AddNew();
			doc2.CSI_ReferenceNumber = "REF0011";
			doc2.CSI_LineNo = 1;
			var doc3 = invoiceLine1.CitesPermitCusSupportingCollection.AddNew();
			doc3.CSI_ReferenceNumber = "REF0012";
			doc3.CSI_LineNo = 2;
			var doc4 = invoiceLine1.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			doc4.CSI_ReferenceNumber = "REF0013";

			invoiceLine1.JI_BrandName = "TOTOTA";
			invoiceLine1.JI_CustomsOwnerPartNo = "CPN: 1111";
			invoiceLine1.JI_CustomsSupplierPartNo = "P/N: 1231";
			invoiceLine1.JI_NDescription = "Test Descriptions. line1";
			invoiceLine1.JI_ExtraInfoForClassification = "Extra line1";
			invoiceLine1.JI_Model = "Model line";
			invoiceLine1.JI_Compositions = "Compositions line1";
			invoiceLine1.JI_PreviousEntryNumber = "previousDoc";
			invoiceLine1.JI_PreviousEntryLineNumber = 2222;
			invoiceLine1.PreviousBondedEntryNumber = "LNNE1234567890";
			invoiceLine1.PreviousBondedEntryLineNumber = 2222;
		}

		[TestDate(2019, 08, 30)]
		[ExpectNoExceptions]
		public void TestEntryLines()
		{
			BasicSetupForTestEntryLines();
			var invoiceLine1 = declaration.Invoices.First().InvoiceLines.Cast<JobComInvoiceLine>().First();

			for (short i = 1; i <= 5; i++)
			{
				var addDoc = invoiceLine1.PermitCusSupportingCollection.AddNew();
				addDoc.CSI_ReferenceNumber = "DocA12" + i.ToString();
				addDoc.CSI_LineNo = new ZShort(i);
			}
			var addDocB1 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			addDocB1.JG_ReferenceNumber = "DocB123";
			var addDocB2 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			addDocB2.JG_ReferenceNumber = "DocB223";
			invoiceLine1.CertificateOfOriginNumber = "LN:111";
			invoiceLine1.CertificateOfOriginNumberItemNumber = 11;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			var sequence = new ZStringBuilder();
			var brand = new ZStringBuilder();
			var detail = new ZStringBuilder();
			var code = new ZStringBuilder();
			var price = new ZStringBuilder();
			var weight = new ZStringBuilder();
			var fob = new ZStringBuilder();
			var statistic = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				sequence.Append(section.Box32ItemNumber);
				brand.Append(section.Box33BrandLine1);
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line1);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line2);
				code.Append(section.Box35CCCCode);
				code.Append(section.Box35BondedGoodsCodeAndAssignedNumber);
				price.Append(section.Box36UnitPrice_Line1);
				price.Append(section.Box36UnitPrice_Line2);
				price.Append(section.Box36UnitPrice_Line3);
				price.Append(section.Box36UnitPrice_Line4);
				weight.Append(section.Box37NetWeight);
				weight.Append(section.Box38QuantityAndUnit);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line1);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line2);
				fob.Append(section.Box40FOBValue_Line1);
				fob.Append(section.Box40FOBValue_Line2);
				fob.Append(section.Box40FOBValue_Line3);
				fob.Append(section.Box40FOBValue_Line4);
				statistic.Append(section.Box41ModeOfStatistics_Line1);
				statistic.Append(section.Box41ModeOfStatistics_Line2);
				statistic.Append(section.Box41ModeOfStatistics_Line3);
				statistic.Append(section.Box41ModeOfStatistics_Line4);
			}
			NUnit.Framework.Assert.That(sequence.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


1











"), "SequenceNum:");

			NUnit.Framework.Assert.That(brand.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


TOTOTA











"), "Brand:");

			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"



group1








賣方料號:P/N: 1231
買方料號:CPN: 1111
Test Descriptions. line1
型號:Model line



規格:Compositions line1



原進倉報單號碼/項次:LNNE1234567890-2222



原報單號碼/項次:previousDoc-2222



輸出入許可文件號碼/項次:



DocA123-3



DocA124-4



DocA125-5



產地證明書號碼/項次:LN:111-11



主管機關指定代號:DocB223



生產國別:TW






"), "Details:");

			NUnit.Framework.Assert.That(code.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











DocA121-1
DocA122-2
1008.21.00.00-5
YB/DocB123













































Total:

"), "Codes:");

			NUnit.Framework.Assert.That(price.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











USD
300,000














































        ----------


"), "Prices:");

			NUnit.Framework.Assert.That(weight.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











2 KGM
2 SET














































----------------------
2 KGM
2 SET
vvvvvvvvvvvv"), "Weight:");

			NUnit.Framework.Assert.That(fob.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











<Currency(15000000,TWD)>















































----------------------
<Currency(15000000,TWD)>
vvvvvvvvvv
"), "FOB:");

			NUnit.Framework.Assert.That(statistic.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











9U


















































"), "Statistic:");

			entryInstruction.CEI_Style = "F5";
			invoiceLine1.JI_BondedGoodsCode = "";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			detail = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
			}
			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"



group1








賣方料號:P/N: 1231
買方料號:CPN: 1111
Test Descriptions. line1
型號:Model line



規格:Compositions line1



原進倉報單號碼/項次:LNNE1234567890-2222



原報單號碼/項次:previousDoc-2222



輸出入許可文件號碼/項次:



DocA123-3



DocA124-4



DocA125-5



產地證明書號碼/項次:LN:111-11



主管機關指定代號:DocB223



生產國別:TW






"), "Details:");
		}

		[TestDate(2019, 08, 30)]
		[ExpectNoExceptions]
		public void TestEntryLines2()
		{
			BasicSetupForTestEntryLines();
			var invoiceLine1 = declaration.Invoices.First().InvoiceLines.Cast<JobComInvoiceLine>().First();
			for (short i = 1; i <= 3; i++)
			{
				var addDoc = invoiceLine1.PermitCusSupportingCollection.AddNew();
				addDoc.CSI_ReferenceNumber = "DocA12" + i.ToString();
				addDoc.CSI_LineNo = new ZShort(i);
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			var sequence = new ZStringBuilder();
			var brand = new ZStringBuilder();
			var detail = new ZStringBuilder();
			var code = new ZStringBuilder();
			var price = new ZStringBuilder();
			var weight = new ZStringBuilder();
			var fob = new ZStringBuilder();
			var statistic = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				sequence.Append(section.Box32ItemNumber);
				brand.Append(section.Box33BrandLine1);
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line1);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line2);
				code.Append(section.Box35CCCCode);
				code.Append(section.Box35BondedGoodsCodeAndAssignedNumber);
				price.Append(section.Box36UnitPrice_Line1);
				price.Append(section.Box36UnitPrice_Line2);
				price.Append(section.Box36UnitPrice_Line3);
				price.Append(section.Box36UnitPrice_Line4);
				weight.Append(section.Box37NetWeight);
				weight.Append(section.Box38QuantityAndUnit);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line1);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line2);
				fob.Append(section.Box40FOBValue_Line1);
				fob.Append(section.Box40FOBValue_Line2);
				fob.Append(section.Box40FOBValue_Line3);
				fob.Append(section.Box40FOBValue_Line4);
				statistic.Append(section.Box41ModeOfStatistics_Line1);
				statistic.Append(section.Box41ModeOfStatistics_Line2);
				statistic.Append(section.Box41ModeOfStatistics_Line3);
				statistic.Append(section.Box41ModeOfStatistics_Line4);
			}
			NUnit.Framework.Assert.That(sequence.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


1








"), "SequenceNum:");
			NUnit.Framework.Assert.That(brand.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


TOTOTA








"), "Brand:");
			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"



group1








賣方料號:P/N: 1231
買方料號:CPN: 1111
Test Descriptions. line1
型號:Model line



規格:Compositions line1



原進倉報單號碼/項次:LNNE1234567890-2222



原報單號碼/項次:previousDoc-2222



輸出入許可文件號碼/項次:



DocA123-3



REF0013



生產國別:TW






"), "Details:");
			NUnit.Framework.Assert.That(code.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











DocA121-1
DocA122-2
1008.21.00.00-5
YB

































Total:

"), "Codes:");
			NUnit.Framework.Assert.That(price.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











USD
300,000


































        ----------


"), "Prices:");
			NUnit.Framework.Assert.That(weight.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











2 KGM
2 SET


































----------------------
2 KGM
2 SET
vvvvvvvvvvvv"), "Weight:");
			NUnit.Framework.Assert.That(fob.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











<Currency(15000000,TWD)>



































----------------------
<Currency(15000000,TWD)>
vvvvvvvvvv
"), "FOB:");
			NUnit.Framework.Assert.That(statistic.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











9U






































"), "Statistic:");

			invoiceLine1.CertificateOfOriginNumber = "LN:111";
			invoiceLine1.CertificateOfOriginNumberItemNumber = 11;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			detail = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
			}

			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"



group1








賣方料號:P/N: 1231
買方料號:CPN: 1111
Test Descriptions. line1
型號:Model line



規格:Compositions line1



原進倉報單號碼/項次:LNNE1234567890-2222



原報單號碼/項次:previousDoc-2222



輸出入許可文件號碼/項次:



DocA123-3



REF0013



產地證明書號碼/項次:LN:111-11



生產國別:TW






"), "Details:");
		}

		[TestDate(2019, 08, 30)]
		[ExpectNoExceptions]
		public void TestEntryLines3()
		{
			BasicSetupForTestEntryLines();
			var invoiceLine1 = declaration.Invoices.First().InvoiceLines.Cast<JobComInvoiceLine>().First();
			var addDocB1 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			addDocB1.JG_ReferenceNumber = "DocB123";
			invoiceLine1.JI_BondedGoodsCode = "";
			invoiceLine1.CertificateOfOriginNumber = "LN:111";
			invoiceLine1.CertificateOfOriginNumberItemNumber = 11;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			var sequence = new ZStringBuilder();
			var brand = new ZStringBuilder();
			var detail = new ZStringBuilder();
			var code = new ZStringBuilder();
			var price = new ZStringBuilder();
			var weight = new ZStringBuilder();
			var fob = new ZStringBuilder();
			var statistic = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				sequence.Append(section.Box32ItemNumber);
				brand.Append(section.Box33BrandLine1);
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line1);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line2);
				code.Append(section.Box35CCCCode);
				code.Append(section.Box35BondedGoodsCodeAndAssignedNumber);
				price.Append(section.Box36UnitPrice_Line1);
				price.Append(section.Box36UnitPrice_Line2);
				price.Append(section.Box36UnitPrice_Line3);
				price.Append(section.Box36UnitPrice_Line4);
				weight.Append(section.Box37NetWeight);
				weight.Append(section.Box38QuantityAndUnit);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line1);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line2);
				fob.Append(section.Box40FOBValue_Line1);
				fob.Append(section.Box40FOBValue_Line2);
				fob.Append(section.Box40FOBValue_Line3);
				fob.Append(section.Box40FOBValue_Line4);
				statistic.Append(section.Box41ModeOfStatistics_Line1);
				statistic.Append(section.Box41ModeOfStatistics_Line2);
				statistic.Append(section.Box41ModeOfStatistics_Line3);
				statistic.Append(section.Box41ModeOfStatistics_Line4);
			}
			NUnit.Framework.Assert.That(sequence.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


1





"), "SequenceNum:");
			NUnit.Framework.Assert.That(brand.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


TOTOTA





"), "Brand:");
			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"



group1








賣方料號:P/N: 1231
買方料號:CPN: 1111
Test Descriptions. line1
型號:Model line



規格:Compositions line1



原進倉報單號碼/項次:LNNE1234567890-2222



原報單號碼/項次:previousDoc-2222



生產國別:TW






"), "Details:");
			NUnit.Framework.Assert.That(code.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











REF0013
LN:111-11
1008.21.00.00-5
DocB123





















Total:

"), "Codes:");
			NUnit.Framework.Assert.That(price.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











USD
300,000






















        ----------


"), "Prices:");
			NUnit.Framework.Assert.That(weight.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











2 KGM
2 SET






















----------------------
2 KGM
2 SET
vvvvvvvvvvvv"), "Weight:");
			NUnit.Framework.Assert.That(fob.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











<Currency(15000000,TWD)>























----------------------
<Currency(15000000,TWD)>
vvvvvvvvvv
"), "FOB:");
			NUnit.Framework.Assert.That(statistic.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











9U


























"), "Statistic:");

			addDocB1.JG_ReferenceNumber = "";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			code = new ZStringBuilder();
			detail = new ZStringBuilder();
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line1);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line2);
				code.Append(section.Box35CCCCode);
				code.Append(section.Box35BondedGoodsCodeAndAssignedNumber);
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
			}

			NUnit.Framework.Assert.That(code.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











REF0013
LN:111-11
1008.21.00.00-5






















Total:

"), "Codes:");
			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"



group1








賣方料號:P/N: 1231
買方料號:CPN: 1111
Test Descriptions. line1
型號:Model line



規格:Compositions line1



原進倉報單號碼/項次:LNNE1234567890-2222



原報單號碼/項次:previousDoc-2222



生產國別:TW






"), "Details:");
		}

		[ExpectNoExceptions]
		public void TestSecondQuantity()
		{
			BasicSetupForTestEntryLines();
			var invoiceLine1 = declaration.Invoices.First().InvoiceLines.Cast<JobComInvoiceLine>().First();
			invoiceLine1.JI_Tariff = "12119067004";
			invoiceLine1.JI_Weight = 1000m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine1.JI_NetWeight = 2m;
			invoiceLine1.JI_InvoiceQuantity = 2;
			invoiceLine1.JI_InvoiceUQ = "PKG";
			invoiceLine1.JI_CustomsSecondQuantity = 4;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_CustomsQuantity = 2;
			invoiceLine1.JI_CustomsSecondUnitQty = "PCE";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			var weight = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				weight.Append(section.Box37NetWeight);
				weight.Append(section.Box38QuantityAndUnit);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line1);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line2);
			}

			NUnit.Framework.Assert.That(weight.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











2 KGM
2 PKG
(4 PCE)





















----------------------
2 KGM
2 PKG
(4 PCE)
vvvvvvvvvvvv


"), "Weight:");
		}

		[ExpectNoExceptions]
		public void TestSupplierPartNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CustomsSupplierPartNo = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Supplier Part Number in detail", ZString.Empty);

			invoiceLine.JI_CustomsSupplierPartNo = "Test Supplier Part No";
			AssertTextInDetail(entryHeader, "Supplier Part Number in detail", "賣方料號:Test Supplier Part No");

			var goodsDescriptionsOptions = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionsOptions.Remove(goodsDescriptionsOptions.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber));
			AssertTextInDetail(entryHeader, "do not have Supplier Part Number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestOwnerPartNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_CustomsOwnerPartNo = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Owner Part Number in detail", ZString.Empty);

			invoiceLine.JI_CustomsOwnerPartNo = "Test Owner Part No";
			AssertTextInDetail(entryHeader, "Owner Part No in detail", "買方料號:Test Owner Part No");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.OwnerPartNumber));
			AssertTextInDetail(entryHeader, "do not have Owner Part No in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestDeclarationGoodsDescriptionInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_NDescription = ZString.Empty;
			invoiceLine.JI_Description = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Declaration Goods Description in detail", ZString.Empty);

			invoiceLine.JI_NDescription = "測試中文描述";
			invoiceLine.JI_Description = "Test English Description";
			AssertTextInDetail(entryHeader, "Declaration Goods Description in detail", @"測試中文描述
Test English Description");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.GoodsDescription));
			AssertTextInDetail(entryHeader, "do not have Declaration Goods Description in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestModelInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_Model = ZString.Empty;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Model in detail", ZString.Empty);

			invoiceLine.JI_Model = "Test Model";
			AssertTextInDetail(entryHeader, "Model in detail", "型號:Test Model");

			invoiceLine.JI_Model = "Test long long long long long Model";
			AssertTextInDetail(entryHeader, "long Model in detail", @"型號:
Test long long long long long Model");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.Model));
			AssertTextInDetail(entryHeader, "do not have Model in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestSpecificationInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Specification in detail", ZString.Empty);

			invoiceLine.JI_Compositions = "Test Specification";
			AssertTextInDetail(entryHeader, "Specification in detail", "規格:Test Specification");

			invoiceLine.JI_Compositions = "Test long long long long Specification";
			AssertTextInDetail(entryHeader, "long Specification in detail", @"規格:
Test long long long long Specification");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.Specification));
			AssertTextInDetail(entryHeader, "do not have Specification in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestPreviousBondedEntryNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.PreviousBondedEntryNumber = ZString.Empty;
			invoiceLine.PreviousBondedEntryLineNumber = ZInt.Zero;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Previous Bonded Entry Number in detail", ZString.Empty);

			invoiceLine.PreviousBondedEntryNumber = "Test1234567890";
			AssertTextInDetail(entryHeader, "do not have Previous Bonded Entry Number in detail", ZString.Empty);

			invoiceLine.PreviousBondedEntryLineNumber = 2;
			AssertTextInDetail(entryHeader, "Previous Bonded Entry Number in detail", "原進倉報單號碼/項次:Test1234567890-2");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			var previousBondedEntryNumberOption = goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber);
			previousBondedEntryNumberOption.Caption = "原進倉報單號碼/項次1111111:";
			AssertTextInDetail(entryHeader, "long Previous Bonded Entry Number caption in detail", @"原進倉報單號碼/項次1111111:
Test1234567890-2");

			goodsDescriptionConfigs.Remove(previousBondedEntryNumberOption);
			AssertTextInDetail(entryHeader, "do not have Previous Bonded Entry Number in detail", "");
		}

		[ExpectNoExceptions]
		public void TestPreviousEntryNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine.JI_PreviousEntryNumber = ZString.Empty;
			invoiceLine.JI_PreviousEntryLineNumber = ZShort.Zero;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Previous Entry Number in detail", ZString.Empty);

			invoiceLine.JI_PreviousEntryNumber = "MMMMMMMMMMMMMM";
			invoiceLine.JI_PreviousEntryLineNumber = 2233;
			AssertTextInDetail(entryHeader, "Previous Entry Number in detail", "原報單號碼/項次:MMMMMMMMMMMMMM-2233");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			var previousEntryNumberOption = goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.PreviousEntryNumber);
			previousEntryNumberOption.Caption = "原報單號碼/項次1111111:";
			AssertTextInDetail(entryHeader, "long Previous Entry Number caption in detail", @"原報單號碼/項次1111111:
MMMMMMMMMMMMMM-2233");

			goodsDescriptionConfigs.Remove(previousEntryNumberOption);
			AssertTextInDetail(entryHeader, "do not have Previous Entry Number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestPermitNumberInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			var permitCusSupportingCollection = invoiceLine.PermitCusSupportingCollection;
			for (short i = 1; i <= 2; i++)
			{
				var addDoc = permitCusSupportingCollection.AddNew();
				addDoc.CSI_ReferenceNumber = "DocA12" + i.ToString();
				addDoc.CSI_LineNo = new ZShort(i);
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have permit number in detail", ZString.Empty);

			var addDoc3 = permitCusSupportingCollection.AddNew();
			addDoc3.CSI_ReferenceNumber = "DocA123111111";
			addDoc3.CSI_LineNo = new ZShort(3);

			AssertTextInDetail(entryHeader, "single permit number in detail", "輸出入許可文件號碼/項次:DocA123111111-3");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			var permitsEntryNumberOption = goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.Permits);
			permitsEntryNumberOption.Caption = "輸出入許可文件號碼/項次1:";
			addDoc3.CSI_ReferenceNumber = "DocA1231111111";
			AssertTextInDetail(entryHeader, "long permit number in detail", @"輸出入許可文件號碼/項次1:
DocA1231111111-3");

			var addDoc4 = permitCusSupportingCollection.AddNew();
			addDoc4.CSI_ReferenceNumber = "DocA124";
			addDoc4.CSI_LineNo = new ZShort(4);

			AssertTextInDetail(entryHeader, "miltiple permit numbers in detail", @"輸出入許可文件號碼/項次1:
DocA1231111111-3
DocA124-4");

			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.Permits));
			AssertTextInDetail(entryHeader, "do not have permit number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestCertificateOfOriginInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			var permitCusSupportingCollection = invoiceLine.PermitCusSupportingCollection;
			var permit1 = permitCusSupportingCollection.AddNew();
			permit1.CSI_ReferenceNumber = "Doc1";
			permit1.CSI_LineNo = 1;
			AssertTextInDetail(entryHeader, "do not have Certificate Of Origin in detail", ZString.Empty);

			invoiceLine.CertificateOfOriginNumber = "TEST123456789";
			AssertTextInDetail(entryHeader, "do not have Certificate Of Origin in detail", ZString.Empty);

			var permit2 = permitCusSupportingCollection.AddNew();
			permit2.CSI_ReferenceNumber = "Doc2";
			permit2.CSI_LineNo = 2;
			AssertTextInDetail(entryHeader, "Certificate Of Origin in detail", "產地證明書號碼/項次:TEST123456789");

			invoiceLine.CertificateOfOriginNumberItemNumber = 3;
			AssertTextInDetail(entryHeader, "Certificate Of Origin in detail", @"產地證明書號碼/項次:TEST123456789-3");

			invoiceLine.CertificateOfOriginNumber = "TEST1234567890000000000000000000000";
			AssertTextInDetail(entryHeader, "Long Certificate Of Origin in detail", @"產地證明書號碼/項次:
TEST1234567890000000000000000000000-3");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.CertificateOfOrigin));
			AssertTextInDetail(entryHeader, "do not have Certificate Of Origin in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestAssignedNumbersInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			var assignedJobComInvLineRefsCollection = invoiceLine.AssignedJobComInvLineRefsCollection;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Assigned Number in detail", ZString.Empty);

			var assignedNumber1 = assignedJobComInvLineRefsCollection.AddNew();
			assignedNumber1.JG_ReferenceNumber = "test number 1";

			AssertTextInDetail(entryHeader, "do not have Assigned Number in detail", ZString.Empty);

			var assignedNumber2 = assignedJobComInvLineRefsCollection.AddNew();
			assignedNumber2.JG_ReferenceNumber = "test number 2";
			AssertTextInDetail(entryHeader, "single Assigned Number in detail", "主管機關指定代號:test number 2");

			assignedNumber2.JG_ReferenceNumber = "test long long long number 2";
			AssertTextInDetail(entryHeader, "long Assigned number in detail", @"主管機關指定代號:
test long long long number 2");

			assignedNumber2.JG_ReferenceNumber = "test number 2";
			var assignedNumber3 = assignedJobComInvLineRefsCollection.AddNew();
			assignedNumber3.JG_ReferenceNumber = "test number 3";

			AssertTextInDetail(entryHeader, "miltiple Assigned Numbers in detail", @"主管機關指定代號:
test number 2
test number 3");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.AssignedNumbers));
			AssertTextInDetail(entryHeader, "do not have Assigned Number in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestGoodsOriginInDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = TWJobMessageTypeList.Codes.Export;
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var jobDeclarationDocumentAddressConfig = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			AssertTextInDetail(entryHeader, "do not have Goods Origin in detail", ZString.Empty);

			invoiceLine.JI_CountryOfOrigin = "TW";
			AssertTextInDetail(entryHeader, "Goods Origin in detail", "生產國別:TW");

			var goodsDescriptionConfigs = jobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
			goodsDescriptionConfigs.Remove(goodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(c => c.Field == ExportDeclarationDocumentFieldList.Codes.GoodsOrigin));
			AssertTextInDetail(entryHeader, "do not have Goods Origin in detail", ZString.Empty);
		}

		[ExpectNoExceptions]
		void AssertTextInDetail(CusEntryHeader entryHeader, ZString message, ZString expectedText)
		{
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var detail = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
			}
			NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends().Trim(), NUnit.Framework.Is.EqualTo(expectedText).Using(CustomComparers.TypeComparison), message.ToString());
		}

		[ExpectNoExceptions]
		public virtual void TestN5203Declaration()
		{
			NUnit.Framework.Assert.That(GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory).N5203Declaration.GetType(), NUnit.Framework.Is.EqualTo(typeof(N5203MessageSendingObject)));
		}

		[ExpectNoExceptions]
		public void TestDeclarationIDBarCode()
		{
			entryHeader.EntryNumber = "CA  0958000143";
			entryHeader.CH_Status = "AWO";
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.DeclarationIDBarCode, NUnit.Framework.Is.EqualTo("*CA  0958000143*").Using(CustomComparers.TypeComparison));
		}

		protected abstract TExportCustomsDeclarationDocumentWrapper GetExportCustomsDeclarationDocumentWrapper(CusEntryHeader cusEntryHeader, BusinessObjectFactory factory);

		void SetupSupplier()
		{
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "TWKEL";
			supplier.OH_FullName = "Supplier company name.";
			var supplierOrgAddress = supplier.Addresses.AddNew();
			supplierOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			supplierOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			supplierOrgAddress.OA_IsActive = true;
			supplierOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			supplierOrgAddress.OA_Address1 = "88899 address line1.";
			supplierOrgAddress.OA_Address2 = "88899 address line2.";
			var supplierTranslatedAddress = supplierOrgAddress.TranslatedAddresses.AddNew();
			supplierTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			supplierTranslatedAddress.Address1 = "Taipei Minsheng E.Rd.";
			supplierTranslatedAddress.Address2 = "Taipei Minsheng W.Rd.";
			supplierTranslatedAddress.CompanyName = "Supplier company name(OTA).";
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "5555555", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "6666666", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "777777", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.TPC, "888888", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "999999", Core.Constants.CountryCodes.Taiwan);
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "98555445", Core.Constants.CountryCodes.Taiwan);
		}
		void SetupBuyer()
		{
			buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer company name.";
			buyer.OH_RL_NKClosestPort = "TWKEL";
			buyer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "B111111", Core.Constants.CountryCodes.Taiwan);
			buyer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "B222222", Core.Constants.CountryCodes.Taiwan);
			buyer.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "B333333", Core.Constants.CountryCodes.Taiwan);
			buyer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "BCPW001", Core.Constants.CountryCodes.Taiwan);
			buyer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "BCCP001", Core.Constants.CountryCodes.Taiwan);
			buyer.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.AEO, "BAEO001", Core.Constants.CountryCodes.Taiwan);
			var buyerOrgAddress = buyer.Addresses.AddNew();
			buyerOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			buyerOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			buyerOrgAddress.OA_IsActive = true;
			buyerOrgAddress.OA_Language = Core.SharedConstants.Languages.English;
			buyerOrgAddress.OA_Address1 = "12345 address line1.";
			buyerOrgAddress.OA_Address2 = "12345 address line2.";
			buyerOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var buyerTranslatedAddress = buyerOrgAddress.TranslatedAddresses.AddNew();
			buyerTranslatedAddress.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			buyerTranslatedAddress.Address1 = "No. 195, Sec. 3, Jianguo N. Rd.,";
			buyerTranslatedAddress.Address2 = "Zhongshan Dist., Taipei City 104, Taiwan (R.O.C.)";
			buyerTranslatedAddress.CompanyName = "Buyer company name(OTA).";
		}
		void SetupBroker()
		{
			broker = Factory.NewWithValidTestData<GlbStaff>();
			var brkCertificate = broker.Certificates.AddNew();
			brkCertificate.XZ_Type = CertificateTypePairList.Codes.BR1;
			brkCertificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			brkCertificate.XZ_RefNumber = "1234";
		}
		void SetUpGoodsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "XXXX0124", "WWWWWW", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
		void SetupCustomsProcedure()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeDTY = referenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateTypeCOM = referenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			var refCusRateTypeSSG = referenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "SSG");

			var refCusProcedure = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "9U", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);

			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "HWS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTA", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "SSG", refCusRateTypeSSG.PK);

			var startDate = ZDate.Today.AddMonths(-3);
			var endDate = ZDate.Today.AddMonths(3);
			referenceDataHelper.CreateTaxOrFee("DDF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("VAT", 0.15000000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);

			refCusProcedure.Attributes.AddNew("COMPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("DTYPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("SSGPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("TATPaymentMethod", "DEF");
			refCusProcedure.Attributes.AddNew("TPFPaymentMethod", "CAS");
			refCusProcedure.Attributes.AddNew("VATPaymentMethod", "DEF");
			Factory.Save();
		}
		void SetupVessel()
		{
			vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "Gabriela2";
			vessel1.RV_LloydsNumber = "Lloyds";
			vessel1.RV_RadioCallSign = "CallSign";
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocumentsLine1()
		{
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_Description = "Entry Line 1";
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var firstPermitNumber = invoiceLine.PermitCusSupportingCollection.AddNew();
			firstPermitNumber.CSI_ReferenceNumber = "A123456";
			firstPermitNumber.CSI_LineNo = 7;
			invoiceLine.CertificateOfOriginNumber = "C567890";
			invoiceLine.CertificateOfOriginNumberItemNumber = 8;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var firstSection = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().First();
			NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("A123456-7").Using(CustomComparers.TypeComparison));

			invoiceLine.PermitCusSupportingCollection.RemoveAndDeleteAll();
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			firstSection = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().First();
			NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("C567890-8").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocumentsLine2()
		{
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_Description = "Entry Line 1";
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var firstPermitNumber = invoiceLine.PermitCusSupportingCollection.AddNew();
			firstPermitNumber.CSI_ReferenceNumber = "A123456";
			firstPermitNumber.CSI_LineNo = 5;

			var secondPermitNumber = invoiceLine.PermitCusSupportingCollection.AddNew();
			secondPermitNumber.CSI_ReferenceNumber = "B123456";
			secondPermitNumber.CSI_LineNo = 6;

			invoiceLine.CertificateOfOriginNumber = "C567890";
			invoiceLine.CertificateOfOriginNumberItemNumber = 8;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var firstSection = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().First();
			NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("B123456-6").Using(CustomComparers.TypeComparison));

			invoiceLine.PermitCusSupportingCollection.Remove(secondPermitNumber);
			wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			firstSection = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().First();
			NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("C567890-8").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public virtual void TestPrintOriginAdditionalDocumentOnGoodsDescriptionWhenNotPrintedOnCodes()
		{
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_Description = "Entry Line 1";
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var firstPermitNumber = invoiceLine.PermitCusSupportingCollection.AddNew();
			firstPermitNumber.CSI_ReferenceNumber = "A123456";
			firstPermitNumber.CSI_LineNo = 5;

			var secondPermitNumber = invoiceLine.PermitCusSupportingCollection.AddNew();
			secondPermitNumber.CSI_ReferenceNumber = "B123456";
			secondPermitNumber.CSI_LineNo = 6;

			invoiceLine.CertificateOfOriginNumber = "C567890";
			invoiceLine.CertificateOfOriginNumberItemNumber = 8;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var firstSection = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().First();
			CombineAssertions("grouping for the first line", () =>
			{
				NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("A123456-5").Using(CustomComparers.TypeComparison), "firstSection.Box34ImportExportPermitNumberAndItemNumber_Line1");
				NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("B123456-6").Using(CustomComparers.TypeComparison), "firstSection.Box34ImportExportPermitNumberAndItemNumber_Line2");
				NUnit.Framework.Assert.That(firstSection.Box33DescriptionOfGoods_Line3, NUnit.Framework.Is.EqualTo("產地證明書號碼/項次:C567890-8").Using(CustomComparers.TypeComparison), "firstSection.Box33DescriptionOfGoods_Line3");
				NUnit.Framework.Assert.That(firstSection.Box33DescriptionOfGoods_Line4.ToString(), NUnit.Framework.Is.Null.Or.Empty, "firstSection.Box33DescriptionOfGoods_Line4 - should be [null] or [empty]");
			});
		}

		[ExpectNoExceptions]
		public void TestCommonDecimalPlaces()
		{
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.CommonDecimalPlaces, NUnit.Framework.Is.EqualTo(2).Using(CustomComparers.TypeComparison), "CommonDecimalPlaces should be 2");
		}

		[ExpectNoExceptions]
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(GetExportCustomsDeclarationDocumentWrapper(null, Factory), NUnit.Framework.Is.EqualTo(default(TExportCustomsDeclarationDocumentWrapper)));
				NUnit.Framework.Assert.That(GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory), NUnit.Framework.Is.TypeOf(typeof(TExportCustomsDeclarationDocumentWrapper)));
			});
		}

		[ExpectNoExceptions]
		public void TestCusEntryHeader()
		{
			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			NUnit.Framework.Assert.That(wrapper.CusEntryHeader, NUnit.Framework.Is.EqualTo(entryHeader));
		}
	}
}

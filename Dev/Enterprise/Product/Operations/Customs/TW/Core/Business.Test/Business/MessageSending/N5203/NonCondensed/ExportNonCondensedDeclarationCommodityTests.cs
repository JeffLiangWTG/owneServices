using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ExportNonCondensedDeclarationCommodityTests : CommodityAbstractTests<ExportNonCondensedDeclarationCommodity>
	{
		[ExpectNoExceptions]
		public override void TestAdditionalDocuments()
		{
			var permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			permitCusSupporting.CSI_ReferenceNumber = "XX222214";
			permitCusSupporting.CSI_LineNo = 89;
			NUnit.Framework.Assert.That(Commodity.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public override void TestDescription()
		{
			invoiceLine.JI_Group = "  Short grouping  ";
			invoiceLine.JI_DeclarationGoodsDescription = "  TW_Dec  ";
			var entryLine = invoiceLine.CusEntryLine;
			entryLine.CL_Description = ZString.Empty;
			NUnit.Framework.Assert.That(Commodity.Description, NUnit.Framework.Is.EqualTo("  TW_Dec").Using(CustomComparers.TypeComparison));
			entryLine.CL_Description = "  CL_Des  ";
			NUnit.Framework.Assert.That(Commodity.Description, NUnit.Framework.Is.EqualTo("  TW_Dec").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestClassifications()
		{
			invoiceLine.JI_HazMatCode = "1100";
			invoiceLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1100", "", "IMO").First().PK;
			invoiceLine.Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(Commodity.Classifications.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public override void TestInvoiceLine()
		{
			NUnit.Framework.Assert.That(Commodity.InvoiceLine.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExportNonCondensedDeclarationInvoiceLine)));
		}

		[ExpectNoExceptions]
		public override void TestCommodity()
		{
			NUnit.Framework.Assert.That(Commodity.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExportNonCondensedDeclarationCommodity)));
		}

		protected override ICommodity Commodity => new ExportNonCondensedDeclarationCommodity(EntryLine, invoiceLine);
	}
}

using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ClassificationsTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			var classification = Factory.New<Customs.Business.BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "87149990905";
			invoiceLine.JI_CC = classification.PK;
			NUnit.Framework.Assert.That(Classification.ID, NUnit.Framework.Is.EqualTo("87149990905").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(DangerousGoodsClassification.ID, NUnit.Framework.Is.EqualTo("1100").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIdentificationTypeCode()
		{
			var declaration = invoiceLine.Declaration;
			NUnit.Framework.Assert.That(Classification.IdentificationTypeCode, NUnit.Framework.Is.EqualTo("HS").Using(CustomComparers.TypeComparison));
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			NUnit.Framework.Assert.That(DangerousGoodsClassification.IdentificationTypeCode, NUnit.Framework.Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison));
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(DangerousGoodsClassification.IdentificationTypeCode, NUnit.Framework.Is.EqualTo("SSO").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
			invoiceLine.Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoiceLine.JI_HazMatCode = "1100";
			invoiceLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1100", "", "IMO").First().PK;
		}

		CusEntryHeader entryHeader;
		CusEntryLine EntryLine => entryHeader.MergedLines.Cast<CusEntryLine>().First();
		ICommodity commodity => new Commodity(EntryLine, invoiceLine);
		JobComInvoiceLine invoiceLine;
		IClassification Classification => commodity.Classifications.First();
		IClassification DangerousGoodsClassification => commodity.Classifications.First(x => x.IdentificationTypeCode != "HS");
	}
}

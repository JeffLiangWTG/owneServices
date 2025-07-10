using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class NX5105GovernmentAgencyGoodsItem_GoodsMeasureAbstractTest<TGoodsMeasure> : TestCaseWithFactory
		where TGoodsMeasure : IGoodsMeasure
	{
		[ExpectNoExceptions]
		public virtual void TestGovernmentAgencyGoodsItem_GoodsMeasure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var instruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine1.JI_NetWeight = 100m;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_NetWeight = 2m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			var goodsMeasure = GetGoodsMeasure(entryLine1);
			NUnit.Framework.Assert.That(goodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(entryLine1.EffectiveNetWeight.InKilogramsSafe), "GoodItem.NetWeightMeasure should be equal to EffectiveNetWeight in kilograms");
			NUnit.Framework.Assert.That(goodsMeasure.NetWeightMeasure, NUnit.Framework.Is.EqualTo(2100m).Using(CustomComparers.TypeComparison), "GoodItem.NetWeightMeasure should be ");
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine2.JI_InvoiceQuantity = 200m;
			NUnit.Framework.Assert.That(goodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(300m).Using(CustomComparers.TypeComparison), "GoodItem.TariffQuantity should be ");
			invoiceLine1.JI_InvoiceUQ = "AAA";
			invoiceLine2.JI_InvoiceUQ = "AAA";
			NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "GoodItem.UnitCode should be ");
			declaration.JE_MergeBy = MergeByCodeList.Codes.CondensedDeclaration;
			NUnit.Framework.Assert.That(goodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo("LOT").Using(CustomComparers.TypeComparison), "GoodItem.UnitCode should be ");
		}

		public void TestCheckArgumentsNotNull()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var instruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				GetGoodsMeasure(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				GetGoodsMeasure(entryLine1);
			}

			);
		}

		protected abstract TGoodsMeasure GetGoodsMeasure(CusEntryLine entryLine);
	}
}

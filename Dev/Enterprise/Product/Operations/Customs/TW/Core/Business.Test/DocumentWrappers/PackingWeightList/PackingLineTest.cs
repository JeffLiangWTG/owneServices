using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingLine))]
	sealed class PackingLineTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestPackingLinePropertiesWhenBothPackQtyAndInvoiceQuantityAreOne()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "BAG";
			package.CW_MarksAndNos = "MARK1";
			package.CW_NetWeight = 10.001;
			package.CW_NetWeightUQ = "KG";
			package.CW_GrossWeight = 10.001;
			package.CW_GrossWeightUQ = "KG";
			package.CW_Volume = 10.001;
			package.CW_VolumeUQ = "L";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var description = "invoice Line 1";
			invoiceLine.JI_NDescription = description;
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "PCS";
			var pivot = (ICusQuantityPivot)supporter.CusPackPivots.AddPivotFor(package);
			pivot.Quantity = 1;
			var packingLine = new PackingLine(package, Factory);
			NUnit.Framework.Assert.That(packingLine.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.QuantityInfo, NUnit.Framework.Is.EqualTo("1 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.NetWeightInfo, NUnit.Framework.Is.EqualTo("10.001 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GrossWeightInfo, NUnit.Framework.Is.EqualTo("10.001 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"10.001 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeUQ, NUnit.Framework.Is.EqualTo(@"L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description}").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestVolumeUQAndVolumeInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "BAG";
			package.CW_MarksAndNos = "MARK1";
			package.CW_NetWeight = 10.001;
			package.CW_NetWeightUQ = "KG";
			package.CW_GrossWeight = 10.001;
			package.CW_GrossWeightUQ = "KG";
			package.CW_Volume = 10.001;
			package.CW_VolumeUQ = "L";
			package.CW_Length = 1.10M;
			package.CW_Width = 1.2M;
			package.CW_Height = 1.0M;
			package.CW_DimensionUQ = "CM";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var description = "invoice Line 1";
			invoiceLine.JI_NDescription = description;
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "PCS";
			var pivot = (ICusQuantityPivot)supporter.CusPackPivots.AddPivotFor(package);
			pivot.Quantity = 1;
			var packingLine = new PackingLine(package, Factory);
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"10.001 L
1.1*1.2*1 CM³").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeUQ, NUnit.Framework.Is.EqualTo(@"L").Using(CustomComparers.TypeComparison));
			package.CW_VolumeUQ = "M3";
			NUnit.Framework.Assert.That(packingLine.VolumeUQ, NUnit.Framework.Is.EqualTo(@"CBM").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"10.001 CBM
1.1*1.2*1 CM³").Using(CustomComparers.TypeComparison));
			package.CW_VolumeUQ = "CF";
			NUnit.Framework.Assert.That(packingLine.VolumeUQ, NUnit.Framework.Is.EqualTo(@"Cuft").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"10.001 Cuft
1.1*1.2*1 CM³").Using(CustomComparers.TypeComparison));
			package.CW_Length = 0.10M;
			package.CW_Width = 1.5M;
			package.CW_Height = 2.0M;
			package.CW_DimensionUQ = "PM";
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"10.001 Cuft
0.1*1.5*2 PM³").Using(CustomComparers.TypeComparison));
			package.CW_Volume = 0M;
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"0.1*1.5*2 PM³").Using(CustomComparers.TypeComparison));
			package.CW_Volume = 12M;
			package.CW_Length = 0M;
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"12 Cuft").Using(CustomComparers.TypeComparison));
			package.CW_Length = 1.5M;
			package.CW_Width = 0M;
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"12 Cuft").Using(CustomComparers.TypeComparison));
			package.CW_Width = 1.5M;
			package.CW_Height = 0M;
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"12 Cuft").Using(CustomComparers.TypeComparison));
			package.CW_Height = 1.5M;
			package.CW_DimensionUQ = "";
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"12 Cuft").Using(CustomComparers.TypeComparison));
			package.CW_Volume = 0M;
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLinePropertiesAverageCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 2;
			package.CW_PackType = "BAG";
			package.CW_MarksAndNos = "MARK1";
			package.CW_NetWeight = 10.002;
			package.CW_NetWeightUQ = "KG";
			package.CW_GrossWeight = 10.002;
			package.CW_GrossWeightUQ = "KG";
			package.CW_Volume = 10.002;
			package.CW_VolumeUQ = "L";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var description = "invoice Line 1";
			invoiceLine.JI_NDescription = description;
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "PCS";
			var pivot = (ICusQuantityPivot)supporter.CusPackPivots.AddPivotFor(package);
			pivot.Quantity = 1;
			var packingLine = new PackingLine(package, Factory);
			NUnit.Framework.Assert.That(packingLine.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@0.5 PCS
1 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@5.001 KG
10.002 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@5.001 KG
10.002 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@5.001 L
10.002 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description}").Using(CustomComparers.TypeComparison));
			package.CW_Volume = 4123123.002;
			packingLine = new PackingLine(package, Factory);
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@2,061,561.501 L
4,123,123.002 L").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLineWhenMarkHasMultipleInvoiceLinesAndNoAverageCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "BAG";
			package.CW_MarksAndNos = "MARK1";
			package.CW_NetWeight = 10;
			package.CW_NetWeightUQ = "KG";
			package.CW_GrossWeight = 10;
			package.CW_GrossWeightUQ = "KG";
			package.CW_Volume = 10;
			package.CW_VolumeUQ = "L";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var supporter1 = (ICusLinkPackageSupporter)invoiceLine1;
			var description1 = "invoice Line 1 package 1 item";
			invoiceLine1.JI_NDescription = description1;
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package);
			pivot1.Quantity = 1;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			var supporter2 = (ICusLinkPackageSupporter)invoiceLine2;
			var description2 = "invoice Line 2";
			invoiceLine2.JI_NDescription = description2;
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			var pivot2 = (ICusQuantityPivot)supporter2.CusPackPivots.AddPivotFor(package);
			pivot2.Quantity = 2;
			var packingLine = new PackingLine(package, Factory);
			NUnit.Framework.Assert.That(packingLine.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.QuantityInfo, NUnit.Framework.Is.EqualTo("1 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.NetWeightInfo, NUnit.Framework.Is.EqualTo("10 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GrossWeightInfo, NUnit.Framework.Is.EqualTo("10 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"10 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GroupPackingLines[0].QuantityInfo, NUnit.Framework.Is.EqualTo("2 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GroupPackingLines[0].GoodsDescription, NUnit.Framework.Is.EqualTo($"{description2}").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLineWhenMarkHasMultipleInvoiceLinesAndRequireAverageCalculation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 5;
			package.CW_PackType = "BAG";
			package.CW_MarksAndNos = "MARK1";
			package.CW_NetWeight = 10;
			package.CW_NetWeightUQ = "KG";
			package.CW_GrossWeight = 10;
			package.CW_GrossWeightUQ = "KG";
			package.CW_Volume = 10;
			package.CW_VolumeUQ = "L";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var supporter1 = (ICusLinkPackageSupporter)invoiceLine1;
			var description1 = "invoice Line 1";
			invoiceLine1.JI_NDescription = description1;
			invoiceLine1.JI_InvoiceQuantity = 20;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package);
			pivot1.Quantity = 20;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			var supporter2 = (ICusLinkPackageSupporter)invoiceLine2;
			var description2 = "invoice Line 2";
			invoiceLine2.JI_NDescription = description2;
			invoiceLine2.JI_InvoiceQuantity = 21;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			var pivot2 = (ICusQuantityPivot)supporter2.CusPackPivots.AddPivotFor(package);
			pivot2.Quantity = 10;
			var packingLine = new PackingLine(package, Factory);
			NUnit.Framework.Assert.That(packingLine.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@4 PCS
20 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@2 KG
10 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@2 KG
10 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@2 L
10 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GroupPackingLines[0].QuantityInfo, NUnit.Framework.Is.EqualTo(@"@2 PCS
10 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine.GroupPackingLines[0].GoodsDescription, NUnit.Framework.Is.EqualTo($"{description2}").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLineWhenMarkHasMultipleInvoiceLinesAndRequireAverageCalculationAndNoMergeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 2;
			package1.CW_PackType = "BAG";
			package1.CW_MarksAndNos = "MARK1";
			package1.CW_NetWeight = 300;
			package1.CW_NetWeightUQ = "KG";
			package1.CW_GrossWeight = 300;
			package1.CW_GrossWeightUQ = "KG";
			package1.CW_Volume = 300;
			package1.CW_VolumeUQ = "L";
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "BAG";
			package2.CW_MarksAndNos = "MARK2";
			package2.CW_NetWeight = 400;
			package2.CW_NetWeightUQ = "KG";
			package2.CW_GrossWeight = 400;
			package2.CW_GrossWeightUQ = "KG";
			package2.CW_Volume = 400;
			package2.CW_VolumeUQ = "KL";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var supporter1 = (ICusLinkPackageSupporter)invoiceLine1;
			var description1 = "invoice Line 1";
			invoiceLine1.JI_NDescription = description1;
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package1);
			pivot1.Quantity = 4;
			var pivot2 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package2);
			pivot2.Quantity = 1;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			var supporter2 = (ICusLinkPackageSupporter)invoiceLine2;
			var description2 = "invoice Line 2";
			invoiceLine2.JI_NDescription = description2;
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			var pivot3 = (ICusQuantityPivot)supporter2.CusPackPivots.AddPivotFor(package2);
			pivot3.Quantity = 2;
			var packingLine1 = new PackingLine(package1, Factory);
			NUnit.Framework.Assert.That(packingLine1.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@2 PCS
4 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@150 L
300 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			var packingLine2 = new PackingLine(package2, Factory);
			NUnit.Framework.Assert.That(packingLine2.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@0.5 PCS
1 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@200 KL
400 KL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GroupPackingLines[0].QuantityInfo, NUnit.Framework.Is.EqualTo(@"@1 PCS
2 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GroupPackingLines[0].GoodsDescription, NUnit.Framework.Is.EqualTo($"{description2}").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLineWhenMarkHasMultipleInvoiceLinesAndRequireAverageCalculationAndMergeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 2;
			package1.CW_PackType = "BAG";
			package1.CW_MarksAndNos = "MARK1";
			package1.CW_NetWeight = 300;
			package1.CW_NetWeightUQ = "KG";
			package1.CW_GrossWeight = 300;
			package1.CW_GrossWeightUQ = "KG";
			package1.CW_Volume = 300;
			package1.CW_VolumeUQ = "L";
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "BAG";
			package2.CW_MarksAndNos = "MARK2";
			package2.CW_NetWeight = 400;
			package2.CW_NetWeightUQ = "KG";
			package2.CW_GrossWeight = 400;
			package2.CW_GrossWeightUQ = "KG";
			package2.CW_Volume = 400;
			package2.CW_VolumeUQ = "KL";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var supporter1 = (ICusLinkPackageSupporter)invoiceLine1;
			var description1 = "invoice Line 1";
			invoiceLine1.JI_NDescription = description1;
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package1);
			pivot1.Quantity = 4;
			var pivot2 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package2);
			pivot2.Quantity = 1;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			var supporter2 = (ICusLinkPackageSupporter)invoiceLine2;
			var description2 = "invoice Line 1";
			invoiceLine2.JI_NDescription = description2;
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			var pivot3 = (ICusQuantityPivot)supporter2.CusPackPivots.AddPivotFor(package2);
			pivot3.Quantity = 2;
			var packingLine1 = new PackingLine(package1, Factory);
			NUnit.Framework.Assert.That(packingLine1.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@2 PCS
4 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@150 L
300 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			var packingLine2 = new PackingLine(package2, Factory);
			var packingLine3 = packingLine2.GroupPackingLines[0];
			NUnit.Framework.Assert.That(packingLine2.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@0.5 PCS
1 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine3.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@1 PCS
2 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@200 KL
400 KL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLineWithMultipleUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 2;
			package1.CW_PackType = "BAG";
			package1.CW_MarksAndNos = "MARK1";
			package1.CW_NetWeight = 300;
			package1.CW_NetWeightUQ = "KG";
			package1.CW_GrossWeight = 300;
			package1.CW_GrossWeightUQ = "KG";
			package1.CW_Volume = 300;
			package1.CW_VolumeUQ = "L";
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "BAG";
			package2.CW_MarksAndNos = "MARK2";
			package2.CW_NetWeight = 400;
			package2.CW_NetWeightUQ = "KG";
			package2.CW_GrossWeight = 400;
			package2.CW_GrossWeightUQ = "KG";
			package2.CW_Volume = 400;
			package2.CW_VolumeUQ = "KL";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var supporter1 = (ICusLinkPackageSupporter)invoiceLine1;
			var description1 = "invoice Line 1";
			invoiceLine1.JI_NDescription = description1;
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package1);
			pivot1.Quantity = 4;
			var pivot2 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package2);
			pivot2.Quantity = 1;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			var supporter2 = (ICusLinkPackageSupporter)invoiceLine2;
			var description2 = "invoice Line 2";
			invoiceLine2.JI_NDescription = description2;
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCG";
			var pivot3 = (ICusQuantityPivot)supporter2.CusPackPivots.AddPivotFor(package2);
			pivot3.Quantity = 2;
			var packingLine1 = new PackingLine(package1, Factory);
			NUnit.Framework.Assert.That(packingLine1.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@2 PCS
4 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@150 L
300 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			var packingLine2 = new PackingLine(package2, Factory);
			NUnit.Framework.Assert.That(packingLine2.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@0.5 PCS
1 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@200 KL
400 KL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GroupPackingLines[0].QuantityInfo, NUnit.Framework.Is.EqualTo(@"@1 PCG
2 PCG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GroupPackingLines[0].GoodsDescription, NUnit.Framework.Is.EqualTo($"{description2}").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLineWithMultipleUQWithDescriptionMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 2;
			package1.CW_PackType = "BAG";
			package1.CW_MarksAndNos = "MARK1";
			package1.CW_NetWeight = 300;
			package1.CW_NetWeightUQ = "KG";
			package1.CW_GrossWeight = 300;
			package1.CW_GrossWeightUQ = "KG";
			package1.CW_Volume = 300;
			package1.CW_VolumeUQ = "L";
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "BAG";
			package2.CW_MarksAndNos = "MARK2";
			package2.CW_NetWeight = 400;
			package2.CW_NetWeightUQ = "KG";
			package2.CW_GrossWeight = 400;
			package2.CW_GrossWeightUQ = "KG";
			package2.CW_Volume = 400;
			package2.CW_VolumeUQ = "KL";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var supporter1 = (ICusLinkPackageSupporter)invoiceLine1;
			var description1 = "invoice Line 1";
			invoiceLine1.JI_NDescription = description1;
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package1);
			pivot1.Quantity = 4;
			var pivot2 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package2);
			pivot2.Quantity = 1;
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
			var supporter2 = (ICusLinkPackageSupporter)invoiceLine2;
			var description2 = "invoice Line 1";
			invoiceLine2.JI_NDescription = description2;
			invoiceLine2.JI_InvoiceQuantity = 2;
			invoiceLine2.JI_InvoiceUQ = "PCG";
			var pivot3 = (ICusQuantityPivot)supporter2.CusPackPivots.AddPivotFor(package2);
			pivot3.Quantity = 2;
			var packingLine1 = new PackingLine(package1, Factory);
			NUnit.Framework.Assert.That(packingLine1.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@2 PCS
4 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@150 KG
300 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@150 L
300 L").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
			var packingLine2 = new PackingLine(package2, Factory);
			var packingLine3 = packingLine2.GroupPackingLines[0];
			NUnit.Framework.Assert.That(packingLine2.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@0.5 PCS
1 PCS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine3.QuantityInfo, NUnit.Framework.Is.EqualTo(@"@1 PCG
2 PCG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.NetWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GrossWeightInfo, NUnit.Framework.Is.EqualTo(@"@200 KG
400 KG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.VolumeInfo, NUnit.Framework.Is.EqualTo(@"@200 KL
400 KL").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine2.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackingLineWithEmptyValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 2;
			package1.CW_PackType = "BAG";
			package1.CW_MarksAndNos = "MARK1";
			package1.CW_NetWeight = 0;
			package1.CW_NetWeightUQ = "KG";
			package1.CW_GrossWeight = 0;
			package1.CW_GrossWeightUQ = "KG";
			package1.CW_Volume = 0;
			package1.CW_VolumeUQ = "L";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice1.InvoiceLines.AddNew();
			var supporter1 = (ICusLinkPackageSupporter)invoiceLine1;
			var description1 = "invoice Line 1";
			invoiceLine1.JI_NDescription = description1;
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.JI_InvoiceUQ = "PCS";
			var pivot1 = (ICusQuantityPivot)supporter1.CusPackPivots.AddPivotFor(package1);
			pivot1.Quantity = 0;
			var packingLine1 = new PackingLine(package1, Factory);
			NUnit.Framework.Assert.That(packingLine1.PackNoInfo, NUnit.Framework.Is.EqualTo("MARK1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.QuantityInfo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.NetWeightInfo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GrossWeightInfo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.VolumeInfo, NUnit.Framework.Is.EqualTo(@"").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(packingLine1.GoodsDescription, NUnit.Framework.Is.EqualTo($"{description1}").Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "BAG";
			package.CW_MarksAndNos = "MARK1";
			package.CW_NetWeight = 10;
			package.CW_NetWeightUQ = "KG";
			package.CW_GrossWeight = 10;
			package.CW_GrossWeightUQ = "KG";
			package.CW_Volume = 10;
			package.CW_VolumeUQ = "L";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var supporter = (ICusLinkPackageSupporter)invoiceLine;
			var description = "invoice Line 1";
			invoiceLine.JI_NDescription = description;
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "PCS";
			var pivot = (ICusQuantityPivot)supporter.CusPackPivots.AddPivotFor(package);
			pivot.Quantity = 1;
			return new PackingLine(package, Factory);
		}
	}
}

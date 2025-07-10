using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMCommodityTest : NX5105GovernmentAgencyGoodsItem_CommodityAbstractTest<NX5105CMCommodity>
	{
		[ExpectNoExceptions]
		public override void TestCommodity_Description()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = "0000.00.00.00Y";
			line.JI_Description = ZString.Empty;
			line.JI_DeclarationGoodsDescription = "Declaration Goods Description";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			entryLine.CL_Description = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("Declaration Goods Description").Using(CustomComparers.TypeComparison));

			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo(ZString.Empty));

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("Declaration Goods Description").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCommodity_EnglishDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00.00Y";
			invoiceLine.JI_Description = "english desc";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo("english desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo("english desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo("english desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo("english desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo("english desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");
		}

		[ExpectNoExceptions]
		public override void TestCommodity_ChineseDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000.00.00.00Y";
			invoiceLine.JI_NDescription = "chinese desc";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("chinese desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("chinese desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("chinese desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.X101;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("chinese desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("chinese desc").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");

			invoiceLine.JI_NDescription = "NO.008 ";
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");
		}

		[ExpectNoExceptions]
		public void TestCommodity_Food()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Food, NUnit.Framework.Is.EqualTo(default(IFood)));
			invoiceLine.JI_PHValue = "7.5";
			commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Food, NUnit.Framework.Is.TypeOf(typeof(FoodWrapper)));
			NUnit.Framework.Assert.That(commodity.Food.PHValueNumeric, NUnit.Framework.Is.EqualTo(7.5m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.Food.SterilizationValueNumeric, NUnit.Framework.Is.Null);
			NUnit.Framework.Assert.That(commodity.Food.Constituents.Count(), NUnit.Framework.Is.EqualTo(0));
			invoiceLine.JI_PHValue = "";
			invoiceLine.JI_SterilizationValue = "50";
			commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Food, NUnit.Framework.Is.TypeOf(typeof(FoodWrapper)));
			NUnit.Framework.Assert.That(commodity.Food.PHValueNumeric, NUnit.Framework.Is.Null);
			NUnit.Framework.Assert.That(commodity.Food.SterilizationValueNumeric, NUnit.Framework.Is.EqualTo(50m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.Food.Constituents.Count(), NUnit.Framework.Is.EqualTo(0));
			invoiceLine.JI_SterilizationValue = "";
			var foodData = invoiceLine.FoodDataCollection.AddNew();
			foodData.CY_Data = "1111";
			foodData.Content = 200m;
			foodData = invoiceLine.FoodDataCollection.AddNew();
			foodData.CY_Data = "3333";
			foodData.Content = 400m;
			commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Food, NUnit.Framework.Is.TypeOf(typeof(FoodWrapper)));
			NUnit.Framework.Assert.That(commodity.Food.PHValueNumeric, NUnit.Framework.Is.Null);
			NUnit.Framework.Assert.That(commodity.Food.SterilizationValueNumeric, NUnit.Framework.Is.Null);
			var constituents = commodity.Food.Constituents;
			NUnit.Framework.Assert.That(constituents.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(constituents.First().ElementName, NUnit.Framework.Is.EqualTo("1111").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(constituents.First().ElementPercentNumeric, NUnit.Framework.Is.EqualTo(200m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(constituents.ElementAt(1).ElementName, NUnit.Framework.Is.EqualTo("3333").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(constituents.ElementAt(1).ElementPercentNumeric, NUnit.Framework.Is.EqualTo(400m).Using(CustomComparers.TypeComparison));
			invoiceLine.FoodDataCollection.RemoveAndDeleteAll();
			foodData = invoiceLine.FoodDataCollection.AddNew();
			foodData.CY_Data = "1111";
			foodData.Content = 200m;
			foodData = invoiceLine.FoodDataCollection.AddNew();
			foodData.CY_Data = "";
			foodData.Content = 400m;
			commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Food, NUnit.Framework.Is.TypeOf(typeof(FoodWrapper)));
			NUnit.Framework.Assert.That(commodity.Food.PHValueNumeric, NUnit.Framework.Is.Null);
			NUnit.Framework.Assert.That(commodity.Food.SterilizationValueNumeric, NUnit.Framework.Is.Null);
			constituents = commodity.Food.Constituents;
			NUnit.Framework.Assert.That(constituents.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(constituents.First().ElementName, NUnit.Framework.Is.EqualTo("1111").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(constituents.First().ElementPercentNumeric, NUnit.Framework.Is.EqualTo(200m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCommodity_Constituent()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_Compositions = ZString.Empty;
			invoiceLine.JI_ProductGrade = ZString.Empty;
			invoiceLine.JI_ProductThickness = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.Constituent, NUnit.Framework.Is.EqualTo(default(IConstituent)));
			invoiceLine.JI_Compositions = "composition";
			invoiceLine.JI_ProductGrade = "LevelID";
			invoiceLine.JI_ProductThickness = "Thickness";
			var constituent = commodity.Constituent;
			NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("composition").Using(CustomComparers.TypeComparison), "Commodity.Constituent.ElementDescription should be");
			NUnit.Framework.Assert.That(constituent.LevelID, NUnit.Framework.Is.EqualTo("LevelID").Using(CustomComparers.TypeComparison), "Commodity.Constituent.LevelID should be");
			NUnit.Framework.Assert.That(constituent.Thickness, NUnit.Framework.Is.EqualTo("Thickness").Using(CustomComparers.TypeComparison), "Commodity.Constituent.Thickness should be");
		}

		[ExpectNoExceptions]
		public override void TestBarCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_BarCode = "013";
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.BarCode, NUnit.Framework.Is.EqualTo("0130000000000").Using(CustomComparers.TypeComparison), "Commodity.BarCode should be");
			invoiceLine.JI_BarCode = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.BarCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.BarCode should be");
		}

		[ExpectNoExceptions]
		public override void TestCommodity_CommodityRelatedPackaging()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_InnerPackType = ZString.Empty;
			invoiceLine.JI_InnerPackingMaterial = ZString.Empty;
			invoiceLine.JI_InnerPackDescription = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.Constituent, NUnit.Framework.Is.EqualTo(default(IConstituent)));
			invoiceLine.JI_InnerPackType = "1";
			invoiceLine.JI_InnerPackingMaterial = "456";
			invoiceLine.JI_InnerPackDescription = new ZString('A', 200);
			var commodityRelatedPackaging = commodity.CommodityRelatedPackaging;
			NUnit.Framework.Assert.That(commodityRelatedPackaging.PackingMethodDescription, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "Commodity.CommodityRelatedPackaging.PackingMethodDescription should be");
			NUnit.Framework.Assert.That(commodityRelatedPackaging.MaterialCode, NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison), "Commodity.CommodityRelatedPackaging.MaterialCode should be");
			NUnit.Framework.Assert.That(commodityRelatedPackaging.Specification, NUnit.Framework.Is.EqualTo(new ZString('A', 200)), "Commodity.CommodityRelatedPackaging.Specification should be");
		}

		[ExpectNoExceptions]
		public override void TestGoodsGroupNameCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_GoodsType = "333";
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo("333").Using(CustomComparers.TypeComparison), "Commodity.GoodsGroupNameCode should be");
			invoiceLine.JI_GoodsType = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.GoodsGroupNameCode should be");
		}

		[ExpectNoExceptions]
		public override void TestCommodity_HandlingInstructionsCodes()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var invLineRefs = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			invLineRefs.JG_ReferenceNumber = "1";
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.HandlingInstructionsCodes.Single(), NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		public void TestCommodity_HandlingInstructionsCodes_Selection()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var random = new Random();
			var expectedCodes = new List<string>();
			for (var i = 1; i >= 11; i++)
			{
				var referenceNumber = random.Next(1, 9).ToString();
				if (i < 10)
				{
					expectedCodes.Add(referenceNumber);
				}
				var invLineRefWithNumber = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
				invLineRefWithNumber.JG_ReferenceNumber = referenceNumber;
				var invLineRefEmpty = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
				invLineRefEmpty.JG_ReferenceNumber = ZString.Empty;
			}

			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			AssertContainsExactElementsInExactOrder(expectedCodes, commodity.HandlingInstructionsCodes);
		}

		[ExpectNoExceptions]
		public override void TestTariffCodeExtensionCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_TariffExtensionCode = "1";
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "Commodity.TariffCodeExtensionCode should be");
			invoiceLine.JI_TariffExtensionCode = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.TariffCodeExtensionCode should be");
		}

		[ExpectNoExceptions]
		public override void TestCommodity_Quarantine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.Quarantine, NUnit.Framework.Is.Not.EqualTo(default(IQuarantine)));
			NUnit.Framework.Assert.That(commodity.Quarantine, NUnit.Framework.Is.TypeOf<QuarantineWrapper>());
		}

		[ExpectNoExceptions]
		public override void TestCheckNotApplicableProperties()
		{
			var header = Factory.New<CusEntryHeader>();
			var line = header.MergedLines.AddNew();
			line.InvoiceLines.AddNew();
			var commodity = GetCommodity(line, line.RandomLine);
			NUnit.Framework.Assert.That(commodity.CargoDescription.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(commodity.BondedNoteCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(commodity.VehicleIDs, NUnit.Framework.Is.EqualTo(default(IEnumerable<ZString>)));
		}

		[ExpectNoExceptions]
		public override void TestCommodity_Wine()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Wine, NUnit.Framework.Is.EqualTo(default(IWine)));
			invoiceLine.JI_AlcoholPercentage = 1;
			entryLine.RefreshInvoiceLines();
			commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Wine, NUnit.Framework.Is.TypeOf(typeof(NX5105CMCommodityWine)));
		}

		protected override ICommodity GetCommodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			return new NX5105CMCommodity(entryLine, invoiceLine);
		}
	}
}

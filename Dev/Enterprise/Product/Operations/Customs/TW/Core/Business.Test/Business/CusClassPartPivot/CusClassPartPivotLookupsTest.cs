using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotLookups))]
	sealed class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestInvoiceUQList()
		{
			new TestTWCreator(Factory).CreateInvoiceUQ();
			var invoiceUQList = lookups.PartPivotUOMList;
			NUnit.Framework.Assert.That(invoiceUQList.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(invoiceUQList[0].Code, NUnit.Framework.Is.EqualTo("AMP"));
			NUnit.Framework.Assert.That(invoiceUQList[0].Description, NUnit.Framework.Is.EqualTo("Ampere"));
		}

		[ExpectNoExceptions]
		public void TestCurrencyList()
		{
			IActiveBusinessObjectCollection fCurrencyList = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { Factory });
			NUnit.Framework.Assert.That(lookups.CurrencyList.Count, NUnit.Framework.Is.EqualTo(fCurrencyList.Count), "Count");
		}

		[ExpectNoExceptions]
		public void TestModeOfStatistics()
		{
			var pivot = setupPivotWhithCusprocedure();
			var list = pivot.Lookups.ModeOfStatistics;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(3), "ModeOfStatistics Count");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("90"), NUnit.Framework.Is.EqualTo("三角貿易之外貨復出口"), "ModeOfStatistics Description");
		}

		[ExpectNoExceptions]
		public void TestDutyTreatment()
		{
			var pivot = setupPivotWhithCusprocedure();
			var list = pivot.Lookups.DutyTreatment;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2), "DutyTreatment Count");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("5E"), NUnit.Framework.Is.EqualTo("外交郵袋"), "DutyTreatment Description");
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode("65"), NUnit.Framework.Is.EqualTo("預估稅捐"), "DutyTreatment Description");
		}

		CusClassPartPivot setupPivotWhithCusprocedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("TW", "EX", "90", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G3,");
			helper.CreateRefCusProcedure("TW", "EX", "94", ZString.Empty, ZString.Empty, "國貨出口供經營國際貿易之非本國籍船舶、航空器或其他運輸工具專用之物料、物品。", "EXP", group: "D1,");
			helper.CreateRefCusProcedure("TW", "EX", "9G", ZString.Empty, ZString.Empty, "三角貿易之外貨復出口", "EXP", group: "G7,");
			helper.CreateRefCusProcedure("TW", "IM", "5E", ZString.Empty, ZString.Empty, "外交郵袋", "IMP", group: "G3,D2,");
			helper.CreateRefCusProcedure("TW", "IM", "65", ZString.Empty, ZString.Empty, "預估稅捐", "IMP", group: "F3,");
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			return orgSupplierPart.PivotsForBinding.AddNew();
		}

		[ExpectNoExceptions]
		public void TestCarTypeCodeList()
		{
			var list = lookups.CarTypeCodeList;
			NUnit.Framework.Assert.That(lookups.CarTypeCodeList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A1, A2, B1, B2, C1, C2, D1, D2, E1, F1, F2, G1, G2, H1, J1, K1"));
		}

		[ExpectNoExceptions]
		public void TestTransmissionCodeList()
		{
			var list = lookups.TransmissionCodeList;
			NUnit.Framework.Assert.That(lookups.TransmissionCodeList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, C, H, M"));
		}

		[ExpectNoExceptions]
		public void TestEngineTypeCodeList()
		{
			var list = lookups.EngineTypeCodeList;
			NUnit.Framework.Assert.That(lookups.EngineTypeCodeList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("CG, DE, DS, ED, EG, EL, GA, GE, GM, LG, OT"));
		}

		[ExpectNoExceptions]
		public void TestLeftSideSteeringCodeList()
		{
			var list = lookups.LeftSideSteeringCodeList;
			NUnit.Framework.Assert.That(lookups.LeftSideSteeringCodeList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("Y, N"));
		}

		[ExpectNoExceptions]
		public void TestCatalystConverterPrintModeList()
		{
			var list = lookups.CatalystConverterPrintModeList;
			NUnit.Framework.Assert.That(lookups.CatalystConverterPrintModeList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("Y, N"));
		}

		[ExpectNoExceptions]
		public void TestCarConditionCodeList()
		{
			var list = lookups.CarConditionCodeList;
			NUnit.Framework.Assert.That(lookups.CarConditionCodeList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3"));
		}

		[ExpectNoExceptions]
		public void TestEquipmentPrintModeList()
		{
			var list = lookups.EquipmentPrintModeList;
			NUnit.Framework.Assert.That(lookups.EquipmentPrintModeList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("EEC, EER, EGR"));
		}

		[ExpectNoExceptions]
		public void TestContainerMaterialList()
		{
			var list = lookups.ContainerMaterialList;
			NUnit.Framework.Assert.That(lookups.ContainerMaterialList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("A, B, C, D, E, F, G, H, I, J, K, L, Z"));
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.TypeOf<ContainerMaterialList>());
		}

		[ExpectNoExceptions]
		public void TestContainerCapacityList()
		{
			var list = lookups.ContainerCapacityList;
			NUnit.Framework.Assert.That(lookups.ContainerCapacityList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("0, 1, 2, 3, 4, 5, 6, 7, 8, 9"));
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.TypeOf<ContainerCapacityList>());
		}

		[ExpectNoExceptions]
		public void TestContainerMaterialNumberList()
		{
			var list = lookups.ContainerMaterialNumberList;
			NUnit.Framework.Assert.That(lookups.ContainerMaterialNumberList, NUnit.Framework.Is.EqualTo(list));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("0, 1, 2, 3, 4, 5"));
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.TypeOf<ContainerMaterialNumberList>());
		}

		[ExpectNoExceptions]
		public void TestDeclarationGoodsDescriptionModeList()
		{
			var list = lookups.DeclarationGoodsDescriptionModeList;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.EqualTo(Factory.GetCachedValue<DeclarationGoodsDescriptionModeList>()).Using(CustomComparers.TypeComparison), "Cached");
				NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("CHT, ENG, BTH"), "Codes As String");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			pivot = part.PivotsForBinding.AddNew();
			lookups = pivot.Lookups;
		}

		OrgSupplierPart part;
		CusClassPartPivot pivot;
		CusClassPartPivotLookups lookups;
	}
}

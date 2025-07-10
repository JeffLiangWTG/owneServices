using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	sealed class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			NUnit.Framework.Assert.That(pivot.CI_DeclGoodsDescMode, NUnit.Framework.Is.EqualTo(DeclarationGoodsDescriptionModeList.Codes.BTH).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTW_DeclGoodsDescMode_Caption()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(pivot.CI_DeclGoodsDescModeInfo, "Declaration Goods Description Mode", "Desc. Mode", "Indicates the mode of bringing the Goods description of the product code into the Goods description of the Invoice Line.");
		}

		[ExpectNoExceptions]
		public void TestICusClassPartPivotRefTypeSupporter_ReloadCollection()
		{
			_ = pivot.AssignedCusClassPartPivotRefCollection;
			var pivotRef = Factory.New<AssignedCusClassPartPivotRef>();
			pivotRef.CIR_ReferenceType = JobComInvLineRefsType.Codes.AssignedNumber;
			pivotRef.CIR_CI = pivot.PK;
			var provider = (ICusClassPartPivotRefTypeSupporter)pivot;
			CombineAssertions(() =>
			{
				provider.ReloadCollection(ZString.Empty);
				NUnit.Framework.Assert.That(pivot.AssignedCusClassPartPivotRefCollection.Count, NUnit.Framework.Is.EqualTo(0), "AssignedCusClassPartPivotRefCollection will not be reloaded");
				provider.ReloadCollection(JobComInvLineRefsType.Codes.AssignedNumber);
				NUnit.Framework.Assert.That(pivot.AssignedCusClassPartPivotRefCollection.Count, NUnit.Framework.Is.EqualTo(1), "AssignedCusClassPartPivotRefCollection is reloaded");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestTariffFormatter()
		{
			var tariffFormatter = (ITariffFormatProvider)pivot;
			NUnit.Framework.Assert.That(tariffFormatter.TariffFormatter, NUnit.Framework.Is.TypeOf(typeof(TaiwanTariffFormatter)));
		}

		[ExpectNoExceptions]
		public void TestProductPermitCusSupportingCollection()
		{
			var permitNumber = pivot.ProductPermitCusSupportingCollection.AddNew();
			permitNumber.CSI_ReferenceNumber = "XX123456789";
			permitNumber.CSI_LineNo = 5;
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, pivot.PK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.PermitNumber);
			var cusSupportingInfo = Factory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(cusSupportingInfo.Length, NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestAssignedCusClassPartPivotRefCollection()
		{
			var assignedNumber = pivot.AssignedCusClassPartPivotRefCollection.AddNew();
			assignedNumber.CIR_ReferenceNumber = "XXX";
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(AssignedCusClassPartPivotRef));
			query.AddToFilter(CusClassPartPivotRefSchema.CIR_CI, pivot.PK);
			query.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceType, JobComInvLineRefsType.Codes.AssignedNumber);
			var cusClassPartPivotRef = Factory.Load(typeof(AssignedCusClassPartPivotRef), query);
			NUnit.Framework.Assert.That(cusClassPartPivotRef.Length, NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingNo()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo1, NUnit.Framework.Is.EqualTo(ZString.Empty));
				pivot.PermitCusSupportingNo1 = "1";
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo1, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));

				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo2, NUnit.Framework.Is.EqualTo(ZString.Empty));
				pivot.PermitCusSupportingNo2 = "2";
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo2, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));

				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo3, NUnit.Framework.Is.EqualTo(ZString.Empty));
				pivot.PermitCusSupportingNo3 = "3";
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo3, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));

				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo4, NUnit.Framework.Is.EqualTo(ZString.Empty));
				pivot.PermitCusSupportingNo4 = "4";
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo4, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));

				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo5, NUnit.Framework.Is.EqualTo(ZString.Empty));
				pivot.PermitCusSupportingNo5 = "5";
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo5, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingNo1()
		{
			TestPermitCusSupportingNo(1);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingNo2()
		{
			TestPermitCusSupportingNo(2);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingNo3()
		{
			TestPermitCusSupportingNo(3);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingNo4()
		{
			TestPermitCusSupportingNo(4);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingNo5()
		{
			TestPermitCusSupportingNo(5);
		}

		[ExpectNoExceptions]
		void TestPermitCusSupportingNo(int index)
		{
			var pivot = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew();
			var propertyInfo = pivot.GetType().GetProperty($"PermitCusSupportingNo{index}");
			for (var i = 0; i < index - 1; i++)
			{
				propertyInfo.SetValue(pivot, new ZString(index.ToString()), null);
			}
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(propertyInfo.GetValue(pivot, null), NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
				propertyInfo.SetValue(pivot, new ZString(index.ToString()), null);
				NUnit.Framework.Assert.That(propertyInfo.GetValue(pivot, null), NUnit.Framework.Is.EqualTo(index.ToString()).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingLineNo1()
		{
			TestPermitCusSupportingLineNo(1);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingLineNo2()
		{
			TestPermitCusSupportingLineNo(2);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingLineNo3()
		{
			TestPermitCusSupportingLineNo(3);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingLineNo4()
		{
			TestPermitCusSupportingLineNo(4);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingLineNo5()
		{
			TestPermitCusSupportingLineNo(5);
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingLineNo(int index)
		{
			var pivot = Factory.New<OrgSupplierPart>().PivotsForBinding.AddNew();
			var propertyInfo = pivot.GetType().GetProperty($"PermitCusSupportingLineNo{index}");
			for (var i = 0; i < index - 1; i++)
			{
				propertyInfo.SetValue(pivot, new ZInt(index), null);
			}
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(propertyInfo.GetValue(pivot, null), NUnit.Framework.Is.EqualTo(ZInt.Zero).Using(CustomComparers.TypeComparison));
				propertyInfo.SetValue(pivot, new ZInt(index), null);
				NUnit.Framework.Assert.That(propertyInfo.GetValue(pivot, null), NUnit.Framework.Is.EqualTo(index).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestSortedPermitCusSupportingCollection()
		{
			var permit1 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permit1.CSI_ItemNumber = 2;
			var permit2 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permit2.CSI_ItemNumber = 3;
			var permit3 = pivot.ProductPermitCusSupportingCollection.AddNew();
			permit3.CSI_ItemNumber = 1;
			NUnit.Framework.Assert.That(pivot.SortedProductPermitCusSupportingCollection, NUnit.Framework.Is.EqualTo(new ProductPermitCusSupporting[] { permit3, permit1, permit2 }));
		}

		[ExpectNoExceptions]
		public void TestPermitCusSupportingMaxLength()
		{
			var expectMaxLength = 14;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo1Info.MaxLength, NUnit.Framework.Is.EqualTo(expectMaxLength), "PermitCusSupportingNo1");
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo2Info.MaxLength, NUnit.Framework.Is.EqualTo(expectMaxLength), "PermitCusSupportingNo2");
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo3Info.MaxLength, NUnit.Framework.Is.EqualTo(expectMaxLength), "PermitCusSupportingNo3");
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo4Info.MaxLength, NUnit.Framework.Is.EqualTo(expectMaxLength), "PermitCusSupportingNo4");
				NUnit.Framework.Assert.That(pivot.PermitCusSupportingNo5Info.MaxLength, NUnit.Framework.Is.EqualTo(expectMaxLength), "PermitCusSupportingNo5");
			});
		}

		[ExpectNoExceptions]
		public void TestCI_NDescriptionInfo()
		{
			NUnit.Framework.Assert.That(pivot.CI_NDescriptionInfo.MaxLength, NUnit.Framework.Is.EqualTo(512));
		}

		[ExpectNoExceptions]
		public void TestShouldAllowEnvironmentalProtectionTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "3513200001", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff, "TRUE", tariff2);
			Factory.Save();
			pivot.CI_TariffNum = "3513200001";
			NUnit.Framework.Assert.That(!pivot.ShouldAllowEnvironmentalProtectionTariff, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			pivot.CI_TariffNum = "2713200001";
			NUnit.Framework.Assert.That(pivot.ShouldAllowEnvironmentalProtectionTariff, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCI_TariffNum()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "3513200001", minDate, maxDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.EnvironmentalProtectionTariff, "TRUE", tariff2);
			Factory.Save();
			pivot.CI_TariffNum = "2713200001";
			pivot.CI_EPTDigit1 = "A";
			pivot.CI_EPTDigit2 = "B";
			pivot.CI_EPTDigit3 = "C";
			pivot.CI_TariffNum = "3513200001";
			NUnit.Framework.Assert.That(pivot.CI_EPTDigit1, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(pivot.CI_EPTDigit2, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(pivot.CI_EPTDigit3, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			NUnit.Framework.Assert.That(Factory.New(typeof(BaseCusClassPartPivot)).GetType(), NUnit.Framework.Is.EqualTo(GetExpectedBusinessObjectType()), "Update Customs.Business.BaseCusClassPartPivot to include a decider for this class");
		}

		[ExpectNoExceptions]
		public void TestIsCarRelatedTariff()
		{
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "86XXXX";
			NUnit.Framework.Assert.That(pivot.IsCarRelatedTariff, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			NUnit.Framework.Assert.That(pivot.IsCarRelatedTariff, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			NUnit.Framework.Assert.That(pivot.IsCarRelatedTariff, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_TariffNum = "XXXXXX";
			NUnit.Framework.Assert.That(pivot.IsCarRelatedTariff, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCI_PartPivotUOM()
		{
			NUnit.Framework.Assert.That(pivot.GetType(), CustomConstraints.HasCustomAttribute<ListAttribute>(nameof(pivot.CI_PartPivotUOM), false, attrib => attrib.ListDataSourceMember == "Lookups.PartPivotUOMList"));
			NUnit.Framework.Assert.That(pivot.CI_PartPivotUOMInfo.MaxLength, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestTW_Compositions_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(pivot.CI_CompositionsInfo);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(resourceStringData.Caption, NUnit.Framework.Is.EqualTo("Specification"), "Caption");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			pivot = part.PivotsForBinding.AddNew();
		}

		OrgSupplierPart part;
		CusClassPartPivot pivot;
	}
}

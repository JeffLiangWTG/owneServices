using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	public class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTariffFieldsCanBeReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(CusClassPartPivot), "CI_TariffNum", true, attrib => attrib.Member == "IsTariffNumReadOnly");
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(CusClassPartPivot), "CI_PercAlcohol", true, attrib => attrib.Member == "IsTariffNumReadOnly");
		}

		public void TestTariffFieldsAreClearedWhenClassificationIsSet()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "ABC123";
			pivot.CI_PercAlcohol = 12.3m;
			pivot.ProductCodes.AddNew("ABC123");
			pivot.CI_CC = ZGuid.NewZGuid();
			AssertEquals("CI_TariffNum is Empty", ZString.Empty, pivot.CI_TariffNum);
			AssertEquals("CI_PercAlcohol is Empty", 0.0m, pivot.CI_PercAlcohol);
			AssertEquals("ProductCodes is Empty", 0, pivot.ProductCodes.Count);
		}

		public void TestTariffFormatter()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			Assert("CurrentTariffFormatter is Enterprise.Customs.SG.V4.Business.TariffFormatter", pivot.CurrentTariffFormatter is TariffFormatter);
		}

		public void TestLookups()
		{
			Assert("Lookups is CusClassPartPivotLookups", CusClassPartPivot.Lookups is CusClassPartPivotLookups);
		}

		#region implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return CusClassPartPivot;
		}

		CusClassPartPivot CusClassPartPivot
		{
			get
			{
				return cusClassPartPivot ?? (cusClassPartPivot = Factory.New<CusClassPartPivot>());
			}
		}

		CusClassPartPivot cusClassPartPivot;
		#endregion
	}
}

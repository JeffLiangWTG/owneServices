using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusClassificationNonInheritedTest : TestCaseWithFactory
	{
		public void TestAuditDetailsReadOnly()
		{
			BaseCusClassification bizO = Factory.New<BaseCusClassification>();
			AssertEquals("CC_LastAuditedUser is readonly", true, bizO.CC_LastAuditedUserInfo.ReadOnly);
			AssertEquals("CC_LastAuditedDate is readonly", true, bizO.CC_LastAuditedDateInfo.ReadOnly);
		}

		public void TestOnTariffSetPassesInOldValue()
		{
			TestCusClassification classification = Factory.New<TestCusClassification>();
			AssertEquals("Precondition: OldValue parameter from last OnTariffSet call", "", classification.OldValue);
			classification.CC_TariffNum = "1111.11.11.11A";
			AssertEquals("OldValue parameter from last OnTariffSet call", "", classification.OldValue);
			classification.CC_TariffNum = "2222.22.22.22B";
			AssertEquals("OldValue parameter from last OnTariffSet call", "1111.11.11.11A", classification.OldValue);
			classification.CC_TariffNum = "3333.33.33.33C";
			AssertEquals("OldValue parameter from last OnTariffSet call", "2222.22.22.22B", classification.OldValue);
		}

		public void TestLoadFromLookupCode()
		{
			BaseCusClassification classification1 = Factory.New<BaseCusClassification>();
			classification1.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification1.CC_LookupCode = "ABC";
			classification1.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			BaseCusClassification classification2 = Factory.New<BaseCusClassification>();
			classification2.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification2.CC_LookupCode = "ABC";
			classification2.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			BaseCusClassification classification3 = Factory.New<BaseCusClassification>();
			classification3.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			classification3.CC_LookupCode = "ABC";
			classification3.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals(classification1, BaseCusClassification.LoadFromLookupCode(Factory, "ABC", BaseCusClassification.ClassificationType.Both, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			AssertEquals(classification2, BaseCusClassification.LoadFromLookupCode(Factory, "ABC", BaseCusClassification.ClassificationType.IMP, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			AssertEquals(classification3, BaseCusClassification.LoadFromLookupCode(Factory, "ABC", BaseCusClassification.ClassificationType.EXP, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
		}

		public void TestITypeDeciderContext()
		{
			CombineAssertions(() =>
			{
				var classification = Factory.New<BaseCusClassification>();
				classification.CC_RN_NKCountryCode = ZString.Empty;
				AssertEquals("From CurrentCompany", "ER", (classification as ITypeDeciderContext).Country);

				classification.CC_RN_NKCountryCode = "CA";
				AssertEquals("From CC_RN_NKCountryCode", "CA", (classification as ITypeDeciderContext).Country);
			});
		}

		#region class TestCusClassification
		class TestCusClassification : BaseCusClassification
		{
			public TestCusClassification(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnTariffSet(ZString oldValue)
			{
				this.OldValue = oldValue;
			}
			public ZString OldValue = "";
		}
		#endregion
	}
}

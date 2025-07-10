using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionType))]
	public class RefCusConditionTypeTesting : EnterpriseBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase
		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefCusConditionCreatorForTest(Factory).CreateRefCusConditionType();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
		#endregion

		public void TestZX2_Description()
		{
			Helper.CreateOrGetLanguage("FR", "French");
			Factory.Save();

			var refCusConditionType = GetNewBusinessObject() as RefCusConditionType;
			refCusConditionType.ZX2_Description = "Description";
			var refCusConditionTypeLanguage = refCusConditionType.Factory.New<RefCusConditionTypeLanguage>();
			refCusConditionTypeLanguage.ZXW_ZX2_ConditionType = refCusConditionType.PK;
			refCusConditionTypeLanguage.ZXW_ZX6_NKLanguage = "FR";
			refCusConditionTypeLanguage.ZXW_Description = "Décrire";
			refCusConditionType.Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("ZX2_Description should be provided in default language.", "Description",
					new BusinessObjectFactory().Load<RefCusConditionType>(refCusConditionType.PK).ZX2_Description);

				var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
				Factory.Save();
				AssertEquals("ZX2_Description should be translated.", "Décrire",
					new BusinessObjectFactory().Load<RefCusConditionType>(refCusConditionType.PK).ZX2_Description);
			});
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}

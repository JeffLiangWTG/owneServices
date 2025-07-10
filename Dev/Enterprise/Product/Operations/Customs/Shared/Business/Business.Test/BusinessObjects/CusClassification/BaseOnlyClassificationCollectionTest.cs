using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusClassification))]
	sealed class BaseOnlyClassificationCollectionTest : ClassificationCollectionTest<ClassificationCollection<BaseCusClassification>, BaseCusClassification>
	{
		public void TestThisCollectionExcludesOtherCountryClassification()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var auTypePart = Factory.New<OrgSupplierPart>();
			auTypePart.OP_PartNum = "PartNum";
			var aUClass = Factory.New<BaseCusClassification>();
			aUClass.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			aUClass.CC_LookupCode = "LOOKUP";
			auTypePart.ClassificationsForBinding.Add(aUClass);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			var factory2 = new BusinessObjectFactory();
			var nzTypePart = factory2.Load<OrgSupplierPart>(auTypePart.PK);
			AssertEquals(false, nzTypePart.ClassificationsForBinding.FindByPK(aUClass.PK) != null);

			var factory3 = new BusinessObjectFactory();
			//this can happen if an AU invoice line loads a part, which should be an AU part. Then classifications should be all AU types
			var auTypePartLoadedInNZ = (OrgSupplierPart)factory3.Load<Integration.Customs.AU.IOrgSupplierPart>(nzTypePart.PK);
			AssertEquals(true, auTypePartLoadedInNZ.ClassificationsForBinding.FindByPK(aUClass.PK) != null);
		}

		protected override ClassificationCollection<BaseCusClassification> GetCollectionToTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new ClassificationCollection<BaseCusClassification>(part, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
	}
}

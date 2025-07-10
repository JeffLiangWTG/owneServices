using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(ClassificationCollection<BaseCusClassification>))]
	public abstract class ClassificationCollectionTest<TCollection, TClassification> : ActiveBusinessObjectCollectionTestCase<TCollection>
			where TCollection : IClassificationCollection<TClassification>
			where TClassification : BaseCusClassification
	{
		/// <summary>
		/// Incident: CS00025838
		/// </summary>
		public void TestClassificationsShouldNotLoadUnrelatedClassifications()
		{
			var class1 = Factory.New<TClassification>();
			class1.CC_LookupCode = "1";
			class1.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = part1.PK.ToString().Replace("-", "");
			var pivot1 = Factory.New<BaseCusClassPartPivot>();
			pivot1.CI_CC = class1.PK;
			pivot1.CI_OP = part1.PK;

			var class2 = Factory.New<TClassification>();
			class2.CC_LookupCode = "2";
			class2.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = part2.PK.ToString().Replace("-", "");
			var pivot2 = Factory.New<BaseCusClassPartPivot>();
			pivot2.CI_CC = class2.PK;
			pivot2.CI_OP = part2.PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			_ = factory2.Load<OrgSupplierPart>(part2.PK);
			AssertEquals("There should be 1 classification", 1, part2.ClassificationsForBinding.Count);

			var queryToLoadClassification = new ZQuery(CusClassificationSchema.PK, class1.PK);
			queryToLoadClassification.FetchOnlyFromLocalCache = true;
			AssertNull("When loading part.Classifications, it should have loaded the other unrelated classifications", factory2.LoadTop1<TClassification>(queryToLoadClassification));
		}

		public void TestClassificationsRemovedFromPartInOneFactoryGetUpdatedInPartInAnotherFactory()
		{
			var classification1 = Factory.New<TClassification>();
			classification1.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification1.CC_Description = "HENRYS FAVOURITE UMBRELLA";
			classification1.CC_LookupCode = "LOOKUP1";
			classification1.CC_TariffNum = "";
			classification1.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var classification2 = Factory.New<TClassification>();
			classification2.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			classification2.CC_Description = "HENRYS FAVOURITE GLOVE";
			classification2.CC_LookupCode = "LOOKUP2";
			classification1.CC_TariffNum = "0000.00.00";
			classification2.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "MAX-FACTOR";
			part.OP_Desc = "HENRYS FAVOURITE LIPPIE";
			part.ClassificationsForBinding.Add(classification1);

			Factory.Save();
			AssertEquals("Precondition: Part.GetClassifications().Count", 1, part.ClassificationsForBinding.Count);
			AssertEquals("Precondition: Part.GetClassifications()[0].CC_Description", classification1.CC_Description, part.ClassificationsForBinding[0].CC_Description);
			AssertEquals("Precondition: Part.GetClassifications()[0].PK", classification1.PK, part.ClassificationsForBinding[0].PK);

			var loadedPart = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
			AssertEquals("Part.GetClassifications().Count", 1, loadedPart.ClassificationsForBinding.Count);
			AssertEquals("Part.GetClassifications()[0].CC_Description", classification1.CC_Description, loadedPart.ClassificationsForBinding[0].CC_Description);
			AssertEquals("Part.GetClassifications()[0].PK", classification1.PK, loadedPart.ClassificationsForBinding[0].PK);

			ChangeClassificationInAnotherFactory(loadedPart.PK, classification2.PK);
			loadedPart = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
			AssertEquals("Part.GetClassifications().Count", 1, loadedPart.ClassificationsForBinding.Count);
			AssertEquals("Part.GetClassifications()[0].CC_Description", classification2.CC_Description, loadedPart.ClassificationsForBinding[0].CC_Description);
			AssertEquals("Part.GetClassifications()[0].PK", classification2.PK, loadedPart.ClassificationsForBinding[0].PK);

			ChangeClassificationInAnotherFactory(loadedPart.PK, classification1.PK);
			loadedPart = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
			AssertEquals("Part.GetClassifications().Count", 1, loadedPart.ClassificationsForBinding.Count);
			AssertEquals("Part.GetClassifications()[0].CC_Description", classification1.CC_Description, loadedPart.ClassificationsForBinding[0].CC_Description);
			AssertEquals("Part.GetClassifications()[0].PK", classification1.PK, loadedPart.ClassificationsForBinding[0].PK);

			ChangeClassificationInAnotherFactory(loadedPart.PK, classification2.PK);
			loadedPart = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
			AssertEquals("Part.GetClassifications().Count", 1, loadedPart.ClassificationsForBinding.Count);
			AssertEquals("Part.GetClassifications()[0].CC_Description", classification2.CC_Description, loadedPart.ClassificationsForBinding[0].CC_Description);
			AssertEquals("Part.GetClassifications()[0].PK", classification2.PK, loadedPart.ClassificationsForBinding[0].PK);
		}

		void ChangeClassificationInAnotherFactory(ZGuid partPk, ZGuid newClassificationPk)
		{
			var anotherFactory = new BusinessObjectFactory();
			var anotherFactoryPart = anotherFactory.Load<OrgSupplierPart>(partPk);
			anotherFactoryPart.ClassificationsForBinding.RemoveFromRelationship(anotherFactoryPart.ClassificationsForBinding[0]);
			var classInThisFactory = anotherFactory.Load<TClassification>(newClassificationPk);
			anotherFactoryPart.ClassificationsForBinding.Add(classInThisFactory);
			anotherFactory.Save();
		}
	}
}

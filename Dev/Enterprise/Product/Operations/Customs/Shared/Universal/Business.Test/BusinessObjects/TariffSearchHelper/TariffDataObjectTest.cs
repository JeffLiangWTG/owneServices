using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffDataObject))]
	class TariffDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			CusTariff.ZZ1_Description = CusTariff.ZZ1_Description.PadRight(110, '1');
			var bizObj = new TariffDataObject(Helper, CusTariff);
			var codeAndDescription = "1020304050 " + "DESC TARIFF".PadRight(110, '1');
			AssertEquals("bizObj.HasSelectableTariff", true, bizObj.HasSelectableTariff);
			AssertEquals("bizObj.CusTariff", CusTariff, bizObj.CusTariff);
			AssertEquals("bizObj.IsNomenclatureGroup", false, bizObj.IsNomenclatureGroup);
			AssertEquals("bizObj.TariffCodeAndDescription", codeAndDescription, bizObj.TariffCodeAndDescription);
			AssertEquals("bizObj.TariffCode", "1020304050", bizObj.TariffCode);
			AssertEquals("bizObj.FullDescription", CusTariff.ZZ1_Description, bizObj.FullDescription);
			AssertEquals("bizObj.CompositeKey", "10.20.30.40.50", bizObj.CompositeKey);
			var compositeKeyComponents = bizObj.GetCompositeKeyComponents();
			AssertEquals("compositeKeyComponents.Length", 5, compositeKeyComponents.Length);
			var other = TariffDataObject.GetOther(Helper);
			AssertEquals("TariffCodeAndDescription of an Other TariffDataObject should be \"Other\"", " Other", other.TariffCodeAndDescription);
			var group = new TariffDataObject(Helper, CusNomenclatureGroup);
			AssertNull("group.CusTariff", group.CusTariff);
			AssertEquals("group.HasSelectableTariff", false, group.HasSelectableTariff);
			group.AddRelatedData(bizObj);
			AssertEquals("group.HasSelectableTariff", true, group.HasSelectableTariff);
		}

		public void TestRelatedDataClearAndLoad()
		{
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 1", "A");
			var group2 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.2", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 2", "A.B");
			var group3 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 3", "C.D");
			var group4 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.3", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC GROUP 4", "A.B");
			var group5 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.2..1", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddYears(1), "DESC GROUP 5", "A.B..D"); // this should not be loaded
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "11.20.00", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A.B.C");
			var tariff2 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "20.00", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2", compositeKey: "C.D.F");
			var tariff3 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "30.00", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 3", compositeKey: "A.C");
			Factory.Save();
			Helper.LoadRefCusNomenclatureGroup(new ZQuery());
			Helper.LoadRefCusTariff(new ZQuery());
			var bizObj = new TariffDataObject(Helper, group1);
			var relatedDataCollection = bizObj.RelatedDataCollection.ToArray();
			AssertEquals("relatedDataCollection.Length", 2, relatedDataCollection.Length);
			AssertEquals("relatedDataCollection[0].FullDescription", "DESC GROUP 2", relatedDataCollection[0].FullDescription);
			AssertEquals("relatedDataCollection[1].FullDescription", "DESC TARIFF 3", relatedDataCollection[1].FullDescription);
			var childBizObj = relatedDataCollection[0];
			var childRelatedDataCollection = childBizObj.RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 1, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[1].FullDescription", "DESC TARIFF 1", childRelatedDataCollection[0].FullDescription);
			bizObj.ClearCachedData();
			bizObj.AddCompositeKeyToCheck("D.D", true);
			relatedDataCollection = bizObj.RelatedDataCollection.ToArray();
			AssertEquals("relatedDataCollection.Length", 2, relatedDataCollection.Length);
			AssertEquals("relatedDataCollection[0].FullDescription", "DESC GROUP 2", relatedDataCollection[0].FullDescription);
			AssertEquals("relatedDataCollection[1].FullDescription", "DESC TARIFF 3", relatedDataCollection[1].FullDescription);
			bizObj.ClearCachedData();
			bizObj.AddCompositeKeyToCheck("A.B", true);
			relatedDataCollection = bizObj.RelatedDataCollection.ToArray();
			AssertEquals("relatedDataCollection.Length", 1, relatedDataCollection.Length);
			AssertEquals("relatedDataCollection[0].FullDescription", "DESC GROUP 2", relatedDataCollection[0].FullDescription);
			bizObj.ClearCachedData();
			bizObj.AddCompositeKeyToCheck("A.G", false);
			relatedDataCollection = bizObj.RelatedDataCollection.ToArray();
			AssertEquals("relatedDataCollection.Length", 2, relatedDataCollection.Length);
			AssertEquals("relatedDataCollection[0].FullDescription", "DESC GROUP 2", relatedDataCollection[0].FullDescription);
			AssertEquals("relatedDataCollection[1].FullDescription", "DESC TARIFF 3", relatedDataCollection[1].FullDescription);
			bizObj.ClearCachedData();
			bizObj.AddCompositeKeyToCheck("A.G", true);
			relatedDataCollection = bizObj.RelatedDataCollection.ToArray();
			AssertEquals("relatedDataCollection.Length", 0, relatedDataCollection.Length);
		}

		public void TestRelatedDataOrderByCompositeKeyAndTariffCode()
		{
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "01", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "LIVE ANIMALS: ANIMAL PRODUCTS", "01");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "01", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "LIVE ANIMALS", "01.01");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "0102", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Live bovine animals", "01.01..02");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "Cattle", "01.01..02.2");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "010229", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddYears(1), "Other", "01.01..02.2.9");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "01022905", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Of the sub-genus Bibos or of the sub-genus Poephagus", compositeKey: "01.01..02.2.9.10");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddYears(1), "Other", "01.01..02.2.9.20");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "01022910", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Of a weight not exceeding 80 kg", compositeKey: "01.01..02.2.9.20.10");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddYears(1), "Of a weight exceeding 80 kg but not exceeding 160 kg", "01.01..02.2.9.20.20");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "01022921", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "For slaughter", compositeKey: "01.01..02.2.9.20.20.10");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "01022929", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Other", compositeKey: "01.01..02.2.9.20.20.20");

			Factory.Save();
			Helper.LoadRefCusNomenclatureGroup(new ZQuery());
			Helper.LoadRefCusTariff(new ZQuery());

			var bizObj = new TariffDataObject(Helper, group1);

			var childRelatedDataCollection = bizObj.RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 1, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[0].FullDescription", "LIVE ANIMALS", childRelatedDataCollection[0].FullDescription);

			childRelatedDataCollection = childRelatedDataCollection[0].RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 1, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[0].FullDescription", "Live bovine animals", childRelatedDataCollection[0].FullDescription);

			childRelatedDataCollection = childRelatedDataCollection[0].RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 1, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[0].FullDescription", "Cattle", childRelatedDataCollection[0].FullDescription);

			childRelatedDataCollection = childRelatedDataCollection[0].RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 1, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[0].FullDescription", "Other", childRelatedDataCollection[0].FullDescription);

			childRelatedDataCollection = childRelatedDataCollection[0].RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 2, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[0].FullDescription", "Of the sub-genus Bibos or of the sub-genus Poephagus", childRelatedDataCollection[0].FullDescription);
			AssertEquals("childRelatedDataCollection[1].FullDescription", "Other", childRelatedDataCollection[1].FullDescription);

			childRelatedDataCollection = childRelatedDataCollection[1].RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 2, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[0].FullDescription", "Of a weight not exceeding 80 kg", childRelatedDataCollection[0].FullDescription);
			AssertEquals("childRelatedDataCollection[1].FullDescription", "Of a weight exceeding 80 kg but not exceeding 160 kg", childRelatedDataCollection[1].FullDescription);

			childRelatedDataCollection = childRelatedDataCollection[1].RelatedDataCollection.ToArray();
			AssertEquals("childRelatedDataCollection.Length", 2, childRelatedDataCollection.Length);
			AssertEquals("childRelatedDataCollection[0].FullDescription", "For slaughter", childRelatedDataCollection[0].FullDescription);
			AssertEquals("childRelatedDataCollection[1].FullDescription", "Other", childRelatedDataCollection[1].FullDescription);
		}

		public void TestRelatedDataNaturalOrder()
		{
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "21", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "WORKS OF ART", "21");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "98", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "COMPLETE INDUSTRIAL PLANT", "21.98");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "9880", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Components parts of complete industrial plant", "21.98..80");
			dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "Other", "21.98..80.4");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "98804000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Classified in Chapter 40", compositeKey: "21.98..80.4");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "9880410000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Classified in Chapter 41", compositeKey: "21.98..80.4.1");
			dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "9880420000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Classified in Chapter 42", compositeKey: "21.98..80.4.2");

			Factory.Save();
			Helper.LoadRefCusNomenclatureGroup(new ZQuery());
			Helper.LoadRefCusTariff(new ZQuery());

			var bizObj = new TariffDataObject(Helper, group1);

			var childRelatedDataCollection = bizObj.RelatedDataCollection.ToArray();
			var (chapter98, _) = FindElement(childRelatedDataCollection, "COMPLETE INDUSTRIAL PLANT");
			AssertNotNull(chapter98);

			childRelatedDataCollection = chapter98.RelatedDataCollection.ToArray();
			var (componentsParts, _) = FindElement(childRelatedDataCollection, "Components parts of complete industrial plant");
			AssertNotNull(componentsParts);

			childRelatedDataCollection = componentsParts.RelatedDataCollection.ToArray();
			var (placeholder, _) = FindElement(childRelatedDataCollection, "Other");
			AssertNotNull(placeholder);

			childRelatedDataCollection = placeholder.RelatedDataCollection.ToArray();

			var (classifiedIn40, classifiedIn40Position) = FindElement(childRelatedDataCollection, "Classified in Chapter 40");
			AssertNotNull(classifiedIn40);

			var (classifiedIn41, classifiedIn41Position) = FindElement(childRelatedDataCollection, "Classified in Chapter 41");
			AssertNotNull(classifiedIn41);

			var (classifiedIn42, classifiedIn42Position) = FindElement(childRelatedDataCollection, "Classified in Chapter 42");
			AssertNotNull(classifiedIn42);

			Assert("Tariffs should be presented in natural order", classifiedIn40Position < classifiedIn41Position && classifiedIn41Position < classifiedIn42Position);

			(TariffDataObject Element, int Position) FindElement(TariffDataObject[] childRelatedDataCollection, string description)
			{
				for (var i = 0; i < childRelatedDataCollection.Length; i++)
				{
					var element = childRelatedDataCollection[i];
					if (string.Equals(element.FullDescription, description, StringComparison.OrdinalIgnoreCase))
					{
						return (element, i);
					}
				}

				return (null, -1);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TariffDataObject(Helper, CusTariff);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dataHelper = new UniversalReferenceTestDataHelper(Factory);
			tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
		}

		RefCusTariffType tariffTypeTST;
		TariffSearchHelper Helper => helper ?? (helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null));
		TariffSearchHelper helper;
		TariffView CusTariff => cusTariff ?? (cusTariff = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "1020304050", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF", compositeKey: "10.20.30.40.50"));
		TariffView cusTariff;
		RefCusNomenclatureGroup CusNomenclatureGroup => cusNomenclatureGroup ?? (cusNomenclatureGroup = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "1020", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP", compositeKey: "10.20.30.40.50"));
		RefCusNomenclatureGroup cusNomenclatureGroup;
		UniversalReferenceTestDataHelper dataHelper;
	}
}

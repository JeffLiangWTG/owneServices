using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGCommonDataCollection))]
	sealed class UNDGCommonDataCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGCommonDataCollection>
	{
		public void TestAdditionalFilter()
		{
			string type = "ZZZ";
			UNDGCommonDataCollection col = new UNDGCommonDataCollection(Factory, type);
			AssertEquals(new ZQuery(UNDGCommonDataSchema.DC_Type, type).LiteralTextADO, col.AdditionalFilter.LiteralTextADO);
		}

		public void TestRelationship_AddRemove()
		{
			UNDGCommonData data = Factory.New<UNDGCommonData>();
			data.DC_Index = "666";
			data.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			data.DC_Descriptor = "XXX";
			data.DC_Language = Core.Constants.Languages.Gujarati;
			Factory.Save();

			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DetailsLanguage = ZString.Empty;
			AssertEquals(0, substance.SpecialProvisions.Count);
			AssertEquals(0, substance.SpecialProvisionsAttributes.Count);

			substance.SpecialProvisions.Add(data);
			AssertEquals(1, substance.SpecialProvisions.Count);
			AssertEquals(1, substance.SpecialProvisionsAttributes.Count);
			var attribute = substance.SpecialProvisionsAttributes[0];
			AssertEquals("666", attribute.DA_Index);
			AssertEquals(ZString.Empty, attribute.DA_Language);
			AssertEquals(ZString.Empty, attribute.DA_Descriptor);
			AssertEquals(ViewUNDGAttributeLookups.TypeConstants.SpecialProvisions, attribute.DA_Type);
			AssertEquals(substance.PK, attribute.DA_DG);

			substance.SpecialProvisions.Delete(data);
			AssertEquals(true, data.IsDeleted);
			AssertEquals(true, attribute.IsDeleted);
			AssertEquals(0, substance.SpecialProvisions.Count);
			AssertEquals(0, substance.SpecialProvisionsAttributes.Count);
		}

		public void TestRelationship_ResetFromAttributes()
		{
			UNDGCommonData data = Factory.New<UNDGCommonData>();
			data.DC_Index = "666";
			data.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			data.DC_Descriptor = "XXX";
			data.DC_Language = Core.Constants.Languages.English;
			Factory.Save();

			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SpecialProvisions;
			attribute.DA_Index = "666";

			AssertEquals(0, substance.SpecialProvisions.Count);
			substance.SpecialProvisionsAttributes.Add(attribute);
			AssertEquals(1, substance.SpecialProvisions.Count);
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestRelationship_RemoveEnglish()
		{
			UNDGCommonData data = Factory.New<UNDGCommonData>();
			data.DC_Index = "666";
			data.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			data.DC_Descriptor = "XXX";
			data.DC_Language = Core.Constants.Languages.English;
			Factory.Save();

			UNDGSubstance substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			substance.SpecialProvisions.Add(data);
			substance.SpecialProvisions.Delete(data);
		}
	}
}

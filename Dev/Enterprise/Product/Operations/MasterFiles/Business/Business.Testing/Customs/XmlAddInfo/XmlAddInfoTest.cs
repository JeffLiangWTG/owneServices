using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class XmlAddInfoTest : TestCaseWithDummy
	{
		public void TestClearAllPropertiesWhenDeserialising()
		{
			Dummy.Z0_Description = "";
			var addInfo = new TestXmlAddInfo(Dummy);
			addInfo.Z3_String = "String";
			addInfo.Serialise();
			var addInfoString = Dummy.Z0_Description;
			addInfo.Z3_Int = 1;
			addInfo.Serialise();
			Dummy.Z0_Description = addInfoString;
			addInfo.Deserialise();
			AssertEquals(0, addInfo.Z3_Int);
		}

		public void TestDeserialiseHandlesAnEmptyString()
		{
			Dummy.Z0_Description = "";
			TestXmlAddInfo addInfo = new TestXmlAddInfo(Dummy);
			addInfo.Z3_Boolean = true;
			addInfo.Serialise();
			AssertEquals("Serialised", false, Dummy.Z0_Description.IsEmpty);

			//US Declarations hooks to OrgCountryData changes made in another factory (AMS participation indicator) 
			//When Save() is performed in another factory, AddInfo should be deserialised again with the db XX_AddInfo
			Dummy.Z0_Description = "";
			addInfo.Deserialise();
			AssertEquals("Deserialised and cleared", false, addInfo.Z3_Boolean);
		}

		public void TestSerialisationWithDefaultValues()
		{
			Dummy.Z0_Description = "";
			TestXmlAddInfo addInfo = new TestXmlAddInfo(Dummy);
			addInfo.Serialise();
			AssertEquals("", Dummy.Z0_Description);
		}

		public void TestSerialisationDeserialisationWithNonDefaultValues()
		{
			AssertSerialisationDeserialisationWithNonDefaultValues(TestXmlAddInfoSchema.Constants.Z3_Boolean, ZBool.True);
			AssertSerialisationDeserialisationWithNonDefaultValues(TestXmlAddInfoSchema.Constants.Z3_String, new ZString("TEST"));
			AssertSerialisationDeserialisationWithNonDefaultValues(TestXmlAddInfoSchema.Constants.Z3_Date, new ZDateTime(2006, 1, 1, 1, 1, 1));
			AssertSerialisationDeserialisationWithNonDefaultValues(TestXmlAddInfoSchema.Constants.Z3_Decimal, new ZDecimal(257845.359874));
			AssertSerialisationDeserialisationWithNonDefaultValues(TestXmlAddInfoSchema.Constants.Z3_Int, new ZInt(257845));
			AssertSerialisationDeserialisationWithNonDefaultValues(TestXmlAddInfoSchema.Constants.Z3_Guid, ZGuid.NewZGuid());
		}

		void AssertSerialisationDeserialisationWithNonDefaultValues(string propertyName, IZType valueToAssign)
		{
			Dummy.Z0_Description = "";
			TestXmlAddInfo addInfo = new TestXmlAddInfo(Dummy);
			addInfo[propertyName] = valueToAssign;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummyLoaded = factory2.Load<DummyBusinessObject>(Dummy.PK);
			TestXmlAddInfo addInfo2 = new TestXmlAddInfo(dummyLoaded);
			AssertEquals(addInfo[propertyName], addInfo2[propertyName]);

			addInfo2[propertyName] = valueToAssign.Default;
			factory2.Save();

			AssertEquals("", Dummy.Z0_Description);
		}

		public void TestSerialisationWithInvalidDateTime()
		{
			Dummy.Z0_Description = "";
			TestXmlAddInfo addInfo = new TestXmlAddInfo(Dummy);
			addInfo.Z3_Date = ZDateTime.Invalid;
			AssertNoExceptionThrown(delegate
			{ addInfo.Serialise(); });
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestDeletingParentBizoWhenXmlAddInfoHasChangedDoesntThrowExceptionOnSave()
		{
			Dummy.Z0_Description = "";
			var addInfo = new TestXmlAddInfo(Dummy);
			addInfo.Z3_String = "foo";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var dummy2Loaded = factory2.Load<DummyBusinessObject>(Dummy.PK);
			var addInfo2 = new TestXmlAddInfo(dummy2Loaded);

			addInfo2.Z3_String = "bar";
			dummy2Loaded.Delete();

			factory2.Save();
		}

		public void TestClearAllPropertiesBeforeDeserialisation()
		{
			Dummy.Z0_Code = "";
			var correctXmlAddInfo = new CorrectDummyXmlAddInfo((ZPropertyInfoString)Dummy.Z0_CodeInfo);
			AssertEquals("The defaulted values were reserved as we told it NOT to call ClearAllProperties in constructor.", "Dummy", correctXmlAddInfo.Z3_String);

			var incorrectXmlAddInfo = new IncorrectDummyXmlAddInfo((ZPropertyInfoString)Dummy.Z0_CodeInfo);
			AssertEquals("The defaulted values were discarded as we called ClearAllProperties in constructor.", ZString.Empty, incorrectXmlAddInfo.Z3_String);
		}
	}
}

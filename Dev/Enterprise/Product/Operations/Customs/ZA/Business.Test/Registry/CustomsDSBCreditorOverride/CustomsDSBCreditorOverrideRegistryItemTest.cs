using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(CustomsDSBCreditorOverrideRegistryItem))]
	sealed class CustomsDSBCreditorOverrideRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CustomsDSBCreditorOverrideCollection>
	{
		public override void TestCasting()
		{
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			Factory.Save();

			base.TestCasting();
		}

		protected override StronglyTypedRegistryItem<CustomsDSBCreditorOverrideCollection, CustomsDSBCreditorOverrideCollection> GetNewRegistryItem()
		{
			return new CustomsDSBCreditorOverrideRegistryItem("", null, null, null);
		}

		protected override CustomsDSBCreditorOverrideCollection ValidValue
		{
			get
			{
				var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
				orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);

				var organisation = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
				var collection = new CustomsDSBCreditorOverrideCollection();
				var creditor = collection.AddNew();
				creditor.DistrictOfficeCode = "BBR";
				creditor.CreditorPK = organisation.PK;
				return collection;
			}
		}
	}

	[TestedType(typeof(CustomsDSBCreditorOverrideRegistryDataType))]
	sealed class CustomsDSBCreditorOverrideRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CustomsDSBCreditorOverrideRegistryDataType>
	{
		public override void TestGetSetValidValues()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			factory.Save();

			base.TestGetSetValidValues();
		}

		protected override string ExpectedEditorName
		{
			get { return "CustomsDSBCreditorOverrideRegistryItemEditor"; }
		}

		protected override CustomsDSBCreditorOverrideRegistryDataType GetNewDataType()
		{
			return new CustomsDSBCreditorOverrideRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
			var organisation = factory.LoadTop1<OrgHeader>(orgHeaderQuery);

			var collection = new CustomsDSBCreditorOverrideCollection();
			var creditor = collection.AddNew();
			creditor.DistrictOfficeCode = "BBR";
			creditor.CreditorPK = organisation.PK;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new CustomsDSBCreditorOverrideRegistryDataType().Serialise(collection))
			};
		}
	}
}

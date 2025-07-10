using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(FinancialAccountNumberPortMapRegistryItem))]
	sealed class FinancialAccountNumberPortMapRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<FinancialAccountNumberPortMapCollection>
	{
		protected override StronglyTypedRegistryItem<FinancialAccountNumberPortMapCollection, FinancialAccountNumberPortMapCollection> GetNewRegistryItem()
		{
			return new FinancialAccountNumberPortMapRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		protected override FinancialAccountNumberPortMapCollection ValidValue
		{
			get
			{
				var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
				testHelper.CreateCustomsOfficeCusCodeEntry("BBR");

				var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
				orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);

				var organisation = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
				organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
				Factory.Save();

				var collection = new FinancialAccountNumberPortMapCollection();
				var creditor = collection.AddNew();
				creditor.OrganizationPK = organisation.PK;
				creditor.CustomsOfficeCode = "BBR";
				creditor.FinancialAccountNumber = "3924089023";
				creditor.CreditorPK = organisation.PK;
				creditor.AccountStartDay = 1;
				return collection;
			}
		}
	}

	[TestedType(typeof(FinancialAccountNumberPortMapRegistryDataType))]
	sealed class FinancialAccountNumberPortMapRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FinancialAccountNumberPortMapRegistryDataType>
	{
		public override void TestGetSetValidValues()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			factory.Save();

			base.TestGetSetValidValues();
		}

		protected override string ExpectedEditorName
		{
			get { return "FinancialAccountNumberPortMapRegistryItemEditor"; }
		}

		protected override FinancialAccountNumberPortMapRegistryDataType GetNewDataType()
		{
			return new FinancialAccountNumberPortMapRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
			var organisation = factory.LoadTop1<OrgHeader>(orgHeaderQuery);
			organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var record = collection.AddNew();
			record.OrganizationPK = organisation.PK;
			record.CustomsOfficeCode = "BBR";
			record.FinancialAccountNumber = "3924089023";
			record.CreditorPK = organisation.PK;
			record.AccountStartDay = 1;

			var collection2 = new FinancialAccountNumberPortMapCollection();
			var record2 = collection2.AddNew();
			record2.OrganizationPK = organisation.PK;
			record2.CustomsOfficeCode = "BFN";
			record2.FinancialAccountNumber = "9653478996";
			record2.CreditorPK = organisation.PK;
			record2.AccountStartDay = 1;

			var dataType = new FinancialAccountNumberPortMapRegistryDataType();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, dataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, dataType.Serialise(collection2))
			};
		}
	}
}

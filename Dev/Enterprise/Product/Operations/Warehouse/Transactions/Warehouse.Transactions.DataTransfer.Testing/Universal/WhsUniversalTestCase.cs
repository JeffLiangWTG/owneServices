using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsUniversalTestCase : OrganizationAddressTestHelper
	{
		#region IsFinalised Assertions

		public static void AssertIsFinalisedPrecondition(WhsDocket docket) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);

		public static void AssertIsFinalisedPrecondition(WhsDocketLine docketLine) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docketLine);

		public static void AssertIsFinalisedPrecondition(WhsPick pick) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

		public static void AssertIsFinalisedPrecondition(WhsPickLine pickLine) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pickLine);

		public static void AssertIsFinalisedPrecondition(WhsVASOrder vasOrder) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(vasOrder);

		#endregion

		public static void AssertCustomFieldsAreExported(List<CustomizedField> customFields, bool assertTextBlob = false)
		{
			customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
			customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
			customFields.AssertCustomFieldWasExported(DataType.String, "Why this is so long?", "123456789012345");
			customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
			customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", new ZDateTime(2013, 1, 1).ToISO8601String());

			if (assertTextBlob)
			{
				customFields.AssertCustomFieldWasExported(DataType.String, "Some Happy Blob", "BLOBO");
			}
		}

		public static void AssertCustomFieldsImported(ICustomLabelsProvider customLabelsProvider, BusinessObject parentBizo)
		{
			var customProperties = customLabelsProvider.GetCustomFields(customLabelsProvider.ConfigOrgProvider.ConfigOrg, customLabelsProvider.ConfigOrgProvider.Factory).Cast<CustomLabelInfoBase>().Where(o => o.IsEnabled);
			AssertEquals("Lots Of Ice", parentBizo[customProperties.FirstOrDefault(o => o.Caption == "What Makes You Happy?").PropertyName]);
			AssertEquals("123456789012345", parentBizo[customProperties.FirstOrDefault(o => o.Caption == "Why this is so long?").PropertyName]);
			AssertEquals(new ZDateTime(2013, 1, 1), parentBizo[customProperties.FirstOrDefault(o => o.Caption == "The Date You Are Happy").PropertyName]);
			AssertEquals(7.7m, parentBizo[customProperties.FirstOrDefault(o => o.Caption == "The Happy Decimal").PropertyName]);
			AssertEquals(true, parentBizo[customProperties.FirstOrDefault(o => o.Caption == "Are you Happy?").PropertyName]);

			var textBlobCustomLabel = customProperties.FirstOrDefault(o => o.Caption == "Some Happy Blob");
			if (textBlobCustomLabel != null)
			{
				AssertEquals("BLOBO", parentBizo[textBlobCustomLabel.PropertyName]);
			}
		}

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory.BOFactory)); }
		}

		protected TestDataForUniversal Data
		{
			get { return data ?? (data = GetNewTestData()); }
		}

		protected abstract TestDataForUniversal GetNewTestData();

		WhsTestHelperFunctions helper;
		TestDataForUniversal data;
	}
}

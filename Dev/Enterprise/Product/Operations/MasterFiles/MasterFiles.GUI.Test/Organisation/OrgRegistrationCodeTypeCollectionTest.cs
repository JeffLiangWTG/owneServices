using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgRegistrationCodeTypeCollection))]
	sealed class OrgRegistrationCodeTypeCollectionTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectCollectionTestCase<OrgRegistrationCodeTypeCollection>
	{
		protected override OrgRegistrationCodeTypeCollection GetCollectionToTest() => new OrgRegistrationCodeTypeCollectionForTest();

		protected override Type GetExpectedCollectionType() => typeof(OrgRegistrationCodeTypeCollection);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new OrgRegistrationCodeType();

		public void TestInitializeCollectionIsPrimaryColumnSameAsUI()
		{
			var collection = new OrgRegistrationCodeTypeCollection();
			collection.InitializeCollection();

			Assert(collection.Any(o => o is OrgRegistrationCodeType orgType
				&& orgType.Type == OrgCusCode.CodeTypes.CreditAgencyCode
				&& orgType.Country == "US"
				&& orgType.Primary == "Yes"));

			Assert(collection.Any(o => o is OrgRegistrationCodeType orgType
				&& orgType.Type == OrgCusCode.CodeTypes.ExternalCreditorAccountCode
				&& orgType.Country == "AU"
				&& orgType.Primary == "Yes"));
		}
	}
}

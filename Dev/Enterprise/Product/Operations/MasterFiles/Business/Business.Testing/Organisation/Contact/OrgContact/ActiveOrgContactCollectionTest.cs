using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Organisation.Contact.OrgContact
{
	[TestedType(typeof(ActiveOrgContactCollection))]
	class ActiveOrgContactCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveOrgContactCollection>
	{
		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ActiveOrgContactCollection);
		}

		protected override ActiveOrgContactCollection GetCollectionToTest()
		{
			return new ActiveOrgContactCollection(Factory, new ZQuery(OrgContactSchema.OC_OH, org.PK));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return org.Contacts.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			org = Factory.NewWithValidTestData<OrgHeader>();
		}

		OrgHeader org;
		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSuppressedDocumentCollection))]
	class OrgSuppressedDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultDeliveryByShouldBeSet()
		{
			var doc = Documents.AddNew();
			AssertEquals("Default DeliveryBy should be set", Constants.ContactNotifyModes.DoNotDeliver, doc.OD_DeliverBy);
		}

		public void TestFkColumnName()
		{
			var collection = new OrgSuppressedDocumentCollectionForTest(Organization);
			AssertEquals("FkColumnName", OrgDocumentSchema.Constants.OD_OH_Suppressed, collection.FkColumnNameExposed);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSuppressedDocumentCollection(Organization, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Organization = OrgHeader.New(Factory);
			Organization.FillWithValidTestData();

			Documents = Organization.SuppressedDocuments;
		}

		OrgHeader Organization;
		OrgSuppressedDocumentCollection Documents;
	}

	class OrgSuppressedDocumentCollectionForTest : OrgSuppressedDocumentCollection
	{
		public OrgSuppressedDocumentCollectionForTest(OrgHeader orgHeader) : base(orgHeader)
		{
		}

		public ZString FkColumnNameExposed => FkColumnName;
	}
}

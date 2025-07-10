using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccClientInvoiceOrderCollection))]
	sealed class AccClientInvoiceOrderCollectionTest : ActiveBusinessObjectCollectionTestCase<AccClientInvoiceOrderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AccClientInvoiceOrder result = Factory.New<AccClientInvoiceOrder>();
			result.AI_OH_Client = Organisation.PK;
			result.AI_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)).PK;
			return result;
		}

		protected override AccClientInvoiceOrderCollection GetCollectionToTest()
		{
			return new AccClientInvoiceOrderCollection(Factory, Organisation);
		}

		public void TestDefaultsForChildren()
		{
			AccClientInvoiceOrder clientInvoiceOrder = GetCollectionToTest().AddNew();
			AssertEquals("AI_OH_Client", Organisation.PK, clientInvoiceOrder.AI_OH_Client);
		}

		public void TestFilterForOrganisationAndCurrentCompany()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TEST1";
			AccClientInvoiceOrder invoiceOrder1 = Factory.New<AccClientInvoiceOrder>();
			invoiceOrder1.AI_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)).PK;
			invoiceOrder1.AI_OH_Client = organisation.PK;
			organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TEST2";
			AccClientInvoiceOrder invoiceOrder2 = Factory.New<AccClientInvoiceOrder>();
			invoiceOrder2.AI_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;
			invoiceOrder2.AI_OH_Client = organisation.PK;
			AccClientInvoiceOrder invoiceOrder3 = Factory.New<AccClientInvoiceOrder>();
			invoiceOrder3.AI_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK)).PK;
			invoiceOrder3.AI_OH_Client = organisation.PK;
			Factory.Save();
			AccClientInvoiceOrderCollection clientInvoiceOrderCollection = new AccClientInvoiceOrderCollection(Factory, organisation);
			AssertEquals("Must be 1", 1, clientInvoiceOrderCollection.Count);
		}

		public void TestAllowsMissingChargeCodes()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			AccClientInvoiceOrder invoiceOrder = Factory.New<AccClientInvoiceOrder>();
			invoiceOrder.AI_OH_Client = organisation.PK;
			invoiceOrder.AI_AC = ZGuid.Missing;

			AccClientInvoiceOrderCollection clientInvoiceOrderCollection = new AccClientInvoiceOrderCollection(Factory, organisation);
			AssertEquals("Count of elements", 1, clientInvoiceOrderCollection.Count);
			AssertCollectionContains(invoiceOrder, clientInvoiceOrderCollection);

			invoiceOrder.AI_AC = ZGuid.Invalid;
			clientInvoiceOrderCollection = new AccClientInvoiceOrderCollection(Factory, organisation);
			AssertEquals("Count of elements", 1, clientInvoiceOrderCollection.Count);
			AssertCollectionContains(invoiceOrder, clientInvoiceOrderCollection);
		}

		#region Implementation

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.New<OrgHeader>();
					fOrganisation.OH_Code = "TESTORG";
				}
				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;

		#endregion
	}
}

using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestsSubclassesOf(typeof(GroupInvoiceChargeLookups))]
	abstract class GroupInvoiceChargeLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
		where T : GroupInvoiceChargeLookups
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			charge = declaration.TopGroupInvoice.Charges.AddNew();
			lookups = charge.Lookups as T;
		}

		protected JobDeclaration declaration;
		protected GroupInvoiceCharge charge;
		protected T lookups;

		protected abstract string MessageType { get; }

		public void TestLookupType()
		{
			AssertType<T>(charge.Lookups);
		}
	}
}

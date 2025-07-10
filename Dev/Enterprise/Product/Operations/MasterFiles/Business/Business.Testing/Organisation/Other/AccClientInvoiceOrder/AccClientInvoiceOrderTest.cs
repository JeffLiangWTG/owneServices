using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccClientInvoiceOrder))]
	public class AccClientInvoiceOrderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAC_Desc()
		{
			AccClientInvoiceOrder invoiceOrder = Factory.New<AccClientInvoiceOrder>();
			invoiceOrder.AI_AC = ZGuid.Empty;
			AssertEquals("AC_Desc must be empty", ZString.Empty, invoiceOrder.AC_Desc);
			AccChargeCodeCollection collection = new AccChargeCodeCollection(Factory, ((IBusinessObjectCollection)invoiceOrder.Lookups.ChargeCodes).CompleteFilter);
			collection.Load();
			invoiceOrder.AI_AC = collection[0].PK;
			AssertEquals("AC_Desc", collection[0].AC_Desc, invoiceOrder.AC_Desc);

			List<ZGuid> validCharges = new List<ZGuid>();
			foreach (AccChargeCode charge in collection)
			{
				validCharges.Add(charge.PK);
			}
			AccChargeCode invalidCharge = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, validCharges.ToArray()));
			AssertNotNull("Precondition: Should be at least one charge not in the Lookup list", invalidCharge);
			invoiceOrder.AI_AC = invalidCharge.PK;

			AssertEquals("AC_Desc must be empty", ZString.Empty, invoiceOrder.AC_Desc);
		}

		public void TestAC_PrintSequence()
		{
			AccClientInvoiceOrder invoiceOrder = Factory.New<AccClientInvoiceOrder>();
			invoiceOrder.AI_AC = ZGuid.Empty;
			AssertEquals("AC_PrintSequence must be 0", ZShort.Zero, invoiceOrder.AC_PrintSequence);
			AccChargeCodeCollection collection = new AccChargeCodeCollection(Factory, ((IBusinessObjectCollection)invoiceOrder.Lookups.ChargeCodes).CompleteFilter);
			collection.Load();
			invoiceOrder.AI_AC = collection[0].PK;
			AssertEquals("AC_PrintSequence", collection[0].AC_PrintSequence, invoiceOrder.AC_PrintSequence);

			List<ZGuid> validCharges = new List<ZGuid>();
			foreach (AccChargeCode charge in collection)
			{
				validCharges.Add(charge.PK);
			}
			AccChargeCode invalidCharge = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, validCharges.ToArray()));
			AssertNotNull("Precondition: Should be at least one charge not in the Lookup list", invalidCharge);
			invoiceOrder.AI_AC = invalidCharge.PK;

			AssertEquals("AC_PrintSequence must be 0", ZShort.Zero, invoiceOrder.AC_PrintSequence);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			return org.InvoiceOrders.AddNew();
		}

		protected AccClientInvoiceOrder InvoiceOrder;

		protected override void SetUp()
		{
			base.SetUp();
			InvoiceOrder = (AccClientInvoiceOrder)GetNewBusinessObject();
		}

		#endregion
	}
}

using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(SecondaryNotifyParty))]
	public class SecondaryNotifyPartyTest : Customs.Business.Testing.CusCodeDataTest<SecondaryNotifyParty>
	{
		public void TestICanDeleteMembers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			var snp = bill.SecondaryNotifyParties.AddNew();
			var snp2 = bill.SecondaryNotifyParties.AddNew();
			var snp3 = bill.SecondaryNotifyParties.AddNew();
			snp3.CY_Order = 3;
			snp2.CY_Order = 2;
			snp.CY_Order = 1;
			ICanDelete canDelete1 = snp;
			ICanDelete canDelete2 = snp2;
			ICanDelete canDelete3 = snp3;
			AssertEquals(false, canDelete1.CanDelete);
			AssertEquals(false, canDelete2.CanDelete);
			AssertEquals(true, canDelete3.CanDelete);

			header.BH_OverrideFreightDefaults = true;
			AssertEquals(true, canDelete1.CanDelete);
			AssertEquals(true, canDelete2.CanDelete);
			AssertEquals(true, canDelete3.CanDelete);
		}

		public void TestCY_OrderReadOnly()
		{
			var snp = Factory.New<SecondaryNotifyParty>();
			AssertEquals(false, snp.CY_OrderInfo.ReadOnly);
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill = header.Bills.AddNew();
			snp.Parent = bill;
			AssertEquals(true, snp.CY_OrderInfo.ReadOnly);
			header.BH_OverrideFreightDefaults = true;
			AssertEquals(false, snp.CY_OrderInfo.ReadOnly);
		}

		public void TestValidation()
		{
			var snp = Factory.New<SecondaryNotifyParty>();
			AssertEquals("Validation", typeof(SecondaryNotifyPartyValidation), snp.Validation.GetType());
		}

		public void TestSetDefaultValues()
		{
			var snp = Factory.New<SecondaryNotifyParty>();
			AssertEquals(SecondaryNotifyParty.SNPType, snp.CY_Type);
		}

		public void TestParent()
		{
			var snp = Factory.New<SecondaryNotifyParty>();
			snp.CY_ParentID = Bill.PK;
			snp.CY_ParentTableCode = Bill.TablePrefix;
			AssertEquals(Bill, snp.Parent);
		}

		public override void TestSettingValueCallsRefreshBinding()
		{
			base.TestSettingValueCallsRefreshBinding();
			if (ErrorReporter.LastKeyReported == "Enterprise.Customs.US.AMS.Business.SecondaryNotifyParty.CY_Type Invalid Setting")
			{
				ErrorReporter.Clear();
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<SecondaryNotifyParty>();
		}

		protected override IEnumerable<SecondaryNotifyParty> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<SecondaryNotifyParty>();

			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.SecondaryNotifyParties.Add(result);

			yield return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var snp = Factory.New<SecondaryNotifyParty>();
			snp.Parent = Bill;
			return snp;
		}

		CusInBondBill Bill
		{
			get
			{
				if (bill == null)
				{
					var header = Factory.New<CusInBondHeader>();
					bill = header.Bills.AddNew();
				}
				return bill;
			}
		}
		CusInBondBill bill;

		#endregion
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvoiceRollupOrGroupWithHelperTest : RollupOrGroupWithHelperBaseTest
	{
		public void TestStyleShouldBeEqualNogWhenDisplayIsNotRolOrSubOrSsq()
		{
			InvoiceRollupOrGroup.GroupOrSubTotal = OrgInvoiceRollupOrGroup.GroupOrSubtotalOptionDefaultCode;
			AssertEquals("Style must change to 'DEF'", OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode, InvoiceRollupOrGroup.GroupOrSubtotalStyle);
		}

		public override void TestValidateInvoicePostingStyle()
		{
			AssertProperty(InvoiceRollOrGroupForTest.PG_InvoicePostingStyleInfo, () => base.TestValidateInvoicePostingStyle(), true);
		}

		public override void TestValidateInvoiceLineDisplayOption()
		{
			AssertProperty(InvoiceRollOrGroupForTest.PG_InvoiceLineDisplayOptionInfo, () => base.TestValidateInvoiceLineDisplayOption(), true);
		}

		public override void TestValidateJobType()
		{
			AssertProperty(InvoiceRollOrGroupForTest.PG_JobTypeInfo, () => base.TestValidateJobType());
		}

		public override void TestValidateServiceDirection()
		{
			AssertProperty(InvoiceRollOrGroupForTest.PG_ServiceDirectionInfo, () => base.TestValidateServiceDirection());
		}

		public override void TestValidateTransportMode()
		{
			AssertProperty(InvoiceRollOrGroupForTest.PG_TransportModeInfo, () => base.TestValidateTransportMode());
		}

		public override void TestValidateGroupOrSubTotal()
		{
			AssertProperty(InvoiceRollOrGroupForTest.PG_GroupOrSubTotalInfo, () => base.TestValidateGroupOrSubTotal(), true);
		}

		public override void TestValidateGroupOrSubTotalStyle()
		{
			AssertProperty(InvoiceRollOrGroupForTest.PG_GroupOrSubtotalStyleInfo, () => base.TestValidateGroupOrSubTotalStyle(), true);
		}
		protected override IInvoiceRollupOrGroup InvoiceRollupOrGroup
		{
			get { return InvoiceRollOrGroupForTest; }
		}

		protected override IInvoiceRollupOrGroup SecondInvoiceRollupOrGroup
		{
			get { return SecondInvoiceRollupOrGroupForTest; }
		}

		protected override bool IsDefaultCodeShouldBeInLookups
		{
			get { return true; }
		}

		protected override void RunPreSaveValidation()
		{
			InvoiceRollOrGroupForTest.RunPreSaveValidation();
			SecondInvoiceRollupOrGroupForTest.RunPreSaveValidation();
		}

		OrgInvoiceRollupOrGroup InvoiceRollOrGroupForTest;
		OrgInvoiceRollupOrGroup SecondInvoiceRollupOrGroupForTest;
		OrgHeader Org;
		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			Org.OH_IsDebtor = true;
			InvoiceRollOrGroupForTest = Org.CompanyData.InvoiceRollupOrGroups[0];
			SecondInvoiceRollupOrGroupForTest = Org.CompanyData.InvoiceRollupOrGroups.AddNew();
			AssertEquals("Precondition: collection must have only 2 elements.", 2, Org.CompanyData.InvoiceRollupOrGroups.Count);
		}

		void AssertProperty(ZPropertyInfo propertyInfo, Action baseTest, bool isDefaultValueApplicable = false)
		{
			Org.OH_IsDebtor = false;
			propertyInfo.Value = (ZString)"RRR";
			AssertNoNotifications("Company is not debtor, no notifications", propertyInfo);

			propertyInfo.Value = ZString.Empty;
			AssertNoNotifications("Company is not debtor, no notifications", propertyInfo);

			Org.OH_IsDebtor = true;
			baseTest();

			if (isDefaultValueApplicable)
			{
				propertyInfo.Value = (ZString)OrgInvoiceRollupOrGroup.GroupOrSubtotalStyleOptionDefaultCode;
				AssertNoNotifications("No notifications for default code.", propertyInfo);
			}
		}
	}
}

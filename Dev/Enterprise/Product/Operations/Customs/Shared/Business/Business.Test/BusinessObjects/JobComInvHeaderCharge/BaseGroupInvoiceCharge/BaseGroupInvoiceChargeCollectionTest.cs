using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class BaseGroupInvoiceChargeCollectionBaseOnlyTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestTypeOfElements()
		{
			var localDec = Factory.New<BaseJobDeclaration>();
			var localInvoice = localDec.TopGroupInvoice;
			AssertEquals("TypeOfElements should be specified for adding a new item by users", typeof(BaseGroupInvoiceCharge), localInvoice.Charges.TypeOfElements);
		}

		public void TestCollectionElementsAreCreatedWithTheRightType()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "ZUS";
			usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "ZUS";
			usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var localDec = Factory.New<BaseJobDeclaration>();
			localDec.JE_GB = GlbBranch.CurrentBranch.PK;
			var localInvoice = localDec.TopGroupInvoice;
			var localInvoiceCharge1 = localInvoice.Charges.AddNew();

			var usDec = Factory.New<BaseJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			var usInvoice = usDec.TopGroupInvoice;
			var usInvoiceCharge1 = usInvoice.Charges.AddNew();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var localDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(localDec.PK);
			var localInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceGroupHeader>(localInvoice.PK);
			var localInvoiceCharge1InDiffFactory = newFactory.Load<BaseGroupInvoiceCharge>(localInvoiceCharge1.PK);

			var usDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(usDec.PK);
			var usInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceGroupHeader>(usInvoice.PK);
			var usInvoiceCharge1InDiffFactory = newFactory.Load<BaseGroupInvoiceCharge>(usInvoiceCharge1.PK);

			AssertEquals("localInvoiceCharge1InDiffFactory Type", typeof(BaseGroupInvoiceCharge), localInvoiceCharge1InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceCharge1InDiffFactory.GetType(), usInvoiceCharge1InDiffFactory.GetType());

			var localInvoiceCharge2InDiffFactory = localInvoiceInDiffFactory.Charges.AddNew();
			var usInvoiceCharge2InDiffFactory = usInvoiceInDiffFactory.Charges.AddNew();
			AssertEquals("localInvoiceCharge2InDiffFactory Type", typeof(BaseGroupInvoiceCharge), localInvoiceCharge2InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceCharge2InDiffFactory.GetType(), usInvoiceCharge2InDiffFactory.GetType());
		}
	}

	public abstract class BaseGroupInvoiceChargeCollectionTest<T> : Common.Testing.ChargeCollectionTest<JobComInvChargeCollection<T>, T> where T : BaseGroupInvoiceCharge
	{
		public void TestApportionmentDirtyWhenAnInvoiceChargeGetsRemoved()
		{
			BaseGroupInvoiceCharge testCharge = GroupHeader.Charges.AddNew();
			testCharge.J7_ChargeType = "OFT";
			testCharge.J7_Amount = 100m;
			testCharge.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			GroupHeader.JobDeclaration.ApportionmentDirty = false;
			GroupHeader.Charges.RemoveAndDelete(testCharge);
			AssertEquals("Apportionment is dirty", true, GroupHeader.JobDeclaration.ApportionmentDirty);
		}

		public void TestOneRowOfGroupChargeRemovedLeadToReapportionTheOtherRow()
		{
			BaseJobComInvoiceHeader invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvHeaderCharge oFT1 = GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, GroupHeader.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oFT2 = GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200, GroupHeader.JobDeclaration.LocalCurrencyCode);
			TestDec.ResumeApportionment();
			AssertEquals("One Apportioned group charge", 1, invoice.GroupCharges.Count);
			AssertEquals("Apportioned Amount", 300m, invoice.GroupCharges[0].J7_Amount);

			GroupHeader.Charges.RemoveAndDelete(oFT1);
			TestDec.ResumeApportionment();
			AssertEquals("One Apportioned group charge", 1, invoice.GroupCharges.Count);
			AssertEquals("Apportioned Amount", 200m, invoice.GroupCharges[0].J7_Amount);
		}

		public void TestRemoveGroupChargeClearApportionedCharges()
		{
			BaseJobComInvoiceHeader invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;

			GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, GroupHeader.JobDeclaration.LocalCurrencyCode);
			TestDec.ResumeApportionment();
			AssertEquals("Invoice has one apportioned charge", 1, invoice.GroupCharges.Count);

			GroupHeader.Charges.RemoveAndDeleteAll();
			TestDec.ResumeApportionment();
			AssertEquals("Invoice has no apportioned charge", 0, invoice.GroupCharges.Count);
		}

		public void TestRemoveGroupChargeReApportionChargesWhenSubGroupChargesAreRemoved()
		{
			BaseJobComInvoiceHeader invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvoiceGroupHeader subGroup = GroupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 1000;
			invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, GroupHeader.JobDeclaration.LocalCurrencyCode);
			subGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 60, GroupHeader.JobDeclaration.LocalCurrencyCode);
			TestDec.ResumeApportionment();
			AssertEquals("Invoice1 OFT", 40m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 OFT", 60m, invoice2.GroupCharges[0].J7_Amount);

			subGroup.Charges.RemoveAndDeleteAll();
			TestDec.ResumeApportionment();
			AssertEquals("Invoice1 OFT", 50m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 OFT", 50m, invoice2.GroupCharges[0].J7_Amount);
		}

		public void TestRemoveGroupChargeReApportionChargesWhenTopGroupChargesAreRemoved()
		{
			BaseJobComInvoiceHeader invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 1000;
			invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			BaseJobComInvoiceGroupHeader subGroup = GroupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 1000;
			invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, GroupHeader.JobDeclaration.LocalCurrencyCode);
			subGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 60, GroupHeader.JobDeclaration.LocalCurrencyCode);
			TestDec.ResumeApportionment();
			AssertEquals("Invoice1 OFT", 40m, invoice1.GroupCharges[0].J7_Amount);
			AssertEquals("Invoice2 OFT", 60m, invoice2.GroupCharges[0].J7_Amount);

			GroupHeader.Charges.RemoveAndDeleteAll();
			TestDec.ResumeApportionment();
			AssertEquals("Invoice1 OFT", 0, invoice1.GroupCharges.Count);
			AssertEquals("Invoice2 OFT", 60m, invoice2.GroupCharges[0].J7_Amount);
		}

		#region Implementation

		BaseJobDeclaration TestDec
		{
			get
			{
				CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
				{
					if (fTestDec == null)
					{
						fTestDec = BaseJobDeclaration.New(Factory);
					}
					return fTestDec;
				}
			}
		}
		BaseJobDeclaration fTestDec;

		BaseJobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (fGroupHeader == null)
				{
					fGroupHeader = TestDec.JobComInvoiceGroupHeaders[0];
				}
				return fGroupHeader;
			}
		}
		BaseJobComInvoiceGroupHeader fGroupHeader;

		protected JobComInvChargeCollection<BaseGroupInvoiceCharge> GroupCharges => (JobComInvChargeCollection<BaseGroupInvoiceCharge>)GroupHeader.Charges;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseGroupInvoiceCharge result = Factory.New<BaseGroupInvoiceCharge>();
			result.Parent = GroupHeader;
			return result;
		}

		#endregion
	}

	[TestedType(typeof(BaseGroupInvoiceCharge))]
	public class BaseGroupInvoiceChargeCollectionTest : BaseGroupInvoiceChargeCollectionTest<BaseGroupInvoiceCharge>
	{
		protected override JobComInvChargeCollection<BaseGroupInvoiceCharge> GetCollectionToTest()
		{
			return GroupCharges;
		}
	}
}

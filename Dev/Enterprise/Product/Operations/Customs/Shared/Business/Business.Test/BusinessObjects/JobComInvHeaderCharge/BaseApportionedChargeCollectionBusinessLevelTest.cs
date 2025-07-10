using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(IJobComInvApportionedChargeCollection<BaseApportionedCharge>))]
	class BaseApportionedChargeCollectionBusinessLevelTest : JobComInvApportionedChargeCollectionTest<IJobComInvApportionedChargeCollection<BaseApportionedCharge>, BaseApportionedCharge>
	{
		public void TestTypeOfElementsSpecified()
		{
			var localDec = Factory.New<BaseJobDeclaration>();
			var localInvoice = localDec.Invoices.AddNew();
			AssertEquals("TypeOfElement should be specified for adding a new BizObj", typeof(BaseApportionedCharge), localInvoice.GroupCharges.TypeOfElements);
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
			var localInvoice = localDec.Invoices.AddNew();
			var localInvoiceCharge1 = localInvoice.GroupCharges.AddNew();

			var usDec = (BaseJobDeclaration)Factory.New<Integration.Customs.US.IJobDeclaration>();
			usDec.JE_GB = usBranch.PK;
			var usInvoice = usDec.Invoices.AddNew();
			var usInvoiceCharge1 = usInvoice.GroupCharges.AddNew();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var localDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(localDec.PK);
			var localInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(localInvoice.PK);
			var localInvoiceCharge1InDiffFactory = newFactory.Load<BaseApportionedCharge>(localInvoiceCharge1.PK);

			var usDecInDiffFactory = newFactory.Load<BaseJobDeclaration>(usDec.PK);
			var usInvoiceInDiffFactory = newFactory.Load<BaseJobComInvoiceHeader>(usInvoice.PK);
			var usInvoiceCharge1InDiffFactory = newFactory.Load<BaseApportionedCharge>(usInvoiceCharge1.PK);

			AssertEquals("localInvoiceCharge1InDiffFactory Type", typeof(BaseApportionedCharge), localInvoiceCharge1InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceCharge1InDiffFactory.GetType(), usInvoiceCharge1InDiffFactory.GetType());

			var localInvoiceCharge2InDiffFactory = localInvoiceInDiffFactory.GroupCharges.AddNew();
			var usInvoiceCharge2InDiffFactory = usInvoiceInDiffFactory.GroupCharges.AddNew();
			AssertEquals("localInvoiceCharge2InDiffFactory Type", typeof(BaseApportionedCharge), localInvoiceCharge2InDiffFactory.GetType());
			AssertNotEquals("Should be different type", localInvoiceCharge2InDiffFactory.GetType(), usInvoiceCharge2InDiffFactory.GetType());
		}

		public void TestDefaultValues()
		{
			BaseApportionedCharge apportionedCharge = InvoiceHeader.GroupCharges.AddNew();
			AssertEquals("IsApportioned set to true", true, apportionedCharge.J7_IsApportionedCharge);
		}

		BaseJobDeclaration fTestDec;
		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = BaseJobDeclaration.New(Factory);
				}
				return fTestDec;
			}
		}

		BaseJobComInvoiceGroupHeader fGroupHeader;
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

		BaseJobComInvoiceHeader fInvoiceHeader;
		protected BaseJobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = GroupHeader.JobComInvoiceHeaders.AddNew();
				}
				return fInvoiceHeader;
			}
		}

		protected override JobComInvApportionedChargeCollection<BaseApportionedCharge> GetCollectionToTest()
		{
			return new JobComInvApportionedChargeCollection<BaseApportionedCharge>(InvoiceHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseApportionedCharge result = Factory.New<BaseApportionedCharge>();
			result.Parent = InvoiceHeader;
			return result;
		}
	}
}

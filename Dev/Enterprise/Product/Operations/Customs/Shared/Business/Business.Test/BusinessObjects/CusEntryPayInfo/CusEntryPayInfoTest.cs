using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfo))]
	public class CusEntryPayInfoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsPending()
		{
			CusEntryPayInfo payInfo = Factory.New<CusEntryPayInfo>();
			AssertEquals("isPending", false, payInfo.IsPending);

			payInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Pending;
			AssertEquals("IsPending", true, payInfo.IsPending);

			payInfo.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
			AssertEquals("IsPending", false, payInfo.IsPending);
		}

		public void TestEntryHeader()
		{
			CusEntryPayInfo payInfo = EntryHeader.EntryPayInfos.AddNew();
			AssertEquals("EntryHeader", EntryHeader, payInfo.EntryHeader);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return EntryHeader.EntryPayInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return EntryHeader.EntryPayInfos.AddNew();
		}

		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<BaseJobDeclaration>();
				}

				return fTestDec;
			}
		}
		BaseJobDeclaration fTestDec;

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = TestDec.CustomsEntryHeaders.AddNew();
				}

				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;
		#endregion

	}
}

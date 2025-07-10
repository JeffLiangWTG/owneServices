using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInbondBillAddRef))]
	sealed class CusInbondBillAddRefTest : Customs.Business.Testing.CusInbondBillAddRefTest<CusInbondBillAddRef>
	{
		public void TestIInBondBillReferenceNumberMembers()
		{
			billAdditionalReference.BR_Qualifier = ReferenceQualifierList.Codes.CN;
			billAdditionalReference.BR_ReferenceNum = "CN2342";
			IInBondBillReferenceNumber reference = billAdditionalReference;
			AssertEquals(ReferenceQualifierList.Codes.CN, reference.Qualifier);
			AssertEquals("CN2342", reference.ReferenceIdentifier);
		}

		public void TestBR_QualifierDescription()
		{
			billAdditionalReference.BR_Qualifier = ZString.Empty;
			AssertEquals(ZString.Empty, billAdditionalReference.BR_QualifierDescription);
			foreach (ICodeDescription pair in Factory.GetCachedValue<ReferenceQualifierList>())
			{
				billAdditionalReference.BR_Qualifier = pair.Code;
				AssertEquals(pair.Code, pair.Description, billAdditionalReference.BR_QualifierDescription);
			}
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInbondBillAddRefLookups), billAdditionalReference.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInbondBillAddRefValidation), billAdditionalReference.Validation.GetType());
		}

		CusInBondHeader header;
		CusInBondBill bill;
		CusInbondBillAddRef billAdditionalReference;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			bill = header.Bills.AddNew();
			billAdditionalReference = bill.AdditionalReferences.AddNew();
		}
	}
}

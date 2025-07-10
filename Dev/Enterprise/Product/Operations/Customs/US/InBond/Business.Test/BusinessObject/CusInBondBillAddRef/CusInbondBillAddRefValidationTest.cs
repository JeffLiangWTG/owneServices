using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInbondBillAddRefValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBR_Qualifier()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInbondBillAddRef reference = bill.AdditionalReferences.AddNew();
			reference.BR_Qualifier = ZString.Empty;
			AssertHasMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
			reference.BR_Qualifier = "Z!";
			AssertNoMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(reference.BR_QualifierInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in Factory.GetCachedValue<ReferenceQualifierList>())
			{
				reference.BR_Qualifier = pair.Code;
				AssertNoMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(reference.BR_QualifierInfo, ListValidation.InvalidCodeMessageError);
			}

			header.BH_ImportTransportMode = Enterprise.Customs.US.Business.InBondTransportModeCodes.Codes.AirNonContainer;
			reference.BR_Qualifier = ZString.Empty;
			AssertNoMessageErrorContaining(reference.BR_QualifierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBR_ReferenceNum()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var reference = bill.AdditionalReferences.AddNew();
			reference.BR_ReferenceNum = ZString.Empty;
			AssertHasMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
			reference.BR_ReferenceNum = "HB23423";
			AssertNoMessageErrorContaining(reference.BR_ReferenceNumInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}

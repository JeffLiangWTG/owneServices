using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class CusFiscalReferenceProviderTest : TestCaseWithFactory
{
	public void TestGetNewValidation() => AssertType<CusFiscalReferenceValidation>(cusFiscalReference.Validation);

	public void TestGetNewLookup() => AssertType<CusFiscalReferenceLookups>(cusFiscalReference.Lookups);

	public void TestReferenceIsReadOnly()
	{
		AssertEquals("Reference is ReadOnly", true, cusFiscalReference.CFR_ReferenceInfo.ReadOnly);
	}

	public void TestRecalculateReferenceIfNeeded()
	{
		cusFiscalReference.CFR_Reference = "XYZ";
		var organisation = Factory.New<OrgHeader>();
		var orgAddress = organisation.Addresses.AddNew();

		cusFiscalReference.CFR_OA_Owner = orgAddress.PK;

		CombineAssertions(() =>
		{
			AssertEquals("TIN not set, Reference set to empty", string.Empty, cusFiscalReference.CFR_Reference);

			cusFiscalReference.CFR_OA_Owner = ZGuid.Empty;
			var orgCusCode = organisation.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			orgCusCode.OK_CustomsRegNo = "ABC";
			cusFiscalReference.CFR_OA_Owner = orgAddress.PK;
			AssertEquals("Reference set to TIN", "ABC", cusFiscalReference.CFR_Reference);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
	}

	CusFiscalReference cusFiscalReference;
}

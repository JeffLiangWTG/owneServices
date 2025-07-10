using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGCountryReferencePivot))]
	public class UNDGCountryReferencePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void Test_StorageInstruction_TankStorageInstructionRetentionTray_ReadOnly()
		{
			var (icpeReferencePivot, psaReferencePivot) = CreateSubstanceWithICPEAndPSAReference();
			var isAllowed = Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed;
			try
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = true;
				AssertEquals(false, icpeReferencePivot.DCP_StorageInstructionInfo.ReadOnly);
				AssertEquals(false, icpeReferencePivot.DCP_TankStorageInstructionRetentionTrayInfo.ReadOnly);
				AssertEquals(true, psaReferencePivot.DCP_StorageInstructionInfo.ReadOnly);
				AssertEquals(true, psaReferencePivot.DCP_StorageInstructionInfo.ReadOnly);

				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = false;
				AssertEquals(true, icpeReferencePivot.DCP_StorageInstructionInfo.ReadOnly);
				AssertEquals(true, icpeReferencePivot.DCP_TankStorageInstructionRetentionTrayInfo.ReadOnly);
				AssertEquals(true, psaReferencePivot.DCP_StorageInstructionInfo.ReadOnly);
				AssertEquals(true, psaReferencePivot.DCP_StorageInstructionInfo.ReadOnly);
			}
			finally
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = isAllowed;
			}
		}

		(UNDGCountryReferencePivot, UNDGCountryReferencePivot) CreateSubstanceWithICPEAndPSAReference()
		{
			var substance = DGSubstanceTestHelper.Create("0000", "A", UNDGSubstanceStandardTypes.IMO);
			var icpeReference = Factory.New<UNDGCountryReference>();
			icpeReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.ICPE;
			icpeReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.France;
			icpeReference.DCR_Code = "1111";
			var psaReference = Factory.New<UNDGCountryReference>();
			psaReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			psaReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			psaReference.DCR_Code = "2222";

			var icpeReferencePivot = Factory.New<UNDGCountryReferencePivot>();
			icpeReferencePivot.DCP_DCR = icpeReference.PK;
			icpeReferencePivot.DCP_Standard = substance.DG_Standard;
			icpeReferencePivot.DCP_Variant = substance.DG_Variant;
			icpeReferencePivot.DCP_UNNO = substance.DG_UNNO;

			var psaReferencePivot = Factory.New<UNDGCountryReferencePivot>();
			psaReferencePivot.DCP_DCR = psaReference.PK;
			psaReferencePivot.DCP_Standard = substance.DG_Standard;
			psaReferencePivot.DCP_Variant = substance.DG_Variant;
			psaReferencePivot.DCP_UNNO = substance.DG_UNNO;

			return (icpeReferencePivot, psaReferencePivot);
		}
	}

	public class UNDGCountryReferencePivotVirtualTests : TestCaseWithFactory
	{
		UNDGSubstance Subs1;
		UNDGSubstance Subs2;

		protected override void SetUp()
		{
			base.SetUp();
			Subs1 = DGSubstanceTestHelper.Create("0000", "A", UNDGSubstanceStandardTypes.IMO);
			Subs2 = DGSubstanceTestHelper.Create("0000", "B", UNDGSubstanceStandardTypes.IMO);
		}

		public void TestVirtualPivotGetter()
		{
			var pivot = Factory.New<UNDGCountryReferencePivot>();
			pivot.DCP_DCR = Factory.NewWithValidTestData<UNDGCountryReference>().PK;
			pivot.DCP_Standard = Subs2.DG_Standard;
			pivot.DCP_Variant = Subs2.DG_Variant;
			pivot.DCP_UNNO = Subs2.DG_UNNO;
			AssertEquals("Virtual PK should be subs2", Subs2.PK, pivot.DCP_DG_Virtual);
			AssertEquals("Virtual PK using [] should be subs2", Subs2.PK, pivot["DCP_DG_Virtual"]);
		}

		public void TestVirtualPivotGet_Null()
		{
			var pivot = Factory.New<UNDGCountryReferencePivot>();
			pivot.DCP_DCR = Factory.NewWithValidTestData<UNDGCountryReference>().PK;
			pivot.DCP_UNNO = "0001";
			AssertEquals(ZGuid.Empty, pivot.DCP_DG_Virtual);
		}

		public void TestVirtualPivotSetter()
		{
			var pivot = Factory.New<UNDGCountryReferencePivot>();
			pivot.DCP_DCR = Factory.NewWithValidTestData<UNDGCountryReference>().PK;
			pivot.DCP_DG_Virtual = Subs1.PK;
			AssertEquals(Subs1.DG_UNNO, pivot.DCP_UNNO);
			AssertEquals(Subs1.DG_Variant, pivot.DCP_Variant);
			AssertEquals(Subs1.DG_Standard, pivot.DCP_Standard);

			var otherObj = Factory.NewWithValidTestData<UNDGCommonData>();
			pivot.DCP_DG_Virtual = otherObj.PK;
			CombineAssertions("Invalid pivot PK should erase values", () =>
			{
				AssertEquals(ZString.Empty, pivot.DCP_UNNO);
				AssertEquals(ZString.Empty, pivot.DCP_Variant);
				AssertEquals(ZString.Empty, pivot.DCP_Standard);
			});
		}
	}
}

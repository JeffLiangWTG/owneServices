using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class OrgHeaderExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestIsUSOrganisation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "USXXX";
			Assert(org.IsUSOrganisation());
			org.OH_RL_NKClosestPort = "PRXXX";
			Assert(org.IsUSOrganisation());
		}

		public void TestIsCustomsDisbursementCreditor()
		{
			OrgHeader org = null;
			AssertEquals(false, org.IsCustomsDisbursementCreditor());

			org = Factory.New<OrgHeader>();
			AssertEquals(false, org.IsCustomsDisbursementCreditor());

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "~US";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var prCompany = Factory.New<GlbCompany>();
			prCompany.GC_Code = "~PR";
			prCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;

			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "~NZ";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(nzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, org.PK.ToGuid());
			AssertEquals("Not under US jurisdiction", false, org.IsCustomsDisbursementCreditor());

			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(prCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, org.PK.ToGuid());
			AssertEquals("Disbursement Creditor set for PR company", true, org.IsCustomsDisbursementCreditor());
		}
	}
}

using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCompanyWrapperTest : TestCaseWithFactory, ITestDynamicObjectHandleSupporter
	{
		public void TestGetWrapper()
		{
			var mockAsycudaCountryProvider = new Mock<IAsycudaCustomsCountryProvider>();
			mockAsycudaCountryProvider.Setup(m => m.IsAsycudaCustomsCountry(new ZString(Core.Constants.CountryCodes.Botswana))).Returns(true);

			var company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = "A1";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var company3 = Factory.New<GlbCompany>();
			company3.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Botswana;
			var company4 = Factory.New<GlbCompany>();
			company4.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Kazakhstan;

			var companyWrappersHash = new Hashtable();
			companyWrappersHash.Add("A1", new TestDynamicObjectHandle(this));
			companyWrappersHash.Add(Core.Constants.CountryCodes.UnitedStates, new TestDynamicObjectHandle(this));
			companyWrappersHash.Add(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, new TestDynamicObjectHandle(this));
			using (ObjectFactory.Substitute("GlbCompanyWrappers", companyWrappersHash))
			{
				AssertNull("Null company", GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(null));
				var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(company1);
				AssertNotNull("company1", wrapper);
				AssertSame("company1 should be cached", wrapper, GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(company1));
				company1 = Factory.New<GlbCompany>();
				company1.GC_RN_NKCountryCode = "A1";
				AssertEquals("new company1 should result in new wrapper", false, object.ReferenceEquals(wrapper, GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(company1)));
				wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(company2);
				AssertNotNull("company2 CustomsCountryOfJurisdiction", wrapper);
				AssertSame("company2 CustomsCountryOfJurisdiction should be cached", wrapper, GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(company2));

				using (ObjectFactory.Substitute(mockAsycudaCountryProvider.Object))
				{
					wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(company3);
					AssertNotNull("company3 Asycuda", wrapper);
					AssertSame("company3 Asycuda should be cached", wrapper, GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForTesting>(company3));
				}

				AssertNotNull("ICS2 tab", GlbCompanyWrapper.GetWrapper<GlbCompanyWrapperForOnlyICS2>(company4));
			}
		}

		public void TestGlbCompanyCredentialICS2()
		{
			CombineAssertions(() =>
			{
				var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany);
				AssertNull("Initial ICS2 certificate is null", wrapper.GlbCompanyCredentialICS2);

				var newCertificate = wrapper.GetGlbExternalPasswordOrCreateNew<GlbCompanyCredentialICS2>(PasswordTypesList.Codes.IC2);
				AssertEquals("Created ICS2 certificate is gettable", newCertificate, wrapper.GlbCompanyCredentialICS2);
			});
		}

		public void TestLoadAllGlbCompanyWrappers()
		{
			var companyWrappers = ObjectFactory.Get<Hashtable>("GlbCompanyWrappers");

			CombineAssertions(() =>
			{
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Argentina);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, "ASYCO");
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Belgium);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Switzerland);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.UnitedKingdom);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Ireland);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Israel);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Italy);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Japan);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.KoreaSouth);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Mexico);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Norway);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.UnitedStates);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Uruguay);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Taiwan);
				AssertGlbCompanyWrappersShouldContainCountry(companyWrappers, Core.Constants.CountryCodes.Australia);
			});
		}

		void AssertGlbCompanyWrappersShouldContainCountry(Hashtable companyWrappers, string countryCode)
		{
			var objectHandle = companyWrappers[countryCode] as ObjectHandle;
			AssertNotNull($"ObjectHandle for {countryCode} should not be null", objectHandle);
		}

		object ITestDynamicObjectHandleSupporter.GetObject()
		{
			throw new NotImplementedException();
		}

		object ITestDynamicObjectHandleSupporter.GetObject(object[] arguments) => new GlbCompanyWrapperForTesting((GlbCompany)arguments[0]);

		Type ITestDynamicObjectHandleSupporter.GetObjectType() => typeof(GlbCompanyWrapperForTesting);
	}

	public class GlbCompanyWrapperForTesting : GlbCompanyWrapper
	{
		public GlbCompanyWrapperForTesting(GlbCompany company)
			: base(company)
		{
		}

		public override bool IsValidWrapper => true;
	}
}

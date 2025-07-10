using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	public class CountrySpecificJobDeclarationFilterBusinessObjectFactoryTest : TestCase
	{
		public void TestCorrectObjectReturnedForCountry()
		{
			RefCountry currentCountry = GlbCompany.CurrentCompany.Country;
			try
			{
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.Canada, ObjectFactory.GetType<Integration.Customs.CA.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.Malaysia, ObjectFactory.GetType<Integration.Customs.MY.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.Singapore, ObjectFactory.GetType<Integration.Customs.SG.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.UnitedStates, ObjectFactory.GetType<Integration.Customs.US.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.PuertoRico, ObjectFactory.GetType<Integration.Customs.US.IJobDeclarationFilterBusinessObject>());
				AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.Iceland, typeof(JobDeclarationFilterBusinessObject)); // Iceland doesn't have its own customs module so far, so a generic type should be returned
				foreach (var country in ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
				{
					switch (country)
					{
						case Core.Constants.CountryCodes.France:
							AssertCorrectFilterTypeReturnedForCountry(Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IJobDeclarationFilterBusinessObject>());
							break;
						default:
							AssertCorrectFilterTypeReturnedForCountry(country, ObjectFactory.GetType<Integration.Customs.EU.IJobDeclarationFilterBusinessObject>());
							break;
					}
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry.Code;
			}
		}

		void AssertCorrectFilterTypeReturnedForCountry(string code, Type expectedType)
		{
			SetCurrentCountry(code);
			JobDeclarationFilterBusinessObject jobDecFilterObject = JobDecFactory.GetJobDeclarationFilterBusinessObjectForCountry();
			AssertEquals("Should be an " + code + " filter business object", expectedType, jobDecFilterObject.GetType());
		}

		void SetCurrentCountry(string code)
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = code;
		}

		JobDeclarationFilterBusinessObjectFactory JobDecFactory
		{
			get
			{
				return jobDecFactory ?? (jobDecFactory = new JobDeclarationFilterBusinessObjectFactory());
			}
		}
		JobDeclarationFilterBusinessObjectFactory jobDecFactory;
	}
}

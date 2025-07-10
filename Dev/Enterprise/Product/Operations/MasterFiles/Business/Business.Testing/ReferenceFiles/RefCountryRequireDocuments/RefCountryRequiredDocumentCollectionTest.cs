using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryRequiredDocumentCollection))]
	public class RefCountryRequiredDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefCountryRequiredDocumentCollection(Factory);
		}

		public void TestLoadCountry()
		{
			RefCountry country1 = Factory.New<RefCountry>();
			country1.Code = "UA";
			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc("AA", country1.Code);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(country1.Code, "BB");
			RefCountryRequiredDocument requiredDoc3 = GetNewRequiredDoc("BB", "AA");
			RefCountryRequiredDocument requiredDoc4 = GetNewRequiredDoc("", country1.Code);
			RefCountryRequiredDocument requiredDoc5 = GetNewRequiredDoc(country1.Code, "");
			RefCountryRequiredDocument requiredDoc6 = GetNewRequiredDoc("", "");
			RefCountryRequiredDocument requiredDoc7 = GetNewRequiredDoc(country1.Code, country1.Code);
			RefCountryRequiredDocument requiredDoc8 = GetNewRequiredDoc("", "AA");
			RefCountryRequiredDocument requiredDoc9 = GetNewRequiredDoc("BB", "");

			RefCountry country2 = Factory.New<RefCountry>();
			country2.Code = country1.Code;
			AssertCollectionContains(requiredDoc1, country2.RequiredDocuments);
			AssertCollectionContains(requiredDoc2, country2.RequiredDocuments);
			AssertCollectionNotContains(requiredDoc3, country2.RequiredDocuments);
			AssertCollectionContains(requiredDoc4, country2.RequiredDocuments);
			AssertCollectionContains(requiredDoc5, country2.RequiredDocuments);
			AssertCollectionContains(requiredDoc6, country2.RequiredDocuments);
			AssertCollectionContains(requiredDoc7, country2.RequiredDocuments);
			AssertCollectionContains(requiredDoc8, country2.RequiredDocuments);
			AssertCollectionContains(requiredDoc9, country2.RequiredDocuments);
		}

		public void CreateRequiredDocumentsForAustraliaAndOriginSingapore()
		{
			var countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			ZString aUS = Constants.CountryCodes.Australia;
			ZString sG = Constants.CountryCodes.Singapore;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.CartageAdvice, sG, aUS, JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ArrivalNotice, sG, aUS, JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All);
			RefCountryRequiredDocument requiredDoc3 = GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BillOfEntry, sG, aUS, JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All);

			countryAU.RequiredDocuments.Add(requiredDoc1);
			countryAU.RequiredDocuments.Add(requiredDoc2);
			countryAU.RequiredDocuments.Add(requiredDoc3);

			Factory.Save();
		}

		public void CreateRequiredDocumentsForUSAAndNoOrigin()
		{
			var countryUS = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates);
			ZString uSA = Constants.CountryCodes.UnitedStates;

			RefCountryRequiredDocument requiredDoc1 = GetNewRequiredDoc(countryUS, Constants.RefDocTypes.CartageAdvice, ZString.Empty, uSA, JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All);
			RefCountryRequiredDocument requiredDoc2 = GetNewRequiredDoc(countryUS, Constants.RefDocTypes.ArrivalNotice, ZString.Empty, uSA, JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All);
			RefCountryRequiredDocument requiredDoc3 = GetNewRequiredDoc(countryUS, Constants.RefDocTypes.BillOfEntry, ZString.Empty, uSA, JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All);

			countryUS.RequiredDocuments.Add(requiredDoc1);
			countryUS.RequiredDocuments.Add(requiredDoc2);
			countryUS.RequiredDocuments.Add(requiredDoc3);

			Factory.Save();
		}

		RefCountryRequiredDocument GetNewRequiredDoc(ZString orig, ZString dest)
		{
			RefCountryRequiredDocument result = Factory.New<RefCountryRequiredDocument>();
			result.RD_RN_NKDestination = orig;
			result.RD_RN_NKOrigin = dest;
			return result;
		}

		RefCountryRequiredDocument GetNewRequiredDoc(RefCountry country, ZString docType, ZString orig, ZString dest, ZString usage, ZString transport)
		{
			RefCountryRequiredDocument result = country.RequiredDocuments.AddNew();
			result.RD_DocType = docType;
			result.RD_RN_NKOrigin = orig;
			result.RD_RN_NKDestination = dest;
			result.RD_DocUsage = usage;
			result.RD_TransportMode = transport;
			result.RD_OnConsol = true;
			result.RD_OnShipment = true;
			result.RD_OnBrokerage = true;
			result.RD_OnOrder = true;
			return result;
		}

		public void TestCountryCode()
		{
			RefCountryRequiredDocumentCollection collection = new RefCountryRequiredDocumentCollection(Factory);
			AssertEquals("", collection.CountryCode);

			RefCountry country = Factory.New<RefCountry>();
			country.Code = "UA";
			collection.LoadCountry(country);
			AssertEquals("UA", collection.CountryCode);
		}
	}
}

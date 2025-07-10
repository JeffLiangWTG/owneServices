using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusCodeValidationUniqueTest : TestCaseWithFactory
	{
		public void TestUniqueOrgCusCodes_OK_OH_OK_RN_NKCodeCountry_OK_CodeType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNewMainAddress();
			var cusCode = org.CustomsCodes.AddNew();
			var orgCusCodeList = cusCode.Lookups.OK_CodeType_List.GetAllCodes();
			Factory.Save();
			var allCountries = new RefCountryCollection(Factory);
			var errorList = new Dictionary<string, HashSet<string>>();

			foreach (var country in allCountries)
			{
				var countryCode = country.RN_Code;
				var consumptionTaxRegistrationOrgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
				foreach (var orgCusCode in orgCusCodeList)
				{
					if ((!consumptionTaxRegistrationOrgCusCode.IsNullOrEmpty() && orgCusCode == consumptionTaxRegistrationOrgCusCode) || !new OrgCusCodePremiseAddressValidator().IsPremiseAddressAllowed(orgCusCode, countryCode))
					{
						var insertSQL = InitOrgCusCodeInsertCmd(orgCusCode, org.PK, ZGuid.Empty, countryCode);
						var insertSQL2 = InitOrgCusCodeInsertCmd(orgCusCode, org.PK, address.PK, countryCode);

						Db.Connection.ExecuteNonQuery(insertSQL);
						try
						{
							Db.Connection.ExecuteNonQuery(insertSQL2);
						}
						catch (SqlException e)
						{
							if (!e.Message.Contains("Cannot insert duplicate key row in object 'dbo.OrgCusCode' with unique index 'NR_UX__OK_OH_OK_RN_NKCodeCountry_OK_CodeType'."))
							{
								if (errorList.TryGetValue(orgCusCode, out var countries))
								{
									countries.Add(countryCode);
								}
								else
								{
									errorList.Add(orgCusCode, new HashSet<string> { countryCode });
								}
							}
						}
					}
				}
			}

			var res = new ZStringBuilder();
			foreach (var pair in errorList)
			{
				res.Append($"Code: {pair.Key}, Countries: {string.Join(",", pair.Value)}");
			}

			AssertMultilineASCIIEquals("All OrgCusCodes are already unique for all countries.", "", res.ToStringWithNewLineBetweenAppends());
		}

		public void TestOrgCusCodes_NotUnique_Dun()
		{
			AssertNoExceptionThrown(() =>
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var address = org.Addresses.AddNewMainAddress();
				Factory.Save();
				var allCountries = new RefCountryCollection(Factory);
				var orgCusCode = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;

				foreach (var country in allCountries)
				{
					var countryCode = country.RN_Code;
					var insertSQL = InitOrgCusCodeInsertCmd(orgCusCode, org.PK, ZGuid.Empty, countryCode);
					var insertSQL2 = InitOrgCusCodeInsertCmd(orgCusCode, org.PK, address.PK, countryCode);
					TestConnection.ExecuteNonQuery(insertSQL);
					TestConnection.ExecuteNonQuery(insertSQL2);
				}
			});
		}

		string InitOrgCusCodeInsertCmd(string orgCusCode, ZGuid orgPK, ZGuid addressPK, ZString countryCode)
		{
			var addrInsertPK = addressPK == ZGuid.Empty ? "null" : $"'{addressPK}'";
			return $@"INSERT INTO[dbo].[OrgCusCode] ([OK_PK],[OK_IsValid],[OK_CustomsRegNo],[OK_CodeType],[OK_OH],[OK_OA_PremisesAddress],[OK_RN_NKCodeCountry],[OK_CountryDefault])
				VALUES (newid(), 1, '12345', '{orgCusCode}', '{orgPK}', {addrInsertPK}, '{countryCode}', 0)";
		}
	}
}

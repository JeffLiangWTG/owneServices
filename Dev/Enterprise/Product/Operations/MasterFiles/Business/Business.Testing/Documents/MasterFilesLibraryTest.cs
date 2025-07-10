using System;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business.Macros;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using static Enterprise.Core.Constants;
using Address = Enterprise.MasterFiles.Business.Macros.Address;

namespace Enterprise.MasterFiles.Business.Documents.Testing
{
	sealed class MasterFilesLibraryTest : TestCaseWithFactory
	{
		#region AddressFormatter
		public void TestFormatAddress_OrgAddress()
		{
			var address = new Mock<IAddress>();
			address.Setup(m => m.CompanyName).Returns("Monsters Inc.");
			address.Setup(m => m.AddressLine1).Returns("address line 1");
			address.Setup(m => m.AddressLine2).Returns("address line 2");
			address.Setup(m => m.City).Returns("Scareville");
			address.Setup(m => m.State).Returns("ZZZ");
			address.Setup(m => m.Postcode).Returns("12345");
			var country = new Mock<ICountry>();
			country.Setup(m => m.Name).Returns("Australia");
			address.Setup(m => m.Country).Returns(country.Object);

			const string macro = "AddressFormatter.Format(@data)";

			using var scope = new MacroScope(address.Object);
			var context = new IMacroLibrary[]
				{
					new MasterFilesLibrary(Factory)
				}
				.CreateContext();

			var expr = macro.With(context).CreateExpression();
			var result = Convert.ToString(expr.Evaluate(scope), CultureInfo.InvariantCulture);

			AssertMultilineASCIIEquals("expression has no errors", "", expr.ToFormatString());

			AssertEquals("formatted address",
				@"MONSTERS INC.
ADDRESS LINE 1
ADDRESS LINE 2
SCAREVILLE ZZZ 12345
AUSTRALIA", result);
		}

		public void TestFormatAddress_IOrganizationAddress()
		{
			var address = new Mock<IOrganizationAddress>();
			var country = new Mock<ICodeNameDataObject>();

			address.Setup(m => m.CompanyName).Returns("Monsters Inc.");
			address.Setup(m => m.Address1).Returns("address line 1");
			address.Setup(m => m.Address2).Returns("address line 2");
			address.Setup(m => m.City).Returns("Scareville");
			address.Setup(m => m.State).Returns("ZZZ");
			address.Setup(m => m.Postcode).Returns("12345");

			country.Setup(m => m.Name).Returns("Australia");
			address.Setup(m => m.Country).Returns(country.Object);
			const string macro = "AddressFormatter.Format(@data)";

			using var scope = new MacroScope(address.Object);
			var context = new IMacroLibrary[]
				{
					new MasterFilesLibrary(Factory)
				}
				.CreateContext();

			var expr = macro.With(context).CreateExpression();
			var result = Convert.ToString(expr.Evaluate(scope), CultureInfo.InvariantCulture);

			AssertMultilineASCIIEquals("expression has no errors", "", expr.ToFormatString());

			AssertEquals("formatted address",
				@"MONSTERS INC.
ADDRESS LINE 1
ADDRESS LINE 2
SCAREVILLE ZZZ 12345
AUSTRALIA", result);

			address.Verify(m => m.CompanyName, Times.Once);
			address.Verify(m => m.Address1, Times.Once);
			address.Verify(m => m.Address2, Times.Once);
			address.Verify(m => m.City, Times.Once);
			address.Verify(m => m.State, Times.Once);
			address.Verify(m => m.Postcode, Times.Once);

			country.Verify(m => m.Name, Times.Once);
			address.Verify(m => m.Country, Times.Once);
		}

		public void TestFormatAddress_Company()
		{
			var company = new Company(GlbCompany.CurrentCompany);

			const string macro = "AddressFormatter.Format(@data)";

			using var scope = new MacroScope(company);
			var context = new IMacroLibrary[] { new MasterFilesLibrary(Factory) }
				.CreateContext();

			var expr = macro.With(context).CreateExpression();
			var result = Convert.ToString(expr.Evaluate(scope), CultureInfo.InvariantCulture);

			AssertMultilineASCIIEquals("expression has no errors", "", expr.ToFormatString());

			AssertEquals("formatted address",
				@"EDI CUSTOMS BROKERS
10 HUTCHESON STREET
ALBION QLD
BRISBANE 4010
AUSTRALIA", result);
		}

		public void TestFormatAddress_NullAddress()
		{
			AssertFormatUnsupportedAddressObject(null);
		}

		public void TestFormatAddress_Unsupported()
		{
			AssertFormatUnsupportedAddressObject(new object());
		}

		void AssertFormatUnsupportedAddressObject(object data)
		{
			const string macro = "AddressFormatter.Format(@data)";

			using (var scope = new MacroScope(data))
			{
				var context = new IMacroLibrary[]
					{
						new MasterFilesLibrary(Factory)
					}
					.CreateContext();

				var expr = macro.With(context).CreateExpression();
				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expression has no errors", "", expr.ToFormatString());
				AssertMultilineASCIIEquals("address", "", Convert.ToString(result, CultureInfo.InvariantCulture));
			}
		}

		#endregion

		#region Tax Numbers

		public void TestGetTaxNumber()
		{
			TestGetTaxNumber("GetTaxNumber(@data.Item1, @data.Item2, @data.Item3)");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Indonesia))
			{
				TestGetTaxNumber("GetTaxNumber(@data.Item1, @data.Item2)");
			}
		}

		public void TestGetTaxNumber(string macro)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var taxNumberPAS = Factory.New<OrgCusCode>();
			taxNumberPAS.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			taxNumberPAS.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			taxNumberPAS.OK_CustomsRegNo = "67890";
			var taxNumberPPN = Factory.New<OrgCusCode>();
			taxNumberPPN.OK_CodeType = OrgCusCode.IndonesiaCodeTypes.PPN;
			taxNumberPPN.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			taxNumberPPN.OK_CustomsRegNo = "12345";
			var context = new IMacroLibrary[] { new MasterFilesLibrary(Factory) }.CreateContext();
			var expr = macro.With(context).CreateExpression();

			org.CustomsCodes.Add(taxNumberPAS);
			var address = new Address(org.MainAddress);
			var data = new Tuple<IAddress, string, string>(address, "ShippingInstruction", CountryCodes.Indonesia);
			var result = expr.Evaluate(data) as Macros.RegistrationNumber;
			CombineAssertions("Should have no parsing errors", delegate
			{
				AssertMultilineASCIIEquals("Result should return the PAS tax number", "", expr.ToFormatString());
				AssertEquals("67890", result.Value);
				AssertEquals(CountryCodes.Indonesia, result.CountryOfIssue.Code);
				AssertEquals(OrgCusCode.CodeTypes.PassportID, result.Type.Code);
			});

			org.CustomsCodes.Add(taxNumberPPN);
			address = new Address(org.MainAddress);
			data = new Tuple<IAddress, string, string>(address, "ShippingInstruction", CountryCodes.Indonesia);
			result = expr.Evaluate(data) as Macros.RegistrationNumber;
			CombineAssertions("Should have no parsing errors", delegate
			{
				AssertMultilineASCIIEquals("Result should return the PNN tax number", "", expr.ToFormatString());
				AssertEquals("12345", result.Value);
				AssertEquals(CountryCodes.Indonesia, result.CountryOfIssue.Code);
				AssertEquals(OrgCusCode.IndonesiaCodeTypes.PPN, result.Type.Code);
			});

			data = new Tuple<IAddress, string, string>(address, "General", CountryCodes.Indonesia);
			result = expr.Evaluate(data) as Macros.RegistrationNumber;
			CombineAssertions("Should have no parsing errors", delegate
			{
				AssertMultilineASCIIEquals("Result should return null", "", expr.ToFormatString());
				AssertNull(result);
			});

			data = new Tuple<IAddress, string, string>(address, "ErrorType", CountryCodes.Indonesia);
			result = expr.Evaluate(data) as Macros.RegistrationNumber;
			CombineAssertions("Should have parsing errors", delegate
			{
				AssertMultilineASCIIEquals("Value of 'ErrorType' is not valid. Valid values are General, AirwayBill, ShippingInstruction.", expr.ToFormatString());
			});
		}

		#endregion

		#region GetVesselType

		public void TestGetVesselType()
		{
			const string macro = "GetVesselType(@data)";
			var context = new IMacroLibrary[]
					{
						new MasterFilesLibrary(Factory)
					}
					.CreateContext();
			var vessel = GetTestVessel();
			var expr = macro.With(context).CreateExpression();

			AssertEquals("BLK", expr.Evaluate(vessel.RV_Code));
		}

		#endregion

		#region GetVesselCallSign

		public void TestGetVesselCallSign()
		{
			const string macro = "GetVesselCallSign(@data)";
			var context = new IMacroLibrary[]
					{
						new MasterFilesLibrary(Factory)
					}
					.CreateContext();
			var vessel = GetTestVessel();
			var expr = macro.With(context).CreateExpression();

			AssertEquals("BREAKER", expr.Evaluate(vessel.RV_Code));
		}

		#endregion

		#region GetTestVessel

		RefVessel GetTestVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "ROGERDODGER";
			vessel.RV_RadioCallSign = "BREAKER";
			vessel.RV_VesselType = "BLK";

			Factory.Save();

			return vessel;
		}

		#endregion

		#region AddInfoUSDateFormatter

		public void TestAddInfoUSDateFormatter()
		{
			ZString addInfoDate = "2016-06-29 00:00:00.000";

			const string macro = "AddInfoUSDateFormatter.Format(@data)";

			using (var scope = new MacroScope(addInfoDate))
			{
				var context = new IMacroLibrary[]
				{
					new MasterFilesLibrary(Factory)
				}
				.CreateContext();

				var expr = macro.With(context).CreateExpression();
				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expression has no errors", "", expr.ToFormatString());
				AssertMultilineASCIIEquals("US Date", "06-29-2016", Convert.ToString(result, CultureInfo.InvariantCulture));
			}
		}

		public void TestAddInfoUSDateFormatter_Unsupported()
		{
			AssertFormatUnsupportedAddressObject(new object());
		}

		#endregion

		#region USPhoneFormatter

		public void TestUSPhoneFormatter()
		{
			ZString phoneNumber = "+18005551212";

			const string macro = "USPhoneFormatter.Format(@data)";

			using (var scope = new MacroScope(phoneNumber))
			{
				var context = new IMacroLibrary[]
				{
					new MasterFilesLibrary(Factory)
				}
				.CreateContext();

				var expr = macro.With(context).CreateExpression();
				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expression has no errors", "", expr.ToFormatString());
				AssertMultilineASCIIEquals("US phone number", "(800) 555-1212", Convert.ToString(result, CultureInfo.InvariantCulture));
			}
		}

		public void TestUSPhoneFormatter_Unsupported()
		{
			AssertFormatUnsupportedAddressObject(new object());
		}

		#endregion

		#region GetRefSysConfigStringValue
		public void TestGetRefSysConfigStringValue()
		{
			const string macro = "GetRefSysConfigStringValue(@data)";
			var context = new IMacroLibrary[]
				{
					new MasterFilesLibrary(Factory)
				}
				.CreateContext();
			var date = ZDateTime.UtcToday;
			var configType = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfigType>();
			configType.ZRT_ConfigCode = "TESTC";
			configType.ZRT_Description = "TEST CASE";
			configType.ZRT_LongDescription = "TEST CASE";
			var config = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode;
			config.ZRC_DecimalValue = 0m;
			config.ZRC_StartDate = date.AddDays(-10);
			config.ZRC_EndDate = date.AddDays(10);
			config.ZRC_StringValue = "20210602";
			Factory.Save();

			var expr = macro.With(context).CreateExpression();

			AssertEquals("20210602", expr.Evaluate("TESTC"));
		}
		#endregion
	}
}

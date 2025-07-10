using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BulkDetentionHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDetentionType()
		{
			CombineAssertions(delegate
			{
				Header.DetentionType = DetentionInvoiceType.Codes.Import;
				AssertNoErrors("Valid value", Header.DetentionTypeInfo);
				Header.DetentionType = "XXX";
				AssertHasError("Invalid value", Header.DetentionTypeInfo, "Enter a valid Detention Type.");
				Header.DetentionType = "";
				AssertNoErrors("Emtpy is valid.", Header.DetentionTypeInfo);
				Header.Validation.ValidateDetentionType();
			});
		}

		public void TestClient()
		{
			CombineAssertions(delegate
			{
				OrgHeader client = Factory.New<OrgHeader>();
				Header.ClientPK = client.PK;
				AssertNoErrors("Valid value", Header.ClientPKInfo);
				Header.ClientPK = ZGuid.NewZGuid();
				AssertHasError("Invalid value", Header.ClientPKInfo, "Enter a valid Client.");
				Header.ClientPK = ZGuid.Empty;
				AssertNoErrors("Empty is valid", Header.ClientPKInfo);
			});
		}

		public void TestPrincipal()
		{
			CombineAssertions(delegate
			{
				OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
				principal.OH_IsShippingProvider = true;
				principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
				Factory.Save();
				Header.PrincipalPK = principal.PK;
				AssertNoErrors("Valid value", Header.PrincipalPKInfo);
				Header.PrincipalPK = ZGuid.NewZGuid();
				AssertHasError("Invalid value", Header.PrincipalPKInfo, "Enter a valid Principal.");
				Header.PrincipalPK = ZGuid.Empty;
				AssertNoErrors("Empty is valid", Header.PrincipalPKInfo);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(delegate
			{
				Header.CountryCode = "AU";
				AssertNoNotifications("Valid value", Header.CountryCodeInfo);
				Header.CountryCode = "XX";
				AssertHasError("Invalid value", Header.CountryCodeInfo, "Enter a valid Country.");
				Header.CountryCode = "";
				AssertNoNotifications("Empty is valid", Header.CountryCodeInfo);
			});
		}

		#region Implementation
		BulkDetentionHeader Header
		{
			get
			{
				return header ?? (header = new BulkDetentionHeader(Factory));
			}
		}

		BulkDetentionHeader header;
		#endregion
	}
}

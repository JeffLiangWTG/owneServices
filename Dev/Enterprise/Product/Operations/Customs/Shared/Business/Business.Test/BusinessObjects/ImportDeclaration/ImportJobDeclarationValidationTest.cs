using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class ImportJobDeclarationValidationTest : TestCaseWithFactory
	{
		public void TestCountryCodeValidation()
		{
			string countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("NZ");
				decToImport.Validation.ValidateCountryCode();
				AssertHasErrors("Select a value", decToImport.CountryCodeInfo);

				decToImport.CountryCode = "NZ";
				AssertHasErrors("Should use copy menu", decToImport.CountryCodeInfo);

				decToImport.CountryCode = "+1";
				AssertHasErrors("Invalid country", decToImport.CountryCodeInfo);

				decToImport.CountryCode = "+1";
				AssertHasErrors("Country doesn't have a module", decToImport.CountryCodeInfo);

				decToImport.CountryCode = "AU";
				AssertNoErrors("Country code", decToImport.CountryCodeInfo);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		public void TestDeclarationWithAttachedShipment()
		{
			GlbCompany.CurrentCompany.SetCountry("NZ");
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			decToImport.CountryCode = "AU";
			decToImport.DeclarationPK = declaration.PK;
			AssertHasErrorContaining(decToImport.DeclarationPKInfo, "This Declaration is attached to a shipment");
		}

		public void TestDeclarationPKValidation()
		{
			string countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("NZ");

				decToImport.CountryCode = "NZ";
				decToImport.Validation.ValidateDeclarationPK();
				AssertHasErrors("DeclarationPK", decToImport.DeclarationPKInfo);

				decToImport.DeclarationPK = ZGuid.Invalid;
				AssertHasErrors("DeclarationPK", decToImport.DeclarationPKInfo);

				decToImport.DeclarationPK = declaration.PK;
				AssertHasErrors("DeclarationPK", decToImport.DeclarationPKInfo);

				decToImport.CountryCode = "AU";
				decToImport.Validation.ValidateDeclarationPK();
				AssertNoErrors("DeclarationPK", decToImport.DeclarationPKInfo);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		#region Implementation

		ImportJobDeclaration decToImport;
		ForwardingShipment shipment;
		BaseJobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.New<ForwardingShipment>();

			GlbCompany aUCompany = Factory.New<GlbCompany>();
			aUCompany.GC_RN_NKCountryCode = "AU";
			aUCompany.GC_Code = "AUC";
			GlbBranch aUBranch = aUCompany.Branches.AddNew();
			aUBranch.GB_Code = "AUB";
			aUBranch.GB_RL_NKHomePort = "AUSYD";

			GlbCompany nZCompany = Factory.New<GlbCompany>();
			nZCompany.GC_RN_NKCountryCode = "NZ";
			nZCompany.GC_Code = "NZC";
			GlbBranch nZBranch = nZCompany.Branches.AddNew();
			nZBranch.GB_Code = "NZB";
			nZBranch.GB_RL_NKHomePort = "NZAKL";

			declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_GB = aUBranch.PK;

			Factory.Save();

			decToImport = new ImportJobDeclaration(Factory);
		}

		#endregion

	}
}

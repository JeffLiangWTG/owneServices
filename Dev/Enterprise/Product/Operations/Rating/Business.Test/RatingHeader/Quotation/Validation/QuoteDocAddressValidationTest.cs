using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	public class QuoteDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestQuoteHeaderValidation()
		{
			var headerLocalClient = Factory.NewWithValidTestData<OrgHeader>();
			var quote = Helper.NewQuote(headerLocalClient);

			quote.TH_OneTimeQuote = true;
			quote.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.LocalClient;

			quote.RunPreSaveValidation();
			AssertHasError(quote.TH_OHInfo, ErrorMessages.InvalidClientRateHeader);

			headerLocalClient.OH_IsConsignee = true;
			Factory.Save();

			quote.RunPreSaveValidation();
			AssertNoError(quote.TH_OHInfo, ErrorMessages.InvalidClientRateHeader);

			var headerOverseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			var quoteOverseasAgent = Helper.NewQuote(headerOverseasAgent);
			quoteOverseasAgent.TH_OneTimeQuote = true;
			quoteOverseasAgent.CurrentOneOffQuote.TT_OrgRole = Core.Constants.OrgRoles.OverseasAgent;

			quoteOverseasAgent.RunPreSaveValidation();
			AssertHasError(quoteOverseasAgent.TH_OHInfo, ErrorMessages.InvalidOverseasAgentRateHeader);
		}

		public void TestOrganisationPKValidation_AddErrorIfSalesRepresentetiveIsNotActive()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var salesRepresentetive = Factory.NewWithValidTestData<GlbStaff>();

			var assignment = header.StaffAssignments.AddNew();
			assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment.O8_GS_NKPersonResponsible = salesRepresentetive.GS_Code;

			var quote = Helper.NewQuote(header);

			quote.TH_OneTimeQuote = true;

			salesRepresentetive.GS_IsActive = false;
			quote.RunPreSaveValidation();
			AssertHasError(quote.TH_OHInfo, ErrorMessages.FirstSignatoryIsInactive);

			salesRepresentetive.GS_IsActive = true;
			quote.RunPreSaveValidation();
			AssertNoError(quote.TH_OHInfo, ErrorMessages.FirstSignatoryIsInactive);

			quote.TH_OneTimeQuote = false;

			salesRepresentetive.GS_IsActive = false;
			quote.RunPreSaveValidation();
			AssertNoError(quote.TH_OHInfo, ErrorMessages.FirstSignatoryIsInactive);

			salesRepresentetive.GS_IsActive = true;
			quote.RunPreSaveValidation();
			AssertNoError(quote.TH_OHInfo, ErrorMessages.FirstSignatoryIsInactive);
		}

		#region Helper

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		TestHelper helper;

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using System;
	using Enterprise.MasterFiles.Business;
	using Registry;
	using static Environment.Env;

	public static class TestHelper
	{
		public static JobDeclaration CreateSendableImportDeclaration(BusinessObjectFactory factory)
		{
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestHelper.SetupMessagingEnvironment();

			TestCaseHelper.ClearTable(NZCCustomsExchangeRate.Schema.TableName);
			var exchangeRate = factory.New<NZCCustomsExchangeRate>();
			exchangeRate.U7_CurrencyCode = "USD";
			exchangeRate.U7_DateActiveTo = ZDateTime.UtcNow.AddDays(7);
			exchangeRate.U7_DateActiveFrom = exchangeRate.U7_DateActiveTo.AddDays(-60);
			exchangeRate.U7_Rate = 1.12m;

			var declarant = GlbStaff.CurrentUser;
			declarant.GS_EmailAddress = "test.user@company.org";
			declarant.GetNZWrapper().NZBPassword.GP_UserID = "40006206E";

			var decCreator = new TestFormalEntryCreator(factory.New<JobDeclaration>());
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");

			return decCreator.Declaration;
		}

		public static void SetupMessagingEnvironment()
		{
			Registry.SMTPServer = "10.0.0.1";
			Registry.SMTPPort = 25;
			Registry.MailServer = "10.0.0.1";
			Registry.MailServerPort = 995;
			Registry.MailboxUserName = "username";
			Registry.MailboxPassword = "password";
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
		}
	}
}

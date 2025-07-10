using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.FetchStrategies.Testing
{
	sealed class JobDeclarationFetchStrategyTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFetchHint_CS00332073()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			foreach (var tariff in File.ReadLines(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\Declaration\Base\JobDeclaration\TestFiles\Lots of NZ Tariffs.csv")))
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = tariff;
			}
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var decInDiffFactory = newFactory.Load<JobDeclaration>(dec.PK);
			decInDiffFactory.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			AssertNoExceptionThrown(() => decInDiffFactory.DoMerge());

			if (ErrorReporter.LastKeyReported == "ExceededAllowableNewObjectCount")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestFetchForLoad()
		{
			var jobDec = Factory.New<JobDeclaration>();
			int count = Factory.ActiveTableFetchHints;
			var strategy = new JobDeclarationFetchStrategy(jobDec);
			AssertEquals(0, Factory.ActiveFetchHintsForTable(JobDocAddressSchema.Constants.TableName));
			strategy.FetchForLoad();
			AssertEquals(1, Factory.ActiveFetchHintsForTable(JobDocAddressSchema.Constants.TableName));
		}

		public void TestFetchForValidateCore()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				Factory.Save();
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoice.JI_Tariff = "123456789";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var declarationInNewFactory = newFactory.Load<JobDeclaration>(declaration.PK);
				var strategy = new JobDeclarationFetchStrategy(declarationInNewFactory);
				strategy.FetchForValidate();
				newFactory.ExecuteAllFetchHints();
				AssertEquals("Fetch hint should be added", 1, newFactory.GetTableHitCount(TariffViewSchema.Constants.TableName));

				newFactory.Load<TariffView>(tariff.PK);
				AssertEquals("Should not have extra db hit", 1, newFactory.GetTableHitCount(TariffViewSchema.Constants.TableName));
			}
		}

		public void TestMergeFetchHint()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "LVY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "AL", rateType.PK);
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoice.JI_Tariff = "123456789";
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var declarationInNewFactory = newFactory.Load<JobDeclaration>(declaration.PK);
				var strategy = new JobDeclarationFetchStrategy(declarationInNewFactory);
				strategy.FetchForMerge();
				newFactory.ExecuteAllFetchHints();
				CombineAssertions("Fetch hint should be added", () =>
				{
					AssertEquals("Tariff table hit count", 1, newFactory.GetTableHitCount(TariffViewSchema.Constants.TableName));
					AssertEquals("Rate table hit count", 1, newFactory.GetTableHitCount(RateViewSchema.Constants.TableName));
				});

				CombineAssertions("Should not have extra db hit", () =>
				{
					newFactory.Load<TariffView>(tariff.PK);
					newFactory.Load<RateView>(rate.PK);
					AssertEquals("Tariff table hit count", 1, newFactory.GetTableHitCount(TariffViewSchema.Constants.TableName));
					AssertEquals("Rate table hit count", 1, newFactory.GetTableHitCount(RateViewSchema.Constants.TableName));
				});
			}
		}
	}
}

using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgSalesAndTradeDictionaryBuilderTest : TestCaseWithFactory
	{
		public void TestBuild_ShouldUseTableValuedParameters()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					var org = Factory.NewWithValidTestData<OrgHeader>();

					var actualSales1 = Factory.New<OrgSales>();
					actualSales1.OW_IsTraded = true;
					actualSales1.OW_OH_Buyer = org.PK;

					var actualSales2 = Factory.New<OrgSales>();
					actualSales2.OW_IsTraded = true;
					actualSales2.OW_OH_Supplier = org.PK;

					var prospectSales = Factory.New<OrgSales>();
					prospectSales.OW_IsTraded = false;
					prospectSales.OW_OH_Primary = org.PK;

					Factory.Save();

					var masterStat = new OrgSalesAndTradeDictionaryBuilder(Factory, org.SalesCollection);

					var executedCommand = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.StartsWith("SELECT \r\nPA_PK") && c.Contains("FROM dbo.OrgTradeDetail"));
					AssertNotNull(executedCommand);
					AssertContains("Should use TVPs", "(PA_OW in (SELECT Value FROM", executedCommand);
					Assert("Should not use explicit values", !Regex.IsMatch(executedCommand, @"\(PA_OW in \((@(.*?),)*?@(.*?)\)"));
				}
			}
		}

		public void TestBuildAppendUpdate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var actualSales1 = Factory.New<OrgSales>();
			actualSales1.OW_IsTraded = true;
			actualSales1.OW_OriginID = ausyd.PK;
			actualSales1.OW_OriginTableCode = "RL";
			actualSales1.OW_DestinationID = uslax.PK;
			actualSales1.OW_DestinationTableCode = "RL";
			actualSales1.OW_OH_Buyer = org.PK;
			var detail1 = actualSales1.TradeDetails.AddNew();
			detail1.PA_TradeMode = "AIR";
			detail1.PA_TradeType = "LSE";

			var actualSales2 = Factory.New<OrgSales>();
			actualSales2.OW_IsTraded = true;
			actualSales2.OW_OriginID = uslax.PK;
			actualSales2.OW_OriginTableCode = "RL";
			actualSales2.OW_DestinationID = nzakl.PK;
			actualSales2.OW_DestinationTableCode = "RL";
			actualSales2.OW_OH_Supplier = org.PK;
			var detail2a = actualSales2.TradeDetails.AddNew();
			detail2a.PA_TradeMode = "SEA";
			detail2a.PA_TradeType = "FCL";
			var detail2b = actualSales2.TradeDetails.AddNew();
			detail2b.PA_TradeMode = "SEA";
			detail2b.PA_TradeType = "LCL";

			var prospectSales = Factory.New<OrgSales>();
			prospectSales.OW_IsTraded = false;
			prospectSales.OW_OH_Primary = org.PK;

			Factory.Save();

			AssertEquals(3, org.SalesCollection.Count);

			// Build

			var masterStat = new OrgSalesAndTradeDictionaryBuilder(Factory, org.SalesCollection);

			AssertEquals(2, masterStat.TradeLanesMap.Count);
			AssertEquals(2, masterStat.TradeDetailsMap.Count);
			AssertEquals(1, masterStat.TradeDetailsMap[actualSales1.PK].Count);
			AssertEquals(2, masterStat.TradeDetailsMap[actualSales2.PK].Count);
			AssertEquals(3, masterStat.TradeDetails.Count);

			// Append

			var factory2 = new BusinessObjectFactory();
			var newSales = factory2.New<OrgSales>();
			newSales.OW_IsTraded = true;
			newSales.OW_OriginID = nzakl.PK;
			newSales.OW_OriginTableCode = "RL";
			newSales.OW_DestinationID = ausyd.PK;
			newSales.OW_DestinationTableCode = "RL";
			newSales.OW_OH_Supplier = org.PK;
			var newDetail = newSales.TradeDetails.AddNew();
			newDetail.PA_TradeMode = "SEA";
			newDetail.PA_TradeType = "FCL";

			masterStat.Append(newSales);
			AssertEquals(3, masterStat.TradeLanesMap.Count);
			AssertEquals(3, masterStat.TradeDetailsMap.Count);
			AssertEquals(1, masterStat.TradeDetailsMap[newSales.PK].Count);
			AssertEquals(4, masterStat.TradeDetails.Count);

			// Update

			factory2.Save();
			masterStat.Update();
			AssertEquals(3, masterStat.TradeLanesMap.Count);
			AssertEquals(3, masterStat.TradeDetailsMap.Count);
			AssertEquals(1, masterStat.TradeDetailsMap[actualSales1.PK].Count);
			AssertEquals(2, masterStat.TradeDetailsMap[actualSales2.PK].Count);
			AssertEquals(1, masterStat.TradeDetailsMap[newSales.PK].Count);
			AssertEquals(4, masterStat.TradeDetails.Count);

			AssertEquals("New sales has been loaed into master stat factory", Factory, masterStat.TradeLanesMap[TradeLinesValueHelper.CreateActualTradeLaneKey(newSales)].Factory);
			AssertEquals("New detail has been loaed into master stat factory", Factory, masterStat.TradeDetailsMap[newSales.PK][TradeLinesValueHelper.CreateTradeDetailKey(newDetail)].Factory);
		}

		[TestDate(2020, 06, 29, 7, 37, 1)]
		public void TestDeleteDuplicates()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			var actualSales1 = Factory.New<OrgSales>();
			actualSales1.OW_IsTraded = true;
			actualSales1.OW_OriginID = ausyd.PK;
			actualSales1.OW_OriginTableCode = "RL";
			actualSales1.OW_DestinationID = uslax.PK;
			actualSales1.OW_DestinationTableCode = "RL";
			actualSales1.OW_OH_Buyer = org.PK;

			var detail1 = actualSales1.TradeDetails.AddNew();
			detail1.PA_TradeMode = "AIR";
			detail1.PA_TradeType = "LSE";

			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			var actualSales11 = Factory.New<OrgSales>();
			actualSales11.OW_IsTraded = true;
			actualSales11.OW_OriginID = ausyd.PK;
			actualSales11.OW_OriginTableCode = "RL";
			actualSales11.OW_DestinationID = uslax.PK;
			actualSales11.OW_DestinationTableCode = "RL";
			actualSales11.OW_OH_Buyer = org.PK;

			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			var actualSales2 = Factory.New<OrgSales>();
			actualSales2.OW_IsTraded = true;
			actualSales2.OW_OriginID = uslax.PK;
			actualSales2.OW_OriginTableCode = "RL";
			actualSales2.OW_DestinationID = ausyd.PK;
			actualSales2.OW_DestinationTableCode = "RL";
			actualSales2.OW_OH_Supplier = org.PK;

			var detail2a = actualSales2.TradeDetails.AddNew();
			detail2a.PA_TradeMode = "SEA";
			detail2a.PA_TradeType = "FCL";

			var detail2aa = actualSales2.TradeDetails.AddNew();
			detail2aa.PA_TradeMode = "SEA";
			detail2aa.PA_TradeType = "FCL";

			Factory.Save();

			org.SalesCollection.Sort(OrgSalesSchema.Constants.OW_SystemCreateTimeUtc);
			var masterStat = new OrgSalesAndTradeDictionaryBuilder(Factory, org.SalesCollection);
			masterStat.DeleteDuplicates();

			Factory.Save();

			var loadedOrg = new BusinessObjectFactory().Load<OrgHeader>(org.PK);
			loadedOrg.SalesCollection.Sort(OrgSalesSchema.Constants.OW_SystemCreateTimeUtc);

			AssertEquals(2, loadedOrg.SalesCollection.Count);
			AssertEquals(actualSales1.PK, loadedOrg.SalesCollection[0].PK);
			AssertEquals(1, loadedOrg.SalesCollection[0].TradeDetails.Count);
			AssertEquals(actualSales2.PK, loadedOrg.SalesCollection[1].PK);
			AssertEquals(1, loadedOrg.SalesCollection[1].TradeDetails.Count);
		}
	}
}

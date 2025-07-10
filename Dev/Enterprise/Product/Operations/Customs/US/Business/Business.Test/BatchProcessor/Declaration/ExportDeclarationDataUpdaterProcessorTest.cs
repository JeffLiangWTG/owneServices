using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.US;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExportDeclarationDataUpdaterProcessorTest : TestCaseWithFactory
	{
		[TestDate(2021, 07, 27)]
		public void TestOnlyCompareDateAnd2CharInFlyghtNo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				declaration.JE_VoyageFlightNo = "93123";
				declaration.US_DateOfExport = ZDateTime.Now.AddHours(1);

				var transport = consol.MostInterestingTransportForBinding[0];
				transport.JW_VoyageFlight = "93124";
				transport.JW_Vessel = "BOSTON EXPRESS";
				transport.JW_ETD = ZDateTime.Now;
				transport.JW_ETA = ZDateTime.Now;
				transport.CarrierPK = carrier.PK;

				Factory.Save();

				var processor = new ExportDeclarationDataUpdaterProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var logs = new ZStringBuilder();
				var enumerator = processor.Logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				var newFactory = new BusinessObjectFactory();
				var declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);
				var consolLoaded = newFactory.Load<ForwardingConsol>(consol.PK);
				AssertNotNull(declarationLoaded);

				var logText = System.FormattableString.Invariant($@"System is synchronizing data from consol for Declaration {declaration.JE_DeclarationReference}.
JE_MasterBill has been updated from {declaration.JE_MasterBill} to {consol.JK_MasterBillNum}.
JE_RL_NKPortOfLoading has been updated from {declaration.JE_RL_NKPortOfLoading} to {consol.JK_RL_NKLoadPort}.
US_RL_NKPortOfExport has been updated from {declaration.US_RL_NKPortOfExport} to {consol.JK_RL_NKLoadForExportTransport}.
JE_OH_ShippingLine has been updated from {shipping.PK} to {carrier.PK}.
Synchronise data from consol for Declaration {declaration.JE_DeclarationReference} has completed and saved successfully.");
				AssertContains(logText, logs.ToStringWithDelimiterBetweenAppends("\r\n"));

				AssertEquals("JE_EntryStatus should be updated to CH_EntryStatus", entry.CH_EntryStatus, declaration.JE_EntryStatus);
				AssertEquals("JE_MessageStatus should be updated to CH_Status", entry.CH_Status, declaration.JE_MessageStatus);
				AssertEquals("CH_Status should be updated to RSR", AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired, declarationLoaded.ActiveEntryHeaders[0].CH_Status);
				AssertEquals("US_ShouldBeReportToCustoms should be updated to true", true, declarationLoaded.ActiveEntryHeaders[0].US_ShouldBeReportToCustoms);
				var maxLastEditTime = consolLoaded.JK_SystemLastEditTimeUtc > declarationLoaded.JE_SystemLastEditTimeUtc ? consolLoaded.JK_SystemLastEditTimeUtc : declarationLoaded.JE_SystemLastEditTimeUtc;
				var exportDeclarationDataUpdateDate = declarationLoaded.GetSystemDefinedValue<ZDateTime>("ExportDeclarationDataUpdateDate");
				AssertEquals(exportDeclarationDataUpdateDate, maxLastEditTime);
			}
		}

		public void TestCESIsAddedWithReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				declaration.JE_VoyageFlightNo = "QF123";
				declaration.US_DateOfExport = ZDateTime.Now;

				var transport = consol.MostInterestingTransportForBinding[0];
				transport.JW_VoyageFlight = "VA124";
				transport.JW_Vessel = "BOSTON EXPRESS";
				transport.JW_ETD = ZDateTime.Now.AddDays(-1);
				transport.JW_ETA = ZDateTime.Now;
				transport.CarrierPK = carrier.PK;

				Factory.Save();

				var processor = new ExportDeclarationDataUpdaterProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var newFactory = new BusinessObjectFactory();
				var declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);
				var cesLog = declarationLoaded.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				CombineAssertions(() =>
				{
					var logs = cesLog.SL_Reference.Split("|");
					AssertEquals("Count of Logs", 7, logs.Length);
					AssertEquals("CusEntryStatus", "RSR", logs[0]);
					AssertEquals("Carrier", "Carrier=SHI->CRR", logs[1]);
					AssertStartsWith("DateOfExport", "DateOfExport=", logs[2]);
					AssertEquals("FlightNo", "FlightNo=QF123->VA124", logs[3]);
					AssertEquals("MasterBill", "MasterBill=M000001->M000002", logs[4]);
					AssertEquals("PortOfExport", "PortOfExport=USLAX->USPHL", logs[5]);
					AssertEquals("PortOfLoading", "PortOfLoading=USLAX->USPHL", logs[6]);
				});
			}
		}

		[TestDate(2021, 07, 27)]
		public void TestProcess()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				declaration.JE_VoyageFlightNo = "93";
				declaration.US_DateOfExport = ZDateTime.Today.AddDays(-1);

				var transport = consol.MostInterestingTransportForBinding[0];
				transport.JW_VoyageFlight = "94";
				transport.JW_Vessel = "BOSTON EXPRESS";
				transport.JW_ETD = ZDateTime.Today.AddDays(1);
				transport.JW_ETA = ZDateTime.Today.AddDays(10);
				transport.CarrierPK = carrier.PK;

				var exportTransport = consol.Transports.ExportTransport;
				exportTransport.JW_ETD = ZDateTime.Today.AddDays(11);
				exportTransport.JW_ETA = ZDateTime.Today.AddDays(15);

				Factory.Save();

				var processor = new ExportDeclarationDataUpdaterProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var logs = new ZStringBuilder();
				var enumerator = processor.Logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);
				var consolLoaded = newFactory.Load<ForwardingConsol>(consol.PK);
				AssertNotNull(declarationLoaded);

				var logText = System.FormattableString.Invariant($@"System is synchronizing data from consol for Declaration {declaration.JE_DeclarationReference}.
JE_MasterBill has been updated from {declaration.JE_MasterBill} to {consol.JK_MasterBillNum}.
JE_RL_NKPortOfLoading has been updated from {declaration.JE_RL_NKPortOfLoading} to {consol.JK_RL_NKLoadPort}.
US_RL_NKPortOfExport has been updated from {declaration.US_RL_NKPortOfExport} to {consol.JK_RL_NKLoadForExportTransport}.
US_DateOfExport has been updated from {declaration.US_DateOfExport} to {exportTransport.JW_ETD}.
JE_VoyageFlightNo has been updated from {declaration.JE_VoyageFlightNo} to {transport.JW_VoyageFlight}.
JE_OH_ShippingLine has been updated from {shipping.PK} to {carrier.PK}.
Synchronise data from consol for Declaration {declaration.JE_DeclarationReference} has completed and saved successfully.");
				AssertContains(logText, logs.ToStringWithDelimiterBetweenAppends("\r\n"));

				AssertEquals("JE_EntryStatus should be updated to CH_EntryStatus", entry.CH_EntryStatus, declaration.JE_EntryStatus);
				AssertEquals("JE_MessageStatus should be updated to CH_Status", entry.CH_Status, declaration.JE_MessageStatus);
				AssertEquals("CH_Status should be updated to RSR", AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired, declarationLoaded.ActiveEntryHeaders[0].CH_Status);
				AssertEquals("US_ShouldBeReportToCustoms should be updated to true", true, declarationLoaded.ActiveEntryHeaders[0].US_ShouldBeReportToCustoms);

				var maxLastEditTime = consolLoaded.JK_SystemLastEditTimeUtc > declarationLoaded.JE_SystemLastEditTimeUtc ? consolLoaded.JK_SystemLastEditTimeUtc : declarationLoaded.JE_SystemLastEditTimeUtc;
				var exportDeclarationDataUpdateDate = declarationLoaded.GetSystemDefinedValue<ZDateTime>("ExportDeclarationDataUpdateDate");
				AssertEquals(exportDeclarationDataUpdateDate, maxLastEditTime);
			}
		}

		[TestDate(2021, 07, 27)]
		public void TestNothingProcessWhenJW_ETDDoesNotMatch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				declaration.JE_VoyageFlightNo = "93";
				declaration.US_DateOfExport = ZDateTime.Today.AddDays(-1);

				var transport = consol.MostInterestingTransportForBinding[0];
				transport.JW_VoyageFlight = "94";
				transport.JW_Vessel = "BOSTON EXPRESS";
				transport.JW_ETD = ZDateTime.Today.AddDays(1);
				transport.JW_ETA = ZDateTime.Today.AddDays(10);
				transport.CarrierPK = carrier.PK;

				var exportTransport = consol.Transports.ExportTransport;
				exportTransport.JW_ETD = new ZDateTime(2001, 01, 01);
				exportTransport.JW_ETA = ZDateTime.Today.AddDays(15);

				Factory.Save();

				var processor = new ExportDeclarationDataUpdaterProcessor(new LoggingInformation());
				processor.ExecuteBatch();

				var logs = new ZStringBuilder();
				var enumerator = processor.Logger.UserLogStrings.GetEnumerator();
				while (enumerator.MoveNext())
				{
					logs.Append(enumerator.Current.Trim());
				}

				Factory.Save();

				AssertEquals(string.Empty, logs.ToStringWithDelimiterBetweenAppends("\r\n"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			shipping = Factory.New<OrgHeader>();
			shipping.OH_Code = "SHI";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_IsCancelled = false;

			declaration.JE_MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_MasterBill = "M000001";
			declaration.JE_OH_ShippingLine = shipping.PK;
			declaration.US_RL_NKPortOfExport = "USLAX";

			entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_EntryStatus = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryType = "ZZZ";
			entryNum.CE_EntryNum = "CE000001";
			entryNum.CE_ParentID = entry.PK;
			entryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.ITN;
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_UniqueConsignRef = "C000001";
			consol.JK_RL_NKLoadPort = "USPHL";
			consol.JK_RL_NKDischargePort = "CAMTL";
			consol.JK_MasterBillNum = "M000002";

			shipment.Consols.Add(consol);

			carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CRR";
		}
		JobDeclaration declaration;
		OrgHeader shipping;
		CusEntryHeader entry;
		ForwardingConsol consol;
		OrgHeader carrier;
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ReportTesting;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.SARSEDIMessage;

namespace Enterprise.Customs.ZA.Business.Testing.ReportTesting
{
	[TestDate(2023, 8, 11)]
	sealed class RCGCargoExceptionsInManifestsReport : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "RCGCargoExceptionsInManifestsReport";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "AMA_JobReference", "AMA_CustomsOffice", "AMA_Nature", "AMA_AgentType", "AMA_TransportMode", "AMA_ContainerMode", "AMA_ManifestType", "AMA_Voyage",
					"MasterBillNo", "PortOfDischarge", "PortOfLoading", "HouseBillNo", "ABL_ShipmentType", "LastCustomsStatus", "LastMessageSentNum" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}
				foreach (var dateCol in new string[] { "EstArrivalDate", "EstDepartureDate", "LastMessageSentDate", "LastMessageReceivedDate" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(DateTime), dateCol));
				}
				return allCols;
			}
		}

		protected override List<string> ParametersValuesList => new List<string>()
		{
			$"'{GlbCompany.CurrentCompany.PK}'" // @CompanyPK
		};

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		int manifestCount;
		int messageCount;

		AsycudaManifestHeader CreateManifest(ZString manifestType)
		{
			manifestCount++;

			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.AMA_Nature = NatureList.Codes.Import23;
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			manifest.AMA_ManifestType = manifestType;
			manifest.AMA_AgentType = Core.Constants.AgentType.Agent;
			manifest.AMA_JobReference = $"MAN000000{manifestCount}";
			manifest.AMA_CustomsOffice = "BFN";
			manifest.AMA_Voyage = "V 1234";

			var masterBill = manifest.MasterBill;
			masterBill.ABL_BillNumber = $"MB-000{manifestCount}";
			masterBill.ABL_E_DEP = ZDateTime.Now.AddDays(-7);
			masterBill.ABL_RL_NKPortOfLoading = "CNSHA";
			masterBill.ABL_E_ARV = ZDateTime.Now.AddDays(7);
			masterBill.ABL_RL_NKPortOfDischarge = "ZADUR";

			var houseBill = manifest.Bills.AddNew();
			houseBill.ABL_BolType = AsycudaBill.HouseBillCode;
			houseBill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			houseBill.ABL_BillNumber = $"HB-000{manifestCount}";
			houseBill.ABL_BillStatus = "8";

			return manifest;
		}

		void AddEDIMessage(Messaging.Business.EDIMessageCollection messages)
		{
			var ediMessage1 = messages.AddNew();
			ediMessage1.EM_MessageType = MessageTypes.CUSCAR;
			ediMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage1.EM_MessageNum = $"000{++messageCount}";
			ediMessage1.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			ediMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);

			var ediMessage2 = messages.AddNew();
			ediMessage2.EM_MessageType = MessageTypes.CUSCAR;
			ediMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage2.EM_MessageNum = $"000{++messageCount}";
			ediMessage2.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			ediMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			var ediMessage3 = messages.AddNew();
			ediMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage3.EM_MessageType = MessageTypes.CUSRES;
			ediMessage2.EM_MessageNum = $"000{++messageCount}";
			ediMessage3.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			ediMessage3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
		}

		protected override void PrepareTestData()
		{
			var manifest1 = CreateManifest(nameof(ManifestDocumentType.RFM));
			AddEDIMessage(manifest1.Messages);
			var manifest2 = CreateManifest(ManifestTypeList.Codes.BulkBreakBulkOutturnReport);
			AddEDIMessage(new Messaging.Business.EDIMessageCollection(manifest2.Bills[0]));
			var manifest3 = CreateManifest(ManifestTypeList.Codes.DepotOutturnReport);
			AddEDIMessage(manifest3.Messages);
			Factory.Save();
		}

		protected override void AssertTestResults(DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, results.Rows.Count);

				var row1 = FormatRowsValues(results.Rows[0], results, true);
				AssertMultilineASCIIEquals("Row 1", "[AMA_JobReference]='MAN0000001'; [AMA_CustomsOffice]='BFN'; [AMA_Nature]='IMP'; [AMA_AgentType]='AGT'; " +
					"[AMA_TransportMode]='SEA'; [AMA_ContainerMode]='CNT'; [AMA_ManifestType]='RFM'; [AMA_Voyage]='V 1234'; [MasterBillNo]='MB-0001'; " +
					"[EstArrivalDate]='2023-08-18T00:00:00'; [EstDepartureDate]='2023-08-04T00:00:00'; [PortOfDischarge]='ZADUR'; [PortOfLoading]='CNSHA'; " +
					"[HouseBillNo]='HB-0001'; [ABL_ShipmentType]='IMP'; [LastCustomsStatus]='8'; [LastMessageSentDate]='2023-08-09T00:00:00'; [LastMessageSentNum]='0003'; " +
					"[LastMessageReceivedDate]='2023-08-10T00:00:00'", row1);

				var row2 = FormatRowsValues(results.Rows[1], results, true);
				AssertMultilineASCIIEquals("Row 2", "[AMA_JobReference]='MAN0000002'; [AMA_CustomsOffice]='BFN'; [AMA_Nature]='IMP'; [AMA_AgentType]='AGT'; " +
					"[AMA_TransportMode]='SEA'; [AMA_ContainerMode]='CNT'; [AMA_ManifestType]='BBB'; [AMA_Voyage]='V 1234'; [MasterBillNo]='MB-0002'; " +
					"[EstArrivalDate]='2023-08-18T00:00:00'; [EstDepartureDate]='2023-08-04T00:00:00'; [PortOfDischarge]='ZADUR'; [PortOfLoading]='CNSHA'; " +
					"[HouseBillNo]='HB-0002'; [ABL_ShipmentType]='IMP'; [LastCustomsStatus]='8'; [LastMessageSentDate]='2023-08-09T00:00:00'; [LastMessageSentNum]='0006'; " +
					"[LastMessageReceivedDate]='2023-08-10T00:00:00'", row2);

				var row3 = FormatRowsValues(results.Rows[2], results, true);
				AssertMultilineASCIIEquals("Row 3", "[AMA_JobReference]='MAN0000003'; [AMA_CustomsOffice]='BFN'; [AMA_Nature]='IMP'; [AMA_AgentType]='AGT'; " +
					"[AMA_TransportMode]='SEA'; [AMA_ContainerMode]='CNT'; [AMA_ManifestType]='DOR'; [AMA_Voyage]='V 1234'; [MasterBillNo]='MB-0003'; " +
					"[EstArrivalDate]='2023-08-18T00:00:00'; [EstDepartureDate]='2023-08-04T00:00:00'; [PortOfDischarge]='ZADUR'; [PortOfLoading]='CNSHA'; " +
					"[HouseBillNo]='HB-0003'; [ABL_ShipmentType]='IMP'; [LastCustomsStatus]='8'; [LastMessageSentDate]=''; [LastMessageSentNum]=''; " +
					"[LastMessageReceivedDate]=''", row3);
			});
		}
	}
}

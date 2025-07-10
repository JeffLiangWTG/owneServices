using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class ZAManifestRCGCargoReportDeviationTest : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "Report_RCGCargoDeviation";

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected override void PrepareTestData()
		{
			var asheader = Factory.New<AsycudaManifestHeader>();
			asheader.AMA_JobReference = "C00001184";
			asheader.AMA_Nature = "IMP";
			asheader.AMA_SystemCreateTimeUtc = new ZDateTime(2023, 01, 01);
			var masterBill = asheader.MasterBill;
			masterBill.ABL_BillNumber = "17612345675";
			masterBill.ABL_AMA = asheader.PK;
			masterBill.ABL_RL_NKPortOfDischarge = "ZADUR";
			masterBill.ABL_RL_NKPortOfLoading = "GBFLX";
			var bills = asheader.Bills.AddNew();
			bills.ABL_BillIssuer = "00838462";
			bills.ABL_BillNumber = "S00001529";
			bills.ABL_AMA = asheader.PK;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			dec.JE_HouseBill = "S00001529";

			var asheader1 = Factory.New<AsycudaManifestHeader>();
			asheader1.AMA_JobReference = "C00001185";
			asheader1.AMA_Nature = "IMP";
			asheader1.AMA_SystemCreateTimeUtc = new ZDateTime(2023, 01, 01);
			var masterBill1 = asheader1.MasterBill;
			masterBill1.ABL_AMA = asheader1.PK;
			masterBill1.ABL_BillNumber = "17612345675";
			masterBill1.ABL_RL_NKPortOfDischarge = "ZACPT";
			masterBill1.ABL_RL_NKPortOfLoading = "AUSYD";
			var bills1 = asheader1.Bills.AddNew();
			bills1.ABL_BillIssuer = "00838463";
			bills1.ABL_BillNumber = "S00001530";
			bills1.ABL_AMA = asheader1.PK;
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec1.JE_HouseBill = "S00001531";

			var asheader2 = Factory.New<AsycudaManifestHeader>();
			asheader2.AMA_JobReference = "C00001186";
			asheader2.AMA_Nature = "IMP";
			asheader2.AMA_Voyage = "EK1457";
			asheader2.AMA_SystemCreateTimeUtc = new ZDateTime(2023, 01, 01);
			var masterBill2 = asheader2.MasterBill;
			masterBill2.ABL_AMA = asheader2.PK;
			masterBill2.ABL_BillNumber = "17612345675";
			masterBill2.ABL_RL_NKPortOfDischarge = "ZAPIT";
			masterBill2.ABL_RL_NKPortOfLoading = "CNSHA";
			var bills2 = asheader2.Bills.AddNew();
			bills2.ABL_BillIssuer = "00838463";
			bills2.ABL_BillNumber = "S00001533";
			bills2.ABL_AMA = asheader2.PK;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec2.JE_TransportMode = Core.Constants.TransportModes.Rail;
			dec2.JE_DeclarationReference = "J0000001";
			dec2.JE_HouseBill = "S00001533";
			dec2.JE_MasterBill = "17612345675";
			dec2.JE_VoyageFlightNo = "EK1457";
			dec2.JE_CargoCarrier = "00838469";

			var asheader3 = Factory.New<AsycudaManifestHeader>();
			asheader3.AMA_JobReference = "C00001187";
			asheader3.AMA_Nature = "IMP";
			asheader3.AMA_Voyage = "EK1457";
			asheader3.AMA_SystemCreateTimeUtc = new ZDateTime(2023, 01, 01);
			var masterBill3 = asheader3.MasterBill;
			masterBill3.ABL_AMA = asheader3.PK;
			masterBill3.ABL_BillNumber = "17612345675";
			masterBill3.ABL_RL_NKPortOfDischarge = "ZAWAT";
			masterBill3.ABL_RL_NKPortOfLoading = "FRCAL";
			var bills3 = asheader3.Bills.AddNew();
			bills3.ABL_BillIssuer = "00838465";
			bills3.ABL_BillNumber = "S00001535";
			bills3.ABL_AMA = asheader3.PK;
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec3.JE_TransportMode = Core.Constants.TransportModes.Rail;
			dec3.JE_DeclarationReference = "J0000002";
			dec3.JE_HouseBill = "S00001535";
			dec3.JE_MasterBill = "17612345676";
			dec3.JE_VoyageFlightNo = "EK1458";
			dec3.JE_CargoCarrier = "00838465";
		}

		protected override void AssertTestResults(DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(4, results.Rows.Count);
				var row1 = FormatRowsValues(results.Rows[0], results, true);
				var row2 = FormatRowsValues(results.Rows[1], results, true);
				var row3 = FormatRowsValues(results.Rows[2], results, true);
				var row4 = FormatRowsValues(results.Rows[3], results, true);

				AssertMultilineASCIIEquals("Row1 - All Matched", $"[ManifestJobReference]='C00001184'; [ManifestCargoCarrierCode]='00838462'; [ManifestHouseBill]='S00001529'; [ManifestMasterBill]='17612345675'; [ManifestVoyage]=''; [ManifestDirection]='IMP'; [ManifestTransportMode]=''; [DeclarationReference]='B00001000'; [DeclarationCargoCarrierCode]=''; [PortOfDischarge]='ZADUR'; [PortOfLoading]='GBFLX'; [DeclarationHouseBill]='S00001529'; [DeclarationMasterBill]=''; [DeclarationVoyage]=''; [DeclarationDirection]='IMP'; [DeclarationTransportMode]='RAI'; [DateCreated]='2023-01-01T00:00:00'", row1);
				AssertMultilineASCIIEquals("Row2 - Inconclusive", $"[ManifestJobReference]='C00001185'; [ManifestCargoCarrierCode]='00838463'; [ManifestHouseBill]='S00001530'; [ManifestMasterBill]='17612345675'; [ManifestVoyage]=''; [ManifestDirection]='IMP'; [ManifestTransportMode]=''; [DeclarationReference]=''; [DeclarationCargoCarrierCode]=''; [PortOfDischarge]='ZACPT'; [PortOfLoading]='AUSYD'; [DeclarationHouseBill]=''; [DeclarationMasterBill]=''; [DeclarationVoyage]=''; [DeclarationDirection]=''; [DeclarationTransportMode]=''; [DateCreated]='2023-01-01T00:00:00'", row2);
				AssertMultilineASCIIEquals("Row3 - IsDeviated - Cargo Carrier", $"[ManifestJobReference]='C00001186'; [ManifestCargoCarrierCode]='00838463'; [ManifestHouseBill]='S00001533'; [ManifestMasterBill]='17612345675'; [ManifestVoyage]='EK1457'; [ManifestDirection]='IMP'; [ManifestTransportMode]=''; [DeclarationReference]='J0000001'; [DeclarationCargoCarrierCode]='00838469'; [PortOfDischarge]='ZAPIT'; [PortOfLoading]='CNSHA'; [DeclarationHouseBill]='S00001533'; [DeclarationMasterBill]='17612345675'; [DeclarationVoyage]=''; [DeclarationDirection]='IMP'; [DeclarationTransportMode]='RAI'; [DateCreated]='2023-01-01T00:00:00'", row3);
				AssertMultilineASCIIEquals("Row4 - IsDeviated - Voyage", $"[ManifestJobReference]='C00001187'; [ManifestCargoCarrierCode]='00838465'; [ManifestHouseBill]='S00001535'; [ManifestMasterBill]='17612345675'; [ManifestVoyage]='EK1457'; [ManifestDirection]='IMP'; [ManifestTransportMode]=''; [DeclarationReference]='J0000002'; [DeclarationCargoCarrierCode]='00838465'; [PortOfDischarge]='ZAWAT'; [PortOfLoading]='FRCAL'; [DeclarationHouseBill]='S00001535'; [DeclarationMasterBill]='17612345676'; [DeclarationVoyage]=''; [DeclarationDirection]='IMP'; [DeclarationTransportMode]='RAI'; [DateCreated]='2023-01-01T00:00:00'", row4);
			});
		}

		protected override List<string> ParametersValuesList
		{
			get
			{
				return new List<string>()
				{
					string.Format(CultureInfo.InvariantCulture, "'{0}'", GlbCompany.CurrentCompany.PK),
					string.Format(CultureInfo.InvariantCulture, "'{0}'", "IMP"),
					string.Format(CultureInfo.InvariantCulture, "{0}", "NULL"),
					string.Format(CultureInfo.InvariantCulture, "{0}", "NULL"),
					string.Format(CultureInfo.InvariantCulture, "{0}", "NULL"),
					string.Format(CultureInfo.InvariantCulture, "{0}", "NULL"),
					string.Format(CultureInfo.InvariantCulture, "{0}", "NULL")
				};
			}
		}

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				var cols = new string[] {
					"ManifestJobReference", "ManifestCargoCarrierCode", "ManifestHouseBill", "ManifestMasterBill",
					"ManifestVoyage", "ManifestDirection", "ManifestTransportMode",
					"DeclarationReference", "DeclarationCargoCarrierCode", "DeclarationHouseBill", "DeclarationMasterBill",
					"DeclarationVoyage", "DeclarationDirection", "DeclarationTransportMode", "PortOfDischarge", "PortOfLoading"
				};
				foreach (var stringCol in cols)
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}
				allCols.Add(new ReportSchemaColumn(typeof(DateTime), "DateCreated"));
				return allCols;
			}
		}
	}
}

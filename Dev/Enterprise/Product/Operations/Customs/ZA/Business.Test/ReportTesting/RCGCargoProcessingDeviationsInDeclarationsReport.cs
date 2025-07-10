using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing.ReportTesting
{
	[TestDate(2023, 11, 14)]
	sealed class RCGCargoProcessingDeviationsInDeclarationsReport : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "RCGCargoProcessingDeviationsInDeclarationsReport";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "JE_DeclarationReference", "CH_BGMReference", "Declaration_EntryStatus", "Declaration_CargoCarrierCode",
					"JE_HouseBill", "Declaration_MasterBillNumber", "JE_VoyageFlightNo", "JE_MessageType", "JE_TransportMode", "JK_UniqueConsignRef", "AMA_JobReference",
					"GlobalManifestCarrier", "BillNumber", "Manifest_MasterBillNumber", "AMA_Voyage" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}
				allCols.Add(new ReportSchemaColumn(typeof(DateTime), "JE_SystemCreateTimeUtc"));
				return allCols;
			}
		}

		protected override List<string> ParametersValuesList => new List<string>()
		{
			$"'{GlbCompany.CurrentCompany.PK}'" // @CompanyPK
		};

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		void CreateDeclarationAndManifest(int index, string messageType = ZAJobMessageTypeList.Codes.Import, bool createManifest = false, bool linkDeclarationToShipment = true)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = $"B0000100{index}";
			declaration.JE_MessageType = messageType;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = $"MB000{index}";
			declaration.JE_HouseBill = $"HB000{index}";
			declaration.JE_VoyageFlightNo = $"V000{index}";
			declaration.JE_CargoCarrier = $"0010100{index}";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = $"1000000{index}";
			entryHeader.CH_EntryStatus = "1";

			if (createManifest)
			{
				var shipment = Factory.New<ForwardingShipment>();
				var consol = shipment.Consols.AddNew();
				consol.JK_UniqueConsignRef = $"100000000{index}";

				var manifestHeader = Factory.New<AsycudaManifestHeader>();
				manifestHeader.AMA_Voyage = $"V000{index}";

				var masterBill = manifestHeader.MasterBill;
				masterBill.ABL_BillNumber = declaration.JE_MasterBill;
				masterBill.ABL_BillIssuer = $"MB Issuer {index}";

				var houseBill = manifestHeader.Bills.AddNew();
				houseBill.ABL_JS_Shipment = shipment.PK;
				houseBill.ABL_BillNumber = declaration.JE_HouseBill;
				houseBill.ABL_BillIssuer = $"HB Issuer {index}";

				if (linkDeclarationToShipment)
				{
					declaration.JE_JS = shipment.PK;
				}
			}
		}

		protected override void PrepareTestData()
		{
			CreateDeclarationAndManifest(1);
			CreateDeclarationAndManifest(2, ZAJobMessageTypeList.Codes.ExBond);
			CreateDeclarationAndManifest(3, createManifest: true, linkDeclarationToShipment: false);
			CreateDeclarationAndManifest(4, createManifest: true);
			Factory.Save();
		}

		protected override void AssertTestResults(DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(3, results.Rows.Count);

				var row1 = FormatRowsValues(results.Rows[0], results, true);
				AssertMultilineASCIIEquals("Row 1: without manifest",
					"[JE_DeclarationReference]='B00001001'; [CH_BGMReference]='10000001'; [Declaration_EntryStatus]='1'; " +
					"[Declaration_CargoCarrierCode]='00101001'; [JE_HouseBill]='HB0001'; [Declaration_MasterBillNumber]='MB0001'; [JE_VoyageFlightNo]='V0001'; " +
					"[JE_MessageType]='IMP'; [JE_TransportMode]='SEA'; [JK_UniqueConsignRef]=''; [AMA_JobReference]=''; [GlobalManifestCarrier]=''; " +
					"[BillNumber]=''; [Manifest_MasterBillNumber]=''; [AMA_Voyage]=''; [JE_SystemCreateTimeUtc]='2023-11-14T00:00:00'", row1);

				var row2 = FormatRowsValues(results.Rows[1], results, true);
				AssertMultilineASCIIEquals("Row 2: with manifest and linked by house bill number",
					"[JE_DeclarationReference]='B00001003'; [CH_BGMReference]='10000003'; [Declaration_EntryStatus]='1'; " +
					"[Declaration_CargoCarrierCode]='00101003'; [JE_HouseBill]='HB0003'; [Declaration_MasterBillNumber]='MB0003'; [JE_VoyageFlightNo]='V0003'; " +
					"[JE_MessageType]='IMP'; [JE_TransportMode]='SEA'; [JK_UniqueConsignRef]=''; [AMA_JobReference]='OGM0000001'; [GlobalManifestCarrier]='MB Issuer 3'; " +
					"[BillNumber]='HB0003'; [Manifest_MasterBillNumber]='MB0003'; [AMA_Voyage]='V0003'; [JE_SystemCreateTimeUtc]='2023-11-14T00:00:00'", row2);

				var row3 = FormatRowsValues(results.Rows[2], results, true);
				AssertMultilineASCIIEquals("Row 3: with manifest",
					"[JE_DeclarationReference]='S00001001'; [CH_BGMReference]='10000004'; [Declaration_EntryStatus]='1'; " +
					"[Declaration_CargoCarrierCode]='00101004'; [JE_HouseBill]='HB0004'; [Declaration_MasterBillNumber]='MB0004'; [JE_VoyageFlightNo]='V0004'; " +
					"[JE_MessageType]='IMP'; [JE_TransportMode]='SEA'; [JK_UniqueConsignRef]='1000000004'; [AMA_JobReference]='OGM0000002'; [GlobalManifestCarrier]='MB Issuer 4'; " +
					"[BillNumber]='HB0004'; [Manifest_MasterBillNumber]='MB0004'; [AMA_Voyage]='V0004'; [JE_SystemCreateTimeUtc]='2023-11-14T00:00:00'", row3);
			});
		}
	}
}

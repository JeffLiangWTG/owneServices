using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using COSTCO = Enterprise.Customs.ZA.Business.ManifestTypeList.Codes;
using GOVGIO = Enterprise.Customs.ZA.Business.GateInOutMessageTypeCodeList.Codes;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForMasterChild))]
	sealed class AsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
	{
		readonly string[] costcoCodes = new ManifestTypeList().GetAllCodes();
		readonly string[] govgioCodes = new GateInOutMessageTypeCodeList().GetAllCodes();

		public void TestCheckABL_E_ARV_WhenCOSTCO()
		{
			SetupPorts();
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.MasterBill.ABL_RL_NKPortOfDischarge = "ZAXXX";
				header.MasterBill.ABL_E_ARV = ZDate.Empty;
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_ARVInfo, MandatoryValidation.YouHaveNotEntered);

				header.MasterBill.ABL_E_ARV = ZDate.Today;
				AssertNoMessageErrors(code, header.MasterBill.ABL_E_ARVInfo);

				header.MasterBill.ABL_RL_NKPortOfDischarge = "ITXXX";
				header.MasterBill.ABL_E_ARV = ZDate.Empty;
				AssertNoMessageErrors(code, header.MasterBill.ABL_E_ARVInfo);

				header.MasterBill.ABL_E_ARV = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_ARVInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckABL_E_ARV_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.DepotGateOut))
				{
					header.MasterBill.ABL_E_ARV = ZDate.Empty;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_ARVInfo, MandatoryValidation.YouHaveNotEntered);

					header.MasterBill.ABL_E_ARV = ZDate.Today;
					AssertNoMessageErrors(code, header.MasterBill.ABL_E_ARVInfo);
				}
				else
				{
					header.MasterBill.ABL_E_ARV = ZDate.Empty;
					AssertNoMessageErrors(code, header.MasterBill.ABL_E_ARVInfo);

					header.MasterBill.ABL_E_ARV = ZDate.Today;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_ARVInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckABL_E_ARV_ShouldNotBeLessThan()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			header.MasterBill.ABL_E_DEP = ZDateTime.Today;
			header.MasterBill.ABL_E_ARV = ZDateTime.Today.AddDays(-1);
			AssertHasMessageErrorContaining(header.MasterBill.ABL_E_ARVInfo, "should not be less than");
			header.MasterBill.ABL_E_DEP = ZDateTime.Today;
			header.MasterBill.ABL_E_ARV = ZDateTime.Today.AddDays(1);
			AssertNoMessageErrorContaining(header.MasterBill.ABL_E_ARVInfo, "should not be less than");
		}

		public void TestCheckABL_E_DEP_WhenCOSTCO()
		{
			SetupPorts();
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.MasterBill.ABL_RL_NKPortOfLoading = "ZAXXX";
				header.MasterBill.ABL_E_DEP = ZDate.Empty;
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_DEPInfo, MandatoryValidation.YouHaveNotEntered);

				header.MasterBill.ABL_E_DEP = ZDate.Today;
				AssertNoMessageErrors(code, header.MasterBill.ABL_E_DEPInfo);

				header.MasterBill.ABL_RL_NKPortOfLoading = "ITXXX";
				header.MasterBill.ABL_E_DEP = ZDate.Empty;
				AssertNoMessageErrors(code, header.MasterBill.ABL_E_DEPInfo);

				header.MasterBill.ABL_E_DEP = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_DEPInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckABL_E_DEP_WhenGOVGIO()
		{
			SetupPorts();
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.DepotGateOut))
				{
					header.MasterBill.ABL_E_DEP = ZDate.Empty;
					AssertNoMessageErrors(code, header.MasterBill.ABL_E_DEPInfo);

					header.MasterBill.ABL_E_DEP = ZDate.Today;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_DEPInfo, MandatoryValidation.DoNotEntered);
				}
				else if (code == GOVGIO.DepotGateIn)
				{
					header.MasterBill.ABL_RL_NKPortOfLoading = "ZAXXX";
					header.MasterBill.ABL_E_DEP = ZDate.Empty;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_DEPInfo, MandatoryValidation.YouHaveNotEntered);

					header.MasterBill.ABL_E_DEP = ZDate.Today;
					AssertNoMessageErrors(code, header.MasterBill.ABL_E_DEPInfo);

					header.MasterBill.ABL_RL_NKPortOfLoading = "ITXXX";
					header.MasterBill.ABL_E_DEP = ZDate.Empty;
					AssertNoMessageErrors(code, header.MasterBill.ABL_E_DEPInfo);

					header.MasterBill.ABL_E_DEP = ZDate.Today;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_DEPInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					header.MasterBill.ABL_E_DEP = ZDate.Empty;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_E_DEPInfo, MandatoryValidation.YouHaveNotEntered);

					header.MasterBill.ABL_E_DEP = ZDate.Today;
					AssertNoMessageErrors(code, header.MasterBill.ABL_E_DEPInfo);
				}
			}
		}

		public void TestCheckABL_E_DEP_ShouldNotBeGreaterThan()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			header.MasterBill.ABL_E_ARV = ZDateTime.Today.AddDays(-1);
			header.MasterBill.ABL_E_DEP = ZDateTime.Today;
			AssertHasMessageErrorContaining(header.MasterBill.ABL_E_DEPInfo, "should not be greater than");
			header.MasterBill.ABL_E_ARV = ZDateTime.Today.AddDays(1);
			header.MasterBill.ABL_E_DEP = ZDateTime.Today;
			AssertNoMessageErrorContaining(header.MasterBill.ABL_E_DEPInfo, "should not be greater than");
		}

		public void TestCheckABL_BillIssueDate_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				header.AMA_IssueDate = ZDate.Empty;
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_BillIssueDateInfo, MandatoryValidation.YouHaveNotEntered);

				header.AMA_IssueDate = ZDate.Today;
				AssertNoMessageErrors(code, header.MasterBill.ABL_BillIssueDateInfo);
			}
		}

		public void TestCheckABL_BillIssueDate_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.AMA_IssueDate = ZDate.Empty;
				AssertNoMessageErrors(code, header.MasterBill.ABL_BillIssueDateInfo);

				header.AMA_IssueDate = ZDate.Today;
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_BillIssueDateInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckABL_RL_NKPortOfDischarge_WhenCOSTCO()
		{
			SetupPorts();
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				foreach (var nature in new NatureList().GetAllCodes())
				{
					header.AMA_Nature = nature;

					if (nature.In(NatureList.Codes.Import23, NatureList.Codes.Transit24, NatureList.Codes.Transhipment28))
					{
						header.MasterBill.ABL_RL_NKPortOfDischarge = ZString.Empty;
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, MandatoryValidation.YouHaveNotEntered);

						header.MasterBill.ABL_RL_NKPortOfDischarge = "XXXXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ListValidation.InvalidCodeMessageError.ToString());

						header.MasterBill.ABL_RL_NKPortOfDischarge = "ITXXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "Port of Discharge must be ZA.");

						header.MasterBill.ABL_RL_NKPortOfDischarge = "ZAXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "Port Code must be 5 characters.");

						header.MasterBill.ABL_RL_NKPortOfDischarge = "ZAXXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, "IATA code is required.");

						header.MasterBill.ABL_RL_NKPortOfDischarge = "ZAZZZ";
						AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo);
					}
					else
					{
						header.MasterBill.ABL_RL_NKPortOfDischarge = ZString.Empty;
						AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo);

						header.MasterBill.ABL_RL_NKPortOfDischarge = "ITXXX";
						AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo);
					}
				}
			}
		}

		public void TestCheckABL_RL_NKPortOfDischarge_WhenGOVGIO()
		{
			SetupPorts();
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.DepotGateIn))
				{
					header.MasterBill.ABL_RL_NKPortOfDischarge = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, MandatoryValidation.YouHaveNotEntered);

					header.MasterBill.ABL_RL_NKPortOfDischarge = "ZAZZZ";
					AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo);

					header.MasterBill.ABL_RL_NKPortOfDischarge = "XXXX";
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, ListValidation.InvalidCodeMessageError.ToString());
				}
				else
				{
					header.MasterBill.ABL_RL_NKPortOfDischarge = ZString.Empty;
					AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo);

					header.MasterBill.ABL_RL_NKPortOfDischarge = "ZAZZZ";
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfDischargeInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckABL_RL_NKPortOfLoading_WhenCOSTCO()
		{
			SetupPorts();
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				foreach (var nature in new NatureList().GetAllCodes())
				{
					if (header.IsExport)
					{
						header.MasterBill.ABL_RL_NKPortOfLoading = ZString.Empty;
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

						header.MasterBill.ABL_RL_NKPortOfLoading = "XXXXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeMessageError.ToString());

						header.MasterBill.ABL_RL_NKPortOfLoading = "ITXXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "Port of Loading must be ZA.");

						header.MasterBill.ABL_RL_NKPortOfLoading = "ZAXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "Port Code must be 5 characters.");

						header.MasterBill.ABL_RL_NKPortOfLoading = "ZAXXX";
						AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo, "IATA code is required.");

						header.MasterBill.ABL_RL_NKPortOfLoading = "ZAZZZ";
						AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo);
					}
					else
					{
						header.MasterBill.ABL_RL_NKPortOfLoading = ZString.Empty;
						AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo);

						header.MasterBill.ABL_RL_NKPortOfLoading = "ITXXX";
						AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo);
					}
				}
			}
		}

		public void TestCheckABL_RL_NKPortOfLoading_WhenGOVGIO()
		{
			SetupPorts();
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				header.MasterBill.ABL_RL_NKPortOfLoading = ZString.Empty;
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

				header.MasterBill.ABL_RL_NKPortOfLoading = "ZAZZZ";
				AssertNoMessageErrors(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo);

				header.MasterBill.ABL_RL_NKPortOfLoading = "XXXXX";
				AssertHasMessageErrorContaining(code, header.MasterBill.ABL_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		public void TestCheckABL_BillNumber_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.VesselOutturnReport, COSTCO.AirExcessOutturnReport))
				{
					header.AMA_MasterBill = ZString.Empty;
					AssertNoMessageErrors(code, header.MasterBill.ABL_BillNumberInfo);

					header.AMA_MasterBill = "12345";
					AssertNoMessageErrors(code, header.MasterBill.ABL_BillNumberInfo);
				}
				else
				{
					header.AMA_MasterBill = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_MasterBill = "12345";
					AssertNoMessageErrors(code, header.MasterBill.ABL_BillNumberInfo);
				}
			}
		}

		public void TestCheckABL_BillNumber_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.SeaDepotConsignmentGateIn, GOVGIO.BreakBulkGateIn))
				{
					header.AMA_MasterBill = ZString.Empty;
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);

					header.AMA_MasterBill = "12345";
					AssertNoMessageErrors(code, header.MasterBill.ABL_BillNumberInfo);
				}
				else
				{
					header.AMA_MasterBill = ZString.Empty;
					AssertNoMessageErrors(code, header.MasterBill.ABL_BillNumberInfo);

					header.AMA_MasterBill = "12345";
					AssertHasMessageErrorContaining(code, header.MasterBill.ABL_BillNumberInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		void SetupPorts()
		{
			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.Code = "ZAXXX";
			unloco1.RL_RN_NKCountryCode = "ZA";

			var unloco3 = Factory.New<RefUNLOCO>();
			unloco3.Code = "ZAXX";
			unloco3.RL_RN_NKCountryCode = "ZA";

			var unloco4 = Factory.New<RefUNLOCO>();
			unloco4.Code = "ZAZZZ";
			unloco4.RL_IATA = "XXX";
			unloco4.RL_RN_NKCountryCode = "ZA";
			Factory.Save();
		}
	}
}

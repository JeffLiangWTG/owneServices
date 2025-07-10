using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using COSTCO = Enterprise.Customs.ZA.Business.ManifestTypeList.Codes;
using GOVGIO = Enterprise.Customs.ZA.Business.GateInOutMessageTypeCodeList.Codes;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForRegularBill))]
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		readonly string[] costcoCodes = new ManifestTypeList().GetAllCodes();
		readonly string[] govgioCodes = new GateInOutMessageTypeCodeList().GetAllCodes();

		public void TestValidateAll()
		{
			var bill = Factory.New<AsycudaBill>();
			var validation = new AsycudaBillValidationForRegularBill(bill);
			AssertNoExceptionThrown(validation.ValidateAll);
		}

		public void TestValidateAll_WhenTypeIsEmpty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertNullOrEmpty("PRE-CONDITION", header.AMA_ManifestType);
			AssertNullOrEmpty("PRE-CONDITION", header.GateInOutMessageType);
			bill.Validation.ValidateAll();
			AssertEquals(0, bill.Notifications.Count());
		}

		public void TestCheckABL_BillNumber_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.AirExcessOutturnReport, COSTCO.VesselOutturnReport))
				{
					bill.ABL_BillNumber = ZString.Empty;
					AssertNoMessageErrors(code, bill.ABL_BillNumberInfo);

					bill.ABL_BillNumber = "12345";
					AssertHasMessageErrorContaining(code, bill.ABL_BillNumberInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					bill.ABL_BillNumber = ZString.Empty;
					AssertNoMessageErrors(code, bill.ABL_BillNumberInfo);

					bill.ABL_BillNumber = "12345";
					AssertNoMessageErrors(code, bill.ABL_BillNumberInfo);
				}
			}
		}

		public void TestCheckABL_BillNumber_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				bill.ABL_BillNumber = ZString.Empty;
				AssertHasMessageErrorContaining(code, bill.ABL_BillNumberInfo, MandatoryValidation.YouHaveNotEntered);

				bill.ABL_BillNumber = "12345";
				AssertNoMessageErrors(code, bill.ABL_BillNumberInfo);
			}
		}

		public void TestCheckABL_BillIssuer_WhenCOSTCO()
		{
			SetupCarrier();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.AirExcessOutturnReport, COSTCO.VesselOutturnReport))
				{
					bill.ABL_BillIssuer = ZString.Empty;
					AssertNoMessageErrors(code, bill.ABL_BillIssuerInfo);

					bill.ABL_BillIssuer = "123";
					AssertHasMessageErrorContaining(code, bill.ABL_BillIssuerInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					bill.ABL_BillIssuer = ZString.Empty;
					AssertHasMessageErrorContaining(code, bill.ABL_BillIssuerInfo, MandatoryValidation.YouHaveNotEntered);

					bill.ABL_BillIssuer = "X";
					AssertHasMessageErrorContaining(code, bill.ABL_BillIssuerInfo, ListValidation.InvalidCodeMessageError.ToString());

					bill.ABL_BillIssuer = "123";
					AssertNoMessageErrors(code, bill.ABL_BillIssuerInfo);
				}
			}
		}

		public void TestCheckABL_BillIssuer_WhenGOVGIO()
		{
			SetupCarrier();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.SeaDepotConsignmentGateIn, GOVGIO.DepotGateIn, GOVGIO.DepotGateOut))
				{
					bill.ABL_BillIssuer = ZString.Empty;
					AssertHasMessageErrorContaining(code, bill.ABL_BillIssuerInfo, MandatoryValidation.YouHaveNotEntered);

					bill.ABL_BillIssuer = "X";
					AssertHasMessageErrorContaining(code, bill.ABL_BillIssuerInfo, ListValidation.InvalidCodeMessageError.ToString());

					bill.ABL_BillIssuer = "123";
					AssertNoMessageErrors(code, bill.ABL_BillIssuerInfo);
				}
				else
				{
					bill.ABL_BillIssuer = ZString.Empty;
					AssertNoMessageErrors(code, bill.ABL_BillIssuerInfo);

					bill.ABL_BillIssuer = "123";
					AssertNoMessageErrors(code, bill.ABL_BillIssuerInfo);
				}
			}
		}

		public void TestCheckMRN_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				bill.MRN = ZString.Empty;
				AssertNoMessageErrors(code, bill.MRNInfo);

				bill.MRN = "12345";
				AssertHasMessageErrorContaining(code, bill.MRNInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckMRN_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				if (code.In(GOVGIO.TerminalGateOut, GOVGIO.TerminalGateIn))
				{
					bill.MRN = ZString.Empty;
					AssertNoMessageErrors(code, bill.MRNInfo);

					bill.MRN = "12345";
					AssertHasMessageErrorContaining(code, bill.MRNInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					bill.MRN = ZString.Empty;
					AssertNoMessageErrors(code, bill.MRNInfo);

					bill.MRN = "12345!";
					AssertHasMessageError(code, bill.MRNInfo, "MRN should be alphanumeric only.");

					bill.MRN = "12345";
					AssertNoMessageErrors(code, bill.MRNInfo);
				}
			}
		}

		public void TestCheckLRN_WhenCOSTCO()
		{
			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.Code = "ZAXX";
			unloco1.RL_RN_NKCountryCode = "ZA";

			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.Code = "ITXX";
			unloco2.RL_RN_NKCountryCode = "IT";
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirCargoOutturnReport))
				{
					header.MasterBill.ABL_RL_NKPortOfLoading = "ZAXX";
					bill.LRN = ZString.Empty;
					AssertHasMessageErrorContaining(code, bill.LRNInfo, MandatoryValidation.YouHaveNotEntered);

					bill.LRN = "12345";
					AssertNoMessageErrors(code, bill.LRNInfo);

					header.MasterBill.ABL_RL_NKPortOfLoading = "ITXX";
					bill.LRN = ZString.Empty;
					AssertNoMessageErrors(code, bill.LRNInfo);

					bill.LRN = "12345";
					AssertHasMessageErrorContaining(code, bill.LRNInfo, MandatoryValidation.DoNotEntered);
				}
				else
				{
					bill.LRN = ZString.Empty;
					AssertNoMessageErrors(code, bill.LRNInfo);

					bill.LRN = "12345";
					AssertHasMessageErrorContaining(code, bill.LRNInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckLRN_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				bill.LRN = ZString.Empty;
				AssertNoMessageErrors(code, bill.LRNInfo);

				bill.LRN = "12345";
				AssertHasMessageErrorContaining(code, bill.LRNInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestCheckCustomsCPC_WhenCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in costcoCodes)
			{
				header.AMA_ManifestType = code;

				if (code.In(COSTCO.DepotOutturnReport, COSTCO.BulkBreakBulkOutturnReport, COSTCO.AirCargoOutturnReport))
				{
					bill.CustomsCPC = ZString.Empty;
					AssertNoMessageErrors(code, bill.CustomsCPCInfo);

					bill.CustomsCPC = "12345";
					AssertHasMessageErrorContaining(code, bill.CustomsCPCInfo, "Export CPP should be one letter followed by 4 numeric.");

					bill.CustomsCPC = "A1234";
					AssertNoMessageErrors(code, bill.CustomsCPCInfo);
				}
				else
				{
					bill.CustomsCPC = ZString.Empty;
					AssertNoMessageErrors(code, bill.CustomsCPCInfo);

					bill.CustomsCPC = "12345";
					AssertHasMessageErrorContaining(code, bill.CustomsCPCInfo, MandatoryValidation.DoNotEntered);
				}
			}
		}

		public void TestCheckCustomsCPC_WhenGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			foreach (var code in govgioCodes)
			{
				header.GateInOutMessageType = code;

				bill.CustomsCPC = ZString.Empty;
				AssertNoMessageErrors(code, bill.CustomsCPCInfo);

				bill.CustomsCPC = "12345";
				AssertHasMessageErrorContaining(code, bill.CustomsCPCInfo, MandatoryValidation.DoNotEntered);
			}
		}

		void SetupCarrier()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var carrierCode = helper.CreateCarrierCode("123", "Description", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCarrierCodeAttribute(carrierCode.PK, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, "CARGOCARRIER");
			Factory.Save();
		}
	}
}

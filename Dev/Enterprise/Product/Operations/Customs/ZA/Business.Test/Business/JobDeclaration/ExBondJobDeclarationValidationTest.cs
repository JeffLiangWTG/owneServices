using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ExBondJobDeclarationValidation))]
	sealed class ExBondJobDeclarationValidationTest : ImportCommonJobDeclarationValidationAbstractTest
	{
		public void TestCheckJE_TransportMode()
		{
			CombineAssertions("Check JE_TransportMode", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertHasMessageError("EXW, country of destination is not BLNS, transport field is not blank", declaration.JE_TransportModeInfo, "Please do not enter a Mode of Transportation.");
				declaration.JE_RL_NKFinalDestination = "LSMSU";
				declaration.Validation.ValidateJE_TransportMode();
				AssertNoMessageError("EXW, country of destination is BLNS, transport field is not blank", declaration.JE_TransportModeInfo, "Please do not enter a Mode of Transportation.");
			});
		}

		public void TestExwarehousingIsExcludedFromValidatingTotalWeight()
		{
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().JI_CustomsQuantity = 1000m;
			declaration.JE_TotalWeight = 0m;
			AssertNoNotifications(declaration.JE_TotalWeightInfo);
		}

		public void TestJE_VoyageFlightNo()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "ABC";
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, ExBondJobDeclarationValidation.VoyageFlightNoMustBeEmpty("Flight/Folio"));
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_VoyageFlightNoInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = "ABC";
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, ExBondJobDeclarationValidation.VoyageFlightNoMustBeEmpty("Voyage"));
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_VoyageFlightNoInfo);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.JE_VoyageFlightNo = "ABC";
			AssertNoMessageErrors(declaration.JE_VoyageFlightNoInfo);
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrors(declaration.JE_VoyageFlightNoInfo);
		}

		public void TestCheckJE_VesselNameExBond()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "ABC";
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ExBondJobDeclarationValidation.VesselMustBeBlank);
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			declaration.JE_RL_NKOrigin = "ZZZZZ";
			AssertHasMessageError(declaration.JE_RL_NKOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestMessageErrorOnEmptyMasterBillWhenExBond()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_MasterBill = "AAA";
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Mail;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.FixedTransportInstallations;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Other;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_TransportMode = "";
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_RemovalTransportCode = Enterprise.Core.Constants.TransportModes.Road;
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoMessageError(declaration.JE_MasterBillInfo, JobDeclarationValidation.MasterBillMustBeEmpty(declaration.JE_MasterBillInfo.HumanReadableName));
			declaration.JE_MasterBill = ZString.Empty;
			AssertHasMessageErrors(declaration.JE_MasterBillInfo);
		}

		public void TestCheckJE_MasterBillIssuedDate_Exbond()
		{
			string[] transportModes = new string[]
			{
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Air,
				Core.Constants.TransportModes.Road,
				Core.Constants.TransportModes.Rail,
				Core.Constants.TransportModes.Mail
			};
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			foreach (string transportMode in transportModes)
			{
				CombineAssertions(transportMode, () =>
				{
					declaration.JE_TransportMode = transportMode;
					declaration.JE_RemovalTransportCode = ZString.Empty;
					declaration.JE_MasterBill = "1";
					declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 6, 16);
					AssertHasMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedDateMustBeEmpty);

					declaration.JE_MasterBill = ZString.Empty;
					declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
					declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 6, 16);
					AssertHasMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedDateMasterBillNotCaptured(declaration.JE_MasterBillInfo.HumanReadableName));

					declaration.JE_MasterBillIssuedDate = ZDateTime.Empty;
					AssertNoMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedDateMasterBillNotCaptured(declaration.JE_MasterBillInfo.HumanReadableName));
				});
			}

			declaration.JE_RL_NKFinalDestination = "LSMSU";
			declaration.JE_MasterBill = "AAA";
			declaration.JE_MasterBillIssuedDate = ZDateTime.Empty;
			AssertHasMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmptyBLNS);

			declaration.JE_RL_NKFinalDestination = "DEFRA";
			declaration.Validation.ValidateJE_MasterBillIssuedDate();
			AssertNoMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmptyBLNS);

			declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
			AssertHasMessageError(declaration.JE_MasterBillIssuedDateInfo, ValidationConstants.Declaration.MasterBillIssuedAtMustBeEmpty);
		}

		public void TestCheckJE_RL_NKMasterBillIssuedAt_Exbond()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_RL_NKFinalDestination = "LSMSU";
			declaration.JE_MasterBill = "AAA";
			declaration.JE_RL_NKMasterBillIssuedAt = "";
			AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmptyBLNS);

			declaration.JE_RL_NKFinalDestination = "DEFRA";
			declaration.Validation.ValidateJE_RL_NKMasterBillIssuedAt();
			AssertNoMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtCannotBeEmptyBLNS);

			declaration.JE_RL_NKMasterBillIssuedAt = "ZAJNB";
			AssertHasMessageError(declaration.JE_RL_NKMasterBillIssuedAtInfo, ValidationConstants.Declaration.MasterBillIssuedAtMustBeEmpty);
		}

		protected override ZString MessageType => ZAJobMessageTypeList.Codes.ExBond;
	}
}

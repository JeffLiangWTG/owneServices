using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class PortAuthorityShipmentValidationTest : BaseAgencyTest
	{
		public void TestPrincipalMessageValidation()
		{
			const string NoCode = "This principal does not have a 1-Stop code.";

			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = principal.PK;
			AssertHasMessageError(shipment.JS_OH_DeliveryAgentInfo, NoCode);

			OrgCusCode oneStop = principal.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.OneStopCode, Constants.CountryCodes.Australia);

			if (oneStop == null)
			{
				oneStop = principal.CustomsCodes.AddNew();
				oneStop.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				oneStop.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			}

			oneStop.OK_CustomsRegNo = "BLAT";

			shipment.Validation.ValidateJS_OH_DeliveryAgent();
			AssertNoMessageError(shipment.JS_OH_DeliveryAgentInfo, NoCode);
		}

		public void TestSailingMessageValidation()
		{
			const string NoSailing = "No sailing selected.";
			const string NoCTO = "This discharge port does not have a CTO address specified.";
			const string NoCode = "The CTO for this discharge port does not have a 1-Stop code.";

			OrgHeader cto = Factory.NewWithValidTestData<OrgHeader>();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();

			JobSailing importSailing = voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE");
			JobSailing exportSailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "NZAKL");

			BillOfLading shipment = Factory.New<BillOfLading>();

			shipment.RunPreSaveValidation();
			AssertHasMessageError(shipment.JS_NKLoadPortInfo, NoSailing);
			AssertHasMessageError(shipment.JS_NKDischargePortInfo, NoSailing);

			shipment.JS_RL_NKOrigin = importSailing.JX_JA_RL_NKPortOfLoading;
			shipment.JS_RL_NKDestination = importSailing.JX_JB_RL_NKPortOfDischarge;
			shipment.JS_JX = importSailing.PK;
			shipment.RunPreSaveValidation();
			AssertNoMessageError(shipment.JS_NKLoadPortInfo, NoSailing);
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoSailing);
			AssertHasMessageError(shipment.JS_NKDischargePortInfo, NoCTO);

			shipment.Sailing.Destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;
			shipment.RunPreSaveValidation();
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoSailing);
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoCTO);
			AssertHasMessageError(shipment.JS_NKDischargePortInfo, NoCode);

			OrgCusCode oneStop = cto.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.OneStopCode, Constants.CountryCodes.Australia);

			if (oneStop == null)
			{
				oneStop = cto.CustomsCodes.AddNew();
				oneStop.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				oneStop.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			}

			oneStop.OK_CustomsRegNo = "BLAT";

			shipment.RunPreSaveValidation();
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoSailing);
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoCTO);
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoCode);

			shipment.JS_RL_NKOrigin = exportSailing.JX_JA_RL_NKPortOfLoading;
			shipment.JS_RL_NKDestination = exportSailing.JX_JB_RL_NKPortOfDischarge;
			shipment.JS_JX = exportSailing.PK;
			shipment.RunPreSaveValidation();
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoSailing);
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoCTO);
			AssertNoMessageError(shipment.JS_NKDischargePortInfo, NoCode);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			EIDOBusinessObjectValidation.RegisterForFactory(Factory);
		}

		#endregion
	}
}

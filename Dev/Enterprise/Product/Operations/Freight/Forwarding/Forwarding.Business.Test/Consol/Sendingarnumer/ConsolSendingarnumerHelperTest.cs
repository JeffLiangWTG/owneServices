using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolSendingarnumerHelperTest : TestCaseWithFactory
	{
		#region Validation

		public void TestValiateShipmentCRNMatch()
		{
			string errorMsg = "This Consolidation has Shipment(s) with invalid Sendingarnumer. Please regenerate the shipment's Sendingarnumer from the Actions Menu";
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_CRN = "F-258-1010-8-IS-REY-9878";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.CustomsEntryNumber = "F-258-1010-8-IS-REY-J012-I";
			AssertEquals(false, consol.JK_CRNInfo.HasError(errorMsg));

			consol.JK_CRN = "F-258-1010-8-IS-XXX-9878";
			AssertEquals(true, consol.JK_CRNInfo.HasError(errorMsg));
		}

		public void TestValidateNoDupliateCRNs()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_CRN = "F-258-1010-8-IS-REY-9878";
			Factory.Save();
			string firstConsolNumber = consol.JK_UniqueConsignRef;

			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_CRN = "F-258-1010-8-IS-REY-9878";
			AssertEquals("should have error message", true, consol.JK_CRNInfo.HasError("This Sendingarnumer is already been used by Consolidation: " + firstConsolNumber));
		}

		public void TestValidateCarrierCode()
		{
			consol.JK_CRN = "1";
			AssertEquals("Carrier code should only be alphabetic", true, consol.JK_CRNInfo.HasError("The Carrier Code must be Alphabetic"));

			consol.JK_CRN = "B";
			AssertEquals("The Carrier Code cannot be: B, Q, T and X", true, consol.JK_CRNInfo.HasError("The Carrier Code cannot be: B, Q, T and X"));

			consol.JK_CRN = "Q";
			AssertEquals("The Carrier Code cannot be: B, Q, T and X", true, consol.JK_CRNInfo.HasError("The Carrier Code cannot be: B, Q, T and X"));

			consol.JK_CRN = "T";
			AssertEquals("The Carrier Code cannot be: B, Q, T and X", true, consol.JK_CRNInfo.HasError("The Carrier Code cannot be: B, Q, T and X"));

			consol.JK_CRN = "X";
			AssertEquals("The Carrier Code cannot be: B, Q, T and X", true, consol.JK_CRNInfo.HasError("The Carrier Code cannot be: B, Q, T and X"));

			consol.JK_CRN = "F";
			AssertNull(consol.ShippingLine);
			AssertEquals("Carrier code", true, consol.JK_CRNInfo.HasError("Please enter a Carrier"));

			OrgHeader org = GetTestOrgWithCarrierCode("E");
			org.OH_FullName = "blahblah";
			consol.JK_OA_ShippingLineAddress = org.MainAddress.PK;
			consol.JK_CRN = "F";
			AssertNotNull(consol.ShippingLine);
			consol.JK_CRN = "F";
			AssertEquals("Carrier does not this carrier code set", true, consol.JK_CRNInfo.HasError("Carrier blahblah does not have Carrier Code 'F'"));
		}

		public void TestVesselFlightNumber()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_CRN = "FF8908088AUSYD";
			AssertEquals("validation for transport type", true, consol.JK_CRNInfo.HasError("This Sendingarnumer is for Sea."));

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_CRN = "F98908088ISREY";
			AssertEquals("validation for transport type", true, consol.JK_CRNInfo.HasError("This Sendingarnumer is for Air."));

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "BB009";
			consol.JK_CRN = "F98908088ISREY";
			AssertEquals("validation for transport type", true, consol.JK_CRNInfo.HasError("Unable to find Flight Number BB009"));
		}

		public void TestValidateArrivalOrDepartureDate()
		{
			consol.JK_CRN = "F099A999";
			AssertEquals("date/month should be numeric", true, consol.JK_CRNInfo.HasError("Arrival/Departure date must be Numeric"));

			consol.JK_CRN = "F0999901";
			AssertEquals("date portion should not be greater than 31", true, consol.JK_CRNInfo.HasError("Days cannot be more than 31"));

			consol.JK_CRN = "F0990713";
			AssertEquals("date portion should not be greater than 31", true, consol.JK_CRNInfo.HasError("Months cannot be more than 12"));

			consol.JK_CRN = "F09931028";
			AssertEquals("valid date", true, consol.JK_CRNInfo.HasError("The date that you have entered is not valid"));
		}

		public void TestValidateYear()
		{
			consol.JK_CRN = "F0980808A";
			AssertEquals("Year should be numeric", true, consol.JK_CRNInfo.HasError("Year must be Numeric"));
		}

		public void TestValidatePortOfLoading()
		{
			consol.JK_CRN = "F09808088X1";
			AssertEquals("finding port within a country", true, consol.JK_CRNInfo.HasError("Unable to find Country/Region with Country/Region Code: X1"));

			consol.JK_CRN = "F09808088AUXXX";
			RefCountry aus = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			AssertEquals("finding port within a country", true, consol.JK_CRNInfo.HasError("Country/Region (AU) " + aus.RN_Desc + " does not have port code XXX"));
		}

		public void TestValidateCarrierNumber()
		{
			consol.JK_CRN = "F09808088AUSYDW000";
			AssertEquals(true, consol.JK_CRNInfo.HasError("The Carrier Number for Air type should only be numeric"));
		}

		public void TestValidateCheckDigit()
		{
			consol.JK_CRN = "F09808088AUSYDW0009";
			AssertEquals(true, consol.JK_CRNInfo.HasError("The Check Digit should be 'J'"));
		}

		#endregion

		#region AutoPopulation

		public void TestAutoPopulateTransportMode()
		{
			helper.FormatCRN("F89908088AUSYD");
			AssertEquals("Consol should be air", Core.Constants.TransportModes.Air, consol.JK_TransportMode);
		}

		public void TestAutoPopulateCarrier()
		{
			OrgHeader org = GetTestOrgWithCarrierCode("E");
			Factory.Save();
			helper.FormatCRN("E");
			AssertNull(consol.ShippingLine);

			helper.FormatCRN("E98708088AUSYD");
			AssertNotNull(consol.ShippingLine);
			AssertEquals("Should be the same org", org.PK, consol.ShippingLine.PK);
		}

		public void TestAutoPopulateTransportLeg()
		{
			int currentYear = ZDateTime.Now.Year;

			OrgHeader org = GetTestOrgWithCarrierCode("E");
			org.MiscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;
			RefAirline airline = org.MiscServ.Airline;

			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_VoyageFlight = airline.RM_TwoCharacterCode + "123";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			VoyageDestination destination = Factory.NewWithValidTestData<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "ISREY";
			ZDateTime eta = new ZDateTime(currentYear, 8, 8, 1, 1, 9);
			ZDateTime ata = new ZDateTime(currentYear, 8, 10, 2, 11, 9);
			destination.JB_E_ARV = eta;
			destination.JB_A_ARV = ata;
			destination.JB_JV = voyage.PK;

			VoyageOrigin origin = Factory.NewWithValidTestData<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_JV = voyage.PK;

			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_JB = destination.PK;
			sailing.JX_JA = origin.PK;
			Factory.Save();

			helper.FormatCRN("E");
			AssertEquals("Transport is not set", ZString.Empty, consol.Transports.MostInterestingTransport.JW_VoyageFlight);
			AssertEquals("Transport is not set", ZString.Empty, consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort);
			AssertEquals("Transport is not set", ZString.Empty, consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort);
			AssertEquals("Transport is not set", ZDateTime.Empty, consol.Transports.MostInterestingTransport.JW_ETA);
			AssertEquals("Transport is not set", ZDateTime.Empty, consol.Transports.MostInterestingTransport.JW_ATA);

			helper.FormatCRN("E1230808" + ((ZString)currentYear.ToString()).Right(1) + "AUSYD");
			AssertEquals("Transport is not set", airline.RM_TwoCharacterCode + "123", consol.Transports.MostInterestingTransport.JW_VoyageFlight);
			AssertEquals("Transport is not set", "ISREY", consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort);
			AssertEquals("Transport is not set", "AUSYD", consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort);
			AssertEquals("Transport is not set", eta, consol.Transports.MostInterestingTransport.JW_ETA);
			AssertEquals("Transport is not set", ata, consol.Transports.MostInterestingTransport.JW_ATA);
		}

		#endregion

		#region Implementation
		ConsolSendingarnumerHelper helper;
		ForwardingConsol consol;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			helper = new ConsolSendingarnumerHelper(consol);
		}

		OrgHeader GetTestOrgWithCarrierCode(ZString carrierCode)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode ccc = Factory.New<OrgCusCode>();
			ccc.OK_OH = result.PK;
			ccc.OK_RN_NKCodeCountry = Iceland.Code;
			ccc.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			ccc.OK_CustomsRegNo = carrierCode;
			return result;
		}

		#region Iceland
		RefCountry Iceland
		{
			get
			{
				if (iceland == null)
				{
					iceland = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iceland);
				}
				return iceland;
			}
		}
		RefCountry iceland;
		#endregion

		#endregion

	}
}

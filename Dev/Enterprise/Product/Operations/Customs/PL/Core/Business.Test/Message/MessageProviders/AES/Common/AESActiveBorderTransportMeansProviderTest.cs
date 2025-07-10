using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESActiveBorderTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<AESActiveBorderTransportMeansProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Declaration", "Value cannot be null.\r\nParameter name: declaration",
				() => new AESDepartureTransportMeansProvider(null, 1));
		});
	}

	public void TestTypeOfIdentification()
	{
		declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._21;
		AssertEquals("Type of Identification", ExportBorderTransportMeansList.Codes._21, GetProvider().TypeOfIdentification);
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_VesselName = "TestVessel";
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._11;
			AssertEquals("Identification Number is Vessel Name", "TestVessel", GetProvider().IdentificationNumber);

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
			declaration.JE_LloydsIMO = "7894450";
			AssertEquals("Identification Number is Lloyds IMO when Transport Mode is SEA and Type of Identification is 10", "7894450", GetProvider().IdentificationNumber);

			declaration.JE_TransportMode = TransportModes.OwnPropulsion;
			declaration.JE_VesselName = "TestVessel";
			AssertEquals("Identification Number is Vessel Name when Transport Mode is not SEA", "TestVessel", GetProvider().IdentificationNumber);

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._21;
			AssertEquals("Identification Number is Vessel Name capitalized", "TESTVESSEL", GetProvider().IdentificationNumber);

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._40;
			declaration.JE_VoyageFlightNo = "Flight123";
			AssertEquals("Identification Number is Voyage Flight Number capitalized", "FLIGHT123", GetProvider().IdentificationNumber);

			declaration.ZG_BorderTransportMeans = "";
			declaration.JE_VesselName = "Abc123";
			AssertEquals("Identification Number is Folio", "Abc123", GetProvider().IdentificationNumber);
		});
	}

	public void TestNationality()
	{
		CombineAssertions(() =>
		{
			declaration.JE_RN_NKTransportNationality = CountryCodes.France;
			vessel.RV_RN_NKCountryOfReg = CountryCodes.Poland;

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
			AssertEquals("Nationality is Vessel's country of registration - vessel lloyd number", CountryCodes.Poland, GetProvider().Nationality);

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._11;
			AssertEquals("Nationality is Vessel's country of registration - vessel name", CountryCodes.Poland, GetProvider().Nationality);

			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._21;
			AssertEquals("Nationality is declaration transport nationality", CountryCodes.France, GetProvider().Nationality);

			vessel.RV_RN_NKCountryOfReg = ZString.Empty;
			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			AssertEquals("No Nationality on Vessel level", CountryCodes.France, GetProvider().Nationality);
		});
	}

	protected override AESActiveBorderTransportMeansProvider GetProvider() => new AESActiveBorderTransportMeansProvider(declaration);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		vessel = Factory.New<RefVessel>();
		vessel.RV_Code = "TestVessel";
		declaration.JE_VesselName = vessel.RV_Code;
	}
	JobDeclaration declaration;
	RefVessel vessel;
}

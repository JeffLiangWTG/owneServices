using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESDepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<AESDepartureTransportMeansProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Declaration", "Value cannot be null.\r\nParameter name: declaration",
				() => new AESDepartureTransportMeansProvider(null, 1));
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals("Sequence Number", 999, GetProvider().SequenceNumber);
	}

	public void TestTypeOfIdentification()
	{
		declaration.JE_TransportMeans = "10";
		AssertEquals("Type of Identification", "10", GetProvider().TypeOfIdentification);
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			AssertNull("JE_TransportIDInland is empty", GetProvider().IdentificationNumber);

			declaration.JE_TransportIDInland = "zxc";
			AssertEquals("JE_TransportIDInland is not empty", "zxc", GetProvider().IdentificationNumber);

			declaration.JE_Trailer1RegNo = "asd";
			AssertEquals("JE_Trailer1RegNo is not empty but JE_TransportModeInland is not Road", "zxc", GetProvider().IdentificationNumber);

			declaration.JE_Trailer2RegNo = "qwe";
			AssertEquals("JE_Trailer1RegNo is not empty but JE_TransportModeInland is not Road", "zxc", GetProvider().IdentificationNumber);

			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
			AssertEquals("JE_TransportIDInland is not empty", "zxc/asd/qwe", GetProvider().IdentificationNumber);

			declaration.JE_Trailer1RegNo = ZString.Empty;
			AssertEquals("JE_Trailer1RegNo is empty", "zxc/qwe", GetProvider().IdentificationNumber);

			declaration.JE_Trailer2RegNo = ZString.Empty;
			AssertEquals("JE_Trailer2RegNo is empty", "zxc", GetProvider().IdentificationNumber);

			declaration.JE_TransportIDInland = ZString.Empty;
			declaration.JE_Trailer1RegNo = "asd";
			declaration.JE_Trailer2RegNo = "qwe";
			AssertEquals("JE_TransportIDInland is empty", "asd/qwe", GetProvider().IdentificationNumber);
		});
	}

	public void TestNationality()
	{
		var vessel = Factory.New<RefVessel>();
		vessel.RV_Code = "asd123";
		vessel.RV_LloydsNumber = "123456";
		vessel.RV_RN_NKCountryOfReg = CountryCodes.Poland;

		declaration.JE_RN_NKTransportNationalityInland = CountryCodes.France;

		CombineAssertions(() =>
		{
			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._10;
			declaration.JE_TransportIDInland = "123456";
			AssertEquals("Nationality is Vessel's country of registration - vessel lloyd number", CountryCodes.Poland, GetProvider().Nationality);

			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			declaration.JE_TransportIDInland = "asd123";
			AssertEquals("Nationality is Vessel's country of registration - vessel name", CountryCodes.Poland, GetProvider().Nationality);

			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._21;
			AssertEquals("Nationality is declaration transport nationality", CountryCodes.France, GetProvider().Nationality);

			vessel.RV_RN_NKCountryOfReg = ZString.Empty;
			declaration.JE_TransportMeans = ExportBorderTransportMeansList.Codes._11;
			AssertEquals("No Nationality on Vessel level", CountryCodes.France, GetProvider().Nationality);
		});
	}

	protected override AESDepartureTransportMeansProvider GetProvider() => new AESDepartureTransportMeansProvider(declaration, 999);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
	}
	JobDeclaration declaration;
}

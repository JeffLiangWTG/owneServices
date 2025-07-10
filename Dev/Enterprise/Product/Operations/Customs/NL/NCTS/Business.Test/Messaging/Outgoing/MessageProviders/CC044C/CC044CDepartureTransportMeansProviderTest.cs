using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CDepartureTransportMeansProvider))]
sealed class CC044CDepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<CC044CDepartureTransportMeansProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CDepartureTransportMeansProvider(null));

	public void TestSequenceNumeric()
	{
		transportMeans.TPM_TransportState = "XXX";
		transportMeans.TPM_SequenceNumber = 2;
		AssertEquals(2, Provider.SequenceNumeric);
	}

	public void TestTypeOfIdentification() => CombineAssertions(() =>
	{
		transportMeans.TPM_TypeOfIdentification = "3";

		transportMeans.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		AssertEquals(3, Provider.TypeOfIdentification);

		transportMeans.TPM_TransportState = "XXX";
		AssertEquals(null, Provider.TypeOfIdentification);

		transportMeans.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
		AssertEquals(null, Provider.TypeOfIdentification);
	});
	
	public void TestId()
	{
		transportMeans.TPM_TransportState = "XXX";
		transportMeans.TPM_IdentificationNumber = "DepTranMeansID";
		AssertEquals(string.Empty, Provider.Id);
	}

	public void TestNationality()
	{
		transportMeans.TPM_TransportState = "XXX";
		transportMeans.TPM_RN_NKTransportNationality = "NL";
		AssertEquals(string.Empty, Provider.Nationality);
	}

	public void TestSequenceNumber_NEW()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_SequenceNumber = 2;
		AssertEquals(2, Provider.SequenceNumeric);
	}

	public void TestTypeOfIdentification_NEW()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_TypeOfIdentification = "3";
		AssertEquals(3, Provider.TypeOfIdentification);
	}

	public void TestId_NEW()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_IdentificationNumber = "DepTranMeansID";
		AssertEquals("DepTranMeansID", Provider.Id);
	}

	public void TestNationality_NEW()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Netherlands;
		AssertEquals(Core.Constants.CountryCodes.Netherlands, Provider.Nationality);
	}

	public void TestSequenceNumeric_MIS()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
		transportMeans.TPM_SequenceNumber = 2;
		AssertEquals(2, Provider.SequenceNumeric);
	}

	public void TestTypeOfIdentification_MIS()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
		transportMeans.TPM_TypeOfIdentification = "3";
		AssertNull(Provider.TypeOfIdentification);
	}

	public void TestId_MIS()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
		transportMeans.TPM_IdentificationNumber = "DepTranMeansID";
		AssertEquals(string.Empty, Provider.Id);
	}

	public void TestNationality_MIS()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.MIS;
		transportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Belgium;
		AssertEquals(string.Empty, Provider.Nationality);
	}

	public void TestInvalidTypeOfIdentification()
	{
		transportMeans.TPM_TransportState = NctsUnloadedStateList.Codes.NEW;
		transportMeans.TPM_TypeOfIdentification = "t";
		AssertEquals(null, provider.TypeOfIdentification);
	}

	protected override CC044CDepartureTransportMeansProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		transportMeans = Factory.New<CusTransportMeans>();
		provider = new CC044CDepartureTransportMeansProvider(transportMeans);
	}

	CC044CDepartureTransportMeansProvider provider;
	CusTransportMeans transportMeans;
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GoodsShipmentConsignmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			NUnit.Framework.Assert.That(consignment.AdditionalInformations.Count(), NUnit.Framework.Is.EqualTo(0));
			header.TW_Notes = "TEST NOTES";
			NUnit.Framework.Assert.That(consignment.AdditionalInformations.Single().StatementDescription, NUnit.Framework.Is.EqualTo("TEST NOTES").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeans()
		{
			NUnit.Framework.Assert.That(consignment.BorderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.JE_VoyageFlightNo = "Flight123";
			NUnit.Framework.Assert.That(consignment.BorderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("Flight123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeans()
		{
			NUnit.Framework.Assert.That(consignment.DepartureTransportMeans, NUnit.Framework.Is.EqualTo(default(ITransportMeans)), "Should be null when VesselName is empty - should be [null]");
			declaration.JE_VesselName = "AS3245";
			NUnit.Framework.Assert.That(consignment.DepartureTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo("AS3245").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLoadingLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFALoadingPort, "ECFA Loading Port", Core.Constants.CountryCodes.Taiwan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECFALoadingPort, "TWXXX", "TAIWANG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";

			AssertLoadingLocation(CertificateTypeList.Codes.Code15, true, false, false, "Y");
			AssertLoadingLocation(CertificateTypeList.Codes.Code15, false, false, false, "N");

			foreach (var code in new List<ZString> { CertificateTypeList.Codes.Code1, CertificateTypeList.Codes.Code7, CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code10, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17 })
			{
				AssertLoadingLocation(code, false, false, true, ZString.Empty);
				AssertLoadingLocation(code, true, false, true, ZString.Empty);
			}
			AssertLoadingLocation(CertificateTypeList.Codes.Code2, false, true, true, ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestLoadingLocationName()
		{
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "TWXXX";
			uNLOCO.RL_PortName = "TAIWANG";

			declaration.JE_RL_NKOrigin = "TWXXX";
			declaration.JE_ExportDate = new ZDateTime(2023, 5, 5);
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			header.TW1_IsEstimatedLoadingDate = true;

			header.TW1_RL_NKPortOfLoading = "TWXXX";
			header.TW1_PortOfLoadingName = "TAI Test";
			NUnit.Framework.Assert.That(consignment.LoadingLocation.Name, NUnit.Framework.Is.EqualTo("TAI Test").Using(CustomComparers.TypeComparison));

			header.TW1_RL_NKPortOfLoading = "TWZ99";
			header.TW1_PortOfLoadingName = "Z99 Port";
			NUnit.Framework.Assert.That(consignment.LoadingLocation.Name, NUnit.Framework.Is.EqualTo("Z99 Port").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		void AssertLoadingLocation(ZString certificateType, ZBool isEstimatedLoadingDate, bool expectEmptyIDandName, bool expectEmptyLoadingDate, ZString expectEstimatedLoadingCode)
		{
			header.TW1_RL_NKPortOfLoading = "TWXXX";
			header.TW1_PortOfLoadingName = "TAIWANG";
			declaration.JE_ExportDate = new ZDateTime(2023, 5, 5);
			header.TW1_CertificateType = certificateType;
			header.TW1_IsEstimatedLoadingDate = isEstimatedLoadingDate;
			var loadingLocation = consignment.LoadingLocation;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(loadingLocation.ID, NUnit.Framework.Is.EqualTo(expectEmptyIDandName ? ZString.Empty : new ZString("TWXXX")));
				NUnit.Framework.Assert.That(loadingLocation.LoadingDateTime, NUnit.Framework.Is.EqualTo(expectEmptyLoadingDate ? ZDate.Empty : new ZDateTime(2023, 5, 5).Date));
				NUnit.Framework.Assert.That(loadingLocation.Name, NUnit.Framework.Is.EqualTo(expectEmptyIDandName ? ZString.Empty : new ZString("TAIWANG")));
				NUnit.Framework.Assert.That(loadingLocation.EstimatedLoadingCode, NUnit.Framework.Is.EqualTo(expectEstimatedLoadingCode));
			});
		}

		[ExpectNoExceptions]
		public void TestTransportEquipment()
		{
			NUnit.Framework.Assert.That(consignment.TransportEquipments.Count(), NUnit.Framework.Is.EqualTo(0));
			var testContainer1 = declaration.CusContainers.AddNew();
			testContainer1.CO_ContainerNumber = "CRXU1234569";
			var testContainer2 = declaration.CusContainers.AddNew();
			testContainer2.CO_ContainerNumber = "DRXU1234569";
			NUnit.Framework.Assert.That(consignment.TransportEquipments.Select(x => x.ID), NUnit.Framework.Is.EquivalentTo(new List<ZString> { "CRXU1234569", "DRXU1234569" }));
		}

		[ExpectNoExceptions]
		public void TestUnloadingLocation()
		{
			NUnit.Framework.Assert.That(consignment.UnloadingLocation.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			NUnit.Framework.Assert.That(consignment.UnloadingLocation.ID, NUnit.Framework.Is.EqualTo("AUSYD").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			goodsShipment = new NX101GoodsShipment(header);
			consignment = goodsShipment.Consignment;
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		NX101GoodsShipment goodsShipment;
		IConsignment consignment;
	}
}

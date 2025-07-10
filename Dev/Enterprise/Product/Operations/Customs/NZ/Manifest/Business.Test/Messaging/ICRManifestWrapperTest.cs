using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class ICRManifestWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new ICRManifestHeaderWrapper(null, null);
		}

		public void TestICRManifestWrapper()
		{
			AssertNotNull(new ICRManifestHeaderWrapper(manifestHeader, null));
		}

		public void TestSenderReferenceNumber()
		{
			manifestHeader.AMA_JobReference = "X12345678";
			AssertEquals("X12345678", wrappedManifestHeader.SenderReferenceNumber);
		}

		public void TestTSWReferenceNumber()
		{
			manifestHeader.AMA_MasterBill = "12345678";
			AssertEquals("12345678", wrappedManifestHeader.TSWReferenceNumber);
		}

		public void TestIsSea()
		{
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Assert(!wrappedManifestHeader.IsSea);
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			Assert(wrappedManifestHeader.IsSea);
		}

		public void TestIsCarrierCargoReport()
		{
			Assert(!wrappedManifestHeader.IsCarrierCargoReport);
		}

		public void TestCraftName()
		{
			manifestHeader.AMA_VesselName = "ABC";
			AssertEquals("ABC", wrappedManifestHeader.CraftName);
		}

		public void TestLloydsNo()
		{
			var testVessel = Factory.New<RefVessel>();
			testVessel.RV_Code = "VESSEL1";
			testVessel.RV_LloydsNumber = "9060443";
			manifestHeader.AMA_VesselName = "INVALID VESSEL";
			Assert(wrappedManifestHeader.LloydsNo.IsEmpty);
			manifestHeader.AMA_VesselName = "VESSEL1";
			AssertEquals("9060443", wrappedManifestHeader.LloydsNo);
		}

		public void TestVoyageNo()
		{
			manifestHeader.AMA_Voyage = "123";
			AssertEquals("123", wrappedManifestHeader.VoyageNo);
		}

		public void TestFlightNo()
		{
			manifestHeader.AMA_Voyage = "VU876";
			AssertEquals("VU876", wrappedManifestHeader.FlightNo);
		}

		public void TestArrivalDate()
		{
			var now = ZDateTime.Now;
			manifestHeader.AMA_E_ARV = now;
			AssertEquals(now, wrappedManifestHeader.ArrivalDate);
		}

		public void TestPortOfArrival()
		{
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "NZCHC";
			AssertEquals("NZCHC", wrappedManifestHeader.PortOfArrival);
		}

		public void TestCarrier()
		{
			AssertNull(wrappedManifestHeader.Carrier);
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "TEST CARRIER";
			manifestHeader.AMA_OA_Carrier = carrier.MainAddress.PK;
			AssertEquals("TEST CARRIER", wrappedManifestHeader.Carrier.Name);
			AssertSame(wrappedManifestHeader.Carrier, wrappedManifestHeader.Carrier);
		}

		public void TestDeclarant()
		{
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = currentStaff.GetNZWrapper();
			wrapper.NZBPassword.GP_UserID = "12345678E";
			Factory.Save();
			AssertEquals("12345678E", wrappedManifestHeader.Declarant.DeclarantID);
			AssertSame(wrappedManifestHeader.Declarant, wrappedManifestHeader.Declarant);
		}

		public void TestAdditionalInformation()
		{
			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Original, Factory, MessageTypeList.Codes.CRE);
			wrappedManifestHeader = new ICRManifestHeaderWrapper(manifestHeader, additionalMessageInformation);
			AssertSame(additionalMessageInformation, wrappedManifestHeader.AdditionalInformation);
		}

		public void TestUseInterfaceSequenceNumber()
		{
			AssertEquals(ZBool.False, wrappedManifestHeader.UseInterfaceSequenceNumber);
		}

		public void TestConsignments()
		{
			manifestHeader.Bills.AddNew();
			Assert(wrappedManifestHeader.Consignments.IsCountEqualTo(1));
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, wrappedManifestHeader.SupportingDocuments.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = (AsycudaManifestHeader)AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			wrappedManifestHeader = new ICRManifestHeaderWrapper(manifestHeader, null);
		}

		AsycudaManifestHeader manifestHeader;
		IInwardCargoReport wrappedManifestHeader;
	}
}

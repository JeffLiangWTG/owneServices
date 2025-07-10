using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestHeader))]
	sealed class ExportCustomsManifestHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLines()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			AssertNotNull(header.Lines);
		}

		public void TestMessages()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			AssertNotNull(header.Messages);
		}

		public void TestBGMReferenceAssignedOnSaving()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			Assert("ED_BGMReference Not Assigned", header.ED_BGMReference.IsEmpty);
			header.Factory.Save();
			Assert("ED_BGMReference Assigned", !header.ED_BGMReference.IsEmpty);
		}

		public void TestMessageStatus()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			AssertEquals("No Messages Sent", header.MessageStatus);
			header.Messages.AddNew(typeof(EDIMessage)).EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Assert("Waiting for response", header.MessageStatus.IndexOf("Waiting for ") != -1);
			header.Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Assert("Response received", header.MessageStatus.IndexOf("Response for ") != -1);
		}

		public void TestCountryOfDestinationSetWhenPortOfDestinationSet()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			header.ED_RL_NKPortOfDestination = "NZAKL";
			AssertEquals("NZ", header.ED_RN_NKCountryOfDestination);
		}

		public void TestMessagesIncludingInterchangeRejections()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			AssertNotNull(header.MessagesIncludingInterchangeRejections);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			if (GetType() == typeof(ExportCustomsManifestHeaderTest))
			{
				Assert($"Covered by {nameof(FetchStrategies.Testing.ExportCustomsManifestHeaderFetchStrategyTest)}.", true);
				return;
			}

			base.TestCalcPropertiesWithDbHitsUseFetchHints();
		}

		public void TestRefVessel_LoadUsingNaturalKey_NullIfDuplicatesFound()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var exportManifest = Factory.New<ExportCustomsManifestHeader>();
			exportManifest.ED_TransportMode = Core.Constants.TransportModes.Sea;
			exportManifest.ED_VesselName = vessel1.RV_Name;
			AssertNotNull("Vessel should return", exportManifest.Vessel);
			AssertSame("Vessel", exportManifest.Vessel, vessel1);

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "DUPLICATE VESSEL";
			exportManifest.ED_VesselName = "DUPLICATE VESSEL";
			AssertNull("Duplicate vessel should return as null", exportManifest.Vessel);
		}

		public void TestCheckED_VesselName_HasDuplicates()
		{
			const string expectedMessageError = "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel.";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var exportManifest = Factory.New<ExportCustomsManifestHeader>();
			exportManifest.ED_TransportMode = Core.Constants.TransportModes.Sea;
			exportManifest.ED_VesselName = vessel1.RV_Name;

			CombineAssertions(() =>
			{
				AssertNoMessageError("No duplicate", exportManifest.ED_VesselNameInfo, expectedMessageError);

				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				exportManifest.Validation.ValidateED_VesselName();
				AssertHasMessageError("Vessel Name has Duplicates", exportManifest.ED_VesselNameInfo, expectedMessageError);
			});
		}

		public void TestED_VesselName_DefaultsLloydsIMOForOneMatch()
		{
			SetupTestVessels();
			var exportManifest = Factory.NewWithValidTestData<ExportCustomsManifestHeader>();
			AssertEquals("Pre-condition", "", exportManifest.ED_LloydsIMO);

			exportManifest.ED_VesselName = "HYOGO MARU";
			AssertEquals("ED_LloydsIMO should have populated from unique Vessel", "4567123", exportManifest.ED_LloydsIMO);

			exportManifest.ED_VesselName = "HYOGO KENU";
			AssertEquals("ED_LloydsIMO should have re-populated from the current unique Vessel", "6712345", exportManifest.ED_LloydsIMO);

			exportManifest.ED_VesselName = "VESSEL NO LLOYDS";
			AssertEquals("ED_LloydsIMO should have been cleared out for this Vessel", "", exportManifest.ED_LloydsIMO);

			exportManifest.ED_VesselName = "HYOGO MARU";
			AssertEquals("ED_LloydsIMO should have populated from Vessel", "4567123", exportManifest.ED_LloydsIMO);

			exportManifest.ED_VesselName = "Invalid Vessel";
			AssertEquals("ED_LloydsIMO should be cleared out if the vessel is not found", "", exportManifest.ED_LloydsIMO);
		}

		public void TestCheckED_VesselName_MessageErrorOnDuplicate()
		{
			const string expectedMessageError = "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel.";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DUPLICATE VESSEL";

			var exportManifest = Factory.NewWithValidTestData<ExportCustomsManifestHeader>();
			exportManifest.ED_TransportMode = Core.Constants.TransportModes.Sea;
			exportManifest.ED_VesselName = vessel1.RV_Name;

			CombineAssertions(() =>
			{
				AssertNoMessageError("No duplicate", exportManifest.ED_VesselNameInfo, expectedMessageError);
				var vessel2 = Factory.NewWithValidTestData<RefVessel>();
				vessel2.RV_Name = "DUPLICATE VESSEL";
				exportManifest.Validation.ValidateED_VesselName();
				AssertHasMessageError("Has Duplicate", exportManifest.ED_VesselNameInfo, expectedMessageError);
			});
		}

		#region Implementation

		void SetupTestVessels()
		{
			testVessel1 = Factory.NewWithValidTestData<RefVessel>();
			testVessel1.RV_Name = "HYOGO MARU";
			testVessel1.RV_LloydsNumber = "4567123";

			testVessel2 = Factory.NewWithValidTestData<RefVessel>();
			testVessel2.RV_Name = "HYOGO KENU";
			testVessel2.RV_LloydsNumber = "6712345";

			testVessel3 = Factory.NewWithValidTestData<RefVessel>();
			testVessel3.RV_Name = "VESSEL NO LLOYDS";
			testVessel3.RV_LloydsNumber = "";

			testVessel4 = Factory.NewWithValidTestData<RefVessel>();
			testVessel4.RV_Name = "DUP_VESS";
			testVessel4.RV_LloydsNumber = "4984731";

			// TODO: implement when unique key constraint is removed from Reference file:

			//TestVessel5 = Factory.NewWithValidTestData<RefVessel>();
			//TestVessel5.RV_Name = "DUP_VESS";
			//TestVessel5.RV_LloydsNumber = "";

			//TestVessel6 = Factory.NewWithValidTestData<RefVessel>();
			//TestVessel6.RV_Name = "DUP_VESS";
			//TestVessel6.RV_LloydsNumber = "5738219";

			Factory.Save();
		}

		RefVessel testVessel1;
		RefVessel testVessel2;
		RefVessel testVessel3;
		RefVessel testVessel4;
		//RefVessel TestVessel5;
		//RefVessel TestVessel6;

		#endregion
	}
}

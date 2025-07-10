using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class StatusControlTest : TestCaseWithFactory
	{
		public void TestSetupGrid()
		{
			var uSDeclarationForTest = Factory.New<JobDeclaration>();
			var tRDeclaration = new TrackingDeclaration(uSDeclarationForTest);

			var testControl = new USImportTestStatusControl();
			testControl.SetUpControlForTest();
			testControl.BindTo = "Declaration";
			testControl.Bind(tRDeclaration);

			uSDeclarationForTest.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			testControl.SetUpGrids();
			Assert(!testControl.ForTest_PGAStatusGrid.Visible);
			Assert(testControl.ForTest_PGALineStatusGrid.Visible);

			uSDeclarationForTest.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			testControl.SetUpGrids();
			Assert(!testControl.ForTest_PGAStatusGrid.Visible);
			Assert(testControl.ForTest_PGALineStatusGrid.Visible);

			uSDeclarationForTest.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			testControl.SetUpGrids();
			Assert(!testControl.ForTest_PGAStatusGrid.Visible);
			Assert(testControl.ForTest_PGALineStatusGrid.Visible);

			uSDeclarationForTest.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			testControl.SetUpGrids();
			Assert(testControl.ForTest_PGAStatusGrid.Visible);
			Assert(!testControl.ForTest_PGALineStatusGrid.Visible);

			uSDeclarationForTest.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			testControl.SetUpGrids();
			Assert(testControl.ForTest_PGAStatusGrid.Visible);
			Assert(!testControl.ForTest_PGALineStatusGrid.Visible);

			uSDeclarationForTest.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			testControl.SetUpGrids();
			Assert(testControl.ForTest_PGAStatusGrid.Visible);
			Assert(!testControl.ForTest_PGALineStatusGrid.Visible);
		}

		public void TestSetupControl()
		{
			JobDeclaration uSDeclarationForTest = Factory.New<JobDeclaration>();
			TrackingDeclaration tRDeclaration = new TrackingDeclaration(uSDeclarationForTest);

			USImportTestStatusControl testControl = new USImportTestStatusControl();
			testControl.SetUpControlForTest();
			testControl.BindTo = "Declaration";
			testControl.Bind(tRDeclaration);

			Assert(!testControl.ForTest_AreaAIIStatus.Visible);
			Assert(!testControl.ForTest_AreaBillOfLadingUpdateStatus.Visible);
			Assert(!testControl.ForTest_AreaITStatus.Visible);

			uSDeclarationForTest.US_EnableAII = ZBool.True;
			testControl.SetupControl();
			Assert(testControl.ForTest_AreaAIIStatus.Visible);
			Assert(!testControl.ForTest_AreaBillOfLadingUpdateStatus.Visible);
			Assert(!testControl.ForTest_AreaITStatus.Visible);

			uSDeclarationForTest.US_EnableAII = ZBool.False;
			uSDeclarationForTest.BLUStatus = "AIO";
			testControl.SetupControl();
			Assert(!testControl.ForTest_AreaAIIStatus.Visible);
			Assert(testControl.ForTest_AreaBillOfLadingUpdateStatus.Visible);
			Assert(!testControl.ForTest_AreaITStatus.Visible);

			uSDeclarationForTest.BLUStatus = ZString.Empty;

			uSDeclarationForTest.JE_MessageType = Customs.Common.US.USJobMessageTypeList.Codes.Import;
			CusEntryHeader entry = uSDeclarationForTest.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MQEDIMessage outMessage = Factory.New<MQEDIMessage>();
			outMessage.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport;
			outMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			outMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			outMessage.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";
			outMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureOriginal;
			entry.Messages.Add(outMessage);

			testControl.SetupControl();
			Assert(!testControl.ForTest_AreaAIIStatus.Visible);
			Assert(!testControl.ForTest_AreaBillOfLadingUpdateStatus.Visible);
			Assert(testControl.ForTest_AreaITStatus.Visible);
		}
	}
}

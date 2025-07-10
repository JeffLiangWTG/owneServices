namespace Enterprise.Customs.NZ.Business.Express.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;

	public class DTRTranshipmentRequestTemplateTest : TestCaseWithFactory
	{
		public void TestNeverSaves()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, transhipmentRequestTemplate.IsSavedByFactory);
				transhipmentRequestTemplate.FillWithValidTestData();
				AssertEquals(false, transhipmentRequestTemplate.IsSavedByFactory);
				transhipmentRequestTemplate.Factory.Save();
				AssertEquals(false, transhipmentRequestTemplate.IsInDatabase);
			});
		}

		public void TestGeneratesPersistentBOs()
		{
			CombineAssertions(() =>
			{
				transhipmentRequestTemplate.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
				transhipmentRequestTemplate.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				transhipmentRequestTemplate.C4_RL_NKTranshipDestPort = "NZWLG";
				transhipmentRequestTemplate.C4_OA_DestinationAddress = ZGuid.BrettsGuid;

				var newTR = transhipmentRequestTemplate.Generate();
				AssertEquals("Cloned object is not of template type", false, newTR is DTRTranshipmentRequestTemplate);
				AssertEquals("Cloned object is ready to be saved", true, newTR.IsSavedByFactory);
				AssertEquals("Cloned:", "DTR", newTR.C4_MovementReason);
				AssertEquals("Cloned:", transhipmentRequestTemplate.C4_ModeOfMovement, newTR.C4_ModeOfMovement);
				AssertEquals("Cloned:", transhipmentRequestTemplate.C4_TranshipModeOfMovement, newTR.C4_TranshipModeOfMovement);
				AssertEquals("Cloned:", transhipmentRequestTemplate.C4_RL_NKTranshipDestPort, newTR.C4_RL_NKTranshipDestPort);
				AssertEquals("Cloned:", transhipmentRequestTemplate.C4_OA_DestinationAddress, newTR.C4_OA_DestinationAddress);

				newTR = TranshipmentRequest.Create(Factory.New<CusHAWB>());
				newTR.OnSaving();
				var parentID = newTR.C4_ParentID;
				var parentTable = newTR.C4_ParentTableCode;
				var c4_SendersMessageReference = newTR.C4_SendersMessageReference;
				newTR.C4_MovementReason = "ITR";
				transhipmentRequestTemplate.Populate(newTR);
				AssertEquals("Copied object is not of template type", false, newTR is DTRTranshipmentRequestTemplate);
				AssertEquals("Copied object is ready to be saved", true, newTR.IsSavedByFactory);
				AssertEquals("Not copied from template:", parentID, newTR.C4_ParentID);
				AssertEquals("Not copied from template:", parentTable, newTR.C4_ParentTableCode);
				AssertEquals("Not copied from template:", c4_SendersMessageReference, newTR.C4_SendersMessageReference);
				AssertEquals("Copied:", "DTR", newTR.C4_MovementReason);
				AssertEquals("Copied:", transhipmentRequestTemplate.C4_ModeOfMovement, newTR.C4_ModeOfMovement);
				AssertEquals("Copied:", transhipmentRequestTemplate.C4_TranshipModeOfMovement, newTR.C4_TranshipModeOfMovement);
				AssertEquals("Copied:", transhipmentRequestTemplate.C4_RL_NKTranshipDestPort, newTR.C4_RL_NKTranshipDestPort);
				AssertEquals("Copied:", transhipmentRequestTemplate.C4_OA_DestinationAddress, newTR.C4_OA_DestinationAddress);
			});
		}

		public void TestInitialValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DTR", transhipmentRequestTemplate.C4_MovementReason);
				AssertNullOrEmpty("C4_ModeOfMovement", transhipmentRequestTemplate.C4_ModeOfMovement);
				AssertNullOrEmpty("C4_TranshipModeOfMovement", transhipmentRequestTemplate.C4_TranshipModeOfMovement);
				AssertNullOrEmpty("C4_RL_NKTranshipDestPort", transhipmentRequestTemplate.C4_RL_NKTranshipDestPort);
				AssertEquals(ZGuid.Empty, transhipmentRequestTemplate.C4_OA_DestinationAddress);
			});
		}

		public void TestLookups()
		{
			AssertType<TranshipmentRequestLookups>(transhipmentRequestTemplate.Lookups);
		}

		public void TestValidation()
		{
			AssertType<TranshipmentRequestValidation>(transhipmentRequestTemplate.Validation);
		}

		public void TestDTRReadOnly()
		{
			AssertEquals(true, transhipmentRequestTemplate.C4_MovementReasonInfo.ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusMAWB mAWB = Factory.New<CusMAWB>();
			transhipmentRequestTemplate = DTRTranshipmentRequestTemplate.GetInstance(mAWB);
		}
		DTRTranshipmentRequestTemplate transhipmentRequestTemplate;
	}

	public class DTRTranshipmentRequestTemplateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTransferTransportModeValidation()
		{
			CombineAssertions(() =>
			{
				trTemplate.Validation.ValidateC4_ModeOfMovement();
				AssertHasMessageErrorContaining(trTemplate.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);
				trTemplate.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				AssertNoMessageErrorContaining(trTemplate.C4_ModeOfMovementInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestTransitDestinationValidation_Air()
		{
			const string message = "Transit Destination Premise code is mandatory for DTR.";
			CombineAssertions(() =>
			{
				trTemplate.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				trTemplate.Validation.ValidateC4_OA_DestinationAddress();
				AssertHasMessageErrorContaining(trTemplate.C4_OA_DestinationAddressInfo, message);

				trTemplate.C4_OA_DestinationAddress = Factory.New<OrgHeader>().Addresses.AddNewMainAddress().PK;
				trTemplate.Validation.ValidateC4_OA_DestinationAddress();
				AssertNoMessageErrorContaining(trTemplate.C4_OA_DestinationAddressInfo, message);
			});
		}

		public void TestTransitDestinationValidation_Sea()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			oceanBill.CB_RL_NKPortOfDischarge = "NZWLG";
			oceanBill.CB_RL_NKPortOfLoading = "FJSUV";
			var trTemplate = DTRTranshipmentRequestTemplate.GetInstance(oceanBill);

			const string message = "Transit Destination Premise code is mandatory for DTR.";
			CombineAssertions(() =>
			{
				trTemplate.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Sea;
				trTemplate.Validation.ValidateC4_OA_DestinationAddress();
				AssertHasMessageErrorContaining(trTemplate.C4_OA_DestinationAddressInfo, message);
				trTemplate.Validation.ValidateC4_RL_NKTranshipDestPort();
				AssertHasMessageErrorContaining(trTemplate.C4_RL_NKTranshipDestPortInfo, message);

				trTemplate.C4_OA_DestinationAddress = Factory.New<OrgHeader>().Addresses.AddNewMainAddress().PK;
				trTemplate.Validation.ValidateC4_OA_DestinationAddress();
				AssertNoMessageErrorContaining(trTemplate.C4_OA_DestinationAddressInfo, message);
				trTemplate.Validation.ValidateC4_RL_NKTranshipDestPort();
				AssertNoMessageErrorContaining(trTemplate.C4_RL_NKTranshipDestPortInfo, message);

				trTemplate.C4_OA_DestinationAddress = ZGuid.Empty;
				trTemplate.C4_RL_NKTranshipDestPort = "NZWLG";
				trTemplate.Validation.ValidateC4_OA_DestinationAddress();
				AssertNoMessageErrorContaining(trTemplate.C4_OA_DestinationAddressInfo, message);
				trTemplate.Validation.ValidateC4_RL_NKTranshipDestPort();
				AssertNoMessageErrorContaining(trTemplate.C4_RL_NKTranshipDestPortInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			mAWB.CM_RL_NKDischargePort = "NZWLG";
			mAWB.CM_RL_NKLoadPort = "FJSUV";
			trTemplate = DTRTranshipmentRequestTemplate.GetInstance(mAWB);
		}
		DTRTranshipmentRequestTemplate trTemplate;
	}
}

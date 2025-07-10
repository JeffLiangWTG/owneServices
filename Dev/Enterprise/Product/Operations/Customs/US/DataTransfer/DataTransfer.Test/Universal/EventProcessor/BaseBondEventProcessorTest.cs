using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using EDIMessage = Enterprise.Customs.US.Business.EDIMessage;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class BaseBondEventProcessorTest : TestCaseWithFactory
	{
		public void TestEmailInfo()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "C1234578";
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CVB;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GS_NKCusAgent = "Z8";
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                               286                  " + "B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " + "B2THIS IS A TEST                                                                " + "10B93A30000012345112614123456789112014121514000000009 NY                        " + "10B93A30000012345112614123456789112014121514000000009 NY                        " + "10B93A30000012345112614123456789112014121514000000009 NY                        " + "10B93A30000012345112614123456789112014121514000000009 NY                        " + "12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " + "12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " + "20107XJ5C1234578                                                                " + "3034 NN-NNNNNNNXXPrincipal Name                                                 " + "35EI YYDDPP-NNNNNCo-principal Name1                                             " + "35EI YYDDPP-NNNNNCo-principal Name2                                             " + "35EI YYDDPP-NNNNNCo-principal Name3                                             " + "36ANI111-NN-NNNN Bond User Name                          D112714121014          " + "36ANI222-NN-NNNN Bond User Name                          D112714121014          " + "36ANI333-NN-NNNN Bond User Name                          D112714121014          " + "36ANI444-NN-NNNN Bond User Name                          D112714121014          " + "36ANI555-NN-NNNN Bond User Name                          D112714121014          " + "40123777-88-9999Surety Name                              0000000005             " + "45111111-88-9999Surety Name                              0000000005             " + "45222222-88-9999Surety Name                              0000000005             " + "45333333-88-9999Surety Name                              0000000005             " + "Y  8888XJ5WR00005";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			AssertNotNull(email);
			AssertContains("Subject", "Customs eBond Status Notification", email.Subject);
			AssertContains("Subject", declaration.JobNumber, email.Subject);
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class BondStatusNotificationProcessorTest : ABIProcessorTest<BondStatusNotificationMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B018888XJ5BS                                               286                  B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          B2THIS IS A TEST                                                                10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          20107XJ5C12345780                                                               3034 NN-NNNNNNNXXPrincipal Name                                                 35EI YYDDPP-NNNNNCo-principal Name1                                             35EI YYDDPP-NNNNNCo-principal Name2                                             35EI YYDDPP-NNNNNCo-principal Name3                                             36ANI111-NN-NNNN Bond User Name                          D112714121014          36ANI222-NN-NNNN Bond User Name                          D112714121014          36ANI333-NN-NNNN Bond User Name                          D112714121014          36ANI444-NN-NNNN Bond User Name                          D112714121014          36ANI555-NN-NNNN Bond User Name                          D112714121014          40123777-88-9999Surety Name                              0000000005             46789111-22-3333Surety Name                                                     Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("new meessage", universalEventMessage);
		}

		public void TestProcessTransactionIDWithCorrectLength()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B018888XJ5BS                                               286                  B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          B2THIS IS A TEST                                                                10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          20107000000000000000000000000XJ512345780                                        3034 NN-NNNNNNNXXPrincipal Name                                                 35EI YYDDPP-NNNNNCo-principal Name1                                             35EI YYDDPP-NNNNNCo-principal Name2                                             35EI YYDDPP-NNNNNCo-principal Name3                                             36ANI111-NN-NNNN Bond User Name                          D112714121014          36ANI222-NN-NNNN Bond User Name                          D112714121014          36ANI333-NN-NNNN Bond User Name                          D112714121014          36ANI444-NN-NNNN Bond User Name                          D112714121014          36ANI555-NN-NNNN Bond User Name                          D112714121014          40123777-88-9999Surety Name                              0000000005             46789111-22-3333Surety Name                                                     Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("new meessage", universalEventMessage);
			message.Reload();
			AssertEquals("000000000000000000000000XJ512345780", message.EM_ApplicationReference);
		}

		public void TestProcessTransactionIDWhenTransactionIDTypeIsEntryNumber()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B018888XJ5BS                                               286                  B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          B2THIS IS A TEST                                                                10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          2010700000000000000000000000000000XJ512345780                                   3034 NN-NNNNNNNXXPrincipal Name                                                 35EI YYDDPP-NNNNNCo-principal Name1                                             35EI YYDDPP-NNNNNCo-principal Name2                                             35EI YYDDPP-NNNNNCo-principal Name3                                             36ANI111-NN-NNNN Bond User Name                          D112714121014          36ANI222-NN-NNNN Bond User Name                          D112714121014          36ANI333-NN-NNNN Bond User Name                          D112714121014          36ANI444-NN-NNNN Bond User Name                          D112714121014          36ANI555-NN-NNNN Bond User Name                          D112714121014          40123777-88-9999Surety Name                              0000000005             46789111-22-3333Surety Name                                                     Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("new meessage", universalEventMessage);
			message.Reload();
			AssertEquals("XJ512345780", message.EM_ApplicationReference);
		}

		public void TestProcessTransactionIDWhenTransactionIDTypeIsISF()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B018888XJ5BS                                               286                  B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          B2THIS IS A TEST                                                                10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          2020700000000000000000000000000000XJ512345780                                   3034 NN-NNNNNNNXXPrincipal Name                                                 35EI YYDDPP-NNNNNCo-principal Name1                                             35EI YYDDPP-NNNNNCo-principal Name2                                             35EI YYDDPP-NNNNNCo-principal Name3                                             36ANI111-NN-NNNN Bond User Name                          D112714121014          36ANI222-NN-NNNN Bond User Name                          D112714121014          36ANI333-NN-NNNN Bond User Name                          D112714121014          36ANI444-NN-NNNN Bond User Name                          D112714121014          36ANI555-NN-NNNN Bond User Name                          D112714121014          40123777-88-9999Surety Name                              0000000005             46789111-22-3333Surety Name                                                     Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("new meessage", universalEventMessage);
			message.Reload();
			AssertEquals("0000XJ512345780", message.EM_ApplicationReference);
		}

		public void TestProcessTransactionIDWhenTransactionIDTypeIsSeizure()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B018888XJ5BS                                               286                  B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          B2THIS IS A TEST                                                                10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          2030700000000000000000000000000000XJ512345780                                   3034 NN-NNNNNNNXXPrincipal Name                                                 35EI YYDDPP-NNNNNCo-principal Name1                                             35EI YYDDPP-NNNNNCo-principal Name2                                             35EI YYDDPP-NNNNNCo-principal Name3                                             36ANI111-NN-NNNN Bond User Name                          D112714121014          36ANI222-NN-NNNN Bond User Name                          D112714121014          36ANI333-NN-NNNN Bond User Name                          D112714121014          36ANI444-NN-NNNN Bond User Name                          D112714121014          36ANI555-NN-NNNN Bond User Name                          D112714121014          40123777-88-9999Surety Name                              0000000005             46789111-22-3333Surety Name                                                     Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("new meessage", universalEventMessage);
			message.Reload();
			AssertEquals("00000XJ512345780", message.EM_ApplicationReference);
		}

		public void TestProcessTransactionIDWithOtherTransactionIDTypes()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = "B018888XJ5BS                                               286                  B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          B2THIS IS A TEST                                                                10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        10B93A30000012345112614123456789112014121514000000009 NY                        12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          20407000000000000000000000000000000XJ51234570                                   3034 NN-NNNNNNNXXPrincipal Name                                                 35EI YYDDPP-NNNNNCo-principal Name1                                             35EI YYDDPP-NNNNNCo-principal Name2                                             35EI YYDDPP-NNNNNCo-principal Name3                                             36ANI111-NN-NNNN Bond User Name                          D112714121014          36ANI222-NN-NNNN Bond User Name                          D112714121014          36ANI333-NN-NNNN Bond User Name                          D112714121014          36ANI444-NN-NNNN Bond User Name                          D112714121014          36ANI555-NN-NNNN Bond User Name                          D112714121014          40123777-88-9999Surety Name                              0000000005             46789111-22-3333Surety Name                                                     Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("new meessage", universalEventMessage);
			message.Reload();
			AssertEquals("XJ51234570", message.EM_ApplicationReference);
		}

		public void TestUpdateBondExpiryDate()
		{
			var org01 = Factory.New<OrgHeader>();
			var org02 = Factory.New<OrgHeader>();
			var org03 = Factory.New<OrgHeader>();

			org01.OH_Code = "ORG01";
			org02.OH_Code = "ORG02";
			org03.OH_Code = "ORG03";

			var bond11 = org01.CusBondDetails.AddNew();
			var bond12 = org01.CusBondDetails.AddNew();
			var bond21 = org02.CusBondDetails.AddNew();
			var bond22 = org02.CusBondDetails.AddNew();
			var bond31 = org03.CusBondDetails.AddNew();
			var bond32 = org03.CusBondDetails.AddNew();

			bond11.PW_BondNumber = "BONDNUM01";
			bond12.PW_BondNumber = "BONDNUM01";
			bond21.PW_BondNumber = "BONDNUM01";
			bond22.PW_BondNumber = "BONDNUM02";
			bond31.PW_BondNumber = "BONDNUM01";
			bond32.PW_BondNumber = "BONDNUM01";

			bond11.PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;
			bond12.PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;
			bond21.PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;
			bond22.PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;
			bond31.PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;

			bond11.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bond12.PW_BondType = BondTypeList.Codes.SingleTransactionBond;
			bond21.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bond22.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bond31.PW_BondType = BondTypeList.Codes.ContinuousBond;
			bond32.PW_BondType = BondTypeList.Codes.ContinuousBond;

			bond11.PW_BondExpiryDate = new ZDateTime(2020, 01, 01);
			bond12.PW_BondExpiryDate = new ZDateTime(2020, 01, 02);
			bond21.PW_BondExpiryDate = new ZDateTime(2020, 02, 01);
			bond22.PW_BondExpiryDate = new ZDateTime(2020, 02, 02);
			bond31.PW_BondExpiryDate = new ZDateTime(2020, 03, 01);
			bond32.PW_BondExpiryDate = new ZDateTime(2020, 03, 02);

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;

			message.EM_MessageText =
				"B018888XJ5BS                                                                    " +
				"B1BONDNUM01 ETS CONT BOND IS SCHEDULED TO BE TERMINATED  1 0310201529Y          " +
				"10T8             031120         031220031320BONDNUM01                           " +
				"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			bond11.Reload();
			bond12.Reload();
			bond21.Reload();
			bond22.Reload();
			bond31.Reload();
			bond32.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("Updated", "13-Mar-20 00:00:00", bond11.PW_BondExpiryDate.ToString());
				AssertEquals("Not Updated - Bond Type Mismatch", "02-Jan-20 00:00:00", bond12.PW_BondExpiryDate.ToString());
				AssertEquals("Updated", "13-Mar-20 00:00:00", bond21.PW_BondExpiryDate.ToString());
				AssertEquals("Not Updated - Bond Number Mismatch", "02-Feb-20 00:00:00", bond22.PW_BondExpiryDate.ToString());
				AssertEquals("Updated", "13-Mar-20 00:00:00", bond31.PW_BondExpiryDate.ToString());
				AssertEquals("Not Updated - Application Code Mismatch", "02-Mar-20 00:00:00", bond32.PW_BondExpiryDate.ToString());
			});
		}

		public void TestReInsurerInformation()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;

			message.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2107XJ5C12345780                            520085                             " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"20107XJ512345780                                                                " +
				"3034 NN-NNNNNNNXXPrincipal Name                                                 " +
				"35EI YYDDPP-NNNNNCo-principal Name1                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name2                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name3                                             " +
				"36ANI111-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI222-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI333-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI444-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI555-NN-NNNN Bond User Name                          D112714121014          " +
				"40123777-88-9999Surety Name                              0000000005             " +
				"45111111-88-9999Surety Name                              0000000005             " +
				"45222222-88-9999Surety Name                              0000000005             " +
				"45333333-88-9999Surety Name                              0000000005             " +
				"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => !string.IsNullOrEmpty(x.Body));
			AssertNotNull(email);
			var emailtext = email.Body;

			message.Reload();
			AssertEquals("XJ512345780", message.EM_ApplicationReference);
			AssertEquals(EM_MessageSubTypeList.Codes.eBondStatusUpdate, message.EM_MessageSubType);

			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("new meessage", universalEventMessage);

			var universalEventMessageText = universalEventMessage.EM_MessageText;
			AssertContains("<Department>CBP</Department>", universalEventMessageText);
			AssertContains("<MessageType>BS</MessageType>", universalEventMessageText);
			AssertContains("<EventReference>~15000</EventReference>", universalEventMessageText);
			AssertContains("<Type>EntryNumber</Type>", universalEventMessageText);
			AssertContains("<Value>XJ512345780</Value>", universalEventMessageText);
			AssertContains("<Type>EntryNumberType</Type>", universalEventMessageText);
			AssertContains("<Value>07</Value>", universalEventMessageText);

			AssertContains("<td>Disposition Code</td><td>ENB / NEW BOND HAS BEEN ADDED IN ACE</td>", emailtext);
			AssertContains("<td>Bond Number</td><td>123456789</td>", emailtext);
			AssertContains("<td>Date And Time of Action</td><td>27-Nov-14 12:30:00</td>", emailtext);
			AssertContains("<td>Re-send Indicator</td><td>Notification has been re-sent</td>", emailtext);
			AssertContains("<td>Source of Action</td><td>Action by CBP - Office of Administration or An Automated ACE Process</td>", emailtext);

			AssertContains("<td>Transaction ID Type</td><td>1</td>", emailtext);
			AssertContains("<td>Drawback Claim Amount</td><td>520085</td>", emailtext);
			AssertContains("<td>Entry Type Code</td><td>07</td>", emailtext);
			AssertContains("<td>Transaction ID</td><td>XJ5C12345780</td>", emailtext);
			AssertContains("<td>Single transaction bond</td>", emailtext);
			AssertContains("<td>Carrier of International Traffic</td>", emailtext);
			AssertContains("<td>Remove flag for reconciliation</td>", emailtext);
			AssertContains("<td>Flag for importation into the U.S. Virgin Islands</td>", emailtext);

			AssertContains("<td>Social Security Number</td>", emailtext);
			AssertContains("<td>Employer Identification Number (IRS #)</td>", emailtext);
			AssertContains("<td>CBP-assigned Number</td>", emailtext);
			AssertContains("<td>Rider to Delete User from the Bond</td>", emailtext);

			AssertContains("enb", email.Subject.ToLower());

			AssertContains("<Type>DesignationType</Type>", universalEventMessageText);
			AssertContains("<Value>B</Value>", universalEventMessageText);

			AssertContains("<Type>PrincipalName</Type>", universalEventMessageText);
			AssertContains("<Value>PRINCIPAL NAME</Value>", universalEventMessageText);

			AssertContains("<Type>DispositionCode</Type>", universalEventMessageText);
			AssertContains("<Value>ENB</Value>", universalEventMessageText);
		}

		public void TestAdjustTheBondAmount()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2THIS IS A TEST                                                                " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"40123777-88-9999SURETY NAME                              000000005              " +
				"46789111-22-3333SURETY NAME                                                     " +
				"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => !string.IsNullOrEmpty(x.Body));
			AssertNotNull(email);

			var emailtext = email.Body;

			AssertContains("<td>Surety Code</td><td>123</td>", emailtext);
			AssertContains("<td>Agent ID Number</td><td>777-88-9999</td>", emailtext);
			AssertContains("<td>Surety Liability Amount</td><td>5</td>", emailtext);

			AssertContains("<td>Surety Code for Re-insurer</td><td>789</td>", emailtext);
			AssertContains("<td>Agent ID Number</td><td>111-22-3333</td>", emailtext);
			AssertContains("<td>Surety Name</td><td>SURETY NAME</td>", emailtext);
		}

		public void TestActionsToTakeOneOrMoreBond()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2THIS IS A TEST                                                                " +
				"36ANI111-NN-NNNN BOND USER NAME                          D112714121014          " +
				"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => !string.IsNullOrEmpty(x.Body));
			AssertNotNull(email);
			var emialtext = email.Body;

			var cesn36Table =
				new HtmlTableCreator(new string[]
				{
						"Bond User ID Number Type", "Bond User ID Number", "Bond User Name", "User Rider Action Code", "User Add Date",
						"User Delete Date"
				});
			cesn36Table.EnableHTMLEncoding = false;
			cesn36Table.WriteRow("CBP-assigned Number", "111-NN-NNNN", "BOND USER NAME", "Rider to Delete User from the Bond",
				"27-Nov-14", "10-Dec-14");
			AssertContains(cesn36Table.ToHtml(), emialtext);
		}

		public void TestActionsRelatedToEntryAndEntrySummary()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2107XJ5C12345780                                                               " +
				"B3THIS IS A TEST                                                                " +
				"T1CNNJXJ5123456780XYZ000TYZ0009072000000000009112021400000001000BrokerABC1204142" +
				"T2CNNJXJ512345678012091411N21121014121114000004125001215140000000114400000002255" +
				"T3CNNJXJ5123456780000000000520000000006500000000098000000000360000000007810279  " +
				"T4CNNJXJ51234567802XYZABC123OK027001DFEOIPWW0QWE1451240HGFSXCVBS0               " +
				"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => !string.IsNullOrEmpty(x.Body));
			AssertNotNull(email);
			var emialtext = email.Body;

			AssertContains("<td>Disposition Code</td><td>ENB / NEW BOND HAS BEEN ADDED IN ACE</td>", emialtext);
			AssertContains("<td>Bond Number</td><td>123456789</td>", emialtext);
			AssertContains("<td>Transaction ID Type</td><td>1</td>", emialtext);
			AssertContains("<td>Entry Type Code</td><td>07</td>", emialtext);
			AssertContains("<td>Transaction ID</td><td>XJ5C12345780</td>", emialtext);
			AssertContains("<td>Remarks</td><td>THIS IS A TEST</td>", emialtext);

			AssertContains("<td>District/Port of Entry</td><td>CNNJ</td>", emialtext);
			AssertContains("<td>Filer Code</td><td>XJ5</td>", emialtext);
			AssertContains("<td>Entry Number</td><td>123456780</td>", emialtext);
			AssertContains("<td>Surety Code</td><td>XYZ</td>", emialtext);
			AssertContains("<td>Bond Number</td><td>000TYZ000</td>", emialtext);
			AssertContains("<td>Bond Type</td><td>Single transaction bond</td>", emialtext);
			AssertContains("<td>Entry Type</td><td>07</td>", emialtext);
			AssertContains("<td>Release/Summary Indicator</td><td>Entry summary data</td>", emialtext);
			AssertContains("<td>Entry Source</td><td>ABI</td>", emialtext);
			AssertContains("<td>Collection Status</td><td>Fully paid</td>", emialtext);

			AssertContains("<td>Record Status</td><td>Release only</td>", emialtext);
			AssertContains("<td>Accelerated Drawback Indicator</td><td>Accelerated drawback not claimed or not approved</td>", emialtext);
			AssertContains("<td>Possible Late Indicator</td><td>More than 30 days late</td>", emialtext);

			AssertContains("<td>Recon Entry Other Issue Entry Type 09</td><td>Value/Class/9802 Recon</td>", emialtext);

			AssertContains("<td>NAFTA Reconciliation Flag</td><td>NAFTA Recon. Filed</td>", emialtext);
			AssertContains("<td>Other Reconciliation Flag</td><td>Value/Class/9802 Recon filed</td>", emialtext);
		}

		public void TestEmailDoesNotContainSSNNumber()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"T1CNNJXJ5123456780XYZ000TYZ0009072123-45-1234 112021400000001000BrokerABC1204142" +
				"Y  8888XJ5WR00005";

			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message1.EM_MessageNum = "~15001";
			message1.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message1.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"T1CNNJXJ5123456780XYZ000TYZ0009072123245-1234 112021400000001000BrokerABC1204142" +
				"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => !string.IsNullOrEmpty(x.Body));
			AssertNotNull(email);
			var emialtext = email.Body;

			AssertNotContains("Email does not contain SSN Number", "<td>Importer of Record</td><td>123-45-1234</td>", emialtext);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.LastOrDefault(x => !string.IsNullOrEmpty(x.Body));
			AssertNotNull(email);
			emialtext = email.Body;

			AssertContains("EIN Number or CBP Number included in the mail", "<td>Importer of Record</td><td>123245-1234</td>", emialtext);
		}

		public void TestSendNoticeMail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B0000124";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "C1234578";

			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GS_NKCusAgent = "Z8";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;

			message.EM_MessageText =
			"B018888XJ5BS                                               286                  " +
			"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
			"B2THIS IS A TEST                                                                " +
			"10B93A30000012345112614123456789112014121514000000009 NY                        " +
			"10B93A30000012345112614123456789112014121514000000009 NY                        " +
			"10B93A30000012345112614123456789112014121514000000009 NY                        " +
			"10B93A30000012345112614123456789112014121514000000009 NY                        " +
			"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
			"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
			"20107XJ5C1234578                                                                " +
			"3034 NN-NNNNNNNXXPrincipal Name                                                 " +
			"35EI YYDDPP-NNNNNCo-principal Name1                                             " +
			"35EI YYDDPP-NNNNNCo-principal Name2                                             " +
			"35EI YYDDPP-NNNNNCo-principal Name3                                             " +
			"36ANI111-NN-NNNN Bond User Name                          D112714121014          " +
			"36ANI222-NN-NNNN Bond User Name                          D112714121014          " +
			"36ANI333-NN-NNNN Bond User Name                          D112714121014          " +
			"36ANI444-NN-NNNN Bond User Name                          D112714121014          " +
			"36ANI555-NN-NNNN Bond User Name                          D112714121014          " +
			"40123777-88-9999Surety Name                              0000000005             " +
			"45111111-88-9999Surety Name                              0000000005             " +
			"45222222-88-9999Surety Name                              0000000005             " +
			"45333333-88-9999Surety Name                              0000000005             " +
			"Y  8888XJ5WR00005";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("Generate new message", universalEventMessage);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			AssertNotNull(email);
			AssertEquals(true, email.Body.Contains(string.Format("<a href=\"{0}\">{1}</a>", CargoWise.Application.ObjectFactory.Get<ZArchitecture.Modules.IShowEditFormUrlCreator>().Create(declaration), declaration.JobNumber)));
			declaration.Messages.Reload(true);
			AssertEquals("message linking to a job", 1, declaration.Messages.Count);
			declaration.Logs.GetAllLogs().Reload(true);
			var log = declaration.Logs.GetAllLogs().Cast<ZArchitecture.Business.StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Enterprise.ZArchitecture.Business.AutoEvents.MessageReceived.ToString());
			AssertNotNull(log);
		}

		public void TestDataTargetUSImporterSecurityFiling()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;

			message.EM_MessageText =
			"B004701739BS                                                                    " +
			"B116S006BLS ENB NEW BOND HAS BEEN ADDED IN ACE           1 1107161717           " +
			"10B916      10000110716000118005110716      16S006BLS                           " +
			"124701739                                                                       " +
			"202  73994506084190                                                             " +
			"30EI 90 - 107748100CADENCE INSOLES LLC                                          " +
			"40856143 - 48 - 7151LEXON INSURANCE COMPANY                      10000          " +
			"Y  4701739BS00006                                                               ";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("Generate new message", universalEventMessage);
			AssertContains("DataTargetType is USImporterSecurityFiling", "<Type>USImporterSecurityFiling</Type>", universalEventMessage.DiagnosticDetails);
		}

		public void TestDataTargetCustomsDeclaration()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;

			message.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2THIS IS A TEST                                                                " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"20107XJ5C1234578                                                                " +
				"3034 NN-NNNNNNNXXPrincipal Name                                                 " +
				"35EI YYDDPP-NNNNNCo-principal Name1                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name2                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name3                                             " +
				"36ANI111-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI222-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI333-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI444-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI555-NN-NNNN Bond User Name                          D112714121014          " +
				"40123777-88-9999Surety Name                              0000000005             " +
				"45111111-88-9999Surety Name                              0000000005             " +
				"45222222-88-9999Surety Name                              0000000005             " +
				"45333333-88-9999Surety Name                              0000000005             " +
				"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("Generate new message", universalEventMessage);
			AssertContains("DataTargetType is CustomsDeclaration", "<Type>CustomsDeclaration</Type>", universalEventMessage.DiagnosticDetails);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		GlbStaff Staff
		{
			get { return staff ?? (staff = CreateStaff("KNZ", "kevin", "Kevin.zhang", "kevin@test.COM")); }
		}
		GlbStaff staff;

		protected override void SetUp()
		{
			base.SetUp();
			var noticeGroup = Factory.NewWithValidTestData<GlbGroup>();
			noticeGroup.GG_Code = "TST";
			noticeGroup.GG_Desc = "Test";
			noticeGroup.Staff.Add(Staff);
			Factory.Save();
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, noticeGroup.PK.ToGuid());
		}

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		ZQuery GetMessageQuery()
		{
			var messageQuery = new ZQuery();
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal);
			messageQuery.AddToFilter(EDIMessageSchema.EM_GB, GlbBranch.CurrentBranch.PK);
			messageQuery.AddToFilter(EDIMessageSchema.EM_GE, GlbDepartment.CurrentDepartment.PK);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			messageQuery.MaximumRows = 100;
			return messageQuery;
		}
	}
}

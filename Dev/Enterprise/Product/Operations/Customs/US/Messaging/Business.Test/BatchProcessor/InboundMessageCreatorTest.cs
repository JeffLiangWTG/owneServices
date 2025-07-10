using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class InboundMessageCreatorTest : TestCaseWithFactory
	{
		public void TestCreateMessagesForInterchange()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "K#@";
			company.GC_Name = "TEST COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$#";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var interchangeHeader =
				"A3910SV9      10281001   110110212849                                00000054211";

			var interchangeBody =
				"B018888BHCEI                                               54443                " +
				"EB A  AND  B  REC DP/FLR/OFFICE CONFLICT                                        " +
				"EBTRANSACTION DATA REJECTED                                                     " +
				"EB Z  RECORD MISSING OR INVALID                                                 " +
				"EBTRANSACTION DATA REJECTED                                                     ";

			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", interchangeHeader, interchangeBody, "");
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);
			interchange.EI_GB = branch.PK;
			Factory.Save();

			var processor = new InboundMessageCreatorForTesting();
			processor.CreateMessagesForInterchange(interchange);
			Factory.Save();

			interchange.Reload();
			AssertEquals("EI", interchange.EI_InterchangeType);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, InboundInterchangeProcessorForTesting.AppCode);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, interchange.EI_InterchangeType);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "0");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<CBPMessageForTesting>(ediMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];

			ZString expectedMessageText = "B018888BHCEI                                               54443                EB A  AND  B  REC DP/FLR/OFFICE CONFLICT                                        EBTRANSACTION DATA REJECTED                                                     EB Z  RECORD MISSING OR INVALID                                                 EBTRANSACTION DATA REJECTED                                                     Y           00000";
			AssertEquals("EM_MessageText", expectedMessageText, msg.EM_MessageText.TrimEnd());
			AssertEquals("EM_EI", interchange.PK, msg.EM_EI);
			AssertEquals("EM_GB", branch.PK, msg.EM_GB);
		}
	}
}

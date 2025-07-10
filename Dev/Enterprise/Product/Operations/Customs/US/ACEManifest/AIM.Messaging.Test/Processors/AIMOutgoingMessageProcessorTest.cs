using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestIsNotBranchFilter()
		{
			var processor = new AIMOutgoingMessageProcessorForTest(new LoggingInformation());
			AssertEquals("IsBranchFilter", false, processor.IsBranchFilterExposed);
		}

		public void TestProcessOutgoingMessage()
		{
			var processingCompany = Factory.New<GlbCompany>();
			processingCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Fiji;
			var processingBranch = processingCompany.Branches.AddNew();
			processingBranch.GB_Code = "PRB";
			processingBranch.GB_RL_NKHomePort = "FJSUV";
			Factory.Save();

			var msgText = @"FRI
JFKXYZ
999-12345675-M
WBL/FRA/T1/K10/TOYS
ARR/XYZ123/25OCT
SHP/TOTLERTOYS
/12 VIRGINIA COURT
/FRANKFURT
/DE
CNE/TOYSRWE
/8812 FUN STREET
/NEWYORK/NY
/US/12345/123-456-7890";

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = "Z2";

				var orgAddress = orgHeader.Addresses.AddNew();
				orgAddress.OA_Address1 = "line 2";
				orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.AirAMSOriginatorCode, "USAMSOC", Core.Constants.CountryCodes.UnitedStates);

				var header = Factory.New<ACEManifest.Business.AsycudaManifestHeader>();
				header.AMA_OA_DeconsolidateAddress = orgAddress.PK;
				header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;

				var outgoingMessage = Factory.New<AIMEDIMessage>();
				outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.FHL;
				outgoingMessage.EM_MessageSubType = AIMMessageSubTypes.FSN;
				outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage.EM_Status = EDIMessage.Status.Queued;
				outgoingMessage.EM_MessageText = msgText;
				outgoingMessage.EM_MessageNum = "TEST11223344";
				outgoingMessage.EM_LinkedObject = header;
				Factory.Save();

				using (Environment.DisposableEnvironment.ForBranch(processingBranch.PK.ToGuid()))
				{
					var processor = new AIMOutgoingMessageProcessor(new LoggingInformation());
					processor.ProcessMessage(CancellationToken.None);

					outgoingMessage.Reload();
					AssertEquals("EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
					var outgoingInterchange = (AIMEDIInterchange)outgoingMessage.Interchange;

					AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.USAMA, outgoingInterchange.EI_ApplicationCode);
					AssertEquals("EI_InterchangeType", EDIMessageTypeList.Codes.FHL, outgoingInterchange.EI_InterchangeType);
					AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, outgoingInterchange.EI_Status);
					AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, outgoingInterchange.EI_TransportType);
					AssertEquals("EI_From", "USAMSOC", outgoingInterchange.EI_From);
					AssertEquals("EI_To", "USC", outgoingInterchange.EI_To);
					AssertEquals("EI_HeaderText", "WASUCCR\x0D\x0A.USAMSOC", outgoingInterchange.EI_HeaderText);
					AssertEquals("EI_BodyText", outgoingMessage.EM_MessageText, outgoingInterchange.EI_BodyText);
					AssertEquals("EI_FooterText", ZString.Empty, outgoingInterchange.EI_FooterText);
					AssertEquals("EI_GB", outgoingMessage.EM_GB, outgoingInterchange.EI_GB);
				}
			}
		}
	}

	class AIMOutgoingMessageProcessorForTest : AIMOutgoingMessageProcessor
	{
		public AIMOutgoingMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public bool IsBranchFilterExposed => IsBranchFilter;
	}
}

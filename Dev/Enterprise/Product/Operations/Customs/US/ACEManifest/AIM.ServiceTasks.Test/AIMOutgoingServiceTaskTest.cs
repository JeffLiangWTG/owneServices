using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AIM.ServiceTasks.Testing
{
	[TestedType(typeof(AIMOutgoingServiceTask))]
	class AIMOutgoingServiceTaskTest : ServiceTaskTestCase<AIMOutgoingServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("AMO", "US Air Manifest Outbound", "USC");
		}

		public void TestPackagingAIMMessage()
		{
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
			using (var dir1 = new TempDirectory())
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

				var serviceTask = new AIMOutgoingServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
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
				AssertEquals("EI_FooterText", "", outgoingInterchange.EI_FooterText);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Air Manifest Outbound",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.USAMA,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}

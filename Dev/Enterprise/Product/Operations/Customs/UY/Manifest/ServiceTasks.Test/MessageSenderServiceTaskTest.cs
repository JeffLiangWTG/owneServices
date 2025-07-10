using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.Customs.UY.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.UY.Manifest.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSenderService))]
	sealed class MessageSenderServiceTaskTest : ServiceTaskTestCase<MessageSenderService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "UYS", hostedServiceAttribute.Code);
				AssertEquals("Description", "UCS Uruguayan Customs Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "UYC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1Minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Uruguay, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask()
		{
			foreach (var branch in GlbCompany.CurrentCompany.ActiveBranches)
			{
				if (branch.PK == GlbBranch.CurrentBranch.PK)
				{
					branch.GB_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(-1);
				}
				else
				{
					branch.GB_SystemCreateTimeUtc = DateTime.UtcNow;
				}
			}
			GlbCompany.CurrentCompany.Factory.Save();

			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_UserID = "USERNAME";
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var bodyTextWithOutSign = Path.Combine(BaseSourcePath, DAETestingConstants.BodyTextWithOutSign);
			var manifest1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var message1 = Factory.New<UYMessage>();

			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message1.EM_ApplicationReference = "REF1";
			message1.EM_GB = GlbBranch.CurrentBranch.PK;
			message1.EM_IsTestMessage = true;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message1.EM_LinkUniqueID = manifest1.PK;
			message1.EM_MessageOwner = "OWNER1";
			message1.EM_MessageText = File.ReadAllText(bodyTextWithOutSign);
			message1.EM_MessageType = MessageTypes.Codes.UYC;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var manifest2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest1.AMA_JobReference = "A1996";

			var message2 = Factory.New<UYMessage>();

			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.UYCustoms;
			message2.EM_ApplicationReference = "REF2";
			message2.EM_GB = GlbBranch.CurrentBranch.PK;
			message2.EM_IsTestMessage = false;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message2.EM_LinkUniqueID = manifest2.PK;
			message2.EM_MessageOwner = "OWNER2";
			message2.EM_MessageText = File.ReadAllText(bodyTextWithOutSign);
			message2.EM_MessageType = MessageTypes.Codes.UYC;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var task = new MessageSenderService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 2, interchangesCreated.Length);

				var zquery = new ZDBOnlyQuery(typeof(EDIInterchange));
				var ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
				ediMessageQuery.AddToFilter(EDIMessageSchema.PK, message1.PK);
				zquery.AddSubQuery(ediMessageQuery, JoinCondition.And);
				var interchange = Factory.LoadTop1<EDIInterchange>(zquery);

				AssertEquals("EI_ApplicationCode", "URU", interchange.EI_ApplicationCode);
				AssertEquals("EI_InterchangeNum", message1.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_InterchangeType", "UYC", interchange.EI_InterchangeType);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_To", "UYCustomsTEST", interchange.EI_To);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);
				AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);

				AssertNotNull(interchange.EI_SessionGUID);

				message1.Reload();
				AssertEquals("EM_EI", interchange.PK, message1.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message1.EM_Status);

				zquery = new ZDBOnlyQuery(typeof(EDIInterchange));
				ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
				ediMessageQuery.AddToFilter(EDIMessageSchema.PK, message2.PK);
				zquery.AddSubQuery(ediMessageQuery, JoinCondition.And);
				interchange = Factory.LoadTop1<EDIInterchange>(zquery);

				AssertEquals("EI_InterchangeNum", message2.EM_MessageNum, interchange.EI_InterchangeNum);
				AssertEquals("EI_To", "UYCustomsPROD", interchange.EI_To);

				message2.Reload();
				AssertEquals("EM_EI", interchange.PK, message2.EM_EI);
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message2.EM_Status);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.UYS,
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.UYCustoms,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_MessageType + "=" + MessageTypes.Codes.UYC),
				};
			}
		}
	}
}

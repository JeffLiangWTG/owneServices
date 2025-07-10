using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class PortAuthorityInterchangeProviderTest : BaseAgencyTest
	{
		public void TestInstructionsStayInSync()
		{
			PortAuthorityInterchangeProvider provider = new PortAuthorityInterchangeProvider(new NonDependentEDIMessageCollection(Factory));
			string instructions = (string)typeof(InterchangeProviderBase).InvokeMember("InstructionHowToSetInterchangeSenderID", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, provider, Array.Empty<object>());
			Enterprise.Integration.IRegistryItemInternals registryItem = AgencyRegistry.Instance.PortAuthoritySettings;
			string expectedInstructions = "You need to set the Sender ID from the registry item: {0}";
			AssertEquals(string.Format(expectedInstructions, registryItem.Location), instructions);
		}

		public void TestGetInterchangeNumberWithLongSenderReceiver()
		{
			PortAuthorityPortCollection ports = new PortAuthorityPortCollection();
			PortAuthorityPort setting = ports.AddNew();
			setting.Port = HomePort;
			setting.ProductionEmail = "bob@freadnet.org";
			setting.ProductionID = "12345678901234567890123456789012345";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ports);
			PortAuthoritySettings settings = new PortAuthoritySettings();
			settings.Settings[0].Status = PortAuthoritySettingStatus.Codes.Production;
			settings.Settings[0].SenderID = "abcdefghijklmnopqrstuvwxyzabcdefghi";
			AgencyRegistry.Instance.PortAuthoritySettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage message = AddMessage(messages, sailing.Origin, "MESSAGE+BODY+END'");
			EDIInterchange[] interchanges = new PortAuthorityInterchangeProvider(messages).Interchanges;
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestGenerateInterchangeSegmentsCorrectly()
		{
			SetPortAuthoritySettings(HomePort);
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage goodMessage1 = AddMessage(messages, sailing.Origin, "MESSAGE+BODY1+END'");
			EDIMessage badMessage = AddMessage(messages, sailing.Origin, "MESSAGE+BODY2+END'");
			badMessage.EM_LinkUniqueID = ZGuid.NewZGuid();
			EDIMessage goodMessage2 = AddMessage(messages, sailing.Origin, "MESSAGE+BODY3+END'");
			EDIInterchange[] interchanges = new PortAuthorityInterchangeProvider(messages).Interchanges;
			AssertEquals("Expecting 2 interchanges", 2, interchanges.Length);
			AssertContainsExactElementsInAnyOrder("The generated interchanges relate to the correct messages", new EDIMessage[] { goodMessage1, goodMessage2 }, new EDIMessage[] { interchanges[0].ContainedMessages[0], interchanges[1].ContainedMessages[0] });
			const string format = "UNB+UNOA:4+SENDERID1+RECIPIENTID1+{0:yyMMdd:HHmm}+{1}'MESSAGE+BODY{2}+END'UNZ+1+{1}'";
			AssertMessageEquals(string.Format(format, goodMessage1.Interchange.PreparationDateTime, EDIInterchange.InterchangeNumberPlaceHolder, 1), goodMessage1.Interchange.EI_InterchangeText);
			AssertMessageEquals(string.Format(format, goodMessage2.Interchange.PreparationDateTime, EDIInterchange.InterchangeNumberPlaceHolder, 3), goodMessage2.Interchange.EI_InterchangeText);
			AssertEquals("The message without a parent should be deleted", true, badMessage.IsDeleted);
		}

		[ExpectException(typeof(MessageProcessingException))]
		public void TestGenerateInterchangeWithBadRegistrySettings()
		{
			SetPortAuthoritySettings();
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			EDIMessage goodMessage1 = AddMessage(messages, sailing.Origin, "MESSAGE+BODY+END'");
			EDIInterchange[] interchanges = new PortAuthorityInterchangeProvider(messages).Interchanges;
		}

		public void TestEachMessageGetsItsOwnInterchange()
		{
			SetPortAuthoritySettings(HomePort, AlternateHomePort, "AUMEL", "AUSYD");
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			var exportSailing = FindOrCreateSailing(Voyage, "AUBNE", "AUSYD");
			var exportSailing2 = FindOrCreateSailing(Voyage2, "AUBNE", "AUSYD");
			AddMessage(messages, exportSailing.Origin, "1");
			AddMessage(messages, exportSailing.Destination, "2");
			AddMessage(messages, exportSailing2.Origin, "3");
			AddMessage(messages, exportSailing2.Destination, "4");
			EDIInterchange[] interchanges = new PortAuthorityInterchangeProvider(messages).Interchanges;
			AssertEquals("Expected 4 interchanges", 4, interchanges.Length);
			foreach (EDIInterchange interchange in interchanges)
			{
				AssertNotNull("Interchange should not be null", interchange);
				AssertEquals("Interchange should have exactly 1 message", 1, interchange.ContainedMessages.Count);
			}
		}

		public void TestSenderReceiverID()
		{
			SetPortAuthoritySettings(AlternateHomePort, HomePort);
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			AddMessage(messages, sailing.Origin, "blat");
			EDIInterchange[] interchanges = new PortAuthorityInterchangeProvider(messages).Interchanges;
			AssertEquals("Expected 1 interchange", 1, interchanges.Length);
			AssertEquals("Expected sender id", "SenderID2", interchanges[0].EI_From);
			AssertEquals("Expected receiver id", "RecipientID2", interchanges[0].EI_To);
		}

		#region Implementation
		#region Voyage

		JobVoyage Voyage
		{
			get
			{
				return voyage ?? (voyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage;

		JobVoyage Voyage2
		{
			get
			{
				return voyage2 ?? (voyage2 = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage2;

		#endregion

		static EDIMessage AddMessage(NonDependentEDIMessageCollection messages, ISailingEndPoint endPoint, ZString text)
		{
			EDIMessage result = endPoint.Messages.AddNew();
			result.FillWithValidTestData();
			result.EM_MessageText = text;
			result.EM_MessageOwner = String.Empty;
			messages.Add(result);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
		}

		#endregion
	}
}

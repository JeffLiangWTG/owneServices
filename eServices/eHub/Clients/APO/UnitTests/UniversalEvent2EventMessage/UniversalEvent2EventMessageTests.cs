using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.APO.Transforms.UniversalEvent2EventMessages;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.APO.Tests
{
	[TestClass]
	public class UniversalEvent2EventMessageTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2EventMessageCAO()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalEvent2EventMessage.TestFiles.UniversalInterchangeCAO.xml";
			string outputFile = "UniversalEvent2EventMessage.TestFiles.EventMessageCAO.xml";
			mapTester.Execute<UniversalEvent2EventMessages>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2EventMessageMSF()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalEvent2EventMessage.TestFiles.UniversalInterchangeMSF.xml";
			string outputFile = "UniversalEvent2EventMessage.TestFiles.EventMessageMSF.xml";
			mapTester.Execute<UniversalEvent2EventMessages>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2EventMessageBatchSplit()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalEvent2EventMessage.TestFiles.UniversalInterchangeSplit.xml";
			string outputFile = "UniversalEvent2EventMessage.TestFiles.EventMessageSplit.xml";
			mapTester.Execute<UniversalEvent2EventMessages>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2EventMessageFilter()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalEvent2EventMessage.TestFiles.UniversalInterchangeFilter.xml";
			string outputFile = "UniversalEvent2EventMessage.TestFiles.EventMessageFilter.xml";
			mapTester.Execute<UniversalEvent2EventMessages>(sourceFile, outputFile);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2Event_AdjustToTimeZone()
		{
			Assert.AreEqual("2013-08-18T22:31:03-07:30", AdjustToTimeZone("2013-08-19T06:01:03.880", "-07:30"));
			Assert.AreEqual("2013-08-19T08:21:03+02:20", AdjustToTimeZone("2013-08-19T06:01:03", "+02:20"));

			Assert.AreEqual("2013-08-19T17:39:00+00:00", AdjustToTimeZone("2013-08-19T17:39:00", ""));
			Assert.AreEqual("2013-08-19T17:39:00+00:00", AdjustToTimeZone("2013-08-19T17:39:00.550", "NULL"));

			Assert.AreEqual("", AdjustToTimeZone("", "-12:00"));
			Assert.AreEqual("", AdjustToTimeZone("", ""));
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2EventMessageFileName()
		{
			var messageContext = InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			exclusionXpaths.Add("//*[local-name()='EventMessageTransmissionDateTime']");
			exclusionXpaths.Add("//*[local-name()='RecordModificationDateTime']");
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalEvent2EventMessage.TestFiles.UniversalInterchangeMSF.xml";
			string outputFile = "UniversalEvent2EventMessage.TestFiles.EventMessageMSF.xml";
			mapTester.Execute<UniversalEvent2EventMessages>(sourceFile, outputFile);

			Assert.AreEqual("APOMEL-8B6B683E-7F7C-4B7C-AA6A-376E16D0E9AE", messageContext.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalEvent2EventMessageEmptyOutput()
		{
			InitialiseCodeMapsTestingContext();

			List<string> exclusionXpaths = new List<string>();
			ICompare comparer = new ExcludingComparer(exclusionXpaths);
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), comparer);

			string sourceFile = "UniversalEvent2EventMessage.TestFiles.UniversalInterchangeEmpty.xml";
			string outputFile = "UniversalEvent2EventMessage.TestFiles.EventMessageEmpty.xml";
			mapTester.Execute<UniversalEvent2EventMessages>(sourceFile, outputFile);
		}

		string AdjustToTimeZone(string utcDateTimeString, string timeZone)
		{
			DateTime utcDateTime;
			if (!DateTime.TryParse(utcDateTimeString.Trim(), out utcDateTime)) return "";
			int hours = 0;
			int minute = 0;

			if (string.IsNullOrWhiteSpace(timeZone) || timeZone == "NULL")
			{
				timeZone = "+00:00";
			}
			else
			{
				var parts = timeZone.Split(':');
				if (parts.Length != 2 || !int.TryParse(parts[0], out hours) || !int.TryParse(parts[1], out minute)) throw new InvalidTimeZoneException("TimeZone string received from CalculateTimeZoneOffset sp has invalid format. Correct is HH:mm");
			}

			if (hours < 0 && minute > 0) minute = -minute;
			utcDateTime = utcDateTime.AddHours(hours).AddMinutes(minute);
			return utcDateTime.ToString("s") + timeZone;

		}

		#region Implementation

		static TestingMessageContext InitialiseCodeMapsTestingContext()
		{
			var messageContext = new TestingMessageContext();

			messageContext.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "APOMELTSH");
			messageContext.Write("MessageTrackingID", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "8B6B683E-7F7C-4B7C-AA6A-376E16D0E9AE");
			messageContext.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "test.csv");

			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(messageContext);

			CodeMapsTestingContext ctx = new CodeMapsTestingContext();
			ctx.eHubClients.Add(new eHubClient { CC_ID = "APOMELTSH" });
			ctx.eHubClients.Add(new eHubClient { CC_ID = "APOMELTSH_GLW" });

			ctx.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "APOMEL Glow Events", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1] });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Event Type", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "SAP Event" });
			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 2, CR_Name = "Held Event" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "MSF" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "DOM-0030" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "" });

			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 2, CK_Key1Value = "CAO" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[0], CV_OutputCode = "DOM-0031" });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults[1], CV_OutputCode = "" });

			ctx.eHubCodeSets.Add(new eHubCodeSet { CS_Name = "Batching", eHubClient_Sender = ctx.eHubClients[0], eHubClient_Recipient = ctx.eHubClients[1], eHubTransformationSet = ctx.eHubTransformationSets[0] });

			ctx.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = ctx.eHubCodeSets.Last(), CR_Order = 1, CR_Name = "Batch Size" });
			ctx.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = ctx.eHubCodeSets.Last(), CK_Order = 1 });
			ctx.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = ctx.eHubCodeMapKeys.Last(), eHubCodeSetResult = ctx.eHubCodeSetResults.Last(), CV_OutputCode = "2" });

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
			        "@UNLOCO", "AUMEL", 
			        "@localtime", "2013-07-04T22:09:32.393"
			    },
				Result = "+11:00"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
			        "@UNLOCO", "USLAX", 
			        "@localtime", "2013-06-13T09:21:52.827"
			    },
				Result = "-08:00"
			});

			ctx.ActionProcedures.Add(new ActionProcedure
			{
				Procedure = "CalculateTimeZoneOffset",
				OutputParm = "@offset",
				InputParms = new List<string> { 
					"@UNLOCO", "AUMEL", 
					"@localtime", "2013-03-26T11:26:00"
				},
				Result = "+11:00"
			});
			var ta = new TransformAccessor();
			ta.SetCodeMapsTestingContext(ctx);

			return messageContext;
		}
		#endregion
	}
}


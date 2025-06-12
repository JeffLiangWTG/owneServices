using System.IO;
using System.Reflection;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Security.AccessControl;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class TransformationPerformerTests : BaseComponentTest
	{
		const string map_type = "CargoWise.eHub.Core.Transforms.OutboxInsert2dbo_InsertOutboxMessage, CargoWise.eHub.Core.Transforms, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350";
		const string sourceResourceName = "CargoWise.eHub.Core.Tests.TestFiles.OutboxInsert2dbo_InsertOutboxMessage_Source.xml";
		const string expectedResourceName = "CargoWise.eHub.Core.Tests.TestFiles.OutboxInsert2dbo_InsertOutboxMessage_Expected.xml";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransformationPerformer_Succeed()
		{
			TempFiles.CleanupBizTalkTempFiles();
			var result = new TransformationPerformer().PerformTransformation(map_type, new PipelineContext(), CreateMessage());
			AssertMessageResult(result);
		}

		//[TestMethod] -- this is not a unit test
		public void TransformationPerformer_HandleRunOutOfTempFiles()
		{
			try
			{
				var path = Path.GetTempPath();

				// use up all possible biztalk temp file names
				for (uint i = 0; i < 0x10000; i++)
				{
					// It would make sense to do this via the GetTempFileName function.
					// However that would be very slow because:
					// *   when uUnique == 0 has to guess and check for a valid filename each time it is called
					// *   when uUnique != 0 doesnt seem to work as documented (no file is created) so we cant use this to just loop through each unique integer
					var filePath = String.Format("{0}/SXN{1:X}.tmp", path, i); // SXN is the string used by BTSXslTransform this can be seen by disassembling the dll
					File.Create(filePath).Close();
					File.SetLastWriteTime(filePath, DateTime.Now.AddHours(-25));
				}

				// Keep a file handle open to trigger IOException.
				var keep_alive = File.Open(path + "/SXN1000.tmp", FileMode.Open);

				// For some files set the write timestamp to < 1 day
				var recentPath1 = path + "/SXN1001.tmp";
				var recentPath2 = path + "/SXN1002.tmp";
				var recentPath3 = path + "/SXN1003.tmp";
				File.SetLastWriteTime(recentPath1, DateTime.Now.AddHours(-23));
				File.SetLastWriteTime(recentPath2, DateTime.Now.AddHours(-1));
				File.SetLastWriteTime(recentPath3, DateTime.Now);

				Assert.AreEqual((uint)0, GetTempFileName(path, "SXN", 0, new StringBuilder(0x100)), "GetTempFileName needs to be failing now");

				var result = new TransformationPerformer().PerformTransformation(map_type, new PipelineContext(), CreateMessage());
				AssertMessageResult(result);

				Assert.IsTrue(File.Exists(recentPath1), "< 1 day old temp file needs to still exist.");
				Assert.IsTrue(File.Exists(recentPath2), "< 1 day old temp file needs to still exist.");
				Assert.IsTrue(File.Exists(recentPath3), "< 1 day old temp file needs to still exist.");

				Assert.IsFalse(File.Exists(path = "/SXN0001.tmp"), "This file should be deleted");
				Assert.IsFalse(File.Exists(path = "/SXN0FFF.tmp"), "This file should be deleted");
				Assert.IsFalse(File.Exists(path = "/SXN1004.tmp"), "This file should be deleted");
				Assert.IsFalse(File.Exists(path = "/SXNFFFF.tmp"), "This file should be deleted");
			}
			finally
			{
				foreach (string f in Directory.EnumerateFiles(Path.GetTempPath(), "SXN*.tmp"))
				{
					try
					{
						File.Delete(f);
					}
					catch (IOException) { }
					catch (UnauthorizedAccessException) { }
				}
			}
		}

		public IBaseMessage CreateMessage()
		{
			Stream data = Assembly.GetExecutingAssembly().GetManifestResourceStream(sourceResourceName);
			var message = new Message();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = data;
			return message;
		}

		public void AssertMessageResult(IBaseMessage message)
		{
			var expected = Assembly.GetExecutingAssembly().GetManifestResourceStream(expectedResourceName);
			var actual = message.BodyPart.GetOriginalDataStream();
			MapTester.AssertSuccess(new XmlDiffTool().Execute(actual, expected), sourceResourceName, expectedResourceName);
		}

		[DllImport("kernel32.dll")]
		internal static extern uint GetTempFileName(string path, string prefix, uint unique, StringBuilder name);
	}
}

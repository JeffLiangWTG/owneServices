using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.BatchProcessor.Testing
{
	sealed class BatchSGCInterchangeSender40WebServicesConnectivityTest : TestCaseWithFactory
	{
		public void TestPackageAttachmentInterchange()
		{
			using (var dir = new TempDirectory())
			{
				var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				var ediMessage = Factory.New<SGEDIMessage>();
				ediInterchange.ContainedMessages.Add(ediMessage);
				var sender = new BatchSGCInterchangeSender40WebServicesForTest();
				var fileName = sender.PackageAttachmentInterchangeExtend(ediInterchange, dir.DirectoryName, "TempInterchangeTextForTest");
				AssertEquals("Should return '.edi' as the file extension.", Path.Combine(dir.DirectoryName, "TempInterchangeTextForTest.edi"), fileName);
				ediInterchange.ContainedMessages.RemoveAndDeleteAll();
				var xmlMessage = Factory.New<SGXmlEDIMessage>();
				ediInterchange.ContainedMessages.Add(xmlMessage);
				fileName = sender.PackageAttachmentInterchangeExtend(ediInterchange, dir.DirectoryName, "TempInterchangeTextForTest");
				AssertEquals("Should return '.xml' as the file extension.", Path.Combine(dir.DirectoryName, "TempInterchangeTextForTest.xml"), fileName);
			}
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestSendWithConnectivityErrors()
		{
			var digest = new StringEffectiveDate();
			digest.PreviousValue = "Mastication";
			digest.NewValue = "E73A42A55EF307A8F277D49BFBC90E40C4E378F97D1989D259BA13C28D6E591D";
			digest.EffectiveDate = ZDateTime.Now;
			var mhAccessVersion = new StringEffectiveDate();
			mhAccessVersion.PreviousValue = "FourZeroTwoOne";
			mhAccessVersion.NewValue = "4.0.4";
			mhAccessVersion.EffectiveDate = ZDateTime.Now;
			SGCustomsDataRegistry.Instance.DigestForLibs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, digest);
			SGCustomsDataRegistry.Instance.MhaccessVersionString.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mhAccessVersion);
			SGCustomsDataRegistry.Instance.VendorID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Daniel");
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			var sender = new BatchSGCInterchangeSender40WebServicesForTest();
			sender.UploadOneFileExposed("anything", "anything", ediInterchange);
			var lastLog = sender.Logger.DebugLogStrings[1];
			AssertContains("Web sez no", lastLog);
			AssertContains("A connection attempt failed", lastLog);
			AssertNotContains("Exception", lastLog);
			AssertEquals(null, ErrorReporter.LastExceptionReported);
		}
	}
}

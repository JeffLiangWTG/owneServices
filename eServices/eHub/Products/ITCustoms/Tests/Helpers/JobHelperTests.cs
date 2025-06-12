using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ITCustoms.Helpers;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Xml;
using CargoWise.eHub.Core.Orchestrations.Helper;

namespace CargoWise.eHub.Products.ITCustoms.Tests.Helpers
{
    [TestClass]
    public class JobHelperTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestBase36Encode()
        {
            Assert.AreEqual("00", JobHelper.Base36Encode(0));
            Assert.AreEqual("01", JobHelper.Base36Encode(1));
            Assert.AreEqual("02", JobHelper.Base36Encode(2));
            Assert.AreEqual("0A", JobHelper.Base36Encode(10));
            Assert.AreEqual("0K", JobHelper.Base36Encode(20));
            Assert.AreEqual("2S", JobHelper.Base36Encode(100));
            Assert.AreEqual("ZZ", JobHelper.Base36Encode(1295));
            try
            {
                JobHelper.Base36Encode(-1);
                Assert.Fail("ArgumentOutOfRangeException Expected");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(
                    "Specified argument was out of the range of valid values.\r\nParameter name: input cannot be negative: -1",
                    ex.Message);
            }
            try
            {
                JobHelper.Base36Encode(1296);
                Assert.Fail("ArgumentOutOfRangeException Expected");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(
                    "Specified argument was out of the range of valid values.\r\nParameter name: input cannot be over 1295: 1296",
                    ex.Message);
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestBase36Decode()
        {
            Assert.AreEqual(0, JobHelper.Base36Decode("00"));
            Assert.AreEqual(1, JobHelper.Base36Decode("01"));
            Assert.AreEqual(2, JobHelper.Base36Decode("02"));
            Assert.AreEqual(10, JobHelper.Base36Decode("0A"));
            Assert.AreEqual(20, JobHelper.Base36Decode("0K"));
            Assert.AreEqual(100, JobHelper.Base36Decode("2S"));
            Assert.AreEqual(1295, JobHelper.Base36Decode("ZZ"));
            try
            {
                JobHelper.Base36Decode(".,");
                Assert.Fail("ArgumentOutOfRangeException Expected");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(
                    "Specified argument was out of the range of valid values.\r\nParameter name: input must be alphanumeric: .,",
                    ex.Message);
            }
            try
            {
                JobHelper.Base36Decode("000");
                Assert.Fail("ArgumentOutOfRangeException Expected");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(
                    "Specified argument was out of the range of valid values.\r\nParameter name: input is limited to two characters: 000",
                    ex.Message);
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetCurrentMaxFileNumber()
        {
            var node = "NNNN";
            var fileType = "P";
            var fileNamePrefix = JobHelper.GetFileNamePrefix(node, fileType, new NoOpLogger());
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            var eHubTransformationSet = new eHubTransformationSet() { TS_Name = "ITCustoms Outbound Message Counter" };
            JobHelper.GetContext = () => mockeHubTransactionsContext;
            mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets)
                .Return(new TestDbSet<eHubTransformationSet>() { eHubTransformationSet }).Repeat.Any();
            mockeHubTransactionsContext.Stub(x => x.eHubInterfaceCounters)
                .Return(new TestDbSet<eHubInterfaceCounter>()
                {
                    new eHubInterfaceCounter() {eHubTransformationSet = eHubTransformationSet, CT_Name = fileNamePrefix, CT_Value = 999}
                });

            Assert.AreEqual(999, JobHelper.GetCurrentMaxFileNumber(fileNamePrefix, new NoOpLogger()));
            mockeHubTransactionsContext.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetCurrentMaxFileNumberWhenCounterNotExists()
        {
            var node = "NNNN";
            var fileType = "P";
            var fileNamePrefix = JobHelper.GetFileNamePrefix(node, fileType, new NoOpLogger());
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            JobHelper.GetContext = () => mockeHubTransactionsContext;
            var eHubTransformationSet = new eHubTransformationSet() { TS_Name = "ITCustoms Outbound Message Counter" };
            mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets)
                .Return(new TestDbSet<eHubTransformationSet>() { eHubTransformationSet }).Repeat.Any();
            mockeHubTransactionsContext.Stub(x => x.eHubInterfaceCounters)
                .Return(new TestDbSet<eHubInterfaceCounter>() { });
            mockeHubTransactionsContext.Stub(x => x.SaveChanges()).Return(0);

            Assert.AreEqual(-1, JobHelper.GetCurrentMaxFileNumber(fileNamePrefix, new NoOpLogger()));
            mockeHubTransactionsContext.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestTryGetErrorDescriptionsFromAnswerFile()
        {
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            JobHelper.GetContext = () => mockeHubTransactionsContext;
            var customs = new eHubClient() { CC_ID = "ITCustoms", CC_FriendlyName = "ITCustoms Production" };
            var transformationSet = new eHubTransformationSet() { TS_Name = "ITCustoms Response Message Error Code", eHubClient_Recipient = customs };
            var codeSet = new eHubCodeSet() { CS_Name = "AnswerFileError", eHubClient_Sender = customs, eHubClient_Recipient = customs, eHubTransformationSet = transformationSet };
            var codeSetResult = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Error Code", CR_Order = 1 };
            var codeSetResult2 = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Error Decription", CR_Order = 2 };
            var codeMapKey = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_Order = 1, CK_Key1Value = "1" };
            var codeMapKey2 = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_Order = 2, CK_Key1Value = "A" };
            var codeMapValue = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey, eHubCodeSetResult = codeSetResult, CV_OutputCode = "ABSENCE OF A MANDATORY FIELD" };
            var codeMapValue2 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey2, eHubCodeSetResult = codeSetResult, CV_OutputCode = "CERTIFICATE EXPIRED" };

            var ehubClients = new TestDbSet<eHubClient>() { customs };
            var transfromationSets = new TestDbSet<eHubTransformationSet>() { transformationSet };
            var codeSets = new TestDbSet<eHubCodeSet>() { codeSet };
            var setResults = new TestDbSet<eHubCodeSetResult>() { codeSetResult, codeSetResult2 };
            var codeMapKeys = new TestDbSet<eHubCodeMapKey>() { codeMapKey, codeMapKey2 };
            var codeMapValues = new TestDbSet<eHubCodeMapValue>() { codeMapValue, codeMapValue2 };

            mockeHubTransactionsContext.Stub(x => x.eHubClients).Return(ehubClients);
            mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets).Return(transfromationSets);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeSets).Return(codeSets);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeSetResults).Return(setResults);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeMapKeys).Return(codeMapKeys);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeMapValues).Return(codeMapValues);

            var answerFile = new XmlDocument();
            answerFile.LoadXml(@"<AnswerMessage xmlns=""http://CargoWise.eHub.Products.ITCustoms.Schemas.FlatFileSchema1"">
				<Head xmlns="""">
					<UserCode>0020</UserCode>
					<StreamName>00200207.UZQ</StreamName>
					<CustomsSectionCode>279100</CustomsSectionCode>
					<TaxIDorVATnumberETC>10715680152</TaxIDorVATnumberETC>
					<AuthorizedUserSite>001</AuthorizedUserSite>
					<NumberOfRecords>00003</NumberOfRecords>
					<TransmissionEnvironment>INVIO IN AMBIENTE REALE</TransmissionEnvironment>
				</Head>
				<DetailOrQueue xmlns="""">4     119. 2    1 4</DetailOrQueue>
				<DetailOrQueue xmlns="""">1     111. 2  3 A</DetailOrQueue>
				<DetailOrQueue xmlns="""">Tue Feb 07 16:36:18 2017</DetailOrQueue>
				</AnswerMessage>");

            Assert.AreEqual("ABSENCE OF A MANDATORY FIELD\r\nCERTIFICATE EXPIRED", JobHelper.TryGetErrorDescriptionsFromAnswerFile(answerFile).Trim());

            answerFile = new XmlDocument();
            answerFile.LoadXml(@"<AnswerMessage xmlns=""http://CargoWise.eHub.Products.ITCustoms.Schemas.FlatFileSchema1"">
				<Head xmlns="""">
				<UserCode>0020</UserCode>
				<StreamName>00200119.ULR</StreamName>
				<CustomsSectionCode>279100</CustomsSectionCode>
				<TaxIDorVATnumberETC>10715680152</TaxIDorVATnumberETC>
				<AuthorizedUserSite>001</AuthorizedUserSite>
				<NumberOfRecords>00003</NumberOfRecords>
				<TransmissionEnvironment>INVIO IN AMBIENTE REALE</TransmissionEnvironment>
				</Head>
				<DetailOrQueue xmlns="""">0     000. 0  0 0</DetailOrQueue>
				<DetailOrQueue xmlns="""">0     000. 0    0 0</DetailOrQueue>
				<DetailOrQueue xmlns="""">Thu Jan 19 09:33:51 2017</DetailOrQueue>
				</AnswerMessage>");

            Assert.AreEqual("", JobHelper.TryGetErrorDescriptionsFromAnswerFile(answerFile));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetNextJobStatusAndPollStartUTC()
        {
            var statusA = "A";
            var pollIntervalA = 1;
            var repeatTimesA = 60;
            var statusB = "B";
            var pollIntervalB = 10;
            var repeatTimesB = 36;
            var ckpk1 = Guid.NewGuid();
            var ckpk2 = Guid.NewGuid();
            var crpk1 = Guid.NewGuid();
            var crpk2 = Guid.NewGuid();

            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            JobHelper.GetContext = () => mockeHubTransactionsContext;
            var customs = new eHubClient() { CC_ID = "ITCustoms", CC_FriendlyName = "ITCustoms Production" };
            var transformationSet = new eHubTransformationSet() { TS_Name = "ITCustoms Response Message Job Status", eHubClient_Recipient = customs };
            var codeSet = new eHubCodeSet() { CS_Name = "JobStatus", eHubClient_Sender = customs, eHubClient_Recipient = customs, eHubTransformationSet = transformationSet, CS_Key1Name = "Status" };
            var codeSetResult1 = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Poll Interval", CR_Order = 1, CR_PK = crpk1 };
            var codeSetResult2 = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Repeat Times", CR_Order = 2, CR_PK = crpk2 };
            var codeMapKey1 = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_PK = ckpk1, CK_Order = 1, CK_Key1Value = statusA };
            var codeMapKey2 = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_PK = ckpk2, CK_Order = 2, CK_Key1Value = statusB };
            var codeMapValue11 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey1, eHubCodeSetResult = codeSetResult1, CV_OutputCode = pollIntervalA.ToString(), CV_CK = ckpk1, CV_CR = crpk1 };
            var codeMapValue12 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey1, eHubCodeSetResult = codeSetResult2, CV_OutputCode = repeatTimesA.ToString(), CV_CK = ckpk1, CV_CR = crpk2 };
            var codeMapValue21 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey2, eHubCodeSetResult = codeSetResult1, CV_OutputCode = pollIntervalB.ToString(), CV_CK = ckpk2, CV_CR = crpk1 };
            var codeMapValue22 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey2, eHubCodeSetResult = codeSetResult2, CV_OutputCode = repeatTimesB.ToString(), CV_CK = ckpk2, CV_CR = crpk2 };

            var ehubClients = new TestDbSet<eHubClient>() { customs };
            var transfromationSets = new TestDbSet<eHubTransformationSet>() { transformationSet };
            var codeSets = new TestDbSet<eHubCodeSet>() { codeSet };
            var setResults = new TestDbSet<eHubCodeSetResult>() { codeSetResult1, codeSetResult2 };
            var codeMapKeys = new TestDbSet<eHubCodeMapKey>() { codeMapKey1, codeMapKey2 };
            var codeMapValues = new TestDbSet<eHubCodeMapValue>() { codeMapValue11, codeMapValue12, codeMapValue21, codeMapValue22 };

            mockeHubTransactionsContext.Stub(x => x.eHubClients).Return(ehubClients);
            mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets).Return(transfromationSets);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeSets).Return(codeSets);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeSetResults).Return(setResults);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeMapKeys).Return(codeMapKeys);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeMapValues).Return(codeMapValues);

            var jobStatus = string.Empty;
            for(int i = 0; i < repeatTimesA; i++)
            {
                var nextPoll = JobHelper.GetNextJobStatusAndPollStartUTC(jobStatus);
                jobStatus = nextPoll.Status;
                var pollDelay = (DateTime.Parse(nextPoll.PollStartUTCString) - DateTime.UtcNow).TotalMinutes;
                Assert.AreEqual(statusA + i.ToString("D2"), jobStatus);
                Assert.IsTrue(Math.Abs(pollIntervalA - pollDelay) < 0.1, $"Next poll start should be within {pollIntervalA} minutes but it's {pollDelay} minutes");
            }

            for (int i = 0; i < repeatTimesB; i++)
            {
                var nextPoll = JobHelper.GetNextJobStatusAndPollStartUTC(jobStatus);
                jobStatus = nextPoll.Status;
                var pollDelay = (DateTime.Parse(nextPoll.PollStartUTCString) - DateTime.UtcNow).TotalMinutes;
                Assert.AreEqual(statusB + i.ToString("D2"), jobStatus);
                Assert.IsTrue(Math.Abs(pollIntervalB - pollDelay) < 0.1, $"Next poll start should be within {pollIntervalB} minutes but it's {pollDelay} minutes");
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetNextJobStatusAndPollStartUTCFromAnswerFile()
        {
            var tsName = "ITCustoms Answer File Job Status";

            var statusA = "A";
            var pollIntervalA = 1;
            var repeatTimesA = 60;
            var statusB = "B";
            var pollIntervalB = 10;
            var repeatTimesB = 36;
            var ckpk1 = Guid.NewGuid();
            var ckpk2 = Guid.NewGuid();
            var crpk1 = Guid.NewGuid();
            var crpk2 = Guid.NewGuid();
            var mockeHubTransactionsContext = TestHelper.eHubTransactionsContextForTest();
            JobHelper.GetContext = () => mockeHubTransactionsContext;
            var customs = new eHubClient() { CC_ID = "ITCustoms", CC_FriendlyName = "ITCustoms Production" };
            var transformationSet = new eHubTransformationSet() { TS_Name = tsName, eHubClient_Recipient = customs };
            var codeSet = new eHubCodeSet() { CS_Name = "JobStatus", eHubClient_Sender = customs, eHubClient_Recipient = customs, eHubTransformationSet = transformationSet, CS_Key1Name = "Status" };
            var codeSetResult1 = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Poll Interval", CR_Order = 1, CR_PK = crpk1 };
            var codeSetResult2 = new eHubCodeSetResult() { eHubCodeSet = codeSet, CR_Name = "Repeat Times", CR_Order = 2, CR_PK = crpk2 };
            var codeMapKey1 = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_PK = ckpk1, CK_Order = 1, CK_Key1Value = statusA };
            var codeMapKey2 = new eHubCodeMapKey() { eHubCodeSet = codeSet, CK_PK = ckpk2, CK_Order = 2, CK_Key1Value = statusB };
            var codeMapValue11 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey1, eHubCodeSetResult = codeSetResult1, CV_OutputCode = pollIntervalA.ToString(), CV_CK = ckpk1, CV_CR = crpk1 };
            var codeMapValue12 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey1, eHubCodeSetResult = codeSetResult2, CV_OutputCode = repeatTimesA.ToString(), CV_CK = ckpk1, CV_CR = crpk2 };
            var codeMapValue21 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey2, eHubCodeSetResult = codeSetResult1, CV_OutputCode = pollIntervalB.ToString(), CV_CK = ckpk2, CV_CR = crpk1 };
            var codeMapValue22 = new eHubCodeMapValue() { eHubCodeMapKey = codeMapKey2, eHubCodeSetResult = codeSetResult2, CV_OutputCode = repeatTimesB.ToString(), CV_CK = ckpk2, CV_CR = crpk2 };

            var ehubClients = new TestDbSet<eHubClient>() { customs };
            var transfromationSets = new TestDbSet<eHubTransformationSet>() { transformationSet };
            var codeSets = new TestDbSet<eHubCodeSet>() { codeSet };
            var setResults = new TestDbSet<eHubCodeSetResult>() { codeSetResult1, codeSetResult2 };
            var codeMapKeys = new TestDbSet<eHubCodeMapKey>() { codeMapKey1, codeMapKey2 };
            var codeMapValues = new TestDbSet<eHubCodeMapValue>() { codeMapValue11, codeMapValue12, codeMapValue21, codeMapValue22 };

            mockeHubTransactionsContext.Stub(x => x.eHubClients).Return(ehubClients);
            mockeHubTransactionsContext.Stub(x => x.eHubTransformationSets).Return(transfromationSets);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeSets).Return(codeSets);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeSetResults).Return(setResults);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeMapKeys).Return(codeMapKeys);
            mockeHubTransactionsContext.Stub(x => x.eHubCodeMapValues).Return(codeMapValues);

            var jobStatus = string.Empty;
            for (int i = 0; i < repeatTimesA; i++)
            {
                var nextPoll = JobHelper.GetNextJobStatusAndPollStartUTC(jobStatus, tsName);
                jobStatus = nextPoll.Status;
                var pollDelay = (DateTime.Parse(nextPoll.PollStartUTCString) - DateTime.UtcNow).TotalMinutes;
                Assert.AreEqual(statusA + i.ToString("D2"), jobStatus);
                Assert.IsTrue(Math.Abs(pollIntervalA - pollDelay) < 0.1, $"Next poll start should be within {pollIntervalA} minutes but it's {pollDelay} minutes");
            }

            for (int i = 0; i < repeatTimesB; i++)
            {
                var nextPoll = JobHelper.GetNextJobStatusAndPollStartUTC(jobStatus, tsName);
                jobStatus = nextPoll.Status;
                var pollDelay = (DateTime.Parse(nextPoll.PollStartUTCString) - DateTime.UtcNow).TotalMinutes;
                Assert.AreEqual(statusB + i.ToString("D2"), jobStatus);
                Assert.IsTrue(Math.Abs(pollIntervalB - pollDelay) < 0.1, $"Next poll start should be within {pollIntervalB} minutes but it's {pollDelay} minutes");
            }
            return;
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetNewFilenameByType()
        {
            var oldFileName = "1111AAAA.R00";
            var newFileName = JobHelper.GetNewFilenameByType(oldFileName, "L");
            Assert.AreEqual("1111AAAA.L00", newFileName);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetNewFilenameByType_InvalidFilename()
        {
            try
            {
                var newFileName = JobHelper.GetNewFilenameByType("1111AAAA.00", "L");
            }
            catch(FatalMessageProcessingException ex)
            {
                Assert.AreEqual("Invalid filename: 1111AAAA.00", ex.Message);
            }
            try
            {
                var newFileName = JobHelper.GetNewFilenameByType("1111AAAA.0000", "L");
            }
            catch (FatalMessageProcessingException ex)
            {
                Assert.AreEqual("Invalid filename: 1111AAAA.0000", ex.Message);
            }
        }
    }
}

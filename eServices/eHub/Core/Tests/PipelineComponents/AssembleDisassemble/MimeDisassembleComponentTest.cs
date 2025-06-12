using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class MimeDisassembleComponentTest : BaseComponentTest
	{
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_Disable()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.01_PgpInlineFiles_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = false;

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.01_PgpInlineFiles_Input.eml");
            expectedMessage.Context = MessageFactory.CreateMessageContext();

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_PgpInline_Body()
		{
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.01_PgpInlineFiles_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.PgpSecretKeyInBase64 = "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";
            component.PgpPassphrase = "get busy living or get busy dying";
            component.Index = 0;

            component.Disassemble(pipelineContext, message);
            
            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.Text.01_PgpInlineFiles_Output_Body.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_PgpInline_Attachment()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.01_PgpInlineFiles_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.PgpSecretKeyInBase64 = "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";
            component.PgpPassphrase = "get busy living or get busy dying";
            component.Index = 1;

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.3by3");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("3by3", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_PgpInline_FileMask()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.01_PgpInlineFiles_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.PgpSecretKeyInBase64 = "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";
            component.PgpPassphrase = "get busy living or get busy dying";
            component.FileMask = "*.(xml|txt|edi|x12|dat)(.pgp)?";

            component.Disassemble(pipelineContext, message);

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("Chinese.txt", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("BillingKiller.txt", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("UniShip.xml", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
			Assert.AreEqual("UniversalShipment", actualMessage.Context.Read("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));

			actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("Unitrans_104_SampleEDIFormat_07112019.dat", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("GENMENLOPDX_UNIT_204_33339312_20190823063845.edi", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_PgpInline_ContentType()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.01_PgpInlineFiles_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.PgpSecretKeyInBase64 = "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";
            component.PgpPassphrase = "get busy living or get busy dying";
            component.ContentType = "application/(xml|octet-stream|edifact|edi-x12|edi-consent)|text/(plain|xml)";

            component.Disassemble(pipelineContext, message);

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("3by3", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("Chinese.txt", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("Parts.png", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("ClientBilling.sql", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("BillingKiller.txt", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("104_07082019.pdf", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("UniShip.xml", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("Unitrans_104_SampleEDIFormat_07112019.dat", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual("PGP with Files", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("GENMENLOPDX_UNIT_204_33339312_20190823063845.edi", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_PgpMime_Body()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.02_PgpMimeHtml_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.PgpSecretKeyInBase64 = "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";
            component.PgpPassphrase = "get busy living or get busy dying";
            component.ContentType = "text/plain";

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.02_PgpMimeHtml_Output_Body.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("PGP with attachment", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_PgpMime_Attachment()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.02_PgpMimeHtml_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.PgpSecretKeyInBase64 = "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";
            component.PgpPassphrase = "get busy living or get busy dying";
            component.FileMask = "3by3";

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.3by3");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("PGP with attachment", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("3by3", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_SmimePlainText_Body()
        {
            var rawCertificate = Convert.FromBase64String("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");
            var store = new X509Store (StoreName.My, StoreLocation.CurrentUser);
            var certs = new X509Certificate2Collection ();
            store.Open (OpenFlags.ReadWrite);
            certs.Import(rawCertificate, "CQ_$V6pq", X509KeyStorageFlags.DefaultKeySet);
            store.AddRange (certs);

            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.03_SmimePlainText_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.ContentType = "text/plain";

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.03_SmimePlainText_Output_Body.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("TestPlainTextSMIME", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            store.RemoveRange(certs);
            store.Close();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_SmimePlainText_Attachment()
        {
            var rawCertificate = Convert.FromBase64String("MIILEQIBAzCCCtcGCSqGSIb3DQEHAaCCCsgEggrEMIIKwDCCBXcGCSqGSIb3DQEHBqCCBWgwggVkAgEAMIIFXQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYwDgQI0/fm8iXDYS4CAggAgIIFMB7pXRF3BesDc068GsmKrs8EMPVNkASMss3ToACHLJ4A/1veccRrjQPHeG9mi5fDu8V6z0eqyXM0uR0WZ2CQwjAeE7WZkk/T16LR72qwR/eWEi8ZbijcVNkbEnBZDCzVzFyif5FhARPT6x5mMMpOq5Xq9BVgO2uLEtvhDkEkbNoJTgKaIO5P80sxR+yl5iwKdf9J1d9SW2Hrsk/SsDqq+xpOb800ONHzuhSuSNtUs9f1UZm/JAEuLNE2lXHemPAF92HjK2OJkIPMeboDiLB27/+zQb1PYnDFtAm1NETley5RB8jn05txNy71634ET0hjuwG/fMifD+AebRYV2g0oD0KzDOzMvwAnhLQz5zRwOsAGdT6Q+VQcmR9XFtvkztJUMsyNoSeIT+NOTceyZcRatEKq2k5XhoS/DnvqbELOtmMu19WsiqROREufRw52eGHtQ6N2XqYIXrLw6QCc6WE5X+Im0lKMlmvfG+UO+1SAy9ouAS0QgLn71gKdiJ/GIN3rQI/eij15SqSl5pVyDgOLqZ2v5n+u/LiiIokQ9q3dDZKK7QHJDpry+VCPXIwQdrkSclw/liMvwV7lf+BMtPlr8WoXYBeytp4/9IIcbwAGApMgVJVEc56S9+tgxbbTYlqmqHISsV8F8y1k3+BXge6DRVTIx2cibDZo3cVXhIsWbM/fjWrOVaascwPbf9xWIxmXLZND2ATfwUZhtctuhp9RyDO0F8b1ChsE45mXR1gjsauYBTqDMRgs1/eUxSDmyxhZSP40RVtI6Rs7EiY9PdTxJVx/PUeZ+jdc+gpFOksW7Ju2vrlsQOQwMGTjBZQhohd7aVotrpWZmo7/Wi9dVHtsEJ/eOjxGrOvp9xd9/QhC3XaUjftIXAKmrTYfeUs2Ue0/WwCb2xm0N0jWMrj1/D/EuZV6j94CkE9wWX9orIFwr/4S7G5skyixya9lH/S/duGcH8jzwcHC2VbAaGZX6G6W9Hp2bSEHxPL64pTYL09tf0PY5RLzdKFWHhIkmcB3bDNvPrg5OdJTFvcg/ZzWYj9ugamiWKPzLeV7k+XcQEUqAQaZN1NXUprNCWO1qZbGlIVDtpYMepy5pGxm9yZbmSBZ+EROdHv242BGB3Cbkqrx1cpCDjLjMRFIUwAePi7o7amesY7+7ANdrHdSo+DAFyOr341+YhE4VUprNWM0xN/dNyeO0zfR6+2/s7dQ9bPDDvMyH9K4kMahQ9zMQ3e5d2sDaT4X5QjNz9IBpId1K29pFcnpogOc+lqtcBAsLGkxuxlP4YmioDZfDrFnDj5E3bbwQJhaWTZCsU3Wk7ihgA1TsaUCr03SjUlN/a3KCr+67Ila+MFJEdbEqCJBLAaidFrEDhhvY0NaPtylnGyWlc/rpefSAxwn+f1CkRIEuLHZbYUxm8Dk+ACRnEGE8sTpBZbJOy9OTzJNfUUtwoMvMwsv9+2uSZO3XuV8S+MCB/QpZC8lC0uQLrN2fcsqgm/GQq9qrUhY5UFrcwnvMJ4Ghq/+ByJ1x+N1/HCj9UlMbYgj39JdcJXavkw42kVMXdtFjoFuRPMLPcgvuXalyKy9f/i4vyvKdfjgAIiFQY4DJPdV6l9DXYIUi1ybA+7YdOr/Kf+tlzBuujR2vKnkaqlQZzRFzeiQjS+oaKqq3+xiGqHeRlqP4vNKCH5rd+VIvTbHCHdk0Z8MyhVLw0HlAtF82wHXbYN7YTgSTutiM3lsbuRd7o94KUi5p99divkRbEfgIbugH3IJxBh6WtzaaISpWV60+ilcMIIFQQYJKoZIhvcNAQcBoIIFMgSCBS4wggUqMIIFJgYLKoZIhvcNAQwKAQKgggTuMIIE6jAcBgoqhkiG9w0BDAEDMA4ECJv5eCNdlhWoAgIIAASCBMgXQjgygbvaxd2ytq5zyQFPCGqTz+J4KpuKjgkYQpHE/71BwgbMEgRuS0weNKVcR7kd4gei0IV0Tl1LfOKcQWWSuyqGwjWpHy8Rru56UsvgCX87vwqFF+OEhlSkB7NDBYihHDxHwOUVQROgKNb6SlkpOa7rl2aVV9p5y3WCEyBHwafvPamrlEXF1nJnDGvnfn3F0mIhXW1U5jBuj3FMgmeQ87iBMKtjydJsE88WAUW7d/v0z4qyzyKFgRNLwjJwFJihN0mTpw/I1SC2tK4PciQORTTmSr4Bw53T4PKFjf67VPlhehz7Fq7SdMxrftCy9MmaHONMK37I0s56Ry1QgdMpM4R15dxz4ts5ct5nG2CXvuiVJHcWw0ORSKyFf126zh0024HsviOsFXKJGw/LzJk1c4ZEvUbgUUjxGbuEnTgFTui43Ob06UF2E0BQyniJAnpcvx76Y6A7MI82UHzqb3OkxamizrapLzt6DYRj3H19apSdN7BUGqYv4cfwCIur2hh6Jlxd0jyVZRjzN46oqke5o82qS+N28NG9WKg48bwPNkyV0SDQX2GZCUWQvqPYDxl4/Fh4l5LYl70E8Q785wLQPehl8UhbMFJBJ+dZ5ZPOFyqdvbQlGD8kw0U3Sy73MtZHbt7YXqnj9TAE31JP85WJNH9EqYyEo/EpEJwsTlaGMboOq7H5Yxm8Hi3obvAhpjb0aPsvlUH39xJizjQnzo+j1uqUAe3pqINQbFBP0JGPYA6qKlWbLnLTqmxvCsKDlgdeuB6ofvdFsQWqAIiVlGEhsVrQOepsGVnI7puckFz0BrlCnk5N5x25CvfILNqsXvuKrs5DHHuaNX6Xy9QdoI8c95kdtwuCUGCanV1ZzAqrMSRPIIlPx4T1Un0d8KRToVRvCw+q8BfZ3pDhnFc9CjvQBGW4aH/FGWCfo0Z1Vhs4nYgz5GUzBW18IavgM9X4RpjJiMJ35zCchMN20PHtGHWCJsLEpeZhrLcG0noR2Irclyd3MN5JnOyZVz3hhR1ODn/gymKK8txICXJ6D3kMHsGT3CPOOM4cqNCZcJ0CngAL8o/8+WPtcm+MhimWAmL5/EiQPkVWIGM8OzXN6+iuSVEhuN1K9KuTTgMqJvJwwK9ETX+Oxll45FFyYIpThjf4F57AiDzmWPCBc3va1zpjW4N28EFs1NOjpmRTg/jwh+WTlBwbAwOY4TjAhRY78214iZbwmgFWAAzvnh/JUt6ZETQym1FbH7oGHfOrFUXca0kzJloxVuhFSFDnyhxigMvBSidmCV3+GZ/SV0kPc0EB4T0gcL3YEqemhuPJ1ipiljbj37sYfaPygx274GnX8D90mK9Xt47wpSwfGDE4Odlelmgk14J1+22i87ef56qtw0ae4SSITVXtep9FnbIgn4iqyIrGVE2Olg5wCH1zCdZCPxgXggL9JIJkALBX5jagXxLd2DdgIJRcmhBw+sqRAPi2Hnf+ZFdpI3txkJX0ZbmZnTspdV6bcwBSuHIWqlj3GJuqsnpLLmQfHopzjvXSOOx6oJrVc/VByITb5SuG7Otg6r22tcChIjYapECbdxoUnPLMNfrKg27MC4HBu3feUaYS6VfRDBl+piOZYOOefOSwyFHOQXn/i7Sqm5gxJTAjBgkqhkiG9w0BCRUxFgQU27ei41sy5VgA6uP9SxOUAJpC9o4wMTAhMAkGBSsOAwIaBQAEFKmNfxSayDJm8euGRjPYbsJNOUL4BAgNjT6m9WoG0AICCAA=");
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            var certs = new X509Certificate2Collection();
            store.Open(OpenFlags.ReadWrite);
            certs.Import(rawCertificate, "CQ_$V6pq", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet);
            store.AddRange(certs);

            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.03_SmimePlainText_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.FileMask = "3by3";

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.3by3");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("TestPlainTextSMIME", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("3by3", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));

            store.RemoveRange(certs);
            store.Close();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_HtmlWithFileAndImages_Body()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.04_HtmlWithFileAndImages_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.ContentType = "text/plain";

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.04_HtmlWithFileAndImages_Output_Body.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("TestAttachmentWithAll", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_HtmlWithFileAndImages_Body_ByteDecoder()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.04_HtmlWithFileAndImages_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.ContentType = "text/plain";
            component.ByteDecoderEnabled = true;
            component.ByteDecoderInHex = "0x54686973-0x74484953,0x6573-";

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.04_HtmlWithFileAndImages_Output_Body.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd().Replace("This", "tHIS").Replace("es", ""), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("TestAttachmentWithAll", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMimeDisassemble_HtmlWithFileAndImages_Body_Regex()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.04_HtmlWithFileAndImages_Input.eml");
			message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
			component.ContentType = "text/plain";
			component.RegexEnabled = true;
			component.RegexEncoding = "";
			component.RegexString = @"^[\d\D]{2}**$&HHH??es**??^**<?xml version=""1.0"" ?>";

			component.Disassemble(pipelineContext, message);

			var expectedMessage = MessageFactory.CreateMessage();
			expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
			expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.04_HtmlWithFileAndImages_Output_Body.txt");
			expectedMessage.Context = MessageFactory.CreateMessageContext();

			var actualMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(@"<?xml version=""1.0"" ?>" + new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd().Replace("Hi,", "HiHHH,").Replace("es", ""), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
			Assert.AreEqual("TestAttachmentWithAll", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
			Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_HtmlWithFileAndImages_Attachment()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.04_HtmlWithFileAndImages_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.FileMask = "*.xml";
			component.FileMaskOnly = true;

			component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.CusDec.xml");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("TestAttachmentWithAll", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("CusDec.xml", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_ForeignCharacters_Body()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.05_ForeignCharacters_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.Index = 0;

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.Chinese.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();
            
            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data, Encoding.GetEncoding("iso-2022-jp")).ReadToEnd());
            Assert.AreEqual("TestUTF8", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_ForeignCharacters_Attachment()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.05_ForeignCharacters_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.Index = 1;

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.Chinese.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("TestUTF8", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("Chinese.txt", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_ForeignCharacters_big5_Body()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.06_ForeignCharacters_big5_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.Index = 0;

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.06_ForeignCharacters_big5_Output_Body.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data, Encoding.GetEncoding("big5")).ReadToEnd());
            Assert.AreEqual("TestUTF-8", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMimeDisassemble_ForeignCharacters_big5_Body_Regex()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.06_ForeignCharacters_big5_Input.eml");
			message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
			component.Index = 0;
			component.RegexEnabled = true;
			component.RegexEncoding = "big5";
			component.RegexString = "玠**介??樺**華";

			component.Disassemble(pipelineContext, message);

			var expectedMessage = MessageFactory.CreateMessage();
			expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
			expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.06_ForeignCharacters_big5_Output_Body.txt");
			expectedMessage.Context = MessageFactory.CreateMessageContext();

			var actualMessage = component.GetNext(pipelineContext);
			Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd().Replace("玠樺", "介華"), new StreamReader(actualMessage.BodyPart.Data, Encoding.GetEncoding("big5")).ReadToEnd());
			Assert.AreEqual("TestUTF-8", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
			Assert.AreEqual(null, actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_ForeignCharacters_big5_Attachment()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.06_ForeignCharacters_big5_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.Index = 1;

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.Chinese.txt");
            expectedMessage.Context = MessageFactory.CreateMessageContext();

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
            Assert.AreEqual("TestUTF-8", actualMessage.Context.Read("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06"));
            Assert.AreEqual("Chinese.txt", actualMessage.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06"));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_BypassMimeDecoder()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.06_ForeignCharacters_big5_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.Index = 0;
            component.BypassMimeDecoder = true;

            component.Disassemble(pipelineContext, message);

            var expectedMessage = MessageFactory.CreateMessage();
            expectedMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
            expectedMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.06_ForeignCharacters_big5_Input.eml");
            expectedMessage.Context = MessageFactory.CreateMessageContext();

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(new StreamReader(expectedMessage.BodyPart.Data).ReadToEnd(), new StreamReader(actualMessage.BodyPart.Data).ReadToEnd());
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMimeDisassemble_IgnoreEmptyText()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.07_IgnoreEmptyText_Input.eml");
            message.Context = MessageFactory.CreateMessageContext();

            var component = new MimeDisassembleComponent();
            component.Enable = true;
            component.Index = 0;

            component.Disassemble(pipelineContext, message);

            var actualMessage = component.GetNext(pipelineContext);
            Assert.AreEqual(null, actualMessage);
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMimeDisassemble_FileMaskOnly_NoProperAttachment()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.MimeDisassembleComponent.08_IncorrectFileWithExcel_Input.eml");
			message.Context = MessageFactory.CreateMessageContext();
			var messageTrackingID = Guid.NewGuid().ToString();
			message.Context.WriteProperty<MessageTrackingID>(messageTrackingID);

			var outboxAccessor = MockRepository.StrictMock<IOutboxAccessor>();
			Expect.Call(() => { outboxAccessor.UpdateMessageStatus(null, null, null, null, null); }).IgnoreArguments()
				.Do(new Action<string, string, string, int?, int?>((m, ip, ok, i, o) =>
				{
					Assert.AreEqual(messageTrackingID, m);
					Assert.AreEqual(3, i);
				}));

			var component = MockRepository.PartialMock<MimeDisassembleComponent>();
			Expect.Call(component.GetOutboxAccessor()).Return(outboxAccessor);

			component.Enable = true;
			component.FileMask = "*.csv";
			component.FileMaskOnly = true;

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}
	}
}

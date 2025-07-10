using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.XtCredential
{
	class XtCredentialSenderTest : TestCaseWithFactory
	{
		public void TestCreateInterchange_Overall()
		{
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);

			var provider = new Mock<IxTCredentialProvider>();
			provider.Setup(p => p.RequiredAction).Returns(XtCredentialAction.New);
			provider.Setup(p => p.Factory).Returns(Factory);
			provider.Setup(p => p.Identifier).Returns(XtCredentialConstants.IdentifierList.CLC);

			XtCredentialSender.SendXtCredential(provider.Object);
			var interchange = (IEDIInterchange)Factory.LoadTop1(ObjectFactory.GetType<IEDIInterchange>(), new ZQuery());

			CombineAssertions("Check created EDIInterchange", () =>
			{
				AssertNotNull("Should have created an EDIInterchange", interchange);
				AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.XHCredentialConfig, interchange.EI_ApplicationCode);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
				AssertEquals("EI_To", XtCredentialConstants.CustomsCredentialChange, interchange.EI_To);
				AssertEquals("EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
				AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
				AssertEquals("EI_InterchangeType", XtCredentialConstants.IdentifierList.CLC, interchange.EI_InterchangeType);
				AssertContains("EI_BodyText", "<ChangeType>New</ChangeType>", interchange.EI_BodyText);
				AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, interchange.EI_GB);
			});
		}

		public void TestCreateInterchange_MessageText()
		{
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);

			var provider = new Mock<IxTCredentialProvider>();
			provider.Setup(p => p.LinkTableName).Returns(GlbCompanySchema.Constants.TableName);
			provider.Setup(p => p.LinkUniqueID).Returns(GlbCompany.CurrentCompany.PK);
			provider.Setup(p => p.RequiredAction).Returns(XtCredentialAction.Update);
			provider.Setup(p => p.ChangeDateTime).Returns(new DateTime(2023, 5, 15));
			provider.Setup(p => p.EnterpriseCode).Returns("ENT001");
			provider.Setup(p => p.DatabaseCode).Returns("DB001");
			provider.Setup(p => p.DatabaseNumber).Returns("DBN001");
			provider.Setup(p => p.Password).Returns("PWD");
			provider.Setup(p => p.OldPassword).Returns("OLDPWD");
			provider.Setup(p => p.Factory).Returns(Factory);
			provider.Setup(p => p.Identifier).Returns(XtCredentialConstants.IdentifierList.CLC);

			XtCredentialSender.SendXtCredential(provider.Object);
			var interchange = (IEDIInterchange)Factory.LoadTop1(ObjectFactory.GetType<IEDIInterchange>(), new ZQuery());

			CombineAssertions("Check created EDIInterchange", () =>
			{
				AssertEquals("EI_InterchangeType", XtCredentialConstants.IdentifierList.CLC, interchange.EI_InterchangeType);

				var outputXml = interchange.EI_BodyText;
				AssertContains("Output xml: ChangeType", "<ChangeType>Update</ChangeType>", outputXml);
				AssertContains("Output xml: ChangeDateTime", "<ChangeDateTime>2023-05-15", outputXml);
				AssertContains("Output xml: EnterpriseCode", "<EnterpriseCode>ENT001</EnterpriseCode>", outputXml);
				AssertContains("Output xml: DatabaseCode", "<DatabaseCode>DB001</DatabaseCode>", outputXml);
				AssertContains("Output xml: DatabaseNumber", "<DatabaseNumber>DBN001</DatabaseNumber>", outputXml);
				AssertContains("Output xml: LicenceType", "<LicenceType>TST</LicenceType>", outputXml);
				AssertContains("Output xml: Password", $"<Password>PWD</Password>", outputXml);
				AssertContains("Output xml: OldPassword", $"<OldPassword>OLDPWD</OldPassword>", outputXml);
			});

			var message = (IEDIMessage)Factory.LoadTop1(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			CombineAssertions("Check created EDIMessage", () =>
			{
				AssertEquals("EM_EI", interchange.PK, message.EM_EI);
				AssertEquals("EM_LinkTable", GlbCompanySchema.Constants.TableName, message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", GlbCompany.CurrentCompany.PK, message.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", XtCredentialConstants.IdentifierList.CLC, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", XtCredentialConstants.MessageSubTypes.Update, message.EM_MessageSubType);
			});
		}
	}
}

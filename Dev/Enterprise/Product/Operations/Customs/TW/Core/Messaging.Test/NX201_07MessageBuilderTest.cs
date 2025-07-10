using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX201_07;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class NX201_07MessageBuilderTest : BaseTWMessageBuilderTest<INX201_07, Declaration>
	{
		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new NX201_07MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX201_7.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX201_07MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX201_7_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX201_07 CreateDataSource() => GetMessage();

		protected override ITWMessageBuilder CreateNewMessageBuilder() => new NX201_07MessageBuilder();

		INX201_07 GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<INX201_07>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.IssueDateTime).Returns(ZDateTime.BrettsBirthday.Date);
			mock.Setup(x => x.AdditionalDocument).Returns(GetAdditionalDocument());
			mock.Setup(x => x.AdditionalInformation).Returns(GetAdditionalInformation());
			mock.Setup(x => x.Application).Returns(GetApplication(optionalNodesArePopulated));
			return mock.Object;
		}

		IApplication GetApplication(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IApplication>();
			mock.Setup(x => x.TypeCode).Returns("ABC");
			mock.Setup(x => x.AdditionalDocuments).Returns(GetAdditionalDocuments(optionalNodesArePopulated));
			mock.Setup(x => x.Agent).Returns(GetApplicationAgent());
			mock.Setup(x => x.ContactOffice).Returns("1234567890123456");
			mock.Setup(x => x.Applicant).Returns(GetApplicant(optionalNodesArePopulated));
			return mock.Object;
		}

		IEnumerable<IAdditionalDocument> GetAdditionalDocuments(bool optionalNodesArePopulated)
		{
			var documents = new List<IAdditionalDocument>();
			if (optionalNodesArePopulated)
			{
				var mock = new Mock<IAdditionalDocument>();
				mock.Setup(x => x.ID).Returns(ZString.Replicate('A', 50));
				mock.Setup(x => x.ImageFileFormat).Returns("ABCDE");
				mock.Setup(x => x.ImageFileFormat).Returns(ZString.Replicate('B', 200));
				mock.Setup(x => x.SequenceNumeric).Returns(10);
				mock.Setup(x => x.TypeCode).Returns("AB");
				documents.Add(mock.Object);

				mock = new Mock<IAdditionalDocument>();
				mock.Setup(x => x.ID).Returns(ZString.Replicate('B', 50));
				mock.Setup(x => x.ImageFileFormat).Returns("BCDEF");
				mock.Setup(x => x.ImageFileFormat).Returns(ZString.Replicate('C', 200));
				mock.Setup(x => x.SequenceNumeric).Returns(20);
				mock.Setup(x => x.TypeCode).Returns("BC");
				documents.Add(mock.Object);
			}

			return documents;
		}

		IPartyDetails GetApplicationAgent()
		{
			var agent = new Mock<IPartyDetails>();
			agent.Setup(x => x.ID).Returns(ZString.Replicate('B', 14));
			agent.Setup(x => x.Name).Returns(ZString.Replicate('C', 70));
			agent.Setup(x => x.TypeCode).Returns("AB");
			agent.Setup(x => x.Address).Returns(() =>
			{
				var mock = new Mock<IAddress>();
				mock.Setup(x => x.ChineseLine).Returns(ZString.Replicate('中', 100));
				return mock.Object;
			});

			return agent.Object;
		}

		IPartyDetails GetApplicant(bool optionalNodesArePopulated)
		{
			var applicant = new Mock<IPartyDetails>();
			applicant.Setup(x => x.ID).Returns(ZString.Replicate('D', 14));
			applicant.Setup(x => x.TypeCode).Returns("BCD");
			applicant.Setup(x => x.AdditionalInformations).Returns(() =>
			{
				var additionalInformations = new List<IAdditionalInformation>();
				if (optionalNodesArePopulated)
				{
					var mock = new Mock<IAdditionalInformation>();
					mock.Setup(x => x.StatementCode).Returns("ABC");
					mock.Setup(x => x.StatementCode).Returns(ZString.Replicate('A', 35));
					additionalInformations.Add(mock.Object);

					mock = new Mock<IAdditionalInformation>();
					mock.Setup(x => x.StatementCode).Returns("DEF");
					mock.Setup(x => x.StatementCode).Returns(ZString.Replicate('B', 35));
					additionalInformations.Add(mock.Object);
				}
				return additionalInformations;
			});

			return applicant.Object;
		}

		IAdditionalInformation GetAdditionalInformation()
		{
			var mock = new Mock<IAdditionalInformation>();
			mock.Setup(x => x.Content).Returns(ZString.Replicate('A', 240));
			return mock.Object;
		}

		IDeclarationAdditionalDocument GetAdditionalDocument()
		{
			var mock = new Mock<IDeclarationAdditionalDocument>();
			mock.Setup(x => x.ID).Returns("12345678901234");
			mock.Setup(x => x.LPCOExpirationDateTime).Returns(ZDateTime.BrettsBirthday);
			return mock.Object;
		}
	}
}

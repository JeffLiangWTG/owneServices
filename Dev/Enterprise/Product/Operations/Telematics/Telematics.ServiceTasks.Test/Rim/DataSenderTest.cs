using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks.Rim;
using Moq;
using WTG.Telematics.Common;

namespace Enterprise.Telematics.ServiceTasks.Test.Rim
{
	class DataSenderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			eHubMessageSenderMock = new Mock<IEHubMessageSender>();
			dataSender = new DataSender(eHubMessageSenderMock.Object);
		}

		public void TestDataSenderPassesDataToXHubGateway()
		{
			var pass = "Pass-123";
			var url = "https://tde-dev.tca.gov.au/rest/";
			TelematicsConfigurationRegistry.Instance.TcaRimPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pass);
			var expectedPass = TelematicsConfigurationRegistry.Instance.TcaRimPassword.Value;
			TelematicsConfigurationRegistry.Instance.TcaRimUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, url);

			CombineAssertions(
				() =>
				{
					Test(
						new[]
						{
							new PortionedDataStub { BatchId = "1", Message = "Hello" }
						},
						new[]
						{
							new TelematicsXtRimMessage
							{
								JsonData = "Hello",
								TcaAddress = url,
								TcaBatchId = "1",
								TcaPassword = expectedPass,
								TcaUsername = "tdewtg2",
							},
						});
					Test(
						new[]
						{
							new PortionedDataStub { BatchId = "1", Message = "Hello" },
							new PortionedDataStub { BatchId = "2", Message = "Goodbye" },
						},
						new[]
						{
							new TelematicsXtRimMessage
							{
								JsonData = "Hello",
								TcaAddress = url,
								TcaBatchId = "1",
								TcaPassword = expectedPass,
								TcaUsername = "tdewtg2",
							},
							new TelematicsXtRimMessage
							{
								JsonData = "Goodbye",
								TcaAddress = url,
								TcaBatchId = "2",
								TcaPassword = expectedPass,
								TcaUsername = "tdewtg2",
							},
						});
				});

			void Test(IEnumerable<IPortionedData> messages, IEnumerable<TelematicsXtRimMessage> expectedMessages)
			{
				// Arrange
				var recipients = new List<string>();
				var sentMessages = new List<string>();

				eHubMessageSenderMock.Reset();
				eHubMessageSenderMock.Setup(
					sender => sender.Send(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<IEnumerable<string>>(),
						It.IsAny<IEnumerable<string>>()))
					.Callback<BusinessObjectFactory, IEnumerable<string>, IEnumerable<string>>(
						(_, sentMessagesParam, recipientsParam) =>
						{
							sentMessages = sentMessagesParam.ToList();
							recipients = recipientsParam.ToList();
						});

				// Act
				dataSender.Send(Factory, messages);

				// Assert
				var decodedSentMessages = new List<TelematicsXtRimMessage>();
				foreach (var sentMessage in sentMessages)
				{
					var element = XElement.Parse(sentMessage);
					var messageText = element.Nodes().SingleOrDefault()?.ToString();
					var rimMessage = XmlDataSerializer.Deserialize<TelematicsXtRimMessage>(messageText);
					rimMessage.TcaPassword = DecodePassword(rimMessage.TcaPassword);
					decodedSentMessages.Add(rimMessage);
				}
				AssertContainsExactElementsInAnyOrder(new TelematicsXtRimMessageEqualityComparer(), expectedMessages, decodedSentMessages);
				AssertContainsExactElementsInAnyOrder(new[] { "XHUB_Telematics_RIM" }, recipients);

				eHubMessageSenderMock.Verify(
					sender => sender.Send(
						Factory,
						It.IsAny<IEnumerable<string>>(),
						It.IsAny<IEnumerable<string>>()),
					Times.Once);
				eHubMessageSenderMock.VerifyNoOtherCalls();
			}
		}

		class TelematicsXtRimMessageEqualityComparer : IEqualityComparer<TelematicsXtRimMessage>
		{
			public bool Equals(TelematicsXtRimMessage x, TelematicsXtRimMessage y)
			{
				return x.TcaPassword == y.TcaPassword &&
					x.JsonData == y.JsonData &&
					x.TcaAddress == y.TcaAddress &&
					x.TcaBatchId == y.TcaBatchId &&
					x.TcaUsername == y.TcaUsername;
			}

			public int GetHashCode(TelematicsXtRimMessage obj)
			{
				throw new NotImplementedException();
			}
		}

		string DecodePassword(string password)
		{
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				var keyXml = "<RSAKeyValue><Modulus>1XmSsYj8fup2QDmo2Yp0nKVzNsonnECcN2OcZXjF+eIEgQyLydnwngBcdW7UOwqQ0E+ffFcdoW44IaTVZ/A7yNC/pRJQy42BBcXteSo7SqgHt7yvPUph+oFpEWLIe0WnPJDmZCRFNoZnMUoOzK89oA/v0I3neveYHjM0bRgtVBE=</Modulus><Exponent>AQAB</Exponent><P>5sXEF2kBcvgWJ1FSo1rahov1KiqogxqBOJio3FmsLsjafwV9O1xKGmjuL1gdQbgd7rPI7x8b42PTfGctlAIPdQ==</P><Q>7M+6YC9HcPeNvr6/S+jX7tP+OV6Xljqe/U9/j7fisEG/9cfFz9lh+H+oItB1MpOD/dKB3RNW5/ODA5TSsHsarQ==</Q><DP>PQtja63jLD5j3dKtQXjvBVhQae8O1F9Wf1oikOdHnLiU07ToA6POFl5bYzqzwoappFL6fAaGogfuEaJZdCV3YQ==</DP><DQ>5iFYtXA8tQNdtCgaLuKwNV++hnHuTgfZycEf7cJ9gVvj+C2ThlFya9NiybJasjO46UlQ+k54/iAfCbPuq6J2YQ==</DQ><InverseQ>k3hB5HGpUKFMYBO+Dh7dkJJ6z7TGKevKWHw3Rhv6aYFOqVvDBKQyW52pbh4aHv5eX15QrBJNPnXYmovSYj47oQ==</InverseQ><D>A/CjvA+bcMCP5f+6cFNtc2OxWe+cELaiO3p6QpGFU+dE7oMmWazM/lmNhfmsRJpdZwmFLR9oKQMsBDZIMwwnH5/WD+gso3VmPRsg5BIdoOs/F0ZkR1qeUlwbjmaD+fPV4b8Tg/3JVeSWT7K8xtKGMLWDcdLMA3vaRo/1/g+KfD0=</D></RSAKeyValue>";
				var text = Convert.FromBase64String(password);
				rsa.FromXmlString(keyXml);
				return new string(System.Text.Encoding.UTF8.GetChars(rsa.Decrypt(text, true)));
			}
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => dataSender.Send(null, new[] { new Mock<IPortionedData>().Object }));
				AssertEquals("factory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => dataSender.Send(Factory, null));
				AssertEquals("data", result.ParamName);
			});
		}

		public void TestWrongCtorParams()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => new DataSender(null));
			AssertEquals("eHubMessageSender", result.ParamName);
		}

		class PortionedDataStub : IPortionedData
		{
			public string BatchId { get; set; }

			public string Message { get; set; }
		}

		DataSender dataSender;
		Mock<IEHubMessageSender> eHubMessageSenderMock;
	}
}

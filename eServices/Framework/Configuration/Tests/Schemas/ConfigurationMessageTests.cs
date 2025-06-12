using System;
using System.IO;
using System.Xml;
using eServices.Configuration.Tests;
using eServices.Configuration.Schemas;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace eServices.Configuration.Tests.Schemas
{
	public class ConfigurationMessageTests : TestBase
	{
		[Test]
		public void Schemas_ConfigurationMessage_GetXsd()
		{
			var expected = GetEmbeddedResourceString("Configuration.xsd");
			var actual = new StreamReader(ConfigurationMessage.GetXsd()).ReadToEnd();

			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void Schemas_ConfigurationMessage_Serialization()
		{
			var configuration = new ConfigurationMessage
			{
				Name = "CONFIG",
				Version = "1.0",
				Timestamp = DateTime.Parse("2018-02-25T16:28:09"),
				TimestampSpecified = true,
				Annotations = new[]
				{
					new Item { Name = "ANN1", Value = "aaaaaa" },
					new Item { Name = "ANN2", Value = "bbbbbb" },
				},
				Group = new[]
				{
					new Group
					{
						Type = "TOP",
						Reference = "AAAAAA",
						Status = "Valid",
						Items = new object[]
						{
							new Group
							{
								Type = "SUB1",
								Reference = "BBBBBB",
								Status = "Current",
								Items = new object[]
								{
									new Credential { Name = "Current", UserName = "WTGDEC17", Password = Convert.FromBase64String("cGFzc3dvcmQ=") },
									new Credential { Name = "NextPassword", Password = Convert.FromBase64String("bmV4dHBhc3M=") },
									new Item { Name = "ITEM", Value = "23123456789-002" },
									new Certificate
									{
										Name = "CERT",
										File = new Configuration.Schemas.File { Name = "KEY", Filename = "key.cer", Value = Convert.FromBase64String("bWFzZGY5OHB1MzEyNW0vJ2FzZGZrbmxhc2RmMDlhc2RmamtsbWRmYXM=") },
										Passphrase = Convert.FromBase64String("bmV4dHBhc3M="),
										ActiveFrom = DateTime.Parse("2018-02-25T16:20:39"),
										ActiveFromSpecified = true,
									},
									new FTP
									{
										Server = "server",
										Port = 21,
										PortSpecified = true,
										UserName = "username",
										Password = Convert.FromBase64String("bmV4dHBhc3M="),
										SendFolder = "send/folder",
										ReceiveFolder = "receive/folder",
										ActiveFrom = DateTime.Parse("2018-02-25T16:20:56.1941094+11:00"),
										ActiveFromSpecified = true,
									},
									new Configuration.Schemas.File { Name = "FILE", Filename = "file.name", Value = Convert.FromBase64String("bWFzZGY5OHB1MzEyNW0vJ2FzZGZrbmxhc2RmMDlhc2RmamtsbWRmYXM=") }
								},
							},
							new Group
							{
								Type = "SUB1",
								Reference = "CCCCCC",
								Status = "Pending",
								Items = new object[]
								{
									new Credential { UserName = "WTGJAN18", Password = Convert.FromBase64String("cGFzc3dvcmQ=") }
								}
							},
						}
					}
				}
			};

			var serializeResult = ConfigurationMessage.SerializeToXmlDocument(configuration);

			AssertXmlAreEqual("Test1_Serialization.xml", serializeResult);
		}

		[Test]
		public void Schemas_ConfigurationMessage_Deserialization()
		{
			var expectedConfiguration = new ConfigurationMessage
			{
				Name = "CONFIG",
				Version = "1.0",
				Timestamp = DateTime.Parse("2018-02-25T16:28:09"),
				TimestampSpecified = true,
				Annotations = new[]
				{
					new Item { Name = "ANN1", Value = "aaaaaa" },
					new Item { Name = "ANN2", Value = "bbbbbb" },
				},
				Group = new[]
				{
					new Group
					{
						Type = "TOP",
						Reference = "AAAAAA",
						Status = "Valid",
						Items = new object[]
						{
							new Group
							{
								Type = "SUB1",
								Reference = "BBBBBB",
								Status = "Current",
								Items = new object[]
								{
									new Credential { Name = "Current", UserName = "WTGDEC17", Password = Convert.FromBase64String("cGFzc3dvcmQ=") },
									new Credential { Name = "NextPassword", Password = Convert.FromBase64String("bmV4dHBhc3M=") },
									new Item { Name = "ITEM", Value = "23123456789-002" },
									new Certificate
									{
										Name = "CERT",
										File = new Configuration.Schemas.File { Name = "KEY", Filename = "key.cer", Value = Convert.FromBase64String("bWFzZGY5OHB1MzEyNW0vJ2FzZGZrbmxhc2RmMDlhc2RmamtsbWRmYXM=") },
										Passphrase = Convert.FromBase64String("bmV4dHBhc3M="),
										ActiveFrom = DateTime.Parse("2018-02-25T16:20:39"),
										ActiveFromSpecified = true,
									},
									new FTP
									{
										Server = "server",
										Port = 21,
										PortSpecified = true,
										UserName = "username",
										Password = Convert.FromBase64String("bmV4dHBhc3M="),
										SendFolder = "send/folder",
										ReceiveFolder = "receive/folder",
										ActiveFrom = DateTime.Parse("2018-02-25T16:20:56.1941094+11:00"),
										ActiveFromSpecified = true,
									},
									new Configuration.Schemas.File { Name = "FILE", Filename = "file.name", Value = Convert.FromBase64String("bWFzZGY5OHB1MzEyNW0vJ2FzZGZrbmxhc2RmMDlhc2RmamtsbWRmYXM=") }
								},
							},
							new Group
							{
								Type = "SUB1",
								Reference = "CCCCCC",
								Status = "Pending",
								Items = new object[]
								{
									new Credential { UserName = "WTGJAN18", Password = Convert.FromBase64String("cGFzc3dvcmQ=") }
								}
							},
						}
					}
				}
			};

			var inputXml = new XmlDocument();
			inputXml.LoadXml(GetEmbeddedResourceString("Test2_Deserialization.xml"));

			var result = ConfigurationMessage.DeserializeFromXmlDocument(inputXml);

			var comparer = new CompareLogic();
			var compareResult = comparer.Compare(expectedConfiguration, result);
			Assert.That(compareResult.AreEqual, Is.True, compareResult.DifferencesString);
		}

		[Test]
		public void Schemas_ConfigurationMessage_Deserialization_EmptyPassword()
		{
			var expectedConfiguration = new ConfigurationMessage
			{
				Name = "Config",
				Version = "1.0",
				Group = new[]
				{
					new Group
					{
						Type = "Test",
						Items = new object[]
						{
							new Credential { Password = new byte[0] },
							new Credential { Password = new byte[0] },
							new Credential { Password = null }
						}
					}
				}
			};

			var inputXml = new XmlDocument();
			inputXml.LoadXml(GetEmbeddedResourceString("Test3_Deserialization_EmptyPassword.xml"));

			var result = ConfigurationMessage.DeserializeFromXmlDocument(inputXml);

			var comparer = new CompareLogic();
			var compareResult = comparer.Compare(expectedConfiguration, result);
			Assert.That(compareResult.AreEqual, Is.True, compareResult.DifferencesString);
		}

		[Test]
		public void Schemas_ConfigurationMessage_Clone()
		{
			var inputXml = new XmlDocument();
			inputXml.LoadXml(GetEmbeddedResourceString("Test1_Serialization.xml"));

			var original = ConfigurationMessage.DeserializeFromXmlDocument(inputXml);
			var clone = ConfigurationMessage.Clone(original);

			Assert.That(clone, Is.Not.SameAs(original));
			var comparer = new CompareLogic();
			var compareResult = comparer.Compare(original, clone);
			Assert.That(compareResult.AreEqual, Is.True, compareResult.DifferencesString);
		}
	}
}

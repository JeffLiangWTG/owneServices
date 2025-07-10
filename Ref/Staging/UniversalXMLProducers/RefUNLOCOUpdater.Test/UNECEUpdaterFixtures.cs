using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	[TestFixture]
	[Platform("64-bit", Reason = "Only support 64 bit ODBC driver")]
	[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
	public class UNECEUpdaterFixtures
	{
		[Test]
		public void UpdaterXmlResult()
		{
			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.loc2023-2mdb.zip", mdbDownloadPath, GetType());
			var updater = new UNECEUpdater(safeRepositoryMock.Object);
			updater.Read(mdbFile);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(iataFilePath);
			var xmlParser = new UXMLParser(updater.UNLOCOes, iataUpdater.IATAs, xmlWriter, xmlWriterWrongOnes, xmlWriterWithCoordinates);
			xmlParser.Parse();
			xmlWriter.SaveXml(dumpUnlocoPath);
			xmlWriterWithCoordinates.SaveXml(dumpUnlocoWithCoordinatesPath);
			var unlocoXml = new XmlDocument();
			unlocoXml.Load(dumpUnlocoPath);
			Assert.That(unlocoXml != null);
			var unlocoes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			Assert.That(unlocoes.Count > 0);
			var unloco = unlocoes.Item(0);
			Assert.That(unloco.ChildNodes[0].Name, Is.EqualTo("RL_Code"));
			Assert.That(unloco.ChildNodes[1].Name, Is.EqualTo("RL_HasAirport"));
			Assert.That(unloco.ChildNodes[2].Name, Is.EqualTo("RL_HasBorderCrossing"));
			Assert.That(unloco.ChildNodes[3].Name, Is.EqualTo("RL_HasPost"));
			Assert.That(unloco.ChildNodes[4].Name, Is.EqualTo("RL_HasRail"));
			Assert.That(unloco.ChildNodes[5].Name, Is.EqualTo("RL_HasRoad"));
			Assert.That(unloco.ChildNodes[6].Name, Is.EqualTo("RL_HasSeaport"));
			Assert.That(unloco.ChildNodes[7].Name, Is.EqualTo("RL_IATA"));
			Assert.That(unloco.ChildNodes[8].Name, Is.EqualTo("RL_IATARegionCode"));
			Assert.That(unloco.ChildNodes[9].Name, Is.EqualTo("RL_IsActive"));
			Assert.That(unloco.ChildNodes[10].Name, Is.EqualTo("RL_NameWithDiacriticals"));
			Assert.That(unloco.ChildNodes[11].Name, Is.EqualTo("RL_PortName"));
			Assert.That(unloco.ChildNodes[12].Name, Is.EqualTo("RL_RN_NKCountryCode"));
			Assert.That(unloco.ChildNodes[13].Name, Is.EqualTo("RL_RW_NKCode"));
			Assert.That(unloco.ChildNodes[14].Name, Is.EqualTo("RL_RW_RN_NKCountryCode"));
			Assert.That(unloco.ChildNodes[0].InnerText, Is.EqualTo("AOPSA"));
			Assert.That(unloco.ChildNodes[1].InnerText, Is.EqualTo("False"));
			Assert.That(unloco.ChildNodes[2].InnerText, Is.EqualTo("False"));
			Assert.That(unloco.ChildNodes[3].InnerText, Is.EqualTo("False"));
			Assert.That(unloco.ChildNodes[4].InnerText, Is.EqualTo("False"));
			Assert.That(unloco.ChildNodes[5].InnerText, Is.EqualTo("False"));
			Assert.That(unloco.ChildNodes[6].InnerText, Is.EqualTo("True"));
			Assert.That(string.IsNullOrEmpty(unloco.ChildNodes[7].InnerText));
			Assert.That(string.IsNullOrEmpty(unloco.ChildNodes[8].InnerText));
			Assert.That(unloco.ChildNodes[9].InnerText, Is.EqualTo("True"));
			Assert.That(unloco.ChildNodes[10].InnerText, Is.EqualTo("Porto Saco (Portosalazar)"));
			Assert.That(unloco.ChildNodes[11].InnerText, Is.EqualTo("Porto Saco (Portosalazar)"));
			Assert.That(unloco.ChildNodes[12].InnerText, Is.EqualTo("AO"));
			Assert.That(string.IsNullOrEmpty(unloco.ChildNodes[13].InnerText));
			Assert.That(string.IsNullOrEmpty(unloco.ChildNodes[14].InnerText));
			var rlNode = unlocoXml.SelectSingleNode("//RL_Code[(text() = 'CHBSA')]");
			Assert.That(rlNode != null);
			var refUNNode = rlNode.ParentNode;
			Assert.That(refUNNode != null);
			var nkCodeNode = refUNNode.SelectSingleNode("//RL_RW_NKCode[(text() = 'JU')]");
			Assert.That(nkCodeNode != null);
			unlocoXml.Load(dumpUnlocoWithCoordinatesPath);
			unloco = unlocoes.Item(0);
			Assert.That(unloco.ChildNodes[0].Name, Is.EqualTo("RL_Code"));
			Assert.That(unloco.ChildNodes[1].Name, Is.EqualTo("RL_CoOrdinates"));
			Assert.That(unloco.ChildNodes[2].Name, Is.EqualTo("RL_GeoLocation"));
			Assert.That(unloco.ChildNodes[0].InnerText, Is.EqualTo("AOPSA"));
			Assert.That(unloco.ChildNodes[1].InnerText, Is.EqualTo("1508S 01208E"));
			Assert.That(unloco.ChildNodes[2].InnerText, Is.EqualTo("POINT (12.133 -15.133)"));
			var unlocoSchema = unlocoXml.GetElementsByTagName("Schema")[0];
			Assert.That(unlocoSchema != null);
			var unlocoEntityTypeNode = unlocoSchema.SelectSingleNode("EntityType");
			Assert.That(unlocoEntityTypeNode != null);
			Assert.That(unlocoEntityTypeNode.Attributes["Name"].Value, Is.EqualTo("RefUNLOCO"));
			var dataValue = unlocoEntityTypeNode.Attributes["Data"] != null ? unlocoEntityTypeNode.Attributes["Data"].Value : "false";
			Assert.That(dataValue, Is.EqualTo("false"));
			var propertyNodeList = unlocoEntityTypeNode.SelectNodes("Property");
			Assert.That(propertyNodeList.Count > 0);
			Assert.That(propertyNodeList[1].Attributes["Data"].Value, Is.EqualTo("true"));
			Assert.That(propertyNodeList[2].Attributes["Data"].Value, Is.EqualTo("true"));
		}

		[Test]
		public void PortNameHasOnlyEnglishNames()
		{
			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.mdbNotEnglishNames.zip", mdbDownloadPath, GetType());
			var updater = new UNECEUpdater(safeRepositoryMock.Object);
			updater.Read(mdbFile);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(iataFilePath);
			var xmlParser = new UXMLParser(updater.UNLOCOes, iataUpdater.IATAs, xmlWriter, xmlWriterWrongOnes, xmlWriterWithCoordinates);
			xmlParser.Parse();
			xmlWriter.SaveXml(dumpUnlocoPath);
			var unlocoXml = new XmlDocument();
			unlocoXml.Load(dumpUnlocoPath);
			Assert.That(unlocoXml != null);
			var unlocoes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			Assert.That(unlocoes.Count > 0);
			var portNames = unlocoes.Cast<XmlNode>().Select(o => o["RL_PortName"]);
			foreach (var port in portNames)
			{
				Assert.True(Regex.IsMatch(port.InnerText, @"^[a-zA-Z0-9\/\s-()']*$"));
			}
		}

		[Test]
		public void IATACodesInExistingAirports()
		{
			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.loc172mdb.zip", mdbDownloadPath, GetType());
			var updater = new UNECEUpdater(safeRepositoryMock.Object);
			updater.Read(mdbFile);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(iataFilePath);
			var xmlParser = new UXMLParser(updater.UNLOCOes, iataUpdater.IATAs, xmlWriter, xmlWriterWrongOnes, xmlWriterWithCoordinates);
			xmlParser.Parse();
			xmlWriter.SaveXml(dumpUnlocoPath);
			var unlocoXml = new XmlDocument();
			unlocoXml.Load(dumpUnlocoPath);
			Assert.That(unlocoXml != null);
			var unlocoes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			Assert.That(unlocoes.Count > 0);
			var unloco = unlocoes.Cast<XmlNode>().FirstOrDefault(p => p["RL_HasAirport"].InnerText == "True" && !string.IsNullOrEmpty(p["RL_IATA"].InnerText));
			Assert.That(unloco.ChildNodes[0].Name == "RL_Code");
			Assert.That(unloco.ChildNodes[7].Name == "RL_IATA");
			Assert.That(unloco.ChildNodes[8].Name == "RL_IATARegionCode");
			Assert.That(!string.IsNullOrEmpty(unloco.ChildNodes[0].InnerText));
			Assert.That(!string.IsNullOrEmpty(unloco.ChildNodes[8].InnerText));
			Assert.That(!string.IsNullOrEmpty(unloco.ChildNodes[9].InnerText));
		}

		[Test]
		public void NoIATAInformationInWrongUNLOCO()
		{
			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.loc172mdb.zip", mdbDownloadPath, GetType());
			var updater = new UNECEUpdater(safeRepositoryMock.Object);
			updater.Read(mdbFile);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(iataFilePath);
			var xmlParser = new UXMLParser(updater.UNLOCOes, iataUpdater.IATAs, xmlWriter, xmlWriterWrongOnes, xmlWriterWithCoordinates);
			xmlParser.Parse();
			xmlWriterWrongOnes.SaveXml(dumpUnlocoWrongOnesPath);
			var unlocoXml = new XmlDocument();
			unlocoXml.Load(dumpUnlocoWrongOnesPath);
			Assert.That(unlocoXml != null);
			var unlocoes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			Assert.That(unlocoes.Count > 0);
			var unloco = unlocoes.Item(0);
			Assert.That(!unloco.ChildNodes.Cast<XmlNode>().Any(o => o.Name == "RL_IATA"));
			Assert.That(!unloco.ChildNodes.Cast<XmlNode>().Any(o => o.Name == "RL_IATARegionCode"));
		}

		[Test]
		public void IsExcludingExpectedStatusFromTheQuery()
		{
			var whereClause = UNECEUpdater.MDBWhereClause;
			Assert.True(!string.IsNullOrEmpty(whereClause));
			var idx = whereClause.LastIndexOf("NOT IN") + "NOT IN".Length;
			var notInPartial = whereClause.Substring(idx, whereClause.Length - idx);
			notInPartial = notInPartial.Replace("(", "").Replace(")", "").Replace("'", "").Trim();
			var splitStatus = notInPartial.Split(',');
			var expectedStatus = new string[] { "XX", "UR", "RR" };
			var notExpectedStatus = new string[] { "AA", "AC", "AF", "AI", "AM", "AQ", "AS", "QQ", "RL", "RN", "RQ" };
			foreach (var status in splitStatus)
			{
				Assert.True(expectedStatus.Contains(status.Trim()));
				Assert.False(notExpectedStatus.Contains(status.Trim()));
			}
		}

		[Test]
		public void IsIncludingHeliportIATA()
		{
			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.WI00216449Fixture.zip", mdbDownloadPath, GetType());
			var updater = new UNECEUpdater(safeRepositoryMock.Object);
			updater.Read(mdbFile);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(iataFilePath);
			var xmlParser = new UXMLParser(updater.UNLOCOes, iataUpdater.IATAs, xmlWriter, xmlWriterWrongOnes, xmlWriterWithCoordinates);
			xmlParser.Parse();
			xmlWriter.SaveXml(dumpUnlocoPath);
			var unlocoXml = new XmlDocument();
			unlocoXml.Load(dumpUnlocoPath);
			Assert.That(unlocoXml != null);
			var unlocoNodes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			var matched = unlocoNodes.Cast<XmlNode>().FirstOrDefault(o => o.ChildNodes[0].InnerText == "AUKAH");
			Assert.IsNotNull(matched);
			Assert.True(matched.ChildNodes[7].InnerText.Trim() == "KAH");
			Assert.True(matched.ChildNodes[8].InnerText.Trim() == "MEL");
			xmlWriterWithCoordinates.SaveXml(dumpUnlocoWithCoordinatesPath);
			unlocoXml.Load(dumpUnlocoWithCoordinatesPath);
			Assert.That(unlocoXml != null);
			unlocoNodes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			Assert.False(unlocoNodes.Cast<XmlNode>().Any(o => o.ChildNodes[0].InnerText == "AUKAH"));
		}

		[Test]
		public void IsCorrectUNLOCO()
		{
			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.WI00216449Fixture.zip", mdbDownloadPath, GetType());
			var updater = new UNECEUpdater(safeRepositoryMock.Object);
			updater.Read(mdbFile);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(iataFilePath);
			var xmlParser = new UXMLParser(updater.UNLOCOes, iataUpdater.IATAs, xmlWriter, xmlWriterWrongOnes, xmlWriterWithCoordinates);
			xmlParser.Parse();
			xmlWriter.SaveXml(dumpUnlocoPath);
			var unlocoXml = new XmlDocument();
			unlocoXml.Load(dumpUnlocoPath);
			Assert.That(unlocoXml != null);
			var unlocoNodes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			var matched = unlocoNodes.Cast<XmlNode>().FirstOrDefault(o => o.ChildNodes[0].InnerText == "CAMTR");
			Assert.IsNotNull(matched);
			Assert.True(matched.ChildNodes[7].InnerText.Trim() == "YUL");
			Assert.True(matched.ChildNodes[8].InnerText.Trim() == "YMQ");
		}

		[Test]
		public void IsIATAMappedMultipleUNLOCO()
		{
			var mdbFile = MdbTestHelper.DownloadMdbFile("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.WI00216449Fixture.zip", mdbDownloadPath, GetType());
			var updater = new UNECEUpdater(safeRepositoryMock.Object);
			updater.Read(mdbFile);
			var iataUpdater = new IATAUpdater();
			iataUpdater.Read(iataFilePath);
			var xmlParser = new UXMLParser(updater.UNLOCOes, iataUpdater.IATAs, xmlWriter, xmlWriterWrongOnes, xmlWriterWithCoordinates);
			xmlParser.Parse();
			xmlWriter.SaveXml(dumpUnlocoPath);
			var unlocoXml = new XmlDocument();
			unlocoXml.Load(dumpUnlocoPath);
			Assert.That(unlocoXml != null);
			var unlocoNodes = unlocoXml.GetElementsByTagName("RefUNLOCO");
			var matched1 = unlocoNodes.Cast<XmlNode>().FirstOrDefault(o => o.ChildNodes[0].InnerText == "CADOR");
			Assert.IsNotNull(matched1);
			Assert.True(matched1.ChildNodes[7].InnerText.Trim() == "YUL");
			Assert.True(matched1.ChildNodes[8].InnerText.Trim() == "YMQ");
			var matched2 = unlocoNodes.Cast<XmlNode>().FirstOrDefault(o => o.ChildNodes[0].InnerText == "CAYUL");
			Assert.IsNotNull(matched2);
			Assert.True(matched2.ChildNodes[7].InnerText.Trim() == "YUL");
			Assert.True(matched2.ChildNodes[8].InnerText.Trim() == "YMQ");
			var matched3 = unlocoNodes.Cast<XmlNode>().FirstOrDefault(o => o.ChildNodes[0].InnerText == "CAMRB");
			Assert.IsNotNull(matched3);
			Assert.True(matched3.ChildNodes[7].InnerText.Trim() == "YMX");
			Assert.True(matched3.ChildNodes[8].InnerText.Trim() == "YMQ");
		}

		[SetUp]
		public void SetUp()
		{
			binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			dumpUnlocoPath = Path.Combine(binPath, "8CBEF76B-E605-478A-A8DB-701513D93E0C_dump", "unloco_full.xml");
			dumpUnlocoWithCoordinatesPath = Path.Combine(binPath, "8CBEF76B-E605-478A-A8DB-701513D93E0C_dump", "unloco_coordinates.xml");
			dumpUnlocoWrongOnesPath = Path.Combine(binPath, "8CBEF76B-E605-478A-A8DB-701513D93E0C_dump", "unloco_wrongones.xml");
			mdbDownloadPath = Path.Combine(binPath, "TestDownloads_8FC081F37ADB4288BAA8795020CC0FC8");
			iataFilePath = Path.Combine(binPath, @"ref\stations_specific_file.dat");
			safeRepositoryMock = new Mock<ISafeRepository>();
			xmlWriter = new RefDbRepo.Common.UniversalXmlWriter.XmlWriter(XmlWriterHelper.GetRefUNLOCOWriterConfiguration(false));
			xmlWriterWrongOnes = new RefDbRepo.Common.UniversalXmlWriter.XmlWriter(XmlWriterHelper.GetRefUNLOCOWriterConfiguration(false, false));
			xmlWriterWithCoordinates = new RefDbRepo.Common.UniversalXmlWriter.XmlWriter(XmlWriterHelper.GetRefUNLOCOWriterConfiguration(true));
			var publicationTime = DateTime.UtcNow;
			XmlWriterHelper.SetXMLWriter(xmlWriter, "UNLOCO UPDATER", publicationTime);
			XmlWriterHelper.SetXMLWriter(xmlWriterWrongOnes, "UNLOCO WRONG WITHOUT IATA", publicationTime);
			XmlWriterHelper.SetXMLWriter(xmlWriterWithCoordinates, "UNLOCO UPDATER With Coordinates", publicationTime);
			safeRepositoryMock.Setup(x => x.Get<RefUNLOCO>()).Returns(new List<RefUNLOCO>().AsQueryable());
		}

		Mock<ISafeRepository> safeRepositoryMock;
		IXmlWriter xmlWriter;
		IXmlWriter xmlWriterWithCoordinates;
		IXmlWriter xmlWriterWrongOnes;
		string binPath;
		string dumpUnlocoPath;
		string dumpUnlocoWrongOnesPath;
		string dumpUnlocoWithCoordinatesPath;
		string mdbDownloadPath;
		string iataFilePath;
	}
}

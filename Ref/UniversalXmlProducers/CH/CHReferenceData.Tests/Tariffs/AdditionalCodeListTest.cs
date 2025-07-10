using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	public class AdditionalCodeListTest
	{
		[Test]
		public void GetDescriptionHashCode_Length()
		{
			var list = new AdditionalCodeList();
			var code = list.GetDescriptionHashCode("Some description");
			Assert.That(code.Length, Is.Not.GreaterThan(15), "Code may not be longer than 15 characters");
		}

		[Test]
		public void GetDescriptionHashCode_Uniqueness()
		{
			var tariffData = XDocument.Parse("CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs.TestFiles.Input.edecTariffMasterData_1_0.xml".ReadManifestResourceContent());
			var hashCodes = new Dictionary<string, string>();
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("t", tariffData.Root.Name.NamespaceName);
			var list = new AdditionalCodeList();
			foreach (var textElement in tariffData.XPathSelectElements(@"//t:commodityCodes/t:commodityCode/t:rate/t:text[@language='D']", namespaceManager))
			{
				var description = textElement.Attribute("value").Value;
				var hashCode = list.GetDescriptionHashCode(description).ToUpperInvariant();
				if (!hashCodes.ContainsKey(description))
				{
					hashCodes.Add(description, hashCode);
				}
				else
				{
					Assert.That(hashCodes[description], Is.EqualTo(hashCode), $@"Ambigous hashcode: rate=""{textElement.Parent.Attribute("value").Value}"" tariffCode={textElement.Parent.Parent.Attribute("value").Value}");
				}
			}
			Assert.That(hashCodes.Count, Is.GreaterThan(0), "Should have processed some texts");
		}

		[Test]
		public void ThrowExceptionIfHashNotUnique()
		{
			var list = new AdditionalCodeListWithBadHashForTesting();
			var description1 = "abcdefghijklmnopqrstuvwxyz";
			var description2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			Assert.Multiple(() =>
			{
				Assert.AreNotEqual(description1, description2);
				list.GetDescriptionHashCode(description1);
				Assert.Throws<InvalidOperationException>(() => list.GetDescriptionHashCode(description2));
			});
		}

		[Test]
		public void Add()
		{
			var list = new AdditionalCodeList();
			list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", sampleDate, sampleDate);
			list.AddOrGetDescriptionHashCode("de2", "en2", "fr2", "it2", sampleDate, sampleDate);
			list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", sampleDate, sampleDate);
			Assert.That(list.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Add_validFrom()
		{
			Assert.Multiple(() =>
			{
				var list = new AdditionalCodeList();
				list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", new DateTime(2012, 1, 1), sampleDate);
				Assert.That(list.GetCusCodeListByDescription("de1").ZZD_StartDate, Is.EqualTo(new DateTime(2012, 1, 1)));
				list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", new DateTime(2013, 1, 1), sampleDate);
				Assert.That(list.GetCusCodeListByDescription("de1").ZZD_StartDate, Is.EqualTo(new DateTime(2012, 1, 1)));
				list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", new DateTime(2011, 1, 1), sampleDate);
				Assert.That(list.GetCusCodeListByDescription("de1").ZZD_StartDate, Is.EqualTo(new DateTime(2011, 1, 1)));
			});
		}

		[Test]
		public void Add_validTo()
		{
			Assert.Multiple(() =>
			{
				var list = new AdditionalCodeList();
				list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", sampleDate, new DateTime(2012, 1, 1));
				Assert.That(list.GetCusCodeListByDescription("de1").ZZD_EndDate, Is.EqualTo(new DateTime(2012, 1, 1, 23, 59, 0)));
				list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", sampleDate, new DateTime(2011, 1, 1));
				Assert.That(list.GetCusCodeListByDescription("de1").ZZD_EndDate, Is.EqualTo(new DateTime(2012, 1, 1, 23, 59, 0)));
				list.AddOrGetDescriptionHashCode("de1", "en1", "fr1", "it1", sampleDate, new DateTime(2013, 1, 1));
				Assert.That(list.GetCusCodeListByDescription("de1").ZZD_EndDate, Is.EqualTo(new DateTime(2013, 1, 1, 23, 59, 0)));
			});
		}

		[Test]
		public void WriteXml()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\AdditionalCodeListTest_WriteXml.xml"))
			{
				var list = new AdditionalCodeList();
				list.AddOrGetDescriptionHashCode("description-1-de", "description-1-en", "description-1-fr", "description-1-it", sampleDate, sampleDate);
				list.AddOrGetDescriptionHashCode("description-2-de", "description-2-en", "description-2-fr", "description-2-it", sampleDate, sampleDate);
				list.AddOrGetDescriptionHashCode("description-3-de", null, null, null, sampleDate, sampleDate);
				list.WriteXml(outputFile.FullPath, sampleDate);
				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				using (var expectedStream = GetType().GetTestStream($"TestFiles.Output.additionalCodes.xml"))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));;
					var expectedXml = XDocument.Load(expectedStream);
					Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), "File content does not match");
				}
			}
		}

		[Test]
		public void WriteXml_Sorted()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\AdditionalCodeListTest_WriteXml_Sorted.xml"))
			{
				var list = new AdditionalCodeList();
				list.AddOrGetDescriptionHashCode("A", null, null, null, sampleDate, sampleDate);
				list.AddOrGetDescriptionHashCode("D", null, null, null, sampleDate, sampleDate);
				list.AddOrGetDescriptionHashCode("B", null, null, null, sampleDate, sampleDate);
				list.AddOrGetDescriptionHashCode("E", null, null, null, sampleDate, sampleDate);
				list.AddOrGetDescriptionHashCode("C", null, null, null, sampleDate, sampleDate);
				list.AddOrGetDescriptionHashCode("F", null, null, null, sampleDate, sampleDate);
				list.WriteXml(outputFile.FullPath, sampleDate);
				var xml = XDocument.Load(outputFile.FullPath);
				Assert.That(xml.XPathSelectElement(@"//RefCusCodeList[1]/ZZD_Description")?.Value, Is.EqualTo("A"));
				Assert.That(xml.XPathSelectElement(@"//RefCusCodeList[2]/ZZD_Description")?.Value, Is.EqualTo("B"));
				Assert.That(xml.XPathSelectElement(@"//RefCusCodeList[3]/ZZD_Description")?.Value, Is.EqualTo("C"));
				Assert.That(xml.XPathSelectElement(@"//RefCusCodeList[4]/ZZD_Description")?.Value, Is.EqualTo("D"));
				Assert.That(xml.XPathSelectElement(@"//RefCusCodeList[5]/ZZD_Description")?.Value, Is.EqualTo("E"));
				Assert.That(xml.XPathSelectElement(@"//RefCusCodeList[6]/ZZD_Description")?.Value, Is.EqualTo("F"));
			}
		}

		readonly DateTime sampleDate = new DateTime(2021, 02, 23, 2, 54, 17);

		class BadHashAlgorithmForTesting : HashAlgorithm
		{
			byte[] hash;
			int next;

			internal BadHashAlgorithmForTesting()
			{
				Initialize();
			}

			public override void Initialize()
			{
				hash = new byte[16];
				next = 0;
			}

			protected override void HashCore(byte[] array, int ibStart, int cbSize)
			{
				for (int i = ibStart; i < ibStart + cbSize; i++)
				{
					hash[next] ^= (byte)(array[i] & 0x0f);
					next = (next + 1) % hash.Length;
				}
			}

			protected override byte[] HashFinal()
			{
				return hash.ToArray();
			}
		}

		class AdditionalCodeListWithBadHashForTesting : AdditionalCodeList
		{
			internal override HashAlgorithm CreateHashAlgorithm() => new BadHashAlgorithmForTesting();
		}
	}
}

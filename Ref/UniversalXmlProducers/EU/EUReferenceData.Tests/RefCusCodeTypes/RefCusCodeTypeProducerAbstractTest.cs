using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	abstract class RefCusCodeTypeProducerAbstractTest<T> where T : RefCusCodeTypeProducer
	{
		[Test]
		public void TestGenerateFile()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\RefCusCodeTypes");
			Producer.GenerateFile(Path.Combine(Path.GetDirectoryName(assembly.Location), @"EU\TestFiles\RefCusCodeTypes"));

			var expectedXML = TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.RefCusCodeTypes.TestFiles.Output.{expectedOutputFileName}");
			var actualXml = File.ReadAllText(Path.Combine(outputPath, expectedOutputFileName));
			Assert.That(actualXml, Is.EqualTo(expectedXML));
		}

		protected abstract string CodeType { get; }

		protected abstract string Description { get; }

		protected abstract bool ReadOnly { get; }

		protected abstract byte MaxLength { get; }

		protected abstract RefCusCodeTypeProducer Producer { get; }

		protected abstract string expectedOutputFileName { get; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class ExportNomenclatureProducerFixture
	{
		[Test]
		public void TestGetNewCompositeKeyTreeGenerator()
		{
			var nomenclatureProducer = new ExportNomenclatureProducerForTest();

			Assert.That(nomenclatureProducer.GetNewCompositeKeyTreeGeneratorExposed(), Is.TypeOf<ExportCompositeKeyTreeGenerator>());
		}

		class ExportNomenclatureProducerForTest : ExportNomenclatureProducer
		{
			public ICompositeKeyTreeGenerator GetNewCompositeKeyTreeGeneratorExposed() => GetNewCompositeKeyTreeGenerator();
		}
	}
}

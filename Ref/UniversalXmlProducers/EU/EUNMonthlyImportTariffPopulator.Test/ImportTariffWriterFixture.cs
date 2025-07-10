using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator.Test
{
	[TestFixture]
	internal class ImportTariffWriterFixture
	{
		[Test]
		public void Write()
		{
			var tariff1 = new RefCusTariff { ZZ1_TariffCode = "1000000000" };
			var tariff2 = new RefCusTariff { ZZ1_TariffCode = "1100000000" };
			var tariff3 = new RefCusTariff { ZZ1_TariffCode = "4000000000" };
			var writer = new Mock<IXmlWriter>();
			var tariffWriter = new ImportTariffWriter(() => writer.Object);
			tariffWriter.Write(new[] { tariff1, tariff2, tariff3 }, new DateTime(2022, 05, 04), true, @"C:\Dev\out.xml");
			writer.Verify(x => x.SetUpdateType(UpdateType.Partial), Times.Exactly(2));
			writer.Verify(x => x.SetPublicationTime(new DateTime(2022, 05, 04)), Times.Exactly(2));
			writer.Verify(x => x.SetDataSource($"Import EUN Tariffs chapters 10-19"), Times.Once());
			writer.Verify(x => x.SetDataSource($"Import EUN Tariffs chapters 40-49"), Times.Once());
			writer.Verify(x => x.SaveXml(@"C:\Dev\out_1.xml", true), Times.Once());
			writer.Verify(x => x.SaveXml(@"C:\Dev\out_4.xml", true), Times.Once());
		}
	}
}

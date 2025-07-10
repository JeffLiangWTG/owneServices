using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using Moq;
using NUnit.Framework;
using System.Reflection;
using System.IO;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	class TariffPublicationTimeBuilderTest
	{
		[Test]
		public void TestBuild()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
			var processingData = new NZTariffProcessingData();

			var dataRepo = new TariffDataRepo() as ITopLevelDataRepo<RefCusTariff>;
			var timeBuilder = new TariffPublicationTimeBuilder<NZTariffProcessingData>(
				(NZTariffProcessingData processingData) => processingData.LastRunDateTariff,
				(NZTariffProcessingData processingData, DateTime dateTime) => processingData.LastRunDateTariff = dateTime);

			timeBuilder.Build(dataRepo, new[] { new BuildersFilePath(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Input\Tariff\time_stamp.txt")) }, processingData);
			var tariffPublishmentDataTime = new DateTime(2022, 12, 14, 4, 0, 0);
			Assert.That(dataRepo.PublicationTime, Is.EqualTo(tariffPublishmentDataTime));
			Assert.That(processingData.LastRunDateTariff, Is.EqualTo(tariffPublishmentDataTime));

			timeBuilder.Build(dataRepo, new[] { new BuildersFilePath(Path.Combine(assemblyDirectoryPath, @"Concession\TestFiles\Input\Concession\time_stamp.txt")) }, processingData);
			var concessionPublishmentDataTime = new DateTime(2024, 12, 26, 4, 0, 13);
			Assert.That(dataRepo.PublicationTime, Is.EqualTo(concessionPublishmentDataTime));
			Assert.That(processingData.LastRunDateTariff, Is.EqualTo(concessionPublishmentDataTime));
		}

		[Test]
		public void TestBuild_NotNewPublication()
		{
			var assemblyDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processingData = new NZTariffProcessingData()
			{
				LastRunDateTariff = new DateTime(2022, 12, 14, 4, 0, 0)
			};
			var dataRepo = new TariffDataRepo() as ITopLevelDataRepo<RefCusTariff>;

			var timeBuilder = new TariffPublicationTimeBuilder<NZTariffProcessingData>(
				(NZTariffProcessingData processingData) => processingData.LastRunDateTariff,
				(NZTariffProcessingData processingData, DateTime dateTime) => processingData.LastRunDateTariff = dateTime);

			var buildResult = timeBuilder.Build(dataRepo, new[] { new BuildersFilePath(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Input\Tariff\time_stamp.txt")) }, processingData);
			Assert.That(!buildResult);

			processingData.LastRunDateTariff = new DateTime(2022, 12, 14, 3, 59, 59);
			buildResult = timeBuilder.Build(dataRepo, new[] { new BuildersFilePath(Path.Combine(assemblyDirectoryPath, @"Tariff\TestFiles\Input\Tariff\time_stamp.txt")) }, processingData);
			Assert.That(buildResult);
		}
	}
}

using System;
using System.Linq;
using CargoWise.RefDbRepo.Staging.ApplicationConfig;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	class IntegrationDeveloperTests
	{
		[Test]
		[Explicit("Developer Test for local integration")]
		public void GetMessages()
		{
			//insert into SourceData values (newid(), 'ZAA','File1.txt','TXT',null,'ZA Prodat Message 1','QUE',getdate(),'PRO',getdate(),'MD WAS HERE',null,null)
			var logger = new TestLogger();
			using (var stagingRepo = StagingRepositoryFactory.GetStagingRepository())
			{
				using (IMessageHandler handler = new MessageHandler(stagingRepo, ConfigurationProvider.InputFolder, ConfigurationProvider.InputFile, logger, SupportedMessageTypes.Prodat))
				{
					var msgs = handler.GetMessages().ToList();

					Console.WriteLine("Errors:");
					Console.WriteLine(logger.ErrorString);

					Console.WriteLine("Msgs in db:");
					msgs.ForEach(x => Console.WriteLine(x.Content));
				}
			}
		}

		[Test]
		[Explicit("Developer Test for local integration")]
		public void UpdateStatus()
		{
			var logger = new TestLogger();
			using (var stagingRepo = StagingRepositoryFactory.GetStagingRepository())
			{
				using (IMessageHandler handler = new MessageHandler(stagingRepo, ConfigurationProvider.InputFolder, string.Empty, logger, SupportedMessageTypes.Prodat))
				{
					var msgsBefore = handler.GetMessages().ToList();

					if (msgsBefore.Any())
					{
						var msg = msgsBefore.First();
						Console.WriteLine($"Updating status of msg '{msg.ID}'");

						handler.UpdateStatus(msg, true);

						var msgsAfter = handler.GetMessages().ToList();

						var gone = !msgsAfter.Any(x => x.ID == msg.ID);
						Assert.That(gone, Is.True);

						var srcData = stagingRepo.Get<SourceData>().Where(x => x.SDA_PK == msg.ID).FirstOrDefault();
						Assert.That(srcData, Is.Not.Null);
						Assert.That(srcData.SDA_Status, Is.EqualTo("MER"));
					}
					else
					{
						Console.WriteLine("No messages to test with, please update the status of one or create a new one");
					}
				}
			}
		}

		[Test]
		[Explicit("Developer Test for local integration")]
		public void FetchMessagesFromProd()
		{
			var backDate = new DateTime(2018, 1, 1);
			using (var stagingRepoProd = new StagingRepository("metadata=res://*/CargoWise.RefDbRepo.Staging.Schema.StagingEntities.csdl|res://*/CargoWise.RefDbRepo.Staging.Schema.StagingEntities.ssdl|res://*/CargoWise.RefDbRepo.Staging.Schema.StagingEntities.msl;provider=System.Data.SqlClient;provider connection string=\";data source=refdbrepo.db.wisegrid.net;User id=<username>;Password=<password>;initial catalog=RefDbRepoStage;MultipleActiveResultSets=True;TrustServerCertificate=True;App=ZAReferenceData.CmdLine\";"))
			using (var stagingRepo = StagingRepositoryFactory.GetStagingRepository())
			{
				var msgs = stagingRepoProd.Get<SourceData>()
							.Where(x => x.SDA_Source == DataSourceConstants.Source.eHubZACustomsRepositoryQueue &&
										x.SDA_ContentType == DataSourceConstants.ContentType.ZA_ProDat &&
										x.SDA_CreatedTime >= backDate)
							.ToList();

				foreach (var m in msgs)
				{
					var sd = new SourceData
					{
						SDA_Contacts = m.SDA_Contacts,
						SDA_Content = m.SDA_Content,
						SDA_ContentText = m.SDA_ContentText,
						SDA_ContentType = m.SDA_ContentType,
						SDA_CreatedTime = backDate,
						SDA_Filename = m.SDA_Filename,
						SDA_Filetype = m.SDA_Filetype,
						SDA_NotProcessedUntil = m.SDA_NotProcessedUntil,
						SDA_PK = m.SDA_PK,
						SDA_Source = m.SDA_Source,
						SDA_SourceTime = m.SDA_SourceTime,
						SDA_Status = m.SDA_Status,
						SDA_SubSource = m.SDA_SubSource
					};

					stagingRepo.Add(sd);
				}

				stagingRepo.SaveChanges();
			}
		}

		[Test]
		[Explicit("Developer Test for local integration")]
		public void GetTariffUniqueId()
		{
			var logger = new TestLogger();
			var dataLoader = new RefDataLoader(ConfigurationProvider.RefDbServiceURI, ConfigurationProvider.IsRefDbServiceSecure);

			var tariffHelper = new TariffHelper(dataLoader, logger) as ITariffHelper;

			var result = tariffHelper.GetUniqueId("312010204", "42", "5906");
			Assert.That(result.UniqueId, Is.EqualTo(1));
			Assert.That(result.Exists, Is.EqualTo(true));
		}

		[Test]
		[Explicit("Developer Test for local integration")]
		public void GetAllRules()
		{
			var logger = new TestLogger();
			var dataLoader = new RefDataLoader(ConfigurationProvider.RefDbServiceURI, ConfigurationProvider.IsRefDbServiceSecure);

			var tariffHelper = new TariffHelper(dataLoader, logger) as ITariffHelper;

			var rules = tariffHelper.GetRules();
			Assert.That(rules, Is.Not.Null);
			Assert.That(rules.Count, Is.EqualTo(1309));
		}
	}
}

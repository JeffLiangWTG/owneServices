using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader.Test
{
	[TestFixture]
	class ZACMessageCreatorFixture
	{
		[TestCase(@"UNB+UNOB:4+SARSINF+WISETECHGLOBAL::WTGWTGWTGWTGWTGW:WTGAS2+20160825:1502+43++PRODAT'
UNH+1+PRODAT:D:96B:UN:ZZZ01'
BGM+6+0+9'
DTM+302:20160825:102'
ETC", DataSourceConstants.ContentType.ZA_ProDat, Constants.ZATariffSubSource, 2016, 08, 25)]
		[TestCase(@"UNB+UNOB:4+SARSINF+WISETECHGLOBAL::WTGWTGWTGWTGWTGW:WTGAS2+20160822:1056+39++GESMES''
UNH+1+GESMES:D:96B:UN:ZZZ01'
BGM+190++9'
DTM+7:20160822:102'
ETC", DataSourceConstants.ContentType.ZA_Gesmes, Constants.ZAExchangeRateSubSource, 2016, 08, 22)]
		public void Create(string messageText, string contentType, string subSource, int sYear, int sMonth, int sDay)
		{
			var currentTime = DateTime.UtcNow;
			sourceData.SDA_ContentText = messageText;
			var message = new ZACMessageCreator().GetSourceDataFromStreamText(messageText, sourceData);
			Assert.That(message.SDA_Filetype, Is.EqualTo(DataSourceConstants.FileType.TXT.ToString()));
			Assert.That(message.SDA_Source, Is.EqualTo(DataSourceConstants.Source.eHubZACustomsRepositoryQueue));
			Assert.That(message.SDA_ContentText, Is.EqualTo(messageText));
			Assert.That(message.SDA_ContentType, Is.EqualTo(contentType));
			Assert.That(message.SDA_SourceTime, Is.EqualTo(new DateTime(sYear, sMonth, sDay)));
			Assert.That(message.IsSourceDataQueuedStatus());
			Assert.AreEqual(message.SDA_SubSource, subSource);

			Assert.That(message.SDA_CreatedTime, Is.GreaterThanOrEqualTo(currentTime));
		}

		[Test]
		public void CreateWithNotMatchContentType()
		{
			sourceData.SDA_ContentText = MessageNotMatchContentType;
			var message = new ZACMessageCreator().GetSourceDataFromStreamText(MessageNotMatchContentType, sourceData);
			Assert.That(message.SDA_Filetype, Is.EqualTo(DataSourceConstants.FileType.TXT.ToString()));
			Assert.That(message.SDA_Source, Is.EqualTo(DataSourceConstants.Source.eHubZACustomsRepositoryQueue));
			Assert.That(message.SDA_ContentText, Is.EqualTo(MessageNotMatchContentType));
			Assert.IsNull(message.SDA_ContentType);
			Assert.IsNull(message.SDA_SourceTime);
			Assert.That(message.IsSourceDataErrorStatus());
			Assert.AreEqual(message.SDA_SubSource, Constants.SubSourceUnknown);
		}

		[Test]
		public void CreateMultipleMessagesMoreThanASecondApart()
		{
			var currentTime = DateTime.UtcNow;
			var currentTimePlus1 = currentTime.AddSeconds(1);

			var messageText = @"UNB+UNOB:4+SARSINF+WISETECHGLOBAL::WTGWTGWTGWTGWTGW:WTGAS2+20160825:1502+43++PRODAT'
UNH+1+PRODAT:D:96B:UN:ZZZ01'
BGM+6+0+9'
DTM+302:20160825:102'
ETC";
			sourceData.SDA_ContentText = messageText;
			var message1 = new ZACMessageCreator().GetSourceDataFromStreamText(messageText, sourceData);
			var message2 = new ZACMessageCreator().GetSourceDataFromStreamText(messageText, sourceData);

			Assert.That(message1.SDA_CreatedTime, Is.GreaterThanOrEqualTo(currentTime), "First message processed in normal time");
			Assert.That(message2.SDA_CreatedTime, Is.GreaterThanOrEqualTo(currentTimePlus1), "Simultaneously processed messages should be delayed by at least 1 second");
		}

		[SetUp]
		public void SetUp()
		{
			sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Filetype = DataSourceConstants.FileType.TXT.ToString(),
				SDA_SubSource = Constants.SubSourceUnknown,
				SDA_Status = StatusProvider.GetERRStatus()
			};
		}
		SourceData sourceData;

		const string MessageNotMatchContentType = @"UNB+UNOB:4+SARSINF+WISETECHGLOBAL::WTGWTGWTGWTGWTGW:WTGAS2+20160822:1056+39++XXXX''
UNH+1+XXXX:D:96B:UN:ZZZ01'
BGM+190++9'
DTM+7:20160822:102'
ETC";
	}
}

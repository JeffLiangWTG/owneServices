using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ExchangeRateProcessorTest : ABIProcessorTest<ExchangeRateProcessor, APLA, APLB, APLY>
	{
		[TestDate(2008, 01, 01)]
		protected override void EndToEndCore()
		{
			var image1 = new System.Drawing.Bitmap(1, 2);
			Registry.Business.SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);

			var filer = new EntryFiler();
			filer.EntryFilerCode = "SSS";

			company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			var f108 = new ERFF108();
			f108.ExchangeRate = 0.9563m;
			f108.ISOCurrencyCode = "AUD";
			f108.Indicator1 = "Q";
			f108.ExchangeRateDate = ZDate.Today;

			generator.AddMessageBlock(f108);

			ProcessMessage(generator);

			Factory.Save();

			var rates = Factory.Load<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.RE_GC, company.PK));
			AssertEquals(1, rates.Length);
			AssertEquals(0.9563m, rates[0].RE_SellRate);
			AssertEquals(ZDate.Today, rates[0].RE_StartDate);
			AssertEquals(ZDate.Today.AddHours(23).AddMinutes(59), rates[0].RE_ExpiryDate);

			var email = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<ZArchitecture.Environment.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(1, image.Width);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		public void TestMessageProcessingWithMultipleDatesAndMultipleCurrencies()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea))
			{
				var filer = new EntryFiler();
				filer.EntryFilerCode = "SSS";

				company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
				Factory.Save();

				var exchangeRateProcessor = new ExchangeRateProcessor();
				var ediMessage = Factory.New<MQEDIMessage>();
				ediMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
				SetMessageText(ediMessage);

				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(ediMessage);

				var query = new ZQuery(RefExchangeRateSchema.RE_GC, company.PK);
				var aUQuery = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, Core.Constants.CurrencyCodes.Australia);
				aUQuery.AddToFilter(RefExchangeRateSchema.RE_StartDate, new ZDate(2007, 01, 24));
				query.AddToFilter(aUQuery);
				var exchangeAUDRates = Factory.Load<RefExchangeRate>(query);
				AssertEquals(1, exchangeAUDRates.Length);
				AssertEquals(0.797m, exchangeAUDRates[0].RE_SellRate);

				query = new ZQuery(RefExchangeRateSchema.RE_GC, company.PK);
				var jPQuery = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, Core.Constants.CurrencyCodes.Japan);
				jPQuery.AddToFilter(RefExchangeRateSchema.RE_StartDate, new ZDate(2007, 01, 24));
				query.AddToFilter(jPQuery);
				var exchangeJPYRates = Factory.Load<RefExchangeRate>(query);
				AssertEquals(1, exchangeJPYRates.Length);
				AssertEquals(0.008416m, exchangeJPYRates[0].RE_SellRate);

				query = new ZQuery(RefExchangeRateSchema.RE_ExRateType, "CUS");
				query.AddToFilter(RefExchangeRateSchema.RE_StartDate, new ZDate(2007, 01, 24));
				var exchangeRates = Factory.Load<RefExchangeRate>(query);
				AssertEquals(35, exchangeRates.Length);

				query = new ZQuery(RefExchangeRateSchema.RE_GC, company.PK);
				aUQuery = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, Core.Constants.CurrencyCodes.Australia);
				query.AddToFilter(RefExchangeRateSchema.RE_StartDate, new ZDate(2007, 01, 25));
				query.AddToFilter(aUQuery);
				exchangeAUDRates = Factory.Load<RefExchangeRate>(query);
				AssertEquals(1, exchangeAUDRates.Length);
				AssertEquals(0.796m, exchangeAUDRates[0].RE_SellRate);

				query = new ZQuery(RefExchangeRateSchema.RE_GC, company.PK);
				jPQuery = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, Core.Constants.CurrencyCodes.Japan);
				query.AddToFilter(RefExchangeRateSchema.RE_StartDate, new ZDate(2007, 01, 25));
				query.AddToFilter(jPQuery);
				exchangeJPYRates = Factory.Load<RefExchangeRate>(query);
				AssertEquals(1, exchangeJPYRates.Length);
				AssertEquals(0.008415m, exchangeJPYRates[0].RE_SellRate);

				query = new ZQuery(RefExchangeRateSchema.RE_ExRateType, "CUS");
				query.AddToFilter(RefExchangeRateSchema.RE_StartDate, new ZDate(2007, 01, 25));
				exchangeRates = Factory.Load<RefExchangeRate>(query);
				AssertEquals(35, exchangeRates.Length);
			}
		}

		public void TestMessageProcessingWithMultipleCurrenciesForAllUSComp()
		{
			CreateUSCompanies();

			var exchangeRateProcessor = new ExchangeRateProcessor();
			var ediMessage = Factory.New<MQEDIMessage>();
			ediMessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			SetMessageText(ediMessage);

			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(ediMessage);
			Factory.Save();
			var query = new ZQuery(RefExchangeRateSchema.RE_GC, company.PK);
			var exchangeRates = Factory.Load<RefExchangeRate>(query);
			AssertEquals("exchangeRates added for US Company with Filer code", 245, exchangeRates.Length);

			query = new ZQuery(RefExchangeRateSchema.RE_GC, company2.PK);
			exchangeRates = Factory.Load<RefExchangeRate>(query);
			AssertEquals("No Filer Code for this company", 0, exchangeRates.Length);

			query = new ZQuery(RefExchangeRateSchema.RE_GC, company3.PK);
			exchangeRates = Factory.Load<RefExchangeRate>(query);
			AssertEquals("No Filer Code For this Company", 0, exchangeRates.Length);

			query = new ZQuery(RefExchangeRateSchema.RE_GC, company4.PK);
			exchangeRates = Factory.Load<RefExchangeRate>(query);
			AssertEquals("exchangeRates added for US Company with Filer code", 245, exchangeRates.Length);

			query = new ZQuery(RefExchangeRateSchema.RE_GC, company5.PK);
			exchangeRates = Factory.Load<RefExchangeRate>(query);
			AssertEquals("Not US Company", 0, exchangeRates.Length);

			query = new ZQuery(RefExchangeRateSchema.RE_GC, prCompany.PK);
			exchangeRates = Factory.Load<RefExchangeRate>(query);
			AssertEquals("exchangeRates added for US Company with Filer code", 245, exchangeRates.Length);
		}

		void SetMessageText(MQEDIMessage ediMessage)
		{
			#region Message Text

			ediMessage.EM_MessageText =
				"B018888XJ5FR                                               16931                " +
				"F108 00 ATATS0124070094213D                                                     " +
				"F108 00 AUAUD0124070797000Q                                                     " +
				"F108 00 BEBEF0124070032137D                                                     " +
				"F108 00 BRBRR0124070468604Q                                                     " +
				"F108 00 CACAD0124070858590Q                                                     " +
				"F108 00 CHCHF0124070824742Q                                                     " +
				"F108 00 CNCNY0124070128121Q                                                     " +
				"F108 00 DEDEM0124070662839D                                                     " +
				"F108 00 DKDKK0124070178193Q                                                     " +
				"F108 00 ESESP0124070007792D                                                     " +
				"F108 00 EUEUR0124071296400D                                                     " +
				"F108 00 FIFIM0124070218039D                                                     " +
				"F108 00 FRFRF0124070197635D                                                     " +
				"F108 00 GBGBP0124071973600Q                                                     " +
				"F108 00 GRGRD0124070003805D                                                     " +
				"F108 00 HKHKD0124070128540Q                                                     " +
				"F108 00 IEIEP0124071646088D                                                     " +
				"F108 00 ILILS0124070000000N                                                     " +
				"F108 00 ININR0124070022660Q                                                     " +
				"F108 00 IRIRR0124070000000N                                                     " +
				"F108 00 ITITL0124070000670D                                                     " +
				"F108 00 JPJPY0124070008416Q                                                     " +
				"F108 00 KRKRW0124070001069D                                                     " +
				"F108 00 LKLKR0124070009292Q                                                     " +
				"F108 00 LULUF0124070032137D                                                     " +
				"F108 00 MXMXP0124070092773Q                                                     " +
				"F108 00 MYMYR0124070283286Q                                                     " +
				"F108 00 NLNLG0124070588281D                                                     " +
				"F108 00 NONOK0124070161789Q                                                     " +
				"F108 00 NZNZD0124070708400Q                                                     " +
				"F108 00 PHPHP0124070000000N                                                     " +
				"F108 00 PTPTE0124070006466D                                                     " +
				"F108 00 SESEK0124070147453Q                                                     " +
				"F108 00 SGSGD0124070652912Q                                                     " +
				"F108 00 THTHB0124070028169Q                                                     " +
				"F108 00 TWTWD0124070030469D                                                     " +
				"F108 00 VEVEB0124070000466Q                                                     " +
				"F108 00 ZAZAR0124070145307Q                                                     " +
				"F108 00 ATATS0125070094329D                                                     " +
				"F108 00 AUAUD0125070796000Q                                                     " +
				"F108 00 BEBEF0125070032177D                                                     " +
				"F108 00 BRBRR0125070468604Q                                                     " +
				"F108 00 CACAD0125070858590Q                                                     " +
				"F108 00 CHCHF0125070824742Q                                                     " +
				"F108 00 CNCNY0125070128121Q                                                     " +
				"F108 00 DEDEM0125070663657D                                                     " +
				"F108 00 DKDKK0125070178193Q                                                     " +
				"F108 00 ESESP0125070007801D                                                     " +
				"F108 00 EUEUR0125071298000D                                                     " +
				"F108 00 FIFIM0125070218308D                                                     " +
				"F108 00 FRFRF0125070197879D                                                     " +
				"F108 00 GBGBP0125071973600Q                                                     " +
				"F108 00 GRGRD0125070003809D                                                     " +
				"F108 00 HKHKD0125070128540Q                                                     " +
				"F108 00 IEIEP0125071648120D                                                     " +
				"F108 00 ILILS0125070000000N                                                     " +
				"F108 00 ININR0125070022660Q                                                     " +
				"F108 00 IRIRR0125070000000N                                                     " +
				"F108 00 ITITL0125070000670D                                                     " +
				"F108 00 JPJPY0125070008415Q                                                     " +
				"F108 00 KRKRW0125070001068D                                                     " +
				"F108 00 LKLKR0125070009292Q                                                     " +
				"F108 00 LULUF0125070032177D                                                     " +
				"F108 00 MXMXP0125070092773Q                                                     " +
				"F108 00 MYMYR0125070283286Q                                                     " +
				"F108 00 NLNLG0125070589007D                                                     " +
				"F108 00 NONOK0125070161789Q                                                     " +
				"F108 00 NZNZD0125070708400Q                                                     " +
				"F108 00 PHPHP0125070000000N                                                     " +
				"F108 00 PTPTE0125070006474D                                                     " +
				"F108 00 SESEK0125070147453Q                                                     " +
				"F108 00 SGSGD0125070652912Q                                                     " +
				"F108 00 THTHB0125070028169Q                                                     " +
				"F108 00 TWTWD0125070030441D                                                     " +
				"F108 00 VEVEB0125070000466Q                                                     " +
				"F108 00 ZAZAR0125070138169D                                                     " +
				"F108 00 ATATS0126070093813D                                                     " +
				"F108 00 AUAUD0126070796000Q                                                     " +
				"F108 00 BEBEF0126070032001D                                                     " +
				"F108 00 BRBRR0126070468604Q                                                     " +
				"F108 00 CACAD0126070858590Q                                                     " +
				"F108 00 CHCHF0126070824742Q                                                     " +
				"F108 00 CNCNY0126070128121Q                                                     " +
				"F108 00 DEDEM0126070660027D                                                     " +
				"F108 00 DKDKK0126070178193Q                                                     " +
				"F108 00 ESESP0126070007758D                                                     " +
				"F108 00 EUEUR0126071290900D                                                     " +
				"F108 00 FIFIM0126070217114D                                                     " +
				"F108 00 FRFRF0126070196796D                                                     " +
				"F108 00 GBGBP0126071973600Q                                                     " +
				"F108 00 GRGRD0126070003788D                                                     " +
				"F108 00 HKHKD0126070128540Q                                                     " +
				"F108 00 IEIEP0126071639105D                                                     " +
				"F108 00 ILILS0126070000000N                                                     " +
				"F108 00 ININR0126070022660Q                                                     " +
				"F108 00 IRIRR0126070000000N                                                     " +
				"F108 00 ITITL0126070000667D                                                     " +
				"F108 00 JPJPY0126070008415Q                                                     " +
				"F108 00 KRKRW0126070001063D                                                     " +
				"F108 00 LKLKR0126070009292Q                                                     " +
				"F108 00 LULUF0126070032001D                                                     " +
				"F108 00 MXMXP0126070092773Q                                                     " +
				"F108 00 MYMYR0126070283286Q                                                     " +
				"F108 00 NLNLG0126070585785D                                                     " +
				"F108 00 NONOK0126070161789Q                                                     " +
				"F108 00 NZNZD0126070708400Q                                                     " +
				"F108 00 PHPHP0126070000000N                                                     " +
				"F108 00 PTPTE0126070006439D                                                     " +
				"F108 00 SESEK0126070147453Q                                                     " +
				"F108 00 SGSGD0126070652912Q                                                     " +
				"F108 00 THTHB0126070029940D                                                     " +
				"F108 00 TWTWD0126070030367D                                                     " +
				"F108 00 VEVEB0126070000466Q                                                     " +
				"F108 00 ZAZAR0126070137363D                                                     " +
				"F108 00 ATATS0127070093813D                                                     " +
				"F108 00 AUAUD0127070796000Q                                                     " +
				"F108 00 BEBEF0127070032001D                                                     " +
				"F108 00 BRBRR0127070468604Q                                                     " +
				"F108 00 CACAD0127070858590Q                                                     " +
				"F108 00 CHCHF0127070824742Q                                                     " +
				"F108 00 CNCNY0127070128121Q                                                     " +
				"F108 00 DEDEM0127070660027D                                                     " +
				"F108 00 DKDKK0127070178193Q                                                     " +
				"F108 00 ESESP0127070007758D                                                     " +
				"F108 00 EUEUR0127071290900D                                                     " +
				"F108 00 FIFIM0127070217114D                                                     " +
				"F108 00 FRFRF0127070196796D                                                     " +
				"F108 00 GBGBP0127071973600Q                                                     " +
				"F108 00 GRGRD0127070003788D                                                     " +
				"F108 00 HKHKD0127070128540Q                                                     " +
				"F108 00 IEIEP0127071639105D                                                     " +
				"F108 00 ILILS0127070000000N                                                     " +
				"F108 00 ININR0127070022660Q                                                     " +
				"F108 00 IRIRR0127070000000N                                                     " +
				"F108 00 ITITL0127070000667D                                                     " +
				"F108 00 JPJPY0127070008415Q                                                     " +
				"F108 00 KRKRW0127070001063D                                                     " +
				"F108 00 LKLKR0127070009292Q                                                     " +
				"F108 00 LULUF0127070032001D                                                     " +
				"F108 00 MXMXP0127070092773Q                                                     " +
				"F108 00 MYMYR0127070283286Q                                                     " +
				"F108 00 NLNLG0127070585785D                                                     " +
				"F108 00 NONOK0127070161789Q                                                     " +
				"F108 00 NZNZD0127070708400Q                                                     " +
				"F108 00 PHPHP0127070000000N                                                     " +
				"F108 00 PTPTE0127070006439D                                                     " +
				"F108 00 SESEK0127070147453Q                                                     " +
				"F108 00 SGSGD0127070652912Q                                                     " +
				"F108 00 THTHB0127070029940D                                                     " +
				"F108 00 TWTWD0127070030367D                                                     " +
				"F108 00 VEVEB0127070000466Q                                                     " +
				"F108 00 ZAZAR0127070137363D                                                     " +
				"F108 00 ATATS0128070093813D                                                     " +
				"F108 00 AUAUD0128070796000Q                                                     " +
				"F108 00 BEBEF0128070032001D                                                     " +
				"F108 00 BRBRR0128070468604Q                                                     " +
				"F108 00 CACAD0128070858590Q                                                     " +
				"F108 00 CHCHF0128070824742Q                                                     " +
				"F108 00 CNCNY0128070128121Q                                                     " +
				"F108 00 DEDEM0128070660027D                                                     " +
				"F108 00 DKDKK0128070178193Q                                                     " +
				"F108 00 ESESP0128070007758D                                                     " +
				"F108 00 EUEUR0128071290900D                                                     " +
				"F108 00 FIFIM0128070217114D                                                     " +
				"F108 00 FRFRF0128070196796D                                                     " +
				"F108 00 GBGBP0128071973600Q                                                     " +
				"F108 00 GRGRD0128070003788D                                                     " +
				"F108 00 HKHKD0128070128540Q                                                     " +
				"F108 00 IEIEP0128071639105D                                                     " +
				"F108 00 ILILS0128070000000N                                                     " +
				"F108 00 ININR0128070022660Q                                                     " +
				"F108 00 IRIRR0128070000000N                                                     " +
				"F108 00 ITITL0128070000667D                                                     " +
				"F108 00 JPJPY0128070008415Q                                                     " +
				"F108 00 KRKRW0128070001063D                                                     " +
				"F108 00 LKLKR0128070009292Q                                                     " +
				"F108 00 LULUF0128070032001D                                                     " +
				"F108 00 MXMXP0128070092773Q                                                     " +
				"F108 00 MYMYR0128070283286Q                                                     " +
				"F108 00 NLNLG0128070585785D                                                     " +
				"F108 00 NONOK0128070161789Q                                                     " +
				"F108 00 NZNZD0128070708400Q                                                     " +
				"F108 00 PHPHP0128070000000N                                                     " +
				"F108 00 PTPTE0128070006439D                                                     " +
				"F108 00 SESEK0128070147453Q                                                     " +
				"F108 00 SGSGD0128070652912Q                                                     " +
				"F108 00 THTHB0128070029940D                                                     " +
				"F108 00 TWTWD0128070030367D                                                     " +
				"F108 00 VEVEB0128070000466Q                                                     " +
				"F108 00 ZAZAR0128070137363D                                                     " +
				"F108 00 ATATS0129070094097D                                                     " +
				"F108 00 AUAUD0129070796000Q                                                     " +
				"F108 00 BEBEF0129070032097D                                                     " +
				"F108 00 BRBRR0129070468604Q                                                     " +
				"F108 00 CACAD0129070858590Q                                                     " +
				"F108 00 CHCHF0129070824742Q                                                     " +
				"F108 00 CNCNY0129070128121Q                                                     " +
				"F108 00 DEDEM0129070662021D                                                     " +
				"F108 00 DKDKK0129070178193Q                                                     " +
				"F108 00 ESESP0129070007782D                                                     " +
				"F108 00 EUEUR0129071294800D                                                     " +
				"F108 00 FIFIM0129070217770D                                                     " +
				"F108 00 FRFRF0129070197391D                                                     " +
				"F108 00 GBGBP0129071973600Q                                                     " +
				"F108 00 GRGRD0129070003800D                                                     " +
				"F108 00 HKHKD0129070128540Q                                                     " +
				"F108 00 IEIEP0129071644057D                                                     " +
				"F108 00 ILILS0129070000000N                                                     " +
				"F108 00 ININR0129070022660Q                                                     " +
				"F108 00 IRIRR0129070000000N                                                     " +
				"F108 00 ITITL0129070000669D                                                     " +
				"F108 00 JPJPY0129070008415Q                                                     " +
				"F108 00 KRKRW0129070001064D                                                     " +
				"F108 00 LKLKR0129070009292Q                                                     " +
				"F108 00 LULUF0129070032097D                                                     " +
				"F108 00 MXMXP0129070092773Q                                                     " +
				"F108 00 MYMYR0129070283286Q                                                     " +
				"F108 00 NLNLG0129070587555D                                                     " +
				"F108 00 NONOK0129070161789Q                                                     " +
				"F108 00 NZNZD0129070708400Q                                                     " +
				"F108 00 PHPHP0129070000000N                                                     " +
				"F108 00 PTPTE0129070006458D                                                     " +
				"F108 00 SESEK0129070147453Q                                                     " +
				"F108 00 SGSGD0129070652912Q                                                     " +
				"F108 00 THTHB0129070028169Q                                                     " +
				"F108 00 TWTWD0129070030312D                                                     " +
				"F108 00 VEVEB0129070000466Q                                                     " +
				"F108 00 ZAZAR0129070136407D                                                     " +
				"F108 00 ATATS0130070094140D                                                     " +
				"F108 00 AUAUD0130070796000Q                                                     " +
				"F108 00 BEBEF0130070032112D                                                     " +
				"F108 00 BRBRR0130070468604Q                                                     " +
				"F108 00 CACAD0130070858590Q                                                     " +
				"F108 00 CHCHF0130070824742Q                                                     " +
				"F108 00 CNCNY0130070128121Q                                                     " +
				"F108 00 DEDEM0130070662328D                                                     " +
				"F108 00 DKDKK0130070178193Q                                                     " +
				"F108 00 ESESP0130070007786D                                                     " +
				"F108 00 EUEUR0130071295400D                                                     " +
				"F108 00 FIFIM0130070217871D                                                     " +
				"F108 00 FRFRF0130070197482D                                                     " +
				"F108 00 GBGBP0130071973600Q                                                     " +
				"F108 00 GRGRD0130070003802D                                                     " +
				"F108 00 HKHKD0130070128540Q                                                     " +
				"F108 00 IEIEP0130071644819D                                                     " +
				"F108 00 ILILS0130070000000N                                                     " +
				"F108 00 ININR0130070022660Q                                                     " +
				"F108 00 IRIRR0130070000000N                                                     " +
				"F108 00 ITITL0130070000669D                                                     " +
				"F108 00 JPJPY0130070008415Q                                                     " +
				"F108 00 KRKRW0130070001061D                                                     " +
				"F108 00 LKLKR0130070009292Q                                                     " +
				"F108 00 LULUF0130070032112D                                                     " +
				"F108 00 MXMXP0130070092773Q                                                     " +
				"F108 00 MYMYR0130070283286Q                                                     " +
				"F108 00 NLNLG0130070587827D                                                     " +
				"F108 00 NONOK0130070161789Q                                                     " +
				"F108 00 NZNZD0130070708400Q                                                     " +
				"F108 00 PHPHP0130070000000N                                                     " +
				"F108 00 PTPTE0130070006461D                                                     " +
				"F108 00 SESEK0130070147453Q                                                     " +
				"F108 00 SGSGD0130070652912Q                                                     " +
				"F108 00 THTHB0130070028169Q                                                     " +
				"F108 00 TWTWD0130070030331D                                                     " +
				"F108 00 VEVEB0130070000466Q                                                     " +
				"F108 00 ZAZAR0130070136715D                                                     " +
				"Y  8888XJ5FR00266";

			#endregion
		}

		void CreateUSCompanies()
		{
			var filer = new EntryFiler();
			filer.EntryFilerCode = "SSS";

			company = Factory.New<GlbCompany>();
			company.GC_Code = "!q1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "!q2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			company3 = Factory.New<GlbCompany>();
			company3.GC_Code = "!q3";
			company3.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			company4 = Factory.New<GlbCompany>();
			company4.GC_Code = "!q4";
			company4.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(company4.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			company5 = Factory.New<GlbCompany>();
			company5.GC_Code = "!q5";
			company5.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Albania;

			prCompany = Factory.New<GlbCompany>();
			prCompany.GC_Code = "!q6";
			prCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(prCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
		}

		GlbCompany company;
		GlbCompany company2;
		GlbCompany company3;
		GlbCompany company4;
		GlbCompany company5;
		GlbCompany prCompany;
	}
}

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate
{
	public class CAExchangeRateProducer
	{
		public CAExchangeRateProducer(IWebServiceCaller serviceCaller, StringBuilder errorBuilder, ICAExchangeRateParser parser)
		{
			_serviceCaller = serviceCaller;
			_errorBuilder = errorBuilder;
			_parser = parser;
		}

		readonly IWebServiceCaller _serviceCaller;
		readonly StringBuilder _errorBuilder;
		readonly ICAExchangeRateParser _parser;

		string FunctionCode => Constants.ProgramFunctions.CAExchangeRate;

		public void QueryDataAndParseToXMLFile(string exportFilePath)
		{
			var now = GetNowInCanada();
			Console.WriteLine("Start processing CA exchange rate");
			var preProcessChecker = new PreProcessChecker(FunctionCode);
			try
			{
				Console.WriteLine("Get exchange rate from webservice...");
				_serviceCaller.QueryAndParseResponse(now.Date);
				Console.WriteLine("All required exchange rates Got, start parsing...");
				var latestUpdateOnDate = Convert.ToDateTime(_serviceCaller.ExchangeRates.Max(x => x.ExchangeRateEffectiveTimestamp), CultureInfo.InvariantCulture).Date;
				if (_serviceCaller.ExchangeRates.Count > 0)
				{
					if (preProcessChecker.UpdateLastPublishDate(latestUpdateOnDate))
					{
						var hasData = _parser.ParseExchangeRateIntoXML(_serviceCaller.ExchangeRates, exportFilePath, latestUpdateOnDate);
						if (!hasData)
						{
							EmailSender.SendEmail(FunctionCode, $"No new exchange rate data found. Last update on {latestUpdateOnDate:s}.");
						}
					}
					else
					{
						Console.WriteLine($"Nothing new published since last process. Skip processing this time.");
					}
				}
			}
			catch (Exception ex)
			{
				var message = $"Error processing:{System.Environment.NewLine}{ex.Message}";
				_errorBuilder.AppendLine(message);
				preProcessChecker.MarkAsProcessRequired();
				EmailSender.SendEmail(FunctionCode, message);
			}

			Console.WriteLine("End processing CA exchange rate");

		}

		public Func<DateTime> GetNowInCanada
		{
			get { return getNowInCanada; }
			set { getNowInCanada = value; }
		}

		Func<DateTime> getNowInCanada = () => DateTime.UtcNow.AddHours(-5); // Eastern Time in CA
	}
}

using System;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExchangeRateDataTXT : IExchangeRateData
	{
		public ExchangeRateDataTXT(string lineData)
		{
			datas = lineData.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
			ProcessData();
		}

		string[] datas;

		void ProcessData()
		{
			var year = Utility.GetValueAsInt(datas[1]) + 1911;
			var month = Utility.GetValueAsInt(datas[2]);
			var tenDay = Utility.GetValueAsInt(datas[3]);
			switch (tenDay)
			{
				case 1:
					StartDate = new DateTime(year, month, 1);
					EndDate = new DateTime(year, month, 10, 23, 59, 00);
					break;
				case 2:
					StartDate = new DateTime(year, month, 11);
					EndDate = new DateTime(year, month, 20, 23, 59, 00);
					break;
				case 3:
					StartDate = new DateTime(year, month, 21);
					EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 00);
					break;
				default:
					StartDate = DateTime.MinValue;
					EndDate = DateTime.MaxValue;
					break;
			}

			InRate = Utility.GetValueAsDecimal(datas[4].TrimEnd('0'));
			ExRate = Utility.GetValueAsDecimal(datas[5].TrimEnd('0'));
		}

		public string Currency => datas[0];

		public DateTime StartDate { get; private set; }

		public DateTime EndDate { get; private set; }

		public decimal InRate { get; private set; }

		public decimal ExRate { get; private set; }
	}
}

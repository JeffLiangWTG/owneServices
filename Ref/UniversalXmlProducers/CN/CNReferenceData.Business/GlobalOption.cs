using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.CNReferenceData.Services;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class GlobalOption
	{
		public ILog CreateDisposableLog(string programName)
		{
			Log = LogGetter.Invoke();
			Log.ProgramName = programName;
			return Log;
		}

		public ILog Log { get; private set; }

		public Func<ILog> LogGetter { get; set; } = () => new FileLogger(GlobalOption.Instance.Setting.GetFullLogFileName(), Logger.LogLevel.Debug);

		public ISetting Setting { get => _setting; set => _setting = value; }

		ISetting _setting = new Setting("CargoWise.RefDbRepo.CNReferenceData.CmdLine.config.json");

		public Func<DateTime> NowGetter { get; set; } = () => DateTime.UtcNow.AddHours(8);

		static readonly Lazy<GlobalOption> lazy = new Lazy<GlobalOption>(() => new GlobalOption());

		public static GlobalOption Instance => lazy.Value;

		public DateTime Now => NowGetter.Invoke();

		public DateTime FirstDayOfThisMonth => new DateTime(Now.Year, Now.Month, 1);

		public DateTime MinSmallDateTime { get; } = new DateTime(1900, 1, 1);
		public DateTime MaxSmallDateTime { get; } = new DateTime(2079, 6, 6, 23, 59, 0);

		public TariffAttributeRepository TariffAttributeRepository { get; } = new TariffAttributeRepository();

		public List<string> OutputFiles { get; set; } = new List<string>();
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using CargoWise.RefDbRepo.CNReferenceData.Services;

namespace CargoWise.RefDbRepo.CNReferenceData.CmdLine
{
	public class TariffsUpdateProgram
	{
		const string ArgumentKey_CheckUpdates = "-CHECKUPDATES";
		const string ArgumentKey_Date = "-DATE";
		const string ArgumentKey_DaysBack = "-DAYSBACK";

		public void Run(string[] args)
		{
			using (GlobalOption.Instance.CreateDisposableLog("CN Tariff Update Program"))
			{
				try
				{
					GlobalOption.Instance.Log.Info($"==========> Run Tariffs Update Program with arguments:{string.Join(" ", args)}");

					var arguments = GetArguments(args);

					bool checkUpdates = arguments.ContainsKey(ArgumentKey_CheckUpdates);

					DateTime? effectiveDate = null;
					int? daysBack = null;

					if (checkUpdates)
					{
						if (arguments.ContainsKey(ArgumentKey_Date) || arguments.ContainsKey(ArgumentKey_DaysBack))
						{
							GlobalOption.Instance.Log.Warning($"Ignore arguments {ArgumentKey_Date} and {ArgumentKey_DaysBack} due to {ArgumentKey_CheckUpdates} is specified");
						}
					}
					else
					{
						if (arguments.ContainsKey(ArgumentKey_Date))
						{
							effectiveDate = DateTime.Parse(arguments[ArgumentKey_Date], CultureInfo.InvariantCulture);
						}

						if (arguments.ContainsKey(ArgumentKey_DaysBack))
						{
							if (effectiveDate.HasValue)
							{
								daysBack = int.Parse(arguments[ArgumentKey_DaysBack], CultureInfo.InvariantCulture);
							}
							else
							{
								GlobalOption.Instance.Log.Warning($"Ignore arguments {ArgumentKey_DaysBack} due to {ArgumentKey_Date} is not specified");
							}
						}
					}

					var tetUpdateTask = new JsonUpdateReader(checkUpdates, effectiveDate, daysBack, CreateEChinaAPIProxy(GlobalOption.Instance.Log)).GetMainTask();
					tetUpdateTask.Start();
					tetUpdateTask.Wait();

					GlobalOption.Instance.Log.Info("==========> finished.");
				}
				catch (Exception ex)
				{
					GlobalOption.Instance.Log.Error("==========> error:", ex);
					throw;
				}
			}
		}

		static Dictionary<string, string> GetArguments(string[] args)
		{
			var result = new Dictionary<string, string>();
			foreach (var keyAndValue in args.Select(x => x.Split(':')))
			{
				result.Add(keyAndValue.ElementAt(0).ToUpperInvariant(), keyAndValue.ElementAtOrDefault(1));
			}

			return result;
		}

		protected virtual EChinaAPIProxy CreateEChinaAPIProxy(ILog logger) => new EChinaAPIProxy(GlobalOption.Instance.Setting, logger);
	}
}

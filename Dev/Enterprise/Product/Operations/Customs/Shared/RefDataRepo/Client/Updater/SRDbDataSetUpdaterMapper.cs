using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using CargoWise.RefDbRepo.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ILogger = CargoWise.RefDbRepo.Client.Common.ILogger;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class SRDbDataSetUpdaterMapper : IDataSetUpdaterMapper
	{
		readonly Dictionary<string, Tuple<ISRDbDataSetUpdater, IDataSetUpdaterManager, IEnumerable<DataSetVersion>>> sRDbDataSetUpdaterMapper;
		readonly IServerProxy proxy;
		readonly ILogger logger;
		readonly IClientConfiguration config;
		readonly IErrorReportingClientWrapper errorReportingClientWrapper;

		public SRDbDataSetUpdaterMapper(IServerProxy proxy, ILogger logger, IClientConfiguration config, IDbConnection connection)
		{
			Argument.NotNull(proxy, nameof(proxy));
			Argument.NotNull(logger, nameof(logger));
			Argument.NotNull(config, nameof(config));

			this.proxy = proxy;
			this.logger = logger;
			this.config = config;
			errorReportingClientWrapper = new ErrorReportingClientWrapper(true);

			sRDbDataSetUpdaterMapper = new Dictionary<string, Tuple<ISRDbDataSetUpdater, IDataSetUpdaterManager, IEnumerable<DataSetVersion>>>();

			SetUpdaterToMapper(connection, new SRDbUpdaterRegistration());
		}

		public IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetDataSets()
		{
			return sRDbDataSetUpdaterMapper.Keys
				.Select(dataSetUpdater => new SharedDataSetUpdater(DataSetUpdaterHelper.GetUpdaterName(dataSetUpdater, UpdaterType.RDU),
					sRDbDataSetUpdaterMapper[dataSetUpdater].Item3.Select(x => x.Name)));
		}

		public ISRDbDataSetUpdater GetSRDbDataSetUpdater(string updaterName)
		{
			return !sRDbDataSetUpdaterMapper.ContainsKey(updaterName)
				? null
				: sRDbDataSetUpdaterMapper[updaterName].Item1;
		}

		public IDataSetUpdaterManager GetDataSetUpdaterManager(string updaterName)
		{
			return !sRDbDataSetUpdaterMapper.ContainsKey(updaterName)
				? null
				: sRDbDataSetUpdaterMapper[updaterName].Item2;
		}

		public DataSetVersion GetDataSetVersion(string updaterName, string dataSetName)
		{
			return !sRDbDataSetUpdaterMapper.ContainsKey(updaterName)
				? null
				: sRDbDataSetUpdaterMapper[updaterName].Item3
					.FirstOrDefault(x => string.Compare(x.Name, dataSetName, StringComparison.OrdinalIgnoreCase) == 0);
		}

		void SetUpdaterToMapper(IDbConnection dbConnection, ISRDbUpdaterRegistration updaterRegistration)
		{
			Argument.NotNull(dbConnection, nameof(dbConnection));
			Argument.NotNull(updaterRegistration, nameof(updaterRegistration));

			var dbHelper = new DBHelper(dbConnection);

			var dataSetUpdaters = updaterRegistration.Get(proxy, dbHelper, errorReportingClientWrapper, logger);

			IDataSetUpdaterManager updaterManager = new DataSetUpdaterManager<ISRDbDataSetUpdater>(logger, dbConnection, new SRDbUpdaterDependencyProvider(dataSetUpdaters), config, errorReportingClientWrapper);

			foreach (var dataSetUpdater in dataSetUpdaters)
			{
				var dataSetVersions = Task.Run(dataSetUpdater.GetServerTimestampAsync).GetAwaiter().GetResult();
				if (dataSetVersions != null)
				{
					sRDbDataSetUpdaterMapper.Add(dataSetUpdater.UpdaterName, Tuple.Create(dataSetUpdater, updaterManager, dataSetVersions));
				}
				else
				{
					logger.WriteLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} is not available at the moment", string.Join(", ", dataSetUpdater.UpdaterName)));
				}
			}
		}

		public IDataSetUpdater GetDataSetUpdater(string updaterName)
		{
			throw new NotImplementedException("GetDataSetUpdater Method should not be called from SRDbDataSetUpdaterMapper");
		}
	}
}

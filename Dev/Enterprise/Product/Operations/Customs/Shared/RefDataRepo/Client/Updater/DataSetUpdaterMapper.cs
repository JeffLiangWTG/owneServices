using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ILogger = CargoWise.RefDbRepo.Client.Common.ILogger;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class DataSetUpdaterMapper : IDataSetUpdaterMapper
	{
		readonly Dictionary<string, Tuple<IDataSetUpdater, IDataSetUpdaterManager, IEnumerable<DataSetVersion>>> dataSetUpdaterMapper;
		readonly IServerProxy proxy;
		readonly ILogger logger;
		readonly IClientConfiguration config;

		public DataSetUpdaterMapper(IServerProxy proxy, ILogger logger, IClientConfiguration config)
		{
			Argument.NotNull(proxy, nameof(proxy));
			Argument.NotNull(logger, nameof(logger));
			Argument.NotNull(config, nameof(config));

			this.proxy = proxy;
			this.logger = logger;
			this.config = config;

			dataSetUpdaterMapper = new Dictionary<string, Tuple<IDataSetUpdater, IDataSetUpdaterManager, IEnumerable<DataSetVersion>>>();

			using (var connection = Db.NewAdminConnection())
			{
				SetUpdaterToMapper(((IDbConnectionInternals)connection).ADOConnection, new MainDbUpdaterRegistration());
			}
		}

		public IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetDataSets()
		{
			return dataSetUpdaterMapper.Keys
				.Select(dataSetUpdater => new SharedDataSetUpdater(DataSetUpdaterHelper.GetUpdaterName(dataSetUpdater, UpdaterType.REF),
					dataSetUpdaterMapper[dataSetUpdater].Item3.Select(x => x.Name)));
		}

		public IDataSetUpdater GetDataSetUpdater(string updaterName)
		{
			return !dataSetUpdaterMapper.ContainsKey(updaterName)
				? null
				: dataSetUpdaterMapper[updaterName].Item1;
		}

		public IDataSetUpdaterManager GetDataSetUpdaterManager(string updaterName)
		{
			return !dataSetUpdaterMapper.ContainsKey(updaterName)
				? null
				: dataSetUpdaterMapper[updaterName].Item2;
		}

		public DataSetVersion GetDataSetVersion(string updaterName, string dataSetName)
		{
			return !dataSetUpdaterMapper.ContainsKey(updaterName)
				? null
				: dataSetUpdaterMapper[updaterName].Item3
					.FirstOrDefault(x => string.Compare(x.Name, dataSetName, StringComparison.OrdinalIgnoreCase) == 0);
		}

		void SetUpdaterToMapper(IDbConnection dbConnection, IUpdaterRegistration updaterRegistration)
		{
			Argument.NotNull(dbConnection, nameof(dbConnection));
			Argument.NotNull(updaterRegistration, nameof(updaterRegistration));

			var dbHelper = new DBHelper(dbConnection);

			var dataSetUpdaters = updaterRegistration.Get(proxy, dbHelper);
			var dependencyProvider = new UpdaterDependencyProvider<IDataSetUpdater>(dataSetUpdaters, dbHelper.GetReferencedForeignKeys().ToArray());
			dependencyProvider.Initialize();

			IDataSetUpdaterManager updaterManager = new DataSetUpdaterManager<IDataSetUpdater>(logger, dbConnection, dependencyProvider, config, new ErrorReportingClientWrapper());

			foreach (var dataSetUpdater in dataSetUpdaters)
			{
				dataSetUpdater.Logger = logger;
				var dataSetVersions = Task.Run(dataSetUpdater.GetServerTimestampAsync).GetAwaiter().GetResult();
				if (dataSetVersions != null)
				{
					dataSetUpdaterMapper.Add(dataSetUpdater.UpdaterName, Tuple.Create(dataSetUpdater, updaterManager, dataSetVersions));
				}
				else
				{
					logger.WriteLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} is not available at the moment", string.Join(", ", dataSetUpdater.UpdaterName)));
				}
			}
		}

		public ISRDbDataSetUpdater GetSRDbDataSetUpdater(string updaterName)
		{
			throw new NotImplementedException("GetSRDbDataSetUpdater Method should not be called from DataSetUpdaterMapper");
		}
	}
}

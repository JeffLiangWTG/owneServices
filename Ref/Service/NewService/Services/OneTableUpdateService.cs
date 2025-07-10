using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Models;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	public abstract class OneTableUpdateService<TSchema, TContract> : ReferenceDataServiceBase<TContract>
		where TContract : RefDataSet
		where TSchema : class
	{
		protected override string TableCode => TypeExtension.GetTablePrefix(typeof(TSchema));

		static OneTableUpdateService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<TSchema, TContract>();
			});
		}

		static MapperConfiguration config;
		protected static MapperConfiguration Config
		{
			get { return config; }
			set
			{
				if (config != null) throw new InvalidProgramException("Mapper configuration should not be set second time");
				else config = value;
			}
		}

		protected OneTableUpdateService(IReadOnlyReferenceDataRepository refDbRepo) : base(refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
		}

		IEnumerable<TContract> GetDataCore(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, int? chunkSize, short dataSetId)
		{
			var mapper = Config.CreateMapper();

			var versionExpression = ExpressionHelper.GetPropertyExpression<RefDbVersionControl, Guid>(typeof(RefDbVersionControl).GetProperty(nameof(RefDbVersionControl.RVC_ParentPK)));

			var dataSets = RefDbRepo.Get<TSchema>()
					.Join(GetVersionControls(dataSetId).Filter(lowerTimestamp, upperTimestamp, checkpointPK),
						ExpressionHelper.GetPKExpression<TSchema>(),
						versionExpression,
						(tbl, version) => new { tbl, version });

			dataSets = dataSets.OrderBy(x => x.version.RVC_ParentPK);

			int idx = 0;
			foreach (var dataSet in dataSets)
			{
				var result = mapper.Map<TContract>(dataSet.tbl);
				result.Deleted = dataSet.version.RVC_Deleted;
				result.WriteCheckpoint(dataSet.version.RVC_ParentPK, ref idx, chunkSize);
				yield return result;
			}
		}

		public override IEnumerable<TContract> GetData(DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpoint, int? chunkSize, short datasetId)
		{
			return GetDataCore(lowerTimestamp, upperTimestamp, checkpoint, chunkSize, datasetId);
		}
	}
}

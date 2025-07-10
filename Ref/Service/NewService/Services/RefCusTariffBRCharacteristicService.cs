using System;
using System.Linq;
using AutoMapper;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService
{
	public class RefCusTariffBRCharacteristicService
	{
		static RefCusTariffBRCharacteristicService()
		{
			Config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<RefCusTariffBRCharacteristic, Models.RefCusTariffBRCharacteristic>();
				cfg.CreateMap<RefCusTariffBRCharacteristicAttribute, Models.RefCusTariffBRCharacteristicAttribute>();
				cfg.CreateMap<RefCusTariffBRCharacteristicValue, Models.RefCusTariffBRCharacteristicValue>();
			});
		}

		public RefCusTariffBRCharacteristicService(IReadOnlyReferenceDataRepository refDbRepo)
		{
			Argument.NotNull(refDbRepo, nameof(refDbRepo));
			mapper = Config.CreateMapper();
			this.RefDbRepo = refDbRepo;
		}

		static MapperConfiguration config;
		static MapperConfiguration Config
		{
			get { return config; }
			set
			{
				if (config != null) throw new InvalidProgramException("Mapper configuration should not be set second time");
				else config = value;
			}
		}

		readonly IMapper mapper;
		readonly IReadOnlyReferenceDataRepository RefDbRepo;

		public RefCusTariffBRCharacteristic[] GetRefCusTariffBRCharacteristicChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			return RefDbRepo.Get<RefCusTariffBRCharacteristic>().Where(x => x.ZB1_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZB1_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public RefCusTariffBRCharacteristicValue[] GetRefCusTariffBRCharacteristicValueChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			return RefDbRepo.Get<RefCusTariffBRCharacteristicValue>().Where(x => x.ZB2_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZB2_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public RefCusTariffBRCharacteristicAttribute[] GetRefCusTariffBRCharacteristicAttributeChunk(string parentCode, DateTime? lowerTimestamp, DateTime upperTimestamp, ICheckpoint checkpointPK, short dataSetId, Guid? startPK, Guid? endPK)
		{
			return RefDbRepo.Get<RefCusTariffBRCharacteristicAttribute>().Where(x => x.ZB3_DataSetCode == parentCode)
				.GetChunk(RefDbRepo.Get<RefDbVersionControl>().Where(x => x.RVC_DataSetId == dataSetId && x.RVC_IsPublished), x => x.ZB3_DataSetPK, lowerTimestamp, upperTimestamp, checkpointPK, startPK, endPK);
		}

		public Models.RefCusTariffBRCharacteristic[] GetSetData(RefCusTariffBRCharacteristic[] bRCharacteristicChunk, RefCusTariffBRCharacteristicValue[] bRCharacteristicValueChunk, RefCusTariffBRCharacteristicAttribute[] bRCharacteristicAttrChunk,
			Guid dataSetPK, ref int bRCharacteristicValueIdx, ref int bRCharacteristicAttrIdx, ref int bRCharacteristicIdx)
		{
			Argument.NotNull(bRCharacteristicChunk, nameof(bRCharacteristicChunk));
			Argument.NotNull(bRCharacteristicValueChunk, nameof(bRCharacteristicValueChunk));
			Argument.NotNull(bRCharacteristicAttrChunk, nameof(bRCharacteristicAttrChunk));
			Argument.GreaterThanOrEqual(bRCharacteristicValueIdx, 0, nameof(bRCharacteristicValueIdx));
			Argument.GreaterThanOrEqual(bRCharacteristicAttrIdx, 0, nameof(bRCharacteristicAttrIdx));
			Argument.GreaterThanOrEqual(bRCharacteristicIdx, 0, nameof(bRCharacteristicIdx));

			var bRCharacteristicValues = bRCharacteristicValueChunk.GetSetData(x => x.ZB2_DataSetPK, dataSetPK, ref bRCharacteristicValueIdx);
			var bRCharacteristicAttrs = bRCharacteristicAttrChunk.GetSetData(x => x.ZB3_DataSetPK, dataSetPK, ref bRCharacteristicAttrIdx);

			return bRCharacteristicChunk.GetSetData(x => x.ZB1_DataSetPK, dataSetPK, ref bRCharacteristicIdx).Select(x =>
			{
				var bm = mapper.Map<Models.RefCusTariffBRCharacteristic>(x);
				bm.RefCusTariffBRCharacteristicValues = bRCharacteristicValues.Where(v => v.ZB2_ZB1_Characteristic == x.ZB1_PK).Select(v =>
				{
					var vm = mapper.Map<Models.RefCusTariffBRCharacteristicValue>(v);
					return vm;
				}).ToArray();
				bm.RefCusTariffBRCharacteristicAttributes = bRCharacteristicAttrs.Where(a => a.ZB3_ZB1_Characteristic == x.ZB1_PK).Select(a =>
				{
					var vm = mapper.Map<Models.RefCusTariffBRCharacteristicAttribute>(a);
					return vm;
				}).ToArray();
				return bm;
			}).ToArray();
		}
	}
}

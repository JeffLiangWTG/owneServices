using AutoMapper;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public class RefCusConditionWithoutApplicability : RefCusCondition
	{
		static readonly MapperConfiguration config = new MapperConfiguration(cfg =>
		{
			cfg.CreateMap<RefCusCondition, RefCusConditionWithoutApplicability>();
			cfg.CreateMap<RefCusConditionWithoutApplicability, RefCusCondition>();
		});
		static readonly IMapper mapper = config.CreateMapper();

		public RefCusConditionWithoutApplicability()
		{
		}

		public RefCusConditionWithoutApplicability(RefCusCondition condition, ISafeRepository safeRepository)
		{
			originalCondition = condition;
			mapper.Map(condition, this);
		}

		public RefCusCondition ConvertToRefCusCondition()
		{
			if (originalCondition == null)
			{
				originalCondition = new RefCusCondition();
			}

			mapper.Map(this, originalCondition);
			return originalCondition;
		}

		RefCusCondition originalCondition;
	}
}

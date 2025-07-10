using AutoMapper;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	public class RefCusRateWithoutApplicability : RefCusRate
	{
		static readonly MapperConfiguration config = new MapperConfiguration(cfg =>
		{
			cfg.CreateMap<RefCusRate, RefCusRateWithoutApplicability>();
			cfg.CreateMap<RefCusRateWithoutApplicability, RefCusRate>();
		});
		static readonly IMapper mapper = config.CreateMapper();

		public RefCusRateWithoutApplicability()
		{
		}

		public RefCusRateWithoutApplicability(RefCusRate rate, ISafeRepository safeRepository)
		{
			originalRate = rate;
			mapper.Map(rate, this);
		}

		public RefCusRate ConvertToRefCusRate()
		{
			if (originalRate == null)
			{
				originalRate = new RefCusRate();
			}

			mapper.Map(this, originalRate);
			return originalRate;
		}

		RefCusRate originalRate;
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public class RateCommodityFMCSelectorViewModel : NonPersistentBusinessObject
	{
		public RateCommodityFMCSelectorViewModel() : this(new RateCommodityFMCViewModels())
		{
		}

		public RateCommodityFMCSelectorViewModel(RateCommodityFMCViewModels companyTariffs)
		{
			CompanyTariffs = companyTariffs;
		}

		public RateCommodityFMCViewModels CompanyTariffs { get; }

		public RateCommodityFMCViewModel SelectedPair { get; set; }
	}

	public class RateCommodityFMCViewModel : NonPersistentBusinessObject
	{
		public RateCommodityFMCViewModel(ZString commodity, ZString localCode, ZString fmcTariffId, ZString description, ZString rateSource)
		{
			Commodity = commodity;
			LocalCode = localCode;
			FMCTariffID = fmcTariffId;
			Description = description;
			RateSource = rateSource;
		}

		public ZString Commodity { get; }

		public ZString LocalCode { get; }

		public ZString FMCTariffID { get; }

		public ZString Description { get; }

		public ZString RateSource { get; }

		public CommodityFMCPair CommodityPair
		{
			get
			{
				return new CommodityFMCPair()
				{
					CommodityCode = Commodity,
					FMCTariffID = FMCTariffID,
					RateSource = RateSource
				};
			}
		}
	}

	public class RateCommodityFMCViewModels : NonPersistentBusinessObjectCollection<RateCommodityFMCViewModel>
	{
		public RateCommodityFMCViewModels()
			: base()
		{
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException();
	}
}

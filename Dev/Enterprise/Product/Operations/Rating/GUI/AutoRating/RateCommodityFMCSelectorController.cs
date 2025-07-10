using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public class RateCommodityFMCSelectorController : IRateCommodityFMCSelectorController
	{
		readonly IDialogService dialogService;
		internal RateCommodityFMCPairLogger logger;

		public RateCommodityFMCSelectorController(ZForm owningForm)
		{
			dialogService = new DialogService(owningForm);
			logger = new RateCommodityFMCPairLogger();
		}

		protected internal RateCommodityFMCSelectorController(IDialogService dialogService, RateCommodityFMCPairLogger logger)
		{
			this.dialogService = dialogService;
			this.logger = logger;
		}

		public void SelectAndUpdate(
			BusinessObjectFactory factory,
			IRatingSupporter supporter,
			DetailedGoodsDescriptionProxy detailedGoodsInfo)
		{
			var adapter = GetAdapter(supporter);
			if (adapter == null)
			{
				return;
			}

			var criteria = GetCriteria(factory, adapter);
			var commodity = criteria.OverriddenCommodity.FirstOrDefault()?.RH_Code;
			if (string.IsNullOrEmpty(commodity))
			{
				throw new InvalidOperationException("SelectAndUpdate method shall always be called with a non-blank RateCommodity");
			}

			var pairsToPickFrom = GetMatches(factory, criteria);
			dialogService.ShowFMCTariffIDLogs(logger.ToString());
			if (!pairsToPickFrom.IsNullOrEmpty())
			{
				var pickedPair = PickPair(factory, pairsToPickFrom);

				if (pickedPair != null)
				{
					var updater = GetUpdater(adapter);
					updater.UpdateRateCommodityCodeAndFMCTariffID(pickedPair.CommodityCode, pickedPair.FMCTariffID);

					var bothNotBlank =
						!string.IsNullOrEmpty(pickedPair.FMCTariffID) &&
						!string.IsNullOrEmpty(pickedPair.CommodityCode);

					if (bothNotBlank)
					{
						UpdateDetailedGoodsDescription(factory, updater, pickedPair, detailedGoodsInfo);
					}
				}
			}
			else
			{
				dialogService.ShowNoFMCTariffIDCombinationFound(commodity);
			}
		}

		protected virtual internal IEnumerable<CommodityFMCPair> GetMatches(BusinessObjectFactory factory, RatingCriteria criteria)
		{
			var selector = new RateCommodityFMCPairProvider(factory, logger);
			return selector.GetMatches(criteria);
		}

		protected virtual internal IJobDataUpdater GetUpdater(IAutoRating adapter)
		{
			return (IJobDataUpdater)adapter;
		}

		protected virtual internal RatingCriteria GetCriteria(BusinessObjectFactory factory, IAutoRating adapter)
		{
			var proxy = new AutoRatingProxy(adapter);
			return new RatingCriteria(proxy, factory);
		}

		protected virtual internal IAutoRating GetAdapter(IRatingSupporter supporter)
		{
			// Getting the adapter can fail if the job is not in the correct state.
			// such as an FCL shipment not being attached to a consol. In this case
			// I cannot get an adapter and will present the user with a message
			// showing the contents in the log. The log will state why the adapter
			// could not be retrieved.
			var logs = new InteractorLog();
			var adapter = supporter.AdaptersProvider.GetAdapters(logs, AutoRateOptions.AutorateRevenue).FirstOrDefault();

			if (adapter == null)
			{
				dialogService.ShowCannotDoOperationCheckMessage(logs.GetLogs());
			}

			return adapter;
		}

		void UpdateDetailedGoodsDescription(
			BusinessObjectFactory factory,
			IJobDataUpdater updater,
			CommodityFMCPair pickedPair,
			DetailedGoodsDescriptionProxy detailedGoodsInfo)
		{
			if (!detailedGoodsInfo.IsAvailable)
			{
				return;
			}

			var response = dialogService.PromptToUpdateOrReplaceDetailedGoodsDescription(detailedGoodsInfo.IsEmpty);
			if (response == ZDialogResult.Cancel)
			{
				return;
			}
			var shouldAppend = response == ZDialogResult.Yes;

			var commodity = factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, pickedPair.CommodityCode);
			updater.UpdateDetailedGoodsDescription(commodity.RH_DescriptionMultilingual, shouldAppend);
		}

		protected virtual internal CommodityFMCPair PickPair(BusinessObjectFactory factory, IEnumerable<CommodityFMCPair> pairsFound)
		{
			if (pairsFound.IsNullOrEmpty())
			{
				return null;
			}

			var collection = new RateCommodityFMCViewModels();
			foreach (var pair in pairsFound)
			{
				var commodity = factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, pair.CommodityCode);
				if (commodity != null)
				{
					var localCode = GetRateLocalCode(factory, pair.CommodityCode);
					collection.Add(new RateCommodityFMCViewModel(pair.CommodityCode, localCode, pair.FMCTariffID, commodity.RH_DescriptionMultilingual, pair.RateSource));
				}
			}

			var viewModel = new RateCommodityFMCSelectorViewModel(collection);
			ZFormModaliser.ShowDialogAndDispose(new RateCommodityFMCSelectorForm(viewModel));

			if (viewModel.SelectedPair != null)
			{
				return new CommodityFMCPair()
				{
					CommodityCode = viewModel.SelectedPair.Commodity,
					FMCTariffID = viewModel.SelectedPair.FMCTariffID
				};
			}
			else
			{
				return null;
			}
		}

		string GetRateLocalCode(BusinessObjectFactory factory, string newCommodityValue)
		{
			var query =
				new ZQuery(RefCommodityCodeMapSchema.LC_RH_NKCommodityCode, newCommodityValue)
				.AddToFilter(new ZQuery(RefCommodityCodeMapSchema.LC_LocalCodeProvider, GlobalCommodityCodeProviderList.Codes.Rating));

			var result = factory.LoadTop1<RefCommodityCodeMap>(query);
			return result?.LC_LocalCode ?? string.Empty;
		}

		class InteractorLog : IAutoRatingInteractor
		{
			readonly List<string> strings = new List<string>();

			public void Log(LogType type, string message)
			{
				strings.Add(message);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				Log(type, message);
			}

			public bool YesNoWarning(string message)
			{
				return false;
			}

			public string GetLogs()
			{
				return String.Join(System.Environment.NewLine, strings);
			}
		}
	}
}

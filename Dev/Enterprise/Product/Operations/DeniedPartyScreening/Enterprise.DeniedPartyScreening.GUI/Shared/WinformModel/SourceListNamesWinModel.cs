using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Business;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class SourceListNamesWinModel : InnerExpanderWinModel<SourceListNamesWinModel>
	{
		public SourceListNamesWinModel(SourceListNamesModel sourceListNamesModel)
		{
			Argument.NotNull(sourceListNamesModel, nameof(sourceListNamesModel));
			SourceListNamesModel = sourceListNamesModel;

			IncludedSourceListNames = new ObservableCollection<SourceListNameWinModel>(SourceListNamesModel.IncludedLists.Select(u => new SourceListNameWinModel(SourceListNamesModel.Factory, u)));
			ExcludedSourceListNames = new ObservableCollection<SourceListNameWinModel>(SourceListNamesModel.ExcludedLists.Select(u => new SourceListNameWinModel(SourceListNamesModel.Factory, u)));
		}

		public ObservableCollection<SourceListNameWinModel> IncludedSourceListNames { get; }

		public ObservableCollection<SourceListNameWinModel> ExcludedSourceListNames { get; }

		public int ExcludedSourceListNamesCount => ExcludedSourceListNames.Count;

		public override string ExpanderTitle => Res.GetString("539C44D9-817A-461A-8E40-51F5CD4EC193", "Source List Names");

		public override string ExpanderDescription => string.Empty;

		public override bool IsExpanderEnabled => true;

		public override string InnerExpanderTitle => IsInnerExpanderExpanded ? Res.GetString("1EDCDD37-917E-4AC6-B9F1-713D77AC36FD", "Hide Excluded ({0})", ExcludedSourceListNamesCount) : Res.GetString("9B69E729-9291-4B46-9DA2-FF80053F76A0", "Show Excluded ({0})", ExcludedSourceListNamesCount);

		public SourceListNamesModel SourceListNamesModel { get; }
	}
}

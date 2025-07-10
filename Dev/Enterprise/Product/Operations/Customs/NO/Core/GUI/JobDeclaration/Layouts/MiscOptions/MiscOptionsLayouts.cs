using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	sealed class MiscOptionsLayouts : IPanelLayoutProvider
	{
		public PanelLayout MiscOptions { get; }

		PanelLayout IPanelLayoutProvider.Layout => MiscOptions;

		public MiscOptionsLayouts()
		{
			MiscOptions = CreateMiscOptionsLayouts();
		}

		PanelLayout CreateMiscOptionsLayouts()
		{
			var builder = new CommonMiscOptionsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			var noBag = MiscOptionsControlBag.Instance;
			builder.AddControlBag(noBag);

			builder.AddColumn();
			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);
			builder.Add(noBag.RelatedDeclarationsUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}
	}
}

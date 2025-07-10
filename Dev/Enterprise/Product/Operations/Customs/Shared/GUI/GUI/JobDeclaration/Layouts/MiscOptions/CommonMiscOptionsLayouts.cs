using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonMiscOptionsLayouts : IPanelLayoutProvider
	{
		PanelLayout MiscOptions { get; }

		PanelLayout IPanelLayoutProvider.Layout => MiscOptions;

		public CommonMiscOptionsLayouts()
		{
			MiscOptions = CreateCommonMiscOptionsLayouts();
		}

		PanelLayout CreateCommonMiscOptionsLayouts()
		{
			var builder = new CommonMiscOptionsLayoutBuilder<BaseJobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.MiscellaneousOptionsSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BranchGuidFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.BrokerCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.MergeByDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PaymentPartyDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PaidByDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}

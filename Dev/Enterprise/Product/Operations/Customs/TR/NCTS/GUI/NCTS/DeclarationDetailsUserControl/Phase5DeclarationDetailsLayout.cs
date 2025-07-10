using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public sealed class Phase5DeclarationDetailsLayout : IPanelLayoutProvider
	{
		public Phase5DeclarationDetailsLayout()
		{
			Layout = CreateDeclarationDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateDeclarationDetailsLayout()
		{
			var builder = new DeclarationDetailsLayoutBuilder<Business.NctsDepartureMovementHeader>();
			var commonBag = builder.CommonBag;
			var trBag = DeclarationDetailsControlBag.Instance;
			builder.AddControlBag(trBag);

			builder.AddColumn();
			builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
			builder.Add(trBag.LrnTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DepartureStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PhaseStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ReleaseDateEdit, ControlWidthClass.Auto, commonBag.MrnTextBox);
			builder.Add(commonBag.AcceptanceDateEdit, ControlWidthClass.Auto, trBag.LrnTextBox);

			return builder.Build();
		}
	}
}

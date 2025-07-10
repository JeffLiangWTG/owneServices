using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI
{
	public sealed class DeclarationDetailsLayout : IPanelLayoutProvider
	{
		public DeclarationDetailsLayout()
		{
			Layout = CreateDeclarationDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateDeclarationDetailsLayout()
		{
			var builder = new DeclarationDetailsLayoutBuilder<Business.NctsDepartureMovementHeader>();
			var commonBag = builder.CommonBag;
			var nlBag = DeclarationDetailsControlBag.Instance;
			builder.AddControlBag(nlBag);

			builder.AddColumn();
			builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
			builder.Add(nlBag.FallbackUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.DepartureStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PhaseStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.ReleaseDateEdit, ControlWidthClass.Auto, commonBag.MrnTextBox);
			builder.Add(commonBag.AcceptanceDateEdit, ControlWidthClass.Auto, commonBag.DepartureStatusDropEdit);
			builder.Add(nlBag.FallbackProcedureCheckBox, ControlWidthClass.Medium, commonBag.PhaseStatusDropEdit);

			builder.SetVisibility(commonBag.MrnTextBox, x => !x.IsFallbackProcedure, l => l.IsFallbackProcedureInfo);
			builder.SetVisibility(nlBag.FallbackUserControl, x => x.IsFallbackProcedure, l => l.IsFallbackProcedureInfo);

			return builder.Build();
		}
	}
}

using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	internal sealed class ConsolidatedDeclarationLayoutProvider : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new ConsolidatedDeclarationLayoutBuilder<ConsolidatedDeclaration>();
				var commonBag = builder.CommonBag;
				var nzBag = ConsolidatedDeclarationControlBag.Instance;
				builder.AddControlBag(nzBag);

				builder.AddColumn();
				builder.Add(commonBag.JobNumberTextBox, ControlWidthClass.Medium);
				builder.Add(nzBag.ConsolidatedDeclarationDetailsUserControl, ControlWidthClass.Auto);
				builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
				builder.Add(nzBag.EntryStyleDropEdit, ControlWidthClass.Long);
				builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
				builder.Add(commonBag.ImporterGuidFindBox, ControlWidthClass.Long);
				builder.Add(nzBag.VesselCodeFindBox, ControlWidthClass.Long);
				builder.Add(nzBag.VoyageFlightNoTextBox, ControlWidthClass.Medium);
				builder.Add(commonBag.DischargeETADateEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				builder.Add(commonBag.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				builder.Add(commonBag.EntryPeriodDateEdit, ControlWidthClass.Auto);

				builder.SetVisibility(commonBag.EntryPeriodDateEdit, consolidatedDeclaration => consolidatedDeclaration.EntryStyle == JobMessageSubTypeList.Codes.Periodic, _ => null);
				builder.SetVisibility(nzBag.VesselCodeFindBox, consolidateDeclaration => consolidateDeclaration.LeadDeclaration.IsSea, _ => null);
				builder.SetCaption(nzBag.VoyageFlightNoTextBox, consolidatedDeclaration => consolidatedDeclaration.LeadDeclaration.IsAir ? Enterprise.Customs.NZ.GUI.Res.GetData("72E36571-8C87-4C89-A9A0-B98220021DFD", "Arrival Flight") : Enterprise.Customs.NZ.GUI.Res.GetData("74B6A257-A84E-4B25-ACED-F59B9C720A81", "Voyage"), _ => null);

				return builder.Build();
			}
		}
	}
}

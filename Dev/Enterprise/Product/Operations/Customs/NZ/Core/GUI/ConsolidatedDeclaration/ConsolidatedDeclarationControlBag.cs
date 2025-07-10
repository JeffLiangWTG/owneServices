using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	internal class ConsolidatedDeclarationControlBag : ControlBag
	{
		public static ConsolidatedDeclarationControlBag Instance => instance ?? (instance = new ConsolidatedDeclarationControlBag());

		[ThreadStatic]
		static ConsolidatedDeclarationControlBag instance;

		ConsolidatedDeclarationControlBag()
		{
			EntryStyleDropEdit = RegisterControl(nameof(EntryStyleDropEdit));
			VesselCodeFindBox = RegisterControl(nameof(VesselCodeFindBox));
			ConsolidatedDeclarationDetailsUserControl = RegisterControl(nameof(ConsolidatedDeclarationDetailsUserControl));
			VoyageFlightNoTextBox = RegisterControl(nameof(VoyageFlightNoTextBox));
		}

		public ControlReference EntryStyleDropEdit { get; }
		public ControlReference VesselCodeFindBox { get; }
		public ControlReference VoyageFlightNoTextBox { get; }
		public ControlReference ConsolidatedDeclarationDetailsUserControl { get; }

		protected override Control CreateTemplate() => new ConsolidatedDeclarationControlBagTemplate();
	}
}

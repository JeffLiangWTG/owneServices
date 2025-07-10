using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public sealed class EntryInstructionBasicDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new EntryInstructionBasicDetailsControl();

		public static EntryInstructionBasicDetailsControlBag Instance => instance ?? (instance = new EntryInstructionBasicDetailsControlBag());

		[ThreadStatic]
		static EntryInstructionBasicDetailsControlBag instance;

		EntryInstructionBasicDetailsControlBag()
		{
			TransNatureDropEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.TransNatureDropEdit));
			IsHighValueOvrdCheckBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.IsHighValueOvrdCheckBox));
		}

		public ControlReference TransNatureDropEdit { get; }
		public ControlReference IsHighValueOvrdCheckBox { get; }
	}
}

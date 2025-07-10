using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class EntryInstructionBasicDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new EntryInstructionBasicDetailsControl();

		public static EntryInstructionBasicDetailsControlBag Instance => instance ?? (instance = new EntryInstructionBasicDetailsControlBag());

		[ThreadStatic]
		static EntryInstructionBasicDetailsControlBag instance;

		EntryInstructionBasicDetailsControlBag()
		{
			StyleDropEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.StyleDropEdit));
			SubStyleDropEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.SubStyleDropEdit));
			CPCDropEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.CPCDropEdit));
			DescriptionTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.DescriptionTextBox));
			AssessmentDateEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.AssessmentDateEdit));
			DetailsLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.DetailsLabel));
			OtherPartiesSeparatorUserControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.OtherPartiesSeparatorUserControl));
			BondHolderOrganisationControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.BondHolderOrganisationControl));
			RemoverOrganisationControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.RemoverOrganisationControl));
			NewOwnerOrganisationControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.NewOwnerOrganisationControl));
		}

		public ControlReference StyleDropEdit { get; }

		public ControlReference SubStyleDropEdit { get; }

		public ControlReference CPCDropEdit { get; }

		public ControlReference DescriptionTextBox { get; }

		public ControlReference AssessmentDateEdit { get; }

		public ControlReference DetailsLabel { get; }

		public ControlReference OtherPartiesSeparatorUserControl { get; }

		public ControlReference BondHolderOrganisationControl { get; }

		public ControlReference RemoverOrganisationControl { get; }

		public ControlReference NewOwnerOrganisationControl { get; }
	}
}

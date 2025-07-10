using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using CusEntryInstruction = Enterprise.Customs.NO.Business.CusEntryInstruction;

namespace Enterprise.Customs.NO.GUI
{
	partial class EntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			SetEntryInstructionGridItemTracking();
			PreviousProcedureTabPage.RunWhenBindingOrFirstShown((_, _) => InitPreviousProcedureUserControl());
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			var layout = InstructionDetailsLayoutProvider;
			DetailsUserControl.UpdateLayout(layout);
		}

		protected IDeclarationFormLayoutProvider DeclarationFormLayoutProvider
		{
			get
			{
				declarationFormLayoutProvider ??= Customs.GUI.DeclarationFormLayoutProvider.GetLayoutProvider(JobDeclaration);
				return declarationFormLayoutProvider;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				entryInstructionGridItemTracker?.Dispose();
			}

			base.Dispose(disposing);
		}

		IPanelLayoutProvider InstructionDetailsLayoutProvider
		{
			get
			{
				if (declaration != JobDeclaration)
				{
					instructionDetailsLayoutProvider = DeclarationFormLayoutProvider.GetInstructionDetailsLayoutProvider(JobDeclaration);
					declaration = JobDeclaration;
				}
				return instructionDetailsLayoutProvider;
			}
		}

		void SetEntryInstructionGridItemTracking()
		{
			entryInstructionGridItemTracker = new ZGridItemTracker<CusEntryInstruction>(EntryInstructionsGrid);
			entryInstructionGridItemTracker.OnCurrentItemChanging += HookUnHookProcedureChangHandler;
			entryInstructionGridItemTracker.OnCurrentItemChanged += OnEntryInstructionChanged;
		}

		void OnEntryInstructionChanged(object sender, EventArgs e)
		{
			UpdatePreviousProcedureTabVisibility(sender, e);
		}

		void InitPreviousProcedureUserControl()
		{
			PreviousProcedureHostedControl.UserControlType = typeof(PreviousProcedureUserControl);
		}

		void UpdatePreviousProcedureTabVisibility(object sender, EventArgs e) => SetPreviousProcedureTabVisibility();

		void HookUnHookProcedureChangHandler(object sender, GridItemChangingEventArg<CusEntryInstruction> e)
		{
			var oldEntryInstruction = e.OldItem;
			if (oldEntryInstruction is not null)
			{
				oldEntryInstruction.CEI_ProcedureInfo.ValueChanged -= UpdatePreviousProcedureTabVisibility;
			}

			var newEntryInstruction = e.NewItem;
			if (newEntryInstruction is not null)
			{
				newEntryInstruction.CEI_ProcedureInfo.ValueChanged += UpdatePreviousProcedureTabVisibility;
			}
		}

		void SetPreviousProcedureTabVisibility()
		{
			PreviousProcedureTabPage.TabVisible = entryInstructionGridItemTracker.CurrentItem?.IsProcedureCodeWithOutOfWarehouse ?? false;
		}

		IDeclarationFormLayoutProvider declarationFormLayoutProvider;
		IPanelLayoutProvider instructionDetailsLayoutProvider;
		BaseJobDeclaration declaration;
		ZGridItemTracker<CusEntryInstruction> entryInstructionGridItemTracker;
	}
}

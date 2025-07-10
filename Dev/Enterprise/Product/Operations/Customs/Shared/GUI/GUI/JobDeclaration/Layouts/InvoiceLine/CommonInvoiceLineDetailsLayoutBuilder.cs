using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CommonInvoiceLineDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonInvoiceLineDetailsControlBag> where T : Business.BaseJobComInvoiceLine
	{
		public override CommonInvoiceLineDetailsControlBag CommonBag { get; } = CommonInvoiceLineDetailsControlBag.Instance;

		protected override int MaxColumns => 3;

		public override bool NarrowColumnForMediumControls => true;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.EntryInstructionGuidDropEdit, x => x.Declaration?.IsPersistent ?? false);
			SetVisibility(CommonBag.BondedWHSOrderLineNumberCalcEdit, x => x.IsBondedWHSOrderNumberVisible);
			SetVisibility(CommonBag.BondedWHSOrderNumberTextBox, x => x.IsBondedWHSOrderNumberVisible);
			SetVisibility(CommonBag.BondedWhsQuantityCalcDropEdit, x => x.IsBondedWhsQuantityVisible, bondedWhsQtyAndPreviousEntryNumberVisibilityDependencies);
			SetVisibility(CommonBag.PreviousEntryNumberTextBox, x => x.IsPreviousEntryNumberVisible, bondedWhsQtyAndPreviousEntryNumberVisibilityDependencies);
			SetVisibility(CommonBag.PreviousEntryLineNumberCalcEdit, x => x.IsPreviousEntryNumberVisible, bondedWhsQtyAndPreviousEntryNumberVisibilityDependencies);
		}

		readonly Func<T, ZPropertyInfo>[] bondedWhsQtyAndPreviousEntryNumberVisibilityDependencies = new Func<T, ZPropertyInfo>[] { x => x.JI_ProcedureInfo, x => x.JI_PreviousEntryNumberInfo, x => x.JI_PreviousEntryLineNumberInfo, x => x.JI_BondedWhsQuantityInfo };
	}
}

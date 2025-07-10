using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class PartImportClassificationForm : ZChildForm
	{
		public PartImportClassificationForm()
		{
			InitializeComponent();
			pivotGrid.AfterBind += new EventHandler(PivotGrid_AfterBind);
		}

		public PartImportClassificationForm(CusClassPartPivot pivot) : base(pivot)
		{
			InitializeComponent();
			pivotGrid.AfterBind += new EventHandler(PivotGrid_AfterBind);
		}

		void PivotGrid_AfterBind(object sender, EventArgs e)
		{
			if (pivotGrid.ListManager != null)
			{
				pivotGrid.ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
				ListManager_CurrentChanged(this, new EventArgs());
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (string.IsNullOrEmpty(dataMember))
			{
				dataMember = "Children";
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		public override string FormCaption
		{
			get { return "Classification"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			var currentPivot = (CusClassPartPivot)pivotGrid.ListManager.GetCurrent();
			importClassificationUserControl.CurrentPivot = currentPivot;
		}
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Guarantees
{
	public partial class GuaranteeTransactionFilterControl : ZFilterStripControl
	{
		public GuaranteeTransactionFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				PerformSearch += new EventHandler<PerformSearchEventArgs>(ZStmALogFilterControl_PerformSearch);
				this.gridCollection = gridCollection as CusGuaranteeLineTransactionCollection;
			}
		}

		public CusGuaranteeLineTransactionCollection gridCollection;

		protected override ZFilterGrid GetNewFilteredGrid()
		{
			return new GuaranteeTransactionLinesGrid();
		}

		protected override void AddAlwaysVisibleFilterStrips()
		{
			base.AddAlwaysVisibleFilterStrips();
			var strip = FilterBusinessObject.FilterStrips.AddNew();
			strip.FilterDescription = GuaranteeTransactionFilterStripBusinessObject.FilterConstants.TransactionDate;
			AddFilterStrip(strip);
		}

		protected void ZStmALogFilterControl_PerformSearch(object sender, EventArgs e)
		{
			gridCollection.AdditionalFilter = FilterBusinessObject.Filter;
		}
	}
}

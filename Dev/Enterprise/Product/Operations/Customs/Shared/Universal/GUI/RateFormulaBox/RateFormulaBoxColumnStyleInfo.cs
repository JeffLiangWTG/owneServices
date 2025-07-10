using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	[ZArchitecture.GUI.Testing.SuppressCheckControlLookupList]
	[ZArchitecture.GUI.Testing.SuppressCheckControlModuleId]
	public class RateFormulaBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public RateFormulaBoxColumnStyleInfo()
		{
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IUnitListForRateFormulaEditProvider UnitListProvider;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType => typeof(RateFormulaBoxColumnStyle);
	}

	public class RateFormulaBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		public RateFormulaBoxColumnStyle(RateFormulaBoxColumnStyleInfo columnInfo)
			: this(() => new RateFormulaBox() { UnitListProvider = columnInfo.UnitListProvider }, columnInfo)
		{
		}

		protected RateFormulaBoxColumnStyle(Func<RateFormulaBox> gridFindBox, RateFormulaBoxColumnStyleInfo columnInfo) : base(gridFindBox, columnInfo) { }

		protected new RateFormulaBoxColumnStyleInfo ColumnInfo => (RateFormulaBoxColumnStyleInfo)base.ColumnInfo;

		protected new RateFormulaBox FindBox => (RateFormulaBox)base.FindBox;
	}
}

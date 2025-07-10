using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	class CustomsReferenceDropEditColumnStyleInfo : ZDropEditColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(CustomsReferenceDropEditColumnStyle);
	}

	class CustomsReferenceDropEditColumnStyle : ZDropEditColumnStyle
	{
		public CustomsReferenceDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo) : this(columnInfo, () => new CustomsReferenceGridDropEdit())
		{
		}

		public CustomsReferenceDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo, Func<Control> editControl) : base(columnInfo, editControl)
		{
		}
	}
}

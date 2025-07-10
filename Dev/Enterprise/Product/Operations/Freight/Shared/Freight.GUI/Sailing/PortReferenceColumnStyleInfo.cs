using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class PortReferenceColumnStyleInfo : ZMultiControlColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(PortReferenceColumnStyle);
	}

	public class PortReferenceColumnStyle : ZMultiControlColumnStyle
	{
		public PortReferenceColumnStyle(PortReferenceColumnStyleInfo info)
			: base(() => new PortReferenceControl(), info)
		{
		}

		public new PortReferenceControl EditControl => (PortReferenceControl)base.EditControl;
	}
}

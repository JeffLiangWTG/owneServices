using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class CityTownColumnStyleInfo : ZMultiControlColumnStyleInfo
	{
		public CityTownColumnStyleInfo()
		{
		}

		public CityTownColumnStyleInfo(string columnName, int width)
			: base(columnName, width)
		{
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType => typeof(CityTownColumnStyle);
	}

	public class CityTownColumnStyle : ZMultiControlColumnStyle
	{
		public CityTownColumnStyle(ZMultiControlColumnStyleInfo columnInfo)
			: base(() => new CityTownMultiCombinationControl(), columnInfo)
		{
		}

		internal void ForceEndEditing()
		{
			IsEditing = false;
		}
	}
}

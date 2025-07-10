using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class TariffColumnStyle : ZCodeFindBoxColumnStyle
	{
		public TariffColumnStyle(TariffColumnStyleInfo columnInfo)
			: base(() => new TariffGridFindBox(), columnInfo)
		{
		}

		protected override void OnInit(Control control)
		{
			base.OnInit(control);
			var findBox = (TariffGridFindBox)control;
			var tarifColumnInfo = (TariffColumnStyleInfo)ColumnInfo;
			findBox.BindToTariffPropertyInfo = tarifColumnInfo.BindToTariffPropertyInfo;
			findBox.TariffInfo.TariffCode = tarifColumnInfo.TariffCode;
			findBox.TariffInfo.TariffType = tarifColumnInfo.TariffType;
		}
	}

	public class TariffColumnStyleInfo : Common.GUI.TariffColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(TariffColumnStyle); }
		}

		[DefaultValue(""), Category(TariffInfoDisplayName)]
		[ZColumnBindingMemberType(typeof(TariffPropertyInfo))]
		public string BindToTariffPropertyInfo { get; set; }

		[DefaultValue(TariffType.Export), DisplayName("Tariff Type"), Category(TariffInfoDisplayName)]
		public TariffType TariffType { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(""), DisplayName("Tariff Code Start With"), Category(TariffInfoDisplayName)]
		public string TariffCode { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used by VS designer")]
		const string TariffInfoDisplayName = "Tariff Info";
	}
}

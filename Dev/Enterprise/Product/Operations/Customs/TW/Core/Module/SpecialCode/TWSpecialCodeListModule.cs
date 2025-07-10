using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.Module
{
	public class TWSpecialCodeListModule : ZZRefCusCodeListWrapperModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.TW.SpecialCode;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalCodes;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override IFilterControl GetNewFilterControl()
		{
			var result = base.GetNewFilterControl();
			var newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = ListTypeCaption, ColumnName = TWSpecialCode.Schema.SC_CodeType };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 70, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = ListDescriptionCaption, ColumnName = TWSpecialCode.Schema.SC_CodeTypeDesc };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 100, true);
			newStyleInfo.IsVisible = false;
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = CountryCaption, ColumnName = TWSpecialCode.Schema.SC_Country };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 60, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = ControllingAgencyCaption, ColumnName = TWSpecialCode.Schema.SC_ControllingAgency };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 120, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZMultiLineTextBoxColumnInfo { Caption = ControllingAgencyDescriptionCaption, ColumnName = TWSpecialCode.Schema.SC_ControllingAgencyDescription };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 200, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZMultiLineTextBoxColumnInfo { Caption = RemarksCaption, ColumnName = TWSpecialCode.Schema.SC_Remarks };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 200, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZMultiLineTextBoxColumnInfo { Caption = SourceCaption, ColumnName = TWSpecialCode.Schema.SC_Source };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, 200, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);
			return result;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TWSpecialCodeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TWSpecialCodeListFilterStripBusinessObject();
		}

		protected override int CodeCaptionColumnWidth => 100;

		protected override bool ShowRecentItemsCore() => false;

		#region Captions

		internal static string ListTypeCaption => Res.GetString("9664D3A2-C790-40C5-B536-57E5A3FE33A8", "List Type");
		internal static string ListDescriptionCaption => Res.GetString("6C3F8156-2DF1-447B-B18B-CFC0700EA2F1", "List Description");
		internal static string CountryCaption => Res.GetString("1873C86C-DAB2-4486-8775-19C230C0CCB0", "Country");
		internal static string ControllingAgencyCaption => Res.GetString("0FC9F8BC-3BC9-45F9-BAE8-FB76CD7B9F3E", "Controlling Agency");
		internal static string ControllingAgencyDescriptionCaption => Res.GetString("E661CDD3-B00F-44D3-908B-ED816F1AEA5E", "Controlling Agency Description");
		internal static string RemarksCaption => Res.GetString("B4D7A7CB-42E4-4238-BAEA-C022965ADAA0", "Remarks");
		internal static string SourceCaption => Res.GetString("B9441149-F8AE-4669-8F20-EA2A1D0929C6", "Source");
		#endregion
	}
}

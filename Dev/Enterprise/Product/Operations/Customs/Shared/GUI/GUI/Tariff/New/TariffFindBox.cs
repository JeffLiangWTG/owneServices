using System;
using System.ComponentModel;
using CargoWise.ComponentModel.Design;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class TariffFindBox : ZCodeFindBox, ITariffFindBoxPopupSupport
	{
		public TariffFindBox()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				TariffInfo = new TariffPropertyInfo();
			}
		}

		#region Overrides

		protected internal IFindBoxPopup GetNewPopupFormInternal() => GetNewPopupForm();
		protected override IFindBoxPopup GetNewPopupForm()
		{
			return TariffFindBoxPopup.GetNewPopup() ?? base.GetNewPopupForm();
		}

		protected internal CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheckInternal(Type dataSourceType, string dataMember) => GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember);
		protected override CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = base.GetBindingMembersForCompileTimeCheck(dataSourceType, dataMember);
			if (!string.IsNullOrEmpty(BindToTariffPropertyInfo))
			{
				result.Add(new CompileTimeCheckBindingMember(dataSourceType, TariffInfo.GetType(), BindToTariffPropertyInfo));
			}
			return result;
		}

		#endregion

		#region Implementation of ITariffFindBoxPopupSupport

		[Category(TariffInfoDisplayName), Description("Bind the control to property which returns object of TariffPropertyInfo type. It will be used by BorderWise to show filtered data.")]
		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, "BindTo")]
		public string BindToTariffPropertyInfo { get; set; }

		[Category(TariffInfoDisplayName), Description("Specifies tariff info which will be used by BorderWise to show filtered data. Note: These data will be ignored if BindToTariffPropertyInfo is specified.")]
		[DisplayName(TariffInfoDisplayName), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TariffPropertyInfo TariffInfo { get; private set; }

		public object BoundItem
		{
			get { return CurrentItem; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used by VS designer")]
		const string TariffInfoDisplayName = "Tariff Info";

		#endregion

		protected internal object CurrentItemInternal => CurrentItem;
	}
}

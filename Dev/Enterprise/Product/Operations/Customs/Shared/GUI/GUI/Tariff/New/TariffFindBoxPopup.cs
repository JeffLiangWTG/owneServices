using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.GUI
{
	public interface ITariffFindBoxPopupSupport
	{
		TariffPropertyInfo TariffInfo { get; }
		object BoundItem { get; }
		string BindToTariffPropertyInfo { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public partial class TariffFindBoxPopup : IFindBoxPopup
	{
		internal TariffFindBoxPopup()
		{
		}

		public static IFindBoxPopup GetNewPopup()
		{
			return Env.Registry.ExternalBorderComplianceTool == ExternalBorderComplianceToolList.Codes.None ? null : new TariffFindBoxPopup();
		}

		#region Implementation of IFindBoxPopup

		#region ShowModal

		void IFindBoxPopup.ShowModal(IFindBox findBox, Form parentForm)
		{
			var findBoxPopUp = BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(CreateTariffFilter(findBox), null);

			if (findBoxPopUp != null)
			{
				findBoxPopUp.ShowModal(findBox, parentForm);
			}
		}

		#region SetTariffFilter

		internal BorderWiseFilters CreateTariffFilter(IFindBox findBox)
		{
			if (findBox is ITariffFindBoxPopupSupport tariffFindBox)
			{
				var filterData = !string.IsNullOrEmpty(tariffFindBox.BindToTariffPropertyInfo) ? GetBoundValue(tariffFindBox) : tariffFindBox.TariffInfo;
				tariffToSend = string.IsNullOrEmpty(filterData.TariffCode) || findBox.Code.StartsWith(filterData.TariffCode, StringComparison.CurrentCulture) ? findBox.Code : filterData.TariffCode;

				var filters = new BorderWiseFilters(tariffToSend);
				filters.ImpExp = Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada ? "I" : new ZString(filterData.TariffType.ToString()).Left(1).ToString();

				var dateForDutyRate = filterData.DateForDutyRate.IsEmpty ? ZDateTime.Today : filterData.DateForDutyRate;
				filters.DateForDutyRate = dateForDutyRate.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
				filters.AdditionalData = new AdditionalDataForBorderWise(filters.ImpExp, dateForDutyRate);

				return filters;
			}
			else
			{
				throw new ArgumentException(FormattableString.Invariant($"'{GetType()}' can be used by '{typeof(ITariffFindBoxPopupSupport)}' only but was '{findBox.GetType()}'."), nameof(findBox));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		internal TariffPropertyInfo GetBoundValue(ITariffFindBoxPopupSupport control)
		{
			var info = new BindingMemberInfo(control.BindToTariffPropertyInfo);
			PropertyDescriptor property = ZCustomTypeDescriptor.GetProperties(control.BoundItem.GetType())[info.BindingField];

			if (property == null)
			{
				const string errorMessage = "Could not find property you are binding to tariff property info. Data Source: '{0}' Data Member: '{1}'.";
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, errorMessage, control.BoundItem.GetType(), control.BindToTariffPropertyInfo));
			}

			if (property.PropertyType != typeof(TariffPropertyInfo))
			{
				const string errorMessage = "The type you are binding to tariff property info is not {0} and not supported by the {1}. Data Member: '{2}', Returned Type: '{3}'.";
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, errorMessage, typeof(TariffPropertyInfo), control.GetType(), control.BindToTariffPropertyInfo, property.PropertyType));
			}

			return (TariffPropertyInfo)property.GetValue(control.BoundItem);
		}

		#endregion

		internal void SetReturnedData(BorderWiseFilters filters, BorderWiseInvoiceLine selectedTariff, IFindBox findBox)
		{
			if (selectedTariff.TariffCode != tariffToSend)
			{
				findBox.Code = filters.AdditionalData.FormatBorderWiseInput((selectedTariff.TariffCode + " " + selectedTariff.StatCode).Trim());
			}
		}

		internal ZString tariffToSend;

		#endregion

		SilentSelectResult IFindBoxPopup.SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup) { return SilentSelectResult.None; }
		void IFindBoxPopup.SelectRowByPK(ZGuid pK) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		void IDisposable.Dispose() { }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		event EventHandler IFindBoxPopup.Closed { add { } remove { } }

		#endregion
	}
}

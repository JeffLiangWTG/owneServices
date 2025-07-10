using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgCusAccountCollectionUserControl : ZUserControl
	{
		public OrgCusAccountCollectionUserControl()
		{
			InitializeComponent();
		}

		#region Run Time Binding Changes

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var orgCusAccountCollection = GetBoundValue<OrgCusAccountCollection>(dataSource, dataMember);
			var newCountryCode = orgCusAccountCollection?.CountryCode ?? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommonDataGrouping;
			if (countryCode != newCountryCode)
			{
				countryCode = newCountryCode;
				var provider = OrgCusAccountGUIProvider.GetByCountryCode(countryCode);

				foreach (var zGridColumnInfo in OrgCusAccountCollectionsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => columnUpdaters.ContainsKey(x.ColumnName)))
				{
					var newColumn = columnUpdaters[zGridColumnInfo.ColumnName](provider);
					if (newColumn == null)
					{
						OrgCusAccountCollectionsGrid.SetAvailability(false, zGridColumnInfo.ColumnName);
					}
					else
					{
						newColumn.Update(zGridColumnInfo);
					}
				}
			}

			base.SetDataBinding(dataSource, dataMember);
		}
		string countryCode;

		readonly Dictionary<ZString, Func<OrgCusAccountGUIProvider, OrgCusAccountGUIProvider.ZGridColumnInfoControl>> columnUpdaters = new Dictionary<ZString, Func<OrgCusAccountGUIProvider, OrgCusAccountGUIProvider.ZGridColumnInfoControl>>
		{
			{ OrgCusAccount.Schema.CZ_Code, provider => provider.CZ_Code },
			{ OrgCusAccount.Schema.CZ_Account, provider => provider.CZ_Account },
			{ OrgCusAccount.Schema.CZ_Type, provider => provider.CZ_Type },
			{ OrgCusAccount.Schema.CZ_Issuer, provider => provider.CZ_Issuer },
			{ OrgCusAccount.Schema.DecryptedPassword, provider => provider.DecryptedPassword },
			{ OrgCusAccount.Schema.CZ_ReportingPeriod, provider => provider.CZ_ReportingPeriod },
			{ OrgCusAccount.Schema.CZ_RepresentativeID, provider => provider.CZ_RepresentativeID }
		};

		#endregion

		T GetBoundValue<T>(object dataSource, string dataMember)
		{
			var descriptor = dataSource != null && !string.IsNullOrEmpty(dataMember)
				? BindingHelper.GetDescriptor(BindingContext, dataSource, dataMember, false)
				: null;
			var value = descriptor != null
				? descriptor.GetValue(dataSource)
				: null;
			return value != null && typeof(T).IsAssignableFrom(value.GetType())
				? (T)value
				: default;
		}
	}
}

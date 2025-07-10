using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI
{
	public class HarmonisedCodeFormManager : MultipleItemFormManager<PackLine>
	{
		public HarmonisedCodeFormManager(ZGrid grid, string bindingPrefix = "", ZGrid containersGrid = null, bool addColumns = true)
			: base(grid, bindingPrefix)
		{
			this.grid = grid;
			this.containersGrid = containersGrid;
			columnsToUpdate = new List<ZString>();
			shouldAddColumns = addColumns;

			grid.CurrentCellChanged += Grid_CurrentCellChanged;
		}

		readonly ZGrid grid;
		readonly ZGrid containersGrid;
		readonly List<ZString> columnsToUpdate;
		readonly bool shouldAddColumns;
		ZMultiControlColumnStyleInfo countryColumnInfo;
		TariffColumnStyleInfo codeColumnInfo;

		CommonShipment Shipment => grid.DataSource as CommonShipment;

		CommonConsol Consol => grid.DataSource as CommonConsol;

		BusinessObjectFactory Factory => ((BusinessObject)grid.DataSource).Factory;

		protected override ZString LinkLabelText => Res.GetString("88afe242-5e7c-443c-a8d2-6e48c30679f3", "Multiple Harmonized System Country/Region Codes");

		protected override ZString[] ColumnsToUpdate => columnsToUpdate.ToArray();

		protected override void AddCountChangedEventHandlerToItemsCollection(PackLine parent, EventHandler eventHandler)
		{
			if (parent != null)
			{
				parent.HarmonisedCodes.CountChanged -= eventHandler;
				parent.HarmonisedCodes.CountChanged += eventHandler;
			}
		}

		protected override int CollectionCount(PackLine parent)
		{
			return parent.HarmonisedCodes.Count;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Binding string")]
		protected override void AddColumnsCore(ZGrid gridToAddTo, string bindingPrefixWithPlus)
		{
			if (!shouldAddColumns)
			{
				return;
			}

			var groupName = Res.GetData("HSCodes|56c0b30e-b3eb-4339-bf14-5b0eb1e91c5b", "Country/Region Harmonized Codes");
			var countryManager = "HSCountryManager";
			var codeManager = "HSCodeManager";

			countryColumnInfo = new ZMultiControlColumnStyleInfo()
			{
				ColumnName = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}HarmonisedCodes+{1}+Value", bindingPrefixWithPlus, countryManager),
				FieldTypeColumnName = string.Format(CultureInfo.InvariantCulture, "{0}HarmonisedCodes+{1}+FieldColumnType", bindingPrefixWithPlus, countryManager),
				BindToList = string.Format(CultureInfo.InvariantCulture, "{0}HarmonisedCodes+{1}", bindingPrefixWithPlus, "Countries"),
				GroupName = groupName,
				CaptionResourceString = Res.GetData("aeefb980-25af-4e66-a748-7a49229fc355", "Country/Region", "HS Country/Region", "Harmonized System Country/Region", "Harmonized System Code Country/Region"),
				IsVisible = false
			};

			codeColumnInfo = new TariffColumnStyleInfo()
			{
				ColumnName = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}HarmonisedCodes+{1}+Value", bindingPrefixWithPlus, codeManager),
				GroupName = groupName,
				CaptionResourceString = Res.GetData("bfbf55f6-4898-42ad-9c82-bec93077d471", "Country/Region HS Code", "Country/Region Harmonized Code", "Country Harmonized System Code"),
				IsVisible = false,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				GetEffectiveDate = () => Shipment != null ? TariffHelper.GetHSCodeEffectiveDate(Shipment) : TariffHelper.GetHSCodeEffectiveDate(Consol),
				TariffType = "HSN"
			};

			AddColumnCore(gridToAddTo, countryManager, countryColumnInfo);
			AddColumnCore(gridToAddTo, codeManager, codeColumnInfo);
		}

		void AddColumnCore(ZGrid gridToAddTo, string name, ZGridColumnInfo columnInfo, int? width = null)
		{
			if (gridToAddTo.ColumnStyles.Cast<ZGridColumnInfo>().Any(columnStyle => columnStyle.ColumnName == columnInfo.ColumnName))
			{
				return;
			}

			if (width != null)
			{
				ControlDpiScalingHelper.SetWidth(ref columnInfo, (int)width, true);
			}

			gridToAddTo.ColumnStyles.Add(columnInfo);
			columnsToUpdate.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)"HarmonisedCodes+{0}+Value", name));
		}

		void Grid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (countryColumnInfo != null && codeColumnInfo != null)
			{
				var packLine = CurrentPackLine;
				if (packLine != null)
				{
					var countryCode = packLine.HarmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountry;
					if (TariffColumnStyle is TariffColumnStyle columnStyle)
					{
						columnStyle.SetCountryCode(countryCode);
						columnStyle.SetDataGrouping(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode));
						var refCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
						if (refCountry == null)
						{
							columnStyle.SetErrorForUnsupportedCountry(Res.GetString("dac1fe95-5b88-4491-b79f-58b9ce75646e", "A valid Country/Region must be specified for Tariff lookup."));
						}
						else
						{
							columnStyle.SetErrorForUnsupportedCountry(Res.GetString("d01a7db9-7db7-4d10-b313-8dba3d7123a0", "Tariff lookup is not supported for country/region {0}. Enter the WCO Harmonized Code or enter the country/region specific HS code manually.", countryCode.ToUpper()));
						}
					}
				}
			}
		}

		PackLine CurrentPackLine
		{
			get
			{
				if (Shipment != null && grid.CurrentRowIndex >= 0)
				{
					return Shipment.OuterPackLines[grid.CurrentRowIndex];
				}
				else if (Consol != null
					&& containersGrid != null
					&& containersGrid.CurrentRowIndex >= 0
					&& grid.CurrentRowIndex >= 0)
				{
					return Consol.Containers[containersGrid.CurrentRowIndex].PackLines[grid.CurrentRowIndex];
				}

				return null;
			}
		}

		protected override void ShowMultipleItemFormCore(PackLine parent, Form parentForm)
		{
			ZFormModaliser.Show(new HarmonisedCodeForm(parent), parentForm);
		}

		TariffColumnStyle TariffColumnStyle
		{
			get
			{
				if (tariffColumnStyle == null)
				{
					var tariffColumn = grid.Columns.FirstOrDefault(x => x.ColumnName == "HarmonisedCodes+HSCodeManager+Value");
					if (tariffColumn != null)
					{
						tariffColumnStyle = (TariffColumnStyle)tariffColumn.ColumnStyle;
					}
				}

				return tariffColumnStyle;
			}
		}
		TariffColumnStyle tariffColumnStyle;

		#region Shortcut Menu

		protected override MultilingualString MenuCaption
		{
			get { return ResString.GetMultilingualString("3871a981-99d4-44c8-98fc-d5366ebc41db", "Harmonized Codes"); }
		}

		protected override Shortcut MenuShortcut
		{
			get { return Shortcut.CtrlH; }
		}

		#endregion
	}
}

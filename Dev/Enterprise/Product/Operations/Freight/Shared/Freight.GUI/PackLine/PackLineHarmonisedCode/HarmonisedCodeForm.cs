using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI
{
	public partial class HarmonisedCodeForm : ZChildForm
	{
		public HarmonisedCodeForm(PackLine parent)
			: base(parent)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				HSCodesGrid.CurrentCellChanged += HSCodesGrid_CurrentCellChanged;
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			bool errors = false;

			foreach (BusinessObject bizo in this.HSCodesGrid.List)
			{
				bizo.RunPreSaveValidation();
				if (bizo.HasErrors)
				{
					errors = true;
				}
			}

			if (errors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Close();
			}
		}

		#region Country Override

		void HSCodesGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (HSCodesGrid.CurrentRowIndex >= 0)
			{
				var countryCode = PackLine.HarmonisedCodes[HSCodesGrid.CurrentRowIndex].JLH_RN_NKCountry;
				var customsCountryOfJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
				tariffColumnStyleInfo.GetCountryCode = () => countryCode;
				tariffColumnStyleInfo.GetDataGrouping = () => customsCountryOfJurisdiction;

				if (TariffColumnStyle is TariffColumnStyle columnStyle)
				{
					columnStyle.SetCountryCode(countryCode);
					columnStyle.SetDataGrouping(customsCountryOfJurisdiction);

					var refCountry = PackLine.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
					if (refCountry == null)
					{
						columnStyle.SetErrorForUnsupportedCountry(Res.GetString("9987abb3-cdfd-4afd-89b6-843654a68b77", "A valid Country/Region must be specified for Tariff lookup."));
					}
					else
					{
						columnStyle.SetErrorForUnsupportedCountry(Res.GetString("a5409855-3b9d-41b6-a01a-0232a6b96680", "Tariff lookup is not supported for country/region {0}. Enter the WCO Harmonized Code or enter the country/region specific HS code manually.", countryCode.ToUpper()));
					}
				}
			}
		}

		#endregion

		#region PackLine

		PackLine PackLine => (PackLine)DataSource;

		#endregion

		#region Columns

		TariffColumnStyle TariffColumnStyle
		{
			get
			{
				if (tariffColumnStyle == null)
				{
					var tariffColumn = HSCodesGrid.Columns.FirstOrDefault(x => x.ColumnName == "JLH_Code");
					if (tariffColumn != null)
					{
						tariffColumnStyle = (TariffColumnStyle)tariffColumn.ColumnStyle;
					}
				}

				return tariffColumnStyle;
			}
		}
		TariffColumnStyle tariffColumnStyle;

		#endregion
	}
}

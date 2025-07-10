using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.GUI;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.GUI
{
	public class HVLVTariffColumnStyle : ZBaseFindBoxColumnStyle
	{
		public HVLVTariffColumnStyle(HVLVTariffColumnStyleInfo info)
			: base(() => new HVLVTariffGridFindBox { IsExport = info.IsExport }, info)
		{
		}
	}

	public class HVLVTariffColumnStyleInfo : Customs.Universal.GUI.TariffColumnStyleInfo
	{
		public HVLVTariffColumnStyleInfo(ZBool isExport) : base()
		{
			IsExport = isExport;
		}

		public ZBool IsExport { get; set; }

		public override Type ColumnStyleType => typeof(HVLVTariffColumnStyle);
	}

	public class HVLVTariffGridFindBox : Customs.Universal.GUI.TariffGridFindBox
	{
		public HVLVTariffGridFindBox() : base()
		{
		}

		public ZBool IsExport { get; set; }

		void UpdatePopupFormAdditionalDataCountryCode()
		{
			if (PopupForm != null && (PopupForm.GetType() == typeof(FindBoxWrapperForBorderWise)))
			{
				((FindBoxWrapperForBorderWise)PopupForm).UpdateAdditionalDataCountryCode(EffectiveDataGrouping);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToUpper")]
		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			var countryCode = IsExport ? ItemLine.HVS_RN_NKOriginCountryCode : DestinationCountryCode;
			GetCountryCode = () => countryCode;
			GetDataGrouping = () => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);

			if (DataRegistry.Instance.ExternalBorderComplianceTool == ExternalBorderComplianceToolList.Codes.None)
			{
				if (new BusinessObjectFactory().LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode) == null)
				{
					ErrorForUnsupportedCountry = Res.GetString("066ee322-51a3-4363-a7d1-2cea8ef659de", "A valid Country/Region must be specified for Tariff lookup.");
				}
				else
				{
					ErrorForUnsupportedCountry = Res.GetString("20de59f7-d353-4b46-83ae-6dc8e9603acf", "Tariff lookup is not supported for country/region {0}. Enter the WCO Harmonized Code or enter the country/region specific HS code manually.", countryCode.ToUpper());
				}
			}
			else
			{
				UpdatePopupFormAdditionalDataCountryCode();
			}

			base.SelectFromPopupForm();
		}

		HVLVItemLine ItemLine => ((ZGrid)this.Parent).GetCurrent() as HVLVItemLine;

		ZString DestinationCountryCode => destinationCountryCode.IsEmpty ? (destinationCountryCode = ItemLine.ShipmentDestinationCountryCode) : destinationCountryCode;
		ZString destinationCountryCode;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(IsExport ? "E" : "I", EffectiveDataGrouping, () => base.GetNewPopupForm());
		}
	}
}

using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class HarmonizedNumber : CusCodeData
	{
		public HarmonizedNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string CY_TariffFormatted = "CY_TariffFormatted";
		}

		#region Properties

		#region CY_Data

		[List(nameof(Lookups) + "." + nameof(CusCodeDataLookups.Tariffs))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		#endregion

		#region CY_TariffFormatted

		[List(nameof(Lookups) + "." + nameof(CusCodeDataLookups.Tariffs))]
		public virtual ZString CY_TariffFormatted
		{
			get { return new TariffFormatter().DisplayFormat(CY_Data).Left(Schema.CY_DataMaxLength); }
			set { CY_Data = value.KeepNumericCharacters(); }
		}

		public ZPropertyInfo CY_TariffFormattedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CY_TariffFormatted, x => CY_DataInfo); }
		}

		public TariffPropertyInfo CY_TariffFormattedTariffInfo
		{
			get
			{
				var tariffType = TariffType.Import;
				var commodity = (Commodity)Parent;
				if (commodity != null)
				{
					var shipment = commodity.Shipment;
					if (shipment != null
						&& shipment.B0_ShipmentType == ShipmentTypes.Codes.Inbond
						&& shipment.InBond.IsExport)
					{
						tariffType = TariffType.Export;
					}
				}
				return new TariffPropertyInfo(tariffType, ZDateTime.Today, ZString.Empty);
			}
		}

		#endregion

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.HarmonizedNumber;
			CY_Code = CusCodeDataTypeList.Codes.HarmonizedNumber;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(Commodity)); }
		}

		public new CusCodeDataLookups Lookups
		{
			get { return (CusCodeDataLookups)base.Lookups; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new CusCodeDataLookups(this);
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new HarmonizedNumberValidation(this);
		}

		#endregion
	}
}

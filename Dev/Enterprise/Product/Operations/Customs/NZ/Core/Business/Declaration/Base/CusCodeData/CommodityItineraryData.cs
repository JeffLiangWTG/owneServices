using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityItineraryData : Customs.Business.CusCodeData
	{
		public CommodityItineraryData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetHumanReadableNameForCY_Data();
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string CY_FormattedData = "CY_FormattedData";
		}

		#region CY_Code

		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				ZString oldValue = CY_Code;
				base.CY_Code = value;

				if (oldValue != CY_Code)
				{
					SetHumanReadableNameForCY_Data();
				}
			}
		}

		#endregion

		#region CY_Data

		[MaxLength(25)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value.ToUpper(); }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusAddInfoTypeAttribute.Codes.NZTSWCommodityItineraryData;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CommodityItinerary)); }
		}

		void SetHumanReadableNameForCY_Data()
		{
			CY_DataInfo.HumanReadableName = CY_Code.IsEmpty ? "CommodityItinerary" : "TSW (" + CY_Code + ")";
		}

		#endregion
	}
}

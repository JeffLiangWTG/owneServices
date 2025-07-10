using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	/// <summary>
	/// DO NOT CHANGE THIS FILE, PLEASE MOVE TO ZZRefCusMapCombined - eh?
	/// </summary>
	public sealed class RefCusMap : AutoRefCusMap
	{
		public RefCusMap(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("CusMapType")]
		public override ZString ZZM_ZZP_NKMapType
		{
			get { return base.ZZM_ZZP_NKMapType; }
			set { base.ZZM_ZZP_NKMapType = value; }
		}

		#endregion

		#region Related BusinessObjects

		public RefCusMapType CusMapType
		{
			get
			{
				if (cusMapType == null || (cusMapType.ZZP_MapType != ZZM_ZZP_NKMapType))
				{
					cusMapType = Factory.LoadFromNaturalKey<RefCusMapType>(RefCusMapTypeSchema.ZZP_MapType, ZZM_ZZP_NKMapType);
				}
				return cusMapType;
			}
		}
		RefCusMapType cusMapType;

		#endregion

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZM_ZZZ_NKDataGrouping
		{
			get { return base.ZZM_ZZZ_NKDataGrouping; }
			set { base.ZZM_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZM_ZZZ_NKDataGrouping); }
		}
	}
}

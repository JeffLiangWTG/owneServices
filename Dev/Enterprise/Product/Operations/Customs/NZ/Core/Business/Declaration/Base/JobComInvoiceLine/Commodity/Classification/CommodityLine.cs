using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CommodityLine : AutoCommodityLine, ICusCodeDataTypeSupporter
	{
		public CommodityLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public CommodityCodeCollection CommodityCodes
		{
			get
			{
				if (commodityCodes == null)
				{
					commodityCodes = new CommodityCodeCollection(this);
					commodityCodes.Load();
					RegisterEditableChildObject(commodityCodes);
				}

				return commodityCodes;
			}
		}
		CommodityCodeCollection commodityCodes;

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.CommodityLine|NZ_Classification", Caption = "Classification")]
		public override ZString NZ_Classification { get => base.NZ_Classification; set => base.NZ_Classification = value; }

		[List(nameof(AddInfoLookups) + "." + nameof(NZCommodityAddInfoLookups.ClassTypeList))]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.CommodityLine|NZ_ClassificationType", Caption = "Class. Type")]
		public override ZString NZ_ClassificationType
		{
			get { return base.NZ_ClassificationType; }
			set { base.NZ_ClassificationType = value; }
		}

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityData, typeof(CommodityCode));
			return result;
		}

		#endregion
	}
}

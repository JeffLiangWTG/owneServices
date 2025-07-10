using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Rating.GUI
{
	public class CommodityGroupViewModel : NonPersistentBusinessObject
	{
		[ResourceStringData("109274F1-7E14-49F5-A84C-1EB6F0F10175", Caption = "Universal Group", ShortCaption = "Univ. Group")]
		public ZString UniversalGroup
		{
			get => universalGroup;
			set => SetNonPersistentPropertyValue(UniversalGroupInfo, ref universalGroup, value);
		}
		ZString universalGroup;
		public ZPropertyInfo UniversalGroupInfo => GetZPropertyInfo(nameof(UniversalGroup));

		[ResourceStringData("488B0152-204A-4C6F-9E3C-4C6FDAC010DD", Caption = "Universal Group Description", ShortCaption = "Univ. Group Desc.")]
		public ZString UniversalGroupDescription
		{
			get => universalGroupDescription;
			set => SetNonPersistentPropertyValue(UniversalGroupDescriptionInfo, ref universalGroupDescription, value);
		}
		ZString universalGroupDescription;
		public ZPropertyInfo UniversalGroupDescriptionInfo => GetZPropertyInfo(nameof(UniversalGroupDescription));

		[ResourceStringData("0CC14F15-255D-4B5E-8D00-0981C41381AE", Caption = "Commodity Code", ShortCaption = "Comm. Code")]
		public ZString CommodityCode
		{
			get => commodityCode;
			set => SetNonPersistentPropertyValue(CommodityCodeInfo, ref commodityCode, value);
		}
		ZString commodityCode;
		public ZPropertyInfo CommodityCodeInfo => GetZPropertyInfo(nameof(CommodityCode));

		[ResourceStringData("BCB94BC6-0B70-4195-92A3-697764FA45B8", Caption = "Commodity Description", ShortCaption = "Comm. Desc.")]
		public ZString CommodityDescription
		{
			get => commodityDescription;
			set => SetNonPersistentPropertyValue(CommodityDescriptionInfo, ref commodityDescription, value);
		}
		ZString commodityDescription;
		public ZPropertyInfo CommodityDescriptionInfo => GetZPropertyInfo(nameof(CommodityDescription));
	}
}

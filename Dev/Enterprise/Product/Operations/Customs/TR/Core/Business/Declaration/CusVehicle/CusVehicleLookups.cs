using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusVehicleLookups : EU.Business.CusVehicleLookups
	{
		public CusVehicleLookups(AutoCusVehicle parent) : base(parent)
		{
		}

		public CodeDescriptionPairList GearsList => Factory.GetCachedValue<GearsList>();

		public CodeDescriptionPairList GearsDescriptionList
		{
			get
			{
				if (gearsDescriptionList == null)
				{
					gearsDescriptionList = new CodeDescriptionPairList();
					foreach (ICodeDescription gears in GearsList)
					{
						gearsDescriptionList.AddPair(gears.Description, gears.Description);
					}
					gearsDescriptionList.RemoveCode(Business.GearsList.Descriptions._0);
				}
				return gearsDescriptionList;
			}
		}
		CodeDescriptionPairList gearsDescriptionList;
	}
}

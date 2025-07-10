
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCusClassPartPivotAddInfo : AutoSGCusClassPartPivotAddInfo
	{
		public SGCusClassPartPivotAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			Parent = addInfoProperty.BizObj;

			this.AddInfoProperty = addInfoProperty;
			LoadPropertiesFromAddInfoProperty(false);
		}
	}
}

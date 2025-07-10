using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business
{
	public abstract class NOAddInfo : AutoNOAddInfo
	{
		protected NOAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}


using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class FieldSynchroniser : Customs.Business.FieldSynchroniser
	{
		public FieldSynchroniser(ZPropertyInfo destination, ZPropertyInfo source)
			: base(destination, source)
		{
		}

		public FieldSynchroniser(ZPropertyInfo destination, Customs.Business.SourceValueDelegate sourceValue, InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider)
			: base(destination, sourceValue, infosToHookValueChangedEventProvider)
		{
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerConditionValueParameterCollection : NonPersistentBusinessObjectCollection<TriggerConditionValueParameter>
	{
		public TriggerConditionValueParameterCollection(string eventCode)
		{
			this.eventCode = eventCode;
		}
		readonly string eventCode;
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TriggerConditionValueParameter(eventCode);
		}
	}
}

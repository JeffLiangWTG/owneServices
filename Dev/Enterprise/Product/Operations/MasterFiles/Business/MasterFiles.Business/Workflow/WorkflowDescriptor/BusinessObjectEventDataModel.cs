namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;

	public class BusinessObjectEventDataModel : EventDataModel<BusinessObject>
	{
		public BusinessObjectEventDataModel(BusinessObject parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		public BusinessObject Source { get; internal set; }

		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		public BusinessObject TriggerSource { get; internal set; }

		public LogEventDataModel Event { get; internal set; }

		public TriggerDataModel Trigger { get; internal set; }
	}

	public class BusinessObjectEventDataModel<T> : BusinessObjectEventDataModel where T : BusinessObject
	{
		public BusinessObjectEventDataModel(T parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		protected new T Parent
		{
			get
			{
				return (T)base.Parent;
			}
		}
	}
}

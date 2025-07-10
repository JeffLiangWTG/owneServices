using CargoWise.Types;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public abstract class BaseRetrievedInterchange
	{
		public string Contents;
		//we need to handle EX1 interchanges differently
		public string InterchangeType;

		public abstract ZDateTime RetrievedTime
		{
			get;
		}
	}
}

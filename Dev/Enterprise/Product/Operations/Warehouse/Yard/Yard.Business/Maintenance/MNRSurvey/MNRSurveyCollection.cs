using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRSurveyCollection : ActiveBusinessObjectCollection<MNRSurvey>
	{
		public MNRSurveyCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}

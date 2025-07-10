using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Business
{
	public class CompanyTariffProcessTask : RatingHeaderProcessTask<CompanyTariff>
	{
		public CompanyTariffProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.GlobalRates; }
		}
	}
}


using System.Data;

using CargoWise.EntityFramework;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Business
{
	public class QuotationProcessTask : RatingHeaderProcessTask<Quote>, IQuotationProcessTask
	{
		public QuotationProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Quotations; }
		}
	}
}


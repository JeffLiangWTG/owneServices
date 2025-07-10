using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolRatingAdaptersProvider<T> : ConsolRatingAdaptersProvider<T>
		where T : CFSLoadListConsol
	{
		public CFSLoadListConsolRatingAdaptersProvider(T parent) : base(parent) { }

		protected override ReadOnlyCollection<IJobInvoicingPlugIn> GetAdditionalJobs(T parent)
		{
			return new ReadOnlyCollection<IJobInvoicingPlugIn>(new List<IJobInvoicingPlugIn>());
		}
	}
}

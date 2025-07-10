using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class NonPersistentRatingSupporter : NonPersistentBusinessObject, IBillingPlugin, IRatingSupporter
	{
		public RatingAdaptersProvider AdaptersProvider
		{
			get { return new NonPersistentRatingSupporterAdaptersProvider(this, JobInvoicingConsumerTypes.Shipment); }
		}

		public Func<List<IAutoRating>> AdaptersGetter { get; set; }
		public Func<ReadOnlyCollection<IJobInvoicingPlugIn>> AdditionalJobsGetter { get; set; }
	}
}

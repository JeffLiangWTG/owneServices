using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PersistentRatingSupporter : DummyBusinessObject, IBillingPlugin, IRatingSupporter
	{
		public PersistentRatingSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public RatingAdaptersProvider AdaptersProvider
		{
			get { return new PersistentRatingSupporterAdaptersProvider(this, JobInvoicingConsumerTypes.Shipment); }
		}

		public Func<List<IAutoRating>> AdaptersGetter { get; set; }
		public Func<ReadOnlyCollection<IJobInvoicingPlugIn>> AdditionalJobsGetter { get; set; }
	}
}

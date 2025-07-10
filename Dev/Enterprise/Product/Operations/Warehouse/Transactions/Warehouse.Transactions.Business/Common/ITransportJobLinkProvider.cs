using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ITransportJobLinkProvider
	{
		event EventHandler TransportJobCreatedAndSaved;
		TransportJobResult TransportJobResult { get; }
	}

	public class TransportJobResult
	{
		public TransportJobResult(IRelatedJob transportJob)
		{
			TransportJob = transportJob;
		}

		TransportJobResult(string reasonForEmpty)
		{
			ReasonForEmptyOverride = reasonForEmpty;
		}

		public readonly IRelatedJob TransportJob;
		public readonly string ReasonForEmptyOverride;

		public static TransportJobResult Empty
		{
			get { return new TransportJobResult(transportJob: null); }
		}

		public static TransportJobResult MultipleJobs
		{
			get { return new TransportJobResult(Res.GetString("474f4a6b-6bca-4aee-a707-94c2323b304a", "Multiple Jobs")); }
		}
	}
}

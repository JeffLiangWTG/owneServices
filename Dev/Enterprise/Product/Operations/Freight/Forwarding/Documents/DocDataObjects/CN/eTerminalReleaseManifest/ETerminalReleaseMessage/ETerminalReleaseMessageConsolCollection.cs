using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	public class ETerminalReleaseMessageConsolCollection : NonPersistentBusinessObjectCollection<ETerminalReleaseMessageConsol>
	{
		public ETerminalReleaseMessageConsolCollection(JobVoyage voyage)
			: base(voyage?.Factory)
		{
			this.voyage = Argument.NotNull(voyage, nameof(voyage));
		}

		readonly JobVoyage voyage;

		public override void Load()
		{
			var consolFilter = new ZDBOnlyQuery(typeof(ForwardingConsol));

			var sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			var originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
			var transportQuery = new ZDBOnlySubQuery(typeof(Freight.Business.Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportQuery.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Core.Constants.TransportModes.Sea);

			var voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.PK);
			voyageQuery.AddToFilter(JobVoyageSchema.PK, voyage.PK);

			originQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, new string[] { "CNNBO", "CNNBG", "CNNGB" });

			originQuery.AddSubQuery(JobVoyOriginSchema.JA_JV, voyageQuery, JoinCondition.And);
			sailingFilter.AddSubQuery(JobSailingSchema.JX_JA, originQuery, JoinCondition.And);
			transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingFilter, JoinCondition.And);
			consolFilter.AddSubQuery(JobConsolSchema.PK, transportQuery, JoinCondition.And);

			foreach (var consol in Factory.Load<ForwardingConsol>(consolFilter))
			{
				Add(new ETerminalReleaseMessageConsol(consol));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Creating new Terminal Message Consol is not supported");
		}

		protected override bool AllowNewCore => false;
	}
}

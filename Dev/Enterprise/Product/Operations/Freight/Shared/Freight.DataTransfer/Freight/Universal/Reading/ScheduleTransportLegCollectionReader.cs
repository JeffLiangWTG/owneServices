using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ScheduleTransportLegCollectionReader<T> : DataObjectCollectionReader<TransportLeg, T> where T : JobSailing
	{
		public ScheduleTransportLegCollectionReader(DataObjectList<TransportLeg> transportLegs, IXmlImportLogger logger, UniversalObjectFactory factory, JobVoyage voyage)
			: base(transportLegs)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.voyage = Argument.NotNull(voyage, nameof(voyage));
		}

		readonly UniversalObjectFactory factory;
		readonly IXmlImportLogger logger;
		readonly JobVoyage voyage;

		#region Implementation

		protected override T[] BusinessObjects
		{
			get { return sailings ?? (sailings = voyage.Sailings.Cast<T>().ToArray()); }
		}
		T[] sailings;

		protected override T FindMatchingBusinessObject(TransportLeg dataObject)
		{
			var finder = new ScheduleTransportLegBusinessObjectFinder(dataObject, voyage);
			return (T)finder.Find(BusinessObjects);
		}

		protected override T ReadIntoBusinessObject(TransportLeg dataObject, T sailing)
		{
			var reader = new ScheduleTransportLegDataObjectReader(dataObject, logger, factory, voyage);
			return (T)reader.ReadIntoBusinessObject();
		}

		protected override void AddToCollection(T sailing)
		{
			voyage.Sailings.Add(sailing);
		}

		protected override void RemoveFromCollection(T sailing)
		{
			if (sailing.IsReferenced())
			{
				sailing.JX_IsPublished = false;
			}
			else
			{
				if (sailing != null && !sailing.IsDeleted && voyage.Sailings.Contains(sailing))
				{
					voyage.Sailings.RemoveAndDelete(sailing);
				}
			}
		}

		protected override void ReadIntoCollectionCore()
		{
			base.ReadIntoCollectionCore();

			if (ContentType == CollectionContent.Complete)
			{
				RemoveUnusedOrigins();
				RemoveUnusedDestinations();
			}
		}

		void RemoveUnusedOrigins()
		{
			var originsToDelete = voyage.Origins.Cast<VoyageOrigin>()
				.Where(o => !voyage.Sailings.Cast<JobSailing>().Any(s => s.JX_JA_RL_NKPortOfLoading == o.JA_RL_NKPortOfLoading))
				.ToArray();

			foreach (var origin in originsToDelete)
			{
				voyage.Origins.RemoveAndDelete(origin);
			}
		}

		void RemoveUnusedDestinations()
		{
			var destinationsToDelete = voyage.Destinations.Cast<VoyageDestination>()
				.Where(d => !voyage.Sailings.Cast<JobSailing>().Any(s => s.JX_JB_RL_NKPortOfDischarge == d.JB_RL_NKPortOfDischarge))
				.ToArray();

			foreach (var destination in destinationsToDelete)
			{
				voyage.Destinations.RemoveAndDelete(destination);
			}
		}

		#endregion
	}
}

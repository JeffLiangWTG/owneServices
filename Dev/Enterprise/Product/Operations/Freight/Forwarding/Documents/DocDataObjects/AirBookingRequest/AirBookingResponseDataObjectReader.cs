using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class AirBookingResponseDataObjectReader : ShipmentDataObjectReader<ForwardingConsol>
	{
		public AirBookingResponseDataObjectReader(UShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipment, logger, factory)
		{
			this.finder = new ConsolFinder(shipment?.DataContext?.DataTargetCollection?.FirstOrDefault(), factory);
			this.userMessages = new List<string>();
		}

		public override DataContextType DataContextType => DataContextType.ForwardingConsol;
		readonly ConsolFinder finder;

		public IReadOnlyCollection<string> UserMessages => userMessages;
		readonly List<string> userMessages;

		public ForwardingConsol Consol => finder.GetBestMatch();

		protected override IMatchingBusinessEntityFinder<ForwardingConsol> GetCombinedReferenceMatcher() => finder;
		protected override ForwardingConsol GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => finder.GetBestMatch();

		protected override void PopulateBusinessObject(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return;
			}

			if (dataObject.TransportLegCollection != null)
			{
				var transportLegPreader = new TransportLegCollectionReader<Freight.Business.Transport>(dataObject.TransportLegCollection, logger, factory, consol);
				transportLegPreader.ReadIntoCollection();
				consol.Transports.TryToSetTransportType();
			}

			if (dataObject.NoteCollection != null)
			{
				var notesReader = new ForwardingConsolNotesCollectionReader(dataObject.NoteCollection, logger, factory, consol);
				notesReader.ReadIntoCollection();
			}

			if (dataObject.ConsolCosts?.ConsolCostLineCollection != null)
			{
				var importer = new AirBookingResponseConsolCostingImporter(logger, factory.BOFactory);
				var messagesForUser = importer.ImportConsolCosting(dataObject.ConsolCosts, consol);
				userMessages.AddRange(messagesForUser);
			}

			consol.TryLogWhileClearTimeOfTransports(logger);
		}

		#region Nested Types

		sealed class ConsolFinder : IMatchingBusinessEntityFinder<ForwardingConsol>
		{
			public ConsolFinder(IDataTargetDataObject dataTarget, UniversalObjectFactory factory)
			{
				this.dataTarget = dataTarget;
				this.factory = factory;
			}

			readonly IDataTargetDataObject dataTarget;
			readonly UniversalObjectFactory factory;

			public ForwardingConsol GetBestMatch()
			{
				var key = dataTarget?.Key ?? ZString.Empty;

				return !key.IsEmpty
					? factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, key))
					: null;
			}
		}

		#endregion
	}
}

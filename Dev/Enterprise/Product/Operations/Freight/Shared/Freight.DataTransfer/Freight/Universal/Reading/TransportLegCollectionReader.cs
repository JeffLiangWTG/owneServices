using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class TransportLegCollectionReader<T> : DataObjectCollectionReader<TransportLeg, T> where T : Transport
	{
		public TransportLegCollectionReader(DataObjectList<TransportLeg> transportLegs, IXmlImportLogger logger, UniversalObjectFactory factory, ITransportParentCommon transportParent, bool allowUpdateSailing = true)
			: base(transportLegs)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.transportParent = Argument.NotNull(transportParent, "transportParent");
			this.allowUpdateSailing = allowUpdateSailing;
		}

		readonly UniversalObjectFactory factory;
		readonly IXmlImportLogger logger;
		readonly ITransportParentCommon transportParent;
		readonly bool allowUpdateSailing;

		TransportCollection Transports
		{
			get
			{
				TransportCollection transports = null;
				var parent = transportParent as ITransportParent;
				if (parent != null)
				{
					transports = parent.Transports;
				}
				else
				{
					transports = new TransportCollection(transportParent);
					transports.Load();
				}
				return transports;
			}
		}

		#region Implementation

		protected override T[] BusinessObjects
		{
			get { return transportLegs ?? (transportLegs = Transports.Cast<T>().ToArray()); }
		}

		T[] transportLegs;

		protected override T FindMatchingBusinessObject(TransportLeg dataObject)
		{
			var finder = new TransportLegBusinessObjectFinder(dataObject, transportParent);
			return (T)finder.Find(BusinessObjects);
		}

		protected override T ReadIntoBusinessObject(TransportLeg dataObject, T transport)
		{
			var reader = new TransportLegDataObjectReader(dataObject, logger, factory, transportParent, data => transport, allowUpdateSailing);
			return (T)reader.ReadIntoBusinessObject();
		}

		protected override void AddToCollection(T transport)
		{
			Transports.Add(transport);
		}

		protected override void RemoveFromCollection(T transport)
		{
			Transports.RemoveAndDelete(transport);
		}

		#endregion
	}

	class TransportLegCollectionReaderForLocalTransport : TransportLegCollectionReader<Transport>, ITransportLegCollectionReader
	{
		public TransportLegCollectionReaderForLocalTransport(DataObjectList<TransportLeg> transportLegs, IXmlImportLogger logger, UniversalObjectFactory factory, ITransportParentCommon transportParent)
			: base(transportLegs, logger, factory, transportParent)
		{
		}
	}

	class TransportLegCollectionReaderForTransitWarehouse : TransportLegCollectionReader<Transport>, ITransportLegCollectionReaderForTransit
	{
		public TransportLegCollectionReaderForTransitWarehouse(DataObjectList<TransportLeg> transportLegs, IXmlImportLogger logger, UniversalObjectFactory factory, ITransportParentCommon transportParent, Func<TransportLeg, bool> shouldSkipTransportLeg)
			: base(transportLegs, logger, factory, transportParent)
		{
			ShouldSkipTransportLeg = shouldSkipTransportLeg;
		}
		readonly Func<TransportLeg, bool> ShouldSkipTransportLeg;

		protected override bool SkipEntity(TransportLeg dataObject)
		{
			return ShouldSkipTransportLeg(dataObject);
		}
	}
}

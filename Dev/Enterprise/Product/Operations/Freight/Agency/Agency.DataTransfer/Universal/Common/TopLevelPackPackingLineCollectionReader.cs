using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class TopLevelPackPackingLineCollectionReader<T> : DataObjectCollectionReader<PackingLine, T> where T : AgencyShipmentContainer
	{
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly IBusinessObjectCollection containersCollection;
		readonly LinksManager links;

		public TopLevelPackPackingLineCollectionReader(PackingLine[] packingLines, IXmlImportLogger logger, UniversalObjectFactory factory, IBusinessObjectCollection containersCollection, LinksManager links)
			: base(packingLines)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.containersCollection = Argument.NotNull(containersCollection, "containersCollection");
			this.links = links;
		}

		#region Implementation

		protected override T[] BusinessObjects
		{
			get { return businessObjects ?? (businessObjects = containersCollection.ToArray<T>()); }
		}

		T[] businessObjects;

		protected override T FindMatchingBusinessObject(PackingLine dataObject)
		{
			return new AgencyContainerBusinessObjectFinderFromPackingLine<T>(dataObject, links).Find(BusinessObjects);
		}

		protected override T ReadIntoBusinessObject(PackingLine dataObject, T container)
		{
			var reader = new TopLevelPackPackingLineDataObjectReader<T>(dataObject, logger, factory, data => container);
			return reader.ReadIntoBusinessObject();
		}

		protected override void AddToCollection(T container)
		{
			containersCollection.Add(container);
		}

		protected override void RemoveFromCollection(T container)
		{
			containersCollection.Delete(container);
		}

		#endregion
	}
}



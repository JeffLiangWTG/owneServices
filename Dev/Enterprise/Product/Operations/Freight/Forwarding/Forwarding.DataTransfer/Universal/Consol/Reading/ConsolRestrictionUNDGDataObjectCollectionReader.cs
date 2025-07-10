using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ConsolRestrictionUNDGDataObjectCollectionReader : DataObjectCollectionReader<UNDG, ConsolDGRestrictions>
	{
		public ConsolRestrictionUNDGDataObjectCollectionReader(DataObjectList<UNDG> dataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, IBusinessObjectCollection undgBOCollection)
			: base(dataObjects)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.undgBOCollection = Argument.NotNull(undgBOCollection, "undgBOCollection");
		}

		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly IBusinessObjectCollection undgBOCollection;

		protected override ConsolDGRestrictions[] BusinessObjects => undgBOCollection.ToArray<ConsolDGRestrictions>();

		protected override void AddToCollection(ConsolDGRestrictions undgBO)
		{
			undgBOCollection.Add(undgBO);
		}

		protected override ConsolDGRestrictions FindMatchingBusinessObject(UNDG dataObject)
		{
			return null;
		}

		protected override ConsolDGRestrictions ReadIntoBusinessObject(UNDG dataObject, ConsolDGRestrictions targetBO)
		{
			return new ConsolRestrictionUNDGDataObjectReader(dataObject, logger, factory, data => targetBO).ReadIntoBusinessObject();
		}

		protected override void RemoveFromCollection(ConsolDGRestrictions businessObject)
		{
			undgBOCollection.Delete(businessObject);
		}
	}
}

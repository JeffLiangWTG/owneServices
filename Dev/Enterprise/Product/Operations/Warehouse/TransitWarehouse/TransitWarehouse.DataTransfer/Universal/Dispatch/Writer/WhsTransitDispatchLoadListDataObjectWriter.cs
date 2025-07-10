using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchLoadListDataObjectWriter : TopLevelDataObjectWriter<WhsItemDispatchLoadList, UniversalShipment>
	{
		public WhsTransitDispatchLoadListDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransitDispatchLoadList;
		}

		protected override void PopulateDataObject(WhsItemDispatchLoadList loadList, UniversalShipment dataObject)
		{
			dataObject.PopulateTransportMode(loadList.WDL_TransportMode);
			PopulateNotes(loadList, dataObject);
			PopulateReferences(loadList, dataObject);
		}

		void PopulateNotes(WhsItemDispatchLoadList loadList, UniversalShipment dataObject)
		{
			dataObject.SetNoteCollection(() =>
			{
				var notes = loadList.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		void PopulateReferences(WhsItemDispatchLoadList loadList, UniversalShipment dataObject)
		{
			dataObject.SetAdditionalReferenceCollection(() => ProcessCollection(loadList.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Partial));
		}
	}
}

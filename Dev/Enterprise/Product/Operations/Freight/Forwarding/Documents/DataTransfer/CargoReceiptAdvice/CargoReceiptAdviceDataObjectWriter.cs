using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class CargoReceiptAdviceDataObjectWriter : DataObjectWriter<CargoReceiptAdvice, UniversalShipment>
	{
		public CargoReceiptAdviceDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(CargoReceiptAdvice cargoReceiptAdvice)
		{
			var uxmlShipment = new UniversalShipment(writeManager.WriterStrategy);

			uxmlShipment.DataContext = cargoReceiptAdvice
				.CreateUXmlDataContext()
				.AddUserBranchAndDepartment();

			uxmlShipment.InterimReceiptNumber = cargoReceiptAdvice.InterimReceipt;

			PopulateDateCollection(cargoReceiptAdvice, uxmlShipment);
			PopulateAdditionalReferences(cargoReceiptAdvice, uxmlShipment);
			PopulateNotes(cargoReceiptAdvice, uxmlShipment);
			PopulateAddresses(cargoReceiptAdvice, uxmlShipment);
			PopulatePackingLines(cargoReceiptAdvice, uxmlShipment);

			return uxmlShipment;
		}

		#region PopulateDateCollection

		void PopulateDateCollection(CargoReceiptAdvice cargoReceiptAdvice, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = CreateDates(cargoReceiptAdvice).ToList();

				return dates.Count > 0
					? dates
					: null;
			});
		}

		IEnumerable<Date> CreateDates(CargoReceiptAdvice cargoReceiptAdvice)
		{
			if (cargoReceiptAdvice.InterimReceiptDate.IsValid)
			{
				yield return new Date
				{
					Type = DateType.Received,
					Value = cargoReceiptAdvice.InterimReceiptDate,
					IsEstimate = false
				};
			}
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(CargoReceiptAdvice cargoReceiptAdvice, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(cargoReceiptAdvice));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(CargoReceiptAdvice cargoReceiptAdvice)
		{
			if (!cargoReceiptAdvice.HIRReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = cargoReceiptAdvice.HIRReference,

					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.EHubInterchangeReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.EHubInterchangeReference
					}
				};
			}
		}

		#endregion

		#region Notes

		void PopulateNotes(CargoReceiptAdvice cargoReceiptAdvice, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>();

				AddNote(notes, PredefinedNoteTypes.Instance.MarksAndNumbers.Description, cargoReceiptAdvice.MarksAndNumbers);

				return notes.Any() ? notes : null;
			});
		}

		void AddNote(DataObjectList<UniversalDataBuss.DataObjects.Universal.Note> notes, ZString description, ZString noteText)
		{
			if (!noteText.IsEmpty)
			{
				notes.Add(new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = description,
					IsCustomDescription = false,
					NoteText = noteText
				});
			}
		}

		#endregion

		#region OrganisationAddresses

		void PopulateAddresses(CargoReceiptAdvice cargoReceiptAdvice, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!cargoReceiptAdvice.BookingParty.IsEmpty())
				{
					addresses.Add(cargoReceiptAdvice.BookingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy));
				}

				if (!cargoReceiptAdvice.DepartureCFSAddress.IsEmpty())
				{
					addresses.Add(cargoReceiptAdvice.DepartureCFSAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCFSAddress), writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		#endregion

		#region PackingLines

		void PopulatePackingLines(CargoReceiptAdvice cargoReceiptAdvice, UniversalShipment uxmlShipment)
		{
			var uxmlPackingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			foreach (var packingLine in cargoReceiptAdvice.PackingLines)
			{
				var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
				uxmlPackingLines.Add(uxmlPackingLine);
			}

			uxmlShipment.SetPackingLineCollection(() => uxmlPackingLines.Any() ? uxmlPackingLines : null);
		}

		#endregion
	}
}

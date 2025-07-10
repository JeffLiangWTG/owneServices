using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.US
{
	sealed class AcasDataObjectWriter : DataObjectWriter<AirCargoAdvanceScreening, UniversalShipment>
	{
		public AcasDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(AirCargoAdvanceScreening acas)
		{
			var writer = GetWriter(acas.State);
			if (writer == null)
			{
				return null;
			}

			var shipment = writer.GetDataObject(acas);
			shipment.WayBillNumber = acas.HAWB;

			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(acas));
				return additionalReferences.Any() ? additionalReferences : null;
			});

			return shipment;
		}

		DataObjectWriter<AirCargoAdvanceScreening, UniversalShipment> GetWriter(AcasState messageType)
		{
			switch (messageType)
			{
				case AcasState.None:
				case AcasState.AmendmentRequired:
				case AcasState.HoldRemoved:
				case AcasState.AssessmentComplete:
					return new AcasOriginalWriter(writeManager);

				case AcasState.AcknowledgementRequired:
					return new AcasAcknowledgementWriter(writeManager);

				default:
					return null;
			}
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(AirCargoAdvanceScreening acas)
		{
			if (!acas.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = acas.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}
	}
}

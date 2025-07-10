using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105GoodsShipment_Consignment : IConsignment
	{
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;
		public NX5105GoodsShipment_Consignment(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
			this.declaration = Argument.NotNull(entryHeader.Declaration, "entryHeader");
			this.entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, "entryHeader");
		}

		IGovernmentAgencyGoodsItem IConsignment.GovernmentAgencyGoodsItem => null;

		ILocation IConsignment.TranshipmentLocation => null;

		ILocation IConsignment.TransitDeparture => null;

		ZString IConsignment.ManifestSerialNumber => declaration.JE_SLD;

		ZString IConsignment.ArrivalTransportMeansTypeCode => declaration.JE_Calc_TWTransportCode;

		ITransportMeans IConsignment.BorderTransportMeans => new NX5105Consignment_BorderTransportMeans(declaration);

		IConsignmentItem IConsignment.ConsignmentItem => new ConsignmentItemWrapper(declaration.JE_SplitMark.ConvertBoolToString("P"));

		ZString IConsignment.GoodsLocation => entryInstruction.CEI_GoodsLocation;

		ILocation IConsignment.LoadingLocation => new LocationWrapper(declaration.JE_RL_NKOrigin);

		IEnumerable<ITransportContractDocument> IConsignment.TransportContractDocuments => declaration.GetTransportContractDocumentsWithMasterBillSegmentID((id, typeCode) => new TransportContractDocumentWrapper(id, typeCode));

		IEnumerable<ITransportEquipment> IConsignment.TransportEquipments
		{
			get
			{
				var containers = entryHeader?.Declaration?.CusContainers;
				if (containers != null)
				{
					foreach (var cusContainer in containers)
					{
						yield return new TransportEquipmentWrapper(cusContainer);
					}
				}
			}
		}

		IBondedGoods IConsignment.BondedGoods
		{
			get
			{
				var bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
				return bondedGoods.Empty ? null : bondedGoods;
			}
		}

		#region Not Applicable

		IEnumerable<IAdditionalInformation> IConsignment.AdditionalInformations => declaration.GetAdditionalInformations();

		IPartyDetails IConsignment.Carrier => null;

		ZString IConsignment.ShippingOrderNumber => ZString.Empty;

		ITransportMeans IConsignment.DepartureTransportMeans => null;

		ZString IConsignment.TransitTransportMeansTypeCode => ZString.Empty;

		IEnumerable<ZString> IConsignment.GoodsLocations => null;

		ILocation IConsignment.UnloadingLocation => null;

		#endregion
	}
}

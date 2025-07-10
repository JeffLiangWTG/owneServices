using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageGoodsShipmentConsignment : LicensingMessageConsignment
	{
		public LicensingMessageGoodsShipmentConsignment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetArrivalTransportMeansTypeCodeCore() => Declaration.JE_Calc_TWTransportCode;

		protected override ITransportMeans GetDepartureTransportMeans() => null;

		protected override ZString GetManifestSerialNumberCore() => Declaration.JE_SLD;

		protected override IEnumerable<IAdditionalInformation> GetAdditionalInformationsCore() => Declaration.ReservedFields.Cast<JobDeclarationReservedField>().Select(x => new AdditionalInformationWrapper(x.CY_Code, x.CY_Data));

		protected override ILocation GetLoadingLocationCore() => new LocationWrapper(Declaration.JE_RL_NKOrigin);

		protected override ILocation GetTransitDepartureCore()
		{
			var portCode = Declaration.Transports.Cast<Transport>().OrderByDescending(x => x.JW_LegOrder).FirstOrDefault(x => !x.IsDomestic)?.JW_RL_NKDiscPortForBinding ?? ZString.Empty;
			return new LocationWrapper(portCode);
		}

		protected override ILocation GetUnloadingLocationCore() => null;

		protected override IEnumerable<ITransportContractDocument> GetTransportContractDocumentsCore()
		{
			var masterBill = Declaration.JE_MasterBill;
			var houseBill = Declaration.JE_HouseBill;
			if (Declaration.IsAir)
			{
				if (!masterBill.IsEmpty)
				{
					yield return new TransportContractDocumentWrapper(masterBill, MessageConstants.TransportContractDocumentTypeCodes._741);
				}

				if (!houseBill.IsEmpty)
				{
					yield return new TransportContractDocumentWrapper(houseBill, MessageConstants.TransportContractDocumentTypeCodes._703);
				}
			}
			else if (Declaration.IsSea)
			{
				if (!masterBill.IsEmpty)
				{
					yield return new TransportContractDocumentWrapper(masterBill, MessageConstants.TransportContractDocumentTypeCodes._704);
				}

				if (!houseBill.IsEmpty)
				{
					yield return new TransportContractDocumentWrapper(houseBill, MessageConstants.TransportContractDocumentTypeCodes._714);
				}
			}
		}

		protected override IEnumerable<ITransportEquipment> GetTransportEquipmentsCore()
		{
			return Declaration.CusContainers.Select(x => new TransportEquipmentWrapper(x));
		}
	}
}

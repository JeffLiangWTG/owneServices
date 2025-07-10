using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public static partial class CusCarMessagingHelper
	{
		public class SingleBillCusCarHeader : ICusCarHeader, IEDIMessageCollectionOwner
		{
			public SingleBillCusCarHeader(CusCarHeader header, AsycudaBill selectedBill)
			{
				this.header = header;
				this.selectedBill = selectedBill;
				selectedBillCountry = new CusCarBill(this.selectedBill);
			}

			readonly CusCarHeader header;
			readonly AsycudaBill selectedBill;
			readonly CusCarBill selectedBillCountry;

			#region forwarded properties

			ICusCarHeader parentInterface => header;

			public ZDateTime ArrivalDate => parentInterface.ArrivalDate;

			public ZString CARN => parentInterface.CARN;

			public ZString CarrierCode => parentInterface.CarrierCode;

			public ZString AgentType => parentInterface.AgentType;

			public ZString ContainerMode => parentInterface.ContainerMode;

			public ZString ConveyanceNumberOrTransportName => parentInterface.ConveyanceNumberOrTransportName;

			public ZString CustomsOffice => parentInterface.CustomsOffice;

			public ZString CW1Reference => parentInterface.CW1Reference;

			public ZDateTime DateAtCustomsOffice => parentInterface.DateAtCustomsOffice;

			public IEnumerable<ICusCarContainer> HeaderContainers => parentInterface.HeaderContainers;

			public ZDateTime DepartureDate => parentInterface.DepartureDate;

			public BusinessObjectFactory Factory => parentInterface.Factory;

			public ZString ImportExportNature => parentInterface.ImportExportNature;

			public ZDateTime ManifestDate => parentInterface.ManifestDate;

			public ManifestDocumentType ManifestDocumentType => parentInterface.ManifestDocumentType;

			public ZString ManifestNumber => parentInterface.ManifestNumber;

			public ZDateTime ManifestRegistrationDate => parentInterface.ManifestRegistrationDate;

			public ZString ManifestTypeOrBolNature => parentInterface.ManifestTypeOrBolNature;

			public ZString PlaceOfExit => parentInterface.PlaceOfExit;

			public ZString PortOfDischarge => parentInterface.PortOfDischarge;

			public ZString PortOfDischargeCountry => parentInterface.PortOfDischargeCountry;

			public ZString PortOfLoading => parentInterface.PortOfLoading;

			public ZString PortOfLoadingCountry => parentInterface.PortOfLoadingCountry;

			public ZString TransportMode => parentInterface.TransportMode;

			public ZString TransportNationality => parentInterface.TransportNationality;

			public ZString VesselID => parentInterface.VesselID;

			public ZString MasterBol => parentInterface.MasterBol;

			public ZDateTime EstimatedTimeOfLoading => parentInterface.EstimatedTimeOfLoading;

			public ZString CARNForAmendOrDelete => parentInterface.CARNForAmendOrDelete;

			public ZString CustomsCodeForContainerMode => parentInterface.CustomsCodeForContainerMode;

			public ZString CustomsCodeForImportExportNature => parentInterface.CustomsCodeForImportExportNature;

			public IEnumerable<ICusCarParty> GetParties(string billIssuer)
			{
				return parentInterface.GetParties(billIssuer);
			}

			public IEnumerable<ICusCarPerson> GetPeople(string billIssuer)
			{
				return parentInterface.GetPeople(billIssuer);
			}

			#endregion

			#region selected Bill lookups

			public EDIMessageCollection Messages
			{
				get => ((IEDIFACTMessageAttachee)selectedBillCountry).Messages;
			}

			public ZString MRNForAmendOrDelete
			{
				get => MessagingProvider.GetMRNForAmendOrDeleteFromMessages(Factory, selectedBill.CountryCode, Messages);
			}

			public IEnumerable<ICusTransport> Transports => null;

			BusinessObject IEDIMessageCollectionOwner.MessageOwner => selectedBill;

			IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => selectedBill.Messages;

			public IEnumerable<ICusCarContainer> GetContainersByBillIssuer(ZString billIssuer)
			{
				foreach (var line in GetLinesByBillIssuer(billIssuer))
				{
					foreach (var pack in line.Packages)
					{
						var c = (pack as CusCarPack)?.Container;
						if (c != null)
						{
							yield return new CusCarContainer(c);
						}
					}
				}
			}

			public ZDecimal GetGrossMassInKilosByBillIssuer(ZString billIssuer)
			{
				return GetLinesByBillIssuer(billIssuer).Sum(line => line.BillWeight.InKilogramsSafe);
			}

			public IEnumerable<ICusCarLine> GetLinesByBillIssuer(ZString billIssuer)
			{
				if (selectedBill.ABL_BillIssuer == billIssuer)
				{
					yield return new CusCarBill(selectedBill);
				}
			}

			#endregion
		}
	}
}

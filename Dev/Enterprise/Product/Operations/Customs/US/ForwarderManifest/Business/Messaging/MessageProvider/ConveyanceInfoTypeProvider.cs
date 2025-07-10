using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.US.MessageContracts.Interfaces;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class ConveyanceInfoTypeProvider : IConveyanceInfoType
	{
		readonly USExportAsycudaManifestHeader header;
		readonly USExportAsycudaBill masterBill;
		readonly USExportAsycudaBill bill;
		readonly string action;

		public ConveyanceInfoTypeProvider(USExportAsycudaBill bill, string action)
		{
			this.bill = Argument.NotNull(bill, "Manifest bill cannot be null");
			header = Argument.NotNull(bill.Header, "Manifest Header cannot be null");
			masterBill = header.MasterBill;
			this.action = action;
		}

		public IManifestStringType CarrierCode => header.IsSea ? new ManifestStringTypeProvider(MasterFiles.Business.GlbCompany.CurrentCompany.OrgProxy?.SCACCode ?? string.Empty) : new ManifestStringTypeProvider(header.AMA_CarrierCode);

		public IManifestStringType ConveyanceId => new ManifestStringTypeProvider(header.AMA_LloydsNumber);

		public IManifestStringType ConveyanceName => new ManifestStringTypeProvider(header.AMA_VesselName);

		public IManifestStringType FlightTripVoyageNumber => new ManifestStringTypeProvider(header.AMA_Voyage);

		public IManifestStringType ConveyanceCountryCode => new ManifestStringTypeProvider(header.AMA_RN_NKConveyanceNationality);

		public IManifestStringType ModeOfTransportationCode => new ManifestStringTypeProvider(TransportModeCalculator.CalculateUSTransportMode(header.AMA_TransportMode, header.AMA_ContainerMode));

		public IManifestStringType ScheduledArrivalDate => new ManifestStringTypeProvider("");

		public IManifestStringType ArrivalPortCode => new ManifestStringTypeProvider("");

		public IManifestStringType ScheduledDepartureDate => new ManifestStringTypeProvider(masterBill?.ABL_E_DEP.ToString("yyyyMMdd") ?? "");

		public IManifestStringType DeparturePortCode => new ManifestStringTypeProvider(masterBill?.ABL_CustomsLoadPort ?? "");

		public Collection<IEventInfoType> ConveyanceEventList => new Collection<IEventInfoType>();

		public Collection<IBOLInfoType> BOLInfoList
		{
			get
			{
				var bolList = new Collection<IBOLInfoType>();
				bolList.Add(new BOLInfoTypeProvider(bill, action, bill.IsChildMasterBill ? BillOfLadingClassificationTypeList.Codes.MasterBillOfLading : BillOfLadingClassificationTypeList.Codes.HouseBillOfLading));
				return bolList;
			}
		}

		public Collection<ICrewInfoType> CrewInfoList => new Collection<ICrewInfoType>();

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());

		public Collection<IActionType> Action => new Collection<IActionType>();
	}
}

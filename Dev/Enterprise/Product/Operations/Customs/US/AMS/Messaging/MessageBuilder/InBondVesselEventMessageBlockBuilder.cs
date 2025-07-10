using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class InBondVesselEventMessageBlockBuilder : ACECommonMessageBlockBuilder
	{
		public InBondVesselEventMessageBlockBuilder(IACEBillManifestMessageAttachee attachee)
			: base(attachee)
		{
		}

		protected override IEnumerable<MessageBlock> BuildCore()
		{
			var messageBlocks = new List<MessageBlock>();
			messageBlocks.AddRange(base.BuildCore());

			AddOtherBlocksGrouping(messageBlocks, attachee.BillOfLadingDetails);
			return messageBlocks;
		}

		void AddOtherBlocksGrouping(List<MessageBlock> messageBlocks, IACEBillOfLading billOfLading)
		{
			var billActionCode = billOfLading.BillActionCode;
			var icmh01 = new ICMH01()
			{
				MessageCode = billActionCode,
				InbondEntity = GetInbondEntity(billOfLading),
				CBPPort = GetPortCode(billOfLading),
				IssuerCode = GetIssuerCode(billOfLading),
				InbondCarrierCode = GetInBondCarrierCode(billOfLading),
				BondedCarrierID = GetBondedCarrierID(billOfLading),
				CityName = GetCityName(billOfLading),
				StateCode = GetStateCode(billOfLading),
				FIRMSLocationOnInBondArrival = GetFIRMS(billOfLading)
			};
			UpdateDateTime(icmh01, billOfLading);
			messageBlocks.Add(icmh01);

			var icmh02 = GetICMH02(billOfLading);
			if (icmh02 != null)
			{
				messageBlocks.Add(icmh02);
			}
		}

		ICMH02 GetICMH02(IACEBillOfLading billOfLading)
		{
			ICMH02 result = null;
			switch (billOfLading.BillActionCode)
			{
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByContainer:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByContainer:
				case InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferByContainer:
					result = GetICMH02ForInBond(billOfLading);
					break;
				case InBondAndVesselEventMessageCodeList.Codes.ExportInBondByContainer:
					result = GetICMH02ForExportInBondByContainer(billOfLading);
					break;
				case InBondAndVesselEventMessageCodeList.Codes.ExportInBond:
				case InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading:
					result = GetICMH02ForExportInBond(billOfLading);
					break;
				case InBondAndVesselEventMessageCodeList.Codes.VesselDeparture:
					result = GetICMH02ForVesselDeparture(billOfLading);
					break;
				case InBondAndVesselEventMessageCodeList.Codes.ReplaceTheUniqueVoyageIdentifier:
					result = GetICMH02ForReplaceTheUniqueVoyageIdentifier(billOfLading);
					break;
			}
			return result;
		}

		ICMH02 GetICMH02ForReplaceTheUniqueVoyageIdentifier(IACEBillOfLading billOfLading)
		{
			return new ICMH02()
			{
				UniqueVoyageIdentifier = GetUniqueVoyageIdentifier(billOfLading)
			};
		}

		ICMH02 GetICMH02ForVesselDeparture(IACEBillOfLading billOfLading)
		{
			return new ICMH02()
			{
				ForeignDeparturePort = attachee.ForeignDeparturePort
			};
		}

		ICMH02 GetICMH02ForExportInBond(IACEBillOfLading billOfLading, ICMH02 existingICMH02 = null)
		{
			ICMH02 result = null;
			var vesselName = billOfLading.MovemenDetails.ExportVesselName;
			if (!vesselName.IsEmpty)
			{
				result = existingICMH02 ?? new ICMH02();
				result.VesselName = vesselName;
				result.TransportationMethod = TransportationMethod;
			}
			return result;
		}
		const string TransportationMethod = "S";

		ICMH02 GetICMH02ForInBond(IACEBillOfLading billOfLading)
		{
			var result = new ICMH02();
			var inBondNumber = GetInBondNumber(billOfLading);
			if (inBondNumber.IsEmpty)
			{
				var billOfLadingNumber = billOfLading.BillOfLadingSequenceNumber;
				if (!billOfLadingNumber.IsEmpty)
				{
					result.ReferenceIdentifierQualifier = GetBillOfLadingQualifier(billOfLading);
					result.ReferenceIdentifier = billOfLadingNumber;
				}
			}
			else
			{
				result.ReferenceIdentifierQualifier = InBondNumberQualifier;
				result.ReferenceIdentifier = inBondNumber;
			}
			return result;
		}
		internal const string InBondNumberQualifier = "IB";

		ZString GetBillOfLadingQualifier(IACEBillOfLading billOfLading)
		{
			return BillOfLadingStatusIndicatorList.IsOceanBillOfLading(billOfLading.BillOfLadingStatusIndicator) ? BillReferenceList.Codes.OB : BillReferenceList.Codes.BM;
		}

		ICMH02 GetICMH02ForExportInBondByContainer(IACEBillOfLading billOfLading)
		{
			var result = GetICMH02ForInBond(billOfLading);
			GetICMH02ForExportInBond(billOfLading, result);
			return result;
		}

		ZString GetUniqueVoyageIdentifier(IACEBillOfLading billOfLading)
		{
			return billOfLading.BillActionCode == InBondAndVesselEventMessageCodeList.Codes.ReplaceTheUniqueVoyageIdentifier ? ZString.Empty : ZString.Empty; // TODO
		}

		ZString GetFIRMS(IACEBillOfLading billOfLading)
		{
			var result = ZString.Empty;
			switch (billOfLading.BillActionCode)
			{
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBond:
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer:
				case InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferArrivalByBillOfLading:
					result = billOfLading.FIRMS.IsEmpty ? attachee.PortDetails.FIRMSCode : billOfLading.FIRMS;
					break;
			}
			return result;
		}

		ZString GetStateCode(IACEBillOfLading billOfLading)
		{
			return billOfLading.BillActionCode == InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability ? billOfLading.MovemenDetails.TOLStateCode : ZString.Empty;
		}

		ZString GetCityName(IACEBillOfLading billOfLading)
		{
			return billOfLading.BillActionCode == InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability ? billOfLading.MovemenDetails.TOLCityName : ZString.Empty;
		}

		ZString GetBondedCarrierID(IACEBillOfLading billOfLading)
		{
			var result = ZString.Empty;
			switch (billOfLading.BillActionCode)
			{
				case InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability:
					result = billOfLading.MovemenDetails.TOLBondedCarrierID;
					break;
				case InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion:
					result = billOfLading.MovemenDetails.BondedCarrierID;
					break;
			}
			return result;
		}

		ZString GetInBondCarrierCode(IACEBillOfLading billOfLading)
		{
			return billOfLading.BillActionCode == InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability ? billOfLading.MovemenDetails.TOLInBondCarrierCode : ZString.Empty;
		}

		ZString GetIssuerCode(IACEBillOfLading billOfLading)
		{
			var result = ZString.Empty;
			switch (billOfLading.BillActionCode)
			{
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferArrivalByBillOfLading:
					result = billOfLading.IssuerCode;
					break;
			}
			return result;
		}

		void UpdateDateTime(ICMH01 icmh01, IACEBillOfLading billOfLading)
		{
			var dateTime = ZDateTime.Empty;
			switch (billOfLading.BillActionCode)
			{
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBond:
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrival:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByContainer:
				case InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion:
					dateTime = billOfLading.MovemenDetails.ArrivalDateTime;
					break;
				case InBondAndVesselEventMessageCodeList.Codes.ExportInBond:
				case InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.ExportInBondByContainer:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondExport:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByContainer:
					dateTime = billOfLading.MovemenDetails.ExportDateTime;
					break;
				case InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability:
				case InBondAndVesselEventMessageCodeList.Codes.CancelTransferOfLiability:
					dateTime = billOfLading.MovemenDetails.TOLDateTime;
					break;
				case InBondAndVesselEventMessageCodeList.Codes.ChangeInTheEstimatedDateOfArrival:
				case InBondAndVesselEventMessageCodeList.Codes.VesselDeparture:
				case InBondAndVesselEventMessageCodeList.Codes.VesselArrival:
					dateTime = attachee.EventDateTime;
					break;
			}
			if (!dateTime.IsEmpty && dateTime.IsValid)
			{
				icmh01.Date = dateTime.Date;
				icmh01.Time = dateTime.ToString("HHmm");
			}
		}

		ZString GetInbondEntity(IACEBillOfLading billOfLading)
		{
			var result = ZString.Empty;
			if (IsInBondNumberRequired(billOfLading.BillActionCode))
			{
				result = GetInBondNumber(billOfLading);
			}
			else if (IsBillOfLadingSequenceNumberRequired(billOfLading.BillActionCode))
			{
				result = billOfLading.BillOfLadingSequenceNumber;
			}
			else if (IsContainerNumberRequired(billOfLading.BillActionCode))
			{
				result = GetContainerNumber(billOfLading);
			}
			return result;
		}

		ZString GetContainerNumber(IACEBillOfLading billOfLading)
		{
			// Send by Container is currently not supported; we only allow this here for certification
			// This is assuming that their is only one container for the Bill Of Lading
			var container = billOfLading.Containers.FirstOrDefault();
			return container == null ? ZString.Empty : container.ContainerEquipmentNo;
		}

		bool IsContainerNumberRequired(ZString code)
		{
			return code == InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer ||
				code == InBondAndVesselEventMessageCodeList.Codes.ExportInBondByContainer ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByContainer ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByContainer ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferByContainer;
		}

		bool IsBillOfLadingSequenceNumberRequired(ZString code)
		{
			return code == InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading ||
				code == InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferArrivalByBillOfLading;
		}

		bool IsInBondNumberRequired(ZString code)
		{
			return code == InBondAndVesselEventMessageCodeList.Codes.ArriveInBond ||
				code == InBondAndVesselEventMessageCodeList.Codes.ExportInBond ||
				code == InBondAndVesselEventMessageCodeList.Codes.TransferOfInBondLiability ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrival ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelInBondExport ||
				code == InBondAndVesselEventMessageCodeList.Codes.CancelTransferOfLiability ||
				code == InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion;
		}

		ZString GetInBondNumber(IACEBillOfLading billOfLading)
		{
			var result = ZString.Empty;
			var moveDetails = billOfLading.MovemenDetails;
			if (moveDetails != null)
			{
				result = moveDetails.PaperlessInbondNumber;
				if (result.IsEmpty)
				{
					result = moveDetails.ConventionalInbondNumber;
				}
				if (result.IsEmpty)
				{
					result = AMSEDIMessage.InBondNumberPlaceHolder;
				}
			}
			return result;
		}

		ZString GetPortCode(IACEBillOfLading billOfLading)
		{
			var result = ZString.Empty;
			switch (billOfLading.BillActionCode)
			{
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBond:
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading:
				case InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer:
					result = billOfLading.MovemenDetails.USPortOfDestination;
					break;
				case InBondAndVesselEventMessageCodeList.Codes.VesselArrival:
					result = attachee.PortDetails.DistrictPortOfUnladingCode;
					break;
				case InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion:
					result = billOfLading.MovemenDetails.USPortOfDestination; // TODO
					break;
			}
			return result;
		}
	}
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using static Enterprise.Integration.Customs.US.InBond;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEInBondWPMessageBlockBuilder
	{
		public ACEInBondWPMessageBlockBuilder(IInBondArriveExportTOLHeader inBondData, IInbondMessageSendingData messageSendingData = null)
		{
			this.inBondData = inBondData;
			this.messageSendingData = messageSendingData;
		}

		public ACEInBondWPMessageBlockBuilder(IInBondArriveExportTOLHeader inBondData, ICusInBondBill iBill, IInbondMessageSendingData messageSendingData = null)
			: this(inBondData, messageSendingData)
		{
			masterBillNumber = iBill.B0_MasterBillNumber;
			masterBillIssuerCode = iBill.B0_IssuerCode;
			houseBillIssuerCode = iBill.B0_HouseBillIssuerCode;
			houseBillNumber = iBill.B0_HouseBillNumber;
		}

		public ACEInBondWPMessageBlockBuilder(IInBondArriveExportTOLHeader inBondData, ICusInBondBill iBill, ICusInBondContainer iContainer, IInbondMessageSendingData messageSendingData = null)
			: this(inBondData, iBill, messageSendingData)
		{
			containerNumber = iContainer.BC_ContainerNum;
		}

		public IEnumerable<MessageBlock> Build(string actionCode)
		{
			blocks = null;
			BuildBlocks(actionCode);
			return Blocks;
		}

		protected void AddBlock(MessageBlock block)
		{
			Blocks.Add(block);
		}

		List<MessageBlock> Blocks
		{
			get { return blocks ?? (blocks = new List<MessageBlock>()); }
		}
		List<MessageBlock> blocks;

		void BuildBlocks(string actionCode)
		{
			GenerateWP10(actionCode);
			GenerateWP20(actionCode);
		}

		void GenerateWP10(string actionCode)
		{
			var wp10 = new INBWP10();
			wp10.ActionCode = actionCode;
			wp10.InbondNumber = inBondData.InBondNumber.Left(12);

			if (InBondWPActionCodeList.IsBillLevel(actionCode))
			{
				wp10.IssuerCodeOfMasterBillOfLading = masterBillIssuerCode.Left(4);
				wp10.MasterBillOfLading = masterBillNumber.Left(12);

				if (InBondWPActionCodeList.IsContainerLevel(actionCode))
				{
					wp10.ContainerNumber = containerNumber.Left(14);
				}

				if (ZZCustomsFunctionality.IsAMSHBREffective
					&& (inBondData.InBondImportTransportMode == TransportModeCodes.Codes.VesselNonContainer
						|| inBondData.InBondImportTransportMode == TransportModeCodes.Codes.VesselContainer))
				{
					wp10.IssuerCodeOfHouseBillOfLading = houseBillIssuerCode.Left(4);
					wp10.HouseBillOfLading = houseBillNumber.Left(12);
				}
				wp10.FIRMSLocationOnInBondArrival = inBondData.ArrivalFirmsCode.Left(4);
			}
			else if (!InBondWPActionCodeList.IsDiversionRequest(actionCode))
			{
				wp10.IssuerCodeOfMasterBillOfLading = inBondData.MasterBillIssuerCode.Left(4);
				wp10.MasterBillOfLading = inBondData.MasterBillNumber.Left(12);
				wp10.ContainerNumber = inBondData.ContainerNumber.Left(14);
				wp10.FIRMSLocationOnInBondArrival = inBondData.ArrivalFirmsCode.Left(4);
			}
			AddBlock(wp10);
		}

		void GenerateWP20(string actionCode)
		{
			var wp20 = new INBWP20();

			var dateTime = ZDateTime.Empty;
			var portCode = ZString.Empty;

			var carrierCode = inBondData.InBondCarrierCode.Left(4);
			var carrierID = inBondData.BondedCarrierID.Left(12);

			if (InBondWPActionCodeList.IsExportationAction(actionCode))
			{
				dateTime = inBondData.ExportDateTime;
				portCode = inBondData.PortOfExport;
			}
			else if (InBondWPActionCodeList.IsTOLAction(actionCode))
			{
				dateTime = inBondData.TOLDateTime;
			}
			else if (InBondWPActionCodeList.IsArrivalAction(actionCode))
			{
				dateTime = inBondData.ArrivalDateTime;
				portCode = inBondData.ScheduleDPortOfArrival;
			}
			else if (InBondWPActionCodeList.IsDiversionRequest(actionCode) && messageSendingData != null)
			{
				dateTime = messageSendingData.DiversionDateTime;
				portCode = messageSendingData.PortCode;
				carrierCode = messageSendingData.InBondCarrierCode.Left(4);
				carrierID = messageSendingData.BondedCarrierID.Left(12);
			}

			if (dateTime.IsValid)
			{
				wp20.Date = dateTime.Date;
				wp20.Time = dateTime.ToString("HHmmss");
			}

			wp20.PortOfArrival = portCode;
			wp20.InbondCarrierCode = carrierCode;
			wp20.BondedCarrierID = carrierID;

			if (!InBondWPActionCodeList.IsDiversionRequest(actionCode))
			{
				wp20.CityName = inBondData.CityName.Left(19);

				if (!inBondData.CityName.IsEmpty)
				{
					wp20.StateCode = inBondData.StateCode.Left(2);
				}
				wp20.ExportMOT = inBondData.InBondExportTransportMode.Left(2);
				wp20.ExportConveyance = inBondData.ExportConveyance.Left(23);
			}

			AddBlock(wp20);
		}

		readonly IInBondArriveExportTOLHeader inBondData;
		readonly IInbondMessageSendingData messageSendingData;
		readonly ZString masterBillNumber;
		readonly ZString masterBillIssuerCode;
		readonly ZString houseBillNumber;
		readonly ZString houseBillIssuerCode;
		readonly ZString containerNumber;
	}
}

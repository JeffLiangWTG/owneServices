using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FTZMessageBlockBuilder
	{
		public FTZMessageBlockBuilder(IFTZHeader ftzHeader)
		{
			this.ftzHeader = ftzHeader;
		}
		readonly IFTZHeader ftzHeader;

		List<MessageBlock> Blocks
		{
			get
			{
				return blocks ?? (blocks = new List<MessageBlock>());
			}
		}
		List<MessageBlock> blocks;

		public IEnumerable<MessageBlock> Build(UpdateActionCode actionCode)
		{
			blocks = null;
			GenerateFT10(actionCode);
			if (actionCode != UpdateActionCode.Delete)
			{
				if (actionCode == UpdateActionCode.Replace)
				{
					GenerateFT11();
					GenerateFT12();
				}

				GenerateFT20();
			}

			return Blocks;
		}

		#region Header level

		void GenerateFT10(UpdateActionCode actionCode)
		{
			var ft10 = new FTZFT10_01();
			ft10.ActionCode = UpdateActionCodeConverter.ConvertToString(actionCode);
			ft10.ZoneID = ftzHeader.ZoneID;
			ft10.CalendarYear = ftzHeader.CalendarYear;
			ft10.ControlNumber = !ftzHeader.ControlNumber.IsEmpty ? ftzHeader.ControlNumber.ToString() : MQEDIMessage.USFTZControlNumberPlaceHolder;
			ft10.ExpandedZoneIDIndicator = ftzHeader.ZoneID.Length == 9 ? YesNoList.Codes.Yes : YesNoList.Codes.No;
			ft10.PortCode = ftzHeader.PortCode;
			ft10.DirectDeliveryIndicator = ftzHeader.DirectDeliveryIndicator ? YesNoList.Codes.Yes : YesNoList.Codes.No;
			ft10.ABIFilerCode = ftzHeader.EntryFilerCode;
			ft10.ABIRoutingCode = ftzHeader.ABIRoutingCode;
			ft10.ZoneOperatorIdentifierformerlyIRSIdentifier = ftzHeader.IRSIdentifier;
			ft10.FIRMSIdentifier = ftzHeader.FirmsIdentifier;
			ft10.ApplicantForAdmission = ftzHeader.ImporterOfRecordID;
			Blocks.Add(ft10);
		}

		void GenerateFT11()
		{
			var ft11 = new FTZFT11();
			ft11.ContactName = ftzHeader.ContactName;
			ft11.ContactPhone = ftzHeader.ContactPhone;
			var reasonCodes = ftzHeader.ReasonCodes.ToList();
			if (reasonCodes.Count > 0)
			{
				ft11.ReasonCode = reasonCodes[0];
				reasonCodes.RemoveAt(0);
				if (reasonCodes.Count > 0)
				{
					ft11.AdditionalReasonCodes = string.Join("", reasonCodes);
				}
			}
			Blocks.Add(ft11);
		}

		void GenerateFT12()
		{
			if (!ftzHeader.Remarks.IsEmpty)
			{
				var ft12 = new FTZFT12();
				ft12.Remarks = ftzHeader.Remarks;
				Blocks.Add(ft12);
			}
		}

		void GenerateFT20()
		{
			foreach (var ftzConveyance in ftzHeader.Conveyances)
			{
				var ft20 = new FTZFT20();
				ft20.AdmissionType = ftzHeader.AdmissionType;
				ft20.ModeOfTransportation = ftzConveyance.TransportMode;
				ft20.SCACIdentifierOrAirlineCarrierCodeOfImportingCarrier = ftzConveyance.CarrierSCAC;
				ft20.ConveyanceName = ftzConveyance.ConveyanceName.Left(23);
				ft20.VoyageTripFlightNumber = ftzConveyance.VoyageNumber;
				ft20.ExportDate = ftzConveyance.ExportDate;
				ft20.ImportDate = ftzConveyance.ImportDate;
				ft20.PortOfUnlading = ftzConveyance.PortOfUnlading;
				ft20.ScheduledDateOfArrival = ftzConveyance.EstimatedDateOfArrival;
				Blocks.Add(ft20);

				foreach (var ftzBill in ftzConveyance.Bills)
				{
					GenerateBillLevelMessages(ftzBill);
				}
			}
		}

		#endregion

		#region Bill level

		void GenerateBillLevelMessages(IFTZBill bill)
		{
			GenerateFT40(bill);
			foreach (var itNumber in bill.ITNumbers)
			{
				GenerateFT41(itNumber.ITNumber);
			}

			if (ftzHeader.IncludePTTInAdmission)
			{
				if (!bill.IRSIdentifier.IsEmpty)
				{
					GenerateFT42(bill.IRSIdentifier);
				}

				foreach (var container in bill.Containers)
				{
					GenerateFT43(container);
				}
			}

			if (ftzHeader.AdmissionType != FTZAdmissionTypeCodeList.Codes.TemporaryDeposit)
			{
				foreach (var line in bill.Lines)
				{
					GenerateLineLevelMessages(line);
				}
			}
		}

		void GenerateFT40(IFTZBill bill)
		{
			var ftBillOfLading = (IFTBillOfLading)new FTZFT40_01();
			ftBillOfLading.BillOfLadingOrAirWaybill = bill.BillOfLading.Left(BillValidator.Constants.MaximumFTZBillLength);
			ftBillOfLading.HouseBill = bill.HouseBill;
			ftBillOfLading.Quantity = bill.Quantity;
			ftBillOfLading.CountryOfExport = bill.CountryOfExport;
			ftBillOfLading.ForeignLoadPort = bill.ForeignLoadPort;
			ftBillOfLading.FIRMSIdentifier = bill.FIRMSCode;
			Blocks.Add((MessageBlock)ftBillOfLading);
		}

		void GenerateFT41(ZString itNumber)
		{
			var ft41 = new FTZFT41();
			ft41.ITNumber = itNumber;
			Blocks.Add(ft41);
		}

		void GenerateFT42(ZString irsIdentifier)
		{
			var ft42 = new FTZFT42();
			ft42.IRSIdentifierBondedCarrier = irsIdentifier;
			Blocks.Add(ft42);
		}

		void GenerateFT43(IContainer container)
		{
			var ft43 = new FTZFT43();
			ft43.ContainerNumber = container.ContainerNumber;
			Blocks.Add(ft43);
		}

		#endregion

		#region Line level

		void GenerateLineLevelMessages(IFTZLine line)
		{
			GenerateFT50(line);
			var i = 0;
			if (!line.LongSPICode.IsEmpty)
			{
				GenerateFT61forSPI(line.LongSPICode);
				i++;
			}
			GenerateFT51(line);

			var description = new StringBuilder(line.Remarks.Trim());
			GenerateFT60("MID", line.ManufacturerReferenceID, ref description);

			if (!line.MiscPermitQualifer.IsEmpty)
			{
				GenerateFT60(line.MiscPermitQualifer, line.MiscPermitNumber, ref description);
			}

			while (description.Length > 0 && i < 99)
			{
				GenerateFT61(ref description);
				i++;
			}
		}

		void GenerateFT50(IFTZLine line)
		{
			var ft50 = new FTZFT50();
			ft50.LineItemNumber = line.LineNumber;
			ft50.HarmonizedTariffScheduleNumber = line.Tariff;
			ft50.SpecialProgramsIndicatorSPI = line.SpecialProgramsIndicatorPrimary;
			ft50.SpecialProgramsIndicatorSPICountry = line.SpecialProgramsIndicatorCountry;
			ft50.SpecialProgramsIndicatorSPISecondary = line.SecondarySPI;
			ft50.CountryOfOrigin = line.CountryOfOrigin;
			ft50.Quantity1 = line.Quantity1;
			ft50.UnitOfMeasure1 = line.UQ1;
			ft50.Quantity2 = line.Quantity2;
			ft50.UnitOfMeasure2 = line.UQ2;
			ft50.QuotaCategory = line.QuotaCategory;
			ft50.PNDisclaimerFlag = line.PNDisclaimer;
			Blocks.Add(ft50);
		}

		void GenerateFT51(IFTZLine line)
		{
			var ft51 = new FTZFT51();
			ft51.Weight = line.Weight;
			ft51.Value = line.Value;
			ft51.Charges = line.Charges;
			ft51.ZoneStatus = line.ZoneStatus;
			ft51.HarborMaintenanceFee = line.HMF;
			Blocks.Add(ft51);
		}

		void GenerateFT60(ZString referenceQualifer, ZString referenceID, ref StringBuilder description)
		{
			if (!referenceID.IsEmpty)
			{
				var ft60 = new FTZFT60();
				var length = Math.Min(description.Length, 45);
				ft60.Description = description.ToString().Substring(0, length);
				ft60.ReferenceID = referenceID;
				ft60.ReferenceQualifier = referenceQualifer;
				description.Remove(0, length);
				Blocks.Add(ft60);
			}
		}

		void GenerateFT61(ref StringBuilder description)
		{
			var ft61 = new FTZFT61();
			var length = Math.Min(description.Length, 78);
			ft61.Remarks = description.ToString().Substring(0, length);
			description.Remove(0, length);
			Blocks.Add(ft61);
		}

		void GenerateFT61forSPI(ZString specialProgramsIndicatorPrimary)
		{
			var ft61 = new FTZFT61();
			ft61.Remarks = specialProgramsIndicatorPrimary;
			Blocks.Add(ft61);
		}

		#endregion
	}
}

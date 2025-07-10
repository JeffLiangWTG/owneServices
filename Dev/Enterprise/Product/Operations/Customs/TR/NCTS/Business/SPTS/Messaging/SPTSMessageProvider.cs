using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSMessageProvider : ISPTS
	{
		public SPTSMessageProvider(SPTSHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
			MovementHeader = header.MovementHeader;
		}

		protected readonly SPTSHeader Header;
		protected readonly SPTSDepartureMovementHeader MovementHeader;

		BusinessObject IMessageSender.Parent => Header;

		IBusinessObjectCollection IMessageSender.Messages => Header.Messages;

		ZString IMessageSender.JobReference => Header.BH_JobReference;
		ZString ISPTS.PortOfPresentationDCode => TRMessageHelper.RemoveCountryCodePrefix(MovementHeader.BM_PortOfPresentationCode);
		ZString ISPTS.DestinationPortDCode => TRMessageHelper.RemoveCountryCodePrefix(MovementHeader.BM_DestinationPortCode);
		ZString ISPTS.BusinessRegNo => GlbCompany.CurrentCompany.GC_BusinessRegNo;
		ZString ISPTS.CarrierBusinessRegNo => MovementHeader?.InBondCarrier?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;

		ZString ISPTS.TransportType => GetTransportType(MovementHeader.BM_InlandTransportMode);

		ZString GetTransportType(ZString transportMode)
		{
			switch (transportMode)
			{
				case SPTSTransportModeList.Codes.SEA:
					return "10";
				case SPTSTransportModeList.Codes.AIR:
					return "40";
				default:
					return transportMode;
			}
		}

		ZString ISPTS.RegistrationNoToBeUpdated => Header?.RegistrationEntryNumber?.CE_EntryLineReference ?? ZString.Empty;

		GlbExternalPassword_TR TRBPassword => TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
		ZString ISPTS.UserID => TRBPassword != null ? TRBPassword.GP_UserID : ZString.Empty;

		ZString ISPTS.VoyageNumber => Header.BH_VoyageNumber;
		ZDateTime ISPTS.VoyageDate => Header.BH_SailingDate;
		ZString ISPTS.XmlRefId => Header.PK.ToString();
		IEnumerable<ISPTSBills> ISPTS.SPTSBills => GetBills(Header);

		IEnumerable<ISPTSBills> GetBills(SPTSHeader header)
		{
			ZInt itemSeq = 0;
			foreach (var item in header.Bills)
			{
				itemSeq++;
				yield return new SPTSBillProvider(item, itemSeq);
			}
		}

		IEnumerable<ISPTSUlds> ISPTS.SPTSUlds => GetSPTSUlds(Header);

		IEnumerable<ISPTSUlds> GetSPTSUlds(SPTSHeader header)
		{
			ZInt itemSeq = 0;
			foreach (var item in header.HeaderContainers)
			{
				itemSeq++;
				yield return new SPTSUldsProvider(item, itemSeq);
			}
		}
	}

	#region Bills

	public class SPTSBillProvider : ISPTSBills
	{
		public SPTSBillProvider(SPTSBill sptsbill, ZInt itemSeq)
		{
			SPTSBill = Argument.NotNull(sptsbill, nameof(sptsbill));
			ItemSeq = itemSeq;
		}

		protected readonly SPTSBill SPTSBill;
		protected readonly ZInt ItemSeq;

		ZString ISPTSBills.BillNumber => SPTSBill.B0_MasterBillNumber;
		ZInt ISPTSBills.BillOrderNo => ItemSeq;
		ZString ISPTSBills.DeclarationType => SPTSBill.B0_ReferenceQualifier;
		ZString ISPTSBills.DeclarationNo => SPTSBill.B0_ReferenceID;
		ZString ISPTSBills.IsSubType => GetSubType(SPTSBill.B0_ServiceType);

		ZString GetSubType(ZString b0_ServiceType)
		{
			switch (b0_ServiceType)
			{
				case "Y":
					return "EVET";
				case "N":
					return "HAYIR";
				default:
					return ZString.Empty;
			}
		}

		public IEnumerable<ISPTSBillLines> SPTSBillLines => SPTSBill.B0_ServiceType == "Y" ? GetSPTSBillLines(SPTSBill) : System.Linq.Enumerable.Empty<ISPTSBillLines>();

		IEnumerable<ISPTSBillLines> GetSPTSBillLines(SPTSBill sPTSBill)
		{
			ZInt itemSeq = 0;
			foreach (var item in sPTSBill.SPTSBillContainers)
			{
				itemSeq++;
				yield return new SPSTSBillLinesProvider(item, itemSeq);
			}
		}
	}

	public class SPSTSBillLinesProvider : ISPTSBillLines
	{
		public SPSTSBillLinesProvider(SPTSContainer sptsBillLines, ZInt itemSeq)
		{
			SPTSBillLines = Argument.NotNull(sptsBillLines, nameof(sptsBillLines));
			ItemSeq = itemSeq;
		}

		protected readonly SPTSContainer SPTSBillLines;
		protected readonly ZInt ItemSeq;

		ZInt ISPTSBillLines.LineOrderNo => ItemSeq;
		ZString ISPTSBillLines.LineContainerNo => SPTSBillLines.BC_ContainerNum;
	}

	#endregion

	public class SPTSUldsProvider : ISPTSUlds
	{
		public SPTSUldsProvider(SPTSContainer sptsUlds, ZInt itemSeq)
		{
			SPTSUlds = Argument.NotNull(sptsUlds, nameof(sptsUlds));
			ItemSeq = itemSeq;
		}

		protected readonly SPTSContainer SPTSUlds;
		protected readonly ZInt ItemSeq;

		ZInt ISPTSUlds.UldOrderNo => ItemSeq;
		ZString ISPTSUlds.UldNumber => SPTSUlds.BC_ContainerNum;
	}
}

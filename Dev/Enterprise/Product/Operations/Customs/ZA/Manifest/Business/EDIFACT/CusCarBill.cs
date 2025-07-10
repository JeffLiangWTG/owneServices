using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class CusCarBill : ICusCarLine
		, ICusCarPackage
		, IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider
		, IInterchangeSenderIdProvider
	{
		public CusCarBill(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		#region Forwarded Properties

		public ZString ABL_RN_NKCountry => bill.CountryCode;

		public ZString ABL_BillIssuer => bill.ABL_BillIssuer;

		#endregion

		#region ICusCarLine - for messaging

		public ZString BillType => bill.ABL_BolType;

		public ZString BillNumber => bill.ABL_BillNumber;

		public ZString BillNumberWithHyphen => bill.BillNumberWithHyphen;

		public ZDateTime BillIssueDate => bill.ABL_BillIssueDate;

		public ZString Origin => bill.Header?.AMA_RL_NKPortOfLoading ?? ZString.Empty;

		public ZString FinalDestination => bill.ABL_RL_NKFinalDestination;

		public ZString UCRNumber => bill.ABL_UCRNumber;

		public ZString LocationOfGoods => bill.ABL_GoodsLocation;

		public ZString BillStatus => bill.ABL_BillStatus;

		public ZString BillStatusDescription => bill.ABL_BillStatusDescription;

		public ZString PlaceOfDispatch => bill.ABL_RL_NKOrigin;

		public IEnumerable<ICusCarParty> Parties
		{
			get
			{
				if (bill.CountryCode == Core.Constants.CountryCodes.SouthAfrica)
				{
					yield return new CusCarPartyFromAsycudaBillAddress(bill.ConsigneeABLAddress);
					yield return new CusCarPartyFromAsycudaBillAddress(bill.ShipperABLAddress);
					yield return new CusCarPartyFromAsycudaBillAddress(bill.NotifyPartyABLAddress);
					yield return new CusCarPartyFromAsycudaBillAddress(bill.ForwarderABLAddress);
				}
				else
				{
					if (bill.Consignee != null)
					{
						yield return new CusCarPartyFromAsycudaBillAddress(bill.ConsigneeABLAddress);
					}

					if (bill.Shipper != null)
					{
						yield return new CusCarPartyFromAsycudaBillAddress(bill.ShipperABLAddress);
					}

					if (bill.NotifyParty != null)
					{
						yield return new CusCarPartyFromAsycudaBillAddress(bill.NotifyPartyABLAddress);
					}

					if (bill.Forwarder != null)
					{
						yield return new CusCarPartyFromAsycudaBillAddress(bill.ForwarderABLAddress);
					}
				}
			}
		}

		public IEnumerable<ICusCarPackage> Packages
		{
			get
			{
				var asycudaPacks = bill.Packs.OfType<AsycudaPack>();
				var packages = asycudaPacks.Select(p => new CusCarPack(p)).ToArray();
				return packages.Any() ? packages : new ICusCarPackage[] { this };
			}
		}

		public ZString CargoReleaseStatus => bill.CargoReleaseStatus;

		public ZString CargoReleaseStatusDescription => bill.CargoReleaseStatusDescription;

		public IEnumerable<ICustomsNumber> CustomsNumbers => bill.CustomsEntryNumbers.Cast<ABLEntryNum>();

		public ZWeight BillWeight => new ZWeight(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ);

		public ZString ExternalReference => ZString.Empty;

		public ZString DepotOfUnpack => bill.Header?.DepotCode ?? ZString.Empty;

		public ZString TerminalOfDischarge => bill.Header?.TerminalCode ?? ZString.Empty;

		public ZString ShipmentType => bill.ABL_ShipmentType;

		#endregion

		#region ICusCarPackage

		ZInt ICusCarPackage.NumberOfPacks => bill.ABL_ManifestQty;

		ZString ICusCarPackage.PackUQ => bill.ABL_ManifestUQ;

		ZString ICusCarPackage.Description => bill.ABL_GoodsDescription;

		CargoStatusIndicator ICusCarPackage.CargoStatusIndicator => bill.CargoStatusIndicator;

		ZDecimal ICusCarPackage.GrossVolumeInM3 => new ZVolume(bill.ABL_Volume, bill.ABL_VolumeUQ).InCubicMetres;

		ZDecimal ICusCarPackage.GrossVolume => IsVolumeInLitre ? bill.ABL_Volume : ((ICusCarPackage)this).GrossVolumeInM3.Round(0);

		ZString ICusCarPackage.GrossVolumeUnitCode => IsVolumeInLitre ? Constants.VolumeUnitCode.Litre : Constants.VolumeUnitCode.CubicMetre;

		ZBool IsVolumeInLitre => bill.ABL_VolumeUQ == Core.Constants.Volume.Litre;

		ZDecimal ICusCarPackage.GrossMassInKilos => new ZWeight(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ).InKilogramsSafe;

		ZString ICusCarPackage.MarksAndNumbers => bill.ABL_MarksAndNumbers;

		ZString ICusCarPackage.UNDGClass => ZString.Empty;

		ZString ICusCarPackage.UNDGNumber => ZString.Empty;

		ZString ICusCarPackage.CommodityCode => ZString.Empty;

		ZString ICusCarPackage.ContainerNumber => ZString.Empty;

		ZString ICusCarPackage.VINNumber => ZString.Empty;

		#endregion

		#region IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider

		IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider EDIFACTMessageProvider => bill;

		EDIFACTMessageStatusCalculator IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider.GetCalculator(string country)
			=> EDIFACTMessageProvider.GetCalculator(country);

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get => EDIFACTMessageProvider.MessageStatus;
			set => EDIFACTMessageProvider.MessageStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get => EDIFACTMessageProvider.JobStatus;
			set => EDIFACTMessageProvider.JobStatus = value;
		}

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			EDIFACTMessageProvider.AddMessage(message);
		}

		ZString IEDIFACTMessageAttachee.JobIdentification => EDIFACTMessageProvider.JobIdentification;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => EDIFACTMessageProvider.TopLevelBusinessObject;

		bool IEDIFACTMessageAttachee.HasChanges => EDIFACTMessageProvider.HasChanges;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => bill.Factory;

		EDIMessageCollection IEDIMessageCollectionProvider.Messages => bill.Messages;

		#endregion

		#region IAgentCodeProviderForInterchanges

		IInterchangeSenderIdProvider AgentCodeProvider => bill;

		ZString IInterchangeSenderIdProvider.SenderID => AgentCodeProvider.SenderID;

		#endregion
	}
}

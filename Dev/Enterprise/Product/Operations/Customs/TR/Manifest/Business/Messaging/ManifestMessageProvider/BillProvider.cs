using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class BillProvider : IBillofLading
	{
		public BillProvider(AsycudaBill asycudaBill, ZDateTime effectiveDate)
		{
			this.bill = asycudaBill;
			this.effectiveDate = effectiveDate;
		}

		readonly AsycudaBill bill;
		readonly ZDateTime effectiveDate;

		ZBool IsManifestContainer => TRManifestMessageHelper.IsManifestContainer(bill);

		public ZString ContainerAgentName
		{
			get
			{
				var returnValue = ZString.Empty;
				var header = bill.ContainerAgent?.Header;
				if (header != null)
				{
					var containerAgentRegNo = header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey);
					if (containerAgentRegNo.IsEmpty)
					{
						returnValue = header.OH_FullName.SubstringSafe(0, 70);
					}
				}
				return returnValue;
			}
		}

		public ZString ContainerAgentRegNo => bill.ContainerAgent?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;
		public ZString ConsigneeName => bill.ABL_ConsigneeName.SubstringSafe(0, 70);
		public ZString ConsigneeRegNo
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (!bill.IsToOrder && !bill.NotOwned)
				{
					returnValue = (TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaBill.Schema.ABL_ConsigneeRegNo, bill.Header.AMA_ManifestType)) ? ZString.Empty : bill.ABL_ConsigneeRegNo;
				}
				else
				{
					if (bill.IsToOrder)
					{
						returnValue = (NoResString)"emre";
					}
					if (bill.NotOwned)
					{
						returnValue = (NoResString)"sahip değildir";
					}
				}
				return returnValue;
			}
		}
		public ZString IsWarehouseExternal => bill.ABL_SpecialCargoCode == Universal.CodeDescriptionPairLists.YesNoList.Codes.Yes ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString NotifyPartyName => bill.ABL_NotifyPartyName.SubstringSafe(0, 70);
		public ZString NotifyPartyRegNo => bill.ABL_NotifyPartyRegNo;
		public ZString Origin => bill.ABL_RL_NKOrigin.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(bill.Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, bill.ABL_RL_NKOrigin.SubstringSafe(0, 2), effectiveDate);
		public ZString SafetySecurityT => ZString.Empty;
		public ZString GoodsLocation => bill.ABL_LocationInformation;
		public ZString TransportValueCurrency => bill.ABL_RX_NKFreightValueCurrency;
		public ZDecimal TransportTotalValue => bill.ABL_FreightValue;
		public ZString ShipperName => bill.ABL_ShipperName;
		public ZString ShipperRegNo => bill.ABL_ShipperRegNo;
		public ZString IsGroup => bill.ABL_BolType == Core.Constants.ShipmentTypes.CoLoadMaster ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString Iscontainer => IsManifestContainer ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString NKFreightValueCurrency => TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency, bill.Header.AMA_ManifestType, bill.Header.AMA_TransportMode) ? ZString.Empty : bill.ABL_RX_NKTransportValueCurrency;
		public ZDecimal FreightValue => TRManifestMessageHelper.IsForbidden(ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue, bill.Header.AMA_ManifestType, bill.Header.AMA_TransportMode) ? ZDecimal.Zero : bill.ABL_TransportValue;
		public ZString PaymentType => bill.PaymentType;
		public ZString PreviousVoyageNo => ZString.Empty;
		public ZDateTime PreviousVoyageArrivalDate => ZDateTime.Empty;
		public ZString EntryNumber => (TRManifestMessageHelper.IsForbidden("Bill.CustomsEntryNumber", bill.Header.AMA_ManifestType)) ? ZString.Empty : bill.CustomsEntryNumber;
		public ZString IsRoro => bill.RoRo ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString SequenceNo => bill.ABL_SequenceNumber.ToString();
		public ZString IsTransshipmentType => bill.TransshipmentType.IsEmpty ? TurkishConstants.AnswerNo : TurkishConstants.AnswerYes;
		public ZString TransshipmentType
		{
			get
			{
				ZString returnValue = ZString.Empty;
				if (!bill.TransshipmentType.IsEmpty)
				{
					switch (bill.TransshipmentType)
					{
						case "1":
							returnValue = (NoResString)"Yurtiçi Aktarma";
							break;
						case "2":
							returnValue = (NoResString)"Yurtdışı Aktarma";
							break;
						case "3":
							returnValue = (NoResString)"Mahrece İade";
							break;
						case "4":
							returnValue = (NoResString)"Transit Ticaret";
							break;
					}
				}
				return returnValue;
			}
		}

		public ZString BillNumber => bill.ABL_BillNumber;
		public IEnumerable<ILadingLines> LadingLines
		{
			get
			{
				var packs = bill.Packs.Cast<AsycudaPack>();
				foreach (var pack in packs)
				{
					yield return new PackProvider(pack, IsManifestContainer);
				}
			}
		}
		public IEnumerable<ILadingExports> LadingExports
		{
			get
			{
				var result = new List<ILadingExports>();
				var exports = bill.RelatedDeclarationForExports;
				foreach (var export in exports)
				{
					result.Add(new LadingExportProvider(export));
				}
				return result;
			}
		}
		public IEnumerable<IBillVisitedCountry> BillVisitedCountry
		{
			get
			{
				var result = new List<IBillVisitedCountry>();
				var ports = bill.VisitedPorts;
				foreach (var port in ports)
				{
					result.Add(new VisitedCountryProvider(port, effectiveDate, bill.Header.AMA_TransportMode, bill.Header.AMA_ManifestType));
				}
				return result;
			}
		}
	}
}

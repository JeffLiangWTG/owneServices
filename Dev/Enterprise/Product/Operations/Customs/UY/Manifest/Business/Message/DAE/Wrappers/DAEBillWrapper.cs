using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.UY.Manifest.Business
{
	internal class DAEBillWrapper : IDaeBillOfLading
	{
		internal DAEBillWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		short IDaeBillOfLading.SequenceNumber => bill.ABL_SequenceNumber;

		string IDaeBillOfLading.BillNumber => bill.ABL_BillNumber;

		string IDaeBillOfLading.PortOfLoading => GetValidatePortCode((ZString)bill?.ABL_RL_NKOrigin);

		string IDaeBillOfLading.BolType => AsycudaBill.UYConstants.HouseBillCode;

		string IDaeBillOfLading.MasterBillNumber => bill.Header.AMA_MasterBill;

		string IDaeBillOfLading.PortOfDischarge => GetValidatePortCode((ZString)bill.Header?.AMA_RL_NKPortOfDischarge);

		string IDaeBillOfLading.ConsigneeDocumentType
		{
			get
			{
				var res = string.Empty;
				if (!bill.ABL_ConsigneeRegNoType.IsEmpty)
				{
					switch (bill.ABL_ConsigneeRegNoType)
					{
						case UruguayOrgCusCodeInfo.OrgCusCodes.CID:
							res = ConsigneeDocumentTypes.PersonalID;
							break;
						case UruguayOrgCusCodeInfo.OrgCusCodes.RUT:
							res = ConsigneeDocumentTypes.TaxID;
							break;
						default:
							res = ConsigneeDocumentTypes.Passport;
							break;
					}
				}
				return res;
			}
		}

		string IDaeBillOfLading.ConsigneeDocumentNo => bill.ABL_ConsigneeRegNo;

		string IDaeBillOfLading.ConsigneeName => bill.ABL_ConsigneeName;

		DateTime IDaeBillOfLading.MessageDate => Convert.ToDateTime(Env.Time.CurrentLocalDateTime.ToShortDateString());

		string IDaeBillOfLading.ConsigneeAddress => ((ZString)(bill.ABL_ConsigneeStreet1 + bill.ABL_ConsigneeCity)).SubstringSafe(0, 60);

		string IDaeBillOfLading.FinalDestination => GetValidatePortCode((ZString)bill?.ABL_RL_NKFinalDestination);

		string IDaeBillOfLading.ShipperName => bill.ABL_ShipperName;

		decimal IDaeBillOfLading.BOL_Volume => decimal.Parse(Core.Constants.Volume.ConvertSafe(bill.ABL_Volume, bill.ABL_VolumeUQ, Core.Constants.Volume.CubicMetres).ToString("0.000"));

		string IDaeBillOfLading.NotifyName => bill.ABL_NotifyPartyName;

		string IDaeBillOfLading.NotifyAddress => bill.ABL_NotifyPartyStreet1 + bill.ABL_NotifyPartyCity + bill.ABL_NotifyPartyPostcode;

		string IDaeBillOfLading.NotifyPhone => bill.ABL_NotifyPartyPhone;

		string IDaeBillOfLading.OnDemand => Core.Constants.BooleanFalseString;

		string IDaeBillOfLading.PrepaidCollect => bill.ABL_PrepaidCollect == Core.Constants.PaymentType.Prepaid ? Core.Constants.AWB.PPDCollect.Prepaid : Core.Constants.AWB.PPDCollect.Collect;

		string IDaeBillOfLading.BOL_AgentMail => GlbStaff.CurrentUser.GS_EmailAddress;

		string IDaeBillOfLading.Transfer => bill.ABL_Transshipment ? AsycudaBill.UYConstants.BooleanTrueString : Core.Constants.BooleanFalseString;

		string IDaeBillOfLading.Category => AsycudaBill.UYConstants.FreightForwarderCategory;

		string IDaeBillOfLading.NKOrigin => GetValidatePortCode((ZString)bill?.ABL_RL_NKOrigin);

		string IDaeBillOfLading.ConsigneeCountry
		{
			get
			{
				var query = new ZQuery(RefCusMapSchema.ZZM_CW1orCommercialValue, bill.ABL_RN_NKConsigneeCountry);
				query.AddToFilter(RefCusMapSchema.ZZM_ZZP_NKMapType, AsycudaBill.UYConstants.CountryMapType);
				query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Uruguay);
				var cusMap = bill.Factory.LoadTop1<RefCusMap>(query);

				return cusMap?.ZZM_CustomsValue ?? string.Empty;
			}
		}

		IReadOnlyCollection<IDaeBillOfLadingLine> IDaeBillOfLading.Lines
		{
			get
			{
				var result = new List<IDaeBillOfLadingLine>();
				foreach (AsycudaPack pack in bill.Packs)
				{
					result.Add(new DAEPackWrapper(pack));
				}
				return result;
			}
		}

		short IDaeBillOfLading.CancellationSequenceNumber => ZShort.ParseSafe(bill.CustomsEntryNumber, ZShort.Zero);

		bool IDaeBillOfLading.IsANewBill => ZBool.False;

		bool IDaeBillOfLading.ConsigneeChange => ZBool.False;

		bool IDaeBillOfLading.BillChange => ZBool.False;

		string GetValidatePortCode(ZString portCode) => string.Concat(portCode.SubstringSafe(0, 2), " ", portCode.SubstringSafe(2, 3));
	}

	internal class DAEAmendBillWrapper : DAEBillWrapper, IDaeBillOfLading
	{
		internal DAEAmendBillWrapper(AsycudaBill bill, ZBool headerChange, DaeMessageBuilderLastSent manifestSent) : base(bill)
		{
			this.bill = bill;
			this.headerChange = headerChange;
			this.manifestSent = manifestSent;
		}
		readonly AsycudaBill bill;
		IDaeBillOfLading idaeBill => this;
		readonly ZBool headerChange;
		readonly DaeMessageBuilderLastSent manifestSent;
		IBillSent billSent => manifestSent.BillInfo.FirstOrDefault();

		bool IDaeBillOfLading.IsANewBill => bill.ABL_BillStatus != CustomsStatusList.Codes.ACP;

		bool IDaeBillOfLading.ConsigneeChange => !idaeBill.IsANewBill && billSent != null && billSent.ConsigneeRegNo != bill.ABL_ConsigneeRegNo;

		IReadOnlyCollection<IDaeBillOfLadingLine> IDaeBillOfLading.Lines
		{
			get
			{
				var result = new List<IDaeBillOfLadingLine>();
				foreach (AsycudaPack pack in bill.Packs)
				{
					result.Add(new DAEAmendPackWrapper(pack, idaeBill.IsANewBill, billSent));
				}
				return result;
			}
		}

		bool IDaeBillOfLading.BillChange
		{
			get
			{
				return !idaeBill.IsANewBill && (headerChange || billSent != null && (billSent.PortOfLoading != idaeBill.PortOfLoading
					|| billSent.PortOfDischarge != idaeBill.PortOfDischarge
					|| billSent.FinalDestination != idaeBill.FinalDestination
					|| billSent.ShipperName != idaeBill.ShipperName
					|| billSent.BOL_Volume != idaeBill.BOL_Volume
					|| billSent.NotifyName != idaeBill.NotifyName
					|| billSent.NotifyAddress != idaeBill.NotifyAddress
					|| billSent.NotifyPhone != idaeBill.NotifyPhone
					|| billSent.PrepaidCollect != idaeBill.PrepaidCollect
					|| billSent.BOL_AgentMail != idaeBill.BOL_AgentMail
					|| billSent.Transfer != idaeBill.Transfer
					|| billSent.NKOrigin != idaeBill.NKOrigin
					|| billSent.ConsigneeCountry != idaeBill.ConsigneeCountry));
			}
		}
	}
}

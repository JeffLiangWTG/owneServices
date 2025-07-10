using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using MessageStatusCodeList = Enterprise.Customs.ASYCUDA.Business.MessageStatusCodeList;

namespace Enterprise.Customs.SG.Access.Business
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();

		protected override ZString GetMostSevereValueMessageStatusCore(ASYCUDA.Business.AsycudaManifestHeader baseManifestHeader)
		{
			var manifestHeader = (AsycudaManifestHeader)baseManifestHeader;
			var allBills = GetAllBills(manifestHeader);
			var allPackedItems = GetAllPackedItems(manifestHeader);
			var list = GetManifestHeaderMessageStatusSeverityList(manifestHeader.Factory);
			var result = list.FirstOrDefault(
				severityItem =>
					allPackedItems.Any(packedCountry => packedCountry.API_MessageStatus == severityItem)
					|| allBills.Any(bill => bill.ABL_MessageStatus == severityItem)
			);

			return result;
		}

		protected override ZString GetMostSevereValueCustomsStatusCore(ASYCUDA.Business.AsycudaManifestHeader baseManifestHeader)
		{
			var manifestHeader = (AsycudaManifestHeader)baseManifestHeader;
			var allBills = GetAllBills(manifestHeader);
			var allPackedItems = GetAllPackedItems(manifestHeader);
			var list = GetManifestHeaderCustomsStatusSeverityList(manifestHeader.Factory);
			var result = list.FirstOrDefault(
				severityItem =>
					allPackedItems.Any(packedItem => packedItem.API_PackStatus == severityItem)
					|| allBills.Any(bill => bill.ABL_BillStatus == severityItem)
			);

			return result;
		}

		static IEnumerable<AsycudaBill> GetAllBills(AsycudaManifestHeader manifestHeader)
		{
			return manifestHeader.Bills
				.Cast<AsycudaBill>();
		}

		static IEnumerable<AsycudaPackedItem> GetAllPackedItems(AsycudaManifestHeader manifestHeader)
		{
			return manifestHeader.Bills
				.Cast<AsycudaBill>()
				.SelectMany(bill => bill.Packs)
				.Cast<AsycudaPack>()
				.Select(pack => pack.PackedItem);
		}

		static IEnumerable<ZString> GetManifestHeaderMessageStatusSeverityList(BusinessObjectFactory factory) => factory.GetCachedValue("SGManifestCountryMessageStatusSeverityList", () => new ZString[]
		{
			MessageStatusCodeList.Codes.Error,
			MessageStatusCodeList.Codes.Unknown,
			MessageStatusCodeList.Codes.NotSent,
			MessageStatusCodeList.Codes.Sent,
			MessageStatusCodeList.Codes.Awaiting,
			MessageStatusCodeList.Codes.Updated,
			MessageStatusCodeList.Codes.Registered,
			MessageStatusCodeList.Codes.Accepted
		});

		static IEnumerable<ZString> GetManifestHeaderCustomsStatusSeverityList(BusinessObjectFactory factory) => factory.GetCachedValue("SGManifestCountryCustomsStatusSeverityList", () => new ZString[]
		{
			ZString.Empty,
			Common.SG.GlobalManifestStatusList.Codes.InspectionRequired,
			Common.SG.GlobalManifestStatusList.Codes.Cancelled,
			Common.SG.GlobalManifestStatusList.Codes.Clear
		});

		#region Calculate CustomsStatus and MessageStatus for AsycudaBill

		protected override ZString GetMostSevereValueCustomsStatusCore(ASYCUDA.Business.AsycudaBill baseBill)
		{
			var bill = (AsycudaBill)baseBill;
			var allPackedItems = bill.Packs.Cast<AsycudaPack>().Select(pack => pack.PackedItem);
			var list = GetBillCustomsStatusSeverityList(bill.Factory);
			var result = list.FirstOrDefault(severityItem => allPackedItems.Any(packedItem => packedItem.API_PackStatus == severityItem));

			if (result == Constants.CustomsStatusCode.Cancelled)
			{
				result = Common.SG.GlobalManifestStatusList.Codes.Cancelled;
			}

			return result;
		}

		static IEnumerable<ZString> GetBillCustomsStatusSeverityList(BusinessObjectFactory factory) => factory.GetCachedValue("SGBillCountryCustomsStatusSeverityList", () => new ZString[]
		{
			ZString.Empty,
			Common.SG.GlobalManifestStatusList.Codes.InspectionRequired,
			Constants.CustomsStatusCode.Cancelled,
			Common.SG.GlobalManifestStatusList.Codes.Clear
		});

		protected override ZString GetMostSevereValueMessageStatusCore(ASYCUDA.Business.AsycudaBill baseBill)
		{
			var bill = (AsycudaBill)baseBill;
			var allPackedItems = bill.Packs.Cast<AsycudaPack>().Select(pack => pack.PackedItem);
			return GetManifestHeaderMessageStatusSeverityList(bill.Factory).FirstOrDefault(severityItem => allPackedItems.Any(packedItem => packedItem.API_MessageStatus == severityItem));
		}

		#endregion

		protected override bool ShouldUpdateBillCustomsStatusCore(ZString actionPurpose)
		{
			return actionPurpose == ASYCUDA.Business.AsycudaEventMessageConstants.ActionPurpose.ACK;
		}

		protected override bool ShouldClearCustomStatusCore(ZString customsStatus) => customsStatus == Constants.CustomsStatusCode.Cancelled;

		public override Type GetAsycudaEDIMessageType() => typeof(AsycudaEDIMessage);
	}
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	public sealed class SGAsycudaManifestHeaderDataObjectWriterHelper : ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectWriterHelper
	{
		public SGAsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
			: base(header)
		{
		}

		protected override IEnumerable<KeyValuePair<ZString, ZString>> GetBillCountryAdditionalAddInfosCore<TBill>(TBill bill)
		{
			if (bill is AsycudaBill sgBill)
			{
				var payeeIndicator = sgBill.SG_PayeeIndicator;
				if (!payeeIndicator.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(AddInfoConstants.BillCountry.PayeeIndicator, payeeIndicator);
				}
				var partyStatus = sgBill.SG_PartyStatus;
				if (!partyStatus.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(AddInfoConstants.BillCountry.PartyStatus, partyStatus);
				}
				var partyID = sgBill.SG_PartyID;
				if (!partyID.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(AddInfoConstants.BillCountry.PartyIndicator, partyID);
				}
				var gstReferenceNo = sgBill.GSTNReferenceNo;
				if (!gstReferenceNo.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(AddInfoConstants.BillCountry.GSTNReferenceNo, gstReferenceNo);
				}
			}
		}

		protected override IEnumerable<KeyValuePair<ZString, ZString>> GetPackedItemAdditionalAddInfosCore<TPackedItem>(TPackedItem packedItem)
		{
			var sgPackedItem = packedItem as AsycudaPackedItem;
			if (sgPackedItem != null)
			{
				var goodsType = sgPackedItem.GoodsType;
				if (!goodsType.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(AddInfoConstants.PackedItem.GoodsType, goodsType);
				}
				var gstPaid = sgPackedItem.GSTPaid;
				if (!gstPaid.IsEmpty)
				{
					yield return new KeyValuePair<ZString, ZString>(AddInfoConstants.PackedItem.GSTPaymentIndicator, gstPaid);
				}
			}
		}

		protected override IEnumerable<KeyValuePair<ZString, ZString>> GetBillCountryEntryInstructionAdditionalAddInfosCore<TCountry>(TCountry billCountry)
		{
			if (billCountry is AsycudaBill sgBill)
			{
				if (sgBill.Header.IsImport)
				{
					yield return new KeyValuePair<ZString, ZString>(AsycudaBill.Schema.CycleDate, sgBill.CycleDate.ToISO8601String());
					yield return new KeyValuePair<ZString, ZString>(AsycudaBill.Schema.CycleNumber, sgBill.CycleNumber);
				}
				else
				{
					yield return new KeyValuePair<ZString, ZString>(AsycudaBill.Schema.BatchDate, sgBill.BatchDate.ToISO8601String());
					yield return new KeyValuePair<ZString, ZString>(AsycudaBill.Schema.BatchNumber, sgBill.BatchNumber);
				}
			}
		}

		protected override IEnumerable<GenAddOnDetail> GetAsycudaPackedItemGenAddOnColumnListCore(ASYCUDA.Business.AsycudaPackedItem basePackedItem)
		{
			var packedItem = (AsycudaPackedItem)basePackedItem;
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(packedItem.GoodsType), AddInfoKey = AddInfoConstants.PackedItem.GoodsType, GenAddOnColumnName = AsycudaPackedItem.Schema.GoodsType, PropertyName = AsycudaPackedItem.Schema.GoodsType };
			foreach (var detail in base.GetAsycudaPackedItemGenAddOnColumnListCore(packedItem))
			{
				yield return detail;
			}
		}

		protected override IDictionary<ZString, ZString> GetPackedItemEntryNumberMappingCore()
		{
			var result = new Dictionary<ZString, ZString>();
			result.Add(AddInfoConstants.PackedItem.PermitNumber, ASYCUDA.Business.Constants.CustomsEntryType.TradeNetPermit);
			result.Add(AddInfoConstants.PackedItem.EntryNumber, ASYCUDA.Business.Constants.CustomsEntryType.ACCESSPermit);
			return result;
		}

		protected override IEnumerable<GenAddOnDetail> GetAsycudaBillGenAddOnColumnListCore(ASYCUDA.Business.AsycudaBill baseBillBO)
		{
			var countryBO = (AsycudaBill)baseBillBO;
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.SG_PayeeIndicator), AddInfoKey = AddInfoConstants.BillCountry.PayeeIndicator, GenAddOnColumnName = AsycudaBill.Schema.SG_PayeeIndicator, PropertyName = AsycudaBill.Schema.SG_PayeeIndicator };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.SG_PartyStatus), AddInfoKey = AddInfoConstants.BillCountry.PartyStatus, GenAddOnColumnName = AsycudaBill.Schema.SG_PartyStatus, PropertyName = AsycudaBill.Schema.SG_PartyStatus };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.SG_PartyID), AddInfoKey = AddInfoConstants.BillCountry.PartyIndicator, GenAddOnColumnName = AsycudaBill.Schema.SG_PartyID, PropertyName = AsycudaBill.Schema.SG_PartyID };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.CycleDate), AddInfoKey = AddInfoConstants.BillCountry.CycleDate, GenAddOnColumnName = AsycudaBill.Schema.CycleDate, PropertyName = AsycudaBill.Schema.CycleDate };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(countryBO.CycleNumber), AddInfoKey = AddInfoConstants.BillCountry.CycleNumber, GenAddOnColumnName = AsycudaBill.Schema.CycleNumber, PropertyName = AsycudaBill.Schema.CycleNumber };

			foreach (var detail in base.GetAsycudaBillGenAddOnColumnListCore(countryBO))
			{
				yield return detail;
			}
		}
	}
}

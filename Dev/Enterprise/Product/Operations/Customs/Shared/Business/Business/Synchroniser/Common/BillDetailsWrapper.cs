using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class BillDetailsWrapper
	{
		public BillDetailsWrapper(IBillDetails bill, string billType, bool isPrimary)
		{
			this.Bill = bill;
			this.billType = billType;
			this.isPrimary = isPrimary;
		}

		public readonly IBillDetails Bill;
		readonly string billType;
		readonly bool isPrimary;

		public ZString BillNumber
		{
			get
			{
				var billNumberInfo = Bill.BillNumberInfo;
				return billNumberInfo != null ? (ZString)Bill.BillNumberInfo.Value : ZString.Empty;
			}
		}

		public ZString AMSBillNumber
		{
			get
			{
				var amsBillNumberInfo = Bill.AMSBillNumberInfo;
				return amsBillNumberInfo != null ? (ZString)Bill.AMSBillNumberInfo.Value : ZString.Empty;
			}
		}

		public ZString BillType
		{
			get { return billType; }
		}

		public IEnumerable<OrgHeader> SCACIssuers
		{
			get { return Bill.SCACIssuers; }
		}

		public ZInt GetNumberOfPacks(ForwardingShipment packedShipment)
		{
			var result = 0;
			foreach (var numberOfPackesInfo in Bill.GetNumberOfPackesInfos(packedShipment))
			{
				result += (ZInt)numberOfPackesInfo.Value;
			}
			return result;
		}

		public ZPropertyInfo[] GetNumberOfPacksSourceInfos(ForwardingShipment packedShipment)
		{
			return Bill.GetNumberOfPackesInfos(packedShipment);
		}

		public ZString GetTypeOfPack(ForwardingShipment packedShipment)
		{
			var typeOfPackInfos = Bill.GetTypeOfPackesInfos(packedShipment);
			return typeOfPackInfos.Length > 0 ? (ZString)typeOfPackInfos[0].Value : ZString.Empty;
		}

		public ZPropertyInfo[] GetTypeOfPacksSourceInfos(ForwardingShipment packedShipment)
		{
			return Bill.GetTypeOfPackesInfos(packedShipment);
		}

		public bool IsPrimary
		{
			get { return isPrimary; }
		}

		public List<KeyValuePair<ZString, ZInt>> ReleaseNumbers
		{
			get
			{
				if (releaseNumbers == null)
				{
					releaseNumbers = new List<KeyValuePair<ZString, ZInt>>();
				}
				return releaseNumbers;
			}
		}
		List<KeyValuePair<ZString, ZInt>> releaseNumbers;

		public Func<Bill> GetParentBill
		{
			get;
			set;
		}
	}
}

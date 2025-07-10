using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFBillRow : AutoISFBillRow
	{
		public ISFBillRow(ISFHeaderRow headerRow)
			: base(headerRow.Factory)
		{
			this.HeaderRow = headerRow;
			SetDefaultData();
		}

		public new class Schema : AutoISFBillRow.Schema
		{
			public const string HouseBillPK = "HouseBillPK";
			public const string OceanBillPK = "OceanBillPK";
		}

		public readonly ISFHeaderRow HeaderRow;

		[List(nameof(Bills))]
		public override ZGuid BillPK
		{
			get { return base.BillPK; }
			set { base.BillPK = value; }
		}

		public CusISFBill Bill
		{
			get { return Factory.Load<CusISFBill>(BillPK); }
		}

		public ActiveBusinessObjectCollection<CusISFBill> Bills
		{
			get
			{
				if (HeaderRow.IsOceanBillData)
				{
					return OceanBills;
				}
				else
				{
					return HouseBills;
				}
			}
		}

		[List(nameof(HouseBills))]
		public ZGuid HouseBillPK
		{
			get { return base.BillPK; }
			set { base.BillPK = value; }
		}

		public ZPropertyInfo HouseBillPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.HouseBillPK, x => BillPKInfo); }
		}

		public ActiveBusinessObjectCollection<CusISFBill> HouseBills
		{
			get
			{
				ZQuery query = new ZQuery(CusISFBillSchema.BB_BillType, BillTypeList.Codes.HouseBillOfLading);
				query.IsNoResultQuery = HeaderRow.IsOceanBillData;
				return new ActiveBusinessObjectCollection<CusISFBill>(HeaderRow.Header, query);
			}
		}

		public CusISFBill HouseBill
		{
			get
			{
				CusISFBill bill = Bill;
				return bill == null || !bill.IsHouseBillOfLading ? null : bill;
			}
		}

		[List(nameof(OceanBills))]
		[ReadOnly(true)]
		public ZGuid OceanBillPK
		{
			get { return base.BillPK; }
			set { base.BillPK = value; }
		}

		public ZPropertyInfo OceanBillPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OceanBillPK, x => BillPKInfo); }
		}

		public ActiveBusinessObjectCollection<CusISFBill> OceanBills
		{
			get
			{
				ZQuery query = new ZQuery(CusISFBillSchema.PK, HeaderRow.MasterBillPK);
				query.AddToFilter(CusISFBillSchema.BB_BillType, BillTypeList.Codes.OceanBillOfLading);
				return new ActiveBusinessObjectCollection<CusISFBill>(HeaderRow.Header, query);
			}
		}

		public CusISFBill OceanBill
		{
			get
			{
				CusISFBill bill = Bill;
				return bill == null || !bill.IsOceanBillOfLading ? null : bill;
			}
		}

		[List(nameof(BuyingParties))]
		public override ZGuid BuyingPartyPK
		{
			get { return base.BuyingPartyPK; }
			set { base.BuyingPartyPK = value; }
		}

		public ActiveBusinessObjectCollection<ISFDocAddress> BuyingParties
		{
			get
			{
				ZQuery query = new ZQuery(JobDocAddressSchema.E2_ParentID, HeaderRow.Header.PK);
				query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
				query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BuyingParty);
				return new ActiveBusinessObjectCollection<ISFDocAddress>(Factory, query);
			}
		}

		[List(nameof(SellingParties))]
		public override ZGuid SellingPartyPK
		{
			get { return base.SellingPartyPK; }
			set { base.SellingPartyPK = value; }
		}

		public ActiveBusinessObjectCollection<ISFDocAddress> SellingParties
		{
			get
			{
				ZQuery query = new ZQuery(JobDocAddressSchema.E2_ParentID, HeaderRow.Header.PK);
				query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
				query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.SellingParty);
				return new ActiveBusinessObjectCollection<ISFDocAddress>(Factory, query);
			}
		}

		public ISFLineRowCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new ISFLineRowCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		ISFLineRowCollection lines;

		#region Implementation

		void SetDefaultData()
		{
			var existingBills = Bills;
			if (existingBills.Count == 1)
			{
				BillPK = existingBills[0].PK;
			}
			if (SellingParties.Count > 0)
			{
				SellingPartyPK = HeaderRow.Header.SellingParty.PK;
			}
			if (BuyingParties.Count > 0)
			{
				BuyingPartyPK = HeaderRow.Header.BuyingParty.PK;
			}
		}

		#endregion
	}
}

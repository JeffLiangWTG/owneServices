using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFHeaderRow : AutoISFHeaderRow
	{
		public ISFHeaderRow(CusISFHeader header)
			: base(header.Factory)
		{
			this.Header = header;
			SetDefaultData();
		}

		public readonly CusISFHeader Header;

		[List(nameof(Consols))]
		public override ZGuid ConsolPK
		{
			get { return base.ConsolPK; }
			set { base.ConsolPK = value; }
		}

		public ForwardingConsolCollection Consols
		{
			get { return new ForwardingConsolCollection(Factory); }
		}

		public ForwardingConsol Consol
		{
			get { return Factory.Load<ForwardingConsol>(ConsolPK); }
		}

		[List(nameof(MasterBills))]
		public override ZGuid MasterBillPK
		{
			get { return base.MasterBillPK; }
			set
			{
				base.MasterBillPK = value;
				if (!IsCopying)
				{
					DefaultConsolDetail();
					ReCalculateBillData();
				}
			}
		}

		public ActiveBusinessObjectCollection<CusISFBill> MasterBills
		{
			get { return masterBills ?? (masterBills = new ActiveBusinessObjectCollection<CusISFBill>(Header, new ZQuery(CusISFBillSchema.BB_BillType, new ZString[] { BillTypeList.Codes.MasterBillOfLading, BillTypeList.Codes.OceanBillOfLading }))); }
		}
		ActiveBusinessObjectCollection<CusISFBill> masterBills;

		public CusISFBill MasterBill
		{
			get
			{
				CusISFBill bill = Factory.Load<CusISFBill>(MasterBillPK);
				return bill == null || (!bill.IsOceanBillOfLading && !bill.IsMasterBillOfLading) ? null : bill;
			}
		}

		public bool IsOceanBillData
		{
			get
			{
				CusISFBill masterBill = MasterBill;
				return masterBill != null && masterBill.IsOceanBillOfLading;
			}
		}

		public ISFBillRowCollection Bills
		{
			get
			{
				if (bills == null)
				{
					bills = new ISFBillRowCollection(this);
					RegisterEditableChildObject(bills);
					bills.CountChanged += new CollectionCountChangedEventHandler(bills_CountChanged);
				}
				return bills;
			}
		}
		ISFBillRowCollection bills;

		public ISFContainerRowCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new ISFContainerRowCollection(this);
					RegisterEditableChildObject(containers);
				}
				return containers;
			}
		}
		ISFContainerRowCollection containers;

		#region Implementation

		void bills_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (Bills.Count == 0)
			{
				Bills.AddNew();
			}
		}

		void DefaultConsolDetail()
		{
			ForwardingConsol[] consols = null;
			CusISFBill masterBill = MasterBill;
			if (masterBill != null)
			{
				consols = Factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, masterBill.BB_BillNum));
			}
			ConsolPK = consols == null || consols.Length != 1 ? ZGuid.Empty : consols[0].PK;
		}

		void SetDefaultData()
		{
			var existingMasterBills = MasterBills;
			MasterBillPK = (existingMasterBills.Count == 1) ? existingMasterBills[0].PK : ZGuid.Empty;
		}

		void ReCalculateBillData()
		{
			CusISFBill masterBill = MasterBill;
			if (masterBill != null && masterBill.IsOceanBillOfLading)
			{
				ISFBillRow billRow = null;
				if (Bills.Count > 0)
				{
					billRow = Bills[masterBill.PK];
					if (billRow == null)
					{
						billRow = Bills[0];
					}
				}

				if (billRow == null)
				{
					billRow = Bills.AddNew();
				}
				else
				{
					foreach (ISFBillRow billToDelete in Bills.ToArray<ISFBillRow>())
					{
						if (billToDelete != billRow)
						{
							Bills.RemoveAndDelete(billToDelete);
						}
					}
				}
				billRow.BillPK = masterBill.PK;
			}
			else
			{
				List<ISFBillRow> existingBills = new List<ISFBillRow>(Bills.ToArray<ISFBillRow>());
				if (!existingBills.Any(x => x.OceanBill == null))
				{
					LoadDefaultHouseBillData(existingBills);
				}
				existingBills.ForEach((ISFBillRow x) =>
				{
					if (x.OceanBill != null)
					{
						Bills.RemoveAndDelete(x);
					}
				});
			}
		}

		void LoadDefaultHouseBillData(List<ISFBillRow> existingBills)
		{
			foreach (CusISFBill bill in Header.ReferenceDatas)
			{
				if (bill.IsHouseBillOfLading)
				{
					var billRow = existingBills.Find(x => x.OceanBill != null) ?? Bills.AddNew();
					billRow.BillPK = bill.PK;
				}
			}
		}

		#endregion
	}
}

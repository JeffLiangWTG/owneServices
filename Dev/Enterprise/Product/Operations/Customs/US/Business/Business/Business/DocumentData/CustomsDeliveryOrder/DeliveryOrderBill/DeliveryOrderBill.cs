using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderBill : Customs.Business.CusCodeData
	{
		public DeliveryOrderBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CY_CodeInfo.HumanReadableName = "Bill Type";
			CY_DataInfo.HumanReadableName = "Bill Number";
		}

		#region Properties

		#region CY_Data

		[List(nameof(Lookups) + "." + nameof(DeliveryOrderBillLookups.BillList))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set
			{
				ZString oldValue = CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != CY_Data)
				{
					UpdateBillDetails(CY_Data);
				}
			}
		}

		#endregion

		#endregion

		public Bill Bill
		{
			get
			{
				DeliveryOrderHeader parent = Parent;
				JobDeclaration declaration = parent != null ? parent.Declaration : null;
				return declaration != null ? declaration.Bills.FindByBillNumberAndType(CY_Data, CY_Code) : null;
			}
		}

		public override void OnSaving()
		{
			if (CY_Code.IsEmpty && CY_Data.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public new DeliveryOrderBillValidation Validation
		{
			get { return (DeliveryOrderBillValidation)base.Validation; }
		}

		public new DeliveryOrderHeader Parent
		{
			get { return (DeliveryOrderHeader)base.Parent; }
		}

		public new DeliveryOrderBillLookups Lookups
		{
			get { return (DeliveryOrderBillLookups)base.Lookups; }
		}

		#region Implementation

		void UpdateBillDetails(ZString billNumber)
		{
			if (!billNumber.IsEmpty)
			{
				DeliveryOrderHeader parent = Parent;
				JobDeclaration declaration = parent != null ? parent.Declaration : null;
				if (declaration != null)
				{
					foreach (Bill bill in declaration.Bills)
					{
						if ((bill.US_UI_NKBillIssuerSCAC + bill.CU_BillNum == billNumber) || bill.CU_BillNum == billNumber)
						{
							CY_Code = bill.CU_BillType;
							if (declaration.ContainersRequired)
							{
								parent.DeliveryOrderContainers.CopyContainersIfMissing(bill.Containers.ToArray<CusContainer>());
							}
							break;
						}
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = DeliveryOrderHeader.Constants.CusCodeDataBill;
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new DeliveryOrderBillValidation(this);
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new DeliveryOrderBillLookups(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(DeliveryOrderHeader)); }
		}

		#endregion

		public ZBool ShowDeliveryOrderBillInReport => !(Parent.DeliveryOrderBills.MasterBills.Count == 1 && PK == Parent.DeliveryOrderBills.MasterBills[0].PK) &&
			!(Parent.DeliveryOrderBills.HouseBills.Count == 1 && PK == Parent.DeliveryOrderBills.HouseBills[0].PK);
	}
}

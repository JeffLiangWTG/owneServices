using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class MasterBillWrapper : IMasterBillTransportDocument
	{
		public MasterBillWrapper(Bill bill)
		{
			this.bill = bill;
		}

		readonly Bill bill;

		IEnumerable<ZGuid> IMasterBillTransportDocument.ChildBills => from Bill childBill in (BusinessObjectCollection)bill.ChildBills select childBill.PK;

		ZString IAssociatedTransportDocument.BillNumber => bill.CU_BillNum;

		ZString IAssociatedTransportDocument.BillType
		{
			get
			{
				var result = bill.CU_BillType;
				switch (bill.CU_BillType)
				{
					case Customs.Business.BillTypeList.Codes.MasterBill:
						result = Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.MB;
						break;
				}

				return result;
			}
		}

		IEnumerable<ZGuid> IAssociatedTransportDocument.RelatedEquipment
		{
			get
			{
				foreach (PackingGroup packGroup in bill.PackingGroups)
				{
					if (packGroup.Container != null)
					{
						yield return packGroup.PK;
					}
				}
			}
		}

		IEnumerable<ZGuid> IAssociatedTransportDocument.RelatedPackages
		{
			get
			{
				foreach (PackingGroup packGroup in bill.PackingGroups)
				{
					if (packGroup.Container == null)
					{
						foreach (Package package in packGroup.Packages)
						{
							if (package.Bill != null)
							{
								yield return package.PK;
							}
						}
					}
				}
			}
		}

		ZInt IAssociatedTransportDocument.MessageSequence
		{
			get { return fMessageSequence; }
			set { fMessageSequence = value; }
		}
		ZInt fMessageSequence;

		ZGuid IAssociatedTransportDocument.PK
		{
			get { return bill.PK; }
		}
	}

	public class BillNumberWrapper : IAssociatedTransportDocument
	{
		public BillNumberWrapper(Bill bill)
		{
			this.bill = Argument.NotNull(bill, "Bill details cannot be null");
		}

		readonly Bill bill;

		public ZString BillNumber
		{
			get { return bill.CU_BillNum; }
		}

		public ZString BillType
		{
			///	BM = Bill of Lading
			///	MB = Master Bill
			///	HWB = House Way Bill
			///	ABU = Parcel Number
			get
			{
				var result = bill.CU_BillType;
				switch (bill.CU_BillType)
				{
					case Customs.Business.BillTypeList.Codes.MasterBill:
						result = Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.MB;
						break;
					case Customs.Business.BillTypeList.Codes.HouseBill:
						result = bill.Declaration.IsPost ? Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.ABU : bill.Declaration.IsSea ? Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.BM : Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.HWB;
						break;
				}

				return result;
			}
		}

		public IEnumerable<ZGuid> RelatedPackages
		{
			get
			{
				foreach (PackingGroup packingGroup in bill.PackingGroups)
				{
					foreach (Package package in packingGroup.Packages)
					{
						if (packingGroup.Container == null)
						{
							if (package.Bill != null)
							{
								yield return package.PK;
							}
						}
					}
				}
			}
		}

		public IEnumerable<ZGuid> RelatedEquipment
		{
			get
			{
				foreach (PackingGroup packingGroup in bill.PackingGroups)
				{
					if (packingGroup.Container != null)
					{
						yield return packingGroup.PK;
					}
				}
			}
		}

		public ZInt MessageSequence
		{
			get { return fMessageSequence; }
			set { fMessageSequence = value; }
		}
		ZInt fMessageSequence;

		public ZGuid PK
		{
			get { return bill.PK; }
		}
	}

	#region Packaging

	internal class Packaging : IPackaging
	{
		public Packaging(Package package, ZString billNo)
		{
			this.package = package;
			this.billNo = billNo;
		}

		readonly Package package;
		readonly ZString billNo;

		public ZString ShippingMarks
		{
			get { return ZString.Empty; }
		}

		public ZInt NumberOfPackages
		{
			get { return package.CW_PackQty; }
		}

		public ZString PackageType
		{
			get { return package.CW_PackType; }
		}

		public ZString PackingMaterialDesc
		{
			get { return string.Empty; }
		}

		public ZDecimal PackageVolumeInMTQ
		{
			get { return ZDecimal.Zero; }
		}

		public ZInt MessageSequence
		{
			get { return fMessageSequence; }
			set { fMessageSequence = value; }
		}
		ZInt fMessageSequence;

		public ZString RelatedHB
		{
			get { return billNo; }
		}

		public ZString RelatedContainer
		{
			get { return package.CW_ContainerNoOrEquipmentNo; }
		}

		public ZGuid PK
		{
			get { return package.PK; }
		}
	}

	#endregion
}

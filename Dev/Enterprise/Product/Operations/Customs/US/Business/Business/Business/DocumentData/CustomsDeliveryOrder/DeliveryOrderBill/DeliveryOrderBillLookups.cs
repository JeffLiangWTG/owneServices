using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderBillLookups : Customs.Business.CusCodeDataLookups
	{
		public DeliveryOrderBillLookups(DeliveryOrderBill parent)
			: base(parent)
		{
		}

		protected new DeliveryOrderBill Parent
		{
			get { return (DeliveryOrderBill)base.Parent; }
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get { return new Customs.Business.BillTypeList(); }
		}

		public CodeDescriptionPairList BillList
		{
			get
			{
				if (billListCached == null)
				{
					billListCached = new CachedProperty<CodeDescriptionPairList>(Factory, delegate
						{
							CodeDescriptionPairList list = new CodeDescriptionPairList();
							DeliveryOrderHeader header = Parent.Parent;
							JobDeclaration declaration = header == null ? null : header.Parent;
							if (declaration != null)
							{
								foreach (Bill bill in declaration.Bills)
								{
									ZString code = declaration.IsAir && bill.IsMasterBill ? bill.CU_BillNum : (ZString)(bill.US_UI_NKBillIssuerSCAC + bill.CU_BillNum);
									list.AddPair(code, bill.CU_BillUniqueCode);
								}
							}
							return list;
						});
				}
				return billListCached.Value;
			}
		}
		CachedProperty<CodeDescriptionPairList> billListCached;
	}
}

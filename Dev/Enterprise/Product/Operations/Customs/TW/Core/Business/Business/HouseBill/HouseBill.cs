using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public partial class Bill : Customs.Business.Bill, Integration.Customs.TW.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString CU_BillType
		{
			get { return base.CU_BillType; }
			set
			{
				bool hasChanged = CU_BillType != value;

				base.CU_BillType = value;
				if (hasChanged && !IsCopying)
				{
					if (IsContainerNoteBill)
					{
						CU_ParentBillUniqueCode = ZString.Empty;
					}

					Declaration?.Bills?.ForEach(c => c.MarkAsNeedingValidation());
				}
			}
		}

		protected override bool CU_ParentBillUniqueCode_ReadOnly => IsContainerNoteBill || base.CU_ParentBillUniqueCode_ReadOnly;

		ZBool IsContainerNoteBill => CU_BillType == BillTypeList.Codes.ContainerNote;
	}
}

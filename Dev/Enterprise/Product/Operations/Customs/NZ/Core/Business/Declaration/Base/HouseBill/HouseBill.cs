using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class Bill : Customs.Business.Bill, Integration.Customs.NZ.IBill
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation
		public new BillValidation Validation
		{
			get { return (BillValidation)base.Validation; }
		}

		protected override Customs.Business.CusDecHouseBillValidation GetNewValidation()
		{
			return new BillValidation(this);
		}

		public override ZGuid CU_JE
		{
			get { return base.CU_JE; }
			set
			{
				bool hasChanged = CU_JE != value;
				base.CU_JE = value;
				if (hasChanged && Declaration != null)
				{
					Declaration.MarkAsNeedingValidation();
				}
			}
		}
		#endregion

		#region PackingGroups
		public new PackingGroupCollection PackingGroups
		{
			get { return (PackingGroupCollection)base.PackingGroups; }
		}

		protected override Customs.Business.BasePackingGroupCollection CreateNewPackingGroup()
		{
			return new PackingGroupCollection(this);
		}
		#endregion

		#region Declaration
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}
		#endregion

		[BusinessObjectTestExclude]
		public override ZString CU_BillNum
		{
			get { return base.CU_BillNum; }
			set
			{
				base.CU_BillNum = value.ToUpper();
			}
		}

		public override ZString CU_BillType
		{
			get { return base.CU_BillType; }
			set
			{
				bool hasChanges = base.CU_BillType != value;
				base.CU_BillType = value;
				if (hasChanges && !IsCopying && Declaration != null)
				{
					if (IsHouseBill)
					{
						Bill masterBill = (Bill)Declaration.PrimaryMasterBill;
						if (masterBill != null)
						{
							CU_CU_ParentBill = masterBill.PK;//there is only one master per declaration for NZ
						}
					}

					foreach (PackingGroup group in PackingGroups)
					{
						foreach (Package packLine in group.Packages)
						{
							packLine.MarkAsNeedingValidation();
							packLine.Validation.ValidateCW_HouseBill();
						}
					}
				}
			}
		}

		public ZInt LoosePackageCount
		{
			get { return PackingGroups.GetElementWithNoContainer()?.TotalPackageCount() ?? 0; }
		}

		public ZDecimal ECI_ApportionedLoosePackageValue
		{
			get { return Declaration?.ECI_ApportionedContainerAndLoosePackageValues.GetValueForLoosePackages() ?? ZDecimal.Zero; }
		}

		public ZDecimal ECI_ApportionedLoosePackageWeight
		{
			get
			{
				ZDecimal totalWeightInKGS = Declaration.JE_DeclaredWeight;
				ZDecimal containerWeightInKGS = Declaration.CusContainers.TotalGoodsWeight;
				ZDecimal loosePackageWeight = totalWeightInKGS - containerWeightInKGS;
				return loosePackageWeight > 0 ? loosePackageWeight : ZDecimal.Zero;
			}
		}

		protected override ZBool ShouldDeleteBillIfNumberIsEmpty
		{
			get { return true; }
		}
	}

	public class BillTypeList : Customs.Business.BillTypeList
	{
	}
}

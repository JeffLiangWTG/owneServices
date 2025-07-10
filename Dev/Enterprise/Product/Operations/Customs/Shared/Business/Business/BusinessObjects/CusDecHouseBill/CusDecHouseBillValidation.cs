using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusDecHouseBillValidation : AutoCusDecHouseBillValidation
	{
		public CusDecHouseBillValidation(AutoCusDecHouseBill parent)
			: base(parent)
		{
		}

		public static string NoPackagesEnteredForThisHouseBill
		{
			get { return Res.GetString("b3612dce-76fb-42ad-9974-dd9a3dbfeaa4", "No Packing Lines against Bill - Please enter some Packing Lines for this Bill."); }
		}

		new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCU_ParentBillUniqueCode();
		}

		#region Calculated Validations

		public void ValidateCU_ParentBillUniqueCode()
		{
			ValidateCalculatedProperty(Parent.CU_ParentBillUniqueCodeInfo);
		}

		protected virtual void CheckCU_ParentBillUniqueCode()
		{
			if (!Bill.CU_ParentBillUniqueCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Bill.CU_ParentBillUniqueCodeInfo, Bill.Lookups.CU_ParentBillList);
			}

			if (Bill.CU_BillType == BillTypeList.Codes.HouseBill && IsMasterBillMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Bill.CU_ParentBillUniqueCodeInfo);
			}
		}

		protected bool IsMasterBillMandatory
		{
			get { return Parent.Declaration != null && Parent.Declaration.Validation is BaseJobDeclarationValidation validation && validation.IsMasterBillMandatory; }
		}

		protected void CheckCU_HouseBill()
		{
			//Do not want ValidateAll() to validate this
		}

		protected void CheckCU_HouseBillIsWesternEuropean()
		{
			//Do not want ValidateAll() to validate this
		}

		protected void CheckCU_MasterBill()
		{
			//Do not want ValidateAll() to validate this
		}

		protected void CheckCU_MasterBillIsWesternEuropean()
		{
			////Do not want ValidateAll() to validate this
		}
		#endregion

		#region CheckCU_BillNum

		protected override void CheckCU_BillNum()
		{
			base.CheckCU_BillNum();

			CheckCU_BillNumForAllTypes();

			switch (Bill.CU_BillType)
			{
				case BillTypeList.Codes.MasterBill:
					CheckCU_BillNumForMasterBillType();

					if (Bill.CU_GUIPresentationRecord && Bill.Declaration != null)
					{
						Bill.Declaration.Validation.ValidateJE_MasterBill();
					}

					break;
				case BillTypeList.Codes.HouseBill:
					CheckCU_BillNumForHouseBillType();
					break;
				case BillTypeList.Codes.SubHouseBill:
					CheckCU_BillNumForSubHouseBill();
					break;
			}
		}

		protected virtual void CheckCU_BillNumForAllTypes()
		{
			CheckDuplicateBill();

			if (Bill.Declaration != null && Bill.IsLowestBill && NeedToValidateHouseBillAndPackages)
			{
				CheckHouseBillAndPackageRecord(Bill.CU_BillNumInfo);
			}
		}

		public virtual INotificationType NotificationTypeForAirWayBillNumber
		{
			get { return CargoWise.EntityFramework.NotificationType.Warning; }
		}

		void CheckDuplicateBill()
		{
			Bill.ClearRowNotifications();

			var declaration = Bill.Declaration;
			if (declaration != null && declaration.IsPackingInformationRelevant
				&& declaration.Bills.Cast<Bill>().Any(IsDuplicateBill))
			{
				Bill.AddRowError(DuplicateBillNumberTypeParent);
			}
		}

		public static string DuplicateBillNumberTypeParent
		{
			get { return Res.GetString("bac0f70f-4f46-4b16-b24c-7216ec8dd709", "This is a duplicate record. You cannot enter the same combination of Bill Type, Bill Number and Parent Bill. Please change one of these values or delete the row."); }
		}

		protected virtual bool IsDuplicateBill(Bill billToCompareTo)
		{
			return
				billToCompareTo != Bill &&
				billToCompareTo.CU_CU_ParentBill == Bill.CU_CU_ParentBill &&
				billToCompareTo.CU_BillNum.EqualsIgnoringCase(Bill.CU_BillNum) &&
				billToCompareTo.CU_BillType == Bill.CU_BillType;
		}

		protected virtual void CheckCU_BillNumForSubHouseBill()
		{
		}

		protected virtual void CheckCU_BillNumForHouseBillType()
		{
			BillValidator.CheckMandatory(IsHouseBillMandatory, Parent.CU_BillNumInfo, Res.GetString("a6ee0087-4948-4905-924f-7b6786dbebf2", "House Bill"));
			if (Declaration != null && Declaration.Branch != null)
			{
				BillValidator.CheckDuplicateDeclaration(Parent.CU_BillNumInfo, Parent.Declaration, Parent.CU_HouseBill, Parent.CU_MasterBill);
			}
		}

		protected bool IsHouseBillMandatory
		{
			get { return Parent.Declaration != null && Parent.Declaration.IsHouseBillMandatory; }
		}

		protected virtual bool NeedToValidateHouseBillAndPackages
		{
			get { return Bill.Declaration != null && Bill.Declaration.IsPackingInformationRelevant; }
		}

		protected void CheckHouseBillAndPackageRecord(ZPropertyInfo info)
		{
			if (!Bill.PackingGroups.HasPackageRecord)
			{
				info.AddMessageError(NoPackagesEnteredForThisHouseBill);
			}
		}

		protected virtual void CheckCU_BillNumForMasterBillType()
		{
			BillValidator.CheckMandatory(IsMasterBillMandatory, Parent.CU_BillNumInfo, Res.GetString("99a662f6-5d96-4313-9a40-69024b2d560c", "Master Bill"));
			if (Declaration != null && !Declaration.IsExWarehouse)
			{
				if (Parent.IsBillNumberAWB)
				{
					BillValidator.CheckAirWayBill(Declaration, Parent.CU_BillNumInfo, NotificationTypeForAirWayBillNumber);
				}

				if (Parent.IsLowestBill)
				{
					BillValidator.CheckDeclarationWithSameDirectMasterBill(Declaration, Parent.CU_BillNumInfo);
				}
			}
		}

		#endregion

		#region CheckCU_BillType

		protected override void CheckCU_BillType()
		{
			base.CheckCU_BillType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CU_BillTypeInfo, Parent.Lookups.CU_BillTypeList);

			if (!ShouldAllowMultiMaster && IsParentAnAdditionalMasterBill)
			{
				Bill.CU_BillTypeInfo.AddMessageError(MultiMastersNotAllowed);
			}
		}

		public static string MultiMastersNotAllowed
		{
			get { return Res.GetString("951b7fee-6773-4ddc-aabf-95f05c03acb8", "There is already a master bill and a job can have only one master bill."); }
		}

		bool IsParentAnAdditionalMasterBill
		{
			get
			{
				return Parent.IsMasterBill &&
					Parent.Declaration != null &&
					Parent.Declaration.PrimaryMasterBill != null &&
					Parent != Parent.Declaration.PrimaryMasterBill;
			}
		}

		protected virtual bool ShouldAllowMultiMaster
		{
			get { return true; }
		}

		#endregion

		protected override void CheckCU_PackType()
		{
			base.CheckCU_PackType();
			CheckCU_PackTypeIsAValidCode();
		}

		protected virtual void CheckCU_PackTypeIsAValidCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CU_PackTypeInfo, Parent.Lookups.NoOfPacksPackType_List, CU_PackTypInvalideMessage);
		}

		protected ResourceString CU_PackTypInvalideMessage => ResString.GetMultilingualString("6c9de89a-ea38-46b5-beed-5a4cae3b2c5f", "Please enter a valid Manifest UQ code. The code you have selected is not in the Manifest UQ codes List.");

		#region Related Objects

		BaseJobDeclaration Declaration
		{
			get { return Bill.Declaration; }
		}

		protected Bill Bill
		{
			get { return (Bill)base.Parent; }
		}

		BillValidator BillValidator
		{
			get
			{
				if (billValidator == null)
				{
					billValidator = GetBillValidator();
				}
				return billValidator;
			}
		}
		BillValidator billValidator;

		protected virtual BillValidator GetBillValidator()
		{
			return new BillValidator();
		}

		#endregion

	}
}

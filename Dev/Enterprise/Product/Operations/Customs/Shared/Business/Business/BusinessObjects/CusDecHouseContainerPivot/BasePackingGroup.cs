using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow()]
	public class BasePackingGroup : AutoCusDecHouseContainerPivot, IPackingSearch, ICanDelete, Integration.Customs.IBasePackingGroup, IClusterKeyWorker, ITypeDeciderContext, IAdditionalDebuggingDetails
	{
		public static readonly BasePackingGroupTypeDecider TypeDecider = new BasePackingGroupTypeDecider();

		public BasePackingGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(BasePackingGroup);
			}

			public BasePackingGroup GetPackingGroupMatching(Bill bill, BusinessObject containerOrEquipment)
			{
				if (bill == null)
				{
					return null;
				}
				var container = containerOrEquipment as BaseCusContainer;
				object containerPK = DBNull.Value;
				CusEquipment equipment = null;
				if (container == null)
				{
					equipment = containerOrEquipment as CusEquipment;
				}
				else
				{
					containerPK = container.PK;
				}
				var equipmentPK = equipment == null ? (object)DBNull.Value : equipment.PK;

				var query = new ZQuery(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, bill.PK);
				query.AddToFilter(CusDecHouseContainerPivotSchema.CR_CO_Container, containerPK);
				query.AddToFilter(CusDecHouseContainerPivotSchema.CR_CEQ_Equipment, equipmentPK);
				query.OrderBy = CusDecHouseContainerPivotSchema.Constants.CR_SystemCreateTimeUtc;
				query.FetchOnlyFromLocalCache = !bill.IsInDatabase;

				return Factory.LoadTop1<BasePackingGroup>(query);
			}
		}

		#region Related Business Objects

		#region Declaration

		public BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					if (Bill != null)
					{
						fDeclaration = Bill.Declaration;
					}
					else if (Container != null)
					{
						fDeclaration = Container.Declaration;
					}
				}
				return fDeclaration;
			}
			set { fDeclaration = value; }
		}
		BaseJobDeclaration fDeclaration;

		#endregion

		#region House Bill

		public Bill Bill
		{
			get { return Factory.Load<Bill>(CR_CU_HouseBill); }
		}

		public ZString BillType => Bill?.CU_BillType ?? ZString.Empty;

		#endregion

		#region Container

		public BaseCusContainer Container => CR_CO_Container.IsValid ? Factory.Load<BaseCusContainer>(CR_CO_Container) : null;

		#endregion

		#region Equipment

		public CusEquipment Equipment => CR_CEQ_Equipment.IsValid ? Factory.Load<CusEquipment>(CR_CEQ_Equipment) : null;

		#endregion

		#endregion

		#region New Collections

		protected BasePackageCollection fPackages;
		public BasePackageCollection Packages
		{
			get
			{
				if (fPackages == null && Declaration != null)
				{
					fPackages = CreateNewPackageCollection();
					fPackages.Rebuild();
				}

				return fPackages;
			}
		}

		protected virtual BasePackageCollection CreateNewPackageCollection()
		{
			return new BasePackageCollection(this);
		}

		[ChildEditable(true)]
		public
#if DEBUG
 virtual
#endif
 EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = GetNewMessageCollection();
					fMessages.Load();
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		protected virtual EDIMessageCollection GetNewMessageCollection()
		{
			return new EDIMessageCollection(this, Factory);
		}

		#endregion

		#region New Methods

		public void DefaultPackTypeIfRequired(ZString packType)
		{
			if (Packages is BasePackageCollection packages && !packages.HasMultiplePackages)
			{
				var package = packages.Count == 0 ? packages.AddNew() : packages[0];
				package.CW_PackType = GetConvertedPackType(packType);
			}
		}

		protected virtual ZString GetConvertedPackType(ZString packType)
		{
			return packType;
		}

		protected virtual bool ShouldCopyDeclarationTotalNoOfPacksCore
		{
			get { return TotalPackageCount() == 0m; }
		}

		public void AddTotalOuterPackageIfRequired(ZInt totalPackQty)
		{
			if (ShouldCopyDeclarationTotalNoOfPacksCore && Packages is BasePackageCollection packages && !packages.HasMultiplePackages)
			{
				var package = packages.Count == 0 ? packages.AddNew() : packages[0];
				package.CW_PackQty = totalPackQty;
			}
		}

		public int TotalPackageCount()
		{
			var result = 0;
			if (Declaration is BaseJobDeclaration declaration && Packages is BasePackageCollection packages && packages.Count > 0)
			{
				var parentPKs = declaration.Packages.Cast<BasePackage>().Select(c => c.CW_CW_Parent).ToHashSet();
				var packagesToSum = packages.Cast<BasePackage>();
				result = parentPKs.Count > 0 ? packagesToSum.Where(c => !parentPKs.Contains(c.PK)).Sum(c => c.CW_PackQty) : packagesToSum.Sum(c => c.CW_PackQty);
			}

			return result;
		}

		#endregion

		#region Property Overrides

		public override ZString CR_CargoStatus
		{
			get { return base.CR_CargoStatus; }
			set
			{
				if (Declaration != null && Declaration.CustomsEntryHeaders.Count > 0)
				{
					using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(Declaration))
					{
						base.CR_CargoStatus = value;
					}
				}
				else
				{
					base.CR_CargoStatus = value;
				}
			}
		}

		[RelatedBusinessObject("Bill")]
		public sealed override ZGuid CR_CU_HouseBill
		{
			get { return base.CR_CU_HouseBill; }
			set
			{
				var previousBill = Bill;
				bool valueHasChanged = value != CR_CU_HouseBill;
				base.CR_CU_HouseBill = value;

				if (valueHasChanged && !IsCopying)
				{
					Declaration.MarkAsNeedingValidationForMajorDataChange();
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						Packages.MarkAsNeedingValidation();
					}

					if (Bill != null)
					{
						CR_ClusterKey = Bill.CU_ClusterKey;
						Bill.RefreshContainers = true;
					}
					if (previousBill != null)
					{
						previousBill.RefreshContainers = true;
					}
				}
			}
		}

		public override ZGuid CR_CO_Container
		{
			get { return base.CR_CO_Container; }
			set
			{
				var oldValue = CR_CO_Container;
				base.CR_CO_Container = value;
				if (!IsCopying)
				{
					var newValue = CR_CO_Container;
					if (newValue != oldValue)
					{
						if (Bill is Bill bill)
						{
							bill.RefreshContainers = true;
							if (!IsValidationSuspended)
							{
								bill.Validation.ValidateCU_BillNum();
							}
						}
						if (newValue.IsValid && CR_CEQ_Equipment.IsValid)
						{
							CR_CEQ_Equipment = ZGuid.Empty;
						}
						Packages?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZGuid CR_CEQ_Equipment
		{
			get => base.CR_CEQ_Equipment;
			set
			{
				var oldValue = CR_CEQ_Equipment;
				base.CR_CEQ_Equipment = value;
				if (!IsCopying)
				{
					var newValue = CR_CEQ_Equipment;
					if (newValue != oldValue)
					{
						if (newValue.IsValid && CR_CO_Container.IsValid)
						{
							CR_CO_Container = ZGuid.Empty;
						}
						Packages.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region Business Object Base Overrides

		public override void Delete()
		{
			if (!IsDeleted)
			{
				BuildDeleteLog();

				var bill = Bill;
				ClearPackages();

				base.Delete();

				if (bill != null)
				{
					bill.RefreshContainers = true;
				}
			}
		}

		protected override void DeleteForDataRefresh()
		{
			BuildDeleteLog();
			ClearPackages();
			base.DeleteForDataRefresh();
		}

		void ClearPackages()
		{
			Packages?.RemoveAndDeleteAll();
		}

		void BuildDeleteLog()
		{
			if (string.IsNullOrWhiteSpace(deleteLog))
			{
				deleteLog = $@"
Properties:

{nameof(CR_CU_HouseBill)}: {CR_CU_HouseBill}

Stack Trace:

{System.Environment.StackTrace}";
			}
		}

		string deleteLog;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		public class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(BasePackingGroup pivot)
				: base(pivot)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();
				foreach (var package in ((BasePackingGroup)BusinessObject).Packages)
				{
					package.FetchStrategy.FetchForDelete();
				}
			}
		}

		protected override StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message)
		{
			var result = base.BuildRowDeletedReport(columnName, ex, versionToUse, version, message);

			if (!string.IsNullOrWhiteSpace(deleteLog))
			{
				result
					.Append((NoResString)"Delete Log: ")
					.AppendLine(deleteLog);
			}

			return result;
		}

		#endregion

		#region Clone
		protected override bool SupportsCloneCore()
		{
			return true;
		}
		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return Declaration != null && (!Declaration.IsPluggedIntoShipment || Declaration.JE_OverrideFreightDefaults); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return Bill.ReasonForCannotDeleteWhenSynchronised; }
		}

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CR_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(Bill);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CR_CU_HouseBillInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(BasePackage), CusDecHouseContainerPackSchema.CW_CR_HouseContainer);
			}
		}

		#endregion

		#region IUniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new BasePackingGroupUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Declaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IAdditionalDebuggingDetails Members
		IEnumerable<string> IAdditionalDebuggingDetails.AdditionalDetails
		{
			get
			{
				yield return $"{GetType().FullName} - In DB:{IsInDatabase} - {PK}";
				if (Bill is IAdditionalDebuggingDetails additionalDebuggingDetails)
				{
					foreach (var detail in additionalDebuggingDetails.AdditionalDetails)
					{
						yield return detail;
					}
				}
			}
		}
		#endregion

		public override bool IsSavedByFactory => base.IsSavedByFactory && (IsDeleted || (Declaration?.IsPersistent ?? false));
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(CusPermitHeaderSchema.Constants.CPH_Number)]
	[DescriptionProperty(CusPermitHeaderSchema.Constants.CPH_Number)]
	public abstract class CommonCusPermitHeader : AutoCusPermitHeader, Integration.Customs.ICommonCusPermitHeader
	{
		protected CommonCusPermitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CommonCusPermitHeaderTypeDecider TypeDecider = new CommonCusPermitHeaderTypeDecider();

		#region Business Object Overrides

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				foreach (var log in editedLogs)
				{
					log.Delete();
				}
			}

			editedLogs.Clear();
		}

		public override void Delete()
		{
			UnlockMutex();
			base.Delete();
		}

		#endregion

		#region Mutex

		public ZGlobalMutex Mutex => mutex ?? (mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "CPH" + PK.ToString()));
		ZGlobalMutex mutex;

		public void UnlockMutex()
		{
			if (mutex != null && mutex.HasLock)
			{
				mutex.Unlock();
			}
		}

		public bool LockMutex => Mutex.Lock();

		public string GetMutexLockInfo() => Mutex.GetMutexLockByInfo();

		#endregion

		#region Implementation

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public abstract ZString ShortName { get; }

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				UnlockMutex();
			}
		}

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();
			if (IsInDatabase)
			{
				var editLogReferences = new Stack<ZString>();

				var fieldsToLog = new ZPropertyInfo[]
				{
					CPH_NumberInfo,
					CPH_StartDateInfo,
					CPH_EndDateInfo,
					CPH_TypeInfo,
					CPH_SubTypeInfo,
					CPH_QtyValIndicatorInfo,
					CPH_UnitOfMeasureInfo
				};

				foreach (var info in fieldsToLog)
				{
					if (info.HasChanges)
					{
						editLogReferences.Push(GetHeaderChangeLogReference(info.HumanReadableName, info.OriginalValue.ToString(), info.Value.ToString()));
					}
				}

				if (CPH_OH_PermitHolderInfo.HasChanges)
				{
					var oldHolder = Factory.Load<OrgHeader>((ZGuid)CPH_OH_PermitHolderInfo.OriginalValue);
					editLogReferences.Push(GetHeaderChangeLogReference(CPH_OH_PermitHolderInfo.HumanReadableName, oldHolder?.OH_Code ?? ZString.Empty, PermitHolder?.OH_Code ?? ZString.Empty));
				}

				if (CPH_OA_AppliesToInfo.HasChanges)
				{
					var oldAddress = Factory.Load<OrgAddress>((ZGuid)CPH_OA_AppliesToInfo.OriginalValue);
					editLogReferences.Push(GetHeaderChangeLogReference(CPH_OA_AppliesToInfo.HumanReadableName, oldAddress?.OA_Code ?? ZString.Empty, AppliesTo?.OA_Code ?? ZString.Empty));
				}

				if (editLogReferences.Count > 0)
				{
					var autoCreatedLog = Logs.AutoCreatedLog;
					if (autoCreatedLog != null && autoCreatedLog.SL_Reference.IsEmpty)
					{
						var firstReference = editLogReferences.Pop();
						using (((IUpdateFieldsLock)Logs.AutoCreatedLog).LockForUpdatingKeyFields())
						{
							Logs.AutoCreatedLog.SL_Reference = firstReference;
						}
					}
				}

				foreach (var logReference in editLogReferences)
				{
					AddHeaderChangeLog(logReference);
				}
			}
		}

		ZString GetHeaderChangeLogReference(ZString field, ZString oldValue, ZString newValue)
		{
			return ZString.Format((NoResString)"{0}: From '{1}' to '{2}'", field, oldValue, newValue);
		}

		void AddHeaderChangeLog(ZString logReference)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			editedLogs.Add(Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, logReference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		readonly List<StmALog> editedLogs = new List<StmALog>();

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPH_OH_PermitHolder = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper() => new CusPermitHeaderBusinessObjectTestDataHelper(CPH_ApplicationCode);

		class CusPermitHeaderBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			public CusPermitHeaderBusinessObjectTestDataHelper(string applicationCode)
			{
				this.applicationCode = applicationCode;
			}
			readonly string applicationCode;

			protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
			{
				if (property.Name != CusPermitHeaderSchema.CPH_JobNumber.Name || applicationCode == CusPermitHeaderApplicationCodeList.Codes.Operational)
				{
					base.PopulateUniqueString(property, propertyPath, maxLength);
				}
			}
		}
#endif

		#endregion
	}
}

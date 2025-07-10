using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class BaseTariffBulkChange : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected BaseTariffBulkChange(BusinessObjectFactory factory, ZString lookupType)
			: base(factory)
		{
			this.lookupType = lookupType;
		}
		protected ZString lookupType;
		public bool IsSaveAllowed { get; protected set; }
		public bool IsImbeddedConcordance { get; protected set; }

		public static Event ChangeDataEvent => Events.CustomsTariffUpdatePending;

		#region Properties & Methods

		protected TariffFormatter CurrentTariffFormatter => fCurrentTariffFormatter ?? (fCurrentTariffFormatter = GetCurrentTariffFormatter());
		TariffFormatter fCurrentTariffFormatter;

		protected virtual TariffFormatter GetCurrentTariffFormatter()
		{
			var classForCountryAndTypeFilter = new ZQuery(CusClassificationSchema.CC_ClassificationType, lookupType);
			classForCountryAndTypeFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, CountryCode);
			var firstClassForCountryAndType = Factory.LoadTop1<BaseCusClassification>(classForCountryAndTypeFilter);
			return firstClassForCountryAndType?.CurrentTariffFormatter ?? new TariffFormatter();
		}

		string[] tariffNums;
		readonly char[] delimiters = { ',' };
		const int maxTariffLength = 15;

		public void LoadConcordance(Stream str, bool automaticConvert, [Optional] bool isSaveAllowed, [Optional] bool isImbeddedConcordance)
		{
			IsSaveAllowed = isSaveAllowed;
			IsImbeddedConcordance = isImbeddedConcordance;

			if (str != null)
			{
				var bulkChangeOldTariffDict = new Dictionary<string, TariffBulkChange.TariffBulkChangeOldTariff>();
				var isOldTariffUsedDict = new Dictionary<string, bool>();
				using (str)
				{
					StreamReader reader = new StreamReader(str);
					while (reader.Peek() >= 0)
					{
						tariffNums = reader.ReadLine().Split(delimiters, 2);
						if (tariffNums.Length == 2 && tariffNums[0].Length <= maxTariffLength && tariffNums[1].Length <= maxTariffLength)
						{
							var oldTariffNum = tariffNums[0];
							var newTariffNum = tariffNums[1];

							if (!string.IsNullOrEmpty(oldTariffNum) && !string.IsNullOrEmpty(newTariffNum))
							{
								if (!isOldTariffUsedDict.TryGetValue(oldTariffNum, out bool isOldTariffUsed))
								{
									isOldTariffUsed = IsOldTariffUsedCore(oldTariffNum);
									isOldTariffUsedDict.Add(oldTariffNum, isOldTariffUsed);
								}

								if (isOldTariffUsed)
								{
									if (!bulkChangeOldTariffDict.TryGetValue(oldTariffNum, out var buldChangeOldTariff))
									{
										buldChangeOldTariff = AddNewTariffNumPair(oldTariffNum, newTariffNum);
										bulkChangeOldTariffDict.Add(oldTariffNum, buldChangeOldTariff);
									}
									else
									{
										AddAdditionalNewTariffNum(buldChangeOldTariff, newTariffNum);
									}
								}
							}
						}
					}
				}
				str.Dispose();

				foreach (var bulkChangeOldTariff in bulkChangeOldTariffDict.Values)
				{
					if (bulkChangeOldTariff.TariffBulkChangeNewTariffs.Count == 1)
					{
						var oldTariffNum = bulkChangeOldTariff.OldTariffNum.KeepNumericCharacters();
						var newTariffNum = bulkChangeOldTariff.TariffBulkChangeNewTariffs[0].NewTariffNum.KeepNumericCharacters();
						if (oldTariffNum == newTariffNum)
						{
							RemoveTariffBulkChangeOldTariff(bulkChangeOldTariff);
						}
						else if (automaticConvert && newTariffNum.Length >= 8)
						{
							ProcessOne2One(oldTariffNum, newTariffNum);
							RemoveTariffBulkChangeOldTariff(bulkChangeOldTariff);
						}
					}
				}
			}

			WarnIfDemoOrDateInvalid();
		}

		protected virtual bool IsOldTariffUsedCore(string oldTariffNum) => Factory.LoadTop1<BaseCusClassification>(GetClassificationQuery(oldTariffNum)) != null;

		protected abstract TariffBulkChange.TariffBulkChangeOldTariff AddNewTariffNumPair(ZString oldtariffnum, ZString newtariffnum);

		protected abstract void AddAdditionalNewTariffNum(TariffBulkChange.TariffBulkChangeOldTariff bulkChangeOldTariff, ZString newtariffnum);

		protected abstract void RemoveTariffBulkChangeOldTariff(TariffBulkChange.TariffBulkChangeOldTariff bulkChangeOldTariff);

		protected abstract void ProcessOne2One(ZString oldTariffNum, ZString newTariffNum);

		protected virtual ZQuery GetClassificationQuery(ZString oldtariffnum) => new ZQuery();

		public virtual bool IsProductionDataBase => ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production;

		protected bool IsDemoCompany => GlbCompany.CurrentCompany.GC_Code == "DEM";

		protected bool IsDateInvalid => DateTimeNow < new ZDateTime(2012, 1, 1).AddDays(-2);

		protected virtual ZDateTime DateTimeNow => ZDateTime.Now;

		public static string DemoBranchInitialWarningMessage
			=> Res.GetString("c4c5811d-8437-40e6-82a2-4155680ce50a", "You are currently logged into a Demonstration Branch. You will not be allowed to save any changes that you make.");

		public static string DemoBranchMessage
			=> Res.GetString("21c2cd40-185b-44c4-81ec-bef5313d631e", "You are currently logged into a Demonstration Branch. Save is not allowed.");

		public static string DateWarningMessage
			=> Res.GetString("b8af6919-d877-41e0-b091-75544653f0b9", "The new tariff changes do not come into effect until the 1st January 2017. You should not update production data at this time.");

		public virtual string SaveNotAllowedMessage
			=> Res.GetString("{3B150044-7146-4405-B1C0-0DB9C6135FCB}", "The HS2022 concordance is not available yet. Save is not allowed.");

		public static string TariffBulkChangeConcordanceFileTitle
			=> Res.GetString("39511107-e65a-4f09-a735-8859884cdd16", "Select Tariff Bulk Change Concordance File");

		public static string TariffBulkChangeFinalConfirmationCaption
			=> Res.GetString("82d04701-7126-4560-a3ba-a86950a9f7d2", "Final Confirmation");

		public virtual string ApplyTariffChangesMessage
			=> Res.GetString("c7e5883f-028c-45b4-afb3-d01be6468405", @"Your Data Base will now be updated with Pending Tariff Changes.
NOTE: If you are applying pending changes for the HS2022 change then you should only apply these changes on, or after, the 1st January 2022.
Do you wish to continue?");

		protected static string SaveNotAllowedResourceString
			=> Res.GetString("1eb29e0e-172a-4a4e-9e63-85b147dfea13", "Save Not Allowed");

		public void WarnIfDemoOrDateInvalid()
		{
			if (IsProductionDataBase)
			{
				if (IsDemoCompany)
				{
					Globals.Message.ShowWarning(DemoBranchInitialWarningMessage, Res.GetString("92d2964e-099a-4000-865c-0534bf67ca98", "Access Denied"));
				}
			}
		}

		public ContinueWithSave AdditionalContinueWithSave(bool automaticConvert)
		{
			ContinueWithSave result = ContinueWithSave.No;
			if (!IsProductionDataBase)
			{
				{
					result = ContinueWithSave.Yes;
				}
			}
			else if (IsDemoCompany)
			{
				Globals.Message.ShowError(DemoBranchMessage, Res.GetString("af831b8b-3754-4664-8114-eb736679f336", "Access Denied"));
			}
			else
			{
				result = ContinueWithSave.Yes;
			}
			return result;
		}

		public ContinueWithSave ApplyAdditionalContinueWithSave()
		{
			var result = ContinueWithSave.No;
			string messageText = "";
			if (!IsProductionDataBase)
			{
				if (IsDateInvalid)
				{
					messageText = DateWarningMessage;
					result = ContinueWithSave.Yes;
				}
				else
				{
					result = ContinueWithSave.Yes;
				}
			}
			else if (IsDemoCompany)
			{
				Globals.Message.ShowError(DemoBranchMessage, Res.GetString("b5a83929-310d-47e2-bef7-6008009e0fe6", "Access Denied"));
			}
			else
			{
				if (!IsSaveAllowed)
				{
					Globals.Message.ShowError(SaveNotAllowedMessage, SaveNotAllowedResourceString);
				}
				else
				{
					result = ContinueWithSave.Yes;
				}
			}
			if (result == ContinueWithSave.Yes)
			{
				if (!string.IsNullOrEmpty(messageText))
				{
					messageText += " ";
				}
				messageText += Res.GetString("db5e6eea-5844-477d-bdec-5b7a0894a026", "Your pending Tariff Changes will now be saved. Note that these pending changes are not applied to your Data Base at this time, but will be applied when you select the 'Apply All Pending Changes' option. Do you wish to continue?");
				if (Globals.Message.Show(messageText, Res.GetString("550c4546-42c3-4968-9aac-71d861f44d20", "Final Confirmation"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question) != ZDialogResult.Yes)
				{
					result = ContinueWithSave.No;
				}
			}
			if (result == ContinueWithSave.Yes && IsImbeddedConcordance)
			{
				var auLogs = Factory.Load<RefCountry>(CountryPK).Logs;
				auLogs.AddNew(ChangeDataEvent, ReferenceKey);
			}

			return result;
		}

		public virtual ZString ReferenceKey => throw new NotImplementedException();

		public virtual ZGuid CountryPK => throw new NotImplementedException();

		public virtual ZString CountryCode => throw new NotImplementedException();

		public void CheckAndRemoveAnyExistingPendingChanges()
		{
			var factory = new BusinessObjectFactory();
			var logQuery = new ZQuery();
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, ChangeDataEvent.Code);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, CountryPK);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, ReferenceKey);
			if (factory.LoadTop1<StmALog>(logQuery) == null)
			{
				var classQuery = new ZQuery();
				classQuery.AddToFilter(CusClassificationSchema.CC_ClassificationType, lookupType);
				classQuery.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, CountryCode);
				classQuery.AddToFilter(CusClassificationSchema.CC_TariffChangePending, true);
				var tBCClassifications = new BaseClassificationCollection<BaseCusClassification>(factory);
				tBCClassifications.Load(classQuery);
				foreach (BaseCusClassification tBCClass in tBCClassifications)
				{
					tBCClass.CC_TariffChangePending = false;
					var changeDataLogs = new LogsForNominatedEvent(tBCClass.Logs, ChangeDataEvent);
					if (changeDataLogs != null)
					{
						changeDataLogs.CancelAll();
					}
				}

				var pivotQuery = new ZQuery();
				pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, CountryCode);
				pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_TariffChangePending, true);
				var pivotClassQuery = new ZDBOnlyQuery(typeof(BaseCusClassPartPivot));
				var classificationFilter = new ZDBOnlySubQuery(typeof(BaseCusClassification), CusClassPartPivotSchema.CI_CC);
				classificationFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, SQLComparisonOperator.Equal, CountryCode);
				classificationFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, SQLComparisonOperator.Equal, lookupType);
				pivotClassQuery.AddSubQuery(classificationFilter, JoinCondition.And);
				pivotQuery.AddToFilter(pivotClassQuery);
				var pivots = new BaseCusClassPartPivotCollection(factory);
				pivots.Load(pivotQuery);
				foreach (BaseCusClassPartPivot pivot in pivots)
				{
					pivot.CI_TariffChangePending = false;
					var changeDataLogs = new LogsForNominatedEvent(pivot.Logs, ChangeDataEvent);
					if (changeDataLogs != null)
					{
						changeDataLogs.CancelAll();
					}
				}
				factory.Save();
			}
		}

		#endregion

		public abstract class TBBaseManager
		{
			protected TBBaseManager(BusinessObject parent)
			{
				this.parent = parent;
			}
			readonly BusinessObject parent;

			protected StmALog TBCChangeDataLog
			{
				get
				{
					if (fTBCChangeDataLog == null)
					{
						var fTBCChangeDataLogs = new LogsForNominatedEvent(parent.GetLogs(), ChangeDataEvent);
						if (fTBCChangeDataLogs != null && fTBCChangeDataLogs.Count > 0)
						{
							fTBCChangeDataLog = fTBCChangeDataLogs[0];
						}
						else
						{
							fTBCChangeDataLog = parent.GetLogs().AddNew(ChangeDataEvent);
						}
						parent.RegisterEditableChildObject(fTBCChangeDataLog);
						SetChangePending(fTBCChangeDataLog);
					}
					return fTBCChangeDataLog;
				}
			}
			StmALog fTBCChangeDataLog;

			protected abstract void SetChangePending(StmALog changeDataLog);

			protected ZString GetReferenceSubString(int start, int length)
			{
				var fixedLengthReference = TBCChangeDataLog.SL_Reference.PadRight(StmALog.Schema.SL_ReferenceMaxLength);
				return fixedLengthReference.Substring(start, length).Trim();
			}

			protected ZString SetReferenceSubString(ZString value, int start, int length)
			{
				var fixedLengthReference = TBCChangeDataLog.SL_Reference.PadRight(StmALog.Schema.SL_ReferenceMaxLength);
				return fixedLengthReference.Substring(0, start) + value.PadRight(length) + fixedLengthReference.Substring(start + length);
			}
		}

		public class TBCClassManager : TBBaseManager
		{
			public TBCClassManager(BaseCusClassification tBCClassification)
				: base(tBCClassification)
			{
				this.tBCClass = tBCClassification;
			}
			readonly BaseCusClassification tBCClass;

			const int NewTariffNumStart = 0;
			const int NewTariffNumLength = BaseCusClassification.Schema.CC_TariffNumMaxLength;
			const int NewLookupCodeStart = NewTariffNumLength;
			const int NewLookupCodeLength = BaseCusClassification.Schema.CC_LookupCodeMaxLength;

			protected override void SetChangePending(StmALog changeDataLog)
			{
				tBCClass.CC_TariffChangePending = !changeDataLog.SL_Reference.IsEmpty;
			}

			public ZString NewTariffNum
			{
				get
				{
					return tBCClass.CC_TariffChangePending ? GetReferenceSubString(NewTariffNumStart, NewTariffNumLength) : ZString.Empty;
				}
				set
				{
					using (((IUpdateFieldsLock)TBCChangeDataLog).LockForUpdatingKeyFields())
					{
						TBCChangeDataLog.SL_Reference = SetReferenceSubString(value, NewTariffNumStart, NewTariffNumLength).TrimEnd();
					}
					SetChangePending(TBCChangeDataLog);
				}
			}

			public ZString NewLookupCode
			{
				get
				{
					return tBCClass.CC_TariffChangePending ? GetReferenceSubString(NewLookupCodeStart, NewLookupCodeLength) : ZString.Empty;
				}
				set
				{
					using (((IUpdateFieldsLock)TBCChangeDataLog).LockForUpdatingKeyFields())
					{
						TBCChangeDataLog.SL_Reference = SetReferenceSubString(value, NewLookupCodeStart, NewLookupCodeLength).TrimEnd();
					}
					SetChangePending(TBCChangeDataLog);
				}
			}
		}

		public class TBCPivotManager : TBBaseManager
		{
			public TBCPivotManager(BaseCusClassPartPivot pivot)
				: base(pivot)
			{
				this.tBCPivot = pivot;
			}
			readonly BaseCusClassPartPivot tBCPivot;

			const int NewLookupCodeStart = 0;
			const int NewLookupCodeLength = 64;
			const int NewTariffNumStart = NewLookupCodeLength;
			const int NewTariffNumLength = BaseCusClassification.Schema.CC_TariffNumMaxLength;

			protected override void SetChangePending(StmALog changeDataLog)
			{
				tBCPivot.CI_TariffChangePending = !changeDataLog.SL_Reference.IsEmpty;
			}

			public ZGuid NewLookupCode
			{
				get
				{
					if (!tBCPivot.CI_TariffChangePending)
					{
						return ZGuid.Empty;
					}
					else
					{
						ZString guidString = GetReferenceSubString(NewLookupCodeStart, NewLookupCodeLength);
						return guidString.IsEmpty ? ZGuid.Empty : new ZGuid(guidString);
					}
				}
				set
				{
					using (((IUpdateFieldsLock)TBCChangeDataLog).LockForUpdatingKeyFields())
					{
						TBCChangeDataLog.SL_Reference = SetReferenceSubString(value.ToString(), NewLookupCodeStart, NewLookupCodeLength).TrimEnd();
					}
					SetChangePending(TBCChangeDataLog);
				}
			}

			public ZString NewTariffNum
			{
				get
				{
					return tBCPivot.CI_TariffChangePending ? GetReferenceSubString(NewTariffNumStart, NewTariffNumLength) : ZString.Empty;
				}
				set
				{
					using (((IUpdateFieldsLock)TBCChangeDataLog).LockForUpdatingKeyFields())
					{
						TBCChangeDataLog.SL_Reference = SetReferenceSubString(value, NewTariffNumStart, NewTariffNumLength).TrimEnd();
					}
					SetChangePending(TBCChangeDataLog);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterData.Business.Tests
{
	class SaveAlwaysFailsFactory : IFactory
	{
		public SaveAlwaysFailsFactory()
		{
			ExceptionToThrow = new NotImplementedException();
		}

		public SaveAlwaysFailsFactory(Exception exceptionToThrow)
		{
			ExceptionToThrow = exceptionToThrow ?? new NotImplementedException();
		}

		readonly Exception ExceptionToThrow;

		#region IFactory Members

		public bool IsProcessingOnFactorySavingBeforeTransaction => throw ExceptionToThrow;

		public bool RefreshEnabled { get; set; }
		public bool ReadOnly { get; set; }
		public bool CanSave { get; set; }

		public RowFactory RowFactory => throw ExceptionToThrow;

		public string ContentsAsXMLForDebugging { get; }

		public int NumberOfBusinessObjects => 0;

		public IReadOnlyList<BusinessObject> AllBusinessObjects => throw ExceptionToThrow;

		public bool DisableQueryCacheReset { get; set; }

		public bool IsProcessingOnAllTransactionsCommitted => throw ExceptionToThrow;

		public bool LastSavingRollbackHadException { get; set; }

		bool includeWithOtherFactoriesForIssueReport = true;

		bool IBusinessObjectFactoryInternals.IncludeWithOtherFactoriesForIssueReport
		{
			get
			{
				return includeWithOtherFactoriesForIssueReport;
			}
			set
			{
				includeWithOtherFactoriesForIssueReport = value;
			}
		}

		public void ClearQueryCache()
		{
			throw ExceptionToThrow;
		}

		public IFactoryChangeSet GetChanges()
		{
			throw ExceptionToThrow;
		}

		public T Load<T>(ZGuid pK) where T : class
		{
			throw ExceptionToThrow;
		}

		public T Load<T>(string tablePrefix, ZGuid pk) where T : class
		{
			throw ExceptionToThrow;
		}

		public T[] Load<T>(ZQuery query) where T : class
		{
			throw ExceptionToThrow;
		}

		public BusinessObject Load(Type bizOType, ZGuid pK)
		{
			throw ExceptionToThrow;
		}

		public BusinessObject[] Load(Type bizOType, ZQuery sQLFilter)
		{
			throw ExceptionToThrow;
		}

		public BusinessObject LoadFromNaturalKey(Type bizOType, SchemaColumn column, ZString naturalKeyValue)
		{
			throw ExceptionToThrow;
		}

		public BusinessObject LoadFromUniqueKey(Type bizOType, SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue)
		{
			throw ExceptionToThrow;
		}

		public BusinessObject LoadTop1(Type bizOType, ZQuery sQLFilter)
		{
			throw ExceptionToThrow;
		}

		public BusinessObject New(Type bizOType)
		{
			throw ExceptionToThrow;
		}

		public void ReloadAll<T>() where T : BusinessObject
		{
		}

		public void Rollback()
		{
		}

		public void Save()
		{
			throw ExceptionToThrow;
		}

		#endregion
	}
}

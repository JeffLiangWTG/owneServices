using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class OrgCodeDbProxy : IOrgCodeDbProxy
	{
		readonly BusinessObjectFactory factory;
		Func<string, Guid, bool> overrideForIsCodeAlreadyInUseByAnotherOrganisationPreliminary;
		INumberFountainProxy numberFountain;

		public OrgCodeDbProxy(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public IDbCommand CreateCommand()
		{
			IDbConnected connected = factory;
			IDbConnectionInternals connection = connected.Connection;
			var result = connection.InternalDbConnection.CreateCommand();
			result.Transaction = connection.InternalDbTransaction;
			return result;
		}

		public bool IsCodeAlreadyInUseByAnotherOrganisation(string code, Guid currentOrganisationPK)
		{
			//Potential performance improvement: If we know we're about to check this for thousands of different codes, we could create a cache and check that cache instead.
			//(See WI00266600 (0.9) for an example implementation)
			var isCodeAlreadyInUse = false;
			var checkDbOnlyFilter = new ZDBOnlyQuery(typeof(OrgHeader));
			checkDbOnlyFilter.AddToFilter(OrgHeaderSchema.OH_Code, code);
			checkDbOnlyFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, currentOrganisationPK);
			if (factory.LoadTop1<OrgHeader>(checkDbOnlyFilter) != null)
			{
				IsCodeAlreadyInUseRetrieveFromCache = false;
				isCodeAlreadyInUse = true;
			}
			else
			{
				//If we didn't find a duplicate, we still have to check the local factory, which might contain a duplicate that hasn't been saved yet. (But we can avoid doing a DB hit, at least - so set FetchOnlyFromLocalCache to true.)
				var checkCacheOnlyFilter = new ZQuery(OrgHeaderSchema.OH_Code, code);
				checkCacheOnlyFilter.FetchOnlyFromLocalCache = true;
				checkCacheOnlyFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, currentOrganisationPK);
				if (factory.LoadTop1<OrgHeader>(checkCacheOnlyFilter) != null)
				{
					IsCodeAlreadyInUseRetrieveFromCache = true;
					isCodeAlreadyInUse = true;
				}
			}

			return isCodeAlreadyInUse;
		}

		public bool IsCodeAlreadyInUseByAnotherOrganisationPreliminary(string code, Guid currentOrganisationPK)
		{
			if (overrideForIsCodeAlreadyInUseByAnotherOrganisationPreliminary != null)
			{
				return overrideForIsCodeAlreadyInUseByAnotherOrganisationPreliminary(code, currentOrganisationPK);
			}

			return IsCodeAlreadyInUseByAnotherOrganisation(code, currentOrganisationPK);
		}

		public string NumberFountainGetNext()
		{
			return NumberFountain.GetNextFormatted(factory);
		}

		public string NumberFountainPeekPreliminary()
		{
			return NumberFountain.PeekPreliminaryFormatted(factory);
		}

		public long NumberFountainPeekPreliminaryLong()
		{
			return NumberFountain.PeekPreliminary(factory);
		}

		public void NumberFountainSetNext(long nextValue)
		{
			NumberFountain.SetNext(factory, nextValue);
		}

		public void OverrideIsCodeAlreadyInUseByAnotherOrganisationPreliminary(Func<string, Guid, bool> value)
		{
			overrideForIsCodeAlreadyInUseByAnotherOrganisationPreliminary = value;
		}

		INumberFountainProxy NumberFountain
		{
			get { return numberFountain ?? (numberFountain = Env.NumberFountains.OrgCodeNumberFountain); }
		}

		public bool CanUserEditOrganisationCode
		{
			get { return Env.Registry.CanUserEditOrganisationCode; }
		}

		public bool IsCodeAlreadyInUseRetrieveFromCache { get; set; }
	}
}

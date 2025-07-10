using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCodeUpdater
	{
		BusinessObjectFactory factory;
		readonly int recordsPerBlock = 100;

		public OrgCodeUpdater()
		{
		}

		protected OrgCodeUpdater(BusinessObjectFactory factory, int recordsPerBlock)
		{
			this.factory = factory;
			this.recordsPerBlock = recordsPerBlock;
		}

		DbConnection Connection
		{
			get { return ((IDbConnected)Factory).Connection; }
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		public event EventHandler<OrgCodeUpdateEventArgs> OrgCodeBlockUpdated;

		protected virtual DateTime GetStartTime()
		{
			return Env.Time.CurrentLocalDateTime;
		}

		protected virtual IOrgCodeInfo[] LoadAllOrgs()
		{
			return OrgCodeInfo.LoadAllAndUseHeapsOfResources(Factory);
		}

		void OnOrgCodeBlockUpdated(int totalProcessed, int totalCount)
		{
			if (OrgCodeBlockUpdated != null)
			{
				OrgCodeUpdateEventArgs e = new OrgCodeUpdateEventArgs(totalProcessed, totalCount);
				OrgCodeBlockUpdated(this, e);
			}
		}

		/// <summary>
		/// Updates all OrgHeader codes with the coding algorithm currently set in the registry. This is a lengthy process.
		/// </summary>
		public void UpdateOrgCodes(OrgCodeAlgorithmType algorithmType)
		{
			DateTime updateStartTime = GetStartTime();
			Dictionary<string, IOrgCodeInfo> dbCodes = new Dictionary<string, IOrgCodeInfo>();
			IOrgCodeInfo[] allOrgs = LoadAllOrgs();

			foreach (IOrgCodeInfo org in allOrgs)
			{
				dbCodes[org.OH_Code] = org;
			}

			OrgCodeGenerator generator = new OrgCodeGenerator();
			int processedCount = 0;

			var dbProxy = new OrgCodeDbProxy(Factory);
			dbProxy.OverrideIsCodeAlreadyInUseByAnotherOrganisationPreliminary((code, pk) =>
			{
				return dbCodes.TryGetValue(code, out IOrgCodeInfo org) && (org.PK != pk);
			});

			List<ZGuid> orgsToIgnore = new List<ZGuid>();
			foreach (IOrgCodeInfo org in allOrgs)
			{
				if (generator.GenerateCodeOnlyIfAlgorithmTypeApplies(algorithmType, org, dbProxy, out IOrgCode orgCode) && !orgsToIgnore.Contains(org.PK))
				{
					UpdateSingleOrgCode(org, dbProxy, orgCode, dbCodes, orgsToIgnore);
				}

				processedCount++;
				if (processedCount % recordsPerBlock == 0)
				{
					OnOrgCodeBlockUpdated(processedCount, allOrgs.Length);
				}
			}

			OnOrgCodeBlockUpdated(processedCount, allOrgs.Length);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal void UpdateSingleOrgCode(IOrgCodeInfo org, IOrgCodeDbProxy orgCodeDbProxy, IOrgCode orgCode, Dictionary<string, IOrgCodeInfo> dbCodes, List<ZGuid> orgsToIgnore)
		{
			if (orgCode.HasChanged(org.OH_Code))
			{
				var newCode = orgCode.GetProposedCode();

				if (newCode.Length > 0)
				{
					bool commit = false;
					using (var transactionManager = Connection.BeginTransactionWithManager())
					{
						bool duplicate;
						do
						{
							using (var command = Connection.Command("UPDATE dbo.OrgHeader SET OH_Code = @code WHERE OH_PK = @pk"))
							{
								try
								{
									command.AddParameterBasedOnDbColumn((NoResString)"@code", newCode, OrgHeaderSchema.OH_Code);
									command.AddParameterBasedOnDbColumn((NoResString)"@pk", org.PK, OrgHeaderSchema.PK);
									command.ExecuteNonQuery();
									commit = true;
									if (orgCode.UseGlobalNumberFountain)
									{
										orgCodeDbProxy.NumberFountainGetNext();
									}
									duplicate = false;
								}
								catch (SqlException ex)
								{
									if (new DbErrorHandler(ex, Connection).ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
									{
										ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, newCode)
										{
											ReLoadExistingRows = true
										};
										var existingOrg = Factory.LoadTop1<OrgHeader>(filter);
										if (existingOrg != null)
										{
											orgsToIgnore.Add(existingOrg.PK);
											newCode = orgCode.GetFinalCode();
										}

										duplicate = true;
									}
									else
									{
										throw;
									}
								}
							}
						} while (duplicate);

						dbCodes.Remove(org.OH_Code);
						dbCodes[newCode] = org;

						if (commit)
						{
							transactionManager.CommitTransaction();
						}
					}
				}
			}
		}

		#region OrgCodeUpdateEventArgs Class

		public class OrgCodeUpdateEventArgs : EventArgs
		{
			readonly int maxRecordsToProcess;
			readonly int totalProcessed;

			public OrgCodeUpdateEventArgs(int totalProcessed, int maxRecordsToProcess)
			{
				this.totalProcessed = totalProcessed;
				this.maxRecordsToProcess = maxRecordsToProcess;
			}

			public int MaxRecordsToProcess
			{
				get { return maxRecordsToProcess; }
			}

			public int TotalProcessed
			{
				get { return totalProcessed; }
			}
		}

		#endregion
	}
}

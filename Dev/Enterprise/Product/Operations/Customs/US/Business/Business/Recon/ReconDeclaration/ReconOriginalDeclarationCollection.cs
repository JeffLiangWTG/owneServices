using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class ReconOriginalDeclarationCollection
	{
		public ReconOriginalDeclarationCollection(ReconDeclaration reconDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
		}

		readonly ReconDeclaration reconDeclaration;

		BusinessObjectFactory Factory
		{
			get { return reconDeclaration.Factory; }
		}

		RowFactory RowFactory
		{
			get { return ((IBusinessObjectFactoryInternals)Factory).RowFactory; }
		}

		public void Load()
		{
			var existingPKs = new List<ZGuid>(DeclarationsByEntrySummaryPK.Keys);

			var oringalEntryPKs = (from ReconOriginalEntryHeader originalEntry in reconDeclaration.OriginalEntries
								   where originalEntry.CH_CH_OriginalEntry.IsValid
								   select originalEntry.CH_CH_OriginalEntry).ToArray();

			if (oringalEntryPKs.Length > 0)
			{
				var missingDetals = new Dictionary<ZGuid, List<IColumnIndexer>>();
				foreach (IColumnIndexer entryRow in RowFactory.Load(CusEntryHeaderSchema.Constants.TableName, new ZQuery(CusEntryHeaderSchema.PK, oringalEntryPKs)))
				{
					var entryPK = entryRow.GetValue(CusEntryHeaderSchema.PK);
					if (existingPKs.Contains(entryPK))
					{
						existingPKs.Remove(entryPK);
					}
					else
					{
						var declarationPK = entryRow.GetValue(CusEntryHeaderSchema.CH_JE);
						List<IColumnIndexer> list;
						if (!missingDetals.TryGetValue(declarationPK, out list))
						{
							list = new List<IColumnIndexer>();
							missingDetals.Add(declarationPK, list);
						}
						if (!list.Contains(entryRow))
						{
							list.Add(entryRow);
						}
					}
				}

				foreach (var entryPK in existingPKs)
				{
					DeclarationsByEntrySummaryPK.Remove(entryPK);
				}

				foreach (IColumnIndexer declarationRow in RowFactory.Load(JobDeclarationSchema.Constants.TableName, new ZQuery(JobDeclarationSchema.PK, missingDetals.Keys)))
				{
					var declarationPK = declarationRow.GetValue(JobDeclarationSchema.PK);
					List<IColumnIndexer> list;
					if (missingDetals.TryGetValue(declarationPK, out list))
					{
						foreach (IColumnIndexer entryRow in list)
						{
							DeclarationsByEntrySummaryPK.Add(entryRow.GetValue(CusEntryHeaderSchema.PK), new ReconOriginalDeclaration(Factory, declarationRow, entryRow));
						}
					}
				}
			}
		}

		public ReconOriginalDeclaration Find(ZGuid entrySummaryEntryPK)
		{
			ReconOriginalDeclaration result = null;
			if (entrySummaryEntryPK.IsValid && !DeclarationsByEntrySummaryPK.TryGetValue(entrySummaryEntryPK, out result))
			{
				result = AddNew(entrySummaryEntryPK);
			}
			return result;
		}

		ReconOriginalDeclaration AddNew(ZGuid entrySummaryEntryPK)
		{
			ReconOriginalDeclaration result = null;
			if (!DeclarationsByEntrySummaryPK.TryGetValue(entrySummaryEntryPK, out result))
			{
				var entryRow = (IColumnIndexer)RowFactory.LoadFromPK(CusEntryHeaderSchema.Constants.TableName, entrySummaryEntryPK);
				if (entryRow != null)
				{
					var declarationRow = (IColumnIndexer)RowFactory.LoadFromPK(JobDeclarationSchema.Constants.TableName, entryRow.GetValue(CusEntryHeaderSchema.CH_JE));
					if (declarationRow != null)
					{
						result = new ReconOriginalDeclaration(Factory, declarationRow, entryRow);
						DeclarationsByEntrySummaryPK.Add(entrySummaryEntryPK, result);
					}
				}
			}
			return result;
		}

		Dictionary<ZGuid, ReconOriginalDeclaration> DeclarationsByEntrySummaryPK
		{
			get { return declarationsByEntrySummaryPK ?? (declarationsByEntrySummaryPK = new Dictionary<ZGuid, ReconOriginalDeclaration>()); }
		}
		Dictionary<ZGuid, ReconOriginalDeclaration> declarationsByEntrySummaryPK;
	}
}

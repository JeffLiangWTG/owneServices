using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgHeaderFetchStrategy(OrgHeader orgHeader)
			: base(orgHeader)
		{
		}

		OrgHeader OrgHeader
		{
			get { return BusinessObject as OrgHeader; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			var currentCompany = GlbCompany.CurrentCompany;

			if (currentCompany != null)
			{
				Factory.AddFetchHint(new CompanyDataFetchHint(OrgHeader, currentCompany));
			}
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			if (OrgHeader.PatternMatchRequiresRegen)
			{
				Factory.AddFetchHint(OrgPatternMatchSchema.OS_OH, OrgHeader.PK);
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			bool mainAddressHinted = false;
			bool miscServHinted = false;
			bool stmNoteHinted = false;

			foreach (TableColumn tc in columns)
			{
				var name = tc.ColumnName;
				if (!mainAddressHinted && name.StartsWith("MainAddress"))
				{
					Factory.AddFetchHint(typeof(OrgAddress), OrgAddressSchema.OA_OH, BusinessObject.PK);
					mainAddressHinted = true;
				}
				else if (!miscServHinted && name.StartsWith("MiscServ"))
				{
					Factory.AddFetchHint(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, BusinessObject.PK);
					miscServHinted = true;
				}
				else if (name == "MainWebURL")
				{
					Factory.AddFetchHint(typeof(OrgWebURL), OrgWebURLSchema.PU_OH, BusinessObject.PK);
				}
				else if (!stmNoteHinted && (name == "AdditionalInformation" || name == "ExportersBankAccount" || name == "ExportersBankName" || name == "ExportersSwiftCode" || name == "MethodOfPayment"))
				{
					Factory.AddFetchHint(typeof(StmNote), StmNoteSchema.ST_ParentID, BusinessObject.PK);
					stmNoteHinted = true;
				}
			}
		}

		class CompanyDataFetchHint : IFetchHint
		{
			public CompanyDataFetchHint(OrgHeader header, GlbCompany currentCompany)
			{
				Argument.NotNull(header, nameof(header));
				Argument.NotNull(currentCompany, nameof(currentCompany));

				this.currentCompanyPK = currentCompany.PK;
				this.headerPK = header.PK;
			}

			readonly ZGuid headerPK;
			readonly ZGuid currentCompanyPK;

			#region IFetchHint Members

			void IFetchHint.GenerateQuery(QueryBuilder builder)
			{
				if (builder.IsEmpty)
				{
					builder.Init(new ZQuery(OrgCompanyDataSchema.OB_GC, currentCompanyPK), OrgCompanyDataSchema.OB_OH);
				}

				builder.AddValue(headerPK);
			}

			ZQuery IFetchHint.GetQuery()
			{
				return GetQuery();
			}

			ZQuery GetQuery()
			{
				ZQuery result = new ZQuery(OrgCompanyDataSchema.OB_OH, headerPK);
				result.AddToFilter(OrgCompanyDataSchema.OB_GC, currentCompanyPK);
				return result;
			}

			string IFetchHint.BuilderKey
			{
				get { return "OrganisationCompanyDataFetchHint" + currentCompanyPK.ToStringKey(); }
			}

			IQueryHashKey IFetchHint.GetHashKeyObject()
			{
				return new FetchHint.EnumerableHashObject { headerPK, currentCompanyPK };
			}

			bool IFetchHint.IsDataHintLoaded
			{
				get { return fIsDataHintLoaded; }
				set { fIsDataHintLoaded = value; }
			}

			bool fIsDataHintLoaded;

			bool IFetchHint.IsNeeded(QueryHistoryProvider historyProvider)
			{
				return !historyProvider.IsQueryCached(TableName, GetQuery());
			}

			IEnumerable<SchemaColumn> IFetchHint.LoadWithBlobs
			{
				get { return System.Array.Empty<SchemaColumn>(); }
			}

			public string TableName
			{
				get { return OrgCompanyDataSchema.Constants.TableName; }
			}

			#endregion
		}
	}
}

using System;
using System.Collections;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	[ModuleID(ModuleId.JobConsol)]
	public class ForwardingModuleConsolCollection : BusinessObjectCollection<ForwardingModuleConsol>, IExternalListValidation
	{
		public ForwardingModuleConsolCollection(BusinessObjectFactory factory) : base(factory)
		{
			ConsolDomainService.GetInstance(factory).ModuleConsolCollection = this;
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new ForwardingModuleConsolCollectionFetchStrategy(this);
		}

		#region Cus Entry Numbers

		public class ModuleEntryNum
		{
			public ModuleEntryNum(ZString number, ZString type, ZString status)
			{
				EntryNum = number;
				EntryNumType = type;
				EntryNumStatus = status;
			}

			public readonly ZString EntryNum;
			public readonly ZString EntryNumType;
			public readonly ZString EntryNumStatus;
		}

		public void LoadCusEntryNumbers(ForwardingModuleConsol consol)
		{
			ForwardingModuleConsol[] cusEntryNumConsols = GetLookAheadArray(consol);
			StringBuilder sqlText = new StringBuilder();

			sqlText.Append("SELECT " + JobConsolSchema.Constants.PK + ", ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryNum + ", ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryType + ", ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryStatus + " FROM ");
			sqlText.Append(CusEntryNumSchema.Constants.TableName + " JOIN ");
			sqlText.Append(JobConsolSchema.Constants.TableName + " ON ");
			sqlText.Append(JobConsolSchema.Constants.PK + " = ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_ParentID + " WHERE ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_RN_NKCountryCode + (NoResString)" = @CurrentCompanyCountryCode AND "); // May be a part of SQL expression
			sqlText.Append(JobConsolSchema.Constants.PK + (NoResString)" IN (SELECT Value FROM @Guids)"); // May be a part of SQL expression.

			using (DbCommand command = Db.Connection.Command(sqlText.ToString())) // Can not access the DB for every row it is too slow.  This gets a range of entry numbers for all visible records.
			{
				command.AddParameterBasedOnDbColumn("@CurrentCompanyCountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString(), CusEntryNumSchema.CE_RN_NKCountryCode);
				command.AddTableValuedParameter("@Guids", JobConsolSchema.PK, cusEntryNumConsols.Select(x => x.PK));
				using (var dr = command.ExecuteReader())
				{
					while (dr.Read())
					{
						ZGuid pK = (Guid)dr[0];
						ZString entryNum = dr[1].ToString();
						ZString entryType = CusEntryNumber.GetEntryNumberTypeForDisplay(dr[2].ToString(), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, false);
						ZString entryStatus = dr[3].ToString();
						CusEntryNums[pK] = new ModuleEntryNum(entryNum, entryType, entryStatus);
					}
				}
			}

			foreach (ForwardingModuleConsol ensureLoaded in cusEntryNumConsols)
			{
				if (CusEntryNums[ensureLoaded.PK] == null)
				{
					CusEntryNums[ensureLoaded.PK] = new ModuleEntryNum("", "", "");
				}
			}
		}

		public ZString GetEntryStatus(ForwardingModuleConsol consol)
		{
			ZString result = "";
			ModuleEntryNum entryNumRecord = GetEntryNumRecord(consol);
			if (entryNumRecord != null)
			{
				result = entryNumRecord.EntryNumStatus;
			}
			return result;
		}

		protected ModuleEntryNum GetEntryNumRecord(ForwardingModuleConsol consol)
		{
			ModuleEntryNum result = (ModuleEntryNum)CusEntryNums[consol.PK];
			if (result == null)
			{
				LoadCusEntryNumbers(consol);
				result = (ModuleEntryNum)CusEntryNums[consol.PK];
			}
			return result;
		}

		protected const int ReadAheadCache = 100;
		protected ForwardingModuleConsol[] GetLookAheadArray(ForwardingModuleConsol consol)
		{
			ArrayList lookAheadList = new ArrayList(ReadAheadCache);
			lookAheadList.Add(consol);
			int currentIndex = Elements.IndexOf(consol);
			if (currentIndex > -1)
			{
				int stopIndex = currentIndex + ReadAheadCache * 2;
				if (stopIndex > Count)
				{
					stopIndex = Count;
				}

				while (currentIndex < stopIndex)
				{
					ForwardingModuleConsol lookAhead = this[currentIndex];
					if (!ConsolCached(lookAhead))
					{
						lookAheadList.Add(lookAhead);
						if (lookAheadList.Count >= ReadAheadCache)
						{
							break;
						}
					}
					currentIndex++;
				}
			}
			return (ForwardingModuleConsol[])lookAheadList.ToArray(typeof(ForwardingModuleConsol));
		}

		protected bool ConsolCached(ForwardingModuleConsol consol)
		{
			return CusEntryNums[consol.PK] != null;
		}

		Hashtable fCusEntryNums;
		protected Hashtable CusEntryNums
		{
			get
			{
				if (fCusEntryNums == null)
				{
					fCusEntryNums = new Hashtable();
				}
				return fCusEntryNums;
			}
		}

		#endregion

		#region Template Records

		public bool AllowTemplateRecords { get; set; }

		protected override IFindBoxListProvider FindBoxListProvider => AllowTemplateRecords
			? new TemplateRecordFindboxListProvider(this)
			: base.FindBoxListProvider;

		bool IExternalListValidation.IsValidTemplateRecordPK(ZGuid pk)
		{
			if (AllowTemplateRecords)
			{
				var templateRecord = Factory.Load<StmTemplateRecord>(pk);
				return templateRecord != null && templateRecord.STR_ModuleID == nameof(ModuleId.JobConsol);
			}

			return false;
		}

		#endregion
	}
}

using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[SingleObjectAroundARow]
	public class CusStatementHeader : BaseCusStatementHeader
	{
		#region Loader

		public new class Loader : AutoCusStatementHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusStatementHeader Load(ZString accountNo, ZString customsOffice, ZString agentCode, ZDateTime processDate, ZDateTime dueDate)
			{
				var cusStatementHeaderFilter = CreateNewCusStatementHeaderFilter(accountNo, customsOffice, agentCode, processDate, dueDate);
				return Factory.LoadTop1<CusStatementHeader>(cusStatementHeaderFilter);
			}

			ZQuery CreateNewCusStatementHeaderFilter(ZString accountNo, ZString customsOffice, ZString agentCode, ZDateTime processDate, ZDateTime dueDate)
			{
				var cusStatementHeaderFilter = new ZQuery();
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_AccountNo, accountNo);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_ProcessDate, processDate);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_ProcessPort, customsOffice);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_EntryFilerCode, agentCode);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_DueDate, dueDate);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
				return cusStatementHeaderFilter;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusStatementHeader);
			}
		}

		#endregion

		public CusStatementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Collections

		[ChildEditable(false)]
		public CusStatementLineCollection StatementLines
		{
			get
			{
				if (fStatementLine == null)
				{
					fStatementLine = new CusStatementLineCollection(this);
					RegisterEditableChildObject(fStatementLine);
				}
				return fStatementLine;
			}
		}
		CusStatementLineCollection fStatementLine;

		#endregion
	}
}

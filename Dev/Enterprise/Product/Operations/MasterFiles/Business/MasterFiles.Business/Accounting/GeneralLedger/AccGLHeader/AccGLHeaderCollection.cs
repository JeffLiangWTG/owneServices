using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccGLHeader)]
	public partial class AccGLHeaderCollection : BusinessObjectCollection<AccGLHeader>
	{
		public AccGLHeaderCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{
		}

		public AccGLHeaderCollection(BusinessObjectFactory factory, ZQuery query, Action<AccGLHeaderCollection, List<AccGLHeader>> glAccountAction) : base(factory, query)
		{
			ShowGLAccountsForImportAction = glAccountAction;
		}

		public AccGLHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccGLHeaderCollection(BusinessObjectFactory factory, AccTransactionLines transactionLines, Action<AccGLHeaderCollection, List<AccGLHeader>> glAccountAction = null) : base(factory)
		{
			ShowGLAccountsForImportAction = glAccountAction;
			TransactionLines = transactionLines;
		}

		readonly AccTransactionLines TransactionLines;

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new AccGLHeaderFindBoxListProvider(this, TransactionLines, ShowGLAccountsForImportAction); }
		}

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		#endregion
	}
}

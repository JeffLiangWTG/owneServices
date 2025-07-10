using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class StatementPaymentActionCollection : NonPersistentBusinessObjectCollection<StatementPaymentAction>
	{
		public StatementPaymentActionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Users cannot create a new element in the grid.");
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public void PopulateStatementHeaderElements(ZGuid[] statementPKs)
		{
			if (statementPKs != null)
			{
				var statements = Factory.Load<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.PK, statementPKs));
				foreach (var statement in statements)
				{
					Add(new StatementPaymentAction(statement));
				}
			}
		}
	}
}

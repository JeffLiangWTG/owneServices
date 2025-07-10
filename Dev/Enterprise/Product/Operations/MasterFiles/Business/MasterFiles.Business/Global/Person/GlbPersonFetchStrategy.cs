using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public GlbPersonFetchStrategy(GlbPerson person) : base(person)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				if (column.ColumnName == GlbPerson.Schema.PER_FaxNum_IsManuallyVerified
					|| column.ColumnName == GlbPerson.Schema.PER_HomePhone_IsManuallyVerified
					|| column.ColumnName == GlbPerson.Schema.PER_MobilePhone_IsManuallyVerified)
				{
					Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				}
				else if (column.ColumnName.StartsWith((NoResString)"Primary", StringComparison.Ordinal))
				{
					Factory.AddFetchHint(typeof(GlbPersonPrimaryRelationship), GlbPersonPrimaryRelationshipSchema.PPR_PER, BusinessObject.PK);
				}
			}
		}
	}
}

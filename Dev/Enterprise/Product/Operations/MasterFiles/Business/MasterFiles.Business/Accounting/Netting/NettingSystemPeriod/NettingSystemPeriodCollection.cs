using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.NettingPeriod)]
	public class NettingSystemPeriodCollection : ActiveBusinessObjectCollection<NettingSystemPeriod>
	{
		public NettingSystemPeriodCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NettingSystemPeriodCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public NettingSystemPeriodCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			if (Globals.IsTest)
			{
				return null;
			}
			var nettingSystemSubQuery = new ZDBOnlySubQuery(typeof(NettingSystem), NettingSystemSchema.PK);
			nettingSystemSubQuery.AddToFilter(NettingSystemSchema.NS_GC, GlbCompany.CurrentCompany.PK.ToGuid());

			var nettingSystemPeriodQuery = new ZDBOnlyQuery(typeof(NettingSystemPeriod));
			nettingSystemPeriodQuery.AddSubQuery(NettingSystemPeriodSchema.NSP_NS_NettingSystem, nettingSystemSubQuery, JoinCondition.And);

			return nettingSystemPeriodQuery;
		}
	}
}

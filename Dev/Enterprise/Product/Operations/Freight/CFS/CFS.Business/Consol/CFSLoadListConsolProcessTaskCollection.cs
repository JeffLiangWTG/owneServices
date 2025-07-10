using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolProcessTaskCollection : ProcessTaskCollection
	{
		public CFSLoadListConsolProcessTaskCollection(CFSLoadListConsol loadList)
			: base(loadList)
		{
		}

		public new CFSLoadListConsol Parent
		{
			get { return (CFSLoadListConsol)base.Parent; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.IsNoResultQuery = Parent.JK_IsForwarding;
			return query;
		}

		public new CFSLoadListConsolProcessTask this[int index]
		{
			get { return (CFSLoadListConsolProcessTask)Elements[index]; }
		}

		public new CFSLoadListConsolProcessTask AddNew()
		{
			return (CFSLoadListConsolProcessTask)base.AddNew();
		}

		public override ZString OriginCountry
		{
			get { return Parent.LoadPort == null ? ZString.Empty : Parent.LoadPort.RL_RN_NKCountryCode; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.DischargePort == null ? ZString.Empty : Parent.DischargePort.RL_RN_NKCountryCode; }
		}
	}
}

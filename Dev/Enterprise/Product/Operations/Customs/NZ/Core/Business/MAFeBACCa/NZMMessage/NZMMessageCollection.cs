using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa
{
	public class NZMMessageCollection : EDIMessageCollection
	{
		public NZMMessageCollection(MAFMessagingBO parent)
			: base(parent.PlugInSupport.Master)
		{
		}

		public new NZMMessage this[int index]
		{
			get { return (NZMMessage)Elements[index]; }
		}

		public virtual new NZMMessage AddNew()
		{
			return (NZMMessage)base.AddNew();
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.NZMAFeBACCa);
			return filter;
		}
	}
}

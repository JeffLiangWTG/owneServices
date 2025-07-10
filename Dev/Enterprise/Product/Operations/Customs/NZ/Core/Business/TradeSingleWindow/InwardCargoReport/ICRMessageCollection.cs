using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public class ICRMessageCollection : EDIMessageCollection
	{
		public ICRMessageCollection(ForwardingConsol consol)
			: base(consol, consol.Factory)
		{
		}

		public ICRMessageCollection(JobDeclaration declaration)
			: base(declaration, declaration.Factory)
		{
		}

		public new ICRMessage this[int index]
		{
			get { return (ICRMessage)Elements[index]; }
		}

		public new ICRMessage AddNew()
		{
			return (ICRMessage)base.AddNew();
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.NZCustoms);
			return filter;
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V3.Business
{
	public class V3MessagesCollection : ActiveBusinessObjectCollection<V3Message>
	{
		public V3MessagesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}

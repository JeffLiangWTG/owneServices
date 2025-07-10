using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V3.Business
{
	public class V3JobDeclarationCollection : ActiveBusinessObjectCollection<V3JobDeclaration>
	{
		public V3JobDeclarationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}

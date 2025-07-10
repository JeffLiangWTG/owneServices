using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFDocAddressCollection : JobDocAddressCollection
	{
		public ISFDocAddressCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new ISFDocAddress this[int index]
		{
			get { return (ISFDocAddress)base[index]; }
		}

		public new ISFDocAddress AddNew()
		{
			return (ISFDocAddress)base.AddNew();
		}
	}
}

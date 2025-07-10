using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class NonCharterSailingCollection : CharterStateSpecificCollection
	{
		public NonCharterSailingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZBool IsCharter
		{
			get { return false; }
		}

		protected override ZString ErrorFormat
		{
			get { return Res.GetString("18167d4e-b692-48cc-8f3a-adbacfa3d9d6", "Only a non chartered {0:G} may be selected"); }
		}
	}
}

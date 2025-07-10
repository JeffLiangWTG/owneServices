using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class CharterSailingCollection : CharterStateSpecificCollection
	{
		public CharterSailingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZBool IsCharter
		{
			get { return true; }
		}

		protected override ZString ErrorFormat
		{
			get { return Res.GetString("27abaf38-0db4-4547-a4a0-a9e0f45535fb", "Only a chartered {0:G} may be selected"); }
		}
	}
}

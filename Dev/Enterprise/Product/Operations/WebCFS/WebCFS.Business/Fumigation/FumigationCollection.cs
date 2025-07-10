
using CargoWise.EntityFramework;

namespace Enterprise.WebCFS.Business
{
	/// <summary>
	/// Summary description for FumigationCollection.
	/// </summary>
	public class FumigationCollection : BusinessObjectCollection<Fumigation>
	{
		public FumigationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}

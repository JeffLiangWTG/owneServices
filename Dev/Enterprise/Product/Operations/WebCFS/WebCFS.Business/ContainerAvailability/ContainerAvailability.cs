using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.WebCFS.Business
{
	/// <summary>
	/// Business object to be displayed in Container availability grid
	/// </summary>
	public class ContainerAvailability : Autovw_List_ContainerAvailability
	{
		public ContainerAvailability(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}

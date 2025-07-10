using System.Web.UI;

using Enterprise.Registry.Business.Web;

namespace Enterprise.Tracking.Web
{
	interface IAccessControllablePage
	{
		IAccessControlled dataSource { get; }
		Control[] GetControls(string caption);
	}
}

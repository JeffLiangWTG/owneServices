using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactNameChangeService : IService
	{
		#region Static

		public static OrgContactNameChangeService GetInstance(BusinessObjectFactory factory)
		{
			var result = factory.ServiceContainer.GetService<OrgContactNameChangeService>();

			if (result == null)
			{
				result = new OrgContactNameChangeService();
				factory.ServiceContainer.AddService(result);
			}

			return result;
		}

		#endregion

		#region Constructor

		OrgContactNameChangeService()
		{
		}

		#endregion

		public void NotifyContactNameChanged(OrgContact contact)
		{
			if (ContactNameChanged != null)
			{
				ContactNameChanged(contact, EventArgs.Empty);
			}
		}

		public event EventHandler ContactNameChanged;
	}
}

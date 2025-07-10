using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPickMethodsWebServiceResponse : WebServiceResponse
	{
		#region Properties

		public WhsPickMethodInfoCollection PickMethods
		{
			get
			{
				if (pickMethods == null)
				{
					pickMethods = new WhsPickMethodInfoCollection();
				}
				return pickMethods;
			}
			set
			{
				pickMethods = value;
			}
		}

		#endregion

		#region Implementation

		WhsPickMethodInfoCollection pickMethods;

		#endregion
	}
}

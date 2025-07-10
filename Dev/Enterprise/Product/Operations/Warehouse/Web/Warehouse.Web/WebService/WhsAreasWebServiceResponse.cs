using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsAreasWebServiceResponse : WebServiceResponse
	{
		#region Properties

		public WhsAreaInfoCollection Areas
		{
			get
			{
				if (areas == null)
				{
					areas = new WhsAreaInfoCollection();
				}
				return areas;
			}
			set
			{
				areas = value;
			}
		}

		#endregion

		#region Implementation

		WhsAreaInfoCollection areas;

		#endregion
	}
}

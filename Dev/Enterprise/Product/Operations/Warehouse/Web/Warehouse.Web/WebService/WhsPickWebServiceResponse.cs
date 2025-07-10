using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPickWebServiceResponse : WebServiceResponse
	{
		#region Properties

		public WhsPickInfo Pick
		{
			get
			{
				if (pick == null)
				{
					pick = new WhsPickInfo();
				}
				return pick;
			}
			set
			{
				pick = value;
			}
		}

		public Guid TaskPK { get; set; }

		#endregion

		#region Implementation

		WhsPickInfo pick;

		#endregion
	}
}

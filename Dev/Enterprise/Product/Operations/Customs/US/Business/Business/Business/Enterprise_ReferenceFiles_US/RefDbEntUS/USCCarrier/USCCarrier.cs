using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business.RefDbEntUS
{
	public class USCCarrier : AutoUSCCarrier
	{
		#region Constructors

		public USCCarrier(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion
	}
}

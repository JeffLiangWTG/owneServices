using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.WebCFS.Business
{
	public class FumigationFilterBusinessObject : AutoFumigationFilterBusinessObject
	{
		public FumigationFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LFV_ContainerNum = ZString.Empty;
			LFV_VoyageFlight = ZString.Empty;
			LFV_Vessel = ZString.Empty;
			FromDate = ZDateTime.Empty;
			ToDate = ZDateTime.Empty;
		}
	}
}

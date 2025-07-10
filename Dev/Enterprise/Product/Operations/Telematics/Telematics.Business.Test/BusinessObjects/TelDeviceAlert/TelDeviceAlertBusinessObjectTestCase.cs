using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelDeviceAlert))]
	public class TelDeviceAlertBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (TelDeviceAlert)base.GetNewBusinessObjectForDeleteTest(factory);
			o.TDA_Occurrences = 1;
			o.TDA_Type = "ASD";
			return o;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var o = (TelDeviceAlert)base.GetBusinessObjectForFetchForLoad();
			o.TDA_Occurrences = 1;
			o.TDA_Type = "ASD";
			return o;
		}
	}
}

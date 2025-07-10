using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelEdge))]
	class TelEdgeBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (TelEdge)base.GetNewBusinessObjectForDeleteTest(factory);
			o.TE_EntityTableCodeFrom = "RQ";
			o.TE_EntityTableCodeTo = "TSE";
			return o;
		}
	}
}

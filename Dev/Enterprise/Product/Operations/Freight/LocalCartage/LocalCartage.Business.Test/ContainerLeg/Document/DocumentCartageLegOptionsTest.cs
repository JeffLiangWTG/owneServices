using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(DocumentCartageLegOptions))]
	public class DocumentCartageLegOptionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DocumentCartageLegCollection legs = new DocumentCartageLegCollection(new CommonCartageLegCollection(Factory));
			return new DocumentCartageLegOptions(legs);
		}
	}
}

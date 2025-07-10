using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ViewValueAnalysisCollection))]
	sealed class ViewValueAnalysisCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			return new ViewValueAnalysisCollection(Factory);
		}

		#endregion
	}
}

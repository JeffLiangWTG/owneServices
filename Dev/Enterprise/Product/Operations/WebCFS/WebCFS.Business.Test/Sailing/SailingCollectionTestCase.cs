using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business.Testing
{
	[TestedType(typeof(SailingCollection))]
	public class SailingCollectionTestCase : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected SailingCollection TestCollection
		{
			get { return (SailingCollection)Collection; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(Sailing));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SailingCollection(WebFactory);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected BusinessObjectFactory WebFactory
		{
			get { return Factory; }
		}

		#endregion Implementation
	}
}

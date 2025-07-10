using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business.Testing
{
	[TestedType(typeof(FumigationCollection))]
	public class FumigationCollectionTestCase : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected FumigationCollection TestCollection
		{
			get { return (FumigationCollection)Collection; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(Fumigation));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FumigationCollection(WebFactory);
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

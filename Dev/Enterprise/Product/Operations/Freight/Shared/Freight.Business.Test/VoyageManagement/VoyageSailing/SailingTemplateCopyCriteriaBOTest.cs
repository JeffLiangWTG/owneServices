using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SailingTemplateCopyCriteria))]
	sealed class SailingTemplateCopyCriteriaBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return new SailingTemplateCopyCriteria(voyage);
		}

		#endregion
	}
}

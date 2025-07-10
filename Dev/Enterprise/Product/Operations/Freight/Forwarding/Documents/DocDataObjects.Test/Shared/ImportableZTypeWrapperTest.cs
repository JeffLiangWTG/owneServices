using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(ImportableZTypeWrapper<ZString>))]
	class ImportableZTypeWrapperTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportableZTypeWrapper<ZString>(ZString.Empty);
		}

		#endregion
	}
}

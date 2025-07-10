using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentRePrintRestrictionCollection))]
	sealed class ComplianceDocumentRePrintRestrictionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceDocumentRePrintRestrictionCollection>
	{
		#region Implementation

		protected override ComplianceDocumentRePrintRestrictionCollection GetCollectionToTest()
		{
			return new ComplianceDocumentRePrintRestrictionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceDocumentRePrintRestriction();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}

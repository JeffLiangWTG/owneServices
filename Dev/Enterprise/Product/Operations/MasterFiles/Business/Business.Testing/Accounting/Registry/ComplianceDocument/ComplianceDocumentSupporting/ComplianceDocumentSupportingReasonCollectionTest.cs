using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentSupportingReasonCollection))]
	sealed class ComplianceDocumentSupportingReasonCollectionTest : RegistryBusinessObjectCollectionTestCase<ComplianceDocumentSupportingReasonCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ComplianceDocumentSupportingReasonCollection GetCollectionToTest()
		{
			return new ComplianceDocumentSupportingReasonCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceDocumentSupportingReason();
		}

		#endregion
	}
}

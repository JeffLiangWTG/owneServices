using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceDocumentImageCollection))]
	sealed class ComplianceDocumentImageCollectionTest : RegistryBusinessObjectCollectionTestCase<ComplianceDocumentImageCollection>
	{
		#region Implementation

		protected override ComplianceDocumentImageCollection GetCollectionToTest()
		{
			return new ComplianceDocumentImageCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceDocumentImage
			{
				ImagePkForTest = ZGuid.NewZGuid(),
				Remark = nextRemark++.ToString()
			};
		}

		int nextRemark;

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

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassRequiredDocument))]
	sealed class GatePassRequiredDocumentTest : EnterpriseBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<GatePassShipment>();
			var docsAndCartage = shipment.DocsAndCartage;

			var result = docsAndCartage.RequiredDocuments.AddNew();
			result.EQ_ParentTableCode = docsAndCartage.TablePrefix;

			return result;
		}

		protected override void InstallBizoForTestDbHits(BusinessObject bizo)
		{
			var document = bizo as GatePassRequiredDocument;
			if (document != null)
			{
				document.ParentType = typeof(GatePassDocsAndCartage);
			}
		}
	}
}

using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitDocumentSupporter))]
	public class CYDTransportationUnitDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			Assert(DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CYDTransportationUnit)));
		}

		public void TestGetDocBusinessObject()
		{
			var wrappers = DocumentSupporter.GetDocumentWrappers(DataContext.CYDTransportationUnit, null);
			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CYDTransportUnit, DocumentSupporter.BusinessContext);
		}

		public void TestGetContactOrganisation()
		{
			var tpu = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var jda = Factory.New<JobDocAddress>();
			var transportOrg = Factory.New<OrgHeader>();
			jda.E2_AddressType = DocAddressTypes.Codes.TransportCompanyDocumentaryAddress;
			jda.E2_ParentID = tpu.PK;
			jda.E2_ParentTableCode = tpu.TablePrefix;
			jda.E2_OA_Address = transportOrg.MainAddress.PK;
			AssertEquals(transportOrg, new CYDTransportationUnitDocumentSupporter(tpu).GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter => new CYDTransportationUnitDocumentSupporter((CYDTransportationUnit)GetDocumentSupportableBusinessObject());

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDTransportationUnit>();
		}

		#endregion
	}
}

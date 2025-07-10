using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class TripAssemblyDataTest : TestCaseWithFactory
	{
		public void TestOverrides()
		{
			var data = new TripAssemblyData();
			AssertEquals("BusinessObjectType", typeof(Trip), data.BusinessObjectType);
			var businessObjectCollection = data.GetBusinessObjectCollection(Factory);
			AssertNotNull(businessObjectCollection);
			AssertEquals("CollectionType", typeof(TripCollection), businessObjectCollection.GetType());
			AssertEquals("ModuleID", ModuleIDs.Customs.US.eManifest, data.ModuleID);
			AssertEquals("ReferenceType", Constants.ReferenceTypes.SupplyChainLogistics, data.ReferenceType);
			AssertEquals("HumanReadableName", "e-Manifest", data.HumanReadableName);
			AssertEquals("IsAllowedForUnallocatedeDocs", true, data.IsAllowedForUnallocatedeDocs);
		}

		public void TestRetriveEDocFromTrip()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var trip = Factory.New<Trip>();
				trip.BH_JobReference = "MAN0000427";
				var docManagerInfo = ((IDocManagerSupport)trip).DocManagerInfo;

				var eDoc = CreateEDocForTest(docManagerInfo, "File1", "INV", true, new ZDateTime(2023, 10, 23, 12, 15, 00));
				docManagerInfo.MasterFactory.Save();
				Factory.Save();
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var requestXml =
$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<DocumentRequest>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>DocManager</Type>
					<Key>MAN MAN0000427</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
			</Company>
			<EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
			<ServerID>{registrationKey.ServerCode}</ServerID>
		</DataContext>
	</DocumentRequest>
</UniversalDocumentRequest>
			";
				var eAdaptorMessageSender = ObjectFactory.Get<IEAdaptorSupportMessageSender>();
				var documentRequestResult = eAdaptorMessageSender.Send(requestXml);
				Assert("Status", documentRequestResult.Contains("<Status>PRS</Status>"));
				Assert("File name", documentRequestResult.Contains($"<FileName>{eDoc.FileName}</FileName>"));
			}
		}

		IeDoc CreateEDocForTest(DocManagerInfo docManagerInfo, string fileName, string docType, bool isPublished, ZDateTime docDateTime, Func<ZBlob> blobFactory = null)
		{
			var eDoc = docManagerInfo.AddFileOrDocument(blobFactory?.Invoke() ?? new ZBlob(new byte[] { 1, 2, 3 }), fileName, docType);
			eDoc.IsPublished = isPublished;
			eDoc.SetValuesForTest(docDateTime, docType);

			return eDoc;
		}
	}
}

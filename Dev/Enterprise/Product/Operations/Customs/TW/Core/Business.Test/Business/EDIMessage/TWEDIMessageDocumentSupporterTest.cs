using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWEDIMessageDocumentSupporter))]
	sealed class TWEDIMessageDocumentSupporterBaseOnlyTest : TWEDIMessageDocumentSupporterTest<TWMessageDocumentSupporter>
	{
	}

	[TestsSubclassesOf(typeof(TWEDIMessageDocumentSupporter))]
	abstract class TWEDIMessageDocumentSupporterTest<T> : DocumentSupporterTest
		where T : TWMessageDocumentSupporter
	{
		[ExpectNoExceptions]
		public void TestTransportMode()
		{
			var message = Factory.NewWithValidTestData<T>();
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			var documentSupporter = ((IDocumentSupportable)message).DocumentSupporter;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			NUnit.Framework.Assert.That(documentSupporter.TransportMode, NUnit.Framework.Is.EqualTo(Core.Constants.TransportModes.Air));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			NUnit.Framework.Assert.That(documentSupporter.TransportMode, NUnit.Framework.Is.EqualTo(Core.Constants.TransportModes.Sea));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Truck;
			NUnit.Framework.Assert.That(documentSupporter.TransportMode, NUnit.Framework.Is.EqualTo(Core.Constants.TransportModes.Truck));
		}

		[ExpectNoExceptions]
		public void TestGetContactOrganisation()
		{
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importerOrg.PK;
			declaration.JE_OH_Supplier = supplierOrg.PK;

			var message = Factory.NewWithValidTestData<T>();
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Factory.Save();

			var documentSupporter = ((IDocumentSupportable)message).DocumentSupporter;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(documentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignee, DocumentDirection.ANY).OrgHeader, NUnit.Framework.Is.EqualTo(importerOrg).Using(CustomComparers.TypeComparison), "GetContactOrganisation(Consignee) returning the Importer");
				NUnit.Framework.Assert.That(documentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Consignor, DocumentDirection.ANY).OrgHeader, NUnit.Framework.Is.EqualTo(supplierOrg).Using(CustomComparers.TypeComparison), "GetContactOrganisation(Consignor) returning the Supplier");
				NUnit.Framework.Assert.That(documentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY), NUnit.Framework.Is.EqualTo(default(Enterprise.DocumentEngineCore.IDocumentDeliveryContact)), "GetContactOrganisation(LocalClient) returning null - should be [null]");
			});
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<T>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}

using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	[TestedType(typeof(UniversalCustomsDataObjectProvider))]
	sealed class UniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestTableSpecificCusSupportingInfoTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var supportingInfoJI = provider.TableSpecificCusSupportingInfoTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			CombineAssertions("JI", () =>
			{
				NUnit.Framework.Assert.That(supportingInfoJI.Count, Is.EqualTo(4), "Count");
				NUnit.Framework.Assert.That(supportingInfoJI.ToCodeStrings(), Is.EqualTo("CPC, ECA, COO, PBN"), "CodeStrings");
			});
		}

		[ExpectNoExceptions]
		public void TestTableSpecificCusAddInfoTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var addinfoTypeCEI = provider.TableSpecificCusAddInfoTypeList("CEI");
			NUnit.Framework.Assert.That(addinfoTypeCEI.Count, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestTableSpecificCusCodeDataTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var list = provider.TableSpecificCusCodeDataTypeList("CEI");
			NUnit.Framework.Assert.That(list.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.ContainsCode(CusCodeDataTypeList.Codes.DeclarationDuplicate), Is.True);
			list = provider.TableSpecificCusCodeDataTypeList("JE");
			NUnit.Framework.Assert.That(list.Count, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestTableSpecificCusCodeDataCodeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var list = provider.TableSpecificCusCodeDataCodeList("CEI");
			NUnit.Framework.Assert.That(list.Count, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestGetNewJobDeclarationDataObjectReader()
		{
			var provider = new UniversalCustomsDataObjectProvider();
			var reader = provider.GetNewJobDeclarationDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory, null);
			NUnit.Framework.Assert.That(reader is TWJobDeclarationDataObjectReader, Is.True);
		}

		[ExpectNoExceptions]
		public void TestGetNewDeclarationDataObjectWriter()
		{
			NUnit.Framework.Assert.That(new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>()))), Is.TypeOf(typeof(TWJobDeclarationDataObjectWriter)));
		}

		[ExpectNoExceptions]
		public void TestGetNewStandaloneCommercialInvoiceDataObjectReader()
		{
			NUnit.Framework.Assert.That(new UniversalCustomsDataObjectProvider().GetNewStandaloneCommercialInvoiceDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new CommercialInvoiceHeader(), new TestErrorLogger(), Factory), Is.TypeOf(typeof(StandaloneCommercialInvoiceDataObjectReader)));
		}

		[ExpectNoExceptions]
		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			NUnit.Framework.Assert.That(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""), Is.EqualTo(default(CargoWise.Integration.ICodeDescriptionPairList)));
		}

		[ExpectNoExceptions]
		public void TestTableSpecificCusReferenceTypeList()
		{
			NUnit.Framework.Assert.That(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty), Is.EqualTo(default(CargoWise.Integration.ICodeDescriptionPairList)));
		}

		[ExpectNoExceptions]
		public void TestGetNewStandaloneCommercialInvoiceDataObjectWriter()
		{
			NUnit.Framework.Assert.That(new UniversalCustomsDataObjectProvider().GetNewStandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobComInvoiceHeader>()))), Is.TypeOf(typeof(StandaloneCommercialInvoiceDataObjectWriter)));
		}
	}
}

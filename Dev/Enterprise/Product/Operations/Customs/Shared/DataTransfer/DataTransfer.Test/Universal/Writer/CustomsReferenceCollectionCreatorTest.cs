using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCusCodeDataMappings()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCusCodeDataSupporter = (ICusCodeDataTypeSupporter)invoiceLine;
			var feeString = "FEE";
			var feeMerchandiseProcessing = "499";
			Type feeTye = null;
			invoiceLineCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(feeString, out feeTye);
			var fee1 = (CusCodeData)Factory.New(feeTye);
			fee1.Parent = invoiceLine;
			fee1.CY_Code = feeMerchandiseProcessing;
			fee1.CY_Data = "100";
			fee1.CY_Order = 1;
			fee1.CY_Date = new ZDateTime(2017, 09, 01);
			var fee2 = (CusCodeData)Factory.New(feeTye);
			fee2.Parent = invoiceLine;
			fee2.CY_Code = feeMerchandiseProcessing;
			fee2.CY_Data = "200";
			fee2.CY_Order = 2;
			Factory.SaveForTesting();
			declaration.ResumeApportionment();
			var helper = new UniversalDataObjectWriterHelper(declaration.Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertNull(CustomsReferenceCollectionCreator.CreateCollection(helper, null, null));
			var collection = CustomsReferenceCollectionCreator.CreateCollection(helper, invoiceLine, null);
			AssertEquals(2, collection.Count);
			AssertContents(collection[0], GetCodeDescriptionPair(feeString, "Fee"), GetCodeDescriptionPair(feeMerchandiseProcessing, "499 Desc from DB"), "100", 1, ZBool.False, new ZDateTime(2017, 09, 01));
			AssertContents(collection[1], GetCodeDescriptionPair(feeString, "Fee"), GetCodeDescriptionPair(feeMerchandiseProcessing, "499 Desc from DB"), "200", 2, ZBool.False, null);
		}

		static void AssertContents(UniversalCustoms.CustomsReference cusCodeData, ICodeDescription type, ICodeDescription subType, ZString reference, ZInt order, ZBool isOverridden, ZDateTime? date = null)
		{
			AssertNotNull("Precondition: cusCodeData", cusCodeData);
			CombineAssertions(delegate
			{
				AssertNotNull("cusCodeData.Type", cusCodeData.Type);
				AssertEquals("cusCodeData.Type.Code", type.Code, cusCodeData.Type.Code);
				AssertEquals("cusCodeData.Type.Description", type.Description, cusCodeData.Type.Description);
				AssertNotNull("cusCodeData.SubType", cusCodeData.SubType);
				AssertEquals("cusCodeData.SubType.Code", subType.Code, cusCodeData.SubType.Code);
				AssertEquals("cusCodeData.SubType.Description", subType.Description, cusCodeData.SubType.Description);
				AssertEquals("cusCodeData.Reference", reference, cusCodeData.Reference);
				AssertEquals("cusCodeData.Order", order, cusCodeData.Order);
				AssertEquals("cusCodeData.IsOverridden", isOverridden, cusCodeData.IsOverridden);
				AssertEquals("cusCodeData.Date", date ?? ZDateTime.Empty, cusCodeData.DateCollection?.FirstOrDefault(x => x.Type.GetValueOrDefault() == UniversalShipment.DateType.DateAtOffice)?.Value.GetValueOrDefault() ?? ZDateTime.Empty);
			});
		}
	}
}

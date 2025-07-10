using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestBasicCusCodeDataLevelFieldMappings()
		{
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLineCusCodeDataSupporter = (ICusCodeDataTypeSupporter)invoiceLine;
			var feeTypeString = "FEE";
			var feeMerchandiseProcessing = "499";
			Assert(invoiceLineCusCodeDataSupporter.GetCusCodeDataTypes().TryGetValue(feeTypeString, out var type));
			var invoiceLineData = new UniversalCustoms.CommercialInvoiceLine()
			{
				CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
				{
					SetupCustomsReference2(feeTypeString, feeMerchandiseProcessing),
				})
			};
			var reader = new CustomsReferenceCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates));
			var cusCodeDataBOs = reader.ReadIntoDataRows(invoiceLine.PK, invoiceLine.TablePrefix, invoiceLine.IsInDatabase, invoiceLineData);

			AssertNotNull(cusCodeDataBOs);

			CombineAssertions(delegate
			{
				AssertEquals("cusCodeDataBOs", 1, cusCodeDataBOs.Length);
				var feeBO = cusCodeDataBOs[0];
				AssertCusCodeDataContents2(feeBO, invoiceLine.TablePrefix, invoiceLine.PK, feeTypeString, feeMerchandiseProcessing);
			});
		}

		//UniversalCustoms.CustomsReference SetupCustomsReference(ZString type, ZString code)
		//{
		//	return SetupCustomsReference(new CodeDescriptionPair() { Code = type }, new CodeDescriptionPair35Char() { Code = code }, "HOUSE", ZBool.False, 1, new ZDateTime(2017, 9, 1));
		//}

		UniversalCustoms.CustomsReference SetupCustomsReference2(ZString type, ZString code)
		{
			return SetupCustomsReference(new CodeDescriptionPair() { Code = type }, new CodeDescriptionPair35Char() { Code = code }, "HOME", ZBool.True, 2, null);
		}

		void AssertCusCodeDataContents2(IColumnIndexer cusCodeDataBO, ZString parentTableCode, ZGuid parentID, ZString type, ZString code)
		{
			AssertCusCodeDataContents(cusCodeDataBO, parentTableCode, parentID, type, code, "HOME", ZBool.True, 2);
		}
	}
}

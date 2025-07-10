using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.DataTransfer.Testing;

[TestedType(typeof(CommercialChargeDataObjectReader))]
sealed class CommercialChargeDataObjectReaderTest : DataObjectReaderTest
{
	[ExpectNoExceptions]
	public void TestDefaultCommercialChargePercentage()
	{
		using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		using (CustomsDataRegistry.Instance.DefaultInsuranceRate.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, 2.0m))
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var declarationData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair()
				{ Code = ZString.Empty },
				MessageSubType = new CodeDescriptionPair()
				{ Code = ZString.Empty }
			};

			declarationData.CommercialInfo = new CommercialInfo()
			{
				Name = "GROUPINV",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
				{
						new CommercialInvoiceHeader() {
							IncoTerm = new CodeDescriptionPair() { Code = "CIF", Description = "Description CIF" },
							CommercialChargeCollection = new List<CommercialCharge>(new[]
							{
								new CommercialCharge()
								{
									ChargeType = new CodeDescriptionPair() { Code = "ONS" },
									Amount = 200m,
									Currency = new Currency() { Code = "TWD" },
									IsDutiable = ZBool.True,
									IsGSTApplicable = ZBool.True,
									IsIncludedInITOT = ZBool.True,
									IsStatisticalValueApplicable = ZBool.False,
								}
							})
						}
					})
			};
			var reader = new TWJobDeclarationDataObjectReader(declarationData, logger, Factory);
			var result = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(result.PK);
			NUnit.Framework.Assert.That(newDeclarationBO.Invoices[0].Charges.Where(x => x.J7_ChargeType == "ONS").FirstOrDefault().J7_Percentage, Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "No PercentageOfLinePrice");

			declarationData.CommercialInfo = new CommercialInfo()
			{
				Name = "GROUPINV",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
				{
						new CommercialInvoiceHeader() {
							IncoTerm = new CodeDescriptionPair() { Code = "CIF", Description = "Description CIF" },
							CommercialChargeCollection = new List<CommercialCharge>(new[]
							{
								new CommercialCharge()
								{
									ChargeType = new CodeDescriptionPair() { Code = "ONS" },
									Amount = 0m,
									Currency = new Currency() { Code = "TWD" },
									IsDutiable = ZBool.True,
									IsGSTApplicable = ZBool.True,
									IsIncludedInITOT = ZBool.True,
									IsStatisticalValueApplicable = ZBool.False,
									PercentageOfLinePrice = 0m,
								}
							})
						}
					})
			};
			reader = new TWJobDeclarationDataObjectReader(declarationData, logger, Factory);
			result = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(result.PK);
			NUnit.Framework.Assert.That(newDeclarationBO.Invoices[0].Charges.Where(x => x.J7_ChargeType == "ONS").FirstOrDefault().J7_Percentage, Is.EqualTo(2m).Using(CustomComparers.TypeComparison), "Amount and PercentageOfLinePrice are 0");
		}
	}
}

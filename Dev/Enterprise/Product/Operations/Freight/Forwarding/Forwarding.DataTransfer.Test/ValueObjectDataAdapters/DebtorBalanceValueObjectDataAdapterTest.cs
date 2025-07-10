using System;
using CargoWise.IO;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(DebtorBalanceValueObjectDataAdapter))]
	sealed class DebtorBalanceValueObjectDataAdapterTest : ValueObjectDataAdapterTest<DebtorBalanceRecordForExport, Xsd.DebtorBalance>
	{
		#region Overrides

		protected override DebtorBalanceRecordForExport NewBusinessObject()
		{
			return EmptyDebtorBalanceRecord;
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "DebtorBalances"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "DebtorBalance"; }
		}

		protected override ValueObjectDataAdapter<DebtorBalanceRecordForExport, Xsd.DebtorBalance> GetNewBizObjXmlDataAdapter()
		{
			return new DebtorBalanceValueObjectDataAdapter();
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return new BusinessObjectAndExpectedOutputFileName[] { GetFullyPopulatedBizObjSample() };
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var temporaryOutputFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ValueObjectDataAdapters.TestFiles.FullyPopulatedDebtorBalance.xml");
			return new BusinessObjectAndExpectedOutputFileName(DebtorBalanceRecord, temporaryOutputFileName, ValidationKind.Xsd, "Fully populated debtor balance");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			DebtorBalanceRecordForExport emptyRecord = new DebtorBalanceRecordForExport(Debtor, Factory);
			return new BusinessObjectAndExpectedOutputFileName(emptyRecord, EmptyDebtorBalanceXmlPath, ValidationKind.Xsd, "Empty debtor balance");
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get { return false; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(EmptyDebtorBalanceRecord, EmptyDebtorBalanceXmlPath, ValidationKind.Xsd, "Empty Bizobj Sampe");
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string emptyDebtorBalanceXmlPath;
		string EmptyDebtorBalanceXmlPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyDebtorBalanceXmlPath))
				{
					emptyDebtorBalanceXmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.Forwarding.DataTransfer.Test.ValueObjectDataAdapters.TestFiles.EmptyDebtorBalance.xml");
				}
				return emptyDebtorBalanceXmlPath;
			}
		}

		OrgHeader Debtor
		{
			get
			{
				if (debtor == null)
				{
					debtor = Factory.New<OrgHeader>();
					debtor.OH_FullName = "Test Company Name";
					debtor.MainAddress.OA_Address1 = "184 Bourke Road";
					debtor.MainAddress.OA_City = "Alexandria";
					debtor.MainAddress.OA_State = "NSW";
					debtor.OH_Code = "Zabc";
					debtor.OH_IsDebtor = true;
					debtor.OH_IsCreditor = false;

					debtor.CompanyData.SetARTaxApplicable(true);
					debtor.MiscServ.OM_ARWHTApplicable = true;

					debtor.CompanyData.OB_AROnCreditHold = true;
					debtor.CompanyData.OB_ARCreditLimit = 1000.5m;

					OrgDebtorGroup debtorGrp = Factory.New<OrgDebtorGroup>();
					debtorGrp.OJ_Code = "AAA";
					debtorGrp.OJ_Desc = "Inter Company";

					debtor.MiscServ.OM_OJ_ARDebtorGroup = debtorGrp.PK;
					Factory.Save();
				}

				return debtor;
			}
		}
		OrgHeader debtor;

		DebtorBalanceRecordForExport DebtorBalanceRecord
		{
			get
			{
				if (debtorBalanceRecord == null)
				{
					debtorBalanceRecord = new DebtorBalanceRecordForExport(Debtor, Factory);
					debtorBalanceRecord.WIPsAmount = 190m;
					debtorBalanceRecord.OutstandingBalanceAmount = 200.45m;
				}
				return debtorBalanceRecord;
			}
		}
		DebtorBalanceRecordForExport debtorBalanceRecord;

		DebtorBalanceRecordForExport EmptyDebtorBalanceRecord
		{
			get
			{
				if (emptyDebtorBalanceRecord == null)
				{
					emptyDebtorBalanceRecord = new DebtorBalanceRecordForExport(Debtor, Factory);
					emptyDebtorBalanceRecord.ResetBalance();
				}
				return emptyDebtorBalanceRecord;
			}
		}
		DebtorBalanceRecordForExport emptyDebtorBalanceRecord;

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
						"DebtorBalance/Debtor",
						"Debtor/OrganisationDetails",
						"Debtor/Notes"
				};
			}
		}

		#endregion
	}
}

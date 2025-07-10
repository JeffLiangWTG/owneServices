using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingPendingTransactionsNotificationGroupRegistryDataType))]
	sealed class EInvoicingPendingTransactionsNotificationGroupRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EInvoicingPendingTransactionsNotificationGroupRegistryDataType>
	{
		#region Implementation

		protected override EInvoicingPendingTransactionsNotificationGroupRegistryDataType GetNewDataType()
			=> new EInvoicingPendingTransactionsNotificationGroupRegistryDataType();

		protected override string ExpectedEditorName => "EInvoicingPendingTransactionsNotificationGroupRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
			=> new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(
					new EInvoicingPendingTransactionsNotificationGroup()
					{
						GroupPK = TestGroup.PK,
						DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate,
						Days = 1
					},
					Encoding.Unicode.GetBytes($"<?xml version='1.0' encoding='utf-16'?><EInvoicingPendingTransactionsNotificationGroup><GroupPK>{TestGroup.PK.ToString().ToLower()}</GroupPK><DateType>PST</DateType><Days>1</Days></EInvoicingPendingTransactionsNotificationGroup>".Replace("'", "\""))),

				new ValidSampleAndBinaryValueInDB(
					new EInvoicingPendingTransactionsNotificationGroup()
					{
						GroupPK = TestGroup.PK,
						DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
						Days = 5
					},
					Encoding.Unicode.GetBytes($"<?xml version='1.0' encoding='utf-16'?><EInvoicingPendingTransactionsNotificationGroup><GroupPK>{TestGroup.PK.ToString().ToLower()}</GroupPK><DateType>INV</DateType><Days>5</Days></EInvoicingPendingTransactionsNotificationGroup>".Replace("'", "\""))),
			};

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			TestGroup = factory.NewWithValidTestData<GlbGroup>();

			factory.Save();
		}

		GlbGroup TestGroup;

		#endregion
	}
}

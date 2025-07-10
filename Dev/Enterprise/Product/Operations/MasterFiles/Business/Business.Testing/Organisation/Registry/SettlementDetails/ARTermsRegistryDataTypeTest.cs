using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ARTermsRegistryDataType))]
	sealed class ARTermsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ARTermsRegistryDataType>
	{
		#region Implementation

		protected override ARTermsRegistryDataType GetNewDataType()
		{
			return new ARTermsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "SettlementDetailsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ARTermsCollection collection = new ARTermsCollection();
			ARTerms terms1 = collection.AddNew();
			ARTerms terms2 = collection.AddNew();

			terms1.InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			terms1.InvoiceDays = 3;
			terms1.InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			terms1.TreatDisbursementsAsStandardValue = 2.5;
			var cycle1 = terms1.ARTermsCycles.AddNew();
			var cycle2 = terms1.ARTermsCycles.AddNew();
			cycle1.ToDay = 3;
			cycle1.PaymentDay = 6;
			cycle2.ToDay = 9;
			cycle2.PaymentDay = 12;

			terms2.InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			terms2.InvoiceDays = 4;
			terms2.InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			terms2.TreatDisbursementsAsStandardValue = 3.5;
			var cycle21 = terms2.ARPaymentCycles.AddNew();
			var cycle22 = terms2.ARPaymentCycles.AddNew();
			cycle21.ToDay = 1;
			cycle21.PaymentDay = 4;
			cycle22.ToDay = 2;
			cycle22.PaymentDay = 9;

			var xml = "<?xml version =\"1.0\" encoding=\"utf-16\"?><ArrayOfARTerms xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ARTerms><JobType>ALL</JobType><BranchPK>00000000-0000-0000-0000-000000000000</BranchPK><DeptPK>00000000-0000-0000-0000-000000000000</DeptPK><Direction>ALL</Direction><TransportMode>ALL</TransportMode><InvoiceClass>DSB</InvoiceClass><InvoiceDays>3</InvoiceDays><InvoiceTerm>MIC</InvoiceTerm><TreatDisbursementsAsStandardValue>3.5</TreatDisbursementsAsStandardValue><ARTermsCycles><ARTermsCycle><ToDay>3</ToDay><PaymentDay>6</PaymentDay></ARTermsCycle><ARTermsCycle><ToDay>9</ToDay><PaymentDay>12</PaymentDay></ARTermsCycle></ARTermsCycles><ARPaymentCycles /></ARTerms><ARTerms><JobType>ALL</JobType><BranchPK>00000000-0000-0000-0000-000000000000</BranchPK><DeptPK>00000000-0000-0000-0000-000000000000</DeptPK><Direction>ALL</Direction><TransportMode>ALL</TransportMode><InvoiceClass>ALL</InvoiceClass><InvoiceDays>4</InvoiceDays><InvoiceTerm>DPC</InvoiceTerm><TreatDisbursementsAsStandardValue>3.5</TreatDisbursementsAsStandardValue><ARTermsCycles /><ARPaymentCycles><ARPaymentCycle><ToDay>1</ToDay><PaymentDay>4</PaymentDay></ARPaymentCycle><ARPaymentCycle><ToDay>2</ToDay><PaymentDay>9</PaymentDay></ARPaymentCycle></ARPaymentCycles></ARTerms></ArrayOfARTerms>";
			var byteArrayValue = Encoding.Unicode.GetBytes(xml);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}

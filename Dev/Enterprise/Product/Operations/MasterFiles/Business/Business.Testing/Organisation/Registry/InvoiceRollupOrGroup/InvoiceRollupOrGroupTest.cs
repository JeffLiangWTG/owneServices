using System;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRollupOrGroup))]
	sealed class InvoiceRollupOrGroupTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateJobType()
		{
			InvoiceRollOrGroup.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			SecondInvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.AgencyBooking.Code;
			ThirdInvoiceRollupOrGroup.JobType = JobInvoicingConsumerTypes.AgencyBillOfLading.Code;

			InvoiceRollOrGroup.RunPreSaveValidation();
			SecondInvoiceRollupOrGroup.RunPreSaveValidation();
			ThirdInvoiceRollupOrGroup.RunPreSaveValidation();
			AssertHasError(InvoiceRollOrGroup.JobTypeInfo, "You must always have a row with Job = All, Direction = All, Mode = All.");
			AssertHasError(SecondInvoiceRollupOrGroup.JobTypeInfo, "You must always have a row with Job = All, Direction = All, Mode = All.");
			AssertHasError(ThirdInvoiceRollupOrGroup.JobTypeInfo, "You must always have a row with Job = All, Direction = All, Mode = All.");
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			InvoiceRollupOrGroup result = new InvoiceRollupOrGroup(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		InvoiceRollupOrGroup InvoiceRollOrGroup;
		InvoiceRollupOrGroup SecondInvoiceRollupOrGroup;
		InvoiceRollupOrGroup ThirdInvoiceRollupOrGroup;

		protected override void SetUp()
		{
			base.SetUp();
			InvoiceRollupOrGroupCollection collection = new InvoiceRollupOrGroupCollection();
			InvoiceRollOrGroup = collection.AddNew();
			SecondInvoiceRollupOrGroup = collection.AddNew();
			ThirdInvoiceRollupOrGroup = collection.AddNew();
		}

		#endregion
	}
}

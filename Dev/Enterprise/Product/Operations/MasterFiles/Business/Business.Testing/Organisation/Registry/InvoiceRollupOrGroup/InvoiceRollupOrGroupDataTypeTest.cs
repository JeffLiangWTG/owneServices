using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRollupOrGroupDataType))]
	sealed class InvoiceRollupOrGroupDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InvoiceRollupOrGroupDataType>
	{
		#region Implementation

		protected override InvoiceRollupOrGroupDataType GetNewDataType()
		{
			return new InvoiceRollupOrGroupDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InvoiceRollupOrGroupRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			InvoiceRollupOrGroupCollection collection = new InvoiceRollupOrGroupCollection();
			InvoiceRollupOrGroup invoiceRollupOrGroup = collection.AddNew();

			invoiceRollupOrGroup.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			invoiceRollupOrGroup.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			invoiceRollupOrGroup.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			invoiceRollupOrGroup.ServiceLevel = ZString.Empty;
			invoiceRollupOrGroup.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			invoiceRollupOrGroup.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			invoiceRollupOrGroup.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			invoiceRollupOrGroup.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection))
			};
		}

		#endregion
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsDocketReferenceDataObjectWriterTest : TestCaseWithFactory
	{
		#region TestBasicReferenceLevelFieldMappings

		public void TestBasicReferenceLevelFieldMappings()
		{
			var reference = Factory.New<WhsDocketReference>();
			reference.WX_RefType = "MAR";
			reference.WX_Reference = "11RR11";

			var referenceDataObject = new WhsDocketReferenceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reference))).GetDataObject(reference);

			AssertNotNull("referenceDataObject", referenceDataObject);

			CombineAssertions(delegate
			{
				AssertEquals("referenceDataObject.ContextInformation", null, referenceDataObject.ContextInformation);
				AssertEquals("referenceDataObject.IssueDate", null, referenceDataObject.IssueDate);
				AssertEquals("referenceDataObject.ReferenceNumber", "11RR11", referenceDataObject.ReferenceNumber);
				AssertEquals("referenceDataObject.Type.Code", "MAR", referenceDataObject.Type.Code);
				AssertEquals("referenceDataObject.Type.Description", "Marks and Numbers", referenceDataObject.Type.Description);
			});
		}

		#endregion
	}
}
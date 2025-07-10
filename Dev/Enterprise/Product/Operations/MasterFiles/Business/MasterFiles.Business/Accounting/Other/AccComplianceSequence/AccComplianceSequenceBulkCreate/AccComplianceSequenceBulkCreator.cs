using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceBulkCreator : NonPersistentBusinessObject
	{
		public AccComplianceSequenceBulkCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccComplianceSequenceCollection ComplianceSequences
		{
			get
			{
				if (fComplianceSequences == null)
				{
					fComplianceSequences = new AccComplianceSequenceCollection(Factory, new AdhocCollectionRelationship(typeof(AccComplianceSequence)));
					RegisterEditableChildObject(fComplianceSequences);
				}
				return fComplianceSequences;
			}
		}
		AccComplianceSequenceCollection fComplianceSequences;
	}
}

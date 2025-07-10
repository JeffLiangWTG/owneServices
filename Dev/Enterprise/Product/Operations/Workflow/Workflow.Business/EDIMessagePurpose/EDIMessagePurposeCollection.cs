using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class EDIMessagePurposeCollection : ActiveBusinessObjectCollection<EDIMessagePurpose>, IEDIMessagePurposeCollection
	{
		public EDIMessagePurposeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public EDIMessagePurposeCollection(BusinessObjectFactory factory, IEnumerable<EDIMessagePurpose> messagePurposes)
			: base(factory, new AdhocCollectionRelationship(typeof(EDIMessagePurpose)))
		{
			AddRange(messagePurposes);
		}
	}
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ConsignmentItemWrapper : IConsignmentItem
	{
		readonly ZString splitMark;

		public ConsignmentItemWrapper(ZString splitMark)
		{
			this.splitMark = splitMark;
		}

		IOrigin IConsignmentItem.Origin => null;

		ZString IConsignmentItem.Split => splitMark;

		#region Not Applicable

		ICommodity IConsignmentItem.Commodity => null;

		IGoodsMeasure IConsignmentItem.GoodsMeasure => null;

		IPackaging IConsignmentItem.Packaging => null;

		IEnumerable<ITransportContractDocument> IConsignmentItem.TransportContractDocuments => null;

		ZString IConsignmentItem.AssociatedGovernmentProcedureCode => null;

		#endregion
	}
}

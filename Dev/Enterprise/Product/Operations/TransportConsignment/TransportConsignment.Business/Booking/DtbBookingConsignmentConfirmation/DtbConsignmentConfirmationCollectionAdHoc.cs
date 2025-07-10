using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentConfirmationCollectionAdHoc : ActiveBusinessObjectCollection<DtbConsignmentConfirmation>, IBindingList
	{
		public DtbConsignmentConfirmationCollectionAdHoc(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(DtbConsignmentConfirmation)))
		{
		}

		#region AllowNew

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region AllowRemove

		bool IBindingList.AllowRemove
		{
			get { return false; }
		}

		#endregion
	}
}

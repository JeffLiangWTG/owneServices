using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class CargoIMPPhase2EDIMessageCollection : ActiveBusinessObjectCollection<CargoIMPPhase2EDIMessage>
	{
		public CargoIMPPhase2EDIMessageCollection(ForwardingShipment master)
			: base(master.Factory, master, new ZQuery(), EDIMessageSchema.EM_LinkUniqueID)
		{
			SetReadOnlyIncludingChildren(true);
			MessageNumberStrategy = new CargoIMPPhase2MessageNumberStrategy(master.Factory);
			ApplySort(EDIMessage.Schema.EM_SystemCreateTimeUtc, ListSortDirection.Descending);
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		readonly IMessageNumberStrategy MessageNumberStrategy;

		#region Implementation

		protected override void SetRelationshipDefaultsForElementCore(CargoIMPPhase2EDIMessage newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.EM_LinkTable = ForwardingShipment.Schema.TableName;
			newElement.EM_ApplicationCode = ApplicationCodeList.Codes.CargoIMPPhase2;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CargoIMPPhase2);
			return query;
		}

		protected override void SetDefaultsForNewElementCore(CargoIMPPhase2EDIMessage newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.MessageNumberStrategy = MessageNumberStrategy;
		}

		#endregion

		#region TestHelpers
#if DEBUG

		public void RemoveAndDeleteAllFromTest()
		{
			try
			{
				foreach (EDIMessage message in this)
				{
					if (message != null)
					{
						message.IsDeletingInTest = true;
					}
				}

				DeleteAll();
			}
			finally
			{
				foreach (EDIMessage message in this)
				{
					if (message != null)
					{
						message.IsDeletingInTest = false;
					}
				}
			}
		}
#endif
		#endregion
	}
}

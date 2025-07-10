using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class CIMEDIMessageDependentCollection : DependentBusinessObjectCollection<CIMEDIMessage, ExportAWBHeader>
	{
		public CIMEDIMessageDependentCollection(ExportAWBHeader master)
			: base(master)
		{
			SetReadOnlyIncludingChildren(true);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return EDIMessageSchema.EM_LinkUniqueID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((CIMEDIMessage)child).EM_LinkTable = ExportAWBHeaderSchema.Constants.TableName;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, CIMEDIMessage.ApplicationCodes.CIM);
			return result;
		}

		internal CIMEDIMessage GetLatestTransmittedMessage()
		{
			Sort(new SortInfo(CIMEDIMessage.Schema.EM_SystemCreateTimeUtc, ListSortDirection.Descending));
			foreach (CIMEDIMessage message in this)
			{
				if (message.EM_ReceiveTransmit == CIMEDIMessage.Direction.Transmit)
				{
					return message;
				}
			}

			return null;
		}

		#region Test Helpers
#if DEBUG

		public void RemoveAndDeleteAllFromTest()
		{
			try
			{
				foreach (EDIMessage message in this)
				{
					message.IsDeletingInTest = true;
				}

				RemoveAndDeleteAll();
			}
			finally
			{
				foreach (EDIMessage message in this)
				{
					message.IsDeletingInTest = false;
				}
			}
		}

#endif
		#endregion

	}
}

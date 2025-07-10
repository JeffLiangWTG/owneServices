using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public abstract class AmendmentMessagesGenerator
	{
		protected AmendmentMessagesGenerator(BusinessObject businessObject)
		{
			this.businessObject = businessObject;
		}

		public EDIMessage[] GenerateAmendmentMessageSet()
		{
			ArrayList result = new ArrayList();
			if (UniqueIdentifierBeingChanged)
			{
				result.Add(GenerateQueuedWithdrawalMessage());
				result.Add(GeneratePendingOriginalMessage());
			}
			else
			{
				result.Add(GenerateQueuedAmendmentMessage());
			}
			return (EDIMessage[])result.ToArray(typeof(EDIMessage));
		}

		internal EDIMessage GeneratePendingOriginalMessage()
		{
			EDIMessage result = GenerateOriginalMessageCore();
			result.EM_Status = EDIMessage.Status.Pending;
			return result;
		}

		internal EDIMessage GenerateQueuedAmendmentMessage()
		{
			EDIMessage result = GenerateAmendmentMessageCore();
			result.EM_Status = EDIMessage.Status.Queued;
			return result;
		}

		internal EDIMessage GenerateQueuedWithdrawalMessage()
		{
			EDIMessage result = GenerateWithdrawalMessageCore(businessObject, GetBusinessObjectInNewFactory(businessObject));
			result.EM_Status = EDIMessage.Status.Queued;
			return result;
		}

		protected virtual BusinessObject GetBusinessObjectInNewFactory(BusinessObject businessObject)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			return newFactory.Load(businessObject.GetType(), businessObject.PK);
		}

		protected virtual internal bool UniqueIdentifierBeingChanged
		{
			get
			{
				foreach (ZPropertyInfo info in UniqueIdentifierInfos)
				{
					if (info.Value.ToString() != info.OriginalValue.ToString())
					{
						return true;
					}
				}
				return false;
			}
		}

		protected BusinessObject businessObject;
		protected abstract ZPropertyInfo[] UniqueIdentifierInfos { get; }
		protected abstract EDIMessage GenerateOriginalMessageCore();
		protected abstract EDIMessage GenerateAmendmentMessageCore();
		protected abstract EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory);
	}
}

using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class NonSecuritySpecialHandlingCollectionDataObjectReader : DataObjectCollectionReader<CodeDescriptionPair, NonSecurityJobConsolAWBSpecialHandling>
	{
		public NonSecuritySpecialHandlingCollectionDataObjectReader(CodeDescriptionPair[] specialHandlingDataObjects, UniversalObjectFactory factory, ForwardingConsol consolBusinessObject)
			: base(specialHandlingDataObjects)
		{
			consolBO = consolBusinessObject;
			universalFactory = factory;
		}

		readonly ForwardingConsol consolBO;
		readonly UniversalObjectFactory universalFactory;

		protected override NonSecurityJobConsolAWBSpecialHandling[] BusinessObjects
		{
			get { return businessObjects ?? (businessObjects = consolBO.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().ToArray()); }
		}

		NonSecurityJobConsolAWBSpecialHandling[] businessObjects;

		protected override void AddToCollection(NonSecurityJobConsolAWBSpecialHandling businessObject)
		{
			if (businessObject is NonSecurityJobConsolAWBSpecialHandling nonSecuritySpecialHandlingBO)
			{
				consolBO.AWBSpecialHandlingItems.Add(nonSecuritySpecialHandlingBO);
			}
		}

		protected override void RemoveFromCollection(NonSecurityJobConsolAWBSpecialHandling businessObject)
		{
			consolBO.AWBSpecialHandlingItems.RemoveAndDelete(businessObject);
		}

		protected override NonSecurityJobConsolAWBSpecialHandling FindMatchingBusinessObject(CodeDescriptionPair dataObject)
		{
			if (consolBO.AWBSpecialHandlingItems.Count == 0)
			{
				return null;
			}

			return FindSpecialHandlingItem(consolBO, dataObject);
		}

		protected override NonSecurityJobConsolAWBSpecialHandling ReadIntoBusinessObject(CodeDescriptionPair dataObject, NonSecurityJobConsolAWBSpecialHandling businessObject)
		{
			if (businessObject != null)
			{
				return businessObject;
			}

			var nonSecuritySpecialHandlingBO = universalFactory.New<NonSecurityJobConsolAWBSpecialHandling>();
			nonSecuritySpecialHandlingBO.JKH_Code = dataObject.Code.Value;

			return nonSecuritySpecialHandlingBO;
		}

		NonSecurityJobConsolAWBSpecialHandling FindSpecialHandlingItem(ForwardingConsol consol, CodeDescriptionPair specialHandlingDO)
		{
			foreach (NonSecurityJobConsolAWBSpecialHandling handlingItem in consol.AWBSpecialHandlingItems)
			{
				if (handlingItem.JKH_Code == specialHandlingDO.Code.Value)
				{
					return handlingItem;
				}
			}

			return null;
		}
	}
}

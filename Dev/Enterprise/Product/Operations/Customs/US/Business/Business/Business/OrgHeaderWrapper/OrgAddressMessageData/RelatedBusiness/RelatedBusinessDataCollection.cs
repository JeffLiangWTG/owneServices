using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class RelatedBusinessDataCollection : NonPersistentBusinessObjectCollection<RelatedBusinessData>
	{
		public RelatedBusinessDataCollection(OrgAddressMessageData messageData)
			: base(messageData.Factory)
		{
			this.messageData = messageData;
		}
		readonly OrgAddressMessageData messageData;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RelatedBusinessData(Factory, messageData);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var relatedBusiness = (RelatedBusinessData)child;
			relatedBusiness.messageData.RelatedBusinessSequenceGenerator.RecalculateWhenAdded(relatedBusiness);
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class PersonIdentityInformationDataCollection : NonPersistentBusinessObjectCollection<PersonIdentityInformationData>
	{
		public PersonIdentityInformationDataCollection(OrgAddressMessageData messageData)
			: base(messageData.Factory)
		{
			this.messageData = messageData;
		}
		readonly OrgAddressMessageData messageData;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PersonIdentityInformationData(Factory, messageData);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var personPII = (PersonIdentityInformationData)child;
			personPII.messageData.PIISequenceGenerator.RecalculateWhenAdded(personPII);
		}
	}
}

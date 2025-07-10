using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTranslatedAddressAdditionalInfoWrapperCollection : NonPersistentBusinessObjectCollection<OrgTranslatedAddressAdditionalInfoWrapper>
	{
		public OrgTranslatedAddressAdditionalInfoWrapperCollection(OrgTranslatedAddress orgTranslatedAddress) : base(orgTranslatedAddress.Factory)
		{
			OrgTranslatedAddress = orgTranslatedAddress;

			(OrgTranslatedAddress?.Address?.AdditionalInfos ?? Enumerable.Empty<OrgAddressAdditionalInfo>()).ToList().ForEach(info =>
			{
				Add(new OrgTranslatedAddressAdditionalInfoWrapper(info, OrgTranslatedAddress));
			});
		}

		public OrgTranslatedAddressAdditionalInfoWrapperCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgTranslatedAddress OrgTranslatedAddress { get; }

		public void ReBuild()
		{
			var additionalInfos = (OrgTranslatedAddress?.Address?.AdditionalInfos ?? Enumerable.Empty<OrgAddressAdditionalInfo>()).ToList();
			var wrapperCollection = this.Cast<OrgTranslatedAddressAdditionalInfoWrapper>().ToList();

			wrapperCollection.ForEach(wrapper =>
			{
				if (additionalInfos.All(info => info != wrapper.AdditionalInfoInfo))
				{
					Remove(wrapper);
				}
			});

			additionalInfos.ForEach(info =>
			{
				if (wrapperCollection.All(wrapper => wrapper.AdditionalInfoInfo != info))
				{
					Add(new OrgTranslatedAddressAdditionalInfoWrapper(info, OrgTranslatedAddress));
				}
			});
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgTranslatedAddressAdditionalInfoWrapper(Factory);
		}

		#region Overrides

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		#endregion
	}
}

using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgAddressAdditionalInfo : AutoOrgAddressAdditionalInfo
	{
		public OrgAddressAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZBool OAI_IsPrimary
		{
			get { return base.OAI_IsPrimary; }
			set
			{
				base.OAI_IsPrimary = value;
				if (base.OAI_IsPrimary)
				{
					var primaryTranslatedAddresses = Address.TranslatedAddresses.Where(t => !string.IsNullOrWhiteSpace(t.OTA_AdditionalAddressInformation)).ToList();
					foreach (var primaryTranslatedAddress in primaryTranslatedAddresses)
					{
						var primaryTranslatedAddressesLanguage = primaryTranslatedAddress.OTA_Language;
						if (TranslatedInfos.All(t => t.OTI_Language != primaryTranslatedAddressesLanguage))
						{
							var newTranslatedInfo = TranslatedInfos.AddNew();
							newTranslatedInfo.OTI_Language = primaryTranslatedAddressesLanguage;
							newTranslatedInfo.OTI_AdditionalInfo = primaryTranslatedAddress.OTA_AdditionalAddressInformation;
						}
						else
						{
							TranslatedInfos.Where(t => t.OTI_Language == primaryTranslatedAddressesLanguage && t.OTI_AdditionalInfo != primaryTranslatedAddress.OTA_AdditionalAddressInformation)
								.ToList().ForEach(t => t.OTI_AdditionalInfo = primaryTranslatedAddress.OTA_AdditionalAddressInformation);
						}
					}

					var redundantTranslatedInfos = TranslatedInfos.Where(t => primaryTranslatedAddresses.All(p => p.OTA_Language != t.OTI_Language));
					redundantTranslatedInfos.DeleteAll();
				}

				var additionalInfos = Address?.AdditionalInfos.Where(a => a.OAI_IsPrimary);
				if (additionalInfos?.Count() == 1)
				{
					var primaryAdditionalAddress = additionalInfos.Single().OAI_AdditionalInfo;
					if (!string.IsNullOrEmpty(primaryAdditionalAddress))
					{
						Address.OA_AdditionalAddressInformation = primaryAdditionalAddress;
					}
				}

				if (!additionalInfos.Any())
				{
					Address.OA_AdditionalAddressInformation = ZString.Empty;
				}
			}
		}

		public override ZString OAI_AdditionalInfo
		{
			get { return base.OAI_AdditionalInfo; }
			set
			{
				base.OAI_AdditionalInfo = value;
				if (OAI_IsPrimary && Address != null && Address.OA_AdditionalAddressInformation != base.OAI_AdditionalInfo && Address.AdditionalInfos.Count(a => a.OAI_IsPrimary) == 1)
				{
					Address.OA_AdditionalAddressInformation = base.OAI_AdditionalInfo;
				}
			}
		}

		public override void Delete()
		{
			var parent = Address;
			var isPrimary = OAI_IsPrimary;
			var anotherPrimaryAdditionalInfo = Address?.AdditionalInfos.FirstOrDefault(a => a.PK != PK && a.OAI_IsPrimary);

			TranslatedInfos.DeleteAll();
			base.Delete();

			if (isPrimary && parent != null && !parent.IsDeleted)
			{
				if (anotherPrimaryAdditionalInfo != null)
				{
					parent.OA_AdditionalAddressInformation = anotherPrimaryAdditionalInfo.OAI_AdditionalInfo;
				}
				else
				{
					parent.OA_AdditionalAddressInformation = ZString.Empty;
					parent.TranslatedAddresses.ForEach(x => x.OTA_AdditionalAddressInformation = ZString.Empty);
				}
			}

			parent?.AdditionalInfos.ForEach(a => a.Validation.ValidateAll());
		}

		[ChildEditable(true)]
		public OrgTranslatedAddressAdditionalInfoCollection TranslatedInfos
		{
			get
			{
				if (translatedInfos == null)
				{
					translatedInfos = new OrgTranslatedAddressAdditionalInfoCollection(this);
					RegisterEditableChildObject(translatedInfos);
				}
				return translatedInfos;
			}
		}
		OrgTranslatedAddressAdditionalInfoCollection translatedInfos;

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (Address != null && Address.Header != null)
			{
				shouldBeReadOnly = !Address.Header.SecurityProvider.HasModifyAddressCapabilitiesSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}

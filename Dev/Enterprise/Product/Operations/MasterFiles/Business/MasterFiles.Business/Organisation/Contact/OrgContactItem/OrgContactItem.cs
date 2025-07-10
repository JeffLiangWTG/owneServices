using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgContactItem : AutoOrgContactItem
	{
		public new class Schema : AutoOrgContactItem.Schema
		{
			public const string OI_Address_IsManuallyVerified = "OI_Address_IsManuallyVerified";
		}

		public OrgContactItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region DefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OI_IsPrimary = true;
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgContactItemFetchStrategy(this);
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !HasModifyContactItemSecurity || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		bool HasModifyContactItemSecurity
		{
			get
			{
				var contact = Contact;
				if (contact != null)
				{
					var header = contact.Header;
					if (header != null)
					{
						return header.SecurityProvider.HasModifyContactContactDetailsSecurity;
					}
				}

				return Env.Security.OrgContactModifyContactDetails.IsAllowed;
			}
		}

		#endregion

		#region OI_Address_IsManuallyVerified

		public ZBool OI_Address_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, OrgContactItemSchema.Constants.OI_Address, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(OI_Address_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, OrgContactItemSchema.Constants.OI_Address, null, null); }
		}

		public ZPropertyInfo OI_Address_IsManuallyVerifiedInfo
		{
			get { return GetZPropertyInfo(Schema.OI_Address_IsManuallyVerified); }
		}

		#endregion

		[ChildEditable(true)]
		public GenCustomAddOnRuleAckCollection AddOnRuleAcks
		{
			get
			{
				if (addOnRuleAcks == null)
				{
					addOnRuleAcks = new GenCustomAddOnRuleAckCollection(this);
					RegisterEditableChildObject(addOnRuleAcks);
				}

				return addOnRuleAcks;
			}
		}
		GenCustomAddOnRuleAckCollection addOnRuleAcks;

		public override void Delete()
		{
			AddOnRuleAcks.DeleteAll();

			base.Delete();
		}
	}
}

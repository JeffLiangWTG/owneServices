using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(OrgCarrierAccountSchema.Constants.OAN_AccountNumber), DescriptionProperty("CarrierName")]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgCarrierAccount : AutoOrgCarrierAccount
	{
		public OrgCarrierAccount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return Carrier != null && (!Carrier.SecurityProvider.HasModifyCarrierSecurity || MetaData.GetReadOnlyExcludingMethodProvider(this, property));
		}

		#endregion

		#region Properties

		[MaxLength(50)]
		public ZString CarrierName
		{
			get { return Carrier == null ? ZString.Empty : Carrier.OH_FullNameTruncated; }
		}

		public ZPropertyInfo CarrierNameInfo
		{
			get { return GetZPropertyInfo(nameof(CarrierName)); }
		}

		public override ZGuid OAN_OH_Carrier
		{
			get { return base.OAN_OH_Carrier; }

			set
			{
				// Until proper logic for Billing Party is implemented
				base.OAN_OH_Carrier = value;
				base.OAN_OH_BillToParty = value;
			}
		}

		#endregion

		#region OrgCarrierMetaData

		public OrgCarrierAccountMetaDataCollection MetaDataCollection
		{
			get { return new OrgCarrierAccountMetaDataCollection(this); }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			Array.ForEach(MetaDataCollection.ToArray(), i => i.Delete());
			base.Delete();
		}

		#endregion
	}
}

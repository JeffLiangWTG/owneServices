using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsOrgPartAttributesInfo : DataObjectInfo
	{
		#region Constructors

		public WhsOrgPartAttributesInfo()
		{
			attribute1Caption = "";
			attribute1IsMandatory = false;
			attribute2Caption = "";
			attribute2IsMandatory = false;
			attribute3Caption = "";
			attribute3IsMandatory = false;
			isSerialNumberUsedByOrganisation = false;
			serialNumberCaption = "";
			expiryDateIsUsed = false;
			packingDateIsUsed = false;
		}

		public WhsOrgPartAttributesInfo(OrgHeader org)
			: this()
		{
			if (org != null)
			{
				var miscServ = org.MiscServ;
				if (miscServ != null)
				{
					attribute1Caption = miscServ.OM_IMPartAttrib1NameMultilingual;
					attribute2Caption = miscServ.OM_IMPartAttrib2NameMultilingual;
					attribute3Caption = miscServ.OM_IMPartAttrib3NameMultilingual;
				}
				attribute1IsMandatory = org.PartAttributeManager.IsPartAttributeMandatory(1);
				attribute2IsMandatory = org.PartAttributeManager.IsPartAttributeMandatory(2);
				attribute3IsMandatory = org.PartAttributeManager.IsPartAttributeMandatory(3);
				isSerialNumberUsedByOrganisation = org.PartAttributeManager.IsSerialNumberUsedByOrganisation;
				expiryDateIsUsed = org.PartAttributeManager.IsExpiryDateUsedByOrganisation;
				packingDateIsUsed = org.PartAttributeManager.IsPackingDateUsedByOrganisation;
				serialNumberCaption = Res.GetString("13480f6e-b9bf-4ae5-a984-7fa433677b52", "Serial #");
			}
		}

		#endregion

		#region Properties

		public string Attribute1Caption
		{
			get => attribute1Caption;
			set => attribute1Caption = value;
		}

		public bool Attribute1IsMandatory
		{
			get => attribute1IsMandatory;
			set => attribute1IsMandatory = value;
		}

		public string Attribute2Caption
		{
			get => attribute2Caption;
			set => attribute2Caption = value;
		}

		public bool Attribute2IsMandatory
		{
			get => attribute2IsMandatory;
			set => attribute2IsMandatory = value;
		}

		public string Attribute3Caption
		{
			get => attribute3Caption;
			set => attribute3Caption = value;
		}

		public bool Attribute3IsMandatory
		{
			get => attribute3IsMandatory;
			set => attribute3IsMandatory = value;
		}

		public bool IsSerialNumberUsedByOrganisation
		{
			get => isSerialNumberUsedByOrganisation;
			set => isSerialNumberUsedByOrganisation = value;
		}

		public string SerialNumberCaption
		{
			get => serialNumberCaption;
			set => serialNumberCaption = value;
		}

		public bool ExpiryDateIsUsed
		{
			get => expiryDateIsUsed;
			set => expiryDateIsUsed = value;
		}

		public bool PackingDateIsUsed
		{
			get => packingDateIsUsed;
			set => packingDateIsUsed = value;
		}

		#endregion

		#region Implementation

		string attribute1Caption;
		bool attribute1IsMandatory;
		string attribute2Caption;
		bool attribute2IsMandatory;
		string attribute3Caption;
		bool attribute3IsMandatory;
		bool isSerialNumberUsedByOrganisation;
		string serialNumberCaption;
		bool expiryDateIsUsed;
		bool packingDateIsUsed;

		#endregion
	}
}

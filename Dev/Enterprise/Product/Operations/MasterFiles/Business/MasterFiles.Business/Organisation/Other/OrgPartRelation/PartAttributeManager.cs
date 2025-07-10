using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PartAttributeManager
	{
		public PartAttributeManager(OrgHeader org)
		{
			if (org == null)
			{
				throw new ArgumentNullException(nameof(org));
			}

			Organisation = org;
		}

		#region VIN Related

		// WORK AROUND UNTIL THIS PARTATTRIBUTE MANAGER IS REFACTORED TO WORK WITH PART ATTRIBUTES AS OBJECTS
		public class PartAttribute
		{
			public PartAttribute(int index, PartAttributeManager manager)
			{
				this.Index = index;
				this.Manager = manager;
			}

			public readonly int Index;
			readonly PartAttributeManager Manager;

			public ZString Name
			{
				get { return Manager.PartAttributeName(Index); }
			}
		}

		public bool HasVINForProduct(OrgSupplierPart product)
		{
			bool result = false;
			PartAttribute attribute = VINPartAttribute;
			if (attribute != null)
			{
				result = IsPartAttributeUsedByProduct(product, attribute.Index);
			}
			return result;
		}

		public PartAttribute VINPartAttribute
		{
			get
			{
				for (int i = 1; i <= 3; i++)
				{
					if (PartAttributeType(i) == PartAttributeTypeList.Codes.VIN)
					{
						return new PartAttribute(i, this);
					}
				}
				return null;
			}
		}

		#endregion

		#region Organisation Level Queries

		public const int MaxAttributes = 4;
		public readonly OrgHeader Organisation;

		public MultilingualString PartAttributeName1
			=> Organisation.MiscServ.OM_IMPartAttrib1NameMultilingual.IsEmpty
				? ResString.GetMultilingualString("7E141A42-2CC1-435e-BBAA-F920A592985F", "Part Attrib. 1")
				: Organisation.MiscServ.OM_IMPartAttrib1NameMultilingual;

		public MultilingualString PartAttributeName2
			=> Organisation.MiscServ.OM_IMPartAttrib2NameMultilingual.IsEmpty
				? ResString.GetMultilingualString("0b69b8b3-264f-422d-9aa5-8483c78d2077", "Part Attrib. 2")
				: Organisation.MiscServ.OM_IMPartAttrib2NameMultilingual;

		public MultilingualString PartAttributeName3
			=> Organisation.MiscServ.OM_IMPartAttrib3NameMultilingual.IsEmpty
				? ResString.GetMultilingualString("70005eb2-921f-4139-9d3e-7d1fc5953ec6", "Part Attrib. 3")
				: Organisation.MiscServ.OM_IMPartAttrib3NameMultilingual;

		public MultilingualString SerialNumberName
			=> ResString.GetMultilingualString("e4d8a78e-aca4-463d-9b9e-42eb142ada31", "Serial Number");

		public MultilingualString PartAttributeName(int attributeNumber)
		{
			switch (attributeNumber)
			{
				case 1:
					return PartAttributeName1;
				case 2:
					return PartAttributeName2;
				case 3:
					return PartAttributeName3;
				case 4:
					return SerialNumberName;
			}

			ErrorReporter.ReportOnce("PartAttributeName(int)", "Bad PartAttribute Index: " + attributeNumber.ToString());
			return (NoResString)string.Empty;
		}

		public ZString PartAttributeType(int attributeNumber)
		{
			string propertyName = "OM_IMPartAttrib" + attributeNumber.ToString() + "Type";
			try
			{
				return (ZString)Organisation.MiscServ[propertyName];
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("PartAttributeType(int)", "Unknown Field: " + propertyName);
			}
			return ZString.Empty;
		}

		public bool IsPartAttributeMandatory(int attributeNumber)
		{
			return IsTypeMandatory(PartAttributeType(attributeNumber));
		}

		public bool IsExpiryDateUsedByOrganisation
		{
			get { return Organisation.MiscServ.OM_IMUseExpiryDate; }
		}

		public bool IsPackingDateUsedByOrganisation
		{
			get { return Organisation.MiscServ.OM_IMUsePackingDate; }
		}

		public bool IsPartAttributeUsedByOrganisation(int attributeNumber)
		{
			return !PartAttributeType(attributeNumber).IsEmpty;
		}

		public bool IsSerialNumberUsedByOrganisation
		{
			get { return Organisation.MiscServ.OM_IMUseSerialNumber; }
		}

		#region IsPartAttributeAJulianBatchNumber

		public bool IsPartAttributeAJulianBatchNumber(int attributeNumber)
		{
			return PartAttributeType(attributeNumber).EqualsIgnoringCase(PartAttributeTypeList.Codes.JulianBatchNumber);
		}

		#endregion

		#endregion

		#region Part Level Queries

		public ZBool IsPartAttributeUsedByProduct(OrgSupplierPart part, int attributeNumber)
		{
			bool result = false;
			if (IsPartAttributeUsedByOrganisation(attributeNumber) && part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					string propertyName = "OU_UsePartAttrib" + attributeNumber.ToString();
					try
					{
						result = (ZBool)relation[propertyName];
					}
					catch (ZException)
					{
						ErrorReporter.ReportOnce("Unknown Field: " + propertyName, "Bugger");
					}
				}
			}
			return result;
		}

		public ZBool IsExpiryDateUsedByProduct(OrgSupplierPart part)
		{
			bool result = false;
			if (IsExpiryDateUsedByOrganisation && part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = relation.OU_UseExpiryDate;
				}
			}
			return result;
		}

		public ZBool IsPackingDateUsedByProduct(OrgSupplierPart part)
		{
			bool result = false;
			if (IsPackingDateUsedByOrganisation && part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = relation.OU_UsePackingDate;
				}
			}
			return result;
		}

		public ZBool IsPartAttributeReleaseCaptured(OrgSupplierPart part, int attributeNumber)
		{
			if (IsPartAttributeUsedByProduct(part, attributeNumber))
			{
				var relation = GetOwnerRelationship(part);

				switch (attributeNumber)
				{
					case 1:
						return relation.OU_IsPartAttrib1ReleaseCaptured;
					case 2:
						return relation.OU_IsPartAttrib2ReleaseCaptured;
					case 3:
						return relation.OU_IsPartAttrib3ReleaseCaptured;
				}
			}

			return false;
		}

		public ZBool IsSerialNumberUsedByProduct(OrgSupplierPart part)
		{
			bool result = false;
			if (IsSerialNumberUsedByOrganisation && part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = relation.OU_UseSerialNumber;
				}
			}
			return result;
		}

		public ZBool IsSerialNumberReleaseCaptured(OrgSupplierPart part)
		{
			bool result = false;
			if (IsSerialNumberUsedByOrganisation && part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = relation.OU_IsSerialNumberReleaseCaptured;
				}
			}
			return result;
		}

		#region IsCompletePalletPickingUsedByProduct

		public bool IsCompletePalletPickingUsedByProduct(OrgSupplierPart part)
		{
			bool result = false;
			if (part != null)
			{
				OrgPartRelation relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = relation.OU_CompletePalletPicking;
				}
			}
			return result;
		}

		#endregion

		#region IsAttributeNeutralUsedByProduct

		public bool IsAttributeNeutralUsedByProduct(OrgSupplierPart part)
		{
			bool result = false;
			if (Organisation != null && part != null)
			{
				OrgPartRelation relation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(Organisation.PK, OrgPartRelation.RelationshipTypes.Owner);
				if (relation != null)
				{
					result = (relation.OU_PickMode == WhsPickMode.Codes.AttributeNeutral);
				}
			}
			return result;
		}

		public bool IsAttributeNeturalAndDocumentRollUp(OrgSupplierPart part)
		{
			var result = false;

			if (part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = (relation.OU_PickMode == WhsPickMode.Codes.AttributeNeutral && relation.OU_RollUpAttributesOnDocuments);
				}
			}

			return result;
		}

		#endregion

		#region IsPartAttributeAJulianBatchNumberAndUsed

		public ZBool IsPartAttributeAJulianBatchNumberAndUsed(OrgSupplierPart part, int attributeNumber)
		{
			return IsPartAttributeUsedByProduct(part, attributeNumber) && PartAttributeType(attributeNumber) == PartAttributeTypeList.Codes.JulianBatchNumber;
		}

		#endregion

		#region IsAJulianBatchNumberAttributeUsed

		public ZBool IsAJulianBatchNumberAttributeUsed(OrgSupplierPart part)
		{
			return part != null &&
				(IsPartAttributeAJulianBatchNumberAndUsed(part, 1) ||
				IsPartAttributeAJulianBatchNumberAndUsed(part, 2) ||
				IsPartAttributeAJulianBatchNumberAndUsed(part, 3));
		}

		#endregion

		public ZString ExpiryDateFormatString(OrgSupplierPart part)
		{
			var result = ZString.Empty;
			if (IsExpiryDateUsedByOrganisation && part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = relation.OU_ExpiryDateFormatString.Replace("m", "M");
				}
			}
			return result;
		}

		public ZString PackingDateFormatString(OrgSupplierPart part)
		{
			var result = ZString.Empty;
			if (IsPackingDateUsedByOrganisation && part != null)
			{
				var relation = GetOwnerRelationship(part);
				if (relation != null)
				{
					result = relation.OU_PackingDateFormatString.Replace("m", "M");
				}
			}
			return result;
		}

		#endregion

		#region Implementation

		OrgPartRelation GetOwnerRelationship(OrgSupplierPart part)
		{
			return part.RelatedOrganisations.FindByOrganisationPKAndRelationship(Organisation.PK, OrgPartRelation.RelationshipTypes.Owner);
		}

		bool IsTypeMandatory(ZString type)
		{
			switch (type)
			{
				case PartAttributeTypeList.Codes.Mandatory:
				case PartAttributeTypeList.Codes.BatchNumber:
				case PartAttributeTypeList.Codes.JulianBatchNumber:
				case PartAttributeTypeList.Codes.VIN:
					return true;
				case PartAttributeTypeList.Codes.NonMandatory:
				default:
					return false;
			}
		}

		#endregion
	}
}

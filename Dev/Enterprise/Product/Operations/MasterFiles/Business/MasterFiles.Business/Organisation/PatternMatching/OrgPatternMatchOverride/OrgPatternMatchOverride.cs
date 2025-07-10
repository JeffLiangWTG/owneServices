using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgPatternMatchOverrideExtension
	{
		public static ZString GetLocalCode(this OrgPatternMatchOverride pattern)
		{
			if (!pattern.OO_Relationship.IsEmpty)
			{
				if (pattern.IsNotGuid)
				{
					return pattern.OO_LocalCode;
				}

				var localBusinessObject = GetBusinessObjectFromGuid(pattern);
				if (localBusinessObject != null)
				{
					return CodePropertyAttribute.CodeFromBusinessObject(localBusinessObject);
				}
			}

			return ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Multiple business object types are mapped to this entity")]
		internal static BusinessObject GetBusinessObjectFromGuid(this OrgPatternMatchOverride pattern)
		{
			var pk = pattern.OO_LocalGuid;
			if (pk.IsEmpty || pattern.IsNotGuid)
			{
				return null;
			}

			var lookups = pattern.Lookups;
			switch (pattern.OO_Relationship)
			{
				case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
					return lookups.RefContainers.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.Country:
					return lookups.RefCountries.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.Currency:
					return lookups.RefCurrencies.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.Commodities:
					return lookups.Commodities.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.Equipment:
					return lookups.Equipment.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.Organisation:
					lookups.Organisations.Load(new ZQuery(OrgHeaderSchema.PK, pk));
					return lookups.Organisations.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.Port:
					return lookups.RefUNLOCOs.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.Warehouse:
					lookups.WhsWarehouses.Load();
					return lookups.WhsWarehouses.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.ServiceLevel:
					lookups.RefServiceLevels.FindByPK(pk);
					return lookups.RefServiceLevels.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.IntZone:
					return lookups.IntZones.FindByPK(pk);
				case Constants.OrgPatternMatchOverrideRelationships.DocumentType:
					return lookups.DocTypes.FindByPK(pk);

				default:
					throw new DeveloperNotificationException(ZString.Format("Any OO_Relationship that makes IsNotGuid false should be included here. Add the following: {0}", pattern.OO_Relationship));
			}
		}
	}

	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgPatternMatchOverride : AutoOrgPatternMatchOverride, IOrgPatternMatchOverride
	{
		public OrgPatternMatchOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Properties

		#region OO_Relationship

		[List("Lookups.OO_Relationship_List")]
		public override ZString OO_Relationship
		{
			get { return base.OO_Relationship; }
			set
			{
				base.OO_Relationship = value;
				OO_LocalCode = ZString.Empty;
				OO_LocalGuid = ZGuid.Empty;
			}
		}

		public ZString OrgCoFieldType
		{
			get
			{
				bool chc = OO_Relationship == Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
				return IsNotGuid ? (chc ? nameof(FieldType.TextCodeFindBox) : nameof(FieldType.TextDropEdit)) : nameof(FieldType.Guid);
			}
		}

		public ZPropertyInfo OrgCoFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OrgCoFieldType)); }
		}

		[RelatedBusinessObjectTestExclude("RelatedBusinessObject OrgCoBusinessObject (BusinessObject) is an abstract class")]
		[BusinessObjectTestExclude]
		[ResourceStringData("OrOrgPatternMatchOverride.OrgCoNameorGuid", Caption = "Local Code")]
		[RelatedBusinessObject("OrgCoBusinessObject")]
		[List("Lookups.OrgCoNames")]
		public ZString OrgCoNameorGuid
		{
			get
			{
				ZString result = "";

				if (IsNotGuid)
				{
					result = OO_LocalCode;
				}
				else if (OO_LocalGuid.IsValid)
				{
					result = OO_LocalGuid.ToString();
				}
				else
				{
					result = ZGuid.Empty.ToString();
				}

				return result;
			}
			set
			{
				if (IsNotGuid)
				{
					OO_LocalCode = value;
				}
				else
				{
					try
					{
						OO_LocalGuid = new Guid(value);
					}
					catch (FormatException)
					{
					}
				}
				OrgCoNameorGuidInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OrgCoNameorGuidInfo
		{
			get { return GetZPropertyInfo(nameof(OrgCoNameorGuid)); }
		}

		public BusinessObject OrgCoBusinessObject => !ZGuid.IsGuid(OrgCoNameorGuid) ? null : this.GetBusinessObjectFromGuid();

		[ReadOnlyMember(nameof(IsNotCode))]
		public override ZString OO_LocalCode
		{
			get { return base.OO_LocalCode; }
			set { base.OO_LocalCode = value; }
		}

		public override ZPropertyInfo OO_LocalCodeInfo
		{
			get { return GetZPropertyInfo(OrgPatternMatchOverrideSchema.Constants.OO_LocalCode, RelationshipCaption); }
		}

		[ReadOnlyMember(nameof(IsNotGuid))]
		public override ZGuid OO_LocalGuid
		{
			get { return base.OO_LocalGuid; }
			set { base.OO_LocalGuid = value; }
		}

		public override ZPropertyInfo OO_LocalGuidInfo
		{
			get { return GetZPropertyInfo(OrgPatternMatchOverrideSchema.Constants.OO_LocalGuid, RelationshipCaption); }
		}

		public bool IsNotGuid
		{
			get
			{
				return
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.Organisation &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.Port &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.Currency &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.Country &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.Commodities &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.Equipment &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.ContainerType &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.Warehouse &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.ServiceLevel &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.IntZone &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.DocumentType;
			}
		}

		public virtual bool IsNotCode
		{
			get
			{
				return
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.IncoTerm &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.ChargeCodes &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.DropMode &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.EventCode &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.PackageType &&
					OO_Relationship != Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel;
			}
		}

		[List("Lookups.OO_Context_List")]
		public override ZString OO_Context { get => base.OO_Context; set => base.OO_Context = value; }

		string RelationshipCaption
		{
			get { return Lookups.OO_Relationship_List.GetDescriptionFromCode(OO_Relationship); }
		}

		#endregion

		[List("Lookups.OO_ForeignCode_List")]
		public override ZString OO_ForeignCode { get => base.OO_ForeignCode; set => base.OO_ForeignCode = value; }

		public ZString OO_ForeignCodeFieldType
		{
			get { return IsComPayMapping ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text); }
		}

		internal bool IsComPayMapping
		{
			get
			{
				return OO_Relationship == Constants.OrgPatternMatchOverrideRelationships.ChargeCodes
					&& ObjectFactory.Get<IAccounting>().IsENettOrganisation(OO_OH);
			}
		}

		#endregion

		#region Lists

		CodeDescriptionPairList feNettGenericChargeCodes;
		public CodeDescriptionPairList eNettGenericChargeCodes
		{
			get { return feNettGenericChargeCodes ?? (feNettGenericChargeCodes = new eNettGenericChargeCodeList()); }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (Header != null)
			{
				shouldBeReadOnly = !Header.SecurityProvider.HasModifyConfigEDICodeMappingSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		public static ZString GetMappingForOrganisationByLocalCode(ZGuid organisationPK, string relationship, ZString localCode, BusinessObjectFactory factory)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, relationship);
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, organisationPK);
			query.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalCode, localCode);

			OrgPatternMatchOverride patternMatchOverride = factory.LoadTop1<OrgPatternMatchOverride>(query);
			return patternMatchOverride == null ? ZString.Empty : patternMatchOverride.OO_ForeignCode;
		}

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (OO_OH.IsEmpty)
			{
				OO_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}

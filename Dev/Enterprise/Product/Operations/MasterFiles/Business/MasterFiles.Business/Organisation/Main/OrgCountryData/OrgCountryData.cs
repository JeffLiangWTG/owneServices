using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgCountryData : AutoOrgCountryData, IOrgCountryData, ICusCodeDataTypeSupporter
	{
		public OrgCountryData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Type Decider

		public static readonly OrgCountryDataTypeDecider TypeDecider = new OrgCountryDataTypeDecider();

		#endregion

		#region Properties

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid OV_OH_OrgHeader
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.OV_OH_OrgHeader; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.OV_OH_OrgHeader = value; }
		}

		#region OV_OA_ApprovedLocation

		[List("OrgHeader.AddressesActive")]
		public override ZGuid OV_OA_ApprovedLocation
		{
			get { return base.OV_OA_ApprovedLocation; }
			set
			{
				base.OV_OA_ApprovedLocation = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOV_EXApprovedOrMajorExporter();
					RevalidateOrgApprovalsIfOrgLevelApprovalApplies();
				}
			}
		}

		public bool OV_OA_ApprovedLocation_ReadOnly => SupplyChainSecurityConfiguration.ApprovalCodeAddressIsReadOnly(OV_EXApprovedOrMajorExporter);

		#endregion

		#region OV_EXApprovedOrMajorExporter

		[List("Lookups.AviationSecuritySchemeMembershipList")]
		public override ZString OV_EXApprovedOrMajorExporter
		{
			get { return base.OV_EXApprovedOrMajorExporter; }
			set
			{
				if (base.OV_EXApprovedOrMajorExporter != value)
				{
					base.OV_EXApprovedOrMajorExporter = value;

					if (OrgHeader != null
						&& OV_OA_ApprovedLocation.IsEmpty
						&& SupplyChainSecurityConfiguration.IsAddressLevelScheme)
					{
						var defaultAddress = SupplyChainSecurityConfiguration.ApprovalCodeDefaultAddress(value, OrgHeader);
						if (defaultAddress != null)
						{
							OV_OA_ApprovedLocation = defaultAddress.PK;
						}
					}

					if (!OV_EXApprovalNumber.IsEmpty && !SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(value))
					{
						OV_EXApprovalNumber = ZString.Empty;
					}

					if (!SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(value))
					{
						OV_EXApprovalExpiryDate = ZDate.Empty;
					}

					OV_EXApprovalNumberInfo.RefreshBinding();
					OV_EXApprovalExpiryDateInfo.RefreshBinding();
					OV_OA_ApprovedLocationInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateOV_EXApprovalNumber();
						Validation.ValidateOV_EXApprovalExpiryDate();
						RevalidateOrgApprovalsIfOrgLevelApprovalApplies();
					}
				}
			}
		}

		void RevalidateOrgApprovalsIfOrgLevelApprovalApplies()
		{
			if (SupplyChainSecurityConfiguration.IsAddressLevelScheme
				&& OrgHeader != null
				&& OrgLevelApprovalApplies)
			{
				foreach (var orgCountryData in OrgHeader.Addresses
					.Cast<OrgAddress>()
					.SelectMany(x => x.KnownShipperDetails)
					.Where(x => x.PK != PK))
				{
					orgCountryData.Validation.ValidateOV_EXApprovedOrMajorExporter();
				}
			}
		}

		internal bool OrgLevelApprovalApplies
		{
			get
			{
				if (orgLevelApprovalApplies == null)
				{
					orgLevelApprovalApplies = false;

					foreach (CodeDescriptionPair codeDescriptionPair in SupplyChainSecurityConfiguration.ApprovalCodesList)
					{
						if (SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval(codeDescriptionPair.Code))
						{
							orgLevelApprovalApplies = true;
							break;
						}
					}
				}

				return orgLevelApprovalApplies.Value;
			}
		}
		bool? orgLevelApprovalApplies;

		protected bool OV_EXApprovalExpiryDate_ReadOnly
		{
			get { return !SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(OV_EXApprovedOrMajorExporter); }
		}

		#endregion

		#region OV_EXApprovalNumber

		public override ZString OV_EXApprovalNumber
		{
			get { return base.OV_EXApprovalNumber; }
			set
			{
				if (OV_EXApprovalNumber != value)
				{
					base.OV_EXApprovalNumber = value;

					if (!OV_EXApprovalNumber.IsEmpty
						&& OrgHeader != null
						&& SupplyChainSecurityConfiguration.ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(OV_EXApprovedOrMajorExporter)
						&& SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberMustMatchRequiredDocumentNumber(OV_EXApprovedOrMajorExporter))
					{
						var requiredDocType = ApprovedOrganisationRequiredDocType;
						if (!requiredDocType.IsEmpty)
						{
							var requiredDocument = OrgHeader.RequiredDocuments.Cast<JobRequiredDocument>()
								.OrderByDescending(x => x.EQ_ValidToDate)
								.FirstOrDefault(x => x.EQ_DocType == requiredDocType && x.EQ_DocNumber == OV_EXApprovalNumber && x.EQ_ValidToDate >= ZDateTime.Today);

							if (requiredDocument != null && SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(OV_EXApprovedOrMajorExporter))
							{
								OV_EXApprovalExpiryDate = requiredDocument.EQ_ValidToDate.Date;
							}
						}
					}
				}
			}
		}

		protected bool OV_EXApprovalNumber_ReadOnly
		{
			get
			{
				return !Env.Security.OrgConsignorModifyExporterScheme.IsAllowed
					|| (SupplyChainSecurityConfiguration.IsEnabled && !SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(OV_EXApprovedOrMajorExporter))
					|| (!SupplyChainSecurityConfiguration.IsEnabled && OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembershipEx.Codes.No);
			}
		}

		#endregion

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			OV_RN_NKClientCountryRelation = DefaultCountryValue;
			OV_RN_NKIssuingAuthorityCountry = GlbCompany.CurrentCompany.Country.RN_Code;
			OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;
			if (Constants.CountryCodes.GetCustomsCountryOfJurisdiction(OV_RN_NKClientCountryRelation) == Constants.CountryCodes.UnitedStates)
			{
				var defaultAddInfoName = OrgCountryDataSchema.Constants.OV_ImportCustomsDefaultAddInfo.Remove(0, 3);
				var addInfoFieldName = USAddInfoSchema.Constants.US_OtherReconIndicator.Remove(0, 3);
				var addInfoXMLString = "<" + defaultAddInfoName + "><" + addInfoFieldName + ">NA</" + addInfoFieldName + "></" + defaultAddInfoName + ">";
				OV_ImportCustomsDefaultAddInfo = addInfoXMLString;
			}
		}

		protected virtual ZString DefaultCountryValue
		{
			get { return GlbCompany.CurrentCompany.Country.RN_Code; }
		}

		#endregion

		#region Aviation Security Scheme

		public virtual void CalculateExporterStatus()
		{ }

		internal ZString ApprovedOrganisationRequiredDocType
		{
			get { return SupplyChainSecurityConfiguration.UseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry ? FreightDataRegistry.Instance.ApprovedOrganisationRequiredDocType.Value : Constants.RefDocTypes.KnownConsignorAgreement; }
		}

		public ShipmentSummaryCollection ShipmentsForAviationSecurityApproval
		{
			get { return shipmentsForAviationSecurity ?? (shipmentsForAviationSecurity = GetShipmentsForAviationSecurityApproval()); }
		}
		ShipmentSummaryCollection shipmentsForAviationSecurity;

		public void ResetShipmentsForAviationSecurityApproval()
		{
			shipmentsForAviationSecurity = null;
		}

		protected virtual ShipmentSummaryCollection GetShipmentsForAviationSecurityApproval()
		{
			return new ShipmentSummaryCollection();
		}

		public virtual ZString AviationSecurityStatusMessage
		{
			get { return ZString.Empty; }
		}

		public virtual ZString AviationSecurityStatusExplanation
		{
			get { return ZString.Empty; }
		}

		#region SupplyChainSecurityConfiguration

		public ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get
			{
				if (OrgHeader != null)
				{
					return OrgHeader.SupplyChainSecurityConfiguration;
				}

				if (supplyChainSecurityConfigurationHelper == null)
				{
					supplyChainSecurityConfigurationHelper = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>();
				}

				return supplyChainSecurityConfigurationHelper.GetConfigurationForCountry(OV_RN_NKClientCountryRelation);
			}
		}
		ISupplyChainSecurityConfigurationHelper supplyChainSecurityConfigurationHelper;

		#endregion

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new OrgCountryDataUniqueIndexFailureHandler(this); }
		}

		#endregion

		#region AddInfo

		internal bool AddedThroughCollection { get; set; }

		public XmlAddInfo ImpAddInfo
		{
			get
			{
				if (impAddInfo == null)
				{
					var type = new ImpAddInfoTypeDecider().GetCountrySpecificTypeForNewOrgAddInfo(this);
					impAddInfo = (XmlAddInfo)Activator.CreateInstance(type, OV_ImportCustomsDefaultAddInfoInfo);
					RegisterEditableChildObject(impAddInfo);
				}
				return impAddInfo;
			}
		}
		XmlAddInfo impAddInfo;

		public XmlAddInfo RegionSpecificImpAddInfo
		{
			get
			{
				if (regionSpecificImpAddInfo == null)
				{
					var type = new ImpAddInfoTypeDecider().GetRegionSpecificTypeForNewOrgAddInfo(this);
					regionSpecificImpAddInfo = (XmlAddInfo)Activator.CreateInstance(type, OV_CustomsEconomicGroupAddInfoInfo);
					RegisterEditableChildObject(regionSpecificImpAddInfo);
				}
				return regionSpecificImpAddInfo;
			}
		}
		XmlAddInfo regionSpecificImpAddInfo;

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			impAddInfo?.Deserialise();
			regionSpecificImpAddInfo?.Deserialise();
		}

		#endregion

		#region ReadOnlySecurity

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (OrgHeader != null)
			{
				shouldBeReadOnly = !OrgHeader.SecurityProvider.HasModifyConsignorExporterSchemeSecurity;

				if (property.Name.ToUpper().Contains("SYSTEM"))
				{
					shouldBeReadOnly = true;
				}
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get
			{
				bool result = base.IsSavedByFactory;
				if (result && !IsDeleted && OV_OH_OrgHeader.IsValid)
				{
					OrgHeader org = OrgHeader;
					result = org != null && (org.IsInDatabase || org.IsSavedByFactory);
				}
				return result;
			}
		}

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				ObjectFactory.Get<IChildrenDeletionHelper>().DeleteChildren(this);
			}
			base.Delete();
		}

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusCodeDataTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.IORBusinessRules, ObjectFactory.GetType<US.IRestrictedCode>());
			result.Add(CusCodeDataTypeList.Codes.ImportersControlledGroupName, ObjectFactory.GetType<US.IImportersControlledGroupName>());
			result.Add(CusCodeDataTypeList.Codes.AllocationQuantityPerFPI, ObjectFactory.GetType<US.IAllocationQuantityPerFPI>());
			return result;
		}

		public static class CusCodeDataTypeList
		{
			public static class Codes
			{
				public const string IORBusinessRules = "IBR";
				public const string ImportersControlledGroupName = "ICG";
				public const string AllocationQuantityPerFPI = "AQF";
			}
		}

		#endregion
	}
}

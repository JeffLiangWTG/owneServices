using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Telematics.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AutoRefEquipment.Schema.RQ_ShortCode), DescriptionProperty(AutoRefEquipment.Schema.RQ_Description)]
	public class RefEquipment : AutoRefEquipment, IRefEquipment, IDocManagerSupport, ICertificatesProvider, ICertificatesValidationProvider
	{
		public RefEquipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Delete

		public override void Delete()
		{
			DeleteActivities();
			Certificates.DeleteAll();
			ClearLastUsedEquipmentFKs();

			base.Delete();
		}

		void DeleteActivities()
		{
			var activityQuery = new ZQuery(LocalCartageVehicleActivitySchema.EN_RQ_Vehicle, PK);
			var activities = (BusinessObject[])Factory.Load<Enterprise.Integration.GPS.IGPSSupporterActivity>(activityQuery);
			foreach (var bo in activities)
			{
				bo.Delete();
			}
		}

		void ClearLastUsedEquipmentFKs()
		{
			var rfRegistries = Factory.Load<IWhsRFRegistry>(new ZQuery(WhsRFRegistrySchema.WRR_RQ_LastUsedEquipment, PK));
			foreach (var registry in rfRegistries)
			{
				registry.WRR_RQ_LastUsedEquipment = ZGuid.Empty;
			}
		}

		#endregion

		#region Property Overrides

		[ReadOnlyMember(nameof(IsNotVehicle))]
		public override ZString RQ_GeoProviderID
		{
			get { return base.RQ_GeoProviderID; }
			set { base.RQ_GeoProviderID = value; }
		}

		[List("Lookups.GPSProviders")]
		[ReadOnlyMember(nameof(IsNotVehicle))]
		public override ZString RQ_GeoProviderType
		{
			get { return base.RQ_GeoProviderType; }
			set { base.RQ_GeoProviderType = value; }
		}

		protected bool IsNotVehicle
		{
			get { return !RQ_IsVehicle; }
		}

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		public override ZString RQ_ShortCode
		{
			get { return base.RQ_ShortCode; }
			set
			{
				base.RQ_ShortCode = value;
				if (RQ_Registration.IsEmpty)
				{
					RQ_Registration = RQ_ShortCode.SubstringSafe(0, RefEquipmentSchema.RQ_Registration.MaxLength);
				}
			}
		}

		[List("Lookups.RQ_F3_NKPackType_List")]
		public override ZString RQ_F3_NKPackType
		{
			get
			{
				return base.RQ_F3_NKPackType;
			}
			set
			{
				base.RQ_F3_NKPackType = value;
			}
		}

		[List("Lookups.RQ_EquipmentGroup_List")]
		public override ZString RQ_EquipmentGroup
		{
			get
			{
				return base.RQ_EquipmentGroup;
			}
			set
			{
				base.RQ_EquipmentGroup = value;
			}
		}

		[List("RQ_UnitOfWeight_List")]
		public override ZString RQ_WeightUnit
		{
			get
			{
				return base.RQ_WeightUnit;
			}
			set
			{
				base.RQ_WeightUnit = value;
			}
		}

		[List("RQ_UnitOfVolume_List")]
		public override ZString RQ_CubicUnit
		{
			get
			{
				return base.RQ_CubicUnit;
			}
			set
			{
				base.RQ_CubicUnit = value;
			}
		}

		[List("RQ_GS_NKPreferredDriverList")]
		public override ZString RQ_GS_NKPreferredDriver
		{
			get
			{
				return base.RQ_GS_NKPreferredDriver;
			}
			set
			{
				base.RQ_GS_NKPreferredDriver = value;
			}
		}

		[List("Lookups.RefCountryStatesList")]
		public override ZString RQ_RegState
		{
			get
			{
				return base.RQ_RegState;
			}
			set
			{
				base.RQ_RegState = value;
			}
		}

		[List("EquipmentTypeList")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Setting default English RQ_Description from default English RC_Description")]
		public override ZGuid RQ_RC_RoadContainerType
		{
			get { return base.RQ_RC_RoadContainerType; }
			set
			{
				var oldValue = base.RQ_RC_RoadContainerType;
				base.RQ_RC_RoadContainerType = value;
				if (oldValue != value && !IsCopying)
				{
					if (!value.IsEmpty)
					{
						var shouldDefault = RQ_Description.IsEmpty;
						if (!shouldDefault)
						{
							var oldRoadContainerType = Factory.Load<RefContainer>(oldValue);
							shouldDefault = oldRoadContainerType != null && RQ_Description == oldRoadContainerType.RC_Description;
						}
						if (shouldDefault && RoadContainerType != null)
						{
							RQ_Description = RoadContainerType.RC_Description.Left(RQ_DescriptionInfo.MaxLength);
						}
					}
				}
			}
		}

		[List("Lookups.RQ_OH_OwnerList")]
		public override ZGuid RQ_OH_Owner
		{
			get { return base.RQ_OH_Owner; }
			set
			{
				var oldValue = base.RQ_OH_Owner;
				base.RQ_OH_Owner = value;
				if (oldValue != value && !IsCopying)
				{
					var header = Factory.Load<OrgHeader>(value);
					if (header != null && RQ_RN_NKRegistrationCountry.IsEmpty)
					{
						RQ_RN_NKRegistrationCountry = header.OH_RL_NKClosestPort.SubstringSafe(0, 2);
					}
				}
			}
		}

		public ZGuid OwnerBranchPK
		{
			get
			{
				ZGuid branchPK = ZGuid.Empty;
				if (RQ_OH_Owner.IsValid)
				{
					var branch = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, RQ_OH_Owner)).FirstOrDefault();
					if (branch != null)
					{
						branchPK = branch.PK;
					}
				}
				return branchPK;
			}
		}

		public override ZString RQ_RN_NKRegistrationCountry
		{
			get { return base.RQ_RN_NKRegistrationCountry; }
			set
			{
				var oldValue = base.RQ_RN_NKRegistrationCountry;
				base.RQ_RN_NKRegistrationCountry = value;
				if (oldValue != value && !IsCopying)
				{
					RQ_RegState = string.Empty;
				}
			}
		}

		#endregion

		#region HasTelematics

		public ZBool HasTelematics
		{
			get { return Factory.LoadTop1(ObjectFactory.GetType<IDeviceAssignmentDivot>(), new ZQuery(GlbDeviceAssignmentDivotSchema.V7_ParentID, PK)) != null; }
		}

		#endregion

		#region GPS Stuff

		public void AddGPSWarning(string warning)
		{
			GPSWarning = warning;
			Validation.ValidateRQ_GeoProviderType();
			GPSWarning = "";
		}
		internal string GPSWarning;

		#endregion

		#region Lookups

		public RefContainerCollection EquipmentTypeList
		{
			get
			{
				if (fEquipmentTypeList == null)
				{
					fEquipmentTypeList = new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Road);
				}

				return fEquipmentTypeList;
			}
		}

		RefContainerCollection fEquipmentTypeList;

		CodeDescriptionPairList fRQ_UnitOfWeight_List;
		public CodeDescriptionPairList RQ_UnitOfWeight_List
		{
			get
			{
				if (fRQ_UnitOfWeight_List == null)
				{
					fRQ_UnitOfWeight_List = new CodeDescriptionPairList();
					fRQ_UnitOfWeight_List.AddPair(Constants.Weight.Grams, Res.GetString("fe8a0c8c-c426-4ec6-9710-35d9c285bc2f", "Grams"));
					fRQ_UnitOfWeight_List.AddPair(Constants.Weight.Kilograms, Res.GetString("a1580770-390e-492b-b963-092ad3311bfc", "Kilograms"));
					fRQ_UnitOfWeight_List.AddPair(Constants.Weight.Pounds, Res.GetString("5d87be92-708a-41c2-927b-c1d624ea552e", "Pounds"));
					fRQ_UnitOfWeight_List.AddPair(Constants.Weight.Tonnes, Res.GetString("2ea6313e-ddf6-4329-b555-abc127579c86", "Tonnes"));
					fRQ_UnitOfWeight_List.AddPair(Constants.Weight.ShortTons, Res.GetString("53d25a2e-92cd-4d06-a7a8-7b26f17ef0a0", "Short Tons (2000 lb)"));
					fRQ_UnitOfWeight_List.AddPair(Constants.Weight.LongTons, Res.GetString("d13e14df-a208-4d03-bcfc-f04fe799f3ee", "Long Tons (2240 lb)"));
				}
				return fRQ_UnitOfWeight_List;
			}
		}

		public CodeDescriptionPairList RQ_UnitOfVolume_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		StaffDriverCollection fRQ_GS_NKPreferredDriverList;
		public StaffDriverCollection RQ_GS_NKPreferredDriverList
		{
			get
			{
				if (fRQ_GS_NKPreferredDriverList == null)
				{
					fRQ_GS_NKPreferredDriverList = new StaffDriverCollection(Factory);
				}
				return fRQ_GS_NKPreferredDriverList;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Equipment);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region ICertificatesProvider Members

		[ChildEditable(true)]
		public GenRegCertAccredMaintListCollection Certificates
		{
			get
			{
				if (certificates == null)
				{
					certificates = new GenRegCertAccredMaintListCollection(this);
					RegisterEditableChildObject(certificates);
				}

				return certificates;
			}
		}
		GenRegCertAccredMaintListCollection certificates;

		ICodeDescriptionPairList ICertificatesProvider.GetCertificateTypeList()
		{
			return ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes.Value;
		}

		public ICodeDescriptionPairList GetActiveCertificateTypeList()
		{
			//Registry values cannot be disabled.
			return ((ICertificatesProvider)this).GetCertificateTypeList();
		}

		ZString ICertificatesProvider.GetDefaultDescription(ZString code)
		{
			var type = ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes.Value.Find(code);
			return type != null ? type.Description : ZString.Empty;
		}

		GenRegCertAccredMaintListValidation ICertificatesValidationProvider.GetValidation(GenRegCertAccredMaintList parent)
		{
			var result = new EquipmentCertificatesValidation(parent);
			foreach (IDefaultCertificateTypesProvider provider in ObjectFactory.Get<IEnumerable>("EquipmentCertificateTypesProviders"))
			{
				var validation = provider.GetDefaultTypesSpecificValidation(parent) as AutoGenRegCertAccredMaintListValidation;
				if (validation != null)
				{
					result.Add(validation);
				}
			}
			return result;
		}

		#endregion

		[TranslatableDataField(Schema.TableName, Schema.RQ_Description, MaxLength = Schema.RQ_DescriptionMaxLength, Type = typeof(RefEquipment), SecurityCheckpoint = "EquipmentModify", Asmid = ResString.AssemblyId)]
		public override ZString RQ_Description
		{
			get { return base.RQ_Description; }
			set { base.RQ_Description = value; }
		}

		public MultilingualString RQ_DescriptionMultilingual
		{
			get { return GetMultilingual(RQ_DescriptionInfo); }
		}

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			AddMandatoryCertificates();
			base.RunPreSaveValidationCore();
		}

		void AddMandatoryCertificates()
		{
			foreach (CertificateType type in ReferenceFilesDataRegistry.Instance.EquipmentCertificateTypes.Value)
			{
				if (type.IsMandatory && Certificates.GetFirstCertificate(type.Code) == null)
				{
					GenRegCertAccredMaintList mandatory = Certificates.AddNew();
					mandatory.XZ_Type = type.Code;
				}
			}
		}

		#endregion
	}
}

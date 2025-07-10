using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgContainerDetentionCollection;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgContainerDetention : AutoOrgContainerDetention, IContainerPenaltyMatchResult
	{
		public OrgContainerDetention(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Properties

		[List("Lookups.Directions")]
		public override ZString PD_Direction
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_Direction; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.PD_Direction = value;
				SetDefaultFreeDayType();
			}
		}

		[List("Lookups.PenaltyTypes")]
		public override ZString PD_PenaltyType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_PenaltyType; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.PD_PenaltyType = value;
				if (value == Core.Constants.ContainerDetentionPenaltyType.DET)
				{
					PD_OH_CTO = ZGuid.Empty;
				}

				SetDefaultFreeDayType();
			}
		}

		public ZString PenaltyDescription
		{
			get
			{
				if (PD_Direction == Core.Constants.ContainerDetentionDirection.Import && PD_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.DET)
				{
					return Res.GetString("9c6a7cd7-329b-4dcb-8069-182552e28a45", "Import Detention");
				}
				else if (PD_Direction == Core.Constants.ContainerDetentionDirection.Export && PD_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.DET)
				{
					return Res.GetString("55bc0e3b-2d9f-459e-9c3d-7a193882ee14", "Export Detention");
				}
				else if (PD_Direction == Core.Constants.ContainerDetentionDirection.Import && PD_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO)
				{
					return Res.GetString("d820f168-e551-4f8e-a3d1-8fe5599cdc0a", "Import Demurrage");
				}
				else if (PD_Direction == Core.Constants.ContainerDetentionDirection.Export && PD_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO)
				{
					return Res.GetString("7832fa59-9328-4d36-90c6-ab8d8ceecc09", "Export Demurrage");
				}
				else if (PD_Direction == Core.Constants.ContainerDetentionDirection.Import && PD_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.MDD)
				{
					return Res.GetString("cc347c35-70a3-4c5a-891a-d2371118eef0", "Import Merged Demurrage and Detention");
				}
				else if (PD_Direction == Core.Constants.ContainerDetentionDirection.Export && PD_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.MDD)
				{
					return Res.GetString("ca60d9cc-5003-4e7d-bb56-8387f9011913", "Export Merged Demurrage and Detention");
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo PenaltyDescriptionInfo => GetZPropertyInfo(nameof(PenaltyDescription));

		[List("Lookups.Carriers")]
		public override ZGuid PD_OH_Carrier
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_OH_Carrier; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PD_OH_Carrier = value; }
		}

		[List("Lookups.Clients")]
		public override ZGuid PD_OH_Client
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_OH_Client; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PD_OH_Client = value; }
		}

		[List("Lookups.StorageCTOs")]
		public override ZGuid PD_OH_CTO
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_OH_CTO; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PD_OH_CTO = value; }
		}

		protected bool PD_OH_CTO_ReadOnly => PD_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.DET;

		[List("Lookups.Locations")]
		public override ZString PD_OriginPortOrCountry
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_OriginPortOrCountry; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PD_OriginPortOrCountry = value; }
		}

		[List("Lookups.Locations")]
		public override ZString PD_DetentionPortOrCountry
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_DetentionPortOrCountry; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PD_DetentionPortOrCountry = value; }
		}

		[List("Lookups.StorageClasses")]
		public override ZString PD_ContainerType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PD_ContainerType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PD_ContainerType = value; }
		}

		[List("Lookups.CreditorTypes")]
		public override ZString PD_CreditorType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => base.PD_CreditorType;
			[System.Diagnostics.DebuggerStepThrough]
			set => base.PD_CreditorType = value;
		}

		[List("Lookups.FreeDayTypes")]
		public override ZString PD_FreeDayType
		{
			get => base.PD_FreeDayType;
			set => base.PD_FreeDayType = value;
		}

		/// <summary>
		/// Proxy of PD_FreeDayType for when PD_Direction is EXP (Export)
		/// </summary>
		[List("Lookups.FirstFreeDayTypes")]
		[ResourceStringData("c3801683-b7be-4569-bf55-f4701d674715", Caption = "1st Free Day")]
		public ZString PD_FirstFreeDayType
		{
			get => PD_FirstFreeDayType_ReadOnly
				? ZString.Empty
				: PD_FreeDayType;

			set => PD_FreeDayType = value;
		}

		public ZPropertyInfo PD_FirstFreeDayTypeInfo => GetWrappedZPropertyInfo(nameof(PD_FirstFreeDayType), _ => PD_FreeDayTypeInfo);

		public bool PD_FirstFreeDayType_ReadOnly => PD_Direction != Core.Constants.ContainerDetentionDirection.Import;

		/// <summary>
		/// Proxy of PD_FreeDayType for when PD_Direction is IMP (Import)
		/// </summary>
		[List("Lookups.LastFreeDayTypes")]
		[ResourceStringData("8e224a07-4538-4244-934d-4e96a6c01486", Caption = "Last Free Day")]
		public ZString PD_LastFreeDayType
		{
			get => PD_LastFreeDayType_ReadOnly
				? ZString.Empty
				: PD_FreeDayType;

			set => PD_FreeDayType = value;
		}

		public ZPropertyInfo PD_LastFreeDayTypeInfo => GetWrappedZPropertyInfo(nameof(PD_LastFreeDayType), _ => PD_FreeDayTypeInfo);

		public bool PD_LastFreeDayType_ReadOnly => PD_Direction != Core.Constants.ContainerDetentionDirection.Export;

		protected override ZString HumanReadableNameCore
		{
			get => PD_PenaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.STO
				? Res.GetString("ed83d8b9-04fa-4188-b9e6-ea6ba73f704e", "CTO Storage")
				: Res.GetString("daa8ad83-9667-49ce-8149-c4aa959c2ef5", "Container Detention");
		}

		void SetDefaultFreeDayType()
		{
			if (PD_Direction == Enterprise.Core.Constants.ContainerDetentionDirection.Export)
			{
				if (PD_PenaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.STO || PD_PenaltyType == Enterprise.Core.Constants.ContainerDetentionPenaltyType.MDD)
				{
					PD_FreeDayType = Enterprise.Core.Constants.ContainerDetentionFreeDayType.FCLLoad;
				}
				else
				{
					PD_FreeDayType = Enterprise.Core.Constants.ContainerDetentionFreeDayType.WharfGateIn;
				}
			}
		}

		[ReadOnly(true)]
		public override ZGuid PD_CEX_DurationExclusion
		{
			get => base.PD_CEX_DurationExclusion;
			set => base.PD_CEX_DurationExclusion = value;
		}

		public ContainerPenaltyDayExclusion DurationExclusionForBinding
		{
			get
			{
				var durationExclusion = Factory.Load<ContainerPenaltyDayExclusion>(PD_CEX_DurationExclusion);
				if (durationExclusion == null)
				{
					durationExclusion = Factory.New<ContainerPenaltyDayExclusion>();
					PD_CEX_DurationExclusion = durationExclusion.PK;
				}

				RegisterEditableChildObject(durationExclusion);
				return durationExclusion;
			}
		}

		[ReadOnly(true)]
		public override ZGuid PD_CEX_FreeDayExclusion
		{
			get => base.PD_CEX_FreeDayExclusion;
			set => base.PD_CEX_FreeDayExclusion = value;
		}

		IContainerPenaltyDayExclusion IContainerPenaltyMatchResult.FreeDayExclusion => FreeDayExclusion;

		IContainerPenaltyDayExclusion IContainerPenaltyMatchResult.DurationExclusion => DurationExclusion;

		public ContainerPenaltyDayExclusion FreeDayExclusionForBinding
		{
			get
			{
				var freeDayExclusion = FreeDayExclusion;
				if (freeDayExclusion == null)
				{
					freeDayExclusion = Factory.New<ContainerPenaltyDayExclusion>();
					PD_CEX_FreeDayExclusion = freeDayExclusion.PK;
				}

				RegisterEditableChildObject(freeDayExclusion);
				return freeDayExclusion;
			}
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !Env.Security.OrgCarrierModify.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var row = ((IBusinessObjectInternals)this).Row;
			row[OrgContainerDetentionSchema.Constants.PD_FreeDayType] = "CTD";
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			DurationExclusion?.Delete();
			FreeDayExclusion?.Delete();
			base.Delete();
		}

		#endregion

		internal ParentType ParentType { get; set; }

		public ZByte FreeDays => PD_FreeDays;

		public ZString FreeDayType => PD_FreeDayType;

		public ZString PenaltyType => PD_PenaltyType;

		public ZString CreditorType => PD_CreditorType;
	}
}

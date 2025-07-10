using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using static Enterprise.Customs.US.Business.UNLOCO_USPortsDefaulter;

#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportVisitedPort : CusCodeData, IShortSequenceNumberLine
	{
		public USExportVisitedPort(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		#region Override

		public new USExportVisitedPortValidation Validation => (USExportVisitedPortValidation)base.Validation;
		protected override CusCodeDataValidation GetNewValidation() => new USExportVisitedPortValidation(this);
		public new USExportVisitedPortLookups Lookups => (USExportVisitedPortLookups)base.Lookups;
		protected override CusCodeDataLookups GetNewLookups() => new USExportVisitedPortLookups(this);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(USExportAsycudaBill), typeof(USExportAsycudaManifestHeader));

		public USExportAsycudaBill USExpAsycudaBill => Parent as USExportAsycudaBill;

		#endregion

		public ZString PortCodeFieldType
		{
			get
			{
				var result = FieldType.Text;
				if (PortDefaulter.HasMultipleMappingPorts)
				{
					result = FieldType.TextDropEdit;
				}
				return result.ToString();
			}
		}

		[List(nameof(Lookups) + "." + nameof(USExportVisitedPortLookups.ScheduleKCodeList))]
		[ResourceStringData("USExportVisitedPort.CY_Code", ShortCaption = "Port", FullDescription = "Schedule K Port (US)", Caption = "Schedule K Port(US)", MediumCaption = "Port(US)")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[List(nameof(Lookups) + "." + nameof(USExportVisitedPortLookups.UNLOCOCodeList))]
		[ResourceStringData("USExportVisitedPort.CY_Data", ShortCaption = "UNLOCO", FullDescription = "Schedule K Port (UNLOCO)", Caption = "Schedule K Port(UNLOCO)", MediumCaption = "Port(UNLOCO)")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = base.CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != value && CY_Code.IsEmpty)
				{
					PortDefaulter.DefaultPort();
				}
			}
		}

		UNLOCO_USPortsDefaulter fPortDefaulter;
		UNLOCO_USPortsDefaulter PortDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.K, CY_Data, USExpAsycudaBill?.ABL_Calc_AMA_TransportMode ?? ZString.Empty, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return fPortDefaulter ?? (fPortDefaulter = new UNLOCO_USPortsDefaulter(Factory, CY_CodeInfo, CY_DataInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}

		public BusinessObjectCollection PortDefaulterRefLocoMappings => PortDefaulter.MappingPorts;

		[ResourceStringData("USExportVisitedPorts.CY_Order", Caption = "Leg No.", ShortCaption = "Leg", FullDescription = "Leg Number", MediumCaption = "Leg No.")]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set
			{
				var oldValue = CY_Order;
				base.CY_Order = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}
		public ShortSequenceNumberGenerator SequenceGenerator => USExpAsycudaBill?.VisitedPorts.SequenceNumberCalculator;

		public override ZString CY_ParentTableCode
		{
			get => base.CY_ParentTableCode;
			set
			{
				var oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldValue != CY_ParentTableCode && !CY_ParentTableCode.IsEmpty)
				{
					AttachedLine();
				}
			}
		}
		public override ZGuid CY_ParentID
		{
			get => base.CY_ParentID;
			set
			{
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					if (!CY_ParentID.IsValid)
					{
						DetachedLine(oldValue);
					}
					else
					{
						AttachedLine();
					}
				}
			}
		}

		void DetachedLine(ZGuid oldValue)
		{
			var header = parentLoaders.LoadBusinessObject(Factory, CY_ParentTableCode, oldValue) as USExportAsycudaBill;
			if (header != null)
			{
				header.VisitedPorts.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedLine()
		{
			var header = USExpAsycudaBill;
			if (header != null)
			{
				header.VisitedPorts.SequenceNumberCalculator.RecalculateWhenAdded(this);
			}
		}
		#region IZShortSequenceNumberLine

		ZGuid ISequenceNumberLine.FKToHeader => USExpAsycudaBill.PK;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => CY_Order; set => CY_Order = value; }

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = USExportCusCodeType.Codes.UVP;
		}
	}
}

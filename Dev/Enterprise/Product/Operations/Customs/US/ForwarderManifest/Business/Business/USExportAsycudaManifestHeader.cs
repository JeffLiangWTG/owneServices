using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.USExportManifest.IAsycudaManifestHeader
	{
		public USExportAsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		internal const string US_Sea_Exp = "US Sea Exp";

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = $"{US_Sea_Exp} {AMA_JobReference}";
				if (!MasterBOL.IsEmpty)
				{
					result += $" - {MasterBill.ABL_BillIssuer}{MasterBOL}";
				}
				return result;
			}
		}

		public ASYCUDA.Business.AsycudaArrivalHeader ArrivalHeader
		{
			get
			{
				if (!IsDeleted && (arrivalHeader == null || arrivalHeader.IsDeleted))
				{
					arrivalHeader = ArrivalHeaders.Cast<ASYCUDA.Business.AsycudaArrivalHeader>().FirstOrDefault();
					if (arrivalHeader == null)
					{
						arrivalHeader = ArrivalHeaders.AddNew();
					}
					RegisterEditableChildObject(arrivalHeader);
				}
				return arrivalHeader;
			}
		}

		ASYCUDA.Business.AsycudaArrivalHeader arrivalHeader;

		public override ZString MasterBOL
		{
			get => base.MasterBOL;
			set
			{
				var oldValue = base.MasterBOL;
				if (oldValue != value)
				{
					fMasterBOLOldValue = oldValue;
					base.MasterBOL = value;
				}
			}
		}

		public ZString MasterBOLOldValue => fMasterBOLOldValue;
		ZString fMasterBOLOldValue;

		public ZBool AMA_CustomsFirstArrivalPortIsDropEdit => PortOfFirstArrivalDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaManifestHeaderLookups.CustomsFirstArrivalPortList))]
		public override ZString AMA_CustomsFirstArrivalPort
		{
			get => base.AMA_CustomsFirstArrivalPort;
			set => base.AMA_CustomsFirstArrivalPort = value;
		}

		public override ZString AMA_RL_NKPortOfFirstArrival
		{
			get => base.AMA_RL_NKPortOfFirstArrival;
			set
			{
				var hasChanges = base.AMA_RL_NKPortOfFirstArrival != value;
				base.AMA_RL_NKPortOfFirstArrival = value;
				if (!IsCopying && hasChanges)
				{
					PortOfFirstArrivalDefaulter.DefaultPort();
				}
			}
		}

		public ZBool AMA_CustomsFinalDeparturePortIsDropEdit => PortOfFinalDepartureDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaManifestHeaderLookups.CustomsFinalDeparturePortList))]
		public override ZString AMA_CustomsFinalDeparturePort
		{
			get => base.AMA_CustomsFinalDeparturePort;
			set => base.AMA_CustomsFinalDeparturePort = value;
		}

		public override ZGuid AMA_OA_Carrier
		{
			get
			{
				return new ZGuid(GetValueFromRowSafely(AsycudaManifestHeaderSchema.AMA_OA_Carrier));
			}
			set
			{
				var oldValue = AMA_OA_Carrier;
				if (oldValue != value)
				{
					base.AMA_OA_Carrier = value;
					if (Carrier != null)
					{
						var scacField = USLocalCustomsCarrierCode(Carrier.OA_OH);
						if (!scacField.IsEmpty)
						{
							MasterBill.ABL_BillIssuer = scacField;
						}
					}
				}
			}
		}

		public override ZString AMA_ApplicationCode
		{
			get => base.AMA_ApplicationCode;
			set
			{
				base.AMA_ApplicationCode = value;
				if (!IsValidationSuspended)
				{
					foreach (var bill in Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZString USLocalCustomsCarrierCode(ZGuid orgPk)
		{
			ZString result = ZString.Empty;
			if (orgPk != null)
			{
				var organisation = Factory.Load<OrgHeader>(orgPk);
				if (organisation != null)
				{
					result = organisation.CustomsCodes.GetCustomsRegNo("CCC", "US");
				}
			}

			return result;
		}

		public override ZString AMA_RL_NKPortOfFinalDeparture
		{
			get => base.AMA_RL_NKPortOfFinalDeparture;
			set
			{
				var hasChanges = base.AMA_RL_NKPortOfFinalDeparture != value;
				base.AMA_RL_NKPortOfFinalDeparture = value;
				if (!IsCopying && hasChanges)
				{
					PortOfFinalDepartureDefaulter.DefaultPort();
					PortOfFinalDepartureForSCDDefaulter.DefaultPort();
					AMA_CustomsFinalDeparturePortInfo.RefreshBinding();
				}
			}
		}

		UNLOCO_USPortsDefaulter PortOfFinalDepartureForSCDDefaulter
		{
			get
			{
				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes);
				}

				return portOfLadingDefaulter ?? (portOfLadingDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, MasterBill.ABL_CustomsLoadPortInfo, AMA_RL_NKPortOfFinalDepartureInfo, GetPortOfFinalDepartureMappings, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter portOfLadingDefaulter;

		public List<RefLocoMap> GetPortOfExportRefLocoMappings(string port, string mode)
		{
			return USScheduleResolver.GetMatchesForSchedule(Schedule.D, port, mode, Factory);
		}

		UNLOCO_USPortsDefaulter PortOfFirstArrivalDefaulter
		{
			get
			{
				List<RefLocoMap> GetPortOfArrivalMappings()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.K, AMA_RL_NKPortOfFirstArrival, ZString.Empty, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return portOfFirstArrivalDefaulter ?? (portOfFirstArrivalDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, AMA_CustomsFirstArrivalPortInfo, AMA_RL_NKPortOfFirstArrivalInfo, GetPortOfArrivalMappings, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter portOfFirstArrivalDefaulter;

		public BusinessObjectCollection PortOfFirstArrivalRefLocoMappings => PortOfFirstArrivalDefaulter.MappingPorts;

		UNLOCO_USPortsDefaulter PortOfFinalDepartureDefaulter
		{
			get
			{
				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes);
				}

				return portOfFinalDepartureDefaulter ?? (portOfFinalDepartureDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, AMA_CustomsFinalDeparturePortInfo, AMA_RL_NKPortOfFinalDepartureInfo, GetPortOfFinalDepartureMappings, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter portOfFinalDepartureDefaulter;

		List<RefLocoMap> GetPortOfFinalDepartureMappings()
		{
			return USScheduleResolver.GetMatchesForSchedule(Schedule.D, AMA_RL_NKPortOfFinalDeparture, AMA_TransportMode, Factory);
		}

		public BusinessObjectCollection PortOfFinalDepartureRefLocoMappings => PortOfFinalDepartureDefaulter.MappingPorts;

		public new USExportAsycudaBill MasterBill => (USExportAsycudaBill)base.MasterBill;
		protected override Type GetBillTypeCore() => typeof(USExportAsycudaBill);

		public new USExportAsycudaBillCollection Bills => (USExportAsycudaBillCollection)base.Bills;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new USExportAsycudaBillCollection(this);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.UnitedStates;

		public override ZBool SupportMultipleCustomsNumbers => true;

		protected override bool IsAMA_NatureReadOnly => true;

		[ReadOnly(true)]
		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set => base.AMA_ManifestType = value;
		}

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups()
		{
			return new USExportAsycudaManifestHeaderLookups(this);
		}

		public new USExportAsycudaManifestHeaderLookups Lookups => (USExportAsycudaManifestHeaderLookups)base.Lookups;

		protected override bool ShowVINNumbersCore => true;

		protected override ASYCUDA.Business.MessageChooser GetNewMessageChooserCore(IEnumerable<ASYCUDA.Business.ISelectionItem> items, string messageType, bool showStatus)
		{
			switch (messageType)
			{
				case MessageTypeList.Codes.ExportManifestSubmission:
					return new UEMMessageChooser(this, items);
			}

			return base.GetNewMessageChooserCore(items, messageType, showStatus);
		}

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new USExportAsycudaManifestHeaderSynchroniser(this, source);

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation()
		{
			return new USExportAsycudaManifestHeaderValidation(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using TransportTypeList = Enterprise.Customs.US.Business.TransportTypeList;

namespace Enterprise.Customs.US.LVS.Business
{
	[CodeProperty(CusUSLVClearance.Schema.ULH_JobNumber), DescriptionProperty(CusUSLVClearance.Schema.ULH_MasterBill)]
	[UniversalDataContext(DataContextType.USCustomsLowValueEntriesClearance)]
	[UserDefinedValues]
	public partial class CusUSLVClearance :
		AutoCusUSLVClearance,
		Integration.Customs.US.LVS.ICusUSLVClearance,
		IWorkflowProvider,
		IEDocsProvider,
		IDocumentSupportable,
		IHaveRequiredDocuments,
		IJobNumber,
		IValidateForCustomsMessagingSupporter,
		IAuthorityToActDeclarationProvider
	{
		public CusUSLVClearance(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Constants

		public new class Schema : AutoCusUSLVClearance.Schema
		{
			public const string SelectAllCusUSLVConsignmentsToSend = "SelectAllCusUSLVConsignmentsToSend";
		}

		#endregion

		#region VoyageFlightNumber

		public override ZString ULH_VoyageFlightNo
		{
			get => base.ULH_VoyageFlightNo;
			set
			{
				if (value != base.ULH_VoyageFlightNo)
				{
					base.ULH_VoyageFlightNo = value;
					airline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, ULH_VoyageFlightNo.Left(2)));
				}
			}
		}

		public ZString VoyageFlightNumber
		{
			get
			{
				var voyageNumber = ULH_VoyageFlightNo;
				var result = voyageNumber;

				if (ULH_TransportMode == TransportTypeList.Codes.Air)
				{
					result = GetFlightNo(voyageNumber);
				}
				return result.KeepAlphanumericCharacters();
			}
		}

		public RefAirline Airline => airline;
		RefAirline airline;

		public ZString AirlineCode => Airline?.RM_TwoCharacterCode ?? ULH_VoyageFlightNo.Left(2);

		ZString GetFlightNo(ZString flightNumber)
		{
			return Airline != null ? flightNumber.SubstringSafe(2) : (ZString)Regex.Replace(flightNumber, "^[a-z]{0,2}", "", RegexOptions.IgnoreCase);
		}

		#endregion

		#region Calculated Properties

		[ReadOnly(true)]
		[MaxLength(2)]
		public ZString ULH_Calc_USTransportMode => TransportModeCalculator.CalculateUSTransportMode(ULH_TransportMode, ULH_ContainerMode);

		#region Properties for Binding

		public ZBool IsNotEmptyForBinding => !ULH_TransportMode.IsEmpty;

		public ZBool IsContainerSupported => IsTruck || IsRail || IsAir || IsSea;

		public ZBool IsAirForBinding => IsAir;

		public ZBool IsSeaForBinding => IsSea;

		public ZBool IsRailForBinding => IsRail;

		public ZBool IsRoadOrTruckForBinding => IsRoad || IsTruck;

		public ZBool IsMailForBinding => IsMail;

		public ZBool IsRailOrRoadForBinding => IsRailForBinding || IsRoadOrTruckForBinding;

		public ZBool IsRailOrRoadOrMailForBinding => IsRailOrRoadForBinding || IsMailForBinding;

		#endregion

		#endregion

		#region CusUSLVConsignments

		[ChildEditable]
		public CusUSLVConsignmentCollection CusUSLVConsignments
		{
			get
			{
				if (cusUSLVConsignments == null)
				{
					cusUSLVConsignments = CreateNewCusUSLVConsignmentCollection();
					cusUSLVConsignments.Load();
					RegisterEditableChildObject(cusUSLVConsignments);
				}
				return cusUSLVConsignments;
			}
		}
		CusUSLVConsignmentCollection cusUSLVConsignments;

		public void PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode updateAction)
		{
			cusUSLVConsignmentsToSend = null;
			CusUSLVConsignments.ForEach(x => ((CusUSLVConsignment)x).InitAction(updateAction));
		}

		public void ResetCusUSLVConsignmentsActions()
		{
			CusUSLVConsignments.ForEach(x => ((CusUSLVConsignment)x).ResetAction());
		}

		public CusUSLVConsignmentForMessagingCollection CusUSLVConsignmentsToSend => cusUSLVConsignmentsToSend ?? (cusUSLVConsignmentsToSend = new CusUSLVConsignmentForMessagingCollection(this));
		CusUSLVConsignmentForMessagingCollection cusUSLVConsignmentsToSend;

		protected virtual CusUSLVConsignmentCollection CreateNewCusUSLVConsignmentCollection() => new CusUSLVConsignmentCollection(this);

		public bool HasPGAOnAnyConsignment => CusUSLVConsignments?.OfType<CusUSLVConsignment>().Any(x => x.HasPGAOnAnyItem) ?? false;

		public bool HasEntryTypeInformalFreeDutiableOnAnyConsignment => CusUSLVConsignments?.OfType<CusUSLVConsignment>().Any(x => x.IsEntryTypeInformalFreeDutiable) ?? false;

		public IEnumerable<CusUSLVConsignment> CombinedConsignmentsToConvert
		{
			get
			{
				if (combinedConsignmentsToConvert == null)
				{
					combinedConsignmentsToConvert = CusUSLVConsignments.Cast<CusUSLVConsignment>().Where(c => c.ULB_ConvertAction == ULBConvertActionList.Codes.Combined && !c.HasBeenConvertedToStandaloneDeclaration);
				}
				return combinedConsignmentsToConvert;
			}
		}
		IEnumerable<CusUSLVConsignment> combinedConsignmentsToConvert;

		#endregion

		#region IWorkflowProvider

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}
		ProcessTaskCollection workflowItems;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CusUSLVClearanceProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria() => new ColumnValueRanker();

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.CusUSLVClearanceWorkflowDescriptorCode;

		#endregion

		#region Overrides

		[ReadOnlyMember(nameof(AnyConsignmentCSAReceived))]
		public override ZGuid ULH_GB
		{
			get => base.ULH_GB;
			set => base.ULH_GB = value;
		}

		bool AnyConsignmentCSAReceived => CusUSLVConsignments.OfType<CusUSLVConsignment>().Any(consignment => consignment.ULB_MessageStatus == ImportMessageStatusList.Codes.ClearACECargoReleaseAdd);

		[ReadOnly(true)]
		public override ZString ULH_EntryFilerCode { get => base.ULH_EntryFilerCode; set => base.ULH_EntryFilerCode = value; }

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_TransportModeList))]
		public override ZString ULH_TransportMode
		{
			get => base.ULH_TransportMode;
			set
			{
				if (value != base.ULH_TransportMode)
				{
					base.ULH_TransportMode = value;
					CusUSLVConsignments.MarkAsNeedingValidationIncludingChildren();
					SetDefaultDatesIfNeeded();
					SetDefaultPortOfEntryIfNeeded();
					SetDefaultIssuerSCACCodeIfNeeded();
					SetDefaultNonAMSIndicatorIfNeeded();
					SetDefaultMasterBillIfNeeded();
					SetDefaultContainerModeIfNeeded();
				}
			}
		}

		public override ZString ULH_MasterBill
		{
			get => base.ULH_MasterBill;
			set
			{
				if (IsAir)
				{
					value = value.Replace("-", "").Replace(" ", "");
				}

				if (value != base.ULH_MasterBill)
				{
					base.ULH_MasterBill = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_ContainerModeList))]
		public override ZString ULH_ContainerMode
		{
			get => base.ULH_ContainerMode;
			set
			{
				base.ULH_ContainerMode = value;
				CusUSLVConsignments.MarkAsNeedingValidationIncludingChildren();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_MasterBillIssuerSCACList))]
		public override ZString ULH_MasterBillIssuerSCAC
		{
			get => base.ULH_MasterBillIssuerSCAC;
			set
			{
				if (value != base.ULH_MasterBillIssuerSCAC)
				{
					base.ULH_MasterBillIssuerSCAC = value;

					SetDefaultMasterBillIfNeeded();
				}
			}
		}

		USCarrierCombined MasterBillIssuerSCAC
		{
			get
			{
				var carriers = Factory.Load<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, ULH_MasterBillIssuerSCAC));
				if (carriers.Length == 1)
				{
					return carriers[0];
				}
				return null;
			}
		}

		internal bool MasterBillCarrierHasNumericBillPrefix
		{
			get { return MasterBillIssuerSCAC != null && !MasterBillIssuerSCAC.UI_AirwayBillPrefix.IsEmpty && MasterBillIssuerSCAC.UI_AirwayBillPrefix.IsNumbersOnlyOrEmpty; }
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.Vessels))]
		public override ZString ULH_ConveyanceName { get => base.ULH_ConveyanceName; set => base.ULH_ConveyanceName = value; }

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_CarrierSCACList))]
		public override ZString ULH_CarrierSCAC
		{
			get => base.ULH_CarrierSCAC;
			set
			{
				if (value != base.ULH_CarrierSCAC)
				{
					base.ULH_CarrierSCAC = value;
					SetDefaultIssuerSCACCodeIfNeeded();
				}
			}
		}

		public ZBool ULH_PortOfLoadingIsDropEdit => PortOfLadingDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_PortOfLoadingList))]
		public override ZString ULH_PortOfLoading
		{
			get => base.ULH_PortOfLoading;
			set
			{
				if (value != base.ULH_PortOfLoading)
				{
					base.ULH_PortOfLoading = value;
					PortOfLadingDefaulter.DefaultUNLOCO();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_PortOfEntryList))]
		public override ZString ULH_PortOfEntry
		{
			get => base.ULH_PortOfEntry;
			set
			{
				if (value != base.ULH_PortOfEntry)
				{
					base.ULH_PortOfEntry = value;
					SetDefaultArrivalDateIfNeeded();
					SetDefaultRemoteLocationFilingIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_US_NKLocationOfGoodsList))]
		public override ZString ULH_US_NKLocationOfGoods { get => base.ULH_US_NKLocationOfGoods; set => base.ULH_US_NKLocationOfGoods = value; }

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_US_NKCentralizedExamSiteList))]
		public override ZString ULH_US_NKCentralizedExamSite { get => base.ULH_US_NKCentralizedExamSite; set => base.ULH_US_NKCentralizedExamSite = value; }

		public ZBool ULH_PortOfDischargeIsDropEdit => PortOfDischargeDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_PortOfDischargeList))]
		public override ZString ULH_PortOfDischarge
		{
			get => base.ULH_PortOfDischarge;
			set
			{
				if (value != base.ULH_PortOfDischarge)
				{
					base.ULH_PortOfDischarge = value;
					PortOfDischargeDefaulter.DefaultUNLOCO();
					SetDefaultArrivalDateIfNeeded();
					SetDefaultPortOfEntryIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_RL_NKPortOfLoadingList))]
		public override ZString ULH_RL_NKPortOfLoading
		{
			get => base.ULH_RL_NKPortOfLoading;
			set
			{
				if (value != base.ULH_RL_NKPortOfLoading)
				{
					base.ULH_RL_NKPortOfLoading = value;

					if (!ULH_RL_NKPortOfLoading.IsEmpty)
					{
						PortOfLadingDefaulter.DefaultPort();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_RL_NKPortOfDischargeList))]
		public override ZString ULH_RL_NKPortOfDischarge
		{
			get => base.ULH_RL_NKPortOfDischarge;
			set
			{
				if (value != base.ULH_RL_NKPortOfDischarge)
				{
					base.ULH_RL_NKPortOfDischarge = value;

					if (!ULH_RL_NKPortOfDischarge.IsEmpty)
					{
						PortOfDischargeDefaulter.DefaultPort();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsNotRemoteLocationFiling))]
		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_PreparerDistrictPortList))]
		public override ZString ULH_PreparerDistrictPort
		{
			get => base.ULH_PreparerDistrictPort;
			set => base.ULH_PreparerDistrictPort = value;
		}

		[ReadOnlyMember(nameof(IsNotRemoteLocationFiling))]
		public override ZString ULH_PreparerOfficeCode
		{
			get => base.ULH_PreparerOfficeCode;
			set => base.ULH_PreparerOfficeCode = value;
		}

		public override ZBool ULH_RemoteLocationFiling
		{
			get => base.ULH_RemoteLocationFiling;
			set
			{
				if (value != base.ULH_RemoteLocationFiling)
				{
					base.ULH_RemoteLocationFiling = value;
					SetDefaultPortOfEntryIfNeeded();
					SetDefaultRemoteLocationFilingDetailsIfNeeded();
				}
			}
		}

		void SetDefaultRemoteLocationFilingDetailsIfNeeded()
		{
			if (ULH_RemoteLocationFiling)
			{
				ULH_PreparerDistrictPort = USCustomsDataRegistry.Instance.PreparerDistrictPort.GetValueWithoutFallback(Guid.Empty, RegistryBranchPK, Guid.Empty);
				ULH_PreparerOfficeCode = new ZString(USCustomsDataRegistry.Instance.PreparerOfficeCode.GetValueWithoutFallback(Guid.Empty, RegistryBranchPK, Guid.Empty)).Left(ULH_PreparerOfficeCodeInfo.MaxLength);
			}
			else
			{
				ULH_PreparerOfficeCode = string.Empty;
				ULH_PreparerDistrictPort = string.Empty;
			}
		}

		ZBool IsNotRemoteLocationFiling => !ULH_RemoteLocationFiling;

		#region Ports Defaulter

		UNLOCO_USPortsDefaulter fPortOfLadingDefaulter;
		UNLOCO_USPortsDefaulter PortOfLadingDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.K, ULH_RL_NKPortOfLoading, ZString.Empty, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return fPortOfLadingDefaulter ?? (fPortOfLadingDefaulter = new UNLOCO_USPortsDefaulter(Factory, ULH_PortOfLoadingInfo, ULH_RL_NKPortOfLoadingInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}

		public BusinessObjectCollection PortOfLadingRefLocoMappings => PortOfLadingDefaulter.MappingPorts;

		UNLOCO_USPortsDefaulter PortOfDischargeDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.D, ULH_RL_NKPortOfDischarge, ULH_TransportMode, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes);
				}

				return fPortOfDischargeDefaulter ?? (fPortOfDischargeDefaulter = new UNLOCO_USPortsDefaulter(Factory, ULH_PortOfDischargeInfo, ULH_RL_NKPortOfDischargeInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter fPortOfDischargeDefaulter;

		public BusinessObjectCollection PortOfDischargeRefLocoMappings => PortOfDischargeDefaulter.MappingPorts;

		#endregion

		public bool IsTruck => ULH_TransportMode == TransportTypeList.Codes.Truck;

		public bool IsRoad => ULH_TransportMode == TransportTypeList.Codes.Road;

		public bool IsAir => ULH_TransportMode == TransportTypeList.Codes.Air;

		public bool IsSea => ULH_TransportMode == TransportTypeList.Codes.Sea;

		public bool IsRail => ULH_TransportMode == TransportTypeList.Codes.Rail;

		public bool IsMail => ULH_TransportMode == TransportTypeList.Codes.Mail;

		#region Set Default Values If Needed

		void SetDefaultMasterBillIfNeeded()
		{
			if (IsAir)
			{
				var issuerSCAC = MasterBillIssuerSCAC;
				var airwayBillPrefix = issuerSCAC != null ? issuerSCAC.UI_AirwayBillPrefix : ZString.Empty;

				if (ULH_MasterBill.IsEmpty || ULH_MasterBill.Length == 3)
				{
					ULH_MasterBill = airwayBillPrefix;
				}
			}
		}

		void SetDefaultContainerModeIfNeeded()
		{
			if (USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty) && (IsTruck || IsAir))
			{
				ULH_ContainerMode = ContainerModes.NonContainerised;
			}
			else if (IsMail)
			{
				ULH_ContainerMode = ZString.Empty;
			}
		}

		void SetDefaultArrivalDateIfNeeded()
		{
			if (ULH_EntryDate.IsEmpty && !ULH_PortOfDischarge.IsEmpty && !ULH_DischargeDate.IsEmpty && ULH_PortOfDischarge == ULH_PortOfEntry)
			{
				ULH_EntryDate = ULH_DischargeDate;
			}
		}

		void SetDefaultDatesIfNeeded()
		{
			if (IsTruck)
			{
				var dateSetTo = ZDate.Today;

				if (ULH_EntryDate.IsEmpty)
				{
					ULH_EntryDate = dateSetTo;
				}

				if (ULH_DepartureDate.IsEmpty)
				{
					ULH_DepartureDate = dateSetTo;
				}

				if (ULH_DischargeDate.IsEmpty)
				{
					ULH_DischargeDate = dateSetTo;
				}
			}
		}

		void SetDefaultPortOfEntryIfNeeded()
		{
			if (ULH_PortOfEntry.IsEmpty && IsTruck && !ULH_RemoteLocationFiling && !ULH_PortOfDischarge.IsEmpty)
			{
				ULH_PortOfEntry = ULH_PortOfDischarge;
			}
		}

		void SetDefaultIssuerSCACCodeIfNeeded()
		{
			if (ULH_MasterBillIssuerSCAC.IsEmpty && IsTruck && !ULH_CarrierSCAC.IsEmpty)
			{
				ULH_MasterBillIssuerSCAC = ULH_CarrierSCAC;
			}
		}

		void SetDefaultNonAMSIndicatorIfNeeded()
		{
			var nonAMSIndicator = IsRoad || IsMail;

			foreach (var consignment in CusUSLVConsignments.Cast<CusUSLVConsignment>())
			{
				consignment.ULB_NonAMSIndicator = nonAMSIndicator;
			}
		}

		void SetDefaultRemoteLocationFilingIfNeeded()
		{
			var userBranch = GlbStaff.CurrentUser.HomeBranch;

			if (userBranch != null && !ULH_PortOfEntry.IsEmpty)
			{
				var branchPortsRelations = USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty);
				var mappedPorts = branchPortsRelations.Cast<BranchDistrictPort>().Where(x => x.BranchPK == userBranch.PK).ToList();

				if (mappedPorts.Count > 0)
				{
					var mapping = mappedPorts.FirstOrDefault(x => ULH_PortOfEntry.StartsWith(x.PortCode, StringComparison.CurrentCulture));
					ULH_RemoteLocationFiling = mapping == null;
				}
				else
				{
					ULH_RemoteLocationFiling = false;
				}
			}
			else
			{
				ULH_RemoteLocationFiling = false;
			}
		}

		public Guid RegistryCompanyPK
		{
			get
			{
				if (fRegistryCompanyPKCached == null)
				{
					fRegistryCompanyPKCached = new CachedProperty<Guid>(Factory, GetRegistryCompanyPK);
				}

				return fRegistryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> fRegistryCompanyPKCached;

		public Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						var branch = Branch;
						return branch == null ? GlbBranch.CurrentBranch.PK.ToGuid() : branch.PK.ToGuid();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		Guid GetRegistryCompanyPK()
		{
			var result = Guid.Empty;
			var branch = Branch;
			if (branch == null)
			{
				result = GlbCompany.CurrentCompany.PK.ToGuid();
			}
			else if (!branch.GB_GC.IsEmpty)
			{
				result = branch.GB_GC.ToGuid();
			}
			return result;
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusUSLVClearanceLookups.ULH_IORTypeList))]
		public override ZString ULH_IORType
		{
			get => base.ULH_IORType;
			set
			{
				var oldValue = ULH_IORType;
				base.ULH_IORType = value;
				if (oldValue != ULH_IORType && !IsCopying)
				{
					DefaultRegistrationNumberByType();
				}
			}
		}

		public override ZGuid ULH_OH_Importer
		{
			get => base.ULH_OH_Importer;
			set
			{
				var oldValue = ULH_OH_Importer;
				base.ULH_OH_Importer = value;
				if (oldValue != ULH_OH_Importer && !IsCopying)
				{
					iorWrapper = null;
					DefaultRegistrationByImporter();
				}
			}
		}

		public override ZDate ULH_EntryDate
		{
			get => base.ULH_EntryDate;
			set
			{
				if (base.ULH_EntryDate != value)
				{
					base.ULH_EntryDate = value;
					foreach (CusUSLVConsignment cusUSLVConsignment in CusUSLVConsignments)
					{
						foreach (CusUSLVItem usLVItem in cusUSLVConsignment.CusUSLVItems)
						{
							usLVItem.ULI_RX_NKCurrEXRateInfo.RefreshBinding();
						}

						cusUSLVConsignment.ULB_GoodsValueInfo.RefreshBinding();
					}
				}
			}
		}

		public override ZDate ULH_DischargeDate
		{
			get => base.ULH_DischargeDate;
			set
			{
				if (value != base.ULH_DischargeDate)
				{
					base.ULH_DischargeDate = value;
					SetDefaultArrivalDateIfNeeded();
				}
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ULH_GB = Env.CurrentBranch.PK;
			ULH_EntryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.Value.EntryFilerCode;

			var defaultFilerContactInformationRegistry = USCustomsDataRegistry.Instance.DefaultFilerContactInformation.Value;
			var regContactName = defaultFilerContactInformationRegistry.ContactName;
			var regPhone = defaultFilerContactInformationRegistry.ContactPhone;

			if (regContactName.IsEmpty && regPhone.IsEmpty && !Env.CurrentUser.IsSystemAccount)
			{
				ULH_ContactName = ((ZString)Env.CurrentUser.FullName).SubstringSafe(0, CusUSLVClearance.Schema.ULH_ContactNameMaxLength);
				var user = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, Env.CurrentUser.LoginName);
				if (user != null)
				{
					ULH_ContactPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(user).SubstringSafe(0, CusUSLVClearance.Schema.ULH_ContactPhoneMaxLength);
				}
			}
			else
			{
				ULH_ContactName = regContactName.SubstringSafe(0, CusUSLVClearance.Schema.ULH_ContactNameMaxLength);
				ULH_ContactPhone = regPhone.SubstringSafe(0, CusUSLVClearance.Schema.ULH_ContactPhoneMaxLength);
			}
		}

		void SetClusterKeyAndJobNumberIfNeeded()
		{
			PopulateNumberPropertyIfRequired(ULH_ClusterKeyInfo, x => GetNewClusterKeyAndJobNumber(x));
		}

		ZInt GetNewClusterKeyAndJobNumber(BusinessObjectFactory factory)
		{
			var result = ZInt.Zero;
			if (!IsDeleted)
			{
				result = (ZInt)Env.NumberFountains.ClusterKey.GetNext(factory);
				ULH_JobNumber = FormattableString.Invariant($"SEC{result:D8}");
			}
			return result;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = base.HumanReadableNameCore;
				if (!ULH_JobNumber.IsEmpty)
				{
					result = ULH_JobNumber;
				}

				return result;
			}
		}

		public override ZInt ULH_ClusterKey
		{
			get { return base.ULH_ClusterKey; }
			set
			{
				base.ULH_ClusterKey = value;
				CusUSLVConsignments.OfType<CusUSLVConsignment>().ForEach(x => x.ULB_ClusterKey = value);
			}
		}

		bool UseCodeIsHVL => ULH_UseCode == LVSConstants.ETailUseCode;

		[ReadOnlyMember(nameof(UseCodeIsHVL))]
		public override ZString ULH_MatchingKey
		{
			get { return base.ULH_MatchingKey; }
			set { base.ULH_MatchingKey = value; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override void OnSaving()
		{
			SetClusterKeyAndJobNumberIfNeeded();
			base.OnSaving();
		}

		public override void Delete()
		{
			CusUSLVConsignments.RemoveAndDeleteAll();
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			RequiredDocuments.RemoveAndDeleteAll();
			base.Delete();
		}

		public void DefaultRegistrationNumberByType()
		{
			var importer = Importer;
			if (importer != null)
			{
				ULH_IORReference = importer.CustomsCodes.GetCustomsRegNo(ULH_IORType, Core.Constants.CountryCodes.UnitedStates).SubstringSafe(0, 12);
			}
		}

		public void DefaultRegistrationByImporter()
		{
			var importer = Importer;
			if (importer != null)
			{
				var customsCode = importer.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber });
				if (customsCode != null)
				{
					ULH_IORType = customsCode.OK_CodeType;
					ULH_IORReference = customsCode.OK_CustomsRegNo.SubstringSafe(0, 12);
				}
			}
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return ULH_JobNumber; }
		}

		#endregion

		public OrgHeaderWrapper IORWrapper
		{
			get
			{
				if (iorWrapper == null && Importer != null)
				{
					iorWrapper = OrgHeaderWrapper.New(Importer);
				}

				return iorWrapper;
			}
		}
		OrgHeaderWrapper iorWrapper;

		#region IEDocsProvider

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new CusUSLVClearanceDocManagerInfo(this, DocManagerCodes.USLowValueEntries)); }
		}

		DocManagerInfo docManagerInfo;

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new EDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable

		CusUSLVClearanceDocumentSupporter documentSupporter;

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get
			{
				if (documentSupporter == null)
				{
					documentSupporter = new CusUSLVClearanceDocumentSupporter(this);
				}

				return documentSupporter;
			}
		}

		#endregion

		#region IHaveRequiredDocuments

		public void PreLogAllDocumentsReceivedEvents()
		{
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return ULH_JobNumber; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return ULH_MasterBill; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return CusUSLVClearanceSchema.Constants.Prefix; }
		}

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}

				return requiredDocuments;
			}
		}

		JobRequiredDocumentDependentCollection requiredDocuments;

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return this;
		}

		#endregion

		#region IAuthorityToActDeclarationProvider

		public GlbCompany Company => Branch?.Company;

		ZString IAuthorityToActDeclarationProvider.CountryCode => Company?.GC_RN_NKCountryCode ?? ZString.Empty;

		GlbCompany IAuthorityToActDeclarationProvider.Company => Company;

		JobDocsAndCartage IAuthorityToActDeclarationProvider.DocsAndCartage => null;

		#endregion

		public bool HasConsignmentWithForeignCurrency => CusUSLVConsignments?.OfType<CusUSLVConsignment>().Any(x => x.HasItemWithForeignCurrency) ?? false;

		[BusinessObjectTestExclude]
		public NonApplicableCusUSLVConsignmentCollection NonApplicableConsignments => nonApplicableConsignments ?? (nonApplicableConsignments = new NonApplicableCusUSLVConsignmentCollection(this));
		NonApplicableCusUSLVConsignmentCollection nonApplicableConsignments;

		#region Global Mutex

		ZGlobalMutex SendCustomsMessageMutex
		{
			get { return fSendCustomsMessageMutex ?? (fSendCustomsMessageMutex = new ZGlobalMutex(MutexIDs.SendCustomsMessage, PK.ToString())); }
		}
		ZGlobalMutex fSendCustomsMessageMutex;

		public bool LockSendCustomsMessageMutex() => SendCustomsMessageMutex.Lock();

		public void UnlockSendCustomsMessageMutex()
		{
			if (fSendCustomsMessageMutex != null && fSendCustomsMessageMutex.HasLock)
			{
				fSendCustomsMessageMutex.Unlock();
			}
		}

		public bool IsSendCustomsMessageMutexLocked => fSendCustomsMessageMutex?.IsLocked ?? false;

		public string GetSendCustomsMessageMutexLockByInfo() => fSendCustomsMessageMutex?.GetMutexLockByInfo() ?? string.Empty;

		#endregion

		#region Batch Collection For Consolidated Entry Summary

		public CusUSLVConsignmentBatchCollection CusUSLVConsignmentBatches => cusUSLVConsignmentBatches ??=
			new CusUSLVConsignmentBatchCollection(CusUSLVConsignments.Where(c => c.ULB_EntryType == EntryTypeList.Codes.InformalFreeDutiable).ToList());
		CusUSLVConsignmentBatchCollection cusUSLVConsignmentBatches;

		#endregion

		public IList<JobDeclaration> ConsolidatedSummaryDeclarations
		{
			get
			{
				var logs = Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ConsolidatedEntryChanged.Code));

				if (logs.IsNullOrEmpty())
				{
					return Array.Empty<JobDeclaration>();
				}

				var jobNumbers = logs
					.Select(x => x.Parameters.TryGetValue(EventReferenceParameters.Codes.JobNumber, out var value) ? value : null)
					.Where(value => !value.IsNullOrEmpty());

				var query = new ZQuery(JobDeclarationSchema.JE_IsCancelled, ZBool.False);
				query.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, jobNumbers);

				return Factory.Load<JobDeclaration>(query);
			}
		}

		public bool HasConsolidatedSummaryDeclarations => !ConsolidatedSummaryDeclarations.IsNullOrEmpty();
	}
}

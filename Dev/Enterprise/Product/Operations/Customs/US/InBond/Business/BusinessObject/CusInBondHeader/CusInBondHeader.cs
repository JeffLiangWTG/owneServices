using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.InBond.Business.Universal.Constants.Header.UniversalCopyIgnoreElement;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	[UniversalDataContext(DataContextType.InBond)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.USCusInBondHeader)]
	[UniversalCopyIgnoreElement(AMSMoveHeaders, CusAddInfos, CusInBondManifestBills, CusInBondOceanBills, CusInBondRegularBills, PTTMoveHeaders, Schema.BH_MessageStatus, Schema.BH_ReleaseStatus, Schema.BH_JobReference)]
	[UniversalCopyWithExtendedEntities]
	public class CusInBondHeader : Customs.Business.CusInBondHeader
		, Integration.Customs.US.InBond.ICusInBondHeader
		, ITemplateCopyable
		, IEDocsProvider
		, IWorkflowTriggerEventSource
		, IWorkflowProvider
		, IJobNumber
		, IHaveRequiredDocuments
		, IControllerIDProvider
		, ICustomLabelsConfigOrgProvider
		, IUniversalXMLNoteParent
		, ICargoManifestStatusQueryHeader
	{
		public CusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.CusInBondHeader.Schema
		{
			public const string DeclarationPK = "DeclarationPK";
			public const string ImporterOrgPK = "ImporterOrgPK";
			public const string SelectedMovementHeader = "SelectedMovementHeader";
			public const string ThreeLetterAirCarrierCode = "ThreeLetterAirCarrierCode";
			public const string SelectedMovementDetail = "SelectedMovementDetail";
		}

		#region New Properties

		[List(nameof(MovementDetails))]
		public ZGuid SelectedMovementDetail
		{
			get { return selectedMovementDetail; }
			set
			{
				selectedMovementDetail = value;
				SelectedMovementDetailInfo.RefreshBinding();
				SelectedMovementDetails.Rebuild();
			}
		}
		ZGuid selectedMovementDetail;

		public ZPropertyInfo SelectedMovementDetailInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedMovementDetail); }
		}

		public ZBool IsDetailedInBond
		{
			get { return BH_HeaderType != InBondHeaderTypeList.Codes.AMS && !IsDocumentOnly; }
		}

		public ZBool IsDocumentOnly
		{
			get { return BH_HeaderType == InBondHeaderTypeList.Codes.DocumentOnly; }
		}

		public ZBool IsAMS
		{
			get { return BH_HeaderType == InBondHeaderTypeList.Codes.AMS; }
		}

		public ZBool IsFullData
		{
			get { return BH_HeaderType == InBondHeaderTypeList.Codes.FullData; }
		}

		public ZBool IsStandAlone
		{
			get { return BH_ParentID == ZGuid.Empty; }
		}

		public ZBool JobReferenceVisible
		{
			get { return Parent == null && !BH_JobReference.IsEmpty; }
		}

		public ZString InBondNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (CusInBondMoveHeader moveHeader in MovementHeaders)
				{
					result = result.IsEmpty ? moveHeader.InBondNumber : moveHeader.InBondNumber.IsEmpty ? result : (ZString)"Multiple InBond Nos.";
				}

				return result;
			}
		}

		public ZBool HasSentMessages
		{
			get
			{
				ZBool result = false;

				if (MovementHeaders.Any(x => x.IsWaitingForResponse || x.IsAcceptedByCustoms || x.IsWithdrawn)
					|| Bills.Any(x => x.IsWaitingForResponse || x.IsAcceptedByCustoms || x.IsWithdrawn)
					|| MovementDetails.Cast<CusInBondMoveDetail>().Any(x => x.Containers.Any(y => y.IsWaitingForResponse || y.IsAcceptedByCustoms || y.IsWithdrawn)))
				{
					result = true;
				}

				return result;
			}
		}

		#region Message Status QP/WP

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondQPStatus", Caption = "QP Message Status", MediumCaption = "QP Msg. Status", ShortCaption = "QP Status")]
		public ZString InBondQPStatus
		{
			get
			{
				var result = ZString.Empty;

				foreach (CusInBondMoveHeader moveHeader in MovementHeaders)
				{
					if (result.IsEmpty)
					{
						result = moveHeader.BM_CustomsStatus;
					}
					else if (!moveHeader.BM_CustomsStatus.IsEmpty && moveHeader.BM_CustomsStatus != result)
					{
						result = InBondMultipleStatusDescription;
						break;
					}
				}

				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondQPStatusDescription", Caption = "QP Status Desc.")]
		public ZString InBondQPStatusDescription
		{
			get
			{
				if (InBondQPStatus == InBondMultipleStatusDescription)
				{
					return InBondQPStatus;
				}
				else
				{
					return Factory.GetCachedValue<MessageStatusListIT>().GetDescriptionFromCode(InBondQPStatus);
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondWPStatus", Caption = "WP Message Status", MediumCaption = "WP Msg. Status", ShortCaption = "WP Status")]
		public ZString InBondWPStatus
		{
			get
			{
				var result = ZString.Empty;

				foreach (CusInBondMoveHeader moveHeader in MovementHeaders)
				{
					if (result.IsEmpty)
					{
						result = moveHeader.BM_MessageStatus;
					}
					else if (!moveHeader.BM_MessageStatus.IsEmpty && moveHeader.BM_MessageStatus != result)
					{
						result = InBondMultipleStatusDescription;
						break;
					}
				}

				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondWPStatusDescription", Caption = "WP Message Status Description", MediumCaption = "WP Msg. Status Desc.", ShortCaption = "WP Status Desc.")]
		public ZString InBondWPStatusDescription
		{
			get
			{
				if (InBondWPStatus == InBondMultipleStatusDescription)
				{
					return InBondWPStatus;
				}
				else
				{
					return Factory.GetCachedValue<MessageStatusListIT>().GetDescriptionFromCode(InBondWPStatus);
				}
			}
		}

		const string InBondMultipleStatusDescription = "Multiple Status";

		#endregion

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondClosedDate", Caption = "In-Bond Closed Date", ShortCaption = "Closed Date")]
		public ZString InBondClosedDate
		{
			get
			{
				var result = "Not All Closed";
				var closedDates = MovementHeaders.Cast<CusInBondMoveHeader>().Select(x => x.BM_InBondClosedDate).ToList();
				if (closedDates.Count > 0 && closedDates.All(x => x.IsValid))
				{
					result = closedDates.Max().ToLongTimeString();
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondCarrier", Caption = "In-Bond Carrier")]
		public ZString InBondCarrier
		{
			get
			{
				var result = MovementHeaders.Cast<CusInBondMoveHeader>().Where(x => x.InBondCarrierOrg != null).Select(x => x.InBondCarrierOrg.OH_Code).Distinct();
				return result.Count() > 1 ? (ZString)Multiple : result.FirstOrDefault();
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondCarrierName", Caption = "In-Bond Carrier Name")]
		public ZString InBondCarrierName
		{
			get
			{
				var result = ZString.Empty;
				if (InBondCarrierCode == Multiple)
				{
					result = Multiple;
				}
				else
				{
					var carrierScac = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, InBondCarrierCode));
					if (carrierScac != null)
					{
						result = carrierScac.UI_Name;
					}
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondCarrierCode", Caption = "In-Bond Carrier Code")]
		public ZString InBondCarrierCode
		{
			get
			{
				var result = MovementHeaders.Cast<CusInBondMoveHeader>().Where(x => !x.BM_InBondCarrierSCAC.IsEmpty).Select(x => x.BM_InBondCarrierSCAC).Distinct();
				return result.Count() > 1 ? (ZString)Multiple : result.FirstOrDefault();
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|USDestinationPortCode", Caption = "US Destination Port Code", ShortCaption = "US Dest. Port Code")]
		public ZString USDestinationPortCode
		{
			get
			{
				var result = MovementHeaders.Cast<CusInBondMoveHeader>().Where(x => !x.BM_DestinationPortCode.IsEmpty && x.BM_DestinationPortCode.IsValid).Select(x => x.BM_DestinationPortCode).Distinct();
				return result.Count() > 1 ? (ZString)Multiple : result.FirstOrDefault();
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|USDestinationPortName", Caption = "US Destination Port Name", ShortCaption = "US Dest. Port Name")]
		public ZString USDestinationPortName
		{
			get
			{
				var result = ZString.Empty;
				if (USDestinationPortCode == Multiple)
				{
					result = Multiple;
				}
				else
				{
					var destinationPortDCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, USDestinationPortCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					if (destinationPortDCode != null)
					{
						result = destinationPortDCode.ZZD_Description;
					}
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|ForeignDestinationPortCode", Caption = "Foreign Destination Port Code", ShortCaption = "Foreign Dest. Port Code")]
		public ZString ForeignDestinationPortCode
		{
			get
			{
				var result = MovementHeaders.Cast<CusInBondMoveHeader>().Where(x => !x.BM_ForeignDestPortKCode.IsEmpty).Select(x => x.BM_ForeignDestPortKCode).Distinct();
				return result.Count() > 1 ? (ZString)Multiple : result.FirstOrDefault();
			}
		}
		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|ForeignDestinationPortName", Caption = "Foreign Destination Port Name", ShortCaption = "Foreign Dest. Port Name")]
		public ZString ForeignDestinationPortName
		{
			get
			{
				var result = ZString.Empty;
				if (ForeignDestinationPortCode == Multiple)
				{
					result = Multiple;
				}
				else
				{
					result = Factory.GetCachedValue("CusInBondHeader|ForeignDestinationPortName|" + ForeignDestinationPortCode, () =>
					{
						var foreignDestPortKCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, ForeignDestinationPortCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
						return foreignDestPortKCode?.ZZD_Description ?? ZString.Empty;
					});
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondHeader|InBondQTY", Caption = "In-Bond QTY")]
		public ZInt InBondQTY
		{
			get
			{
				ZInt result = 0;
				foreach (CusInBondMoveHeader moveHeader in MovementHeaders)
				{
					foreach (CusInBondMoveDetail moveDetail in moveHeader.MovementDetails)
					{
						result += moveDetail.B9_InBoundQty;
					}
				}
				return result;
			}
		}
		public const string Multiple = "MUL";

		public ZString InBondEntryTypes
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (CusInBondMoveHeader moveHeader in MovementHeaders)
				{
					if (result.IsEmpty)
					{
						result = moveHeader.BM_InBondEntryType;
					}
					else if (!moveHeader.BM_InBondEntryType.IsEmpty && moveHeader.BM_InBondEntryType != result)
					{
						result = "Multiple Entry Types";
					}
				}

				return result;
			}
		}

		public ZBool IsAttorneyInFact
		{
			get { return USCustomsDataRegistry.Instance.IsAttorneyInFact.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		#region ImporterOrgPK
		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ImporterList))]
		public ZGuid ImporterOrgPK
		{
			get { return BH_OA_Importer_ZAddress.OrgPK; }
			set { BH_OA_Importer_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ImporterOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ImporterOrgPK, x => BH_OA_Importer_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ImporterOrg
		{
			get { return (OrgHeader)BH_OA_Importer_ZAddress.OrgHeader; }
		}

		public ZString ImporterName
		{
			get { return ImporterOrg != null ? ImporterOrg.OH_FullNameTruncated : ZString.Empty; }
		}

		#endregion

		/// <summary>
		/// This is a non-persisted proprerty to which users set to filter movements
		/// </summary>
		[List(nameof(MovementHeaders))]
		public ZGuid SelectedMovementHeader
		{
			get { return selectedMovementHeader; }
			set
			{
				selectedMovementHeader = value;
				SelectedMovementHeaderInfo.RefreshBinding();
				ActiveBusinessObjectCollection<CusInBondMoveHeader>.RefreshAll(Factory);
				FilteredMovementDetails.Rebuild();
			}
		}
		ZGuid selectedMovementHeader;

		public ZPropertyInfo SelectedMovementHeaderInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedMovementHeader); }
		}

		// Movement containers are deleted when transport mode is air
		public bool IsAir
		{
			get { return BH_ImportTransportMode == InBondTransportModeCodes.Codes.AirNonContainer; }
		}

		public bool IsSea
		{
			get { return BH_ImportTransportMode == InBondTransportModeCodes.Codes.VesselContainer || BH_ImportTransportMode == InBondTransportModeCodes.Codes.VesselNonContainer; }
		}

		public bool IsRail
		{
			get { return BH_ImportTransportMode == InBondTransportModeCodes.Codes.RailContainer || BH_ImportTransportMode == InBondTransportModeCodes.Codes.RailNonContainer; }
		}

		public bool IsTruck
		{
			get { return BH_ImportTransportMode == InBondTransportModeCodes.Codes.TruckContainer || BH_ImportTransportMode == InBondTransportModeCodes.Codes.TruckNonContainer; }
		}

		public ZBool IsContainerised
		{
			get { return BH_ImportTransportMode == InBondTransportModeCodes.Codes.VesselContainer || ContainerNumbers.Count > 0; }
		}

		public IReadOnlyList<ZString> ContainerNumbers
		{
			get
			{
				if (containerNumbersCached == null)
				{
					containerNumbersCached = new CachedProperty<IReadOnlyList<ZString>>(Factory, delegate
					{
						var result = new List<ZString>();
						foreach (CusInBondMoveHeader moveHeader in MovementHeaders)
						{
							foreach (CusInBondMoveDetail moveDetail in moveHeader.MovementDetails)
							{
								foreach (CusInBondContainer container in moveDetail.Containers)
								{
									if (!container.BC_ContainerNum.IsEmpty && !container.IsNonContainerized && !result.Contains(container.BC_ContainerNum))
									{
										result.Add(container.BC_ContainerNum);
									}
								}
							}
						}
						return result;
					});
				}
				return containerNumbersCached.Value;
			}
		}
		CachedProperty<IReadOnlyList<ZString>> containerNumbersCached;

		#region Validation Modes

		public void RecalculateValidationModesOnHeader(InBondMessageType messageType)
		{
			ValidationModesCalculator.RecalculateValidationModesOnHeader(messageType);
		}

		public ValidationModesCalculator ValidationModesCalculator
		{
			get
			{
				if (validationModesCalculator == null)
				{
					validationModesCalculator = new ValidationModesCalculator(this);
				}
				return validationModesCalculator;
			}
		}
		ValidationModesCalculator validationModesCalculator;

		public ValidationModes ValidationModes
		{
			get
			{
				if (!validationModes.HasValue)
				{
					validationModes = IsAir ? ValidationModes.AirInitiationAndDeletion : ValidationModes.Departure;
				}
				return validationModes.Value;
			}
			set
			{
				bool hasChanges = ValidationModes != value;
				validationModes = value;
				if (hasChanges && !IsMarkingAsNeedingValidationSuspended)
				{
					MarkAsNeedingValidationIncludingChildren();
				}
			}
		}
		ValidationModes? validationModes;

		public bool IsArrivalValidationMode
		{
			get { return IsInBondLevelArrivalValidationMode || IsBillOfLadingArrivalValidationMode || IsContainerArrivalValidationMode; }
		}

		public bool IsInBondLevelArrivalValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondLevelArrival); }
		}

		public bool IsBillOfLadingArrivalValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.BillOfLadingLevelArrival); }
		}

		public bool IsContainerArrivalValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.ContainerLevelArrival); }
		}

		public bool IsInBondLevelDepartureValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.Departure); }
		}

		public bool IsExportationValidationMode
		{
			get { return IsInBondLevelExportationValidationMode || IsBillOfLadingExportationValidationMode || IsContainerExportationValidationMode; }
		}

		public bool IsInBondLevelExportationValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondLevelExportation); }
		}

		public bool IsBillOfLadingExportationValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.BillOfLadingLevelExportation); }
		}

		public bool IsContainerExportationValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.ContainerLevelExportation); }
		}

		public bool IsInBondLevelTransferOfLiabilityValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondLevelTransferOfLiability); }
		}

		public bool IsAirInBondInitiationAndDeletionMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.AirInitiationAndDeletion); }
		}

		public bool IsAirEntireInBondArrivalMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.AirEntireInBondArrival); }
		}

		public bool IsAirEntireInBondExportationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.AirEntireInBondExportation); }
		}

		public bool IsAirInBondLevelMode
		{
			get { return IsAirEntireInBondArrivalMode || IsAirEntireInBondExportationMode; }
		}

		public bool IsDepartureDeleteValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.DepartureDelete); }
		}

		public bool IsDiversionRequestMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.DiversionRequest); }
		}

		public bool IsBillOfLadingDeleteMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.BillOfLadingDelete); }
		}

		#endregion

		public bool HasAtLeastOneMovementMarkedForBondedWarhousing
		{
			get
			{
				if (hasAtLeastOneMovementMarkedForBondedWarhousingCached == null)
				{
					hasAtLeastOneMovementMarkedForBondedWarhousingCached = new CachedProperty<bool>(Factory, () =>
					{
						var result = false;
						var supportsBondedWarehousing = SupportsBondedWarehousing;
						foreach (CusInBondMoveHeader movementHeader in MovementHeaders)
						{
							if (movementHeader.HasWHSTransaction || (supportsBondedWarehousing && movementHeader.WhsWarehouse != null))
							{
								result = true;
								break;
							}
						}
						return result;
					});
				}
				return hasAtLeastOneMovementMarkedForBondedWarhousingCached.Value;
			}
		}
		CachedProperty<bool> hasAtLeastOneMovementMarkedForBondedWarhousingCached;

		public bool IsABondedWarehousingImporter
		{
			get
			{
				var importerAddress = Importer;
				var importer = importerAddress == null ? null : importerAddress.Header;
				return importer != null && importer.CompanyData.OB_IMUsedBondedWhs;
			}
		}

		internal bool SupportsBondedWarehousing
		{
			get { return BH_FTZMove && IsABondedWarehousingImporter; }
		}

		public bool HasAtLeastOneMovementWithWHSTransaction
		{
			get
			{
				if (hasAtLeastOneMovementWithWHSTransactionCached == null)
				{
					hasAtLeastOneMovementWithWHSTransactionCached = new CachedProperty<bool>(Factory, () =>
					{
						return MovementHeaders.OfType<CusInBondMoveHeader>().Any(x => x.HasWHSTransaction);
					});
				}
				return hasAtLeastOneMovementWithWHSTransactionCached.Value;
			}
		}
		CachedProperty<bool> hasAtLeastOneMovementWithWHSTransactionCached;

		#endregion

		#region Override Properties

		public override ZDateTime BH_SystemCreateTimeUtc
		{
			get { return base.BH_SystemCreateTimeUtc; }
			set
			{
				var oldValue = BH_SystemCreateTimeUtc;
				base.BH_SystemCreateTimeUtc = value;
				if (oldValue != BH_SystemCreateTimeUtc)
				{
					MovementHeaders.MarkAsNeedingValidation();
					MovementHeaders.MarkCommoditiesAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(BH_PostDepartureOnly_ReadOnly))]
		public override ZBool BH_PostDepartureOnly
		{
			get { return base.BH_PostDepartureOnly; }
			set
			{
				var oldValue = BH_PostDepartureOnly;
				base.BH_PostDepartureOnly = value;
				if (oldValue != BH_PostDepartureOnly)
				{
					MovementHeaders.MarkAsNeedingValidation();

					if (oldValue && MovementHeaders != null)
					{
						foreach (CusInBondMoveHeader movement in MovementHeaders)
						{
							movement.InBondNumber = ZString.Empty;
							movement.InBondNumberInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public bool BH_PostDepartureOnly_ReadOnly
		{
			get
			{
				return HasSentMessages;
			}
		}

		public override ZGuid BH_OH_Supplier
		{
			get { return base.BH_OH_Supplier; }
			set
			{
				var oldValue = BH_OH_Supplier;
				base.BH_OH_Supplier = value;
				if (!IsCopying && oldValue != BH_OH_Supplier)
				{
					ClearCommoditySupplierValuesIfSameAndMarkAsNeedingValidation(BH_OH_Supplier);
				}
			}
		}

		public override ZGuid BH_OA_Importer
		{
			get { return base.BH_OA_Importer; }
			set
			{
				var oldValue = BH_OA_Importer;
				base.BH_OA_Importer = value;
				if (!IsCopying && oldValue != BH_OA_Importer)
				{
					MovementHeaders.MarkAsNeedingValidation();
					foreach (var movementHeader in MovementHeaders)
					{
						movementHeader.MarkAsNeedingValidation();
					}
					MovementHeaders.MarkCommoditiesAsNeedingValidation((x) => x.RefreshPivot());
				}
			}
		}

		public override ZDateTime BH_ETA
		{
			get { return base.BH_ETA; }
			set
			{
				var oldValue = BH_ETA;
				base.BH_ETA = value;
				if (!IsCopying && oldValue != BH_ETA)
				{
					MovementHeaders.MarkCommoditiesAsNeedingValidation((x) => x.RefreshPivot());
				}
			}
		}

		public override ZGuid BH_GB
		{
			get { return base.BH_GB; }
			set
			{
				ZGuid oldValue = BH_GB;
				base.BH_GB = value;
				if (!IsCopying && oldValue != BH_GB)
				{
					MovementHeaders.MarkAsNeedingValidation();
					Bills.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.HeaderTypeList))]
		[MaxLength(1)]
		public override ZString BH_HeaderType
		{
			get { return base.BH_HeaderType; }
			set
			{
				ZString oldValue = BH_HeaderType;
				base.BH_HeaderType = value;
				if (!IsCopying && oldValue != BH_HeaderType)
				{
					MovementHeaders.MarkAsNeedingValidationIncludingChildren();
					Bills.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public override ZPropertyInfo BH_HeaderTypeInfo
		{
			get { return GetZPropertyInfo(Schema.BH_HeaderType); }
		}

		public override ZBool BH_FTZMove
		{
			get { return base.BH_FTZMove; }
			set
			{
				var oldValue = BH_FTZMove;
				base.BH_FTZMove = value;
				if (!IsCopying && oldValue != BH_FTZMove)
				{
					MovementHeaders.MarkAsNeedingValidation();
					foreach (var movementHeader in MovementHeaders)
					{
						movementHeader.MarkAsNeedingValidation();
					}
					Bills.MarkAsNeedingValidationIncludingChildren();

					if (BH_FTZMove)
					{
						BH_CarrierSCAC = ZString.Empty;
						ThreeLetterAirCarrierCode = ZString.Empty;
						Bills.ForEach(bill => bill.SetIssuerCodeAndFTZForeignPortOfLadingIfRequired());
						Bills.RefreshBinding();
						ResetValueOnContainer();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.CarrierCollection))]
		public override ZString BH_CarrierSCAC
		{
			get { return base.BH_CarrierSCAC; }
			set
			{
				bool hasChanged = BH_CarrierSCAC != value;
				base.BH_CarrierSCAC = value;
				if (hasChanged && !IsCopying)
				{
					if (ShouldSendThreeLetterAirCarrierCode)
					{
						ThreeLetterAirCarrierCode = ZString.Empty;
						Validation.ValidateThreeLetterAirCarrierCode();
						ThreeLetterAirCarrierCodeInfo.RefreshBinding();
					}
				}
			}
		}

		public bool BH_CarrierSCAC_ReadOnly
		{
			get { return BH_FTZMove && !IsAir; }
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.AirlineCollection))]
		public ZString ThreeLetterAirCarrierCode
		{
			get
			{
				var airCarrierCode = ThreeLetterRefAirCarrierCode;
				return AddOnAirCarrierCode != null ? AddOnAirCarrierCode.XA_Data : (!airCarrierCode.IsEmpty ? airCarrierCode : ZString.Empty);
			}
			set { GenAddOnColumnHelper.AddOnColumnCarrierCode(AddOnAirCarrierCode, Schema.ThreeLetterAirCarrierCode, value, ThreeLetterAirCarrierCodeInfo, delegate { CheckMaximumLength(ThreeLetterAirCarrierCodeInfo, value); }); }
		}

		ZString ThreeLetterRefAirCarrierCode
		{
			get { return GenAddOnColumnHelper.GetRefAirlineThreeLetterCodeIfNecessary(BH_CarrierSCAC); }
		}

		public bool ThreeLetterAirCarrierCode_ReadOnly
		{
			get { return BH_FTZMove && !IsAir; }
		}

		public bool ShouldSendThreeLetterAirCarrierCode
		{
			get { return IsAir && ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired(BH_CarrierSCAC); }
		}

		public ZPropertyInfo ThreeLetterAirCarrierCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ThreeLetterAirCarrierCode); }
		}

		GenAddOnColumn AddOnAirCarrierCode
		{
			get
			{
				if (addOnAirCarrierCode == null)
				{
					addOnAirCarrierCode = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GenAddOnColumnHelper.GetAddOnCarrierCode(Schema.ThreeLetterAirCarrierCode);
					});
				}
				this.RegisterEditableChildObject(addOnAirCarrierCode.Value);
				return addOnAirCarrierCode.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnAirCarrierCode;

		ThreeLetterAirCarrierCodeHelper GenAddOnColumnHelper
		{
			get
			{
				if (genAddOnColumnHelper == null)
				{
					genAddOnColumnHelper = new ThreeLetterAirCarrierCodeHelper(this);
				}
				return genAddOnColumnHelper;
			}
		}
		ThreeLetterAirCarrierCodeHelper genAddOnColumnHelper;

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.Countries))]
		public override ZString BH_ImportConveyanceCountry
		{
			get { return base.BH_ImportConveyanceCountry; }
			set { base.BH_ImportConveyanceCountry = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ScheduleDCodes))]
		[RelatedBusinessObject("PortUnladingDCode")]
		public override ZString BH_PortUnladingDCode
		{
			get { return base.BH_PortUnladingDCode; }
			set
			{
				var oldValue = BH_PortUnladingDCode;
				base.BH_PortUnladingDCode = value;
				if (oldValue != BH_PortUnladingDCode)
				{
					bH_Calc_PortUnladingUNLOCOCached = null;
				}
			}
		}

		public ZString BH_Calc_PortUnladingUNLOCO
		{
			get
			{
				if (!bH_Calc_PortUnladingUNLOCOCached.HasValue)
				{
					bH_Calc_PortUnladingUNLOCOCached = USScheduleResolver.MatchingUNLOCO(BH_PortUnladingDCode, Factory);
				}
				return bH_Calc_PortUnladingUNLOCOCached.Value;
			}
		}
		ZString? bH_Calc_PortUnladingUNLOCOCached;

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ScheduleKCodes))]
		[RelatedBusinessObject("ImportLoadPortKCode")]
		public override ZString BH_ImportLoadPortKCode
		{
			get { return base.BH_ImportLoadPortKCode; }
			set
			{
				var oldValue = BH_ImportLoadPortKCode;
				base.BH_ImportLoadPortKCode = value;
				if (oldValue != BH_ImportLoadPortKCode)
				{
					bH_Calc_ImportLoadPortUNLOCOCached = null;
				}
			}
		}

		public ZString BH_Calc_ImportLoadPortUNLOCO
		{
			get
			{
				if (!bH_Calc_ImportLoadPortUNLOCOCached.HasValue)
				{
					bH_Calc_ImportLoadPortUNLOCOCached = USScheduleResolver.MatchingUNLOCO(BH_ImportLoadPortKCode, Factory);
				}
				return bH_Calc_ImportLoadPortUNLOCOCached.Value;
			}
		}
		ZString? bH_Calc_ImportLoadPortUNLOCOCached;

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.TransportModeCodes))]
		public override ZString BH_ImportTransportMode
		{
			get { return base.BH_ImportTransportMode; }
			set
			{
				var oldValue = BH_ImportTransportMode;
				var isAirForOldValue = IsAir;
				base.BH_ImportTransportMode = value;

				if (oldValue != BH_ImportTransportMode)
				{
					if (IsAir || isAirForOldValue)
					{
						Bills.ForEach(x => x.RefreshDocAddresses());
					}

					bH_Calc_FreightTransportModeCached = null;
					if (!IsCopying)
					{
						Bills.MarkAsNeedingValidation();
						MovementHeaders.MarkAsNeedingValidation();
						FilteredMovementDetails.MarkAsNeedingValidation();
						MovementHeaders.MarkCommoditiesAsNeedingValidation();
						validationModes = null;
					}
				}

				if (IsAir)
				{
					DeleteContainers();
				}
				else
				{
					MovementHeaders.ForEach(x =>
					{
						x.BM_SplitCarrierSCAC = string.Empty;
						x.BM_SplitFlightNo = string.Empty;
					});
				}
			}
		}

		public ZString BH_Calc_FreightTransportMode
		{
			get
			{
				if (!bH_Calc_FreightTransportModeCached.HasValue)
				{
					switch (BH_ImportTransportMode)
					{
						case InBondTransportModeCodes.Codes.AirNonContainer:
							bH_Calc_FreightTransportModeCached = Enterprise.Customs.US.Business.TransportTypeList.Codes.Air;
							break;
						case InBondTransportModeCodes.Codes.RailNonContainer:
							bH_Calc_FreightTransportModeCached = Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail;
							break;
						case InBondTransportModeCodes.Codes.TruckNonContainer:
							bH_Calc_FreightTransportModeCached = Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck;
							break;
						case InBondTransportModeCodes.Codes.VesselContainer:
							bH_Calc_FreightTransportModeCached = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
							break;
						case InBondTransportModeCodes.Codes.VesselNonContainer:
							bH_Calc_FreightTransportModeCached = Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea;
							break;
						case InBondTransportModeCodes.Codes.FixedTransportInstallations:
							bH_Calc_FreightTransportModeCached = Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations;
							break;
						default:
							bH_Calc_FreightTransportModeCached = ZString.Empty;
							break;
					}
				}
				return bH_Calc_FreightTransportModeCached.Value;
			}
		}
		ZString? bH_Calc_FreightTransportModeCached;

		protected void DeleteContainers()
		{
			foreach (CusInBondMoveHeader header in MovementHeaders)
			{
				foreach (CusInBondMoveDetail detail in header.MovementDetails)
				{
					if (detail.Containers != null)
					{
						detail.Containers.DeleteAll();
					}
				}
			}
		}

		protected void ResetValueOnContainer()
		{
			foreach (CusInBondMoveHeader header in MovementHeaders)
			{
				if (header.SupportsBondedWarehousing)
				{
					foreach (CusInBondMoveDetail detail in header.MovementDetails)
					{
						if (detail.Containers != null)
						{
							detail.Containers.ResetPieceCount();
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.FIRMSCollection))]
		[RelatedBusinessObject("FIRMS")]
		public override ZString BH_FIRMS
		{
			get { return base.BH_FIRMS; }
			set
			{
				base.BH_FIRMS = value;
				if (!IsCopying)
				{
					MovementHeaders.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ImportingConveyanceList))]
		[RelatedBusinessObject("ImportConveyance")]
		[ReadOnlyMember(nameof(BH_ImportConveyanceName_ReadOnly))]
		public override ZString BH_ImportConveyanceName
		{
			get { return base.BH_ImportConveyanceName; }
			set
			{
				ZString oldValue = BH_ImportConveyanceName;
				base.BH_ImportConveyanceName = value;
				if (!IsCopying && oldValue != BH_ImportConveyanceName)
				{
					DefaultFromImportConveyanceIfPossible();
				}
			}
		}

		bool BH_ImportConveyanceName_ReadOnly
		{
			get { return IsAir; }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					foreach (var movementHeader in MovementHeaders)
					{
						result.Add(movementHeader);
						result.AddRange(movementHeader.BusinessObjectsWithRelatedEvents);
					}
				}
				return result.ToArray();
			}
		}

		#endregion

		#region Override Methods

		public override void Delete()
		{
			Bills.DeleteAll();
			MovementHeaders.DeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Related Objects

		public ZZRefCusCodeListCombined FIRMS
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BH_FIRMS, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombined PortUnladingDCode
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BH_PortUnladingDCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombined ImportLoadPortKCode
		{
			get
			{
				return Factory.GetCachedValue("CusInBondHeader|ImportLoadPortKCode|" + BH_ImportLoadPortKCode, () =>
				{
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, BH_ImportLoadPortKCode,
						Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today,
						attributeFilters: new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, ForeignPortTypeList.Codes.Common) });
				});
			}
		}

		public new CusInBondHeaderLookups Lookups
		{
			get { return (CusInBondHeaderLookups)base.Lookups; }
		}

		public new CusInBondHeaderValidation Validation
		{
			get { return (CusInBondHeaderValidation)base.Validation; }
		}

		public RefVessel ImportConveyance
		{
			get { return Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, BH_ImportConveyanceName); }
		}

		[ActionFieldFollow(false)]
		[ChildEditable]
		[UniversalCopyCollectionEntity(CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.Constants.B0_BH)]
		public new CusInBondBillCollection Bills
		{
			get { return (CusInBondBillCollection)base.Bills; }
		}

		protected override ICusInBondBillCollection GetNewBillsCollection()
		{
			return new CusInBondBillCollection(this);
		}

		public new CusInBondMoveHeader MovementHeader
		{
			get { return (CusInBondMoveHeader)base.MovementHeader; }
		}

		[ChildEditable]
		[UniversalCopyCollectionEntity(CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.Constants.BM_BH)]
		public new CusInBondMoveHeaderCollection MovementHeaders
		{
			get { return (CusInBondMoveHeaderCollection)base.MovementHeaders; }
		}

		[ChildEditable]
		public CusInBondMoveHeaderCollection MovementHeadersMessages
		{
			get
			{
				if (movementHeadersMessages == null)
				{
					movementHeadersMessages = (CusInBondMoveHeaderCollection)GetMovementHeaders();
					RegisterEditableChildObject(movementHeadersMessages);
				}
				return movementHeadersMessages;
			}
		}
		CusInBondMoveHeaderCollection movementHeadersMessages;

		[ChildEditable]
		public CusInBondMoveHeaderCollection CBP7512MovementHeaders
		{
			get
			{
				if (cbp7512movementHeaders == null)
				{
					cbp7512movementHeaders = (CusInBondMoveHeaderCollection)GetMovementHeaders();
					RegisterEditableChildObject(cbp7512movementHeaders);
				}
				return cbp7512movementHeaders;
			}
		}
		CusInBondMoveHeaderCollection cbp7512movementHeaders;

		[ChildEditable]
		public CusInBondBillCollection BillsOfLadingMessages
		{
			get
			{
				if (billsOfLadingMessages == null)
				{
					billsOfLadingMessages = (CusInBondBillCollection)GetNewBillsCollection();
					RegisterEditableChildObject(billsOfLadingMessages);
				}
				return billsOfLadingMessages;
			}
		}
		CusInBondBillCollection billsOfLadingMessages;

		protected override Customs.Business.CusInBondMoveHeaderCollection GetMovementHeaders()
		{
			return new CusInBondMoveHeaderCollection(this);
		}

		[ChildEditable]
		public CusInBondMoveHeaderFilteredActiveCollection FilteredMovementHeaders
		{
			get
			{
				if (filteredMovementHeaders == null)
				{
					filteredMovementHeaders = new CusInBondMoveHeaderFilteredActiveCollection(this);
					RegisterEditableChildObject(filteredMovementHeaders);
				}
				return filteredMovementHeaders;
			}
		}
		CusInBondMoveHeaderFilteredActiveCollection filteredMovementHeaders;

		[ChildEditable]
		public CusInBondMoveDetailFilteredCollection FilteredMovementDetails
		{
			get
			{
				if (filteredMovementDetails == null)
				{
					filteredMovementDetails = new CusInBondMoveDetailFilteredCollection(this);
					RegisterEditableChildObject(filteredMovementDetails);
				}
				return filteredMovementDetails;
			}
		}
		CusInBondMoveDetailFilteredCollection filteredMovementDetails;

		[ChildEditable]
		public CusInBondMoveDetailFilteredCollection SelectedMovementDetails
		{
			get
			{
				if (selectedMovementDetails == null)
				{
					selectedMovementDetails = new CusInBondMoveDetailFilteredCollection(this, true);
					RegisterEditableChildObject(selectedMovementDetails);
				}
				return selectedMovementDetails;
			}
		}
		CusInBondMoveDetailFilteredCollection selectedMovementDetails;

		[ChildEditable]
		public CusInBondMoveDetailsCollection MovementDetails
		{
			get
			{
				if (movementDetails == null)
				{
					movementDetails = new CusInBondMoveDetailsCollection(this);
					RegisterEditableChildObject(movementDetails);
					movementDetails.Load();
				}
				return movementDetails;
			}
		}
		CusInBondMoveDetailsCollection movementDetails;

		public void ReloadCachedCollections()
		{
			movementDetails = null;
			filteredMovementDetails = null;
			_ = FilteredMovementDetails;
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			PopulateJobReferenceIfNeeded();
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				BH_JobReference = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		void PopulateJobReferenceIfNeeded()
		{
			PopulateNumberPropertyIfRequired(BH_JobReferenceInfo, x => GetNewJobReference(x));
		}

		ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			var shipment = Parent;
			if (shipment != null)
			{
				shipment.PopulateJobNumberIfNeeded();
				result = shipment.JobNumber;
			}
			else
			{
				result = Env.NumberFountains.USInBondJobReference.GetNextFormatted(factory);
			}

			return result;
		}

		#endregion

		#region Declaration

		public ZGuid DeclarationPK
		{
			get
			{
				if (!declarationPK.HasValue || !declarationPK.Value.IsValid)
				{
					if (!declarationPK.HasValue)
					{
						declarationPK = ZGuid.Empty;
					}
					var parent = Parent;
					if (parent != null)
					{
						var company = Company;
						if (company != null)
						{
							declarationPK = parent.GetDeclarationPK(company.PK);
						}
					}
				}
				return declarationPK.Value;
			}
		}
		ZGuid? declarationPK;

		public ZPropertyInfo DeclarationPKInfo
		{
			get { return GetZPropertyInfo(Schema.DeclarationPK); }
		}

		public JobDeclaration Declaration
		{
			get { return Factory.Load<JobDeclaration>(DeclarationPK); }
		}

		#endregion

		#region Parent

		public ICusInBondParent Parent
		{
			get
			{
				ICusInBondParent result = null;
				if (!BH_ParentID.IsEmpty)
				{
					switch (BH_ParentTableCode)
					{
						case JobShipmentSchema.Constants.Prefix:
							result = Factory.Load<ForwardingShipment>(BH_ParentID);
							break;
						case JobDeclarationSchema.Constants.Prefix:
							result = Factory.Load<JobDeclaration>(BH_ParentID);
							break;
						case JobConsolSchema.Constants.Prefix:
							result = Factory.Load<ForwardingConsol>(BH_ParentID);
							break;
					}
				}

				return result;
			}
		}

		#endregion

		#region Consol

		protected override ForwardingConsol GetConsol()
		{
			return Synchroniser.RelevantConsol;
		}

		#endregion

		#region Synchronizer

		internal bool CopyParentDefault
		{
			get { return Parent != null && Parent.TablePrefix != JobConsolSchema.Constants.Prefix && !BH_OverrideFreightDefaults; }
		}

		internal bool ActiveInMessaging
		{
			get { return MovementHeaders.Cast<CusInBondMoveHeader>().Any(moveHeader => moveHeader.ActiveInMessaging); }
		}

		internal string ParentTableName
		{
			get { return Parent != null ? Parent.TableName : string.Empty; }
		}

		public override bool ShouldSynchronise
		{
			get { return CopyParentDefault && !ActiveInMessaging; }
		}

		protected override void SynchroniseWithParentIfNeededCore()
		{
			if (ShouldSynchronise)
			{
				try
				{
					using (SuspendSettingHasChanges())
					using (GetValidationSuspender())
					{
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
						Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
						MovementHeader.Messages.CountChanged += new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
					}
				}
				finally
				{
					ClearSynchronisationHasChanges();
				}
			}
		}

		void ClearSynchronisationHasChanges()
		{
			ClearHasChanges();
		}

		void StopSynchronisationOnMessages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			MovementHeader.Messages.CountChanged -= new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
			Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));
		}

		public new CusInBondHeaderCommonSynchronizer Synchroniser
		{
			get { return (CusInBondHeaderCommonSynchronizer)base.Synchroniser; }
		}

		protected override BusinessObjectSynchroniser GetNewSynchroniserCore()
		{
			if (shipmentSynchroniser == null)
			{
				var parent = Parent ?? throw new NotSupportedException("You can't synchronise as the parent does not exist.");

				shipmentSynchroniser = CreateNewSynchroniser(parent);
#pragma warning disable
				((IBusinessObjectState)parent).UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
				((IBusinessObjectState)parent).UpdatedByDataRefreshIncludingChildren += OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
			}
			return shipmentSynchroniser;
		}
		CusInBondHeaderCommonSynchronizer shipmentSynchroniser;

		CusInBondHeaderCommonSynchronizer CreateNewSynchroniser(ICusInBondParent parent)
		{
			CusInBondHeaderCommonSynchronizer result = null;
			var shipment = parent as ForwardingShipment;
			if (shipment != null)
			{
				result = new CusInBondHeaderShipmentSynchronizer(this, shipment);
			}
			else
			{
				var declaration = parent as JobDeclaration;
				if (declaration != null)
				{
					result = new CusInBondHeaderDeclarationSynchronizer(this, declaration);
				}
			}
			return result;
		}

		void OnConsolWasUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			if (shipmentSynchroniser != null)
			{
				shipmentSynchroniser.Synchronise();
			}
		}

		protected void RefreshBindingAndChildrenReadOnly()
		{
			RefreshBindingIncludingChildren();
		}

		#endregion

		[ChildEditable]
		public ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader> WorkflowItems
		{
			get { return (ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>)((IWorkflowProvider)this).WorkflowItems; }
		}

		#region Message Initiator

		public ISendsMessagesToCustoms MessageInitiator
		{
			get
			{
				if (fMessageInitiator == null)
				{
					throw new ApplicationException("You can't perform this action that results in a message being sent because you have not hooked up a ISendsMessagesToCustoms to the CusInBondHeader");
				}

				return fMessageInitiator;
			}
			set { fMessageInitiator = value; }
		}

		public bool HasMessageInitiator
		{
			get { return fMessageInitiator != null; }
		}

		protected ISendsMessagesToCustoms fMessageInitiator;

		#endregion

		#region Bonded Warehouse Licence and Security

		public event LicenceLoginEventHandler BondedWarehouseLicenceLogin;

		public bool CurrentUserHasBondedWarehouseSecurityAccess
		{
			get
			{
				return Env.Security.USInBondEditBondedWarehouse.IsAllowed;
			}
		}

		internal bool CanRaiseBondedWarehouseLicenceLogin
		{
			get { return CurrentUserHasBondedWarehouseSecurityAccess && BondedWarehouseLicenceLogin != null; }
		}

		public void RaiseBondedWarehouseLicenceLogin(LicenceLoginEventArgs e)
		{
			if (BondedWarehouseLicenceLogin != null)
			{
				BondedWarehouseLicenceLogin(this, e);
			}
		}

		#endregion

		#region Implementation

		void ClearCommoditySupplierValuesIfSameAndMarkAsNeedingValidation(ZGuid headerValue)
		{
			MovementHeaders.MarkCommoditiesAsNeedingValidation((x) => x.ClearSupplierIfSameWithHeaderValue(headerValue));
		}

		protected override Type BillTypeCore
		{
			get { return typeof(CusInBondBill); }
		}

		protected override Type MovementHeaderTypeCore
		{
			get { return typeof(CusInBondMoveHeader); }
		}

		public sealed override ZBool BH_OverrideFreightDefaults
		{
			get { return base.BH_OverrideFreightDefaults; }
			set { SetOverrideFreightDefaults(value); }
		}

		public event CancelEventHandler OnOverrideFreightDefaultsChanging;

		void SetOverrideFreightDefaults(ZBool value)
		{
			var oldValue = BH_OverrideFreightDefaults;
			if (oldValue != value)
			{
				if (!value && Parent != null)
				{
					var args = new CancelEventArgs(false);
					if (OnOverrideFreightDefaultsChanging != null)
					{
						OnOverrideFreightDefaultsChanging(this, args);
					}

					if (!args.Cancel)
					{
						base.BH_OverrideFreightDefaults = value;
						BH_OverrideFreightDefaultsInfo.RefreshBinding(oldValue);
						Synchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
					}
					else
					{
						BH_OverrideFreightDefaultsInfo.RefreshBinding();
					}
				}
				else
				{
					base.BH_OverrideFreightDefaults = value;
					BH_OverrideFreightDefaultsInfo.RefreshBinding(oldValue);
					if (shipmentSynchroniser != null)
					{
						shipmentSynchroniser.SetEnabled(false, shipmentSynchroniser.DetectEnabled);
						shipmentSynchroniser.Dispose();
						shipmentSynchroniser = null;
					}
				}

				RefreshBindingAndChildrenReadOnly();
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (humanReadableNameCoreCached == null)
				{
					humanReadableNameCoreCached = new CachedProperty<ZString>(Factory, delegate
					{
						if (Parent == null)
						{
							return BH_JobReference;
						}
						else
						{
							return "In-Bond " + BH_JobReference;
						}
					});
				}
				return humanReadableNameCoreCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableNameCoreCached;

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var importerName = (ImporterOrg != null) ? string.Format(CultureInfo.CurrentCulture, " - {0}", ImporterOrg.OH_FullName) : string.Empty;
				return Res.GetString("345E83E2-0E05-4250-9E81-26102A594220", "{0}{1}", BH_JobReference, importerName);
			}
		}

		void DefaultFromImportConveyanceIfPossible()
		{
			RefVessel vessel = ImportConveyance;
			if (vessel != null)
			{
				BH_ImportConveyanceCountry = vessel.RV_RN_NKCountryOfReg;
			}
		}

		protected override ZAddress GetNewBH_OA_Importer_ZAddress()
		{
			ZAddress result = base.GetNewBH_OA_Importer_ZAddress();
			result.GetDefaultAddress = GetDefaultImporterAddress;
			return result;
		}

		ZGuid GetDefaultImporterAddress(IOrgHeader orgHeader)
		{
			OrgHeader header = orgHeader as OrgHeader;
			return header == null ? ZGuid.Empty : header.MainAddress.PK;
		}

		protected override Customs.Business.CusInBondHeaderLookups GetNewLookups()
		{
			return new CusInBondHeaderLookups(this);
		}

		protected override Customs.Business.CusInBondHeaderValidation GetNewValidation()
		{
			return new CusInBondHeaderValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
		}

		#region Synchronize on Creation

		public void Synchronize()
		{
			if (Parent != null)
			{
				SetOverrideFreightDefaults(false);
			}
		}

		#endregion

		#region Saving and Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#endregion

		#region ITemplateCopyable Members

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return new CusInBondHeaderDeepCloneStrategy(this).Clone();
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new CusInBondHeaderDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new CusInBondHeaderDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IHaveRequiredDocuments Members

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return null; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return ZString.Empty; }
		}

		Logs IHaveRequiredDocuments.Logs
		{
			get { return Logs; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return ZString.Empty; }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
		}

		[ChildEditable]
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

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return TablePrefix; }
		}

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return BH_JobReference; }
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var provider = Parent as IControllerIDProvider;
				return provider != null ? provider.BusinessObjectPK : PK.ToGuid();
			}
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var provider = Parent as IControllerIDProvider;
				return provider != null ? provider.ControllerID : ControllerIDs.Customs.US.InBond;
			}
		}

		#endregion

		#region IWorkflowProvider

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		protected override ProcessTaskCollection GetNewCusInBondHeaderProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>(this);
		}

		#endregion

		#region IWorkflowProviderCore Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var importer = Importer;
			if (importer != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, importer.OA_OH, ZGuid.Empty);
			}
			else
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, ZGuid.Empty);
			}
			result.Add(ProcessTaskTemplateSchema.P0_GB, BH_GB, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, BH_Calc_ImportLoadPortUNLOCO, BH_Calc_ImportLoadPortUNLOCO.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, BH_Calc_PortUnladingUNLOCO, BH_Calc_PortUnladingUNLOCO.Substring(0, 2), ZString.Empty);
			return result;
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var parent = Parent as IWorkflowProviderCore;
				if (parent != null)
				{
					list.Add(parent);
				}
				return list;
			}
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return BH_JobReference; }
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				EventHandler passed = value;
				BH_OA_ImporterInfo.ValueChanged += passed;
			}
			remove
			{
				EventHandler passed = value;
				BH_OA_ImporterInfo.ValueChanged -= passed;
			}
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return ImporterOrg; }
		}

		#endregion

		#region ICargoManifestStatusQueryHeader Members

		CodeDescriptionPairList ICargoManifestStatusQueryHeader.ActionList
		{
			get
			{
				var actionList = new CargoManifestStatusQueryActionList();
				actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.Entry);

				if (!IsAir)
				{
					actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.MAWB);
					actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.HAWB);
				}
				else
				{
					actionList.RemoveCode(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill);
				}

				actionList.Sort();
				return actionList;
			}
		}

		IEnumerable<ICargoManifestStatusQueryData> ICargoManifestStatusQueryHeader.ObjectsForQuery
		{
			get
			{
				var result = new List<ICargoManifestStatusQueryData>();

				foreach (CusInBondMoveHeader moveHeader in MovementHeaders)
				{
					result.Add(moveHeader);
				}

				AddBillsForCargoManifestQuery(result);

				return result;
			}
		}

		void AddBillsForCargoManifestQuery(List<ICargoManifestStatusQueryData> bills)
		{
			if (ZZCustomsFunctionality.IsAMSHBREffective && IsSea)
			{
				var addedMasterBills = new List<ZString>();
				foreach (var bill in Bills)
				{
					if (!addedMasterBills.Contains(bill.B0_MasterBillNumber))
					{
						bills.Add(new CusInBondBillCargoManifestStatusQueryWrapper(bill, false, true));
						addedMasterBills.Add(bill.B0_MasterBillNumber);
					}
					if (!bill.B0_HouseBillNumber.IsEmpty)
					{
						bills.Add(new CusInBondBillCargoManifestStatusQueryWrapper(bill, true, true));
					}
				}
			}
			else
			{
				foreach (var bill in Bills)
				{
					bills.Add(new CusInBondBillCargoManifestStatusQueryWrapper(bill, false, false));
				}
			}
		}

		ZBool ICargoManifestStatusQueryHeader.IsACEQuery
		{
			get { return true; }
		}

		ZString ICargoManifestStatusQueryHeader.ProcessingPortCode
		{
			get { return USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(GlbBranch.CurrentBranch.GB_GC.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty); }
		}

		ZString ICargoManifestStatusQueryHeader.ProcessingOfficeCode
		{
			get { return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty); }
		}

		ZString ICargoManifestStatusQueryHeader.TransportMode
		{
			get { return BH_Calc_FreightTransportMode; }
		}

		ZString ICargoManifestStatusQueryHeader.EntryFilerCode
		{
			get { return USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode; }
		}

		#endregion
	}
}

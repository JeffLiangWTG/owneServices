using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using MetaData = CargoWise.ComponentModel.MetaData;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business
{
	[CodeProperty(AutoJobContainer.Schema.JC_ContainerNum), DescriptionProperty(AutoJobContainer.Schema.JC_ContainerNum)]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = "OnUniversalCopyFinish")]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	[UniversalCopyIgnoreElement("OriginPickupConfirms", "DestinationDeliveryConfirms", "OriginCFSArrivalConfirms", "OriginCFSDepartureConfirms", "DestinationCFSArrivalConfirms", "DestinationCFSDepartureConfirms")]
	[UniversalDataContext(DataContextType.ForwardingContainer)]
	public class CommonContainer : AutoJobContainer,
		Enterprise.Integration.Freight.ICommonContainer,
		IDocManagerSupport,
		IHaveServices,
		IContainer,
		IImportExport,
		IDocAddresses,
		ISupportDataImporting,
		IPRAContainerMessaging,
		IContainerForBinding,
		IWorkflowProvider,
		IConfirmAddressParent,
		INoteSource,
		IContainerStorageDataProvider,
		ICreditControlledDocumentDelivery,
		IJobAddressAdditionalInfoSupport,
		IJobNumberForWorkflow,
		IJobNumber,
		IDefaultNumberOfDecimalsSupporterWithSchemaColumn,
		IEventDatePropertyChecker,
		IAdditionalReferenceNumberSupporter,
		IAdditionalReferenceNumberTypeProvider,
		ICusEntryNumberValidationDeciderOfType,
		IRoutingSupport
	{
		public CommonContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CreationStackTrace = new StackTrace(1).ToString();
			CreationDateTimeUtc = ZDateTime.UtcNow;
			JC_OA_ArrivalContainerYardAddress_ZAddress.DefaultAddressType = AddressType.DLV;
			JC_OA_DepartureContainerYardAddress_ZAddress.DefaultAddressType = AddressType.PIC;
		}

		public string CreationStackTrace { get; }
		public ZDateTime CreationDateTimeUtc { get; }
		public string DeletionLoggingDetails { get; private set; }
		public void AddLoggingDetail(string detail)
		{
			if (!string.IsNullOrEmpty(detail))
			{
				var builder = loggingDetails ?? (loggingDetails = new ZStringBuilder());
				builder.AppendLine(detail);
			}
		}
		ZStringBuilder loggingDetails;

		#region Type Decider

		public static readonly ContainerTypeDecider TypeDecider = new ContainerTypeDecider();

		#endregion

		#region Schema

		public new abstract class Schema : AutoJobContainer.Schema
		{
			public const string ContainerCode = "ContainerCode";
			public const string ArrivalContainerYardAddressOrg = "ArrivalContainerYardAddressOrg";
			public const string ArrivalContainerYardAddressCode = "ArrivalContainerYardAddressCode";
			public const string ArrivalCTOAddressOrg = "ArrivalCTOAddressOrg";
			public const string ArrivalCTOAddress = "ArrivalCTOAddress";
			public const string ArrivalCTOAddressCode = "ArrivalCTOAddressCode";
			public const string ArrivalUnpackAddressCode = "ArrivalUnpackAddressCode";
			public const string ArrivalUnpackAddress = "ArrivalUnpackAddress";
			public const string ArrivalUnpackAddressOrg = "ArrivalUnpackAddressOrg";
			public const string DepartureContainerYardAddressCode = "DepartureContainerYardAddressCode";
			public const string DepartureContainerYardAddressOrg = "DepartureContainerYardAddressOrg";
			public const string DepartureCTOAddressCode = "DepartureCTOAddressCode";
			public const string DepartureCTOAddress = "DepartureCTOAddress";
			public const string DepartureCTOAddressOrg = "DepartureCTOAddressOrg";
			public const string DeparturePackAddressOrg = "DeparturePackAddressOrg";
			public const string DeparturePackAddressCode = "DeparturePackAddressCode";
			public const string DeparturePackAddress = "DeparturePackAddress";
			public const string IsFreezer = "IsFreezer";
			public const string IsChiller = "IsChiller";

			// Calculated From CommonShipment Totals
			public const string JC_Calc_TotalVolume = "JC_Calc_TotalVolume";
			public const string JC_Calc_TotalVolumeUnit = "JC_Calc_TotalVolumeUnit";
			public const string JC_Calc_TotalVolumeInM3 = "JC_Calc_TotalVolumeInM3";
			public const string JC_Calc_TotalWeight = "JC_Calc_TotalWeight";
			public const string JC_Calc_TotalWeightUnit = "JC_Calc_TotalWeightUnit";
			public const string JC_Calc_TotalWeightInKgs = "JC_Calc_TotalWeightInKgs";
			public const string JC_Calc_TotalPackages = "JC_Calc_TotalPackages";
			public const string JC_Calc_TotalPackagesUnit = "JC_Calc_TotalPackagesUnit";

			public const string JC_Calc_ActualGrossWeightInKgs = "JC_Calc_ActualGrossWeightInKgs";

			// Calculated from dbo.RefContainer
			public const string JC_Calc_ContainerCapacity = "JC_Calc_ContainerCapacity";
			public const string JC_Calc_TareWeight = "JC_Calc_TareWeight";
			public const string JC_Calc_MaxGrossWeight = "JC_Calc_MaxGrossWeight";
			public const string JC_Calc_Length = "JC_Calc_Length";
			public const string JC_Calc_Width = "JC_Calc_Width";
			public const string JC_Calc_Height = "JC_Calc_Height";
			public const string JC_Calc_TEUCount = "JC_Calc_TEUCount";
			public const string JC_Calc_ContainerCount = "JC_Calc_ContainerCount";
			public const string JC_Is20GP = "JC_Is20GP";
			public const string JC_Is40GP = "JC_Is40GP";
			public const string JC_Is20RE = "JC_Is20RE";
			public const string JC_Is40RE = "JC_Is40RE";
			public const string JC_IsOtherContainerType = "JC_IsOtherContainerType";

			// Calculated from Actual Sizes entered against this container
			public const string JC_Calc_ActualCapacity = "JC_Calc_ActualCapacity";
			public const string JC_Calc_OverhangHeight = "JC_Calc_OverhangHeight";
			public const string JC_Calc_OverhangLength = "JC_Calc_OverhangLength";
			public const string JC_Calc_OverhangWidth = "JC_Calc_OverhangWidth";
			public const string JC_Calc_OverhangFront = "JC_Calc_OverhangFront";
			public const string JC_Calc_OverhangLeft = "JC_Calc_OverhangLeft";

			public const string JC_Calc_DeparturePackAddressOrg = "JC_Calc_DeparturePackAddressOrg";
			public const string JC_Calc_DeparturePackAddressCode = "JC_Calc_DeparturePackAddressCode";
			public const string JC_Calc_DepartureCTOAddressOrg = "JC_Calc_DepartureCTOAddressOrg";
			public const string JC_Calc_DepartureCTOAddressCode = "JC_Calc_DepartureCTOAddressCode";
			public const string JC_Calc_DepartureContainerYardAddressOrg = "JC_Calc_DepartureContainerYardAddressOrg";
			public const string JC_Calc_DepartureContainerYardAddressCode = "JC_Calc_DepartureContainerYardAddressCode";
			public const string JC_Calc_ArrivalUnpackAddressOrg = "JC_Calc_ArrivalUnpackAddressOrg";
			public const string JC_Calc_ArrivalUnpackAddressCode = "JC_Calc_ArrivalUnpackAddressCode";
			public const string JC_Calc_ArrivalCTOAddressOrg = "JC_Calc_ArrivalCTOAddressOrg";
			public const string JC_Calc_ArrivalCTOAddressCode = "JC_Calc_ArrivalCTOAddressCode";
			public const string JC_Calc_ArrivalContainerYardAddressOrg = "JC_Calc_ArrivalContainerYardAddressOrg";
			public const string JC_Calc_ArrivalContainerYardAddressCode = "JC_Calc_ArrivalContainerYardAddressCode";
			public const string JC_Calc_DeparturePackAddress = "JC_Calc_DeparturePackAddress";
			public const string JC_Calc_DepartureCTOAddress = "JC_Calc_DepartureCTOAddress";
			public const string JC_Calc_DepartureContainerYardAddress = "JC_Calc_DepartureContainerYardAddress";
			public const string JC_Calc_ArrivalUnpackAddress = "JC_Calc_ArrivalUnpackAddress";
			public const string JC_Calc_ArrivalCTOAddress = "JC_Calc_ArrivalCTOAddress";
			public const string JC_Calc_ArrivalContainerYardAddress = "JC_Calc_ArrivalContainerYardAddress";

			public const string JC_Calc_CartageType = "JC_Calc_CartageType";
			public const string JC_Calc_TransportJobNum = "JC_Calc_TransportJobNum";
			public const string JC_Calc_ConsolID = "JC_Calc_ConsolID";
			public const string JC_Calc_MasterBillNum = "JC_Calc_MasterBillNum";

			public const string JC_JA_NKPortOfLoading = "JC_JA_NKPortOfLoading";
			public const string JC_JB_NKPortOfDischarge = "JC_JB_NKPortOfDischarge";
			public const string JC_JV_NKVessel = "JC_JV_NKVessel";
			public const string JC_JV_VoyageFlight = "JC_JV_VoyageFlight";
			public const string JC_JA_E_DEP = "JC_JA_E_DEP";
			public const string JC_JB_E_ARV = "JC_JB_E_ARV";

			public const string JC_OverriddenFCLAvailable = "JC_OverriddenFCLAvailable";
			public const string JC_OverriddenFCLStorage = "JC_OverriddenFCLStorage";
			public const string JC_OverriddenLCLAvailable = "JC_OverriddenLCLAvailable";
			public const string JC_OverriddenLCLStorage = "JC_OverriddenLCLStorage";

			public const string JC_ContainerCode = "JC_ContainerCode";

			public const string JC_Calc_NetWeight = "JC_Calc_NetWeight";

			public const string CurrentPRAStatus = "CurrentPRAStatus";

			public const string JC_LastFreeDay = "JC_LastFreeDay";

			public const string GoodsWeightForBinding = "GoodsWeightForBinding";
			public const string ContainerWeightUnit = "ContainerWeightUnit";
			public const string Transport = "Transport";

			public const string GrossWeightVerifiedByNameOrPK = "GrossWeightVerifiedByNameOrPK";

			public const string RelatedContainerLoadListPK = "RelatedContainerLoadListPK";

			public const string EmptyPickupByTransportMode = "EmptyPickupByTransportMode";
			public const string EmptyReturnToTransportMode = "EmptyReturnToTransportMode";
		}

		#endregion

		#region Related Business Objects

		#region JC_TempRecorderSerialNo

		public bool JC_TempRecorderSerialNo_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		#endregion

		#region JC_HumidityPercent

		public bool JC_HumidityPercent_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		#endregion

		#region JC_RefrigGeneratorID

		public bool JC_RefrigGeneratorID_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		#endregion

		#region IsNonOperativeReefer

		public bool JC_IsNonOperativeReefer_ReadOnly
		{
			get
			{
				if (RefContainer?.RC_ISOType.SubstringSafe(2, 1).ToString() == "R")
				{
					if (RefContainer.RC_ContainerType == Constants.ContainerTypes.DryStorage)
					{
						return true;
					}
					return false;
				}
				else if (JC_IsNonOperativeReefer)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public override ZBool JC_IsNonOperativeReefer
		{
			get
			{
				return base.JC_IsNonOperativeReefer;
			}
			set
			{
				base.JC_IsNonOperativeReefer = value;

				if (value)
				{
					JC_IsControlledAtmosphere = false;
					IsFreezer = false;
					JC_SetPointTemp = 0m;
					JC_SetPointTempUnit = ZString.Empty;
					JC_AirVentFlow = 0m;
					JC_AirVentFlowRateUnit = ZString.Empty;
					JC_TempRecorderSerialNo = ZString.Empty;
					JC_RefrigGeneratorID = ZString.Empty;
					JC_HumidityPercent = 0;
				}
				else if (!IsValidationSuspended)
				{
					if (!JC_RC.IsEmpty)
					{
						BehaviorStrategy.RefContainerAttached(this);
					}
					Validation.ValidateJC_TempRecorderSerialNo();
					Validation.ValidateJC_SetPointTemp();
					Validation.ValidateJC_IsControlledAtmosphere();
					Validation.ValidateIsChiller();
					Validation.ValidateIsFreezer();
				}
				Validation.ValidateJC_SetPointTempUnit();
				Validation.ValidateJC_HumidityPercent();
				Validation.ValidateJC_AirVentFlow();
				Validation.ValidateJC_AirVentFlowRateUnit();
			}
		}

		#endregion

		#region ContainerParent

		public IContainerParent ContainerParent
		{
			get { return (IContainerParent)Consol ?? (IContainerParent)Declaration; }
		}

		#endregion

		#region Cartage Helper

		ICartageContainerHelper CartageContainerHelper
		{
			get
			{
				if (cartageContainerHelper == null)
				{
					var type = ObjectFactory.GetType<ICartageContainerHelper>();

					cartageContainerHelper = (ICartageContainerHelper)Activator.CreateInstance(type, Factory, this);
				}

				return cartageContainerHelper;
			}
		}

		ICartageContainerHelper cartageContainerHelper;

		public ICommonCartage DestinationCartage
		{
			get { return CartageContainerHelper.DestinationCartage; }
		}

		#endregion

		#region Consol

		public CommonConsol Consol
		{
			get { return LoadParentConsol(); }
		}

		protected virtual CommonConsol LoadParentConsol()
		{
			return Factory.Load<CommonConsol>(JC_JK);
		}

		#endregion

		#region Confirms

		[ChildEditable()]
		public CommonPickupDeliveryConfirmCollection Confirms
		{
			get
			{
				if (confirms == null)
				{
					confirms = new CommonPickupDeliveryConfirmCollection(this);
					if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
					{
						RegisterEditableChildObject(confirms);
					}
				}
				return confirms;
			}
		}
		CommonPickupDeliveryConfirmCollection confirms;

		#endregion

		#region OriginConfirm

		public CommonPickupDeliveryConfirm OriginConfirm
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.OriginPickup, true); }
		}

		public CommonPickupDeliveryConfirm DestinationConfirm
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery, true); }
		}

		/// <summary>
		/// Retrieves the existing OriginConfirm. If one does not exist then it
		/// will not be created and `null` is returned instead.
		/// </summary>
		public CommonPickupDeliveryConfirm OriginGetConfirm
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.OriginPickup, false); }
		}

		/// <summary>
		/// Retrieves the existing DestinationConfirm. If one does not exist then it
		/// will not be created and `null` is returned instead.
		/// </summary>
		public CommonPickupDeliveryConfirm DestinationGetConfirm
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery, false); }
		}

		#region Only used in CFS atm

		public CommonPickupDeliveryConfirm OriginCFSArrival
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.OriginCFSArrival, true); }
		}

		public CommonPickupDeliveryConfirm OriginCFSDeparture
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.OriginCFSDeparture, true); }
		}

		public CommonPickupDeliveryConfirm DestinationCFSArrival
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival, true); }
		}

		public CommonPickupDeliveryConfirm DestinationCFSDeparture
		{
			get { return GetOrCreateConfirm(Core.Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture, true); }
		}

		#endregion

		/// <summary>
		/// Get existing confirm or create a new empty confirm on the container.
		/// If the confirm is new and empty, Factory.Save will not save it to the database, see <see cref="CommonPickupDeliveryConfirm.IsSavedByFactory">confirm.IsSavedByFactory</see>
		/// </summary>
		protected virtual CommonPickupDeliveryConfirm GetOrCreateConfirm(string pickupDeliveryType, bool isAllowToBeCreated)
		{
			var result = FindConfirmOfType(pickupDeliveryType);

			if (result == null && isAllowToBeCreated)
			{
				result = Confirms.AddNew();

				using (result.SuspendSettingHasChanges())
				{
					result.EU_PickupDeliveryType = pickupDeliveryType;
					result.EU_JC = PK;
				}

				result.HasChanges = false;
			}

			return result;
		}

		internal CommonPickupDeliveryConfirm FindConfirmOfType(string pickupDeliveryType)
		{
			CommonPickupDeliveryConfirm result = null;

			foreach (CommonPickupDeliveryConfirm confirm in Confirms)
			{
				if (confirm.EU_PickupDeliveryType == pickupDeliveryType)
				{
					result = confirm;
					break;
				}
			}

			return result;
		}

		#endregion

		#region JobServices

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(JobServiceSchema.Constants.TableName, JobServiceSchema.Constants.ES_ParentID, OverrideCollectionName = "Services")]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (fServices == null)
				{
					fServices = GetNewServiceCollection();
					fServices.Load();
					if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
					{
						RegisterEditableChildObject(fServices);
					}
				}
				return fServices;
			}
		}

		protected virtual JobServiceDependentCollection GetNewServiceCollection()
		{
			return new JobServiceDependentCollection(this, Factory);
		}

		JobServiceDependentCollection fServices;

		#endregion

		#region Container

		public RefContainer RefContainer
		{
			get { return Factory.Load<RefContainer>(JC_RC); }
		}

		#endregion

		#region PackLines

		[ChildEditable(true)]
		public PackLineManyToManyCollection PackLines
		{
			get
			{
				if (fPackLines == null)
				{
					fPackLines = GetNewPackLineCollection();
					fPackLines.Load();
					if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
					{
						RegisterEditableChildObject(fPackLines);
					}
					fPackLines.CountChanged += new CollectionCountChangedEventHandler(PackLines_CountChanged);
				}
				return fPackLines;
			}
		}

		PackLineManyToManyCollection fPackLines;

		protected virtual PackLineManyToManyCollection GetNewPackLineCollection()
		{
			PackLineManyToManyCollection result = new PackLineManyToManyCollection(this);
			if (!IsDeleted && Sailing != null)
			{
				result.SetReadOnlyIncludingChildren(true);
			}
			return result;
		}

		protected virtual void OnPackLineCountChanged(CollectionCountChangedEventArgs e)
		{
			if (!IsDeleted && !PackLines.IsLoading)
			{
				SetGrossWeightFromCombinedWeights();
			}
			parentShipments = null;
		}

		void PackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnPackLineCountChanged(e);
		}

		public void AddPackLine(PackLine line)
		{
			if (ShipmentHasMultiplePackLinesInCollection(line, PackLines.ToArray()))
			{
				ReJoinPackLines(line, PackLines.ToArray());
				line.Delete();
			}
			else
			{
				if (Consol != null)
				{
					line.SetContainer(Consol, this);
				}
				else
				{
					PackLines.Add(line);
				}
				line.RefreshBinding();
			}

			JC_Calc_TotalWeightInfo.RefreshBinding();
			JC_Calc_TotalVolumeInfo.RefreshBinding();
			JC_Calc_TotalPackagesInfo.RefreshBinding();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This code is only ever called from the view model layer.")]
		public void AdjustOverriddenGrossWeightWithUserConfirmation(PackLine packline)
		{
			if (JC_IsGrossWeightOverridden && ShouldPromptUser())
			{
				var msg = Res.GetString("5de9651c-7d81-4d8e-8bae-267b8284d57d", "Attaching Shipment {0}. Gross Weight of Container {1} is overridden. Would you like to retain the Overridden Gross Weight?", packline.JL_Calc_JS_UniqueConsignRef, JC_ContainerNum);
				var answer = Globals.Message.Show(msg, "", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes);
				if (answer == ZDialogResult.No)
				{
					JC_IsGrossWeightOverridden = false;
					SetGrossWeightFromCombinedWeights();
				}
			}

			bool ShouldPromptUser()
			{
				var shipmentPackLines = PackLines.FilterByShipment(packline.Shipment);
				var packlinesCount = shipmentPackLines.Count;

				if (packlinesCount == 1 && shipmentPackLines.Single().PK == packline.PK)
				{
					return true;
				}

				return packlinesCount == 0;
			}
		}

		public void AddPackLines(BusinessObject[] packLines)
		{
			foreach (PackLine line in packLines)
			{
				this.AddPackLine(line);
			}
		}

		public void RemovePackLine(PackLine line)
		{
			if (ShipmentHasMultiplePackLinesInCollection(line, GetMasterUnallocatedPackLinesArray()))
			{
				ReJoinPackLines(line, GetMasterUnallocatedPackLinesArray());
				line.Delete();
			}
			else
			{
				PackLines.Remove(line);
				line.RefreshBinding(); //Refreshes Unallocated PackLines Grid
			}

			JC_Calc_TotalWeightInfo.RefreshBinding();
			JC_Calc_TotalVolumeInfo.RefreshBinding();
			JC_Calc_TotalPackagesInfo.RefreshBinding();
		}

		public void RemovePackLines(BusinessObject[] packLines)
		{
			foreach (PackLine line in packLines)
			{
				this.RemovePackLine(line);
			}
		}

		public void UnpackPackLinesFrom(CommonShipment shipment)
		{
			var packLinesToRemove = PackLines.FilterByShipment(shipment).ToArray();

			RemovePackLines(packLinesToRemove);
		}

		#region PackLine Rejoining

		protected bool ShipmentHasMultiplePackLinesInCollection(PackLine packLine, BusinessObject[] packLines)
		{
			bool shouldRejoinSameShipmentPackLines = false;

			if (packLine != null && packLines != null)
			{
				for (int packCount = 0; packCount < packLines.Length && !shouldRejoinSameShipmentPackLines; packCount++)
				{
					var existingLine = (PackLine)packLines[packCount];

					if (IsRejoinablePackLine(packLine, existingLine))
					{
						for (int count = 0; count < ((IBusinessObjectInternals)this).ParentCollections.Length && !shouldRejoinSameShipmentPackLines; count++)
						{
							var containerCollection = ((IBusinessObjectInternals)this).ParentCollections[count] as CommonContainerCollection;
							if (containerCollection != null)
							{
								shouldRejoinSameShipmentPackLines = containerCollection.CheckReJoinPackLines(packLine);
							}
						}
					}
				}
			}

			return shouldRejoinSameShipmentPackLines;
		}

		protected void ReJoinPackLines(PackLine line, BusinessObject[] lines)
		{
			if (line != null && lines != null)
			{
				foreach (var existingLine in lines.Cast<PackLine>())
				{
					if (IsRejoinablePackLine(line, existingLine))
					{
						existingLine.JL_ActualVolume += line.JL_ActualVolume;
						existingLine.JL_ActualWeight += line.JL_ActualWeight;
						existingLine.JL_PackageCount += line.JL_PackageCount;

						ZString newMarksAndNumbers = existingLine.JL_MarksAndNumbers.Trim() + System.Environment.NewLine + line.JL_MarksAndNumbers.Trim();
						existingLine.JL_MarksAndNumbers = newMarksAndNumbers.SubstringSafe(0, existingLine.JL_MarksAndNumbersInfo.MaxLength);

						UpdateExistingLineDivot(existingLine, line.JL_PackageCount);

						break;
					}
				}
			}
		}

		void UpdateExistingLineDivot(PackLine packLine, ZInt packageCount)
		{
			var divotses = packLine.ConfirmDivots.GroupBy(c => c.Confirm != null ? c.Confirm.EU_PickupDeliveryType : ZString.Empty);
			foreach (var divots in divotses)
			{
				if (!string.IsNullOrWhiteSpace(divots.Key))
				{
					var divot = divots.FirstOrDefault();
					if (divot != null)
					{
						divot.J8_PackagesDelivered += packageCount;
					}
				}
			}
		}

		protected bool IsRejoinablePackLine(PackLine lineToRejoin, PackLine existingLine)
		{
			bool oKToJoin = false;

			if (lineToRejoin != null && existingLine != null && lineToRejoin.PK != existingLine.PK)
			{
				bool sameShipment = existingLine.JL_Calc_JS_UniqueConsignRef == lineToRejoin.JL_Calc_JS_UniqueConsignRef;
				bool samePackType = existingLine.JL_F3_NKPackType == lineToRejoin.JL_F3_NKPackType;
				bool existingHasDims = !existingLine.JL_Height.IsEmpty && !existingLine.JL_Length.IsEmpty && !existingLine.JL_Width.IsEmpty;
				bool lineHasDims = !lineToRejoin.JL_Height.IsEmpty && !lineToRejoin.JL_Length.IsEmpty && !lineToRejoin.JL_Width.IsEmpty;
				bool existingHasNoDims = existingLine.JL_Height.IsEmpty && existingLine.JL_Length.IsEmpty && existingLine.JL_Width.IsEmpty;
				bool lineHasNoDims = lineToRejoin.JL_Height.IsEmpty && lineToRejoin.JL_Length.IsEmpty && lineToRejoin.JL_Width.IsEmpty;
				bool heightEqual = existingLine.JL_Height == lineToRejoin.JL_Height;
				bool lengthEqual = existingLine.JL_Length == lineToRejoin.JL_Length;
				bool widthEqual = existingLine.JL_Width == lineToRejoin.JL_Width;

				if (sameShipment && samePackType &&
					((existingHasNoDims && lineHasNoDims) ||
					(existingHasDims && lineHasDims && heightEqual && lengthEqual && widthEqual)))
				{
					oKToJoin = true;
				}
			}

			return oKToJoin;
		}

		#endregion

		public virtual BusinessObject[] GetMasterUnallocatedPackLinesArray()
		{
			return (Sailing != null) ? Sailing.UnAllocatedPackLines.ToArray() : null;
		}

		public virtual UnAllocatedPackLinesView GetMasterUnallocatedPackLinesView()
		{
			return (Sailing != null) ? Sailing.UnAllocatedPackLines : null;
		}

		#endregion

		#region Declaration

		internal BusinessObject Declaration
		{
			get
			{
				BusinessObject result = null;
				if (CanHaveRelatedDeclaration)
				{
					BusinessObject cusContainer = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, PK));
					if (cusContainer != null && !cusContainer.IsDeleted)
					{
						ZGuid decPK = (ZGuid)cusContainer[CusContainerSchema.CO_JE];
						result = (BusinessObject)Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(decPK);
					}
				}

				return result;
			}
		}

		protected virtual bool CanHaveRelatedDeclaration
		{
			get { return true; }
		}

		#endregion

		#region Sailing

		public JobSailing Sailing
		{
			get { return Consol != null ? Consol.Schedule : Factory.Load<JobSailing>(JC_JX); }
		}

		protected JobSailing StandaloneSailing
		{
			get
			{
				return Booking?.Sailing
					?? (JobSailing)DestinationCartage?.SailingStandalone;
			}
		}

		#endregion

		#region Transport

		protected Transport ArrivalTransport
		{
			get
			{
				if (ContainerParent != null)
				{
					var transportParent = (ITransportParent)ContainerParent;
					var lastLeg = new TransportOrderHelper(transportParent.Transports).LastLeg;
					return lastLeg;
				}
				return null;
			}
		}

		#endregion

		#region Booking

		public CommonShipment Booking
		{
			get { return (CommonShipment)Factory.Load(ShipmentType, JC_JS_FCLBookingOnlyLink); }
		}

		protected virtual Type ShipmentType
		{
			get { return typeof(CommonShipment); }
		}

		#endregion

		#region DocsAndCartage

		IEnumerable<JobDocsAndCartage> DocsAndCartage
		{
			get
			{
				if (ContainerParent != null)
				{
					foreach (JobDocsAndCartage docsAndCartage in ContainerParent.DocsAndCartage(this))
					{
						yield return docsAndCartage;
					}
				}
			}
		}

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				if (fNoteTypes == null)
				{
					fNoteTypes = base.NoteTypesCore;
					fNoteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
					fNoteTypes.Add(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);
					fNoteTypes.Add(AdditionalNoteTypes);
				}
				return fNoteTypes;
			}
		}
		NoteTypeCollection fNoteTypes;

		protected virtual NoteTypeCollection AdditionalNoteTypes
		{
			get
			{
				return new NoteTypeCollection
				{
					PredefinedNoteTypes.Instance.DeliveryInstructionsNote,
					PredefinedNoteTypes.Instance.PickupInstructionsNote,
					PredefinedNoteTypes.Instance.HandlingInstructions,
					PredefinedNoteTypes.Instance.AutoRatingAuditLog
				};
			}
		}

		#endregion

		#region CartageCompanyDeliveringToCTO

		public OrgHeader CartageCompanyDeliveringToCTO
		{
			get
			{
				foreach (PackLine packLine in PackLines)
				{
					if (packLine.Shipment != null)
					{
						if (packLine.Shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr.IsValid)
						{
							return packLine.Shipment.DocsAndCartage.PickupCartageCo;
						}
					}
				}

				return null;
			}
		}

		#endregion

		#region Supplier Booking

		public IJobSupplierBooking SupplierBooking
		{
			get
			{
				if (supplierBooking == null)
				{
					supplierBooking = Factory.Load<IJobSupplierBooking>(JC_JSB_SupplierBooking);
				}

				return supplierBooking;
			}
		}

		IJobSupplierBooking supplierBooking;

		#endregion

		public CommonShipment LinkedShipment { get; set; }

		#endregion

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();

			if (!IsDeleted)
			{
				LogDeleteOnParents();

				for (int i = PackLines.Count - 1; i > 0; i--)
				{
					PackLine packLine = PackLines[i];
					if (!packLine.IsDeleted)
					{
						if (packLine.JL_JS.IsEmpty)
						{
							packLine.Delete();
						}
					}
				}

				PackLines.RemoveAll();

				var cusContainers = Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, PK));
				foreach (var cusContainer in cusContainers)
				{
					if (!cusContainer.IsDeleted && !cusContainer.IsDeletingJobContainer)
					{
						if (DeletionLoggingDetails == null)
						{
							var details = new ZStringBuilder(new StackTrace(1).ToString());
							if (loggingDetails != null)
							{
								details.Prepend(loggingDetails.ToStringWithNewLineBetweenAppends());
							}
							DeletionLoggingDetails = details.ToStringWithNewLineBetweenAppends();
						}
						cusContainer.CO_JC = ZGuid.Empty;
					}
				}

				CartageContainerHelper.DeleteAllBookedMoves();
				Confirms.DeleteAll();

				ImportPenalties.DeleteAll();
				ExportPenalties.DeleteAll();
				PickupPenalties.DeleteAll();
				DeliveryPenalties.DeleteAll();
				(JobAddressAdditionalInfoCollection as JobAddressAdditionalInfoCollection).DeleteAll();
			}

			base.Delete();
		}

		protected override void DeleteForDataRefresh()
		{
			for (var i = PackLines.Count - 1; i > 0; i--)
			{
				var packLine = PackLines[i];
				if (!packLine.IsDeleted && !packLine.IsInDatabase && packLine.JL_JS.IsEmpty)
				{
					((IBusiness)packLine).DeleteForDataRefresh();
				}
			}
			PackLines.RemoveAll();

			var cusContainers = (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, PK) { FetchOnlyFromLocalCache = true });
			foreach (var cusContainer in cusContainers.Where(cusContainer => !cusContainer.IsInDatabase))
			{
				cusContainer[CusContainerSchema.CO_JC] = ZGuid.Empty;
			}

			CartageContainerHelper.DeleteAllBookedMoves(forDataRefresh: true);

			using (((IBusinessObjectCollection)Confirms).SuspendListChanged())
			{
				for (var i = Confirms.Count - 1; i >= 0; i--)
				{
					var confirm = Confirms[i];
					if (!confirm.IsDeleted && !confirm.IsInDatabase)
					{
						((IBusiness)confirm).DeleteForDataRefresh();
					}
				}
			}

			base.DeleteForDataRefresh();
		}

		void LogDeleteOnParents()
		{
			if (IsInDatabase)
			{
				foreach (var parent in Parents)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					parent.Logs.AddNew(AutoEvents.EditedARecord, string.Format(CultureInfo.CurrentCulture, "{0} DELETED", JC_ContainerCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		IEnumerable<EnterpriseBusinessObject> Parents
		{
			get { return new[] { Consol, Declaration, Booking }.Cast<EnterpriseBusinessObject>().Where(x => x != null && !x.IsDeleted); }
		}

		public override bool CanDelete => !HasLinkedNonEditableSupplierBooking && !HasLinkedShipmentsWithDeclarations;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (HasLinkedNonEditableSupplierBooking)
				{
					return Validation.CanNotDeleteContainerWhenItHasLoadListLineMessage;
				}
				else if (HasLinkedShipmentsWithDeclarations)
				{
					return ResString.GetMultilingualString(
						"63eaff2b-3a0e-43cc-b754-f76ab8545ea7",
						"This Container may not be deleted because there are attached shipments with declarations linked to this container. You should remove the container from any linked declarations first."
					);
				}

				return base.ReasonForNotAbleToDelete;
			}
		}

		public bool HasLinkedNonEditableSupplierBooking => Validation.HasNonEditableSupplierBooking() || Validation.SupplierBookingChangedFromNonEditableState();

		public bool HasLinkedShipmentsWithDeclarations => Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(new ZQuery(CusContainerSchema.CO_JC, PK)) != null;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ContainerFetchStrategy(this);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			settingDefaultValues = true;
			try
			{
				JC_ContainerMode = "";
				JC_DeliveryMode = "";
				JC_ContainerCount = 1;
				JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
				JC_ContainerCount = 1;
				JC_SetPointTempUnit = "";
				ContainerEventDataVendor.Instance.NotifyContainerCreated(this);
			}
			finally
			{
				settingDefaultValues = false;
			}
		}
		protected bool settingDefaultValues;

		public bool IsDefaultingFromLoadList { get; set; }

		#endregion

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (HasChanges || (Consol?.Transports.HasChanges ?? false))
			{
				LogContainerCountChangeOnParents();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges(Consol?.Transports.HasChanges ?? false));
				ProposeWorkflowRelationships();
			}
		}

		void LogContainerCountChangeOnParents()
		{
			if (!IsDeleted)
			{
				var originalCount = (ZShort)JC_ContainerCountInfo.OriginalValue;
				if (originalCount != JC_ContainerCount)
				{
					foreach (var parent in Parents)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						parent.Logs.AddNew(AutoEvents.EditedARecord, string.Format(CultureInfo.CurrentCulture, "{0} COUNT CHANGED FROM {1} TO {2}", JC_ContainerCode, originalCount, JC_ContainerCount));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
			}
		}

		public override void OnSaving()
		{
			SetContainerID();

			base.OnSaving();

			LogEvents();
			LogErrors();

			CalculateJC_EmptyReturnedBy();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				JC_ContainerJobID = ZString.Empty;
			}
		}

		protected void SetContainerID()
		{
			if (JC_ContainerJobID.IsEmpty)
			{
				JC_ContainerJobID = Env.NumberFountains.JobContainerJobID.GetNextFormatted(Factory);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return JC_ContainerNum.IsEmpty ? Res.GetString("cceab679-3a01-4375-b340-6211a455bc15", "Container") : (Res.GetString("0393527c-d8fe-43fe-b568-17cff1ede28f", "Container '{0}'", JC_ContainerNum)); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			CommonContainer container = (CommonContainer)base.CloneInternal(args);
			container.JC_Calc_NetWeight = JC_Calc_NetWeight;

			return container;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(JobContainerSchema.Constants.JC_JK);
			result.Add(JobContainerSchema.Constants.JC_JS_FCLBookingOnlyLink);
			result.Add(JobContainerSchema.Constants.JC_ContainerJobID);
			result.Add(JobContainerSchema.Constants.JC_JSB_SupplierBooking);
			result.Add(JobContainerSchema.Constants.JC_RCA_AllocationLine);
			result.Add(JobContainerSchema.Constants.JC_CLH_LoadListPlan);

			return result;
		}

		#endregion

		#region Properties

		#region Times Suspension

		#region SuspendSettingContainerDates

		public IDisposable SuspendSettingContainerDates()
		{
			return new SetContainerDatesSuspender(this);
		}

		sealed class SetContainerDatesSuspender : IDisposable
		{
			public SetContainerDatesSuspender(CommonContainer container)
			{
				if (container == null)
				{
					throw new ArgumentNullException(nameof(container));
				}

				this.container = container;
				container.fSetContainerDatesSuspensionLevel++;
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					container.fSetContainerDatesSuspensionLevel--;
				}
			}

			bool disposed;

			#endregion

			readonly CommonContainer container;
		}

		internal int SetContainerDatesSuspensionLevel
		{
			get { return fSetContainerDatesSuspensionLevel; }
		}
		int fSetContainerDatesSuspensionLevel;

		#endregion

		#region SuspendSettingConfirmDates

		public IDisposable SuspendSettingConfirmDates()
		{
			return new SetConfirmDatesSuspender(this);
		}

		sealed class SetConfirmDatesSuspender : IDisposable
		{
			public SetConfirmDatesSuspender(CommonContainer container)
			{
				if (container == null)
				{
					throw new ArgumentNullException(nameof(container));
				}

				this.container = container;
				container.fSetConfirmDatesSuspensionLevel++;
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					container.fSetConfirmDatesSuspensionLevel--;
				}
			}

			bool disposed;

			#endregion

			readonly CommonContainer container;
		}

		internal int SetConfirmDatesSuspensionLevel
		{
			get { return fSetConfirmDatesSuspensionLevel; }
		}
		int fSetConfirmDatesSuspensionLevel;

		#endregion

		#endregion

		#region JC_DepartureCartageComplete

		public override ZDateTime JC_DepartureCartageComplete
		{
			get { return new ZDateTime(base.JC_DepartureCartageComplete, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = JC_DepartureCartageComplete;
				base.JC_DepartureCartageComplete = value;

				if (OriginConfirm != null)
				{
					ConfirmTimesSyncHelper.SetConfirmTimes(this, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual, originalValue, value);
				}
			}
		}

		#endregion

		#region EffectiveGrossWeight

		public void SetGrossWeightFromCombinedWeights()
		{
			if (!StandAloneCustomsContainer)
			{
				SetGrossWeightFromCombinedWeights(GoodsWeight, GoodsWeightUQ);
			}
		}

		public void SetGrossWeightFromCombinedWeights(ZDecimal goodsWeight, ZString goodsWeightUQ)
		{
			if (setGrossWeightFromCombinedWeightsSuspensionLevel == 0 &&
				!IsGrossWeightVerified &&
				!IsGrossWeightOverrideActive)
			{
				JC_GrossWeight = EffectiveGrossWeight(goodsWeight, goodsWeightUQ);
			}
		}

		public IDisposable SuspendSettingGrossWeightFromCombinedWeights()
		{
			return new SetGrossWeightFromCombinedWeightsSuspender(this);
		}

		sealed class SetGrossWeightFromCombinedWeightsSuspender : IDisposable
		{
			public SetGrossWeightFromCombinedWeightsSuspender(CommonContainer container)
			{
				if (container == null)
				{
					throw new ArgumentNullException(nameof(container));
				}

				this.container = container;
				container.setGrossWeightFromCombinedWeightsSuspensionLevel++;
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					container.setGrossWeightFromCombinedWeightsSuspensionLevel--;
				}
			}

			bool disposed;

			#endregion

			readonly CommonContainer container;
		}

		int setGrossWeightFromCombinedWeightsSuspensionLevel;

		public bool IsEffectiveGrossWeightValid
		{
			get { return EffectiveGrossWeight().IsWithinSqlPrecisionAndScale(JobContainerSchema.JC_GrossWeight.Precision, JobContainerSchema.JC_GrossWeight.Scale); }
		}

		internal ZDecimal EffectiveGrossWeightInternal() => EffectiveGrossWeight();

		ZDecimal EffectiveGrossWeight()
		{
			return EffectiveGrossWeight(GoodsWeight, GoodsWeightUQ);
		}

		ZDecimal EffectiveGrossWeight(ZDecimal goodsWeight, ZString goodsWeightUQ)
		{
			ZString validGoodsWeightUnit = FreightUtilities.IsValidWeightUnit(goodsWeightUQ) ? goodsWeightUQ : ContainerWeightUnit;
			ZDecimal goodsWeightInLocalWeightUnit = Core.Constants.Weight.Convert(goodsWeight, validGoodsWeightUnit, ContainerWeightUnit, false);
			return JC_TareWeight + JC_DunnageWeight + goodsWeightInLocalWeightUnit;
		}

		#endregion

		#region JC_ArrivalEstimatedDelivery

		public override ZDateTime JC_ArrivalEstimatedDelivery
		{
			get { return new ZDateTime(base.JC_ArrivalEstimatedDelivery, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = JC_ArrivalEstimatedDelivery;
				base.JC_ArrivalEstimatedDelivery = value;

				if (DestinationConfirm != null)
				{
					ConfirmTimesSyncHelper.SetConfirmTimes(this, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned, originalValue, value);
				}
			}
		}

		#endregion

		#region JC_DepartureEstimatedPickup

		public override ZDateTime JC_DepartureEstimatedPickup
		{
			get { return new ZDateTime(base.JC_DepartureEstimatedPickup, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = JC_DepartureEstimatedPickup;
				base.JC_DepartureEstimatedPickup = value;

				if (DestinationConfirm != null)
				{
					ConfirmTimesSyncHelper.SetConfirmTimes(this, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned, originalValue, value);
				}
			}
		}

		#endregion

		#region JC_OA_ArrivalContainerYardAddress

		[List("JC_OA_ArrivalContainerYardAddress_ZAddress.OrgAddress_List")]
		public override ZGuid JC_OA_ArrivalContainerYardAddress
		{
			get
			{
				ZGuid result = base.JC_OA_ArrivalContainerYardAddress;
				if (result.IsEmpty && Consol != null)
				{
					result = Consol.JK_OA_ContainerYardEmptyReturnAddress;
				}

				return result;
			}
			set { base.JC_OA_ArrivalContainerYardAddress = value; }
		}

		#endregion

		#region JC_OA_DepartureContainerYardAddress

		[List("JC_OA_DepartureContainerYardAddress_ZAddress.OrgAddress_List")]
		public override ZGuid JC_OA_DepartureContainerYardAddress
		{
			get
			{
				ZGuid result = base.JC_OA_DepartureContainerYardAddress;
				if (result.IsEmpty && Consol != null)
				{
					result = Consol.JK_OA_ContainerYardEmptyPickupAddress;
				}

				return result;
			}
			set { base.JC_OA_DepartureContainerYardAddress = value; }
		}

		#endregion

		#region JC_GrossWeight

		[ReadOnlyMember(nameof(IsGrossWeightReadOnly))]
		[DecimalPlaces(3)]
		[MeasureUnit(AutoJobContainer.Schema.JC_GrossWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JC_GrossWeight
		{
			[DebuggerStepThrough]
			get { return base.JC_GrossWeight; }
			set
			{
				var previousGrossWeight = base.JC_GrossWeight;
				var roundedValue = this.GetRoundedValue(JobContainerSchema.JC_GrossWeight, JC_GrossWeightInfo, value);

				CheckNotWithinSqlPrecisionAndScale(JobContainerSchema.JC_GrossWeight, previousGrossWeight, roundedValue);

				base.JC_GrossWeight = roundedValue;
				JC_Calc_NetWeightInfo.RefreshBinding();
			}
		}

		public bool IsGrossWeightReadOnly
		{
			get { return JC_JK.IsValid && !IsGrossWeightOverrideActive && !IsGrossWeightVerified; }
		}

		public bool IsGrossWeightOverriddenReadonly
			=> !IsGrossWeightOverrideAvailable;

		/// <summary>
		/// Determine the availability of the Gross Weight Override flag - only for some consol configurations.
		/// </summary>
		public virtual bool IsGrossWeightOverrideAvailable => false;

		[LightValidationTestExempt]
		public override ZBool JC_IsGrossWeightOverridden { get => base.JC_IsGrossWeightOverridden; set => base.JC_IsGrossWeightOverridden = value; }

		/// <summary>
		/// The overall state of the gross weight override.
		/// True only if both the flag is true and available.
		/// </summary>
		public bool IsGrossWeightOverrideActive
			=> JC_IsGrossWeightOverridden && IsGrossWeightOverrideAvailable;

		/// <summary>
		/// IsGrossWeightOverridden for binding - so the UI can show unticked when JC_IsGrossWeightOverridden is true, but the override is not available.
		/// </summary>
		[ReadOnlyMember(nameof(IsGrossWeightOverriddenReadonly))]
		public virtual ZBool IsGrossWeightOverriddenForBinding
		{
			get => IsGrossWeightOverrideActive;
			set
			{
				if (JC_IsGrossWeightOverridden != value)
				{
					JC_IsGrossWeightOverridden = value;
					IsGrossWeightOverriddenForBindingInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsGrossWeightOverriddenForBindingInfo
			=> GetWrappedZPropertyInfo(nameof(IsGrossWeightOverriddenForBinding), _ => JC_IsGrossWeightOverriddenInfo);

		void CheckNotWithinSqlPrecisionAndScale(SchemaDecimalColumn schemaColumn, ZDecimal previousValue, ZDecimal newValue)
		{
			if (IsValidationSuspended && !newValue.IsWithinSqlPrecisionAndScale(schemaColumn.Precision, schemaColumn.Scale))
			{
				stackTraceForJC_GrossWeightOutOfRange.AppendFormat(CultureInfo.InvariantCulture,
@"Container PK = {0}
JC_ContainerNum = {1}
JC_ContainerCount = {2}
JC_TareWeight = {3}
JC_Calc_TareWeight = {4}
{5} previous value = {6}, new value = {7}
Stack trace is:
{8}
",
					PK,
					JC_ContainerNum,
					JC_ContainerCount,
					JC_TareWeight,
					JC_Calc_TareWeight,
					schemaColumn.Name,
					previousValue,
					newValue,
					System.Environment.StackTrace);
			}
		}

		readonly StringBuilder stackTraceForJC_GrossWeightOutOfRange = new StringBuilder();

		#endregion

		#region GrossWeightForBinding

		[ReadOnlyMember(nameof(IsGrossWeightReadOnly))]
		[DecimalPlaces(3)]
		public ZDecimal GrossWeightForBinding
		{
			get { return JC_GrossWeight; }
			set { JC_GrossWeight = value; }
		}

		public ZPropertyInfo GrossWeightForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(GrossWeightForBinding), x => JC_GrossWeightInfo); }
		}

		#endregion

		#region IsMeasurementsOutOfRange

		public bool IsMeasurementsOutOfRange
		{
			get
			{
				return !JC_GrossWeight.IsWithinSqlPrecisionAndScale(9, 3) || !JC_GrossVolume.IsWithinSqlPrecisionAndScale(9, 3);
			}
		}

		#endregion

		#region JC_Calc_NetWeight

		[DecimalPlaces(3)]
		[ResourceStringData("JobContainer|JC_Calc_NetWeight", ShortCaption = "Net Wt.", Caption = "Net Weight")]
		[MeasureUnit(AutoJobContainer.Schema.JC_GrossWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal JC_Calc_NetWeight
		{
			get
			{
				var result = JC_GrossWeight - JC_TareWeight;
				return this.GetRoundedValue(JC_Calc_NetWeightInfo, result);
			}
			set { JC_GrossWeight = value + JC_TareWeight; }
		}

		public ZPropertyInfo JC_Calc_NetWeightInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_NetWeight); }
		}

		#endregion

		#region ActualWeightUnitText

		public ZString ActualWeightUnitText
		{
			get { return Res.GetString("CommonContainer|ActualWeightUnitText", "Actual ({0})", ContainerWeightUnit.ToLower()); }
		}

		public ZPropertyInfo ActualWeightUnitTextInfo
		{
			get { return GetZPropertyInfo(nameof(ActualWeightUnitText)); }
		}

		#endregion

		#region OnFileWeightUnitText

		public ZString OnFileWeightUnitText
		{
			get { return Res.GetString("CommonContainer|OnFileWeightUnitText", "On File ({0})", ContainerWeightUnit.ToLower()); }
		}

		public ZPropertyInfo OnFileWeightUnitTextInfo
		{
			get { return GetZPropertyInfo(nameof(OnFileWeightUnitText)); }
		}

		#endregion

		#region JC_GrossWeightVerificationType

		[List("Lookups.GrossWeightVerificationTypeList")]
		public override ZString JC_GrossWeightVerificationType
		{
			get { return base.JC_GrossWeightVerificationType; }
			set
			{
				if (value != base.JC_GrossWeightVerificationType)
				{
					SetGrossWeightVerificationLoadPort();
					base.JC_GrossWeightVerificationType = value;

					UpdateVGMStatus();

					if (IsGrossWeightVerified)
					{
						JC_GrossWeightVerificationDateTime = ZDateTime.Now;
					}
					else if (GrossWeightVerificationNotRequired)
					{
						JC_GrossWeightVerificationDateTime = ZDateTime.Now;
						GrossWeightVerifiedByAddress.OrganisationPK = ZGuid.Empty;
					}
					else
					{
						SetGrossWeightFromCombinedWeights();
						JC_GrossWeightVerificationDateTime = ZDateTime.Empty;
						GrossWeightVerifiedByAddress.OrganisationPK = ZGuid.Empty;
					}

					if (Consol != null
						&& GrossWeightVerifiedByAddress.OrganisationPK == ZGuid.Empty)
					{
						if (value == Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container || value == Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages)
						{
							if (Consol.JK_AgentType == Constants.AgentType.Direct && Consol.Shipments.Count == 1)
							{
								GrossWeightVerifiedByAddress.OrganisationPK = Consol.Shipments[0].Consignor?.PK ?? ZGuid.Empty;
							}
							else if (Consol.JK_AgentType != Constants.AgentType.Direct)
							{
								GrossWeightVerifiedByAddress.OrganisationPK = GetDefaultVGMAddress();
							}
						}
						else if (value == Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal && Consol.DepartureCTOAddress != null)
						{
							GrossWeightVerifiedByAddress.OrganisationPK = Consol.DepartureCTOAddress.OA_OH;
						}
					}

					if (!GrossWeightVerifiedByAddress.IsValidationSuspended)
					{
						GrossWeightVerifiedByAddress.Validation.ValidateOrganisationNameOrPK();
					}
				}
			}
		}

		ZGuid GetDefaultVGMAddress()
		{
			var defaultAddress = ZGuid.Empty;
			switch (FreightDataRegistry.Instance.VGMVerifiedByDefaultsTo.Value)
			{
				case (Constants.VGMVerifiedParties.OwnSendingAgent):
					if (Consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK == GlbCompany.CurrentCompany.GC_OH_OrgProxy)
					{
						defaultAddress = Consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK;
					}

					break;

				case (Constants.VGMVerifiedParties.AnySendingAgent):
					defaultAddress = Consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK;
					break;

				case (Constants.VGMVerifiedParties.DepartureCFS):
					defaultAddress = Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK;
					break;
			}
			return defaultAddress;
		}

		#endregion

		#region JC_GrossWeightVerificationStatus

		[ReadOnly(true)]
		[List("Lookups.VGMStatusList")]
		public override ZString JC_GrossWeightVerificationStatus
		{
			get => base.JC_GrossWeightVerificationStatus;
			set
			{
				if (value != base.JC_GrossWeightVerificationStatus)
				{
					base.JC_GrossWeightVerificationStatus = value;
				}
			}
		}

		void UpdateVGMStatus()
		{
			if (JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified)
			{
				JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotVerified;
			}
			else if (JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired
				|| JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal)
			{
				JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired;
			}
			else if ((JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container
					|| JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages)
					&& MostRecentVGMLogByPostedTime(Events.MessageSent) == null)
			{
				JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			}
			else if (JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod)
			{
				JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired;
			}
			else if (MostRecentVGMLogByPostedTime(Events.MessageSent) != null)
			{
				JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent;
			}

			JC_GrossWeightVerificationStatusInfo.RefreshBinding();
		}

		public StmALog MostRecentVGMLogByPostedTime(Event @event)
		{
			return Logs.MostRecentLogByPostedTime(@event, log =>
				log.Parameters.ContainsKey(Params.MessageType)
				&& log.Parameters[Params.MessageType] == Constants.EventReferenceMessageTypes.VerifiedGrossContainerWeight);
		}

		#endregion

		#region JC_EPANStatus

		public ZString JC_EPANStatus
		{
			get
			{
				var containerLogs = Logs?
					.GetAllLogs()
					.OfType<StmALog>()
					.Where(log => log.Parameters.ContainsKey(Params.MessageType) && log.Parameters[Params.MessageType] == EventReferenceMessageTypes.ExportPreAdviceNotification)
					.OrderByDescending(log => log.SL_PostedTimeUtc).ToList();

				if (!containerLogs.IsNullOrEmpty())
				{
					foreach (var log in containerLogs)
					{
						switch (log.SL_SE_NKEvent)
						{
							case Events.MessageSentCode:
								return FreightConstants.NZExportPreAdviceStatus.Codes.Sent;
							case Events.MessageAcceptedCode:
								return FreightConstants.NZExportPreAdviceStatus.Codes.Accepted;
							case Events.InterchangeRejectedCode:
							case Events.MessageRejectedCode:
								return FreightConstants.NZExportPreAdviceStatus.Codes.Rejected;
							case Events.MessageWithdrawCancelRequestCode:
								return FreightConstants.NZExportPreAdviceStatus.Codes.WithdrawalSent;
							case Events.MessageWithdrawCancelAcceptedCode:
								return FreightConstants.NZExportPreAdviceStatus.Codes.Withdrawn;
						}
					}
				}

				return FreightConstants.NZExportPreAdviceStatus.Codes.NotSent;
			}
		}

		#endregion

		#region JC_GrossWeightVerificationLoadPort

		internal ZString JC_GrossWeightVerificationLoadPort
		{
			get
			{
				if (!JC_GrossWeightVerificationLoadPortIsLoaded)
				{
					SetGrossWeightVerificationLoadPort();
					JC_GrossWeightVerificationLoadPortIsLoaded = true;
				}
				return jc_GrossWeightVerificationLoadPort;
			}
			private set
			{
				jc_GrossWeightVerificationLoadPort = value;
				JC_GrossWeightVerificationLoadPortIsLoaded = true;
			}
		}

		ZString jc_GrossWeightVerificationLoadPort;

		ZBool JC_GrossWeightVerificationLoadPortIsLoaded = false;

		void SetGrossWeightVerificationLoadPort()
		{
			JC_GrossWeightVerificationLoadPort = GetCurrentFirstSeaLoadPort();
		}

		internal ZString GetCurrentFirstSeaLoadPort()
		{
			var route = MovementLegComparer.FirstOrDefaultLegForTransportMode(Consol?.Transports.Cast<Transport>(), Constants.TransportModes.Sea);
			var loadPort = route != null && route.TransportMode == Constants.TransportModes.Sea ? route.JW_RL_NKLoadPort : ZString.Empty;

			return loadPort;
		}

		#endregion

		#region GrossWeightVerifiedByFieldType

		public ZString GrossWeightVerifiedByFieldType
		{
			get
			{
				return GrossWeightVerifiedByAddress.E2_AddressOverride ?
					nameof(ZArchitecture.FieldType.Text) :
					nameof(ZArchitecture.FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo GrossWeightVerifiedByFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(GrossWeightVerifiedByFieldType)); }
		}

		#endregion

		#region GrossWeightVerifiedByNameOrPK

		[List("Lookups.GrossWeightVerifiedByList")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member HasEmptyContainerNumber")]
		[ReadOnlyMember("HasEmptyContainerNumber")]
		[RelatedBusinessObject("GrossWeightVerifiedBy")]
		public virtual ZString GrossWeightVerifiedByNameOrPK
		{
			get { return GrossWeightVerifiedByAddress.OrganisationNameOrPK; }
			set { GrossWeightVerifiedByAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo GrossWeightVerifiedByNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.GrossWeightVerifiedByNameOrPK, x => GrossWeightVerifiedByAddress.OrganisationNameOrPKInfo); }
		}

		public OrgHeader GrossWeightVerifiedBy
		{
			get { return (GrossWeightVerifiedByAddress == null || !GrossWeightVerifiedByAddress.HasRealOrganisation) ? null : GrossWeightVerifiedByAddress.Organisation; }
		}

		#endregion

		#region JC_LastFreeDay

		public virtual ZDateTime JC_LastFreeDay
		{
			get
			{
				var lastFreeDay = ZDateTime.Empty;
				if (JC_ArrivalCTOStorageStartDate.IsValid)
				{
					lastFreeDay = JC_ArrivalCTOStorageStartDate;
				}
				else if (JC_LCLStorageCommences.IsValid)
				{
					lastFreeDay = JC_LCLStorageCommences;
				}
				if (!lastFreeDay.IsEmpty)
				{
					return lastFreeDay.Date.AddDays(-1);
				}
				return lastFreeDay;
			}
		}

		public ZPropertyInfo JC_LastFreeDayInfo
		{
			get { return GetZPropertyInfo(Schema.JC_LastFreeDay); }
		}

		#endregion

		#region JC_SetPointTemp

		public bool JC_SetPointTemp_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		[DecimalPlaces(1)]
		[MeasureUnit(Schema.JC_SetPointTempUnit, MeasureUnitType.Temperature)]
		public override ZDecimal JC_SetPointTemp
		{
			[DebuggerStepThrough]
			get { return base.JC_SetPointTemp; }
			set
			{
				if (JC_SetPointTemp == value)
				{
					base.JC_SetPointTemp = value;
				}
				else
				{
					base.JC_SetPointTemp = value;

					if (value != 0m)
					{
						JC_IsControlledAtmosphere = true;

						if (JC_SetPointTempUnit.IsEmpty)
						{
							JC_SetPointTempUnit = Constants.Temperature.Centigrade;
						}
					}
				}
			}
		}

		#endregion

		#region JC_SetPointTempUnit

		public bool JC_SetPointTempUnit_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		[List("BindToLists.TemperatureUnits")]
		public override ZString JC_SetPointTempUnit
		{
			[DebuggerStepThrough]
			get { return base.JC_SetPointTempUnit; }
			[DebuggerStepThrough]
			set { base.JC_SetPointTempUnit = value; }
		}

		#endregion

		#region JC_IsControlledAtmosphere

		public bool JC_IsControlledAtmosphere_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		public override ZBool JC_IsControlledAtmosphere
		{
			[DebuggerStepThrough]
			get { return base.JC_IsControlledAtmosphere; }
			set
			{
				base.JC_IsControlledAtmosphere = value;
				if (!value)
				{
					JC_SetPointTemp = 0m;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJC_SetPointTemp();
				}
			}
		}

		#endregion

		#region JC_RC

		[List("RefContainer_List")]
		public override ZGuid JC_RC
		{
			[DebuggerStepThrough]
			get { return base.JC_RC; }
			set
			{
				if (base.JC_RC == value)
				{
					base.JC_RC = value;
					SetContainerValues();
					UpdateIsNonOperativeReefer();
				}
				else
				{
					if (!base.JC_RC.IsEmpty)
					{
						BehaviorStrategy.RefContainerRemoved(this);
					}

					base.JC_RC = value;
					SetContainerValues();
					UpdateIsNonOperativeReefer();

					if (!JC_RC.IsEmpty)
					{
						BehaviorStrategy.RefContainerAttached(this);
					}

					var verificationTypes = ParentShipmentsCached.Select(CalculateVerificationType);
					if (verificationTypes.Any(type => type == Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified))
					{
						JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
					}
					else if (verificationTypes.Any(type => type == Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired))
					{
						JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;
					}

					JC_IsNonOperativeReeferInfo.RefreshBinding();
					JC_Calc_LengthInfo.RefreshBinding();
					JC_Calc_WidthInfo.RefreshBinding();
					JC_Calc_OverhangLengthInfo.RefreshBinding();
					JC_Calc_OverhangFrontInfo.RefreshBinding();
					JC_Calc_OverhangWidthInfo.RefreshBinding();
					JC_Calc_OverhangLeftInfo.RefreshBinding();
				}
			}
		}

		void UpdateIsNonOperativeReefer()
		{
			JC_IsNonOperativeReefer = false;
			if (RefContainer?.RC_ISOType.SubstringSafe(2, 1).ToString() == "R")
			{
				if (RefContainer.RC_ContainerType == Constants.ContainerTypes.DryStorage)
				{
					JC_IsNonOperativeReefer = true;
				}
			}
		}

		internal string CalculateVerificationType(CommonShipment shipment)
		{
			if (shipment.Consignor != null
				&& shipment.IsDirectShipment
				&& (shipment.ConsignorPickupAddress?.Address?.OA_VerifiesContainerGrossWeight ?? false))
			{
				return (JC_GrossWeight.IsEmpty || JC_GrossWeight == 0)
					? Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified
					: Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;
			}
			return JC_GrossWeightVerificationType;
		}

		void SetContainerValues()
		{
			DefaultContainerYardsFromContainer();

			Validation.ValidateJC_HumidityPercent();

			RefreshBinding();
		}

		protected virtual bool JC_RC_ReadOnly
		{
			get { return IsAir; }
		}

		#endregion

		#region JC_ContainerCount

		public override ZShort JC_ContainerCount
		{
			get { return base.JC_ContainerCount; }
			set
			{
				base.JC_ContainerCount = value;

				if (!isWeightDefaultingSuppressed)
				{
					BehaviorStrategy.ContainerCountChanged(this);
				}

				JC_Calc_MaxGrossWeightInfo.RefreshBinding();

				if (!IsSettingWeightsSuspendedFromUniversalShipmentReader)
				{
					JC_Calc_TareWeightInfo.RefreshBinding();
				}
			}
		}

		public IDisposable SuppressDefaultingOfWeights()
		{
			isWeightDefaultingSuppressed = true;
			return new DisposableAction(() => isWeightDefaultingSuppressed = false);
		}

		bool isWeightDefaultingSuppressed;

		public IDisposable SuppressSettingRelatedWeightsFromUniversalShipment()
		{
			IsSettingWeightsSuspendedFromUniversalShipmentReader = true;
			return new DisposableAction(() => IsSettingWeightsSuspendedFromUniversalShipmentReader = false);
		}

		public bool IsSettingWeightsSuspendedFromUniversalShipmentReader;

		#endregion

		#region JC_AirVentFlow

		public bool JC_AirVentFlow_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		[DecimalPlaces(1)]
		// MeasureUnit does not support volume per unit of time
		public override ZDecimal JC_AirVentFlow
		{
			get { return base.JC_AirVentFlow; }
			set
			{
				base.JC_AirVentFlow = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJC_AirVentFlowRateUnit();
				}
			}
		}

		#endregion

		#region JC_AirVentFlowRateUnit

		public bool JC_AirVentFlowRateUnit_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		[List("BindToLists.AirVentFlowRateUnits")]
		public override ZString JC_AirVentFlowRateUnit
		{
			[DebuggerStepThrough]
			get { return base.JC_AirVentFlowRateUnit; }
			[DebuggerStepThrough]
			set { base.JC_AirVentFlowRateUnit = value; }
		}

		#endregion

		#region JC_DeliveryMode

		[List("JC_DeliveryMode_List")]
		public override ZString JC_DeliveryMode
		{
			get { return base.JC_DeliveryMode; }
			set { base.JC_DeliveryMode = value; }
		}

		public ICodeDescriptionPairList JC_DeliveryMode_List
		{
			get { return GetDeliveryMode_ListCore(); }
		}

		protected virtual ICodeDescriptionPairList GetDeliveryMode_ListCore()
		{
			return Transport != Constants.TransportModes.Air
				? FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.ToCodeDescription()
				: new CodeDescriptionPairList();
		}

		public virtual ZString UserDefinedDeliveryMode
		{
			get
			{
				return FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.ConvertToUserDefinedCode(JC_DeliveryMode);
			}
		}

		public ICodeDescriptionPairList UserDefinedDeliveryMode_List
		{
			get
			{
				return GetUserDefinedDeliveryMode_List();
			}
		}

		protected virtual ICodeDescriptionPairList GetUserDefinedDeliveryMode_List()
		{
			return Transport != Constants.TransportModes.Air ? FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.ToUserDefinedCodeDescription() : new CodeDescriptionPairList();
		}
		#endregion

		#region JC_ContainerStatus

		[List("Lookups.ContainerStatuses")]
		public override ZString JC_ContainerStatus
		{
			get { return base.JC_ContainerStatus; }
			set { base.JC_ContainerStatus = value; }
		}

		#endregion

		#region JC_ContainerQuality

		[List("Lookups.ContainerQualities")]
		public override ZString JC_ContainerQuality
		{
			get { return base.JC_ContainerQuality; }
			set { base.JC_ContainerQuality = value; }
		}

		#endregion

		#region JC_GrossWeightUQ

		[List("BindToLists.WeightUnits")]
		public override ZString JC_GrossWeightUQ
		{
			[DebuggerStepThrough]
			get { return base.JC_GrossWeightUQ; }
			[DebuggerStepThrough]
			set
			{
				var oldValue = base.JC_GrossWeightUQ;
				base.JC_GrossWeightUQ = value;

				if (IsSettingWeightsSuspendedFromUniversalShipmentReader && oldValue != value && Core.Constants.Weight.ContainsCode(oldValue) && Core.Constants.Weight.ContainsCode(value))
				{
					JC_DunnageWeight = Core.Constants.Weight.Convert(JC_DunnageWeight, oldValue, value);
					JC_TareWeight = Core.Constants.Weight.Convert(JC_TareWeight, oldValue, value);

					if (!IsGrossWeightOverrideActive)
					{
						JC_GrossWeight = Core.Constants.Weight.Convert(JC_GrossWeight, oldValue, value);
					}
				}

				this.SetRoundedValue(JobContainerSchema.JC_TareWeight, JC_TareWeightInfo);
				this.SetRoundedValue(JobContainerSchema.JC_DunnageWeight, JC_DunnageWeightInfo);
				this.SetRoundedValue(JobContainerSchema.JC_GrossWeight, JC_GrossWeightInfo);
			}
		}

		#endregion

		#region JC_TareWeight

		[MeasureUnit(AutoJobContainer.Schema.JC_GrossWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JC_TareWeight
		{
			[DebuggerStepThrough]
			get { return base.JC_TareWeight; }
			set
			{
				var adjustment = value - JC_TareWeight;
				base.JC_TareWeight = this.GetRoundedValue(JobContainerSchema.JC_TareWeight, JC_TareWeightInfo, value);

				if (!IsGrossWeightVerified && !IsSettingWeightsSuspendedFromUniversalShipmentReader)
				{
					JC_GrossWeight += adjustment;
				}
			}
		}

		#endregion

		#region JC_DunnageWeight

		public override ZDecimal JC_DunnageWeight
		{
			[DebuggerStepThrough]
			get { return base.JC_DunnageWeight; }
			set
			{
				var adjustment = value - JC_DunnageWeight;
				base.JC_DunnageWeight = this.GetRoundedValue(JobContainerSchema.JC_DunnageWeight, JC_DunnageWeightInfo, value);

				if (!IsGrossWeightVerified && !IsSettingWeightsSuspendedFromUniversalShipmentReader)
				{
					JC_GrossWeight += adjustment;
				}
			}
		}

		#endregion

		#region JC_EmptyReturnedBy

		public override ZDateTime JC_EmptyReturnedBy
		{
			get { return new ZDateTime(base.JC_EmptyReturnedBy, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = base.JC_EmptyReturnedBy;
				base.JC_EmptyReturnedBy = value;

				if (originalValue != value)
				{
					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
				}

				BehaviorStrategy.EmptyReturnedByChanged(this, originalValue);
			}
		}

		public void ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType dateType)
		{
			ContainerPenaltyCalculateHandlers?.ForEach(handler => handler.HandleContainerDateChanging(dateType));
		}

		public virtual IContainerPenaltyCalculateHandler[] ContainerPenaltyCalculateHandlers => Array.Empty<IContainerPenaltyCalculateHandler>();

		#endregion

		#region JC_EmptyRequired

		public override ZDateTime JC_EmptyRequired
		{
			get { return new ZDateTime(base.JC_EmptyRequired, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = base.JC_EmptyRequired;
				base.JC_EmptyRequired = value;
				BehaviorStrategy.EmptyRequiredChanged(this, originalValue);
			}
		}

		#endregion

		#region JC_ArrivalSlotDateTime

		public override ZDateTime JC_ArrivalSlotDateTime
		{
			get { return new ZDateTime(base.JC_ArrivalSlotDateTime, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = base.JC_ArrivalSlotDateTime;
				base.JC_ArrivalSlotDateTime = value;
				BehaviorStrategy.ArrivalSlotDateOrReferenceChanged(this, originalValue);
			}
		}

		#endregion

		#region JC_ArrivalSlotReference

		public override ZString JC_ArrivalSlotReference
		{
			get { return base.JC_ArrivalSlotReference; }
			set
			{
				base.JC_ArrivalSlotReference = value;
				BehaviorStrategy.ArrivalSlotDateOrReferenceChanged(this, JC_ArrivalSlotDateTime);
			}
		}

		#endregion

		#region JC_DepartureSlotDateTime

		public override ZDateTime JC_DepartureSlotDateTime
		{
			get { return new ZDateTime(base.JC_DepartureSlotDateTime, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime originalValue = base.JC_DepartureSlotDateTime;
				base.JC_DepartureSlotDateTime = value;
				BehaviorStrategy.DepartureSlotDateOrReferenceChanged(this, originalValue);
			}
		}

		#endregion

		#region JC_DepartureSlotReference

		public override ZString JC_DepartureSlotReference
		{
			get { return base.JC_DepartureSlotReference; }
			set
			{
				base.JC_DepartureSlotReference = value;
				BehaviorStrategy.DepartureSlotDateOrReferenceChanged(this, JC_DepartureSlotDateTime);
			}
		}

		#endregion

		#region JC_JSB_SupplierBooking

		[List("Lookups.SupplierBookingList")]
		public override ZGuid JC_JSB_SupplierBooking { get => base.JC_JSB_SupplierBooking; set => base.JC_JSB_SupplierBooking = value; }

		#endregion

		#region JC_TotalLength

		[DecimalPlaces(3)]
		public override ZDecimal JC_TotalLength
		{
			get { return base.JC_TotalLength; }
			set
			{
				base.JC_TotalLength = value;
				JC_Calc_OverhangLengthInfo.RefreshBinding();
				JC_Calc_OverhangFrontInfo.RefreshBinding();
			}
		}

		#endregion

		#region JC_TotalWidth

		[DecimalPlaces(3)]
		public override ZDecimal JC_TotalWidth
		{
			get { return base.JC_TotalWidth; }
			set
			{
				base.JC_TotalWidth = value;
				JC_Calc_OverhangWidthInfo.RefreshBinding();
				JC_Calc_OverhangLeftInfo.RefreshBinding();
			}
		}

		#endregion

		#region DepartureTruckWaitCost

		public ContainerPenalty FindDepartureTruckWaitPenalty()
			=> ExportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport);

		[DecimalPlaces(2)]
		public ZDecimal DepartureTruckWaitCost
		{
			get
			{
				var penalty = ExportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport);
				return penalty?.CPY_PerUnitCost ?? 0m;
			}
			set
			{
				var penalty = ExportPenalties.FindOrCreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport,
					timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
					creditor: ContainerParent?.DeparturePackCFSTransportAddress);
				penalty.CPY_PerUnitCost = value;

				DepartureTruckWaitCostInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DepartureTruckWaitCostInfo => GetZPropertyInfo(nameof(DepartureTruckWaitCost));
		#endregion

		#region IsFreezer

		public bool IsFreezer_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		[ResourceStringData("JobContainer|IsFreezer", Caption = "Frozen")]
		public ZBool IsFreezer
		{
			get { return JC_IsControlledAtmosphere && JC_SetPointTemp <= 0m; }
			set
			{
				JC_IsControlledAtmosphere = value;

				if (value && JC_SetPointTemp >= 0m)
				{
					JC_SetPointTemp = -5m;
				}

				IsFreezerInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsFreezer();
				}
			}
		}

		public ZPropertyInfo IsFreezerInfo
		{
			get { return GetZPropertyInfo(Schema.IsFreezer); }
		}

		#endregion

		#region IsChiller

		public bool IsChiller_ReadOnly
		{
			get => JC_IsNonOperativeReefer;
		}

		[ResourceStringData("JobContainer|IsChiller", Caption = "Chiller")]
		public ZBool IsChiller
		{
			get { return JC_IsControlledAtmosphere && JC_SetPointTemp > 0m; }
			set
			{
				JC_IsControlledAtmosphere = value;

				if (value && JC_SetPointTemp <= 0m)
				{
					JC_SetPointTemp = 5m;
				}

				IsChillerInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsChiller();
				}
			}
		}

		public ZPropertyInfo IsChillerInfo
		{
			get { return GetZPropertyInfo(Schema.IsChiller); }
		}

		#endregion

		#region ContainerCode

		public ZString ContainerCode
		{
			get
			{
				ZString result;

				result = JC_ContainerNum;
				if (result.IsEmpty && Container != null)
				{
					result = Container.RC_Code + " (" + JC_ContainerCount + ")";
				}

				return result;
			}
		}

		public ZPropertyInfo ContainerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerCode); }
		}

		#endregion

		#region SupportsContainerPenalties

		public bool SupportsContainerPenalties => Consol != null || Declaration != null;

		#endregion

		public bool IsAirContainer
		{
			get
			{
				return JC_ContainerMode == Core.Constants.ContainerModes.AIR ||
					JC_ContainerMode == Core.Constants.ContainerModes.ULD;
			}
		}

		public bool IsSeaContainer
		{
			get
			{
				return JC_ContainerMode == Core.Constants.ContainerModes.FCL ||
					JC_ContainerMode == Core.Constants.ContainerModes.LCL ||
					JC_ContainerMode == Core.Constants.ContainerModes.Groupage ||
					JC_ContainerMode == Core.Constants.ContainerModes.BuyersConsol;
			}
		}

		public bool IsRoadContainer
		{
			get
			{
				return JC_ContainerMode == Core.Constants.ContainerModes.FTL ||
					JC_ContainerMode == Core.Constants.ContainerModes.LTL;
			}
		}

		public bool IsConsolContainer
		{
			get { return !JC_JK.IsEmpty; }
		}

		public bool IsArrivalContainerModeFCLorULD
		{
			get
			{
				return Constants.ContainerModes.IsFCLType(JC_ContainerMode) || Consol != null && Consol.IsBuyersConsol;
			}
		}

		public bool IsGrossWeightVerified
		{
			get
			{
				return JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container
					|| JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages
					|| JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal
					|| JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod;
			}
		}

		public bool GrossWeightVerificationNotRequired => JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;

		#region IsMandatoryContainerType

		public bool IsMandatoryContainerType
		{
			get
			{
				return
					JC_ContainerMode != Core.Constants.ContainerModes.Bulk
					&& JC_ContainerMode != Core.Constants.ContainerModes.Liquid
					&& JC_ContainerMode != Core.Constants.ContainerModes.BreakBulk
					&& JC_ContainerMode != Core.Constants.ContainerModes.AIR
					&& JC_ContainerMode != Core.Constants.ContainerModes.Loose
					&& JC_ContainerMode != Core.Constants.ContainerModes.RollOnRollOff
					&& JC_ContainerMode != Core.Constants.ContainerModes.NonContainerised;
			}
		}

		#endregion

		#region JC_IsRefrigerated

		public ZBool JC_IsRefrigerated
		{
			get { return RefContainer != null && RefContainer.RC_ContainerType == Core.Constants.ContainerTypes.Refrigerated; }
		}

		#endregion

		#region Is Attached To Container Load List

		public virtual bool IsAttachedToContainerLoadList() => false;

		#endregion

		public bool CreatedFromCusContainer { get; set; }

		public bool StandAloneCustomsContainer { get; set; }

		#region Event Properties

		public override ZDateTime JC_FCLWharfGateOut
		{
			get { return new ZDateTime(base.JC_FCLWharfGateOut, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_FCLWharfGateOut != value)
				{
					base.JC_FCLWharfGateOut = value;

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
				}
			}
		}

		public override ZDateTime JC_FCLWharfGateIn
		{
			get { return new ZDateTime(base.JC_FCLWharfGateIn, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_FCLWharfGateIn != value)
				{
					base.JC_FCLWharfGateIn = value;

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn);
				}
			}
		}

		public override ZDateTime JC_ContainerYardEmptyPickupGateOut
		{
			get { return new ZDateTime(base.JC_ContainerYardEmptyPickupGateOut, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_ContainerYardEmptyPickupGateOut != value)
				{
					base.JC_ContainerYardEmptyPickupGateOut = value;

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
				}
			}
		}

		public override ZDateTime JC_ContainerYardEmptyReturnGateIn
		{
			get { return new ZDateTime(base.JC_ContainerYardEmptyReturnGateIn, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_ContainerYardEmptyReturnGateIn != value)
				{
					base.JC_ContainerYardEmptyReturnGateIn = value;

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
				}
			}
		}

		[EventDateProperty(AutoEvents.FreightLoadedCode, EstimateActual.Actual, shouldOnlyUpdateEmptyDate: true)]
		public override ZDateTime JC_FCLOnBoardVessel
		{
			get { return new ZDateTime(base.JC_FCLOnBoardVessel, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_FCLOnBoardVessel != value)
				{
					base.JC_FCLOnBoardVessel = value;

					// Assuming that value is local to current branch
					LogEvent(AutoEvents.FreightLoaded, value.ToOffset());

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
				}
			}
		}

		[EventDateProperty(AutoEvents.FreightUnloadedCode, EstimateActual.Actual)]
		public override ZDateTime JC_FCLUnloadFromVessel
		{
			get { return new ZDateTime(base.JC_FCLUnloadFromVessel, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_FCLUnloadFromVessel != value)
				{
					base.JC_FCLUnloadFromVessel = value;

					// Assuming that value is local to current branch
					LogEvent(AutoEvents.FreightUnloaded, value.ToOffset());
				}
			}
		}

		[EventDateProperty(AutoEvents.ContainerReadyForEmptyReturnCode, EstimateActual.Actual)]
		public override ZDateTime JC_EmptyReadyForReturn
		{
			get { return new ZDateTime(base.JC_EmptyReadyForReturn, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_EmptyReadyForReturn != value)
				{
					base.JC_EmptyReadyForReturn = value;

					// Assuming that value is local to current branch
					LogEvent(AutoEvents.ContainerReadyForEmptyReturn, value.ToOffset());
				}
			}
		}

		#endregion

		#region Available / Storage

		public ZDateTime AvailableDate
		{
			get
			{
				return IsArrivalContainerModeFCLorULD ? JC_FCLAvailable : JC_LCLAvailable;
			}
		}

		#region JC_FCLAvailable

		public override ZDateTime JC_FCLAvailable
		{
			get
			{
				return JC_FCLAvailableCore;
			}
			set
			{
				if (JC_FCLAvailableCore != value)
				{
					if (!value.IsEmpty)
					{
						JC_OverrideFCLAvailableStorage = true;
					}

					JC_FCLAvailableCore = value;
				}
			}
		}

		protected bool JC_FCLAvailable_ReadOnly
		{
			get { return !JC_OverrideFCLAvailableStorage; }
		}

		public ZDateTime JC_FCLAvailableCore
		{
			get
			{
				var result = ZDateTime.Empty;

				if (!base.JC_FCLAvailable.IsEmpty || JC_OverrideFCLAvailableStorage)
				{
					result = base.JC_FCLAvailable;
				}
				else if (ArrivalTransport != null)
				{
					result = ArrivalTransport.JW_TerminalAvailabilityDate;
				}
				else if (StandaloneSailing?.Destination != null)
				{
					result = StandaloneSailing.Destination.JB_AvailabilityDate;
				}

				return new ZDateTime(result, DateTimeKind.Unspecified);
			}
			set
			{
				if (JC_FCLAvailableCore != value)
				{
					base.JC_FCLAvailable = value;

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable);

					UpdateFCLAvailableDateOnDocsAndCartage(value);
				}
			}
		}

		void UpdateFCLAvailableDateOnDocsAndCartage(ZDateTime fCLAvailableDate)
		{
			foreach (JobDocsAndCartage cartage in DocsAndCartage)
			{
				if (cartage.IsSkipUpdateAvailableDateFromContainer(Consol))
				{
					continue;
				}

				cartage.UpdateFCLAvailableDateFromContainer(fCLAvailableDate);
			}
		}

		public ZDateTime JC_OverriddenFCLAvailable
		{
			get { return base.JC_FCLAvailable; }
			set { base.JC_FCLAvailable = value; }
		}

		public bool JC_OverriddenFCLAvailableHasChanges
		{
			get
			{
				var originalValue = (ZDateTime)base.JC_FCLAvailableInfo.OriginalValue;
				return !JC_OverriddenFCLAvailable.Equals(originalValue);
			}
		}

		#endregion

		#region JC_ArrivalCTOStorageStartDate

		public override ZDateTime JC_ArrivalCTOStorageStartDate
		{
			get
			{
				return JC_ArrivalCTOStorageStartDateCore;
			}
			set
			{
				if (JC_ArrivalCTOStorageStartDate != value)
				{
					if (!value.IsEmpty)
					{
						JC_OverrideFCLAvailableStorage = true;
					}

					JC_ArrivalCTOStorageStartDateCore = value;
				}
			}
		}

		protected bool JC_ArrivalCTOStorageStartDate_ReadOnly
		{
			get { return !JC_OverrideFCLAvailableStorage; }
		}

		public ZDateTime JC_ArrivalCTOStorageStartDateCore
		{
			get => GetArrivalCTOStorageStartDate();
			set
			{
				if (JC_ArrivalCTOStorageStartDate != value)
				{
					base.JC_ArrivalCTOStorageStartDate = value;

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.ArrivalCTOStorageStartDate);

					UpdateFCLStorageDateOnDocsAndCartage(value);
				}
			}
		}

		public ZDateTime GetArrivalCTOStorageStartDate(bool shouldIgnoreField = false)
		{
			if (!shouldIgnoreField && (!base.JC_ArrivalCTOStorageStartDate.IsEmpty || JC_OverrideFCLAvailableStorage))
			{
				return new ZDateTime(base.JC_ArrivalCTOStorageStartDate, DateTimeKind.Unspecified);
			}
			else if (ArrivalTransport != null)
			{
				return new ZDateTime(ArrivalTransport.JW_TerminalStorageDate, DateTimeKind.Unspecified);
			}
			else if (StandaloneSailing?.Destination != null)
			{
				return new ZDateTime(StandaloneSailing.Destination.JB_StorageDate, DateTimeKind.Unspecified);
			}

			return new ZDateTime(ZDateTime.Empty, DateTimeKind.Unspecified);
		}

		public bool SuspendedContainerPenalties { get; protected set; }

		public IDisposable SuspendContainerPenalties()
		{
			if (SuspendedContainerPenalties)
			{
				return new DisposableAction(() => { });
			}

			return new DisposableAction(() => SuspendedContainerPenalties = true, () => SuspendedContainerPenalties = false);
		}

		void UpdateFCLStorageDateOnDocsAndCartage(ZDateTime fclStorageDate)
		{
			foreach (JobDocsAndCartage cartage in DocsAndCartage)
			{
				cartage.UpdateFCLStorageDateFromContainer(fclStorageDate);
			}
		}

		public ZDateTime JC_OverriddenFCLStorage
		{
			get { return base.JC_ArrivalCTOStorageStartDate; }
			set { JC_ArrivalCTOStorageStartDate = value; }
		}

		public bool CalculateJC_ArrivalCTOStorageStartDate()
		{
			var strategy = NewContainerDefaultingStrategy();
			if (strategy != null)
			{
				var (storageStart, _, _) = strategy.CalculateStorageStart(Constants.ContainerDetentionDirection.Import);

				if (storageStart.IsValid)
				{
					JC_ArrivalCTOStorageStartDateCore = storageStart;
					Validation.ValidateJC_ArrivalCTOStorageStartDate();
					return true;
				}
			}

			return false;
		}

		#endregion

		#region JC_LCLAvailable

		public override ZDateTime JC_LCLAvailable
		{
			get
			{
				var result = ZDateTime.Empty;

				if (JC_OverrideLCLAvailableStorage)
				{
					result = base.JC_LCLAvailable;
				}
				else if (ArrivalTransport != null)
				{
					result = ArrivalTransport.JW_DepotAvailabilityDate;
				}
				else if (StandaloneSailing?.Destination != null)
				{
					result = StandaloneSailing.JX_DepotAvailabilityDate;
				}

				return new ZDateTime(result, DateTimeKind.Unspecified);
			}
			set
			{
				if (JC_LCLAvailable != value)
				{
					if (!value.IsEmpty)
					{
						JC_OverrideLCLAvailableStorage = true;
					}
					base.JC_LCLAvailable = value;
					UpdateLCLAvailableDateOnDocsAndCartage(value);
				}
			}
		}

		void UpdateLCLAvailableDateOnDocsAndCartage(ZDateTime lCLAvailableDate)
		{
			foreach (JobDocsAndCartage cartage in DocsAndCartage)
			{
				if (cartage.IsSkipUpdateAvailableDateFromContainer(Consol))
				{
					continue;
				}

				cartage.UpdateLCLAvailableDateFromContainer(lCLAvailableDate);
			}
		}

		protected bool JC_LCLAvailable_ReadOnly
		{
			get { return !JC_OverrideLCLAvailableStorage; }
		}

		public ZDateTime JC_OverriddenLCLAvailable
		{
			get { return base.JC_LCLAvailable; }
		}

		#endregion

		#region JC_LCLStorageCommences

		public override ZDateTime JC_LCLStorageCommences
		{
			get
			{
				var result = ZDateTime.Empty;

				if (JC_OverrideLCLAvailableStorage)
				{
					result = base.JC_LCLStorageCommences;
				}
				else if (ArrivalTransport != null)
				{
					result = ArrivalTransport.JW_DepotStorageDate;
				}
				else if (StandaloneSailing?.Destination != null)
				{
					result = StandaloneSailing.JX_DepotStorageDate;
				}
				return new ZDateTime(result, DateTimeKind.Unspecified);
			}
			set
			{
				if (JC_LCLStorageCommences != value)
				{
					if (!value.IsEmpty)
					{
						JC_OverrideLCLAvailableStorage = true;
					}
					base.JC_LCLStorageCommences = value;
					UpdateLCLStorageDateOnDocsAndCartage(value);
				}
			}
		}

		void UpdateLCLStorageDateOnDocsAndCartage(ZDateTime lCLStorageDate)
		{
			foreach (JobDocsAndCartage cartage in DocsAndCartage)
			{
				cartage.UpdateLCLStorageDateFromContainer(lCLStorageDate);
			}
		}

		protected bool JC_LCLStorageCommences_ReadOnly
		{
			get { return !JC_OverrideLCLAvailableStorage; }
		}

		public ZDateTime JC_OverriddenLCLStorage
		{
			get { return base.JC_LCLStorageCommences; }
		}

		#endregion

		#region JC_OverrideFCLAvailableStorage

		public override ZBool JC_OverrideFCLAvailableStorage
		{
			get { return base.JC_OverrideFCLAvailableStorage; }
			set
			{
				if (base.JC_OverrideFCLAvailableStorage != value)
				{
					if (value)
					{
						var newFCLAvailable = JC_FCLAvailable;
						var newFCLStorage = JC_ArrivalCTOStorageStartDate;
						base.JC_OverrideFCLAvailableStorage = true;
						base.JC_FCLAvailable = newFCLAvailable;
						base.JC_ArrivalCTOStorageStartDate = newFCLStorage;
					}
					else
					{
						base.JC_FCLAvailable = ZDateTime.Empty;
						base.JC_ArrivalCTOStorageStartDate = ZDateTime.Empty;
						base.JC_OverrideFCLAvailableStorage = false;
						ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.OverrideFCLAvailableStorageSetToFalse);
					}

					UpdateFCLAvailableDateOnDocsAndCartage(JC_FCLAvailable);
					UpdateFCLStorageDateOnDocsAndCartage(JC_ArrivalCTOStorageStartDate);
				}
			}
		}

		#endregion

		#region JC_OverrideLCLAvailableStorage

		public override ZBool JC_OverrideLCLAvailableStorage
		{
			get { return base.JC_OverrideLCLAvailableStorage; }
			set
			{
				if (base.JC_OverrideLCLAvailableStorage != value)
				{
					ZDateTime newLCLAvailable = value ? JC_LCLAvailable : ZDateTime.Empty;
					ZDateTime newLCLStorage = value ? JC_LCLStorageCommences : ZDateTime.Empty;
					if (value)
					{
						base.JC_OverrideLCLAvailableStorage = true;
					}

					base.JC_LCLAvailable = newLCLAvailable;
					base.JC_LCLStorageCommences = newLCLStorage;
					if (!value)
					{
						base.JC_OverrideLCLAvailableStorage = false;
					}

					UpdateLCLAvailableDateOnDocsAndCartage(newLCLAvailable);
					UpdateLCLStorageDateOnDocsAndCartage(newLCLStorage);
				}
			}
		}

		#endregion

		#endregion

		#region Import Release

		public static ResourceStringData ImportReleaseOrderStatusStringData
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
				{
					return Res.GetData("fd08060d-08cf-49a0-bd96-db3b0a73f554", "E-IDO", "E-IDO Status", "The current E-IDO messaging status.");
				}

				return Res.GetData("9b5d6b2e-ab4a-11e4-bea8-902b34dc814a", "Import Release Order Status", "Import Release Order Status", "The current Import Release Order messaging status.");
			}
		}

		public static ResourceStringData ImportReleaseNumberStringData
		{
			get
			{
				if (!DesignModeFinder.IsDesigning && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
				{
					return Res.GetData("42fea63e-7a02-4b32-997b-9073a2ba5813", "Carrier/DO Release");
				}

				return Res.GetData("8af1ca78-19f1-4c7e-948b-6fe43864d37c", "Imp. Release #", "Imp. Release Num.", "Import Release Number", "Specifies the Import Release Number for this container.");
			}
		}

		#endregion

		public override ZGuid JC_JK
		{
			get { return base.JC_JK; }
			set
			{
				var oldValue = JC_JK;
				if (value.IsEmpty)
				{
					PackLines.RemoveAll();
				}
				base.JC_JK = value;
				if (!IsCopying)
				{
					var newValue = JC_JK;
					if (oldValue != newValue)
					{
						AddLoggingDetail((NoResString)$"JC_JK changed from '{oldValue}' to '{newValue}'");
					}
				}
			}
		}

		public override ZGuid JC_JS_FCLBookingOnlyLink
		{
			get { return base.JC_JS_FCLBookingOnlyLink; }
			set
			{
				if (value.IsEmpty)
				{
					PackLines.RemoveAll();
				}
				base.JC_JS_FCLBookingOnlyLink = value;
			}
		}

		public virtual bool RequireCalculateJC_EmptyReturnedBy => false;

		public void CalculateJC_EmptyReturnedBy()
		{
			if (((Env.CurrentUser.IsBatchProcessor || !Globals.IsUserInteractive) && !RequireCalculateJC_EmptyReturnedBy) ||
				!JC_EmptyReturnedBy.IsEmpty ||
				(Factory.IsInTransaction && Factory.TransactionId == emptyReturnedByCalculationTransactionID))
			{
				return;
			}

			IContainerDefaultingStrategy strategy = NewContainerDefaultingStrategy();
			var (requiredBy, _, _) = strategy == null ? (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty) : strategy.CalculateRequiredBy();

			if (!requiredBy.IsEmpty && hasValidImportDetentionDate)
			{
				JC_EmptyReturnedBy = requiredBy;
			}

			if (Factory.IsInTransaction)
			{
				emptyReturnedByCalculationTransactionID = Factory.TransactionId;
			}
		}

		bool hasValidImportDetentionDate => JC_FCLAvailable.IsValid
			|| JC_FCLWharfGateOut.IsValid
			|| JC_FCLUnloadFromVessel.IsValid
			|| JC_FCLWharfGateOut.IsValid
			|| (Consol?.JK_ArrivalForLastImportTransport ?? ZDateTime.Empty).IsValid;

		long emptyReturnedByCalculationTransactionID;

		protected ZString Transport
		{
			get
			{
				ZString result = Core.Constants.TransportModes.Sea;
				if (Sailing != null && Sailing.Voyage != null)
				{
					result = Sailing.Voyage.JV_AirSeaRoad;
				}
				else if (Booking != null)
				{
					result = Booking.JS_TransportMode;
				}
				else if (Consol != null)
				{
					result = Consol.JK_TransportMode;
				}
				return result;
			}
		}

		protected virtual bool JC_TrainWagonNumber_ReadOnly
		{
			get { return (Consol != null && Consol.JK_TransportMode != Core.Constants.TransportModes.Rail); }
		}

		public ZBool IsContainerised
		{
			get
			{
				return JC_ContainerMode.IsEmpty ||
					!(JC_ContainerMode == Core.Constants.ContainerModes.BreakBulk ||
					JC_ContainerMode == Core.Constants.ContainerModes.Bulk ||
					JC_ContainerMode == Core.Constants.ContainerModes.RollOnRollOff ||
					JC_ContainerMode == Core.Constants.ContainerModes.Liquid ||
					JC_ContainerMode == Core.Constants.ContainerModes.NonContainerised);
			}
		}

		/// <summary>
		/// The commodity code to use for Autorating purposes.
		/// It decides which one out of RatingCommodityCode or CommodityCode
		/// should apply.
		/// </summary>
		public ZString CommodityCodeForRating =>
			JC_RH_NKRatingCommodityCode.IsEmpty
			? JC_RH_NKContainerCommodityCode
			: JC_RH_NKRatingCommodityCode;

		#region Property Overrides

		[List("ContainerCommodityCode_List")]
		public override ZString JC_RH_NKContainerCommodityCode
		{
			get { return base.JC_RH_NKContainerCommodityCode; }
			set { base.JC_RH_NKContainerCommodityCode = value; }
		}

		[List("JC_ContainerMode_List")]
		public override ZString JC_ContainerMode
		{
			get { return base.JC_ContainerMode; }
			set
			{
				base.JC_ContainerMode = value;
				ResetContainerCollection();

				if (IsAir)
				{
					JC_RC = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJC_RC();
					Validation.ValidateJC_ContainerNum();
				}
			}
		}

		[List("JC_VehicleTransmission_List")]
		public override ZString JC_VehicleTransmission
		{
			get { return base.JC_VehicleTransmission; }
			set { base.JC_VehicleTransmission = value; }
		}

		[List("RefCurrency_List")]
		public override ZString JC_RX_NKGoodsCurrency
		{
			get { return base.JC_RX_NKGoodsCurrency; }
			set { base.JC_RX_NKGoodsCurrency = value; }
		}

		[List(nameof(AllocationLineCollection))]
		public override ZGuid JC_RCA_AllocationLine
		{
			get => base.JC_RCA_AllocationLine;
			set => base.JC_RCA_AllocationLine = value;
		}

		public override ZBool JC_IsEmptyContainer
		{
			get { return base.JC_IsEmptyContainer; }
			set
			{
				base.JC_IsEmptyContainer = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJC_GrossWeightVerificationType();
				}
			}
		}

		void DefaultContainerYardsFromContainer()
		{
			IContainerDefaultingStrategy strat = NewContainerDefaultingStrategy();

			if (strat != null)
			{
				OrgAddress releaseContainerYard = strat.CalculateReleaseContainerYard();
				OrgAddress returnContainerYard = strat.CalculateReturnContanierYard();

				if (releaseContainerYard != null)
				{
					JC_OA_DepartureContainerYardAddress = releaseContainerYard.PK;
				}

				if (returnContainerYard != null)
				{
					JC_OA_ArrivalContainerYardAddress = returnContainerYard.PK;
				}
			}
		}

		public IContainerDefaultingStrategy NewContainerDefaultingStrategy()
		{
			return NewContainerDefaultingStrategyCore();
		}

		protected virtual IContainerDefaultingStrategy NewContainerDefaultingStrategyCore()
		{
			return null;
		}

		#region PackLineType

		public Type PackLineType
		{
			get { return PackLines.TypeOfElements; }
		}

		#endregion

		#region SealParty

		[List("Lookups.SealParty_List")]
		public override ZString JC_SealParty
		{
			get { return base.JC_SealParty; }
			set
			{
				base.JC_SealParty = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJC_SealNum();
				}
			}
		}

		[List("Lookups.SealParty_List")]
		public override ZString JC_Additional2SealParty
		{
			get { return base.JC_Additional2SealParty; }
			set
			{
				base.JC_Additional2SealParty = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJC_Additional2SealNum();
				}
			}
		}

		[List("Lookups.SealParty_List")]
		public override ZString JC_AdditionalSealParty
		{
			get { return base.JC_AdditionalSealParty; }
			set
			{
				base.JC_AdditionalSealParty = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJC_AdditionalSealNum();
				}
			}
		}

		#endregion

		#endregion

		#region Units/Package Types

		public ZString ParentPackageUnit
		{
			get { return FreightPacksDataRegistry.Instance.OuterPackUnit.Value; }
		}

		public ZString ParentWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		public ZString ParentVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		public ZString ContainerWeightUnit
		{
			get { return FreightUtilities.IsValidWeightUnit(JC_GrossWeightUQ) ? JC_GrossWeightUQ : ParentWeightUnit; }
		}

		#endregion

		#region Calculated Properties

		#region JC_ContainerCode

		public ZString JC_ContainerCode
		{
			get
			{
				ZString result;

				result = JC_ContainerNum;
				if (result.IsEmpty && Container != null)
				{
					result = Container.RC_Code + " (" + JC_ContainerCount + ")";
				}

				return result;
			}
		}

		public ZPropertyInfo JC_ContainerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_ContainerCode); }
		}

		#endregion

		#region JC_Calc_ConsolID

		public ZString JC_Calc_ConsolID
		{
			get { return (Consol != null) ? Consol.JK_UniqueConsignRef : ZString.Empty; }
		}

		public ZPropertyInfo JC_Calc_ConsolIDInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ConsolID); }
		}

		#endregion

		#region JC_Calc_MasterBillNum

		public ZString JC_Calc_MasterBillNum
		{
			get { return (Consol != null) ? Consol.JK_MasterBillNum : ZString.Empty; }
		}

		public ZPropertyInfo JC_Calc_MasterBillNumInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_MasterBillNum); }
		}

		#endregion

		#region JC_Calc_TEUCount

		#region JC_Calc_TEUCount

		[DecimalPlaces(2)]
		public ZDecimal JC_Calc_TEUCount
		{
			get
			{
				ZDecimal result = 1;
				if (RefContainer != null)
				{
					if (JC_ContainerCount > 0)
					{
						result = JC_ContainerCount * RefContainer.RC_TEU;
					}
					else
					{
						result = RefContainer.RC_TEU;
					}
				}
				else if (JC_ContainerCount > 0)
				{
					result = new ZDecimal(JC_ContainerCount);
				}
				return result;
			}
		}

		public ZPropertyInfo JC_Calc_TEUCountInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TEUCount); }
		}

		#endregion

		#endregion

		#region JC_Calc_ContainerCount

		public ZInt JC_Calc_ContainerCount
		{
			get { return (JC_ContainerCount > 0) ? (ZInt)JC_ContainerCount : new ZInt(1); }
		}

		public ZPropertyInfo JC_Calc_ContainerCountInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ContainerCount); }
		}

		#endregion

		#region JC_Is20GP

		public ZBool JC_Is20GP
		{
			get
			{
				return (RefContainer != null && RefContainer.Is20GP);
			}
		}

		public ZPropertyInfo JC_Is20GPInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Is20GP); }
		}

		#endregion

		#region JC_Is40GP

		public ZBool JC_Is40GP
		{
			get
			{
				return (RefContainer != null && RefContainer.Is40GP);
			}
		}

		public ZPropertyInfo JC_Is40GPInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Is40GP); }
		}

		#endregion

		#region JC_Is20RE

		public ZBool JC_Is20RE
		{
			get { return (RefContainer != null && RefContainer.Is20RE); }
		}

		public ZPropertyInfo JC_Is20REInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Is20RE); }
		}

		#endregion

		#region JC_Is40RE

		public ZBool JC_Is40RE
		{
			get { return (RefContainer != null && RefContainer.Is40RE); }
		}

		public ZPropertyInfo JC_Is40REInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Is40RE); }
		}

		#endregion

		#region JC_IsOtherContainerType

		public ZBool JC_IsOtherContainerType
		{
			get { return RefContainer != null && RefContainer.IsOtherContainerType; }
		}

		public ZPropertyInfo JC_IsOtherContainerTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_IsOtherContainerType); }
		}

		#endregion

		#region Totals

		#region JC_Calc_ActualGrossWeightInKgs

		public ZDecimal JC_Calc_ActualGrossWeightInKgs
		{
			get
			{
				var result = JC_TareWeight + JC_Calc_TotalWeightInKgs + JC_DunnageWeight;
				return this.GetRoundedValue(JC_Calc_ActualGrossWeightInKgsInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_ActualGrossWeightInKgsInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ActualGrossWeightInKgs); }
		}

		#endregion

		#region JC_Calc_TotalVolume

		public ZDecimal JC_Calc_TotalVolume
		{
			get
			{
				var result = ((IPackLineCollection)PackLines).Totals.TotalVolume;
				return this.GetRoundedValue(JC_Calc_TotalVolumeInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_TotalVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalVolume); }
		}

		#endregion

		#region GetTotalVolumeByShipment

		public ZDecimal GetTotalVolumeByShipment(CommonShipment shipmentBO)
		{
			return ((IPackLineCollection)PackLines).Totals.TotalVolumeByShipment(shipmentBO);
		}

		#endregion

		#region JC_Calc_TotalVolumeUnit

		[List("TotalVolumeUnit_List")]
		public ZString JC_Calc_TotalVolumeUnit
		{
			get { return ((IPackLineCollection)PackLines).Totals.TotalVolumeUnit; }
		}

		public ZPropertyInfo JC_Calc_TotalVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalVolumeUnit); }
		}

		#endregion

		#region JC_Calc_TotalVolumeInM3

		public ZDecimal JC_Calc_TotalVolumeInM3
		{
			get
			{
				var result = Constants.Volume.Convert(JC_Calc_TotalVolume, JC_Calc_TotalVolumeUnit, Constants.Volume.CubicMetres, false);
				return this.GetRoundedValue(JC_Calc_TotalVolumeInM3Info, result);
			}
		}

		public ZPropertyInfo JC_Calc_TotalVolumeInM3Info
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalVolumeInM3); }
		}

		#endregion

		#region JC_Calc_TotalWeight

		public ZDecimal JC_Calc_TotalWeight
		{
			get
			{
				var result = ((IPackLineCollection)PackLines).Totals.TotalWeight;
				return this.GetRoundedValue(JC_Calc_TotalWeightInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_TotalWeightInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalWeight); }
		}

		#endregion

		#region GetTotalWeightByShipment

		public ZDecimal GetTotalWeightByShipment(CommonShipment shipmentBO)
		{
			return ((IPackLineCollection)PackLines).Totals.TotalWeightByShipment(shipmentBO);
		}

		#endregion

		#region GetTotalPackagesByShipment

		public ZDecimal GetTotalPackagesByShipment(CommonShipment shipmentBO)
		{
			return ((IPackLineCollection)PackLines).Totals.TotalPackagesByShipment(shipmentBO);
		}

		#endregion

		#region JC_Calc_TotalWeightUnit
		[List("TotalWeightUnit_List")]
		public ZString JC_Calc_TotalWeightUnit
		{
			get { return ((IPackLineCollection)PackLines).Totals.TotalWeightUnit; }
		}

		public ZPropertyInfo JC_Calc_TotalWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalWeightUnit); }
		}

		#endregion

		#region JC_Calc_TotalWeightInKgs

		public ZDecimal JC_Calc_TotalWeightInKgs
		{
			get
			{
				var result = Constants.Weight.Convert(JC_Calc_TotalWeight, JC_Calc_TotalWeightUnit, Constants.Weight.Kilograms, false);
				return this.GetRoundedValue(JC_Calc_TotalWeightInKgsInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_TotalWeightInKgsInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalWeightInKgs); }
		}

		#endregion

		#region GetTotalWeightInKgsByShipment

		public ZDecimal GetTotalWeightInKgsByShipment(CommonShipment shipmentBO)
		{
			return Core.Constants.Weight.Convert(GetTotalWeightByShipment(shipmentBO), JC_Calc_TotalWeightUnit, Core.Constants.Weight.Kilograms);
		}

		#endregion

		#region JC_Calc_TotalPackages

		public ZInt JC_Calc_TotalPackages
		{
			get { return ((IPackLineCollection)PackLines).Totals.TotalPackages; }
		}

		public ZPropertyInfo JC_Calc_TotalPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalPackages); }
		}

		#endregion

		#region JC_Calc_TotalPackagesUnit

		[List("TotalPackagesUnit_List")]
		public ZString JC_Calc_TotalPackagesUnit
		{
			get { return ((IPackLineCollection)PackLines).Totals.TotalPackagesUnit; }
		}

		public ZPropertyInfo JC_Calc_TotalPackagesUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TotalPackagesUnit); }
		}

		#endregion

		#region JC_Calc_ActualCapacity

		public ZDecimal JC_Calc_ActualCapacity
		{
			get
			{
				decimal result = Constants.Volume.Convert(JC_TotalHeight * JC_TotalWidth * JC_TotalLength, Constants.Volume.CubicFeet, Constants.Volume.CubicMetres, false);
				return this.GetRoundedValue(JC_Calc_ActualCapacityInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_ActualCapacityInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ActualCapacity); }
		}

		#endregion

		#endregion

		#region Ref Container Properties

		#region JC_Calc_ContainerCapacity

		public ZDecimal JC_Calc_ContainerCapacity
		{
			get
			{
				var result = (RefContainer != null) ? RefContainer.RC_CubicCapacity : new ZDecimal(0m);
				return this.GetRoundedValue(JC_Calc_ContainerCapacityInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_ContainerCapacityInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ContainerCapacity); }
		}

		#endregion

		#region JC_Calc_MaxGrossWeight

		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_MaxGrossWeight
		{
			get
			{
				ZDecimal result = 0;

				if (RefContainer != null)
				{
					result = JC_ContainerCount * Core.Constants.Weight.Convert(RefContainer.RC_GrossWeight, Core.Constants.Weight.Kilograms, ContainerWeightUnit, false);
				}

				return this.GetRoundedValue(JC_Calc_MaxGrossWeightInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_MaxGrossWeightInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_MaxGrossWeight); }
		}

		#endregion

		#region JC_Calc_TareWeight

		public ZDecimal JC_Calc_TareWeight
		{
			get
			{
				ZDecimal result = 0;

				if (RefContainer != null)
				{
					result = JC_ContainerCount * Core.Constants.Weight.Convert(RefContainer.RC_TareWeight, Core.Constants.Weight.Kilograms, ContainerWeightUnit, false);
				}

				return this.GetRoundedValue(JC_Calc_TareWeightInfo, result);
			}
		}

		public ZPropertyInfo JC_Calc_TareWeightInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_TareWeight); }
		}

		#endregion

		#region JC_Calc_Length

		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_Length
		{
			get { return RefContainer != null ? RefContainer.RC_Length : new ZDecimal(0m); }
		}

		public ZPropertyInfo JC_Calc_LengthInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_Length); }
		}

		#endregion

		#region JC_Calc_Width

		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_Width
		{
			get { return RefContainer != null ? RefContainer.RC_Width : new ZDecimal(0m); }
		}

		public ZPropertyInfo JC_Calc_WidthInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_Width); }
		}

		#endregion

		#region JC_Calc_Height

		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_Height
		{
			get { return RefContainer != null ? RefContainer.RC_Height : new ZDecimal(0m); }
		}

		public ZPropertyInfo JC_Calc_HeightInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_Height); }
		}

		#endregion

		#region JC_Calc_OverhangLength

		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_OverhangLength
		{
			get { return (JC_TotalLength > JC_Calc_Length) ? JC_TotalLength - JC_Calc_Length : 0m; }
		}

		public ZPropertyInfo JC_Calc_OverhangLengthInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_OverhangLength); }
		}

		#endregion

		#region JC_Calc_OverhangWidth

		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_OverhangWidth
		{
			get { return (JC_TotalWidth > JC_Calc_Width) ? JC_TotalWidth - JC_Calc_Width : 0m; }
		}

		public ZPropertyInfo JC_Calc_OverhangWidthInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_OverhangWidth); }
		}

		#endregion

		#region JC_Calc_OverhangHeight

		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_OverhangHeight
		{
			get { return (JC_TotalHeight > JC_Calc_Height) ? JC_TotalHeight - JC_Calc_Height : 0m; }
		}

		public ZPropertyInfo JC_Calc_OverhangHeightInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_OverhangHeight); }
		}

		#endregion

		#region JC_Calc_OverhangFront

		[ResourceStringData("JobContainer|JC_Calc_OverhangFront", Caption = "Overhang Front", ShortCaption = "Front")]
		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_OverhangFront
		{
			get { return (JC_Calc_OverhangLength > JC_OverhangBack) ? JC_Calc_OverhangLength - JC_OverhangBack : 0m; }
		}

		public ZPropertyInfo JC_Calc_OverhangFrontInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_OverhangFront); }
		}

		#endregion

		#region JC_Calc_OverhangLeft

		[ResourceStringData("JobContainer|JC_Calc_OverhangLeft", Caption = "Overhang Left", ShortCaption = "Left")]
		[DecimalPlaces(3)]
		public ZDecimal JC_Calc_OverhangLeft
		{
			get { return (JC_Calc_OverhangWidth > JC_OverhangRight) ? JC_Calc_OverhangWidth - JC_OverhangRight : 0m; }
		}

		public ZPropertyInfo JC_Calc_OverhangLeftInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_OverhangLeft); }
		}

		#endregion

		#endregion

		#endregion

		#region Sailing Calculated Properties

		#region JC_JA_NKPortOfLoading
		[List("RefUNLOCO_List")]
		public ZString JC_JA_NKPortOfLoading
		{
			get
			{
				ZString result = "";

				if (Consol != null)
				{
					result = Consol.JK_RL_NKLoadPort;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JA_RL_NKPortOfLoading;
				}

				return result;
			}
		}

		public ZPropertyInfo JC_JA_NKPortOfLoadingInfo
		{
			get { return GetZPropertyInfo(Schema.JC_JA_NKPortOfLoading); }
		}

		#endregion

		#region JC_JB_NKPortOfDischarge

		[List("RefUNLOCO_List")]
		public ZString JC_JB_NKPortOfDischarge
		{
			get
			{
				ZString result = "";

				if (Consol != null)
				{
					result = Consol.JK_RL_NKDischargePort;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JB_RL_NKPortOfDischarge;
				}

				return result;
			}
		}

		public ZPropertyInfo JC_JB_NKPortOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_JB_NKPortOfDischarge); }
		}

		#endregion

		#region JC_JV_NKVessel

		[List("RefVessel_List")]
		public ZString JC_JV_NKVessel
		{
			get
			{
				ZString result;

				if (Consol != null)
				{
					result = Consol.JK_JX_JV_NKVessel;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JV_NKVessel;
				}
				else
				{
					result = "";
				}

				return result;
			}
		}

		public ZPropertyInfo JC_JV_NKVesselInfo
		{
			get { return GetZPropertyInfo(Schema.JC_JV_NKVessel); }
		}

		#endregion

		#region JC_JV_VoyageFlight

		public ZString JC_JV_VoyageFlight
		{
			get
			{
				ZString result;

				if (Consol != null)
				{
					result = Consol.JK_JX_JV_VoyageFlight;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JV_VoyageFlight;
				}
				else
				{
					result = "";
				}

				return result;
			}
		}

		public ZPropertyInfo JC_JV_VoyageFlightInfo
		{
			get { return GetZPropertyInfo(Schema.JC_JV_VoyageFlight); }
		}

		#endregion

		#region JC_JA_E_DEP

		public ZDateTime JC_JA_E_DEP
		{
			get
			{
				ZDateTime result;

				if (Consol != null)
				{
					result = Consol.Transports.DepartureTransport.JW_ETD;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JA_E_DEP;
				}
				else
				{
					result = ZDateTime.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JC_JA_E_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JC_JA_E_DEP); }
		}

		#endregion

		#region JC_JB_E_ARV

		public ZDateTime JC_JB_E_ARV
		{
			get
			{
				ZDateTime result;

				if (Consol != null)
				{
					result = Consol.Transports.ArrivalTransport.JW_ETA;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JB_E_ARV;
				}
				else
				{
					result = ZDateTime.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo JC_JB_E_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JC_JB_E_ARV); }
		}

		#endregion

		#endregion

		#region New Bound Properties

		#region OrgAddress Columns

		#region Arrival

		public virtual ZGuid JC_JK_OA_ArrivalUnpackAddress
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (result.IsEmpty && Consol != null)
				{
					result = Consol.JK_OA_UnpackDepotAddress;
				}
				return result;
			}
			set
			{
				if (Consol != null && value != Consol.JK_OA_UnpackDepotAddress)
				{
					Consol.JK_OA_UnpackDepotAddress = value;
					JC_Calc_ArrivalUnpackAddressOrgInfo.RefreshBinding();
					JC_Calc_ArrivalUnpackAddressCodeInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region Departure

		public virtual ZGuid JC_JK_OA_DeparturePackAddress
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (result.IsEmpty && Consol != null)
				{
					result = Consol.JK_OA_PackDepotAddress;
				}
				return result;
			}
			set
			{
				if (Consol != null && value != Consol.JK_OA_PackDepotAddress)
				{
					Consol.JK_OA_PackDepotAddress = value;
					JC_Calc_DeparturePackAddressOrgInfo.RefreshBinding();
					JC_Calc_DeparturePackAddressCodeInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#endregion

		#region Address Split Properties

		#region DeparturePackAddress Split Properties

		#region JC_Calc_DeparturePackAddressOrg

		[List("PackDepot_List")]
		public ZGuid JC_Calc_DeparturePackAddressOrg
		{
			get
			{
				if (Consol != null)
				{
					ZGuid result = Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK;
					return result;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_PackDepotAddress_ZAddress.OrgPK = value;
					fJC_DeparturePackAddressList = null;
				}
				JC_Calc_DeparturePackAddressOrgInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					this.Validation.ValidateJC_Calc_DeparturePackAddressOrg();
				}
			}
		}

		public ZPropertyInfo JC_Calc_DeparturePackAddressOrgInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JC_Calc_DeparturePackAddressOrg);
			}
		}

		protected bool JC_Calc_DeparturePackAddressOrg_ReadOnly
		{
			get { return (Consol == null); }
		}

		#endregion

		#region JC_Calc_DeparturePackAddress

		public ZGuid JC_Calc_DeparturePackAddress
		{
			get
			{
				if (Consol != null)
				{
					return Consol.JK_OA_PackDepotAddress;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_PackDepotAddress = value;
				}
				JC_Calc_DeparturePackAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_DeparturePackAddressInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JC_Calc_DeparturePackAddress);
			}
		}

		#endregion

		#region JC_Calc_DeparturePackAddressCode

		[BusinessObjectTestExclude]
		[List("JC_DeparturePackAddressList")]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JC_Calc_DeparturePackAddressCode
		{
			get
			{
				if (Consol != null)
				{
					var address = Factory.Load<OrgAddress>(Consol.JK_OA_PackDepotAddress);
					if (address != null)
					{
						return address.OA_Code;
					}
				}
				return ZString.Empty;
			}
			set
			{
				if (Consol != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JC_Calc_DeparturePackAddressOrg);
					filter.AddToFilter(OrgAddressSchema.OA_Code, value);
					var address = Factory.LoadTop1<OrgAddress>(filter);
					Consol.JK_OA_PackDepotAddress = (address != null) ? address.PK : ZGuid.Empty;
				}
				JC_Calc_DeparturePackAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_DeparturePackAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_DeparturePackAddressCode); }
		}

		protected bool JC_Calc_DeparturePackAddressCode_ReadOnly
		{
			get { return (Consol == null || Consol.PackDepotAddress == null || !JC_Calc_DeparturePackAddressOrg.IsValid); }
		}

		#endregion

		#endregion

		#region DepartureCTOAddress Split Properties

		#region JC_Calc_DepartureCTOAddressOrg
		[List("CTO_List")]
		public ZGuid JC_Calc_DepartureCTOAddressOrg
		{
			get
			{
				if (Consol != null)
				{
					return Consol.JK_OA_DepartureCTOAddress_ZAddress.OrgPK;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_DepartureCTOAddress_ZAddress.OrgPK = value;
					fJC_DepartureCTOAddressList = null;
				}
				JC_Calc_DepartureCTOAddressOrgInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					this.Validation.ValidateJC_Calc_DepartureCTOAddressOrg();
				}
			}
		}

		public ZPropertyInfo JC_Calc_DepartureCTOAddressOrgInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JC_Calc_DepartureCTOAddressOrg);
			}
		}

		#endregion

		#region JC_Calc_DepartureCTOAddress

		public ZGuid JC_Calc_DepartureCTOAddress
		{
			get
			{
				if (Consol != null)
				{
					return Consol.JK_OA_DepartureCTOAddress;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_DepartureCTOAddress = value;
				}
				JC_Calc_DepartureCTOAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_DepartureCTOAddressInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JC_Calc_DepartureCTOAddress);
			}
		}

		#endregion

		#region JC_Calc_DepartureCTOAddressCode

		[BusinessObjectTestExclude]
		[List("JC_DepartureCTOAddressList")]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JC_Calc_DepartureCTOAddressCode
		{
			get
			{
				ZString result = "";
				if (Consol != null)
				{
					var address = Factory.Load<OrgAddress>(Consol.JK_OA_DepartureCTOAddress);
					if (address != null)
					{
						result = address.OA_Code;
					}
				}
				return result;
			}
			set
			{
				if (Consol != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JC_Calc_DepartureCTOAddressOrg);
					filter.AddToFilter(OrgAddressSchema.OA_Code, value);
					var address = Factory.LoadTop1<OrgAddress>(filter);
					Consol.JK_OA_DepartureCTOAddress = (address != null) ? address.PK : ZGuid.Empty;
				}
				JC_Calc_DepartureCTOAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_DepartureCTOAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_DepartureCTOAddressCode); }
		}

		protected bool JC_Calc_DepartureCTOAddressCode_ReadOnly
		{
			get { return (Consol == null) || (Consol.DepartureCTOAddress == null) || !JC_Calc_DepartureCTOAddressOrg.IsValid; }
		}

		#endregion

		#endregion

		#region DepartureContainerYardAddress Split Properties

		#region JC_Calc_DepartureContainerYardAddressOrg

		[List("Lookups.ContainerYard_List")]
		public ZGuid JC_Calc_DepartureContainerYardAddressOrg
		{
			get { return JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK; }
			set
			{
				JC_OA_DepartureContainerYardAddress_ZAddress.OrgPK = value;
				fJC_DepartureContainerYardAddressList = null;
				JC_Calc_DepartureContainerYardAddressOrgInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					this.Validation.ValidateJC_Calc_DepartureContainerYardAddressOrg();
				}
			}
		}

		public ZPropertyInfo JC_Calc_DepartureContainerYardAddressOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_DepartureContainerYardAddressOrg); }
		}

		#endregion

		#region JC_Calc_DepartureContainerYardAddressCode

		[BusinessObjectTestExclude]
		[List("JC_DepartureContainerYardAddressList")]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JC_Calc_DepartureContainerYardAddressCode
		{
			get
			{
				var address = Factory.Load<OrgAddress>(JC_OA_DepartureContainerYardAddress);
				return (address != null) ? address.OA_Code : ZString.Empty;
			}
			set
			{
				ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JC_Calc_DepartureContainerYardAddressOrg);
				filter.AddToFilter(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(filter);
				JC_OA_DepartureContainerYardAddress = (address != null) ? address.PK : ZGuid.Empty;
				JC_Calc_DepartureContainerYardAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_DepartureContainerYardAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_DepartureContainerYardAddressCode); }
		}

		protected bool JC_Calc_DepartureContainerYardAddressCode_ReadOnly
		{
			get { return (DepartureContainerYardAddress == null || !JC_Calc_DepartureContainerYardAddressOrg.IsValid); }
		}

		#endregion

		#endregion

		#region ArrivalUnpackAddress Split Properties

		#region JC_Calc_ArrivalUnpackAddressOrg
		[List("UnpackDepot_List")]
		public ZGuid JC_Calc_ArrivalUnpackAddressOrg
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (Consol != null)
				{
					result = Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK;
				}
				return result;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_UnpackDepotAddress_ZAddress.OrgPK = value;
					fJC_ArrivalUnpackAddressList = null;
				}
				JC_Calc_ArrivalUnpackAddressOrgInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					this.Validation.ValidateJC_Calc_ArrivalUnpackAddressOrg();
				}
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalUnpackAddressOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalUnpackAddressOrg); }
		}

		#endregion

		#region JC_Calc_ArrivalUnpackAddress

		public ZGuid JC_Calc_ArrivalUnpackAddress
		{
			get
			{
				if (Consol != null)
				{
					return Consol.JK_OA_UnpackDepotAddress;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_UnpackDepotAddress = value;
				}
				JC_Calc_ArrivalUnpackAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalUnpackAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalUnpackAddress); }
		}

		#endregion

		#region JC_Calc_ArrivalUnpackAddressCode

		[List("JC_ArrivalUnpackAddressList")]
		[BusinessObjectTestExclude]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JC_Calc_ArrivalUnpackAddressCode
		{
			get
			{
				if (Consol != null)
				{
					var address = Factory.Load<OrgAddress>(Consol.JK_OA_UnpackDepotAddress);
					return (address != null) ? address.OA_Code : ZString.Empty;
				}
				return ZString.Empty;
			}
			set
			{
				if (Consol != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JC_Calc_ArrivalUnpackAddressOrg);
					filter.AddToFilter(OrgAddressSchema.OA_Code, value);
					var address = Factory.LoadTop1<OrgAddress>(filter);
					Consol.JK_OA_UnpackDepotAddress = (address != null) ? address.PK : ZGuid.Empty;
				}
				JC_Calc_ArrivalUnpackAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalUnpackAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalUnpackAddressCode); }
		}

		protected bool JC_Calc_ArrivalUnpackAddressCode_ReadOnly
		{
			get { return (Consol == null || Consol.UnpackDepotAddress == null || !JC_Calc_ArrivalUnpackAddressOrg.IsValid); }
		}

		#endregion

		#endregion

		#region ArrivalCTOAddress Split Properties

		#region JC_Calc_ArrivalCTOAddressOrg

		[List("CTO_List")]
		public ZGuid JC_Calc_ArrivalCTOAddressOrg
		{
			get
			{
				if (Consol != null)
				{
					return Consol.JK_OA_ArrivalCTOAddress_ZAddress.OrgPK;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_ArrivalCTOAddress_ZAddress.OrgPK = value;
					fJC_ArrivalCTOAddressList = null;
				}
				JC_Calc_ArrivalCTOAddressOrgInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					this.Validation.ValidateJC_Calc_ArrivalCTOAddressOrg();
				}
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalCTOAddressOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalCTOAddressOrg); }
		}

		#endregion

		#region JC_Calc_ArrivalCTOAddress

		public ZGuid JC_Calc_ArrivalCTOAddress
		{
			get
			{
				if (Consol != null)
				{
					return Consol.JK_OA_ArrivalCTOAddress;
				}
				return ZGuid.Empty;
			}
			set
			{
				if (Consol != null)
				{
					Consol.JK_OA_ArrivalCTOAddress = value;
				}
				JC_Calc_ArrivalCTOAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalCTOAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalCTOAddress); }
		}

		#endregion

		#region JC_Calc_ArrivalCTOAddressCode

		[BusinessObjectTestExclude]
		[List("JC_ArrivalCTOAddressList")]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JC_Calc_ArrivalCTOAddressCode
		{
			get
			{
				if (Consol != null)
				{
					var address = Factory.Load<OrgAddress>(Consol.JK_OA_ArrivalCTOAddress);
					return (address != null) ? address.OA_Code : ZString.Empty;
				}
				return ZString.Empty;
			}
			set
			{
				if (Consol != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JC_Calc_ArrivalCTOAddressOrg);
					filter.AddToFilter(OrgAddressSchema.OA_Code, value);
					var address = Factory.LoadTop1<OrgAddress>(filter);
					Consol.JK_OA_ArrivalCTOAddress = (address != null) ? address.PK : ZGuid.Empty;
				}
				JC_Calc_ArrivalCTOAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalCTOAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalCTOAddressCode); }
		}

		protected bool JC_Calc_ArrivalCTOAddressCode_ReadOnly
		{
			get { return (Consol == null || Consol.ArrivalCTOAddress == null) || !JC_Calc_ArrivalCTOAddressOrg.IsValid; }
		}

		#endregion

		#endregion

		#region ArrivalContainerYardAddress Split Properties

		#region JC_Calc_ArrivalContainerYardAddressOrg

		[List("Lookups.ContainerYard_List")]
		public ZGuid JC_Calc_ArrivalContainerYardAddressOrg
		{
			get { return JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK; }
			set
			{
				JC_OA_ArrivalContainerYardAddress_ZAddress.OrgPK = value;
				fJC_ArrivalContainerYardAddressList = null;
				JC_Calc_ArrivalContainerYardAddressOrgInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					this.Validation.ValidateJC_Calc_ArrivalContainerYardAddressOrg();
				}
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalContainerYardAddressOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalContainerYardAddressOrg); }
		}

		#endregion

		#region JC_Calc_ArrivalContainerYardAddressCode

		[BusinessObjectTestExclude]
		[List("JC_ArrivalContainerYardAddressList")]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JC_Calc_ArrivalContainerYardAddressCode
		{
			get
			{
				var address = Factory.Load<OrgAddress>(JC_OA_ArrivalContainerYardAddress);
				return (address != null) ? address.OA_Code : ZString.Empty;
			}
			set
			{
				ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JC_Calc_ArrivalContainerYardAddressOrg);
				filter.AddToFilter(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(filter);
				JC_OA_ArrivalContainerYardAddress = (address != null) ? address.PK : ZGuid.Empty;
				JC_Calc_ArrivalContainerYardAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_Calc_ArrivalContainerYardAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ArrivalContainerYardAddressCode); }
		}

		protected bool JC_Calc_ArrivalContainerYardAddressCode_ReadOnly
		{
			get { return (ArrivalContainerYardAddress == null || !JC_Calc_ArrivalContainerYardAddressOrg.IsValid); }
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region JC_ContainerNum

		public override ZString JC_ContainerNum
		{
			get { return base.JC_ContainerNum; }
			set
			{
				if (base.JC_ContainerNum != value)
				{
					base.JC_ContainerNum = value.ToUpper();
					ContainerEventDataVendor.Instance.NotifyContainerNumberChanged(this);

					if (Consol != null && !Consol.IsDeleted)
					{
						foreach (CommonContainer container in Consol.Containers)
						{
							if (!IsValidationSuspended)
							{
								container.Validation.ValidateJC_ContainerNum();
							}
						}
					}

					if (Sailing != null && !Sailing.IsDeleted)
					{
						foreach (CommonContainer container in Sailing.Containers)
						{
							if (!container.IsValidationSuspended)
							{
								container.Validation.ValidateJC_ContainerNum();
							}
						}
					}
				}
			}
		}

		#endregion

		#region JC_ArrivalCartageComplete

		public override ZDateTime JC_ArrivalCartageComplete
		{
			get { return new ZDateTime(base.JC_ArrivalCartageComplete, DateTimeKind.Unspecified); }
			set
			{
				if (base.JC_ArrivalCartageComplete != value)
				{
					ZDateTime originalValue = JC_ArrivalCartageComplete;
					base.JC_ArrivalCartageComplete = value;

					if (DestinationConfirm != null)
					{
						ConfirmTimesSyncHelper.SetConfirmTimes(this, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual, originalValue, value);
					}
				}
			}
		}

		#endregion

		#region JC_DepartureCartageAdvised

		public override ZDateTime JC_DepartureCartageAdvised
		{
			get
			{
				return new ZDateTime(base.JC_DepartureCartageAdvised, DateTimeKind.Unspecified);
			}
			set
			{
				if (base.JC_DepartureCartageAdvised != value)
				{
					base.JC_DepartureCartageAdvised = value;

					foreach (CommonShipment shipment in ParentShipmentsCached)
					{
						if (shipment.DocsAndCartage.JP_PickupCartageAdvised.IsEmpty)
						{
							ZDateTime latestAdvisedDateDeparture = ZDateTime.Empty;

							foreach (CommonContainer container in shipment.Containers)
							{
								if (!container.JC_DepartureCartageAdvised.IsValid ||
									container.JC_DepartureCartageAdvised.IsEmpty)
								{
									latestAdvisedDateDeparture = ZDateTime.Empty;
									break;
								}

								if (latestAdvisedDateDeparture.IsEmpty ||
									container.JC_DepartureCartageAdvised > latestAdvisedDateDeparture)
								{
									latestAdvisedDateDeparture = container.JC_DepartureCartageAdvised;
								}
							}

							if (!latestAdvisedDateDeparture.IsEmpty)
							{
								shipment.DocsAndCartage.JP_PickupCartageAdvised = latestAdvisedDateDeparture;
							}
						}
					}
				}
			}
		}

		#endregion

		#region JC_ArrivalCartageAdvised

		public override ZDateTime JC_ArrivalCartageAdvised
		{
			get
			{
				return new ZDateTime(base.JC_ArrivalCartageAdvised, DateTimeKind.Unspecified);
			}
			set
			{
				if (base.JC_ArrivalCartageAdvised != value)
				{
					base.JC_ArrivalCartageAdvised = value;

					foreach (CommonShipment shipment in ParentShipmentsCached)
					{
						if (shipment.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty)
						{
							ZDateTime latestAdvisedDateArrival = ZDateTime.Empty;

							foreach (CommonContainer container in shipment.Containers)
							{
								if (!container.JC_ArrivalCartageAdvised.IsValid ||
									container.JC_ArrivalCartageAdvised.IsEmpty)
								{
									latestAdvisedDateArrival = ZDateTime.Empty;
									break;
								}

								if (latestAdvisedDateArrival.IsEmpty ||
									container.JC_ArrivalCartageAdvised > latestAdvisedDateArrival)
								{
									latestAdvisedDateArrival = container.JC_ArrivalCartageAdvised;
								}
							}

							if (!latestAdvisedDateArrival.IsEmpty)
							{
								shipment.DocsAndCartage.JP_DeliveryCartageAdvised = latestAdvisedDateArrival;
							}
						}
					}
				}
			}
		}

		#endregion

		#region JC_PackDate

		public override ZDateTime JC_PackDate
		{
			get { return new ZDateTime(base.JC_PackDate, DateTimeKind.Unspecified); }
			set { base.JC_PackDate = value; }
		}

		#endregion

		#region JC_PivotBreak

		public override ZDecimal JC_PivotBreak
		{
			get => base.JC_PivotBreak;
			set
			{
				if (base.JC_PivotBreak != value)
				{
					base.JC_PivotBreak = value;

					if (!IsSettingWeightsSuspendedFromUniversalShipmentReader)
					{
						Consol?.Containers.OfType<CommonContainer>().Where(x => x.JC_RC == this.JC_RC && x.PK != this.PK)
							.ForEach(container => container.SetPivotBreakForThisContainerOnly(value));
					}
				}
			}
		}

		void SetPivotBreakForThisContainerOnly(ZDecimal pivotBreak)
			=> base.JC_PivotBreak = pivotBreak;

		#endregion

		#region JC_LCLUnpack

		public override ZDateTime JC_LCLUnpack
		{
			get { return new ZDateTime(base.JC_LCLUnpack, DateTimeKind.Unspecified); }
			set { base.JC_LCLUnpack = value; }
		}

		#endregion

		#region JC_DepartureTruckWaitTime

		[ZDateTimeDurationValueExclude1900]
		public ZDateTime DepartureTruckWaitTime
		{
			get
			{
				var penalty = ExportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport);
				return penalty?.CPY_Duration ?? ZDateTime.Empty;
			}
			set
			{
				var penalty = ExportPenalties.FindOrCreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport,
					timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
					creditor: ContainerParent?.DeparturePackCFSTransportAddress);
				penalty.CPY_Duration = value;

				DepartureTruckWaitTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DepartureTruckWaitTimeInfo => GetZPropertyInfo(nameof(DepartureTruckWaitTime));

		#endregion

		#region JC_GrossVolume

		public override ZDecimal JC_GrossVolume
		{
			get { return base.JC_GrossVolume; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobContainerSchema.JC_GrossVolume, JC_GrossVolumeInfo, value);
				base.JC_GrossVolume = roundedValue;
			}
		}

		#endregion

		#region JC_GrossVolumeUQ

		public override ZString JC_GrossVolumeUQ
		{
			get { return base.JC_GrossVolumeUQ; }
			set
			{
				base.JC_GrossVolumeUQ = value;
				this.SetRoundedValue(JobContainerSchema.JC_GrossVolume, JC_GrossVolumeInfo);
			}
		}

		#endregion

		#region JC_VolumeCapacity

		public override ZDecimal JC_VolumeCapacity
		{
			get { return base.JC_VolumeCapacity; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobContainerSchema.JC_VolumeCapacity, JC_VolumeCapacityInfo, value);
				base.JC_VolumeCapacity = roundedValue;
			}
		}

		#endregion

		#region JC_VolumeCapacityUQ

		public override ZString JC_VolumeCapacityUQ
		{
			get { return base.JC_VolumeCapacityUQ; }
			set
			{
				base.JC_VolumeCapacityUQ = value;
				this.SetRoundedValue(JobContainerSchema.JC_VolumeCapacity, JC_VolumeCapacityInfo);
			}
		}

		#endregion

		#region JC_WeightCapacity

		public override ZDecimal JC_WeightCapacity
		{
			get { return base.JC_WeightCapacity; }
			set
			{
				var roundedValue = this.GetRoundedValue(JobContainerSchema.JC_WeightCapacity, JC_WeightCapacityInfo, value);
				base.JC_WeightCapacity = roundedValue;
			}
		}

		#endregion

		#region JC_WeightCapacityUQ

		public override ZString JC_WeightCapacityUQ
		{
			get { return base.JC_WeightCapacityUQ; }
			set
			{
				base.JC_WeightCapacityUQ = value;
				this.SetRoundedValue(JobContainerSchema.JC_WeightCapacity, JC_WeightCapacityInfo);
			}
		}

		#endregion

		#region New Properties

		internal bool IsOnARVFCLConsol
		{
			get
			{
				foreach (PackLine packedPackLine in PackLines)
				{
					if (packedPackLine.Shipment != null && packedPackLine.Shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery))
					{
						if (packedPackLine.Shipment.ArrivalContainers.Contains(this))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		internal bool IsOnDEPFCLConsol
		{
			get
			{
				foreach (PackLine packedPackLine in PackLines)
				{
					if (packedPackLine.Shipment != null && packedPackLine.Shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup))
					{
						if (packedPackLine.Shipment.DepartureContainers.Contains(this))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		protected bool IsFCL
		{
			get { return JC_ContainerMode == Core.Constants.ContainerModes.FCL; }
		}

		protected bool IsFAK
		{
			get { return JC_ContainerMode == Core.Constants.ContainerModes.Groupage; }
		}

		protected bool IsULD
		{
			get { return JC_ContainerMode == Core.Constants.ContainerModes.ULD; }
		}

		protected bool IsAir
		{
			get { return JC_ContainerMode == Core.Constants.ContainerModes.AIR; }
		}

		#endregion

		#region ParentShipments

		public IEnumerable<CommonShipment> GetParentShipments()
		{
			return PackLines.Cast<PackLine>().Select(x => x.Shipment).Where(x => x != null).Distinct().ToArray();
		}

		public IEnumerable<CommonShipment> ParentShipmentsCached
		{
			get
			{
				return parentShipments ?? (parentShipments = GetParentShipments().ToArray());
			}
		}
		IEnumerable<CommonShipment> parentShipments;

		#endregion

		#region HasHazardous

		public virtual ZBool HasHazardous
		{
			get
			{
				return JC_RH_NKContainerCommodityCode == "HAZ" ||
						JC_RH_NKContainerCommodityCode == "HAZD" ||
						JC_RH_NKContainerCommodityCode == "MTHZ";
			}
		}

		#endregion

		public void DefaultOutturnFromManifested()
		{
			foreach (PackLine packLine in PackLines)
			{
				if (packLine.JL_Outturn == 0)
				{
					packLine.JL_Outturn = packLine.JL_PackageCount;
				}
			}
		}

		#region ParentLoadPort / ParentDischargePort

		internal ZString ParentLoadPort
		{
			get { return ContainerParent != null && ContainerParent.LoadPort != null ? ContainerParent.LoadPort.RL_Code : ZString.Empty; }
		}

		internal ZString ParentDischargePort
		{
			get { return ContainerParent != null && ContainerParent.DischargePort != null ? ContainerParent.DischargePort.RL_Code : ZString.Empty; }
		}

		#endregion

		public string ECNOrCAN
		{
			get
			{
				ZString result = ZString.Empty;
				CommonShipment firstAndOnlyShipment = null;
				foreach (PackLine packLine in PackLines)
				{
					CommonShipment packLineShipment = packLine.Shipment;
					if (!packLineShipment.IsDeleted && !packLineShipment.JS_IsCancelled)
					{
						if (firstAndOnlyShipment == null)
						{
							firstAndOnlyShipment = packLineShipment;
							result = packLineShipment.ECNOrCAN;
						}
						else
						{
							if (firstAndOnlyShipment != packLineShipment
								&& packLineShipment != null
								&& packLineShipment.ECNOrCAN != result)
							{
								result = ZString.Empty;
								break;
							}
						}
					}
				}
				return result;
			}
		}

		#region Container Spot Rates

		#region JC_SellSpotRate

		[DecimalPlaces(4)]
		public override ZDecimal JC_SellSpotRate
		{
			get { return base.JC_SellSpotRate; }
			set
			{
				if (base.JC_SellSpotRate != value)
				{
					base.JC_SellSpotRate = value;

					if (value != ZDecimal.Zero)
					{
						if (JC_RX_NKSellSpotRateCurrency.IsEmpty)
						{
							JC_RX_NKSellSpotRateCurrency = GetCurrencyForSpotRate();
						}

						if (JC_SellSpotRateMode.IsEmpty || JC_SellSpotRateMode == Constants.FreightRateAutoratingModes.Code.StandardRate)
						{
							JC_SellSpotRateMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
						}
					}

					JC_SellSpotRateModeInfo.RefreshBinding();
					JC_RX_NKSellSpotRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.SpotRateAutoratingModesList")]
		public override ZString JC_SellSpotRateMode
		{
			get { return base.JC_SellSpotRateMode; }
			set
			{
				if (base.JC_SellSpotRateMode != value)
				{
					base.JC_SellSpotRateMode = value;
					if (value == Constants.FreightRateAutoratingModes.Code.StandardRate)
					{
						JC_SellSpotRate = ZDecimal.Zero;
					}

					JC_SellSpotRateInfo.RefreshBinding();
					JC_RX_NKSellSpotRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region JC_CostSpotRate

		[DecimalPlaces(4)]
		public override ZDecimal JC_CostSpotRate
		{
			get { return base.JC_CostSpotRate; }
			set
			{
				if (base.JC_CostSpotRate != value)
				{
					base.JC_CostSpotRate = value;

					if (value != ZDecimal.Zero)
					{
						if (JC_RX_NKCostSpotRateCurrency.IsEmpty)
						{
							JC_RX_NKCostSpotRateCurrency = GetCurrencyForSpotRate();
						}

						if (JC_CostSpotRateMode.IsEmpty || JC_CostSpotRateMode == Constants.FreightRateAutoratingModes.Code.StandardRate)
						{
							JC_CostSpotRateMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
						}
					}

					JC_CostSpotRateModeInfo.RefreshBinding();
					JC_RX_NKCostSpotRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.SpotRateAutoratingModesList")]
		public override ZString JC_CostSpotRateMode
		{
			get { return base.JC_CostSpotRateMode; }
			set
			{
				if (base.JC_CostSpotRateMode != value)
				{
					base.JC_CostSpotRateMode = value;
					if (value == Constants.FreightRateAutoratingModes.Code.StandardRate)
					{
						JC_CostSpotRate = ZDecimal.Zero;
					}

					JC_CostSpotRateInfo.RefreshBinding();
					JC_RX_NKCostSpotRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region JC_GatewaySellSpotRate

		[DecimalPlaces(4)]
		public override ZDecimal JC_GatewaySellSpotRate
		{
			get { return base.JC_GatewaySellSpotRate; }
			set
			{
				if (base.JC_GatewaySellSpotRate != value)
				{
					base.JC_GatewaySellSpotRate = value;

					if (value != ZDecimal.Zero)
					{
						if (JC_RX_NKGatewaySellSpotRateCurrency.IsEmpty)
						{
							JC_RX_NKGatewaySellSpotRateCurrency = GetCurrencyForSpotRate();
						}

						if (JC_GatewaySellSpotRateMode.IsEmpty || JC_GatewaySellSpotRateMode == Constants.FreightRateAutoratingModes.Code.StandardRate)
						{
							JC_GatewaySellSpotRateMode = Constants.FreightRateAutoratingModes.Code.FreightPlusRate;
						}
					}

					JC_GatewaySellSpotRateModeInfo.RefreshBinding();
					JC_RX_NKGatewaySellSpotRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		[List("Lookups.SpotRateAutoratingModesList")]
		public override ZString JC_GatewaySellSpotRateMode
		{
			get { return base.JC_GatewaySellSpotRateMode; }
			set
			{
				if (base.JC_GatewaySellSpotRateMode != value)
				{
					base.JC_GatewaySellSpotRateMode = value;
					if (value == Constants.FreightRateAutoratingModes.Code.StandardRate)
					{
						JC_GatewaySellSpotRate = ZDecimal.Zero;
					}

					JC_GatewaySellSpotRateInfo.RefreshBinding();
					JC_RX_NKGatewaySellSpotRateCurrencyInfo.RefreshBinding();
				}
			}
		}

		#endregion

		ZString GetCurrencyForSpotRate()
		{
			ZString result = ZString.Empty;
			if (Consol != null)
			{
				switch (Consol.TransportMode)
				{
					case Constants.TransportModes.Sea:
						result = (Consol.JK_ConsolMode == Constants.ContainerModes.FCL
								 || Consol.JK_ConsolMode == Constants.ContainerModes.LCL
								 || Consol.JK_ConsolMode == Constants.ContainerModes.Groupage
								 || Consol.JK_ConsolMode == Constants.ContainerModes.Other)
							? new ZString(Constants.CurrencyCodes.UnitedStates)
							: ZString.Empty;
						break;

					case Constants.TransportModes.Air:
						var originLocation = LocationHelper.GetLocationFromString(JC_JA_NKPortOfLoading, Factory);
						result = GetCurrencyForSpotRate(originLocation);
						break;

					default:
						var destinationLocation = LocationHelper.GetLocationFromString(JC_JB_NKPortOfDischarge, Factory);
						result = GetCurrencyForSpotRate(destinationLocation);
						break;
				}
			}

			return result;
		}

		ZString GetCurrencyForSpotRate(ILocation location)
		{
			return (location != null && location.Country != null)
						? location.Country.RN_RX_NKLocalCurrency
						: GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
		}

		#endregion

		#endregion

		#region Behaviour

		CommonContainerBehaviorStrategy BehaviorStrategy
		{
			get { return BehaviorStrategyProvider.GetProvider(Factory).ContainerBehaviorStrategy; }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList PreCarriageOnCarriageTransportMode_List
		{
			get { return Factory.GetCachedValue<JobAddressAdditionalInfoTransportModeCodeDescriptionPairList>(); }
		}

		protected override JobContainerLookups GetNewLookups()
		{
			return new CommonContainerLookups(this);
		}

		public new CommonContainerLookups Lookups
		{
			get { return (CommonContainerLookups)base.Lookups; }
		}

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public OrgHeaderCollection OrgHeader_List
		{
			get { return BindingLists.OrgHeader_List; }
		}

		public RefContainerCollection RefContainer_List
		{
			get
			{
				if (fRefContainer_List == null)
				{
					ZQuery filter = new ZQuery();

					if (IsAirContainer)
					{
						fRefContainer_List = new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Air);
					}
					else if (IsSeaContainer)
					{
						fRefContainer_List = new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Sea);
					}
					else if (IsRoadContainer)
					{
						fRefContainer_List = new RefContainerCollection(Factory, RefContainerLookups.ShippingModes.Road);
					}
					else
					{
						fRefContainer_List = new RefContainerCollection(Factory, filter);
					}
				}

				return fRefContainer_List;
			}
		}

		RefContainerCollection fRefContainer_List;

		protected void ResetContainerCollection()
		{
			fRefContainer_List = null;
		}

		public PackDepotCollection PackDepot_List
		{
			get { return BindingLists.PackDepot_List; }
		}

		public UnpackDepotCollection UnpackDepot_List
		{
			get { return BindingLists.UnpackDepot_List; }
		}

		public CTOCollection CTO_List
		{
			get { return BindingLists.OrgCTO_List; }
		}

		public LocalTransportCollection MiscServLocalTransport_List
		{
			get { return BindingLists.OrgMiscServLocalTransport_List; }
		}

		public UNDGSubstanceCollection UNDGSubstance_List
		{
			get { return BindingLists.UNDGSubstance_List; }
		}

		public OrganisationsFindBoxCollection PackDepotOrExporter_List
		{
			get
			{
				ZQuery exporterFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
				return new PackDepotCollection(Factory, exporterFilter);
			}
		}

		public OrganisationsFindBoxCollection UnpackDepotOrImporter_List
		{
			get
			{
				ZQuery importerFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				return new UnpackDepotCollection(Factory, importerFilter);
			}
		}

		public UNDGSubstanceCollection UNDGSubstances_List
		{
			get { return BindingLists.UNDGSubstance_List; }
		}

		public CodeDescriptionPairList JC_TemperatureUnit_List
		{
			get
			{
				return Factory.GetCachedValue("Container.JC_TemperatureUnit_List", () =>
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair("C", Res.GetString("6300af2f-3c34-408b-804e-e8954ef457df", "Centigrade"));
						result.AddPair("F", Res.GetString("5782f9d2-afdf-4489-80f4-cd7ce497b73a", "Fahrenheit"));
						return result;
					});
			}
		}

		public virtual RefCommodityCodeCollection ContainerCommodityCode_List
		{
			get { return containerCommodityCode_List ?? (containerCommodityCode_List = GetNewContainerCommodityCodeListCore()); }
		}
		RefCommodityCodeCollection containerCommodityCode_List;

		protected virtual RefCommodityCodeCollection GetNewContainerCommodityCodeListCore()
		{
			return new RefCommodityCodeCollection(Factory);
		}

		public virtual IRatingContractAllocationLineCollection AllocationLineCollection
		{
			get
			{
				if (allocationLineCollection == null)
				{
					allocationLineCollection = ObjectFactory.Get<IRatingContractAllocationLineCollection>(nameof(IRatingContractAllocationLineCollection), Factory);
				}

				return allocationLineCollection;
			}
		}

		protected IRatingContractAllocationLineCollection allocationLineCollection;

		#region OrgAddress Lists - Dependent on respective OrgHeaders

		BusinessObjectCollection fJC_DeparturePackAddressList;
		public BusinessObjectCollection JC_DeparturePackAddressList
		{
			get
			{
				if (fJC_DeparturePackAddressList == null)
				{
					fJC_DeparturePackAddressList = GetOrganisationAddressList(JC_Calc_DeparturePackAddressOrg);
				}
				return fJC_DeparturePackAddressList;
			}
		}

		BusinessObjectCollection fJC_DepartureCTOAddressList;
		public BusinessObjectCollection JC_DepartureCTOAddressList
		{
			get
			{
				if (fJC_DepartureCTOAddressList == null)
				{
					fJC_DepartureCTOAddressList = GetOrganisationAddressList(JC_Calc_DepartureCTOAddressOrg);
				}
				return fJC_DepartureCTOAddressList;
			}
		}

		BusinessObjectCollection fJC_DepartureContainerYardAddressList;
		public BusinessObjectCollection JC_DepartureContainerYardAddressList
		{
			get
			{
				if (fJC_DepartureContainerYardAddressList == null)
				{
					fJC_DepartureContainerYardAddressList = GetOrganisationAddressList(JC_Calc_DepartureContainerYardAddressOrg);
				}
				return fJC_DepartureContainerYardAddressList;
			}
		}

		BusinessObjectCollection fJC_ArrivalUnpackAddressList;
		public BusinessObjectCollection JC_ArrivalUnpackAddressList
		{
			get
			{
				if (fJC_ArrivalUnpackAddressList == null)
				{
					fJC_ArrivalUnpackAddressList = GetOrganisationAddressList(JC_Calc_ArrivalUnpackAddressOrg);
				}
				return fJC_ArrivalUnpackAddressList;
			}
		}

		BusinessObjectCollection fJC_ArrivalCTOAddressList;
		public BusinessObjectCollection JC_ArrivalCTOAddressList
		{
			get
			{
				if (fJC_ArrivalCTOAddressList == null)
				{
					fJC_ArrivalCTOAddressList = GetOrganisationAddressList(JC_Calc_ArrivalCTOAddressOrg);
				}
				return fJC_ArrivalCTOAddressList;
			}
		}

		BusinessObjectCollection fJC_ArrivalContainerYardAddressList;
		public BusinessObjectCollection JC_ArrivalContainerYardAddressList
		{
			get
			{
				if (fJC_ArrivalContainerYardAddressList == null)
				{
					fJC_ArrivalContainerYardAddressList = GetOrganisationAddressList(JC_Calc_ArrivalContainerYardAddressOrg);
				}
				return fJC_ArrivalContainerYardAddressList;
			}
		}

		protected BusinessObjectCollection GetOrganisationAddressList(ZGuid orgHeaderPK)
		{
			OrgHeader organisation = null;
			if (orgHeaderPK.IsValid)
			{
				organisation = Factory.Load<OrgHeader>(orgHeaderPK);
			}

			if (organisation == null)
			{
				return new OrgAddressCollection(Factory);
			}
			else
			{
				OrgAddressDependentCollection addressList = new OrgAddressDependentCollection(organisation);
				addressList.Load();
				return addressList;
			}
		}

		#endregion

		#region JC_ContainerMode_List

		public virtual CodeDescriptionPairList JC_ContainerMode_List
		{
			get { return CommonContainerLookups.GetContainerModesList(Transport); }
		}

		#endregion

		#region JC_DeliveryMode_List

		protected virtual CommonConsol fMaster
		{
			get { return Consol; }
		}

		#endregion

		#region TotalVolumeUnit_List

		public CodeDescriptionPairList TotalVolumeUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region TotalWeightUnit_List

		public CodeDescriptionPairList TotalWeightUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region TotalPackagesUnit_List

		public RefPackTypeCollection TotalPackagesUnit_List
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		#endregion

		public CodeDescriptionPairList JC_VehicleTransmission_List
		{
			get
			{
				return Factory.GetCachedValue("CommonContainer.JC_VehicleTransmission_List",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.VehicleTransmissionType.Automatic, ResString.GetMultilingualString("9b457d85-5c8c-451b-a730-7d33bec4858d", "Automatic"));
						result.AddPair(Core.Constants.VehicleTransmissionType.Manual, ResString.GetMultilingualString("6904d896-bfdd-43a6-8daa-570c0fc5240d", "Manual"));
						return result;
					});
			}
		}

		public RefCurrencyCollection RefCurrency_List
		{
			get { return BindingLists.RefCurrency_List; }
		}

		#endregion

		#region Validation

		protected override JobContainerValidation GetNewValidation()
		{
			return BehaviorStrategy.GetNewValidation(this);
		}

		public new CommonContainerValidation Validation
		{
			get { return (CommonContainerValidation)base.Validation; }
		}

		protected virtual void CheckContainerNumberAgainstRelatedContainers()
		{
		}

		public void OuterCheckContainerNumberAgainstRelatedContainers()
		{
			CheckContainerNumberAgainstRelatedContainers();
		}

		#endregion

		#region IHaveServices Members

		public virtual IHaveServices[] DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		ZString IHaveServices.TableCode
		{
			get { return JobContainerSchema.Constants.Prefix; }
		}

		ZString IHaveServices.TransportMode
		{
			get { return Transport; }
		}

		ZString IHaveServices.ContainerMode
		{
			get { return JC_ContainerMode; }
		}

		BusinessObject IHaveServices.ServiceParent
		{
			get { return this; }
		}

		bool IHaveServices.NeedsServiceEvents
		{
			get { return true; }
		}

		IBranch IHaveServices.ServiceBranch => Env.CurrentBranch;

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		#endregion

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = NewDocManagerInfo();
				}
				return docManagerInfo;
			}
		}

		protected virtual DocManagerInfo NewDocManagerInfo()
		{
			return new DocManagerInfo(this, Core.Constants.DocManagerCodes.Container);
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IContainer Members

		ZString IContainer.ContainerMode
		{
			get { return this.JC_ContainerMode; }
		}

		ZBool IContainer.IsQuarantineRequired
		{
			get { return Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.QuarantineInspection); }
		}

		ZBool IContainer.IsCustomsHold
		{
			get { return Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.CustomsHold); }
		}

		ZBool IContainer.IsFumigationRequired
		{
			get { return Services.IsServiceRequired(Core.Constants.FreightServiceType.Codes.Fumigation); }
		}

		#endregion

		#region IContainerExtra Members

		public ZDecimal GoodsWeight
		{
			get { return JC_Calc_TotalWeight; }
		}

		public ZString GoodsWeightUQ
		{
			get { return JC_Calc_TotalWeightUnit; }
		}

		#endregion

		#region GrossWeightVerifiedByAddress

		public JobDocAddress GrossWeightVerifiedByAddress
		{
			get
			{
				if (IsNullOrDeleted(grossWeightVerifiedByAddress))
				{
					grossWeightVerifiedByAddress = DocAddresses.FindOrCreateWithRequirement(GrossWeightVerifiedByDocAddressRequirement);
					grossWeightVerifiedByAddress.DocAddressChanged += new EventHandler(GrossWeightVerifiedByAddress_DocAddressChanged);
				}

				return grossWeightVerifiedByAddress;
			}
		}

		void GrossWeightVerifiedByAddress_DocAddressChanged(object sender, EventArgs e)
		{
			if (grossWeightVerifiedByAddress?.Organisation != null)
			{
				OrgContact contactForAllDocumentGroup = null;
				foreach (OrgContact contact in grossWeightVerifiedByAddress.Organisation.Contacts)
				{
					if (contact.Documents.Any(orgDoc => ((OrgDocument)orgDoc).OD_DocumentGroup == ContactType.VerifiedGrossWeightContact.Code))
					{
						grossWeightVerifiedByAddress.E2_Contact = contact.OC_ContactName;
						return;
					}
					else if (contactForAllDocumentGroup == null && contact.Documents.Any(orgDoc => ((OrgDocument)orgDoc).OD_DocumentGroup == ContactType.All.Code))
					{
						contactForAllDocumentGroup = contact;
					}
				}

				if (contactForAllDocumentGroup != null)
				{
					grossWeightVerifiedByAddress.E2_Contact = contactForAllDocumentGroup.OC_ContactName;
				}
			}
		}

		JobDocAddress grossWeightVerifiedByAddress;

		#endregion

		#region GrossWeightVerifiedByDocAddressRequirement

		JobDocAddressRequirement GrossWeightVerifiedByDocAddressRequirement
		{
			get
			{
				if (grossWeightVerifiedByDocAddressRequirement == null)
				{
					grossWeightVerifiedByDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.GrossWeightVerifiedBy, ContactType.VerifiedGrossWeightContact);
					SetupVerifiedByAddressValidation();
				}

				return grossWeightVerifiedByDocAddressRequirement;
			}
		}

		void SetupVerifiedByAddressValidation()
		{
			GrossWeightVerifiedByDocAddressRequirement.ValidateOrganisationPK = ValidateVerifiedByOrganisationPK;
			GrossWeightVerifiedByDocAddressRequirement.ValidateCompanyName = ValidateVerifiedByCompanyName;
		}

		void ValidateVerifiedByCompanyName(JobDocAddressValidation validation)
		{
			if (IsGrossWeightVerified)
			{
				if (GrossWeightVerifiedByAddress.E2_AddressOverride)
				{
					MandatoryValidation.CheckEntered(GrossWeightVerifiedByAddress.E2_CompanyNameInfo);
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(GrossWeightVerifiedByAddress.E2_CompanyNameInfo);
			}
		}

		void ValidateVerifiedByOrganisationPK(JobDocAddressValidation validation)
		{
			if (IsGrossWeightVerified)
			{
				if (!GrossWeightVerifiedByAddress.E2_AddressOverride && GrossWeightVerifiedByAddress.OrganisationPKInfo.Value.IsEmpty)
				{
					GrossWeightVerifiedByAddress.OrganisationPKInfo.AddWarning(Res.GetString("ff3e8efb-b1f3-4a58-91c5-f12b3dc3e48e", "The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message."));
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(GrossWeightVerifiedByAddress.OrganisationPKInfo);
			}

			if (JC_GrossWeightVerificationType == Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages
				&& Consol != null && Consol.IsSea && Consol.IsExport()
				&& (Consol.LoadPort.Country.Code == Constants.CountryCodes.UnitedKingdom || Consol.LoadPort.Country.Code == Constants.CountryCodes.SouthAfrica)
				&& GrossWeightVerifiedByAddress.Organisation != null
				&& !GrossWeightVerifiedByAddress.Organisation.CustomsCodes.Cast<OrgCusCode>().Any(code =>
					code.OK_CodeType == OrgCusCode.CodeTypes.VGMRegistrationNumber
					&& code.CodeCountry == Consol.LoadPort.Country
					&& code.OK_OA_PremisesAddress == GrossWeightVerifiedByAddress.Address.PK))
			{
				GrossWeightVerifiedByAddress.OrganisationPKInfo.AddWarning(Res.GetString("f31b4cb6-63f1-465a-b85d-d9c917206c8d", "The 'VGM Verified By' party is not approved to use Method 2. Press F3 on the VGM Verified By organization code to view their Organization and check their status (Config tab > Registration Numbers & Codes tab, select Type = VGM)."));
			}
		}

		JobDocAddressRequirement grossWeightVerifiedByDocAddressRequirement;

		#endregion

		#region IImportExport Members

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(JC_JA_NKPortOfLoading, JC_JB_NKPortOfDischarge); }
		}

		#endregion

		#region IContainerLegParent Members

		public JobDocAddress GetConsigneeDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetConsigneeDeliveryDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetConsignorDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetConsignorPickupDocAddress
		{
			get { return null; }
		}

		public JobDocAddress GetArrivalCFSDocAddress
		{
			get
			{
				JobDocAddress result = null;

				if (Consol != null)
				{
					result = Consol.GetArrivalCFSDocAddress;
				}

				return result;
			}
		}

		public JobDocAddress GetArrivalCTODocAddress
		{
			get
			{
				JobDocAddress result = null;

				if (Consol != null)
				{
					result = Consol.GetArrivalCTODocAddress;
				}

				return result;
			}
		}

		public JobDocAddress GetArrivalContainerYardDocAddress
		{
			get
			{
				JobDocAddress result = null;
				ZGuid arrivalCYDAddressPK = JC_OA_ArrivalContainerYardAddress;

				if (arrivalCYDAddressPK.IsValid)
				{
					DocAddressType addressType = DocAddressType.ArrivalCYDAddress;
					result = JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, arrivalCYDAddressPK);
				}
				else if (Consol != null)
				{
					result = Consol.GetArrivalContainerYardDocAddress;
				}

				return result;
			}
		}

		public JobDocAddress GetDepartureCFSDocAddress
		{
			get
			{
				JobDocAddress result = null;

				if (Consol != null)
				{
					result = Consol.GetDepartureCFSDocAddress;
				}

				return result;
			}
		}

		public JobDocAddress GetDepartureCTODocAddress
		{
			get
			{
				JobDocAddress result = null;

				if (Consol != null)
				{
					result = Consol.GetDepartureCTODocAddress;
				}

				return result;
			}
		}

		public JobDocAddress GetDepartureContainerYardDocAddress
		{
			get
			{
				JobDocAddress result = null;
				ZGuid departureCYDAddressPK = JC_OA_DepartureContainerYardAddress;

				if (departureCYDAddressPK.IsValid)
				{
					DocAddressType addressType = DocAddressType.DepartureCYDAddress;
					result = JobDocAddress.GetOrCreateNonPersistantDocAddress(this, addressType, departureCYDAddressPK);
				}
				else if (Consol != null)
				{
					result = Consol.GetDepartureContainerYardDocAddress;
				}

				return result;
			}
		}

		#endregion

		#region IDocAddresses Members

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
					{
						RegisterEditableChildObject(fDocAddresses);
					}
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		public SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return null;
		}

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		public IReadOnlyList<DocAddressType> SupportedAddressTypes
		{
			get { return new DocAddressType[] { DocAddressType.GrossWeightVerifiedBy }; }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region IPRAContainerMessaging Members

		[UniversalCopyCollectionEntity(EDIMessageSchema.Constants.TableName, EDIMessageSchema.Constants.EM_LinkUniqueID, OverrideCollectionName = "PRAMessages")]
		public PRAMessageCollection PRAMessages
		{
			get
			{
				if (praMessages == null)
				{
					praMessages = new PRAMessageCollection(this, Factory);
					praMessages.Load();
					praMessages.IsManagedForDataRefresh = true;
				}

				return praMessages;
			}
		}
		PRAMessageCollection praMessages;

		public ZString CurrentPRAStatus
		{
			get { return (currentPRAStatus ?? (currentPRAStatus = new CachedProperty<ZString>(Factory, this.GetCurrentPRAStatus))).Value; }
		}
		CachedProperty<ZString> currentPRAStatus;

		public ZPropertyInfo CurrentPRAStatusInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentPRAStatus); }
		}

		public bool IsWaitingForPRAResponse
		{
			get { return (isWaitingForPRAResponse ?? (isWaitingForPRAResponse = new CachedProperty<bool>(Factory, this.GetIsWaitingForPRAResponse))).Value; }
		}
		CachedProperty<bool> isWaitingForPRAResponse;

		public bool LastPRAMessageSentWasCancellation
		{
			get { return (lastPRAMessageSentWasCancellation ?? (lastPRAMessageSentWasCancellation = new CachedProperty<bool>(Factory, this.GetLastPRAMessageSentWasCancellation))).Value; }
		}
		CachedProperty<bool> lastPRAMessageSentWasCancellation;

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					messages.Load();
					messages.Sort(EDIMessageSchema.EM_SystemCreateTimeUtc.Name, ListSortDirection.Ascending);
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		public ComTracMessageCollection ComTracMessages
		{
			get
			{
				if (comTracMessages == null)
				{
					comTracMessages = new ComTracMessageCollection(this, Factory);
					comTracMessages.Load();
					comTracMessages.IsManagedForDataRefresh = true;
				}

				return comTracMessages;
			}
		}
		ComTracMessageCollection comTracMessages;

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		bool fIsImportingData;

		#endregion

		#region FillWithValidTestData
#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (collectionProperty.Name != "PackLines" && collectionProperty.Name != "ContainerLegs") //ContainerLegs use DeliveryLegs & PickupLegs
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}
		}

#endif
		#endregion

		#region IContainerForBinding Members

		public CommonContainer JobContainer
		{
			get { return this; }
		}

		public ZString ContainerNumberForBinding
		{
			get { return JC_ContainerNum; }
			set { JC_ContainerNum = value; }
		}

		public ZPropertyInfo ContainerNumberForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ContainerNumberForBinding), x => JC_ContainerNumInfo); }
		}

		public ZString SealNumberForBinding
		{
			get { return JC_SealNum; }
			set { JC_SealNum = value; }
		}

		public ZPropertyInfo SealNumberForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SealNumberForBinding), x => JC_SealNumInfo); }
		}

		[List("JobContainer.RefContainer_List")]
		public ZGuid RCForBinding
		{
			get { return JC_RC; }
			set { JC_RC = value; }
		}

		public ZPropertyInfo RCForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RCForBinding), x => JC_RCInfo); }
		}

		[List("JobContainer.ContainerMode_ListForBinding")]
		public ZString ContainerModeForBinding
		{
			get { return JC_ContainerMode; }
			set { JC_ContainerMode = value; }
		}

		public ZPropertyInfo ContainerModeForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ContainerModeForBinding), x => JC_ContainerModeInfo); }
		}

		[List("JobContainer.DeliveryMode_ListForBinding")]
		public ZString DeliveryModeForBinding
		{
			get
			{
				if (string.IsNullOrEmpty(deliveryModeForBinding))
				{
					deliveryModeForBinding = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.ConvertToUserDefinedCode(JC_DeliveryMode);
				}

				return deliveryModeForBinding;
			}
			set
			{
				deliveryModeForBinding = value;
				var code = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.ConvertToCode(value);
				JC_DeliveryMode = string.IsNullOrWhiteSpace(code) ? value : code;
			}
		}

		ZString deliveryModeForBinding;

		public ZPropertyInfo DeliveryModeForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DeliveryModeForBinding), x => JC_DeliveryModeInfo); }
		}

		[ReadOnly(true)]
		public ZDecimal GoodsWeightForBinding
		{
			get
			{
				var result = Constants.Weight.Convert(JC_Calc_TotalWeight, JC_Calc_TotalWeightUnit, ContainerWeightUnit, false);
				return this.GetRoundedValue(GoodsWeightForBindingInfo, result);
			}

			set { GoodsWeightForBindingInfo.RefreshBinding(); }
		}

		public ZPropertyInfo GoodsWeightForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(GoodsWeightForBinding)); }
		}

		[List("JobContainer.TotalWeightUnit_List")]
		public ZString WeightUnitForBinding
		{
			get { return JC_GrossWeightUQ; }
			set { JC_GrossWeightUQ = value; }
		}

		public ZPropertyInfo WeightUnitForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(WeightUnitForBinding), x => JC_GrossWeightUQInfo); }
		}

		public CodeDescriptionPairList ContainerMode_ListForBinding
		{
			get { return JC_ContainerMode_List; }
		}

		public ICodeDescriptionPairList DeliveryMode_ListForBinding
		{
			get { return UserDefinedDeliveryMode_List; }
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkFlowTypeCore; }
		}

		protected virtual ZString WorkFlowTypeCore
		{
			get { return WorkflowDescriptors.ContainerWorkflowDescriptorCode; }
		}

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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(NewWorkflowItemsCollection);
					if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
					{
						RegisterEditableChildObject(workflowItems);
					}
					workflowItems.IsManagedForDataRefresh = true;
				}
				return workflowItems;
			}
		}
		ContainerProcessTaskCollection workflowItems;

		protected virtual ContainerProcessTaskCollection NewWorkflowItemsCollection()
		{
			return new ContainerProcessTaskCollection(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return GetTemplateSelectionCriteriaCore();
		}

		protected virtual IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, ParentLoadPort, ParentLoadPort.SubstringSafe(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, ParentDischargePort, ParentDischargePort.SubstringSafe(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GB, GlbBranch.CurrentBranch.PK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, ContainerParent != null ? ContainerParent.TransportMode : ZString.Empty, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, JC_ContainerMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, ContainerParent != null && ContainerParent.ShippingLine != null ? ContainerParent.ShippingLine.PK : ZGuid.Empty, ZGuid.Empty);

			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		public void ProposeWorkflowRelationships()
		{
			if (!IsDeleted && JC_JK.IsValid)
			{
				var fromEntity = this as IWorkflowProviderCore;
				var toEntity = Consol as IWorkflowProviderCore;

				if (fromEntity != null && toEntity != null)
				{
					ObjectFactory.Get<IWorkflowProvidersLinkageService>().WorkflowProvidersLinked(fromEntity, toEntity, Factory);
				}
			}
		}

		#endregion

		#region INoteSource Members

		public ZString NoteSourceName
		{
			get { return DataBoundResourceStrings.GetTableDescriptiveName(JobContainerSchema.Constants.TableName) + ": " + JC_ContainerNum; }
		}

		#endregion

		#region IContainerStorageDataProvider Members

		ZString IContainerStorageDataProvider.ContainerNumber
		{
			get { return JC_ContainerNum; }
		}

		IJobInvoicingPlugIn[] IContainerStorageDataProvider.OperationsJobs
		{
			get
			{
				return ParentShipmentsCached.ToArray();
			}
		}

		ZDateTime IContainerStorageDataProvider.PickupDate
		{
			get { return JC_ArrivalSlotDateTime; }
		}

		StmALog IContainerStorageDataProvider.CreateEditLog(string reference)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			return Logs.AddNew(Events.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		#endregion

		#region ICreditControlledDocumentDelivery Members

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { getDocumentLogin += value; }
			remove { getDocumentLogin -= value; }
		}
		event EventHandler<SecurityLoginEventArgs> getDocumentLogin;

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get
			{
				return IsDPSFreightMovementRestrictedCore();
			}
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return GetScreeningPartiesCore();
		}

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				var list = new List<OrgHeader>();

				if (LinkedShipment != null)
				{
					list.AddRange(((ICreditControlledDocumentDelivery)LinkedShipment).OrganisationsForCreditChecks);
				}
				else if (Consol != null)
				{
					list.AddRange(((ICreditControlledDocumentDelivery)Consol).OrganisationsForCreditChecks);
					foreach (CommonShipment shipment in ParentShipmentsCached)
					{
						list.AddRange(((ICreditControlledDocumentDelivery)shipment).OrganisationsForCreditChecks);
					}
				}

				return list.Distinct(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer).ToArray();
			}
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (getDocumentLogin != null)
			{
				getDocumentLogin(this, e);
			}
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("CDCDD04B-1C68-4E51-9A25-19BB6A75BF26", "Sending Agent, Receiving Agent of the associated Consol or Consignee, Consignor, Local Client or any Debtors in the linked Shipment"); }
		}

		protected virtual bool IsDPSFreightMovementRestrictedCore()
		{
			bool result = false;

			if (LinkedShipment != null)
			{
				result = ((ICreditControlledDocumentDelivery)LinkedShipment).IsDPSFreightMovementRestricted;
			}
			else if (Consol != null)
			{
				result = ((ICreditControlledDocumentDelivery)Consol).IsDPSFreightMovementRestricted;
			}

			return result;
		}

		ScreeningParty[] GetScreeningPartiesCore()
		{
			ScreeningParty[] parties;

			if (LinkedShipment != null)
			{
				parties = ((ICreditControlledDocumentDelivery)LinkedShipment).GetScreeningParties();
			}
			else if (Consol != null)
			{
				parties = ((ICreditControlledDocumentDelivery)Consol).GetScreeningParties();
			}
			else
			{
				parties = Array.Empty<ScreeningParty>();
			}

			return parties;
		}

		#endregion

		#region IRelatedJobNumber Members

		string[] IRelatedJobNumber.JobNumber
		{
			get
			{
				return JobNumberCore();
			}
		}

		protected virtual string[] JobNumberCore()
		{
			if (LinkedShipment != null)
			{
				return ((ICreditControlledDocumentDelivery)LinkedShipment).JobNumber;
			}
			else if (Consol != null)
			{
				return ((ICreditControlledDocumentDelivery)Consol).JobNumber;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region IJobNumberForWorkflow

		string IJobNumberForWorkflow.JobNumber
		{
			get
			{
				string result = string.Empty;

				if (!JC_ContainerCode.IsEmpty)
				{
					result = JC_ContainerCode;
				}
				else
				{
					result = ((IJobNumber)this).JobNumber;
				}

				return result;
			}
		}

		#endregion

		#region IJobNumber

		public string JobNumber
		{
			get { return JC_ContainerJobID; }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return TransportModeForDefaultNumberOfDecimals; }
		}

		protected virtual ZString TransportModeForDefaultNumberOfDecimals
		{
			get
			{
				switch (Transport)
				{
					case Constants.TransportModes.AirSea:
						return Constants.TransportModes.Air;

					case Constants.TransportModes.SeaAir:
						return Constants.TransportModes.Sea;

					default:
						return Transport;
				}
			}
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return ShouldApplyDefaultNumberOfDecimalsRegistry
				? GetDefaultNumberOfDecimalsCore(property)
				: GetDecimalPlacesMetaDataIgnoringRegistry(property);
		}

		protected virtual int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			return GetUnitOfMeasureCore(property);
		}

		protected virtual ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.JC_GrossWeight:
				case Schema.JC_Calc_NetWeight:
				case Schema.JC_TareWeight:
				case Schema.JC_DunnageWeight:
					unitOfMeasure = JC_GrossWeightUQ;
					break;

				case Schema.JC_GrossVolume:
					unitOfMeasure = JC_GrossVolumeUQ;
					break;

				case Schema.JC_VolumeCapacity:
					unitOfMeasure = JC_VolumeCapacityUQ;
					break;

				case Schema.JC_WeightCapacity:
					unitOfMeasure = JC_WeightCapacityUQ;
					break;

				case Schema.JC_Calc_ActualGrossWeightInKgs:
				case Schema.JC_Calc_TotalWeightInKgs:
					unitOfMeasure = Constants.Weight.Kilograms;
					break;

				case Schema.JC_Calc_TotalVolume:
					unitOfMeasure = JC_Calc_TotalVolumeUnit;
					break;

				case Schema.JC_Calc_TotalVolumeInM3:
				case Schema.JC_Calc_ActualCapacity:
				case Schema.JC_Calc_ContainerCapacity:
					unitOfMeasure = Constants.Volume.CubicMetres;
					break;

				case Schema.JC_Calc_TotalWeight:
					unitOfMeasure = JC_Calc_TotalWeightUnit;
					break;

				case Schema.JC_Calc_MaxGrossWeight:
				case Schema.JC_Calc_TareWeight:
				case Schema.GoodsWeightForBinding:
					unitOfMeasure = ContainerWeightUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public ZDecimal GetRoundedValue(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return ShouldApplyDefaultNumberOfDecimalsRegistry
				? GetRoundedValueCore(column, property, value)
				: value;
		}

		public ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return ShouldApplyDefaultNumberOfDecimalsRegistry
				? GetRoundedValueCore(null, property, value)
				: value;
		}

		protected virtual ZDecimal GetRoundedValueCore(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForFreight.GetRoundedValue(this, column, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			RoundMeasurePropertiesOnTransportModeChangedCore();
		}

		protected virtual void RoundMeasurePropertiesOnTransportModeChangedCore()
		{
			this.SetRoundedValue(JobContainerSchema.JC_TareWeight, JC_TareWeightInfo);
			this.SetRoundedValue(JobContainerSchema.JC_DunnageWeight, JC_DunnageWeightInfo);

			this.SetRoundedValue(JobContainerSchema.JC_GrossWeight, JC_GrossWeightInfo);
			this.SetRoundedValue(JobContainerSchema.JC_GrossVolume, JC_GrossVolumeInfo);

			this.SetRoundedValue(JobContainerSchema.JC_VolumeCapacity, JC_VolumeCapacityInfo);
			this.SetRoundedValue(JobContainerSchema.JC_WeightCapacity, JC_WeightCapacityInfo);

			if (confirms != null)
			{
				foreach (CommonPickupDeliveryConfirm pickupDeliveryConfirm in confirms)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = pickupDeliveryConfirm;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		public bool ShouldApplyDefaultNumberOfDecimalsRegistry
		{
			get { return IsConsolContainer || !JC_JS_FCLBookingOnlyLink.IsEmpty; }
		}

		public virtual int GetDecimalPlacesMetaDataIgnoringRegistry(PropertyDescriptor property)
		{
			int result = (int)MetaData.GetMetaData(this, property, MetaDataTypes.DecimalPlaces, true);

			if (result < 0)
			{
				result = DefaultNumberOfDecimalsSupporterHelperForFreight.GetNumberOfDecimalsFromDatabaseSchema(this, property);
			}

			return (result < 0) ? 0 : result;
		}

		#endregion

		#region IEventDatePropertyChecker

		bool IEventDatePropertyChecker.CanUpdateProperty(IStmALog log, ZPropertyInfo property)
		{
			bool? canUpdate = null;

			if (ShouldCheckIfCanUpdatePropertyForSpecialCases())
			{
				canUpdate = CanUpdatePropertyForSpecialCases(log, property);
			}

			return canUpdate.GetValueOrDefault(IsEventFacilityMatched(log) && IsEventLocationMatched(log));
		}

		protected virtual bool ShouldCheckIfCanUpdatePropertyForSpecialCases()
		{
			return false;
		}

		#region CanUpdateFacilitySpecificEvent

		bool? CanUpdatePropertyForSpecialCases(IStmALog log, ZPropertyInfo property)
		{
			var eventType = log.SL_SE_NKEvent;
			if (eventType == Events.DehireCode
				&& property != null
				&& property.Name == JobContainerSchema.Constants.JC_ContainerYardEmptyReturnGateIn)
			{
				return true;
			}

			bool? isUpdatable = null;

			string facility;
			log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out facility);

			if (property != null && !facility.IsNullOrEmpty())
			{
				string eventLocation;
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out eventLocation);

				switch (property.Name)
				{
					case JobContainerSchema.Constants.JC_FCLWharfGateIn:
						isUpdatable = eventType == AutoEvents.GateInCode
							&& IsTerminalCode(facility)
							&& !eventLocation.IsNullOrEmpty()
							&& eventLocation == GetEventLocationForTerminal(eventType);
						break;

					case JobContainerSchema.Constants.JC_FCLWharfGateOut:
						isUpdatable = eventType == AutoEvents.GateOutCode
							&& IsTerminalCode(facility)
							&& !eventLocation.IsNullOrEmpty()
							&& eventLocation == GetEventLocationForTerminal(eventType);
						break;

					case JobContainerSchema.Constants.JC_ContainerYardEmptyReturnGateIn:
						isUpdatable = eventType == AutoEvents.GateInCode
							&& IsContainerYardCode(facility)
							&& (eventLocation.IsNullOrEmpty()
								|| eventLocation == GetEventLocationForContainerYard(eventType));
						break;

					case JobContainerSchema.Constants.JC_ContainerYardEmptyPickupGateOut:
						isUpdatable = eventType == AutoEvents.GateOutCode
							&& IsContainerYardCode(facility);
						break;
				}
			}

			return isUpdatable;
		}

		public ZString GetEventLocationForTerminal(ZString eventCode)
		{
			var result = ZString.Empty;

			switch (eventCode)
			{
				case Events.GateInCode:
					result = FreightEventsHelper.GetLoadPort(ContainerParent);
					break;
				case Events.GateOutCode:
					result = FreightEventsHelper.GetDischargePort(ContainerParent);
					break;
			}
			return result;
		}

		public ZString GetEventLocationForContainerYard(ZString eventCode)
		{
			switch (eventCode)
			{
				case Events.GateInCode:
				case Events.DehireCode:
					return ArrivalContainerYardAddress != null ? ArrivalContainerYardAddress.OA_RL_NKRelatedPortCode : FreightEventsHelper.GetDischargePort(ContainerParent);

				case Events.GateOutCode:
					return DepartureContainerYardAddress != null ? DepartureContainerYardAddress.OA_RL_NKRelatedPortCode : FreightEventsHelper.GetLoadPort(ContainerParent);

				default:
					return ZString.Empty;
			}
		}

		bool IsTerminalCode(string stringToCheck)
		{
			return !string.IsNullOrEmpty(stringToCheck) && stringToCheck.Equals(EventConstants.Facilities.Code.Terminal, StringComparison.OrdinalIgnoreCase);
		}

		bool IsContainerYardCode(string stringToCheck)
		{
			return !string.IsNullOrEmpty(stringToCheck) && stringToCheck.Equals(EventConstants.Facilities.Code.ContainerYard, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		protected virtual bool IsEventFacilityMatched(IStmALog log)
		{
			if (log.SL_SE_NKEvent == Events.FreightLoadedCode || log.SL_SE_NKEvent == Events.FreightUnloadedCode)
			{
				string facilityInEvent;
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out facilityInEvent);

				return string.IsNullOrEmpty(facilityInEvent) || facilityInEvent == EventConstants.Facilities.Code.Terminal;
			}

			if (log.SL_SE_NKEvent == AutoEvents.GateInCode || log.SL_SE_NKEvent == Events.GateOutCode)
			{
				string facilityInEvent;
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Facility, out facilityInEvent);

				return IsTerminalCode(facilityInEvent) || IsContainerYardCode(facilityInEvent);
			}

			return true;
		}

		protected virtual bool IsEventLocationMatched(IStmALog log)
		{
			if (log.SL_SE_NKEvent == Events.FreightLoadedCode || log.SL_SE_NKEvent == Events.FreightUnloadedCode)
			{
				var parameters = GetParametersForEvent(Events.All[log.SL_SE_NKEvent]);

				string containerLocation;
				string locationInEvent;

				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out locationInEvent);
				parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out containerLocation);

				return (containerLocation ?? string.Empty) == (locationInEvent ?? string.Empty);
			}

			return true;
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandlers == null)
				{
					uniqueIndexFailureHandlers = new List<IUniqueIndexFailureHandler>();
					uniqueIndexFailureHandlers.Add(new CommonContainerNumberFountainUniqueIndexHandler(this));
					uniqueIndexFailureHandlers.Add(new CommonContainerUniqueContainerNumIndexFailureHandler(this));
				}

				return uniqueIndexFailureHandlers;
			}
		}
		List<IUniqueIndexFailureHandler> uniqueIndexFailureHandlers;

		#endregion

		#region EnterpriseBusinessObject

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			if (log.SL_SE_NKEvent == Events.CargoAvailable.Code)
			{
				ProcessCAVEvent(log);
			}
			else if (log.SL_SE_NKEvent == Events.StatusUpdated.Code)
			{
				ProcessSTUEvent(log);
			}
		}

		void ProcessCAVEvent(IStmALog log)
		{
			var eventLocation = log.Parameters.GetValueOrDefault(Params.Location);
			var eventFacitlity = log.Parameters.GetValueOrDefault(Params.Facility);
			var containerLocation = GetCAVEventLocation();

			if (StringComparer.OrdinalIgnoreCase.Compare(eventLocation, containerLocation) == 0)
			{
				if (string.IsNullOrEmpty(eventFacitlity))
				{
					using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
					{
						log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = eventFacitlity = EventConstants.Facilities.Code.Terminal;
					}
				}

				if (StringComparer.OrdinalIgnoreCase.Compare(eventFacitlity, EventConstants.Facilities.Code.Terminal) == 0)
				{
					JC_FCLAvailable = log.SL_EventTime;
				}
				else if (StringComparer.OrdinalIgnoreCase.Compare(eventFacitlity, EventConstants.Facilities.Code.Depot) == 0)
				{
					JC_LCLAvailable = log.SL_EventTime;
				}
			}
		}

		void ProcessSTUEvent(IStmALog log)
		{
			var eventType = log.Parameters.GetValueOrDefault(Params.Type);
			var eventFacitlity = log.Parameters.GetValueOrDefault(Params.Facility);

			if (StringComparer.OrdinalIgnoreCase.Compare(eventType, Constants.EventReferenceParameterTypes.LastFreeDay) == 0
				&& StringComparer.OrdinalIgnoreCase.Compare(eventFacitlity, EventConstants.Facilities.Code.Terminal) == 0)
			{
				JC_ArrivalCTOStorageStartDate = log.SL_EventTime.AddHours(24);
			}
		}

		#endregion

		#region Events

		void LogEvents()
		{
			LogCAVEvents();
			LogQTVEvents();
		}

		void LogCAVEvents()
		{
			if (JC_ContainerMode == Constants.ContainerModes.Groupage)
			{
				if (JC_OverrideFCLAvailableStorage && (JC_FCLAvailableInfo.HasChanges || !IsInDatabase))
				{
					var ctoParameters = new Dictionary<string, string>();
					ctoParameters[Params.Location] = GetCAVEventLocation();
					ctoParameters[Params.Facility] = EventConstants.Facilities.Code.Terminal;

					// Assuming that JC_FCLAvailable is local to the current timezone.
					Logs.CreateRecreateOrUpdateEventLog(Events.CargoAvailable, EstimateActual.Actual, JC_FCLAvailable.ToOffset(), GetReferenceFreeTextForEvent(Events.CargoAvailable.Code), ctoParameters.ToArray());
				}

				if (JC_OverrideLCLAvailableStorage && (JC_LCLAvailableInfo.HasChanges || !IsInDatabase))
				{
					var cfsParameters = new Dictionary<string, string>();
					cfsParameters[Params.Location] = GetCAVEventLocation();
					cfsParameters[Params.Facility] = EventConstants.Facilities.Code.Depot;

					// Assuming that JC_LCLAvailable is local to the current timezone.
					Logs.CreateRecreateOrUpdateEventLog(Events.CargoAvailable, EstimateActual.Actual, JC_LCLAvailable.ToOffset(), GetReferenceFreeTextForEvent(Events.CargoAvailable.Code), cfsParameters.ToArray());
				}
			}
			else if ((IsArrivalContainerModeFCLorULD && JC_OverrideFCLAvailableStorage && (JC_FCLAvailableInfo.HasChanges || !IsInDatabase))
				|| (!IsArrivalContainerModeFCLorULD && JC_OverrideLCLAvailableStorage && (JC_LCLAvailableInfo.HasChanges || !IsInDatabase)))
			{
				var eventParameters = GetParametersForEvent(Events.CargoAvailable).ToArray();
				string eventFreeTextReference = GetReferenceFreeTextForEvent(Events.CargoAvailable.Code);

				// Assuming that AvailableDate is local to the current timezone.
				Logs.CreateRecreateOrUpdateEventLog(Events.CargoAvailable, EstimateActual.Actual, AvailableDate.ToOffset(), eventFreeTextReference, eventParameters);
			}
		}

		void LogQTVEvents()
		{
			if (JC_GrossWeightVerificationTypeInfo.HasChanges || (!IsInDatabase && !JC_GrossWeightVerificationType.IsEmpty))
			{
				Logs.Find(l => l.SL_SE_NKEvent == Events.QuantityVerified.Code).ForEach(x => x.Cancel());

				if (IsGrossWeightVerified)
				{
					Logs.CreateOrRecreateEventLog(Events.QuantityVerified, EstimateActual.Actual, JC_GrossWeightVerificationDateTime.ToOffset(), ZString.Empty, new KeyValuePair<string, string>(Params.Type, (NoResString)"Verified Gross Mass"), new KeyValuePair<string, string>(Params.Department, GrossWeightVerifiedByAddress.E2_CompanyName), new KeyValuePair<string, string>(Params.Reason, Lookups.GrossWeightVerificationTypeList.GetDescriptionFromCode(JC_GrossWeightVerificationType)));
				}
				else if (GrossWeightVerificationNotRequired)
				{
					Logs.CreateOrRecreateEventLog(Events.QuantityVerified, EstimateActual.Actual, JC_GrossWeightVerificationDateTime.ToOffset(), ZString.Empty, new KeyValuePair<string, string>(Params.Type, (NoResString)"Verified Gross Mass"), new KeyValuePair<string, string>(Params.Reason, Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotRequired));
				}
			}
		}

		void LogErrors()
		{
			if (stackTraceForJC_GrossWeightOutOfRange.Length != 0
				&& !JC_GrossWeight.IsWithinSqlPrecisionAndScale(JobContainerSchema.JC_GrossWeight.Precision, JobContainerSchema.JC_GrossWeight.Scale))
			{
				ErrorReporter.ReportOnce("CommonContainer_NotWithinSqlPrecisionAndScale", stackTraceForJC_GrossWeightOutOfRange.ToString());
			}

			stackTraceForJC_GrossWeightOutOfRange.Clear();
		}

		protected void LogEvent(Event eventType, ZDateTimeOffset dateTime)
		{
			var eventParameters = GetParametersForEvent(eventType).ToArray();
			string eventFreeTextReference = GetReferenceFreeTextForEvent(eventType.Code);

			Logs.CreateOrRecreateEventLog(eventType, EstimateActual.Actual, dateTime, eventFreeTextReference, eventParameters);
		}

		protected void LogEvent(Event eventToLog, ZDateTimeOffset dateTime, ZString facility)
		{
			if (ShouldCheckIfCanUpdatePropertyForSpecialCases())
			{
				if (IsTerminalCode(facility) || IsContainerYardCode(facility))
				{
					var parameters = new Dictionary<string, string>();
					parameters[EventConstants.EventReferenceParameters.Codes.Facility] = facility;

					var location = IsTerminalCode(facility)
						? GetEventLocationForTerminal(eventToLog.Code)
						: GetEventLocationForContainerYard(eventToLog.Code);

					if (!location.IsEmpty)
					{
						parameters[EventConstants.EventReferenceParameters.Codes.Location] = location;
					}

					Logs.CreateOrRecreateEventLog(eventToLog, EstimateActual.Actual, dateTime, ZString.Empty, parameters.ToArray());
				}
			}
		}

		protected virtual string GetReferenceFreeTextForEvent(string eventCode)
		{
			return null;
		}

		public virtual IDictionary<string, string> GetParametersForEvent(Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType == Events.CargoAvailable)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = GetCAVEventLocation();
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = IsArrivalContainerModeFCLorULD ? EventConstants.Facilities.Code.Terminal
																												: EventConstants.Facilities.Code.Depot;
			}
			else if (eventType == Events.FreightLoaded)
			{
				var loadPort = FreightEventsHelper.GetLoadPort(ContainerParent);
				var loadTransportMode = FreightEventsHelper.GetLoadTransportMode(ContainerParent);
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = string.IsNullOrEmpty(loadPort) ? null : loadPort;
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[EventConstants.EventReferenceParameters.Codes.Mode] = string.IsNullOrEmpty(loadTransportMode) ? null : loadTransportMode;
			}
			else if (eventType == Events.FreightUnloaded)
			{
				var dischargePort = FreightEventsHelper.GetDischargePort(ContainerParent);
				var dischargeTransportMode = FreightEventsHelper.GetDischargeTransportMode(ContainerParent);
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = string.IsNullOrEmpty(dischargePort) ? null : dischargePort;
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[EventConstants.EventReferenceParameters.Codes.Mode] = string.IsNullOrEmpty(dischargeTransportMode) ? null : dischargeTransportMode;
			}

			return parameters;
		}

		string GetCAVEventLocation()
		{
			if (ContainerParent != null)
			{
				var lastTransport = new TransportOrderHelper(ContainerParent.Transports).LastLeg;
				return lastTransport != null ? lastTransport.JW_RL_NKDiscPort : ParentDischargePort;
			}

			return null;
		}

		public Decimal GetCalculatedVolumeWeight(ZString loadedWeightUnit, ZString loadedVolumeUnit, Decimal loadedWeight, Decimal loadedVolume)
		{
			var conversionFactor = GetContainerConversionFactor();
			if (conversionFactor.IsEmpty
			|| loadedWeightUnit.IsEmpty
			|| loadedVolumeUnit.IsEmpty
			|| !Constants.Weight.ContainsCode(loadedWeightUnit)
			|| !Constants.Volume.ContainsCode(loadedVolumeUnit))
			{
				return 0m;
			}

			var quantityToConvert = IsContainerChargeableByWeight()
				? new ZVolume(loadedVolume, loadedVolumeUnit)
				: (IQuantity)new ZWeight(loadedWeight, loadedWeightUnit);

			return quantityToConvert.Convert(
				IsContainerChargeableByWeight() ? loadedWeightUnit : loadedVolumeUnit,
				new[] { conversionFactor },
				true).Amount;
		}

		public bool IsContainerChargeableByWeight()
		{
			return Constants.Weight.ContainsCode(JC_TotalUnitOfMeasure);
		}

		public ConversionFactor GetContainerConversionFactor()
		{
			if (Consol == null)
			{
				return ConversionFactor.Empty;
			}
			var chargeableFactor = ChargeableFactor.GetDefault(
				Consol.IsDomesticFreight ? ChargeableFactorSource.Domestic : ChargeableFactorSource.International, Transport);
			if (chargeableFactor == null)
			{
				return ConversionFactor.Empty;
			}

			var isImperial = IsContainerChargeableByWeight()
				? Constants.Weight.IsImperial(JC_TotalUnitOfMeasure)
				: Constants.Volume.IsImperial(JC_TotalUnitOfMeasure);

			return isImperial ? chargeableFactor.ImperialFactor : chargeableFactor.MetricFactor;
		}

		#endregion

		#region ICusEntryNumberValidationDeciderOfType

		Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType()
		{
			return typeof(CommonContainerAdditionalRefEntryNumValidation);
		}

		#endregion

		#region AdditionalReferenceNumbers

		[ChildEditable()]
		public CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					additionalReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					additionalReferenceNumbers.Load();
					RegisterEditableChildObject(additionalReferenceNumbers);
					OnNumbersLoaded();
				}

				return additionalReferenceNumbers;
			}
		}
		CusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		public ZString AdditionalReferenceNumbersAsString => AdditionalReferenceNumbers.AllNumbersAsString;

		protected virtual void OnNumbersLoaded()
		{
			foreach (var cusEntryNumber in AdditionalReferenceNumbers.OfType<CusEntryNumber>())
			{
				if (AdditionalReferenceNumberCannotBeDeleted(cusEntryNumber))
				{
					AddCannotDeleteNumberHandler(cusEntryNumber, ResString.GetMultilingualString("AAB2B8F8-142C-45C7-9F2D-612EBEB6605E", "The {0} is system generated and cannot be deleted.", cusEntryNumber.CE_EntryType));
				}
			}
		}

		protected virtual bool AdditionalReferenceNumberCannotBeDeleted(CusEntryNumber cusEntryNumber)
		{
			return false;
		}

		protected void AddCannotDeleteNumberHandler(CusEntryNumber number, MultilingualString reasonForNotAbleToDelete)
		{
			if (number != null)
			{
				number.ReadOnly = true;
				number.CanDeleteHandler += (s, eventArgs) =>
				{
					eventArgs.CanDelete = false;
					eventArgs.ReasonForNotAbleToDelete = reasonForNotAbleToDelete;
				};
			}
		}

		#endregion

		#region IAdditionalReferenceNumberSupporter

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify)
		{
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
		{
			get { return true; }
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
			OnEntryNumChangedCore(additionalReferenceNumber);
		}

		protected virtual void OnEntryNumChangedCore(CusEntryNumber additionalReferenceNumber)
		{
		}

		CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
		{
			get { return AdditionalReferenceNumbers; }
		}

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "GetAdditionalReferenceNumberTypeList_CommonContainer_{0}_{1}", category, countryCode);
			return Factory.GetCachedValue(cacheKey, delegate
			{
				var result = new CodeDescriptionPairList();

				if (category == CusEntryNumber.Categories.AdditionalReferenceNumber)
				{
					result = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(countryCode);
					result.AddPairsIfNotExist(new ContainerNonCustomsAdditionalReferenceCodesCodeList().ToArray());
				}

				return result;
			});
		}

		#endregion

		#region IRoutingSupport

		RoutingCollection IRoutingSupport.TransportsIncludingRelated => RoutingSupportProvider?.TransportsIncludingRelated;

		TransportCollection IRoutingSupport.Transports => RoutingSupportProvider?.Transports;

		ZString IRoutingSupport.TransportMode => RoutingSupportProvider?.TransportMode ?? ZString.Empty;

		string IRoutingSupport.AdditionalETAUpdateMsg => RoutingSupportProvider?.AdditionalETAUpdateMsg;

		string IRoutingSupport.AdditionalETDUpdateMsg => RoutingSupportProvider?.AdditionalETDUpdateMsg;

		IRoutingSupport RoutingSupportProvider => (IRoutingSupport)ContainerParent;

		#endregion

		#region Universal Copy Handling

		protected void OnUniversalCopyFinish()
		{
			JC_ContainerJobID = ZString.Empty;
		}

		#endregion

		#region Import Penalties

		[ChildEditable(true)]
		public ImportContainerPenaltyCollection ImportPenalties
		{
			get
			{
				if (importPenalties == null)
				{
					importPenalties = new ImportContainerPenaltyCollection(this, Core.Constants.ContainerPenaltyProcessType.Import);
					RegisterEditableChildObject(importPenalties);
				}

				return importPenalties;
			}
		}
		ImportContainerPenaltyCollection importPenalties;

		#endregion

		#region Delivery Penalties

		public ImportContainerPenaltyCollection DeliveryPenalties
		{
			get
			{
				if (deliveryPenalties == null)
				{
					deliveryPenalties = new ImportContainerPenaltyCollection(this, Core.Constants.ContainerPenaltyProcessType.Delivery);
				}

				return deliveryPenalties;
			}
		}

		ImportContainerPenaltyCollection deliveryPenalties;

		#endregion

		#region Export Penalties

		[ChildEditable(true)]
		public ExportContainerPenaltyCollection ExportPenalties
		{
			get
			{
				if (exportPenalties == null)
				{
					exportPenalties = new ExportContainerPenaltyCollection(this, Core.Constants.ContainerPenaltyProcessType.Export);
					RegisterEditableChildObject(exportPenalties);
				}

				return exportPenalties;
			}
		}
		ExportContainerPenaltyCollection exportPenalties;

		#endregion

		#region Pickup Penalties

		public ExportContainerPenaltyCollection PickupPenalties
		{
			get
			{
				if (pickupPenalties == null)
				{
					pickupPenalties = new ExportContainerPenaltyCollection(this, Core.Constants.ContainerPenaltyProcessType.Pickup);
				}

				return pickupPenalties;
			}
		}

		ExportContainerPenaltyCollection pickupPenalties;

		#endregion

		#region Container Penalty

		#region ArrivalCarrierDetentionDays

		public ContainerPenalty FindArrivalCarrierDetentionPenalty()
			=> ImportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Detention,
					Constants.ContainerPenaltyCreditorType.Codes.Carrier);

		public ZByte ArrivalCarrierDetentionDays
		{
			get
			{
				return ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false)?.DurationAsDays ?? (ZByte)0;
			}
			set
			{
				ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(true, false).DurationAsDays = value;

				ArrivalCarrierDetentionDaysInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalCarrierDetentionDaysInfo => GetZPropertyInfo(nameof(ArrivalCarrierDetentionDays));

		#endregion

		#region ArrivalCarrierDetentionCost

		[DecimalPlaces(2)]
		public ZDecimal ArrivalCarrierDetentionCost
		{
			get
			{
				var penalty = ImportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Detention,
					Constants.ContainerPenaltyCreditorType.Codes.Carrier);
				return penalty?.CPY_PerUnitCost ?? 0m;
			}
			set
			{
				var penalty = ImportPenalties.FindOrCreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Detention,
					Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days);
				penalty.CPY_PerUnitCost = value;

				ArrivalCarrierDetentionCostInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalCarrierDetentionCostInfo => GetZPropertyInfo(nameof(ArrivalCarrierDetentionCost));

		#endregion

		#region ArrivalCTOStorageDays

		public ContainerPenalty FindArrivalCTOStoragePenalty() => ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false);

		public ZByte ArrivalCTOStorageDays
		{
			get
			{
				return ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false)?.DurationAsDays ?? (ZByte)0;
			}
			set
			{
				ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(true, false).DurationAsDays = value;

				ArrivalCTOStorageDaysInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalCTOStorageDaysInfo => GetZPropertyInfo(nameof(ArrivalCTOStorageDays));

		#endregion

		#region DepartureCTOStorageDays

		public ZByte DepartureCTOStorageDays
		{
			get
			{
				return ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(false)?.DurationAsDays ?? (ZByte)0;
			}
			set
			{
				ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(true, false).DurationAsDays = value;

				DepartureCTOStorageDaysInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DepartureCTOStorageDaysInfo => GetZPropertyInfo(nameof(DepartureCTOStorageDays));

		#endregion

		#region DepartureCARDetentionDays

		public ZByte DepartureCarrierDetentionDays
		{
			get
			{
				return ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false)?.DurationAsDays ?? (ZByte)0;
			}
			set
			{
				ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(true, false).DurationAsDays = value;

				DepartureCarrierDetentionDaysInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DepartureCarrierDetentionDaysInfo => GetZPropertyInfo(nameof(DepartureCarrierDetentionDays));

		#endregion
		#region ArrivalCTOStorageCost

		[DecimalPlaces(2)]
		public ZDecimal ArrivalCTOStorageCost
		{
			get
			{
				var penalty = ImportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Storage,
					Constants.ContainerPenaltyCreditorType.Codes.CTO);
				return penalty?.CPY_PerUnitCost ?? 0m;
			}
			set
			{
				var penalty = ImportPenalties.FindOrCreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.Storage,
					Constants.ContainerPenaltyCreditorType.Codes.CTO,
					timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days);
				penalty.CPY_PerUnitCost = value;

				ArrivalCTOStorageCostInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalCTOStorageCostInfo => GetZPropertyInfo(nameof(ArrivalCTOStorageCost));

		#endregion

		#region ArrivalTruckWaitTime

		[ZDateTimeDurationValueExclude1900]
		public ZDateTime ArrivalTruckWaitTime
		{
			get
			{
				var penalty = ImportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport);
				return penalty?.CPY_Duration ?? ZDateTime.Empty;
			}
			set
			{
				var penalty = ImportPenalties.FindOrCreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport,
					timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
					creditor: ContainerParent?.ArrivalUnpackCFSTransportAddress);
				penalty.CPY_Duration = value;

				ArrivalTruckWaitTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalTruckWaitTimeInfo => GetZPropertyInfo(nameof(ArrivalTruckWaitTime));

		#endregion

		#region ArrivalTruckWaitCost

		public ContainerPenalty FindArrivalTruckWaitPenalty()
			=> ImportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport);

		[DecimalPlaces(2)]
		public ZDecimal ArrivalTruckWaitCost
		{
			get
			{
				var penalty = ImportPenalties.FindContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport);
				return penalty?.CPY_PerUnitCost ?? 0m;
			}
			set
			{
				var penalty = ImportPenalties.FindOrCreateContainerPenalty(Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport,
					timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
					creditor: ContainerParent?.ArrivalUnpackCFSTransportAddress);
				penalty.CPY_PerUnitCost = value;

				ArrivalTruckWaitCostInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ArrivalTruckWaitCostInfo => GetZPropertyInfo(nameof(ArrivalTruckWaitCost));

		#endregion

		#endregion

		#region RelatedContainerLoadList

		[List("Lookups.RelatedContainerLoadListCollection")]
		public virtual ZGuid RelatedContainerLoadListPK => ZGuid.Empty;

		public ZPropertyInfo RelatedContainerLoadListPKInfo => GetZPropertyInfo(Schema.RelatedContainerLoadListPK);

		public virtual ZBool RelatedContainerLoadListVisible => ZBool.False;

		public virtual ZBool RelatedSupplierBookingVisible => ZBool.False;

		public virtual ZBool RelatedContainerLoadList_ReadOnly => ZBool.True;

		#endregion

		#region RelatedContainerLoadPlan

		public virtual bool JC_JSB_SupplierBooking_ReadOnly => ZBool.False;

		[List("Lookups.RelatedContainerLoadPlanCollection")]
		public override ZGuid JC_CLH_LoadListPlan
		{
			get => base.JC_CLH_LoadListPlan;
			set => base.JC_CLH_LoadListPlan = value;
		}

		public virtual ZBool RelatedContainerLoadPlanVisible => ZBool.False;

		public virtual bool JC_CLH_LoadListPlan_ReadOnly => ZBool.True;

		#endregion

		#region AddressAdditionalInfo

		[JobAddressAdditionalInfoAddressTypes(AutoDocAddressTypes.Codes.DepartureCYDAddress, AutoDocAddressTypes.Codes.ArrivalCYDAddress)]
		public IJobAddressAdditionalInfoCollection JobAddressAdditionalInfoCollection
		{
			get
			{
				if (jobAddressAdditionalInfoCollection == null)
				{
					jobAddressAdditionalInfoCollection = new JobAddressAdditionalInfoCollection(this, Factory);
					if (!IsDeleted && !IsDeleting)
					{
						this.HookEventsToDependentAddresses();
					}
					RegisterEditableChildObject(jobAddressAdditionalInfoCollection as JobAddressAdditionalInfoCollection);
				}
				return jobAddressAdditionalInfoCollection;
			}
		}

		IJobAddressAdditionalInfoCollection jobAddressAdditionalInfoCollection { get; set; }

		ZPropertyInfo IJobAddressAdditionalInfoSupport.GetDependentAddress(string addressType)
		{
			return addressType switch
			{
				AutoDocAddressTypes.Codes.DepartureCYDAddress => JC_OA_DepartureContainerYardAddressInfo,
				AutoDocAddressTypes.Codes.ArrivalCYDAddress => JC_OA_ArrivalContainerYardAddressInfo,
				_ => null
			};
		}

		[BusinessObjectTestExclude]
		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public virtual ZString EmptyPickupByTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.DepartureCYDAddress);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.DepartureCYDAddress, value);
		}

		public ZPropertyInfo EmptyPickupByTransportModeInfo => GetZPropertyInfo(Schema.EmptyPickupByTransportMode);

		public bool EmptyPickupByTransportMode_ReadOnly => JC_OA_DepartureContainerYardAddress.IsEmpty;

		[BusinessObjectTestExclude]
		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public virtual ZString EmptyReturnToTransportMode
		{
			get => this.GetTransportModeOrDefault(AutoDocAddressTypes.Codes.ArrivalCYDAddress);
			set => this.SetTransportMode(AutoDocAddressTypes.Codes.ArrivalCYDAddress, value);
		}

		public ZPropertyInfo EmptyReturnToTransportModeInfo => GetZPropertyInfo(Schema.EmptyReturnToTransportMode);

		public bool EmptyReturnToTransportMode_ReadOnly => JC_OA_ArrivalContainerYardAddress.IsEmpty;

		public ZGuid JobAddressAdditionalInfoParentID => PK;

		public ZString JobAddressAdditionalInfoTableCode => TablePrefix;

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSContainer : CommonContainer,
		Integration.CFS.ICFSContainer,
		ISendEmailSource,
		IJobInvoicingPlugIn,
		IRatingSupporter,
		ICartageContainer,
		IEDocsProvider
	{
		#region Schema

		public new class Schema : CommonContainer.Schema
		{
			public const string JC_TransportMode = "JC_TransportMode";

			public const string ContainerYardAddress = "ContainerYardAddress";
			public const string JC_PackUnpackDate = "JC_PackUnpackDate";
			public const string JC_Calc_ConsolBookingRef = "JC_Calc_ConsolBookingRef";

			public const string JC_OA_CTOAddress = "JC_OA_CTOAddress";
			public const string JC_CTOName = "JC_CTOName";
			public const string JC_ContainerYardName = "JC_ContainerYardName";

			public const string TotalShipmentPacks = "TotalShipmentPacks";
			public const string TotalShipmentWeight = "TotalShipmentWeight";
			public const string TotalShipmentVolume = "TotalShipmentVolume";
			public const string JC_LCLAvailable_Readonly = "JC_LCLAvailable_Readonly";
			public const string JC_LCLStorageCommences_Readonly = "JC_LCLStorageCommences_Readonly";

			public const string JC_ArrivalTime = "JC_ArrivalTime";
			public const string JC_ArrivalTruckDriversLicense = "JC_ArrivalTruckDriversLicense";
			public const string JC_ArrivalTruckDrivers = "JC_ArrivalTruckDrivers";
			public const string JC_GS_ArrivalDriver = "JC_GS_ArrivalDriver";
			public const string JC_ArrivalTruckRegistration = "JC_ArrivalTruckRegistration";
			public const string JC_DepartureTime = "JC_DepartureTime";
			public const string JC_DepartureTruckDriversLicense = "JC_DepartureTruckDriversLicense";
			public const string JC_DepartureTruckDrivers = "JC_DepartureTruckDrivers";
			public const string JC_GS_DepartureDriver = "JC_GS_DepartureDriver";
			public const string JC_DepartureTruckRegistration = "JC_DepartureTruckRegistration";
			public const string JC_OA_ArrivalTransportAddress = "JC_OA_ArrivalTransportAddress";
			public const string JC_OA_DepartureTransportAddress = "JC_OA_DepartureTransportAddress";

			public const string JC_ImpendingArrivalStatus = "JC_ImpendingArrivalStatus";
			public const string JC_ImpendingArrivalDate = "JC_ImpendingArrivalDate";
			public const string JC_CargoStatusAdviceStatus = "JC_CargoStatusAdviceStatus";
			public const string JC_CargoStatusAdviceDate = "JC_CargoStatusAdviceDate";
		}

		#endregion

		public CFSContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetupEventStuff();
		}

		public int JC_ArrivalTruckRegistration_MaxLength
		{
			get { return 8; }
		}

		public int JC_DepartureTruckRegistration_MaxLength
		{
			get { return 8; }
		}

		public int JC_DepartureTruckDriversLicense_MaxLength
		{
			get { return 15; }
		}

		public int JC_ArrivalTruckDriversLicense_MaxLength
		{
			get { return 15; }
		}

		#region BusinessObject Overrides

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JC_IsCFSRegistered = true;
			JC_TransportMode = Constants.TransportModes.Sea;
			JC_ContainerMode = Constants.ContainerModes.FCL;
		}

		#endregion

		#region Auto Logged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override bool ShouldCheckIfCanUpdatePropertyForSpecialCases()
		{
			return true;
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Consol

		public new CFSLoadListConsol Consol
		{
			get { return (CFSLoadListConsol)base.Consol; }
		}

		protected override CommonConsol LoadParentConsol()
		{
			return Factory.Load<CFSLoadListConsol>(JC_JK);
		}

		#endregion

		#region Servies

		[ChildEditable(true)]
		public new CFSServiceDependentCollection Services
		{
			get { return (CFSServiceDependentCollection)base.Services; }
		}

		protected override JobServiceDependentCollection GetNewServiceCollection()
		{
			return new CFSServiceDependentCollection(this, Factory);
		}

		#endregion

		#region PackLines

		[ChildEditable(true)]
		public new CFSPackLineManyToManyCollection PackLines
		{
			get { return (CFSPackLineManyToManyCollection)base.PackLines; }
		}

		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new CFSPackLineManyToManyCollection(this);
		}

		public override BusinessObject[] GetMasterUnallocatedPackLinesArray()
		{
			return (Consol != null) ? Consol.UnAllocatedPackLines.ToArray() : null;
		}

		public override UnAllocatedPackLinesView GetMasterUnallocatedPackLinesView()
		{
			return (Consol != null) ? Consol.UnAllocatedPackLines : null;
		}

		#endregion

		protected override CommonPickupDeliveryConfirm GetOrCreateConfirm(string pickupDeliveryType, bool isAllowToBeCreated)
		{
			var result = base.GetOrCreateConfirm(pickupDeliveryType, isAllowToBeCreated);
			if (!FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(Factory))
			{
				RegisterEditableChildObject(result);
			}
			return result;
		}

		#region CFSArrival

		public CommonPickupDeliveryConfirm CFSArrival
		{
			get { return this.IsExport() ? OriginCFSArrival : DestinationCFSArrival; }
		}

		#endregion

		#region CFSDispatch

		public CommonPickupDeliveryConfirm CFSDispatch
		{
			get { return this.IsExport() ? OriginCFSDeparture : DestinationCFSDeparture; }
		}

		#endregion

		protected override bool CanHaveRelatedDeclaration
		{
			get { return false; }
		}

		#endregion

		#region Document MenuTemplate Filter Helper Properties

		protected CFSShipmentDependentCollection Shipments
		{
			get { return PackUnpackShipments; }
		}

		public CFSShipmentDependentCollection PackUnpackShipments
		{
			get
			{
				if (fPackUnpackShipments == null)
				{
					fPackUnpackShipments = GetNewPackUnpackShipmentDependentCollection();
					fPackUnpackShipments.Load();
					fPackUnpackShipments.Sort(CFSShipment.Schema.JS_UniqueConsignRef, ListSortDirection.Ascending);
				}

				return fPackUnpackShipments;
			}
		}
		CFSShipmentDependentCollection fPackUnpackShipments;

		protected virtual CFSShipmentDependentCollection GetNewPackUnpackShipmentDependentCollection()
		{
			CFSShipmentDependentCollection collection = new CFSShipmentDependentCollection(Factory, this);
			collection.ValidateTotalsAgainstPackLines = false;
			collection.AllowSurplusPacks = false;
			return collection;
		}

		public ZBool ContainsImportShipments
		{
			get
			{
				if (Sailing != null && Shipments != null)
				{
					foreach (CFSShipment shipment in Shipments)
					{
						if (shipment.Destination != null && !shipment.Destination.Code.IsEmpty &&
						!Sailing.JX_JB_RL_NKPortOfDischarge.IsEmpty &&
						shipment.Destination.Code == Sailing.JX_JB_RL_NKPortOfDischarge)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public ZBool ContainsTranshipments
		{
			get
			{
				if (Sailing != null && Shipments != null)
				{
					foreach (CFSShipment shipment in Shipments)
					{
						if (shipment.Destination != null && !shipment.Destination.Code.IsEmpty &&
						!Sailing.JX_JB_RL_NKPortOfDischarge.IsEmpty &&
						shipment.Destination.Code != Sailing.JX_JB_RL_NKPortOfDischarge &&
						shipment.Destination.Code.SubstringSafe(0, 2) != Sailing.JX_JB_RL_NKPortOfDischarge.SubstringSafe(0, 2))
						{
							return ZBool.True;
						}
					}
				}
				return ZBool.False;
			}
		}

		public ZBool ContainsOnForwardingShipments
		{
			get
			{
				if (Sailing != null && Shipments != null)
				{
					foreach (CFSShipment shipment in Shipments)
					{
						if (shipment.Destination != null && !shipment.Destination.Code.IsEmpty &&
						!Sailing.JX_JB_RL_NKPortOfDischarge.IsEmpty &&
						shipment.Destination.Code != Sailing.JX_JB_RL_NKPortOfDischarge &&
						shipment.Destination.Code.SubstringSafe(0, 2) == Sailing.JX_JB_RL_NKPortOfDischarge.SubstringSafe(0, 2))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#endregion

		#region Property Overrides

		#region JC_Purpose

		[ReadOnly(true)]
		[List("JC_Purpose_List")]
		public override ZString JC_Purpose
		{
			get { return base.JC_Purpose; }
			set { base.JC_Purpose = value; }
		}

		#endregion

		#region JC_ContainerMode

		[List("JC_ContainerMode_List")]
		public override ZString JC_ContainerMode
		{
			get { return base.JC_ContainerMode; }
			set
			{
				if (value != JC_ContainerMode)
				{
					base.JC_ContainerMode = value;

					if (!IsMandatoryContainerType)
					{
						JC_RC = ZGuid.Empty;
					}

					PackUnpackShipments.SetContainerModeFromParent();

					if (Consol != null)
					{
						Consol.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region JC_OH_CFSClient

		[List("ClientOrgHeaderList")]
		public override ZGuid JC_OH_CFSClient
		{
			get { return base.JC_OH_CFSClient; }
			set
			{
				if (JC_OH_CFSClient != value)
				{
					if (!IsDefaultingFromLoadList)
					{
						if (Consol != null && Consol.JK_OH_Forwarder != value)
						{
							ContinueWithChanging = true;
							MessageCaption = Res.GetString("ebc24cb0-d7dc-4622-8628-223728c35fce", "Change Client");
							string message1 = "";
							string message2 = "";
							if (PackLines.Count > 0)
							{
								message1 = Res.GetString("418bbc1b-ffc8-4d30-9b60-06a979e5a6fe", "This container has been packed.") + "\r\n";
								message2 = Res.GetString("82bbea9a-eb20-4793-87bd-7d4821c8a426", "unpack the container,");
							}
							WarningMessage = message1 + Res.GetString("8934a304-8bed-479c-baaa-760ff0c5eb70", @"Changing client will {1} detach the container from the current load list and override the current client.
Do you want to continue?", message2);
							WarningMessage = "";
							if (!ContinueWithChanging)
							{
								return;
							}
							ContinueWithChanging = false;
							if (Consol.Containers.Contains(PK))
							{
								Consol.Containers.Remove(this);
							}
						}
					}

					base.JC_OH_CFSClient = value;

					if (!IsDefaultingFromLoadList)
					{
						if (Consol != null && Consol.JK_OH_Forwarder != JC_OH_CFSClient)
						{
							base.JC_JK = ZGuid.Empty;
						}
					}

					PackUnpackShipments.SetClient(value);
				}
			}
		}
		public bool ContinueWithChanging;

		#endregion

		#region JC_JK

		[List("LoadListConsol_List")]
		public override ZGuid JC_JK
		{
			get { return base.JC_JK; }
			set
			{
				if (JC_JK != value)
				{
					base.JC_JK = value;
					if (Consol != null)
					{
						JC_JX = ZGuid.Empty;
						IsDefaultingFromLoadList = true;
						try
						{
							JC_OH_CFSClient = Consol.JK_OH_Forwarder;
						}
						finally
						{
							IsDefaultingFromLoadList = false;
						}
					}

					if (Consol != null)
					{
						Consol.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region JC_JX

		[BusinessObjectTestExclude()]
		public override ZGuid JC_JX
		{
			get { return (Consol != null && Consol.Schedule != null) ? Consol.Schedule.PK : base.JC_JX; }
			set
			{
				if (Consol == null)
				{
					if (JC_JX != value)
					{
						base.JC_JX = value;

						JC_JA_NKPortOfLoadingInfo.RefreshBinding();
						JC_JB_NKPortOfDischargeInfo.RefreshBinding();
						JC_JV_NKVesselInfo.RefreshBinding();
						JC_JV_VoyageFlightInfo.RefreshBinding();
						JC_JA_E_DEPInfo.RefreshBinding();
						JC_JB_E_ARVInfo.RefreshBinding();
					}
				}
				else if (value.IsEmpty)
				{
					base.JC_JX = value;
				}
				else
				{
					ErrorReporter.ReportOnce("SettingTheSailingOnAContainerAlreadyOnALoadlist", "It is invalid to set the sailing on a Container once it is attached to a LoadList");
				}

				if (Consol != null)
				{
					Consol.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JC_TrainWagonNumber

		protected override bool JC_TrainWagonNumber_ReadOnly
		{
			get { return (JC_TransportMode != Constants.TransportModes.Rail) || base.JC_TrainWagonNumber_ReadOnly; }
		}

		#endregion

		#region JC_RC

		public override ZGuid JC_RC
		{
			get { return base.JC_RC; }
			set
			{
				base.JC_RC = value;
				Services.MarkAsNeedingValidation();
			}
		}

		protected override bool JC_RC_ReadOnly
		{
			get { return !IsMandatoryContainerType; }
		}

		#endregion

		#region JC_OH_ShippingLine

		[ReadOnly(true)]
		[List("ShippingProviderList")]
		public override ZGuid JC_OH_ShippingLine
		{
			get
			{
				ZGuid result = ZGuid.Empty;

				if (Consol != null)
				{
					result = Consol.ShippingLinePK;
				}
				else if (Sailing != null)
				{
					result = Sailing.JX_JV_OH_Line;
				}

				return result;
			}
		}

		#endregion

		#region JC_JS_FCLBookingOnlyLink

		public override ZGuid JC_JS_FCLBookingOnlyLink
		{
			get
			{
				return base.JC_JS_FCLBookingOnlyLink;
			}
			set
			{
				base.JC_JS_FCLBookingOnlyLink = value;
				if (Consol != null)
				{
					Consol.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JC_GrossWeightUQ

		public override ZString JC_GrossWeightUQ
		{
			get { return base.JC_GrossWeightUQ; }
			set
			{
				if (JC_GrossWeightUQ != value)
				{
					var oldUnit = ContainerWeightUnit;
					base.JC_GrossWeightUQ = value;
					JC_GrossWeight = Constants.Weight.Convert(JC_GrossWeight, oldUnit, ContainerWeightUnit, false);
				}
			}
		}

		#endregion

		#region Gate In/GateOut Events

		[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual)]
		public override ZDateTime JC_FCLWharfGateIn
		{
			get { return base.JC_FCLWharfGateIn; }
			set
			{
				if (base.JC_FCLWharfGateIn != value)
				{
					base.JC_FCLWharfGateIn = value;

					LogEvent(AutoEvents.GateIn, value.ToOffset(), EventConstants.Facilities.Code.Terminal);
				}
			}
		}

		[EventDateProperty(AutoEvents.GateOutCode, EstimateActual.Actual)]
		public override ZDateTime JC_FCLWharfGateOut
		{
			get { return base.JC_FCLWharfGateOut; }
			set
			{
				if (base.JC_FCLWharfGateOut != value)
				{
					base.JC_FCLWharfGateOut = value;

					LogEvent(AutoEvents.GateOut, value.ToOffset(), EventConstants.Facilities.Code.Terminal);
				}
			}
		}

		[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual)]
		[EventDateProperty(AutoEvents.DehireCode, EstimateActual.Actual)]
		public override ZDateTime JC_ContainerYardEmptyReturnGateIn
		{
			get { return base.JC_ContainerYardEmptyReturnGateIn; }
			set
			{
				if (base.JC_ContainerYardEmptyReturnGateIn != value)
				{
					base.JC_ContainerYardEmptyReturnGateIn = value;

					LogEvent(AutoEvents.GateIn, value.ToOffset(), EventConstants.Facilities.Code.ContainerYard);
					LogEvent(AutoEvents.Dehire, value.ToOffset(), EventConstants.Facilities.Code.ContainerYard);
				}
			}
		}

		[EventDateProperty(AutoEvents.GateOutCode, EstimateActual.Actual)]
		public override ZDateTime JC_ContainerYardEmptyPickupGateOut
		{
			get { return base.JC_ContainerYardEmptyPickupGateOut; }
			set
			{
				if (base.JC_ContainerYardEmptyPickupGateOut != value)
				{
					base.JC_ContainerYardEmptyPickupGateOut = value;

					LogEvent(AutoEvents.GateOut, value.ToOffset(), EventConstants.Facilities.Code.ContainerYard);
				}
			}
		}

		#endregion

		#endregion

		#region Calculated Properties

		#region TotalShipmentPacks

		public ZInt TotalShipmentPacks
		{
			get { return PackUnpackShipments.TotalPacks; }
		}

		public ZPropertyInfo TotalShipmentPacksInfo
		{
			get { return GetZPropertyInfo(Schema.TotalShipmentPacks); }
		}

		#endregion

		#region TotalShipmentWeight

		public ZDecimal TotalShipmentWeight
		{
			get { return this.GetRoundedValue(TotalShipmentWeightInfo, PackUnpackShipments.TotalWeight); }
		}

		public ZPropertyInfo TotalShipmentWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalShipmentWeight); }
		}

		#endregion

		#region TotalShipmentVolume

		public ZDecimal TotalShipmentVolume
		{
			get { return this.GetRoundedValue(TotalShipmentVolumeInfo, PackUnpackShipments.TotalVolume); }
		}

		public ZPropertyInfo TotalShipmentVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalShipmentVolume); }
		}

		#endregion

		#region JC_Calc_ConsolBookingRef

		public ZString JC_Calc_ConsolBookingRef
		{
			get { return (Consol != null) ? Consol.JK_BookingReference : ZString.Empty; }
		}

		public ZPropertyInfo JC_Calc_ConsolBookingRefInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ConsolBookingRef); }
		}

		#endregion

		#region JC_Calc_ConsolEntryNumber

		public ZString JC_Calc_ConsolEntryNumber
		{
			get { return (Consol != null) ? Consol.JK_CustomsReference : ZString.Empty; }
		}

		public ZPropertyInfo JC_Calc_ConsolEntryNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JC_Calc_ConsolEntryNumber)); }
		}

		#endregion

		#endregion

		#region Container Arrival/Dispatch

		#region Container Arrival

		[BusinessObjectTestExclude()]
		[List("LocalTransportList")]
		public ZGuid JC_OA_ArrivalTransportAddress
		{
			get { return CFSArrival != null ? CFSArrival.EU_OA_TransportProvider : ZGuid.Empty; }
			set
			{
				if (CFSArrival != null)
				{
					CFSArrival.EU_OA_TransportProvider = value;
				}
				JC_OA_ArrivalTransportAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_OA_ArrivalTransportAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_OA_ArrivalTransportAddress, x => CFSArrival.EU_OA_TransportProviderInfo); }//GetZPropertyInfo(Schema.JC_OH_ArrivalTransport); }
		}

		public ZBool JC_OA_ArrivalTransportAddress_ReadOnly
		{
			get { return CFSArrival == null; }
		}

		public ZGuid JC_ArrivalTransportPK
		{
			get
			{
				OrgAddress add = Factory.Load<OrgAddress>(JC_OA_ArrivalTransportAddress);
				return add != null ? add.OA_OH : ZGuid.Empty;
			}
			set
			{
				OrgHeader org = Factory.Load<OrgHeader>(value);
				JC_OA_ArrivalTransportAddress = org != null ? org.MainAddress.PK : ZGuid.Empty;
			}
		}

		[BusinessObjectTestExclude()]
		public ZString JC_ArrivalTruckDriversLicense
		{
			get { return CFSArrival != null ? CFSArrival.EU_DriversLicence : ZString.Empty; }
			set
			{
				if (CFSArrival != null)
				{
					CFSArrival.EU_DriversLicence = value;
				}
				JC_ArrivalTruckDriversLicenseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_ArrivalTruckDriversLicenseInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_ArrivalTruckDriversLicense, x => CFSArrival.EU_DriversLicenceInfo); }
		}

		public ZBool JC_ArrivalTruckDriversLicense_ReadOnly
		{
			get { return CFSArrival == null; }
		}

		[BusinessObjectTestExclude()]
		public ZString JC_ArrivalTruckDrivers
		{
			get { return CFSArrival != null ? CFSArrival.EU_DriversName : ZString.Empty; }
			set
			{
				if (CFSArrival != null)
				{
					CFSArrival.EU_DriversName = value;
				}
				JC_ArrivalTruckDriversInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_ArrivalTruckDriversInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_ArrivalTruckDrivers, x => CFSArrival.EU_DriversNameInfo); }
		}

		public ZBool JC_ArrivalTruckDrivers_ReadOnly
		{
			get { return CFSArrival == null; }
		}

		[BusinessObjectTestExclude()]
		public ZString JC_ArrivalTruckRegistration
		{
			get { return CFSArrival != null ? CFSArrival.EU_VehicleRegistration : ZString.Empty; }
			set
			{
				if (CFSArrival != null)
				{
					CFSArrival.EU_VehicleRegistration = value;
				}

				JC_ArrivalTruckRegistrationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_ArrivalTruckRegistrationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_ArrivalTruckRegistration, x => CFSArrival.EU_VehicleRegistrationInfo); }
		}

		public ZBool JC_ArrivalTruckRegistration_ReadOnly
		{
			get { return CFSArrival == null; }
		}

		[BusinessObjectTestExclude()]
		public ZDateTime JC_ArrivalTime
		{
			get { return CFSArrival != null ? CFSArrival.EU_PickupDeliveryTime : ZDateTime.Empty; }
			set
			{
				if (CFSArrival != null)
				{
					CFSArrival.EU_PickupDeliveryTime = value;
				}
				JC_ArrivalTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_ArrivalTimeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_ArrivalTime, x => CFSArrival.EU_PickupDeliveryTimeInfo); }
		}

		public ZBool JC_ArrivalTime_ReadOnly
		{
			get { return CFSArrival == null; }
		}

		#endregion

		#region Container Dispatch

		[BusinessObjectTestExclude()]
		[List("LocalTransportList")]
		public ZGuid JC_OA_DepartureTransportAddress
		{
			get { return CFSDispatch != null ? CFSDispatch.EU_OA_TransportProvider : ZGuid.Empty; }
			set
			{
				if (CFSDispatch != null)
				{
					CFSDispatch.EU_OA_TransportProvider = value;
				}
				JC_OA_DepartureTransportAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_OA_DepartureTransportAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_OA_DepartureTransportAddress, x => CFSDispatch.EU_OA_TransportProviderInfo); }
		}

		public ZBool JC_OA_DepartureTransportAddress_ReadOnly
		{
			get { return CFSDispatch == null; }
		}

		public ZGuid JC_DepartureTransportPK
		{
			get
			{
				OrgAddress add = Factory.Load<OrgAddress>(JC_OA_DepartureTransportAddress);
				return add != null ? add.OA_OH : ZGuid.Empty;
			}
			set
			{
				OrgHeader org = Factory.Load<OrgHeader>(value);
				JC_OA_DepartureTransportAddress = org != null ? org.MainAddress.PK : ZGuid.Empty;
			}
		}

		[BusinessObjectTestExclude()]
		public ZString JC_DepartureTruckDriversLicense
		{
			get { return CFSDispatch != null ? CFSDispatch.EU_DriversLicence : ZString.Empty; }
			set
			{
				if (CFSDispatch != null)
				{
					CFSDispatch.EU_DriversLicence = value;
				}
				JC_DepartureTruckDriversLicenseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_DepartureTruckDriversLicenseInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_DepartureTruckDriversLicense, x => CFSDispatch.EU_DriversLicenceInfo); }
		}

		public ZBool JC_DepartureTruckDriversLicense_ReadOnly
		{
			get { return CFSDispatch == null; }
		}

		[BusinessObjectTestExclude()]
		public ZString JC_DepartureTruckDrivers
		{
			get { return CFSDispatch != null ? CFSDispatch.EU_DriversName : ZString.Empty; }
			set
			{
				if (CFSDispatch != null)
				{
					CFSDispatch.EU_DriversName = value;
				}
				JC_DepartureTruckDriversInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_DepartureTruckDriversInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_DepartureTruckDrivers, x => CFSDispatch.EU_DriversNameInfo); }
		}

		public ZBool JC_DepartureTruckDrivers_ReadOnly
		{
			get { return CFSDispatch == null; }
		}

		[BusinessObjectTestExclude()]
		public ZString JC_DepartureTruckRegistration
		{
			get { return CFSDispatch != null ? CFSDispatch.EU_VehicleRegistration : ZString.Empty; }
			set
			{
				if (CFSDispatch != null)
				{
					CFSDispatch.EU_VehicleRegistration = value;
				}
				JC_DepartureTruckRegistrationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_DepartureTruckRegistrationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_DepartureTruckRegistration, x => CFSDispatch.EU_VehicleRegistrationInfo); }
		}

		public ZBool JC_DepartureTruckRegistration_ReadOnly
		{
			get { return CFSDispatch == null; }
		}

		[BusinessObjectTestExclude()]
		public ZDateTime JC_DepartureTime
		{
			get { return CFSDispatch != null ? CFSDispatch.EU_PickupDeliveryTime : ZDateTime.Empty; }
			set
			{
				if (CFSDispatch != null)
				{
					CFSDispatch.EU_PickupDeliveryTime = value;
				}
				JC_DepartureTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JC_DepartureTimeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JC_DepartureTime, x => CFSDispatch.EU_PickupDeliveryTimeInfo); }
		}

		public ZBool JC_DepartureTime_ReadOnly
		{
			get { return CFSDispatch == null; }
		}

		#endregion

		#endregion

		#region Additional Properties

		#region JC_LCLAvailable_Readonly

		public ZDateTime JC_LCLAvailable_Readonly
		{
			get { return JC_LCLAvailable; }
		}

		public ZPropertyInfo JC_LCLAvailable_ReadonlyInfo
		{
			get { return GetZPropertyInfo(Schema.JC_LCLAvailable_Readonly); }
		}

		#endregion

		#region JC_CargoStatusAdviceStatus

		protected StmALog MostRecentCargoStatusAdviceEvent
		{
			get
			{
				StmALog result = null;
				foreach (StmALog log in Logs.GetAllLogs())
				{
					if (log.SL_SE_NKEvent == Events.SeaCargoDepotEvent.Code && !log.SL_IsCancelled)
					{
						if (log.SL_Reference.StartsWith("CLEAR", StringComparison.Ordinal) || log.SL_Reference.StartsWith("DETAIN", StringComparison.Ordinal))
						{
							if (result == null)
							{
								result = log;
							}
							else if (result.SL_EventTime < log.SL_EventTime)
							{
								result = log;
							}
						}
					}
				}
				return result;
			}
		}

		[MaxLength(StmALog.Schema.SL_ReferenceMaxLength)]
		public ZString JC_CargoStatusAdviceStatus
		{
			get
			{
				ZString result = "";
				if (MostRecentCargoStatusAdviceEvent != null)
				{
					result = MostRecentCargoStatusAdviceEvent.SL_Reference;
				}
				return result;
			}
		}

		public ZPropertyInfo JC_CargoStatusAdviceStatusInfo
		{
			get { return GetZPropertyInfo(nameof(JC_CargoStatusAdviceStatus)); }
		}

		public ZDateTime JC_CargoStatusAdviceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Invalid;
				if (MostRecentCargoStatusAdviceEvent != null)
				{
					result = MostRecentCargoStatusAdviceEvent.SL_EventTime;
				}
				return result;
			}
		}

		public ZPropertyInfo JC_CargoStatusAdviceDateInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(JC_CargoStatusAdviceDate));
			}
		}

		#endregion

		#region JC_ImpendingArrivalStatus

		protected StmALog MostRecentImpendingArrivalEvent
		{
			get
			{
				StmALog result = null;
				foreach (StmALog log in Logs.GetAllLogs())
				{
					if (log.SL_SE_NKEvent == Events.SeaCargoDepotEvent.Code && !log.SL_IsCancelled)
					{
						if (log.SL_Reference.StartsWith("IMPEND", StringComparison.Ordinal))
						{
							if (result == null)
							{
								result = log;
							}
							else if (result.SL_EventTime < log.SL_EventTime)
							{
								result = log;
							}
						}
					}
				}
				return result;
			}
		}

		[MaxLength(StmALog.Schema.SL_ReferenceMaxLength)]
		public ZString JC_ImpendingArrivalStatus
		{
			get
			{
				ZString result = "";
				if (MostRecentImpendingArrivalEvent != null)
				{
					result = MostRecentImpendingArrivalEvent.SL_Reference;
				}
				return result;
			}
		}

		public ZPropertyInfo JC_ImpendingArrivalStatusInfo
		{
			get { return GetZPropertyInfo(nameof(JC_ImpendingArrivalStatus)); }
		}

		public ZDateTime JC_ImpendingArrivalDate
		{
			get
			{
				ZDateTime result = ZDateTime.Invalid;
				if (MostRecentImpendingArrivalEvent != null)
				{
					result = MostRecentImpendingArrivalEvent.SL_EventTime;
				}
				return result;
			}
		}

		public ZPropertyInfo JC_ImpendingArrivalDateInfo
		{
			get { return GetZPropertyInfo(nameof(JC_ImpendingArrivalDate)); }
		}

		#endregion

		#region JC_LCLStorageCommences_Readonly

		public ZDateTime JC_LCLStorageCommences_Readonly
		{
			get { return JC_LCLStorageCommences; }
		}

		public ZPropertyInfo JC_LCLStorageCommences_ReadonlyInfo
		{
			get { return GetZPropertyInfo(Schema.JC_LCLStorageCommences_Readonly); }
		}

		#endregion

		#region JC_JK_UniqueConsignRef

		public ZString JC_JK_UniqueConsignRef
		{
			get
			{
				if (Consol == null)
				{
					return ZString.Empty;
				}
				return Consol.JK_UniqueConsignRef;
			}
		}

		public ZPropertyInfo JC_JK_UniqueConsignRefInfo
		{
			get { return GetZPropertyInfo(nameof(JC_JK_UniqueConsignRef)); }
		}

		#endregion

		#region JC_TransportMode

		[List("JC_TransportMode_List")]
		[MaxLength(3)]
		public ZString JC_TransportMode
		{
			get
			{
				if (fJC_TransportMode.IsEmpty && !fJC_TransportModeForcedToEmpty)
				{
					fJC_TransportMode = Transport.IsEmpty ? new ZString(Constants.TransportModes.Sea) : Transport;
				}
				return fJC_TransportMode;
			}
			set
			{
				if (fJC_TransportMode != value)
				{
					fJC_TransportModeForcedToEmpty = value.IsEmpty;
					CheckMaximumLength(JC_TransportModeInfo, value);
					fJC_TransportMode = value;

					if (!settingDefaultValues)
					{
						((IDefaultNumberOfDecimalsSupporter)this).RoundMeasurePropertiesOnTransportModeChanged();
					}

					JC_TransportModeInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJC_TransportMode();
					}

					JC_JX = ZGuid.Empty;

					if (JC_TrainWagonNumberInfo.ReadOnly)
					{
						JC_TrainWagonNumber = "";
					}
				}
			}
		}
		bool fJC_TransportModeForcedToEmpty;
		ZString fJC_TransportMode;

		public ZPropertyInfo JC_TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.JC_TransportMode); }
		}

		#endregion

		#region ContainerYardAddress

		public ZGuid ContainerYardAddress
		{
			get
			{
				var status = PackOrUnpackStatus;
				ZGuid address = ZGuid.Empty;
				switch (status)
				{
					case PackUnpackStatusHelper.PackUnpackStatus.Pack:
						address = JC_OA_DepartureContainerYardAddress;
						break;

					case PackUnpackStatusHelper.PackUnpackStatus.Unpack:
						address = JC_OA_ArrivalContainerYardAddress;
						break;
				}

				return address;
			}
			set
			{
				PackUnpackStatusHelper.PackUnpackStatus status = PackOrUnpackStatus;
				if (status == PackUnpackStatusHelper.PackUnpackStatus.Pack)
				{
					JC_OA_DepartureContainerYardAddress = value;
				}
				else if (status == PackUnpackStatusHelper.PackUnpackStatus.Unpack)
				{
					JC_OA_ArrivalContainerYardAddress = value;
				}
				ContainerYardAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ContainerYardAddressInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerYardAddress); }
		}

		public bool ContainerYardAddress_ReadOnly
		{
			get
			{
				var packUnpackStatus = PackOrUnpackStatus;
				return packUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Pack
					&& packUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Unpack;
			}
		}

		public ZAddress ContainerYardAddress_ZAddress
		{
			get
			{
				if (fContainerYardAddress_ZAddress == null || fContainerYardAddress_ZAddress.PK != ContainerYardAddress)
				{
					fContainerYardAddress_ZAddress = new ZAddress(ContainerYardAddressInfo);
				}

				switch (PackOrUnpackStatus)
				{
					case PackUnpackStatusHelper.PackUnpackStatus.Pack:
						fContainerYardAddress_ZAddress.DefaultAddressType = JC_OA_DepartureContainerYardAddress_ZAddress.DefaultAddressType;
						break;
					case PackUnpackStatusHelper.PackUnpackStatus.Unpack:
						fContainerYardAddress_ZAddress.DefaultAddressType = JC_OA_ArrivalContainerYardAddress_ZAddress.DefaultAddressType;
						break;
				}
				return fContainerYardAddress_ZAddress;
			}
		}

		ZAddress fContainerYardAddress_ZAddress;

		#endregion

		#region JC_PackUnpackDate

		[BusinessObjectTestExclude]
		public ZDateTime JC_PackUnpackDate
		{
			get { return (PackOrUnpackStatus == PackUnpackStatusHelper.PackUnpackStatus.Pack) ? JC_PackDate : JC_LCLUnpack; }
		}

		public ZPropertyInfo JC_PackUnpackDateInfo
		{
			get { return GetZPropertyInfo(Schema.JC_PackUnpackDate); }
		}

		#endregion

		public ZBool HasPackDate
		{
			get { return PackOrUnpackStatus == PackUnpackStatusHelper.PackUnpackStatus.Pack && !JC_PackDate.IsEmpty; }
		}

		public ZBool HasUnpackDate
		{
			get { return PackOrUnpackStatus == PackUnpackStatusHelper.PackUnpackStatus.Unpack && !JC_LCLUnpack.IsEmpty; }
		}

		public virtual PackUnpackStatusHelper.PackUnpackStatus PackOrUnpackStatus
		{
			get
			{
				PackUnpackStatusHelper.PackUnpackStatus result = PackUnpackStatusHelper.PackUnpackStatus.None;

				if (Sailing != null)
				{
					result = PackUnpackStatusHelper.GetPackUnpackStatus(Sailing.JX_JA_RL_NKPortOfLoading, Sailing.JX_JB_RL_NKPortOfDischarge, Factory);
				}
				else if (Consol != null && Consol.Transports.MostInterestingTransport != null)
				{
					result = PackUnpackStatusHelper.GetPackUnpackStatus(Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort, Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort, Factory);
				}

				return result;
			}
		}

		public bool IsStorageJob
		{
			get { return JC_Purpose == ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage; }
		}

		public bool IsCFSJob
		{
			get { return JC_Purpose == ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS; }
		}

		#endregion

		#region Delete Checking

		public bool IsAttachedToLoadList
		{
			get { return JC_JK.IsEmpty; }
		}

		public bool HasShipments
		{
			get { return PackUnpackShipments.Count > 0; }
		}

		public bool IsPackedOrUnpacked
		{
			get
			{
				Event eventType = this.IsExport() ? Events.CFSContainerPacked : (this.IsImport() ? Events.CFSContainerUnpacked : null);
				StmALog log = null;

				if (eventType != null)
				{
					log = Logs.MostRecentLogByEventTime(eventType);
				}

				return (log != null);
			}
		}

		#endregion

		#region MessageCaption

		public ZString MessageCaption
		{
			get { return fMessageCaption; }
			set { fMessageCaption = value; }
		}
		ZString fMessageCaption;

		#endregion

		#region WarningMessage

		[MaxLength(250)]
		public ZString WarningMessage
		{
			get { return fWarningMessage; }
			set
			{
				CheckMaximumLength(WarningMessageInfo, value);
				fWarningMessage = value;
				WarningMessageInfo.RefreshBinding();
			}
		}
		ZString fWarningMessage;

		public ZPropertyInfo WarningMessageInfo
		{
			get { return GetZPropertyInfo(nameof(WarningMessage)); }
		}

		#endregion

		#region Validation

		protected override JobContainerValidation GetNewValidation()
		{
			return new CFSContainerValidation(this);
		}

		public new CFSContainerValidation Validation
		{
			get { return (CFSContainerValidation)GetNewValidation(); }
		}

		#endregion

		#region Lists

		#region ClientOrgHeaderList

		public OrgHeaderCollection ClientOrgHeaderList
		{
			get
			{
				OrgHeaderCollection orgs = new OrgHeaderCollection(Factory);
				orgs.RemoveAll();
				orgs.AddRange(new ForwarderCollection(Factory));
				orgs.AddRange(new ShippingProviderCollection(Factory));

				if (PackOrUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Unpack)
				{
					orgs.AddRange(new ConsignorCollection(Factory));
				}

				return orgs;
			}
		}

		#endregion

		#region JC_Purpose_List

		public CodeDescriptionPairList JC_Purpose_List
		{
			get
			{
				if (fJC_Purpose_List == null)
				{
					fJC_Purpose_List = new ContainerPurposeTypeCodeDescriptionPairList();
				}
				return fJC_Purpose_List;
			}
		}
		CodeDescriptionPairList fJC_Purpose_List;

		#endregion

		#region JC_TransportMode_List

		public virtual CodeDescriptionPairList JC_TransportMode_List
		{
			get
			{
				if (fJC_TransportMode_List == null)
				{
					fJC_TransportMode_List = new BookingTransportModeCodeDescriptionPairList();
				}
				return fJC_TransportMode_List;
			}
		}
		CodeDescriptionPairList fJC_TransportMode_List;

		#endregion

		#region JC_ContainerMode_List

		public override CodeDescriptionPairList JC_ContainerMode_List
		{
			get
			{
				if (Consol != null)
				{
					return base.JC_ContainerMode_List;
				}
				else
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
					if (JC_TransportMode == Constants.TransportModes.Air)
					{
						list.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					}
					else
					{
						list.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
						list.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
						list.AddPair(Constants.ContainerModes.Groupage, Constants.ContainerModeDescriptions.Groupage);
						list.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
						list.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					}
					return list;
				}
			}
		}

		#endregion

		#region OrgForwarderList

		public ForwarderCollection OrgForwarderList
		{
			get
			{
				if (fOrgForwarderList == null)
				{
					fOrgForwarderList = new ForwarderCollection(Factory);
				}
				return fOrgForwarderList;
			}
		}
		ForwarderCollection fOrgForwarderList;

		#endregion

		#region OrgReceivables_List

		public DebtorCollection OrgDebtor_List
		{
			get
			{
				if (fOrgDebtor_List == null)
				{
					fOrgDebtor_List = new DebtorCollection(Factory);
				}

				return fOrgDebtor_List;
			}
		}
		DebtorCollection fOrgDebtor_List;

		#endregion

		#region SeaCTO_List

		public OrgHeaderCollection SeaCTO_List
		{
			get
			{
				if (fSeaCTO_List == null)
				{
					fSeaCTO_List = new SeaCTOCollection(Factory);
				}
				return fSeaCTO_List;
			}
		}
		SeaCTOCollection fSeaCTO_List;

		#endregion

		#region OrgDepartureCTOList

		public OrgHeaderCollection OrgDepartureCTOList
		{
			get
			{
				OrgHeaderCollection fOrgDepartureCTOList = SeaCTO_List;

				if (Sailing != null && !Sailing.JX_JA_RL_NKPortOfLoading.IsEmpty)
				{
					fOrgDepartureCTOList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Sailing.JX_JA_RL_NKPortOfLoading));
				}

				return fOrgDepartureCTOList;
			}
		}

		#endregion

		#region OrgArrivalCTOList

		public OrgHeaderCollection OrgArrivalCTOList
		{
			get
			{
				OrgHeaderCollection fOrgArrivalCTOList = SeaCTO_List;

				if (Sailing != null && !Sailing.JX_JB_RL_NKPortOfDischarge.IsEmpty)
				{
					fOrgArrivalCTOList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Sailing.JX_JB_RL_NKPortOfDischarge));
				}

				return fOrgArrivalCTOList;
			}
		}

		#endregion

		#region OrgPackDepotList

		public PackDepotCollection OrgPackDepotList
		{
			get
			{
				if (fOrgPackDepotList == null)
				{
					fOrgPackDepotList = new PackDepotCollection(Factory);

					if (Sailing != null && !Sailing.JX_JA_RL_NKPortOfLoading.IsEmpty)
					{
						fOrgPackDepotList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Sailing.JX_JA_RL_NKPortOfLoading));
					}
				}

				return fOrgPackDepotList;
			}
		}
		PackDepotCollection fOrgPackDepotList;

		#endregion

		#region OrgUnpackDepotList

		public UnpackDepotCollection OrgUnpackDepotList
		{
			get
			{
				if (fOrgUnpackDepotList == null)
				{
					fOrgUnpackDepotList = new UnpackDepotCollection(Factory);

					if (Sailing != null && !Sailing.JX_JA_RL_NKPortOfLoading.IsEmpty)
					{
						fOrgUnpackDepotList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Sailing.JX_JA_RL_NKPortOfLoading));
					}
				}
				return fOrgUnpackDepotList;
			}
		}
		UnpackDepotCollection fOrgUnpackDepotList;

		#endregion

		#region JC_OH_ShippingLineList

		public ShippingProviderCollection JC_OH_ShippingLineList
		{
			get { return BindingLists.SeaShippingProvider_List; }
		}

		#endregion

		#region OrgContainerYardList

		public ContainerYardCollection OrgContainerYardList
		{
			get
			{
				if (fOrgContainerYardList == null)
				{
					fOrgContainerYardList = new ContainerYardCollection(Factory);
				}

				return fOrgContainerYardList;
			}
		}
		ContainerYardCollection fOrgContainerYardList;

		#endregion

		#region OrgDepartureContainerYardList

		public ContainerYardCollection OrgDepartureContainerYardList
		{
			get
			{
				if (fOrgDepartureContainerYardList == null)
				{
					fOrgDepartureContainerYardList = new ContainerYardCollection(Factory);

					if (Sailing != null && !Sailing.JX_JA_RL_NKPortOfLoading.IsEmpty)
					{
						fOrgDepartureContainerYardList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Sailing.JX_JA_RL_NKPortOfLoading));
					}
				}

				return fOrgDepartureContainerYardList;
			}
		}
		ContainerYardCollection fOrgDepartureContainerYardList;

		#endregion

		#region OrgArrivalContainerYardList

		public ContainerYardCollection OrgArrivalContainerYardList
		{
			get
			{
				if (fOrgArrivalContainerYardList == null)
				{
					fOrgArrivalContainerYardList = new ContainerYardCollection(Factory);

					if (Sailing != null && !Sailing.JX_JB_RL_NKPortOfDischarge.IsEmpty)
					{
						fOrgArrivalContainerYardList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Sailing.JX_JB_RL_NKPortOfDischarge));
					}
				}
				return fOrgArrivalContainerYardList;
			}
		}
		ContainerYardCollection fOrgArrivalContainerYardList;

		#endregion

		#region LocalTransportList

		public LocalTransportCollection LocalTransportList
		{
			get
			{
				if (fLocalTransportList == null)
				{
					fLocalTransportList = new LocalTransportCollection(Factory);
				}

				return fLocalTransportList;
			}
		}
		LocalTransportCollection fLocalTransportList;

		#endregion

		#region OrgShippingProviderSeaList

		public SeaShippingProviderCollection OrgShippingProviderSeaList
		{
			get
			{
				if (fOrgShippingProviderSeaList == null)
				{
					fOrgShippingProviderSeaList = new SeaShippingProviderCollection(Factory);
				}
				return fOrgShippingProviderSeaList;
			}
		}
		SeaShippingProviderCollection fOrgShippingProviderSeaList;

		#endregion

		#region RefUNLOCO_List

		public RefUNLOCOCollection RefUNLOCO_List
		{
			get
			{
				if (fRefUNLOCO_List == null)
				{
					fRefUNLOCO_List = new RefUNLOCOCollection(Factory);
				}

				return fRefUNLOCO_List;
			}
		}
		RefUNLOCOCollection fRefUNLOCO_List;

		#endregion

		#region RefVessel_List

		public RefVesselCollection RefVessel_List
		{
			get
			{
				if (fRefVessel_List == null)
				{
					fRefVessel_List = new RefVesselCollection(Factory);
				}
				return fRefVessel_List;
			}
		}
		RefVesselCollection fRefVessel_List;

		#endregion

		#region LoadListConsol_List

		public CFSLoadListConsolCollection LoadListConsol_List
		{
			get
			{
				if (fLoadListConsol_List == null)
				{
					fLoadListConsol_List = new CFSLoadListConsolCollection(Factory);
				}

				#region Set Default Filters

				CFSConsolDefaultFilterProvider provider = new CFSConsolDefaultFilterProvider();

				if (JC_OH_CFSClient.IsValid)
				{
					provider.Client = JC_OH_CFSClient;
				}

				if (Sailing != null)
				{
					provider.TransportMode = Sailing.JX_TransportMode;
					provider.LoadPort = Sailing.JX_JA_RL_NKPortOfLoading;
					provider.DischargePort = Sailing.JX_JB_RL_NKPortOfDischarge;
					provider.Voyage = Sailing.JX_JV_VoyageFlight;
					provider.Vessel = Sailing.JX_JV_NKVessel;
				}

				provider.SetDefaultFilters(fLoadListConsol_List);

				#endregion

				return fLoadListConsol_List;
			}
		}
		CFSLoadListConsolCollection fLoadListConsol_List;

		#endregion

		#region ShippingProviderList

		public ShippingProviderCollection ShippingProviderList
		{
			get
			{
				switch (JC_TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						return BindingLists.AirShippingProvider_List;

					case Core.Constants.TransportModes.Sea:
						return BindingLists.SeaShippingProvider_List;

					case Core.Constants.TransportModes.Road:
						return BindingLists.LineHaulShippingProvider_List;

					case Core.Constants.TransportModes.Rail:
						return BindingLists.RailShippingProvider_List;

					case Core.Constants.TransportModes.Mail:
						return BindingLists.AirShippingProvider_List;

					case Core.Constants.TransportModes.Other:
						return BindingLists.ShippingProvider_List;

					default:
						return BindingLists.ShippingProvider_List;
				}
			}
		}

		#endregion

		#region Drivers
		public GlbStaffCollection Drivers
		{
			get { return new GlbStaffCollection(Factory); }
		}
		#endregion

		#endregion

		#region Additional Properties

		#region JC_OA_CTOAddress

		public ZGuid JC_OA_CTOAddress
		{
			get
			{
				ZGuid result = ZGuid.Empty;

				PackUnpackStatusHelper.PackUnpackStatus status = PackOrUnpackStatus;
				if (Consol != null)
				{
					if (status == PackUnpackStatusHelper.PackUnpackStatus.Pack)
					{
						result = Consol.JK_OA_DepartureCTOAddress;
					}
					else if (status == PackUnpackStatusHelper.PackUnpackStatus.Unpack)
					{
						result = Consol.JK_OA_ArrivalCTOAddress;
					}
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Create New Container

		public CFSContainer CreateNewContainer()
		{
			var result = Factory.New<CFSContainer>();
			result.JC_TransportMode = JC_TransportMode;
			result.JC_ContainerMode = JC_ContainerMode;
			result.JC_RC = JC_RC;
			result.JC_ContainerNum = JC_ContainerNum;
			result.JC_SealNum = JC_SealNum;
			result.JC_SealParty = JC_SealParty;
			result.JC_AdditionalSealParty = JC_AdditionalSealParty;
			result.JC_Additional2SealParty = JC_Additional2SealParty;
			result.JC_OH_CFSClient = JC_OH_CFSClient;

			result.JC_EmptyReturnedBy = JC_EmptyReturnedBy;
			result.JC_OA_DepartureContainerYardAddress = JC_OA_DepartureContainerYardAddress;
			result.JC_OA_ArrivalContainerYardAddress = JC_OA_ArrivalContainerYardAddress;

			if (JC_Purpose == ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS)
			{
				result.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
				result.JC_JX = Consol != null ? Consol.JK_JX_Sailing : JC_JX;
				result.JC_FCLStorageModuleOnlyMaster = JC_Calc_MasterBillNum;
				if (Consol == null)
				{
					result.JC_OverrideFCLAvailableStorage = true;
				}
			}
			else
			{
				result.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
				result.JC_JX = JC_JX;
			}

			JC_DepartureTime = ZDateTime.Now;
			result.JC_ArrivalTime = ZDateTime.Now;

			return result;
		}

		#endregion

		#region JC_LCLUnpack

		[BusinessObjectTestExclude]
		public override ZDateTime JC_LCLUnpack
		{
			get { return base.JC_LCLUnpack; }
			set
			{
				if (JC_LCLUnpack != value)
				{
					base.JC_LCLUnpack = value;
					JC_LCLAvailable = NextAvailableDate;
					JC_LCLStorageCommences = LCLStorageStartDate;

					if (PackUnpackShipments != null)
					{
						foreach (CFSShipment shipment in PackUnpackShipments)
						{
							shipment.DocsAndCartage.ClientForStorage = JC_OH_CFSClient;
							shipment.DocsAndCartage.UpdateLCLAvailableDate(value);
						}
					}
				}
			}
		}

		ZDateTime NextAvailableDate
		{
			get
			{
				if (JC_LCLUnpack.IsValidSmallDateTime)
				{
					var dateCalculator = new DateCalculator(Factory, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK);

					return dateCalculator.CalculateDate(JC_LCLUnpack.ToDateTime(), DatesToCalculate.Available, TransportMode, false);
				}

				return ZDateTime.Empty;
			}
		}

		#endregion

		#region JC_LCLAvailable / JC_LCLStorageCommences

		public override ZDateTime JC_LCLAvailable
		{
			get { return JC_OverriddenLCLAvailable; }
			set
			{
				base.JC_LCLAvailable = value;

				if (JC_LCLStorageCommences == ZDateTime.Empty)
				{
					shouldUpdateShipmentStorageDates = false;

					try
					{
						JC_LCLStorageCommences = LCLStorageStartDate;
					}
					finally
					{
						shouldUpdateShipmentStorageDates = true;
					}
				}

				UpdateShipmentsAvailables();
			}
		}

		public override ZDateTime JC_LCLStorageCommences
		{
			get { return JC_OverriddenLCLStorage; }
			set
			{
				base.JC_LCLStorageCommences = value;

				if (shouldUpdateShipmentStorageDates)
				{
					UpdateShipmentsStorageDates();
				}
			}
		}

		ZBool shouldUpdateShipmentStorageDates = true;

		ZDateTime LCLStorageStartDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (JC_LCLAvailable.IsValidSmallDateTime)
				{
					var dateCalculator = new DateCalculator(Factory, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK);
					int freeDays = 0;
					if (!JC_OH_CFSClient.IsEmpty)
					{
						OrgHeader client = Factory.Load<OrgHeader>(JC_OH_CFSClient);
						if (client != null)
						{
							freeDays = (TransportMode == Core.Constants.TransportModes.Air)
								? client.MiscServ.OM_IMAirDepotFreeDays
								: client.MiscServ.OM_IMSeaDepotFreeDays;
						}
					}

					result = dateCalculator.CalculateDate(JC_LCLAvailable.ToDateTime(), DatesToCalculate.Storage, TransportMode, false, freeDays);
				}

				return result;
			}
		}

		void UpdateShipmentsAvailables()
		{
			if (PackUnpackShipments != null)
			{
				foreach (CFSShipment shipment in PackUnpackShipments)
				{
					shipment.DocsAndCartage.ClientForStorage = JC_OH_CFSClient;

					if (shipment.DocsAndCartage.JP_LCLAvailable == ZDateTime.Empty && JC_LCLAvailable.IsValid)
					{
						shipment.DocsAndCartage.JP_LCLAvailable = JC_LCLAvailable;
					}

					shipment.RefreshBinding();
				}
			}
		}

		void UpdateShipmentsStorageDates()
		{
			if (PackUnpackShipments != null)
			{
				foreach (CFSShipment shipment in PackUnpackShipments)
				{
					if (shipment.DocsAndCartage.JP_LCLStorageCommences == ZDateTime.Empty && JC_LCLStorageCommences.IsValid)
					{
						shipment.DocsAndCartage.JP_LCLStorageCommences = JC_LCLStorageCommences;
						shipment.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region JC_ArrivalCTOStorageStartDate

		[BusinessObjectTestExclude]
		public override ZDateTime JC_ArrivalCTOStorageStartDate
		{
			get { return base.JC_ArrivalCTOStorageStartDate; }
			set
			{
				base.JC_ArrivalCTOStorageStartDate = value;

				if (!value.IsEmpty && IsStorageJob && JC_ArrivalTime.IsEmpty)
				{
					JC_ArrivalTime = value;
				}
			}
		}

		#endregion

		#region JC_FCLAvailable

		public override ZDateTime JC_FCLAvailable
		{
			get
			{
				var result = ZDateTime.Empty;

				if (JC_OverrideFCLAvailableStorage)
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
				base.JC_FCLAvailable = value;
			}
		}

		#endregion

		#region ZAddresses

		protected override ZAddress GetNewJC_OA_ArrivalContainerYardAddress_ZAddress()
		{
			ZAddress result = base.GetNewJC_OA_ArrivalContainerYardAddress_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		protected override ZAddress GetNewJC_OA_DepartureContainerYardAddress_ZAddress()
		{
			ZAddress result = base.GetNewJC_OA_DepartureContainerYardAddress_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		#endregion

		#region RefreshAllSailingFields

		public void RefreshAllSailingFields()
		{
			JC_JA_NKPortOfLoadingInfo.RefreshBinding();
			JC_JB_NKPortOfDischargeInfo.RefreshBinding();
			JC_JV_NKVesselInfo.RefreshBinding();
			JC_JV_VoyageFlightInfo.RefreshBinding();
			JC_JA_E_DEPInfo.RefreshBinding();
			JC_JB_E_ARVInfo.RefreshBinding();
		}

		#endregion

		#region Implementation

		protected void InsertAddedOrUpdatedEvent()
		{
			if (HasChanges)
			{
				if (IsInDatabase)
				{
					ServiceEventUtils.SetupEvent(this, true, Events.EditedARecord);
				}
				else
				{
					ServiceEventUtils.SetupEvent(this, true, Events.AddedARecordToTheSystem);
				}
			}
		}

		#region SetupEventStuff

		void SetupEventStuff()
		{
			Event[] eventsToExclude =
			{
				Events.ServiceRequested,
				Events.ServiceCompleted,
				Events.QuarantineRequired,
				Events.QuarantineComplete,
				Events.CustomsImpedimentReceived,
				Events.CustomsCleared,
				Events.ExportCustomsCleared,
				Events.InspectionComplete,
			};

			ServiceEventUtils.ExcludeEventsFromAdd(this, eventsToExclude);
			ServiceEventUtils.ExcludeEventsFromCancel(this, eventsToExclude);
		}

		#endregion

		#endregion

		#region IDocumentSupportable Members

		public string TransportMode
		{
			get { return JC_TransportMode; }
		}

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new CFSContainerDocumentSupporter(this); }
		}

		#endregion

		#region ISendEmailSource Members

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			return GetAddressBookSelection();
		}

		public virtual AddressBookSelection GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(CFSClient);
			return result;
		}

		string ISendEmailSource.EmailSubject
		{
			get { return Res.GetString("060053a2-ea19-4a8e-a499-68e28fe1b0f8", "Container - {0}", this.JC_ContainerNum); }
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.Tally; }
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocDocPackUnpackContainerRego>(); }
		}

		#endregion

		#region IHaveServices Members

		public override IHaveServices[] DependentServiceParents
		{
			get
			{
				IHaveServices[] result = new IHaveServices[Shipments.Count];

				for (int i = 0; i < Shipments.Count; i++)
				{
					result[i] = Shipments[i].DocsAndCartage;
				}

				return result;
			}
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return JC_ContainerJobID; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			SetContainerID();
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		CFSContainerInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = GetNewInvoicingSupporter()); }
		}

		protected virtual CFSContainerInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new CFSContainerInvoicingSupporter(this);
		}

		#endregion

		#region ICartageContainer Members

		ZString ICartageContainer.ContainerMode
		{
			get { return JC_ContainerMode; }
		}

		ZString ICartageContainer.ContainerNumber
		{
			get { return JC_ContainerNum; }
		}

		ZGuid ICartageContainer.ContainerRC
		{
			get { return JC_RC; }
		}

		ZGuid ICartageContainer.JobContainerPK
		{
			get { return PK; }
		}

		IReadOnlyCollection<ICartageLooseCargo> ICartageContainer.LooseCargo
		{
			get { return (ICartageLooseCargo[])PackLines.ToArray(typeof(ICartageLooseCargo)); }
		}

		ZDecimal ICartageContainer.NetWeight
		{
			get { return JC_Calc_NetWeight; }
		}

		ZString ICartageContainer.Seal
		{
			get { return JC_SealNum; }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new CFSContainerRatingAdaptersProvider(this); }
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		protected override ZString TransportModeForDefaultNumberOfDecimals
		{
			get { return JC_TransportMode; }
		}

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return property.PropertyType == typeof(ZString)
				? -1
				: DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		public override int GetDecimalPlacesMetaDataIgnoringRegistry(PropertyDescriptor property)
		{
			if (property.PropertyType == typeof(ZString))
			{
				return -1;
			}

			return base.GetDecimalPlacesMetaDataIgnoringRegistry(property);
		}

		protected override ZString GetUnitOfMeasureCore(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.TotalShipmentWeight:
					unitOfMeasure = GetTotalShipmentWeightUnit();
					break;

				case Schema.TotalShipmentVolume:
					unitOfMeasure = GetTotalShipmentVolumeUnit();
					break;

				default:
					unitOfMeasure = base.GetUnitOfMeasureCore(property);
					break;
			}

			return unitOfMeasure;
		}

		ZString GetTotalShipmentWeightUnit()
		{
			return (PackUnpackShipments.Count > 0) ? PackUnpackShipments[0].JS_UnitOfWeight : ZString.Empty;
		}

		ZString GetTotalShipmentVolumeUnit()
		{
			return (PackUnpackShipments.Count > 0) ? PackUnpackShipments[0].JS_UnitOfVolume : ZString.Empty;
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CFSContainerFetchStrategy(this);
		}

		#endregion
	}

	public class CFSContainerInvoicingSupporter : JobInvoicingSupporter
	{
		public CFSContainerInvoicingSupporter(CFSContainer parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly CFSContainer Parent;

		public override OrgHeader Consignor
		{
			get { return Parent.CFSClient; }
		}

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override ZString TransportMode
		{
			get { return Parent.JC_TransportMode; }
		}

		public override ZString ContainerMode
		{
			get { return Parent.JC_ContainerMode; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.FCLStorage; }
		}

		public override ZString MasterBillNumber
		{
			get
			{
				ZString result = Parent.JC_FCLStorageModuleOnlyMaster;

				if (result.IsEmpty)
				{
					if (!Parent.JC_JK.IsEmpty)
					{
						result = Parent.Consol.JK_MasterBillNum;
					}
				}

				return result;
			}
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}

		public override int ContainerCount
		{
			get { return 1; }
		}

		public override ZDecimal TEUCount
		{
			get { return Parent.JC_Calc_TEUCount; }
		}
	}
}

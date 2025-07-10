using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Confirmations.Business
{
	public class QuickPOD : NonPersistentBusinessObject, IObsoleteValidation
	{
		public QuickPOD(QuickPODs quickPODHost)
			: base(quickPODHost.Factory)
		{
			QuickPODHost = quickPODHost;
			fReadOnlyProperty = ReadOnlyProperty.None;
		}

		readonly QuickPODs QuickPODHost;

		#region Schema

		public static class Schema
		{
			public const string ShipmentID = "ShipmentID";
			public const string HouseBill = "HouseBill";
			public const string PacksDelivered = "PacksDelivered";
			public const string DeliveryWeight = "DeliveryWeight";
			public const string DeliveryVolume = "DeliveryVolume";
			public const string ChargeCode = "ChargeCode";
		}

		#endregion

		#region Properties

		#region HB / Shipment ID

		enum ReadOnlyProperty
		{
			HouseBill,
			ShipmentID,
			None
		}
		ReadOnlyProperty fReadOnlyProperty;

		#region HouseBill

		protected bool HouseBill_ReadOnly
		{
			get
			{
				return fReadOnlyProperty == ReadOnlyProperty.HouseBill;
			}
		}

		[MaxLength(CommonShipment.Schema.JS_HouseBillMaxLength)]
		public ZString HouseBill
		{
			get { return houseBill; }
			set
			{
				if (houseBill != value)
				{
					if (houseBill.IsEmpty && shipmentID.IsEmpty && !value.IsEmpty)
					{
						fReadOnlyProperty = ReadOnlyProperty.ShipmentID;
					}
					else if (value.IsEmpty)
					{
						fReadOnlyProperty = ReadOnlyProperty.None;
					}

					CheckMaximumLength(HouseBillInfo, value);
					SetNonPersistentPropertyValue(HouseBillInfo, ref houseBill, value);
					SetupShipmentWithHouseBill();
					if (Shipment != null)
					{
						shipmentID = Shipment.JS_UniqueConsignRef;
					}
					else
					{
						shipmentID = ZString.Empty;
					}
					if (!IsValidationSuspended)
					{
						ValidateHouseBill();
						ValidateShipmentID();
					}
				}
			}
		}
		ZString houseBill;

		public ZPropertyInfo HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.HouseBill); }
		}

		#endregion

		#region ShipmentNumber

		protected bool ShipmentID_ReadOnly
		{
			get
			{
				return fReadOnlyProperty == ReadOnlyProperty.ShipmentID;
			}
		}

		[MaxLength(CommonShipment.Schema.JS_UniqueConsignRefMaxLength)]
		public ZString ShipmentID
		{
			get { return shipmentID; }
			set
			{
				if (ShipmentID != value)
				{
					if (houseBill.IsEmpty && shipmentID.IsEmpty && !value.IsEmpty)
					{
						fReadOnlyProperty = ReadOnlyProperty.HouseBill;
					}
					else if (value.IsEmpty)
					{
						fReadOnlyProperty = ReadOnlyProperty.None;
					}

					CheckMaximumLength(ShipmentIDInfo, value);
					SetNonPersistentPropertyValue(ShipmentIDInfo, ref shipmentID, value);
					SetupShipmentWithShipmentID();
					if (Shipment != null)
					{
						houseBill = Shipment.JS_HouseBill;
					}
					else
					{
						houseBill = ZString.Empty;
					}
					if (!IsValidationSuspended)
					{
						ValidateHouseBill();
						ValidateShipmentID();
					}
				}
			}
		}

		ZString shipmentID;

		public ZPropertyInfo ShipmentIDInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentID); }
		}

		#endregion

		#endregion

		#region PacksDelivered

		[BusinessObjectTestExclude]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member NoDeliveryLeg")]
		[ReadOnlyMember("NoDeliveryLeg")]
		public ZInt PacksDelivered
		{
			get
			{
				ZInt result = 0;
				if (DeliveryConfirm != null)
				{
					foreach (CommonConfirmDivot divot in DeliveryConfirm.Divots)
					{
						result += divot.J8_PackagesDelivered;
					}
				}
				return result;
			}
			set
			{
				if (DeliveryConfirm != null && PacksDelivered != value && DeliveryConfirm.Divots.Count > 0)
				{
					ZInt leftOver = value;
					for (int i = 0; i < DeliveryConfirm.Divots.Count; i++)
					{
						if (!DeliveryConfirm.Divots[0].J8_JL.IsEmpty)
						{
							bool finalDivot = (i == DeliveryConfirm.Divots.Count - 1);
							CommonConfirmDivot divot = DeliveryConfirm.Divots[i];
							divot.J8_PackagesDelivered = leftOver > divot.PackLine.JL_PackageCount && !finalDivot ? divot.PackLine.JL_PackageCount : leftOver;
							leftOver -= divot.J8_PackagesDelivered;
						}
					}
				}
				PacksDeliveredInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PacksDeliveredInfo
		{
			get { return GetZPropertyInfo(Schema.PacksDelivered); }
		}

		#endregion

		#region DeliveryWeight

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member NoDeliveryLeg")]
		[ReadOnlyMember("NoDeliveryLeg")]
		public ZDecimal DeliveryWeight
		{
			get
			{
				ZDecimal result = 0;
				if (DeliveryConfirm != null)
				{
					result = DeliveryConfirm.TotalDeliveredWeight;
				}
				return result;
			}
			set
			{
				if (DeliveryConfirm != null && DeliveryWeight != value && DeliveryConfirm.Divots.Count > 0)
				{
					decimal leftOverInMasterWeightUnit = value;
					for (int i = 0; i < DeliveryConfirm.Divots.Count; i++)
					{
						if (!DeliveryConfirm.Divots[0].J8_JL.IsEmpty)
						{
							bool finalDivot = (i == DeliveryConfirm.Divots.Count - 1);
							CommonConfirmDivot divot = DeliveryConfirm.Divots[i];

							//have to ensure jl_weightUQ is valid
							decimal divotBookedWeightInMasterUnit = Constants.Weight.Convert(divot.PackLine.JL_ActualWeight, divot.PackLine.JL_ActualWeightUQ, DeliveryConfirm.TotalWeightUnit);
							decimal weightInMasterUnit = leftOverInMasterWeightUnit > divotBookedWeightInMasterUnit && !finalDivot ? divotBookedWeightInMasterUnit : leftOverInMasterWeightUnit;
							divot.J8_DeliveryWeight = Constants.Weight.Convert(weightInMasterUnit, DeliveryConfirm.TotalWeightUnit, divot.PackLine.JL_ActualWeightUQ);
							decimal divotDeliveryWeightInMasterUnit = Constants.Weight.Convert(divot.J8_DeliveryWeight, divot.PackLine.JL_ActualWeightUQ, DeliveryConfirm.TotalWeightUnit);
							leftOverInMasterWeightUnit -= divotDeliveryWeightInMasterUnit;
						}
					}
				}
				DeliveryWeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryWeightInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryWeight); }
		}

		#endregion

		#region DeliveryVolume

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member NoDeliveryLeg")]
		[ReadOnlyMember("NoDeliveryLeg")]
		public ZDecimal DeliveryVolume
		{
			get
			{
				ZDecimal result = 0;
				if (DeliveryConfirm != null)
				{
					result = DeliveryConfirm.TotalDeliveredVolume;
				}
				return result;
			}
			set
			{
				if (DeliveryConfirm != null && DeliveryVolume != value && DeliveryConfirm.Divots.Count > 0)
				{
					decimal leftOverInMasterVolumeUnit = value;
					for (int i = 0; i < DeliveryConfirm.Divots.Count; i++)
					{
						if (!DeliveryConfirm.Divots[0].J8_JL.IsEmpty)
						{
							bool finalDivot = (i == DeliveryConfirm.Divots.Count - 1);
							CommonConfirmDivot divot = DeliveryConfirm.Divots[i];

							//have to ensure jl_VolumeUQ is valid
							decimal divotBookedVolumeInMasterUnit = Constants.Volume.Convert(divot.PackLine.JL_ActualVolume, divot.PackLine.JL_ActualVolumeUQ, DeliveryConfirm.TotalVolumeUnit);
							decimal volumeInMasterUnit = leftOverInMasterVolumeUnit > divotBookedVolumeInMasterUnit && !finalDivot ? divotBookedVolumeInMasterUnit : leftOverInMasterVolumeUnit;
							divot.J8_DeliveryVolume = Constants.Volume.Convert(volumeInMasterUnit, DeliveryConfirm.TotalVolumeUnit, divot.PackLine.JL_ActualVolumeUQ);
							decimal divotDeliveryVolumeInMasterUnit = Constants.Volume.Convert(divot.J8_DeliveryVolume, divot.PackLine.JL_ActualVolumeUQ, DeliveryConfirm.TotalVolumeUnit);
							leftOverInMasterVolumeUnit -= divotDeliveryVolumeInMasterUnit;
						}
					}
				}
				DeliveryVolumeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryVolume); }
		}

		#endregion

		#region ChargeCode

		//JR_AC
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member NoDeliveryLeg")]
		[ReadOnlyMember("NoDeliveryLeg")]
		[List("Charge.Lookups.ChargeCodes")]
		public ZGuid ChargeCode
		{
			get { return chargeCode; }
			set
			{
				if (chargeCode != value)
				{
					SetNonPersistentPropertyValue(ChargeCodeInfo, ref chargeCode, value);
					SetupCharge();
					if (!IsValidationSuspended)
					{
						ValidateChargeCode();
					}
				}
			}
		}
		ZGuid chargeCode;

		public ZPropertyInfo ChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCode); }
		}

		#endregion

		#endregion

		#region Related Objects

		#region Shipment

		public CommonShipment Shipment
		{
			get { return shipment; }
			set
			{
				if (shipment != value)
				{
					if (shipment != null)
					{
						DiscardShipmentChanges();
					}

					shipment = value;

					if (shipment != null)
					{
						RegisterEditableChildObject(shipment);
						SetupShipmentsChildren();
					}
				}
			}
		}
		CommonShipment shipment;

		#endregion

		#region DeliveryLeg

		public CommonPickupDeliveryConfirm DeliveryConfirm
		{
			get { return deliveryConfirm; }
			set { deliveryConfirm = value; }
		}

		protected bool NoDeliveryConfirm
		{
			get { return DeliveryConfirm == null; }
		}

		CommonPickupDeliveryConfirm deliveryConfirm;

		#endregion

		#region Charge

		public JobCharge Charge
		{
			get { return charge; }
			set
			{
				if (charge != value)
				{
					if (charge != null)
					{
						DiscardChargeChanges();
					}

					charge = value;
				}
			}
		}
		JobCharge charge;

		#endregion

		#endregion

		#region Validation

		#region ValidateShipmentNumber

		public void ValidateShipmentID()
		{
			ValidateShipment(ShipmentIDInfo);
		}

		#endregion

		#region ValidateHouseBill

		public void ValidateHouseBill()
		{
			ValidateShipment(HouseBillInfo);
		}

		public void ValidateChargeCode()
		{
			ChargeCodeInfo.ClearAllNotifications();

			if (!ChargeCode.IsEmpty && Charge == null)
			{
				ChargeCodeInfo.AddError(Res.GetString("153e1806-3b4b-4292-9465-0cadd677c112", "Please enter a valid Charge Code"));
			}
			//want errors from jr_ac
		}

		#endregion

		#region ValidateAll

		public void ValidateAll()
		{
			ValidateHouseBill();
			ValidateChargeCode();
		}

		#endregion

		#endregion

		#region Saving

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				ReleaseShipmentJobHeaderMutex();
			}
			base.OnFactorySaved(saveSucceeded);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			DiscardShipmentChanges();
			base.Delete();
		}

		#endregion

		#region Implementation

		#region ValidateShipment

		void ValidateShipment(ZPropertyInfo iDInfo)
		{
			ZString shipmentNumberID = !shipmentID.IsEmpty ? Res.GetString("8e35542c-0972-4e72-802e-3c0b1955b216", "Shipment '{0}'", shipmentID) : Res.GetString("8acd3227-c079-4107-8a7c-66308e6d85d1", "Shipment with House Bill '{0}'", houseBill);

			iDInfo.ClearAllNotifications();

			if (Shipment != null)
			{
				ValidateShipmentDetails(shipmentNumberID, iDInfo);
			}
			else if (iDInfo.Value.IsEmpty)
			{
				iDInfo.AddError(Res.GetString("34f35195-3d1e-43a7-8b28-3ce8c1d6277f", "Please enter a {0}.", iDInfo.HumanReadableName));
			}
			else
			{
				iDInfo.AddError(Res.GetString("a811b013-9ee1-4ce0-8b9b-ed1c5b5936a4", "no match found."));
			}
		}

		void ValidateShipmentDetails(string shipmentIdentifier, ZPropertyInfo propertyInfo)
		{
			if (QuickPODHost.QuickPODsCollection.ContainsShipment(Shipment.PK, PK))
			{
				propertyInfo.AddError(Res.GetString("d4690808-ac4f-4e0a-a7a8-2cb444168227", "{0} already appears in this list.", shipmentIdentifier));
			}
			else
			{
				if (Shipment.OuterPackLines.Count == 0)
				{
					propertyInfo.AddError(Res.GetString("31f4b83a-7b26-4ace-a409-083c6171e0a9", "{0} has nothing to deliver. (No Pack Lines)", shipmentIdentifier));
				}

				if (DeliveryConfirm == null)
				{
					if (Shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery))
					{
						propertyInfo.AddError(Res.GetString("2b8a79da-40fd-4ff6-b8f7-2f30189a777f", "{0} is Containerized. Only Loose Delivery Shipments can be used in Quick POD.", shipmentIdentifier));
					}
					else if (Shipment.DeliveryConfirms.Count == 1 && !Shipment.DeliveryConfirms[0].EU_PickupDeliveryTime.IsEmpty)
					{
						propertyInfo.AddError(Res.GetString("f895183b-0b65-47f6-8154-99ef9853541e", "{0} already has a delivered Confirmation.", shipmentIdentifier));
					}
					else if (Shipment.DeliveryConfirms.Count > 1)
					{
						propertyInfo.AddError(Res.GetString("2c791a6a-fc60-4cfe-8dea-db1dda8bfa45", "{0} already has {1} Confirmation(s).", shipmentIdentifier, Shipment.DeliveryConfirms.Count));
					}
					else if (!Shipment.DeliveryConfirms.Relationship.SupportsAddToRelationship())
					{
						if (Shipment.IsAssemblyMaster)
						{
							propertyInfo.AddError(Res.GetString("6d852a6a-4b47-44c4-ab67-7ebfe84e2ae7", "{0} is an Assembly Master. Confirmations need to be entered on the related Shipments.", shipmentIdentifier));
						}
						else if (Shipment.IsCoLoadMaster || Shipment.IsBlindCoLoadMaster)
						{
							propertyInfo.AddError(Res.GetString("8afa60d1-e9ed-435d-bad2-c7c8e4ee6fd8", "{0} is a Co-Load Master. Confirmations need to be entered on the related Shipments.", shipmentIdentifier));
						}
						else
						{
							propertyInfo.AddError(Res.GetString("de6cdc39-4676-446c-99bc-c3657b22d965", "{0} does not support New Confirmations.", shipmentIdentifier));
						}
					}
				}

				if (shipment.ShipmentJobHeader == null)
				{
					var errorMessage = shipment.CreateShipmentJobHeaderWithMutex();
					if (!string.IsNullOrEmpty(errorMessage))
					{
						propertyInfo.AddError(Res.GetString("ed3e6248-265c-4ac0-90bb-a915313d80c2", "{0} cannot load a Charge;", shipmentIdentifier) + " " + errorMessage);
					}
				}
			}
		}

		#endregion

		#region SetupShipment

		#region with Shipment Number

		void SetupShipmentWithShipmentID()
		{
			if (shipmentID.IsEmpty)
			{
				Shipment = null;
			}
			else
			{
				SetShipment(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, SQLComparisonOperator.Contains, shipmentID), shipmentID);
			}
			if (Shipment != null)
			{
				shipmentID = Shipment.JS_UniqueConsignRef;
				ShipmentIDInfo.RefreshBinding();
			}
		}

		#endregion

		#region with house bill

		void SetupShipmentWithHouseBill()
		{
			if (houseBill.IsEmpty)
			{
				Shipment = null;
			}
			else
			{
				SetShipment(new ZQuery(JobShipmentSchema.JS_HouseBill, houseBill), houseBill);
			}
		}

		#endregion

		void SetShipment(ZQuery query, ZString identifier)
		{
			ZQuery cfsOrForwardQuery = new ZQuery(JobShipmentSchema.JS_IsCFSRegistered, true);
			cfsOrForwardQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_IsForwardRegistered, true);

			query.AddToFilter(cfsOrForwardQuery);

			ShipmentCollection shipmentsWithShipmentNumber = new ShipmentCollection(Factory);
			shipmentsWithShipmentNumber.SuspendValidation();
			try
			{
				shipmentsWithShipmentNumber.Load(query);

				if (shipmentsWithShipmentNumber.Count > 1)
				{
					QuickPODMultipleShipmentsEventArgs e = new QuickPODMultipleShipmentsEventArgs(identifier, shipmentsWithShipmentNumber);
					QuickPODHost.RaiseQuickPODMultipleShipments(e);
					Shipment = e.SelectedShipment;
				}
				else
				{
					Shipment = shipmentsWithShipmentNumber.Count == 1 ? shipmentsWithShipmentNumber[0] : null;
				}
			}
			finally
			{
				shipmentsWithShipmentNumber.ResumeValidation();
			}
		}

		#endregion

		#region SetupShipmentsChildren

		void SetupShipmentsChildren()
		{
			if (!QuickPODHost.QuickPODsCollection.ContainsShipment(Shipment.PK, PK) &&
				!Shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery) &&
				Shipment.DeliveryConfirms.Relationship.SupportsAddToRelationship())
			{
				SetupDefaultDeliveryLeg();
				SetupDefaultCharge();
			}
		}

		void SetupDefaultDeliveryLeg()
		{
			if (Shipment.DeliveryConfirms.Count == 0 && Shipment.OuterPackLines.Count > 0)
			{
				DeliveryConfirm = Shipment.DeliveryConfirms.AddNew();
			}
			else if (Shipment.DeliveryConfirms.Count == 1 && Shipment.DeliveryConfirms[0].EU_PickupDeliveryTime.IsEmpty)
			{
				DeliveryConfirm = Shipment.DeliveryConfirms[0];
			}
		}

		void SetupDefaultCharge()
		{
		}

		void SetupCharge()
		{
			JobCharge newCharge = null;

			if (ChargeCode.IsValid && shipment != null)
			{
				shipment.CreateShipmentJobHeaderWithMutex();
				IPODCharge chargeHeader = shipment.ShipmentJobHeader as IPODCharge;
				if (chargeHeader != null)
				{
					newCharge = chargeHeader.CreateCharge;
					newCharge.JR_AC = ChargeCode;
				}
			}

			Charge = newCharge;
		}

		#endregion

		#region RefreshShipment

		public void RefreshShipment()
		{
			if (Shipment == null)
			{
				throw new Exception("Shipment needs to be loaded to refresh it.");
			}

			if (DeliveryConfirm == null)
			{
				SetupDefaultDeliveryLeg();
			}

			if (Charge == null)
			{
				SetupDefaultCharge();
			}

			ValidateAll();
		}

		#endregion

		#region DiscardShipmentChanges

		void DiscardShipmentChanges()
		{
			if (!DiscardingShipmentChanges)
			{
				try
				{
					DiscardingShipmentChanges = true;
					ZGuid shipmentPK = Shipment != null ? Shipment.PK : ZGuid.Empty;

					DiscardBizo(DeliveryConfirm);

					if (DeliveryConfirm != null && DeliveryConfirm.IsInDatabase)
					{
						foreach (var divot in DeliveryConfirm.Divots)
						{
							DiscardBizo(divot);
						}
					}

					DiscardChargeChanges();
					DiscardBizo(Shipment);

					DeliveryConfirm = null;
					Charge = null;
					Shipment = null;

					RefreshPODWithShipment(shipmentPK);
				}
				finally
				{
					DiscardingShipmentChanges = false;
				}
			}
		}
		bool DiscardingShipmentChanges;

		void DiscardChargeChanges()
		{
			if (!DiscardingChargeChanges)
			{
				DiscardingChargeChanges = true;
				try
				{
					DiscardBizo(Charge);
					ReleaseShipmentJobHeaderMutex();
					if (Shipment != null)
					{
						DiscardBizo(Shipment.ShipmentJobHeader);
					}

					Charge = null;
				}
				finally
				{
					DiscardingChargeChanges = false;
				}
			}
		}
		bool DiscardingChargeChanges;

		void DiscardBizo(BusinessObject bizoToRemove)
		{
			if (bizoToRemove != null)
			{
				if (bizoToRemove.IsInDatabase)
				{
					bizoToRemove.Reload();
				}
				else
				{
					bizoToRemove.Delete();
				}
			}
		}

		void RefreshPODWithShipment(ZGuid shipmentPK)
		{
			if (!shipmentPK.IsEmpty)
			{
				QuickPOD podWithSameShipment = QuickPODHost.QuickPODsCollection[shipmentPK];
				if (podWithSameShipment != null)
				{
					podWithSameShipment.RefreshShipment();
				}
			}
		}

		#endregion

		#region ReleaseShipmentJobHeaderMutex

		public void ReleaseShipmentJobHeaderMutex()
		{
			if (Shipment != null && Shipment.ShipmentJobHeader != null)
			{
				Shipment.ShipmentJobHeader.Dispose();
			}
		}

		#endregion

		#endregion
	}
}

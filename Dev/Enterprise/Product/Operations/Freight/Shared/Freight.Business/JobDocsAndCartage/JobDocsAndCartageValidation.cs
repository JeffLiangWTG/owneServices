//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobDocsAndCartageValidation
//
//    This class should be used for overriding validation in AutoJobDocsAndCartageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class JobDocsAndCartageValidation : AutoJobDocsAndCartageValidation
	{
		public JobDocsAndCartageValidation(AutoJobDocsAndCartage parent)
			: base(parent)
		{
		}

		#region Parents

		public new JobDocsAndCartage Parent
		{
			get { return (JobDocsAndCartage)base.Parent; }
		}

		public CommonShipment ParentShipment
		{
			get { return Parent.ShipmentInParent; }
		}

		#endregion

		#region JP_EstimatedPickup

		protected override void CheckJP_EstimatedPickup()
		{
			base.CheckJP_EstimatedPickup();
			if (ParentShipment != null)
			{
				string pickupDateAfterETDMessage = Res.GetString("4d24cc34-92e5-433f-a50d-0319060024ab", "The Pickup date can not be after the ETD on an export shipment");
				if (ParentShipment.IsSea
					&& ParentShipment.IsExport()
					&& !ParentShipment.JS_E_DEP.IsEmpty
					&& !Parent.JP_EstimatedPickup.IsEmpty
					&& ParentShipment.JS_E_DEP.Date == Parent.JP_EstimatedPickup.Date)
				{
					Parent.JP_EstimatedPickupInfo.AddWarning(pickupDateAfterETDMessage);
				}
				else if (ParentShipment.JS_E_DEP < Parent.JP_EstimatedPickup && ParentShipment.IsExport() && !Parent.JP_EstimatedPickupInfo.HasErrors())
				{
					Parent.JP_EstimatedPickupInfo.AddError(pickupDateAfterETDMessage);
				}
			}

			if (Parent.JP_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
			{
				BusinessObject declaration = (BusinessObject)Parent.Factory.LoadTop1<Enterprise.Integration.Customs.IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, Parent.JP_ParentID));
				if (declaration != null)
				{
					if ((ZDateTime)declaration[JobDeclarationSchema.JE_DateAtOrigin.Name] < Parent.JP_EstimatedPickup && !(new ZBool(declaration["IsImport"])))
					{
						Parent.JP_EstimatedPickupInfo.AddError(Res.GetString("7a10cdde-dfcc-4b8a-bebd-f5d9e3610104", "The Pickup date can not be after the ETD on the declaration"));
					}
				}
			}
		}

		#endregion

		#region JP_EstimatedDelivery

		protected override void CheckJP_EstimatedDelivery()
		{
			base.CheckJP_EstimatedDelivery();

			var errorOrWarningText = Res.GetString("116d4141-730f-4710-a258-888128cb3b86", "The Delivery date can not be before the ETA on an import shipment");
			if (ParentShipment != null && ParentShipment.IsImport() && !Parent.JP_EstimatedDeliveryInfo.HasErrors())
			{
				if (ParentShipment.Consols.Count > 0 && (Parent.JP_EstimatedDelivery < ParentShipment.ArrivalConsol?.JK_JX_JB_E_ARV))
				{
					Parent.JP_EstimatedDeliveryInfo.AddWarning(errorOrWarningText);
				}
				else if (ParentShipment.JS_E_ARV > Parent.JP_EstimatedDelivery)
				{
					Parent.JP_EstimatedDeliveryInfo.AddError(errorOrWarningText);
				}
			}

			if (Parent.JP_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
			{
				BusinessObject declaration = (BusinessObject)Parent.Factory.LoadTop1<Enterprise.Integration.Customs.IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, Parent.JP_ParentID));
				if (declaration != null)
				{
					if ((ZDateTime)declaration[JobDeclarationSchema.JE_DateAtFinalDestination.Name] > Parent.JP_EstimatedDelivery && new ZBool(declaration["IsImport"]))
					{
						Parent.JP_EstimatedDeliveryInfo.AddError(Res.GetString("95bd9665-1933-43d5-ad71-063f82facf94", "The Delivery date can not be before the ETA on the declaration"));
					}
				}
			}
		}

		#endregion

		#region JP_FCLDeliveryEquipmentNeeded

		protected override void CheckJP_FCLDeliveryEquipmentNeeded()
		{
			base.CheckJP_FCLDeliveryEquipmentNeeded();
			if (ParentShipment == null || ParentShipment.JS_IsForwardRegistered || !ParentShipment.JS_IsBooking)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JP_FCLDeliveryEquipmentNeededInfo, Parent.Lookups.DeliveryEquipmentNeededList);
			}
		}

		#endregion

		#region JP_FCLPickupEquipmentNeeded

		protected override void CheckJP_FCLPickupEquipmentNeeded()
		{
			base.CheckJP_FCLPickupEquipmentNeeded();
			if (ParentShipment == null || ParentShipment.JS_IsForwardRegistered || !ParentShipment.JS_IsBooking)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JP_FCLPickupEquipmentNeededInfo, Parent.Lookups.PickupEquipmentNeededList);
			}
		}

		#endregion

		#region JP_OrderItemsAsString

		public void ValidateJP_OrderItemsAsString()
		{
			ValidateCalculatedProperty(Parent.JP_OrderItemsAsStringInfo);
		}

		protected virtual void CheckJP_OrderItemsAsString()
		{
			ZString[] orderNumbers = Parent.JP_OrderItemsAsString.Replace(" ", ",").Split(',');
			foreach (ZString orderNumber in orderNumbers)
			{
				if (orderNumber.Length > JobOrderItemSchema.JT_OrderReference.MaxLength)
				{
					Parent.JP_OrderItemsAsStringInfo.AddError(Res.GetString("5b86191b-1b2d-46fe-801d-960de240aad7", "Order numbers can be a maximum of {0} characters in length. You can separate multiple order numbers with a comma (,).", JobOrderItemSchema.JT_OrderReference.MaxLength));
					break;
				}
			}
		}

		#endregion

		#region JP_PickupRequiredFrom

		protected override void CheckJP_PickupRequiredFrom()
		{
			base.CheckJP_PickupRequiredFrom();
			if (ParentShipment != null && !Parent.JP_PickupRequiredFrom.IsEmpty && !Parent.JP_PickupRequiredBy.IsEmpty && Parent.JP_PickupRequiredFrom > Parent.JP_PickupRequiredBy)
			{
				Parent.JP_PickupRequiredFromInfo.AddError(Res.GetString("9e667ab6-1bc1-4a55-b221-5de81485a758", "The date in Required From must be before Required By."));
			}
		}

		#endregion

		#region JP_PickupRequiredBy

		protected override void CheckJP_PickupRequiredBy()
		{
			base.CheckJP_PickupRequiredBy();
			if (ParentShipment != null && !Parent.JP_PickupRequiredBy.IsEmpty && !Parent.JP_PickupRequiredFrom.IsEmpty && Parent.JP_PickupRequiredFrom > Parent.JP_PickupRequiredBy)
			{
				Parent.JP_PickupRequiredByInfo.AddError(Res.GetString("6fe2d662-dd71-4a29-8785-52dfc39f48b0", "The date in Required By must be after Required From."));
			}
		}

		#endregion

		#region JP_DeliveryRequiredFrom

		protected override void CheckJP_DeliveryRequiredFrom()
		{
			base.CheckJP_DeliveryRequiredFrom();
			if (ParentShipment != null && !Parent.JP_DeliveryRequiredFrom.IsEmpty && !Parent.JP_DeliveryRequiredBy.IsEmpty && Parent.JP_DeliveryRequiredFrom > Parent.JP_DeliveryRequiredBy)
			{
				Parent.JP_DeliveryRequiredFromInfo.AddError(Res.GetString("cb2b46cb-3fe7-46e2-85d4-04d17edbbac0", "The date in Required From must be before Required By."));
			}
		}

		#endregion

		#region JP_DeliveryRequiredFrom

		protected override void CheckJP_DeliveryRequiredBy()
		{
			base.CheckJP_DeliveryRequiredBy();
			if (ParentShipment != null && !Parent.JP_DeliveryRequiredBy.IsEmpty && !Parent.JP_DeliveryRequiredFrom.IsEmpty && Parent.JP_DeliveryRequiredFrom > Parent.JP_DeliveryRequiredBy)
			{
				Parent.JP_DeliveryRequiredByInfo.AddError(Res.GetString("f2821076-a109-4ff9-a699-706e9626f528", "The date in Required By must be after Required From."));
			}
		}

		#endregion

		#region CheckJP_PickupCartageCompleted

		protected override void CheckJP_PickupCartageCompleted()
		{
			base.CheckJP_PickupCartageCompleted();
			if (ParentShipment == null)
			{
				return;
			}

			if (!ParentShipment.IsAssemblyMaster &&
				!ParentShipment.IsCoLoadMaster &&
				!ParentShipment.IsBlindCoLoadMaster &&
				!ParentShipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual) &&
				!Parent.JP_PickupCartageCompleted.IsEmpty)
			{
				string msg = Res.GetString("4fb3f651-3a4d-4e37-9bcd-624b9834f630", "Pickup Confirmations are not complete. Edit the Pickup Confirmations on the Confirmations Tab or clear Actual Pickup and re-enter a date to automatically fill in the missing Pickup Confirmations.");
				if (((IBusinessObjectState)ParentShipment.PickupConfirms).HasChanges)
				{
					Parent.JP_PickupCartageCompletedInfo.AddError(msg);
				}
				else
				{
					Parent.JP_PickupCartageCompletedInfo.AddWarning(msg);
				}
			}

			AddMessageIfActualPickupCartageCompletedInTheFuture(Parent.JP_PickupCartageCompleted);
		}

		public bool IsActualPickupCartageCompletedInTheFuture(ZDateTime pickupCartageCompleted)
		{
			if (ParentShipment == null)
			{
				return false;
			}

			var pickupAddress = JobDocAddress.Load(ParentShipment, DocAddressType.ConsignorPickupDeliveryAddress);
			var unloco = ((ILocation)pickupAddress)?.UNLOCO ?? ParentShipment.Origin;
			return unloco != null && pickupCartageCompleted > (unloco.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime);
		}

		void AddMessageIfActualPickupCartageCompletedInTheFuture(ZDateTime pickupCartageCompleted)
		{
			if (ParentShipment == null)
			{
				return;
			}
			var pickupAddress = JobDocAddress.Load(ParentShipment, DocAddressType.ConsignorPickupDeliveryAddress);
			var unloco = ((ILocation)pickupAddress)?.UNLOCO ?? ParentShipment.Origin;
			if (unloco != null && pickupCartageCompleted > (unloco.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime))
			{
				Parent.JP_PickupCartageCompletedInfo.AddError(Res.GetString("a4f04d87-b831-4079-a09b-01de5ea9ba4f", "Actual pickup date cannot be in the future."));
			}
			else if (unloco == null && pickupCartageCompleted > (GlbStaff.CurrentUser.HomeBranch?.HomePort.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime))
			{
				Parent.JP_PickupCartageCompletedInfo.AddWarning(Res.GetString("1657d986-2345-44e4-ad2d-a71c1d29c41d", "The Actual pickup date is in the future."));
			}
		}

		#endregion

		#region CheckJP_DeliveryCartageCompleted

		protected override void CheckJP_DeliveryCartageCompleted()
		{
			base.CheckJP_PickupCartageCompleted();
			if (ParentShipment == null)
			{
				return;
			}

			if (!ParentShipment.IsAssemblyMaster &&
				!ParentShipment.IsCoLoadMaster &&
				!ParentShipment.IsBlindCoLoadMaster &&
				!ParentShipment.IsConfirmsComplete(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual) &&
				!Parent.JP_DeliveryCartageCompleted.IsEmpty)
			{
				string msg = Res.GetString("d1ab426c-a5bf-4c66-aae9-3d1ac2694da7", "Delivery Confirmations are not complete. Edit the Delivery Confirmations on the Confirmations Tab or clear Actual Delivery and re-enter a date to automatically fill in the missing Delivery Confirmations.");
				if (((IBusinessObjectState)ParentShipment.DeliveryConfirms).HasChanges)
				{
					Parent.JP_DeliveryCartageCompletedInfo.AddError(msg);
				}
				else
				{
					Parent.JP_DeliveryCartageCompletedInfo.AddWarning(msg);
				}
			}

			AddMessageIfActualDeliveryCartageCompletedInTheFuture(Parent.JP_DeliveryCartageCompleted);
		}

		public bool IsActualDeliveryCartageCompletedInTheFuture(ZDateTime deliveryCargageCompleted)
		{
			if (ParentShipment == null)
			{
				return false;
			}

			var deliveryAddress = JobDocAddress.Load(ParentShipment, DocAddressType.ConsigneePickupDeliveryAddress);
			var unloco = ((ILocation)deliveryAddress)?.UNLOCO ?? ParentShipment.Destination;
			return unloco != null && deliveryCargageCompleted > (unloco.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime);
		}

		void AddMessageIfActualDeliveryCartageCompletedInTheFuture(ZDateTime deliveryCargageCompleted)
		{
			if (ParentShipment == null)
			{
				return;
			}
			var deliveryAddress = JobDocAddress.Load(ParentShipment, DocAddressType.ConsigneePickupDeliveryAddress);
			var unloco = ((ILocation)deliveryAddress)?.UNLOCO ?? ParentShipment.Destination;
			if (unloco != null && deliveryCargageCompleted > (unloco.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime))
			{
				Parent.JP_DeliveryCartageCompletedInfo.AddError(Res.GetString("7804b1b7-1def-4232-83f0-a23762d63db0", "Actual delivery date cannot be in the future."));
			}
			else if (unloco == null && deliveryCargageCompleted > (GlbStaff.CurrentUser.HomeBranch?.HomePort.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime))
			{
				Parent.JP_DeliveryCartageCompletedInfo.AddWarning(Res.GetString("498ae6dc-069c-4c12-93d2-89b86ce2564f", "The Actual delivery date is in the future."));
			}
		}

		#endregion

		#region DeliveryCartage

		public void ValidateDeliveryCartageCoPK()
		{
			ValidateCalculatedProperty(Parent.DeliveryCartageCoPKInfo);
		}

		protected virtual void CheckDeliveryCartageCoPK()
		{
			if (!Parent.DeliveryCartageCoPK.IsEmpty && (Parent.Parent?.ShouldValidateDeliveryCoPK() ?? true))
			{
				if ((!Parent.IsInDatabase || Parent.JP_OA_DeliveryCartageCoAddrInfo.HasChanges) && !Globals.IsWeb)
				{
					ListValidation.ErrorIfInvalidPK(Parent.DeliveryCartageCoPKInfo);
				}
				else
				{
					ListValidation.WarnIfInvalidPK(Parent.DeliveryCartageCoPKInfo);
				}
			}
		}

		protected override void CheckJP_OA_DeliveryCartageCoAddr()
		{
			Parent.JP_OA_DeliveryCartageCoAddrInfo.AddAllNotificationsFrom(Parent.DeliveryCartageCoPKInfo);
		}

		#endregion

		#region PickupCartage

		public void ValidatePickupCartageCoPK()
		{
			ValidateCalculatedProperty(Parent.PickupCartageCoPKInfo);
		}

		protected virtual void CheckPickupCartageCoPK()
		{
			if (!Parent.PickupCartageCoPK.IsEmpty)
			{
				if ((!Parent.IsInDatabase || Parent.JP_OA_PickupCartageCoAddrInfo.HasChanges) && !Globals.IsWeb)
				{
					ListValidation.ErrorIfInvalidPK(Parent.PickupCartageCoPKInfo);
				}
				else
				{
					ListValidation.WarnIfInvalidPK(Parent.PickupCartageCoPKInfo);
				}
			}
		}

		protected override void CheckJP_OA_PickupCartageCoAddr()
		{
			Parent.JP_OA_PickupCartageCoAddrInfo.AddAllNotificationsFrom(Parent.PickupCartageCoPKInfo);
		}

		#endregion

		#region JP_ExportStatement

		ZString[] ExportStatementExemptedDestinations => new ZString[]
		{
			Constants.CountryCodes.China,
			Constants.CountryCodes.HongKong,
			Constants.CountryCodes.Russia,
			Constants.CountryCodes.Venezuela
		};

		protected override void CheckJP_ExportStatement()
		{
			base.CheckJP_ExportStatement();
			if (ParentShipment != null)
			{
				if (!Parent.IsInDatabase || Parent.IsInDatabase && Parent.JP_ExportStatementInfo.HasChanges)
				{
					ListValidation.ErrorIfInvalidCode(Parent.JP_ExportStatementInfo, Parent.Lookups.JP_ExportStatementList);
				}

				if (Constants.CountryCodes.IsUsaOrTerritory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					&& Constants.CountryCodes.IsUsaOrTerritory(ParentShipment.JS_RL_NKOrigin.SubstringSafe(0, 2))
					&& ExportStatementExemptedDestinations.Contains(ParentShipment.JS_RL_NKDestination.SubstringSafe(0, 2))
					&& Parent.JP_ExportStatement == "LOW")
				{
					Parent.JP_ExportStatementInfo.AddWarning(Res.GetString("a099fa72-04ea-4604-aa0f-f24d1e58e692", "For exports to China, Hong Kong, Russia or Venezuela, using exemption NOEEI §30.37(a) indicates none of the commodities are other than EAR99."));
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateDeliveryCartageCoPK();
			ValidatePickupCartageCoPK();

			base.ValidateAll();
			ValidateJP_OrderItemsAsString();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Business
{
	public static class ConfirmTimesSyncHelper
	{
		public enum ConfirmType
		{
			Pickup,
			Delivery,
		}

		public enum ConfirmDateType
		{
			Planned,
			RequestedBy,
			Actual
		}

		public static void SetConfirmTimes(CommonShipment shipment, ConfirmType confirmType, ConfirmDateType dateType, ZDateTime oldValue, ZDateTime newValue, bool autoCreateLooseConfirmations)
		{
			if (IsDateValidOrEmpty(newValue) && shipment.DocsAndCartage.SetShipmentDatesSuspensionLevel == 0 && IsValidShipmentForAddingConfirmations(shipment))
			{
				SetShipmentConfirms(shipment, confirmType, dateType, oldValue, newValue, autoCreateLooseConfirmations);
			}
		}

		public static void SetConfirmTimes(CommonContainer container, ConfirmType confirmType, ConfirmDateType dateType, ZDateTime oldValue, ZDateTime newValue)
		{
			using (container.SuspendSettingConfirmDates())
			{
				if (IsDateValidOrEmpty(newValue) && container.SetContainerDatesSuspensionLevel == 0)
				{
					if (container.IsOnDEPFCLConsol && confirmType == ConfirmType.Pickup)
					{
						SetConfirmDate(container.OriginConfirm, dateType, oldValue, newValue);
					}
					else if (container.IsOnARVFCLConsol && confirmType == ConfirmType.Delivery)
					{
						SetConfirmDate(container.DestinationConfirm, dateType, oldValue, newValue);
					}
				}
			}
		}

		public static void SetContainerTimes(CommonContainer container, ConfirmType confirmType, ConfirmDateType dateType, ZDateTime oldValue, ZDateTime newValue)
		{
			using (container.SuspendSettingContainerDates())
			{
				if (IsDateValidOrEmpty(newValue) && container.SetConfirmDatesSuspensionLevel == 0)
				{
					if (dateType == ConfirmDateType.Actual)
					{
						if (confirmType == ConfirmType.Pickup)
						{
							if (container.JC_DepartureCartageComplete.IsEmpty || container.JC_DepartureCartageComplete == oldValue)
							{
								container.JC_DepartureCartageComplete = newValue;
							}
						}
						else
						{
							if (container.JC_ArrivalCartageComplete.IsEmpty || container.JC_ArrivalCartageComplete == oldValue)
							{
								container.JC_ArrivalCartageComplete = newValue;
							}
						}
					}
					else if (dateType == ConfirmDateType.Planned)
					{
						if (confirmType == ConfirmType.Pickup)
						{
							if (container.JC_DepartureEstimatedPickup.IsEmpty || container.JC_DepartureEstimatedPickup == oldValue)
							{
								container.JC_DepartureEstimatedPickup = newValue;
							}
						}
						else
						{
							if (container.JC_ArrivalEstimatedDelivery.IsEmpty || container.JC_ArrivalEstimatedDelivery == oldValue)
							{
								container.JC_ArrivalEstimatedDelivery = newValue;
							}
						}
					}
				}
			}
		}

		public static void SetConfirmSignedBy(CommonShipment shipment, ConfirmType confirmType, ZString signedBy, IXmlSessionTracker logger)
		{
			if (!signedBy.IsEmpty && IsValidShipmentForAddingConfirmations(shipment))
			{
				if (shipment.ShowContainerisedConfirms(confirmType))
				{
					foreach (CommonContainer container in shipment.Containers)
					{
						var confirm = (confirmType == ConfirmType.Pickup) ? container.OriginConfirm : container.DestinationConfirm;
						if (confirm != null)
						{
							confirm.EU_GoodsSignForBy = signedBy.Left(confirm.EU_GoodsSignForByInfo.MaxLength);
							logger.Log(LogType.Information, Res.GetString("ConfirmTimesSynchHelper|GoodsSignedByUpdatedOnContainer", "'Goods Signed By' has been updated to value '{0}' on the confirmation of {1}.", signedBy, container.HumanReadableName));
						}
					}
				}
				else
				{
					var confirms = (confirmType == ConfirmType.Pickup) ? shipment.PickupConfirms : shipment.DeliveryConfirms;
					foreach (var confirm in confirms)
					{
						confirm.EU_GoodsSignForBy = signedBy.Left(confirm.EU_GoodsSignForByInfo.MaxLength);
					}
					logger.Log(LogType.Information, Res.GetString("ConfirmTimesSynchHelper|GoodsSignedByUpdatedOnShipment", "'Goods Signed By' has been updated to value '{0}' on the confirmation of {1}.", signedBy, shipment.HumanReadableName));
				}
			}
		}

		public static bool LooseConfirmNeedsToBeCreated(CommonShipment shipment, ConfirmType confirmType)
		{
			if (shipment.DocsAndCartage.SetShipmentDatesSuspensionLevel == 0 && IsValidShipmentForAddingConfirmations(shipment) && !shipment.ShowContainerisedConfirms(confirmType))
			{
				var confirms = (confirmType == ConfirmType.Pickup) ? shipment.PickupConfirms : shipment.DeliveryConfirms;
				return confirms.Count == 0 && shipment.OuterPackLines.Count > 0 && HasValidCartageDate(shipment, confirmType);
			}

			return false;
		}

		public static void CreateLooseConfirmIfNeeded(CommonShipment shipment, ConfirmType confirmType)
		{
			if (LooseConfirmNeedsToBeCreated(shipment, confirmType))
			{
				var confirms = (confirmType == ConfirmType.Pickup) ? shipment.PickupConfirms : shipment.DeliveryConfirms;
				SetLooseConfirm(shipment, confirms.AddNew(), confirmType);
			}
		}

		public static string GetConfirmDateFieldName(ConfirmDateType dateType)
		{
			switch (dateType)
			{
				case ConfirmTimesSyncHelper.ConfirmDateType.Planned:
					return CommonPickupDeliveryConfirm.Schema.EU_PlannedPickupDeliveryTime;
				case ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy:
					return CommonPickupDeliveryConfirm.Schema.EU_RequestedPickupDeliveryTime;
				case ConfirmTimesSyncHelper.ConfirmDateType.Actual:
					return CommonPickupDeliveryConfirm.Schema.EU_PickupDeliveryTime;
				default:
					return string.Empty;
			}
		}

		public static string GetCartageDateFieldName(ConfirmType confirmType, ConfirmDateType dateType)
		{
			switch (dateType)
			{
				case ConfirmTimesSyncHelper.ConfirmDateType.Planned:
					return (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? JobDocsAndCartage.Schema.JP_EstimatedPickup : JobDocsAndCartage.Schema.JP_EstimatedDelivery;
				case ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy:
					return (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? JobDocsAndCartage.Schema.JP_PickupRequiredBy : JobDocsAndCartage.Schema.JP_DeliveryRequiredBy;
				case ConfirmTimesSyncHelper.ConfirmDateType.Actual:
					return (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? JobDocsAndCartage.Schema.JP_PickupCartageCompleted : JobDocsAndCartage.Schema.JP_DeliveryCartageCompleted;
				default:
					return string.Empty;
			}
		}

		public static bool IsValidShipmentForAddingConfirmations(CommonShipment shipment)
		{
			return !shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster && !shipment.IsAssemblyMaster && (shipment.JS_IsCFSRegistered || shipment.JS_IsForwardRegistered);
		}

		public static void SetShipmentTimesFromConfirmIfRequired(CommonShipment shipment, CommonPickupDeliveryConfirm confirm)
		{
			using (shipment.DocsAndCartage.SuspendSettingShipmentDates())
			{
				if (confirm.EU_PickupDeliveryType == Core.Constants.PickupDeliveryConfirmTypes.OriginPickup)
				{
					if (shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty && shipment.IsConfirmsComplete(ConfirmType.Pickup, ConfirmDateType.Actual))
					{
						shipment.DocsAndCartage.JP_PickupCartageCompleted = GetMostRecentConfirmDate(shipment, ConfirmType.Pickup, ConfirmDateType.Actual, confirm.EU_PickupDeliveryTime);
					}
					if (shipment.DocsAndCartage.JP_PickupRequiredBy.IsEmpty && shipment.IsConfirmsComplete(ConfirmType.Pickup, ConfirmDateType.RequestedBy))
					{
						shipment.DocsAndCartage.JP_PickupRequiredBy = GetMostRecentConfirmDate(shipment, ConfirmType.Pickup, ConfirmDateType.RequestedBy, confirm.EU_RequestedPickupDeliveryTime);
					}
					if (shipment.DocsAndCartage.JP_EstimatedPickup.IsEmpty && shipment.IsConfirmsComplete(ConfirmType.Pickup, ConfirmDateType.Planned))
					{
						shipment.DocsAndCartage.JP_EstimatedPickup = GetMostRecentConfirmDate(shipment, ConfirmType.Pickup, ConfirmDateType.Planned, confirm.EU_PlannedPickupDeliveryTime);
					}
				}
				else if (confirm.EU_PickupDeliveryType == Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery)
				{
					if (shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty && shipment.IsConfirmsComplete(ConfirmType.Delivery, ConfirmDateType.Actual))
					{
						shipment.DocsAndCartage.JP_DeliveryCartageCompleted = GetMostRecentConfirmDate(shipment, ConfirmType.Delivery, ConfirmDateType.Actual, confirm.EU_PickupDeliveryTime);
					}
					if (shipment.DocsAndCartage.JP_DeliveryRequiredBy.IsEmpty && shipment.IsConfirmsComplete(ConfirmType.Delivery, ConfirmDateType.RequestedBy))
					{
						shipment.DocsAndCartage.JP_DeliveryRequiredBy = GetMostRecentConfirmDate(shipment, ConfirmType.Delivery, ConfirmDateType.RequestedBy, confirm.EU_RequestedPickupDeliveryTime);
					}
					if (shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty && shipment.IsConfirmsComplete(ConfirmType.Delivery, ConfirmDateType.Planned))
					{
						shipment.DocsAndCartage.JP_EstimatedDelivery = GetMostRecentConfirmDate(shipment, ConfirmType.Delivery, ConfirmDateType.Planned, confirm.EU_PlannedPickupDeliveryTime);
					}
				}

				shipment.RefreshBinding();
			}
		}

		#region Implementation

		static ZDateTime GetMostRecentConfirmDate(CommonShipment shipment, ConfirmType confirmType, ConfirmDateType dateType, ZDateTime date)
		{
			ZDateTime mostRecentDate = date;
			var confirms = new List<CommonPickupDeliveryConfirm>();

			if (shipment.ShowContainerisedConfirms(confirmType))
			{
				var containers = (confirmType == ConfirmType.Pickup) ? shipment.DepartureContainers : shipment.ArrivalContainers;
				foreach (CommonContainer container in containers)
				{
					var confirm = (confirmType == ConfirmType.Pickup) ? container.OriginConfirm : container.DestinationConfirm;
					confirms.Add(confirm);
				}
			}
			else
			{
				var shipmentConfirms = (confirmType == ConfirmType.Pickup) ? shipment.PickupConfirms : shipment.DeliveryConfirms;
				confirms.AddRange(shipmentConfirms);
			}

			string confirmDateFieldName = GetConfirmDateFieldName(dateType);
			foreach (var confirm in confirms)
			{
				ZDateTime confirmDate = (ZDateTime)confirm[confirmDateFieldName];
				if ((confirmDate.IsValid && confirmDate > mostRecentDate) || mostRecentDate.IsEmpty)
				{
					mostRecentDate = confirmDate;
				}
			}

			return mostRecentDate;
		}

		static void SetShipmentConfirms(CommonShipment shipment, ConfirmType confirmType, ConfirmDateType dateType, ZDateTime oldValue, ZDateTime newValue, bool autoCreateLooseConfirmations)
		{
			if (shipment.ShowContainerisedConfirms(confirmType))
			{
				SetShipmentContainersConfirms(shipment, confirmType, dateType, oldValue, newValue);
			}
			else
			{
				SetShipmentLooseConfirms(shipment, confirmType, dateType, oldValue, newValue, autoCreateLooseConfirmations);
			}
		}

		static void SetShipmentContainersConfirms(CommonShipment shipment, ConfirmType confirmType, ConfirmDateType dateType, ZDateTime oldValue, ZDateTime newValue)
		{
			foreach (CommonContainer container in shipment.Containers.ToArray())
			{
				var confirm = (confirmType == ConfirmType.Pickup) ? container.OriginConfirm : container.DestinationConfirm;
				SetConfirmDate(confirm, dateType, oldValue, newValue);
			}
		}

		static void SetShipmentLooseConfirms(CommonShipment shipment, ConfirmType confirmType, ConfirmDateType dateType, ZDateTime oldValue, ZDateTime newValue, bool autoCreateLooseConfirmations)
		{
			if (IsDateValidOrEmpty(newValue))
			{
				var confirms = (confirmType == ConfirmType.Pickup) ? shipment.PickupConfirms : shipment.DeliveryConfirms;
				foreach (var confirm in confirms)
				{
					SetConfirmDate(confirm, dateType, oldValue, newValue);
				}

				if (!newValue.IsEmpty && shipment.OuterPackLines.Count > 0)
				{
					if (confirms.Count == 0)
					{
						if (autoCreateLooseConfirmations)
						{
							SetLooseConfirm(shipment, confirms.AddNew(), confirmType, dateType, newValue);
						}
					}
					else if (!shipment.IsConfirmsComplete(confirmType, dateType) && shipment.HasUndeliveredPackages(confirmType)) // Checking for undelivered packages to avoid bugger when Delivered > Booked
					{
						// WI00748370 - Remove error logging to resolve performance issues in CS01594069
						//string debugInfoBeforeFinalConfirm = string.Empty;
						//debugInfoBeforeFinalConfirm += string.Format(System.Globalization.CultureInfo.InvariantCulture, "Confirm Type: '{0}', Date Type: '{1}', Old Date: '{2}', New Date: '{3}'{4}", confirmType, dateType, oldValue, newValue, System.Environment.NewLine);

						//foreach (PackLine packLine in shipment.OuterPackLines)
						//{
						//	debugInfoBeforeFinalConfirm += "Packline " + packLine.PK.ToString() + "\r\n";
						//	debugInfoBeforeFinalConfirm += "Packages Confirmed: " + packLine.PackagesConfirmed(confirms) + "\r\n";
						//	debugInfoBeforeFinalConfirm += "Packages to Deliver: " + packLine.JL_Calc_PackagesToDeliver + "\r\n";
						//	debugInfoBeforeFinalConfirm += "+=+=+=+=+=+=+=+\r\n";
						//}

						var finalConfirm = confirms.AddNew();
						if (finalConfirm.TotalDeliveredPackages > 0)
						{
							SetLooseConfirm(shipment, finalConfirm, confirmType, dateType, newValue);
						}
						else
						{
							#region Report Error

							// WI00748370 - Remove error logging to resolve performance issues in CS01594069
							//ZStringBuilder packLineData = new ZStringBuilder();
							//foreach (PackLine packLine in shipment.OuterPackLines)
							//{
							//	packLineData.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture,
							//		"Packline ({0}) Packages: {1}, Packline Outturn: {2}, Use Outturn: {3}, Packages Confirmed: {4}, Packages to Deliver:{5}\r\n",
							//		packLine.PK.ToString(),
							//		packLine.JL_PackageCount,
							//		packLine.JL_Outturn,
							//		packLine.UseOutturn,
							//		packLine.PackagesConfirmed(confirms),
							//		packLine.JL_Calc_PackagesToDeliver));
							//}

							//string dateFieldName = GetConfirmDateFieldName(dateType);

							//foreach (CommonPickupDeliveryConfirm confirm in confirms)
							//{
							//	string isFinalConfirm = confirm.PK == finalConfirm.PK ? "*" : "";
							//	packLineData.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture,
							//		"{0}Confirm Total Booking Packages: {1}, Packline Total Booking Packages: {2}",
							//		isFinalConfirm,
							//		confirm.TotalBookedPackages,
							//		confirm.TotalDeliveredPackages));

							//	packLineData.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture,
							//		"Confirm Is Containerised: {0}",
							//		confirm.IsContainerised));

							//	packLineData.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture,
							//		"Date Field: '{0}', Value: '{1}'", dateFieldName, (ZDateTime)confirm[dateFieldName]));

							//	foreach (CommonConfirmDivot divot in confirm.Divots)
							//	{
							//		packLineData.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture,
							//			" - Divot PackLine ({0}) Packages Delivered: {1}",
							//			divot.J8_JL, divot.J8_PackagesDelivered));
							//	}
							//}

							//string message = "Confirms were not complete\n";
							//message += "Data\n";
							//message += debugInfoBeforeFinalConfirm;
							//message += packLineData.ToStringWithNewLineBetweenAppends();
							//message += "-=-=-=-=-=-=-=-=-=-=-";

							ErrorReporter.ReportOnce("SetShipmentLooseConfirmsWithData", "details suppressed");

							#endregion

							finalConfirm.Delete();
						}
					}
				}
			}
		}

		static void SetLooseConfirm(CommonShipment shipment, CommonPickupDeliveryConfirm confirm, ConfirmType confirmType)
		{
			SetLooseConfirm(shipment, confirm, confirmType, ConfirmDateType.Planned, ZDateTime.Empty);
		}

		static void SetLooseConfirm(CommonShipment shipment, CommonPickupDeliveryConfirm confirm, ConfirmType confirmType, ConfirmDateType dateTypeToExclude, ZDateTime newValue)
		{
			foreach (ConfirmDateType dateType in Enum.GetValues(typeof(ConfirmDateType)))
			{
				if (!newValue.IsEmpty && dateType == dateTypeToExclude)
				{
					SetConfirmDate(confirm, dateType, ZDateTime.Empty, newValue);
				}
				else
				{
					ZDateTime cartageDate = (ZDateTime)shipment.DocsAndCartage[GetCartageDateFieldName(confirmType, dateType)];
					if (cartageDate.IsValidSmallDateTime)
					{
						SetConfirmDate(confirm, dateType, ZDateTime.Empty, cartageDate);
					}
				}
			}
		}

		static bool HasValidCartageDate(CommonShipment shipment, ConfirmType confirmType)
		{
			switch (confirmType)
			{
				case ConfirmType.Pickup:
					return shipment.DocsAndCartage.JP_EstimatedPickup.IsValidSmallDateTime
						|| shipment.DocsAndCartage.JP_PickupRequiredBy.IsValidSmallDateTime
						|| shipment.DocsAndCartage.JP_PickupCartageCompleted.IsValidSmallDateTime;
				case ConfirmType.Delivery:
					return shipment.DocsAndCartage.JP_EstimatedDelivery.IsValidSmallDateTime
						|| shipment.DocsAndCartage.JP_DeliveryRequiredBy.IsValidSmallDateTime
						|| shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsValidSmallDateTime;
				default:
					return false;
			}
		}

		static bool IsDateValidOrEmpty(ZDateTime date)
		{
			return date.IsEmpty || date.IsValidSmallDateTime;
		}

		static void SetConfirmDate(CommonPickupDeliveryConfirm confirm, ConfirmDateType dateType, ZDateTime oldValue, ZDateTime newValue)
		{
			string confirmDateFieldName = GetConfirmDateFieldName(dateType);
			ZDateTime confirmDate = (ZDateTime)confirm[confirmDateFieldName];
			if (confirmDate.IsEmpty || confirmDate == oldValue)
			{
				confirm[confirmDateFieldName] = newValue;
			}
		}

		#endregion
	}
}

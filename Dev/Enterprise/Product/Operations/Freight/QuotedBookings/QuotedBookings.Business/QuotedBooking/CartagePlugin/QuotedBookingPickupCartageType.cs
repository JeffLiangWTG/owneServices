using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingPickupCartageType : CartageType
	{
		public QuotedBookingPickupCartageType(QuotedBooking quotedBooking) : base(quotedBooking) { }

		protected QuotedBooking QuotedBookingParent
		{
			get { return (QuotedBooking)CartageParent; }
		}

		JobSailing Sailing
		{
			get { return QuotedBookingParent.ScheduleChooser.Sailing; }
		}

		JobDocsAndCartage DocsAndCartage
		{
			get { return QuotedBookingParent.Booking.DocsAndCartage; }
		}

		public override ZString PortOfLoading
		{
			get { return Sailing != null ? Sailing.JX_JA_RL_NKPortOfLoading : ZString.Empty; }
		}

		public override ZString PortOfDischarge
		{
			get { return Sailing != null ? Sailing.JX_JB_RL_NKPortOfDischarge : ZString.Empty; }
		}

		public override ZString Vessel
		{
			get { return Sailing != null ? Sailing.JX_JV_NKVessel : ZString.Empty; }
		}

		public override ZString VoyageFlight
		{
			get { return Sailing != null ? Sailing.JX_JV_VoyageFlight : ZString.Empty; }
		}

		public override ZDateTime E_ARV
		{
			get { return Sailing != null ? Sailing.JX_JB_E_ARV : ZDateTime.Empty; }
		}

		public override ZDateTime E_DEP
		{
			get { return Sailing != null ? Sailing.JX_JA_E_DEP : ZDateTime.Empty; }
		}

		public override ZDateTime A_ARV
		{
			get { return Sailing != null ? Sailing.JX_JB_A_ARV : ZDateTime.Empty; }
		}

		public override ZDateTime A_DEP
		{
			get { return Sailing != null ? Sailing.JX_JA_A_DEP : ZDateTime.Empty; }
		}

		public override ZDateTime FCLCutOff
		{
			get { return Sailing != null ? Sailing.JX_JA_CTOCutOff : ZDateTime.Empty; }
		}

		public override ZDateTime FCLReceivalCommences
		{
			get { return Sailing != null ? Sailing.JX_JA_CTOReceivalCommences : ZDateTime.Empty; }
		}

		public override ZDateTime LCLCutOff
		{
			get { return Sailing != null ? Sailing.JX_DepotCutOff : ZDateTime.Empty; }
		}

		public override ZDateTime LCLReceivalCommences
		{
			get { return Sailing != null ? Sailing.JX_DepotReceivalCommences : ZDateTime.Empty; }
		}

		public override ZDateTime FCLAvailabilityDate
		{
			get
			{
				ZDateTime result;
				if (!DocsAndCartage.JP_FCLAvailable.IsEmpty)
				{
					result = DocsAndCartage.JP_FCLAvailable;
				}
				else
				{
					result = Sailing != null ? Sailing.JX_JB_CTOAvailabilityDate : ZDateTime.Empty;
				}
				return result;
			}
		}

		public override ZDateTime FCLStorageDate
		{
			get
			{
				ZDateTime result;
				if (!DocsAndCartage.JP_FCLStorageCommences.IsEmpty)
				{
					result = DocsAndCartage.JP_FCLStorageCommences;
				}
				else
				{
					result = Sailing != null ? Sailing.JX_JB_CTOStorageDate : ZDateTime.Empty;
				}
				return result;
			}
		}

		public override ZDateTime LCLAvailabilityDate
		{
			get
			{
				ZDateTime result;
				if (!DocsAndCartage.JP_LCLAvailable.IsEmpty)
				{
					result = DocsAndCartage.JP_LCLAvailable;
				}
				else
				{
					result = Sailing != null ? Sailing.JX_DepotAvailabilityDate : ZDateTime.Empty;
				}
				return result;
			}
		}

		public override ZDateTime LCLStorageDate
		{
			get
			{
				ZDateTime result;
				if (!DocsAndCartage.JP_LCLStorageCommences.IsEmpty)
				{
					result = DocsAndCartage.JP_LCLStorageCommences;
				}
				else
				{
					result = Sailing != null ? Sailing.JX_DepotStorageDate : ZDateTime.Empty;
				}
				return result;
			}
		}

		public override MultilingualString Description
		{
			get { return ResString.GetMultilingualString("59a4654e-76ac-47dc-8257-4b19a62679c3", "Pickup"); }
		}

		public override OrgAddress LocalTransportProviderAddress
		{
			get { return DocsAndCartage.PickupCartageCoAddr; }
		}

		public override ZPropertyInfo CartageAddressInfo
		{
			get { return DocsAndCartage.JP_OA_PickupCartageCoAddrInfo; }
		}

		public override ZString CartageJobType
		{
			get
			{
				if (QuotedBookingParent.Booking.IsDomestic())
				{
					if (QuotedBookingParent.Booking.IsLoose || QuotedBookingParent.Booking.IsRoad)
					{
						return Constants.CartageJobType.NEW_DomesticLoosePickup;
					}
					else
					{
						return Constants.CartageJobType.NEW_DomesticContainerizedPickup;
					}
				}
				else
				{
					if (QuotedBookingParent.Booking.IsAir)
					{
						return Constants.CartageJobType.NEW_AirExport;
					}
					else if (QuotedBookingParent.Booking.IsLoose || QuotedBookingParent.Booking.IsRoad)
					{
						return Constants.CartageJobType.NEW_LCLExport;
					}
					else if (QuotedBookingParent.Booking.IsContainerised)
					{
						if (!GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CFS).IsEmpty)
						{
							return Constants.CartageJobType.NEW_FCLExportPack;
						}
						else
						{
							return Constants.CartageJobType.NEW_FCLExportToSHP;
						}
					}
				}

				return "";
			}
		}

		public override JobDocAddress GetCartageAddress(ZString orgType)
		{
			switch (orgType)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					return QuotedBookingParent.Booking.GetDepartureCFSDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return QuotedBookingParent.Booking.GetDepartureCFSDocAddress;   //This is deliberate as CTO and CFS addresses are intertwined on the GUI
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return QuotedBookingParent.Booking.GetConsigneeDeliveryDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return QuotedBookingParent.Booking.GetConsignorDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return QuotedBookingParent.Booking.GetDepartureContainerYardDocAddress;
				default:
					return null;
			}
		}

		public override IReadOnlyCollection<ICartageContainer> CartageContainers
		{
			get { return (ICartageContainer[])QuotedBookingParent.QuotedBookingContainers.ToArray(typeof(ICartageContainer)); }
		}

		public override IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo
		{
			get { return (ICartageLooseCargo[])QuotedBookingParent.Booking.OuterPackLines.ToArray(typeof(ICartageLooseCargo)); }
		}

		protected override ZPropertyInfo[] GetCartageAddressInfosToMonitor()
		{
			return new ZPropertyInfo[] { QuotedBookingParent.Booking.JS_OA_ExportReceivingDepotInfo };
		}

		public override void CartageAdvised(BusinessObjectFactory saveInFactory)
		{
			ZGuid bookingPK = QuotedBookingParent.Booking != null ? QuotedBookingParent.Booking.PK : ZGuid.Empty;
			ZGuid quotePK = QuotedBookingParent.Quote != null ? QuotedBookingParent.Quote.PK : ZGuid.Empty;

			QuotedBooking quotedBookingInFactory = QuotedBooking.New(quotePK, bookingPK, saveInFactory);
			if (quotedBookingInFactory != null)
			{
				quotedBookingInFactory.Booking.DocsAndCartage.JP_PickupCartageAdvisedInfo.Value = ZDateTime.Now;
			}
		}

		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
		}

		public override void SetTotalDemurrage(TimeSpan demurrage)
		{
		}

		public override ZDateTime EstimatedCartagePickup
		{
			get { return DocsAndCartage.JP_EstimatedPickup; }
		}

		public override ZDateTime EstimatedCartageDelivery
		{
			get { return DocsAndCartage.JP_PickupRequiredBy; }
		}

		public override ZString DropMode
		{
			get { return DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
		}

		public override IEnumerable<ZString> GetMatchingDirectionCodes()
		{
			return new ZString[] { CartageDirection.Export, CartageDirection.Origin };
		}
	}
}

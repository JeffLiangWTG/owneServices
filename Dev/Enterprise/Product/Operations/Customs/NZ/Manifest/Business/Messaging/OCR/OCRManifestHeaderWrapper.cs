using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class OCRManifestHeaderWrapper : IOutwardCargoReportHeader
	{
		public OCRManifestHeaderWrapper(AsycudaManifestHeader manifestHeader, IAdditionalInformation additionalMessageInformation)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, "ManifestHeader cannot be null");
			this.AdditionalInformation = additionalMessageInformation;
		}

		readonly AsycudaManifestHeader manifestHeader;

		public IOrganisationSimple Consolidator => OrgHeaderWrapper.New(GlbCompany.CurrentCompany.OrgProxy);

		public ZBool IsSea => manifestHeader.IsSea;

		public ZBool IsConsolidation
		{
			get
			{
				var result = true;
				var shippingLine = manifestHeader.Carrier?.Header;
				if (shippingLine != null && (shippingLine.PK == GlbCompany.CurrentCompany.OrgProxy.PK || shippingLine.PK == GlbBranch.CurrentBranch.OrgProxy.PK))
				{
					result = false;
				}

				return result;
			}
		}

		public ZString TSWReferenceNumber => manifestHeader.RegistrationNumber;

		public ZString SenderReferenceNumber => manifestHeader.AMA_JobReference;

		public ZString MasterBillNumber => manifestHeader.MasterBill?.ABL_BillNumber ?? ZString.Empty;

		public ZString CraftName
		{
			get
			{
				if (!craftName.HasValue)
				{
					craftName = manifestHeader.IsSea ? manifestHeader.AMA_VesselName : ZString.Empty;
				}
				return craftName.Value;
			}
		}
		ZString? craftName;

		public ZString LloydsNo
		{
			get
			{
				var result = ZString.Empty;
				if (!CraftName.IsEmpty)
				{
					var vessel = new RefVessel.Loader(manifestHeader.Factory).LoadUnique(CraftName, ZString.Empty, ZString.Empty, ZString.Empty);
					if (vessel != null)
					{
						result = vessel.RV_LloydsNumber;
					}
				}

				return result;
			}
		}

		public ZString VoyageNo => manifestHeader.AMA_Voyage;

		public ZString FlightNo => manifestHeader.AMA_Voyage;

		public ZDateTime DepartureDate => manifestHeader.AMA_E_DEP;

		public IEnumerable<ZString> RoutingCountryCodes => new[] { manifestHeader.AMA_RL_NKPortOfDischarge.Left(2) };

		public IOrganisationSimple Carrier => OrgHeaderWrapper.New(manifestHeader.Carrier?.Header);

		public ZString PortOfDeparture => manifestHeader.AMA_RL_NKPortOfLoading;

		public IEnumerable<IOCRConsignment> OCRLines
		{
			get
			{
				foreach (AsycudaBill bill in manifestHeader.Bills)
				{
					yield return new OCRBillWrapper(bill);
				}
			}
		}

		public IEnumerable<ITransportEquipment> Containers
		{
			get
			{
				var header = manifestHeader;
				if (!header.IsAir && header.HasContainers)
				{
					foreach (AsycudaContainer container in manifestHeader.Containers)
					{
						yield return new ContainerWrapper(container);
					}
				}
			}
		}

		IEnumerable<ZString> IOutwardCargoReportHeader.NotifyPartyCodes
		{
			get
			{
				var notifyPartyBizO = manifestHeader.DeliveryNotificationParty;
				if (notifyPartyBizO.E2_OA_DeliveryNotificationParty_ZAddress != null)
				{
					var notifyParty = notifyPartyBizO.E2_OA_DeliveryNotificationParty_ZAddress.OrgHeader as OrgHeader;
					var notifyPartyAddress = notifyPartyBizO.E2_OA_DeliveryNotificationParty_ZAddress.OrgAddress as OrgAddress;

					var codeTypes = new[] { OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, OrgCusCode.CodeTypes.CustomsClientCode };
					foreach (var codeType in codeTypes)
					{
						var number = GetCustomsRegNo(notifyParty, notifyPartyAddress, codeType);
						if (!number.IsEmpty)
						{
							yield return number;
						}
					}
				}

				if (!notifyPartyBizO.DeliveryNotificationPartyPort.IsEmpty)
				{
					yield return notifyPartyBizO.DeliveryNotificationPartyPort;
				}
			}
		}

		ZString GetCustomsRegNo(OrgHeader notifyParty, OrgAddress notifyPartyAddress, ZString code)
		{
			var result = notifyPartyAddress?.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.NewZealand) ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = notifyParty?.CustomsCodes.GetCustomsRegNo(code, Core.Constants.CountryCodes.NewZealand, ZGuid.Empty) ?? ZString.Empty;
			}
			return result;
		}

		ZString IOutwardCargoReportHeader.NotifyPartyName => manifestHeader.DeliveryNotificationParty.DeliveryNotificationPartyName;

		ZString IOutwardCargoReportHeader.NotifyPartyEmail => manifestHeader.DeliveryNotificationParty.DeliveryNotificationPartyEmail;

		public IAdditionalInformation AdditionalInformation { get; }
	}
}

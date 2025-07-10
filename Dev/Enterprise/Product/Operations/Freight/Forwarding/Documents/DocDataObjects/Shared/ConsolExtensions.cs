using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class ConsolExtensions
	{
		public static Address GetPickupFromAddress(this ForwardingConsol consol, IContext context, bool isPickup, bool includedContactDetails = true)
		{
			if (consol == null)
			{
				return AddressBuilder.Create(context, (OrgAddress)null);
			}

			if (isPickup)
			{
				var shipment = consol.Shipments.Cast<ForwardingShipment>().FirstOrDefault();
				if (shipment?.ExportReceivingDepot != null)
				{
					return AddressBuilder.Create(context, shipment?.ExportReceivingDepot, includedContactDetails);
				}
				else if (shipment?.ConsignorPickupAddress != null && !shipment.ConsignorPickupAddress.IsEmpty)
				{
					return AddressBuilder.Create(context, shipment?.ConsignorPickupAddress, includedContactDetails);
				}
			}

			return AddressBuilder.Create(context, consol.PackDepotAddress, includedContactDetails);
		}

		public static Address GetDeliverToAddress(this ForwardingConsol consol, IContext context, bool isDelivery, bool includedContactDetails = true)
		{
			if (consol == null)
			{
				return AddressBuilder.Create(context, (OrgAddress)null);
			}

			if (isDelivery)
			{
				var shipment = consol.Shipments.Cast<ForwardingShipment>().FirstOrDefault();
				if (shipment?.ImportReleaseDepot != null)
				{
					return AddressBuilder.Create(context, shipment?.ImportReleaseDepot, includedContactDetails);
				}
				else if (shipment?.ConsigneeDeliveryAddress != null && !shipment.ConsigneeDeliveryAddress.IsEmpty)
				{
					return AddressBuilder.Create(context, shipment?.ConsigneeDeliveryAddress, includedContactDetails);
				}
			}

			return AddressBuilder.Create(context, consol.UnpackDepotAddress, includedContactDetails);
		}

		public static Address GetCustomsBrokerAddress(this ForwardingConsol consol, IContext context)
		{
			if (consol != null && consol.IsDirect)
			{
				OrgHeader GetOrgHeader()
				{
					if (consol.DirectShipment?.GetDeclaration() is BaseJobDeclaration jobDeclaration && jobDeclaration.ExternalBroker?.MainAddress != null)
					{
						return jobDeclaration.ExternalBroker;
					}

					if (consol.DirectShipment?.ExportBroker?.MainAddress != null)
					{
						return consol.DirectShipment.ExportBroker;
					}

					if ((GlbBranch.CurrentBranch.OrgProxy?.OH_IsBroker ?? false) && GlbBranch.CurrentBranch.OrgProxy.MainAddress != null)
					{
						return GlbBranch.CurrentBranch.OrgProxy;
					}

					if ((GlbCompany.CurrentCompany.OrgProxy?.OH_IsBroker ?? false) && GlbCompany.CurrentCompany.OrgProxy.MainAddress != null)
					{
						return GlbCompany.CurrentCompany.OrgProxy;
					}

					return null;
				}

				OrgHeader orgHeader = GetOrgHeader();

				if (orgHeader != null)
				{
					var allDocs = orgHeader.GetActiveContacts().Cast<OrgContact>()
						.SelectMany(c => c.Documents).Cast<OrgDocument>()
						.Select(document =>
						{
							if (document.OD_DocumentGroup == ContactType.ExportBroker
								&& (document.OD_FilterShipmentMode == TransportModes.Sea || document.OD_FilterShipmentMode == TransportModes.All)
							)
							{
								var score = 32 + (document.OD_FilterShipmentMode == TransportModes.Sea ? 16 : 8);

								var local = document.OD_FilterLocalPort;
								if (local.IsEmpty)
								{
									score++;
									return (score, document);
								}

								if (local.Length == 2 && local == consol.JK_RL_NKLoadPort.Substring(0, 2))
								{
									score += 2;
									return (score, document);
								}

								if (local == consol.JK_RL_NKLoadPort)
								{
									score += 4;
									return (score, document);
								}
							}

							return (0, document);
						}).OrderByDescending(result => result.Item1);

					var contact = allDocs.FirstOrDefault(d => d.Item1 > 0).document?.Contact;

					return AddressBuilder.Create(context, orgHeader, contact);
				}
			}

			return AddressBuilder.Create(context, (OrgAddress)null);
		}

		public static bool IsDoorPickup(this ForwardingConsol consol)
		{
			return consol?
			.Containers
			.OfType<ForwardingContainer>()
			.Any(c => c.JC_DeliveryMode == Constants.DeliveryModes.Codes.CFS_CFS
				|| c.JC_DeliveryMode == Constants.DeliveryModes.Codes.CFS_CY) ?? false;
		}

		public static bool IsDoorDelivery(this ForwardingConsol consol)
		{
			return consol?
			.Containers
			.OfType<ForwardingContainer>()
			.Any(c => c.JC_DeliveryMode == Constants.DeliveryModes.Codes.CFS_CFS
				|| c.JC_DeliveryMode == Constants.DeliveryModes.Codes.CY_CFS) ?? false;
		}

		public static bool IsFCLOrLCL(this ForwardingConsol consol)
		{
			if (consol == null)
			{
				return false;
			}

			return (consol.JK_ConsolMode == Constants.ContainerModes.FCL || consol.JK_ConsolMode == Constants.ContainerModes.LCL)
				|| !consol.IsCoLoad
					&& (consol.JK_ConsolMode == Constants.ContainerModes.Groupage
					|| consol.JK_ConsolMode == Constants.ContainerModes.BuyersConsol
					|| consol.JK_ConsolMode == Constants.ContainerModes.Other && consol.Containers.Any());
		}

		public static bool IsPickup(this ForwardingConsol consol, bool isPickup)
		{
			return isPickup
				&& (consol.IsDirect
				|| consol.IsFCLOrLCL()
				&& (DoAllConsolShipmentsHaveSameAddress(consol, s => s.ConsignorPickupAddress) && DoAllConsolShipmentsHaveSameAddress(consol, s => s.ExportReceivingDepot)));
		}

		public static bool IsDeliver(this ForwardingConsol consol, bool isDelivery)
		{
			return isDelivery
				&& (consol.IsDirect
				|| consol.IsFCLOrLCL()
				&& (DoAllConsolShipmentsHaveSameAddress(consol, s => s.ConsigneeDeliveryAddress) && DoAllConsolShipmentsHaveSameAddress(consol, s => s.ImportReleaseDepot)));
		}

		#region Implementation

		static bool DoAllConsolShipmentsHaveSameAddress(ForwardingConsol consol, Func<ForwardingShipment, JobDocAddress> getShipmentAddress)
		{
			var shipments = consol?.Shipments.OfType<ForwardingShipment>();

			if (!shipments.Any())
			{
				return false;
			}

			var firstAddress = getShipmentAddress(shipments.First());
			if (!firstAddress.HasRealAddress)
			{
				return shipments
					.Skip(1)
					.All(s => !getShipmentAddress(s).HasRealAddress);
			}

			return shipments
				.Skip(1)
				.All(s => getShipmentAddress(s).E2_OA_Address == firstAddress.E2_OA_Address);
		}

		static bool DoAllConsolShipmentsHaveSameAddress(ForwardingConsol consol, Func<ForwardingShipment, OrgAddress> getShipmentAddress)
		{
			var shipments = consol?.Shipments.OfType<ForwardingShipment>();

			if (!shipments.Any())
			{
				return false;
			}

			var firstAddress = getShipmentAddress(shipments.First());
			if (firstAddress == null)
			{
				return shipments
					.Skip(1)
					.All(s => getShipmentAddress(s) == null);
			}

			return shipments
				.Skip(1)
				.All(s => (getShipmentAddress(s)?.PK ?? ZGuid.Empty) == firstAddress.PK);
		}

		public static ZString GetCarrierCodeWithFallback(this ForwardingConsol consol)
		{
			return consol.GetOrgCusCodeCarrierWithFallback()?.OK_CustomsRegNo ?? ZString.Empty;
		}

		public static RefShippingLine GetRefShippingLineFromCarrierWithFallback(this ForwardingConsol consol)
		{
			var orgCusCode = consol.GetOrgCusCodeCarrierWithFallback();

			if (orgCusCode == null || (orgCusCode?.OK_CustomsRegNo ?? ZString.Empty).IsEmpty)
			{
				return null;
			}

			switch (orgCusCode.OK_CodeType)
			{
				case OrgCusCode.CodeTypes.CarrierCode:
					return consol.Factory.LoadFromNaturalKey<RefShippingLine>(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, orgCusCode.OK_CustomsRegNo);
				case OrgCusCode.CodeTypes.CargoWiseOneCarrierCode:
					return consol.Factory.LoadFromNaturalKey<RefShippingLine>(RefShippingLineSchema.RSL_CargoWiseOneCode, orgCusCode.OK_CustomsRegNo);
			}

			return null;
		}

		static OrgCusCode GetOrgCusCodeCarrierWithFallback(this ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			var org = consol.IsCoLoad ? consol.CreditorAddress : consol.ShippingLineAddress;
			var customsCodes = org?.Header.CustomsCodes;

			var usCCCOrgCusCode = customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Constants.CountryCodes.UnitedStates);
			if (!(usCCCOrgCusCode?.OK_CustomsRegNo ?? ZString.Empty).IsEmpty)
			{
				return usCCCOrgCusCode;
			}

			return customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, ZString.Empty);
		}

		#endregion
	}
}

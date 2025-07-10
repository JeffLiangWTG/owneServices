using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	/// <summary>
	/// Commonly used collections for findbox BindToLists.
	/// </summary>
	public sealed class CartageBindToLists : BindToLists
	{
		public CartageBindToLists(BusinessObjectFactory factory) : base(factory) { }

		public new static CartageBindToLists GetCachedLists(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Freight.LocalCartage.Business.CartageBindToLists", () => new CartageBindToLists(factory));
		}

		public CommonWorkSheetCollection WorkSheets
		{
			get { return workSheets ?? (workSheets = new CommonWorkSheetCollection(Factory)); }
		}
		CommonWorkSheetCollection workSheets;

		public CodeDescriptionPairList CartageAddressList(CommonCartage cartage)
		{
			var cartageAddressList = cartage != null
				? Factory.GetCachedValue("CartageBindToLists|CartageAddressList|" + cartage.PK.ToString(), () => GetCartageAddressList(cartage))
				: new CachedProperty<CodeDescriptionPairList>(Factory, () => new CodeDescriptionPairList());
			return cartageAddressList.Value;
		}

		CachedProperty<CodeDescriptionPairList> GetCartageAddressList(CommonCartage cartage)
		{
			return new CachedProperty<CodeDescriptionPairList>(Factory, delegate
			{
				var result = new CodeDescriptionPairList();

				foreach (AddressSelectionElement element in CartageAddressElements(cartage))
				{
					result.AddPair(element.DocOrOrgAddressPK, string.Format(CultureInfo.CurrentCulture, "{0} ({1}) {2}", element.OrgType, element.OrgCode, element.AddressShortCode), element.AddressAsASingleLine);
				}

				return result;
			});
		}

		public AddressSelectionElement[] CartageAddressElements(CommonCartage cartage)
		{
			var cartageAddressElements = Factory.GetCachedValue("CartageBindToLists|CartageAddressElements|" + cartage.PK.ToString(), () => GetCartageAddressElements(cartage));
			return cartageAddressElements.Value;
		}

		CachedProperty<AddressSelectionElement[]> GetCartageAddressElements(CommonCartage cartage)
		{
			return new CachedProperty<AddressSelectionElement[]>(Factory, delegate
			{
				return GetCartageAddressElementsCore(cartage);
			});
		}

		AddressSelectionElement[] GetCartageAddressElementsCore(CommonCartage cartage)
		{
			Argument.NotNull(cartage, "cartage");

			var primaryAddresses = new List<AddressSelectionElement>();
			var additionalAddresses = new List<AddressSelectionElement>();

			foreach (JobDocAddress docAddress in cartage.DocAddresses)
			{
				AddPrimaryAddresses(primaryAddresses, docAddress);
				AddAdditionalAddresses(additionalAddresses, docAddress);
			}

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
			if (orgProxy != null)
			{
				var orgProxyAddressSelections = Factory.GetCachedValue(orgProxy.OH_Code,
						() =>
						{
							var addressSelectionElements = new List<AddressSelectionElement>();
							foreach (OrgAddress orgAddress in orgProxy.Addresses.ToArray())
							{
								var codeDescriptionPairForOrgProxy = GetCodeDescriptionPairAddress(orgAddress, DocAddressType.LocalCartageCFS);
								if (codeDescriptionPairForOrgProxy != null)
								{
									addressSelectionElements.Add(codeDescriptionPairForOrgProxy);
								}
							}
							return addressSelectionElements;
						}, CacheStalenessPolicy.StaleOnFactorySave);
				additionalAddresses.AddRange(orgProxyAddressSelections);
			}

			var result = new List<AddressSelectionElement>();
			foreach (AddressSelectionElement pair in primaryAddresses)
			{
				if (result.Find(delegate(AddressSelectionElement ase)
				{ return ase.Matches(pair); }) == null)
				{
					result.Add(pair);
				}
			}

			foreach (AddressSelectionElement pair in additionalAddresses)
			{
				if (result.Find(delegate(AddressSelectionElement ase)
				{ return ase.Matches(pair); }) == null)
				{
					result.Add(pair);
				}
			}

			result.Sort(delegate(AddressSelectionElement l, AddressSelectionElement r)
			{ return l.OrgCode.CompareTo(r.OrgCode) == 0 ? l.AddressAsASingleLine.CompareTo(r.AddressAsASingleLine) : l.OrgCode.CompareTo(r.OrgCode); });
			return result.ToArray();
		}

		void AddAdditionalAddresses(List<AddressSelectionElement> additionalAddresses, JobDocAddress docAddress)
		{
			if (docAddress.Organisation != null && !docAddress.E2_AddressOverride)
			{
				var addressSelections = Factory.GetCachedValue(docAddress.Organisation.OH_Code,
					() =>
					{
						var addressSelection = new List<AddressSelectionElement>();
						foreach (OrgAddress orgAddress in docAddress.Organisation.Addresses)
						{
							var codeDescriptionPairForOrgAddress = GetCodeDescriptionPairAddress(orgAddress, docAddress.DocAddressType);
							if (codeDescriptionPairForOrgAddress != null)
							{
								addressSelection.Add(codeDescriptionPairForOrgAddress);
							}
						}

						return addressSelection;
					}, CacheStalenessPolicy.StaleOnFactorySave);
				additionalAddresses.AddRange(addressSelections);
			}
		}

		void AddPrimaryAddresses(List<AddressSelectionElement> primaryAddresses, JobDocAddress docAddress)
		{
			AddressSelectionElement pair = null;
			if (docAddress.E2_AddressOverride || !docAddress.E2_OA_Address.IsValid)
			{
				pair = GetCodeDescriptionPairAddress(docAddress);
			}
			else
			{
				var orgAddressPk = docAddress.PK;
				pair = Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "GetCartageAddressElements: {0}", orgAddressPk.ToStringKey()),
					() =>
					{
						return GetCodeDescriptionPairAddress(docAddress);
					}, CacheStalenessPolicy.StaleOnFactorySave);
			}

			if (pair != null)
			{
				primaryAddresses.Add(pair);
			}
		}

		AddressSelectionElement GetCodeDescriptionPairAddress(JobDocAddress docAddress)
		{
			if (docAddress.DocAddressType == DocAddressType.NonPersistent || (docAddress.Address != null && !docAddress.Address.OA_IsActive))
			{
				return null;
			}

			var orgCode = "";
			var addressShortCode = "";

			if (docAddress.E2_AddressOverride)
			{
				orgCode = docAddress.E2_CompanyNameTruncated;
				addressShortCode = docAddress.AddressAsASingleLine;
			}
			else if (docAddress.Organisation != null && docAddress.Address != null)
			{
				orgCode = docAddress.Organisation.OH_Code;
				addressShortCode = docAddress.Address.OA_Code;
			}

			return new AddressSelectionElement(docAddress.PK, docAddress.DocAddressType, orgCode, addressShortCode, docAddress.AddressAsASingleLine);
		}

		AddressSelectionElement GetCodeDescriptionPairAddress(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			var result = new AddressSelectionElement(orgAddress.PK, docAddressType, orgAddress.Header.OH_Code, orgAddress.OA_Code, orgAddress.AddressAsASingleLine);
			if (!orgAddress.OA_IsActive)
			{
				result = null;
			}
			return result;
		}

		public class AddressSelectionElement
		{
			public AddressSelectionElement(ZGuid docOrOrgAddressPK, DocAddressType docAddressType, ZString orgCode, ZString addressShortCode, ZString addressAsASingleLine)
			{
				DocOrOrgAddressPK = docOrOrgAddressPK;
				OrgType = CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(docAddressType);
				OrgTypeDescription = CommonCartageAddressHelper.GetOrgTypeDescriptionFromCartageDocAddressType(docAddressType);
				OrgCode = orgCode;
				AddressShortCode = addressShortCode;
				AddressAsASingleLine = addressAsASingleLine;
			}

			public ZGuid DocOrOrgAddressPK { get; set; }
			public ZString OrgType { get; set; }
			public ZString OrgTypeDescription { get; set; }
			public ZString OrgCode { get; set; }
			public ZString AddressShortCode { get; set; }
			public ZString AddressAsASingleLine { get; set; }

			public ZBool Matches(AddressSelectionElement compareElement)
			{
				return DocOrOrgAddressPK == compareElement.DocOrOrgAddressPK
					|| (OrgType == compareElement.OrgType
						&& OrgTypeDescription == compareElement.OrgTypeDescription
						&& OrgCode == compareElement.OrgCode
						&& AddressShortCode == compareElement.AddressShortCode
						&& AddressAsASingleLine == compareElement.AddressAsASingleLine);
			}
		}

		public CodeDescriptionPairList CartageContainerModes
		{
			get
			{
				if (containerModes == null)
				{
					containerModes = new CodeDescriptionPairList();
					containerModes.AddPair(Constants.CartageContainerMode.Containerized, Constants.CartageContainerModeDescription.Containerized);
					containerModes.AddPair(Constants.CartageContainerMode.Loose, Constants.CartageContainerModeDescription.Loose);
					containerModes.AddPair(Constants.CartageContainerMode.Mixed, Constants.CartageContainerModeDescription.Mixed);
				}
				return containerModes;
			}
		}
		CodeDescriptionPairList containerModes;

		public CodeDescriptionPairList ParentJobTypes
		{
			get
			{
				if (parentJobTypes == null)
				{
					parentJobTypes = new CodeDescriptionPairList();
					parentJobTypes.AddPair(StandAloneCartageCode, Res.GetString("5d555535-8c36-4aca-b0e5-c864afad3d14", "Standalone Cartage"));
					parentJobTypes.AddPair(TransportBookingCode, Res.GetString("69b3da2a-3576-4e3e-b5ce-7eb21021a355", "Transport Booking"));
					parentJobTypes.AddPair(CustomsDeclarationCode, Res.GetString("32a2cf3f-061d-44f0-b47c-a855606abb79", "Customs Declaration"));
					parentJobTypes.AddPair(ForwardingShipmentCode, Res.GetString("6f183c7c-31c9-4b9b-a7dc-942edc643f04", "Forwarding Shipment"));
					parentJobTypes.AddPair(CFSShipmentCode, Res.GetString("8c65c093-4ea7-4656-846b-2a912c890745", "CFS Shipment"));
					parentJobTypes.AddPair(CFSLoadListConsolCode, Res.GetString("e93890f8-6feb-4c11-92ef-26f7d5c643ab", "CFS Load List Consolidation"));
					parentJobTypes.AddPair(WarehouseOrderCode, Res.GetString("1b20ed03-d4c3-4cd0-8bd2-47825b6f5757", "Warehouse Order"));
				}
				return parentJobTypes;
			}
		}
		CodeDescriptionPairList parentJobTypes;

		public const string StandAloneCartageCode = "STC";
		public const string TransportBookingCode = "TBK";
		public const string CustomsDeclarationCode = "CUS";
		public const string ForwardingShipmentCode = "SHP";
		public const string CFSShipmentCode = "CFS";
		public const string CFSLoadListConsolCode = "CFC";
		public const string WarehouseOrderCode = "WHO";
	}
}

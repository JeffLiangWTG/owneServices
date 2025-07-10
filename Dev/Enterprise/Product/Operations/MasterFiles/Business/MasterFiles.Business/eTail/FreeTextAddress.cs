using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using ConsignmentAddressType =  Enterprise.Integration.Customs.ConsignmentAddressType;

namespace Enterprise.MasterFiles.Business;

/// <summary>
/// Generic representation of a free-text address from a consignment
/// </summary>
/// <typeparam name="T">Type that implements IConsignmentAddressProvider</typeparam>
public class FreeTextAddress<T> : NonPersistentBusinessObject where T : BusinessObject, Enterprise.Integration.Customs.IConsignmentAddressProvider
{
	public static class Schema
	{
		public const string WaybillNumber = "WaybillNumber";
		public const string PartyType = "PartyType";
		public const string PartyName = "PartyName";
		public const string Address1 = "Address1";
		public const string Address2 = "Address2";
		public const string City = "City";
		public const string State = "State";
		public const string Postcode = "Postcode";
		public const string Country = "Country";
		public const string Phone = "Phone";
		public const string Mobile = "Mobile";
		public const string Fax = "Fax";
		public const string Email = "Email";
	}

	public FreeTextAddress(BusinessObjectFactory factory) : base(factory)
	{
	}

	public FreeTextAddress(T consignment, ConsignmentAddressType addressType) : base(consignment.Factory)
	{
		Consignment = Argument.NotNull(consignment, nameof(consignment));
		RegisterEditableChildObject(Consignment);
		AddressType = addressType;

		if (addressType == ConsignmentAddressType.Return)
		{
			throw new DeveloperNotificationException("Return address is not supported.");
		}

		WaybillNumber = consignment.WaybillNumber;

		switch (AddressType)
		{
			case ConsignmentAddressType.Consignee:
				PartyType = Res.GetString("4e0953bf-0f5c-4a38-a566-4e53505c37d4", "Consignee");
				PartyName = Consignment.ConsigneeName;
				Address1 = Consignment.ConsigneeAddress1;
				Address2 = Consignment.ConsigneeAddress2;
				City = Consignment.ConsigneeCity;
				State = Consignment.ConsigneeState;
				Postcode = Consignment.ConsigneePostcode;
				Country = Consignment.ConsigneeCountryCode;
				Phone = Consignment.ConsigneePhone;
				Mobile = Consignment.ConsigneeMobile;
				Fax = Consignment.ConsigneeFax;
				Email = Consignment.ConsigneeEmail;
				break;
			case ConsignmentAddressType.Shipper:
				PartyType = Res.GetString("0138b77f-f3f4-4325-ad6b-6093a9353405", "Shipper");
				PartyName = Consignment.ShipperName;
				Address1 = Consignment.ShipperAddress1;
				Address2 = Consignment.ShipperAddress2;
				City = Consignment.ShipperCity;
				State = Consignment.ShipperState;
				Postcode = Consignment.ShipperPostcode;
				Country = Consignment.ShipperCountryCode;
				Phone = Consignment.ShipperPhone;
				Mobile = Consignment.ShipperMobile;
				Fax = Consignment.ShipperFax;
				Email = Consignment.ShipperEmail;
				break;
		}
	}

	public T Consignment { get; }

	public bool HasSelectedAddress =>
		Consignment != null
		&& ((AddressType == ConsignmentAddressType.Consignee && !Consignment.ConsigneeAddressId.IsEmpty)
			|| (AddressType == ConsignmentAddressType.Shipper && !Consignment.ShipperAddressId.IsEmpty));

	public ConsignmentAddressType AddressType { get; }

	public ZBool Ignored { get; set; }

	public ZBool AddressSettled => Ignored || HasSelectedAddress;

	public OrgPatternMatchCollection SimilarOrgMatches => Factory.GetCachedValue(AddressKey, FindSimilarOrg, CacheStalenessPolicy.StaleOnFactorySave);

	OrgPatternMatchCollection FindSimilarOrg()
	{
		var organization = Factory.New<OrgHeader>();
		FillOrgHeaderWithConsignmentDetails(organization);

		organization.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(organization);
		organization.SimilarOrgFinder.FindSimilarOrganisations();

		var similarOrgMatches = organization.SimilarOrgMatches;
		organization.Delete();
		return similarOrgMatches;
	}

	public OrgHeader CreateNewOrganizationByCurrentAddressInfo()
	{
		var org = new BusinessObjectFactory().New<OrgHeader>();
		FillOrgHeaderWithConsignmentDetails(org);

		return org;
	}

	void FillOrgHeaderWithConsignmentDetails(OrgHeader org)
	{
		if (Consignment == null)
		{
			return;
		}

		var mainAddress = org.MainAddress;
		switch (AddressType)
		{
			case ConsignmentAddressType.Shipper:
				org.OH_IsConsignor = true;
				org.OH_FullName = Consignment.ShipperName?.Substring(0, Math.Min(Consignment.ShipperName.Length, org.OH_FullNameInfo.MaxLength));
				mainAddress.OA_Address1 = Consignment.ShipperAddress1?.Substring(0, Math.Min(Consignment.ShipperAddress1.Length, mainAddress.OA_Address1Info.MaxLength));
				mainAddress.OA_Address2 = Consignment.ShipperAddress2?.Substring(0, Math.Min(Consignment.ShipperAddress2.Length, mainAddress.OA_Address2Info.MaxLength));
				mainAddress.OA_City = Consignment.ShipperCity;
				mainAddress.OA_State = Consignment.ShipperState.Substring(0, Math.Min(Consignment.ShipperState.Length, mainAddress.OA_StateInfo.MaxLength));
				mainAddress.OA_PostCode = Consignment.ShipperPostcode?.Substring(0, Math.Min(Consignment.ShipperPostcode.Length, mainAddress.OA_PostCodeInfo.MaxLength));
				mainAddress.OA_RN_NKCountryCode = Consignment.ShipperCountryCode;
				mainAddress.OA_Phone = Consignment.ShipperPhone;
				mainAddress.OA_Mobile = Consignment.ShipperMobile;
				mainAddress.OA_Fax = Consignment.ShipperFax;
				mainAddress.OA_Email = Consignment.ShipperEmail;
				break;
			case ConsignmentAddressType.Consignee:
				org.OH_IsConsignee = true;
				org.OH_FullName = Consignment.ConsigneeName?.Substring(0, Math.Min(Consignment.ConsigneeName.Length, org.OH_FullNameInfo.MaxLength));
				mainAddress.OA_Address1 = Consignment.ConsigneeAddress1?.Substring(0, Math.Min(Consignment.ConsigneeAddress1.Length, mainAddress.OA_Address1Info.MaxLength));
				mainAddress.OA_Address2 = Consignment.ConsigneeAddress2?.Substring(0, Math.Min(Consignment.ConsigneeAddress2.Length, mainAddress.OA_Address2Info.MaxLength));
				mainAddress.OA_City = Consignment.ConsigneeCity;
				mainAddress.OA_State = Consignment.ConsigneeState?.Substring(0, Math.Min(Consignment.ConsigneeState.Length, mainAddress.OA_StateInfo.MaxLength));
				mainAddress.OA_PostCode = Consignment.ConsigneePostcode?.Substring(0, Math.Min(Consignment.ConsigneePostcode.Length, mainAddress.OA_PostCodeInfo.MaxLength));
				mainAddress.OA_RN_NKCountryCode = Consignment.ConsigneeCountryCode;
				mainAddress.OA_Phone = Consignment.ConsigneePhone;
				mainAddress.OA_Mobile = Consignment.ConsigneeMobile;
				mainAddress.OA_Fax = Consignment.ConsigneeFax;
				mainAddress.OA_Email = Consignment.ConsigneeEmail;
				break;
		}
	}

	public void SelectAddress(ZGuid addressPK)
	{
		if (AddressType == ConsignmentAddressType.Shipper)
		{
			Consignment.ShipperAddressId = addressPK;
		}
		else if (AddressType == ConsignmentAddressType.Consignee)
		{
			Consignment.ConsigneeAddressId = addressPK;
		}

		Consignment.RefreshBinding();
		RefreshBinding();
	}

	public void Ignore()
	{
		Ignored = true;
		RefreshBinding();
	}

	ZString AddressKey => $"{PartyName}+{Address1}+{Address2}+{City}+{State}+{Country}+{Postcode}+{Phone}+{Mobile}+{Fax}+{Email}";

	public override bool HasChanges => Consignment != null && Consignment.HasChanges;

	#region NPBO Properties

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|WaybillNumber", Caption = "Waybill Number")]
	public ZString WaybillNumber { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|PartyType", Caption = "Party Type")]
	public ZString PartyType { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|PartyName", Caption = "Name")]
	public ZString PartyName { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Address1", Caption = "Address 1")]
	public ZString Address1 { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Address2", Caption = "Address 2")]
	public ZString Address2 { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|City", Caption = "City")]
	public ZString City { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|State", Caption = "State")]
	public ZString State { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Postcode", Caption = "Postcode")]
	public ZString Postcode { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Country", ShortCaption = "Ctry/Rgn.", Caption = "Country/Region")]
	public ZString Country { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Phone", Caption = "Phone")]
	public ZString Phone { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Mobile", Caption = "Mobile")]
	public ZString Mobile { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Fax", Caption = "Fax")]
	public ZString Fax { get; }

	[ResourceStringData("NPBO:Enterprise.MasterFiles.Business.FreeTextAddress|Email", Caption = "Email")]
	public ZString Email { get; }

	#endregion
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondBillShipmentSynchronizer : BusinessObjectSynchroniser
	{
		public CusInBondBillShipmentSynchronizer(CusInBondBill destination)
			: base(destination, destination.Header.Parent as ForwardingShipment)
		{
			header = destination.Header;
			var declaration = Declaration;
			if (declaration == null)
			{
				header.DeclarationPKInfo.ValueChanged += DeclarationPKInfo_ValueChanged;
			}
			isAMSHBREffective = ZZCustomsFunctionality.IsAMSHBREffective;
			validSCACs = Source.GetValidSCACIssuerCodes(Source.TransportMode);
		}
		readonly bool isAMSHBREffective;
		readonly IEnumerable<ZString> validSCACs;

		protected new CusInBondBill Destination
		{
			get { return (CusInBondBill)base.Destination; }
		}

		protected new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		readonly CusInBondHeader header;

		JobDeclaration Declaration
		{
			get { return header.Declaration; }
		}

		CusInBondHeaderShipmentDataCalculator ConsolDataCalculator
		{
			get
			{
				var headerSynchroniser = header.Synchroniser as CusInBondHeaderShipmentSynchronizer;
				return headerSynchroniser == null ? null : headerSynchroniser.ConsolDataCalculator;
			}
		}

		CusInBondBillDataCalculator BillDataCalculator
		{
			get { return billDataCalculator ?? (billDataCalculator = new CusInBondBillDataCalculator(Source, Declaration)); }
		}
		CusInBondBillDataCalculator billDataCalculator;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			masterBillIssuerCodeFieldSynchroniser = new FieldSynchroniser(Destination.B0_IssuerCodeInfo, () => ConsolDataCalculator.MasterBillIssuerCode, ConsolDataCalculator.GetInfosAffectingMasterBillIssuerCode);
			Synchronisers.Add(masterBillIssuerCodeFieldSynchroniser);
			masterBillFieldSynchroniser = new FieldSynchroniser(Destination.B0_MasterBillNumberInfo, () => ConsolDataCalculator.MasterBill, ConsolDataCalculator.GetInfosAffectingMasterBill);
			Synchronisers.Add(masterBillFieldSynchroniser);
			houseBillFieldSynchroniser = new FieldSynchroniser(Destination.B0_HouseBillNumberInfo, GetHouseBill, GetHouseBillRelatedInfos);
			Synchronisers.Add(houseBillFieldSynchroniser);
			houseBillIssuerCodeFieldSynchroniser = new FieldSynchroniser(Destination.B0_HouseBillIssuerCodeInfo, () => BillDataCalculator.GetHouseBillIssuerCode(), GetHouseBillRelatedInfos);
			Synchronisers.Add(houseBillIssuerCodeFieldSynchroniser);
			manifestQtyFieldSynchroniser = new FieldSynchroniser(Destination.B0_ManifestQtyInfo, GetManifestQty, GetManifestQtyRelatedInfos);
			Synchronisers.Add(manifestQtyFieldSynchroniser);
			manifestUQFieldSynchroniser = new FieldSynchroniser(Destination.B0_ManifestUQInfo, GetManifestUQ, GetManifestUQRelatedInfos);
			Synchronisers.Add(manifestUQFieldSynchroniser);
			weightFieldSynchroniser = new FieldSynchroniser(Destination.B0_WeightInfo, GetWeight, GetWeightRelatedInfos);
			Synchronisers.Add(weightFieldSynchroniser);
			weightUQFieldSynchroniser = new FieldSynchroniser(Destination.B0_WeightUQInfo, GetWeightUQ, GetWeightUQRelatedInfos);
			Synchronisers.Add(weightUQFieldSynchroniser);
			volumeFieldSynchroniser = new FieldSynchroniser(Destination.B0_VolumeInfo, GetVolume, GetVolumeRelatedInfos);
			Synchronisers.Add(volumeFieldSynchroniser);
			volumeUQFieldSynchroniser = new FieldSynchroniser(Destination.B0_VolumeUQInfo, GetVolumeUQ, GetVolumeUQRelatedInfos);
			Synchronisers.Add(volumeUQFieldSynchroniser);

			var consignorAddress = Source.ConsignorDocumentaryAddress;
			if (consignorAddress != null)
			{
				HookSynchronisersForAddressDetails(Destination.ForeignShipper, consignorAddress);
			}

			var consigneeAddress = Source.ConsigneeDocumentaryAddress;
			if (consigneeAddress != null)
			{
				HookSynchronisersForAddressDetails(Destination.Consignee, consigneeAddress);
			}

			var notifyParty = Source.NotifyPartyDocumentaryAddress;
			if (notifyParty != null)
			{
				HookSynchronisersForAddressDetails(Destination.NotifyParty, notifyParty);
			}

			var movementHeader = header.MovementHeader;

			CusInBondMoveDetail moveDetail = null;
			foreach (CusInBondMoveDetail existingMoveDetail in movementHeader.MovementDetails.ToArray())
			{
				CusInBondMoveDetail moveDetailThatShouldBeDeleted = null;
				if (existingMoveDetail.B9_B0 == Destination.PK)
				{
					if (moveDetail == null)
					{
						moveDetail = existingMoveDetail;
					}
					else
					{
						if (moveDetail.B9_SeqNo > existingMoveDetail.B9_SeqNo)
						{
							moveDetailThatShouldBeDeleted = moveDetail;
							moveDetail = existingMoveDetail;
						}
						else
						{
							moveDetailThatShouldBeDeleted = existingMoveDetail;
						}
					}
					if (moveDetailThatShouldBeDeleted != null && !moveDetailThatShouldBeDeleted.ActiveInMessaging)
					{
						moveDetailThatShouldBeDeleted.Delete();
					}
				}
			}
			Synchronisers.Add(new CusInBondMoveDetailShipmentSynchronizer(moveDetail ?? movementHeader.MovementDetails.AddNew(), Destination));
		}
		FieldSynchroniser masterBillIssuerCodeFieldSynchroniser;
		FieldSynchroniser masterBillFieldSynchroniser;
		FieldSynchroniser houseBillFieldSynchroniser;
		FieldSynchroniser houseBillIssuerCodeFieldSynchroniser;
		FieldSynchroniser manifestQtyFieldSynchroniser;
		FieldSynchroniser manifestUQFieldSynchroniser;
		FieldSynchroniser weightFieldSynchroniser;
		FieldSynchroniser weightUQFieldSynchroniser;
		FieldSynchroniser volumeFieldSynchroniser;
		FieldSynchroniser volumeUQFieldSynchroniser;

		void HookSynchronisersForAddressDetails(JobDocAddress destinationAddress, JobDocAddress sourceAddress)
		{
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_AddressOverrideInfo, sourceAddress.E2_AddressOverrideInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_CompanyNameInfo, sourceAddress.E2_CompanyNameInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_Address1Info, sourceAddress.E2_Address1Info));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_Address2Info, sourceAddress.E2_Address2Info));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_PostcodeInfo, sourceAddress.E2_PostcodeInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_CityInfo, sourceAddress.E2_CityInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_StateInfo, sourceAddress.E2_StateInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_RN_NKCountryCodeInfo, sourceAddress.E2_RN_NKCountryCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_PhoneInfo, sourceAddress.E2_PhoneInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_FaxInfo, sourceAddress.E2_FaxInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_EmailInfo, sourceAddress.E2_EmailInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_OA_AddressInfo, sourceAddress.E2_OA_AddressInfo));
			Synchronisers.Add(new FieldSynchroniser(destinationAddress.E2_ContactInfo, sourceAddress.E2_ContactInfo));
		}

		void DeclarationPKInfo_ValueChanged(object sender, System.EventArgs e)
		{
			header.DeclarationPKInfo.ValueChanged -= DeclarationPKInfo_ValueChanged;
			billDataCalculator = null;
			UpdateInfoEventsAndReSynchronise(masterBillIssuerCodeFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(masterBillFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(houseBillFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(houseBillIssuerCodeFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(manifestQtyFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(manifestUQFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(weightFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(weightUQFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(volumeFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(volumeUQFieldSynchroniser);
		}

		protected override void DisposeCore()
		{
			header.DeclarationPKInfo.ValueChanged -= DeclarationPKInfo_ValueChanged;
			base.DisposeCore();
		}

		#region Synchronise Properties

		#region B0_HouseBillNumber

		IZType GetHouseBill()
		{
			var result = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null && (declaration.IsAir || (declaration.IsSea && isAMSHBREffective)))
			{
				result = declaration.JE_HouseBill;
			}
			else if (Source.IsAir)
			{
				result = Source.JS_HouseBill;
			}
			else if (Source.IsSea && isAMSHBREffective)
			{
				var billNumber = Source.JS_HouseBill.KeepValidBillNumberCharacters();
				result = billNumber.ShouldTrimSCACFromBills(validSCACs) ? billNumber.GetBillNumberTrimSCAC() : billNumber;
			}
			return result.KeepAlphanumericCharacters();
		}

		IEnumerable<ZPropertyInfo> GetHouseBillRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TransportModeInfo;
				yield return declaration.JE_HouseBillInfo;
				yield return declaration.JE_HouseBillIssuerSCACInfo;
			}
			else
			{
				yield return Source.JS_TransportModeInfo;
				yield return Source.JS_HouseBillInfo;
			}
		}

		#endregion

		#region B0_ManifestQty

		IZType GetManifestQty()
		{
			IZType result = ZInt.Zero;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.JE_TotalNoOfPacks;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetManifestQtyRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TotalNoOfPacksInfo;
			}
			else
			{
				yield return Destination.B0_ManifestQtyInfo;
			}
		}

		#endregion

		#region B0_ManifestUQ

		IZType GetManifestUQ()
		{
			ZString result = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.JE_TotalNoOfPacksPackType;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetManifestUQRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TotalNoOfPacksPackTypeInfo;
			}
			else
			{
				yield return Destination.B0_ManifestUQInfo;
			}
		}

		#endregion

		#region B0_Weight

		IZType GetWeight()
		{
			var result = ZDecimal.Zero;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.JE_TotalWeight;
			}
			else
			{
				result = Source.JS_ActualWeight;
			}

			return result;
		}

		IEnumerable<ZPropertyInfo> GetWeightRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TotalWeightInfo;
			}
			else
			{
				yield return Source.JS_ActualWeightInfo;
			}
		}

		#endregion

		#region B0_WeightUQ

		IZType GetWeightUQ()
		{
			var result = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.JE_TotalWeightUnit;
			}
			else
			{
				result = Source.JS_UnitOfWeight;
			}

			return result;
		}

		IEnumerable<ZPropertyInfo> GetWeightUQRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TotalWeightUnitInfo;
			}
			else
			{
				yield return Source.JS_UnitOfWeightInfo;
			}
		}

		#endregion

		#region B0_Volume

		IZType GetVolume()
		{
			var result = ZDecimal.Zero;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.JE_TotalVolume;
			}
			else
			{
				result = Source.JS_ActualVolume;
			}

			return result;
		}

		IEnumerable<ZPropertyInfo> GetVolumeRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TotalVolumeInfo;
			}
			else
			{
				yield return Source.JS_ActualVolumeInfo;
			}
		}

		#endregion

		#region B0_VolumeUQ

		IZType GetVolumeUQ()
		{
			var result = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.JE_TotalVolumeUnit;
			}
			else
			{
				result = Source.JS_UnitOfVolume;
			}

			return result;
		}

		IEnumerable<ZPropertyInfo> GetVolumeUQRelatedInfos()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_TotalVolumeUnitInfo;
			}
			else
			{
				yield return Source.JS_UnitOfVolumeInfo;
			}
		}

		#endregion

		#endregion
	}
}

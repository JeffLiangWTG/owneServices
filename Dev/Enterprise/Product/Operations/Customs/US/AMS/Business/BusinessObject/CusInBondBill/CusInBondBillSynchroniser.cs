using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondBillSynchroniser : ManifestBillSynchroniser<CusInBondBill>
	{
		public CusInBondBillSynchroniser(CusInBondBill destination, ForwardingShipment source)
			: base(destination, source)
		{
			var header = destination.Header;
			this.consolSource = header == null ? null : header.Consol;
		}

		#region Implementation

		protected override IZType GetBillNumber()
		{
			var result = ZString.Empty;
			if (Destination.IsBillAlreadyOnFile)
			{
				result = ((IManifestBillForSynchroniser)Destination).MasterBillNumberInfo.Value.ToString();
			}
			else
			{
				var billNumber = Source.JS_HouseBill.KeepValidBillNumberCharacters();
				var validSCACs = Source.GetValidSCACIssuerCodes(Source.TransportMode);
				result = billNumber.ShouldTrimSCACFromBills(validSCACs) ? billNumber.GetBillNumberTrimSCAC() : billNumber;
			}
			return result;
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.B0_IssuerCodeInfo, GetIssuerCode, GetInfosAffectingIssuerCode));
			billOfLadingStatusCodeSynchroniser = new FieldSynchroniser(Destination.B0_BillStatusInfo, () => USConsolDataCalculator.BillOfLadingStatusCode, USConsolDataCalculator.GetInfosAffectingBillOfLadingStatusCode);
			Synchronisers.Add(billOfLadingStatusCodeSynchroniser);

			contractualPortSynchroniser = new FieldSynchroniser(Destination.B0_RL_NKForeignPortOfContractInfo, GetContractualPort, USConsolDataCalculator.GetInfosAffectingFirstCarrierContractualTransport);
			Synchronisers.Add(contractualPortSynchroniser);
			Synchronisers.Add(new FieldSynchroniser(Destination.B0_IssuerSCACInfo, () => USConsolDataCalculator.OrgProxySCAC, GetInfosAffectingOrgProxySCAC));

			Synchronisers.Add(new CusInBondContainerCollectionSynchroniser(Source, Destination, consolSource, Destination.ShouldSynchronise, Destination.MovementDetail.Containers));
			var header = Destination.Header;
			if (!header.IsNVOCCHeader)
			{
				var oceanBill = Destination.ShipmentReferenceDetails[BillReferenceList.Codes.OB] ?? Destination.ShipmentReferenceDetails.AddNew();
				Synchronisers.Add(new CusInbondBillAddRefSynchroniser(oceanBill, consolSource));
			}
			var snp1 = Destination.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Order == 1);
			if (snp1 == null)
			{
				snp1 = Destination.SecondaryNotifyParties.AddNew();
				snp1.CY_Order = 1;
			}
			secondaryNotifyParty1Synchroniser = new FieldSynchroniser(
				snp1.CY_DataInfo,
				() => USConsolDataCalculator.CarrierSCAC,
				() => new[]
				{
					consolSource.MasterBillIssuingPartyDocumentaryAddress.OrganisationPKInfo,
					consolSource.JK_OA_ShippingLineAddressInfo
				});
			Synchronisers.Add(secondaryNotifyParty1Synchroniser);
			var snp2 = Destination.SecondaryNotifyParties.FirstOrDefault(x => x.CY_Order == 2);
			if (snp2 == null)
			{
				snp2 = Destination.SecondaryNotifyParties.AddNew();
				snp2.CY_Order = 2;
			}
			secondaryNotifyParty2Synchroniser = new FieldSynchroniser(snp2.CY_DataInfo, () => USConsolDataCalculator.OrgProxySCAC, GetInfosAffectingOrgProxySCAC);
			Synchronisers.Add(secondaryNotifyParty2Synchroniser);
		}
		FieldSynchroniser billOfLadingStatusCodeSynchroniser;
		FieldSynchroniser secondaryNotifyParty1Synchroniser;
		FieldSynchroniser secondaryNotifyParty2Synchroniser;
		FieldSynchroniser contractualPortSynchroniser;

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			billOfLadingStatusCodeSynchroniser = null;
			secondaryNotifyParty1Synchroniser = null;
			secondaryNotifyParty2Synchroniser = null;
			contractualPortSynchroniser = null;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingOrgProxySCAC()
		{
			yield return Destination.B0_BHInfo;

			foreach (var info in USConsolDataCalculator.GetInfosAffectingOrgProxySCAC())
			{
				yield return info;
			}
		}

		protected override IZType GetPortOfLading()
		{
			var transport = USConsolDataCalculator.LoadTransportForUSBoundVessel;
			return transport != null ? transport.JW_RL_NKLoadPort : ZString.Empty;
		}

		protected override IEnumerable<ZPropertyInfo> GetInfosAffectingPortOfLading()
		{
			return USConsolDataCalculator.GetInfosAffectingLoadTransportForUSBoundVessel();
		}

		protected override IZType GetPlaceOfReceipt()
		{
			var result = ZString.Empty;
			var transport = USConsolDataCalculator.FirstCarrierContractualTransport;
			if (transport != null)
			{
				var port = transport.LoadPort;
				result = port != null ? port.RL_PortName : transport.JW_RL_NKLoadPort;
			}
			return result.Left(CusInBondBill.Schema.B0_PlaceOfReceiptMaxLength);
		}

		protected override IEnumerable<ZPropertyInfo> GetInfosAffectingPlaceOfReceipt()
		{
			return USConsolDataCalculator.GetInfosAffectingFirstCarrierContractualTransport();
		}

		IZType GetContractualPort()
		{
			var result = ZString.Empty;
			var transport = USConsolDataCalculator.FirstCarrierContractualTransport;
			if (transport != null)
			{
				var port = transport.LoadPort;
				result = port != null ? port.RL_Code : ZString.Empty;
			}
			return result.Left(CusInBondBill.Schema.B0_RL_NKForeignPortOfContractMaxLength);
		}

		protected override IZType GetLastForeignPort()
		{
			var port = USConsolDataCalculator.LastForeignPortOfLoading;
			return port != null ? port.RL_Code : ZString.Empty;
		}

		protected override IEnumerable<ZPropertyInfo> GetLastForeignPortInfo()
		{
			return USConsolDataCalculator.GetInfosAffectingLastForeignPortOfLoading();
		}

		protected override void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			Synchronise(billOfLadingStatusCodeSynchroniser);
			base.Transports_CountChanged(sender, e);
		}

		void Synchronise(FieldSynchroniser synchroniser)
		{
			if (synchroniser != null)
			{
				synchroniser.UpdateInfoEventsAndReSynchronise();
			}
		}

		FieldSynchroniser qtySynchroniser;
		FieldSynchroniser unitSynchroniser;
		protected override void AddPacksSynchroniser()
		{
			qtySynchroniser = new FieldSynchroniser(Destination.B0_ManifestQtyInfo, GetPackQty, GetSourceValuesAffectingPackQtyAndUQ, false);
			Synchronisers.Add(qtySynchroniser);

			unitSynchroniser = new FieldSynchroniser(Destination.B0_ManifestUQInfo, GetPackUQ, GetSourceValuesAffectingPackQtyAndUQ, false);
			Synchronisers.Add(unitSynchroniser);

			Source.OuterPackLines.CountChanged -= PackLines_CountChanged;
			Source.OuterPackLines.CountChanged += PackLines_CountChanged;
			Source.InnerPackLines.CountChanged -= PackLines_CountChanged;
			Source.InnerPackLines.CountChanged += PackLines_CountChanged;
		}

		IZType GetPackUQ()
		{
			var result = ZString.Empty;
			var firstPackType = Source.MergeOuterPackLinesIntoInnerPackLines().OrderBy(x => x.JL_ContainerPackingOrder).FirstOrDefault()?.JL_F3_NKPackType;
			if (string.IsNullOrEmpty(firstPackType))
			{
				firstPackType = ManifestUnitList.Codes.Package;
			}
			var totalPacksUnit = Source.MergeOuterPackLinesIntoInnerPackLines().All(packLine => packLine.JL_F3_NKPackType.Equals(firstPackType.Value))
				? firstPackType.Value : new ZString(ManifestUnitList.Codes.Package);
			var refPacks = CusRefPacksHelper.LoadFilteredRefPacks(Destination.Factory, Core.Constants.CountryCodes.UnitedStates, RPTypeList.Codes.AMSManifest, totalPacksUnit);
			if (refPacks.Count > 0)
			{
				var manifestUnitList = Destination.Lookups.ManifestUnitList;
				result = refPacks
					.Select(y => y.RP_CustomsPack.Left(3))
					.FirstOrDefault(x => manifestUnitList.ContainsCode(x));
			}
			if (string.IsNullOrEmpty(result))
			{
				result = new PackageTypeMapping().GetPackageType(totalPacksUnit);
			}
			return result;
		}

		IZType GetPackQty()
		{
			return (ZInt)Source.MergeOuterPackLinesIntoInnerPackLines().Sum(x => x.JL_PackageCount);
		}

		IEnumerable<ZPropertyInfo> GetSourceValuesAffectingPackQtyAndUQ()
		{
			var packLineCollection = Source.OuterPackLines.Cast<PackLine>().Union(Source.InnerPackLines.Cast<PackLine>());
			foreach (var line in packLineCollection)
			{
				yield return line.JL_JL_OuterPackLineInfo;
				yield return line.JL_PackageCountInfo;
				yield return line.JL_F3_NKPackTypeInfo;
			}
		}

		void PackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			qtySynchroniser?.UpdateInfoEventsAndReSynchronise();
			unitSynchroniser?.UpdateInfoEventsAndReSynchronise();
		}

		IZType GetIssuerCode()
		{
			var result = ZString.Empty;
			if (Destination.IsBillAlreadyOnFile)
			{
				result = Destination.B0_IssuerCode;
			}
			else
			{
				var billNumber = Source.JS_HouseBill.KeepValidBillNumberCharacters();
				var validSCACs = Source.GetValidSCACIssuerCodes(Source.TransportMode);
				result = billNumber.GetSCAC(validSCACs);
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingIssuerCode()
		{
			if (Source.HouseBillIssuingParty != null)
			{
				return new List<ZPropertyInfo>(new[] { Source.HouseBillIssuingPartyDocumentaryAddress.OrganisationPKInfo }).ToArray();
			}
			else
			{
				var result = new List<ZPropertyInfo>(new[] { Source.JS_HouseBillInfo });
				result.AddRange(GetInfosAffectingOrgProxySCAC());
				return result;
			}
		}

		protected override IEnumerable<ZPropertyInfo> GetInfosAffectingBillNumber()
		{
			var list = new List<ZPropertyInfo>(new[] { consolSource.JK_AgentTypeInfo, Source.JS_HouseBillInfo });
			var moveDetail = Destination.MovementDetail;
			if (moveDetail != null)
			{
				list.Add(moveDetail.B9_CustomsStatusInfo);
			}
			return list.ToArray();
		}

		protected override Customs.Business.ConsolDataCalculator ConsolDataCalculator
		{
			get { return consolDataCalculator ?? (consolDataCalculator = new ConsolDataCalculator(consolSource, Destination.Header)); }
		}
		ConsolDataCalculator consolDataCalculator;

		ConsolDataCalculator USConsolDataCalculator
		{
			get { return (ConsolDataCalculator)(ConsolDataCalculator); }
		}

		protected override void DisposeCore()
		{
			base.DisposeCore();
			if (consolDataCalculator != null)
			{
				consolDataCalculator.Dispose();
				consolDataCalculator = null;
			}
		}

		#endregion
	}
}

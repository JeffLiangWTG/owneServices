using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaManifestHeaderDocWrapper : ASYCUDA.Business.AsycudaManifestHeaderDocWrapper
	{
		public AsycudaManifestHeaderDocWrapper(AsycudaManifestHeader manifestHeader)
			: base(manifestHeader)
		{
		}
		public new AsycudaManifestHeader Manifest => (AsycudaManifestHeader)base.Manifest;
		public AsycudaBillCollection Bills => Manifest.Bills;

		public PreviousDeclarationsDocWrapperCollection PreviousDeclarations
		{
			get
			{
				if (previousDeclarations == null)
				{
					previousDeclarations = new PreviousDeclarationsDocWrapperCollection(Factory);
					foreach (AsycudaBill bill in Manifest.Bills)
					{
						if (!bill.CustomsEntryNumbers.Any())
						{
							previousDeclarations.Add(new PreviousDeclarationsDocWrapper(this, bill, ZString.Empty));
						}
						else
						{
							var preDecNumbers = bill.CustomsEntryNumbers;
							foreach (ASYCUDA.Business.ABLEntryNum preDecNumber in preDecNumbers)
							{
								previousDeclarations.Add(new PreviousDeclarationsDocWrapper(this, bill, preDecNumber.CE_EntryNum));
							}
						}
					}
				}
				return previousDeclarations;
			}
		}
		PreviousDeclarationsDocWrapperCollection previousDeclarations;

		public BusinessObjectCollectionWrapper<AsycudaBillDocWrapper> BillDocWrappers
		{
			get
			{
				var billDocWrappers = new List<AsycudaBillDocWrapper>();

				foreach (AsycudaBill bill in Manifest.Bills)
				{
					billDocWrappers.Add(new AsycudaBillDocWrapper(bill));
				}

				return new BusinessObjectCollectionWrapper<AsycudaBillDocWrapper>(billDocWrappers);
			}
		}

		public BusinessObjectCollectionWrapper<TRCarrierManifestItemWrapper> PackedItemDocWrappers
		{
			get
			{
				var packedItemDocWrappers = new List<TRCarrierManifestItemWrapper>();
				foreach (AsycudaBill bill in Manifest.Bills)
				{
					if (!bill.Packs.Any())
					{
						packedItemDocWrappers.Add(new TRCarrierManifestItemWrapper(bill));
					}
					else
					{
						foreach (AsycudaPack pack in bill.Packs)
						{
							var packedItems = pack.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => (AsycudaPackedItem)x.PackedItem);

							if (packedItems == null || !packedItems.Any())
							{
								packedItemDocWrappers.Add(new TRCarrierManifestItemWrapper(pack));
							}
							else
							{
								packedItems.ForEach(packedItem => packedItemDocWrappers.Add(new TRCarrierManifestItemWrapper(packedItem)));
							}
						}
					}
				}
				return new BusinessObjectCollectionWrapper<TRCarrierManifestItemWrapper>(packedItemDocWrappers);
			}
		}

		public ZString RegNoOfAgentCarrier => Manifest.Carrier?.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;

		public ZString RegNoOfDeclarant => GlbCompany.CurrentCompany.GC_BusinessRegNo;

		public ZDecimal TotalABLManifestQty => Manifest.Bills.Cast<AsycudaBill>().Select(x => (int)x.ABL_ManifestQty).Sum();

		public ZString TotalGrossWeight => string.Format(DefaultCulture.Instance.NumberFormat, "{0:#,0.00}", Manifest.Bills.Cast<AsycudaBill>().SelectMany(bill => bill.Packs.Cast<AsycudaPack>().SelectMany(pack => pack.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => x.PackedItem))).Sum(x => Core.Constants.Weight.ConvertSafe(x.API_GrossWeight, x.API_GrossWeightUQ, Core.Constants.Weight.Kilograms)));

		public ZString PresentationCustomsOfficeDescription => Manifest.Lookups.TR_GM_PresentationCustomsOfficeList.GetDescriptionFromCode(Manifest.TR_GM_PresentationCustomsOffice);

		public ZString LoadPortNameForCarrierManifest => CustomsLoadPortName.IsEmpty ? LoadPortName : CustomsLoadPortName;

		public ZString DischargePortNameForCarrierManifest => CustomsDischargePortName.IsEmpty ? DischargePortName : CustomsDischargePortName;

		public ZString ConveyanceTRMappedNationality => ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.Turkey, TurkishConstants.CountryMapType, ConveyanceNationality, ZDateTime.Now);

		public ZString ConveyanceTRMappedNationalityName => Manifest.ConveyanceNationality?.Description ?? ZString.Empty;

		public ZInt TotalContainers => Manifest.Containers.Count;

		protected override ZString GetCustomsDischargePortDescription(ZString customsPortCode)
		{
			if (Manifest.Lookups.CustomsDischargePortList is RefUNLOCOCollection)
			{
				return GetPortName(customsPortCode);
			}

			return base.GetCustomsDischargePortDescription(customsPortCode);
		}

		protected override ZString GetCustomsLoadingPortDescription(ZString customsPortCode)
		{
			if (Manifest.Lookups.CustomsLoadingPortList is RefUNLOCOCollection)
			{
				return GetPortName(customsPortCode);
			}

			return base.GetCustomsLoadingPortDescription(customsPortCode);
		}

		ZString GetPortName(ZString customsPortCode)
		{
			RefUNLOCO portList = Factory.LoadFromNaturalKey<RefUNLOCO>(Enterprise.ZArchitecture.Schema.RefUNLOCOSchema.RL_Code, customsPortCode);
			return portList?.RL_PortName ?? ZString.Empty;
		}

		public ZString DeclarationOwnerFullName => GetDeclarationOwnerFullName();

		ZString GetDeclarationOwnerFullName()
		{
			var result = ZString.Empty;
			var manifest = Manifest;

			if (!manifest.RegistrationNumber.IsEmpty)
			{
				var message = manifest.Messages.Cast<TRManifestMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && x.EM_MessageType == TRMessageTypes.Codes.TRO && !x.EM_MessageOwner.IsEmpty);

				if (message != null)
				{
					var glbStaff = manifest.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, message.EM_MessageOwner)).FirstOrDefault();

					result = glbStaff?.GS_FullName ?? ZString.Empty;
				}
			}

			return result;
		}

		public ZString IssuingCarrierAgentName => Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName;

		public ZString IssuingCarrierAgentIATACode => Manifest.IsAir ? Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode : string.Empty;

		public ZString ShortRegistrationNumber
		{
			get
			{
				var result = ZString.Empty;
				var regNumber = Manifest.RegistrationNumber;
				if (!regNumber.IsEmpty)
				{
					result = regNumber.SubstringSafe(regNumber.IndexOf("IM") + 2);
				}
				return result;
			}
		}
	}
}

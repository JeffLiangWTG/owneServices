using System;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
#if !WINZOR
using Enterprise.ZArchitecture.GUI.BrowserInterop;
#endif
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI
{
#if !WINZOR
	internal abstract class ContractNumberGlowPopupProxy
	{
		protected ContractNumberGlowPopupProxy(IContractNumberFindBoxPopupSupport contractInfo)
		{
			this.contractInfo = contractInfo;
		}

		/// <summary>
		/// The URL portion to be used when calling for the popup.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		internal abstract bool TryGenerateURL(out Uri url);

		/// <summary>
		/// Used to filter the command that the popup is listening to.
		/// </summary>
		internal abstract Regex FormFlowRegex { get; }

		/// <summary>
		/// A value from Enerprise.Core.Constants.RatingContractTypes.
		/// </summary>
		internal abstract string RatingContractType { get; }

		ResourceString Description
			=> RatingContractType.Equals(RatingContractTypes.Provider) ?
				ResString.GetMultilingualString("4b1a72db-88cf-4f77-8bef-a0548797664f", "Carrier") :
				ResString.GetMultilingualString("01bdd2aa-5fd6-48b1-af1d-4a470900da15", "Client");

		internal ResourceString WindowTitle
			=> ResString.GetMultilingualString("651814a3-a412-4a16-a093-d2ca800f6276", "{0} Contracts Lookup", Description);

		public string ContractNumber
		{
			get
			{
				return contractInfo.ContractNumber;
			}
			set
			{
				contractInfo.ContractNumber = value;
			}
		}
		protected IContractNumberFindBoxPopupSupport contractInfo;

		/// <summary>
		/// Gets the data to be used when preparing the initial set of filter
		/// that are shown when the popup appears.
		/// </summary>
		/// <returns></returns>
		internal abstract IUpdateFilterData GetUpdateFilterData();
	}

	internal class ClientContractNumberGlowPopupProxy : ContractNumberGlowPopupProxy
	{
		internal ClientContractNumberGlowPopupProxy(IContractNumberFindBoxPopupSupport contractInfo) : base(contractInfo)
		{
		}

		internal override Regex FormFlowRegex
			=> URLHelpers.GetFormFlowRegex("CLX", "/formFlow/b91b1fe8-2bc6-4218-b5c0-78eade5a6975"); // Command name

		internal override string RatingContractType => RatingContractTypes.Client;

		internal override bool TryGenerateURL(out Uri url)
			=> URLHelpers.TryGenerateURL("goto/ClientContractsImportG1", out url);

		internal override IUpdateFilterData GetUpdateFilterData()
		{
			var entry = contractInfo.RateEntry;
			return new ClientUpdateFilterData()
			{
				startDate = entry.TI_RateStartDate,
				expiryDate = entry.TI_RateEndDate,
				clientPK = entry.ServiceProviderPK(),
				contractID = contractInfo.ContractNumber,
			};
		}
	}

	internal class CarrierContractNumberGlowPopupProxy : ContractNumberGlowPopupProxy
	{
		internal CarrierContractNumberGlowPopupProxy(IContractNumberFindBoxPopupSupport contractInfo) : base(contractInfo)
		{
		}

		internal override Regex FormFlowRegex
			=> URLHelpers.GetFormFlowRegex("CCA", (NoResString)"/formFlow/fe8d37f7-513c-4dcd-a4b0-61f941543291"); // Command name

		internal override string RatingContractType => RatingContractTypes.Provider;

		internal override bool TryGenerateURL(out Uri url)
			=> URLHelpers.TryGenerateURL("goto/CarrierContractsImport", out url);

		internal override IUpdateFilterData GetUpdateFilterData()
		{
			var data = new CarrierUpdateFilterData();
			var entry = contractInfo.RateEntry;

			data.startDate = entry.TI_RateStartDate;
			data.expiryDate = entry.TI_RateEndDate;
			data.serviceProviderPK = entry.ServiceProviderPK();

			if (entry.IsSea())
			{
				data.transportMode = TransportModes.Sea;
			}
			else if (entry.IsAir())
			{
				data.transportMode = TransportModes.Air;
			}
			else
			{
				data.transportMode = entry.TI_Mode;
			}
			var commodity = entry.Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, entry.TI_RH_NKCommodityCode);
			data.isCommodityHazardousRequired = commodity?.RH_IsHazardous ?? false;

			data.containerType = entry.Container?.RC_ContainerType ?? "";
			
			data.contractID = contractInfo.ContractNumber;
			return data;
		}
	}
#endif
}

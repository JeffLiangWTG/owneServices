using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class NonSecurityJobConsolAWBSpecialHandlingLookups : JobConsolAWBSpecialHandlingLookups
	{
		public NonSecurityJobConsolAWBSpecialHandlingLookups(AutoJobConsolAWBSpecialHandling parent) : base(parent)
		{
		}

		CodeDescriptionPairList LoadAirlineSpecialHandlingCodes(CodeDescriptionPairList initialSpecialHandlingList)
		{
			if (Parent is NonSecurityJobConsolAWBSpecialHandling parent && parent?.Consol != null)
			{
				var consol = parent.Consol;
				var airline = RefAirline.LoadFromAirlinePrefix(Factory, consol.MasterBillAirlinePrefix);
				if (airline == null || consol.LoadPort == null || consol.DischargePort == null)
				{
					return initialSpecialHandlingList;
				}

				var definedSpecialHandling = airline.RefAirlineSpecialHandlingCodeCollection
					.Where(x => (x.RHC_OriginPortOrCountry.EqualsIgnoringCase(consol.LoadPort?.Code)
					|| x.RHC_OriginPortOrCountry == ZString.Empty
					|| x.RHC_OriginPortOrCountry.EqualsIgnoringCase(consol.LoadPort?.Country?.Code))
				&& (x.RHC_DestinationPortOrCountry.EqualsIgnoringCase(consol.DischargePort?.Code)
					|| x.RHC_DestinationPortOrCountry == ZString.Empty
					|| x.RHC_DestinationPortOrCountry.EqualsIgnoringCase(consol.DischargePort?.Country?.Code))
				&& x.RHC_RM_Airline == airline?.PK);

				foreach (var code in definedSpecialHandling)
				{
					initialSpecialHandlingList.AddPairIfNotExist(code.RHC_Code, code.RHC_Description);
				}
			}
			initialSpecialHandlingList.SortByDescription();
			return initialSpecialHandlingList;
		}

		public CodeDescriptionPairList SpecialHandlingCodeDescriptionList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (ICodeDescription codePair in new AWBSpecialHandlingCodeDescriptionPairList())
				{
					if (!AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(codePair.Code))
					{
						result.Add(codePair);
					}
				}

				return LoadAirlineSpecialHandlingCodes(result);
			}
		}

		public CodeDescriptionPairList SpecialHandlingCodeDescriptionListFromAirline
		{
			get
			{
				return LoadAirlineSpecialHandlingCodes(new CodeDescriptionPairList());
			}
		}
	}
}

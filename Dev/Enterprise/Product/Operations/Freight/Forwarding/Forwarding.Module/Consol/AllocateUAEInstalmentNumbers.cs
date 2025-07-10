using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class AllocateUAEInstalmentNumbers
	{
		#region SuppressResourceStringsCheckRegion

		public AllocateUAEInstalmentNumbers()
		{
		}

		public ZString Allocate(BusinessObject[] bizos)
		{
			if (bizos.Length > 0)
			{
				var factory = new BusinessObjectFactory();

				var listOfPKs = new List<ZGuid>();

				foreach (BusinessObject bizo in bizos)
				{
					listOfPKs.Add(bizo.PK);
				}

				var consols = factory.Load<ForwardingConsol>(new ZQuery(JobConsolSchema.PK, listOfPKs)).OrderBy(c => c.JK_UniqueConsignRef);

				var consolGroups = new Dictionary<string, List<ForwardingConsol>>();

				foreach (ForwardingConsol consol in consols)
				{
					Transport legDomestic = null;
					Transport legImport = consol.Transports.Cast<Transport>().FirstOrDefault(x =>
											(x.JW_TransportMode == Core.Constants.TransportModes.Sea || x.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport) &&
											x.JW_RL_NKLoadPort.SubstringSafe(0, 2) != Core.Constants.CountryCodes.UnitedArabEmirates &&
											x.JW_RL_NKDiscPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.UnitedArabEmirates);

					if (legImport == null)
					{
						legDomestic = consol.Transports.Cast<Transport>().FirstOrDefault(x =>
										(x.JW_TransportMode == Core.Constants.TransportModes.Sea || x.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport) &&
										x.JW_RL_NKLoadPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.UnitedArabEmirates &&
										x.JW_RL_NKDiscPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.UnitedArabEmirates);
					}

					if (legImport != null || legDomestic != null)
					{
						var leg = legImport ?? legDomestic;

						var key = leg.JW_RL_NKDiscPort + ":" + leg.JW_Vessel + ":" + leg.JW_VoyageFlight;

						List<ForwardingConsol> likeConsols;
						if (!consolGroups.TryGetValue(key, out likeConsols))
						{
							likeConsols = new List<ForwardingConsol>();
							consolGroups.Add(key, likeConsols);
						}

						likeConsols.Add(consol);
					}
				}

				foreach (List<ForwardingConsol> likeConsol in consolGroups.Values)
				{
					int instalNumber = 1;
					foreach (ForwardingConsol consol in likeConsol)
					{
						AllocateNumber(consol, instalNumber++);
					}
				}

				try
				{
					factory.Save();
					return "Installment numbers allocated.";
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			else
			{
				return "Please select at least ONE consolidation for installment number allocation.";
			}

			return ZString.Empty;
		}

		void AllocateNumber(ForwardingConsol consol, int number)
		{
			CusEntryNumber instalment = consol.Numbers.Find(num => num.CE_EntryType == UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber).FirstOrDefault();
			if (instalment == null)
			{
				instalment = consol.Numbers.AddNew();
				instalment.CE_EntryType = UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber;
			}

			instalment.CE_EntryNum = number.ToString(CultureInfo.InvariantCulture);
		}

		#endregion
	}
}

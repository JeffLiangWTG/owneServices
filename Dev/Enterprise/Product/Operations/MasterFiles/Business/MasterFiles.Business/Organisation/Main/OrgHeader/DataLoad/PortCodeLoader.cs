using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This function was implemented to assist with the generation of OrgHeader codes for imported data
	/// when a valid UNLOCO code has not been provided.
	/// 
	/// It will first try and generate the UNLOCO Port Code needed for the OrgHeader code generation by looking for the port based
	/// on the ISO Country Code or Country Name and City provided.
	/// If no City is provided it will generate the port code using the 2 character country code and the default port code of ZZZ.
	/// If neither Country or City is provided, it will find the default port for any Australian State value (abbreviation or text) provided.
	/// Finally, if not port can be determined, the default port code of ZZZZZ will be returned.
	/// </summary>
	public class PortCodeLoader
	{
		public PortCodeLoader()
		{
		}

		public ZString GenerateRequiredPortCode(BusinessObjectFactory factory, ZString locodeCountry, ZString locodeCity, ZString state)
		{
			ZString portCode = "";

			if (!locodeCountry.IsEmpty)
			{
				portCode = GeneratePortCodeBasedOnCountry(factory, locodeCountry, state, locodeCity);
			}
			if (portCode.IsEmpty || portCode.EndsWith("ZZZ", StringComparison.Ordinal))
			{
				var portCodeFromState = GetPortCodeFromState(state);
				if (!portCodeFromState.Equals("ZZZZZ", StringComparison.Ordinal) || portCode.IsEmpty)
				{
					portCode = portCodeFromState;
				}
			}

			return portCode;
		}

		#region Protected Methods

		protected ZString GeneratePortCodeBasedOnCountry(BusinessObjectFactory factory, ZString locodeCountry, ZString state, ZString locodeCity)
		{
			ZString portCode = "";

			if (IsCountryCodeFormat(locodeCountry))
			{
				if (!locodeCity.IsEmpty)
				{
					portCode = GetPortFromNameAndCountryCode(factory, locodeCity, state, locodeCountry);
				}
				else
				{
					portCode = GetPortCodeFromCountry(locodeCountry);
				}
			}
			else
			{
				if (!locodeCity.IsEmpty)
				{
					portCode = GetPortFromNameAndCountryName(factory, locodeCity, state, locodeCountry);
				}
				else
				{
					portCode = GetPortCodeFromCountry(GetCodeForCountry(factory, locodeCountry));
				}
			}

			return portCode;
		}

		bool IsCountryCodeFormat(ZString locodeCountry)
		{
			return (locodeCountry.Length == 2);
		}

		protected ZString GetCodeForCountry(BusinessObjectFactory factory, ZString countryName)
		{
			ZString countryCode = "";
			ZQuery codeFilter = new ZQuery(RefCountrySchema.RN_Desc, countryName);
			RefCountry country = (RefCountry)factory.LoadTop1(typeof(RefCountry), codeFilter);
			if (country != null)
			{
				countryCode = country.RN_Code;
			}

			return countryCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected string GetPortCodeFromState(string stateCode)
		{
			string result = "";

			switch (stateCode.ToUpper().Replace(".", ""))
			{
				case "NSW":
				case "NEW SOUTH WALES":
					result = "AUSYD";
					break;

				case "VIC":
				case "VICTORIA":
					result = "AUMEL";
					break;

				case "QLD":
				case "QUEENSLAND":
					result = "AUBNE";
					break;

				case "WA":
				case "WESTERN AUSTRALIA":
					result = "AUPER";
					break;

				case "SA":
				case "SOUTH AUSTRALIA":
					result = "AUADL";
					break;

				case "TAS":
				case "TASMANIA":
					result = "AUHBA";
					break;

				case "NT":
				case "NORTHERN TERRITORY":
					result = "AUDRW";
					break;

				default:
					result = "ZZZZZ";
					break;
			}

			return result;
		}

		protected ZString GetPortCodeFromCountry(ZString countryCode)
		{
			return countryCode.Left(2).PadRight(5, 'Z');
		}

		protected ZString GetPortFromNameAndCountryCode(BusinessObjectFactory factory, ZString portName, ZString state, ZString countryCode)
		{
			ZQuery portFilter = GetPortFilter(portName, state);
			RefUNLOCO[] nameMatchingPorts = (RefUNLOCO[])factory.Load(typeof(RefUNLOCO), portFilter);

			ZString result = ZString.Empty;

			if (nameMatchingPorts.Length > 0)
			{
				string trimUpperCountryCode = countryCode.Trim().ToUpper();

				for (int i = 0; i < nameMatchingPorts.Length; i++)
				{
					if (nameMatchingPorts[i].Country != null)
					{
						string matchingPortCountryCode = nameMatchingPorts[i].RL_RN_NKCountryCode.Trim().ToUpper();
						if (matchingPortCountryCode == trimUpperCountryCode)
						{
							result = nameMatchingPorts[i].Code;
							break;
						}
					}
				}
			}

			if (result.IsEmpty)
			{
				result = GetPortCodeFromCountry(countryCode);
			}

			return result;
		}

		protected ZString GetPortFromNameAndCountryName(BusinessObjectFactory factory, ZString portName, ZString state, ZString countryName)
		{
			ZQuery portFilter = GetPortFilter(portName, state);
			RefUNLOCO[] nameMatchingPorts = (RefUNLOCO[])factory.Load(typeof(RefUNLOCO), portFilter);

			ZString result = ZString.Empty;

			if (nameMatchingPorts.Length > 0)
			{
				string trimUpperCountryName = countryName.Trim().ToUpper();

				for (int i = 0; i < nameMatchingPorts.Length; i++)
				{
					if (nameMatchingPorts[i].Country != null)
					{
						string matchingPortCountryName = nameMatchingPorts[i].Country.Description.Trim().ToUpper();
						if (matchingPortCountryName == trimUpperCountryName)
						{
							result = nameMatchingPorts[i].Code;
							break;
						}
					}
				}
			}

			if (result.IsEmpty)
			{
				result = GetPortCodeFromCountry(GetCodeForCountry(factory, countryName));
			}

			return result;
		}

		ZQuery GetPortFilter(ZString portName, ZString state)
		{
			ZQuery query = new ZQuery(RefUNLOCOSchema.RL_PortName, portName);
			query.OrderBy = RefUNLOCOSchema.RL_Code.Name;
			if (!state.IsEmpty)
			{
				ZDBOnlyQuery stateQuery = new ZDBOnlyQuery(typeof(RefUNLOCO));

				ZDBOnlySubQuery sub = new ZDBOnlySubQuery(typeof(RefCountryStates), RefUNLOCOSchema.RL_RW);
				sub.AddToFilter(RefCountryStatesSchema.RW_Code, state);
				stateQuery.AddSubQuery(sub, JoinCondition.And);
				query.AddToFilter(stateQuery);
			}
			return query;
		}

		#endregion
	}
}

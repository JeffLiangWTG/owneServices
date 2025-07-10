using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// This class holds supported Transport Modes and Container Modes.
	/// It also provides the validity of the combinations of Transport and Container modes.
	/// </summary>
	public class SupportedTransportAndContainerModes
	{
		/// <summary>
		/// Codes and Descriptions of supported Transport Modes.
		/// </summary>
		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair(Core.Constants.TransportModes.Sea, Res.GetString("4d9cba66-6f68-4f2c-a388-289386282610", "Sea Freight"));
					transportModeList.AddPair(Core.Constants.TransportModes.Air, Res.GetString("bd7b9e83-ec95-453d-bb57-1a739f960f07", "Air Freight"));
				}
				return transportModeList;
			}
		}
		CodeDescriptionPairList transportModeList;

		/// <summary>
		/// Codes and Descriptions of supported Container Modes.
		/// </summary>
		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				if (containerModeList == null)
				{
					containerModeList = new CodeDescriptionPairList();
					containerModeList.AddPair(Core.Constants.ContainerModes.FCL, Res.GetString("1170d1f5-2d7d-4ffa-a8a6-dd82e4f6b885", "Full Container Load"));
					containerModeList.AddPair(Core.Constants.ContainerModes.LCL, Res.GetString("61DB1901-D662-41C8-946A-68A54C81FD9E", "Less Container Load"));
					containerModeList.AddPair(Core.Constants.ContainerModes.BuyersConsol, Res.GetString("c56d8b9a-f69d-4d31-a3a2-353f4329fdc1", "Buyer's Consolidation"));
					containerModeList.AddPair(Core.Constants.ContainerModes.Groupage, Res.GetString("287677e3-ae94-4d58-ad63-d78b4aef269f", "Groupage / Freight All Kinds"));
					containerModeList.AddPair(Core.Constants.ContainerModes.ULD, Res.GetString("A2F170BB-E379-4426-905E-5069399A26CC", "Unit Load Device"));
					containerModeList.AddPair(Core.Constants.ContainerModes.Loose, Res.GetString("f5b115c6-518f-4e46-bfe5-b89ab9e5b409", "Loose"));
				}

				return containerModeList;
			}
		}
		CodeDescriptionPairList containerModeList;

		public enum Validity
		{
			ValidForAll,
			ValidForSupportOnly,
			Invalid
		}

		public Validity GetValidity(string transportMode, string containerMode)
		{
			if (validForSupportOnlyOptions.ContainsKey(transportMode) && validForSupportOnlyOptions[transportMode].Contains(containerMode))
			{
				return Validity.ValidForSupportOnly;
			}

			if (validForAllOptions.ContainsKey(transportMode) && validForAllOptions[transportMode].Contains(containerMode))
			{
				return Validity.ValidForAll;
			}

			return Validity.Invalid;
		}

		/// <summary>
		/// Valid combinations of Transport Modes and Container Modes
		/// </summary>
		readonly Dictionary<string, HashSet<string>> validForAllOptions = new Dictionary<string, HashSet<string>>
		{
			{
				Core.Constants.TransportModes.Sea,
				new HashSet<string>
				{
					Core.Constants.ContainerModes.FCL,
					Core.Constants.ContainerModes.BuyersConsol,
					Core.Constants.ContainerModes.Groupage,
					Core.Constants.ContainerModes.LCL
				}
			},
			{
				Core.Constants.TransportModes.Air,
				new HashSet<string>
				{
					Core.Constants.ContainerModes.Loose,
					Core.Constants.ContainerModes.ULD,
					Core.Constants.ContainerModes.BuyersConsol
				}
			}
		};

		/// <summary>
		/// These are combinations of transport modes and container modes which are not fully supported in production
		/// and which only CWSupport users may use. Primarily for functional testing.
		/// </summary>
		readonly Dictionary<string, HashSet<string>> validForSupportOnlyOptions = new Dictionary<string, HashSet<string>>
		{
			//TODO: Remove this dictionary and Validity.ValidForSupportOnly if we didn't use them in a year (2023)
		};
	}
}

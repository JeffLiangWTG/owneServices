using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Freight.Business
{
	public sealed class FreightValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public FreightValueSource(IBillGenerationSupport support)
		{
			this.support = support;

			providers = new INumberGeneratorValueProvider[]
			{
				new NumberGeneratorValueProvider(Keys.CarrierPrincipalCode, CarrierPrincipalCode),
				new NumberGeneratorValueProvider(Keys.ContainerTranshipmentIndicator, TranshipmentIndicator),
				new NumberGeneratorValueProvider(Keys.DestinationIATA, DestinationIATA),
				new NumberGeneratorValueProvider(Keys.DestinationUNLOCO, DestinationUNLOCO),
				new NumberGeneratorValueProvider(Keys.Direction, Direction),
				new NumberGeneratorValueProvider(Keys.OriginIATA, OriginIATA),
				new NumberGeneratorValueProvider(Keys.OriginUNLOCO, OriginUNLOCO),
				new NumberGeneratorValueProvider(Keys.TransportMode, TransportMode),
				new NumberGeneratorValueProvider(Keys.LoadIATA, LoadIATA),
				new NumberGeneratorValueProvider(Keys.LoadUNLOCO, LoadUNLOCO),
				new NumberGeneratorValueProvider(Keys.DischargeIATA, DischargeIATA),
				new NumberGeneratorValueProvider(Keys.DischargeUNLOCO, DischargeUNLOCO),
				new NumberGeneratorValueProvider(Keys.FirstLoadIATA, LoadIATA),
				new NumberGeneratorValueProvider(Keys.FirstLoadUNLOCO, LoadUNLOCO),
				new NumberGeneratorValueProvider(Keys.LastDischargeIATA, DischargeIATA),
				new NumberGeneratorValueProvider(Keys.LastDischargeUNLOCO, DischargeUNLOCO),
				new NumberGeneratorValueProvider(Keys.ServiceLevel, ServiceLevel),
			};
		}

		#region IEnumerable<INumberGeneratorValueProvider> Members

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return ((IEnumerable<INumberGeneratorValueProvider>)providers).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		string CarrierPrincipalCode(NumberGenerator generator, string detail)
		{
			OrgHeader carrierPrincipal = support.CarrierPrincipal;
			string result = null;

			if (carrierPrincipal != null)
			{
				result = carrierPrincipal.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode).Left(3);

				if (string.IsNullOrEmpty(result))
				{
					result = carrierPrincipal.OH_Code.Left(3);
				}
			}

			return result;
		}

		string TranshipmentIndicator(NumberGenerator generator, string detail)
		{
			return support.TranshipmentIndicator;
		}

		string DestinationIATA(NumberGenerator generator, string detail)
		{
			return PortIATA(support.Destination);
		}

		string DestinationUNLOCO(NumberGenerator generator, string detail)
		{
			return PortUNLOCOWithCarrierMapping(support.Destination, support.CarrierPrincipal);
		}

		string Direction(NumberGenerator generator, string detail)
		{
			RefUNLOCO originPort = support.Origin;
			RefUNLOCO destinationPort = support.Destination;
			ZString origin = originPort == null ? ZString.Empty : originPort.RL_Code;
			ZString destination = destinationPort == null ? ZString.Empty : destinationPort.RL_Code;

			var direction = ImportExportHelper.GetJobDirection(origin, destination);

			switch (direction)
			{
				case Directions.Import:
					return "I";
				case Directions.Export:
					return "E";
				case Directions.Domestic:
					return "D";
				default:
					return "O";
			}
		}

		string OriginIATA(NumberGenerator generator, string detail)
		{
			return PortIATA(support.Origin);
		}

		string OriginUNLOCO(NumberGenerator generator, string detail)
		{
			return PortUNLOCOWithCarrierMapping(support.Origin, support.CarrierPrincipal);
		}

		string TransportMode(NumberGenerator generator, string detail)
		{
			switch (support.TransportMode)
			{
				case Constants.TransportModes.Air:
					return "A";
				case Constants.TransportModes.Sea:
					return "S";
				case Constants.TransportModes.Rail:
					return "R";
				case Constants.TransportModes.Road:
					return "O";
				default:
					return "X";
			}
		}

		string LoadIATA(NumberGenerator generator, string detail)
		{
			return PortIATA(support.Load);
		}

		string LoadUNLOCO(NumberGenerator generator, string detail)
		{
			return PortUNLOCOWithCarrierMapping(support.Load, support.CarrierPrincipal);
		}

		string DischargeIATA(NumberGenerator generator, string detail)
		{
			return PortIATA(support.Discharge);
		}

		string DischargeUNLOCO(NumberGenerator generator, string detail)
		{
			return PortUNLOCOWithCarrierMapping(support.Discharge, support.CarrierPrincipal);
		}

		string ServiceLevel(NumberGenerator generator, string detail)
		{
			return support.ServiceLevel;
		}

		static string PortIATA(RefUNLOCO port)
		{
			return port == null ? string.Empty : port.RL_IATA.ToString();
		}

		static string PortUNLOCOWithCarrierMapping(RefUNLOCO port, OrgHeader carrierPrincipal)
		{
			if (port == null)
			{
				return string.Empty;
			}
			else
			{
				string result = port.RL_Code;

				if (carrierPrincipal != null)
				{
					ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_LocalGuid, port.PK);
					filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, carrierPrincipal.PK);
					filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.Port);
					filter.OrderBy = OrgPatternMatchOverrideSchema.OO_ForeignCode.Name;

					var ediMappings = carrierPrincipal.Factory.LoadTop1<OrgPatternMatchOverride>(filter);

					if (ediMappings != null)
					{
						result = ediMappings.OO_ForeignCode;
					}
				}

				return result;
			}
		}

		readonly INumberGeneratorValueProvider[] providers;
		readonly IBillGenerationSupport support;
	}
}

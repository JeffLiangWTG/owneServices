using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.SADH
{
	public class SADHFormDataLookups : AutoSADHFormDataLookups
	{
		public SADHFormDataLookups(AutoSADHFormData parent)
			: base(parent)
		{
			sADHData = (SADHFormData)parent;
			declaration = sADHData.Declaration;
		}
		readonly SADHFormData sADHData;
		readonly BaseJobDeclaration declaration;

		public ConsignorCollection SuppliersList
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsigneeCollection ImportersList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		#region OriginList
		public RefUNLOCOCollection OriginList
		{
			get
			{
				RefUNLOCOCollection result = new RefUNLOCOCollection(Factory, OriginPortFilter(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", sADHData.D1_RL_NKCountryOfOrigin));
				return result;
			}
		}

		protected virtual ZQuery OriginPortFilter()
		{
			PortLocation portLocation = PortLocation.All;
			if (sADHData.IsExport && !sADHData.IsImport)
			{
				portLocation = PortLocation.Local;
			}
			else if (sADHData.IsImport && !sADHData.IsExport)
			{
				portLocation = PortLocation.Foreign;
			}
			return PortQuery("", portLocation);
		}
		#endregion

		#region FinalDestinationList
		public RefUNLOCOCollection FinalDestinationList
		{
			get
			{
				RefUNLOCOCollection result = new RefUNLOCOCollection(Factory, FinalDestinationPortFilter(), LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", sADHData.D1_RL_NKCountryOfDestination));
				return result;
			}
		}

		ZQuery FinalDestinationPortFilter()
		{
			PortLocation portLocation = PortLocation.All;

			if (sADHData.IsImport && !sADHData.IsExport)
			{
				portLocation = PortLocation.Local;
			}
			else if (sADHData.IsExport && !sADHData.IsImport)
			{
				portLocation = PortLocation.Foreign;
			}
			return PortQuery(string.Empty, portLocation);
		}
		#endregion

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		#region DischargeList
		public RefUNLOCOCollection DischargeList
		{
			get
			{
				ZQuery portFilter = DischargePortFilter();
				RefUNLOCOCollection result = new RefUNLOCOCollection(Factory, portFilter, LocoMapSystemUsage);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", sADHData.D1_RL_NKPlaceOfUnloading));
				return result;
			}
		}

		protected virtual ZQuery DischargePortFilter()
		{
			PortLocation portLocation = PortLocation.All;

			if (sADHData.IsImport && !sADHData.IsExport)
			{
				portLocation = PortLocation.Local;
			}
			else if (sADHData.IsExport && !sADHData.IsImport)
			{
				portLocation = PortLocation.Foreign;
			}
			return PortQuery(sADHData.D1_ModeOfTransportAtTheBorder, portLocation);
		}
		#endregion

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public CodeDescriptionPairList MessageTypeList
		{
			get { return declaration.Lookups.MessageTypeList; }
		}

		public CodeDescriptionPairList ModeOfTransportAtBorderList
		{
			get { return declaration.Lookups.TransportTypeList; }
		}

		public CodeDescriptionPairList DeliveryTermsList
		{
			get { return declaration.Lookups.IncoTermList; }
		}

		public CodeDescriptionPairList PackagesTypeList
		{
			get { return declaration.Lookups.JE_TotalNoOfPacksPackType_List; }
		}

		public GlbBranchCollection BranchesList
		{
			get { return declaration.Lookups.BranchCollection; }
		}

		public RefVesselCollection VesselList
		{
			get { return declaration.Lookups.Vessels; }
		}

		public CodeDescriptionPairList InvoiceLineUQList
		{
			get
			{
				return RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);
			}
		}

		public CodeDescriptionPairList WeightUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#region Implementation
		public enum PortLocation { Local, Foreign, All }

		ZQuery PortQuery(string transportMode, PortLocation localForeign)
		{
			ZQuery result = new ZQuery();

			if (transportMode != null)
			{
				SchemaBoolColumn filterColumn = null;
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Air:
						filterColumn = RefUNLOCOSchema.RL_HasAirport;
						break;
					case Core.Constants.TransportModes.Sea:
						filterColumn = RefUNLOCOSchema.RL_HasSeaport;
						break;
					case Core.Constants.TransportModes.Road:
						filterColumn = RefUNLOCOSchema.RL_HasRoad;
						break;
					case Core.Constants.TransportModes.Rail:
						filterColumn = RefUNLOCOSchema.RL_HasRail;
						break;
					case Core.Constants.TransportModes.Mail:
						filterColumn = RefUNLOCOSchema.RL_HasPost;
						break;
					default:
						break;
				}
				if (filterColumn != null)
				{
					result.AddToFilter(filterColumn, true);
				}
			}
			if (localForeign != PortLocation.All)
			{
				SQLComparisonOperator oper = localForeign == PortLocation.Local ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith;
				result.AddToFilter(RefUNLOCOSchema.RL_Code, oper, declaration.CountryCode);
			}
			return result;
		}

		protected virtual ZString LocoMapSystemUsage
		{
			get { return ZString.Empty; }
		}
		#endregion
	}
}

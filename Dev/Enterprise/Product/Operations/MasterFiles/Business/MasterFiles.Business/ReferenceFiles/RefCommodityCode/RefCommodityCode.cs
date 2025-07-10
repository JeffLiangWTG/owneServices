using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityCode : AutoRefCommodityCode, IRefCommodityCode, IDocManagerSupport
	{
		public RefCommodityCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region RefCommodityCodeMaps

		[ChildEditable(true)]
		public RefCommodityCodeMapCollection RefCommodityCodeMaps
		{
			get
			{
				if (fRefCommodityCodeMaps == null)
				{
					fRefCommodityCodeMaps = new RefCommodityCodeMapCollection(this);
					fRefCommodityCodeMaps.AdditionalFilter = new ZQuery(RefCommodityCodeMapSchema.LC_RH_NKCommodityCode, RH_Code);
					RegisterEditableChildObject(fRefCommodityCodeMaps);
				}
				return fRefCommodityCodeMaps;
			}
		}

		RefCommodityCodeMapCollection fRefCommodityCodeMaps;

		#endregion

		#region RefCommodityRatingCodeMaps

		[ChildEditable(true)]
		public RefCommodityRatingCodeMapCollection RefCommodityRatingCodeMaps
		{
			get
			{
				if (fRefCommodityRatingCodeMaps == null)
				{
					fRefCommodityRatingCodeMaps = new RefCommodityRatingCodeMapCollection(this);
					fRefCommodityRatingCodeMaps.AdditionalFilter = new ZQuery(RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityParent, RH_Code);
					RegisterEditableChildObject(fRefCommodityRatingCodeMaps);
				}
				return fRefCommodityRatingCodeMaps;
			}
		}

		RefCommodityRatingCodeMapCollection fRefCommodityRatingCodeMaps;

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region Property Overrides

		public override ZDecimal RH_ReeferMinTemperature
		{
			get { return base.RH_ReeferMinTemperature; }
			set
			{
				if (value > RH_ReeferMaxTemperature)
				{
					RH_ReeferMaxTemperature = value;
				}
				base.RH_ReeferMinTemperature = value;
			}
		}

		public override ZBool RH_IsForwarding
		{
			get { return base.RH_IsForwarding; }
			set
			{
				if (base.RH_IsForwarding != value)
				{
					base.RH_IsForwarding = value;
					if (!IsValidationSuspended)
					{
						ValidateCommodityType();
					}
				}
			}
		}

		public override ZBool RH_IsShipping
		{
			get { return base.RH_IsShipping; }
			set
			{
				if (base.RH_IsShipping != value)
				{
					base.RH_IsShipping = value;
					if (!IsValidationSuspended)
					{
						ValidateCommodityType();
					}
				}
			}
		}

		public override ZBool RH_IsLandTransport
		{
			get { return base.RH_IsLandTransport; }
			set
			{
				if (base.RH_IsLandTransport != value)
				{
					base.RH_IsLandTransport = value;
					if (!IsValidationSuspended)
					{
						ValidateCommodityType();
					}
				}
			}
		}

		[System.ComponentModel.ReadOnly(true)]
		public override ZBool RH_IsSystem
		{
			get { return base.RH_IsSystem; }
			set { base.RH_IsSystem = value; }
		}

		public override ZBool RH_IsPersonalEffects
		{
			get { return base.RH_IsPersonalEffects; }
			set
			{
				if (base.RH_IsPersonalEffects != value)
				{
					base.RH_IsPersonalEffects = value;
					if (!IsValidationSuspended)
					{
						ValidateCommodityType();
					}
				}
			}
		}
		public override ZString RH_Code
		{
			get
			{
				return base.RH_Code;
			}
			set
			{
				base.RH_Code = value;
				if (fRefCommodityCodeMaps != null && !RH_CodeInfo.HasErrors())
				{
					foreach (var localMaps in fRefCommodityCodeMaps.ToArray())
					{
						localMaps.LC_RH_NKCommodityCode = value;
					}
					fRefCommodityCodeMaps.AdditionalFilter = new ZQuery(RefCommodityCodeMapSchema.LC_RH_NKCommodityCode, RH_Code);
					fRefCommodityCodeMaps.RefreshBinding();
				}
			}
		}

		void ValidateCommodityType()
		{
			Validation.ValidateRH_IsForwarding();
			Validation.ValidateRH_IsShipping();
			Validation.ValidateRH_IsLandTransport();
			Validation.ValidateRH_IsPersonalEffects();
		}

		#region RH_UniversalCommodityGroup

		[List("Lookups.UniversalCommodityCodeBizoList")]
		public override ZString RH_UniversalCommodityGroup
		{
			get => base.RH_UniversalCommodityGroup;
			set => base.RH_UniversalCommodityGroup = value;
		}

		#endregion

		#region RH_IATACommodityItem

		[List("Lookups.IATACommodityItemList")]
		public override ZString RH_IATACommodityItem
		{
			get { return base.RH_IATACommodityItem; }
			set
			{
				if (base.RH_IATACommodityItem != value)
				{
					base.RH_IATACommodityItem = value;
				}
			}
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CommodityCode);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		[TranslatableDataField(Schema.TableName, Schema.RH_Description, DataXmlFilePaths.RefTables, MaxLength = Schema.RH_DescriptionMaxLength, Type = typeof(RefCommodityCode), SecurityCheckpoint = "CommodityModify", Asmid = ResString.AssemblyId)]
		public override ZString RH_Description
		{
			get { return base.RH_Description; }
			set { base.RH_Description = value; }
		}

		public MultilingualString RH_DescriptionMultilingual
		{
			get { return GetMultilingual(RH_DescriptionInfo); }
		}

		public static IEnumerable<RefCommodityCode> GetCommodities(BusinessObjectFactory factory, string universalCommodityGroup)
		{
			ZQuery commoditiesQuery;
			switch (universalCommodityGroup)
			{
				case HAZD:
					commoditiesQuery = new ZQuery(RefCommodityCodeSchema.RH_IsHazardous, true);
					break;
				case PERS:
					commoditiesQuery = new ZQuery(RefCommodityCodeSchema.RH_IsPerishable, true);
					break;
				case TIMB:
					commoditiesQuery = new ZQuery(RefCommodityCodeSchema.RH_IsTimber, true);
					break;
				case FLAM:
					commoditiesQuery = new ZQuery(RefCommodityCodeSchema.RH_IsFlammable, true);
					break;
				case CNVT:
					commoditiesQuery = new ZQuery(RefCommodityCodeSchema.RH_ContainerVentRequired, true);
					break;
				default:
					if (!string.IsNullOrEmpty(universalCommodityGroup))
					{
						commoditiesQuery = new ZQuery(RefCommodityCodeSchema.RH_UniversalCommodityGroup, universalCommodityGroup);
					}
					else
					{
						return Enumerable.Empty<RefCommodityCode>();
					}
					break;
			}

			return factory.Load<RefCommodityCode>(commoditiesQuery) ?? Enumerable.Empty<RefCommodityCode>();
		}

		public bool HasGroup(string commodityGroup)
		{
			return commodityGroup == RH_UniversalCommodityGroup
				|| (commodityGroup == HAZD && RH_IsHazardous)
				|| (commodityGroup == PERS && RH_IsPerishable)
				|| (commodityGroup == TIMB && RH_IsTimber)
				|| (commodityGroup == FLAM && RH_IsFlammable)
				|| (commodityGroup == CNVT && RH_ContainerVentRequired);
		}

		// TODO: Move to UniversalGroups class
		public const string HAZD = "HAZD";
		public const string PERS = "PERS";
		public const string TIMB = "TIMB";
		public const string FLAM = "FLAM";
		public const string CNVT = "CNVT";

		public static class UniversalGroups
		{
			public const string General = "GENL";
			public const string NotClassified = "NCLS";
		}

		#region RefAirlineCommodityCode

		public RefAirlineCommodityCode RefIATACommodityCode
		{
			get
			{
				var query = new ZQuery(RefAirlineCommodityCodeSchema.RAC_Code, RH_IATACommodityItem);
				query.AddToFilter(RefAirlineCommodityCodeSchema.RAC_AirlineID, SQLComparisonOperator.Equal, ZString.Empty);
				return Factory.LoadTop1<RefAirlineCommodityCode>(query);
			}
		}

		#endregion

		#region RatingLocalCode

		public RefCommodityCodeMap RatingLocalCode
		{
			get => RefCommodityCodeMaps.FirstOrDefault(lc => lc.LC_LocalCodeProvider == GlobalCommodityCodeProviderList.Codes.Rating);
		}

		#endregion

		public ZString LocalCodesAsString => string.Join(",", RefCommodityCodeMaps.Select(lc => $"{lc.LC_LocalCodeProvider}:{lc.LC_LocalCode}"));

		public ZString RatingCodesAsString => string.Join(",", RefCommodityRatingCodeMaps.Select(lc => lc.RI_RH_NKCommodityChild));
	}
}

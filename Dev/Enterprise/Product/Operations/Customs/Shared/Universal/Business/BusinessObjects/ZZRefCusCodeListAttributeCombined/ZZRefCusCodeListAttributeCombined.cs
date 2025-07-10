using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class ZZRefCusCodeListAttributeCombined : AutoZZRefCusCodeListAttributeCombined, ICanDelete, ITransportModeListSupporter
	{
		public ZZRefCusCodeListAttributeCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoZZRefCusCodeListAttributeCombined.Schema
		{
			public const string ZZE_TransportModes = "ZZE_TransportModes";
			public const string CodeListAttributeNamePK = "CodeListAttributeNamePK";
		}

		[ResourceStringData("E493209A-6990-42D4-B5D4-5C90AD223E65", Caption = "Start Date")]
		[ReadOnlyMember(nameof(IsDateRangeNotUsed))]
		public override ZDateTime ZZE_StartDate { get => base.ZZE_StartDate; set => base.ZZE_StartDate = value; }

		[ResourceStringData("48605255-4289-418E-ADF0-3D6E95E4ACB4", Caption = "End Date")]
		[ReadOnlyMember(nameof(IsDateRangeNotUsed))]
		public override ZDateTime ZZE_EndDate { get => base.ZZE_EndDate; set => base.ZZE_EndDate = value; }

		bool IsDateRangeNotUsed => !CodeListAttributeName?.ZXE_IsDateRangeUsed ?? true;

		[RelatedBusinessObject("CodeList")]
		public override ZGuid ZZE_ZZD_CodeList
		{
			get { return base.ZZE_ZZD_CodeList; }
			set { base.ZZE_ZZD_CodeList = value; }
		}

		public ZZRefCusCodeListCombined CodeList
		{
			get { return Factory.Load<ZZRefCusCodeListCombined>(ZZE_ZZD_CodeList); }
		}

		public ZString NameDescription
		{
			get { return CodeListAttributeName?.ZXE_Description ?? ZString.Empty; }
		}

		public RefCusCodeListAttributeName CodeListAttributeName
		{
			get
			{
				RefCusCodeListAttributeName codeListAttributeName = null;
				if (!ZZE_ZXE_NKName.IsEmpty)
				{
					var codeList = CodeList;
					if (codeList != null)
					{
						var country = codeList.ZZD_CountryOrGrouping;
						var codeType = codeList.ZZD_CodeType;
						if (!codeType.IsEmpty)
						{
							codeListAttributeName = Factory.GetCachedValue(
							string.Format(CultureInfo.InvariantCulture, "CodeListAttributeName{0}_{1}_{2}", country, codeType, ZZE_ZXE_NKName), () =>
							{
								var query = new ZQuery(RefCusCodeListAttributeNameSchema.ZXE_ZZK_NKCodeType, CodeList.ZZD_CodeType);
								query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_ZZZ_NKDataGrouping, CodeList.ZZD_CountryOrGrouping);
								query.AddToFilter(RefCusCodeListAttributeNameSchema.ZXE_Name, ZZE_ZXE_NKName);
								return Factory.LoadTop1<RefCusCodeListAttributeName>(query);
							});
						}
					}
				}
				return codeListAttributeName;
			}
		}

		[List("Lookups.NameList")]
		public ZGuid CodeListAttributeNamePK
		{
			get => CodeListAttributeName?.PK ?? ZGuid.Empty;
			set
			{
				RefCusCodeListAttributeName codeListAttributeName = null;

				var codeList = CodeList;
				if (value.IsValid && codeList != null)
				{
					codeListAttributeName = Factory.Load<RefCusCodeListAttributeName>(value);
				}
				if (codeListAttributeName != null && codeListAttributeName.ZXE_ZZK_NKCodeType == codeList.ZZD_CodeType && codeListAttributeName.ZXE_ZZZ_NKDataGrouping == codeList.ZZD_CountryOrGrouping)
				{
					ZZE_ZXE_NKName = codeListAttributeName.ZXE_Name;
				}
				else
				{
					ZZE_ZXE_NKName = ZString.Empty;
				}
				CodeListAttributeNamePKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CodeListAttributeNamePKInfo => GetZPropertyInfo(Schema.CodeListAttributeNamePK);

		[List("Lookups.ValueList")]
		public override ZString ZZE_Value
		{
			get => base.ZZE_Value;
			set => base.ZZE_Value = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined|DescrptionOfZZE_Value", Caption = "Value Description", ShortCaption = "Value Desc.", FullDescription = "Shows the description of the attribute value if the description can be derived from another code list.")]
		public ZString DescriptionOfZZE_Value
		{
			get
			{
				if (!string.IsNullOrEmpty(CodeListAttributeName?.ZXE_ZZK_NKCodeTypeForValueList))
				{
					return Lookups.ValueList.GetDescriptionFromCode(ZZE_Value);
				}

				return ZString.Empty;
			}
		}

		public ZString ZZE_ValueDataFieldType
		{
			get
			{
				var result = nameof(FieldType.Text);
				var codeListAttributeName = CodeListAttributeName;
				if (codeListAttributeName != null)
				{
					var valueDataType = codeListAttributeName.ZXE_ValueDataType.ToUpperInvariant();
					switch (valueDataType)
					{
						case "":
						case Constants.RefCusCodeListAttributeName.ValueDataTypes.String:
							if (Lookups.ValueList.Count > 0)
							{
								result = nameof(FieldType.TextDropEdit);
							}
							break;
						case Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean:
							result = nameof(FieldType.TextDropEdit);
							break;
						case Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer:
							result = nameof(FieldType.Integer);
							break;
						case Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal:
							result = nameof(FieldType.Decimal);
							break;
					}
				}
				return result;
			}
		}

		public ZByte ZZE_ValueDecimalPlaces
		{
			get
			{
				var result = ZByte.Zero;
				if (ZZE_ValueDataFieldType == nameof(FieldType.Decimal))
				{
					result = CodeListAttributeName.ZXE_DecimalPlaces;
				}
				return result;
			}
		}

		#region Transport Modes

		[ResourceStringData("Enterprise.Customs.Universal.ZZRefCusCodeListAttributeCombined|ZZE_TransportModes", Caption = "Transport Modes", ShortCaption = "Trans. Modes")]
		public ZString ZZE_TransportModes
		{
			get { return ZString.Join(",", TransportModePairList.Where(x => x.Value).Select(x => x.Description).ToArray()); }
		}

		public ZPropertyInfo ZZE_TransportModesInfo => GetZPropertyInfo(Schema.ZZE_TransportModes);

		public ZBoolDescriptionPairList TransportModePairList
		{
			get { return fTransportModePairList ?? (fTransportModePairList = this.CreateNewTransportModePairList()); }
		}
		ZBoolDescriptionPairList fTransportModePairList;

		public static string GetTransportModePropertyName(ZString transportMode)
		{
			switch (transportMode)
			{
				case RefTransportModeList.Codes.AIR:
					return AutoZZRefCusCodeListAttributeCombined.Schema.ZZE_IsAir;
				case RefTransportModeList.Codes.SEA:
					return AutoZZRefCusCodeListAttributeCombined.Schema.ZZE_IsSea;
				case RefTransportModeList.Codes.FIX:
					return AutoZZRefCusCodeListAttributeCombined.Schema.ZZE_IsFix;
				case RefTransportModeList.Codes.RAI:
					return AutoZZRefCusCodeListAttributeCombined.Schema.ZZE_IsRai;
				case RefTransportModeList.Codes.ROA:
					return AutoZZRefCusCodeListAttributeCombined.Schema.ZZE_IsRoa;
				case RefTransportModeList.Codes.MAI:
					return AutoZZRefCusCodeListAttributeCombined.Schema.ZZE_IsMai;
				case RefTransportModeList.Codes.INW:
					return AutoZZRefCusCodeListAttributeCombined.Schema.ZZE_IsInw;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Transport Mode {0} is not supported for ZZRefCusCodeListAttributeCombined.", transportMode));
			}
		}

		string ITransportModeListSupporter.GetTransportModePropertyName(ZString transportMode)
		{
			return GetTransportModePropertyName(transportMode);
		}

		ZPropertyInfo ITransportModeListSupporter.TransportModesPropertyInfo => ZZE_TransportModesInfo;

		#endregion

		public bool IsSystem
		{
			get
			{
				var codeList = CodeList;
				return codeList != null && codeList.ZZD_IsSystem;
			}
		}

		public override bool ReadOnly
		{
			get
			{
				var codeList = CodeList;
				return (codeList != null && codeList.ReadOnly) || base.ReadOnly;
			}
			set { base.ReadOnly = value; }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get
			{
				var codeList = CodeList;
				return codeList == null || ((ICanDelete)codeList).CanDelete;
			}
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get
			{
				var codeList = CodeList;
				return codeList == null ? (NoResString)string.Empty : ((ICanDelete)codeList).ReasonForNotAbleToDelete;
			}
		}

		#endregion
	}
}

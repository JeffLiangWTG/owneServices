using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.RtfConverter;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgRateFeeChargeLevel : AutoOrgRateFeeChargeLevel
	{
		public OrgRateFeeChargeLevel(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ORF_ServiceType

		[List("Lookups.ServiceTypeList")]
		public override ZString ORF_ServiceType
		{
			get { return base.ORF_ServiceType; }
			set
			{
				base.ORF_ServiceType = value;
			}
		}

		#endregion

		#region ServiceDescription

		public ZString ServiceDescription
		{
			get
			{
				if (ORF_ServiceType == ZString.Empty || ChargeType == null)
				{
					return ZString.Empty;
				}

				return ChargeType.Description;
			}
		}

		#endregion

		#region ORF_Amount1Type

		[List("Lookups.Amount1TypeList")]
		public override ZString ORF_Amount1Type
		{
			get { return base.ORF_Amount1Type; }
			set
			{
				base.ORF_Amount1Type = value;

				if (IsAmount1TypeNone)
				{
					ORF_Amount1 = 0;
					ORF_RX_NKAmount1Currency = ZString.Empty;
				}
			}
		}

		#endregion

		#region ORF_NoteData_HTML

		public ZBlob ORF_NoteData_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(ORF_NoteData);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.ORF_NoteData = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion

		#region Amount 1 Read Only Checking

		public bool IsAmount1TypeNone
		{
			get
			{
				return ORF_Amount1Type == OrgConstants.ServiceLevelAmountTypes.Code.None;
			}
		}

		#endregion

		#region ORF_Amount1

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IsAmount1TypeNone))]
		public override ZDecimal ORF_Amount1
		{
			get
			{
				return base.ORF_Amount1;
			}
			set
			{
				base.ORF_Amount1 = value;
			}
		}

		#endregion

		#region ORF_RX_NKAmount1Currency

		[ReadOnlyMember(nameof(IsAmount1TypeNone))]
		public override ZString ORF_RX_NKAmount1Currency
		{
			get
			{
				return base.ORF_RX_NKAmount1Currency;
			}
			set
			{
				base.ORF_RX_NKAmount1Currency = value;
			}
		}

		#endregion

		#region ORF_Amount2Type

		[List("Lookups.Amount2TypeList")]
		public override ZString ORF_Amount2Type
		{
			get { return base.ORF_Amount2Type; }
			set
			{
				base.ORF_Amount2Type = value;

				if (IsAmount2TypeNone)
				{
					ORF_Amount2 = 0;
					ORF_RX_NKAmount2Currency = ZString.Empty;
				}
			}
		}

		#endregion

		#region Amount 2 readonly

		public bool IsAmount2TypeNone
		{
			get
			{
				return ORF_Amount2Type == OrgConstants.ServiceLevelAmountTypes.Code.None;
			}
		}

		#endregion

		#region Amount2

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IsAmount2TypeNone))]
		public override ZDecimal ORF_Amount2
		{
			get
			{
				return base.ORF_Amount2;
			}
			set
			{
				base.ORF_Amount2 = value;
			}
		}

		#endregion

		#region ORF_RX_NKAmount2Currency

		[ReadOnlyMember(nameof(IsAmount2TypeNone))]
		public override ZString ORF_RX_NKAmount2Currency
		{
			get
			{
				return base.ORF_RX_NKAmount2Currency;
			}
			set
			{
				base.ORF_RX_NKAmount2Currency = value;
			}
		}

		#endregion

		#region service level description

		public ZString LevelDescription
		{
			get
			{
				if (ChargeLevel == null)
				{
					return ZString.Empty;
				}

				return (ZString)ChargeLevel.Description;
			}
		}

		#endregion

		#region ORF_Level

		[List("Lookups.ServiceLevelList")]
		public override ZString ORF_Level
		{
			get { return base.ORF_Level; }
			set
			{
				base.ORF_Level = value;

				if (ChargeLevel != null)
				{
					bool noChange = ORF_Amount1.IsDefault && ORF_Amount1Type.IsDefault && ORF_RX_NKAmount1Currency.IsDefault
						&& ORF_Amount2.IsDefault && ORF_Amount2Type.IsDefault && ORF_RX_NKAmount2Currency.IsDefault;

					if (noChange)
					{
						ORF_Amount1 = ChargeLevel.Amount1;
						ORF_Amount1Type = ChargeLevel.Amount1Type;
						ORF_RX_NKAmount1Currency = ChargeLevel.Amount1Currency;
						ORF_Amount2 = ChargeLevel.Amount2;
						ORF_Amount2Type = ChargeLevel.Amount2Type;
						ORF_RX_NKAmount2Currency = ChargeLevel.Amount2Currency;
					}
				}
			}
		}

		#endregion

		#region Charge Type

		public FeeChargeType ChargeType
		{
			get
			{
				return OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value.FeeChargeTypes.Cast<FeeChargeType>().FirstOrDefault(t => t.Code == ORF_ServiceType);
			}
		}

		#endregion

		#region Charge Level

		public FeeChargeLevel ChargeLevel
		{
			get
			{
				if (ChargeType != null)
				{
					return ChargeType.FeeChargeLevels.Cast<FeeChargeLevel>().FirstOrDefault(t => t.Code == ORF_Level);
				}

				return null;
			}
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return Header != null &&
				(!Header.SecurityProvider.HasModifyDetailsRatingAndTariffsSecurity ||
				!Header.SecurityProvider.HasModifyDetailsSecurity ||
				MetaData.GetReadOnlyExcludingMethodProvider(this, property));
		}

		#endregion
	}
}

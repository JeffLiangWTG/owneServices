using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EntityFramework.ColumnValueRanker;

namespace Enterprise.MasterFiles.Business
{
	public class StmNumberRangeMatchingDetail : AutoStmNumberRangeMatchingDetail
	{
		public StmNumberRangeMatchingDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoStmNumberRangeMatchingDetail.Schema
		{
			public const string PatentNumber = "PatentNumber";
			public const string CustomsArea = "CustomsArea";
		}

		#region NumberFountain values

		[ReadOnly(true)]
		public ZLong CurrentNumber
		{
			get { return LinkedFountain?.SN_ValueForDisplay ?? ZLong.Zero; }
		}

		[ReadOnly(true)]
		public ZLong MinimumValue
		{
			get { return LinkedFountain?.SN_MinimumValue ?? ZLong.Zero; }
		}

		[ReadOnly(true)]
		public ZLong MaximumValue
		{
			get { return LinkedFountain?.SN_MaximumValue ?? ZLong.Zero; }
		}

		[ReadOnly(true)]
		[ResourceStringData("StmNumberRangeMatchingDetail|CanRollover", Caption = "Can Roll-over")]
		public ZBool CanRollover
		{
			get { return LinkedFountain?.SN_CanRollover ?? false; }
		}

		#endregion

		#region LinkedFountain

		public ViewStmNums LinkedFountain
		{
			get { return linkedFountain ??= LinkedFountainCore; }
		}
		protected ViewStmNums linkedFountain;

		protected virtual ViewStmNums LinkedFountainCore => Owner?.Fountains.TryGetStmNums(NRM_RangeType, NRM_Prefix);

		#endregion

		#region Owner

		public IViewStmNumsOwner Owner => NRM_OwnerTableCode == OrgHeaderSchema.Constants.Prefix ? Factory.Load<OrgHeader>(NRM_OwnerId) : Factory.Load<GlbStaff>(NRM_OwnerId);

		#endregion

		#region Override Properties

		#region NRM_Prefix

		[List(nameof(Lookups) + "." + nameof(StmNumberRangeMatchingDetailLookups.Prefixes))]
		[ResourceStringData("StmNumberRangeMatchingDetail|NRM_Prefix", Caption = "Prefix")]
		[ReadOnlyMember(nameof(PrefixIsReadOnly))]
		public override ZString NRM_Prefix
		{
			get { return base.NRM_Prefix; }

			set
			{
				if (base.NRM_Prefix != value)
				{
					linkedFountain = null;
					base.NRM_Prefix = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateNRM_OwnerId();
						Validation.ValidateNRM_RangeType();
					}
				}
			}
		}

		#endregion

		#region PrefixIsReadOnly

		bool PrefixIsReadOnly
		{
			get { return string.IsNullOrEmpty(NRM_RangeType); }
		}

		#endregion

		#region NRM_RangeType

		[List(nameof(Lookups) + "." + nameof(StmNumberRangeMatchingDetailLookups.RangeTypes))]
		[MaxLength(3)]
		[ResourceStringData("StmNumberRangeMatchingDetail|NRM_RangeType", Caption = "Range Type")]
		public override ZString NRM_RangeType
		{
			get { return base.NRM_RangeType; }

			set
			{
				if (base.NRM_RangeType != value)
				{
					linkedFountain = null;
					base.NRM_RangeType = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateNRM_OwnerId();
						Validation.ValidateNRM_Prefix();
						Validation.ValidateNRM_OH_Client();
						Validation.ValidateNRM_WW_Whs();
						Validation.ValidatePatentNumber();
						Validation.ValidateCustomsArea();
					}
					NRM_PrefixInfo.RefreshBinding();
				}
			}
		}

		public bool IsTransportReferenceNumbers => NRM_RangeType == OrgConstants.NumberFountains.Code.TransportReferenceNumbers;

		public bool IsPatentNumber => NRM_RangeType == OrgConstants.NumberFountains.Code.PatentNumber;

		#endregion

		#region NRM_OwnerId

		public override ZGuid NRM_OwnerId
		{
			get { return base.NRM_OwnerId; }

			set
			{
				if (base.NRM_OwnerId != value)
				{
					linkedFountain = null;
					base.NRM_OwnerId = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateNRM_Prefix();
						Validation.ValidateNRM_RangeType();
						Validation.ValidateNRM_OH_Client();
						Validation.ValidateNRM_WW_Whs();
						Validation.ValidatePatentNumber();
						Validation.ValidateCustomsArea();
					}
				}
			}
		}

		#endregion

		#region NRM_OH_Client

		[ResourceStringData("StmNumberRangeMatchingDetail|NRM_OH_Client", Caption = "Client")]
		public override ZGuid NRM_OH_Client
		{
			get { return base.NRM_OH_Client; }

			set
			{
				if (base.NRM_OH_Client != value)
				{
					base.NRM_OH_Client = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateNRM_OwnerId();
						Validation.ValidateNRM_RangeType();
						Validation.ValidateNRM_WW_Whs();
					}
				}
			}
		}

		#endregion

		#region NRM_WW_Whs

		[List(nameof(Lookups) + "." + nameof(StmNumberRangeMatchingDetailLookups.Warehouses))]
		[ResourceStringData("StmNumberRangeMatchingDetail|NRM_WW_Whs", Caption = "Warehouse")]
		public override ZGuid NRM_WW_Whs
		{
			get { return base.NRM_WW_Whs; }

			set
			{
				if (base.NRM_WW_Whs != value)
				{
					base.NRM_WW_Whs = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateNRM_OwnerId();
						Validation.ValidateNRM_RangeType();
						Validation.ValidateNRM_OH_Client();
					}
				}
			}
		}

		#endregion

		#endregion

		#region MatchWithValueOrNull

		public static ColumnValuesPair MatchWithValueOrNull(SchemaColumn nullableColumn, object value)
		{
			return new ColumnValuesPair(nullableColumn, new object[] { value, null });
		}

		#endregion

		#region GetMatchingNumberRange

		public static StmNumberRangeMatchingDetail GetMatchingNumberRange(OrgHeader owner, string rangeType, IReadOnlyList<ColumnValuesPair> rankConditions = null)
		{
			Argument.NotNull(owner, nameof(owner));

			var ranker = new ColumnValueRanker();
			var conditionsHashSet = new HashSet<SchemaColumn>();
			if (rankConditions != null)
			{
				foreach (var condition in rankConditions)
				{
					ranker.Add(condition.Column, condition.Values);
					conditionsHashSet.Add(condition.Column);
				}
			}

			// if ranking columns (i.e. Whs, Client) are not specified this loop will add null conditions
			// to main query to prevent find wrong match. (e.g. null warehouse will not match a range for a specific warehouse)
			foreach (var condition in GetRankColumnAndDefaultValues.Where(c => !conditionsHashSet.Contains(c.Key)))
			{
				ranker.Add(condition.Key, condition.Value);
			}

			return GetMatchingNumberRange(owner, rangeType, ranker);
		}

		static StmNumberRangeMatchingDetail GetMatchingNumberRange(OrgHeader owner, string rangeType, ColumnValueRanker ranker)
		{
			StmNumberRangeMatchingDetail matchingNumberRange = null;
			if (owner.OrgFountains.Any())
			{
				var mainQuery = new ZQuery(StmNumberRangeMatchingDetailSchema.NRM_OwnerId, owner.PK);
				mainQuery.AddToFilter(StmNumberRangeMatchingDetailSchema.NRM_RangeType, rangeType);

				matchingNumberRange = ranker.GetBestMatch<StmNumberRangeMatchingDetail>(owner.Factory, mainQuery);
			}
			return matchingNumberRange;
		}

		#endregion

		#region GetRankColumnAndDefaultValues

		protected static Dictionary<SchemaGuidColumn, object> GetRankColumnAndDefaultValues
		{
			get
			{
				return new Dictionary<SchemaGuidColumn, object>
				{
					{ StmNumberRangeMatchingDetailSchema.NRM_WW_Whs, null },
					{ StmNumberRangeMatchingDetailSchema.NRM_OH_Client, null }
				};
			}
		}

		#endregion

		#region NRM_MatchingKey

		ZString[] MatchingKeys => NRM_MatchingKey.Split(Separator);

		[ReadOnly(true)]
		[BusinessObjectTestExclude()]
		[ResourceStringData("StmNumberRangeMatchingDetail|PatentNumber", Caption = "Patent Number")]
		public ZString PatentNumber
		{
			get => MatchingKeys.ElementAtOrDefault(0);
			set
			{
				if (PatentNumber != value)
				{
					NRM_MatchingKey = $"{value}{Separator}{CustomsArea}";
					if (!IsValidationSuspended)
					{
						Validation.ValidateNRM_OwnerId();
						Validation.ValidateNRM_RangeType();
						Validation.ValidatePatentNumber();
						Validation.ValidateCustomsArea();
					}
				}
				PatentNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PatentNumberInfo => GetZPropertyInfo(Schema.PatentNumber);

		[BusinessObjectTestExclude()]
		[ResourceStringData("StmNumberRangeMatchingDetail|CustomsArea", Caption = "Customs Area")]
		[List(nameof(Lookups) + "." + nameof(StmNumberRangeMatchingDetailLookups.CustomsAreaList))]
		public ZString CustomsArea
		{
			get
			{
				return MatchingKeys.ElementAtOrDefault(1);
			}
			set
			{
				if (CustomsArea != value)
				{
					NRM_MatchingKey = $"{PatentNumber}{Separator}{value}";
					if (!IsValidationSuspended)
					{
						Validation.ValidateNRM_OwnerId();
						Validation.ValidateNRM_RangeType();
						Validation.ValidatePatentNumber();
						Validation.ValidateCustomsArea();
					}
				}
				CustomsAreaInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsAreaInfo => GetZPropertyInfo(Schema.CustomsArea);

		const string Separator = "|";

		#endregion

		#region Implementation

#if DEBUG
		#region FillWithValidTestDataCore

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NRM_RangeType = OrgConstants.NumberFountains.Code.TransportReferenceNumbers;
			NRM_OwnerTableCode = ZArchitecture.Schema.OrgHeaderSchema.Constants.Prefix;
		}

		#endregion

#endif

		#endregion
	}
}

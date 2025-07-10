using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class InBond : AutoCusInBondMoveHeader, Integration.Customs.US.eManifest.ICusInBondMoveHeader
	{
		public InBond(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoCusInBondMoveHeader.Schema
		{
			public const string InBondNumber = "InBondNumber";
		}

		#endregion

		#region Loader

		internal static InBond LoadOrCreate(Shipment master)
		{
			var query = new ZDBOnlyQuery(typeof(InBond));
			var subQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID);
			subQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, master.PK);
			subQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, master.TablePrefix);
			subQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusInBondMoveHeaderSchema.Constants.Prefix);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var inBond = master.Factory.LoadTop1<InBond>(query);
			if (inBond == null)
			{
				inBond = master.Factory.New<InBond>();
				inBond.BM_BH = master.Trip.PK;
				var inBondPivot = master.Factory.New<GenPivot>();
				inBondPivot.XX_Relation1ID = master.PK;
				inBondPivot.XX_Relation1TableCode = master.TablePrefix;
				inBondPivot.XX_Relation2ID = inBond.PK;
				inBondPivot.XX_Relation2TableCode = inBond.TablePrefix;
			}
			inBond.Shipment = master;
			return inBond;
		}

		internal Shipment Shipment { get; set; }

		#endregion

		#region Properties

		#region BM_BH

		[RelatedBusinessObject("Trip")]
		public override ZGuid BM_BH
		{
			get { return base.BM_BH; }
			set { base.BM_BH = value; }
		}

		public Trip Trip
		{
			get { return Factory.Load<Trip>(BM_BH); }
		}

		#endregion

		#region BM_DestinationPortCode

		[List(nameof(Lookups) + "." + nameof(InBondLookups.ScheduleDPortCodes))]
		public override ZString BM_DestinationPortCode
		{
			get { return base.BM_DestinationPortCode; }
			set { base.BM_DestinationPortCode = value; }
		}

		#endregion

		#region BM_ForeignDestPortKCode

		[List(nameof(Lookups) + "." + nameof(InBondLookups.ScheduleKPortCodes))]
		public override ZString BM_ForeignDestPortKCode
		{
			get { return base.BM_ForeignDestPortKCode; }
			set { base.BM_ForeignDestPortKCode = value; }
		}

		#endregion

		#region BM_InBondEntryType

		[List(nameof(Lookups) + "." + nameof(InBondLookups.InBondTypes))]
		public override ZString BM_InBondEntryType
		{
			get { return base.BM_InBondEntryType; }
			set
			{
				var hasChanges = BM_InBondEntryType != value;
				base.BM_InBondEntryType = value;
				if (hasChanges && !IsCopying)
				{
					MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		#endregion

		#region BM_RL_NKDestinationPort

		public override ZString BM_RL_NKDestinationPort
		{
			get { return base.BM_RL_NKDestinationPort; }
			set
			{
				var oldValue = BM_RL_NKDestinationPort;
				base.BM_RL_NKDestinationPort = value;
				if (!IsCopying && oldValue != BM_RL_NKDestinationPort)
				{
					BM_DestinationPortCode = USScheduleResolver.GetScheduleCode(Schedule.D, BM_RL_NKDestinationPort, USLocoMapSystemUsageList.Codes.All, Factory).Left(BM_DestinationPortCodeInfo.MaxLength);
				}
			}
		}

		#endregion

		#region BM_RL_NKForeignDestPort

		public override ZString BM_RL_NKForeignDestPort
		{
			get { return base.BM_RL_NKForeignDestPort; }
			set
			{
				var oldValue = BM_RL_NKForeignDestPort;
				base.BM_RL_NKForeignDestPort = value;
				if (!IsCopying && oldValue != BM_RL_NKForeignDestPort)
				{
					BM_ForeignDestPortKCode = USScheduleResolver.GetScheduleCode(Schedule.K, BM_RL_NKForeignDestPort, USLocoMapSystemUsageList.Codes.SCK, Factory).Left(BM_ForeignDestPortKCodeInfo.MaxLength);
				}
			}
		}

		#endregion

		#region BM_OnwardCarrier

		[List(nameof(Lookups) + "." + nameof(InBondLookups.SCACCarrierCodes))]
		public override ZString BM_OnwardCarrier
		{
			get { return base.BM_OnwardCarrier; }
			set { base.BM_OnwardCarrier = value; }
		}

		#endregion

		#region InBondNumber

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString InBondNumber
		{
			get
			{
				var entryNumber = InBondNumberObj;
				return entryNumber == null ? ZString.Empty : entryNumber.CE_EntryNum;
			}
			set
			{
				var oldValue = InBondNumber;
				if (oldValue != value)
				{
					if (value.IsEmpty)
					{
						ThrowAwayInBondNumber();
					}
					else
					{
						CreateInBondNumberObjIfNeeded();
						InBondNumberObj.CE_EntryNum = value;
					}
				}
				InBondNumberInfo.RefreshBinding(oldValue);
			}
		}

		void ThrowAwayInBondNumber()
		{
			if (InBondNumberObj != null)
			{
				UnRegisterEditableChildObject(InBondNumberObj);
				InBondNumberObj.Delete();
			}
		}

		void CreateInBondNumberObjIfNeeded()
		{
			if (InBondNumberObj == null)
			{
				CusEntryNumber.LoadOrCreate(this, CusEntryNumber.EntryType.InBond, CountryCode);
			}
		}

		public ZPropertyInfo InBondNumberInfo
		{
			get { return GetZPropertyInfo(Schema.InBondNumber); }
		}

		protected CusEntryNumber InBondNumberObj
		{
			get
			{
				if (inBondNumberObjCache == null)
				{
					inBondNumberObjCache = new CachedProperty<CusEntryNumber>(
						Factory,
						() =>
						{
							var result = CusEntryNumber.Load(this, CusEntryNumber.EntryType.InBond, CountryCode);
							RegisterEditableChildObject(result);
							return result;
						});
				}
				return inBondNumberObjCache.Value;
			}
		}

		CachedProperty<CusEntryNumber> inBondNumberObjCache;

		#endregion

		#region CountryCode

		public ZString CountryCode
		{
			get
			{
				var trip = Trip;
				var branch = trip == null ? null : trip.Branch;
				var company = branch == null ? null : branch.Company;
				return company == null ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : company.GC_RN_NKCountryCode;
			}
		}

		#endregion

		public bool IsExport
		{
			get
			{
				return BM_InBondEntryType == InbondTypes.Codes.ImmediateExportation
					   || BM_InBondEntryType == InbondTypes.Codes.TransportationAndExportation;
			}
		}

		#endregion

		#region Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!IsExport)
			{
				BM_RL_NKForeignDestPortInfo.ClearValue();
				BM_ForeignDestPortKCodeInfo.ClearValue();
				BM_ExportDateInfo.ClearValue();
				BM_PedimentoNumberInfo.ClearValue();
			}
		}

		#endregion

		#region Validation

		public new InBondValidation Validation
		{
			get { return (InBondValidation)base.Validation; }
		}

		protected override CusInBondMoveHeaderValidation GetNewValidation()
		{
			return new InBondValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			if (Shipment.B0_ShipmentType == ShipmentTypes.Codes.Inbond)
			{
				base.RunPreSaveValidationCore();
			}
		}

		#endregion

		#region Lookups

		public new InBondLookups Lookups
		{
			get { return (InBondLookups)base.Lookups; }
		}

		protected override CusInBondMoveHeaderLookups GetNewLookups()
		{
			return new InBondLookups(this);
		}

		#endregion

		#endregion
	}
}

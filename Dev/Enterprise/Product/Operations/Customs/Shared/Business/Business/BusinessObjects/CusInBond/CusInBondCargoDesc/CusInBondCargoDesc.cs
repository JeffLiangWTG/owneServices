using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public abstract class CusInBondCargoDesc : AutoCusInBondCargoDesc
		, ISynchroniserReadOnlyMembersProvider
		, Integration.Customs.ICusInBondCargoDesc
		, ITariffFormatProvider
		, ICusInBondFeeTypeSupporter
	{
		protected CusInBondCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondCargoDesc.Schema
		{
			public const string BY_FormattedHarmonisedTariff = nameof(CusInBondCargoDesc.BY_FormattedHarmonisedTariff);
			public const int BY_FormattedHarmonisedTariffMaxLength = 12;
			public new const int BY_HarmonisedTariffMaxLength = 10;
		}

		public static readonly TypeDecider TypeDecider = new CusInBondCargoDescTypeDecider();

		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		[BusinessObjectTestExclude]
		[MaxLength(Schema.BY_FormattedHarmonisedTariffMaxLength)]
		public virtual ZString BY_FormattedHarmonisedTariff
		{
			get { return TariffFormatter.DisplayFormat(BY_HarmonisedTariff); }
			set
			{
				var oldValue = BY_FormattedHarmonisedTariff;
				BY_HarmonisedTariff = TariffFormatter.Format(value);
				BY_FormattedHarmonisedTariffInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BY_FormattedHarmonisedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.BY_FormattedHarmonisedTariff, x => BY_HarmonisedTariffInfo); }
		}

		protected TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = GetNewTariffFormatter()); }
		}
		TariffFormatter tariffFormatter;

		protected virtual TariffFormatter GetNewTariffFormatter() => new TariffFormatter();

		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.TransportChargesModeOfPaymentList))]
		public override ZString BY_TransportChargesMethodOfPayment
		{
			get => base.BY_TransportChargesMethodOfPayment;
			set => base.BY_TransportChargesMethodOfPayment = value;
		}

		#region ITariffFormatProvider Members

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		#endregion

		public Type FeeType
		{
			get { return FeeTypeCore; }
		}

		protected abstract Type FeeTypeCore { get; }

		public BaseCusInBondHeader Header
		{
			get
			{
				switch (BY_ParentTableCode)
				{
					case CusInBondMoveHeaderSchema.Constants.Prefix:
						return MoveHeader?.Header;
					case CusInBondMoveDetailSchema.Constants.Prefix:
						return MoveDetail?.MoveHeader?.Header;
					case CusInBondBillSchema.Constants.Prefix:
						return Bill?.Header;
					case CusInBondContainerSchema.Constants.Prefix:
						return Container?.Header;
					case CusInBondCargoDescSchema.Constants.Prefix:
						return ParentCargoDesc?.Header;
					default:
						return null;
				}
			}
		}

		BaseCusInBondMoveHeader MoveHeader => (moveHeader ?? (moveHeader = new CachedRelatedBusinessObject<BaseCusInBondMoveHeader>((ZPropertyInfoGuid)BY_ParentIDInfo, () => Factory.Load<BaseCusInBondMoveHeader>(BY_ParentID)))).Value;
		CachedRelatedBusinessObject<BaseCusInBondMoveHeader> moveHeader;

		CusInBondMoveDetail MoveDetail => (moveDetail ?? (moveDetail = new CachedRelatedBusinessObject<CusInBondMoveDetail>((ZPropertyInfoGuid)BY_ParentIDInfo, () => Factory.Load<CusInBondMoveDetail>(BY_ParentID)))).Value;
		CachedRelatedBusinessObject<CusInBondMoveDetail> moveDetail;

		CusInBondBill Bill => (bill ?? (bill = new CachedRelatedBusinessObject<CusInBondBill>((ZPropertyInfoGuid)BY_ParentIDInfo, () => Factory.Load<CusInBondBill>(BY_ParentID)))).Value;
		CachedRelatedBusinessObject<CusInBondBill> bill;

		CusInBondContainer Container => (container ?? (container = new CachedRelatedBusinessObject<CusInBondContainer>((ZPropertyInfoGuid)BY_ParentIDInfo, () => Factory.Load<CusInBondContainer>(BY_ParentID)))).Value;
		CachedRelatedBusinessObject<CusInBondContainer> container;

		CusInBondCargoDesc ParentCargoDesc => (parentCargoDesc ?? (parentCargoDesc = new CachedRelatedBusinessObject<CusInBondCargoDesc>((ZPropertyInfoGuid)BY_ParentIDInfo, () => Factory.Load<CusInBondCargoDesc>(BY_ParentID)))).Value;
		CachedRelatedBusinessObject<CusInBondCargoDesc> parentCargoDesc;
	}
}

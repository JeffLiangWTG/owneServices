using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMenuItemMessageSendingObject : AutoInBondMenuItemMessageSendingObject
	{
		#region Schema

		public new class Schema : AutoInBondMenuItemMessageSendingObject.Schema
		{
			public const string ImporterName = "ImporterName";
			public const string InBondCarrierName = "InBondCarrierName";
			public const string USDestinationPortName = "USDestinationPortName";
			public const string ForeignDestinationPortName = "ForeignDestinationPortName";
			public const string QPMessageStatus = "QPMessageStatus";
			public const string WPMessageStatus = "WPMessageStatus";
			public const string MessageStatusDescription = "MessageStatusDescription";
		}

		#endregion

		public InBondMenuItemMessageSendingObject(InBondMenuItemMessageData inBondMenuItemMessageData)
			: base(inBondMenuItemMessageData.Factory)
		{
			Argument.NotNull(inBondMenuItemMessageData, nameof(inBondMenuItemMessageData));
			Parent = inBondMenuItemMessageData;
		}
		public InBondMenuItemMessageData Parent;

		public InBondMenuItemMessageSendingObject(InBondMenuItemMessageData inBondMenuItemMessageData, CusInBondMoveHeader movementHeader)
			: this(inBondMenuItemMessageData)
		{
			this.MovementHeader = movementHeader;
			GetPropertyValueFromPersistentBO();
		}

		public CusInBondMoveHeader MovementHeader;
		public CusInBondHeader Header => MovementHeader?.Header;

		#region Properties

		[ReadOnlyMember(nameof(InBondNumber_ReadOnly))]
		public override ZString InBondNumber
		{
			get { return base.InBondNumber; }
			set
			{
				var oldValue = base.InBondNumber;
				base.InBondNumber = value;
				if (oldValue != value && MovementHeader == null)
				{
					var inBondNumber = CusEntryNumber.LoadMostRecentByCreateTime(Factory, CusEntryHeaderMessageTypeList.Codes.InBond, value, Core.Constants.CountryCodes.UnitedStates);
					if (inBondNumber != null && inBondNumber.Parent is CusInBondMoveHeader moveHeader)
					{
						MovementHeader = moveHeader;
					}

					GetPropertyValueFromPersistentBO();
				}

				Validation.ValidateInBondNumber();
			}
		}

		public ZString JobReference => Header?.BH_JobReference ?? ZString.Empty;

		[ReadOnlyMember(nameof(OtherProperties_ReadOnly))]
		[RelatedBusinessObject(nameof(Importer))]
		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.ImporterList))]
		public override ZGuid ImporterOrgPK
		{
			get => base.ImporterOrgPK;
			set => base.ImporterOrgPK = value;
		}

		public OrgHeader Importer => Factory.Load<OrgHeader>(ImporterOrgPK);

		public ZString ImporterName => Importer?.OH_FullName ?? ZString.Empty;

		[ReadOnlyMember(nameof(OtherProperties_ReadOnly))]
		[RelatedBusinessObject(nameof(InBondCarrier))]
		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.ShippingProviders))]
		public override ZGuid InBondCarrierOrgPK
		{
			get => base.InBondCarrierOrgPK;
			set
			{
				var oldValue = InBondCarrierOrgPK;
				base.InBondCarrierOrgPK = value;
				if (oldValue != InBondCarrierOrgPK)
				{
					CusInBondMoveHeader.DefaultInBondCarrierDetails(InBondCarrier, InBondCarrierCodeSCACInfo, null, IsCopying);
				}
			}
		}

		public OrgHeader InBondCarrier => Factory.Load<OrgHeader>(InBondCarrierOrgPK);

		public ZString InBondCarrierName => InBondCarrier?.OH_FullName ?? ZString.Empty;

		[ReadOnlyMember(nameof(OtherProperties_ReadOnly))]
		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.CarrierCollection))]
		public override ZString InBondCarrierCodeSCAC
		{
			get => base.InBondCarrierCodeSCAC;
			set => base.InBondCarrierCodeSCAC = value;
		}

		[ReadOnlyMember(nameof(OtherProperties_ReadOnly))]
		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.RegionDistrictPorts))]
		[RelatedBusinessObject(nameof(USDestinationPortCodeBO))]
		public override ZString USDestinationPortCode
		{
			get { return base.USDestinationPortCode.IsEmpty ? Parent.USDestinationPortCode : base.USDestinationPortCode; }
			set => base.USDestinationPortCode = value;
		}

		public ZZRefCusCodeListCombined USDestinationPortCodeBO
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, USDestinationPortCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZString USDestinationPortName => USDestinationPortCodeBO?.ZZD_Description ?? ZString.Empty;

		[ReadOnlyMember(nameof(OtherProperties_ReadOnly))]
		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.ForeignPorts))]
		[RelatedBusinessObject(nameof(ForeignDestPortKCodeBO))]
		public override ZString ForeignDestinationPortCode
		{
			get => base.ForeignDestinationPortCode;
			set => base.ForeignDestinationPortCode = value;
		}

		public ZZRefCusCodeListCombined ForeignDestPortKCodeBO
		{
			get
			{
				return Factory.GetCachedValue("InBondMenuItemMessageData|" + ForeignDestinationPortCode, () =>
				{
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, ForeignDestinationPortCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today,
						attributeFilters:
						new[]
						{
							new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond })
						});
				});
			}
		}

		public ZString ForeignDestinationPortName => ForeignDestPortKCodeBO?.ZZD_Description ?? ZString.Empty;

		[ReadOnlyMember(nameof(EntryType_ReadOnly))]
		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.EntryTypeList))]
		public override ZString EntryType
		{
			get => base.EntryType;
			set => base.EntryType = value;
		}

		[ReadOnlyMember(nameof(PedimentoNumber_ReadOnly))]
		public override ZString PedimentoNumber { get => base.PedimentoNumber; set => base.PedimentoNumber = value; }

		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.MessageStatusList))]
		[ReadOnly(true)]
		public ZString QPMessageStatus
		{
			get => MovementHeader?.BM_CustomsStatus ?? ZString.Empty;
		}

		public ZPropertyInfo QPMessageStatusInfo => GetZPropertyInfo(Schema.QPMessageStatus);

		public ZString QPMessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(QPMessageStatus) ?? ZString.Empty; }
		}

		[List(nameof(Lookups) + "+" + nameof(InBondMenuItemMessageSendingObjectLookups.MessageStatusList))]
		[ReadOnly(true)]
		public ZString WPMessageStatus
		{
			get => MovementHeader?.BM_MessageStatus ?? ZString.Empty;
		}

		public ZPropertyInfo WPMessageStatusInfo => GetZPropertyInfo(Schema.WPMessageStatus);

		public ZString WPMessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(WPMessageStatus) ?? ZString.Empty; }
		}

		[ResourceStringData("20378282-C14E-4131-AA61-2AC1DA397167", Caption = "FIRMS Code")]
		public ZString ArrivalFirmsCode => MovementHeader?.BM_FIRMS ?? ZString.Empty;

		public ZPropertyInfo ArrivalFirmsCodeInfo => GetZPropertyInfo(nameof(ArrivalFirmsCode));

		public bool InBondNumber_ReadOnly { get; private set; }

		bool EntryType_ReadOnly => InBondNumber.IsEmpty;

		bool PedimentoNumber_ReadOnly => InBondNumber.IsEmpty || !Parent.IsPedimento;

		bool OtherProperties_ReadOnly => MovementHeader != null || InBondNumber.IsEmpty;

		public InBondMenuItemMessageSendingObjectLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new InBondMenuItemMessageSendingObjectLookups(this);
				}
				return fLookups;
			}
		}
		InBondMenuItemMessageSendingObjectLookups fLookups;

		#endregion

		void GetPropertyValueFromPersistentBO()
		{
			if (MovementHeader is CusInBondMoveHeader movementHeader)
			{
				if (!movementHeader.InBondNumber.IsEmpty)
				{
					InBondNumber_ReadOnly = true;
				}
				if (InBondNumber.IsEmpty)
				{
					InBondNumber = movementHeader.InBondNumber.SubstringSafe(0, Schema.InBondNumberMaxLength);
				}
				ImporterOrgPK = movementHeader.Header.BH_OA_Importer_ZAddress.OrgPK;
				InBondCarrierOrgPK = movementHeader.InBondCarrierOrgPK;
				InBondCarrierCodeSCAC = movementHeader.BM_InBondCarrierSCAC;
				USDestinationPortCode = movementHeader.BM_DestinationPortCode.SubstringSafe(0, Schema.USDestinationPortCodeMaxLength);
				ForeignDestinationPortCode = movementHeader.BM_ForeignDestPortKCode.SubstringSafe(0, Schema.ForeignDestinationPortCodeMaxLength);
				if (!movementHeader.BM_InBondEntryType.IsEmpty)
				{
					EntryType = movementHeader.BM_InBondEntryType.SubstringSafe(0, EntryTypeInfo.MaxLength);
				}
				if (Parent.IsPedimento)
				{
					var bill = movementHeader.MovementDetails.Select(x => x.Bill).OrderBy(x => x.B0_MasterBillNumber).FirstOrDefault();
					PedimentoNumber = bill?.AdditionalReferences.Where(x => x.BR_Qualifier == ReferenceQualifierList.Codes.FEN).OrderBy(x => x.BR_ReferenceNum).FirstOrDefault()?.BR_ReferenceNum ?? ZString.Empty;
				}
			}
		}
	}
}

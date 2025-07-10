using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[CodeProperty("MovementUniqueCode"), DescriptionProperty("MovementDescription")]
	public abstract class CusInBondMoveHeader : Customs.Business.CusInBondMoveHeader
	{
		protected CusInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.CusInBondMoveHeader.Schema
		{
			public const string BM_CustomsStatusDescription = "BM_CustomsStatusDescription";
			public const string BM_MessageStatusDescription = "BM_MessageStatusDescription";
			public const string InBondNumber = "InBondNumber";
			public const string MovementUniqueCode = "MovementUniqueCode";
			public const string InBondCarrierOrgPK = "InBondCarrierOrgPK";
			public const string TOLCarrierOrgPK = "TOLCarrierOrgPK";
			public const string WarehouseAddressOrgPK = "WarehouseAddressOrgPK";
			public new const int BM_DestinationPortCodeMaxLength = 4;
		}

		#region Constants

		public static class Constants
		{
			public static string InBondNumberAlreadyAllocated(string number)
			{
				return string.Format("An In-Bond Number ({0}) has already been allocated to this movement.\r\nOnce an In-Bond Number is used, a new In-Bond Number cannot be allocated.", number);
			}
		}

		#endregion

		#region New Properties

		public bool IsBTAFDA
		{
			get { return BM_BTAIndicator == YesNoDefaultList.Codes.Yes; }
		}

		public virtual ZString MovementDescription
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				if (!BM_InBondEntryType.IsEmpty)
				{
					builder.Append("Type:" + BM_InBondEntryType);
				}
				if (!BM_InBondCarrierSCAC.IsEmpty)
				{
					builder.Append("SCAC:" + BM_InBondCarrierSCAC);
				}
				if (!BM_DestinationPortCode.IsEmpty)
				{
					builder.Append("US Dest.:" + BM_DestinationPortCode);
				}
				if (!BM_ForeignDestPortKCode.IsEmpty)
				{
					builder.Append("Foreign Dest.:" + BM_ForeignDestPortKCode);
				}
				if (!BM_InBondCarrierID.IsEmpty)
				{
					builder.Append("Carrier ID:" + BM_InBondCarrierID);
				}
				if (!BM_BTAIndicator.IsEmpty)
				{
					builder.Append("BTA:" + BM_BTAIndicator);
				}
				if (!BM_MonetaryValue.IsEmpty)
				{
					builder.Append("Value:" + BM_MonetaryValue);
				}
				if (builder.IsEmpty)
				{
					builder.Append(InBondNumber);
				}
				return builder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		#region InBondNumber

		[ResourceStringData("Enterprise.Customs.US.Business.CusInBondMoveHeader|InBondNumber", Caption = "In-Bond Number")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public virtual ZString InBondNumber
		{
			get
			{
				CusEntryNumber numberObj = InBondNumberObj;
				return numberObj == null ? ZString.Empty : numberObj.CE_EntryNum;
			}
			set
			{
				ZString oldValue = InBondNumber;
				if (oldValue != value)
				{
					if (value.IsEmpty)
					{
						ThrowAwayInBondNumber();
					}
					else
					{
						inBondNumberObj = CreateInBondNumberObjIfNeeded();
						inBondNumberObj.CE_EntryNum = value;
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateInBondNumber();
				}
				InBondNumberInfo.RefreshBinding(oldValue);
			}
		}

		void ThrowAwayInBondNumber()
		{
			if (InBondNumberObj != null)
			{
				InBondNumberObj.Delete();
			}
		}

		public ZPropertyInfo InBondNumberInfo
		{
			get { return GetZPropertyInfo(Schema.InBondNumber); }
		}

		public CusEntryNumber InBondNumberObj
		{
			get
			{
				if (inBondNumberObj == null || inBondNumberObj.IsDeleted)
				{
					inBondNumberObj = CusEntryNumber.Load(this, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates, reLoadExistingRows: IsInDatabase);
				}
				return inBondNumberObj;
			}
		}
		CusEntryNumber inBondNumberObj;

		#endregion

		#region InBondCarrierOrgPK

		[ResourceStringData("Enterprise.Customs.US.Business.CusInBondMoveHeader|InBondCarrierOrgPK", Caption = "In-Bond Carrier")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.ShippingProviders))]
		public ZGuid InBondCarrierOrgPK
		{
			get { return BM_OA_InBondCarrier_ZAddress.OrgPK; }
			set { BM_OA_InBondCarrier_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo InBondCarrierOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.InBondCarrierOrgPK, x => BM_OA_InBondCarrier_ZAddress.OrgPKInfo); }
		}

		public OrgHeader InBondCarrierOrg
		{
			get { return (OrgHeader)BM_OA_InBondCarrier_ZAddress.OrgHeader; }
		}

		#endregion

		#region TOLCarrierOrgPK

		[ResourceStringData("Enterprise.Customs.US.Business.CusInBondMoveHeader|TOLCarrierOrgPK", Caption = "Transfer Of Liability Carrier", MediumCaption = "TOL Carrier", ShortCaption = "TOL Carr.")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.ShippingProviders))]
		public ZGuid TOLCarrierOrgPK
		{
			get { return BM_OA_TOLCarrier_ZAddress.OrgPK; }
			set
			{
				BM_OA_TOLCarrier_ZAddress.OrgPK = value;
				if (BM_OA_TOLCarrier_ZAddress.OrgPK.IsValid)
				{
					var carrierAddress = Factory.Load<OrgAddress>(BM_OA_TOLCarrier_ZAddress.AddressFK);
					if (carrierAddress != null && carrierAddress.OA_RL_NKRelatedPortCode.StartsWith(Core.Constants.CountryCodes.UnitedStates))
					{
						BM_TOLCityName = carrierAddress.OA_City.Left(CusInBondMoveHeader.Schema.BM_TOLCityNameMaxLength);
						BM_TOLStateCode = carrierAddress.OA_State.SubstringSafe(0, 2);
					}
				}

				BM_OA_TOLCarrierInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TOLCarrierOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.TOLCarrierOrgPK, x => BM_OA_TOLCarrier_ZAddress.OrgPKInfo); }
		}

		public OrgHeader TOLCarrierOrg
		{
			get { return (OrgHeader)BM_OA_TOLCarrier_ZAddress.OrgHeader; }
		}

		#endregion

		#region WarehouseAddressOrgPK

		[ResourceStringData("Enterprise.Customs.US.Business.CusInBondMoveHeader|WarehouseAddressOrgPK", Caption = "Bonded Warehouse/FTZ")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.BondedWarehouseCollection))]
		public ZGuid WarehouseAddressOrgPK
		{
			get { return BM_OA_WarehouseAddress_ZAddress.OrgPK; }
			set
			{
				BM_OA_WarehouseAddress_ZAddress.OrgPK = value;
				BM_OA_WarehouseAddressInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WarehouseAddressOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.WarehouseAddressOrgPK, x => BM_OA_WarehouseAddress_ZAddress.OrgPKInfo); }
		}

		public OrgHeader WarehouseAddressOrg
		{
			get { return (OrgHeader)BM_OA_WarehouseAddress_ZAddress.OrgHeader; }
		}

		[List(nameof(BM_OA_WarehouseAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid BM_OA_WarehouseAddress
		{
			get { return base.BM_OA_WarehouseAddress; }
			set { base.BM_OA_WarehouseAddress = value; }
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.US.Business.CusInBondMoveHeader|MovementUniqueCode", Caption = "In-Bond Number")]
		public ZString MovementUniqueCode
		{
			get
			{
				if (movementUniqueCodeCached == null)
				{
					movementUniqueCodeCached = new CachedProperty<ZString>(Factory, delegate
					{
						ZString result = InBondNumber;
						if (result.IsEmpty)
						{
							result = ResString.GetMultilingualString("CusInBondMoveHeader|370C00C9-3727-4fa6-91EF-628DE560B9B4", "NOT YET SPECIFIED");
							var header = Header;
							if (header != null)
							{
								int index = 0;
								foreach (CusInBondMoveHeader moveHeader in GetMovementHeaders(header))
								{
									if (moveHeader.InBondNumber.IsEmpty)
									{
										index++;
									}

									if (moveHeader == this)
									{
										result += " " + index.ToString();
										break;
									}
								}
							}
						}
						return result;
					});
				}
				return movementUniqueCodeCached.Value;
			}
		}
		CachedProperty<ZString> movementUniqueCodeCached;

		public ZPropertyInfo MovementUniqueCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.MovementUniqueCode, x => InBondNumberInfo); }
		}

		#endregion

		#region Override Properties

		#region BM_OA_TOLCarrier

		[List(nameof(BM_OA_InBondCarrier_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid BM_OA_InBondCarrier
		{
			get { return base.BM_OA_InBondCarrier; }
			set
			{
				var oldOrg = InBondCarrier?.OA_OH;
				base.BM_OA_InBondCarrier = value;
				if (oldOrg == null || (InBondCarrier is OrgAddress inBondCarrier && oldOrg != inBondCarrier.OA_OH))
				{
					DefaultInBondCarrierDetails(InBondCarrierOrg, BM_InBondCarrierSCACInfo, BM_InBondCarrierIDInfo, IsCopying);
				}
			}
		}

		#endregion

		#region BM_OA_TOLCarrier

		[List(nameof(BM_OA_TOLCarrier_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid BM_OA_TOLCarrier
		{
			get { return base.BM_OA_TOLCarrier; }
			set { base.BM_OA_TOLCarrier = value; }
		}

		#endregion

		#region BM_InBondEntryType

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.EntryTypeList))]
		public override ZString BM_InBondEntryType
		{
			get { return base.BM_InBondEntryType; }
			set
			{
				ZString oldValue = BM_InBondEntryType;
				base.BM_InBondEntryType = value;
				if (!IsCopying && oldValue != BM_InBondEntryType && BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport)
				{
					BM_BTAIndicator = YesNoDefaultList.Codes.No;
				}
			}
		}

		#endregion

		#region BM_ExportTransportMode

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.TransportModeCodes))]
		public override ZString BM_ExportTransportMode
		{
			get { return base.BM_ExportTransportMode; }
			set { base.BM_ExportTransportMode = value; }
		}

		#endregion

		#region BM_InBondCarrierSCAC

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.CarrierCollection))]
		[RelatedBusinessObject("InBondCarrierSCAC")]
		public override ZString BM_InBondCarrierSCAC
		{
			get { return base.BM_InBondCarrierSCAC; }
			set { base.BM_InBondCarrierSCAC = value; }
		}

		public USCarrierCombined InBondCarrierSCAC
		{
			get { return Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, BM_InBondCarrierSCAC)); }
		}

		#endregion

		#region BM_DestinationPortCode
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.RegionDistrictPorts))]
		[MaxLength(Schema.BM_DestinationPortCodeMaxLength)]
		[RelatedBusinessObject("DestinationPortCode")]
		public override ZString BM_DestinationPortCode
		{
			get { return base.BM_DestinationPortCode; }
			set { base.BM_DestinationPortCode = value; }
		}

		public ZZRefCusCodeListCombined DestinationPortCode
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BM_DestinationPortCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		#endregion

		#region BM_ForeignDestPortKCode

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.ForeignPorts))]
		[RelatedBusinessObject("ForeignDestPortKCode")]
		public override ZString BM_ForeignDestPortKCode
		{
			get { return base.BM_ForeignDestPortKCode; }
			set { base.BM_ForeignDestPortKCode = value; }
		}

		public ZZRefCusCodeListCombined ForeignDestPortKCode
		{
			get
			{
				return Factory.GetCachedValue("CusInBondMoveHeader|" + BM_ForeignDestPortKCode, () =>
				{
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, BM_ForeignDestPortKCode,
						Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today,
						attributeFilters:
						new[]
						{
							new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, JoinCondition.And,
							new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond })
						});
				});
			}
		}

		#endregion

		#region BM_BTAIndicator

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.YesNoList))]
		[ReadOnlyMember(nameof(BM_BTAIndicator_ReadOnly))]
		public override ZString BM_BTAIndicator
		{
			get { return base.BM_BTAIndicator; }
			set { base.BM_BTAIndicator = value; }
		}

		protected virtual bool BM_BTAIndicator_ReadOnly
		{
			get { return BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport; }
		}

		#endregion

		#region BM_TOLCarrierCode

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.CarrierCollection))]
		[RelatedBusinessObject("TOLCarrierCODE")]
		public override ZString BM_TOLCarrierCode
		{
			get { return base.BM_TOLCarrierCode; }
			set { base.BM_TOLCarrierCode = value; }
		}

		public USCarrierCombined TOLCarrierCODE
		{
			get { return Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, BM_TOLCarrierCode)); }
		}

		#endregion

		#region BM_TOLStateCode

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.USStatesList))]
		public override ZString BM_TOLStateCode
		{
			get { return base.BM_TOLStateCode; }
			set { base.BM_TOLStateCode = value; }
		}

		#endregion

		#region BM_ExportLadenOn

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.ConveyanceList))]
		[RelatedBusinessObject("ExportLadenOn")]
		public override ZString BM_ExportLadenOn
		{
			get { return base.BM_ExportLadenOn; }
			set { base.BM_ExportLadenOn = value; }
		}

		public RefVessel ExportLadenOn
		{
			get { return Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, BM_ExportLadenOn); }
		}

		#endregion

		#region CustomsStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.MessageStatusList))]
		public override ZString BM_CustomsStatus
		{
			get { return base.BM_CustomsStatus; }
			set { base.BM_CustomsStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.CusInBondMoveHeader|BM_CustomsStatusDescription", Caption = "Customs Status Description", MediumCaption = "Cus. Status Desc.", ShortCaption = "Status Desc.")]
		public virtual ZString BM_CustomsStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(BM_CustomsStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo BM_CustomsStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BM_CustomsStatusDescription); }
		}

		#endregion

		#region BM_MessageStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.MessageStatusList))]
		public override ZString BM_MessageStatus
		{
			get { return base.BM_MessageStatus; }
			set { base.BM_MessageStatus = value; }
		}

		public virtual ZString BM_MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(BM_MessageStatus) ?? ZString.Empty; }
		}

		public ZPropertyInfo BM_MessageStatusDescriptionInfo => GetZPropertyInfo(Schema.BM_MessageStatusDescription);

		#endregion

		public new CusInBondMoveHeaderLookups Lookups
		{
			get { return (CusInBondMoveHeaderLookups)base.Lookups; }
		}

		public new CusInBondMoveHeaderValidation Validation
		{
			get { return (CusInBondMoveHeaderValidation)base.Validation; }
		}

		#endregion

		#region Allocate In-Bond Number

		/// <summary>
		/// this in-bond number has been created to allow allocation of an in-bond number to a movement before it is sent.
		/// This in-bond number is added to the CusEntryNum table, and when in-bond numbers are allocated as part of the saving of MQEDIMessage,
		/// the process first checks to see whether this number exists. If it does, then this will become the in-bond number for the movement.
		/// </summary>
		public string DisallowAllocateInBondNumber
		{
			get { return DoubleCheckThatInBondNumberIsEmpty() ? "" : Constants.InBondNumberAlreadyAllocated(InBondNumber); }
		}

		public void AllocateInBondNumber(string userEnteredInBondNumber)
		{
			if (string.IsNullOrEmpty(userEnteredInBondNumber))
			{
				allocateInBondNumberOnSaving = true;
			}
			else
			{
				InBondNumber = userEnteredInBondNumber;
				InBondNumberObj.CE_EntryIsSystemGenerated = false;
			}
		}
		bool allocateInBondNumberOnSaving;

		public bool LockInBondNumberAllocationMutex()
		{
			return InBondNumberAllocationMutex.IsLocked ? (bool)InBondNumberAllocationMutex.HasLock : InBondNumberAllocationMutex.Lock();
		}

		public void UnLockInBondNumberAllocationMutex()
		{
			if (inBondNumberAllocationMutex != null && inBondNumberAllocationMutex.IsLocked && inBondNumberAllocationMutex.HasLock)
			{
				inBondNumberAllocationMutex.Unlock();
			}
		}

		public bool InBondNumberAllocationMutexHasLock()
		{
			return InBondNumberAllocationMutex.HasLock;
		}

		public bool InBondNumberAllocationMutexIsLocked()
		{
			return InBondNumberAllocationMutex.IsLocked;
		}

		public string GetInBondNumberAllocationMutexLockInfo() => InBondNumberAllocationMutex.GetMutexLockByInfo();

		ZGlobalMutex InBondNumberAllocationMutex
		{
			get
			{
				if (inBondNumberAllocationMutex == null)
				{
					inBondNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "INB" + PK.ToString());
				}
				return inBondNumberAllocationMutex;
			}
		}
		ZGlobalMutex inBondNumberAllocationMutex;

		public static string InBondNumberAllocationMutexLockText(string lockInfo)
		{
			return Res.GetString("USCusInBondMoveHeader|InBondNumberAllocationMutex", "{0} is in the process of allocating InBond Number for this movement.\r\nPlease re-open the job later.", lockInfo);
		}

		#endregion

		[ActionFieldFollow(false)]
		public new ICusInBondMoveDetailCollection MovementDetails
		{
			get { return (ICusInBondMoveDetailCollection)base.MovementDetails; }
		}

		public CusInBondMoveDetail FirstMoveDetail
		{
			get
			{
				if (firstMoveDetailCached == null)
				{
					firstMoveDetailCached = new CachedProperty<CusInBondMoveDetail>(Factory, delegate
					{
						List<CusInBondMoveDetail> details = new List<CusInBondMoveDetail>(new TypedEnumerable<CusInBondMoveDetail>(MovementDetails));
						details.Sort(new CusInBondMoveDetailComparer());
						return details.Count > 0 ? details[0] : null;
					});
				}
				return firstMoveDetailCached.Value;
			}
		}
		CachedProperty<CusInBondMoveDetail> firstMoveDetailCached;

		#region Implementation

		public override void Delete()
		{
			if (!InBondNumberAllocationMutexIsLocked())
			{
				ThrowAwayInBondNumber();
				UnLockInBondNumberAllocationMutex();
				base.Delete();
			}
		}

		public override bool CanDelete => !InBondNumberAllocationMutexIsLocked() && base.CanDelete;

		public override MultilingualString ReasonForNotAbleToDelete => InBondNumberAllocationMutexIsLocked() ? ResString.GetMultilingualString("CA084A23-F625-41A7-94DF-3CB84418DB7B", "{0} is in the process of allocating a new InBond Number for this movement; this movement cannot be deleted.", GetInBondNumberAllocationMutexLockInfo()) : base.ReasonForNotAbleToDelete;

		protected abstract IEnumerable<CusInBondMoveHeader> GetMovementHeaders(Customs.Business.CusInBondHeader header);

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (allocateInBondNumberOnSaving)
			{
				oldInBondNumber = InBondNumber;
				AllocateNextInBondNumber();
			}
		}
		ZString oldInBondNumber;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (allocateInBondNumberOnSaving)
			{
				allocateInBondNumberOnSaving = false;
				if (saveSucceeded)
				{
					InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(HeaderBranch);
				}
				else
				{
					InBondNumber = oldInBondNumber;
				}
				oldInBondNumber = ZString.Empty;
			}
		}

		void AllocateNextInBondNumber()
		{
			ZString inbondNumber;
			if (!Header.IsPostDepartureMessageOnly && InBondNumberGenerator.TryGetNextInBondNumber(HeaderBranch, out inbondNumber))
			{
				InBondNumber = inbondNumber;
				var obj = InBondNumberObj;
				if (obj != null)
				{
					obj.CE_EntryIsSystemGenerated = true;
				}
			}
		}

		CusEntryNumber CreateInBondNumberObjIfNeeded()
		{
			return InBondNumberObj ?? CusEntryNumber.LoadOrCreate(this, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates);
		}

		protected void FillInInBondNumberDetailsIfNeeded()
		{
			if (DoubleCheckThatInBondNumberIsEmpty())
			{
				AllocateNextInBondNumber();
			}
		}

		public void ReloadInBondNumber()
		{
			if (IsInDatabase)
			{
				var currentInBondNumber = inBondNumberObj == null || inBondNumberObj.IsDeleted ? ZString.Empty : inBondNumberObj.CE_EntryNum;
				if (inBondNumberObj == null)
				{
					inBondNumberObj = CusEntryNumber.Load(this, CusEntryHeaderMessageTypeList.Codes.InBond, Core.Constants.CountryCodes.UnitedStates, reLoadExistingRows: true);
				}
				else if (inBondNumberObj.IsInDatabase)
				{
					inBondNumberObj.Reload();
				}

				if (inBondNumberObj != null && !inBondNumberObj.IsDeleted && inBondNumberObj.CE_EntryNum != currentInBondNumber)
				{
					InBondNumberInfo.RefreshBinding(currentInBondNumber);
				}
			}
		}

		protected bool DoubleCheckThatInBondNumberIsEmpty()
		{
			ReloadInBondNumber();
			return InBondNumber.IsEmpty;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		public class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusInBondMoveHeader moveHeader)
				: base(moveHeader)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusInBondMoveDetail), CusInBondMoveDetailSchema.B9_BM, BusinessObject.PK);
			}
		}

		protected override Customs.Business.CusInBondMoveHeaderLookups GetNewLookups()
		{
			return new CusInBondMoveHeaderLookups(this);
		}

		protected override Customs.Business.CusInBondMoveHeaderValidation GetNewValidation()
		{
			return new CusInBondMoveHeaderValidation(this);
		}

		protected override ZAddress GetNewBM_OA_TOLCarrier_ZAddress()
		{
			ZAddress result = base.GetNewBM_OA_TOLCarrier_ZAddress();
			result.OnOrgChanged += new EventHandler(BM_OA_TOLCarrier_ZAddress_OnOrgChanged);
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetCBPAddress);
			result.AddresssListOverride = AddressListOverrider.ShowCBPNoInAddressList;
			result.IsOrgVisible = true;
			return result;
		}

		void BM_OA_TOLCarrier_ZAddress_OnOrgChanged(object sender, EventArgs e)
		{
			DefaultTOLCarrierDetails();
		}

		protected override ZAddress GetNewBM_OA_InBondCarrier_ZAddress()
		{
			ZAddress result = base.GetNewBM_OA_InBondCarrier_ZAddress();
			result.GetDefaultAddress = GetDefaultCarrierAddress;
			return result;
		}

		ZGuid GetDefaultCarrierAddress(IOrgHeader orgHeader)
		{
			OrgHeader header = orgHeader as OrgHeader;
			return header == null ? ZGuid.Empty : header.MainAddress.PK;
		}

		void DefaultTOLCarrierDetails()
		{
			if (!IsCopying)
			{
				OrgHeader carrier = TOLCarrierOrg;
				if (carrier != null)
				{
					BM_TOLCarrierCode = carrier.USLocalCustomsCarrierCode();
					BM_TOLCarrierID = carrier.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber);
				}
			}
		}

		public static void DefaultInBondCarrierDetails(OrgHeader carrier, ZPropertyInfo carrierSCACPropertyInfo, ZPropertyInfo carrierIDPropertyInfo, ZBool isCopying)
		{
			if (!isCopying)
			{
				if (carrier != null)
				{
					if (carrierSCACPropertyInfo != null)
					{
						carrierSCACPropertyInfo.Value = carrier.USLocalCustomsCarrierCode(true);
					}

					if (carrierIDPropertyInfo != null)
					{
						var customsRegNo = carrier.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.SocialSecurityNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber);
						if (IsValidCodeToPopulateCarrierID(customsRegNo))
						{
							carrierIDPropertyInfo.Value = customsRegNo;
						}
					}
				}
			}
		}

		public static bool IsValidCodeToPopulateCarrierID(ZString customsRegNo)
		{
			return EmployerIdentificationNumberValidator.IsValidEIN(customsRegNo) ||
					CBPAssignedNumberValidator.IsValidCBPAssignedNumber(customsRegNo) ||
					SocialSecurityNumberValidator.IsValidSSN(customsRegNo);
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (humanReadableNameCoreCached == null)
				{
					humanReadableNameCoreCached = new CachedProperty<ZString>(Factory, delegate
					{
						return InBondNumber;
					});
				}
				return humanReadableNameCoreCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableNameCoreCached;

		#endregion
	}

	public class CusInBondMoveHeaderComparer : Comparer<CusInBondMoveHeader>
	{
		public override int Compare(CusInBondMoveHeader x, CusInBondMoveHeader y)
		{
			int result = x.InBondNumber.CompareTo(y.InBondNumber);
			if (result == 0)
			{
				result = x.BM_SystemCreateTimeUtc.CompareTo(y.BM_SystemCreateTimeUtc);
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == GetType();
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}
	}
}

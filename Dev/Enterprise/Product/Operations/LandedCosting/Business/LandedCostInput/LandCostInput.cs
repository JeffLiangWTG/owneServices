using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.LandedCosting.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(LandedCostHeader), "CostInputs")]
	public class LandCostInput : AutoLandCostInput, Integration.LandedCosting.ILandCostInput, IChargeWithChargeCode, IClusterKeyWorker
	{
		public new class Schema : AutoLandCostInput.Schema
		{
			public const string LinkedObjectUniqueCode = "LinkedObjectUniqueCode";
			public const string LCGroupString = "LCGroupString";
		}

		public LandCostInput(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void DefaultFromHost(ILandedCostChargeHolder chargeHolder, IDefaultLandedCostInput chargeToImport)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				DefaultDistributeLevel(chargeHolder as ILandedCostDistributeTo);

				LI_AC_ChargeCode = chargeToImport.FKToChargeCode;
				LI_ChargeDescription = chargeToImport.ChargeDescription.SubstringSafe(0, LandCostInput.Schema.LI_ChargeDescriptionMaxLength);
				LI_CostAmount = chargeToImport.AmountToDistribute.Amount;
				LI_RX_NKCostCurrency = chargeToImport.AmountToDistribute.Currency.Code;
				LI_IsUserEntered = false;
				if (chargeToImport.ExchangeRate != 0)
				{
					LI_ServiceExRate = chargeToImport.ExchangeRate;
				}
			}
		}

		internal void DefaultDistributeLevel(ILandedCostDistributeTo preferableDistributeLevel)
		{
			ILandedCostDistributeTo distributeLevel = preferableDistributeLevel;

			if (distributeLevel == null && Header.Parent != null)
			{
				IEnumerable<ILandedCostDistributeTo> allDistributeLevels = Header.Parent.CandidatesToDistributeCostTo;
				distributeLevel = allDistributeLevels.FirstOrDefault();
			}

			if (distributeLevel != null)
			{
				LI_ParentID = distributeLevel.PK;
				LI_ParentTableCode = distributeLevel.TableCode;
			}
		}

		#region Calculated Properties

		public ZDecimal Group1AmountInLocalCurrency
		{
			get { return GetLCGroupAmount(1); }
		}

		public ZDecimal Group2AmountInLocalCurrency
		{
			get { return GetLCGroupAmount(2); }
		}

		public ZDecimal Group3AmountInLocalCurrency
		{
			get { return GetLCGroupAmount(3); }
		}

		public ZDecimal Group4AmountInLocalCurrency
		{
			get { return GetLCGroupAmount(4); }
		}

		public ZDecimal Group5AmountInLocalCurrency
		{
			get { return GetLCGroupAmount(5); }
		}

		public ZDecimal Group6AmountInLocalCurrency
		{
			get { return GetLCGroupAmount(6); }
		}

		public ZDecimal GroupMiscAmountInLocalCurrency
		{
			get { return GetLCGroupAmount(7); }
		}

		public ZString DistributionByDescription
		{
			get
			{
				ILandedCostHeader lCHost = Header.Parent;
				ZString effectiveCode = LI_DistributeCostBy;
				if (lCHost != null && LI_DistributeCostBy == CostDistributionMechanismList.Codes.Actual)
				{
					effectiveCode = lCHost.IsAir ? CostDistributionMechanismList.Codes.ActualWeight : CostDistributionMechanismList.Codes.ActualVolume;
				}
				return Lookups.DistributeCostBy.GetDescriptionFromCode(effectiveCode);
			}
		}

		ZDecimal GetLCGroupAmount(ZByte groupID)
		{
			ZDecimal result = 0m;

			if (LI_LandedCostGroup == groupID || groupID > 6 && LI_LandedCostGroup > 6)
			{
				result = CostAmountInLocalCurrency;
			}
			return result;
		}
		#endregion

		#region New Bindable Properties

		public Money CostAmountMoney
		{
			get
			{
				Money result = Money.Empty;
				if (this.CostCurrency != null)
				{
					result = new Money(LI_CostAmount, CostCurrency);
				}
				return result;
			}
		}

		public ZDecimal CostAmountInLocalCurrency
		{
			get
			{
				if (cachedCostAmountInLocalCurrency == null)
				{
					cachedCostAmountInLocalCurrency = new CachedProperty<ZDecimal>(Factory, GetCostAmountInLocalCurrency);
				}
				return cachedCostAmountInLocalCurrency.Value;
			}
		}
		CachedProperty<ZDecimal> cachedCostAmountInLocalCurrency;

		ZDecimal GetCostAmountInLocalCurrency()
		{
			var header = Header;
			if (CostCurrency != null && LI_ServiceExRate != 0 && header != null)
			{
				return header.IsReciprocalExRate ? LI_CostAmount * LI_ServiceExRate : LI_CostAmount / LI_ServiceExRate;
			}
			else
			{
				return 0m;
			}
		}

		[BusinessObjectTestExclude()]
		public ZString LinkedObjectUniqueCode
		{
			get
			{
				if (fLinkedObjectUniqueCode == "" && Parent != null)
				{
					fLinkedObjectUniqueCode = Parent.UniqueCode;
				}
				return fLinkedObjectUniqueCode;
			}
			set
			{
				if (!IsSettingHasChangesSuspended)
				{
					HasChanges |= fLinkedObjectUniqueCode != value;
				}
				SetNonPersistentPropertyValue(LinkedObjectUniqueCodeInfo, ref fLinkedObjectUniqueCode, value);
				SetParentProperties();
				Validation.ValidateLinkedObjectUniqueCode();
			}
		}
		ZString fLinkedObjectUniqueCode;

		public ZPropertyInfo LinkedObjectUniqueCodeInfo
		{
			get { return GetZPropertyInfo(Schema.LinkedObjectUniqueCode); }
		}

		void SetParentProperties()
		{
			if (Header.Parent != null)
			{
				foreach (ILandedCostDistributeTo lCDistributee in Header.Parent.CandidatesToDistributeCostTo)
				{
					if (lCDistributee.UniqueCode == LinkedObjectUniqueCode)
					{
						LI_ParentID = lCDistributee.PK;
						LI_ParentTableCode = lCDistributee.TableCode;
						break;
					}
				}
			}
		}

		[BusinessObjectTestExclude()]
		public ZString LCGroupString
		{
			get { return LI_LandedCostGroup.ToString(); }
			set
			{
				ZByte parsedResult = ZByte.Zero;
				ZByte.TryParse(value, out parsedResult);
				LI_LandedCostGroup = parsedResult;
				LCGroupStringInfo.RefreshBinding();
				Validation.ValidateLCGroupString();
			}
		}

		public ZPropertyInfo LCGroupStringInfo
		{
			get { return GetZPropertyInfo(Schema.LCGroupString); }
		}

		#endregion

		#region Property overrides

		[RelatedBusinessObject("Header")]
		public override ZGuid LI_LT
		{
			get { return base.LI_LT; }
			set { base.LI_LT = value; }
		}

		public override ZGuid LI_ParentID
		{
			get { return base.LI_ParentID; }
			set
			{
				base.LI_ParentID = value;
				NeedToRefreshParent = true;
			}
		}

		public override ZString LI_ParentTableCode
		{
			get { return base.LI_ParentTableCode; }
			set
			{
				base.LI_ParentTableCode = value;
				NeedToRefreshParent = true;
			}
		}

		public override ZString LI_RX_NKCostCurrency
		{
			get { return base.LI_RX_NKCostCurrency; }
			set
			{
				base.LI_RX_NKCostCurrency = value;

				if (Header != null && LI_RX_NKCostCurrency == Header.Company.GC_RX_NKLocalCurrency)
				{
					LI_ServiceExRate = 1;
				}
				else
				{
					if (!LI_RX_NKCostCurrency.IsEmpty && Header.Parent != null)
					{
						LI_ServiceExRate = Header.ExchangeRates.GetCostingExchangeRate(value);

						if (LI_ServiceExRate == 0m)
						{
							Dictionary<ZString, ZDecimal> defaultRates = Header.Parent.GetDefaultExchangeRates();
							if (defaultRates != null)
							{
								ZDecimal defaultValue;

								defaultRates.TryGetValue(LI_RX_NKCostCurrency, out defaultValue);
								LI_ServiceExRate = defaultValue;
							}
						}
					}
				}

				if (LI_CostAmount > 0m)
				{
					LI_CostAmount = LI_CostAmount.Round(DecimalPlaces);
				}
			}
		}

		public ZInt DecimalPlaces
		{
			get
			{
				ZInt result = 2;

				RefCurrency currency = CostCurrency;

				if (currency != null)
				{
					result = currency.Decimals;
				}

				return result;
			}
		}

		public override ZGuid LI_AC_ChargeCode
		{
			get { return base.LI_AC_ChargeCode; }
			set
			{
				base.LI_AC_ChargeCode = value;
				DefaultChargeGroupAndDistributeBy();
			}
		}

		protected void DefaultChargeGroupAndDistributeBy()
		{
			AccChargeCode chargeCodeCached = ChargeCode;
			if (chargeCodeCached != null)
			{
				LI_ChargeDescription = new ZString(chargeCodeCached.AC_DescMultilingual.GetUnresolvedString()).SubstringSafe(0, LandCostInput.Schema.LI_ChargeDescriptionMaxLength);

				if (Header != null && Header.PreferenceCalculator != null)
				{
					ILandedCostPreference preference = Header.PreferenceCalculator.GetPreferenceFromAccChargeCode(chargeCodeCached);
					if (preference != null)
					{
						LI_LandedCostGroup = preference.LCGroupID;
						LI_DistributeCostBy = preference.DistributionBy;
					}
				}
			}
		}

		#endregion

		#region Related Objects

		public LandedCostHeader Header
		{
			get
			{
				if (fHeader == null || fHeader.PK != LI_LT)
				{
					fHeader = (LandedCostHeader)Factory.Load(typeof(LandedCostHeader), LI_LT);
				}
				return fHeader == null || fHeader.IsDeleted ? null : fHeader;
			}
		}
		LandedCostHeader fHeader;

		public ILandedCostDistributeTo Parent
		{
			get
			{
				if (fParent == null || NeedToRefreshParent)
				{
					NeedToRefreshParent = false;

					if (Header != null)
					{
						fParent = (ILandedCostDistributeTo)Header.ParentLoaderForLandedCostInputDistributeTo.LoadBusinessObject(Factory, LI_ParentTableCode, LI_ParentID);
					}
				}
				return fParent;
			}
		}
		bool NeedToRefreshParent = true;
		ILandedCostDistributeTo fParent;

		#endregion

		string IChargeWithChargeCode.AC_Code => ChargeCode?.AC_Code;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		string IChargeWithChargeCode.AC_Desc => ChargeCode?.AC_Desc;

		#region Implementation of IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)LI_ClusterKeyInfo;

		Type IClusterKeyWorker.ParentBizObjType => typeof(LandedCostHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)LI_LTInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}

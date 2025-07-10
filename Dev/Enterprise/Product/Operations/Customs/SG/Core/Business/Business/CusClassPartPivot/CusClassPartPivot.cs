using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusClassPartPivot : Customs.Business.BaseCusClassPartPivot
		, Customs.Business.IAddInfoManager
		, Integration.Customs.SG.ICusClassPartPivot
		, ICusCodeDataTypeSupporter
		, ITariffData
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public new class Schema : Customs.Business.BaseCusClassPartPivot.Schema
		{
			public const string CI_PercAlcohol = "CI_PercAlcohol";
		}

		[ResourceStringData("SGCusClassPartPivot.CI_PercAlcohol", Caption = "% of Alcohol")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsTariffNumReadOnly")]
		[ReadOnlyMember("IsTariffNumReadOnly")]
		public ZDecimal CI_PercAlcohol
		{
			get => AddInfo.ZA_PercAlcohol;
			set
			{
				AddInfo.ZA_PercAlcohol = value;
				CI_PercAlcoholInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CI_PercAlcoholInfo
		{
			get { return GetWrappedZPropertyInfo(CusClassPartPivot.Schema.CI_PercAlcohol, x => AddInfo.ZA_PercAlcoholInfo); }
		}

		[ChildEditable(true)]
		public ProductCodeCollection ProductCodes
		{
			get
			{
				if (productCodes == null)
				{
					productCodes = new ProductCodeCollection(this);
					productCodes.Load();
					productCodes.SetReadOnlyIncludingChildren(CI_CC.IsValid);
					RegisterEditableChildObject(productCodes);
				}
				return productCodes;
			}
		}
		ProductCodeCollection productCodes;

		#endregion

		#region CI_CC

		[ResourceStringData("SGCusClassPartPivot.CI_CC", Caption = "Classification")]
		public override ZGuid CI_CC
		{
			get => base.CI_CC;
			set
			{
				var hasChanged = CI_CC != value;
				base.CI_CC = value;
				if (hasChanged)
				{
					ClearTariffFields();
					if (Part != null)
					{
						Part.MarkAsNeedingValidation();
					}
				}
			}
		}

		public void ClearTariffFields()
		{
			if (!IsCopying && CI_CC.IsValid)
			{
				CI_PercAlcohol = 0.0m;
				ProductCodes.RemoveAndDeleteAll();
				CI_TariffNum = ZString.Empty;
			}
			ProductCodes.SetReadOnlyIncludingChildren(CI_CC.IsValid);
		}

		#endregion // CI_CC

		#region CI_TariffNum

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsTariffNumReadOnly")]
		[ReadOnlyMember("IsTariffNumReadOnly")]
		public override ZString CI_TariffNum
		{
			get => base.CI_TariffNum;
			set
			{
				var hasChanged = CI_TariffNum != value;
				base.CI_TariffNum = value;
				if (hasChanged)
				{
					ProductCodes.MarkAsNeedingValidation();
				}
			}
		}

		public TariffView Tariff => UniversalReferenceDataHelper.LoadBestMatch(Factory, CI_TariffNum, ZDateTime.Today);

		#endregion

		#region CI_OP

		public override ZGuid CI_OP
		{
			get
			{
				return base.CI_OP;
			}
			set
			{
				base.CI_OP = value;
				if (Part != null)
				{
					Part.MarkAsNeedingValidation();
				}
			}
		}

		#endregion // CI_OP

		#region CI_RN_NKCountry

		public override ZString CI_RN_NKCountry
		{
			get
			{
				return base.CI_RN_NKCountry;
			}
			set
			{
				base.CI_RN_NKCountry = value;
				if (Part != null)
				{
					Part.MarkAsNeedingValidation();
				}
			}
		}

		#endregion // CI_RN_NKCountry

		#region CI_CustomsUQ

		[MaxLength(3)]
		[List(nameof(AddInfoLookups) + "." + nameof(SGCusClassPartPivotAddInfoLookups.CustomsUQs))]
		[ResourceStringData("SGCusClassPartPivot.CI_CustomsUQ", Caption = "Customs UQ")]
		public ZString CI_CustomsUQ
		{
			get { return AddInfo.ZA_CustomsEquivalentUQ; }
			set
			{
				AddInfo.ZA_CustomsEquivalentUQ = value;
				CI_CustomsUQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CI_CustomsUQInfo
		{
			get { return GetZPropertyInfo(nameof(CI_CustomsUQ)); }
		}

		#endregion

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return new TariffFormatter();
		}

		#region Lookups

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		#endregion

		#region AddInfo

		protected SGCusClassPartPivotAddInfo AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					addInfo = new SGCusClassPartPivotAddInfo(CI_AddInfoInfo);
					RegisterEditableChildObject(addInfo);
				}
				return addInfo;
			}
		}
		SGCusClassPartPivotAddInfo addInfo;

		public SGCusClassPartPivotAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		#endregion

		#region Validation

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		public new CusClassPartPivotValidation Validation
		{
			get { return (CusClassPartPivotValidation)GetNewValidation(); }
		}

		#endregion

		#region IAddInfoManager Members

		Customs.Business.IAddInfo Customs.Business.IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IProductCodeProvider Members

		ProductCodeCollection ITariffData.ProductCodes => ProductCodes;

		ZString ITariffData.TariffNum => CI_TariffNum;

		ZDecimal ITariffData.PercAlcohol => CI_PercAlcohol;

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.ProductCode, typeof(ProductCode));
			return result;
		}

		#endregion
	}
}

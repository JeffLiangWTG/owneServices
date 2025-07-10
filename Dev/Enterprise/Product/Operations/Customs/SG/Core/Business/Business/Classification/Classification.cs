using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.SG.V4.Business
{
	public class Classification : BaseCusClassification
		, Integration.Customs.SG.ICusClassification
		, IAddInfoManager
		, ICusCodeDataTypeSupporter
		, ITariffData
	{
		public Classification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : BaseCusClassification.Schema
		{
			public const string CC_PercAlcohol = "CC_PercAlcohol";
		}

		public new ClassificationLookups Lookups
		{
			get { return (ClassificationLookups)base.Lookups; }
		}

		public new ClassificationValidation Validation
		{
			get { return (ClassificationValidation)base.Validation; }
		}

		protected override CusClassificationLookups GetNewLookups()
		{
			return new ClassificationLookups(this);
		}

		protected override CusClassificationValidation GetNewValidation()
		{
			return new ClassificationValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_ClassificationType = DefaultClassificationType;
		}

		public override ZString CC_TariffNum
		{
			get { return base.CC_TariffNum; }
			set
			{
				bool changed = base.CC_TariffNum != value;
				base.CC_TariffNum = value;
				if (changed)
				{
					ProductCodes.MarkAsNeedingValidation();
				}
			}
		}

		public TariffView Tariff => UniversalReferenceDataHelper.LoadBestMatch(Factory, CC_TariffNum, ZDateTime.Today);

		protected AddInfoClassification AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					addInfo = new AddInfoClassification(CC_AddInfoInfo);
					RegisterEditableChildObject(addInfo);
				}
				return addInfo;
			}
		}
		AddInfoClassification addInfo;

		public ZDecimal CC_PercAlcohol
		{
			get { return AddInfo.SG_PercAlcohol; }
			set { AddInfo.SG_PercAlcohol = value; }
		}

		public ZPropertyInfo CC_PercAlcoholInfo
		{
			get { return GetWrappedZPropertyInfo(Classification.Schema.CC_PercAlcohol, x => AddInfo.SG_PercAlcoholInfo); }
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
					RegisterEditableChildObject(productCodes);
				}
				return productCodes;
			}
		}
		ProductCodeCollection productCodes;

		public const string DefaultClassificationType = "SG4";

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IProductCodeProvider Members

		ProductCodeCollection ITariffData.ProductCodes => ProductCodes;
		ZString ITariffData.TariffNum => CC_TariffNum;
		ZDecimal ITariffData.PercAlcohol => CC_PercAlcohol;

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

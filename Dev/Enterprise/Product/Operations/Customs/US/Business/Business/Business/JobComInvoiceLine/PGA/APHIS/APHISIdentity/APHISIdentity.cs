using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSAPHISIdentity")]
	public class APHISIdentity : CusCodeData, ICusAddInfoTypeSupporter, IAPHISIdentity
	{
		public APHISIdentity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string UseMultipleNumbers = "UseMultipleNumbers";
		}

		[ReadOnlyMember(nameof(UseMultipleNumbers_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISIdentity|UseMultipleNumbers", Caption = "Use Multiple Numbers", ShortCaption = "Multiple")]
		public ZBool UseMultipleNumbers
		{
			get
			{
				if (!useMultipleNumbers.HasValue)
				{
					useMultipleNumbers = NumberRanges.Count > 0;
				}
				return useMultipleNumbers.Value;
			}
			set
			{
				var oldValue = UseMultipleNumbers;
				if (!useMultipleNumbersSettingInProgress)
				{
					try
					{
						useMultipleNumbersSettingInProgress = true;
						useMultipleNumbers = value;
						if (!IsCopying && oldValue != UseMultipleNumbers)
						{
							if (value)
							{
								CY_Data = ZString.Empty;
							}
							else
							{
								NumberRanges.RemoveAndDeleteAll();
							}
						}
						UseMultipleNumbersInfo.RefreshBinding(oldValue);
						if (!IsValidationSuspended)
						{
							Validation.ValidateUseMultipleNumbers();
						}
						NumberRanges.RefreshBinding();
					}
					finally
					{
						useMultipleNumbersSettingInProgress = false;
					}
				}
			}
		}
		ZBool? useMultipleNumbers;
		bool useMultipleNumbersSettingInProgress;

		public ZPropertyInfo UseMultipleNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.UseMultipleNumbers); }
		}

		bool UseMultipleNumbers_ReadOnly
		{
			get
			{
				var header = Header;
				return header == null || !header.IsLiveAnimalsCategory;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISIdentity|CY_Code", Caption = "Identification Type", ShortCaption = "Type")]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set { base.CY_Code = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISIdentity|CY_Data", Caption = "Identification Number", ShortCaption = "Number")]
		[ReadOnlyMember(nameof(CY_Data_ReadOnly))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		bool CY_Data_ReadOnly
		{
			get { return UseMultipleNumbers; }
		}

		#region Lookups / Validation

		public new APHISIdentityLookups Lookups
		{
			get { return (APHISIdentityLookups)base.Lookups; }
		}

		public new APHISIdentityValidation Validation
		{
			get { return (APHISIdentityValidation)base.Validation; }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new APHISIdentityLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new APHISIdentityValidation(this);
		}

		#endregion

		#region Related Objects

		[ChildEditable(true)]
		public APHISIdentityNumberRangeCollection NumberRanges
		{
			get
			{
				if (numberRanges == null)
				{
					numberRanges = new APHISIdentityNumberRangeCollection(this);
					numberRanges.Load();
					RegisterEditableChildObject(numberRanges);
				}
				return numberRanges;
			}
		}
		APHISIdentityNumberRangeCollection numberRanges;

		public APHISProduct Product
		{
			get { return Parent as APHISProduct; }
		}

		public APHISHeader Header
		{
			get
			{
				var product = Product;
				return product == null ? null : product.Header;
			}
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(APHISProduct)); }
		}

		#endregion

		public override void Delete()
		{
			NumberRanges.RemoveAndDeleteAll();
			base.Delete();
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.APHISIdentity;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (APHISIdentity)base.CloneInternal(args);
			foreach (APHISIdentityNumberRange numberRange in NumberRanges)
			{
				result.NumberRanges.Add((APHISIdentityNumberRange)numberRange.Clone());
			}
			return result;
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISIdentityNumberRange, typeof(APHISIdentityNumberRange));
			return result;
		}

		#endregion

		#region IAPHISIdentity Members

		ZString IAPHISIdentity.IdentityType
		{
			get { return CY_Code; }
		}

		IEnumerable<INumberRange> IAPHISIdentity.Numbers
		{
			get
			{
				if (UseMultipleNumbers)
				{
					foreach (var numberRange in NumberRanges.OfType<INumberRange>())
					{
						yield return numberRange;
					}
				}
				else
				{
					yield return new NumberRangeWrappper() { StartNumber = CY_Data.Right(USAPHISIdentityNumberRangeAddInfoSchema.US_StartNumber.MaxLength) };
				}
			}
		}

		class NumberRangeWrappper : INumberRange
		{
			#region INumberRange Members

			public ZString StartNumber
			{
				get;
				set;
			}

			public ZString EndNumber
			{
				get;
				set;
			}

			#endregion
		}
		#endregion
	}
}

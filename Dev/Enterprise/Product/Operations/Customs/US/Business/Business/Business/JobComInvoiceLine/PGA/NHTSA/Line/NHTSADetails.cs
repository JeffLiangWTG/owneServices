using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business
{
	public class NHTSADetails : USNHTSADetails, ICusAddInfoTypeSupporter, INHTSADetails, Integration.Customs.US.INHTSADetails
	{
		public NHTSADetails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			US_NHTIdentityNumQualifierInfo.HumanReadableName = "Number Type";
			US_NHTIdentityNumberInfo.HumanReadableName = "Number";
			US_NHTLPCOTypeInfo.HumanReadableName = "LPCO Type";
			US_NHTLPCONumberInfo.HumanReadableName = "LPCO Number";
			US_NHTLPCODateTypeInfo.HumanReadableName = "LPCO Date Type";
			US_NHTLPCODateInfo.HumanReadableName = "LPCO Date";
			US_NHTLPCOQuantityInfo.HumanReadableName = "LPCO Quantity";
		}

		#region Schema

		public new class Schema : USNHTSADetails.Schema
		{
			public const string US_NHTIdentityNumber = "US_NHTIdentityNumber";
			public const string US_NHTIdentityNumQualifier = "US_NHTIdentityNumQualifier";
			public const string US_NHTLPCODate = "US_NHTLPCODate";
			public const string US_NHTLPCODateType = "US_NHTLPCODateType";
			public const string US_NHTLPCONumber = "US_NHTLPCONumber";
			public const string US_NHTLPCOQuantity = "US_NHTLPCOQuantity";
			public const string US_NHTLPCOType = "US_NHTLPCOType";
		}

		#endregion

		#region AddInfo Properties

		[MaxLength(17)]
		public ZString US_NHTIdentityNumber
		{
			get { return FirstAdditionalNumber != null ? FirstAdditionalNumber.US_NHTAdditionalIdentityNumber : ZString.Empty; }
			set
			{
				var hasChanges = US_NHTIdentityNumber != value;
				if (hasChanges && !IsCopying)
				{
					var additionalNumber = FirstAdditionalNumber;
					if (additionalNumber == null && !value.IsEmpty)
					{
						additionalNumber = AdditionalNumbers.AddNew();
					}

					if (additionalNumber != null)
					{
						additionalNumber.US_NHTAdditionalIdentityNumber = value;
					}
				}

				Validation.ValidateUS_NHTIdentityNumber();
				US_NHTIdentityNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_NHTIdentityNumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_NHTIdentityNumber); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.NumberTypes))]
		[MaxLength(3)]
		public ZString US_NHTIdentityNumQualifier
		{
			get { return FirstAdditionalNumber != null ? FirstAdditionalNumber.US_NHTAdditionalIdentityNumQualifier : ZString.Empty; }
			set
			{
				var hasChanges = US_NHTIdentityNumQualifier != value;
				if (hasChanges && !IsCopying)
				{
					var additionalNumber = FirstAdditionalNumber;
					if (additionalNumber == null && !value.IsEmpty)
					{
						additionalNumber = AdditionalNumbers.AddNew();
					}

					if (additionalNumber != null)
					{
						additionalNumber.US_NHTAdditionalIdentityNumQualifier = value;
					}
				}

				Validation.ValidateUS_NHTIdentityNumQualifier();
				US_NHTIdentityNumQualifierInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_NHTIdentityNumQualifierInfo
		{
			get { return GetZPropertyInfo(Schema.US_NHTIdentityNumQualifier); }
		}

		public ZDateTime US_NHTLPCODate
		{
			get { return FirstNHTSAPermitAndLicenses != null ? FirstNHTSAPermitAndLicenses.US_NHTLPCODate : ZDateTime.Empty; }
			set
			{
				var hasChanges = US_NHTLPCODate != value;
				if (hasChanges && !IsCopying)
				{
					var permitAndLicense = FirstNHTSAPermitAndLicenses;
					if (permitAndLicense == null && !value.IsEmpty)
					{
						permitAndLicense = PermitAndLicenses.AddNew();
					}

					if (permitAndLicense != null)
					{
						permitAndLicense.US_NHTLPCODate = value;
					}
				}

				Validation.ValidateUS_NHTLPCODate();
				US_NHTLPCODateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_NHTLPCODateInfo
		{
			get { return GetZPropertyInfo(Schema.US_NHTLPCODate); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.LPCODateTypes))]
		[MaxLength(1)]
		public ZString US_NHTLPCODateType
		{
			get { return FirstNHTSAPermitAndLicenses != null ? FirstNHTSAPermitAndLicenses.US_NHTLPCODateType : ZString.Empty; }
			set
			{
				var hasChanges = US_NHTLPCODateType != value;
				if (hasChanges && !IsCopying)
				{
					var permitAndLicense = FirstNHTSAPermitAndLicenses;
					if (permitAndLicense == null && !value.IsEmpty)
					{
						permitAndLicense = PermitAndLicenses.AddNew();
					}

					if (permitAndLicense != null)
					{
						permitAndLicense.US_NHTLPCODateType = value;
					}
				}

				Validation.ValidateUS_NHTLPCODateType();
				US_NHTLPCODateTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_NHTLPCODateTypeInfo
		{
			get { return GetZPropertyInfo(Schema.US_NHTLPCODateType); }
		}

		[MaxLength(33)]
		public ZString US_NHTLPCONumber
		{
			get { return FirstNHTSAPermitAndLicenses != null ? FirstNHTSAPermitAndLicenses.US_NHTLPCONumber : ZString.Empty; }
			set
			{
				var hasChanges = US_NHTLPCONumber != value;
				if (hasChanges && !IsCopying)
				{
					var permitAndLicense = FirstNHTSAPermitAndLicenses;
					if (permitAndLicense == null && !value.IsEmpty)
					{
						permitAndLicense = PermitAndLicenses.AddNew();
					}

					if (permitAndLicense != null)
					{
						permitAndLicense.US_NHTLPCONumber = value;
					}
				}

				Validation.ValidateUS_NHTLPCONumber();
				US_NHTLPCONumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_NHTLPCONumberInfo
		{
			get { return GetZPropertyInfo(Schema.US_NHTLPCONumber); }
		}

		[MaxLength(12)]
		public ZDecimal US_NHTLPCOQuantity
		{
			get { return FirstNHTSAPermitAndLicenses != null ? FirstNHTSAPermitAndLicenses.US_NHTLPCOQuantity : ZDecimal.Zero; }
			set
			{
				var hasChanges = US_NHTLPCOQuantity != value;
				if (hasChanges && !IsCopying)
				{
					var permitAndLicense = FirstNHTSAPermitAndLicenses;
					if (permitAndLicense == null && !value.IsEmpty)
					{
						permitAndLicense = PermitAndLicenses.AddNew();
					}

					if (permitAndLicense != null)
					{
						permitAndLicense.US_NHTLPCOQuantity = value;
					}
				}

				US_NHTLPCOQuantityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_NHTLPCOQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.US_NHTLPCOQuantity); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSADetailsAddInfoLookups.LPCOTypes))]
		[MaxLength(3)]
		public ZString US_NHTLPCOType
		{
			get { return FirstNHTSAPermitAndLicenses != null ? FirstNHTSAPermitAndLicenses.US_NHTLPCOType : ZString.Empty; }
			set
			{
				var hasChanges = US_NHTLPCOType != value;
				if (hasChanges && !IsCopying)
				{
					var permitAndLicense = FirstNHTSAPermitAndLicenses;
					if (permitAndLicense == null && !value.IsEmpty)
					{
						permitAndLicense = PermitAndLicenses.AddNew();
					}

					if (permitAndLicense != null)
					{
						permitAndLicense.US_NHTLPCOType = value;
					}
				}

				Validation.ValidateUS_NHTLPCOType();
				US_NHTLPCOTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_NHTLPCOTypeInfo
		{
			get { return GetZPropertyInfo(Schema.US_NHTLPCOType); }
		}

		[ChildEditable(true)]
		public NHTSAAdditionalNumCollection AdditionalNumbers
		{
			get
			{
				if (fAdditionalNumbers == null)
				{
					fAdditionalNumbers = new NHTSAAdditionalNumCollection(this);
					fAdditionalNumbers.Load();
					RegisterEditableChildObject(fAdditionalNumbers);
				}
				return fAdditionalNumbers;
			}
		}
		NHTSAAdditionalNumCollection fAdditionalNumbers;

		public NHTSAAdditionalNum FirstAdditionalNumber
		{
			get
			{
				if (firstAdditionalNumber == null || firstAdditionalNumber.IsDeleted || firstAdditionalNumber.IsDeleting)
				{
					firstAdditionalNumber = AdditionalNumbers.OfType<NHTSAAdditionalNum>().FirstOrDefault();
				}

				return firstAdditionalNumber;
			}
		}
		NHTSAAdditionalNum firstAdditionalNumber;

		[ChildEditable(true)]
		public NHTSAPermitAndLicensesCollection PermitAndLicenses
		{
			get
			{
				if (fPermitAndLicenses == null)
				{
					fPermitAndLicenses = new NHTSAPermitAndLicensesCollection(this);
					fPermitAndLicenses.Load();
					RegisterEditableChildObject(fPermitAndLicenses);
				}
				return fPermitAndLicenses;
			}
		}
		NHTSAPermitAndLicensesCollection fPermitAndLicenses;

		public NHTSAPermitAndLicenses FirstNHTSAPermitAndLicenses
		{
			get
			{
				if (firstNHTSAPermitAndLicenses == null || firstNHTSAPermitAndLicenses.IsDeleted || firstNHTSAPermitAndLicenses.IsDeleting)
				{
					firstNHTSAPermitAndLicenses = PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().FirstOrDefault();
				}

				return firstNHTSAPermitAndLicenses;
			}
		}
		NHTSAPermitAndLicenses firstNHTSAPermitAndLicenses;

		public ZBool IsVehicleIdentificationNumber
		{
			get { return Header != null && Header.IsMotorVehicles && US_NHTIdentityNumQualifier == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN; }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "NHTSADetails"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();

			var result = (NHTSADetails)base.CloneInternal(args);
			result.AdditionalNumbers.RemoveAndDeleteAll();

			foreach (NHTSAAdditionalNum additionalNumber in AdditionalNumbers)
			{
				result.AdditionalNumbers.Add(additionalNumber.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NHTSAAdditionalNum), false)));
			}

			foreach (NHTSAPermitAndLicenses permitAndLicense in PermitAndLicenses)
			{
				result.PermitAndLicenses.Add(permitAndLicense.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NHTSAPermitAndLicenses), false)));
			}

			return result;
		}

		public override void Delete()
		{
			AdditionalNumbers.RemoveAndDeleteAll();
			PermitAndLicenses.RemoveAndDeleteAll();
			base.Delete();
		}

		public new NHTSADetailsValidation Validation
		{
			get { return (NHTSADetailsValidation)base.Validation; }
		}

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new NHTSADetailsValidation(this);
		}

		#endregion

		#region AddInfo object

		Integration.Customs.US.INHTSADetailsAddInfo Integration.Customs.US.INHTSADetails.AddInfo
		{
			get { return AddInfo; }
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
			result.Add(CusAddInfoTypeAttribute.Codes.USNHTSAAdditionalNumber, typeof(NHTSAAdditionalNum));
			result.Add(CusAddInfoTypeAttribute.Codes.USNHTSAPermitAndLicense, typeof(NHTSAPermitAndLicenses));
			return result;
		}

		#endregion

		#region INHTSADetails Members

		ZString INHTSADetails.BrandName
		{
			get { return US_NHTBrandName; }
		}

		ZString INHTSADetails.Model
		{
			get { return US_NHTModel; }
		}

		ZString INHTSADetails.YearOfManufacturer
		{
			get { return US_NHTYearOfMFR; }
		}

		ZString INHTSADetails.MonthOfManufacturer
		{
			get { return US_NHTMonthOfMFR; }
		}

		ZString INHTSADetails.NumberType
		{
			get
			{
				var numberType = US_NHTIdentityNumQualifier == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN ? (ZString)"AKG" : US_NHTIdentityNumQualifier;
				return !numberType.IsEmpty && !US_NHTIdentityNumber.IsEmpty ? numberType : ZString.Empty;
			}
		}

		ZString INHTSADetails.Number
		{
			get { return US_NHTIdentityNumber; }
		}

		ZString INHTSADetails.CategoryType
		{
			get { return US_NHTCategoryType; }
		}

		ZString INHTSADetails.CategoryCode
		{
			get { return US_NHTCategoryCode; }
		}

		ZString INHTSADetails.DriveSide
		{
			get { return US_NHTDriveSide; }
		}

		ZString INHTSADetails.ModelYear
		{
			get { return US_NHTModelYear; }
		}

		IEnumerable<INHTSAAdditionalNumber> INHTSADetails.AdditionalNumbers
		{
			get { return AdditionalNumbers.OfType<INHTSAAdditionalNumber>().Where(x => x.NumberType == ((INHTSADetails)this).NumberType && x.Number != US_NHTIdentityNumber); }
		}

		IEnumerable<INHTSAPermitAndLicense> INHTSADetails.PermitAndLicenses
		{
			get { return PermitAndLicenses.OfType<INHTSAPermitAndLicense>(); }
		}

		IEnumerable<IEnumerable<INHTSAAdditionalNumber>> INHTSADetails.OtherAdditionalNumbers
		{
			get
			{
				var otherAdditionalNumbersList = new List<List<INHTSAAdditionalNumber>>();
				var otherAdditionalNumbers = AdditionalNumbers.OfType<INHTSAAdditionalNumber>().Where(x => x.NumberType != ((INHTSADetails)this).NumberType && !x.NumberType.IsEmpty && !x.Number.IsEmpty);
				var differentAdditionalNumberTypes = otherAdditionalNumbers.Select(x => x.NumberType).Distinct();
				foreach (var differentAdditionalNumberType in differentAdditionalNumberTypes)
				{
					otherAdditionalNumbersList.Add(otherAdditionalNumbers.Where(x => x.NumberType == differentAdditionalNumberType).ToList());
				}

				return otherAdditionalNumbersList;
			}
		}

		#endregion
	}
}

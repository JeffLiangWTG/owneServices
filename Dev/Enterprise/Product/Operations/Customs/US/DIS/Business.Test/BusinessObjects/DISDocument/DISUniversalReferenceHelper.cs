using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	public class DISUniversalReferenceHelper
	{
		public DISUniversalReferenceHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;
		public RefCusCodeList CreateDisCodeEntry(ZString code, ZString uSDISDocCode)
		{
			return CreateDisCodeEntry(code, uSDISDocCode, "", "", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public RefCusCodeList CreateDisCodeEntry(ZString code, ZString uSDISDocCode, ZString formGroup)
		{
			return CreateDisCodeEntry(code, uSDISDocCode, formGroup, "", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public RefCusCodeList CreateDisCodeEntry(ZString code, ZString uSDISDocCode, ZString formGroup, ZString packageCategory)
		{
			return CreateDisCodeEntry(code, uSDISDocCode, formGroup, packageCategory, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public RefCusCodeList CreateDisCodeEntry(ZString code, ZString uSDISDocCode, ZDateTime startDate, ZDateTime endDate)
		{
			return CreateDisCodeEntry(code, uSDISDocCode, "", "", startDate, endDate);
		}

		public RefCusCodeList CreateDisCodeEntry(ZString code, ZString uSDISDocCode, ZString formGroup, ZString packageCategory, ZDateTime startDate, ZDateTime endDate)
		{
			if (formGroup.IsEmpty)
			{
				formGroup = DISFormGroupCodes.NoGroup;
			}

			var refCusCodeList = DataHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, code, code, startDate, endDate, RefCusCodeListAttributeTypes.Codes.USDISFormGroup, formGroup);
			if (!uSDISDocCode.IsEmpty)
			{
				DataHelper.CreateCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.USDISDocCode, uSDISDocCode);
			}

			if (!packageCategory.IsEmpty)
			{
				DataHelper.CreateCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.USDISPackageCategory, packageCategory);
			}

			return refCusCodeList;
		}

		public UniversalReferenceTestDataHelper DataHelper
		{
			get
			{
				if (universalReferenceHelper == null)
				{
					universalReferenceHelper = new UniversalReferenceTestDataHelper(factory);
					universalReferenceHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, "US DIS Form List");
				}

				return universalReferenceHelper;
			}
		}

		UniversalReferenceTestDataHelper universalReferenceHelper;
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public abstract class CustomsNumberViewStmNumsAuthorisationProvider : CustomsNumberViewStmNumsBusinessProvider
	{
		protected CustomsNumberViewStmNumsAuthorisationProvider(BusinessObjectFactory factory, ZString providerKey, ZGuid ownerPk) : base(factory, providerKey, ownerPk)
		{
		}

		protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
		{
			return new CustomsNumberViewStmNumsSetting(Parent, rangeType);
		}

		protected override ICustomsNumberViewStmNumsParent GetParentCore(BusinessObjectFactory factory, ZGuid ownerPk)
		{
			return factory.Load<CusAuthorisationHeader>(ownerPk);
		}

		protected override BusinessObject GetOwnerCore(BusinessObjectFactory factory, ZGuid ownerPk)
		{
			return factory.Load<CusAuthorisationHeader>(ownerPk);
		}

		protected override ZString GetOwnerTypeCore(CustomsNumberViewStmNums stmNums)
		{
			return Res.GetString("66513403-A24D-4F8E-9586-7EE90505B47D", "Authorization");
		}

		protected override ZString GetOwnerForDisplayCore(BusinessObject owner)
		{
			return owner is CusAuthorisationHeader authorisation ? authorisation.CPH_Number : ZString.Empty;
		}

		protected override ZString GetProviderKeyCore()
		{
			return ProviderKey;
		}

		protected override ZString GetDetailCore(CustomsNumberViewStmNumsWrapper wrapper)
		{
			return Res.GetString("AA91AB8E-9A27-43FC-85C5-93212A88021A", "Range Type: {0}, Name: {1}", wrapper.SN_Type, wrapper.SN_FountainName);
		}

		protected override IBusinessObjectCollection GetOwnerCollectionCore(BusinessObjectFactory factory, CustomsNumberViewStmNums stmNums)
		{
			return new CusAuthorisationHeaderCollection(factory);
		}
	}
}

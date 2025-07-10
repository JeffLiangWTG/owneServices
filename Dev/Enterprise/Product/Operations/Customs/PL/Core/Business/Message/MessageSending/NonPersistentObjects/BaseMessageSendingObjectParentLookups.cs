using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

public class BaseMessageSendingObjectParentLookups : ZLookups
{
	public BaseMessageSendingObjectParentLookups(BaseMessageSendingObjectParent parent) : base(parent)
	{
	}

	public CodeDescriptionPairList PurposeOfSendingList => Factory.GetCachedValue<MessageSendingPurposeOfSendingList>();

	public CodeDescriptionPairList ProcedureList => Factory.GetCachedValue<MessageSendingProcedureList>();

	public CodeDescriptionPairList EnquiryInformationCodeList => EnquiryInformationCodeListCore();

	public CodeDescriptionPairList EnquiryInformationCodeListCore() => Factory.GetCachedValue("BaseMessageSendingObjectParent.EnquiryInformationCodeListCL210", () =>
	{
		var result = new UntranslatableCodeDescriptionPairList((NoResString)"Enquiry Information is not translatable now");
		var cachedList = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Poland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210, ZDateTime.Today);
		result.AddRangeOverwriteIfExists(cachedList);
		return result;
	});
}

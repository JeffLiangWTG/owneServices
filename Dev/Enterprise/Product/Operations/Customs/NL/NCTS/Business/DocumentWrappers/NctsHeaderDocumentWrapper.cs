using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.NCTS.Business;
using CusAuthorizationUsage = Enterprise.Customs.EU.Business.CusAuthorizationUsage;

namespace Enterprise.Customs.NL.NCTS.DocumentWrappers;

public class NctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper
{
	protected NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
	{
	}

	public static Enterprise.DocumentWrappers.Customs.EU.NCTS.NctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
	{
		Argument.NotNull(nctsHeader, nameof(nctsHeader));
		return new NctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
	}

	public abstract class Schema
	{
		public const int EmergencySequenceNumberLength = 10;
	}

	protected new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	protected override ZBool GetShowStampOnBoxC() => MovementHeader.IsFallbackProcedure;

	protected override ZString GetBoxCDeparureOfficeCountryCode() => Core.Constants.CountryCodes.Netherlands;

	protected override ZString GetBoxCOfficeOfDepartureCode() => NLNctsConstants.DocumentWrapper.OfficeOfDepartureCode;

	protected override ZString GetBoxCUniqueReferenceNumber() => MovementHeader.FallbackNumber;

	protected override ZString GetBoxCDate() => ConvertDateToString(MovementHeader.FallbackTime);

	protected override ZString GetBoxCAuthorizedConsignorName() => AuthorizationUsage?.Owner?.OH_Code ?? ZString.Empty;

	protected override ZString GetBoxCAuthorisationNumber() => AuthorizationUsage?.AGC_Number.SubstringSafe(AuthorizationUsage.AGC_Number.Length - 8) ?? ZString.Empty;

	protected override ZString GetNotReleasedWatermark() => ZString.Empty;

	CusAuthorizationUsage AuthorizationUsage => authorizationUsage ??= MovementHeader?.CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Code.EqualsIgnoringCase(CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit));
	CusAuthorizationUsage authorizationUsage;
}

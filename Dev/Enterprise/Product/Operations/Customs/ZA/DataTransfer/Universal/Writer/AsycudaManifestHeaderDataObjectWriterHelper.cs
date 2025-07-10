using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Business.AsycudaManifestHeader;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class AsycudaManifestHeaderDataObjectWriterHelper : AsycudaManifestUniversalCommonHelper
	{
		public AsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader headerBO)
			: base(headerBO.Factory)
		{
			Header = headerBO;
		}

		public readonly AsycudaManifestHeader Header;

		public CodeDescriptionPair GetCodeDescriptionPair(ZString code, CodeDescriptionPairList list)
		{
			return new CodeDescriptionPair() { Code = code, Description = list?.GetDescriptionFromCode(code) };
		}

		public ContainerType ConvertGuidToContainerType(ZGuid containerTypePK)
		{
			var containerType = Header.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.PK, containerTypePK));
			if (containerType == null)
			{
				return null;
			}
			else
			{
				return new ContainerType() { Code = containerType.RC_Code, Description = containerType.RC_DescriptionMultilingual };
			}
		}

		public CodeDescriptionPair10Char GetCodeDescriptionPair10Char(ZString code, CodeDescriptionPairList list)
		{
			return new CodeDescriptionPair10Char() { Code = code, Description = list?.GetDescriptionFromCode(code) };
		}

		public CodeDescriptionPair35Char GetCodeDescriptionPair35Char(ZString code, ZString description)
		{
			return new CodeDescriptionPair35Char() { Code = code, Description = description };
		}

		public EntryStatus GetEntryStatus(ZString code, CodeDescriptionPairList list)
		{
			return new EntryStatus() { Code = code, Description = list?.GetDescriptionFromCode(code) };
		}

		public ContainerMode GetContainerMode(ZString code, CodeDescriptionPairList list)
		{
			return new ContainerMode() { Code = code, Description = list?.GetDescriptionFromCode(code) };
		}

		public ZString ConvertIndicatorToYesNo(ZString value) => value == Indicator_1 ? Yes : No;

		public ZString ConvertBoolToYesNo(ZBool value) => value ? Yes : No;

		public ZString ConvertDateTimeToString(ZDateTime dateTime) => dateTime.IsEmpty ? string.Empty : dateTime.ToString();

		public readonly ZString Indicator_1 = "1";
		public readonly ZString Yes = YesNoList.Descriptions.Yes;
		public readonly ZString No = YesNoList.Descriptions.No;
	}
}


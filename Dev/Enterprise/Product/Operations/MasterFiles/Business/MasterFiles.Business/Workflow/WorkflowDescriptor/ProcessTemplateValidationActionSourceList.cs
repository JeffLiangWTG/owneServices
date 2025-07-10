using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationActionSourceList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string FieldSpecificValidation = ProcessTemplateValidationActionSource.Codes.FieldSpecificValidation;
			public const string Save = ProcessTemplateValidationActionSource.Codes.Save;
		}

		public static class Descriptions
		{
			public static MultilingualString FieldSpecificValidation { get { return ResString.GetMultilingualString("ProcessTemplateValidationActionSourceList|FieldSpecificValidation", ProcessTemplateValidationActionSource.Descriptions.FieldSpecificValidation); } }
			public static MultilingualString Save { get { return ResString.GetMultilingualString("ProcessTemplateValidationActionSourceList|Save", ProcessTemplateValidationActionSource.Descriptions.Save); } }
		}

		public ProcessTemplateValidationActionSourceList()
		{
			if (FieldSpecificValidationEnabled())
			{
				AddPair(Codes.FieldSpecificValidation, Descriptions.FieldSpecificValidation);
			}
			
			AddPair(Codes.Save, Descriptions.Save);
		}

		internal static bool FieldSpecificValidationEnabled()
		{
			var functionality = ObjectFactory.Get<IZZCustomsFunctionalityEffectiveDate>();
			return functionality.IsFunctionalityValid(FSVFunctionalityCode, CommonDataGrouping, ZDateTime.Now)
				|| functionality.IsFunctionalityValid(FSVPilotFunctionCode, CommonDataGrouping, ZDateTime.Now);
		}

		internal const string FSVFunctionalityCode = "WVFSV";
		internal const string FSVPilotFunctionCode = "PWFSV";
	}
}

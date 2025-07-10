using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationSeverityList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Error = ProcessTemplateValidationSeverity.Codes.Error;
			public const string Message = ProcessTemplateValidationSeverity.Codes.Message;
			public const string Warning = ProcessTemplateValidationSeverity.Codes.Warning;
		}

		public static class Descriptions
		{
			public static MultilingualString Error { get { return ResString.GetMultilingualString("ProcessTemplateValidationSeverityList|Error", ProcessTemplateValidationSeverity.Descriptions.Error); } }
			public static MultilingualString Message { get { return ResString.GetMultilingualString("ProcessTemplateValidationSeverityList|Message", ProcessTemplateValidationSeverity.Descriptions.Message); } }
			public static MultilingualString Warning { get { return ResString.GetMultilingualString("ProcessTemplateValidationSeverityList|Warning", ProcessTemplateValidationSeverity.Descriptions.Warning); } }
		}

		public ProcessTemplateValidationSeverityList()
		{
			AddPair(Codes.Error, Descriptions.Error);
			AddPair(Codes.Message, Descriptions.Message);
			AddPair(Codes.Warning, Descriptions.Warning);
		}
	}
}

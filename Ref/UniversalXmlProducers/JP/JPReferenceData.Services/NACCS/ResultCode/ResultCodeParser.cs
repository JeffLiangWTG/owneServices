namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ResultCodeParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.ResultCode;

		protected override bool CustomisedRowValidate(string[] columns) => columns[0] != "jobcode";

		protected override string GetZZD_Description(string[] columns) => columns[5] + columns[6];

		protected override string GetZZD_Code(string[] columns)
		{
			var businessCode = PadBusinessCodeIfNeeded(columns[0]);
			var resultCode = columns[2];
			return businessCode + resultCode;
		}
		
		static string PadBusinessCodeIfNeeded(string businessCode)
		{
			return businessCode.Trim().PadRight(5, '_');
		}
	}
}

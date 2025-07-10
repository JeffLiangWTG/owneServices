namespace Enterprise.Customs.US.Business
{
	partial class ForeignPortTypeList
	{
		public static string GetCodeFromType(USCForeignPortWrapper.Type type)
		{
			switch (type)
			{
				case USCForeignPortWrapper.Type.AES:
					return Codes.AES;
				case USCForeignPortWrapper.Type.InBond:
					return Codes.InBond;
				default:
					return Codes.Common;
			}
		}
	}
}

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal
{
	class CusCodeDataTypeAndCodeListProvider
	{
		public static ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetTypeListForJobDeclaration();
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetTypeListForJobComInvoiceLine();
					break;
			}
			return result;
		}

		#region Implementation

		static CodeDescriptionPairList GetTypeListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.CALicenceNumber, CusCodeDataTypeList.Descriptions.CALicenceNumber);
			return result;
		}

		static CodeDescriptionPairList GetTypeListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusCodeDataTypeList.Codes.CASCode1, CusCodeDataTypeList.Descriptions.CASCode1);
			result.AddPair(CusCodeDataTypeList.Codes.CASCode2, CusCodeDataTypeList.Descriptions.CASCode2);
			result.AddPair(CusCodeDataTypeList.Codes.CASCode3, CusCodeDataTypeList.Descriptions.CASCode3);

			return result;
		}

		#endregion
	}
}

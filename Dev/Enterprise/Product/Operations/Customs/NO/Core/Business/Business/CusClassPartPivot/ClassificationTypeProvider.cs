using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

class ClassificationTypeProvider : IClassificationTypeProvider
{
	public ZString HTICode => ClassificationTypeList.Codes.HTI;

	public ZString HTECode => ClassificationTypeList.Codes.HTE;

	public ZString HTBCode => ClassificationTypeList.Codes.HTB;

	public ZString SHBCode => ZString.Empty;
}

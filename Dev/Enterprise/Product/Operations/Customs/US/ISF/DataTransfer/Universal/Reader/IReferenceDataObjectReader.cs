using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader
{
	public interface IReferenceDataObjectReader
	{
		ZString BillType { get; }
		CusISFBill BillBO { get; }
	}
}

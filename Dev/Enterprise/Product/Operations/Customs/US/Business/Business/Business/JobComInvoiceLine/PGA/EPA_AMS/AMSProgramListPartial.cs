using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class AMSProgramList
	{
		public static bool IsAMSProgramButNotNOP(BusinessObjectFactory factory, ZString program)
		{
			return GetInstanceList(factory).ContainsCode(program) && program != AMSProgramList.Codes.OR1 && program != AMSProgramList.Codes.OR2;
		}

		public static AMSProgramList GetInstanceList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AMSProgramList", () =>
			{
				return new AMSProgramList();
			});
		}
	}
}

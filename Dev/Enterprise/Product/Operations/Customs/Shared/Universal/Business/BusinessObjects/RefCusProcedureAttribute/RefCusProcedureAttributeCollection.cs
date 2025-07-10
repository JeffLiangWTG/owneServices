using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProcedureAttributeCollection : ActiveBusinessObjectCollection<RefCusProcedureAttribute>
	{
		public RefCusProcedureAttributeCollection(RefCusProcedure procedure)
			: base(procedure.Factory, procedure, new ZQuery(), RefCusProcedureAttributeSchema.ZXB_ZZ6_ProcedureCode)
		{
		}

		public RefCusProcedureAttribute AddNew(ZString name, ZString value)
		{
			var result = AddNew();
			result.ZXB_Name = name;
			result.ZXB_Value = value;
			return result;
		}
	}
}

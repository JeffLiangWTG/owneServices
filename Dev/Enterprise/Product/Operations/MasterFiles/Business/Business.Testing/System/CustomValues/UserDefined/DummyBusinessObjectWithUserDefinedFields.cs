using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[UserDefinedValues]
	class DummyBusinessObjectWithUserDefinedFields : DummyEnterpriseBusinessObject
	{
		public DummyBusinessObjectWithUserDefinedFields(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[MaxLength(100)]
		public override ZString Z0_VarCharMax
		{
			get { return base.Z0_VarCharMax; }
			set { base.Z0_VarCharMax = value; }
		}
	}
}

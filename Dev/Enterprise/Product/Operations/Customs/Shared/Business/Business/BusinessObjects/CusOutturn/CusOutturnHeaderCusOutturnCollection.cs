using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusOutturnHeaderCusOutturnCollection : DependentBusinessObjectCollection<CusOutturn, CusOutturnHeader>
	{
		public CusOutturnHeaderCusOutturnCollection(CusOutturnHeader parent) : base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusOutturnSchema.C5_C6; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CusOutturn);
		}
	}
}

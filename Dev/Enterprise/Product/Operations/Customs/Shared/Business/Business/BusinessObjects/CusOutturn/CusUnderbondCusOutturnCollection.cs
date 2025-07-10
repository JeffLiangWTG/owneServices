using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusUnderbondCusOutturnCollection : DependentBusinessObjectCollection<CusOutturn, CusUnderbond>
	{
		public CusUnderbondCusOutturnCollection(CusUnderbond underbond)
			: base(underbond)
		{
			this.Underbond = underbond;
		}

		public readonly CusUnderbond Underbond;

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CusOutturn);
		}

		public new CusOutturn AddNew(Type businessObjectType)
		{
			return base.AddNew(businessObjectType);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get
			{
				return CusOutturnSchema.C5_C4_Underbond;
			}
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			CusOutturn outturn = child as CusOutturn;
			if (outturn != null && !Underbond.C4_C6.IsEmpty)
			{
				outturn.C5_C6 = Underbond.C4_C6;
			}
		}
	}
}

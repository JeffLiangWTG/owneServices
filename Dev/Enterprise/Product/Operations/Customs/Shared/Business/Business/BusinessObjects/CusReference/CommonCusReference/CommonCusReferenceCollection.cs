using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CommonCusReferenceCollection<T> : DependentBusinessObjectCollection<T, BusinessObject> where T : CommonCusReference
	{
		public CommonCusReferenceCollection(BusinessObject parent, ZString type) : base(parent, SetAdditionalFilter(Argument.NotNullOrEmpty(type, nameof(type))))
		{
			this.type = type;
		}
		readonly ZString type;

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusReferenceSchema.CFR_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var reference = (CommonCusReference)child;
			reference.CFR_Type = type;
		}

		static ZQuery SetAdditionalFilter(ZString type) => new ZQuery(CusReferenceSchema.CFR_Type, type);

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();
	}
}

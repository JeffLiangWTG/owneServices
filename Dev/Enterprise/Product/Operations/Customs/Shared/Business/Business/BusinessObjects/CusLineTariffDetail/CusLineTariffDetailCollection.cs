using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusLineTariffDetailCollection<TCusLineTariffDetail> : DependentBusinessObjectCollection<TCusLineTariffDetail, BusinessObject>, ICusLineTariffDetailCollection<TCusLineTariffDetail>
		where TCusLineTariffDetail : CusLineTariffDetail
	{
		public CusLineTariffDetailCollection(BaseJobComInvoiceLine parent)
			: base(parent)
		{
			Argument.NotNull(parent, nameof(parent));
		}

		public CusLineTariffDetailCollection(BaseCusClassPartPivot parent)
			: base(parent)
		{
			Argument.NotNull(parent, nameof(parent));
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusLineTariffDetailSchema.BZ_ParentID; }
		}

		public TCusLineTariffDetail AddNew(ZString tariffType, ZString tariffCode)
		{
			var result = (CusLineTariffDetail)AddNew();
			result.BZ_Type = tariffType;
			result.BZ_Tariff = tariffCode;
			return (TCusLineTariffDetail)result;
		}

		public IEnumerator<TCusLineTariffDetail> GetEnumerator() => Elements.Cast<TCusLineTariffDetail>().GetEnumerator();
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusPersonCountryCollection<out TCusPersonCountry> : IBusinessObjectCollection<TCusPersonCountry>
		where TCusPersonCountry : AutoCusPersonCountry
	{
		new TCusPersonCountry this[int index] { get; }
	}

	public class CusPersonCountryCollection<TCusPersonCountry> : DependentBusinessObjectCollection<TCusPersonCountry, AutoCusPerson>, ICusPersonCountryCollection<TCusPersonCountry>
		where TCusPersonCountry : AutoCusPersonCountry
	{
		public CusPersonCountryCollection(AutoCusPerson master)
			: base(master)
		{
		}

		public IEnumerator<TCusPersonCountry> GetEnumerator() => Elements.Cast<TCusPersonCountry>().GetEnumerator();

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusPersonCountrySchema.CPC_CPN_Person;
	}
}

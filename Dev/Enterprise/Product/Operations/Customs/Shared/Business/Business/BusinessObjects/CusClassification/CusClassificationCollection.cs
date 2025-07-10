using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IClassificationCollection<out T> : IActiveBusinessObjectCollection<T>
		where T : BaseCusClassification
	{
		new T this[int index] { get; }
	}

	public class ClassificationCollection<T> : ActiveBusinessObjectCollection<T>, IClassificationCollection<T>
		where T : BaseCusClassification
	{
		/// <summary>
		/// Part.Classifications should contain only classifications that are linked for the country
		/// If a part is hooked up to an AU invoice line loaded in a NZ company, Part.Classifications should contain AU classifications only 
		/// </summary>
		public ClassificationCollection(OrgSupplierPart associatedObject, ZString countryCodeToLoad)
			: base(associatedObject, typeof(BaseCusClassPartPivot), new ZQuery(CusClassificationSchema.CC_RN_NKCountryCode, countryCodeToLoad))
		{
			Relationship.AddFilter(new ZQuery(CusClassPartPivotSchema.CI_RN_NKCountry, countryCodeToLoad));
			var manyToMany = Relationship as ManyToManyRelationship;
			if (manyToMany != null)
			{
				manyToMany.AdditionalDivotFilter = new ZQuery(CusClassPartPivotSchema.CI_RN_NKCountry, countryCodeToLoad);
			}
		}
	}
}

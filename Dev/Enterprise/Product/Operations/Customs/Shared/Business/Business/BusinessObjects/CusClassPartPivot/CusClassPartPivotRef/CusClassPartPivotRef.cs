using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(BaseCusClassPartPivot), "CusClassPartPivotRefs")]
	public class CusClassPartPivotRef : AutoCusClassPartPivotRef, Integration.Customs.Shared.ICusClassPartPivotRef, ITypeDeciderContext
	{
		public CusClassPartPivotRef(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static readonly CusClassPartPivotRefTypeDecider TypeDecider = new CusClassPartPivotRefTypeDecider();

		[BusinessObjectTestExclude]
		[RelatedBusinessObject(nameof(CusClassPartPivot))]
		public override ZGuid CIR_CI
		{
			get => base.CIR_CI;
			set => base.CIR_CI = value;
		}

		public BaseCusClassPartPivot CusClassPartPivot => Factory.Load<BaseCusClassPartPivot>(CIR_CI);

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new CusClassPartPivotRefUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country
		{
			get
			{
				var result = CusClassPartPivot?.CI_RN_NKCountry ?? ZString.Empty;
				return result.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : result;
			}
		}

		#endregion
	}
}

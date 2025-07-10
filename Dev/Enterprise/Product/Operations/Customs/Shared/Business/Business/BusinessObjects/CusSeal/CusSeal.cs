using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusSeal : AutoCusSeal
	{
		public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusSealTypeDecider TypeDecider = new CusSealTypeDecider();

		[List($"{nameof(Lookups)}.{nameof(CusSealLookups.UnloadingStatesList)}")]
		public override ZString BK_UnloadingState { get => base.BK_UnloadingState; set => base.BK_UnloadingState = value; }

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			BK_UnloadingState = "NEW";
			BK_SealType = ZString.Empty;
		}
#endif
	}
}

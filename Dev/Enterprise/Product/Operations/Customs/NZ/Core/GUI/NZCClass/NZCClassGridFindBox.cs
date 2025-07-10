
namespace Enterprise.Customs.NZ.GUI
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Common.GUI;
	using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.GUI.Testing;

	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class NZCClassGridFindBox : TariffGridFindBox
	{
		protected override IFindBoxPopup GetNewNonBorderWisePopupForm()
		{
			return new NZCClassForm(new FamilyMemberCollectionForBinding());
		}

		protected override IFindBoxListProvider GetNewListProvider()
		{
			return new NZCClassFindBoxListProvider();
		}
	}
}



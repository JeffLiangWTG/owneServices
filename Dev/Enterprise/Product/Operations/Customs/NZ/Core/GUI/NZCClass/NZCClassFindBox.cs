
namespace Enterprise.Customs.NZ.GUI
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.GUI.Testing;

	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class NZCClassFindBox : Common.GUI.TariffFindBox
	{
		int fMaxTariffLength;
		public int MaxTariffLength
		{
			get { return fMaxTariffLength; }
			set { fMaxTariffLength = value; }
		}

		protected override IFindBoxPopup GetNewNonBorderWisePopupForm()
		{
			NZCClassForm result = new NZCClassForm(new FamilyMemberCollectionForBinding());
			result.MaxTariffLength = MaxTariffLength;
			return result;
		}

		#region ListProvider
		protected override IFindBoxListProvider ListProvider
		{
			get { return new NZCClassFindBoxListProvider(); }
		}

		#endregion
	}
}

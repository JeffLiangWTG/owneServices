using System;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.Common.GUI.Testing;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	public class NZCClassGridFindBoxTest : TariffGridFindBoxTest
	{
		protected override TariffColumnStyleInfo GetNewTariffColumnStyleInfo()
		{
			return new NZCClassColumnStyleInfo();
		}

		protected override Type ExpectedFormTypeWhenBorderWiseNotEnabled
		{
			get
			{
				return typeof(NZCClassForm);
			}
		}

		protected override Type ExpectedListProviderType
		{
			get
			{
				return typeof(NZCClassFindBoxListProvider);
			}
		}

		protected override TariffGridFindBox GetNewTariffGridFindBox()
		{
			return new NZCClassGridFindBox();
		}
	}
}

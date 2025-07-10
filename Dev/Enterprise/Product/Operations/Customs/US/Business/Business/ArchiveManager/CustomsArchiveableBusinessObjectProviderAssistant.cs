using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Core;

namespace Enterprise.Customs.US.Business.ArchiveManager
{
	class CustomsArchiveableBusinessObjectProviderAssistant : Customs.Business.ArchiveManager.CustomsArchiveableBusinessObjectProviderAssistant
	{
		public override string CountryCode
		{
			get { return Constants.CountryCodes.UnitedStates; }
		}

		public override IArchiveableBusinessObject LoadArchiveableBusinessObject(BusinessObject businessObjectLoadedByProvider)
		{
			Argument.NotNull(businessObjectLoadedByProvider, "businessObjectLoadedByProvider");

			if (businessObjectLoadedByProvider is Integration.Customs.US.IJobDeclaration)
			{
				return new ArchiveableJobDeclaration((Integration.Customs.US.IJobDeclaration)businessObjectLoadedByProvider);
			}
			else
			{
				throw new ArgumentException("businessObjectLoadedByProvider type expected to be Enterprise.Integration.Customs.US.IJobDeclaration, but was " + businessObjectLoadedByProvider.GetType().ToString());
			}
		}
	}
}

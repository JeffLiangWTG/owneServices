using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.TW.Business
{
	public class PackingSynchroniser : Customs.Business.PackingSynchroniser
	{
		public PackingSynchroniser(JobDeclarationSynchroniser parentSynchroniser, BaseJobDeclaration declaration)
			: base(parentSynchroniser, declaration)
		{
		}

		protected override void SetSynchroniserAdditionalPackLineFields(IPackingInformation newPackage, PackLine packLine)
		{
			base.SetSynchroniserAdditionalPackLineFields(newPackage, packLine);
			if (newPackage is Package package)
			{
				using (package.SuspendSettingHasChanges())
				using (package.GetValidationSuspender())
				{
					package.CW_GrossWeight = packLine.JL_ActualWeight;
					package.CW_GrossWeightUQ = packLine.JL_ActualWeightUQ;
					package.CW_Volume = packLine.JL_ActualVolume;
					package.CW_VolumeUQ = packLine.JL_ActualVolumeUQ;
					package.CW_Length = packLine.JL_Length;
					package.CW_Width = packLine.JL_Width;
					package.CW_Height = packLine.JL_Height;
					package.CW_DimensionUQ = packLine.JL_UnitOfDimension;
				}
			}
		}
	}
}

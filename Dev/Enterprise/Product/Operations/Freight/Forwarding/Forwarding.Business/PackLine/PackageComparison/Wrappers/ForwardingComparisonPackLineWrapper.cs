using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	class ForwardingComparisonPackLineWrapper : IComparisonPackageWrapper
	{
		public ForwardingComparisonPackLineWrapper(ForwardingPackLine packLine)
		{
			Argument.NotNull(packLine, nameof(packLine));

			this.packLine = packLine;
		}

		readonly ForwardingPackLine packLine;

		public ZString Name => "PackLine";

		public ZString CommodityCode => packLine.JL_RH_NKCommodityCode;

		public ZString HarmonisedCode => packLine.JL_HarmonisedCode;

		public ZString PackTypeCode => packLine.JL_F3_NKPackType;

		public ZString InspectionTypeCode => packLine.JL_InspectionTypeCode;

		public ZBool IsHighRisk => packLine.JL_IsHighRisk;

		public ZString AdditionalInspectionTypeCode => packLine.JL_AdditionalInspectionTypeCode;

		public ZDecimal Length => packLine.JL_Length;

		public ZDecimal Width => packLine.JL_Width;

		public ZDecimal Height => packLine.JL_Height;

		public ZString UnitOfLength => packLine.JL_Calc_HeightUnit;

		public ZString MarksAndNumbers => packLine.JL_MarksAndNumbers;

		public ZInt PackageCount => packLine.JL_PackageCount;

		public ZDecimal Volume => packLine.JL_ActualVolume;

		public ZDecimal PkgPackageCollection_TotalVolume => packLine.PkgPackageCollection_TotalVolume;

		public ZString UnitOfVolume => packLine.JL_ActualVolumeUQ;

		public ZDecimal Weight => packLine.JL_ActualWeight;

		public ZDecimal PkgPackageCollection_TotalWeight => packLine.PkgPackageCollection_TotalWeight;

		public ZString UnitOfWeight => packLine.JL_ActualWeightUQ;

		public ZString GoodsDescription => packLine.JL_Description;

		public ZBool RequiresTemperatureControl => packLine.JL_RequiresTemperatureControl;

		public ZDecimal RequiredTemperatureMinimum => packLine.JL_RequiredTemperatureMinimum;

		public ZDecimal RequiredTemperatureMaximum => packLine.JL_RequiredTemperatureMaximum;

		public ZString RequiredTemperatureUnit => packLine.JL_RequiredTemperatureUnit;

		public ZString ContainerNumber => packLine.Containers.Cast<ForwardingContainer>().FirstOrDefault()?.JC_ContainerNum ?? ZString.Empty;
	}
}

using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusClassPartPivotLookups : USAddInfoLookups
	{
		public AddInfoCusClassPartPivotLookups(AddInfoCusClassPartPivot parent)
			: base(parent)
		{
		}

		public new AddInfoCusClassPartPivot Parent => (AddInfoCusClassPartPivot)base.Parent;

		public CodeDescriptionPairList ProductExclusionList
		{
			get
			{
				var partPivot = Parent.Parent;

				return partPivot != null
					? Factory.GetCachedValue("CusClassPartPivotProductExclusionList" + partPivot.CI_TariffNum, () =>
					{
						var result = new CodeDescriptionPairList();

						if (partPivot.IsOnlySteelProductAvailable)
						{
							result.AddPair(AdditionalDeclarationTypeCodeList.Codes._02, AdditionalDeclarationTypeCodeList.Descriptions._02);
						}
						else if (partPivot.IsOnlyAluminumProductAvailable)
						{
							result.AddPair(AdditionalDeclarationTypeCodeList.Codes._03, AdditionalDeclarationTypeCodeList.Descriptions._03);
						}
						else
						{
							result.AddPair(AdditionalDeclarationTypeCodeList.Codes._02, AdditionalDeclarationTypeCodeList.Descriptions._02);
							result.AddPair(AdditionalDeclarationTypeCodeList.Codes._03, AdditionalDeclarationTypeCodeList.Descriptions._03);
						}

						return result;
					})
					: new CodeDescriptionPairList();
			}
		}
	}
}

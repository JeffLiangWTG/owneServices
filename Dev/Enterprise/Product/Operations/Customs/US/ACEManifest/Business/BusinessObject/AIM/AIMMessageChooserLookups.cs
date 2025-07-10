using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public sealed class AIMMessageChooserLookups : MessageChooserLookups
	{
		public AIMMessageChooserLookups(BusinessObjectFactory factory, AIMMessageChooser parent)
			: base(parent)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;
		new AIMMessageChooser Parent => (AIMMessageChooser)base.Parent;

		public CodeDescriptionPairList ReasonList => factory.GetCachedValue<AIMReasonCodes>();

		public CodeDescriptionPairList FreightStatusRequestCodeList
		{
			get
			{
				var result = factory.GetCachedValue(Parent.IsManifestMessage ? "AIMFreightStatusRequestCodes" : "Filtered_AIMFreightStatusRequestCodes", () =>
				{
					var list = new AIMFreightStatusRequestCodes();
					if (!Parent.IsManifestMessage)
					{
						list.RemoveCode(AIMFreightStatusRequestCodes.Codes.RequestForHouseInformationAssociatedToMaster);
					}
					list.Sort();
					return list;
				});
				return result;
			}
		}
	}
}

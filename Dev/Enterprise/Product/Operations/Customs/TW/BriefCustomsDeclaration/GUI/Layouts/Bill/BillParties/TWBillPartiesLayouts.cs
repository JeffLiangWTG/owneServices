using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public sealed class TWBillPartiesLayouts : IPanelLayoutProvider
	{
		PanelLayout BillParties { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillParties;

		public TWBillPartiesLayouts()
		{
			BillParties = CreateBillPartiesLayout();
		}

		PanelLayout CreateBillPartiesLayout()
		{
			var builder = new TWBillPartiesLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.ShipperAddressUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(common.ConsigneeAddressUserControl, ControlWidthClass.LongNoCaption);

			builder.SetCaption(common.ShipperAddressUserControl, b => b.Header?.IsImport ?? false ? new ResourceStringData("ABD52784-31B0-4552-A5DD-4E6222E2438D", (NoResString)"Supplier") : new ResourceStringData("B0E2D47C-85B7-47CC-8C05-292F7632C61F", (NoResString)"Exporter"), b => b.Header?.AMA_NatureInfo);
			builder.SetCaption(common.ConsigneeAddressUserControl, b => b.Header?.IsImport ?? false ? new ResourceStringData("1ACE0591-A00D-4A75-B092-B1D8883D6651", (NoResString)"Consignee") : new ResourceStringData("ECF6A166-F659-45FC-BE80-6696666B1870", (NoResString)"Buyer"), b => b.Header?.AMA_NatureInfo);

			return builder.Build();
		}
	}
}

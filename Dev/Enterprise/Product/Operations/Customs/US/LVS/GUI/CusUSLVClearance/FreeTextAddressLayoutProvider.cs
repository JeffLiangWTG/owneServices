using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.LVS.GUI;

public class FreeTextAddressGridLayoutProvider : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.WaybillNumber, 200, info => { info.CaptionResourceString = Res.GetData("67b8579c-c28a-4998-8d7c-1561ca2b0dcf", "House Bill"); });
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.PartyName, 160);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.Address1, 200);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.Address2, 160);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.City, 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.State, 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.Postcode, 140);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(FreeTextAddress<CusUSLVConsignment>.Schema.Country, 140);
		return builder.Build();
	}
}

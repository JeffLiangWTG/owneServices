using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>))]
sealed class ImportAdditionalDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ImportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>, BaseMessageSendingObject, ImportAdditionalDetailsControlBag>
{
	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	protected override int ExpectedMaxColumns => 1;

	protected override ImportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject> GetColumnLayoutBuilderForTesting()
	{
		var builder = new ImportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>();
		builder.AddControlBag(ImportAdditionalDetailsControlBag.Instance);
		return builder;
	}
}

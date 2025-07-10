using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(EntryHeaderController))]
	sealed class EntryHeaderControllerBaseOnlyTest : EntryHeaderControllerTest
	{
		protected override CusEntryHeader GetNewEntryHeader()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration.CustomsEntryHeaders.AddNew();
		}

		protected override Type ExpectedFormType => typeof(BaseJobDeclarationForm);
	}
}

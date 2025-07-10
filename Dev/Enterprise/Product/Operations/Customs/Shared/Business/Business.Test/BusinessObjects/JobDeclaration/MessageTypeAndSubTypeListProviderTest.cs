using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(MessageTypeAndSubTypeListProvider))]
	public abstract class MessageTypeAndSubTypeListProviderTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			var list = (CodeDescriptionPairList)Provider.MessageTypeList(Factory, GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("List matches", string.Join(",", ExpectedMessageTypeList), string.Join(",", list.GetAllCodes()));
		}

		protected virtual string[] ExpectedMessageTypeList => Factory.New<BaseJobDeclaration>().Lookups.MessageTypeList.GetAllCodes();

		public void TestMessageSubTypeList()
		{
			var messageTypes = ((CodeDescriptionPairList)Provider.MessageTypeList(Factory, GlbCompany.CurrentCompany.GC_Code)).GetAllCodes();
			var declaration = Factory.New<BaseJobDeclaration>();
			foreach (var messageType in messageTypes)
			{
				declaration.JE_MessageType = messageType;
				var expectedMessageSubTypes = ExpectedMessageSubTypeListFor(declaration);
				AssertEquals($"Sub message list matches for message type {messageType}", string.Join(",", expectedMessageSubTypes), string.Join(",", ((CodeDescriptionPairList)Provider.MessageSubTypeList(Factory, GlbCompany.CurrentCompany.GC_Code, messageType)).GetAllCodesZString()));
			}
		}

		protected virtual string[] ExpectedMessageSubTypeListFor(BaseJobDeclaration declaration)
		{
			return declaration.Lookups.MessageSubTypeList.GetAllCodes();
		}

		MessageTypeAndSubTypeListProvider Provider =>
			(MessageTypeAndSubTypeListProvider)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
	}
}

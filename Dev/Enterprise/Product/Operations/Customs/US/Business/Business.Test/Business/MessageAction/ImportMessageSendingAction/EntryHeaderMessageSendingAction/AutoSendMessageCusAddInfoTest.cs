using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AutoSendMessageCusAddInfo))]
	sealed class AutoSendMessageCusAddInfoTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<AutoSendMessageCusAddInfo>
	{
		public void TestLoader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var sendMessageCusAddInfo = Factory.New<AutoSendMessageCusAddInfo>();
			sendMessageCusAddInfo.B7_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
			sendMessageCusAddInfo.B7_ParentID = entry.PK;
			sendMessageCusAddInfo.B7_AddInfoData = "Some explanation";
			Factory.Save();
			var cusAddInfo = new AutoSendMessageCusAddInfo.Loader(Factory).Load(entry);
			AssertNotNull("Found data", cusAddInfo);
			AssertEquals("Explanation", "Some explanation", cusAddInfo.B7_AddInfoData);
		}

		protected override IEnumerable<AutoSendMessageCusAddInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var sendMessageCusAddInfo = factory.New<AutoSendMessageCusAddInfo>();
			sendMessageCusAddInfo.B7_ParentTableCode = entry.TablePrefix;
			sendMessageCusAddInfo.B7_ParentID = entry.PK;
			sendMessageCusAddInfo.B7_AddInfoData = "Some explanation";
			yield return sendMessageCusAddInfo;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<AutoSendMessageCusAddInfo>();
	}
}

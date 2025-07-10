using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AvailableEDocListTest : Customs.Business.Testing.AvailableEDocListTest
	{
		public override void TestList()
		{
			var list = GetAvailableEdocsList(GetBizOFactory());
			var completeList = new ZStringBuilder();
			foreach (ICodeDescription data in list)
			{
				completeList.Append(data.Code + " | " + data.Description);
				AssertNotEquals("element.PK", ZGuid.Empty, data.PK);
			}

			const string expectedCompleteList = @"
Declaration - AAA-File1.PDF | Added: 13-Dec-08 - Landed Fish
Declaration - DDD-File4.PDF | Added: 16-Dec-08 - Crab Sticks
";
			AssertMultilineASCIIEquals("Complete List", expectedCompleteList.Trim(), completeList.ToStringWithNewLineBetweenAppends());
		}

		public override void TestListFromMultipleCollection()
		{
			var list = GetMultipleAvailableEdocsList(GetBizOFactory());
			var completeList = new ZStringBuilder();
			foreach (ICodeDescription data in list)
			{
				completeList.Append(data.Code + " | " + data.Description);
				AssertNotEquals("element.PK", ZGuid.Empty, data.PK);
			}

			const string expectedCompleteList = @"
Declaration - AAA-File1.PDF | Added: 13-Dec-08 - Landed Fish
Declaration - DDD-File4.PDF | Added: 16-Dec-08 - Crab Sticks
Shipment - AAA-File21.PDF | Added: 13-Dec-08 - Landed Fish
Shipment - DDD-File24.PDF | Added: 16-Dec-08 - Crab Sticks
";
			AssertMultilineASCIIEquals("Complete List", expectedCompleteList.Trim(), completeList.ToStringWithNewLineBetweenAppends());
		}

		protected override List<ZString> ExtensionFilterForTest => new List<ZString> { Core.Constants.FileFormats.PDF };
	}
}

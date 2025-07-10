
namespace Enterprise.Customs.TW.Business
{
	public class BillTypeList : Customs.Business.BillTypeList
	{
		public new static class Codes
		{
			public const string ContainerNote = "CN";
		}

		public new static class Descriptions
		{
			public static string ContainerNote
			{
				get { return Res.GetString("E33F0732-FAFC-4155-9730-8180D6AAAC8D", "Container Note"); }
			}
		}

		public BillTypeList()
		{
			AddPair(Codes.ContainerNote, Descriptions.ContainerNote);
		}
	}
}

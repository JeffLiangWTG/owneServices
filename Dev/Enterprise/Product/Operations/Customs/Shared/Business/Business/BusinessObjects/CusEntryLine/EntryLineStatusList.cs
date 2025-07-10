using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class EntryLineStatusList : CodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
		public class Codes
		{
			public const string Active = "ACT";
			public const string DeletePending = "DPD";
			public const string Deleted = "DLT";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
		public class Descriptions
		{
			public static string Active
			{
				get { return Res.GetString("af3afaea-8138-4db1-a838-5b29b0132ddb", "Active"); }
			}
			public static string DeletePending
			{
				get { return Res.GetString("28778db4-2580-43cb-bf29-13ed971d056c", "Deletion Pending"); }
			}
			public static string Deleted
			{
				get { return Res.GetString("55cbfeb0-a858-4ff4-a9f0-47d30f8860fe", "Deleted"); }
			}
		}

		public EntryLineStatusList()
		{
			AddPair(Codes.Active, Descriptions.Active);
			AddPair(Codes.DeletePending, Descriptions.DeletePending);
			AddPair(Codes.Deleted, Descriptions.Deleted);
		}
	}
}

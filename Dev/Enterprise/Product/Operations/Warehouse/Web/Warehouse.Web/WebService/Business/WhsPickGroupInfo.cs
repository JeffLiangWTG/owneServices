using System;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsPickGroupInfo : DataObjectInfo
	{
		#region Constructors

		public WhsPickGroupInfo()
			: this(0, "")
		{
		}

		public WhsPickGroupInfo(PickGroup pickGroup)
			: this(pickGroup.PickSequence, pickGroup.Description)
		{
		}

		WhsPickGroupInfo(short pickSequence, string description)
		{
			PickSequence = pickSequence;
			Description = description;
		}

		#endregion

		#region Properties

		public short PickSequence { get; set; }
		public string Description { get; set; }

		#endregion

		internal static WhsPickGroupInfo GetDefaultPickGroup()
			=> new WhsPickGroupInfo
			{
				PickSequence = 0,
				Description = Res.GetString("c8d033a4-ea5e-4a87-af43-fc859a6ed209", "ANY")
			};
	}
}

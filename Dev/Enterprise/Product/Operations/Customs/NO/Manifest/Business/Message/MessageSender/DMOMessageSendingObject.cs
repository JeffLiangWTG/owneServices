using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
namespace Enterprise.Customs.NO.Manifest.Business;

public abstract class DMOMessageSendingObject : BaseMessageSendingObject
{
	public DMOMessageSendingObject(BusinessObjectFactory factory) : base(factory)
	{
	}

	public static class Schema
	{
		public const string ShouldSend = nameof(DMOMessageSendingObject.ShouldSend);
		public const string CustomsLevel = nameof(DMOMessageSendingObject.CustomsLevel);
		public const string BillNumber = nameof(DMOMessageSendingObject.BillNumber);
		public const string Representative = nameof(DMOMessageSendingObject.Representative);
		public const string Consignee = nameof(DMOMessageSendingObject.Consignee);
	}

	#region Properties

	#region CustomsLevel

	[ResourceStringData("A59C7244-2756-49B3-AFA3-3CAF46DFE272", Caption = "Customs Level")]
	public abstract ZString CustomsLevel { get; }

	#endregion

	#region BillNumber

	[ResourceStringData("61CE023A-C344-4E07-969C-0A5842C58882", Caption = "Bill Number")]
	public abstract ZString BillNumber { get; }

	#endregion

	#region Representative

	[ResourceStringData("A86A703C-AFC8-4920-9E20-B8096974AA00", Caption = "Representative")]
	public abstract ZString Representative { get; }

	#endregion

	#region Consignee

	[ResourceStringData("0EF5D451-19A1-43E4-A712-7F7D12F07956", Caption = "Consignee")]
	public abstract ZString Consignee { get; }

	#endregion

	#endregion
}

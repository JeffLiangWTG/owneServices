using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.NO.Manifest.Business;

public sealed class DMOMessageSendingObjectParent : BaseMessageSendingObjectParent<DMOMessageSendingObject>
{
	public DMOMessageSendingObjectParent(AsycudaManifestHeader manifestHeader) : base(manifestHeader?.Factory)
	{
		this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
	}

	public override BusinessObject TopLevelBusinessObject => manifestHeader;

	public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.GlobalManifestSendWithMessageErrors;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			yield return new MessageSendingObjectProperty(DMOMessageSendingObject.Schema.ShouldSend, true, 100);
			yield return new MessageSendingObjectProperty(DMOMessageSendingObject.Schema.CustomsLevel, true, 200);
			yield return new MessageSendingObjectProperty(DMOMessageSendingObject.Schema.BillNumber, true, 200);
			yield return new MessageSendingObjectProperty(DMOMessageSendingObject.Schema.Representative, true, 200);
			yield return new MessageSendingObjectProperty(DMOMessageSendingObject.Schema.Consignee, true, 200);
		}
	}

	protected override NonPersistentBusinessObjectCollection<DMOMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectsCollection = new DMOMessageSendingObjectCollection(Factory) {
			new DMOManifestHeaderMessageSendingObject(manifestHeader),
			new DMOBillMessageSendingObject(manifestHeader.MasterBill)
		};

		manifestHeader.Bills.ForEach(bill => sendingObjectsCollection.Add(new DMOBillMessageSendingObject(bill)));

		RegisterEditableChildObject(sendingObjectsCollection);
		return sendingObjectsCollection;
	}

	public bool CreateAndSaveMessage() => false;

	readonly AsycudaManifestHeader manifestHeader;
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CargoManifestStatusQueryHeaderObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CargoManifestStatusQueryHeaderObject(ICargoManifestStatusQueryHeader header, IAutoQueryFilter filter = null)
			: base(header.Factory)
		{
			this.Header = header;
			this.filter = filter;
		}
		public readonly ICargoManifestStatusQueryHeader Header;
		public bool ShouldSendMessage;
		readonly IAutoQueryFilter filter;

		#region Action

		[CargoWise.ComponentModel.MaxLength(3)]
		[CargoWise.ComponentModel.List(nameof(ActionList))]
		public ZString ActionCode
		{
			get { return actionCode; }
			set
			{
				bool hasChanged = actionCode != value;
				CheckMaximumLength(ActionCodeInfo, value);
				SetNonPersistentPropertyValue(ActionCodeInfo, ref actionCode, value);

				if (hasChanged)
				{
					SendingObjects.PopulateObjects(ActionCode, filter);

					if (SendingObjects.Count == 1)
					{
						SendingObjects[0].ShouldSendMessage = true;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateActionCode();
				}
			}
		}
		ZString actionCode;

		public ZPropertyInfo ActionCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ActionCode)); }
		}

		public void ValidateActionCode()
		{
			if (!IsValidationSuspended)
			{
				ActionCodeInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(ActionCodeInfo, ActionList);

				if (ActionCode.IsEmpty)
				{
					ActionCodeInfo.AddError(EmptyActionCode);
				}
			}
		}
		internal const string EmptyActionCode = "Please enter an Action code.";

		public CodeDescriptionPairList ActionList
		{
			get
			{
				if (actionList == null)
				{
					actionList = Header.ActionList;
				}
				return actionList;
			}
		}
		CodeDescriptionPairList actionList;

		#endregion

		[ChildEditable(true)]
		public CargoManifestQuerySendingObjectCollection SendingObjects
		{
			get
			{
				if (sendingObjects == null)
				{
					sendingObjects = new CargoManifestQuerySendingObjectCollection(this);
					RegisterEditableChildObject(sendingObjects);
				}
				return sendingObjects;
			}
		}
		CargoManifestQuerySendingObjectCollection sendingObjects;

		CargoManifestQuerySendingObject[] GetObjectsMarkedForSending()
		{
			return SendingObjects.OfType<CargoManifestQuerySendingObject>().Where(x => x.ShouldSendMessage).ToArray();
		}

		public bool HasObjectsMarkedForSending
		{
			get { return GetObjectsMarkedForSending().Length > 0; }
		}

		public ZInt SendQueryMessage()
		{
			var sendingObjs = GetObjectsMarkedForSending();
			foreach (CargoManifestQuerySendingObject sendingObj in sendingObjs)
			{
				new CargoManifestStatusQueryMessageBuilder(Header, sendingObj).GenerateMessages();
			}

			return sendingObjs.Length;
		}

		public ZInt PopulateAndSendQueryMessages(ZString code, ZBool updateEntryWithResults, ZString limitOutputOption, bool requestForReleatedBOL = false)
		{
			ActionCode = code;

			foreach (CargoManifestQuerySendingObject sendingObj in SendingObjects)
			{
				sendingObj.RequestForRelatedBOL = filter?.RequestForRelatedBOL ?? requestForReleatedBOL;
				sendingObj.ShouldSendMessage = true;
				sendingObj.UpdateEntryWithResults = updateEntryWithResults;
				sendingObj.LimitOutputOption = limitOutputOption;
			}

			return SendQueryMessage();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateActionCode();
		}
	}
}

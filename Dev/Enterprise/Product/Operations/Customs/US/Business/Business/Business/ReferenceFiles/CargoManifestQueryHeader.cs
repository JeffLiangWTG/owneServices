using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CargoManifestQueryHeader : NonPersistentBusinessObject
		, IObsoleteValidation
	{
		public CargoManifestQueryHeader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool IsIssuerVisible => ActionCode == CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
		public bool IsMasterBillNumberVisible => ActionCode == CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill || ActionCode == CargoManifestStatusQueryActionList.Codes.AIR;
		public bool IsHouseBillNumberVisible => ActionCode == CargoManifestStatusQueryActionList.Codes.AIR;
		public bool IsInBondNumberVisible => ActionCode == CargoManifestStatusQueryActionList.Codes.InBond;

		[ResourceStringData("Enterprise.Customs.US.Business.CargoManifestQueryHeader|IsGroupQueries", Caption = "Group Queries")]
		public ZBool IsGroupQueries
		{
			get { return isGroupQueries; }
			set { SetNonPersistentPropertyValue(IsGroupQueriesInfo, ref isGroupQueries, value); }
		}
		ZBool isGroupQueries;

		public ZPropertyInfo IsGroupQueriesInfo
		{
			get { return GetZPropertyInfo(nameof(IsGroupQueries)); }
		}

		#region Action

		[CargoWise.ComponentModel.MaxLength(3)]
		[CargoWise.ComponentModel.List(nameof(ActionCodeList))]
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
					SendingObjects.RemoveAndDeleteAll();
				}

				ValidateActionCode();
			}
		}
		ZString actionCode;

		public ZPropertyInfo ActionCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ActionCode)); }
		}

		public CodeDescriptionPairList ActionCodeList => Factory.GetCachedValue("USCargoManifestStatusQueryActionList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(CargoManifestStatusQueryActionList.Codes.AIR, CargoManifestStatusQueryActionList.Descriptions.AIR);
				result.AddPair(CargoManifestStatusQueryActionList.Codes.InBond, CargoManifestStatusQueryActionList.Descriptions.InBond);
				result.AddPair(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill, CargoManifestStatusQueryActionList.Descriptions.OceanRailTruckBill);
				return result;
			});

		void ValidateActionCode()
		{
			if (!IsValidationSuspended)
			{
				ActionCodeInfo.ClearAllNotifications();

				MandatoryValidation.CheckEntered(ActionCodeInfo, "Action Code");

				if (!ActionCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(ActionCodeInfo, ActionCodeList, "Action Code");
				}
			}
		}

		#endregion

		[ChildEditable(true)]
		public CargoManifestQueryBizObjCollection SendingObjects
		{
			get
			{
				if (sendingObjects == null)
				{
					sendingObjects = new CargoManifestQueryBizObjCollection(this);
					RegisterEditableChildObject(sendingObjects);
				}
				return sendingObjects;
			}
		}
		CargoManifestQueryBizObjCollection sendingObjects;

		#region Override

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateActionCode();
		}

		#endregion
	}
}

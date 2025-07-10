using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class CusSCAHouseWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("A9211AA5-9EC2-4970-9E83-5C7C902C2BE0", "Sea Cargo House");
		public override ControllerID ControllerID
		{
			get
			{
				ControllerID result;
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.Australia:
						result = ControllerIDs.Customs.AU.SeaCargoHouseController;
						break;
					default:
						result = null;
						break;
				}
				return result;
			}
		}

		public override Type WorkflowProviderType
		{
			get
			{
				switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					case Core.Constants.CountryCodes.Australia:
						return ObjectFactory.GetType<Integration.Customs.AU.ICusSCAHouse>();
					default:
						return typeof(BaseCusSCAHouse);
				}
			}
		}

		public override bool SupportsEventTracking => true;
		public override bool SupportsBufferManagement => false;
		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) => true;

		#region Requirements

		public override bool RequiresBranch => true;
		public override bool RequiresClient => true;
		public override bool RequiresDepartment => false;
		public override bool RequiresPort1 => true;
		public override bool RequiresPort2 => true;
		public override ZString Port1Name => Res.GetString("0e4cc709-e68e-4774-ac93-29888d639ae7", "Loading Port");
		public override ZString Port2Name => Res.GetString("0e7cf482-6e55-4ef2-9e0e-c6dc0d632a49", "Discharge Port");
		public override ZString ClientName => Res.GetString("d87c5c12-34fb-45f5-94e8-24be40085b3c", "Shipping Line");

		#endregion

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.OrgProxy;
	}
}

using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentCargoWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CarrierShipmentCargoWorkflowDescriptorCode;
		public override IMultilingualString Description => ResString.GetMultilingualString("28D0C493-B8E8-4B1F-944A-50D6A860ED54", "Ocean Carrier Cargo");
		public override ControllerID ControllerID => null; // Does not have Controller ID
		public override Type WorkflowProviderType => typeof(CarrierShipmentCargo);
		public override bool RequiresClient => false;
		public override bool RequiresBranch => true;
		public override bool RequiresDepartment => true;
		public override bool SupportsEventTracking => true;
		public override bool SupportsBufferManagement => false;
		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				return new[]
				{
					new ProcessTemplateSubType(Res.GetString("2E46A8D1-C9F4-46B8-9BAF-CD6106314F23", "Cargo Type"), new CodeDescriptionPairList(OLookUpEditType.OceanCarrierCargoTypes)),
					new ProcessTemplateSubType(GetSubType2Title(), GetSubType2List())
				};
			}
		}

		string GetSubType2Title()
		{
			var subTypeTitle = $"Sub Type";
			if (LastProcessTaskTemplate == null)
			{
				return subTypeTitle;
			}

			switch (LastProcessTaskTemplate.P0_SubType1)
			{
				case Core.Constants.OceanCarrierCargoTypes.Codes.RoRo:
					return Core.Constants.OceanCarrierCargoTypes.SubType1Label.RoRo;
				case Core.Constants.OceanCarrierCargoTypes.Codes.BreakBulk:
					return Core.Constants.OceanCarrierCargoTypes.SubType1Label.BreakBulk;
				case Core.Constants.OceanCarrierCargoTypes.Codes.Container:
					return Core.Constants.OceanCarrierCargoTypes.SubType1Label.Container;
				default:
					return subTypeTitle;
			}
		}

		CodeDescriptionPairList GetSubType2List()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("", Res.GetString("65E9F6D6-237F-4FD4-95F7-0FE8F1500F62", "All"));
			if (LastProcessTaskTemplate == null)
			{
				return result;
			}

			switch (LastProcessTaskTemplate.P0_SubType1)
			{
				case Core.Constants.OceanCarrierCargoTypes.Codes.Container:
					return new CodeDescriptionPairList(OLookUpEditType.ContainerType);
				case Core.Constants.OceanCarrierCargoTypes.Codes.RoRo:
					return new CodeDescriptionPairList(OLookUpEditType.RoRoTypes);
				case Core.Constants.OceanCarrierCargoTypes.Codes.BreakBulk:
					return PackTypeCodeList;
				default:
					return result;
			}
		}

		CodeDescriptionPairList PackTypeCodeList
		{
			get
			{
				if (fPackTypeList == null)
				{
					fPackTypeList = new CodeDescriptionPairList();

					RefPackTypeCollection packTypes = new RefPackTypeCollection(new BusinessObjectFactory(), true);
					packTypes.ApplySort(RefPackTypeSchema.F3_Code.Name, ListSortDirection.Ascending);

					foreach (RefPackType packType in packTypes)
					{
						fPackTypeList.AddPair(packType.F3_Code, packType.F3_DescriptionMultilingual);
					}
				}
				return fPackTypeList;
			}
		}

		CodeDescriptionPairList fPackTypeList;
	}
}

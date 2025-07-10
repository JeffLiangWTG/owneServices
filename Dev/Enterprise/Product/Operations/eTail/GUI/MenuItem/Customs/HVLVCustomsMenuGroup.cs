using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.eTail.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.GUI
{
	public class HVLVCustomsMenuGroup : BaseHVLVMenuGroup
	{
		public HVLVCustomsMenuGroup(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("29213a31-7037-4578-8ca7-cca0f0e5912e", "Customs"), shipment)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			MenuItems.Clear();

			AddMenuGroupsFromRelatedJobCommands();

			MenuItems.Add(new SendACASMenuGroup(shipment));
			MenuItems.Add(new USISFMenuGroup(shipment));
			MenuItems.Add(new CreateDA306MenuItem(shipment));
			MenuItems.Add(new DeclarationMenuGroup(shipment));
		}

		void AddMenuGroupsFromRelatedJobCommands()
		{
			var validCommands = GetNeededCommands();
			foreach (var command in validCommands)
			{
				var type = typeof(CommandJobTypeMenuGroup<>).MakeGenericType(command).GetConstructor(new Type[] { typeof(ForwardingShipment) });
				var commandMenuGroup = type.Invoke(new object[] { shipment }) as BaseHVLVMenuGroup;
				MenuItems.Add(commandMenuGroup);
			}
		}

		public override void UpdateVisibilityAndCaption()
		{
			var menuGroups = MenuItems.OfType<BaseHVLVMenuGroup>().Where(x => !x.GetType().IsGenericType);
			foreach (var group in menuGroups)
			{
				group.UpdateVisibilityAndCaption();
			}

			var menus = MenuItems.OfType<BaseHVLVMenuItem>().Where(x => !x.GetType().IsGenericType);
			foreach (var menu in menus)
			{
				menu.UpdateVisibilityAndCaption();
			}
		}

		protected override void OnPopup(EventArgs e)
		{
			BuildChildrenMenuItems();
			base.OnPopup(e);
		}

		IEnumerable<Type> GetNeededCommands()
		{
			var commandTypes = RelatedJobCommandTypes;

#if DEBUG
			if (Globals.IsTest)
			{
				bool IsRelatedJobCommand(Type type)
				{
					return !type.IsAbstract && typeof(BaseHVLVRelatedJobCommand).IsAssignableFrom(type);
				}

				var testingCommandTypes = AssemblyLoader.LoadAssembly("Enterprise.eTail.DataTransfer.Testing").GetTypes().Where(type => IsRelatedJobCommand(type));
				commandTypes = commandTypes.Concat(testingCommandTypes).ToArray();
			}
#endif

			return commandTypes.Where(type => BaseHVLVRelatedJobCommand.IsNeeded(type, shipment));
		}

		readonly Type[] RelatedJobCommandTypes =
		[
			typeof(AUAirCargoReportCommand),
			typeof(AUSeaCargoReportCommand),
			typeof(CustomsDeclarationCommand),
			typeof(EUICS2ManifestCommand),
			typeof(ESH7DeclarationCommand),
			typeof(FRH7DeclarationCommand),
			typeof(GBH7DeclarationCommand),
			typeof(IEH7DeclarationCommand),
			typeof(ITH7DeclarationCommand),
			typeof(NZAirCargoReportCommand),
			typeof(NZSeaCargoReportCommand),
			typeof(SGCargoReportCommand),
			typeof(TRETradeManifestCommand),
			typeof(TWBriefCustomsDeclarationCommand),
			typeof(TWForwarderManifestCommand),
			typeof(USAirAMSCommand),
			typeof(USRoadEManifestCommand),
			typeof(USSeaAMSCommand),
			typeof(USLowValueCommand),
		];
	}
}

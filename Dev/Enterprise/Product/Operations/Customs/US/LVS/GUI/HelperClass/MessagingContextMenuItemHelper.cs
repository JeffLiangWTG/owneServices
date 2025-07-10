using System;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public static class MessagingContextMenuItemHelper
	{
		public static ZMenuItem CreateMessagingContextMenuItems(EventHandler sendOriginalMessage, EventHandler sendReplacementMessage, EventHandler sendDeletionMessage)
		{
			var result = new ZMenuItem(ResString.GetMultilingualString("5fcfb66b-5a1b-42ef-899d-c305d3e91157", "&Messaging"));
			result.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("3f81c5cf-6a7d-4f6c-a778-224c00bac422", "Send Original Messages"), sendOriginalMessage));
			result.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("52311df4-7320-48b4-a827-6d75fb926542", "Send Replacement Messages"), sendReplacementMessage));
			result.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("71cb9b68-a2be-4782-a7de-7492518ffd21", "Send Deletion Messages"), sendDeletionMessage));
			return result;
		}
	}
}

using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.Business
{
	public sealed class UYCustomsDataRegistry : RegistryItemSet
	{
		#region Construction

		public static UYCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new UYCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static UYCustomsDataRegistry instance;

		public UYCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Uruguay { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("49FF4673-F26E-44E8-9D29-D5E600FE7340", "Uruguay")); } }

			public static MultilingualString Customs_Uruguay_Manifest { get { return CombineCategories(Customs_Uruguay, ResString.GetMultilingualString("1906FD4E-1EED-4F6F-B793-6F8AEA66F0A4", "Manifest")); } }
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem UYTestingSystem
		{
			get
			{
				return GetItem("IsUYTesting", delegate
				{
					return new BooleanRegistryItem(
						"IsUYTesting",
						Categories.Customs_Uruguay,
						ResString.GetMultilingualString("06FE1A69-317E-4554-A352-FD1BA8C60BE7", "Is Test Mode?"),
						ResString.GetMultilingualString("53204E3D-FC77-4E7B-A5B5-F7BE6DC2F5A2", "Should UY messages be sent to Test System rather than Production System?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public ZBool IsUYTestingSystem
		{
			get { return Instance.UYTestingSystem.Value; }
		}

		public ManifestGroupNotificationRegistryItem UYMANGroupNotification
		{
			get
			{
				return GetItem("UYMANGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"UYMANGroupNotification",
						Categories.Customs_Uruguay_Manifest,
						ResString.GetMultilingualString("5A5DC044-BCE0-449A-84C3-F329691F76CA", "Notification Group"),
						ResString.GetMultilingualString("ECA58D5E-A760-4F3F-8EAE-291D024685A8", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ManifestGroupNotification.Default);

					result.CountryFilterPKs = CountryFilterPKs.Uruguay;
					return result;
				}
				);
			}
		}
	}
}

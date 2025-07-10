using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataRegistry.Business
{
	public sealed class DeclarationEventLockInfoLookups : ZLookups
	{
		public DeclarationEventLockInfoLookups(DeclarationEventLockInfo parent, BusinessObjectFactory factory)
			: base(parent)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		public CodeDescriptionPairList EventTypeList
		{
			get
			{
				var isPWEnabled = ZArchitecture.Environment.DataRegistry.Instance.ProductivityWiseModeEnabled;
				return factory.GetCachedValue<CodeDescriptionPairList>($"DeclarationEventLockInfoLookups_EventTypeList_{isPWEnabled}",
					() =>
					{
						var eventTypeList = new StmEventCodeDescriptionPairList();
						eventTypeList.RemoveCode(AutoEvents.AddedARecordToTheSystemCode);
						eventTypeList.RemoveCode(AutoEvents.EditedARecordCode);
						eventTypeList.RemoveCode(AutoEvents.DeletedARecordInTheSystemCode);
						eventTypeList.RemoveCode(AutoEvents.LockForEditCode);
						eventTypeList.RemoveCode(AutoEvents.UnlockForEditCode);
						eventTypeList.RemoveCode(AutoEvents.StaffFlaggedAsDeviceOnlyCode);
						eventTypeList.RemoveCode(AutoEvents.StaffUnFlaggedAsDeviceOnlyCode);
						eventTypeList.RemoveCode(AutoEvents.UserSeatCode);

						return eventTypeList;
					});
			}
		}

		public CodeDescriptionPairList EventSourceList
		{
			get
			{
				if (eventSourceList == null)
				{
					eventSourceList = new CodeDescriptionPairList();
					switch (((DeclarationEventLockInfo)Parent).LockConfig?.DeclarationType)
					{
						case "ARN":
						case "DEP":
						case "ULR":
							eventSourceList.AddPair(Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, Constants.Customs.EventLockSourceTypes.Descriptions.NctsHeader);
							break;
						case Constants.Customs.EventLockSourceTypes.Codes.TemporaryStorage:
							eventSourceList.AddPair(Constants.Customs.EventLockSourceTypes.Codes.TemporaryStorage, Constants.Customs.EventLockSourceTypes.Descriptions.TemporaryStorage);
							break;
						default:
							eventSourceList.AddPair(Constants.Customs.EventLockSourceTypes.Codes.Declaration, Constants.Customs.EventLockSourceTypes.Descriptions.Declaration);
							eventSourceList.AddPair(Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, Constants.Customs.EventLockSourceTypes.Descriptions.EntryHeader);
							break;
					}
				}

				return eventSourceList;
			}
		}
		CodeDescriptionPairList eventSourceList;

		public CodeDescriptionPairList EntryTypeList
		{
			get
			{
				if (entryTypeList == null)
				{
					entryTypeList = new CodeDescriptionPairList();
					entryTypeList.AddPair(Constants.Customs.EntryHeaderTypes.Codes.All, Constants.Customs.EntryHeaderTypes.Descriptions.All);

					var parent = (DeclarationEventLockInfo)Parent;
					var countryCode = parent.GetCompanyCountry();
					var customsCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);

					Integration.Customs.ICusEntryHeaderTypeListProvider listProvider = null;

					switch (customsCountryCode)
					{
						case Core.Constants.CountryCodes.UnitedStates:
							{
								listProvider = ObjectFactory.Get<Integration.Customs.IUSCusEntryHeaderTypeListProvider>();
								break;
							}

						case Core.Constants.CountryCodes.Canada:
							{
								listProvider = ObjectFactory.Get<Integration.Customs.ICACusEntryHeaderTypeListProvider>();
								break;
							}
					}

					var entryTypes = listProvider?.GetEntryTypes();
					if (entryTypes != null)
					{
						entryTypeList.AddRange(entryTypes);
					}
				}

				return entryTypeList;
			}
		}
		CodeDescriptionPairList entryTypeList;
	}
}

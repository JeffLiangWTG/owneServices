using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IRefContainerMapProvider
	{
		UsageRequirement IsUsageNeeded { get; }
		ICodeDescriptionPairList GetUsageList(BusinessObjectFactory factory);
		ICodeDescriptionPairList GetCodeList(BusinessObjectFactory factory, ZString usage);
		ZString GetDefaultCustomsCode(BusinessObjectFactory factory, ZString containerType);
	}

	public enum UsageRequirement
	{
		Require,
		MayRequire,
		NotRequire
	}

	public class RefContainerCodeMap : AutoRefContainerCodeMap
	{
		public RefContainerCodeMap(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public RefContainer RefContainer => Factory.Load<RefContainer>(RCM_RC_Container);

		[List("Lookups.UsageList")]
		[ReadOnlyMember(nameof(RCM_UsageReadonly))]
		public override ZString RCM_Usage
		{
			get => base.RCM_Usage;
			set => base.RCM_Usage = value;
		}
		bool RCM_UsageReadonly => ContainerMapProvider == null || ContainerMapProvider.IsUsageNeeded == UsageRequirement.NotRequire;

		[List("Lookups.CodeList")]
		public override ZString RCM_Code
		{
			get => base.RCM_Code;
			set => base.RCM_Code = value;
		}

		public ZString RCM_Description => Lookups.CodeList.GetDescriptionFromCode(RCM_Code);

		public override ZString RCM_RN_NKCountry
		{
			get => base.RCM_RN_NKCountry;
			set
			{
				var oldValue = base.RCM_RN_NKCountry;
				base.RCM_RN_NKCountry = value;
				if (!IsCopying && RCM_RN_NKCountry != oldValue && !IsValidationSuspended)
				{
					RefreshUsageAndCode();

					var containerCode = RefContainer?.RC_Code ?? ZString.Empty;
					if (!RCM_RN_NKCountry.IsEmpty && !containerCode.IsEmpty)
					{
						var defaultCustomsCode = ContainerMapProvider?.GetDefaultCustomsCode(Factory, containerCode) ?? ZString.Empty;
						if (!defaultCustomsCode.IsEmpty)
						{
							RCM_Code = defaultCustomsCode;
						}
					}
				}
			}
		}

		void RefreshUsageAndCode()
		{
			RCM_Usage = ZString.Empty;
			RCM_Code = ZString.Empty;
		}

		public IRefContainerMapProvider ContainerMapProvider
		{
			get
			{
				var providers = ObjectFactory.Get<Hashtable>("IRefContainerMapProvider");
				var objectHandle = (ObjectHandle)providers[RCM_RN_NKCountry.ToString()];
				return objectHandle?.GetObject() as IRefContainerMapProvider;
			}
		}
	}
}

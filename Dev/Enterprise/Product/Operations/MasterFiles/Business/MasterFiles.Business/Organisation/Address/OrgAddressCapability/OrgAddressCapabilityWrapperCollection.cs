using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressCapabilityWrapperCollection : NonPersistentBusinessObjectCollection<OrgAddressCapabilityWrapper>
	{
		public OrgAddressCapabilityWrapperCollection(OrgAddress parentAddressCapability)
			: base(parentAddressCapability.Factory)
		{
			this.Master = parentAddressCapability;
			using (Master.SuspendSettingHasChanges())
			{
				CreateAllCapabilities();
				Sort();
			}
			HasChanges = false;
		}

		public readonly OrgAddress Master;

		void CreateAllCapabilities()
		{
			foreach (CodeDescriptionPair pair in OrgCodeLists.AddressType_List(Factory))
			{
				OrgAddressCapabilityWrapper capabilityWrapper = AddNew();
				using (capabilityWrapper.SuspendSettingHasChanges())
				{
					capabilityWrapper.AddressCapabilityType = pair.Code;
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrgAddressCapabilityWrapper(Master);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public ZString GetListOfCodes()
		{
			return string.Join(", ", EnabledCapabilities.Select(pair => pair.Code));
		}

		protected void Sort()
		{
			Sort("AddressCapabilityType", System.ComponentModel.ListSortDirection.Ascending);
		}

		public ZBool IsEmpty
		{
			get { return !this.Cast<OrgAddressCapabilityWrapper>().Any(wrapper => wrapper.Enabled); }
		}

		public IEnumerable<ICodeDescription> EnabledCapabilities
		{
			get
			{
				var types = OrgCodeLists.AddressType_List(Factory);
				return this.Cast<OrgAddressCapabilityWrapper>()
					.Where(wrapper => wrapper.Enabled)
					.Select(wrapper => types[wrapper.AddressCapabilityType, StringComparison.CurrentCulture]);
			}
		}

		public void DisableAllCapabilities()
		{
			foreach (OrgAddressCapabilityWrapper oAC in this)
			{
				oAC.Enabled = ZBool.False;
				oAC.Main = ZBool.False;
			}
		}

		#region Has Capability

		public void SetCapabilityEnabled(String code)
		{
			OrgAddressCapabilityWrapper oAC = GetAddressCapabilityOnCode(code);
			if (oAC != null)
			{
				oAC.Enabled = true;
			}
		}

		public void SetCapabilityDisabled(String code)
		{
			OrgAddressCapabilityWrapper oAC = GetAddressCapabilityOnCode(code);
			if (oAC != null)
			{
				oAC.Enabled = false;
			}
		}

		public ZBool GetCapabilityEnabled(String code)
		{
			OrgAddressCapabilityWrapper oAC = GetAddressCapabilityOnCode(code);
			return oAC == null ? ZBool.False : oAC.Enabled;
		}

		#endregion

		#region IsMainAddress

		public ZBool GetIsMainAddress(String code)
		{
			OrgAddressCapabilityWrapper oAC = GetAddressCapabilityOnCode(code);
			return oAC == null ? ZBool.False : oAC.Main;
		}

		public void SetIsMainAddress(String code)
		{
			OrgAddressCapabilityWrapper oAC = GetAddressCapabilityOnCode(code);
			if (oAC != null)
			{
				oAC.Main = true;
				oAC.ResetValidationCache(code);
			}
		}

		public void SetIsNotMainAddress(String code)
		{
			OrgAddressCapabilityWrapper oAC = GetAddressCapabilityOnCode(code);
			if (oAC != null)
			{
				oAC.Main = false;
				oAC.ResetValidationCache(code);
			}
		}

		#endregion

		#region Get AddressCapability Element On Code

		public OrgAddressCapabilityWrapper GetAddressCapabilityOnCode(ZString code)
		{
			return this.Cast<OrgAddressCapabilityWrapper>().FirstOrDefault(oAC => oAC.AddressCapabilityType == code);
		}

		#endregion

		public ZBool GetCapabilityEnabledMain(string code)
		{
			return this.Cast<OrgAddressCapabilityWrapper>().Any(oac => oac.AddressCapabilityType == code && oac.Enabled && oac.Main);
		}

		internal void ClearHasChanges()
		{
			foreach (OrgAddressCapabilityWrapper oAC in this)
			{
				if (oAC.Capability != null)
				{
					oAC.Capability.HasChanges = false;
				}

				oAC.HasChanges = false;
			}
			this.HasChanges = false;
		}
	}
}

using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Module
{
	public class EntryNumberModuleFilterValidation : ModuleTextFilterValidation
	{
		public EntryNumberModuleFilterValidation(EntryNumberModuleFilter parent)
			: base(parent) { }

		public void ValidateEntryType()
		{
			ValidateCalculatedProperty(Parent.EntryTypeInfo);
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test TestEntryType (EntryNumberModuleFilterValidationTest)")]
		void CheckEntryType()
		{
			if (Parent.EntryTypeList.Count > 0)
			{
				ListValidation.ErrorIfInvalidCode(Parent.EntryTypeInfo, Parent.EntryTypeList);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEntryType();
		}

		#region Implementation

		new EntryNumberModuleFilter Parent
		{
			get { return (EntryNumberModuleFilter)base.Parent; }
		}

		#endregion
	}
}

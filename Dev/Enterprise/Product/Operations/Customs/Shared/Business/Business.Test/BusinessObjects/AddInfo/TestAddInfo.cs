using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public class TestAddInfo : AutoTestAddInfo
	{
		#region Constructor

		public TestAddInfo(IAddInfoManager parent)
			: base(((BusinessObject)parent).Factory)
		{
			isInitialised = false;
			if (parent == null)
			{
				throw new ApplicationException("AddInfo requires a non null parameter in it's constructor");
			}
			this.Parent = (BusinessObject)parent;
			isInitialised = true;
		}

		public TestAddInfo(ZPropertyInfo addInfoProperty)
			: this((IAddInfoManager)addInfoProperty.BizObj)
		{
			this.AddInfoProperty = addInfoProperty;
			using (GetValidationSuspender())
			{
				LoadPropertiesFromString((ZString)addInfoProperty.Value);
			}
			HasChanges = false;
		}

		#endregion

		public override ZString UZ_String
		{
			get { return base.UZ_String; }
			set
			{
				base.UZ_String = value;
				WasMarkingAsNeedingValidationSuspendedInProperty = Parent.IsMarkingAsNeedingValidationSuspended;
			}
		}

		public ZString UZ_SomeProperty
		{
			get { return someProperty; }
			set
			{
				someProperty = value;
				UZ_SomePropertyInfo.RefreshBinding();
			}
		}
		ZString someProperty;

		public ZPropertyInfo UZ_SomePropertyInfo
		{
			get { return GetZPropertyInfo(nameof(UZ_SomeProperty)); }
		}

		public bool WasMarkingAsNeedingValidationSuspendedInProperty;

		protected override void AssignDefaultValueToPropertySimple(ZPropertyInfo propertyInfo)
		{
			base.AssignDefaultValueToPropertySimple(propertyInfo);
			WasMarkingAsNeedingValidationSuspendedInProperty = Parent.IsMarkingAsNeedingValidationSuspended;
		}

		protected override void AssignValueToPropertySimple(ZPropertyInfo propertyInfo, object inputValue)
		{
			base.AssignValueToPropertySimple(propertyInfo, inputValue);
			WasMarkingAsNeedingValidationSuspendedInProperty = Parent.IsMarkingAsNeedingValidationSuspended;
		}

		public SchemaColumn[] ColumnsForFastSearchExposed;

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get { return ColumnsForFastSearchExposed ?? Array.Empty<SchemaColumn>(); }
		}

		public BaseAddInfo DbAddInfoExposed => DbAddInfo;

		public Func<string, string> GetDBNameForTesting;
		protected override string GetDBName(string propertyName)
		{
			return GetDBNameForTesting == null ? base.GetDBName(propertyName) : GetDBNameForTesting(propertyName);
		}

		public void SetupEventsAndLoadValuesExposed(ZPropertyInfo addInfoProperty, bool isInitialisedStatus)
		{
			isInitialised = isInitialisedStatus;
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}

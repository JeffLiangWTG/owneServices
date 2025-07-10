using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business
{
	[SystemDefinedValues]
	public class CustomsNumberViewStmNumsSetting : NonPersistentBusinessObject
	{
		public CustomsNumberViewStmNumsSetting(ICustomsNumberViewStmNumsParent parent, ZString rangeType)
			: base(parent.Factory)
		{
			this.Parent = Argument.NotNull(parent, nameof(parent));
			this.RangeType = rangeType;
		}

		public readonly ICustomsNumberViewStmNumsParent Parent;
		public readonly ZString RangeType;

		public ZString GetPrefix()
		{
			return GetPrefixCore();
		}

		protected virtual ZString GetPrefixCore()
		{
			return ZString.Empty;
		}

		public ZLong? DefaultTypeRangeMax()
		{
			return DefaultTypeRangeMaxCore();
		}

		protected virtual ZLong? DefaultTypeRangeMaxCore()
		{
			return null;
		}

		public int RequiredDigit()
		{
			return RequiredDigitCore();
		}

		protected virtual int RequiredDigitCore()
		{
			return 8;
		}

		public ZLong GetThresholdRunOutWarning()
		{
			return GetThresholdRunOutWarningCore();
		}

		protected virtual ZLong GetThresholdRunOutWarningCore()
		{
			return ZLong.Zero;
		}

		public void DefaultDataOnSettingOwner(CustomsNumberViewStmNums stmNums) => DefaultDataOnSettingOwnerCore(stmNums);
		protected virtual void DefaultDataOnSettingOwnerCore(CustomsNumberViewStmNums stmNums) { }

		public bool CanRollover()
		{
			return CanRolloverCore();
		}

		protected virtual bool CanRolloverCore()
		{
			return false;
		}

		public bool AllowDuplicate()
		{
			return AllowDuplicateCore();
		}

		protected virtual bool AllowDuplicateCore()
		{
			return true;
		}

		public bool IsNumberUsed(CustomsNumberViewStmNums stmNums, ZString number)
		{
			return IsNumberUsedCore(stmNums, number);
		}

		protected virtual bool IsNumberUsedCore(CustomsNumberViewStmNums stmNums, ZString number)
		{
			return false;
		}

		public ZString GenerateCustomsNumber(CustomsNumberViewStmNums stmNums, ZString number)
		{
			return GenerateCustomsNumberCore(stmNums, number);
		}

		protected virtual ZString GenerateCustomsNumberCore(CustomsNumberViewStmNums stmNums, ZString number)
		{
			return number;
		}

		public override bool IsInDatabase => true;
		public override string TablePrefix => Parent.TablePrefix;
		public override string TableName => Parent.TableName;

		protected override void AddToFactoryCache()
		{
			// should be called after parent is set
		}

		protected override ZGuid GetPK()
		{
			return Parent.PK;
		}

		protected override bool SupportsCloneCore() => false;
	}
}

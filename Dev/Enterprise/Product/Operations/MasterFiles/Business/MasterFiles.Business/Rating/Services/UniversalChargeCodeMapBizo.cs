using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Rating.Services
{
	/// <summary>
	/// Business object for mapping universal code to global charge code or charge code.
	/// Used for binding.
	/// </summary>
	public sealed class UniversalChargeCodeMapBizo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UniversalChargeCodeMapBizo(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UniversalChargeCodeMapBizo(UniversalChargeCodeMapBizoCollection universalChargeCodeMapBizoCollection, string universalCode, string description)
			: this(universalChargeCodeMapBizoCollection.Factory)
		{
			Argument.NotNull(universalChargeCodeMapBizoCollection, nameof(universalChargeCodeMapBizoCollection));
			Argument.NotNull(universalCode, nameof(universalCode));
			Argument.NotNull(description, nameof(description));

			this.universalChargeCodeMapBizoCollection = universalChargeCodeMapBizoCollection;
			Code = universalCode;
			Description = description;
		}
		readonly UniversalChargeCodeMapBizoCollection universalChargeCodeMapBizoCollection;

		public ZString Code { get; }
		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		public ZString Description { get; }
		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		[List("GlobalChargeCodeList")]
		public ZGuid GlobalChargeCodePk
		{
			get => globalChargeCodePk;
			set
			{
				ZGuid prevValue = globalChargeCodePk;
				SetNonPersistentPropertyValue(GlobalChargeCodePkInfo, ref globalChargeCodePk, value);
				if (!IsValidationSuspended)
				{
					ValidateGlobalChargeCodePk();
					foreach (var other in universalChargeCodeMapBizoCollection.Cast<UniversalChargeCodeMapBizo>().Where(x => x != this))
					{
						var otherPk = other.GlobalChargeCodePk;
						if (!otherPk.IsEmpty && (otherPk == prevValue || otherPk == globalChargeCodePk))
						{
							other.ValidateGlobalChargeCodePk();
						}
					}
					ValidateLocalChargeCodePk();
				}
			}
		}
		ZGuid globalChargeCodePk;
		public ZPropertyInfo GlobalChargeCodePkInfo => GetZPropertyInfo(nameof(GlobalChargeCodePk));
		public AccChargeCode GlobalChargeCode => Factory.Load<AccChargeCode>(globalChargeCodePk);

		void ValidateGlobalChargeCodePk()
		{
			var info = GlobalChargeCodePkInfo;
			info.ClearAllNotifications();
			if (globalChargeCodePk.IsEmpty)
			{
				if (localChargeCodePk.IsEmpty)
				{
					info.AddError(ChargeCodeNotMappedMsg);
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(info, GlobalChargeCodeList);
			}
		}

		public AccGlobalChargeCodeCollection GlobalChargeCodeList
		{
			get => globalChargeCodeList ?? (globalChargeCodeList = new UnmappedGlobalChargeCodeCollection(Factory));
		}
		AccGlobalChargeCodeCollection globalChargeCodeList;

		[List("LocalChargeCodeList")]
		public ZGuid LocalChargeCodePk
		{
			get => localChargeCodePk;
			set
			{
				ZGuid prevValue = localChargeCodePk;
				SetNonPersistentPropertyValue(LocalChargeCodePkInfo, ref localChargeCodePk, value);
				if (!IsValidationSuspended)
				{
					ValidateLocalChargeCodePk();
					foreach (var other in universalChargeCodeMapBizoCollection.Cast<UniversalChargeCodeMapBizo>().Where(x => x != this))
					{
						var otherPk = other.LocalChargeCodePk;
						if (!otherPk.IsEmpty && (otherPk == prevValue || otherPk == localChargeCodePk))
						{
							other.ValidateLocalChargeCodePk();
						}
					}
					ValidateGlobalChargeCodePk();
				}
			}
		}
		ZGuid localChargeCodePk;
		public ZPropertyInfo LocalChargeCodePkInfo => GetZPropertyInfo(nameof(LocalChargeCodePk));
		public AccChargeCode LocalChargeCode => Factory.Load<AccChargeCode>(localChargeCodePk);

		void ValidateLocalChargeCodePk()
		{
			var info = LocalChargeCodePkInfo;
			info.ClearAllNotifications();
			if (localChargeCodePk.IsEmpty)
			{
				if (globalChargeCodePk.IsEmpty)
				{
					info.AddError(ChargeCodeNotMappedMsg);
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(info, LocalChargeCodeList);
			}
		}

		static string ChargeCodeNotMappedMsg
			=> Res.GetString("CBC8DF77-9925-4A7C-939A-66401205FBCC", "Please enter a Global Charge Code or Charge Code");

		public AccChargeCodeCollection LocalChargeCodeList
		{
			get => localChargeCodeList ?? (localChargeCodeList = new UnmappedChargeCodeCollection(Factory));
		}
		AccChargeCodeCollection localChargeCodeList;

		protected override void RunPreSaveValidationCore()
		{
			ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public void ValidateAll()
		{
			ValidateLocalChargeCodePk();
			ValidateGlobalChargeCodePk();
		}

		public void ApplyUniversalCodeToChargeCodeForSaving()
		{
			var global = GlobalChargeCode;
			if (global != null)
			{
				var mapping = global.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = Code;
			}
			var local = LocalChargeCode;
			if (local != null)
			{
				var mapping = local.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = Code;
			}
		}
	}

	public class UnmappedGlobalChargeCodeCollection : AccGlobalChargeCodeCollection
	{
		public UnmappedGlobalChargeCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
			var filterDefaults = FilterBusinessObjectDefaults;
			filterDefaults.Add(new FilterBusinessObjectDefault("Universal Charge Code", ModuleTextFilter.ComparisonOperator, new ZString(ModuleTextFilter.IsBlank)));
		}
	}

	public class UnmappedChargeCodeCollection : AccChargeCodeCollection
	{
		public UnmappedChargeCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
			var filterDefaults = FilterBusinessObjectDefaults;
			filterDefaults.Add(new FilterBusinessObjectDefault("Universal Charge Code", ModuleTextFilter.ComparisonOperator, new ZString(ModuleTextFilter.IsBlank)));
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded")]
	static class ModuleTextFilter
	{
		public const string ComparisonOperator = "ComparisonOperator";
		public const string IsBlank = "is blank";
	}
}

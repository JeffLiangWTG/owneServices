using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO
{
	class UNLCOIdentifierModuleFilter : ModuleFlagsFilter
	{
		#region Constructor

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "valid unit test failure")]
		public UNLCOIdentifierModuleFilter() : base("UNLOCO Identifiers",
					new string[] {
							ResString.GetMultilingualString("35A80CDB-B706-4CAE-8A17-8E669A5DA1E6", "Airport"),
							ResString.GetMultilingualString("AFE09ACB-15C0-47AC-B9E7-B8A55F29FCDF", "Rail"),
							ResString.GetMultilingualString("A38BF5D3-E403-4C1C-B2DA-9A25E20B670D","Road"),
							ResString.GetMultilingualString("0150AA2B-9F55-4935-9491-53D59F701CBB","Seaport"),
							ResString.GetMultilingualString("1ED58180-3BDE-4852-8B12-8E4A81D93B86","Terminal"),
							ResString.GetMultilingualString("C9B035FD-5FEC-4B8D-832A-85285A847C41","Outport"),
							ResString.GetMultilingualString("4B46045F-39C2-4476-9F94-7DE85EB74A1D","Discharge"),
							ResString.GetMultilingualString("8B0D3A8E-ADEC-4883-B2B0-EA32A429C9F7","Unload"),
							ResString.GetMultilingualString("87852D9F-D111-4592-9B4B-E43E539174C7","Store"),
							ResString.GetMultilingualString("6795AC9D-592F-4188-8322-BBCF72ADD796","Post"),
							ResString.GetMultilingualString("7FEE10B7-B874-43FE-9B50-EA4CFDF2FC00","Customs Lodge"),
							ResString.GetMultilingualString("D20D8689-5F54-4D5C-9E24-B57927389633","Border Crossing") },
					new SchemaBoolColumn[] {
							RefUNLOCOSchema.RL_HasAirport,
							RefUNLOCOSchema.RL_HasRail,
							RefUNLOCOSchema.RL_HasRoad,
							RefUNLOCOSchema.RL_HasSeaport,
							RefUNLOCOSchema.RL_HasTerminal,
							RefUNLOCOSchema.RL_HasOutport,
							RefUNLOCOSchema.RL_HasDischarge,
							RefUNLOCOSchema.RL_HasUnload,
							RefUNLOCOSchema.RL_HasStore,
							RefUNLOCOSchema.RL_HasPost,
							RefUNLOCOSchema.RL_HasCustomsLodge,
							RefUNLOCOSchema.RL_HasBorderCrossing })
		{
		}

		protected override bool ShouldCheckMaximumFlags
		{
			get { return false; }
		}

		protected override bool ShouldCheckFlagAmountEqualsDelegateAmount
		{
			get { return false; }
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("AndJoinCondition", AndJoinCondition.ToString());
			writer.WriteElementString("OrJoinCondition", OrJoinCondition.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "AndJoinCondition")
			{
				ZBool valueAnd = new ZBool(reader.ReadElementString("AndJoinCondition"));
				AndJoinCondition = valueAnd;
			}

			if (reader.Name == "OrJoinCondition")
			{
				ZBool valueOr = new ZBool(reader.ReadElementString("OrJoinCondition"));
				OrJoinCondition = valueOr;
			}
		}
		#endregion

		#region Property10

		public ZBool Property10
		{
			get { return property10; }
			set
			{
				if (property10 != value)
				{
					SetNonPersistentPropertyValue(Property10Info, ref property10, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty10();
					}

					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property10Info
		{
			get { return GetZPropertyInfo(nameof(Property10)); }
		}

		ZBool property10;

		#endregion

		#region Property11

		public ZBool Property11
		{
			get { return property11; }
			set
			{
				if (property11 != value)
				{
					SetNonPersistentPropertyValue(Property11Info, ref property11, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty11();
					}

					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property11Info
		{
			get { return GetZPropertyInfo(nameof(Property11)); }
		}

		ZBool property11;

		#endregion

		#region Property12

		public ZBool Property12
		{
			get { return property12; }
			set
			{
				if (property12 != value)
				{
					SetNonPersistentPropertyValue(Property12Info, ref property12, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty12();
					}

					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property12Info
		{
			get { return GetZPropertyInfo(nameof(Property12)); }
		}

		ZBool property12;

		#endregion

		#region Validation

		public new ModuleUNLCOIdentifierModuleFilterValidation Validation
		{
			get { return (ModuleUNLCOIdentifierModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleUNLCOIdentifierModuleFilterValidation(this);
		}

		#endregion
	}

	#region class Validation

	class ModuleUNLCOIdentifierModuleFilterValidation : ModuleFlagsFilterValidation
	{
		public ModuleUNLCOIdentifierModuleFilterValidation(UNLCOIdentifierModuleFilter parent)
			: base(parent)
		{
		}

		#region Validate Properties

		public void ValidateProperty10()
		{
			// run user delegate here
		}

		public void ValidateProperty11()
		{
			// run user delegate here
		}

		public void ValidateProperty12()
		{
			// run user delegate here
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateProperty10();
			ValidateProperty11();
			ValidateProperty12();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		#endregion
	}

	#endregion

}

using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Module
{
	public class ProcedureCodesModuleFilter : ModuleCodeFilter
	{
		public ProcedureCodesModuleFilter(ZString description, GetCodeQuery queryDelegate)
			: base(description, queryDelegate, DummyGetList, DummyGetList)
		{
			Property1Validation = info => { ListValidation.WarnIfInvalidCode(info); };
			Property2Validation = info => { ListValidation.WarnIfInvalidCode(info); };
		}

		#region override

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		public new BusinessObjectFactory Factory => factory ?? (factory = CreateNewFactory());
		BusinessObjectFactory factory;

		[MaxLength(3)]
		[List(nameof(CPCCodes))]
		public override ZString Property1
		{
			get { return base.Property1; }
			set { base.Property1 = value; }
		}

		[MaxLength(3)]
		[List(nameof(PPCCodes))]
		public override ZString Property2
		{
			get { return base.Property2; }
			set { base.Property2 = value; }
		}

		#endregion

		#region List

		public CodeDescriptionPairList CPCCodes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var cpcCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountry(Factory, Env.CurrentCompany?.Country?.Code ?? ZString.Empty, ZDateTime.Today);
				foreach (var cpc in cpcCollection)
				{
					result.AddPairIfNotExist(cpc.ZZ6_ProcedureCode, cpc.ZZ6_Description);
				}
				result.Sort();
				return result;
			}
		}

		public CodeDescriptionPairList PPCCodes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (!Property1.IsEmpty)
				{
					var ppcCollection = RefCusProcedureCollection.LoadPreviousProceduresCodesForCountryProcedureCode(Factory, Env.CurrentCompany?.Country?.Code ?? ZString.Empty, Property1, ZDateTime.Today);
					foreach (var cpc in ppcCollection)
					{
						result.AddPairIfNotExist(cpc.ZZ6_PreviousProcedureCode, cpc.ZZ6_Description);
					}
				}
				else
				{
					result = CPCCodes;
					if (result.Count > 0)
					{
						result.AddPairIfNotExist("00", "No Previous Procedure");
					}
				}
				result.Sort();
				return result;
			}
		}

		#endregion

		internal static GetList DummyGetList
		{
			get
			{
				return () => null;
			}
		}
	}
}

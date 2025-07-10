using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class ViewUNDGAttribute : AutoViewUNDGAttribute
	{
		public ViewUNDGAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ViewUNDGAttribute[] LoadFromSubstancePK(ZGuid substancePK)
			{
				var query = new ZQuery(ViewUNDGAttributeSchema.DA_DG, substancePK);
				return Factory.Load<ViewUNDGAttribute>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(AutoViewUNDGAttribute);
			}
		}

		#endregion

		#region ReadOnly

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			bool result = DA_Language == Core.Constants.Languages.English && UNDGSubstance != null && IsInDatabase && UNDGSubstance.DG_IsSystem && !DA_LanguageInfo.HasChanges;
			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsValidEnglishSubstance && IsInDatabase)
			{
				throw new CannotDeleteException("English details cannot be changed or deleted for system defined Dangerous Goods.");
			}
			else
			{
				base.Delete();
			}
		}

		#endregion

		public UNDGSubstance UNDGSubstance
		{
			get
			{
				return (UNDGSubstance)Factory.Load(typeof(UNDGSubstance), DA_DG);
			}
		}

		#region Properties

		[List("Lookups.UNDGSubstances")]
		public override ZGuid DA_DG
		{
			get { return base.DA_DG; }
			set { base.DA_DG = value; }
		}

		[List("Lookups.Languages")]
		public override ZString DA_Language
		{
			get { return base.DA_Language; }
			set { base.DA_Language = value; }
		}

		protected bool DA_Language_ReadOnly
		{
			get
			{
				return UNDGSubstance != null && !UNDGSubstance.DetailsLanguage.IsEmpty;
			}
		}

		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(DA_Type); }
		}

		public bool IsValidEnglishSubstance => DA_Language == Core.Constants.Languages.English && UNDGSubstance != null && UNDGSubstance.DG_IsSystem && DA_Type != ViewUNDGAttributeLookups.TypeConstants.UsrUSDOTShippingNames;

		#endregion
	}
}

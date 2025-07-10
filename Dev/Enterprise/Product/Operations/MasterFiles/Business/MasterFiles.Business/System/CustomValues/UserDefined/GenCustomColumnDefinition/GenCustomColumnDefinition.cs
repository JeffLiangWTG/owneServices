using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomColumnDefinition : AutoGenCustomColumnDefinition, ICustomColumnDefinition, IGenCustomColumnDefinition
	{
		public GenCustomColumnDefinition(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[List("Lookups.Rules")]
		public override ZGuid XC_XR
		{
			get { return base.XC_XR; }
			set { base.XC_XR = value; }
		}

		[List("Lookups.Types")]
		public override ZString XC_Type
		{
			get { return base.XC_Type; }
			set
			{
				base.XC_Type = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateXC_Name();
				}
			}
		}

		[TranslatableDataField(Schema.TableName, Schema.XC_Name, MaxLength = Schema.XC_NameMaxLength, Type = typeof(GenCustomColumnDefinition), Asmid = ResString.AssemblyId)]
		public override ZString XC_Name
		{
			get => base.XC_Name;
			set => base.XC_Name = value;
		}

		public MultilingualString XC_NameMultilingual
		{
			get
			{
				return GetMultilingual(XC_NameInfo);
			}
		}

		public bool AppendTypeToCaption { get; set; }

		public string NameMultilingual => AppendTypeToCaption ? Res.GetString("1A392D14-613F-48C0-ACA7-AF71334F4DF0", "{0} ({1})", XC_NameMultilingual, XC_Type) : XC_NameMultilingual;

		#region ICustomColumnDefinition Members

		ZGuid ICustomColumnDefinition.Identifier => PK;

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		string ICustomColumnDefinition.Name => XC_Name;

		string ICustomColumnDefinition.NameLocalized => NameMultilingual;

		string ICustomColumnDefinition.Type => XC_Type;

		int? ICustomColumnDefinition.Sequence => XC_DisplaySequence;

		bool ICustomColumnDefinition.IsDeleted => IsDeleted;

		ZGuid? ICustomColumnDefinition.RuleDefinitionReference => XC_XR;

		bool ICustomColumnDefinition.IsRuleActive => CustomAddOnRule?.XR_IsActive ?? true;

		ICustomAddOnRule[] ICustomColumnDefinition.GetRules() => Factory.GetCachedValue((XC_XR, "AddOnRuleMetaData"), () => CustomAddOnRule?.GetRules() ?? Array.Empty<ICustomAddOnRule>(), CacheStalenessPolicy.StaleOnFactorySave);

		int ICustomColumnDefinition.MaxLength => GenCustomAddOnValue.Schema.XV_DataMaxLength;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			GenCustomColumnDefinition newCustom = (GenCustomColumnDefinition)base.CloneInternal(args);

			return newCustom;
		}

		public T Clone<T>() where T : GenCustomColumnDefinition
		{
			return (T)Clone(typeof(T));
		}

		public GenCustomColumnDefinition Clone(Type clonedType)
		{
			GenCustomColumnDefinition result = (GenCustomColumnDefinition)Factory.New(clonedType);
			using (result.GetValidationSuspender())
			{
				List<string> excludedProperties = new List<string>();
				excludedProperties.Add(GenCustomColumnDefinition.Schema.XC_ParentID);
				excludedProperties.Add(GenCustomColumnDefinition.Schema.XC_ParentTableCode);

				BusinessObjectCloneArgs args = new BusinessObjectCloneArgs(excludedProperties.ToArray(), clonedType);
				result.CopyPersistentValuesFrom(this, args);
			}
			return result;
		}

		#endregion

		protected override GenCustomColumnDefinitionValidation GetNewValidation()
		{
			var validation = base.GetNewValidation();

			if (XC_ParentTableCode == ProcessTaskTemplateSchema.Constants.Prefix)
			{
				var processTaskTemplate = Factory.Load<ProcessTaskTemplate>(XC_ParentID);
				if (processTaskTemplate != null && processTaskTemplate.WorkflowDescriptor != null)
				{
					var additionalValidation = processTaskTemplate.WorkflowDescriptor.GetAdditionalCustomColumnDefinitionValidation(this);

					if (additionalValidation != null)
					{
						validation.Add(additionalValidation);
					}
				}
			}

			return validation;
		}

		#region Test Helpers
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (XC_ParentID.IsEmpty)
			{
				XC_ParentTableCode = "";
			}
		}

#endif
		#endregion
	}
}

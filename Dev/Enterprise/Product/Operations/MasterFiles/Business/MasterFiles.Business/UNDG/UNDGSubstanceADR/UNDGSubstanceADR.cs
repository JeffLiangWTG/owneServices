using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.ADR_UNNO), DescriptionProperty(Schema.ADR_PSN)]
	public class UNDGSubstanceADR : AutoUNDGSubstanceADR,
		IUNDGAttributeParent,
		IUNDGStandardSubstance,
		IDGSubstance
	{
		public UNDGSubstanceADR(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("9e50adba-2541-589d-4581-d3325d3d81c1", "Dangerous Goods Substance - {0}", CalculateShortcutName());

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDefaultDetailsLanguage();
		}

		#region Properties

		#region FormattedLabels

		public ZString ADR_FormattedLabels
		{
			get
			{
				return base.ADR_Labels.Replace("+", ", ");
			}
		}

		#endregion

		#region Labels

		public ZString ADR_Label1
		{
			get
			{
				var handlingCodes = base.ADR_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					return split[0];
				}

				return ZString.Empty;
			}
		}

		public ZString ADR_Label2
		{
			get
			{
				var handlingCodes = base.ADR_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					if (split.Length > 1)
					{
						return split[1];
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ADR_Label3
		{
			get
			{
				var handlingCodes = base.ADR_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					if (split.Length > 2)
					{
						return split[2];
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ADR_Label4
		{
			get
			{
				var handlingCodes = base.ADR_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					if (split.Length > 3)
					{
						return split[3];
					}
				}

				return ZString.Empty;
			}
		}
		#endregion

		#region ADR_IsSystemDefined

		public ZBool ADR_IsSystemDefined => true;

		#endregion

		#region ADR_Class

		[List("Lookups.ADRClassList")]
		public override ZString ADR_Class
		{
			get { return base.ADR_Class; }
			set { base.ADR_Class = value; }
		}

		#endregion

		#region ADR_PG

		[List("Lookups.PackingGroupList")]
		public override ZString ADR_PG
		{
			get { return base.ADR_PG; }
			set { base.ADR_PG = value; }
		}

		#endregion

		#region DG_ExceptedQuantityCode

		[List("Lookups.ExceptedQuantityList")]
		public override ZString ADR_ExceptedQuantityCode
		{
			get { return base.ADR_ExceptedQuantityCode; }
			set { base.ADR_ExceptedQuantityCode = value; }
		}

		#endregion

		#region Names

		[ChildEditable(true)]
		public UNDGAttributeZZCollection Names
		{
			get
			{
				if (names == null)
				{
					names = new UNDGAttributeZZCollection(Factory, this, ViewUNDGAttributeLookups.TypeConstants.ProperShippingName);
					RegisterEditableChildObject(names);
				}

				return names;
			}
		}

		UNDGAttributeZZCollection names;

		#endregion

		#region Classification Codes

		public ZString ClassificationCode1
		{
			get
			{
				var classificationCode = base.ADR_ClassificationCode;
				if (!classificationCode.IsEmpty)
				{
					var split = classificationCode.Split(ClassificationCodeSplitters);
					return split.Any() ? split[0].Trim() : classificationCode;
				}

				return ZString.Empty;
			}
		}

		public ZString ClassificationCode2
		{
			get
			{
				var classificationCode = base.ADR_ClassificationCode;
				if (!classificationCode.IsEmpty)
				{
					var split = classificationCode.Split(ClassificationCodeSplitters);
					if (split.Length > 1)
					{
						return split[1].Trim();
					}
				}

				return ZString.Empty;
			}
		}

		string[] ClassificationCodeSplitters => new[] { (NoResString)" or ", " OR " };

		#endregion

		#region Qualifying Descriptive Text

		[ChildEditable(true)]
		public UNDGAttributeZZCollection QualifyingDescriptiveTexts
		{
			get
			{
				if (qualifyingDescriptiveTexts == null)
				{
					qualifyingDescriptiveTexts = new UNDGAttributeZZCollection(Factory, this, ViewUNDGAttributeLookups.TypeConstants.QualifyingDescriptiveText);
					RegisterEditableChildObject(qualifyingDescriptiveTexts);
				}

				return qualifyingDescriptiveTexts;
			}
		}

		UNDGAttributeZZCollection qualifyingDescriptiveTexts;

		#endregion

		#region IReferenceSubstance

		[List("Lookups.Languages")]
		[MaxLength(7)]
		public ZString DetailsLanguage
		{
			get { return detailsLanguage; }
			set
			{
				if (detailsLanguage != value)
				{
					CheckMaximumLength(DetailsLanguageInfo, value); // you are likely missing this line!
					detailsLanguage = value;
					DetailsLanguageInfo.RefreshBinding();
				}
			}
		}

		ZString detailsLanguage;

		public ZPropertyInfo DetailsLanguageInfo => GetZPropertyInfo(nameof(DetailsLanguage));

		void SetDefaultDetailsLanguage()
		{
			DetailsLanguage = ZString.Empty;
		}

		public ZString TableCode => TablePrefix;

		ZString IDGSubstance.UNNO => ADR_UNNO;

		ZString IDGSubstance.Variant => ADR_Variant;

		ZString IDGSubstance.Standard => TablePrefix;

		#endregion

		#region UNDGCountryReferences

		[List("Lookups.UNDGCountryReferences")]
		public UNDGCountryReferenceManyToManyCollection<UNDGSubstanceADR> UNDGCountryReferences
		{
			get
			{
				if (undgCountryReferences == null)
				{
					undgCountryReferences = UNDGSubstance.GetUNDGCountryReferenceManyToManyCollection(this);
				}

				return undgCountryReferences;
			}
		}

		UNDGCountryReferenceManyToManyCollection<UNDGSubstanceADR> undgCountryReferences;

		#endregion

		#endregion
	}
}

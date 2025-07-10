using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.JTT_UNNO), DescriptionProperty(Schema.JTT_PSN)]
	public class UNDGSubstanceJTT : AutoUNDGSubstanceJTT,
		IUNDGAttributeParent,
		IUNDGStandardSubstance,
		IDGSubstance
	{
		public UNDGSubstanceJTT(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDefaultDetailsLanguage();
		}

		#region Properties

		#region FormattedLabels

		public ZString JTT_FormattedLabels
		{
			get
			{
				return base.JTT_Labels.Replace("+", ", ");
			}
		}

		#endregion

		#region Labels

		public ZString JTT_Label1
		{
			get
			{
				var handlingCodes = base.JTT_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					return split[0];
				}

				return ZString.Empty;
			}
		}

		public ZString JTT_Label2
		{
			get
			{
				var handlingCodes = base.JTT_Labels;
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

		public ZString JTT_Label3
		{
			get
			{
				var handlingCodes = base.JTT_Labels;
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

		#endregion

		#region JTT_IsSystemDefined

		public ZBool JTT_IsSystemDefined => true;

		#endregion

		#region JTT_Class

		[List("Lookups.JTTClassList")]
		public override ZString JTT_Class
		{
			get { return base.JTT_Class; }
			set { base.JTT_Class = value; }
		}

		#endregion

		#region JTT_PG

		[List("Lookups.PackingGroupList")]
		public override ZString JTT_PG
		{
			get { return base.JTT_PG; }
			set { base.JTT_PG = value; }
		}

		#endregion

		#region DG_ExceptedQuantityCode

		[List("Lookups.ExceptedQuantityList")]
		public override ZString JTT_ExceptedQuantityCode
		{
			get { return base.JTT_ExceptedQuantityCode; }
			set { base.JTT_ExceptedQuantityCode = value; }
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
				var classificationCode = base.JTT_ClassificationCode;
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
				var classificationCode = base.JTT_ClassificationCode;
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

		ZString IDGSubstance.UNNO => JTT_UNNO;

		ZString IDGSubstance.Variant => JTT_Variant;

		ZString IDGSubstance.Standard => TablePrefix;

		#endregion

		#region UNDGCountryReferences

		[List("Lookups.UNDGCountryReferences")]
		public UNDGCountryReferenceManyToManyCollection<UNDGSubstanceJTT> UNDGCountryReferences
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

		UNDGCountryReferenceManyToManyCollection<UNDGSubstanceJTT> undgCountryReferences;

		#endregion

		#endregion
	}
}

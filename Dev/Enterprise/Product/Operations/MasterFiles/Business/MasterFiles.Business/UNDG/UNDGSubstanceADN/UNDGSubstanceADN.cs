using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.ADN_UNNO), DescriptionProperty(Schema.ADN_PSN)]
	public class UNDGSubstanceADN : AutoUNDGSubstanceADN,
		IUNDGAttributeParent,
		IUNDGStandardSubstance,
		IDGSubstance
	{
		public UNDGSubstanceADN(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region ADN_Class

		[List("Lookups.ADNClassList")]
		public override ZString ADN_Class { get => base.ADN_Class; set => base.ADN_Class = value; }

		#endregion

		#region ADN_ExceptedQuatityCode

		[List("Lookups.ExceptedQuantityList")]
		public override ZString ADN_ExceptedQuantityCode { get => base.ADN_ExceptedQuantityCode; set => base.ADN_ExceptedQuantityCode = value; }

		#endregion

		#region ADN_IsSystemDefined

		public ZBool ADN_IsSystemDefined => true;

		#endregion

		#region ADN_PG

		[List("Lookups.PackingGroupList")]
		public override ZString ADN_PG { get => base.ADN_PG; set => base.ADN_PG = value; }

		#endregion

		#region Classification Codes

		public ZString ClassificationCode1
		{
			get
			{
				var classificationCode = base.ADN_ClassificationCode;
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
				var classificationCode = base.ADN_ClassificationCode;
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

		#region Labels

		public ZString ADN_Label1
		{
			get
			{
				var handlingCodes = base.ADN_Labels;
				if (!handlingCodes.IsEmpty)
				{
					var split = handlingCodes.Split('+');
					return split[0];
				}

				return ZString.Empty;
			}
		}

		public ZString ADN_Label2
		{
			get
			{
				var handlingCodes = base.ADN_Labels;
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

		public ZString ADN_Label3
		{
			get
			{
				var handlingCodes = base.ADN_Labels;
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

		public ZString ADN_Label4
		{
			get
			{
				var handlingCodes = base.ADN_Labels;
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

		#endregion

		#region UNDGCountryReferences

		[List("Lookups.UNDGCountryReferences")]
		public UNDGCountryReferenceManyToManyCollection<UNDGSubstanceADN> UNDGCountryReferences
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

		UNDGCountryReferenceManyToManyCollection<UNDGSubstanceADN> undgCountryReferences;

		#endregion

		#endregion

		#region OnLoaded

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetDefaultDetailsLanguage();
		}

		#endregion

		#region IDGSubstance

		ZString IDGSubstance.UNNO => ADN_UNNO;

		ZString IDGSubstance.Variant => ADN_Variant;

		ZString IDGSubstance.Standard => TablePrefix;

		#endregion
	}
}

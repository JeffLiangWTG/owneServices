using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefLanguageTextPageEntry : NonPersistentBusinessObject
	{
		ZString translation;

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public RefLanguageTextPageEntry(ZString language, MultilingualLanguageText caption)
		{
			Language = language;
			Caption = caption;
			English = caption.EnglishText ?? caption.ToString(Res.DefaultLanguage);
			translation = caption.Language != language ? caption.EnglishText : caption.Translation ?? caption.ToString(language);
		}

		[ReadOnly(true)]
		public ZString Language { get; set; }

		public ZString LanguageDescription
		{
			get { return languageDescription ?? (languageDescription = new CodeDescriptionPairList(OLookUpEditType.Language).GetDescriptionFromCode(Language)).Value; }
		}
		ZString? languageDescription;

		[ReadOnly(true)]
		public ZString English { get; set; }

		[BusinessObjectTestExclude]
		public ZString Translation
		{
			get
			{
				return translation;
			}
			set
			{
				if (value.IsEmpty)
				{
					translation = Caption.ToString(Language);
				}
				else
				{
					translation = value;
				}
			}
		}

		public MultilingualLanguageText Caption { get; }
	}
}

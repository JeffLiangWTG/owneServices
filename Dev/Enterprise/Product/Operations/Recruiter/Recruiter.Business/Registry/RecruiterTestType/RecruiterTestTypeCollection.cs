using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class RecruiterTestTypeCollection : RegistryBusinessObjectCollection
	{
		public RecruiterTestTypeCollection()
		{
		}

		public RecruiterTestTypeCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public RecruiterTestTypeCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list, 0)
		{
		}

		public new RecruiterTestType this[int i]
		{
			get { return (RecruiterTestType)base[i]; }
		}

		public RecruiterTestType Add(string code, string description)
		{
			return Add(code, description, "");
		}

		public RecruiterTestType Add(string code, string description, string category)
		{
			return Add(code, description, category, "");
		}

		public RecruiterTestType Add(string code, string description, string testTypeCategory, string testNote, bool isVisibleOnWeb = true)
		{
			RecruiterTestType result = AddNew();
			result.Code = code;
			result.Description = (NoResString)description;
			result.Category = testTypeCategory;
			result.NotificationType = RecruiterTestNotificationType.Codes.DoNotDeliver;
			result.TestNote = testNote;
			result.IsVisibleOnWeb = isVisibleOnWeb;
			return result;
		}

		public new RecruiterTestType AddNew()
		{
			return (RecruiterTestType)base.AddNew();
		}

		#region Clone

		protected sealed override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			RecruiterTestTypeCollection clone = GetNewCollection();
			clone.CodeMaxLength = CodeMaxLength;
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		internal RecruiterTestTypeCollection GetNewCollection()
		{
			return new RecruiterTestTypeCollection(CurrentFallbackLevel);
		}

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RecruiterTestType();
		}

		public string GetDescriptionFromCode(string code)
		{
			RecruiterTestType element = (RecruiterTestType)FindByCode(code);
			return (element == null) ? ZString.Empty : element.Description;
		}

		public string GetCategoryFromCode(string code)
		{
			RecruiterTestType element = (RecruiterTestType)FindByCode(code);
			return (element == null) ? ZString.Empty : element.Category;
		}

		public string[] GetCodesFromCategory(string category)
		{
			return this.Cast<RecruiterTestType>().Where(type => type.Category == category).Select(type => type.Code.ToString()).ToArray();
		}

		public string[] GetWebVisibleCodesFromCategory(string category)
		{
			return this.Cast<RecruiterTestType>().Where(type => type.Category == category && type.IsVisibleOnWeb).Select(type => type.Code.ToString()).ToArray();
		}

		public string[] GetCodes()
		{
			return this.Cast<RecruiterTestType>().Select(type => type.Code.ToString()).ToArray();
		}

		public string[] GetWebVisibleCodes()
		{
			return this.Cast<RecruiterTestType>().Where(type => type.IsVisibleOnWeb).Select(type => type.Code.ToString()).ToArray();
		}
	}
}

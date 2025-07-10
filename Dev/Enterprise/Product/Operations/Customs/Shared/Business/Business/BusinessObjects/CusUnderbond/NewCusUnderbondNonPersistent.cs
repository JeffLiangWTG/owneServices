using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class NewCusUnderbondNonPersistent : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string UnderbondParentStringRepresentation = "UnderbondParentStringRepresentation";
		}

		#endregion

		public NewCusUnderbondNonPersistent(ICusUnderbondDependentCollectionParent[] possibleParents)
		{
			this.possibleParents = possibleParents;
			CodeDescriptionPairList underbondForList = this.UnderbondForList;
			if (underbondForList.Count > 0)
			{
				UnderbondParentStringRepresentation = underbondForList[0].Code;
			}
		}

		ZString fUnderbondParentStringRepresentation;
		[CargoWise.ComponentModel.MaxLength(70)]
		public ZString UnderbondParentStringRepresentation
		{
			get
			{
				return fUnderbondParentStringRepresentation;
			}
			set
			{
				if (fUnderbondParentStringRepresentation != value)
				{
					CheckMaximumLength(UnderbondParentStringRepresentationInfo, value);
					SetNonPersistentPropertyValue(UnderbondParentStringRepresentationInfo, ref fUnderbondParentStringRepresentation, value);
				}
			}
		}

		public ZPropertyInfo UnderbondParentStringRepresentationInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.UnderbondParentStringRepresentation);
			}
		}

		public CodeDescriptionPairList UnderbondForList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				foreach (ICusUnderbondDependentCollectionParent provided in possibleParents)
				{
					result.AddPair(provided.UnderbondHumanReadableName.ToString(), provided.UnderbondHumanReadableName.ToString());
				}
				return result;
			}
		}

		public ICusUnderbondDependentCollectionParent SelectedParent
		{
			get
			{
				foreach (ICusUnderbondDependentCollectionParent possibleParent in possibleParents)
				{
					if (possibleParent.UnderbondHumanReadableName.ToUpper() == UnderbondParentStringRepresentation.ToUpper())
					{
						return possibleParent;
					}
				}
				return null;
			}
		}

		readonly ICusUnderbondDependentCollectionParent[] possibleParents;
	}
}

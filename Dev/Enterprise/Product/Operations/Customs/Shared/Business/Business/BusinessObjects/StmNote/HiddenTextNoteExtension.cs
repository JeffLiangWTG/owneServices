using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class HiddenTextNoteExtension
	{
		public static void SetNoteText(this HiddenTextNote hiddenNoteText, BusinessObject parent, ZPropertyInfo propertyInfo, ZString value)
		{
			hiddenNoteText.SetNoteText(parent, propertyInfo, value, null);
		}

		public static void SetNoteText(this HiddenTextNote hiddenNoteText, BusinessObject parent, ZPropertyInfo propertyInfo, ZString value, Action validator)
		{
			if (hiddenNoteText.Text != value)
			{
				BusinessObject.CheckMaximumLength(propertyInfo, value);
				hiddenNoteText.Text = value;
				parent.HasChanges = true;
				if (!parent.IsValidationSuspended && validator != null)
				{
					validator.Invoke();
				}
			}
			propertyInfo.RefreshBinding();
		}

		public static void DeleteHiddenNotes(this BusinessObject parentBizObj)
		{
			if (parentBizObj != null)
			{
				var query = new ZQuery(StmNoteSchema.ST_ParentID, parentBizObj.PK);
				query.AddToFilter(StmNoteSchema.ST_Table, parentBizObj.TableName);
				query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
				query.FetchOnlyFromLocalCache = !parentBizObj.IsInDatabase;
				parentBizObj.Factory.Load<StmNote>(query).DeleteAll();
			}
		}
	}
}

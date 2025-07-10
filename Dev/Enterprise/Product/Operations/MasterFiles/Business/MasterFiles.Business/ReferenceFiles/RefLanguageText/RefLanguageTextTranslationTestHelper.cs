#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefLanguageTextTranslationTestHelper
	{
		public RefLanguageTextTranslationTestHelper(BusinessObjectFactory factory)
		{
			ColumnToTest = AutoRefCountryStates.Schema.RW_Description;
			ParentTableCode = BusinessObjectFactory.GetTableCodeFromType(typeof(RefCountryStates));
			BoFactory = factory;
			CreateRefLanguageTextToTest();
		}

		BusinessObjectFactory BoFactory { get; set; }

		public MultilingualLanguageText GetMultilingualLanguage(string caption)
		{
			var parent = GetContextObject(null, caption);
			return new MultilingualLanguageText(parent.PK.ToString(), ParentTableCode, ColumnToTest, caption, BoFactory);
		}

		public string[] GetFilterColumns()
		{
			return new string[] { RefCountryStatesSchema.Constants.RW_RN_NKCountryCode };
		}

		List<BusinessObject> BusinessObjects { get; set; }

		public BusinessObject GetContextObject(BusinessObjectFactory factory, string englishCaption)
		{
			var index = Array.IndexOf(EnglishNumbers.ToArray(), englishCaption);
			if (BusinessObjects == null)
			{
				GenerateBusinessObjects(factory);
			}

			return BusinessObjects.ElementAt(index);
		}
		public IEnumerable<string> ChineseNumbers => new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
		public IEnumerable<string> FrenchNumbers => new string[] { "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix" };
		public IEnumerable<string> EnglishNumbers => new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
		public ZString ColumnToTest { get; set; }
		public ZString ParentTableCode { get; set; }

		void GenerateBusinessObjects(BusinessObjectFactory factory)
		{
			BusinessObjects = new List<BusinessObject>();
			for (var x = 0; x <= 10; x++)
			{
				factory.Load<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, "XX"));

				var obj = factory.NewWithValidTestData<RefCountryStates>();
				obj.RW_RN_NKCountryCode = "XX";
				BusinessObjects.Add(obj);
				factory.Save();
			}
		}

		void CreateRefLanguageTextToTest()
		{
			for (var x = 0; x <= 10; x++)
			{
				var businessObject = GetContextObject(new BusinessObjectFactory(), EnglishNumbers.ElementAt(x));

				var eng = BoFactory.New<RefLanguageText>();
				eng.RLT_ColumnName = ColumnToTest;
				eng.RLT_ParentTableCode = ParentTableCode;
				eng.RLT_Text = EnglishNumbers.ElementAt(x);
				eng.RLT_IsSystem = true;
				eng.RLT_Language = SharedConstants.Languages.English;
				eng.RLT_ParentId = businessObject.PK;

				var chn = BoFactory.New<RefLanguageText>();
				chn.RLT_ColumnName = ColumnToTest;
				chn.RLT_ParentTableCode = ParentTableCode;
				chn.RLT_Text = ChineseNumbers.ElementAt(x);
				chn.RLT_IsSystem = true;
				chn.RLT_Language = SharedConstants.Languages.ChineseSimplified;
				chn.RLT_ParentId = businessObject.PK;

				var frn = BoFactory.New<RefLanguageText>();
				frn.RLT_ColumnName = ColumnToTest;
				frn.RLT_ParentTableCode = ParentTableCode;
				frn.RLT_Text = FrenchNumbers.ElementAt(x);
				frn.RLT_IsSystem = true;
				frn.RLT_Language = SharedConstants.Languages.French;
				frn.RLT_ParentId = businessObject.PK;

				BoFactory.Save();
			}
		}
	}
}
#endif
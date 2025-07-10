using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class SingleCusSupportingInfoTest<T> : Customs.Business.Testing.CusSupportingInfoTest<T> where T : SingleCusSupportingInfo
	{
		[ExpectNoExceptions]
		public virtual void TestGetUsedFieldsInfos()
		{
			var bizObj = GetSingleCusSupportingInfoForTesting(Factory, false);
			var expected = new ZPropertyInfo[] { bizObj.CSI_CodeInfo, bizObj.CSI_ReferenceNumberInfo, bizObj.CSI_ReferenceNumber2Info, bizObj.CSI_DescriptionInfo };
			NUnit.Framework.Assert.That(bizObj.GetUsedFieldsInfos().ToArray(), NUnit.Framework.Is.EqualTo(expected));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetSingleCusSupportingInfoForTesting(factory);
		}

		[ExpectNoExceptions]
		public void TestSaving()
		{
			var singleCusSupportingInfo = GetSingleCusSupportingInfoForTesting(Factory);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var parentPK = singleCusSupportingInfo.Parent.PK;
			var query = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			query.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, parentPK);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, singleCusSupportingInfo.CSI_Type);
			var singleCusSupportingInfoInDB = newFactory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(singleCusSupportingInfoInDB.Length, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(singleCusSupportingInfoInDB.Single().PK, NUnit.Framework.Is.EqualTo(singleCusSupportingInfo.PK));
			singleCusSupportingInfo.GetUsedFieldsInfos().ToList().ForEach(x => x.ClearValue());
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			singleCusSupportingInfoInDB = newFactory.Load(typeof(CusSupportingInfo), query);
			NUnit.Framework.Assert.That(!singleCusSupportingInfoInDB.Any(), NUnit.Framework.Is.True, "The singleCusSupportingInfoInDB was deleted");
		}

		[ExpectNoExceptions]
		public void TestIsSavedByFactory()
		{
			var singleCusSupportingInfo = GetSingleCusSupportingInfoForTesting(Factory);
			var usedFieldsInfosList = singleCusSupportingInfo.GetUsedFieldsInfos().ToList();
			usedFieldsInfosList.ForEach(x => x.ClearValue());
			NUnit.Framework.Assert.That(!singleCusSupportingInfo.IsSavedByFactory, NUnit.Framework.Is.True, "IsSavedByFactory is false because the singleCusSupportingInfo is empty.");
			SetValueToEitherOfUsedFields(usedFieldsInfosList);
			NUnit.Framework.Assert.That(singleCusSupportingInfo.IsSavedByFactory, NUnit.Framework.Is.True, "IsSavedByFactory is false because the singleCusSupportingInfo is not empty.");
			singleCusSupportingInfo.Delete();
			NUnit.Framework.Assert.That(!singleCusSupportingInfo.IsSavedByFactory, NUnit.Framework.Is.True);
		}

		void SetValueToEitherOfUsedFields(List<ZPropertyInfo> infos)
		{
			int randomIndex = new Random().Next(infos.Count);
			var randomPropertyInfo = infos[randomIndex];
			var propertyType = randomPropertyInfo.PropertyType;
			if (propertyType == typeof(ZInt))
			{
				randomPropertyInfo.Value = (ZInt)1;
			}
			else if (propertyType == typeof(ZDecimal))
			{
				randomPropertyInfo.Value = (ZDecimal)1;
			}
			else
			{
				randomPropertyInfo.Value = new ZString("A");
			}
		}

		protected SingleCusSupportingInfo GetSingleCusSupportingInfoForTesting(BusinessObjectFactory factory, bool addExtraData = true)
		{
			var singleCusSupportingInfo = (SingleCusSupportingInfo)GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();
			if (addExtraData)
			{
				var usedFieldsInfosList = singleCusSupportingInfo.GetUsedFieldsInfos().ToList();
				SetValueToEitherOfUsedFields(usedFieldsInfosList);
			}

			return singleCusSupportingInfo;
		}
	}
}

using System;
using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	sealed class TWEntryInstructionDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestFillTradersRemarksFromAddInfos()
		{
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, Enterprise.Core.Constants.CountryCodes.Taiwan, Enterprise.Core.Constants.CountryCodes.Taiwan);
			var output = new TWEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as CusEntryInstruction;
			NUnit.Framework.Assert.That(output.TW_TradersRemarks, NUnit.Framework.Is.EqualTo("長期委任： 核准案號99出業9999").Using(CustomComparers.TypeComparison));

			input.AddInfoCollection.Clear();
			input.AddInfoCollection.Add(new AddInfo { Key = PredefinedNoteTypes.Instance.TWTradersRemarks.Description, Value = string.Empty });

			output = new TWEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as CusEntryInstruction;
			AssertNullOrEmpty(output.TW_TradersRemarks);
		}

		public void TestFillTradersRemarksFromAddInfosWhenSourceAndTargetCountryIsNotSame()
		{
			var logger = new TestErrorLogger();
			var helper = new UniversalDataObjectReaderHelper(Factory, Enterprise.Core.Constants.CountryCodes.Taiwan, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var output = new TWEntryInstructionDataObjectReader(input, logger, helper, Factory, declaration).ReadIntoBusinessObject() as CusEntryInstruction;
			AssertNullOrEmpty(output.TW_TradersRemarks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "BLT";

			input = new EntryInstruction
			{
				AddInfoCollection = new List<AddInfo>(),
				AddInfoGroupCollection = new List<AddInfoGroup>()
			};

			input.AddInfoCollection.Add(new AddInfo
			{
				Key = PredefinedNoteTypes.Instance.TWTradersRemarks.Description,
				Value = "長期委任： 核准案號99出業9999"
			});
		}

		EntryInstruction input;

		JobDeclaration declaration;
	}
}

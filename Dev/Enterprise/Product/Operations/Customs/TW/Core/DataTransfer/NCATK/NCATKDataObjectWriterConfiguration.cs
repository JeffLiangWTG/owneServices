using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.DataTransfer.Universal;

namespace Enterprise.Customs.TW.DataTransfer
{
	public class NCATKDataObjectWriterConfiguration : DeclarationDataObjectWriterConfiguration
	{
		public NCATKDataObjectWriterConfiguration(BusinessObject header, IJobDeclarationMessageSendingObjectParent messageSendingObjectParent)
		{
			CAHeaderPKsToPopulate = new List<ZGuid> { header.PK };
			var entryHeader = messageSendingObjectParent?.ParentDeclaration?.CustomsEntryHeaders?.Cast<CusEntryHeader>()?.FirstOrDefault();
			if (entryHeader != null)
			{
				EntryHeaderPKsToPopulate = new List<ZGuid> { entryHeader.PK };
			}
			AdditionalWriterSettingActions += SetAdditionalWriterSetting;
		}

		public IEnumerable<ZGuid> CAHeaderPKsToPopulate { get; }

		void SetAdditionalWriterSetting(DeclarationDataObjectWriter writer)
		{
			if (writer is TWJobDeclarationDataObjectWriter twWriter)
			{
				twWriter.SetCAHeaderPKsToPopulate(CAHeaderPKsToPopulate);
			}
		}
	}
}

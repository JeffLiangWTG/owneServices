using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business;

class CusEntryHeaderProcessTask<TCusEntryHeader>(BusinessObjectFactory factory, DataRow row) : ProcessTask(factory, row) where TCusEntryHeader : CusEntryHeader
{
	protected override Type ParentType => typeof(TCusEntryHeader);
}

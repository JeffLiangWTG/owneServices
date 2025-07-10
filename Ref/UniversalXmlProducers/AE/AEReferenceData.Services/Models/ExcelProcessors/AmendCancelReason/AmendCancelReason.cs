using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public sealed class AmendCancelReason
{
	public string ID { get; set; }

	public string Name { get; set; }

	public List<string> NKCodeTypeList { get; set; }

	public static class NKCodeType
	{
		public const string Amend = "AMEND";
		public const string Cancel = "CANCL";
		public const string TransferAmend = "TRAMD";
		public const string TransferCancel = "TRCAN";
	}
}

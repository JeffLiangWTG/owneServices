using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.PL.Business;

static class RecognizableMessages
{
	[ThreadSafe]
	static IRecognizableMessages Instance { get; } = RecognizableMessagesFactory.Create();

	public static IMessageDefinitionRepository All => Instance.All;
	public static IMessageDefinitionRepository Common => Instance.Common;
	public static IMessageDefinitionRepository DocumentHandlingPort => Instance.DocumentHandlingPort;
	public static IMessageDefinitionRepository AESAIS => Instance.AESAIS;
	public static IMessageDefinitionRepository NCTS => Instance.NCTS;
	public static IMessageDefinitionRepository ExportControl => Instance.ExportControl;

	public static IMessageDefinition UPP => All.ByUniqueMessageName["UPP"];
	public static IMessageDefinition NPP => All.ByUniqueMessageName["NPP"];
}

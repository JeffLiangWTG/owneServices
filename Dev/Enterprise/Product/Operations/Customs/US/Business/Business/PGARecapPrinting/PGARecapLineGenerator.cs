using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	public abstract class MessageBlockMandatoryCharacters
	{
		protected MessageBlockMandatoryCharacters()
		{
		}

		public const string AEPAPG02 = "PG02";
		public const string AEPAPG04 = "PG04";
		public const string AEPAPG05 = "PG05";
		public const string AEPAPG06 = "PG06";
		public const string AEPAPG07 = "PG07";
		public const string AEPAPG08 = "PG08";
		public const string AEPAPG10 = "PG10";
		public const string AEPAPG13 = "PG13";
		public const string AEPAPG14 = "PG14";
		public const string AEPAPG17 = "PG17";
		public const string AEPAPG18 = "PG18";
		public const string AEPAPG19 = "PG19";
		public const string AEPAPG20 = "PG20";
		public const string AEPAPG21 = "PG21";
		public const string AEPAPG22 = "PG22";
		public const string AEPAPG23 = "PG23";
		public const string AEPAPG24 = "PG24";
		public const string AEPAPG25 = "PG25";
		public const string AEPAPG26 = "PG26";
		public const string AEPAPG27 = "PG27";
		public const string AEPAPG28 = "PG28";
		public const string AEPAPG29 = "PG29";
		public const string AEPAPG30 = "PG30";
		public const string AEPAPG31 = "PG31";
		public const string AEPAPG32 = "PG32";
		public const string AEPAPG33 = "PG33";
		public const string AEPAPG34 = "PG34";
		public const string AEPAPG35 = "PG35";
		public const string AEPAPG50 = "PG50";
		public const string AEPAPG51 = "PG51";
		public const string AEPAPG55 = "PG55";
		public const string AEPAPG60 = "PG60";
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class PGARecapLineGenerator
	{
		protected PGARecapLineGenerator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<ZString> GenerateLines(IEnumerable<(ICusEntryLine cusEntryLine, List<MessageBlock> messages)> entryLineMessageBlockCollection, BusinessObjectFactory factory)
		{
			foreach (var entryLineMessageBlock in entryLineMessageBlockCollection)
			{
				foreach (var line in GenerateLinesCore(entryLineMessageBlock, factory))
				{
					yield return line;
				}
			}
		}
		readonly BusinessObjectFactory factory;

		static IEnumerable<ZString> GenerateLinesCore((ICusEntryLine entryLine, List<MessageBlock> messages) entryLineMessagesBlock, BusinessObjectFactory factory)
		{
			var result = new List<ZString>();

			if (entryLineMessagesBlock.messages != null && entryLineMessagesBlock.messages.Count > 0)
			{
				var relatedBlocks = new List<MessageBlock>();
				var entryLineNo = ZString.Empty;
				var description = ZString.Empty;
				AEPAPG01 currentAEPAPG01 = null;
				ICusEntryLine entryLine = null;

				foreach (var messageBlock in entryLineMessagesBlock.messages)
				{
					var asese40 = messageBlock as ASESE40;
					if (asese40 != null)
					{
						entryLineNo = asese40.LineItemIdentifier.ToString();
						entryLine = entryLineMessagesBlock.entryLine;
					}
					else
					{
						var aens40 = messageBlock as AENS40;
						if (aens40 != null)
						{
							if (!entryLineNo.IsEmpty && currentAEPAPG01 != null)
							{
								result.AddRange(GenerateLines(factory, entryLineNo, entryLine, description, currentAEPAPG01, relatedBlocks));
								description = ZString.Empty;
								currentAEPAPG01 = null;
								relatedBlocks.Clear();
							}
							entryLineNo = aens40.LineItemIdentifier;
							entryLine = entryLineMessagesBlock.entryLine;
						}
						else
						{
							var aensoi = messageBlock as AENSOI;
							if (aensoi != null)
							{
								if (!description.IsEmpty && currentAEPAPG01 != null)
								{
									result.AddRange(GenerateLines(factory, entryLineNo, entryLine, description, currentAEPAPG01, relatedBlocks));
									currentAEPAPG01 = null;
									relatedBlocks.Clear();
								}
								description = aensoi.CommercialDescriptionText;
							}
							else
							{
								var aepapg01 = messageBlock as AEPAPG01;
								if (aepapg01 != null)
								{
									if (currentAEPAPG01 != null)
									{
										result.AddRange(GenerateLines(factory, entryLineNo, entryLine, description, currentAEPAPG01, relatedBlocks));
										relatedBlocks.Clear();
									}
									currentAEPAPG01 = aepapg01;
								}
								else if (IsRelatedBlocks(messageBlock))
								{
									relatedBlocks.Add(messageBlock);
								}
							}
						}
					}
				}
				if (currentAEPAPG01 != null)
				{
					result.AddRange(GenerateLines(factory, entryLineNo, entryLine, description, currentAEPAPG01, relatedBlocks));
				}
			}
			return result;
		}

		static void AddLineIfNotNull(List<ZString> list, IEnumerable<ZString> lines)
		{
			if (lines != null)
			{
				list.AddRange(lines.Where(x => !x.IsEmpty));
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		static bool IsRelatedBlocks(MessageBlock messageBlock)
		{
			return messageBlock is AEPAPG02 || messageBlock is AEPAPG04 || messageBlock is AEPAPG05 || messageBlock is AEPAPG06 || messageBlock is AEPAPG07 ||
				messageBlock is AEPAPG08 || messageBlock is AEPAPG10 || messageBlock is AEPAPG13 || messageBlock is AEPAPG14 || messageBlock is AEPAPG17 ||
				messageBlock is AEPAPG18 || messageBlock is AEPAPG19 || messageBlock is AEPAPG20 || messageBlock is AEPAPG21 || messageBlock is AEPAPG22 ||
				messageBlock is AEPAPG23 || messageBlock is AEPAPG24 || messageBlock is AEPAPG25 || messageBlock is AEPAPG26 || messageBlock is AEPAPG27 ||
				messageBlock is AEPAPG28 || messageBlock is AEPAPG29 || messageBlock is AEPAPG30 || messageBlock is AEPAPG31 || messageBlock is AEPAPG32 ||
				messageBlock is AEPAPG33 || messageBlock is AEPAPG34 || messageBlock is AEPAPG35 || messageBlock is AEPAPG50 || messageBlock is AEPAPG51 ||
				messageBlock is AEPAPG55 || messageBlock is AEPAPG60;
		}

		static IEnumerable<ZString> GenerateLines(BusinessObjectFactory factory, ZString entryLineNo, ICusEntryLine entryLine, ZString description, AEPAPG01 aepapg01, IEnumerable<MessageBlock> relatedBlocks)
		{
			PGARecapLineGenerator generator = null;
			switch (aepapg01.GovernmentAgencyCode)
			{
				default:
					generator = new PGARecapLineGenerator(factory);
					break;
			}
			return generator.GenerateLinesWithFactory(entryLineNo, entryLine, description, aepapg01, relatedBlocks);
		}

		IEnumerable<ZString> GenerateLinesWithFactory(ZString entryLineNo, ICusEntryLine entryLine, ZString description, AEPAPG01 aepapg01, IEnumerable<MessageBlock> relatedBlocks)
		{
			var result = new List<ZString>();

			AddLineIfNotNull(result, aepapg01.SerialiseHeaderDetailForRecap(factory, entryLineNo, entryLine, description));
			AddLineIfNotNull(result, aepapg01.SerialiseForRecap(factory));
			result.AddRange(GenerateLines(relatedBlocks));
			if (result.Count > 1)
			{
				result.Add(ZString.Empty);
			}
			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected virtual IEnumerable<ZString> GenerateLines(IEnumerable<MessageBlock> relatedBlocks)
		{
			var factory = this.factory;
			var result = new List<ZString>();
			var pg07AndPG08s = new PG07AndPG08sData(factory);
			var pg13AndPG14 = new PG13AndPG14Data(factory);
			var organisation = new PGOrganisationData(factory);
			var pg23s = new PG23sData(factory);
			var pg24s = new PG24sData(factory);
			var pg25s = new PG25sData(factory);
			var pg26s = new PG26sData(factory);
			var pg27s = new PG27sData();
			foreach (var block in relatedBlocks)
			{
				var mandatoryCharacters = block.MandatoryCharacters;
				if (mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG08)
				{
					AddLineIfNotNull(result, AddDetail(pg07AndPG08s));
				}
				if (pg13AndPG14.PG13 != null && mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG14)
				{
					AddLineIfNotNull(result, AddDetail(pg13AndPG14));
				}
				if (organisation.PG21 != null)
				{
					if (mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG55
						&& mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG60)
					{
						AddLineIfNotNull(result, AddDetail(organisation));
					}
				}
				else if (organisation.PG20 != null)
				{
					if (mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG21
						&& mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG55
						&& mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG60)
					{
						AddLineIfNotNull(result, AddDetail(organisation));
					}
				}
				else if (organisation.PG19 != null)
				{
					if (mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG20
						&& mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG55
						&& mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG60)
					{
						AddLineIfNotNull(result, AddDetail(organisation));
					}
				}
				if (pg23s.HasData && mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG23)
				{
					AddLineIfNotNull(result, AddDetail(pg23s));
				}
				if (pg24s.HasData && mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG24)
				{
					AddLineIfNotNull(result, AddDetail(pg24s));
				}
				if (pg25s.HasData && mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG25)
				{
					AddLineIfNotNull(result, AddDetail(pg25s));
				}
				if (pg26s.HasData && mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG26)
				{
					AddLineIfNotNull(result, AddDetail(pg26s));
				}
				if (pg27s.HasData && mandatoryCharacters != MessageBlockMandatoryCharacters.AEPAPG27)
				{
					AddLineIfNotNull(result, AddDetail(pg27s));
				}

				switch (mandatoryCharacters)
				{
					case MessageBlockMandatoryCharacters.AEPAPG02:
						AddLineIfNotNull(result, ((AEPAPG02)block).SerialiseForRecap());
						break;
					case MessageBlockMandatoryCharacters.AEPAPG04:
						AddLineIfNotNull(result, ((AEPAPG04)block).SerialiseForRecap());
						break;
					case MessageBlockMandatoryCharacters.AEPAPG05:
						AddLineIfNotNull(result, ((AEPAPG05)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG06:
						AddLineIfNotNull(result, ((AEPAPG06)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG07:
						pg07AndPG08s.PG07 = (AEPAPG07)block;
						break;
					case MessageBlockMandatoryCharacters.AEPAPG08:
						pg07AndPG08s.Add((AEPAPG08)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG10:
						AddLineIfNotNull(result, ((AEPAPG10)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG13:
						pg13AndPG14.PG13 = (AEPAPG13)block;
						break;
					case MessageBlockMandatoryCharacters.AEPAPG14:
						pg13AndPG14.PG14 = (AEPAPG14)block;
						AddLineIfNotNull(result, AddDetail(pg13AndPG14));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG17:
						AddLineIfNotNull(result, ((AEPAPG17)block).SerialiseForRecap());
						break;
					case MessageBlockMandatoryCharacters.AEPAPG18:
						AddLineIfNotNull(result, ((AEPAPG18)block).SerialiseForRecap());
						break;
					case MessageBlockMandatoryCharacters.AEPAPG19:
						organisation.PG19 = (AEPAPG19)block;
						break;
					case MessageBlockMandatoryCharacters.AEPAPG20:
						organisation.PG20 = (AEPAPG20)block;
						break;
					case MessageBlockMandatoryCharacters.AEPAPG21:
						organisation.PG21 = (AEPAPG21)block;
						break;
					case MessageBlockMandatoryCharacters.AEPAPG60:
						organisation.AddPG60((AEPAPG60)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG55:
						organisation.AddPG55((AEPAPG55)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG22:
						AddLineIfNotNull(result, ((AEPAPG22)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG23:
						pg23s.Add((AEPAPG23)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG24:
						pg24s.Add((AEPAPG24)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG25:
						pg25s.Add((AEPAPG25)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG26:
						pg26s.Add((AEPAPG26)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG27:
						pg27s.Add((AEPAPG27)block);
						break;
					case MessageBlockMandatoryCharacters.AEPAPG28:
						AddLineIfNotNull(result, ((AEPAPG28)block).SerialiseForRecap());
						break;
					case MessageBlockMandatoryCharacters.AEPAPG29:
						AddLineIfNotNull(result, ((AEPAPG29)block).SerialiseForRecap());
						break;
					case MessageBlockMandatoryCharacters.AEPAPG30:
						AddLineIfNotNull(result, ((AEPAPG30)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG31:
						AddLineIfNotNull(result, ((AEPAPG31)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG32:
						AddLineIfNotNull(result, ((AEPAPG32)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG33:
						AddLineIfNotNull(result, ((AEPAPG33)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG34:
						AddLineIfNotNull(result, ((AEPAPG34)block).SerialiseForRecap(factory));
						break;
					case MessageBlockMandatoryCharacters.AEPAPG35:
						AddLineIfNotNull(result, ((AEPAPG35)block).SerialiseForRecap(factory));
						break;
				}
			}
			AddLineIfNotNull(result, AddDetail(pg07AndPG08s));
			AddLineIfNotNull(result, AddDetail(pg13AndPG14));
			AddLineIfNotNull(result, AddDetail(organisation));
			AddLineIfNotNull(result, AddDetail(pg23s));
			AddLineIfNotNull(result, AddDetail(pg24s));
			AddLineIfNotNull(result, AddDetail(pg25s));
			AddLineIfNotNull(result, AddDetail(pg26s));
			AddLineIfNotNull(result, AddDetail(pg27s));
			return result;
		}

		IEnumerable<ZString> AddDetail(IDataSerialiser data)
		{
			foreach (var result in data.Serialise())
			{
				yield return result;
			}
			data.Clear();
		}
	}
}

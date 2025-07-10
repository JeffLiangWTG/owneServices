using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class UserDefinedConditionSplitter : IUserDefinedConditionSplitter
	{
		public IMacroBooleanExpressionClause Split(ZString condition)
			=> UserDefinedConditionSplitterImpl.Split(condition);
	}

	public static class UserDefinedConditionSplitterImpl
	{
		class UserDefinedBooleanExpressionTokenizer : IMacroBooleanExpressionTokenizer
		{
			public IEnumerable<Token> Tokenize(string expression)
			{
				var quoters = new Stack<char>();

				var length = expression.Length;
				Token currentToken = null;

				char? lastNonWhitespaceChar = null;

				for (int index = 0; index < length; index++)
				{
					var current = expression[index];
					var isWhitespace = char.IsWhiteSpace(current);
					var stackTop = quoters.Count > 0 ? (char?)quoters.Peek() : null;

					try
					{
						if (isWhitespace && quoters.Count == 0)
						{
							if (currentToken != null)
							{
								yield return CutLastToken(ref currentToken);
							}
							continue;
						}

						if (current == '"' || current == '\'')
						{
							var isPush = false;
							if (current == stackTop)
							{
								quoters.Pop();
							}
							else
							{
								quoters.Push(current);
								isPush = true;
							}

							if (quoters.Count == 0 || isPush && quoters.Count == 1)
							{
								if (currentToken != null)
								{
									if (quoters.Count == 0)
									{
										currentToken.EndIndex = index;
									}
									yield return CutLastToken(ref currentToken);
								}

								if (quoters.Count == 0)
								{
									continue;
								}
							}
						}
						else if (char.IsLetter(current) && lastNonWhitespaceChar.HasValue && lastNonWhitespaceChar.Value == '<')
						{
							quoters.Push('<');
						}
						else if (current == '>' && stackTop == '<')
						{
							if (currentToken != null)
							{
								yield return CutLastToken(ref currentToken);
							}

							quoters.Pop();
						}
						else if (current == '(')
						{
							if (quoters.Count > 0)
							{
								quoters.Push(current);
							}
							else
							{
								if (currentToken != null)
								{
									yield return CutLastToken(ref currentToken);
								}

								yield return new Token { Type = TokenType.OpenParenthesis, StartIndex = index, EndIndex = index, };
								continue;
							}
						}
						else if (current == ')')
						{
							if (stackTop == '(')
							{
								quoters.Pop();
							}
							else
							{
								if (quoters.Count != 0)
								{
									throw new UnmatchedParenthesesException();
								}

								if (currentToken != null)
								{
									yield return CutLastToken(ref currentToken);
								}

								yield return new Token { Type = TokenType.CloseParenthesis, StartIndex = index, EndIndex = index, };
								continue;
							}
						}

						if (quoters.Count == 0 && index < expression.Length - 1 && (current == '&' && expression[index + 1] == '&' || current == '|' && expression[index + 1] == '|'))
						{
							if (currentToken != null)
							{
								yield return CutLastToken(ref currentToken);
							}

							yield return new Token { Type = current == '&' ? TokenType.And : TokenType.Or, StartIndex = index, EndIndex = index + 1, };
							index++;
							continue;
						}

						if (currentToken == null)
						{
							currentToken = new Token { Type = TokenType.String, StartIndex = index, EndIndex = index, };
						}
						else
						{
							currentToken.EndIndex = index;
						}
					}
					finally
					{
						if (!isWhitespace)
						{
							lastNonWhitespaceChar = current;
						}
					}
				}

				if (quoters.Count > 0)
				{
					throw new UnmatchedParenthesesException();
				}

				if (currentToken != null)
				{
					yield return currentToken;
				}

				yield return new Token { Type = TokenType.End, };
			}

			static Token CutLastToken(ref Token token)
			{
				var result = token;
				token = null;
				return result;
			}
		}

		public static IMacroBooleanExpressionClause Split(ZString condition)
		{
			try
			{
				if (WorkflowDataRegistry.Instance.EnableUserDefinedConditionFastFailing.Value)
				{
					return SplitCore(condition);
				}
			}
			catch (UnmatchedParenthesesException)
			{
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error in UserDefinedConditionSplitter", $"Failed on UdfCondition: {condition}", e);
			}

			return new MacroBooleanExpressionClause(condition);
		}

		static IMacroBooleanExpressionClause SplitCore(ZString conditionString)
		{
			if (conditionString.IsEmpty)
			{
				return new MacroBooleanExpressionClause(conditionString);
			}

			var tokenizer = new UserDefinedBooleanExpressionTokenizer();
			var clause = MacroBooleanExpressionClauseEvaluator.CreateClause(tokenizer.Tokenize(conditionString).GetEnumerator(), conditionString);
			return clause;
		}
	}
}

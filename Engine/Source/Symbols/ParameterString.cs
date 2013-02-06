﻿/* 
 * Struct: GregValure.NaturalDocs.Engine.Symbols.ParameterString
 * ____________________________________________________________________________
 * 
 * A struct encapsulating parameters from a symbol, which is a normalized way of representing the parenthetical
 * section of a code element or topic, such as "(int, int)" in "PackageA.PackageB.FunctionC(int, int)".  When
 * generated from prototypes, ParameterStrings only store the types of each parameter, not the names or default 
 * values.
 * 
 * The encoding uses SeparatorChars.Level1.
 */

// This file is part of Natural Docs, which is Copyright © 2003-2012 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using System.Collections.Generic;
using GregValure.NaturalDocs.Engine.Symbols;
using GregValure.NaturalDocs.Engine.Tokenization;


namespace GregValure.NaturalDocs.Engine.Symbols
	{
	public struct ParameterString : IComparable
		{
		
		// Group: Constants
		// __________________________________________________________________________

		/* Constant: SeparatorChar
		 * The character used to separate parameter strings.
		 */
		public const char SeparatorChar = SeparatorChars.Level1;



		// Group: Functions
		// __________________________________________________________________________
		
		
		/* Function: ParameterString
		 */
		private ParameterString (string newParameterString)
			{
			parameterString = newParameterString;
			}


		/* Function: GetEndingParenthesesIndex
		 * If a plain text string ends in parentheses, returns the index of the opening parenthesis character.  Returns -1 otherwise.
		 */
		public static int GetEndingParenthesesIndex (string input)
			{
			if (input == null)
				{  return -1;  }

			input = input.TrimEnd();

			if (input.Length >= 2 && input[input.Length - 1] == ')')
				{
				// We have to count parentheses so it correctly returns "(paren2)" from "text (paren) text2 (paren2)" and not 
				// "(paren) text2 (paren2)".  We also want to handle nested parentheses.

				// The start position LastIndexOfAny() takes is the character at the end of the string to be examined first.
				// The count is the number of characters to examine, so it's one higher as index 4 goes for 5 characters: indexes 0, 1, 2,
				// 3, and 4.  The character at the start position is examined, it's not a limit.
				int nextParen = input.LastIndexOfAny(parenthesesChars, input.Length - 2, input.Length - 1);
				int nesting = 1;
				
				while (nextParen != -1)
					{
					if (input[nextParen] == ')')
						{  nesting++;  }
					else if (input[nextParen] == '(')
						{  
						nesting--;  
						
						if (nesting == 0)
							{  break;  }
						}

					nextParen = input.LastIndexOfAny(parenthesesChars, nextParen - 1, nextParen);
					}

				// We want nextParen to be greater than zero so we don't include cases where the entire title is surrounded
				// by parentheses.
				if (nesting == 0 && nextParen > 0)
					{
					return nextParen;
					}
				}

			return -1;
			}


		/* Function: SplitFromEndingParentheses
		 * If a plain text string ends in parentheses, returns them and the rest of the text as separate strings.  If it doesn't, it will
		 * return the original string and null.
		 */
		public static void SplitFromEndingParentheses (string input, out string output, out string parentheses)
			{
			int index = GetEndingParenthesesIndex(input);

			if (index == -1)
				{
				output = input;
				parentheses = null;
				}
			else
				{
				output = input.Substring(0, index).TrimEnd();
				parentheses = input.Substring(index);
				}
			}


		/* Function: FromExportedString
		 * Creates a ParameterString from the passed string which originally came from another ParameterString object.  This skips 
		 * the normalization stage because it should already be in the proper format.  Only use this when retrieving ParameterStrings
		 * that were stored as plain text in a database or other data file.
		 */
		public static ParameterString FromExportedString (string exportedParameterString)
			{
			if (exportedParameterString != null && exportedParameterString.Length == 0)
				{  exportedParameterString = null;  }

			return new ParameterString(exportedParameterString);
			}


		/* Function: FromParameterTypes
		 * Creates a ParameterString from a list of individual plain text parameter types.  The strings should be the type of each
		 * parameter only and not include the name or default value.
		 */
		public static ParameterString FromParameterTypes (IList<string> parameterStrings)
			{
			if (parameterStrings.Count == 0)
				{  return new ParameterString();  }

			System.Text.StringBuilder output = new System.Text.StringBuilder();

			for (int i = 0; i < parameterStrings.Count; i++)
				{
				if (i > 0)
					{  output.Append(SeparatorChar);  }

				NormalizeAndAppend(parameterStrings[i], output);
				}

			return new ParameterString(output.ToString());
			}


		/* Function: FromParenthesesString
		 * Creates a ParameterString from a parentheses string, such as "(int, int)".  You can extract them from a plain text string
		 * with <GetEndingParenthesesIndex()>.
		 */
		public static ParameterString FromParenthesesString (string input)
			{
			if (input == null)
				{  throw new NullReferenceException();  }

			input = input.Trim();

			if (input.Length < 2 || input[0] != '(' || input[input.Length - 1] != ')')
				{  throw new FormatException();  }

			input = input.Substring(1, input.Length - 2);  // Strip surrounding parentheses.
			input = input.Trim();

			if (input == "")
				{  return new ParameterString();  }

			System.Text.StringBuilder output = new System.Text.StringBuilder(input.Length);

			// Ignore separators appearing within braces.  We've already filtered out the surrounding parentheses and we shouldn't
			// have to worry about quotes because we should only have types, not default values.
			Collections.SafeStack<char> braces = new Collections.SafeStack<char>();

			int startParam = 0;
			int index = input.IndexOfAny(bracesAndParamSeparators);

			while (index != -1)
				{
				char character = input[index];

				if (character == '(' || character == '[' || character == '{' || character == '<')
					{  
					braces.Push(character);
					}

				else if ( (character == ')' && braces.Peek() == '(') ||
							  (character == ']' && braces.Peek() == '[') ||
							  (character == '}' && braces.Peek() == '{') ||
							  (character == '>' && braces.Peek() == '<') )
					{
					braces.Pop();
					}
				else if ((character == ',' || character == ';') && braces.Count == 0)
					{
					NormalizeAndAppend(input.Substring(startParam, index - startParam), output);
					output.Append(SeparatorChar);
					startParam = index + 1;
					}

				index = input.IndexOfAny(bracesAndParamSeparators, index + 1);
				}

			NormalizeAndAppend(input.Substring(startParam), output);

			return new ParameterString(output.ToString());
			}


		/* Function: GetParameter
		 * Returns the bounds of the numbered parameter and whether or not it exists.  Numbers start at zero.
		 */
		public bool GetParameter (int parameterIndex, out SimpleTokenIterator start, out SimpleTokenIterator end)
			{
			if (parameterString == null)
				{
				start = new SimpleTokenIterator();
				end = new SimpleTokenIterator();
				return false;
				}

			int parameterCharIndex = 0;

			for (int i = 0; i < parameterIndex; i++)
				{
				int separatorCharIndex = parameterString.IndexOf(SeparatorChar, parameterCharIndex);

				if (separatorCharIndex == -1)
					{  
					start = new SimpleTokenIterator();
					end = new SimpleTokenIterator();
					return false;
					}

				parameterCharIndex = separatorCharIndex + 1;
				}

			int endCharIndex = parameterString.IndexOf(SeparatorChar, parameterCharIndex);

			if (endCharIndex == -1)
				{  endCharIndex = parameterString.Length;  }

			start = new SimpleTokenIterator(parameterString, parameterCharIndex);
			end = new SimpleTokenIterator(parameterString, endCharIndex);
			return true;
			}

			
			
		// Group: Properties
		// __________________________________________________________________________


		/* Property: NumberOfParameters
		 * The number of parameters in the string.
		 */
		public int NumberOfParameters
			{
			get
				{
				if (parameterString == null)
					{  return 0;  }
				else
					{  return parameterString.Count(SeparatorChar) + 1;  }
				}
			}



		// Group: Operators
		// __________________________________________________________________________
		
		
		/* operator: operator string
		 * A cast operator to covert the params to a string.
		 */
		public static implicit operator string (ParameterString p)
			{
			return p.parameterString;
			}
						
		/* Operator: operator ==
		 */
		public static bool operator== (ParameterString a, object b)
			{
			// We need to make the operator compare against object intead of another ParameterString in order to support
			// directly comparing against null.
			return a.Equals(b);
			}

		/* Operator: operator !=
		 */
		public static bool operator!= (ParameterString a, object b)
			{
			return !(a.Equals(b));
			}

		/* Function: ToString
		 * Returns the SymbolString as a string.
		 */
		public override string ToString ()
			{
			return parameterString;
			}
			
		/* Function: GetHashCode
		 */
		public override int GetHashCode ()
			{
			if (parameterString == null)
				{  return 0;  }
			else
				{  return parameterString.GetHashCode();  }
			}

		/* Function: Equals
		 */
		public override bool Equals (object other)
			{
			if (other == null)
				{  return (parameterString == null);  }
			else if (other is ParameterString)
				{  return (parameterString == ((ParameterString)other).parameterString);  }
			else if (other is string)
				{  return (parameterString == (string)other);  }
			else
				{  return false;  }
			}
			
		/* Function: CompareTo
		 */
		public int CompareTo (object other)
			{
			return parameterString.CompareTo(other);
			}
		
			
		
		// Group: Private Functions
		// __________________________________________________________________________


		/* Function: NormalizeAndAppend
		 * 
		 * Normalizes the individual parameter and appends it to the passed StringBuilder.  It does not append a <SeparatorChar>,
		 * that must be done by the calling code.
		 * 
		 *		- Applies canonical normalization to Unicode (FormC).
		 *		- Removes all existing instances of the <SeparatorChars>.
		 *		- Whitespace is removed unless it is between two text characters as defined by <Tokenizer.FundamentalTypeOf()>.
		 *		- Whitespace not removed is condensed into a single space.
		 *		- Unlike <SymbolString>, does NOT replace the common package separator symbols (. :: ->) with <SeparatorChar>.
		 */
		private static void NormalizeAndAppend (string parameter, System.Text.StringBuilder output)
			{
			if (parameter == null)
				{  return;  }

			parameter = parameter.Trim();

			if (parameter == "")
				{  return;  }
				
			parameter = parameter.Normalize(System.Text.NormalizationForm.FormC);  // Canonical decomposition and recombination

			int nextChar = parameter.IndexOfAny(separatorCharsAndWhitespace);
			int index = 0;
			
			// Set to true if we just passed whitespace, since we only want to add it to the normalized string if it's between two 
			// text characters.  We also want to condense multiple characters to a single space.
			bool addWhitespace = false;
			
			while (nextChar != -1)
				{
				if (nextChar > index)
					{
					if (addWhitespace && output.Length > 0 &&
						Tokenizer.FundamentalTypeOf( output[output.Length - 1] ) == FundamentalType.Text &&
						Tokenizer.FundamentalTypeOf( parameter[index] ) == FundamentalType.Text)
						{
						output.Append(' ');
						}
					
					output.Append(parameter, index, nextChar - index);
					addWhitespace = false;
					}

				if (parameter[nextChar] >= SeparatorChars.LowestValue && parameter[nextChar] <= SeparatorChars.HighestValue)
					{
					// Ignore, doesn't affect anything.
					index = nextChar + 1;
					}				
				else if (parameter[nextChar] == ' ' || parameter[nextChar] == '\t')
					{  
					addWhitespace = true;
					index = nextChar + 1;  
					}

				nextChar = parameter.IndexOfAny(separatorCharsAndWhitespace, index);
				}
			
			if (index < parameter.Length)
				{
				if (addWhitespace && output.Length > 0 &&
					Tokenizer.FundamentalTypeOf( output[output.Length - 1] ) == FundamentalType.Text &&
					Tokenizer.FundamentalTypeOf( parameter[index] ) == FundamentalType.Text)
					{
					output.Append(' ');
					}
				
				output.Append(parameter, index, parameter.Length - index);
				}
			}



		// Group: Variables
		// __________________________________________________________________________
		
		
		/* string: parameterString
		 * The parameter string, _always_ in normalized form.
		 */
		private string parameterString;
	


		// Group: Static Variables
		// __________________________________________________________________________


		/* var: separatorCharAndWhitespace
		 * An array containing the whitespace and separator characters.
		 */
		static private char[] separatorCharsAndWhitespace = new char[] { ' ', '\t', 
																																SeparatorChars.Level1, SeparatorChars.Level2,
																																SeparatorChars.Level3, SeparatorChars.Level4 };

		/* var: bracesAndParamSeparators
		 * An array containing all forms of braces, comma, and semicolon.
		 */
		static private char[] bracesAndParamSeparators = new char[] { '(', '[', '{', '<', ')', ']', '}', '>', ',', ';' };

		/* var: parenthesesChars
		 * An array containing the opening and closing parentheses characters.
		 */
		static private char[] parenthesesChars = new char[] { '(', ')' };

		}
	}
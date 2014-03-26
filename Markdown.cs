﻿/* This is a modified version of Jeff Atwood's "MarkdownSharp" (original comments preserved below).  The only
 * real changes to this file were to reorganize the (important) regexes to a central location and mark them all
 * as internally visible.
 * 
 * Since the original is MIT-licensed (license is below), whatever updates I'm making are as well.
 */

/*
 * MarkdownSharp
 * -------------
 * a C# Markdown processor
 * 
 * Markdown is a text-to-HTML conversion tool for web writers
 * Copyright (c) 2004 John Gruber
 * http://daringfireball.net/projects/markdown/
 * 
 * Markdown.NET
 * Copyright (c) 2004-2009 Milan Negovan
 * http://www.aspnetresources.com
 * http://aspnetresources.com/blog/markdown_announced.aspx
 * 
 * MarkdownSharp
 * Copyright (c) 2009 Jeff Atwood
 * http://stackoverflow.com
 * http://www.codinghorror.com/blog/
 * http://block.google.com/p/markdownsharp/
 * 
 * History: Milan ported the Markdown processor to C#. He granted license to me so I can open source it
 * and let the community contribute to and improve MarkdownSharp.
 * 
 */

#region Copyright and license

/*

Copyright (c) 2009 Jeff Atwood

http://www.opensource.org/licenses/mit-license.php
  
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Copyright (c) 2003-2004 John Gruber
<http://daringfireball.net/>   
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

* Redistributions of source block must retain the above copyright notice,
  this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright
  notice, this list of conditions and the following disclaimer in the
  documentation and/or other materials provided with the distribution.

* Neither the name "Markdown" nor the names of its contributors may
  be used to endorse or promote products derived from this software
  without specific prior written permission.

This software is provided by the copyright holders and contributors "as
is" and any express or implied warranties, including, but not limited
to, the implied warranties of merchantability and fitness for a
particular purpose are disclaimed. In no event shall the copyright owner
or contributors be liable for any direct, indirect, incidental, special,
exemplary, or consequential damages (including, but not limited to,
procurement of substitute goods or services; loss of use, data, or
profits; or business interruption) however caused and on any theory of
liability, whether in contract, strict liability, or tort (including
negligence or otherwise) arising in any way out of the use of this
software, even if advised of the possibility of such damage.
*/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.Text;
using System.Linq;

namespace MarkdownSharp
{


    /// <summary>
    /// Markdown is a text-to-HTML conversion tool for web writers. 
    /// Markdown allows you to write using an easy-to-read, easy-to-write plain text format, 
    /// then convert it to structurally valid XHTML (or HTML).
    /// </summary>
    public class Markdown
    {

        #region Configurable options

        /// <summary>
        /// use ">" for HTML output, or " />" for XHTML output
        /// </summary>
        public static string EmptyElementSuffix
        {
            get { return _emptyElementSuffix; }
            set { _emptyElementSuffix = value; }
        }
        private static string _emptyElementSuffix = " />";

        /// <summary>
        /// Tabs are automatically converted to spaces as part of the transform  
        /// this variable determines how "wide" those tabs become in spaces  
        /// WARNING: this configuration option does NOT work yet!
        /// </summary>
        public static int TabWidth
        {
            get { return _tabWidth; }
            set { _tabWidth = value; }
        }
        private static int _tabWidth = 4;

        /// <summary>
        /// when false, email addresses will never be auto-linked  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public static bool LinkEmails
        {
            get { return _linkEmails; }
            set { _linkEmails = value; }
        }
        private static bool _linkEmails = true;

        /// <summary>
        /// when true, bold and italic require non-word characters on either side  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public static bool StrictBoldItalic
        {
            get { return _strictBoldItalic; }
            set { _strictBoldItalic = value; }
        }
        private static bool _strictBoldItalic = false;

        /// <summary>
        /// when true, RETURN becomes a literal newline  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public static bool AutoNewLines
        {
            get { return _autoNewlines; }
            set { _autoNewlines = value; }
        }
        private static bool _autoNewlines = false;

        /// <summary>
        /// when true, (most) bare plain URLs are auto-hyperlinked  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public static bool AutoHyperlink
        {
            get { return _autoHyperlink; }
            set { _autoHyperlink = value; }
        }
        private static bool _autoHyperlink = false;

        /// <summary>
        /// when true, problematic URL characters like [, ], (, and so forth will be encoded 
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        public static bool EncodeProblemUrlCharacters
        {
            get { return _encodeProblemUrlCharacters; }
            set { _encodeProblemUrlCharacters = value; }
        }
        private static bool _encodeProblemUrlCharacters = false;

        #endregion

        private string documenPath;

        private enum HTMLTokenType { Text, Tag }

        private struct HTMLToken
        {
            public HTMLToken(HTMLTokenType type, string value)
            {
                this.Type = type;
                this.Value = value;
            }
            public HTMLTokenType Type;
            public string Value;
        }

        #region Regexes and static setup

        internal static readonly Dictionary<string, string> EscapeTable;
        internal static readonly Dictionary<string, string> BackslashEscapeTable;

        /// <summary>
        /// Static constructor
        /// </summary>
        /// <remarks>
        /// In the static constuctor we'll initialize what stays the same across all transforms.
        /// </remarks>
        static Markdown()
        {
            // Table of hash values for escaped characters:
            EscapeTable = new Dictionary<string, string>();
            // Table of hash value for backslash escaped characters:
            BackslashEscapeTable = new Dictionary<string, string>();

            foreach (char c in @"\`*_{}[]()>#+-.!")
            {
                string key = c.ToString();
                string hash = key.GetHashCode().ToString();
                EscapeTable.Add(key, hash);
                BackslashEscapeTable.Add(@"\" + key, hash);
            }

        }
        
        internal static Regex _blankLines = new Regex(@"^[ \t]+$", RegexOptions.Multiline | RegexOptions.Compiled);
        internal static Regex _newlinesLeadingTrailing = new Regex(@"^\n+|\n+\z", RegexOptions.Compiled);
        internal static Regex _newlinesMultiple = new Regex(@"\n{2,}", RegexOptions.Compiled);
        internal static Regex _leadingWhitespace = new Regex(@"^([ \t]*)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        internal static Regex _entireLines = new Regex(@"^.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        // Lists

        internal const string MarkerUL = @"[*+-]";
        internal const string MarkerOL = @"\d+[.]";
        
        internal static string WholeListRegex = string.Format(@"
            (                               # $1 = whole list
              (                             # $2
                [ ]{{0,{1}}}
                ({0})                       # $3 = first list item marker
                [ \t]+
              )
              (?s:.+?)
              (                             # $4
                  \z
                |
                  \n{{2,}}
                  (?=\S)
                  (?!                       # Negative lookahead for another list item marker
                    [ \t]*
                    {0}[ \t]+
                  )
              )
            )", string.Format("(?:{0}|{1})", MarkerUL, MarkerOL), _tabWidth - 1);

        internal static Regex ListNestedRegex = new Regex(@"^" + WholeListRegex,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        internal static Regex ListTopLevelRegex = new Regex(@"(?:(?<=\n\n)|\A\n?)" + WholeListRegex,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        // Links

        internal static Regex LinkDefRegex = new Regex(string.Format(@"
                ^[ ]{{0,{0}}}\[(.+)\]:  # id = $1
                  [ \t]*
                  \n?                   # maybe *one* newline
                  [ \t]*
                <?(\S+?)>?              # url = $2
                  [ \t]*
                  \n?                   # maybe one newline
                  [ \t]*
                (?:
                    (?<=\s)             # lookbehind for whitespace
                    [\x22(]
                    (.+?)               # title = $3
                    [\x22)]
                    [ \t]*
                )?                      # title is optional
                (?:\n+|\Z)", _tabWidth - 1), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        
        // Anchors

        internal static Regex AnchorRefRegex = new Regex(string.Format(@"
            (                               # wrap whole match in $1
                \[
                    ({0})                   # link text = $2
                \]

                [ ]?                        # one optional space
                (?:\n[ ]*)?                 # one optional newline followed by spaces

                \[
                    (.*?)                   # id = $3
                \]
            )", GetNestedBracketsPattern()), RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        internal static Regex AnchorInlineRegex = new Regex(string.Format(@"
                (                           # wrap whole match in $1
                    \[
                        ({0})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ \t]*
                        ({1})               # href = $3
                        [ \t]*
                        (                   # $4
                        (['\x22])           # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        [ \t]*              # ignore any spaces/tabs between closing quote and )
                        )?                  # title is optional
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPattern()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        internal static Regex AnchorRefShortcutRegex = new Regex(@"
            (                               # wrap whole match in $1
              \[
                 ([^\[\]]+)                 # link text = $2; can't contain [ or ]
              \]
            )", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


        // Images

        internal static Regex ImagesRefRegex = new Regex(@"
                    (               # wrap whole match in $1
                    !\[
                        (.*?)       # alt text = $2
                    \]

                    [ ]?            # one optional space
                    (?:\n[ ]*)?     # one optional newline followed by spaces

                    \[
                        (.*?)       # id = $3
                    \]

                    )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        internal static Regex ImagesInlineRegex = new Regex(String.Format(@"
              (                     # wrap whole match in $1
                !\[
                    (.*?)           # alt text = $2
                \]
                \s?                 # one optional whitespace character
                \(                  # literal paren
                    [ \t]*
                    ({0})           # href = $3
                    [ \t]*
                    (               # $4
                    (['\x22])       # quote char = $5
                    (.*?)           # title = $6
                    \5              # matching quote
                    [ \t]*
                    )?              # title is optional
                \)
              )", GetNestedParensPattern()),
                  RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);


        // Headers
        
        internal static Regex HeaderSetextRegex = new Regex(@"
                ^(.+?)
                [ \t]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ \t]*
                \n+",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        internal static Regex HeaderAtxRegex = new Regex(@"
                ^(\#{1,6})  # $1 = string of #'s
                [ \t]*
                (.+?)       # $2 = Header text
                [ \t]*
                \#*         # optional closing #'s (not counted)
                (?:\z|\n+)",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


        // Horizontal rule

        internal static Regex HorizontalRulesRegex = new Regex(@"
            ^[ ]{0,3}         # Leading space
                ([-*_])       # $1: First marker
                (?>           # Repeated marker group
                    [ ]{0,2}  # Zero, one, or two spaces.
                    \1        # Marker character
                ){2,}         # Group repeated at least twice
                [ ]*          # Trailing spaces
                $             # End of line.
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        // Blockquote
        
        internal static Regex BlockquoteRegex = new Regex(@"
            (                           # Wrap whole match in $1
                (
                ^[ \t]*>[ \t]?          # '>' at the start of a line
                    .+\n                # rest of the first line
                (.+\n)*                 # subsequent consecutive lines
                \n*                     # blanks
                )+
            )", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);


        // Bold/italic
        
        internal static Regex BoldRegex = new Regex(
            _strictBoldItalic ?
            @"([\W_]|^) (\*\*|__) (?=\S) ([^\r]*?\S[\*_]*) \2 ([\W_]|$)" :
            @"(\*\*|__) (?=\S) (.+?[*_]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        internal static Regex ItalicRegex = new Regex(
            _strictBoldItalic ?
            @"([\W_]|^) (\*|_) (?=\S) ([^\r\*_]*?\S) \2 ([\W_]|$)" :
            @"(\*|_) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);


        // Links

        internal static Regex AutolinkBareRegex = new Regex(@"(^|\s)(https?|ftp)(://[-A-Z0-9+&@#/%?=~_|\[\]\(\)!:,\.;]*[-A-Z0-9+&@#/%=~_|\[\]])($|\W)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        
        // Code

        internal static Regex CodeBlockRegex = new Regex(string.Format(@"
                    (?:\n\n|\A)
                    (                        # $1 = the code block -- one or more lines, starting with a space/tab
                    (?:
                        (?:[ ]{{{0}}} | \t)  # Lines must start with a tab or a tab-width of spaces
                        .*\n+
                    )+
                    )
                    ((?=^[ ]{{0,{0}}}\S)|\Z) # Lookahead for non-space at line-start, or end of doc",
                    _tabWidth), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        
        internal static Regex CodeSpanRegex = new Regex(@"
                    (?<!\\)   # Character before opening ` can't be a backslash
                    (`+)      # $1 = Opening run of `
                    (.+?)     # $2 = The code block
                    (?<!`)
                    \1
                    (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);


        // HTML

        internal static Regex BlocksHtmlRegex = new Regex(GetBlockPattern(), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        private static string GetBlockPattern()
        {

            // Hashify HTML blocks:
            // We only want to do this for block-level HTML tags, such as headers,
            // lists, and tables. That's because we still want to wrap <p>s around
            // "paragraphs" that are wrapped in non-block-level tags, such as anchors,
            // phrase emphasis, and spans. The list of tags we're looking for is
            // hard-coded:
            //
            // *  List "a" is made of tags which can be both inline or block-level.
            //    These will be treated block-level when the start tag is alone on 
            //    its line, otherwise they're not matched here and will be taken as 
            //    inline later.
            // *  List "b" is made of tags which are always block-level;
            //
            string blockTagsA = "ins|del";
            string blockTagsB = "p|div|h[1-6]|blockquote|pre|table|dl|ol|ul|address|script|noscript|form|fieldset|iframe|math";

            // Regular expression for the content of a block tag.
            string attr = @"
            (?>				            # optional tag attributes
              \s			            # starts with whitespace
              (?>
                [^>""/]+	            # text outside quotes
              |
                /+(?!>)		            # slash not followed by >
              |
                ""[^""]*""		        # text inside double quotes (tolerate >)
              |
                '[^']*'	                # text inside single quotes (tolerate >)
              )*
            )?	
            ";

            string content = RepeatString(@"
                (?>
                  [^<]+			        # content without tag
                |
                  <\2			        # nested opening tag
                    " + attr + @"       # attributes
                  (?>
                      />
                  |
                      >", _nestDepth) +   // end of opening tag
                      ".*?" +             // last level nested tag content
            RepeatString(@"
                      </\2\s*>	        # closing nested tag
                  )
                  |				
                  <(?!/\2\s*>           # other tags with a different name
                  )
                )*", _nestDepth);

            string content2 = content.Replace(@"\2", @"\3");

            // First, look for nested blocks, e.g.:
            // 	<div>
            // 		<div>
            // 		tags for inner block must be indented.
            // 		</div>
            // 	</div>
            //
            // The outermost tags must start at the left margin for this to match, and
            // the inner nested divs must be indented.
            // We need to do this before the next, more liberal match, because the next
            // match will start at the first `<div>` and stop at the first `</div>`.
            string pattern = @"
            (?>
                  (?>
                    (?<=\n)     # Starting after a blank line
                    |           # or
                    \A\n?       # the beginning of the doc
                  )
                  (             # save in $1

                    # Match from `\n<tag>` to `</tag>\n`, handling nested tags 
                    # in between.
                      
                        [ ]{0,$less_than_tab}
                        <($block_tags_b_re)   # start tag = $2
                        $attr>                # attributes followed by > and \n
                        $content              # content, support nesting
                        </\2>                 # the matching end tag
                        [ ]*                  # trailing spaces/tabs
                        (?=\n+|\Z)            # followed by a newline or end of document

                  | # Special version for tags of group a.

                        [ ]{0,$less_than_tab}
                        <($block_tags_a_re)   # start tag = $3
                        $attr>[ ]*\n          # attributes followed by >
                        $content2             # content, support nesting
                        </\3>                 # the matching end tag
                        [ ]*                  # trailing spaces/tabs
                        (?=\n+|\Z)            # followed by a newline or end of document
                      
                  | # Special case just for <hr />. It was easier to make a special 
                    # case than to make the other regex more complicated.
                  
                        [ ]{0,$less_than_tab}
                        <(hr)                 # start tag = $2
                        $attr                 # attributes
                        /?>                   # the matching end tag
                        [ ]*
                        (?=\n{2,}|\Z)         # followed by a blank line or end of document
                  
                  | # Special case for standalone HTML comments:
                  
                      [ ]{0,$less_than_tab}
                      (?s:
                        <!-- .*? -->
                      )
                      [ ]*
                      (?=\n{2,}|\Z)            # followed by a blank line or end of document
                  
                  | # PHP and ASP-style processor instructions (<? and <%)
                  
                      [ ]{0,$less_than_tab}
                      (?s:
                        <([?%])                # $2
                        .*?
                        \2>
                      )
                      [ ]*
                      (?=\n{2,}|\Z)            # followed by a blank line or end of document
                      
                  )
            )";

            pattern = pattern.Replace("$less_than_tab", (_tabWidth - 1).ToString());
            pattern = pattern.Replace("$block_tags_b_re", blockTagsB);
            pattern = pattern.Replace("$block_tags_a_re", blockTagsA);
            pattern = pattern.Replace("$attr", attr);
            pattern = pattern.Replace("$content2", content2);
            pattern = pattern.Replace("$content", content);

            return pattern;
        }

        internal static Regex HtmlTokensRegex = new Regex(@"
            (<!(?:--.*?--\s*)+>)|        # match <!-- foo -->
            (<\?.*?\?>)|                 # match <?foo?> " +
            RepeatString(@" 
            (<[A-Za-z\/!$](?:[^<>]|", _nestDepth) + RepeatString(@")*>)", _nestDepth) +
                                       " # match <tag> and </tag>",
            RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// maximum nested depth of [] and () supported by the transform; implementation detail
        /// </summary>
        private const int _nestDepth = 6;

        private readonly Dictionary<string, string> _urls = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _titles = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _htmlBlocks = new Dictionary<string, string>();

        private int _listLevel;

        /// <summary>
        /// current version of MarkdownSharp;  
        /// see http://block.google.com/p/markdownsharp/ for the latest block or to contribute
        /// </summary>
        public string Version
        {
            get { return "1.009"; }
        }

        /// <summary>
        /// Set the path of the document to be transformed
        /// </summary>
        /// <param name="path">document path</param>
        public void SetPathOfDocumentToTransform(string path)
        {
            this.documenPath = path;
        }

        /// <summary>
        /// Transforms the provided Markdown-formatted text to HTML;  
        /// see http://en.wikipedia.org/wiki/Markdown
        /// </summary>
        /// <remarks>
        /// The order in which other subs are called here is
        /// essential. Link and image substitutions need to happen before
        /// EscapeSpecialChars(), so that any *'s or _'s in the a
        /// and img tags get encoded.
        /// </remarks>
        public string Transform(string text)
        {
            if (text == null) return "";

            Setup();

            // Standardize line endings
            text = text.Replace("\r\n", "\n");    // DOS to Unix
            text = text.Replace("\r", "\n");      // Mac to Unix

            // Make sure $text ends with a couple of newlines:
            text += "\n\n";

            text = Detab(text);

            // Strip any lines consisting only of spaces and tabs.
            // This makes subsequent regexen easier to write, because we can
            // match consecutive blank lines with /\n+/ instead of something
            // contorted like /[ \t]*\n+/ .
            text = _blankLines.Replace(text, "");

            text = HashHTMLBlocks(text);
            text = StripLinkDefinitions(text);
            text = RunBlockGamut(text);
            text = UnescapeSpecialChars(text);

            Cleanup();

            return text + "\n";
        }


        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private string RunBlockGamut(string text)
        {
            text = DoHeaders(text);
            text = DoHorizontalRules(text);
            text = DoLists(text);
            text = DoCodeBlocks(text);
            text = DoBlockQuotes(text);

            // We already ran HashHTMLBlocks() before, in Markdown(), but that
            // was to escape raw HTML in the original Markdown source. This time,
            // we're escaping the markup we've just created, so that we don't wrap
            // <p> tags around block-level tags.
            text = HashHTMLBlocks(text);

            text = FormParagraphs(text);

            return text;
        }


        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private string RunSpanGamut(string text)
        {
            text = DoCodeSpans(text);
            text = EscapeSpecialCharsWithinTagAttributes(text);
            text = EncodeBackslashEscapes(text);

            // Images must come first, because ![foo][f] looks like an anchor.
            text = DoImages(text);
            text = DoAnchors(text);

            // Must come after DoAnchors(), because you can use < and >
            // delimiters in inline links like [this](<url>).
            text = DoAutoLinks(text);

            text = EncodeAmpsAndAngles(text);
            text = DoItalicsAndBold(text);
            text = DoHardBreaks(text);

            return text;
        }

        private void Setup()
        {
            // Clear the global hashes. If we don't clear these, you get conflicts
            // from other articles when generating a page which contains more than
            // one article (e.g. an index page that shows the N most recent
            // articles):
            _urls.Clear();
            _titles.Clear();
            _htmlBlocks.Clear();
            _listLevel = 0;
        }

        private void Cleanup()
        {
            Setup();
        }

        private static string _nestedBracketsPattern;

        /// <summary>
        /// Reusable pattern to match balanced [brackets]. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedBracketsPattern()
        {
            // in other words [this] and [this[also]] and [this[also[too]]]
            // up to _nestDepth
            if (_nestedBracketsPattern == null)
                _nestedBracketsPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^\[\]]+      # Anything other than brackets
                     |
                       \[
                           ", _nestDepth) + RepeatString(
                    @" \]
                    )*"
                    , _nestDepth);
            return _nestedBracketsPattern;
        }

        private static string _nestedParensPattern;

        /// <summary>
        /// Reusable pattern to match balanced (parens). See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPattern()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if (_nestedParensPattern == null)
                _nestedParensPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()\s]+      # Anything other than parens or whitespace
                     |
                       \(
                           ", _nestDepth) + RepeatString(
                    @" \)
                    )*"
                    , _nestDepth);
            return _nestedParensPattern;
        }

        /// <summary>
        /// Strips link definitions from text, stores the URLs and titles in hash references.
        /// </summary>
        /// <remarks>
        /// ^[id]: url "optional title"
        /// </remarks>
        private string StripLinkDefinitions(string text)
        {
            return LinkDefRegex.Replace(text, new MatchEvaluator(LinkEvaluator));
        }

        private string LinkEvaluator(Match match)
        {
            string linkID = match.Groups[1].Value.ToLowerInvariant();

            // qisun: deal with relative path to render image
            string link = match.Groups[2].Value;
            if (link.StartsWith(".") || link.StartsWith("/"))
            {
                if (!String.IsNullOrEmpty(documenPath))
                {
                    link = Path.Combine(documenPath.Substring(0, documenPath.LastIndexOf('\\')), link.Remove(0, 2).Replace('/', '\\'));
                }
            }
            _urls[linkID] = EncodeAmpsAndAngles(link);

            if (match.Groups[3] != null && match.Groups[3].Length > 0)
                _titles[linkID] = match.Groups[3].Value.Replace("\"", "&quot;");

            return "";
        }

        /// <summary>
        /// replaces any block-level HTML blocks with hash entries
        /// </summary>
        private string HashHTMLBlocks(string text)
        {
            return BlocksHtmlRegex.Replace(text, new MatchEvaluator(HtmlEvaluator));
        }

        private string HtmlEvaluator(Match match)
        {
            string text = match.Groups[1].Value;
            string key = text.GetHashCode().ToString();
            _htmlBlocks[key] = text;

            return string.Concat("\n\n", key, "\n\n");
        }

        /// <summary>
        /// returns an array of HTML tokens comprising the input string. Each token is 
        /// either a tag (possibly with nested, tags contained therein, such 
        /// as &lt;a href="&lt;MTFoo&gt;"&gt;, or a run of text between tags. Each element of the 
        /// array is a two-element array; the first is either 'tag' or 'text'; the second is 
        /// the actual value.
        /// </summary>
        private List<HTMLToken> TokenizeHTML(string text)
        {
            int pos = 0;
            int tagStart = 0;
            var tokens = new List<HTMLToken>();

            // this regex is derived from the _tokenize() subroutine in Brad Choate's MTRegex plugin.
            // http://www.bradchoate.com/past/mtregex.php
            foreach (Match m in HtmlTokensRegex.Matches(text))
            {
                tagStart = m.Index;

                if (pos < tagStart)
                    tokens.Add(new HTMLToken(HTMLTokenType.Text, text.Substring(pos, tagStart - pos)));

                tokens.Add(new HTMLToken(HTMLTokenType.Tag, m.Value));
                pos = tagStart + m.Length;
            }

            if (pos < text.Length)
                tokens.Add(new HTMLToken(HTMLTokenType.Text, text.Substring(pos, text.Length - pos)));

            return tokens;
        }


        /// <summary>
        /// Within tags -- meaning between &lt; and &gt; -- encode [\ ` * _] so they 
        /// don't conflict with their use in Markdown for block, italics and strong. 
        /// We're replacing each such character with its corresponding hash 
        /// value; this is likely overkill, but it should prevent us from colliding 
        /// with the escape values by accident.
        /// </summary>
        private string EscapeSpecialCharsWithinTagAttributes(string text)
        {
            var tokens = TokenizeHTML(text);

            // now, rebuild text from the tokens
            var sb = new StringBuilder(text.Length);

            foreach (var token in tokens)
            {
                string value = token.Value;

                if (token.Type == HTMLTokenType.Tag)
                {
                    value = value.Replace(@"\", EscapeTable[@"\"]);
                    value = Regex.Replace(value, "(?<=.)</?block>(?=.)", EscapeTable[@"`"]);
                    value = EscapeBoldItalic(value);
                }

                sb.Append(value);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Turn Markdown link shortcuts into HTML anchor tags
        /// </summary>
        /// <remarks>
        /// [link text](url "title") 
        /// [link text][id] 
        /// [id] 
        /// </remarks>
        private string DoAnchors(string text)
        {
            // First, handle reference-style links: [link text] [id]
            text = AnchorRefRegex.Replace(text, new MatchEvaluator(AnchorRefEvaluator));

            // Next, inline-style links: [link text](url "optional title") or [link text](url "optional title")
            text = AnchorInlineRegex.Replace(text, new MatchEvaluator(AnchorInlineEvaluator));

            //  Last, handle reference-style shortcuts: [link text]
            //  These must come last in case you've also got [link test][1]
            //  or [link test](/foo)
            text = AnchorRefShortcutRegex.Replace(text, new MatchEvaluator(AnchorRefShortcutEvaluator));
            return text;
        }

        private string AnchorRefEvaluator(Match match)
        {
            string wholeMatch = match.Groups[1].Value;
            string linkText = match.Groups[2].Value;
            string linkID = match.Groups[3].Value.ToLowerInvariant();

            string result;

            // for shortcut links like [this][].
            if (linkID == "")
                linkID = linkText.ToLowerInvariant();

            if (_urls.ContainsKey(linkID))
            {
                string url = _urls[linkID];

                url = EscapeBoldItalic(url);
                url = EncodeProblemUrlChars(url);
                result = "<a href=\"" + url + "\"";

                if (_titles.ContainsKey(linkID))
                {
                    string title = _titles[linkID];
                    title = EscapeBoldItalic(title);
                    result += " title=\"" + title + "\"";
                }

                result += ">" + linkText + "</a>";
            }
            else
                result = wholeMatch;

            return result;
        }

        private string AnchorRefShortcutEvaluator(Match match)
        {
            string wholeMatch = match.Groups[1].Value;
            string linkText = match.Groups[2].Value;
            string linkID = Regex.Replace(linkText.ToLowerInvariant(), @"[ ]*\n[ ]*", " ");  // lower case and remove newlines / extra spaces

            string result;

            if (_urls.ContainsKey(linkID))
            {
                string url = _urls[linkID];

                url = EscapeBoldItalic(url);
                url = EncodeProblemUrlChars(url);
                result = "<a href=\"" + url + "\"";

                if (_titles.ContainsKey(linkID))
                {
                    string title = _titles[linkID];
                    title = EscapeBoldItalic(title);
                    result += " title=\"" + title + "\"";
                }

                result += ">" + linkText + "</a>";
            }
            else
                result = wholeMatch;

            return result;
        }

        /// <summary>
        /// escapes Bold [ * ] and Italic [ _ ] characters
        /// </summary>
        private string EscapeBoldItalic(string s)
        {
            s = s.Replace("*", EscapeTable["*"]);
            s = s.Replace("_", EscapeTable["_"]);
            return s;
        }


        /// <summary>
        /// encodes problem characters in URLs, such as 
        /// * _  and optionally ' () []  :
        /// this is to avoid problems with markup later
        /// </summary>
        private string EncodeProblemUrlChars(string url)
        {
            if (_encodeProblemUrlCharacters)
            {
                url = url.Replace("*", "%2A");
                url = url.Replace("_", "%5F");
                url = url.Replace("'", "%27");
                url = url.Replace("(", "%28");
                url = url.Replace(")", "%29");
                url = url.Replace("[", "%5B");
                url = url.Replace("]", "%5D");
                if (url.Length > 7 && url.Substring(7).Contains(":"))
                {
                    // replace any colons in the body of the URL that are NOT followed by 2 or more numbers
                    url = url.Substring(0, 7) + Regex.Replace(url.Substring(7), @":(?!\d{2,})", "%3A");
                }
            }

            return url;
        }

        private string AnchorInlineEvaluator(Match match)
        {
            string linkText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            string title = match.Groups[6].Value;
            string result;

            url = EscapeBoldItalic(url);
            if (url.StartsWith("<") && url.EndsWith(">"))
                url = url.Substring(1, url.Length - 2); // remove <>'s surrounding URL, if present
            url = EncodeProblemUrlChars(url);

            result = string.Format("<a href=\"{0}\"", url);

            if (!String.IsNullOrEmpty(title))
            {
                title = title.Replace("\"", "&quot;");
                title = EscapeBoldItalic(title);
                result += string.Format(" title=\"{0}\"", title);
            }

            result += string.Format(">{0}</a>", linkText);
            return result;
        }

        /// <summary>
        /// Turn Markdown image shortcuts into HTML img tags. 
        /// </summary>
        /// <remarks>
        /// ![alt text][id]
        /// ![alt text](url "optional title")
        /// </remarks>
        private string DoImages(string text)
        {
            // First, handle reference-style labeled images: ![alt text][id]
            text = ImagesRefRegex.Replace(text, new MatchEvaluator(ImageReferenceEvaluator));

            // Next, handle inline images:  ![alt text](url "optional title")
            // Don't forget: encode * and _
            text = ImagesInlineRegex.Replace(text, new MatchEvaluator(ImageInlineEvaluator));

            return text;
        }

        private string ImageReferenceEvaluator(Match match)
        {
            string wholeMatch = match.Groups[1].Value;
            string altText = match.Groups[2].Value;
            string linkID = match.Groups[3].Value.ToLowerInvariant();
            string result;

            // for shortcut links like ![this][].
            if (linkID == "")
                linkID = altText.ToLowerInvariant();

            altText = altText.Replace("\"", "&quot;");

            if (_urls.ContainsKey(linkID))
            {
                string url = _urls[linkID];
                url = EscapeBoldItalic(url);
                url = EncodeProblemUrlChars(url);
                result = string.Format("<img src=\"{0}\" alt=\"{1}\"", url, altText);

                if (_titles.ContainsKey(linkID))
                {
                    string title = _titles[linkID];
                    title = EscapeBoldItalic(title);

                    result += string.Format(" title=\"{0}\"", title);
                }

                result += _emptyElementSuffix;
            }
            else
            {
                // If there's no such link ID, leave intact:
                result = wholeMatch;
            }

            return result;
        }

        private string ImageInlineEvaluator(Match match)
        {
            string alt = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            string title = match.Groups[6].Value;
            string result;

            alt = alt.Replace("\"", "&quot;");
            title = title.Replace("\"", "&quot;");

            url = EscapeBoldItalic(url);
            if (url.StartsWith("<") && url.EndsWith(">"))
                url = url.Substring(1, url.Length - 2);    // Remove <>'s surrounding URL, if present

            url = EncodeProblemUrlChars(url);

            result = string.Format("<img src=\"{0}\" alt=\"{1}\"", url, alt);

            if (!String.IsNullOrEmpty(title))
            {
                title = EscapeBoldItalic(title);
                result += string.Format(" title=\"{0}\"", title);
            }

            result += _emptyElementSuffix;

            return result;
        }

        /// <summary>
        /// Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        /// Header 1  
        /// ========  
        /// 
        /// Header 2  
        /// --------  
        /// 
        /// # Header 1  
        /// ## Header 2  
        /// ## Header 2 with closing hashes ##  
        /// ...  
        /// ###### Header 6  
        /// </remarks>
        private string DoHeaders(string text)
        {
            text = HeaderSetextRegex.Replace(text, new MatchEvaluator(SetextHeaderEvaluator));
            text = HeaderAtxRegex.Replace(text, new MatchEvaluator(AtxHeaderEvaluator));
            return text;
        }

        private string SetextHeaderEvaluator(Match match)
        {
            string header = match.Groups[1].Value;
            int level = match.Groups[2].Value.StartsWith("=") ? 1 : 2;
            return string.Format("<h{1}>{0}</h{1}>\n\n", RunSpanGamut(header), level);
        }

        private string AtxHeaderEvaluator(Match match)
        {
            string header = match.Groups[2].Value;
            int level = match.Groups[1].Value.Length;
            return string.Format("<h{1}>{0}</h{1}>\n\n", RunSpanGamut(header), level);
        }

        /// <summary>
        /// Turn Markdown horizontal rules into HTML hr tags
        /// </summary>
        /// <remarks>
        /// ***  
        /// * * *  
        /// ---
        /// - - -
        /// </remarks>
        private string DoHorizontalRules(string text)
        {
            return HorizontalRulesRegex.Replace(text, "<hr" + _emptyElementSuffix + "\n");
        }

        /// <summary>
        /// Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        private string DoLists(string text)
        {
            // We use a different prefix before nested lists than top-level lists.
            // See extended comment in _ProcessListItems().
            if (_listLevel > 0)
                text = ListNestedRegex.Replace(text, new MatchEvaluator(ListEvaluator));
            else
                text = ListTopLevelRegex.Replace(text, new MatchEvaluator(ListEvaluator));

            return text;
        }

        private string ListEvaluator(Match match)
        {
            string list = match.Groups[1].Value;
            string listType = Regex.IsMatch(match.Groups[3].Value, MarkerUL) ? "ul" : "ol";
            string result;

            // Turn double returns into triple returns, so that we can make a
            // paragraph for the last item in a list, if necessary:
            list = Regex.Replace(list, @"\n{2,}", "\n\n\n");
            result = ProcessListItems(list, listType == "ul" ? MarkerUL : MarkerOL);

            result = string.Format("<{0}>\n{1}</{0}>\n", listType, result);
            return result;
        }

        /// <summary>
        /// Process the contents of a single ordered or unordered list, splitting it
        /// into individual list items.
        /// </summary>
        private string ProcessListItems(string list, string marker)
        {
            // The listLevel global keeps track of when we're inside a list.
            // Each time we enter a list, we increment it; when we leave a list,
            // we decrement. If it's zero, we're not in a list anymore.

            // We do this because when we're not inside a list, we want to treat
            // something like this:

            //    I recommend upgrading to version
            //    8. Oops, now this line is treated
            //    as a sub-list.

            // As a single paragraph, despite the fact that the second line starts
            // with a digit-period-space sequence.

            // Whereas when we're inside a list (or sub-list), that line will be
            // treated as the start of a sub-list. What a kludge, huh? This is
            // an aspect of Markdown's syntax that's hard to parse perfectly
            // without resorting to mind-reading. Perhaps the solution is to
            // change the syntax rules such that sub-lists must start with a
            // starting cardinal number; e.g. "1." or "a.".

            _listLevel++;

            // Trim trailing blank lines:
            list = Regex.Replace(list, @"\n{2,}\z", "\n");

            string pattern = string.Format(
              @"(\n)?                      # leading line = $1
                (^[ \t]*)                  # leading whitespace = $2
                ({0}) [ \t]+               # list marker = $3
                ((?s:.+?)                  # list item text = $4
                (\n{{1,2}}))      
                (?= \n* (\z | \2 ({0}) [ \t]+))", marker);

            list = Regex.Replace(list, pattern, new MatchEvaluator(ListItemEvaluator),
                                  RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            _listLevel--;
            return list;
        }

        private string ListItemEvaluator(Match match)
        {
            string item = match.Groups[4].Value;
            string leadingLine = match.Groups[1].Value;

            if (!String.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
                // we could correct any bad indentation here..
                item = RunBlockGamut(Outdent(item) + "\n");
            else
            {
                // recursion for sub-lists
                item = DoLists(Outdent(item));
                item = item.TrimEnd('\n');
                item = RunSpanGamut(item);
            }

            return string.Format("<li>{0}</li>\n", item);
        }

        /// <summary>
        /// /// Turn Markdown 4-space indented block into HTML pre block blocks
        /// </summary>
        private string DoCodeBlocks(string text)
        {
            text = CodeBlockRegex.Replace(text, new MatchEvaluator(CodeBlockEvaluator));
            return text;
        }

        private string CodeBlockEvaluator(Match match)
        {
            string codeBlock = match.Groups[1].Value;

            codeBlock = EncodeCode(Outdent(codeBlock));
            codeBlock = Detab(codeBlock);
            codeBlock = _newlinesLeadingTrailing.Replace(codeBlock, "");

            return string.Concat("\n\n<pre><code>", codeBlock, "\n</code></pre>\n\n");
        }

        /// <summary>
        /// Turn Markdown `block spans` into HTML block tags
        /// </summary>
        private string DoCodeSpans(string text)
        {
            //    * You can use multiple backticks as the delimiters if you want to
            //        include literal backticks in the block span. So, this input:
            //
            //        Just type ``foo `bar` baz`` at the prompt.
            //
            //        Will translate to:
            //
            //          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
            //
            //        There's no arbitrary limit to the number of backticks you
            //        can use as delimters. If you need three consecutive backticks
            //        in your block, use four for delimiters, etc.
            //
            //    * You can use spaces to get literal backticks at the edges:
            //
            //          ... type `` `bar` `` ...
            //
            //        Turns to:
            //
            //          ... type <code>`bar`</code> ...         
            //

            return CodeSpanRegex.Replace(text, new MatchEvaluator(CodeSpanEvaluator));
        }

        private string CodeSpanEvaluator(Match match)
        {
            string span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ \t]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ \t]*$", ""); // trailing whitespace
            span = EncodeCode(span);

            return string.Concat("<code>", span, "</code>");
        }


        /// <summary>
        /// Encode/escape certain characters inside Markdown block runs.
        /// </summary>
        /// <remarks>
        /// The point is that in block, these characters are literals, and lose their 
        /// special Markdown meanings.
        /// </remarks>
        private string EncodeCode(string code)
        {
            // Encode all ampersands; HTML entities are not
            // entities within a Markdown block span.
            code = code.Replace("&", "&amp;");

            // Do the angle bracket song and dance
            code = code.Replace("<", "&lt;");
            code = code.Replace(">", "&gt;");

            // Now, escape characters that are magic in Markdown
            code = code.Replace(@"\", EscapeTable[@"\"]);
            code = code.Replace("*", EscapeTable["*"]);
            code = code.Replace("_", EscapeTable["_"]);
            code = code.Replace("{", EscapeTable["{"]);
            code = code.Replace("}", EscapeTable["}"]);
            code = code.Replace("[", EscapeTable["["]);
            code = code.Replace("]", EscapeTable["]"]);

            return code;
        }

        /// <summary>
        /// Turn Markdown *italics* and **bold** into HTML strong and em tags
        /// </summary>
        private string DoItalicsAndBold(string text)
        {
            // <strong> must go first:
            text = BoldRegex.Replace(text, _strictBoldItalic ? "$1<strong>$3</strong>$4" : "<strong>$2</strong>");
            // Then <em>:
            text = ItalicRegex.Replace(text, _strictBoldItalic ? "$1<em>$3</em>$4" : "<em>$2</em>");
            return text;
        }

        /// <summary>
        /// Turn markdown line breaks (two space at end of line) into HTML break tags
        /// </summary>
        private string DoHardBreaks(string text)
        {
            if (_autoNewlines)
                text = Regex.Replace(text, @"\n", string.Format("<br{0}\n", _emptyElementSuffix));
            else
                text = Regex.Replace(text, @" {2,}\n", string.Format("<br{0}\n", _emptyElementSuffix));
            return text;
        }

        /// <summary>
        /// Turn Markdown > quoted blocks into HTML blockquote blocks
        /// </summary>
        private string DoBlockQuotes(string text)
        {
            return BlockquoteRegex.Replace(text, new MatchEvaluator(BlockQuoteEvaluator));
        }

        private string BlockQuoteEvaluator(Match match)
        {
            string bq = match.Groups[1].Value;

            bq = Regex.Replace(bq, @"^[ \t]*>[ \t]?", "", RegexOptions.Multiline);   // trim one level of quoting
            bq = Regex.Replace(bq, @"^[ \t]+$", "", RegexOptions.Multiline);         // trim whitespace-only lines
            bq = RunBlockGamut(bq);                                                  // recurse

            bq = Regex.Replace(bq, @"^", "  ", RegexOptions.Multiline);

            // These leading spaces screw with <pre> content, so we need to fix that:
            bq = Regex.Replace(bq, @"(\s*<pre>.+?</pre>)", new MatchEvaluator(BlockQuoteEvaluator2), RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

            return string.Format("<blockquote>\n{0}\n</blockquote>\n\n", bq);
        }

        private string BlockQuoteEvaluator2(Match match)
        {
            return Regex.Replace(match.Groups[1].Value, @"^  ", "", RegexOptions.Multiline);
        }

        /// <summary>
        /// removes leading and trailing newlines, splits on two or more newlines, to form "paragraphs".  
        /// each paragraph is then unhashed (if it is a hash) or wrapped in HTML p tags
        /// </summary>
        private string FormParagraphs(string text)
        {
            text = _newlinesLeadingTrailing.Replace(text, "");

            string[] grafs = _newlinesMultiple.Split(text);

            // Wrap <p> tags.
            for (int i = 0; i < grafs.Length; i++)
            {
                if (!_htmlBlocks.ContainsKey(grafs[i]))
                {
                    string block = grafs[i];

                    block = RunSpanGamut(block);
                    block = _leadingWhitespace.Replace(block, "<p>");
                    block += "</p>";

                    grafs[i] = block;
                }
            }

            // Unhashify HTML blocks
            for (int i = 0; i < grafs.Length; i++)
            {
                if (_htmlBlocks.ContainsKey(grafs[i]))
                    grafs[i] = _htmlBlocks[grafs[i]];
            }

            return string.Join("\n\n", grafs);
        }

        /// <summary>
        /// Turn angle-delimited URLs into HTML anchor tags
        /// </summary>
        /// <remarks>
        /// &lt;http://www.example.com&gt;
        /// </remarks>
        private string DoAutoLinks(string text)
        {

            if (_autoHyperlink)
            {
                // fixup arbitrary URLs by adding Markdown < > so they get linked as well
                // note that at this point, all other URL in the text are already hyperlinked as <a href=""></a>
                // *except* for the <http://www.foo.com> case
                text = AutolinkBareRegex.Replace(text, @"$1<$2$3>$4");
            }

            // Hyperlinks: <http://foo.com>
            text = Regex.Replace(text, "<((https?|ftp):[^'\">\\s]+)>", new MatchEvaluator(HyperlinkEvaluator));

            if (_linkEmails)
            {
                // Email addresses: <address@domain.foo>
                string pattern =
                    @"<
                      (?:mailto:)?
                      (
                        [-.\w]+
                        \@
                        [-a-z0-9]+(\.[-a-z0-9]+)*\.[a-z]+
                      )
                      >";
                text = Regex.Replace(text, pattern, new MatchEvaluator(EmailEvaluator), RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }

            return text;
        }

        private string HyperlinkEvaluator(Match match)
        {
            string link = match.Groups[1].Value;
            return string.Format("<a href=\"{0}\">{0}</a>", link);
        }

        private string EmailEvaluator(Match match)
        {
            string email = UnescapeSpecialChars(match.Groups[1].Value);

            //
            //    Input: an email address, e.g. "foo@example.com"
            //
            //    Output: the email address as a mailto link, with each character
            //            of the address encoded as either a decimal or hex entity, in
            //            the hopes of foiling most address harvesting spam bots. E.g.:
            //
            //      <a href="&#x6D;&#97;&#105;&#108;&#x74;&#111;:&#102;&#111;&#111;&#64;&#101;
            //        x&#x61;&#109;&#x70;&#108;&#x65;&#x2E;&#99;&#111;&#109;">&#102;&#111;&#111;
            //        &#64;&#101;x&#x61;&#109;&#x70;&#108;&#x65;&#x2E;&#99;&#111;&#109;</a>
            //
            //    Based by a filter by Matthew Wickline, posted to the BBEdit-Talk
            //    mailing list: <http://tinyurl.com/yu7ue>
            //
            email = "mailto:" + email;

            // leave ':' alone (to spot mailto: later) 
            email = EncodeEmailAddress(email);

            email = string.Format("<a href=\"{0}\">{0}</a>", email);

            // strip the mailto: from the visible part
            email = Regex.Replace(email, "\">.+?:", "\">");
            return email;
        }

        /// <summary>
        /// encodes email address randomly  
        /// roughly 10% raw, 45% hex, 45% dec 
        /// note that @ is always encoded and : never is
        /// </summary>
        private string EncodeEmailAddress(string addr)
        {
            var sb = new StringBuilder(addr.Length * 5);
            var rand = new Random();
            int r;
            foreach (char c in addr)
            {
                r = rand.Next(1, 100);
                if ((r > 90 || c == ':') && c != '@')
                    sb.Append(c);                         // m
                else if (r < 45)
                    sb.AppendFormat("&#x{0:x};", (int)c); // &#x6D
                else
                    sb.AppendFormat("&#{0};", (int)c);    // &#109
            }
            return sb.ToString();
        }


        private static Regex _amps = new Regex(@"&(?!(#[0-9]+)|(#[xX][a-fA-F0-9])|([a-zA-Z][a-zA-Z0-9]*);)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static Regex _angles = new Regex(@"<(?![A-Za-z/?\$!])", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        /// <summary>
        /// Encode any ampersands (that aren't part of an HTML entity) and left or right angle brackets
        /// </summary>
        private string EncodeAmpsAndAngles(string text)
        {
            text = _amps.Replace(text, "&amp;");
            text = _angles.Replace(text, "&lt;");
            return text;
        }

        /// <summary>
        /// Encodes any escaped characters such as \`, \*, \[ etc
        /// </summary>
        private string EncodeBackslashEscapes(string text)
        {
            foreach (var pair in BackslashEscapeTable)
                text = text.Replace(pair.Key, pair.Value);
            return text;
        }

        /// <summary>
        /// swap back in all the special characters we've hidden
        /// </summary>
        private string UnescapeSpecialChars(string text)
        {
            foreach (var pair in EscapeTable)
                text = text.Replace(pair.Value, pair.Key);
            return text;
        }

        private static Regex _outDent = new Regex(@"^(\t|[ ]{1," + _tabWidth + @"})", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Remove one level of line-leading tabs or spaces
        /// </summary>
        private string Outdent(string block)
        {
            return _outDent.Replace(block, "");
        }

        private static Regex _deTab = new Regex(@"^(.*?)(\t+)", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Convert all tabs to spaces
        /// </summary>
        private string Detab(string text)
        {
            // Inspired from a post by Bart Lateur: 
            // http://www.nntp.perl.org/group/perl.macperl.anyperl/154
            //
            // without a beginning of line anchor, the above has HIDEOUS performance
            // so I added a line anchor and we count the # of tabs beyond that.
            return _deTab.Replace(text, new MatchEvaluator(TabEvaluator));
        }

        private string TabEvaluator(Match match)
        {
            string leading = match.Groups[1].Value;
            int tabCount = match.Groups[2].Value.Length;
            return String.Concat(leading, new String(' ', (_tabWidth - leading.Length % _tabWidth) + ((tabCount - 1) * _tabWidth)));
        }

        /// <summary>
        /// this is to emulate what's evailable in PHP
        /// </summary>
        private static string RepeatString(string text, int count)
        {
            var sb = new StringBuilder(text.Length * count);

            for (int i = 0; i < count; i++)
                sb.Append(text);

            return sb.ToString();
        }

    }
}

var hljs = new function () {

    /* Utility functions */

    function escape(value) {
        return value.replace(/&/gm, '&amp;').replace(/</gm, '&lt;').replace(/>/gm, '&gt;');
    }

    function findCode(pre) {
        for (var node = pre.firstChild; node; node = node.nextSibling) {
            if (node.nodeName == 'CODE')
                return node;
            if (!(node.nodeType == 3 && node.nodeValue.match(/\s+/)))
                break;
        }
    }

    function blockText(block, ignoreNewLines) {
        return Array.prototype.map.call(block.childNodes, function (node) {
            if (node.nodeType == 3) {
                return ignoreNewLines ? node.nodeValue.replace(/\n/g, '') : node.nodeValue;
            }
            if (node.nodeName == 'BR') {
                return '\n';
            }
            return blockText(node, ignoreNewLines);
        }).join('');
    }

    function blockLanguage(block) {
        var classes = (block.className + ' ' + (block.parentNode ? block.parentNode.className : '')).split(/\s+/);
        classes = classes.map(function (c) { return c.replace(/^language-/, '') });
        for (var i = 0; i < classes.length; i++) {
            if (languages[classes[i]] || classes[i] == 'no-highlight') {
                return classes[i];
            }
        }
    }

    /* Stream merging */

    function nodeStream(node) {
        var result = [];
        (function _nodeStream(node, offset) {
            for (var child = node.firstChild; child; child = child.nextSibling) {
                if (child.nodeType == 3)
                    offset += child.nodeValue.length;
                else if (child.nodeName == 'BR')
                    offset += 1;
                else if (child.nodeType == 1) {
                    result.push({
                        event: 'start',
                        offset: offset,
                        node: child
                    });
                    offset = _nodeStream(child, offset);
                    result.push({
                        event: 'stop',
                        offset: offset,
                        node: child
                    });
                }
            }
            return offset;
        })(node, 0);
        return result;
    }

    function mergeStreams(stream1, stream2, value) {
        var processed = 0;
        var result = '';
        var nodeStack = [];

        function selectStream() {
            if (stream1.length && stream2.length) {
                if (stream1[0].offset != stream2[0].offset)
                    return (stream1[0].offset < stream2[0].offset) ? stream1 : stream2;
                else {
                    /*
                    To avoid starting the stream just before it should stop the order is
                    ensured that stream1 always starts first and closes last:
          
                    if (event1 == 'start' && event2 == 'start')
                      return stream1;
                    if (event1 == 'start' && event2 == 'stop')
                      return stream2;
                    if (event1 == 'stop' && event2 == 'start')
                      return stream1;
                    if (event1 == 'stop' && event2 == 'stop')
                      return stream2;
          
                    ... which is collapsed to:
                    */
                    return stream2[0].event == 'start' ? stream1 : stream2;
                }
            } else {
                return stream1.length ? stream1 : stream2;
            }
        }

        function open(node) {
            function attr_str(a) { return ' ' + a.nodeName + '="' + escape(a.value) + '"' };
            return '<' + node.nodeName + Array.prototype.map.call(node.attributes, attr_str).join('') + '>';
        }

        while (stream1.length || stream2.length) {
            var current = selectStream().splice(0, 1)[0];
            result += escape(value.substr(processed, current.offset - processed));
            processed = current.offset;
            if (current.event == 'start') {
                result += open(current.node);
                nodeStack.push(current.node);
            } else if (current.event == 'stop') {
                var node, i = nodeStack.length;
                do {
                    i--;
                    node = nodeStack[i];
                    result += ('</' + node.nodeName.toLowerCase() + '>');
                } while (node != current.node);
                nodeStack.splice(i, 1);
                while (i < nodeStack.length) {
                    result += open(nodeStack[i]);
                    i++;
                }
            }
        }
        return result + escape(value.substr(processed));
    }

    /* Initialization */

    function compileLanguage(language) {

        function reStr(re) {
            return (re && re.source) || re;
        }

        function langRe(value, global) {
            return RegExp(
              reStr(value),
              'm' + (language.case_insensitive ? 'i' : '') + (global ? 'g' : '')
            );
        }

        function compileMode(mode, parent) {
            if (mode.compiled)
                return;
            mode.compiled = true;

            var keywords = []; // used later with beginWithKeyword but filled as a side-effect of keywords compilation
            if (mode.keywords) {
                var compiled_keywords = {};

                function flatten(className, str) {
                    str.split(' ').forEach(function (kw) {
                        var pair = kw.split('|');
                        compiled_keywords[pair[0]] = [className, pair[1] ? Number(pair[1]) : 1];
                        keywords.push(pair[0]);
                    });
                }

                mode.lexemsRe = langRe(mode.lexems || hljs.IDENT_RE + '(?!\\.)', true);
                if (typeof mode.keywords == 'string') { // string
                    flatten('keyword', mode.keywords)
                } else {
                    for (var className in mode.keywords) {
                        if (!mode.keywords.hasOwnProperty(className))
                            continue;
                        flatten(className, mode.keywords[className]);
                    }
                }
                mode.keywords = compiled_keywords;
            }
            if (parent) {
                if (mode.beginWithKeyword) {
                    mode.begin = '\\b(' + keywords.join('|') + ')\\b(?!\\.)\\s*';
                }
                mode.beginRe = langRe(mode.begin ? mode.begin : '\\B|\\b');
                if (!mode.end && !mode.endsWithParent)
                    mode.end = '\\B|\\b';
                if (mode.end)
                    mode.endRe = langRe(mode.end);
                mode.terminator_end = reStr(mode.end) || '';
                if (mode.endsWithParent && parent.terminator_end)
                    mode.terminator_end += (mode.end ? '|' : '') + parent.terminator_end;
            }
            if (mode.illegal)
                mode.illegalRe = langRe(mode.illegal);
            if (mode.relevance === undefined)
                mode.relevance = 1;
            if (!mode.contains) {
                mode.contains = [];
            }
            for (var i = 0; i < mode.contains.length; i++) {
                if (mode.contains[i] == 'self') {
                    mode.contains[i] = mode;
                }
                compileMode(mode.contains[i], mode);
            }
            if (mode.starts) {
                compileMode(mode.starts, parent);
            }

            var terminators = [];
            for (var i = 0; i < mode.contains.length; i++) {
                terminators.push(reStr(mode.contains[i].begin));
            }
            if (mode.terminator_end) {
                terminators.push(reStr(mode.terminator_end));
            }
            if (mode.illegal) {
                terminators.push(reStr(mode.illegal));
            }
            mode.terminators = terminators.length ? langRe(terminators.join('|'), true) : { exec: function (s) { return null; } };
        }

        compileMode(language);
    }

    /*
    Core highlighting function. Accepts a language name and a string with the
    code to highlight. Returns an object with the following properties:
  
    - relevance (int)
    - keyword_count (int)
    - value (an HTML string with highlighting markup)
  
    */
    function highlight(language_name, value, ignore_illegals) {

        function subMode(lexem, mode) {
            for (var i = 0; i < mode.contains.length; i++) {
                var match = mode.contains[i].beginRe.exec(lexem);
                if (match && match.index == 0) {
                    return mode.contains[i];
                }
            }
        }

        function endOfMode(mode, lexem) {
            if (mode.end && mode.endRe.test(lexem)) {
                return mode;
            }
            if (mode.endsWithParent) {
                return endOfMode(mode.parent, lexem);
            }
        }

        function isIllegal(lexem, mode) {
            return !ignore_illegals && mode.illegal && mode.illegalRe.test(lexem);
        }

        function keywordMatch(mode, match) {
            var match_str = language.case_insensitive ? match[0].toLowerCase() : match[0];
            return mode.keywords.hasOwnProperty(match_str) && mode.keywords[match_str];
        }

        function processKeywords() {
            var buffer = escape(mode_buffer);
            if (!top.keywords)
                return buffer;
            var result = '';
            var last_index = 0;
            top.lexemsRe.lastIndex = 0;
            var match = top.lexemsRe.exec(buffer);
            while (match) {
                result += buffer.substr(last_index, match.index - last_index);
                var keyword_match = keywordMatch(top, match);
                if (keyword_match) {
                    keyword_count += keyword_match[1];
                    result += '<span class="' + keyword_match[0] + '">' + match[0] + '</span>';
                } else {
                    result += match[0];
                }
                last_index = top.lexemsRe.lastIndex;
                match = top.lexemsRe.exec(buffer);
            }
            return result + buffer.substr(last_index);
        }

        function processSubLanguage() {
            if (top.subLanguage && !languages[top.subLanguage]) {
                return escape(mode_buffer);
            }
            var result = top.subLanguage ? highlight(top.subLanguage, mode_buffer) : highlightAuto(mode_buffer);
            // Counting embedded language score towards the host language may be disabled
            // with zeroing the containing mode relevance. Usecase in point is Markdown that
            // allows XML everywhere and makes every XML snippet to have a much larger Markdown
            // score.
            if (top.relevance > 0) {
                keyword_count += result.keyword_count;
                relevance += result.relevance;
            }
            return '<span class="' + result.language + '">' + result.value + '</span>';
        }

        function processBuffer() {
            return top.subLanguage !== undefined ? processSubLanguage() : processKeywords();
        }

        function startNewMode(mode, lexem) {
            var markup = mode.className ? '<span class="' + mode.className + '">' : '';
            if (mode.returnBegin) {
                result += markup;
                mode_buffer = '';
            } else if (mode.excludeBegin) {
                result += escape(lexem) + markup;
                mode_buffer = '';
            } else {
                result += markup;
                mode_buffer = lexem;
            }
            top = Object.create(mode, { parent: { value: top } });
        }

        function processLexem(buffer, lexem) {
            mode_buffer += buffer;
            if (lexem === undefined) {
                result += processBuffer();
                return 0;
            }

            var new_mode = subMode(lexem, top);
            if (new_mode) {
                result += processBuffer();
                startNewMode(new_mode, lexem);
                return new_mode.returnBegin ? 0 : lexem.length;
            }

            var end_mode = endOfMode(top, lexem);
            if (end_mode) {
                var origin = top;
                if (!(origin.returnEnd || origin.excludeEnd)) {
                    mode_buffer += lexem;
                }
                result += processBuffer();
                do {
                    if (top.className) {
                        result += '</span>';
                    }
                    relevance += top.relevance;
                    top = top.parent;
                } while (top != end_mode.parent);
                if (origin.excludeEnd) {
                    result += escape(lexem);
                }
                mode_buffer = '';
                if (end_mode.starts) {
                    startNewMode(end_mode.starts, '');
                }
                return origin.returnEnd ? 0 : lexem.length;
            }

            if (isIllegal(lexem, top))
                throw new Error('Illegal lexem "' + lexem + '" for mode "' + (top.className || '<unnamed>') + '"');

            /*
            Parser should not reach this point as all types of lexems should be caught
            earlier, but if it does due to some bug make sure it advances at least one
            character forward to prevent infinite looping.
            */
            mode_buffer += lexem;
            return lexem.length || 1;
        }

        var language = languages[language_name];
        compileLanguage(language);
        var top = language;
        var mode_buffer = '';
        var relevance = 0;
        var keyword_count = 0;
        var result = '';
        try {
            var match, count, index = 0;
            while (true) {
                top.terminators.lastIndex = index;
                match = top.terminators.exec(value);
                if (!match)
                    break;
                count = processLexem(value.substr(index, match.index - index), match[0]);
                index = match.index + count;
            }
            processLexem(value.substr(index))
            return {
                relevance: relevance,
                keyword_count: keyword_count,
                value: result,
                language: language_name
            };
        } catch (e) {
            if (e.message.indexOf('Illegal') != -1) {
                return {
                    relevance: 0,
                    keyword_count: 0,
                    value: escape(value)
                };
            } else {
                throw e;
            }
        }
    }

    /*
    Highlighting with language detection. Accepts a string with the code to
    highlight. Returns an object with the following properties:
  
    - language (detected language)
    - relevance (int)
    - keyword_count (int)
    - value (an HTML string with highlighting markup)
    - second_best (object with the same structure for second-best heuristically
      detected language, may be absent)
  
    */
    function highlightAuto(text) {
        var result = {
            keyword_count: 0,
            relevance: 0,
            value: escape(text)
        };
        var second_best = result;
        for (var key in languages) {
            if (!languages.hasOwnProperty(key))
                continue;
            var current = highlight(key, text, false);
            current.language = key;
            if (current.keyword_count + current.relevance > second_best.keyword_count + second_best.relevance) {
                second_best = current;
            }
            if (current.keyword_count + current.relevance > result.keyword_count + result.relevance) {
                second_best = result;
                result = current;
            }
        }
        if (second_best.language) {
            result.second_best = second_best;
        }
        return result;
    }

    /*
    Post-processing of the highlighted markup:
  
    - replace TABs with something more useful
    - replace real line-breaks with '<br>' for non-pre containers
  
    */
    function fixMarkup(value, tabReplace, useBR) {
        if (tabReplace) {
            value = value.replace(/^((<[^>]+>|\t)+)/gm, function (match, p1, offset, s) {
                return p1.replace(/\t/g, tabReplace);
            });
        }
        if (useBR) {
            value = value.replace(/\n/g, '<br>');
        }
        return value;
    }

    /*
    Applies highlighting to a DOM node containing code. Accepts a DOM node and
    two optional parameters for fixMarkup.
    */
    function highlightBlock(block, tabReplace, useBR) {
        var text = blockText(block, useBR);
        var language = blockLanguage(block);
        if (language == 'no-highlight')
            return;
        var result = language ? highlight(language, text, true) : highlightAuto(text);
        language = result.language;
        var original = nodeStream(block);
        if (original.length) {
            var pre = document.createElement('pre');
            pre.innerHTML = result.value;
            result.value = mergeStreams(original, nodeStream(pre), text);
        }
        result.value = fixMarkup(result.value, tabReplace, useBR);

        var class_name = block.className;
        if (!class_name.match('(\\s|^)(language-)?' + language + '(\\s|$)')) {
            class_name = class_name ? (class_name + ' ' + language) : language;
        }
        block.innerHTML = result.value;
        block.className = class_name;
        block.result = {
            language: language,
            kw: result.keyword_count,
            re: result.relevance
        };
        if (result.second_best) {
            block.second_best = {
                language: result.second_best.language,
                kw: result.second_best.keyword_count,
                re: result.second_best.relevance
            };
        }
    }

    /*
    Applies highlighting to all <pre><code>..</code></pre> blocks on a page.
    */
    function initHighlighting() {
        if (initHighlighting.called)
            return;
        initHighlighting.called = true;
        Array.prototype.map.call(document.getElementsByTagName('pre'), findCode).
          filter(Boolean).
          forEach(function (code) { highlightBlock(code, hljs.tabReplace) });
    }

    /*
    Attaches highlighting to the page load event.
    */
    function initHighlightingOnLoad() {
        window.attachEvent('DOMContentLoaded', initHighlighting, false);
        window.attachEvent('load', initHighlighting, false);
    }

    var languages = {}; // a shortcut to avoid writing "this." everywhere

    /* Interface definition */

    this.LANGUAGES = languages;
    this.highlight = highlight;
    this.highlightAuto = highlightAuto;
    this.fixMarkup = fixMarkup;
    this.highlightBlock = highlightBlock;
    this.initHighlighting = initHighlighting;
    this.initHighlightingOnLoad = initHighlightingOnLoad;

    // Common regexps
    this.IDENT_RE = '[a-zA-Z][a-zA-Z0-9_]*';
    this.UNDERSCORE_IDENT_RE = '[a-zA-Z_][a-zA-Z0-9_]*';
    this.NUMBER_RE = '\\b\\d+(\\.\\d+)?';
    this.C_NUMBER_RE = '(\\b0[xX][a-fA-F0-9]+|(\\b\\d+(\\.\\d*)?|\\.\\d+)([eE][-+]?\\d+)?)'; // 0x..., 0..., decimal, float
    this.BINARY_NUMBER_RE = '\\b(0b[01]+)'; // 0b...
    this.RE_STARTERS_RE = '!|!=|!==|%|%=|&|&&|&=|\\*|\\*=|\\+|\\+=|,|\\.|-|-=|/|/=|:|;|<<|<<=|<=|<|===|==|=|>>>=|>>=|>=|>>>|>>|>|\\?|\\[|\\{|\\(|\\^|\\^=|\\||\\|=|\\|\\||~';

    // Common modes
    this.BACKSLASH_ESCAPE = {
        begin: '\\\\[\\s\\S]', relevance: 0
    };
    this.APOS_STRING_MODE = {
        className: 'string',
        begin: '\'', end: '\'',
        illegal: '\\n',
        contains: [this.BACKSLASH_ESCAPE],
        relevance: 0
    };
    this.QUOTE_STRING_MODE = {
        className: 'string',
        begin: '"', end: '"',
        illegal: '\\n',
        contains: [this.BACKSLASH_ESCAPE],
        relevance: 0
    };
    this.C_LINE_COMMENT_MODE = {
        className: 'comment',
        begin: '//', end: '$'
    };
    this.C_BLOCK_COMMENT_MODE = {
        className: 'comment',
        begin: '/\\*', end: '\\*/'
    };
    this.HASH_COMMENT_MODE = {
        className: 'comment',
        begin: '#', end: '$'
    };
    this.NUMBER_MODE = {
        className: 'number',
        begin: this.NUMBER_RE,
        relevance: 0
    };
    this.C_NUMBER_MODE = {
        className: 'number',
        begin: this.C_NUMBER_RE,
        relevance: 0
    };
    this.BINARY_NUMBER_MODE = {
        className: 'number',
        begin: this.BINARY_NUMBER_RE,
        relevance: 0
    };
    this.REGEXP_MODE = {
        className: 'regexp',
        begin: /\//, end: /\/[gim]*/,
        illegal: /\n/,
        contains: [
          this.BACKSLASH_ESCAPE,
          {
              begin: /\[/, end: /\]/,
              relevance: 0,
              contains: [this.BACKSLASH_ESCAPE]
          }
        ]
    }

    // Utility functions
    this.inherit = function (parent, obj) {
        var result = {}
        for (var key in parent)
            result[key] = parent[key];
        if (obj)
            for (var key in obj)
                result[key] = obj[key];
        return result;
    }
}();

hljs.LANGUAGES.bash = function (hljs) {
    var VAR1 = {
        className: 'variable', begin: /\$[\w\d#@][\w\d_]*/
    };
    var VAR2 = {
        className: 'variable', begin: /\$\{(.*?)\}/
    };
    var QUOTE_STRING = {
        className: 'string',
        begin: /"/, end: /"/,
        contains: [
          hljs.BACKSLASH_ESCAPE,
          VAR1,
          VAR2,
          {
              className: 'variable',
              begin: /\$\(/, end: /\)/,
              contains: hljs.BACKSLASH_ESCAPE
          }
        ],
        relevance: 0
    };
    var APOS_STRING = {
        className: 'string',
        begin: /'/, end: /'/,
        relevance: 0
    };

    return {
        lexems: /-?[a-z]+/,
        keywords: {
            keyword:
              'if then else elif fi for break continue while in do done exit return set ' +
              'declare case esac export exec',
            literal:
              'true false',
            built_in:
              'printf echo read cd pwd pushd popd dirs let eval unset typeset readonly ' +
              'getopts source shopt caller type hash bind help sudo',
            operator:
              '-ne -eq -lt -gt -f -d -e -s -l -a' // relevance booster
        },
        contains: [
          {
              className: 'shebang',
              begin: /^#![^\n]+sh\s*$/,
              relevance: 10
          },
          {
              className: 'function',
              begin: /\w[\w\d_]*\s*\(\s*\)\s*\{/,
              returnBegin: true,
              contains: [{ className: 'title', begin: /\w[\w\d_]*/ }],
              relevance: 0
          },
          hljs.HASH_COMMENT_MODE,
          hljs.NUMBER_MODE,
          QUOTE_STRING,
          APOS_STRING,
          VAR1,
          VAR2
        ]
    };
}(hljs);

hljs.LANGUAGES.cs = function (hljs) {
    return {
        keywords:
          // Normal keywords.
          'abstract as base bool break byte case catch char checked class const continue decimal ' +
          'default delegate do double else enum event explicit extern false finally fixed float ' +
          'for foreach goto if implicit in int interface internal is lock long namespace new null ' +
          'object operator out override params private protected public readonly ref return sbyte ' +
          'sealed short sizeof stackalloc static string struct switch this throw true try typeof ' +
          'uint ulong unchecked unsafe ushort using virtual volatile void while async await ' +
          // Contextual keywords.
          'ascending descending from get group into join let orderby partial select set value var ' +
          'where yield',
        contains: [
          {
              className: 'comment',
              begin: '///', end: '$', returnBegin: true,
              contains: [
                {
                    className: 'xmlDocTag',
                    begin: '///|<!--|-->'
                },
                {
                    className: 'xmlDocTag',
                    begin: '</?', end: '>'
                }
              ]
          },
          hljs.C_LINE_COMMENT_MODE,
          hljs.C_BLOCK_COMMENT_MODE,
          {
              className: 'preprocessor',
              begin: '#', end: '$',
              keywords: 'if else elif endif define undef warning error line region endregion pragma checksum'
          },
          {
              className: 'string',
              begin: '@"', end: '"',
              contains: [{ begin: '""' }]
          },
          hljs.APOS_STRING_MODE,
          hljs.QUOTE_STRING_MODE,
          hljs.C_NUMBER_MODE
        ]
    };
}(hljs);

hljs.LANGUAGES.ruby = function (hljs) {
    var RUBY_IDENT_RE = '[a-zA-Z_][a-zA-Z0-9_]*(\\!|\\?)?';
    var RUBY_METHOD_RE = '[a-zA-Z_]\\w*[!?=]?|[-+~]\\@|<<|>>|=~|===?|<=>|[<>]=?|\\*\\*|[-/+%^&*~`|]|\\[\\]=?';
    var RUBY_KEYWORDS = {
        keyword:
          'and false then defined module in return redo if BEGIN retry end for true self when ' +
          'next until do begin unless END rescue nil else break undef not super class case ' +
          'require yield alias while ensure elsif or include'
    };
    var YARDOCTAG = {
        className: 'yardoctag',
        begin: '@[A-Za-z]+'
    };
    var COMMENTS = [
      {
          className: 'comment',
          begin: '#', end: '$',
          contains: [YARDOCTAG]
      },
      {
          className: 'comment',
          begin: '^\\=begin', end: '^\\=end',
          contains: [YARDOCTAG],
          relevance: 10
      },
      {
          className: 'comment',
          begin: '^__END__', end: '\\n$'
      }
    ];
    var SUBST = {
        className: 'subst',
        begin: '#\\{', end: '}',
        lexems: RUBY_IDENT_RE,
        keywords: RUBY_KEYWORDS
    };
    var STR_CONTAINS = [hljs.BACKSLASH_ESCAPE, SUBST];
    var STRINGS = [
      {
          className: 'string',
          begin: '\'', end: '\'',
          contains: STR_CONTAINS,
          relevance: 0
      },
      {
          className: 'string',
          begin: '"', end: '"',
          contains: STR_CONTAINS,
          relevance: 0
      },
      {
          className: 'string',
          begin: '%[qw]?\\(', end: '\\)',
          contains: STR_CONTAINS
      },
      {
          className: 'string',
          begin: '%[qw]?\\[', end: '\\]',
          contains: STR_CONTAINS
      },
      {
          className: 'string',
          begin: '%[qw]?{', end: '}',
          contains: STR_CONTAINS
      },
      {
          className: 'string',
          begin: '%[qw]?<', end: '>',
          contains: STR_CONTAINS,
          relevance: 10
      },
      {
          className: 'string',
          begin: '%[qw]?/', end: '/',
          contains: STR_CONTAINS,
          relevance: 10
      },
      {
          className: 'string',
          begin: '%[qw]?%', end: '%',
          contains: STR_CONTAINS,
          relevance: 10
      },
      {
          className: 'string',
          begin: '%[qw]?-', end: '-',
          contains: STR_CONTAINS,
          relevance: 10
      },
      {
          className: 'string',
          begin: '%[qw]?\\|', end: '\\|',
          contains: STR_CONTAINS,
          relevance: 10
      }
    ];
    var FUNCTION = {
        className: 'function',
        beginWithKeyword: true, end: ' |$|;',
        keywords: 'def',
        contains: [
          {
              className: 'title',
              begin: RUBY_METHOD_RE,
              lexems: RUBY_IDENT_RE,
              keywords: RUBY_KEYWORDS
          },
          {
              className: 'params',
              begin: '\\(', end: '\\)',
              lexems: RUBY_IDENT_RE,
              keywords: RUBY_KEYWORDS
          }
        ].concat(COMMENTS)
    };

    var RUBY_DEFAULT_CONTAINS = COMMENTS.concat(STRINGS.concat([
      {
          className: 'class',
          beginWithKeyword: true, end: '$|;',
          keywords: 'class module',
          contains: [
            {
                className: 'title',
                begin: '[A-Za-z_]\\w*(::\\w+)*(\\?|\\!)?',
                relevance: 0
            },
            {
                className: 'inheritance',
                begin: '<\\s*',
                contains: [{
                    className: 'parent',
                    begin: '(' + hljs.IDENT_RE + '::)?' + hljs.IDENT_RE
                }]
            }
          ].concat(COMMENTS)
      },
      FUNCTION,
      {
          className: 'constant',
          begin: '(::)?(\\b[A-Z]\\w*(::)?)+',
          relevance: 0
      },
      {
          className: 'symbol',
          begin: ':',
          contains: STRINGS.concat([{ begin: RUBY_METHOD_RE }]),
          relevance: 0
      },
      {
          className: 'symbol',
          begin: RUBY_IDENT_RE + ':',
          relevance: 0
      },
      {
          className: 'number',
          begin: '(\\b0[0-7_]+)|(\\b0x[0-9a-fA-F_]+)|(\\b[1-9][0-9_]*(\\.[0-9_]+)?)|[0_]\\b',
          relevance: 0
      },
      {
          className: 'number',
          begin: '\\?\\w'
      },
      {
          className: 'variable',
          begin: '(\\$\\W)|((\\$|\\@\\@?)(\\w+))'
      },
      { // regexp container
          begin: '(' + hljs.RE_STARTERS_RE + ')\\s*',
          contains: COMMENTS.concat([
            {
                className: 'regexp',
                begin: '/', end: '/[a-z]*',
                illegal: '\\n',
                contains: [hljs.BACKSLASH_ESCAPE, SUBST]
            }
          ]),
          relevance: 0
      }
    ]));
    SUBST.contains = RUBY_DEFAULT_CONTAINS;
    FUNCTION.contains[1].contains = RUBY_DEFAULT_CONTAINS;

    return {
        lexems: RUBY_IDENT_RE,
        keywords: RUBY_KEYWORDS,
        contains: RUBY_DEFAULT_CONTAINS
    };
}(hljs);

hljs.LANGUAGES.diff = function (hljs) {
    return {
        contains: [
          {
              className: 'chunk',
              begin: '^\\@\\@ +\\-\\d+,\\d+ +\\+\\d+,\\d+ +\\@\\@$',
              relevance: 10
          },
          {
              className: 'chunk',
              begin: '^\\*\\*\\* +\\d+,\\d+ +\\*\\*\\*\\*$',
              relevance: 10
          },
          {
              className: 'chunk',
              begin: '^\\-\\-\\- +\\d+,\\d+ +\\-\\-\\-\\-$',
              relevance: 10
          },
          {
              className: 'header',
              begin: 'Index: ', end: '$'
          },
          {
              className: 'header',
              begin: '=====', end: '=====$'
          },
          {
              className: 'header',
              begin: '^\\-\\-\\-', end: '$'
          },
          {
              className: 'header',
              begin: '^\\*{3} ', end: '$'
          },
          {
              className: 'header',
              begin: '^\\+\\+\\+', end: '$'
          },
          {
              className: 'header',
              begin: '\\*{5}', end: '\\*{5}$'
          },
          {
              className: 'addition',
              begin: '^\\+', end: '$'
          },
          {
              className: 'deletion',
              begin: '^\\-', end: '$'
          },
          {
              className: 'change',
              begin: '^\\!', end: '$'
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.javascript = function (hljs) {
    return {
        keywords: {
            keyword:
              'in if for while finally var new function do return void else break catch ' +
              'instanceof with throw case default try this switch continue typeof delete ' +
              'let yield const',
            literal:
              'true false null undefined NaN Infinity'
        },
        contains: [
          hljs.APOS_STRING_MODE,
          hljs.QUOTE_STRING_MODE,
          hljs.C_LINE_COMMENT_MODE,
          hljs.C_BLOCK_COMMENT_MODE,
          hljs.C_NUMBER_MODE,
          { // "value" container
              begin: '(' + hljs.RE_STARTERS_RE + '|\\b(case|return|throw)\\b)\\s*',
              keywords: 'return throw case',
              contains: [
                hljs.C_LINE_COMMENT_MODE,
                hljs.C_BLOCK_COMMENT_MODE,
                hljs.REGEXP_MODE,
                { // E4X
                    begin: /</, end: />;/,
                    subLanguage: 'xml'
                }
              ],
              relevance: 0
          },
          {
              className: 'function',
              beginWithKeyword: true, end: /{/,
              keywords: 'function',
              contains: [
                {
                    className: 'title', begin: /[A-Za-z$_][0-9A-Za-z$_]*/
                },
                {
                    className: 'params',
                    begin: /\(/, end: /\)/,
                    contains: [
                      hljs.C_LINE_COMMENT_MODE,
                      hljs.C_BLOCK_COMMENT_MODE
                    ],
                    illegal: /["'\(]/
                }
              ],
              illegal: /\[|%/
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.css = function (hljs) {
    var IDENT_RE = '[a-zA-Z-][a-zA-Z0-9_-]*';
    var FUNCTION = {
        className: 'function',
        begin: IDENT_RE + '\\(', end: '\\)',
        contains: ['self', hljs.NUMBER_MODE, hljs.APOS_STRING_MODE, hljs.QUOTE_STRING_MODE]
    };
    return {
        case_insensitive: true,
        illegal: '[=/|\']',
        contains: [
          hljs.C_BLOCK_COMMENT_MODE,
          {
              className: 'id', begin: '\\#[A-Za-z0-9_-]+'
          },
          {
              className: 'class', begin: '\\.[A-Za-z0-9_-]+',
              relevance: 0
          },
          {
              className: 'attr_selector',
              begin: '\\[', end: '\\]',
              illegal: '$'
          },
          {
              className: 'pseudo',
              begin: ':(:)?[a-zA-Z0-9\\_\\-\\+\\(\\)\\"\\\']+'
          },
          {
              className: 'at_rule',
              begin: '@(font-face|page)',
              lexems: '[a-z-]+',
              keywords: 'font-face page'
          },
          {
              className: 'at_rule',
              begin: '@', end: '[{;]', // at_rule eating first "{" is a good thing
              // because it doesn’t let it to be parsed as
              // a rule set but instead drops parser into
              // the default mode which is how it should be.
              contains: [
                {
                    className: 'keyword',
                    begin: /\S+/
                },
                {
                    begin: /\s/, endsWithParent: true, excludeEnd: true,
                    relevance: 0,
                    contains: [
                      FUNCTION,
                      hljs.APOS_STRING_MODE, hljs.QUOTE_STRING_MODE,
                      hljs.NUMBER_MODE
                    ]
                }
              ]
          },
          {
              className: 'tag', begin: IDENT_RE,
              relevance: 0
          },
          {
              className: 'rules',
              begin: '{', end: '}',
              illegal: '[^\\s]',
              relevance: 0,
              contains: [
                hljs.C_BLOCK_COMMENT_MODE,
                {
                    className: 'rule',
                    begin: '[^\\s]', returnBegin: true, end: ';', endsWithParent: true,
                    contains: [
                      {
                          className: 'attribute',
                          begin: '[A-Z\\_\\.\\-]+', end: ':',
                          excludeEnd: true,
                          illegal: '[^\\s]',
                          starts: {
                              className: 'value',
                              endsWithParent: true, excludeEnd: true,
                              contains: [
                                FUNCTION,
                                hljs.NUMBER_MODE,
                                hljs.QUOTE_STRING_MODE,
                                hljs.APOS_STRING_MODE,
                                hljs.C_BLOCK_COMMENT_MODE,
                                {
                                    className: 'hexcolor', begin: '#[0-9A-Fa-f]+'
                                },
                                {
                                    className: 'important', begin: '!important'
                                }
                              ]
                          }
                      }
                    ]
                }
              ]
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.xml = function (hljs) {
    var XML_IDENT_RE = '[A-Za-z0-9\\._:-]+';
    var TAG_INTERNALS = {
        endsWithParent: true,
        relevance: 0,
        contains: [
          {
              className: 'attribute',
              begin: XML_IDENT_RE,
              relevance: 0
          },
          {
              begin: '="', returnBegin: true, end: '"',
              contains: [{
                  className: 'value',
                  begin: '"', endsWithParent: true
              }]
          },
          {
              begin: '=\'', returnBegin: true, end: '\'',
              contains: [{
                  className: 'value',
                  begin: '\'', endsWithParent: true
              }]
          },
          {
              begin: '=',
              contains: [{
                  className: 'value',
                  begin: '[^\\s/>]+'
              }]
          }
        ]
    };
    return {
        case_insensitive: true,
        contains: [
          {
              className: 'pi',
              begin: '<\\?', end: '\\?>',
              relevance: 10
          },
          {
              className: 'doctype',
              begin: '<!DOCTYPE', end: '>',
              relevance: 10,
              contains: [{ begin: '\\[', end: '\\]' }]
          },
          {
              className: 'comment',
              begin: '<!--', end: '-->',
              relevance: 10
          },
          {
              className: 'cdata',
              begin: '<\\!\\[CDATA\\[', end: '\\]\\]>',
              relevance: 10
          },
          {
              className: 'tag',
              /*
              The lookahead pattern (?=...) ensures that 'begin' only matches
              '<style' as a single word, followed by a whitespace or an
              ending braket. The '$' is needed for the lexem to be recognized
              by hljs.subMode() that tests lexems outside the stream.
              */
              begin: '<style(?=\\s|>|$)', end: '>',
              keywords: { title: 'style' },
              contains: [TAG_INTERNALS],
              starts: {
                  end: '</style>', returnEnd: true,
                  subLanguage: 'css'
              }
          },
          {
              className: 'tag',
              // See the comment in the <style tag about the lookahead pattern
              begin: '<script(?=\\s|>|$)', end: '>',
              keywords: { title: 'script' },
              contains: [TAG_INTERNALS],
              starts: {
                  end: "<\/script>", returnEnd: true,
                  subLanguage: 'javascript'
              }
          },
          {
              begin: '<%', end: '%>',
              subLanguage: 'vbscript'
          },
          {
              className: 'tag',
              begin: '</?', end: '/?>',
              relevance: 0,
              contains: [
                {
                    className: 'title', begin: '[^ /><]+'
                },
                TAG_INTERNALS
              ]
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.http = function (hljs) {
    return {
        illegal: '\\S',
        contains: [
          {
              className: 'status',
              begin: '^HTTP/[0-9\\.]+', end: '$',
              contains: [{ className: 'number', begin: '\\b\\d{3}\\b' }]
          },
          {
              className: 'request',
              begin: '^[A-Z]+ (.*?) HTTP/[0-9\\.]+$', returnBegin: true, end: '$',
              contains: [
                {
                    className: 'string',
                    begin: ' ', end: ' ',
                    excludeBegin: true, excludeEnd: true
                }
              ]
          },
          {
              className: 'attribute',
              begin: '^\\w', end: ': ', excludeEnd: true,
              illegal: '\\n|\\s|=',
              starts: { className: 'string', end: '$' }
          },
          {
              begin: '\\n\\n',
              starts: { subLanguage: '', endsWithParent: true }
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.java = function (hljs) {
    return {
        keywords:
          'false synchronized int abstract float private char boolean static null if const ' +
          'for true while long throw strictfp finally protected import native final return void ' +
          'enum else break transient new catch instanceof byte super volatile case assert short ' +
          'package default double public try this switch continue throws',
        contains: [
          {
              className: 'javadoc',
              begin: '/\\*\\*', end: '\\*/',
              contains: [{
                  className: 'javadoctag', begin: '(^|\\s)@[A-Za-z]+'
              }],
              relevance: 10
          },
          hljs.C_LINE_COMMENT_MODE,
          hljs.C_BLOCK_COMMENT_MODE,
          hljs.APOS_STRING_MODE,
          hljs.QUOTE_STRING_MODE,
          {
              className: 'class',
              beginWithKeyword: true, end: '{',
              keywords: 'class interface',
              excludeEnd: true,
              illegal: ':',
              contains: [
                {
                    beginWithKeyword: true,
                    keywords: 'extends implements',
                    relevance: 10
                },
                {
                    className: 'title',
                    begin: hljs.UNDERSCORE_IDENT_RE
                }
              ]
          },
          hljs.C_NUMBER_MODE,
          {
              className: 'annotation', begin: '@[A-Za-z]+'
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.php = function (hljs) {
    var VARIABLE = {
        className: 'variable', begin: '\\$+[a-zA-Z_\x7f-\xff][a-zA-Z0-9_\x7f-\xff]*'
    };
    var STRINGS = [
      hljs.inherit(hljs.APOS_STRING_MODE, { illegal: null }),
      hljs.inherit(hljs.QUOTE_STRING_MODE, { illegal: null }),
      {
          className: 'string',
          begin: 'b"', end: '"',
          contains: [hljs.BACKSLASH_ESCAPE]
      },
      {
          className: 'string',
          begin: 'b\'', end: '\'',
          contains: [hljs.BACKSLASH_ESCAPE]
      }
    ];
    var NUMBERS = [hljs.BINARY_NUMBER_MODE, hljs.C_NUMBER_MODE];
    var TITLE = {
        className: 'title', begin: hljs.UNDERSCORE_IDENT_RE
    };
    return {
        case_insensitive: true,
        keywords:
          'and include_once list abstract global private echo interface as static endswitch ' +
          'array null if endwhile or const for endforeach self var while isset public ' +
          'protected exit foreach throw elseif include __FILE__ empty require_once do xor ' +
          'return implements parent clone use __CLASS__ __LINE__ else break print eval new ' +
          'catch __METHOD__ case exception php_user_filter default die require __FUNCTION__ ' +
          'enddeclare final try this switch continue endfor endif declare unset true false ' +
          'namespace trait goto instanceof insteadof __DIR__ __NAMESPACE__ __halt_compiler',
        contains: [
          hljs.C_LINE_COMMENT_MODE,
          hljs.HASH_COMMENT_MODE,
          {
              className: 'comment',
              begin: '/\\*', end: '\\*/',
              contains: [{
                  className: 'phpdoc',
                  begin: '\\s@[A-Za-z]+'
              }]
          },
          {
              className: 'comment',
              excludeBegin: true,
              begin: '__halt_compiler.+?;', endsWithParent: true
          },
          {
              className: 'string',
              begin: '<<<[\'"]?\\w+[\'"]?$', end: '^\\w+;',
              contains: [hljs.BACKSLASH_ESCAPE]
          },
          {
              className: 'preprocessor',
              begin: '<\\?php',
              relevance: 10
          },
          {
              className: 'preprocessor',
              begin: '\\?>'
          },
          VARIABLE,
          {
              className: 'function',
              beginWithKeyword: true, end: '{',
              keywords: 'function',
              illegal: '\\$|\\[|%',
              contains: [
                TITLE,
                {
                    className: 'params',
                    begin: '\\(', end: '\\)',
                    contains: [
                      'self',
                      VARIABLE,
                      hljs.C_BLOCK_COMMENT_MODE
                    ].concat(STRINGS).concat(NUMBERS)
                }
              ]
          },
          {
              className: 'class',
              beginWithKeyword: true, end: '{',
              keywords: 'class',
              illegal: '[:\\(\\$]',
              contains: [
                {
                    beginWithKeyword: true, endsWithParent: true,
                    keywords: 'extends',
                    contains: [TITLE]
                },
                TITLE
              ]
          },
          {
              begin: '=>' // No markup, just a relevance booster
          }
        ].concat(STRINGS).concat(NUMBERS)
    };
}(hljs);

hljs.LANGUAGES.python = function (hljs) {
    var PROMPT = {
        className: 'prompt', begin: /^(>>>|\.\.\.) /
    }
    var STRINGS = [
      {
          className: 'string',
          begin: /(u|b)?r?'''/, end: /'''/,
          contains: [PROMPT],
          relevance: 10
      },
      {
          className: 'string',
          begin: /(u|b)?r?"""/, end: /"""/,
          contains: [PROMPT],
          relevance: 10
      },
      {
          className: 'string',
          begin: /(u|r|ur)'/, end: /'/,
          contains: [hljs.BACKSLASH_ESCAPE],
          relevance: 10
      },
      {
          className: 'string',
          begin: /(u|r|ur)"/, end: /"/,
          contains: [hljs.BACKSLASH_ESCAPE],
          relevance: 10
      },
      {
          className: 'string',
          begin: /(b|br)'/, end: /'/,
          contains: [hljs.BACKSLASH_ESCAPE]
      },
      {
          className: 'string',
          begin: /(b|br)"/, end: /"/,
          contains: [hljs.BACKSLASH_ESCAPE]
      }
    ].concat([
      hljs.APOS_STRING_MODE,
      hljs.QUOTE_STRING_MODE
    ]);
    var TITLE = {
        className: 'title', begin: hljs.UNDERSCORE_IDENT_RE
    };
    var PARAMS = {
        className: 'params',
        begin: /\(/, end: /\)/,
        contains: ['self', hljs.C_NUMBER_MODE, PROMPT].concat(STRINGS)
    };
    var FUNC_CLASS_PROTO = {
        beginWithKeyword: true, end: /:/,
        illegal: /[${=;\n]/,
        contains: [TITLE, PARAMS],
        relevance: 10
    };

    return {
        keywords: {
            keyword:
              'and elif is global as in if from raise for except finally print import pass return ' +
              'exec else break not with class assert yield try while continue del or def lambda ' +
              'nonlocal|10',
            built_in:
              'None True False Ellipsis NotImplemented'
        },
        illegal: /(<\/|->|\?)/,
        contains: STRINGS.concat([
          PROMPT,
          hljs.HASH_COMMENT_MODE,
          hljs.inherit(FUNC_CLASS_PROTO, { className: 'function', keywords: 'def' }),
          hljs.inherit(FUNC_CLASS_PROTO, { className: 'class', keywords: 'class' }),
          hljs.C_NUMBER_MODE,
          {
              className: 'decorator',
              begin: /@/, end: /$/
          },
          {
              begin: /\b(print|exec)\(/ // don’t highlight keywords-turned-functions in Python 3
          }
        ])
    };
}(hljs);

hljs.LANGUAGES.sql = function (hljs) {
    return {
        case_insensitive: true,
        contains: [
          {
              className: 'operator',
              begin: '(begin|end|start|commit|rollback|savepoint|lock|alter|create|drop|rename|call|delete|do|handler|insert|load|replace|select|truncate|update|set|show|pragma|grant)\\b(?!:)', // negative look-ahead here is specifically to prevent stomping on SmallTalk
              end: ';', endsWithParent: true,
              keywords: {
                  keyword: 'all partial global month current_timestamp using go revoke smallint ' +
                    'indicator end-exec disconnect zone with character assertion to add current_user ' +
                    'usage input local alter match collate real then rollback get read timestamp ' +
                    'session_user not integer bit unique day minute desc insert execute like ilike|2 ' +
                    'level decimal drop continue isolation found where constraints domain right ' +
                    'national some module transaction relative second connect escape close system_user ' +
                    'for deferred section cast current sqlstate allocate intersect deallocate numeric ' +
                    'public preserve full goto initially asc no key output collation group by union ' +
                    'session both last language constraint column of space foreign deferrable prior ' +
                    'connection unknown action commit view or first into float year primary cascaded ' +
                    'except restrict set references names table outer open select size are rows from ' +
                    'prepare distinct leading create only next inner authorization schema ' +
                    'corresponding option declare precision immediate else timezone_minute external ' +
                    'varying translation true case exception join hour default double scroll value ' +
                    'cursor descriptor values dec fetch procedure delete and false int is describe ' +
                    'char as at in varchar null trailing any absolute current_time end grant ' +
                    'privileges when cross check write current_date pad begin temporary exec time ' +
                    'update catalog user sql date on identity timezone_hour natural whenever interval ' +
                    'work order cascade diagnostics nchar having left call do handler load replace ' +
                    'truncate start lock show pragma exists number trigger if before after each row',
                  aggregate: 'count sum min max avg'
              },
              contains: [
                {
                    className: 'string',
                    begin: '\'', end: '\'',
                    contains: [hljs.BACKSLASH_ESCAPE, { begin: '\'\'' }],
                    relevance: 0
                },
                {
                    className: 'string',
                    begin: '"', end: '"',
                    contains: [hljs.BACKSLASH_ESCAPE, { begin: '""' }],
                    relevance: 0
                },
                {
                    className: 'string',
                    begin: '`', end: '`',
                    contains: [hljs.BACKSLASH_ESCAPE]
                },
                hljs.C_NUMBER_MODE
              ]
          },
          hljs.C_BLOCK_COMMENT_MODE,
          {
              className: 'comment',
              begin: '--', end: '$'
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.ini = function (hljs) {
    return {
        case_insensitive: true,
        illegal: '[^\\s]',
        contains: [
          {
              className: 'comment',
              begin: ';', end: '$'
          },
          {
              className: 'title',
              begin: '^\\[', end: '\\]'
          },
          {
              className: 'setting',
              begin: '^[a-z0-9\\[\\]_-]+[ \\t]*=[ \\t]*', end: '$',
              contains: [
                {
                    className: 'value',
                    endsWithParent: true,
                    keywords: 'on off true false yes no',
                    contains: [hljs.QUOTE_STRING_MODE, hljs.NUMBER_MODE],
                    relevance: 0
                }
              ]
          }
        ]
    };
}(hljs);

hljs.LANGUAGES.perl = function (hljs) {
    var PERL_KEYWORDS = 'getpwent getservent quotemeta msgrcv scalar kill dbmclose undef lc ' +
      'ma syswrite tr send umask sysopen shmwrite vec qx utime local oct semctl localtime ' +
      'readpipe do return format read sprintf dbmopen pop getpgrp not getpwnam rewinddir qq' +
      'fileno qw endprotoent wait sethostent bless s|0 opendir continue each sleep endgrent ' +
      'shutdown dump chomp connect getsockname die socketpair close flock exists index shmget' +
      'sub for endpwent redo lstat msgctl setpgrp abs exit select print ref gethostbyaddr ' +
      'unshift fcntl syscall goto getnetbyaddr join gmtime symlink semget splice x|0 ' +
      'getpeername recv log setsockopt cos last reverse gethostbyname getgrnam study formline ' +
      'endhostent times chop length gethostent getnetent pack getprotoent getservbyname rand ' +
      'mkdir pos chmod y|0 substr endnetent printf next open msgsnd readdir use unlink ' +
      'getsockopt getpriority rindex wantarray hex system getservbyport endservent int chr ' +
      'untie rmdir prototype tell listen fork shmread ucfirst setprotoent else sysseek link ' +
      'getgrgid shmctl waitpid unpack getnetbyname reset chdir grep split require caller ' +
      'lcfirst until warn while values shift telldir getpwuid my getprotobynumber delete and ' +
      'sort uc defined srand accept package seekdir getprotobyname semop our rename seek if q|0 ' +
      'chroot sysread setpwent no crypt getc chown sqrt write setnetent setpriority foreach ' +
      'tie sin msgget map stat getlogin unless elsif truncate exec keys glob tied closedir' +
      'ioctl socket readlink eval xor readline binmode setservent eof ord bind alarm pipe ' +
      'atan2 getgrent exp time push setgrent gt lt or ne m|0 break given say state when';
    var SUBST = {
        className: 'subst',
        begin: '[$@]\\{', end: '\\}',
        keywords: PERL_KEYWORDS,
        relevance: 10
    };
    var VAR1 = {
        className: 'variable',
        begin: '\\$\\d'
    };
    var VAR2 = {
        className: 'variable',
        begin: '[\\$\\%\\@\\*](\\^\\w\\b|#\\w+(\\:\\:\\w+)*|[^\\s\\w{]|{\\w+}|\\w+(\\:\\:\\w*)*)'
    };
    var STRING_CONTAINS = [hljs.BACKSLASH_ESCAPE, SUBST, VAR1, VAR2];
    var METHOD = {
        begin: '->',
        contains: [
          { begin: hljs.IDENT_RE },
          { begin: '{', end: '}' }
        ]
    };
    var COMMENT = {
        className: 'comment',
        begin: '^(__END__|__DATA__)', end: '\\n$',
        relevance: 5
    }
    var PERL_DEFAULT_CONTAINS = [
      VAR1, VAR2,
      hljs.HASH_COMMENT_MODE,
      COMMENT,
      {
          className: 'comment',
          begin: '^\\=\\w', end: '\\=cut', endsWithParent: true
      },
      METHOD,
      {
          className: 'string',
          begin: 'q[qwxr]?\\s*\\(', end: '\\)',
          contains: STRING_CONTAINS,
          relevance: 5
      },
      {
          className: 'string',
          begin: 'q[qwxr]?\\s*\\[', end: '\\]',
          contains: STRING_CONTAINS,
          relevance: 5
      },
      {
          className: 'string',
          begin: 'q[qwxr]?\\s*\\{', end: '\\}',
          contains: STRING_CONTAINS,
          relevance: 5
      },
      {
          className: 'string',
          begin: 'q[qwxr]?\\s*\\|', end: '\\|',
          contains: STRING_CONTAINS,
          relevance: 5
      },
      {
          className: 'string',
          begin: 'q[qwxr]?\\s*\\<', end: '\\>',
          contains: STRING_CONTAINS,
          relevance: 5
      },
      {
          className: 'string',
          begin: 'qw\\s+q', end: 'q',
          contains: STRING_CONTAINS,
          relevance: 5
      },
      {
          className: 'string',
          begin: '\'', end: '\'',
          contains: [hljs.BACKSLASH_ESCAPE],
          relevance: 0
      },
      {
          className: 'string',
          begin: '"', end: '"',
          contains: STRING_CONTAINS,
          relevance: 0
      },
      {
          className: 'string',
          begin: '`', end: '`',
          contains: [hljs.BACKSLASH_ESCAPE]
      },
      {
          className: 'string',
          begin: '{\\w+}',
          relevance: 0
      },
      {
          className: 'string',
          begin: '\-?\\w+\\s*\\=\\>',
          relevance: 0
      },
      {
          className: 'number',
          begin: '(\\b0[0-7_]+)|(\\b0x[0-9a-fA-F_]+)|(\\b[1-9][0-9_]*(\\.[0-9_]+)?)|[0_]\\b',
          relevance: 0
      },
      { // regexp container
          begin: '(' + hljs.RE_STARTERS_RE + '|\\b(split|return|print|reverse|grep)\\b)\\s*',
          keywords: 'split return print reverse grep',
          relevance: 0,
          contains: [
            hljs.HASH_COMMENT_MODE,
            COMMENT,
            {
                className: 'regexp',
                begin: '(s|tr|y)/(\\\\.|[^/])*/(\\\\.|[^/])*/[a-z]*',
                relevance: 10
            },
            {
                className: 'regexp',
                begin: '(m|qr)?/', end: '/[a-z]*',
                contains: [hljs.BACKSLASH_ESCAPE],
                relevance: 0 // allows empty "//" which is a common comment delimiter in other languages
            }
          ]
      },
      {
          className: 'sub',
          beginWithKeyword: true, end: '(\\s*\\(.*?\\))?[;{]',
          keywords: 'sub',
          relevance: 5
      },
      {
          className: 'operator',
          begin: '-\\w\\b',
          relevance: 0
      }
    ];
    SUBST.contains = PERL_DEFAULT_CONTAINS;
    METHOD.contains[1].contains = PERL_DEFAULT_CONTAINS;

    return {
        keywords: PERL_KEYWORDS,
        contains: PERL_DEFAULT_CONTAINS
    };
}(hljs);

hljs.LANGUAGES.json = function (hljs) {
    var LITERALS = { literal: 'true false null' };
    var TYPES = [
      hljs.QUOTE_STRING_MODE,
      hljs.C_NUMBER_MODE
    ];
    var VALUE_CONTAINER = {
        className: 'value',
        end: ',', endsWithParent: true, excludeEnd: true,
        contains: TYPES,
        keywords: LITERALS
    };
    var OBJECT = {
        begin: '{', end: '}',
        contains: [
          {
              className: 'attribute',
              begin: '\\s*"', end: '"\\s*:\\s*', excludeBegin: true, excludeEnd: true,
              contains: [hljs.BACKSLASH_ESCAPE],
              illegal: '\\n',
              starts: VALUE_CONTAINER
          }
        ],
        illegal: '\\S'
    };
    var ARRAY = {
        begin: '\\[', end: '\\]',
        contains: [hljs.inherit(VALUE_CONTAINER, { className: null })], // inherit is also a workaround for a bug that makes shared modes with endsWithParent compile only the ending of one of the parents
        illegal: '\\S'
    };
    TYPES.splice(TYPES.length, 0, OBJECT, ARRAY);
    return {
        contains: TYPES,
        keywords: LITERALS,
        illegal: '\\S'
    };
}(hljs);

hljs.LANGUAGES.cpp = function (hljs) {
    var CPP_KEYWORDS = {
        keyword: 'false int float while private char catch export virtual operator sizeof ' +
          'dynamic_cast|10 typedef const_cast|10 const struct for static_cast|10 union namespace ' +
          'unsigned long throw volatile static protected bool template mutable if public friend ' +
          'do return goto auto void enum else break new extern using true class asm case typeid ' +
          'short reinterpret_cast|10 default double register explicit signed typename try this ' +
          'switch continue wchar_t inline delete alignof char16_t char32_t constexpr decltype ' +
          'noexcept nullptr static_assert thread_local restrict _Bool complex',
        built_in: 'std string cin cout cerr clog stringstream istringstream ostringstream ' +
          'auto_ptr deque list queue stack vector map set bitset multiset multimap unordered_set ' +
          'unordered_map unordered_multiset unordered_multimap array shared_ptr'
    };
    return {
        keywords: CPP_KEYWORDS,
        illegal: '</',
        contains: [
          hljs.C_LINE_COMMENT_MODE,
          hljs.C_BLOCK_COMMENT_MODE,
          hljs.QUOTE_STRING_MODE,
          {
              className: 'string',
              begin: '\'\\\\?.', end: '\'',
              illegal: '.'
          },
          {
              className: 'number',
              begin: '\\b(\\d+(\\.\\d*)?|\\.\\d+)(u|U|l|L|ul|UL|f|F)'
          },
          hljs.C_NUMBER_MODE,
          {
              className: 'preprocessor',
              begin: '#', end: '$',
              contains: [
                { begin: '<', end: '>', illegal: '\\n' },
                hljs.C_LINE_COMMENT_MODE
              ]
          },
          {
              className: 'stl_container',
              begin: '\\b(deque|list|queue|stack|vector|map|set|bitset|multiset|multimap|unordered_map|unordered_set|unordered_multiset|unordered_multimap|array)\\s*<', end: '>',
              keywords: CPP_KEYWORDS,
              relevance: 10,
              contains: ['self']
          }
        ]
    };
}(hljs);

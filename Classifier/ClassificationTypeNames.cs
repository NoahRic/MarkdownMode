namespace MarkdownMode
{
    internal static class ClassificationTypeNames
    {
        public const string Markdown = "markdown";

        public const string Italics = Prefix + "italics";
        public const string Bold = Prefix + "bold";
        public const string BoldItalics = Prefix + "bolditalics";

        public const string Header = Prefix + "header";
        public const string HeaderH1 = HeaderLevelPrefix + "1";
        public const string HeaderH2 = HeaderLevelPrefix + "2";
        public const string HeaderH3 = HeaderLevelPrefix + "3";
        public const string HeaderH4 = HeaderLevelPrefix + "4";
        public const string HeaderH5 = HeaderLevelPrefix + "5";
        public const string HeaderH6 = HeaderLevelPrefix + "6";

        public const string List = Prefix + "list";
        public const string ListUnordered = ListPrefix + "unordered";
        public const string ListOrdered = ListPrefix + "ordered";

        public const string Preformatted = Prefix + "pre";
        public const string Block = Prefix + "block";
        public const string BlockQuote = Prefix + "blockquote";

        public const string Label = Prefix + "label";
        public const string Link = Prefix + "link";
        public const string LinkText = LinkPrefix + "text";
        public const string LinkTitle = LinkPrefix + "title";
        public const string LinkLabel = LinkPrefix + "label";
        public const string LinkPunctuation = LinkPrefix + "punctuation";

        public const string Image = Prefix + "image";
        public const string ImageAlt = ImagePrefix + "alt";
        public const string ImageLabel = ImagePrefix + "label";
        public const string ImageTitle = ImagePrefix + "title";

        public const string Url = Prefix + "url";
        public const string UrlInline = UrlPrefix + "inline";
        public const string UrlDefinition = UrlPrefix + "definition";
        public const string UrlAutomatic = UrlPrefix + "automatic";
        public const string UrlImage = UrlPrefix + "image";

        public const string HorizontalRule = Prefix + "horizontalrule";

        private const string Prefix = Markdown + ".";
        private const string HeaderLevelPrefix = Header + ".h";
        private const string ListPrefix = List + ".";
        private const string LinkPrefix = Link + ".";
        private const string ImagePrefix = Image + ".";
        private const string UrlPrefix = Url + ".";
    }
}

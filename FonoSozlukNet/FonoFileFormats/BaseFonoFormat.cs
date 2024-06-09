using RtfPipe;
using System.Text.RegularExpressions;

namespace FonoFileFormats;

public abstract class BaseFonoFormat : IEnumerable<Entry>
{
    private static readonly (Regex,string)[] HtmlReplacementPatterns = CreateHtmlReplacementPatterns();
    protected string _Abbreviations = string.Empty;
    protected string _Metadata = string.Empty;
    protected List<Entry>? entries;

    public IEnumerator<Entry> GetEnumerator()
    {
        if (entries != null)
            return entries.GetEnumerator();
        else
            return Enumerable.Empty<Entry>().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public string Abbreviations => _Abbreviations;
    public int Length => entries == null ? 0 : entries.Count;
    public string Metadata => _Metadata;

    private static (Regex,string)[] CreateHtmlReplacementPatterns()
    {
        var patterns = new(Regex, string)[]
        {
            (new Regex("<p[^>]+>", RegexOptions.Compiled), "<p>"),
            (new Regex("<strong[^>]+>", RegexOptions.Compiled), "<strong>"),
            (new Regex("<span[^>]+>", RegexOptions.Compiled), "<span>"),
            (new Regex("<em[^>]+>", RegexOptions.Compiled), "<em>"),
            (new Regex("<div[^>]+>", RegexOptions.Compiled), "<div>"),
            (new Regex("<sup[^>]+>", RegexOptions.Compiled), "<sup>")
        };
        return patterns;
    }

    protected string HandleHeadword(string headword)
    {
        bool endsWithDigit = char.IsDigit(headword.Last());
        return endsWithDigit ? headword.Remove(headword.Length-1) : headword;
    }

    protected string Rtf2Html(string rtf)
    {
        var html = Rtf.ToHtml(rtf);
        foreach (var pattern in HtmlReplacementPatterns)
        {
            html = pattern.Item1.Replace(html, pattern.Item2);
        }
        return html;
    }

    protected async Task<string> Rtf2HtmlAsync(string rtf)
    {
        await Task.Delay(1);
        var html = Rtf.ToHtml(rtf);
        foreach (var pattern in HtmlReplacementPatterns)
        {
            html = pattern.Item1.Replace(html, pattern.Item2);
        }
        return html;
    }

    protected abstract void ReadEntries();
}
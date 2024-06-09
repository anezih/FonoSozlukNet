using System.Text;
using System.Xml;

namespace FonoFileFormats;

public class FonoXmlReaderAsync : BaseFonoFormat
{
    private XmlDocument fonoXml = new XmlDocument();

    private FonoXmlReaderAsync(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var streamReader = new StreamReader(stream, Encoding.GetEncoding(1254));
        fonoXml.Load(streamReader);
    }

    public async static Task<FonoXmlReaderAsync> CreateAsync(Stream stream, Progress progress)
    {
        FonoXmlReaderAsync fonoXmlReaderAsync = new(stream);
        await fonoXmlReaderAsync.ReadEntriesAsync(progress);
        return fonoXmlReaderAsync;
    }

    public static async Task<int> GetWordCountAsync(Stream stream)
    {
        XmlDocument _fonoXml = new XmlDocument();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var streamReader = new StreamReader(stream, Encoding.GetEncoding(1254));
        await Task.Delay(2);
        _fonoXml.Load(streamReader);
        var kelimeler = _fonoXml.GetElementsByTagName("KELIME");
        stream.Seek(0, SeekOrigin.Begin);
        return kelimeler.Count;
    }

    protected async Task ReadEntriesAsync(Progress progress)
    {
        var kelimeler = fonoXml.GetElementsByTagName("KELIME");
        if (kelimeler.Count == 0)
            return;
        progress.Total = kelimeler.Count;
        entries = new List<Entry>(kelimeler.Count);
        int entriesCnt = 0;
        foreach (XmlNode kelime in kelimeler)
        {
            var sozcuk = kelime.SelectSingleNode(".//SOZCUK");
            var aciklama = kelime.SelectSingleNode(".//ACIKLAMA");
            if (!(sozcuk != null && aciklama != null))
                continue;
            var headword = sozcuk.InnerText.Trim();
            var definition = aciklama.InnerText.Trim();
            if (!string.IsNullOrEmpty(headword))
            {
                var entry = new Entry
                {
                    Headword = HandleHeadword(headword),
                    Definition = await Rtf2HtmlAsync(definition)
                };
                entries.Add(entry);
                entriesCnt++;
                progress.Step = entriesCnt;
            }
        }
    }

    protected override void ReadEntries(){}
}
using System.Text;
using System.Xml;

namespace FonoFileFormats;

public class FonoXmlReader : BaseFonoFormat
{
    private XmlDocument fonoXml = new XmlDocument();

    public FonoXmlReader(string path)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var streamReader = new StreamReader(path, Encoding.GetEncoding(1254));
        fonoXml.Load(streamReader);
        ReadEntries();
    }

    public FonoXmlReader(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var streamReader = new StreamReader(stream, Encoding.GetEncoding(1254));
        fonoXml.Load(streamReader);
        ReadEntries();
    }

    protected override void ReadEntries()
    {
        var kelimeler = fonoXml.GetElementsByTagName("KELIME");
        if (kelimeler.Count == 0)
            return;
        entries = new List<Entry>(kelimeler.Count);
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
                    Definition = Rtf2Html(definition)
                };
                entries.Add(entry);
            }
        }
    }
}
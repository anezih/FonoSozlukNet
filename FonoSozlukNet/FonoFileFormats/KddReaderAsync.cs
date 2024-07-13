using System.IO.Compression;
using System.Text;

namespace FonoFileFormats;

public class KddReaderAsync : BaseFonoFormat
{
    private readonly BinaryReader kddStream;

    private static readonly byte[] Header = [0x30, 0x31, 0x2E, 0x30, 0x31];
    private const int entryLengthsOffset = 0x1D;
    private const int headwordsOffset = 0x4D5;
    private static readonly byte[] Utf16CarriageReturn = [0x0D, 0x00];
    private static readonly byte[] Utf16Newline = [0x0A, 0x00];
    private static readonly byte[] ZlibLastThreeBytes = [0x00, 0x78, 0xDA];

    private KddReaderAsync(Stream stream)
    {
        kddStream = new BinaryReader(stream);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public async static Task<KddReaderAsync> CreateAsync(Stream stream, Progress progress)
    {
        KddReaderAsync kddReaderAsync = new(stream);
        await kddReaderAsync.ReadEntriesAsync(progress);
        return kddReaderAsync;
    }

    private static bool CheckHeader(BinaryReader kddStream)
    {
        if (kddStream.BaseStream.Length < 5)
            return false;
        var first5Bytes = kddStream.ReadBytes(5);
        if (first5Bytes.SequenceEqual(Header))
        {
            kddStream.BaseStream.Seek(-5, SeekOrigin.Current);
            return true;
        }
        else
        {
            Console.WriteLine("Unknown header.");
            kddStream.BaseStream.Seek(-5, SeekOrigin.Current);
            return false;
        }
    }

    private async Task FindZlib(BinaryReader binaryReader)
    {
        int pass = 0;
        while (true)
        {
            if (pass % 500 == 0)
                await Task.Delay(2);
            var oneByte = binaryReader.ReadByte();
            pass++;
            if (oneByte == 0x00)
            {
                var threeBytes = binaryReader.ReadBytes(3);
                if (threeBytes.SequenceEqual(ZlibLastThreeBytes))
                {
                    binaryReader.BaseStream.Seek(-6, SeekOrigin.Current);
                    break;
                }
                else
                {
                    binaryReader.BaseStream.Seek(-3, SeekOrigin.Current);
                }
            }
        }
    }

    private async Task<string> ReadZlib(BinaryReader binaryReader, int length, int skipFromBeginning = 0)
    {
        var bytes = binaryReader.ReadBytes(length);
        var msIn = new MemoryStream(bytes);
        var msOut = new MemoryStream();
        var zlibStream = new ZLibStream(msIn, CompressionMode.Decompress);
        await zlibStream.CopyToAsync(msOut);
        var bytesOut = msOut.ToArray().Skip(skipFromBeginning).ToArray();
        var result = Encoding.UTF8.GetString(bytesOut);
        return result;
    }

    protected async Task ReadEntriesAsync(Progress progress)
    {
        if (!CheckHeader(kddStream) | !kddStream.BaseStream.CanSeek)
            return;
        var totalWords = 0;
        kddStream.BaseStream.Seek(entryLengthsOffset, SeekOrigin.Begin);
        while (true)
        {
            await Task.Delay(1);
            var firstLetter = Encoding.Unicode.GetString(kddStream.ReadBytes(2));
            if (firstLetter == "#")
                break;
            kddStream.BaseStream.Seek(14, SeekOrigin.Current);
            var length = kddStream.ReadUInt16();
            totalWords += length;
            kddStream.BaseStream.Seek(6, SeekOrigin.Current);
        }
        progress.Total = totalWords;
        entries = new List<Entry>(totalWords);
        kddStream.BaseStream.Seek(headwordsOffset, SeekOrigin.Begin);
        List<byte> headword = new();
        int headwordCnt = 0;
        while (true)
        {
            if (headwordCnt % 250 == 0)
                await Task.Delay(2);
            if (headwordCnt == totalWords)
                break;
            var twoBytes = kddStream.ReadBytes(2);
            if (twoBytes.SequenceEqual(Utf16CarriageReturn))
            {
                var newlineTest = kddStream.ReadBytes(2);
                if (newlineTest.SequenceEqual(Utf16Newline))
                {
                    var skip = headword[0] == 0x00 ? 8 : 0;
                    var hw = HandleHeadword(Encoding.Unicode.GetString(headword.Skip(skip).ToArray()));
                    entries.Add(new Entry());
                    entries[headwordCnt].Headword = hw;
                    headwordCnt++;
                    headword.Clear();
                }
                else
                {
                    kddStream.BaseStream.Seek(-2, SeekOrigin.Current);
                    headword.AddRange(twoBytes);
                }
            }
            else
            {
                headword.AddRange(twoBytes);
            }
        }
        await FindZlib(kddStream);
        var abbreviationsLength = kddStream.ReadUInt16();
        kddStream.BaseStream.Seek(2, SeekOrigin.Current);
        _Abbreviations = await Rtf2HtmlAsync(await ReadZlib(kddStream, abbreviationsLength, 4));

        await FindZlib(kddStream);
        var metadataLength = kddStream.ReadUInt16();
        kddStream.BaseStream.Seek(2, SeekOrigin.Current);
        _Metadata = await ReadZlib(kddStream, metadataLength, 4);

        await FindZlib(kddStream);
        int definitionCnt = 0;
        while (kddStream.BaseStream.Position < kddStream.BaseStream.Length)
        {
            var definitionLength = kddStream.ReadUInt16();
            kddStream.BaseStream.Seek(2, SeekOrigin.Current);
            var definition = await Rtf2HtmlAsync(await ReadZlib(kddStream, definitionLength, 4));
            entries[definitionCnt].Definition = definition;
            definitionCnt++;
            progress.Step = definitionCnt;
            kddStream.BaseStream.Seek(4, SeekOrigin.Current);
        }
    }

    protected override void ReadEntries(Progress progress){}
}
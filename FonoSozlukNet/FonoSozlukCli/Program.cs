using CommandLine;
using FonoConverter;
using FonoFileFormats;
using HunspellWordForms;

namespace FonoSozlukCli
{
    public class Options
    {
        [Option('f', "file", Required = true, HelpText = "Girdi Fono veri dosyasının konumu.")]
        public string? FonoFile {get; set;}

        [Option('o', "out", HelpText = "Çıktı dosyalarının kaydedileceği konum.")]
        public string? OutFolder {get; set;}

        [Option(Group = "OutputChoice", HelpText = "Sözlük Tsv biçiminde kaydedilsin.")]
        public bool Tsv {get; set;}

        [Option(Group = "OutputChoice", HelpText = "Sözlük StarDict biçiminde kaydedilsin.")]
        public bool StarDict {get; set;}

        [Option("hunspell", HelpText = "Madde başlarının çekimli durumlarının üretilmesi için gerekli Hunspell Dic dosyasının konumu.")]
        public string? HunspellPath {get; set;}
    }

    class Program
    {
        private static bool CheckPath(string filePath)
        {
            if (!Path.Exists(filePath))
            {
                Console.WriteLine($"[!] {filePath} konumu geçerli değil.");
                return false;
            }
            return true;
        }
        private static string GetOutputFolder(string outFolder)
        {
            if (string.IsNullOrEmpty(outFolder) || !CheckPath(outFolder))
            {
                var res = Environment.CurrentDirectory;
                Console.WriteLine($"[!] Çıktı konumu verilmemiş veya geçerli değil, dosyalar çalışma dizinine kaydedilecek: {res}");
                return res;
            }
            return outFolder;
        }
        private static DictInfo GetDictInfo(string fileName)
        {
            var stem = Path.GetFileNameWithoutExtension(fileName);
            var dictInfo = new DictInfo()
            {
                Author = "FonoSozlukNet @ https://github.com/anezih/FonoSozlukNet",
                FileName = stem,
                Title = stem,
                Description = $"{stem} Fono Sözlüğü"
            };
            return dictInfo;
        }

        private static WordForms? GetWordForms(string hunspellPath)
        {
            if (string.IsNullOrEmpty(hunspellPath))
            {
                return null;
            }
            if (!CheckPath(hunspellPath))
            {
                return null;
            }
            return new WordForms(hunspellPath);
        }

        private static void PreOutputMsg(string msg)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        private static void PostOutputMsg(string msg)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"{msg}\n");
            Console.ResetColor();
        }

        static void RunOptions(Options opts)
        {
            Progress readProgress = new();

            readProgress.StepIncremented += (object source, EventArgs e) =>
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.ForegroundColor = ConsoleColor.Black;
                    var percent = readProgress.Total != 0 ? ((double)readProgress.Step / readProgress.Total) : 0;
                    Console.Write($"\r> ({percent:P2}) {readProgress.Step:n0} / {readProgress.Total:n0} tane girdi veri dosyasından okundu.");
                    if (readProgress.Step == readProgress.Total)
                    {
                        Console.ResetColor();
                        Console.WriteLine("\n\n");
                    }
                };
            var fname = opts.FonoFile;
            if (!CheckPath(fname!))
            {
                return;
            }
            var outFolder = GetOutputFolder(opts.OutFolder!);
            var wordForms = GetWordForms(opts.HunspellPath!);
            var dictInfo = GetDictInfo(fname!);
            var format = FonoFormat.GuessTypeFromFileExtension(fname!);
            var fonoFormat = FonoFormat.GetFonoFormat(fname!, readProgress, format);
            var converter = wordForms == null
                ? new Converter(dictInfo, fonoFormat!)
                : new Converter(dictInfo, fonoFormat!, wordForms);
            if (opts.Tsv)
            {
                PreOutputMsg("Sözlük Tsv biçiminde kaydediliyor...");
                converter.ToTsv(outFolder);
                PostOutputMsg("Sözlük Tsv biçiminde kaydedildi.");
            }
            if (opts.StarDict)
            {
                PreOutputMsg("Sözlük StarDict biçiminde kaydediliyor...");
                converter.ToStardict(outFolder);
                PostOutputMsg("Sözlük StarDict biçiminde kaydedildi.");
            }
        }

        static void Main(string[] args)
        {
            #if DEBUG
                List<string> list = new List<string>();
                if (args.Length == 0)
                {
                    string? arg = "";
                    do
                    {
                        arg = Console.ReadLine();
                        if (!string.IsNullOrEmpty(arg)) list.Add(arg);
                    } while (arg != "");
                    args = list.ToArray();
                }
            #endif
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }
    }
}
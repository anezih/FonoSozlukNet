﻿using System.Text;
using StarDictNet;
using FonoFileFormats;
using HunspellWordForms;

namespace FonoConverter;

public class Converter
{
    private readonly DictInfo dictInfo;
    private readonly BaseFonoFormat fonoFormat;
    private readonly WordForms? wordForms;

    public Converter(DictInfo dictInfo, BaseFonoFormat fonoFormat)
    {
        this.dictInfo = dictInfo;
        this.fonoFormat = fonoFormat;
    }

    public Converter(DictInfo dictInfo, BaseFonoFormat fonoFormat, WordForms wordForms)
    {
        this.dictInfo = dictInfo;
        this.fonoFormat = fonoFormat;
        this.wordForms = wordForms;
    }

    public async Task<MemoryStream> ToStardict()
    {
        List<OutputEntry> outputEntries = new(fonoFormat.Length);
        if (wordForms != null)
        {
            foreach (var item in fonoFormat)
            {
                await Task.Delay(1);
                var wf = wordForms.GetWordForms(item.Headword, NoPFX:true, NoCross:true).SFX;
                outputEntries.Add(
                    new OutputEntry(item.Headword!, item.Definition!, wf)
                );
            }
        }
        else
        {
            foreach (var item in fonoFormat)
            {
                outputEntries.Add(
                    new OutputEntry(item.Headword!, item.Definition!)
                );
            }
        }
        var stardictMs = await StarDictNet.StarDictNet.WriteAsync(
            entries:outputEntries,
            fileName:dictInfo.FileName,
            title:dictInfo.Title,
            author:dictInfo.Author,
            description:dictInfo.Description
        );
        return stardictMs;
    }

    public void ToStardict(string outFolder)
    {
        List<OutputEntry> outputEntries = new(fonoFormat.Length);
        if (wordForms != null)
        {
            foreach (var item in fonoFormat)
            {
                var wf = wordForms.GetWordForms(item.Headword, NoPFX:true, NoCross:true).SFX;
                outputEntries.Add(
                    new OutputEntry(item.Headword!, item.Definition!, wf)
                );
            }
        }
        else
        {
            foreach (var item in fonoFormat)
            {
                outputEntries.Add(
                    new OutputEntry(item.Headword!, item.Definition!)
                );
            }
        }
        var stardictMs = StarDictNet.StarDictNet.Write(
            entries:outputEntries,
            fileName:dictInfo.FileName,
            title:dictInfo.Title,
            author:dictInfo.Author,
            description:dictInfo.Description
        );

        string outPath = Path.Combine(outFolder, $"{dictInfo.FileName}.zip");
        using (var fs = new FileStream(outPath, FileMode.Create))
        {
            stardictMs.CopyTo(fs);
        }
    }

    public async Task<MemoryStream> ToTsv()
    {
        MemoryStream ms = new();
        UTF8Encoding utf8NoBom = new(false);
        if (fonoFormat != null && fonoFormat.Length > 0)
        {
            if (!string.IsNullOrEmpty(fonoFormat.Abbreviations))
                await ms.WriteAsync(utf8NoBom.GetBytes($"Abbreviations\t{fonoFormat.Abbreviations}\n"));
            foreach (var entry in fonoFormat)
            {
                await ms.WriteAsync(utf8NoBom.GetBytes($"{entry.Headword}\t{entry.Definition}\n"));
            }
            ms.Seek(0, SeekOrigin.Begin);
        }
        return ms;
    }

    public void ToTsv(string outFolder)
    {
        MemoryStream ms = new();
        UTF8Encoding utf8NoBom = new(false);
        if (fonoFormat != null && fonoFormat.Length > 0)
        {
            if (!string.IsNullOrEmpty(fonoFormat.Abbreviations))
                ms.Write(utf8NoBom.GetBytes($"Abbreviations\t{fonoFormat.Abbreviations}\n"));
            foreach (var entry in fonoFormat)
            {
                ms.Write(utf8NoBom.GetBytes($"{entry.Headword}\t{entry.Definition}\n"));
            }
            ms.Seek(0, SeekOrigin.Begin);
        }

        string outPathTsv = Path.Combine(outFolder, $"{dictInfo.FileName}.tsv");
        using (var fs = new FileStream(outPathTsv, FileMode.Create))
        {
            ms.CopyTo(fs);
        }
    }
}
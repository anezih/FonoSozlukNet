Fono sözlüklerini StarDict ve TSV biçimlerine çevirebilen bir Blazor Webassembly uygulaması.

İlgili sözlüklerin veri dosyaları Fono'nun 2004-2008 aralığında çıkarttığı
Büyük (ve nadiren de olsa Modern) Sözlük'ün yanında gelen CD-ROM'larda yer almaktadır.

FonoSozlukNet, EuroDict XP (.KDD) ve XML formatlarını okuyabilmektedir.

# Blazor Uygulamasının Kullanımı
Sözlükleri dönüştürmek için veri dosyasını dosya alanına sürükleyin. İlerleme durumu
ekranın ortasında gösterilecektir. Okuma bitene kadar **sekmeyi değiştirmeyin**,
yoksa tarayıcı arka plana alınan uygulamanın çalışmasını duraklatabilir. Dosya başarılı bir
şekilde okunduktan sonra madde başları ve tanımlar tablo biçimide alt kısımda gösterilecek ve kaydetme
seçenekleri etkin duruma gelecektir. "TSV Olarak Kaydet" tuşuyla sözlüğü TAB (\t) karakteriyle ayrılmış
biçimde kaydedebilirsiniz. "StarDict Olarak Kaydet" tuşuyla sözlük StarDict biçimine dönüştürülerek ZIP
arşivi olarak kaydedilir. "Hunspell Dic Dosyası" ve "Hunspell Aff Dosyası" kullanıcının yüklediği Hunspell
dosyaları doğrultusunda StarDict biçimine madde başlarının çekimli durumlarını ekler, böylece örneğin
"yapıtlarını" araması "yapıt" sonucunu döndürür. Hunspell dosyalarını
<a href="https://github.com/wooorm/dictionaries">https://github.com/wooorm/dictionaries</a> adresinden
edinebilirsiniz.

# Blazor Uygulaması Önizlemesi

[FonoSozlukNet_onizleme.webm](https://github.com/anezih/FonoSozlukNet/assets/90565940/a9cfeef2-605f-45b1-8fad-45e7e970bccf)


# Uçbirim Uygulamasının Kullanımı

`./FonoSozlukCli --help`
```
FonoSozlukCli 1.0.0+8b09c3cbb4575adc386ad9af6636c0d8e400882f
Copyright (C) 2024 https://github.com/anezih

  -f, --file    Required. Girdi Fono veri dosyasının konumu.

  -o, --out     Çıktı dosyalarının kaydedileceği konum.

  --tsv         (Group: OutputChoice) Sözlük Tsv biçiminde kaydedilsin.

  --stardict    (Group: OutputChoice) Sözlük StarDict biçiminde kaydedilsin.

  --hunspell    Madde başlarının çekimli durumlarının üretilmesi için gerekli Hunspell Dic dosyasının konumu.

  --help        Display this help screen.

  --version     Display version information.
```

## Örnek kullanım:

```powershell
./FonoSozlukCli -f ./KDD/TURITA_P.KDD --stardict --tsv --hunspell ./hunspell/tr_TR.dic
```
Fono sözlüklerini StarDict ve TSV biçimlerine çevirebilen bir Blazor Webassembly uygulaması.

İlgili sözlüklerin veri dosyaları Fono'nun 2004-2008 aralığında çıkarttığı
Büyük (ve nadiren de olsa Modern) Sözlük'ün yanında gelen CD-ROM'larda yer almaktadır.

FonoSozlukNet, EuroDict XP (.KDD) ve XML formatlarını okuyabilmektedir.

# Kullanımı
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
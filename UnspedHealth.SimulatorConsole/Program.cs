using System.Net.Http.Json;
using UnspedHealth.Client;

const string ApiBaseUrl = "https://api.unsped.com/";

var serviceList = new List<(Guid Id, string Name, string IP)>
{
    (Guid.Parse("69DC0444-08F2-F011-8153-0050569CBA97"), "UGMRadar.SepetMailKorelasyon", "172.27.2.243"),
    (Guid.Parse("E626930E-0AF2-F011-8153-0050569CBA97"), "UGMRadar.SepetSendDCD", "172.27.2.243"),
    (Guid.Parse("E7E31D24-0AF2-F011-8153-0050569CBA97"), "UGMRadar.EvrimArsivAddSepet", "172.27.2.243"),
    (Guid.Parse("8428D73D-0AF2-F011-8153-0050569CBA97"), "UGMRadar.FileDetailClassifier", "172.27.2.243"),
    (Guid.Parse("5F2DB046-0AF2-F011-8153-0050569CBA97"), "UGMRadar.FileDetailClassifier (Excel)", "172.27.2.243"),
    (Guid.Parse("DCF91A55-0AF2-F011-8153-0050569CBA97"), "UGMRadar.FileDetailData", "172.27.2.243"),
    (Guid.Parse("82B3E66C-0AF2-F011-8153-0050569CBA97"), "UGMRadar.FileSplitter", "172.27.2.157"),
    (Guid.Parse("0ADC2678-0AF2-F011-8153-0050569CBA97"), "UGMRadar.FileSplitter_API", "172.27.2.157"),
    (Guid.Parse("3A93BC86-0AF2-F011-8153-0050569CBA97"), "UGMRadar.FileSplitter (Status7)", "172.27.2.157")
};

using var client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };

Console.WriteLine("=====================================================");
Console.WriteLine("   UNSPED WATCHDOG SİSTEM SİMÜLASYONU (ÇOKLU SERVİS)   ");
Console.WriteLine("=====================================================\n");

while (true)
{
    Console.WriteLine("Seçim Yapın:");
    Console.WriteLine("1 - Tüm Servisler İçin 'Normal' Heartbeat Gönder (Sistemi Yeşile Döndür)");
    Console.WriteLine("2 - Sadece Belirli Bir Servis İçin HATA Bildir (Örn: SepetMail)");
    Console.WriteLine("3 - OTOMATİK MOD: Tüm servisleri 30 saniyede bir güncelle");
    Console.WriteLine("4 - RASTGELE KESİNTİ: Bazı servisleri gönder, bazılarını gönderme (Test)");
    Console.WriteLine("X - Çıkış");
    Console.Write("\nİşlem: ");

    var key = Console.ReadLine()?.ToUpper();

    switch (key)
    {
        case "1":
            await SendBatchHeartbeat(client, serviceList);
            break;
        case "2":
            // İlk servisi örnek hata olarak gönderiyoruz
            await SendReport(client, serviceList[0].Id, "Unexpected document format.", true);
            break;
        case "3":
            await StartAutoMode(client, serviceList);
            break;
        case "4":
            await SendRandomHeartbeat(client, serviceList);
            break;
        case "X": return;
    }
    Console.WriteLine("\n-----------------------------------------------------\n");
}


// --- METOTLAR ---

async Task SendBatchHeartbeat(HttpClient http, List<(Guid Id, string Name, string IP)> list)
{
    Console.WriteLine("\n[TOPLU SİNYAL] Gönderiliyor...");
    foreach (var item in list)
    {
        var response = await http.PostAsJsonAsync($"healthcheck/health/heartbeat/{item.Id}", new { });
        Console.WriteLine($"{(response.IsSuccessStatusCode ? "OK" : "FAILED")} {item.Name} ({item.IP})");
    }
}

async Task SendRandomHeartbeat(HttpClient http, List<(Guid Id, string Name, string IP)> list)
{
    var rnd = new Random();
    Console.WriteLine("\n[RASTGELE TEST] Bazı servisler sinyal göndermiyor...");
    foreach (var item in list)
    {
        if (rnd.Next(0, 2) == 1) // %50 şansla gönder
        {
            await http.PostAsJsonAsync($"healthcheck/health/heartbeat/{item.Id}", new { });
            Console.WriteLine($"{item.Name} Sinyal GÖNDERDİ.");
        }
        else
        {
            Console.WriteLine($"{item.Name} Sinyal KESTİ! (Worker mail tetikleyebilir)");
        }
    }
}

 async Task SendReport(HttpClient http, Guid id, string msg, bool isError)
{
    var req = new { ServiceID = id, Message = msg, IsError = isError };
    var response = await http.PostAsJsonAsync("healthcheck/health/report", req);
    Console.WriteLine(response.IsSuccessStatusCode ? "Rapor iletildi." : "Rapor başarısız.");
}

async Task StartAutoMode(HttpClient http, List<(Guid Id, string Name, string IP)> list)
{
    Console.WriteLine("\n[OTOMATİK] Çıkmak için Ctrl+C. 30sn bekliyor...");
    while (true)
    {
        await SendBatchHeartbeat(http, list);
        await Task.Delay(30000);
    }
}
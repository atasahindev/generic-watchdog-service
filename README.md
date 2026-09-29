# UGM Watchdog System

UnspedHealth, yüksek erişilebilirlik gerektiren kurumsal Windows ekosistemleri için geliştirilmiş, dağıtık mimarili bir "Self-Healing" (Kendi Kendini Onaran) izleme çözümüdür.
Sistem, kritik süreçlerin (Console, Service, Web) yaşam döngüsünü proaktif olarak izler ve insan müdahalesine gerek kalmadan kesintileri minimize eder.

## Sistem Mimarisi
- **UnspedHealth.Worker:** Ana kontrolör ve izleyici servis. Merkezden gelen heartbeat isteklerini işler, izleme görevlerini planlar ve sonuçları değerlendirir. | .NET 8 Worker Service
- **UnspedHealth.Agent:** İzlenecek servislerin çalıştığı hedef sunucularda barınır. Merkezden gelen restart komutlarını işletim sistemi seviyesinde icra eder. | .NET 8 Minimal API
- **UnspedHealth.Core:** Tüm projelerin kullandığı modeller ve servisler, ortak kod tabanı. | .NET 8 Class Library
- **UnspedHealth.API:** Sistem içi uygulamaların aktif olarak heartbeat (hayattayım) isteği göndermesi için kurgulanan opsiyonel API servisidir. | .NET 8 API

## Güvenlik ve İletişim
- *Kimlik Doğrulama: Agent ve Worker arasındaki tüm trafik API Key korumalıdır. Her istek, HTTP Header üzerinden gönderilen X-Agent-Key bilgisi ile doğrulanır. Anahtarı eşleşmeyen talepler işleme alınmaz.*
- *Hata Yönetimi: Ağ üzerindeki iletişim hataları, zaman aşımı (timeout) durumları veya süreç başlatma başarısızlıkları Serilog kütüphanesi kullanılarak merkezi olarak kayıt altına alınır.*
- *Yapılandırma Yönetimi: Port, ApiKey ve DB bağlantı dizeleri gibi hassas bilgiler appsettings.json üzerinden okunur. Uygulama içerisinde bu verilere AppConfig static sınıfı üzerinden erişilir.*

## Kurulum ve Dağıtım

1. **Agent Kurulumu:**
UnspedHealth.Agent projesini Release modunda publish alın.
Publish edilen dosyaları hedef sunucuya (izlenecek sunucu) kopyalayın.
appsettings.json dosyasında Port ve ApiKey bilgilerini güncelleyin.
Uygulamayı bir Windows Servisi olarak sisteme kaydedin (Installer klasöründeki yardımcı betikleri kullanabilirsiniz).

2. **Worker Kurulumu:**
UnspedHealth.Worker projesinin appsettings.json dosyasında ConnectionStrings (UGMArsiv DB) ve AgentSettings (Port ve ApiKey) alanlarını tanımlayın.
Servisi merkezi izleme sunucusunda başlatın.

3. **Veritabanı (Database) Yapılandırması:**
İzlenecek sunucuların IP adresleri, port bilgileri ve ilgili servislerin dosya yolları **UGMArsiv** veritabanındaki **UGM_ServiceHealthChecks** tablosuna eklenmelidir.
Sistem, worker tarafından işlenen Down/Degraded durumlarını CheckInterval süresine göre dinamik olarak algılar.

4. **API Kurulumu:**
UnspedHealth.API projesini publish alarak merkezi izleme sunucusuna kurabilirsiniz.
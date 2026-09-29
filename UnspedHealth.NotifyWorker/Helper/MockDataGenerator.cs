using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnspedHealth.Core.Entities;

namespace UnspedHealth.Worker.Helper
{
    public static class MockDataGenerator
    {
        public static List<ServiceHealthCheck> GetFakeServices()
        {
            return new List<ServiceHealthCheck>
        {
            // Senaryo 1: Python API - Otomatik kurtarma denenecek (Başarılı olacak)
            new ServiceHealthCheck
            {
                ID = Guid.NewGuid(),
                ApplicationName = "AI Prediction API",
                ServerIP = "10.20.30.50",
                ServicePath = "/app/ai/main.py",
                HealthEndpoint = "http://10.20.30.50:5000/health",
                AutoRestartEnabled = false,
                BatchFilePath = "restart_ai_api.sh",
                LastHeartbeat = DateTime.Now.AddMinutes(-10),
                MonitoringMode = 1, // Active
                Contacts = "[\"atasahin@ugm.com.tr\", \"atasahindev@outlook.com\"]"
            },
            // Senaryo 2: Windows Servisi - Otomatik kurtarma denenecek (Başarısız kalacak)
            new ServiceHealthCheck
            {
                ID = Guid.NewGuid(),
                ApplicationName = "ERP Integration Service",
                ServerIP = "10.20.30.60",
                ServicePath = "C:\\Services\\ErpSync.exe",
                AutoRestartEnabled = false,
                BatchFilePath = "C:\\Scripts\\restart_erp.bat",
                LastHeartbeat = DateTime.Now.AddMinutes(-30),
                MonitoringMode = 2, // Passive
                Contacts = "[\"atasahin@ugm.com.tr\", \"atasahindev@outlook.com\"]"
            },
            // Senaryo 3: Klasik Servis - Kurtarma izni yok (Doğrudan DOWN gidecek)
            new ServiceHealthCheck
            {
                ID = Guid.NewGuid(),
                ApplicationName = "Legacy Report Engine",
                ServerIP = "10.20.30.70",
                ServicePath = "C:\\Legacy\\Report.exe",
                AutoRestartEnabled = true,
                LastHeartbeat = DateTime.Now.AddHours(-2),
                MonitoringMode = 2,
                Contacts = "atasahin2@gmail.com"
            }
        };
        }

        public static List<Server> GetFakeServers()
        {
            return new List<Server>
        {
            new Server
            {
                ID = Guid.NewGuid(),
                ServerName = "UGM-PROD-DB01",
                IpAddress = "10.20.30.10",
                LastPingTime = DateTime.Now.AddMinutes(-5),
                Status = 0 // Offline
            }
        };
        }
    }
}
